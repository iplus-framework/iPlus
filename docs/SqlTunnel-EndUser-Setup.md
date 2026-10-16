# SQL Tunnel — Setup Guide for End Users

This guide explains how to set up the **SQL Tunnel** (relay) in iPlus so that a
browser or Android client can reach the SQL Server through the iPlus server —
including TLS encryption and Microsoft Entra ID login.

No programming knowledge is required. You only need:

- Access to the iPlus **Studio** (configuration application)
- Administrator rights on the machine running the iPlus server
- Access to the **Azure Portal** (only if you want Entra ID login)

---

## 1. Overview: What gets installed where

```
Browser / Android client
        |  wss:// (TLS) + Entra token
        v
+---------------------------+        +---------------------------+
| iPlus Server              |        | SQL Server                |
|  - PASqlTunnelHost :8751  | -----> |  gipDLVmSQL1:1433         |
|  - WCF host        :8000  |  TCP   +---------------------------+
+---------------------------+
```

| Component | Where it lives | What it does |
|---|---|---|
| `PASqlTunnelHost` | iPlus Studio, child of the **Service** project (`\Service\PASqlTunnelHost`) | Relay: accepts client connections (wss), forwards raw SQL traffic to the SQL Server, issues login tokens |
| `BSOiPlusNetConfig` | iPlus Studio, iPlus network configuration BSO | Edits the **service user instance** record in the database (WCF host address, TLS settings) |
| Settings grid | Browser/Android client, login screen | Client-side connection settings (server URL, tunnel URL, Entra login) |
| Configuration file | Desktop client (`ConnectionStrings.config` / app settings) | Same settings for the desktop client |
| Entra App registration | Azure Portal | Provides the **Client ID** and **Tenant ID** used for login |

---

## 2. Add the SQL Tunnel Host in iPlus Studio

1. Start the iPlus **Studio** and log in (e.g. `superuser`).
2. Open the application tree and navigate to the **Service** project
   (typically `\Service`).
3. Add a new child component of type **SQL Tunnel Host** (`PASqlTunnelHost`)
   to the Service project. The resulting URL must be
   `\Service\PASqlTunnelHost`.
4. Save.

> The tunnel host is started automatically with the iPlus server
> (it starts in `ACPostInit` when *IsEnabled* is checked).

---

## 3. Configure the Tunnel Host (Configuration tab)

Select `\Service\PASqlTunnelHost` in the tree and open its **Configuration**
tab. Enter the following values:

| Configuration value | Meaning | Example |
|---|---|---|
| **IsEnabled** | Turns the relay on | ✔ checked |
| **SqlServer** | Host name of the SQL Server the relay forwards to | `gipDLVmSQL1.incus` |
| **SqlPort** | SQL Server TCP port | `1433` |
| **UseTls** | Serve the relay with HTTPS/WSS (recommended) | ✔ checked |
| **TlsCertificatePath** | Path to a PFX certificate file. **Leave empty** to auto-generate a self-signed certificate next to the server executable (`iplus-relay-tls.pfx`) | *(empty)* |
| **TlsCertificatePassword** | Password of the PFX. Leave empty when auto-generating (default `iplus`) | *(empty)* |
| **RequireOAuth** | Require a login token on every connection | ✔ checked (recommended) |
| **TokenIssuer** | `Local` (iPlus users) or `Entra` (Microsoft Entra ID) | `Entra` |
| **EntraTenantId** | Your Entra **Tenant ID** (see section 6) | `organizations` or a GUID |
| **EntraAudience** | The **Client ID** of your app registration (see section 6) | `03a3ca8b-…` |
| **EntraInstance** | Cloud instance; leave empty for global Microsoft cloud | *(empty)* |
| **SqlUser / SqlPassword** | SQL login that the relay **delivers** to authenticated clients (so no SQL password is stored on the client). Optional — if empty, the client uses its own credentials | *(optional)* |
| **RequireIPlusLogin** | Additionally require an iPlus user/password frame | optional |

> **TLS certificate:** with an empty path the server generates a self-signed
> certificate on first start (valid ~13 months, hostname + localhost as
> subject names). Clients must trust it — see section 7. For production,
> install a proper certificate and enter its PFX path here.

**Restart the iPlus server** after changing these values — the relay is built
during startup.

---

## 4. Configure the WCF host (service user instance)

The WCF service host (port 8000) is configured **per service user instance in
the database**, edited through the **BSOiPlusNetConfig** BSO:

1. Open the iPlus network configuration (`BSOiPlusNetConfig`) in Studio.
2. Select the **service user instance** used by remote clients (e.g. `00`).
3. Set:
   - **Service URL / host name** — the DNS name of the server, e.g.
     `gipsoft-dlmhp-26` (must match the TLS certificate name!).
   - **ServiceUseTLS** — check this to serve WCF over HTTPS/WSS.
   - **ServiceTlsCertificatePath / Password** — leave **empty** to reuse the
     same shared certificate as the relay (`iplus-relay-tls.pfx`).
4. Save and **restart the iPlus server** (the WCF host is rebuilt at startup).

---

## 5. Client settings

### 5.1 Browser / Android (Settings grid)

On the login screen open the **settings** (gear icon) and fill in:

