# Threat model — iPlus (iplus-framework)

## What this project is

iPlus is an open-source Manufacturing Execution System (MES) and process-control
framework for industrial companies (GPL-3.0-or-later, .NET 10, C#). It exposes
server components over a company network and talks to industrial equipment.
It is used by production machines and control systems, so vulnerabilities can
have physical-world impact.

## Where untrusted input enters

Treat all of the following as adversary-controlled unless they require an
authenticated session:

- The WCF/web services (`gip.core.webservices`, HTTP listeners in the service
  and console hosts, typically ports 8750/8751/8001). Remote without
  authentication where anonymous access is configured. The MCP server host
  (`PAMcpServerHost`, port 8750) and the SQL tunnel host
  (`PASqlTunnelHost`, port 8751) run on HTTP/Kestrel and are covered in
  their own bullets below.
- The MCP Server host (`PAMcpServerHost`, `gip.core.webservices/Components/MCP`,
  Kestrel/ASP.NET **HTTP** endpoint, configured port 8750, default config 0 =
  disabled): a Model Context Protocol server that exposes iPlus tools
  (`MCPIPlusTools`, `MCPIPlusDevTools`, `MCPIPlusPrompts`) to LLM agents —
  thesaurus/type/instance-tree navigation plus AC-method execution helpers.
  Every request must authenticate via the `Authorization` header in
  `username#password` form (`ApiKeyAuthenticationHandler` /
  `ApiKeyValidator` -> iPlus `CheckLogin`), and every tool call runs with the
  rights of that iPlus user (`VBUserContextService` session). Attack surface:
  the credential parsing in `ApiKeyAuthorizationFilter`/`ApiKeyValidator`
  (separator handling, array indexes, error paths), the port-based
  `FindPAWebService` host resolution (local-port confusion between hosts),
  per-request session add/cleanup (`AddSession`/`RemoveSession`,
  `CleanupExpiredSessions`), the tool input handling in
  `MCPToolBase`/`MCPToolAppTree` (attacker-controlled strings reach the AC
  query methods and the application tree), and the fact that the endpoint
  itself is plain HTTP (credentials and tool payloads are transmitted
  unencrypted — TLS termination is expected to happen in front of it;
  transport-level interception therefore depends on deployment, but logic
  flaws in auth and tool handling are always in scope).
- The SQL Tunnel host (`PASqlTunnelHost`, `gip.core.webservices/Components/SqlTunnel`,
  WebSocket endpoint `/sqltunnel` on port 8751): browser and mobile clients
  relay raw TDS database traffic through it after a two-stage auth (OAuth
  bearer token in the WebSocket handshake plus the iPlus login). It is a
  byte relay between the WebSocket and a TcpClient to SQL Server. Attack
  surface: the OAuth token validation (`RequireOAuth` / `TokenIssuer`
  Entra-vs-issuer modes, `ValidateToken` before any byte is relayed), the
  WebSocket handshake upgrade handling, the connection-string resolution
  done server-side (credentials must never leak into the tunnel), and the
  relay loop itself (partial reads, connection cleanup, resource exhaustion
  per authenticated user). Any bypass that lets an unauthenticated client
  reach the byte relay is critical. Docs: `docs/SqlTunnel-EndUser-Setup.md`,
  `docs/SqlTunnel-OptionB-Implementation.md`.
- The EF Core / SQL access layer (`gip.core.datamodel`, `gip.core.sql`,
  `gip.core.dbsyncer`): SQL injection through report/AC-method queries, dynamic
  SQL and script generation.
- XML / file deserialization: AC method definitions (XML), report templates
  (Scryber/XSLT), imported license files (`gip.core.licensing` — signature
  verification is security critical), VBControl script files, uploaded media
  (`gip.core.media`).
- The licensing subsystem: license files are untrusted attacker input when
  placed by a user; anything that skips signature checks is at least high.

## Components that matter most / least

Most important (server side, in scope):

- `gip.core.webservices`, `gip.core.sql`, `gip.core.dbsyncer`,
  `gip.core.datamodel`, `gip.core.autocomponent` (AC component engine and XML
  method execution), `gip.core.licensing`, `gip.core.archiver`,
  `gip.core.reporthandler`, `gip.core.processapplication`,
  `gip.iplus.service`, `gip.iplus.console`, `gip.iplus.startup`.
- Inside `gip.core.webservices` the individually hosted endpoints matter
  most: the SQL Tunnel host (`PASqlTunnelHost`), the MCP Server host
  (`PAMcpServerHost` — LLM-agent access to the application tree with iPlus
  user rights), and the JSON/REST service hosts with their authorization
  managers (`ApiKeyAuthorizationManager`, `OAuthBearerAuthorizationManager`,
  `WSRestAuthorizationManager`).
- MES business objects in the `iplus-framework/iPlusMES` repository (enrolled
  as a separate project) build on top of these.

Less important but still in scope: client-side code (`gip.iplus.client*`,
Avalonia/WPF/browser UI) — report cross-site scripting in the browser client
and unsafe deserialization in any client.

Out of scope:

- `gip.ext.*` components are vendored/forked third-party libraries
  (charting, OPC toolbox, designer, XAML DOM). Only report issues in the code
  we changed versus upstream, in issues reachable from iPlus code.
- The legacy V4 database schema (`Database/*.bak` contents) itself.

## How to exercise it

- A headless runtime is provided: `gip.iplus.console` starts the full iPlus
  runtime (database, AC root, web services) without UI. It is documented in
  `gip.iplus.console/readme.md`.
- `.oss-scanner/run-console.sh` starts a local SQL Server (pre-installed in the
  image), restores the sample database from `Database/iPlusV5.7z`, writes
  `vbiplus.config` and runs the console with login user `00` / password `00`.
- Unit tests: `gip.core.licensing.unit.test` (runs offline).
- Log output goes to stdout and to `Logs/iPlusConsoleDebugLog_*.txt`.

The image needs at least ~2 GB of RAM because SQL Server runs inside it.

## How you rate severity

- Critical: unauthenticated remote code execution (including via XML
  deserialization, script/AC-method execution, or report engines), authentication
  bypass, or anything that escapes the service host process.
- High: SQL injection reachable through web services, unsafe deserialization
  of untrusted XML, path traversal / arbitrary file read-write in any server
  component, license signature bypass, hardcoded credentials that ship to
  production.
- Medium: authenticated information disclosure, SSRF, DoS of the service host
  through a crash or unhandled exception in a listener loop.
- Low: client-side issues that require a malicious local user.

## Anything to leave alone

- Do not report the dev database credentials found in committed
  `ConnectionStrings.config` / `App.config` / documentation examples
  (`gip`/`netspirit`, `00`/`00`, `superuser`) — they are sample/dev values.
  A hardcoded *production* credential in product code would still be in scope.
- Do not report the weak TLS posture of self-signed development certificates,
  or the fact that `Encrypt=False` appears in sample connection strings.
- Do not report style/performance issues; vulnerabilities only.
