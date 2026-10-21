// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    /// <summary>
    /// SQL Tunnel relay host (iPlus fork extension).
    ///
    /// Hosts a WebSocket endpoint (<c>/sqltunnel</c>) that acts as a dumb byte relay between
    /// browser WASM clients (which have no raw TCP sockets) and a real SQL Server TCP endpoint.
    /// The relay never parses TDS: every WebSocket binary message payload is forwarded to the
    /// SQL Server socket and vice versa, preserving order. TLS/SSPI inside the TDS stream
    /// terminates at SQL Server, so credentials never pass through iPlus logic.
    ///
    /// Client side: the forked <c>Microsoft.Data.SqlClient</c> opens the tunnel when the
    /// connection string contains <c>Tunnel URL=wss://host:port/sqltunnel</c>
    /// (see <c>SniWebSocketHandle</c> / <c>WebSocketStream</c> in the SqlClient fork).
    ///
    /// Modeled after <see cref="PAMcpServerHost"/>: own Kestrel <c>WebApplication</c>,
    /// config via <c>ACPropertyConfigValue</c>, alarms via binding source.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SQL Tunnel Host'}de{'SQL Tunnel Host'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class PASqlTunnelHost : PAWebServiceBase
    {
        #region c'tors
        public PASqlTunnelHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IsEnabled = new ACPropertyConfigValue<bool>(this, nameof(IsEnabled), false);
            _SqlServer = new ACPropertyConfigValue<string>(this, nameof(SqlServer), "");
            _SqlPort = new ACPropertyConfigValue<int>(this, nameof(SqlPort), 1433);
            _RequireOAuth = new ACPropertyConfigValue<bool>(this, nameof(RequireOAuth), false);
            _RequireIPlusLogin = new ACPropertyConfigValue<bool>(this, nameof(RequireIPlusLogin), false);
            _SqlUser = new ACPropertyConfigValue<string>(this, nameof(SqlUser), "");
            _SqlPassword = new ACPropertyConfigValue<string>(this, nameof(SqlPassword), "");
            _TokenIssuer = new ACPropertyConfigValue<string>(this, nameof(TokenIssuer), "");
            _EntraTenantId = new ACPropertyConfigValue<string>(this, nameof(EntraTenantId), "");
            _EntraAudience = new ACPropertyConfigValue<string>(this, nameof(EntraAudience), "");
            _EntraInstance = new ACPropertyConfigValue<string>(this, nameof(EntraInstance), "");
            _UseTls = new ACPropertyConfigValue<bool>(this, nameof(UseTls), false);
            _TlsCertificatePath = new ACPropertyConfigValue<string>(this, nameof(TlsCertificatePath), "");
            _TlsCertificatePassword = new ACPropertyConfigValue<string>(this, nameof(TlsCertificatePassword), "");
            // Command to enable http-Service:
            // >netsh http add urlacl url=http://+:ServicePort/ user="\Everyone"
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _ = IsEnabled;
            _ = ServicePort;
            _ = SqlServer;
            _ = SqlPort;
            _ = RequireOAuth;
            _ = RequireIPlusLogin;
            _ = SqlUser;
            _ = SqlPassword;
            _ = TokenIssuer;
            _ = EntraTenantId;
            _ = EntraAudience;
            _ = EntraInstance;
            _ = UseTls;
            _ = TlsCertificatePath;
            _ = TlsCertificatePassword;
            return result;
        }

        public override bool ACPostInit()
        {
            StartService();
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            StopService();
            bool result = await base.ACDeInit(deleteACClassTask);
            return result;
        }
        #endregion

        #region Properties

        private ACPropertyConfigValue<bool> _IsEnabled;
        [ACPropertyConfig("en{'Enable SQL Tunnel'}de{'SQL Tunnel aktivieren'}")]
        public bool IsEnabled
        {
            get => _IsEnabled.ValueT;
            set => _IsEnabled.ValueT = value;
        }

        private ACPropertyConfigValue<string> _SqlServer;
        [ACPropertyConfig("en{'SQL Server host'}de{'SQL Server Host'}")]
        public string SqlServer
        {
            get => _SqlServer.ValueT;
            set => _SqlServer.ValueT = value;
        }

        private ACPropertyConfigValue<int> _SqlPort;
        [ACPropertyConfig("en{'SQL Server port'}de{'SQL Server Port'}")]
        public int SqlPort
        {
            get => _SqlPort.ValueT;
            set => _SqlPort.ValueT = value;
        }

        private ACPropertyConfigValue<string> _SqlUser;
        /// <summary>
        /// Server-side SQL login delivered to authenticated tunnel clients via
        /// GET /sqltunnel-sqlcred (docs §9.24): the client fetches the credentials after
        /// stage-1 auth and puts them into its connection string, so no SQL password is
        /// stored client-side. Env override: SQLTUNNEL_SQL_USER.
        /// </summary>
        [ACPropertyConfig("en{'SQL user (delivered)'}de{'SQL-Benutzer (zugewiesen)'}")]
        public string SqlUser
        {
            get
            {
                string v = _SqlUser.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_SQL_USER");
                return v;
            }
            set => _SqlUser.ValueT = value;
        }

        private ACPropertyConfigValue<string> _SqlPassword;
        /// <summary>Server-side SQL password delivered via /sqltunnel-sqlcred (see SqlUser). Env override: SQLTUNNEL_SQL_PASSWORD.</summary>
        [ACPropertyConfig("en{'SQL password (delivered)'}de{'SQL-Kennwort (zugewiesen)'}")]
        public string SqlPassword
        {
            get
            {
                string v = _SqlPassword.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_SQL_PASSWORD");
                return v;
            }
            set => _SqlPassword.ValueT = value;
        }

        /// <summary>True when the relay delivers SQL credentials to authenticated clients.</summary>
        private bool SqlCredentialDeliveryEnabled =>
            !String.IsNullOrEmpty(SqlUser) && !String.IsNullOrEmpty(SqlPassword);

        private ACPropertyConfigValue<bool> _RequireOAuth;
        /// <summary>
        /// Stage 1 (docs §9.21): require a valid bearer token (issued by
        /// <c>POST /sqltunnel-auth/token</c> on this host) on the WebSocket handshake and on
        /// every <c>/sqltunnel-sync</c> request. Default false so existing local tests keep
        /// working. Env override for testing/deployment: SQLTUNNEL_REQUIRE_OAUTH=1.
        /// </summary>
        [ACPropertyConfig("en{'Require OAuth token'}de{'OAuth-Token erforderlich'}")]
        public bool RequireOAuth
        {
            get => _RequireOAuth.ValueT || System.Environment.GetEnvironmentVariable("SQLTUNNEL_REQUIRE_OAUTH") == "1";
            set => _RequireOAuth.ValueT = value;
        }

        private ACPropertyConfigValue<bool> _RequireIPlusLogin;
        /// <summary>
        /// Stage 2 (docs §9.21): require the iPlus login auth frame (WebSocket first message
        /// resp. shim session body) and validate it via the iPlus login stack BEFORE opening
        /// the TcpClient to SQL Server. An auth frame is always validated when present, even
        /// if this switch is false. Env override: SQLTUNNEL_REQUIRE_IPLUSLOGIN=1.
        /// </summary>
        [ACPropertyConfig("en{'Require iPlus login'}de{'iPlus-Login erforderlich'}")]
        public bool RequireIPlusLogin
        {
            get => _RequireIPlusLogin.ValueT || System.Environment.GetEnvironmentVariable("SQLTUNNEL_REQUIRE_IPLUSLOGIN") == "1";
            set => _RequireIPlusLogin.ValueT = value;
        }

        [ACPropertyBindingSource(200, "Error", "en{'SQL Tunnel Alarm'}de{'SQL Tunnel Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsTunnelAlarm { get; set; }

        public override Type ServiceType => null;

        public override Type ServiceInterfaceType => null;

        // The relay is hosted by StartService() via its own WebApplication; no separate
        // IHost service instance is needed (same approach as PAMcpServerHost).
        public override IHost CreateService() => null;

        private WebApplication _Host = null;
        private CancellationTokenSource _CancellationTokenSource = null;
        private Task _HostTask = null;

        /// <summary>Active sync-XHR shim sessions (browser clients). Key = sessionId.</summary>
        private readonly ConcurrentDictionary<string, SyncSession> _SyncSessions = new ConcurrentDictionary<string, SyncSession>();
        private Task _SyncSessionCleanupTask = null;

        /// <summary>Idle time after which a shim session's SQL socket is closed.</summary>
        private static readonly TimeSpan SyncSessionIdleTimeout = TimeSpan.FromMinutes(5);

        /// <summary>Max time /recv holds the request before returning 204 (client retries).</summary>
        private static readonly TimeSpan RecvHoldTime = TimeSpan.FromSeconds(1);

        #region Tunnel Auth (docs §9.21)

        /// <summary>Lifetime of an OAuth-style bearer token issued by /sqltunnel-auth/token.</summary>
        private static readonly TimeSpan OAuthTokenLifetime = TimeSpan.FromHours(8);

        /// <summary>Issued bearer tokens (token -> UTC expiry). Cleared lazily on validation.</summary>
        private static readonly ConcurrentDictionary<string, DateTime> _OAuthTokens = new ConcurrentDictionary<string, DateTime>();

        /// <summary>Magic prefix of the WebSocket auth frame (must match TunnelAuthInfo in the fork).</summary>
        private const string AuthFrameMagic = "IPTUNNEL-AUTH1\n";

        /// <summary>
        /// Stage 1: validates a bearer token against the in-memory token store.
        /// When the issuer mode is "Entra" (SQLTUNNEL_TOKEN_ISSUER=Entra), the token is
        /// instead validated as a Microsoft Entra ID JWT (see ValidateEntraTokenAsync).
        /// </summary>
        private async Task<bool> ValidateBearerTokenAsync(string token)
        {
            if (String.IsNullOrEmpty(token))
                return false;
            if (String.Equals(TokenIssuer, "Entra", StringComparison.OrdinalIgnoreCase))
                return await ValidateEntraTokenAsync(token);
            if (_OAuthTokens.TryGetValue(token, out DateTime expiry))
            {
                if (expiry > DateTime.UtcNow)
                    return true;
                _OAuthTokens.TryRemove(token, out _);
            }
            return false;
        }

        #region Entra ID issuer (stage 1, real IdP — docs §9.21 / open question §6.1)

        private ACPropertyConfigValue<string> _TokenIssuer;
        /// <summary>
        /// Issuer mode: "Local" (default, in-memory tokens) or "Entra" (JWT validation).
        /// Env override: SQLTUNNEL_TOKEN_ISSUER.
        /// </summary>
        [ACPropertyConfig("en{'Token issuer mode'}de{'Token-Issuer-Modus'}")]
        public string TokenIssuer
        {
            get
            {
                string v = _TokenIssuer.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_TOKEN_ISSUER");
                return v;
            }
            set => _TokenIssuer.ValueT = value;
        }

        private ACPropertyConfigValue<string> _EntraTenantId;
        /// <summary>
        /// Entra tenant: GUID or domain (e.g. contoso.onmicrosoft.com). "organizations"
        /// accepts any work/school tenant (multi-tenant app). Env override: SQLTUNNEL_ENTRA_TENANT.
        /// </summary>
        [ACPropertyConfig("en{'Entra tenant'}de{'Entra-Mandant'}")]
        public string EntraTenantId
        {
            get
            {
                string v = _EntraTenantId.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_ENTRA_TENANT");
                return v;
            }
            set => _EntraTenantId.ValueT = value;
        }

        private ACPropertyConfigValue<string> _EntraAudience;
        /// <summary>
        /// Expected "aud" claim: the app registration's Client ID or its api:// URI.
        /// Env override: SQLTUNNEL_ENTRA_AUDIENCE.
        /// </summary>
        [ACPropertyConfig("en{'Entra audience'}de{'Entra-Zielgruppe'}")]
        public string EntraAudience
        {
            get
            {
                string v = _EntraAudience.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_ENTRA_AUDIENCE");
                return v;
            }
            set => _EntraAudience.ValueT = value;
        }

        private ACPropertyConfigValue<string> _EntraInstance;
        /// <summary>
        /// Cloud instance override (sovereign clouds); default global Entra.
        /// Env override: SQLTUNNEL_ENTRA_INSTANCE.
        /// </summary>
        [ACPropertyConfig("en{'Entra instance'}de{'Entra-Instanz'}")]
        public string EntraInstance
        {
            get
            {
                string v = _EntraInstance.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_ENTRA_INSTANCE");
                return String.IsNullOrEmpty(v) ? "https://login.microsoftonline.com" : v;
            }
            set => _EntraInstance.ValueT = value;
        }

        private ACPropertyConfigValue<bool> _UseTls;
        /// <summary>
        /// Serves the relay over HTTPS/WSS (docs §9.28): the WebSocket endpoint, the sync
        /// shim, the token and sqlcred endpoints are all TLS-encrypted. When no certificate
        /// is configured, a self-signed certificate is generated and persisted next to the
        /// executable (clients must trust it — see §9.28). Env override: SQLTUNNEL_USE_TLS=1.
        /// </summary>
        [ACPropertyConfig("en{'Use TLS (https/wss)'}de{'TLS verwenden (https/wss)'}")]
        public bool UseTls
        {
            get => _UseTls.ValueT || System.Environment.GetEnvironmentVariable("SQLTUNNEL_USE_TLS") == "1";
            set => _UseTls.ValueT = value;
        }

        private ACPropertyConfigValue<string> _TlsCertificatePath;
        /// <summary>Path to a PFX/PKCS#12 certificate. Empty = auto self-signed at iplus-relay-tls.pfx. Env override: SQLTUNNEL_TLS_CERT_PATH.</summary>
        [ACPropertyConfig("en{'TLS certificate path (PFX)'}de{'TLS-Zertifikatspfad (PFX)'}")]
        public string TlsCertificatePath
        {
            get
            {
                string v = _TlsCertificatePath.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_TLS_CERT_PATH");
                return v;
            }
            set => _TlsCertificatePath.ValueT = value;
        }

        private ACPropertyConfigValue<string> _TlsCertificatePassword;
        /// <summary>PFX password. Env override: SQLTUNNEL_TLS_CERT_PASSWORD.</summary>
        [ACPropertyConfig("en{'TLS certificate password'}de{'TLS-Zertifikatskennwort'}")]
        public string TlsCertificatePassword
        {
            get
            {
                string v = _TlsCertificatePassword.ValueT;
                if (String.IsNullOrEmpty(v))
                    v = System.Environment.GetEnvironmentVariable("SQLTUNNEL_TLS_CERT_PASSWORD");
                return v;
            }
            set => _TlsCertificatePassword.ValueT = value;
        }

        private static readonly System.Net.Http.HttpClient _EntraHttp = new System.Net.Http.HttpClient();

        /// <summary>Cached JWKS signing keys (kid -> RSA parameters). Refreshed after 12h or on unknown kid.</summary>
        private static readonly ConcurrentDictionary<string, (DateTime Fetched, byte[] Modulus, byte[] Exponent)> _JwksKeys = new();
        private static DateTime _JwksFetchedUtc = DateTime.MinValue;
        private static readonly TimeSpan JwksCacheLifetime = TimeSpan.FromHours(12);
        private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Validates a Microsoft Entra ID access token (JWT) against the tenant's published
        /// signing keys: RS256 signature, issuer, audience and lifetime. No external package
        /// dependency (hand-rolled JWS verification via System.Security.Cryptography).
        /// </summary>
        private async Task<bool> ValidateEntraTokenAsync(string token)
        {
            try
            {
                string[] parts = token.Split('.');
                if (parts.Length != 3)
                    return false;
                byte[] headerBytes = Base64UrlDecode(parts[0]);
                byte[] payloadBytes = Base64UrlDecode(parts[1]);
                byte[] signature = Base64UrlDecode(parts[2]);
                if (headerBytes == null || payloadBytes == null || signature == null)
                    return false;

                using JsonDocument headerDoc = JsonDocument.Parse(headerBytes);
                string alg = headerDoc.RootElement.TryGetProperty("alg", out JsonElement a) && a.ValueKind == JsonValueKind.String ? a.GetString() : null;
                string kid = headerDoc.RootElement.TryGetProperty("kid", out JsonElement k) && k.ValueKind == JsonValueKind.String ? k.GetString() : null;
                if (!String.Equals(alg, "RS256", StringComparison.Ordinal) || String.IsNullOrEmpty(kid))
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Entra token rejected: unsupported alg '{alg}' or missing kid");
                    return false;
                }

                // The signing keys are published PER TENANT — "organizations" is not a
                // valid discovery tenant, so always resolve the JWKS via the token's tid.
                string tid = null;
                string iss = null, aud = null;
                long exp = 0, nbf = 0;
                try
                {
                    using JsonDocument payloadDoc = JsonDocument.Parse(payloadBytes);
                    JsonElement root = payloadDoc.RootElement;
                    iss = root.TryGetProperty("iss", out JsonElement i) && i.ValueKind == JsonValueKind.String ? i.GetString() : null;
                    aud = root.TryGetProperty("aud", out JsonElement au) && au.ValueKind == JsonValueKind.String ? au.GetString() : null;
                    tid = root.TryGetProperty("tid", out JsonElement ti) && ti.ValueKind == JsonValueKind.String ? ti.GetString() : null;
                    exp = root.TryGetProperty("exp", out JsonElement e) && e.TryGetInt64(out long expV) ? expV : 0;
                    nbf = root.TryGetProperty("nbf", out JsonElement n) && n.TryGetInt64(out long nbfV) ? nbfV : 0;
                }
                catch
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", "Entra token rejected: payload is not valid JSON");
                    return false;
                }
                if (String.IsNullOrEmpty(tid))
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", "Entra token rejected: missing tid claim");
                    return false;
                }

                (byte[] modulus, byte[] exponent)? key = await GetEntraSigningKeyAsync(kid, tid);
                if (key == null)
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Entra token rejected: unknown kid '{kid}' for tenant '{tid}'");
                    return false;
                }

                byte[] signed = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);
                using RSA rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters { Modulus = key.Value.modulus, Exponent = key.Value.exponent });
                if (!rsa.VerifyData(signed, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", "Entra token rejected: signature invalid");
                    return false;
                }

                // Single-tenant config: accept the v2.0 issuer of the configured tenant AND
                // the v1 issuer (https://sts.windows.net/{tid}/) — the device-code endpoint
                // returns v1 tokens when no v2.0-specific scope is requested. Multi-tenant
                // ("organizations"): accept any tenant issuer on the configured instance.
                string configuredTenant = String.IsNullOrEmpty(EntraTenantId) ? "organizations" : EntraTenantId;
                string expectedIssV2 = $"{EntraInstance.TrimEnd('/')}/{configuredTenant}/v2.0";
                string expectedIssV1 = $"https://sts.windows.net/{configuredTenant}/";
                string instanceBase = EntraInstance.TrimEnd('/') + "/";
                bool issuerOk = String.Equals(iss, expectedIssV2, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(iss, expectedIssV1, StringComparison.OrdinalIgnoreCase)
                    || (String.Equals(EntraTenantId, "organizations", StringComparison.OrdinalIgnoreCase)
                        && iss != null
                        && (iss.StartsWith(instanceBase, StringComparison.OrdinalIgnoreCase) && iss.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase)
                            || iss.StartsWith("https://sts.windows.net/", StringComparison.OrdinalIgnoreCase) && iss.EndsWith("/")));
                bool audienceOk = !String.IsNullOrEmpty(EntraAudience) && String.Equals(aud, EntraAudience, StringComparison.OrdinalIgnoreCase);
                bool lifetimeOk = exp > DateTimeOffset.UtcNow.Add(-ClockSkew).ToUnixTimeSeconds()
                    && nbf < DateTimeOffset.UtcNow.Add(ClockSkew).ToUnixTimeSeconds();

                if (!issuerOk || !audienceOk || !lifetimeOk)
                {
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Entra token rejected: issuerOk={issuerOk} (iss='{iss}') audienceOk={audienceOk} (aud='{aud}') lifetimeOk={lifetimeOk}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "SqlTunnelAuth", new Exception("Entra token validation failed: " + ex.Message));
                return false;
            }
        }

        /// <summary>Fetches (and caches) the tenant JWKS and returns the RSA parameters for kid.</summary>
        private async Task<(byte[] Modulus, byte[] Exponent)?> GetEntraSigningKeyAsync(string kid, string tenantId)
        {
            string cacheKey = tenantId + "/" + kid;
            if (_JwksKeys.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow - cached.Fetched < JwksCacheLifetime)
                return (cached.Modulus, cached.Exponent);

            string url = $"{EntraInstance.TrimEnd('/')}/{tenantId}/discovery/v2.0/keys";
            string json;
            try
            {
                json = await _EntraHttp.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "SqlTunnelAuth", new Exception($"JWKS fetch failed for {url}: {ex.Message}"));
                return null;
            }
            using JsonDocument doc = JsonDocument.Parse(json);
            foreach (JsonElement keyEl in doc.RootElement.GetProperty("keys").EnumerateArray())
            {
                string keyKid = keyEl.TryGetProperty("kid", out JsonElement kk) && kk.ValueKind == JsonValueKind.String ? kk.GetString() : null;
                string kty = keyEl.TryGetProperty("kty", out JsonElement kt) && kt.ValueKind == JsonValueKind.String ? kt.GetString() : null;
                string n = keyEl.TryGetProperty("n", out JsonElement nn) && nn.ValueKind == JsonValueKind.String ? nn.GetString() : null;
                string e = keyEl.TryGetProperty("e", out JsonElement ee) && ee.ValueKind == JsonValueKind.String ? ee.GetString() : null;
                if (keyKid != null && kty == "RSA" && n != null && e != null)
                    _JwksKeys[tenantId + "/" + keyKid] = (DateTime.UtcNow, Base64UrlDecode(n), Base64UrlDecode(e));
            }
            return _JwksKeys.TryGetValue(cacheKey, out var found) ? (found.Modulus, found.Exponent) : null;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            if (String.IsNullOrEmpty(input))
                return null;
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            try { return Convert.FromBase64String(s); }
            catch { return null; }
        }

        #endregion

        /// <summary>
        /// Stage 2: validates iPlus credentials via the iPlus login stack
        /// (VBUser.CheckEnteredPassword, MD5 — same check as the WCF login).
        /// </summary>
        private bool ValidateIPlusUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password))
                return false;
            try
            {
                Database db = ACRoot.SRoot?.Database as Database;
                if (db == null)
                {
                    Messages.LogException(GetACUrl(), "SqlTunnelAuth", new Exception("ACRoot.SRoot.Database not available for iPlus login validation"));
                    return false;
                }
                using (ACMonitor.Lock(db.QueryLock_1X000))
                {
                    VBUser vbUser = db.VBUser.Where(c => c.VBUserName.ToLower() == userName.ToLower()).FirstOrDefault();
                    return vbUser != null && vbUser.CheckEnteredPassword(password);
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "SqlTunnelAuth", ex);
                return false;
            }
        }

        private static string GetBearerToken(IHeaderDictionary headers)
        {
            string value = headers["Authorization"];
            if (!String.IsNullOrEmpty(value) && value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return value.Substring(7).Trim();
            return null;
        }

        /// <summary>Parses the auth frame payload (magic + flat JSON) sent by the fork.</summary>
        private static (string User, string Password, string Token) ParseAuthFrame(byte[] payload, int count)
        {
            try
            {
                string text = Encoding.UTF8.GetString(payload, 0, count);
                if (!text.StartsWith(AuthFrameMagic, StringComparison.Ordinal))
                    return default;
                using JsonDocument doc = JsonDocument.Parse(text.Substring(AuthFrameMagic.Length));
                JsonElement root = doc.RootElement;
                string user = root.TryGetProperty("user", out JsonElement u) && u.ValueKind == JsonValueKind.String ? u.GetString() : null;
                string password = root.TryGetProperty("password", out JsonElement p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
                string token = root.TryGetProperty("token", out JsonElement t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                return (user, password, token);
            }
            catch
            {
                return default;
            }
        }

        #endregion

        #endregion

        #region Methods

        [ACMethodInteraction("SqlTunnel", "en{'Start SQL Tunnel'}de{'SQL Tunnel starten'}", 200, true)]
        public override void StartService()
        {
            if (!IsEnabledStartService())
                return;

            StopService();

            try
            {
                var builder = WebApplication.CreateBuilder();
                builder.Logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Information;
                });

                X509Certificate2 tlsCertificate = null;
                if (UseTls)
                {
                    tlsCertificate = ACTlsCertificateHelper.GetOrCreateCertificate(TlsCertificatePath, TlsCertificatePassword, "SQL Tunnel");
                    if (tlsCertificate == null)
                        throw new InvalidOperationException("UseTls is enabled but no TLS certificate could be loaded or generated.");
                    // Explicit Listen binding (the certificate cannot be attached to a
                    // URL-based binding). RunAsync() is then called WITHOUT a URL so the
                    // Listen endpoint applies.
                    builder.WebHost.UseKestrel(k =>
                    {
                        k.Listen(IPAddress.Any, ServicePort, l => l.UseHttps(tlsCertificate));
                    });
                }

                WebApplication app = builder.Build();
                app.UseWebSockets();

                // Permissive CORS for browser WASM clients (sync XHR preflights).
                // Auth token via header, same gate as §4.2.
                app.Use(async (HttpContext context, Func<Task> next) =>
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    context.Response.Headers["Access-Control-Allow-Methods"] = "POST, DELETE, OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
                    if (String.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 204;
                        return;
                    }
                    await next();
                });

                // Stage 1 token issuer (v1 local IdP, docs §9.21): validates iPlus
                // credentials and issues an opaque short-lived bearer token. Production:
                // replace with a real IdP (open question §6.1).
                app.MapPost("/sqltunnel-auth/token", async (HttpContext context) =>
                {
                    string username = null, password = null;
                    try
                    {
                        using JsonDocument doc = await JsonDocument.ParseAsync(context.Request.Body);
                        username = doc.RootElement.TryGetProperty("username", out JsonElement u) && u.ValueKind == JsonValueKind.String ? u.GetString() : null;
                        password = doc.RootElement.TryGetProperty("password", out JsonElement p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
                    }
                    catch
                    {
                    }

                    if (!ValidateIPlusUser(username, password))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Invalid iPlus credentials.");
                        return;
                    }

                    byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
                    string token = Convert.ToHexString(tokenBytes);
                    _OAuthTokens[token] = DateTime.UtcNow + OAuthTokenLifetime;
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsJsonAsync(new { token, expiresIn = (int)OAuthTokenLifetime.TotalSeconds });
                });

                app.MapGet("/sqltunnel", async (HttpContext context) =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("SQL Tunnel: WebSocket connection required.");
                        return;
                    }

                    // Stage 1 on the handshake: reject before the socket is even accepted.
                    string bearerToken = GetBearerToken(context.Request.Headers);
                    if (RequireOAuth && !await ValidateBearerTokenAsync(bearerToken))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                        return;
                    }

                    using System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await RelayAsync(webSocket, bearerToken);
                });

                // SQL credential delivery (docs §9.24): authenticated clients fetch the
                // server-side SQL login here and put it into their connection string, so
                // no SQL password is stored client-side. Requires a valid bearer token.
                app.MapGet("/sqltunnel-sqlcred", async (HttpContext context) =>
                {
                    if (!SqlCredentialDeliveryEnabled)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("SQL Tunnel: credential delivery not configured.");
                        return;
                    }
                    if (RequireOAuth && !await ValidateBearerTokenAsync(GetBearerToken(context.Request.Headers)))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                        return;
                    }
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        System.Text.Json.JsonSerializer.Serialize(new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["user"] = SqlUser,
                            ["password"] = SqlPassword,
                        }));
                });

                MapSyncShimEndpoints(app);

                // Entra proxy (docs §9.26): Browser/WASM clients cannot call
                // login.microsoftonline.com directly — the device-code/token endpoints send
                // no CORS headers, so the browser blocks the fetch. Forward the two OAuth
                // POSTs (devicecode + token) through the relay. No auth required: this only
                // relays public OAuth endpoints and returns exactly what Entra returns.
                app.MapPost("/sqltunnel-entra-proxy/{**path}", async (HttpContext context, string path) =>
                {
                    using var reader = new System.IO.StreamReader(context.Request.Body);
                    string body = await reader.ReadToEndAsync();
                    using var client = new System.Net.Http.HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post,
                        "https://login.microsoftonline.com/" + path);
                    req.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8,
                        "application/x-www-form-urlencoded");
                    using var resp = await client.SendAsync(req);
                    context.Response.StatusCode = (int)resp.StatusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(await resp.Content.ReadAsStringAsync());
                });

                _Host = app;
                _CancellationTokenSource = new CancellationTokenSource();

                _SyncSessionCleanupTask = Task.Run(() => SyncSessionCleanupLoopAsync(_CancellationTokenSource.Token));

                _HostTask = Task.Run(async () =>
                {
                    try
                    {
                        if (tlsCertificate != null)
                        {
                            await _Host.RunAsync();
                        }
                        else
                        {
                            string strUri = String.Format("http://{0}:{1}", this.Root.Environment.UserInstance.Hostname, ServicePort);
                            await _Host.RunAsync(strUri);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                    }
                    catch (Exception ex)
                    {
                        Messages.LogException(GetACUrl(), "SqlTunnel Host Error", ex);
                        IsTunnelAlarm.ValueT = PANotifyState.AlarmOrFault;
                        ErrorText.ValueT = ex.Message;
                        OnNewAlarmOccurred(IsTunnelAlarm, "SQL Tunnel Host Error", true);
                    }
                });

                Messages.LogInfo(GetACUrl(), "StartSqlTunnel", String.Format("SQL Tunnel relay started on port {0} ({1}) -> {2}:{3}", ServicePort, tlsCertificate != null ? "https/wss" : "http/ws", SqlServer, SqlPort));
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "StartSqlTunnel", ex);
                IsTunnelAlarm.ValueT = PANotifyState.AlarmOrFault;
                ErrorText.ValueT = ex.Message;
                OnNewAlarmOccurred(IsTunnelAlarm, "StartSqlTunnel Error", true);
            }
        }

        public override bool IsEnabledStartService()
        {
            return _Host == null && IsEnabled && ServicePort > 0 && !String.IsNullOrEmpty(SqlServer);
        }

        [ACMethodInteraction("SqlTunnel", "en{'Stop SQL Tunnel'}de{'SQL Tunnel stoppen'}", 201, true)]
        public override void StopService()
        {
            if (!IsEnabledStopService())
                return;

            try
            {
                _CancellationTokenSource?.Cancel();
                _HostTask?.Wait(TimeSpan.FromSeconds(10));
                _Host = null;
                _CancellationTokenSource?.Dispose();
                _CancellationTokenSource = null;
                _HostTask = null;

                // Tear down all sync-XHR shim sessions.
                foreach (SyncSession session in _SyncSessions.Values)
                {
                    _SyncSessions.TryRemove(session.Id, out _);
                    session.Dispose();
                }

                Messages.LogInfo(GetACUrl(), "StopSqlTunnel", "SQL Tunnel relay stopped");
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "StopSqlTunnel", ex);
            }
        }

        public override bool IsEnabledStopService()
        {
            return _Host != null;
        }

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsTunnelAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsTunnelAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsTunnelAlarm);
            }
            base.AcknowledgeAlarms();
        }

        /// <summary>
        /// Blind byte relay between one WebSocket and one SQL Server TCP connection.
        /// No TDS parsing: order-preserving copy in both directions until either side closes.
        /// The TCP connection to SQL Server is opened ONLY after the two-stage auth
        /// (docs §9.21): stage 1 bearer token (handshake header or auth frame), stage 2
        /// iPlus login frame as the first WebSocket message.
        /// </summary>
        private async Task RelayAsync(System.Net.WebSockets.WebSocket webSocket, string bearerToken)
        {
            byte[] firstBuffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
            System.Net.WebSockets.WebSocketReceiveResult firstMessage;
            try
            {
                firstMessage = await webSocket.ReceiveAsync(new ArraySegment<byte>(firstBuffer), CancellationToken.None);
            }
            catch (Exception)
            {
                ArrayPool<byte>.Shared.Return(firstBuffer);
                await CloseWebSocketAsync(webSocket, System.Net.WebSockets.WebSocketCloseStatus.ProtocolError, "No auth frame received.");
                return;
            }

            // Stage 2: the first message is either the auth frame (fork sends it when the
            // connection string carries iplususer/ipluspass) or — for legacy clients without
            // credentials — the first TDS bytes.
            string authError = null;
            bool frameConsumed = false;
            (string frameUser, string framePassword, string frameToken) = ParseAuthFrame(firstBuffer, firstMessage.Count);
            if (frameUser != null || frameToken != null)
            {
                frameConsumed = true;
                if (RequireOAuth && !await ValidateBearerTokenAsync(frameToken ?? bearerToken))
                {
                    authError = "invalid OAuth token";
                }
                // Stage 2 (iPlus login) only applies when the frame carries iPlus
                // credentials. A token-only frame (Entra ID mode — the JWT is the
                // identity) skips stage 2; user mapping to VBUser is a future step.
                // An EMPTY user counts as absent (the fork always serializes "user":"").
                if (authError == null && !String.IsNullOrEmpty(frameUser) && !ValidateIPlusUser(frameUser, framePassword))
                {
                    authError = "iPlus login failed";
                }
            }
            else if ((RequireIPlusLogin || RequireOAuth) && String.IsNullOrEmpty(bearerToken))
            {
                // No auth frame AND no bearer token validated at the handshake (e.g. the
                // ?authtoken= query parameter) — nothing to authenticate with.
                authError = "auth frame required";
            }
            if (authError != null)
            {
                Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"WebSocket relay rejected: {authError}");
                ArrayPool<byte>.Shared.Return(firstBuffer);
                await CloseWebSocketAsync(webSocket, System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation, "SQL Tunnel auth failed: " + authError);
                return;
            }

            string target = String.Format("{0}:{1}", SqlServer, SqlPort);
            using TcpClient tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(SqlServer, SqlPort);
            }
            catch (Exception ex)
            {
                ArrayPool<byte>.Shared.Return(firstBuffer);
                Messages.LogException(GetACUrl(), "SqlTunnelRelay", ex);
                await CloseWebSocketAsync(webSocket, System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, "Cannot reach SQL Server " + target);
                return;
            }

            NetworkStream tcpStream = tcp.GetStream();
            // IMPORTANT: each direction needs its OWN buffer. The two pumps run
            // concurrently; sharing one ArrayPool buffer corrupted TDS in both
            // directions (observed 2026-10-14: Android/desktop WS path failed with
            // "error during the pre-login handshake" while the browser shim worked).
            byte[] wsBuffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
            byte[] tcpBuffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
            try
            {
                // Legacy path: the first message was TDS bytes, not the auth frame — forward it.
                try
                {
                    if (!frameConsumed && firstMessage.Count > 0)
                    {
                        await tcpStream.WriteAsync(firstBuffer, 0, firstMessage.Count);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(firstBuffer);
                }

                // NOTE (docs §9.23): server-side SQL credential injection into the TDS
                // LOGIN7 packet was investigated and is NOT possible — SQL Server always
                // encrypts the login packet (TLS via SslOverTdsStream), even with
                // Encrypt=False. The relay is a plain byte proxy and never sees the
                // credentials; the client must keep them (protected end-to-end by TLS).

                Task wsToTcp = PumpWebSocketToTcpAsync(webSocket, tcpStream, wsBuffer);
                Task tcpToWs = PumpTcpToWebSocketAsync(tcpStream, webSocket, tcpBuffer);

                await Task.WhenAny(wsToTcp, tcpToWs);

                // Tear down both directions; SQL Server rolls back open transactions on close.
                try { tcpStream.Close(); } catch { }
                await CloseWebSocketAsync(webSocket, System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, null);
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "SqlTunnelRelay", ex);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(wsBuffer);
                ArrayPool<byte>.Shared.Return(tcpBuffer);
            }
        }

        private async Task PumpWebSocketToTcpAsync(System.Net.WebSockets.WebSocket webSocket, NetworkStream tcpStream, byte[] buffer)
        {
            try
            {
                while (!webSocket.CloseStatus.HasValue)
                {
                    System.Net.WebSockets.WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        break;
                    }
                    if (result.Count > 0)
                    {
                        await tcpStream.WriteAsync(buffer, 0, result.Count);
                    }
                }
            }
            catch (Exception ex) when (ex is System.Net.WebSockets.WebSocketException
                                            or System.IO.IOException
                                            or System.Net.Sockets.SocketException
                                            or ObjectDisposedException
                                            or OperationCanceledException)
            {
                // Client dropped the connection (abrupt close, aborted TCP, close without
                // handshake) — this is the normal end of a tunnel session, not an error.
            }
        }

        private async Task PumpTcpToWebSocketAsync(NetworkStream tcpStream, System.Net.WebSockets.WebSocket webSocket, byte[] buffer)
        {
            try
            {
                while (true)
                {
                    int read = await tcpStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0)
                    {
                        break;
                    }
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, read), System.Net.WebSockets.WebSocketMessageType.Binary, endOfMessage: true, CancellationToken.None);
                }
            }
            catch (Exception ex) when (ex is System.Net.WebSockets.WebSocketException
                                            or System.IO.IOException
                                            or System.Net.Sockets.SocketException
                                            or ObjectDisposedException
                                            or OperationCanceledException)
            {
                // SQL Server closed the TCP connection or the WebSocket was torn down by the
                // other pump finishing first — normal end of a tunnel session, not an error.
            }
        }

        private static async Task CloseWebSocketAsync(System.Net.WebSockets.WebSocket webSocket, System.Net.WebSockets.WebSocketCloseStatus status, string description)
        {
            try
            {
                if (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    await webSocket.CloseAsync(status, description, CancellationToken.None);
                }
            }
            catch
            {
                // best effort
            }
        }

        #endregion


        #region HTTP Sync Shim (browser WASM clients)

        /// <summary>
        /// Registers the four sync-XHR shim endpoints used by browser WASM clients
        /// (see docs/SqlTunnel-OptionB-Implementation.md §7.2). The browser performs the
        /// wait internally via synchronous XMLHttpRequest; the relay keeps one live SQL
        /// socket per session and never parses TDS.
        /// </summary>
        private void MapSyncShimEndpoints(WebApplication app)
        {
            // Stage 1 gate for every shim request (docs §9.21). The session endpoint also
            // accepts the token in its JSON body; send/recv/delete require the header.
            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                if (context.Request.Path.StartsWithSegments("/sqltunnel-sync") &&
                    !String.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase) &&
                    RequireOAuth &&
                    !await ValidateBearerTokenAsync(GetBearerToken(context.Request.Headers)))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                    return;
                }
                await next();
            });

            // POST /sqltunnel-sync/session -> validate auth, open TcpClient to SQL Server,
            // return {sessionId}. Body: JSON {"user","password","token"} (stage 2 + optional
            // stage-1 token); empty body = legacy unauthenticated session.
            app.MapPost("/sqltunnel-sync/session", async (HttpContext context) =>
            {
                string bodyUser = null, bodyPassword = null, bodyToken = null;
                if (context.Request.ContentLength > 0)
                {
                    try
                    {
                        using JsonDocument doc = await JsonDocument.ParseAsync(context.Request.Body);
                        bodyUser = doc.RootElement.TryGetProperty("user", out JsonElement u) && u.ValueKind == JsonValueKind.String ? u.GetString() : null;
                        bodyPassword = doc.RootElement.TryGetProperty("password", out JsonElement p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
                        bodyToken = doc.RootElement.TryGetProperty("token", out JsonElement t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                    }
                    catch
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("SQL Tunnel: invalid session body.");
                        return;
                    }
                }

                // Stage 1: Authorization header OR token from the body.
                if (RequireOAuth && !await ValidateBearerTokenAsync(GetBearerToken(context.Request.Headers) ?? bodyToken))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                    return;
                }

                // Stage 2: iPlus login BEFORE the TCP socket to SQL Server is opened. An
                // auth frame is always validated when present; RequireIPlusLogin makes it
                // mandatory. NOTE: the browser shim always serializes "user":"" in the
                // session body, so an EMPTY user counts as "absent" — otherwise Entra
                // mode (JWT-only, no iPlus credentials) would always fail here.
                if (!String.IsNullOrEmpty(bodyUser) || RequireIPlusLogin)
                {
                    if (!ValidateIPlusUser(bodyUser, bodyPassword))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("SQL Tunnel: iPlus login failed.");
                        return;
                    }
                }
                string sessionId = Guid.NewGuid().ToString("N");
                TcpClient tcp = new TcpClient();
                try
                {
                    using CancellationTokenSource connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    await tcp.ConnectAsync(SqlServer, SqlPort, connectCts.Token);
                }
                catch (Exception ex)
                {
                    tcp.Dispose();
                    Messages.LogException(GetACUrl(), "SyncShimSession", ex);
                    context.Response.StatusCode = 502;
                    await context.Response.WriteAsync("Cannot reach SQL Server " + SqlServer + ":" + SqlPort);
                    return;
                }

                SyncSession session = new SyncSession(sessionId, tcp);
                _SyncSessions[sessionId] = session;
                Messages.LogInfo(GetACUrl(), "SyncShim", String.Format("Session {0} created -> {1}:{2}", sessionId, SqlServer, SqlPort));
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(new { sessionId });
            });

            // POST /sqltunnel-sync/{id}/send -> write body bytes to the SQL socket
            app.MapPost("/sqltunnel-sync/{id}/send", async (HttpContext context, string id) =>
            {
                if (!_SyncSessions.TryGetValue(id, out SyncSession session))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                await session.Lock.WaitAsync();
                try
                {
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
                    int total = 0;
                    byte[] firstBytes = new byte[32];
                    try
                    {
                        int read;
                        while ((read = await context.Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            if (total < firstBytes.Length)
                            {
                                int copy = Math.Min(read, firstBytes.Length - total);
                                Buffer.BlockCopy(buffer, 0, firstBytes, total, copy);
                            }
                            await session.Stream.WriteAsync(buffer, 0, read);
                            total += read;
                        }
                        await session.Stream.FlushAsync();
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                    session.LastAccess = DateTime.UtcNow;
                }
                catch (Exception)
                {
                    Messages.LogInfo(GetACUrl(), "SyncShim", String.Format("Session {0}: send failed, socket dead", id));
                    RemoveSession(session);
                    context.Response.StatusCode = 410; // Gone: SQL socket is dead, client must re-create session
                    return;
                }
                finally
                {
                    session.Lock.Release();
                }

                context.Response.StatusCode = 200;
            });

            // POST /sqltunnel-sync/{id}/recv -> long-poll next chunk; 200 body = TDS bytes,
            // 204 = no data within hold time (client retries), 410 = session/socket dead.
            app.MapPost("/sqltunnel-sync/{id}/recv", async (HttpContext context, string id) =>
            {
                if (!_SyncSessions.TryGetValue(id, out SyncSession session))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                await session.Lock.WaitAsync();
                try
                {
                    // Keep the pending read across 204 retries so no bytes are ever lost:
                    // an abandoned ReadAsync would consume data into a buffer we discard.
                    if (session.PendingRead == null)
                    {
                        session.PendingRead = session.Stream.ReadAsync(session.ReadBuffer, 0, session.ReadBuffer.Length);
                    }

                    Task completed = await Task.WhenAny(session.PendingRead, Task.Delay(RecvHoldTime));
                    if (completed != session.PendingRead)
                    {
                        context.Response.StatusCode = 204; // no data yet; client retries
                        return;
                    }

                    int count = await session.PendingRead;
                    session.PendingRead = null;
                    session.LastAccess = DateTime.UtcNow;

                    // Coalesce: drain whatever else is already buffered in the socket so each
                    // poll returns a FULL chunk. Each /recv is a blocking sync XHR on the
                    // WASM main thread, so round trips dominate throughput for large result
                    // sets (e.g. ACClassPropertyList). DataAvailable never blocks; if no more
                    // data is buffered the chunk is simply smaller.
                    while (count < session.ReadBuffer.Length && session.Stream.DataAvailable)
                    {
                        int more = await session.Stream.ReadAsync(session.ReadBuffer, count, session.ReadBuffer.Length - count);
                        if (more <= 0)
                            break;
                        count += more;
                    }

                    if (count == 0)
                    {
                        RemoveSession(session);
                        context.Response.StatusCode = 410; // SQL Server closed the connection
                        return;
                    }

                    context.Response.StatusCode = 200;
                    // Base64 text: synchronous XHR in window context only supports the ''
                    // and 'text' response types (no 'arraybuffer'), so the browser client
                    // cannot consume a raw binary response body.
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(Convert.ToBase64String(session.ReadBuffer, 0, count));
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), "SyncShimRecv", ex);
                    RemoveSession(session);
                    context.Response.StatusCode = 410;
                    return;
                }
                finally
                {
                    session.Lock.Release();
                }
            });

            // DELETE /sqltunnel-sync/{id} -> close SQL socket, drop session
            app.MapDelete("/sqltunnel-sync/{id}", (HttpContext context, string id) =>
            {
                if (_SyncSessions.TryRemove(id, out SyncSession session))
                {
                    session.Dispose();
                }
                context.Response.StatusCode = 200;
                return Task.CompletedTask;
            });
        }

        private void RemoveSession(SyncSession session)
        {
            if (_SyncSessions.TryRemove(session.Id, out _))
            {
                session.Dispose();
            }
        }

        /// <summary>Periodically closes shim sessions idle beyond <see cref="SyncSessionIdleTimeout"/>.</summary>
        private async Task SyncSessionCleanupLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                DateTime now = DateTime.UtcNow;
                foreach (SyncSession session in _SyncSessions.Values)
                {
                    if (now - session.LastAccess > SyncSessionIdleTimeout)
                    {
                        RemoveSession(session);
                    }
                }
            }
        }

        /// <summary>
        /// One browser sync-XHR session: one live SQL Server TCP socket plus a persistent
        /// receive buffer. The pending read is kept across 204 responses so no TDS bytes
        /// are lost between long-poll retries.
        /// </summary>
        private sealed class SyncSession : IDisposable
        {
            public SyncSession(string id, TcpClient tcp)
            {
                Id = id;
                Tcp = tcp;
                Stream = tcp.GetStream();
                // 64 KB per poll: each /recv is a blocking sync XHR on the browser main
                // thread, so fewer/larger chunks directly translate into higher throughput.
                // Must stay in sync with SyncHttpTunnelStream.MaxChunkSize (SqlClient fork).
                ReadBuffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
                LastAccess = DateTime.UtcNow;
            }

            public string Id { get; }
            public TcpClient Tcp { get; }
            public NetworkStream Stream { get; }
            public byte[] ReadBuffer { get; }
            public Task<int> PendingRead { get; set; }
            public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);
            public DateTime LastAccess { get; set; }

            public void Dispose()
            {
                try { Stream.Close(); } catch { }
                try { Tcp.Close(); } catch { }
                if (ReadBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(ReadBuffer);
                }
                Lock.Dispose();
            }
        }

        #endregion
    }
}