| Field | Value |
|---|---|
| Server (WCF) | `gipsoft-dlmhp-26:8000` (the name from section 4) |
| SQL Tunnel URL | `wss://gipsoft-dlmhp-26:8751/sqltunnel` |
| Entra login | enable, if `TokenIssuer = Entra` |

The client automatically uses `wss://`/`https://` when the server speaks TLS —
no extra switch needed on the client.

> **Note for browser clients:** environment variables do **not** reach the
> browser runtime. All settings must come from the settings grid or the
> server-delivered configuration.

### 5.2 Desktop client (configuration file)

For the desktop client, edit the configuration file next to the executable
(`ConnectionStrings.config` / application settings) and set the same values:
WCF server address, tunnel URL (`wss://…`), and Entra login options.

---

## 6. Microsoft Entra ID: create Client ID and Tenant ID

You need an **App registration** in the Azure Portal. One-time setup:

1. Sign in to <https://portal.azure.com> with an account that may create
   app registrations.
2. **Microsoft Entra ID → App registrations → New registration**
   - Name: e.g. `iPlus SQL Tunnel`
   - **Supported account types:**
     - *Single tenant* — only your own organization.
     - *Accounts in any organizational directory (multitenant)* — choose this
       if different customer tenants should log in (then use
       `organizations` as the **EntraTenantId** value).
   - Redirect URI: **Public client/native (mobile & desktop)** — needed for
     the device-code login flow. No secret is required.
3. After registering, copy from the **Overview** page:
   - **Application (client) ID** → this is the **EntraAudience** value in
     section 3.
   - **Directory (tenant) ID** → this is the **EntraTenantId** value
     (or use `organizations` for multitenant).
4. **API permissions**: none special are needed for pure identity tokens.
   If you later restrict access, you can add an *App role* and assign users.
5. **Authentication → Advanced settings → Allow public client flows: Enable**
   (required for the device-code flow shown on the login screen).

Enter the two IDs into the tunnel host configuration (section 3) and restart
the server.

**How the login works for the user:** on the client login screen the user
chooses Entra login; a code is displayed (e.g. `ABCD-1234`) together with a
URL (`https://microsoft.com/devicelogin`). The user opens the URL on any
device, enters the code, and signs in with their Microsoft account. The client
then continues automatically.

---

## 7. Trust the self-signed certificate (TLS)

If you use the auto-generated self-signed certificate, each machine running a
**desktop or Android client** must trust it once:

1. Copy the file `iplus-relay-tls.pfx` **certificate** (the `.crt`/public key
   part) from the server to the client machine.
2. **Windows:** double-click the `.crt` → *Install Certificate* →
   *Local Machine* → *Trusted Root Certification Authorities*.
3. **Linux:** copy to `/usr/local/share/ca-certificates/iplus-relay-tls.crt`
   and run `sudo update-ca-certificates`.
4. **Android:** *Settings → Security → Install a certificate → CA certificate*.

**Browser clients** use the browser's own trust store — the browser must be
started with the system trust store, or the certificate must be imported into
the browser/OS trust store. If the certificate is not trusted, the browser
blocks the connection with a certificate warning that cannot be bypassed for
background requests.

> Alternative for testing only: start the desktop client with the environment
> variable `IPLUS_ACCEPT_UNTRUSTED_CERT=1` to accept untrusted relay
> certificates. This is **not** recommended for production and does not work
> in the browser.

---

## 8. Checklist: bring it all up

1. ☐ `PASqlTunnelHost` added under `\Service` in Studio.
2. ☐ Configuration values set (SqlServer, SqlPort, UseTls, TokenIssuer,
     EntraTenantId, EntraAudience).
3. ☐ Service user instance configured in `BSOiPlusNetConfig`
     (host name, ServiceUseTLS).
4. ☐ Entra app registration created; Client ID + Tenant ID entered.
5. ☐ iPlus server **restarted**.
6. ☐ Certificate trusted on client machines (section 7).
7. ☐ Client settings entered (section 5).
8. ☐ Test: open the client, log in via Entra, check that the database
     connection works (green connection icon).

### Quick verification on the server

After restart, the server log should contain:

```
SQL Tunnel relay started on port 8751 (https/wss) -> <SqlServer>:<SqlPort>
```

If instead an alarm appears on the tunnel host component, check the error
text on the component (usually a certificate or port problem).

---

## 9. Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| Relay does not start | `IsEnabled` not checked, port occupied, or `SqlServer` empty — check the Configuration tab and the server log |
| Certificate error in browser | Self-signed certificate not trusted — section 7, or install a proper certificate |
| Login fails with 401 | Wrong `EntraAudience`/`EntraTenantId`, or the token expired — re-login on the client |
| "iPlus login failed" | Stage-2 iPlus user check failed — verify the iPlus user name/password |
| Client shows red connection icon | WCF host not reachable — check service user instance host name and `ServiceUseTLS`, restart server after DB changes |
| SQL connection fails after login | Wrong `SqlServer`/`SqlPort` on the relay, or SQL Server blocks the server's IP |
| Changes have no effect | Both the relay and the WCF host are built at server startup — **restart the iPlus server** after any configuration change |
