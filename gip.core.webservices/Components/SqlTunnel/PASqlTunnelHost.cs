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
            _IsEnabled = new ACPropertyConfigValue<bool>(this, "IsEnabled", false);
            _SqlServer = new ACPropertyConfigValue<string>(this, "SqlServer", "");
            _SqlPort = new ACPropertyConfigValue<int>(this, "SqlPort", 1433);
            _RequireOAuth = new ACPropertyConfigValue<bool>(this, "RequireOAuth", false);
            _RequireIPlusLogin = new ACPropertyConfigValue<bool>(this, "RequireIPlusLogin", false);
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
        /// Production upgrade path: replace this method with real IdP validation
        /// (JWT bearer via Microsoft.Identity.Web) — open question §6.1.
        /// </summary>
        private static bool ValidateBearerToken(string token)
        {
            if (String.IsNullOrEmpty(token))
                return false;
            if (_OAuthTokens.TryGetValue(token, out DateTime expiry))
            {
                if (expiry > DateTime.UtcNow)
                    return true;
                _OAuthTokens.TryRemove(token, out _);
            }
            return false;
        }

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
                        // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                        Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Token request rejected for user '{username}'");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Invalid iPlus credentials.");
                        return;
                    }

                    byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
                    string token = Convert.ToHexString(tokenBytes);
                    _OAuthTokens[token] = DateTime.UtcNow + OAuthTokenLifetime;
                    // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Token issued for user '{username}' (valid {OAuthTokenLifetime.TotalHours}h)");
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
                    if (RequireOAuth && !ValidateBearerToken(bearerToken))
                    {
                        // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                        Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", "WebSocket handshake rejected: missing/invalid bearer token");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                        return;
                    }

                    using System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await RelayAsync(webSocket, bearerToken);
                });

                MapSyncShimEndpoints(app);

                _Host = app;
                _CancellationTokenSource = new CancellationTokenSource();

                _SyncSessionCleanupTask = Task.Run(() => SyncSessionCleanupLoopAsync(_CancellationTokenSource.Token));

                _HostTask = Task.Run(async () =>
                {
                    try
                    {
                        string strUri = String.Format("http://{0}:{1}", this.Root.Environment.UserInstance.Hostname, ServicePort);
                        await _Host.RunAsync(strUri);
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

                Messages.LogInfo(GetACUrl(), "StartSqlTunnel", String.Format("SQL Tunnel relay started on port {0} -> {1}:{2}", ServicePort, SqlServer, SqlPort));
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
                if (RequireOAuth && !ValidateBearerToken(frameToken ?? bearerToken))
                {
                    authError = "invalid OAuth token";
                }
                if (authError == null && !ValidateIPlusUser(frameUser, framePassword))
                {
                    authError = "iPlus login failed";
                }
                // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"WebSocket auth frame for user '{frameUser}': {(authError == null ? "OK" : authError)}");
            }
            else if (RequireIPlusLogin || (RequireOAuth && String.IsNullOrEmpty(bearerToken)))
            {
                authError = "auth frame required";
            }

            if (authError != null)
            {
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
            byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
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

                Task wsToTcp = PumpWebSocketToTcpAsync(webSocket, tcpStream, buffer);
                Task tcpToWs = PumpTcpToWebSocketAsync(tcpStream, webSocket, buffer);

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
                ArrayPool<byte>.Shared.Return(buffer);
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
                    !ValidateBearerToken(GetBearerToken(context.Request.Headers)))
                {
                    // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Shim request {context.Request.Path} rejected: missing/invalid bearer token");
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
                if (RequireOAuth && !ValidateBearerToken(GetBearerToken(context.Request.Headers) ?? bodyToken))
                {
                    // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", "Shim session rejected: missing/invalid bearer token");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("SQL Tunnel: invalid or missing bearer token.");
                    return;
                }

                // Stage 2: iPlus login BEFORE the TCP socket to SQL Server is opened. An
                // auth frame is always validated when present; RequireIPlusLogin makes it
                // mandatory.
                if (bodyUser != null || RequireIPlusLogin)
                {
                    if (!ValidateIPlusUser(bodyUser, bodyPassword))
                    {
                        // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                        Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Shim session rejected: iPlus login failed for user '{bodyUser}'");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("SQL Tunnel: iPlus login failed.");
                        return;
                    }
                    // TEMPORARY diagnostics: remove with the auth-tracing cleanup.
                    Messages.LogInfo(GetACUrl(), "SqlTunnelAuth", $"Shim session: iPlus login OK for user '{bodyUser}'");
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
                    // TEMPORARY diagnostics: TDS header + login7 option flags of the first send.
                    string headerHex = BitConverter.ToString(firstBytes, 0, Math.Min(32, total));
                    Messages.LogInfo(GetACUrl(), "SyncShim", String.Format("Session {0}: sent {1} bytes to SQL socket, first bytes [{2}]", id, total, headerHex));
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
                    // TEMPORARY diagnostics: TDS packet header (type, status, length) of the chunk.
                    string headerHex = BitConverter.ToString(session.ReadBuffer, 0, Math.Min(8, count));
                    ushort tdsLen = count >= 8 ? (ushort)((session.ReadBuffer[6] << 8) | session.ReadBuffer[7]) : (ushort)0;
                    Messages.LogInfo(GetACUrl(), "SyncShim", String.Format("Session {0}: received {1} bytes from SQL socket, header [{2}], TDS packet length {3}", id, count, headerHex, tdsLen));
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
                ReadBuffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
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
