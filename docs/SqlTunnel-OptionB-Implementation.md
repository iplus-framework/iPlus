# Implementation: SQL Tunnel via Forked SqlClient (Option B′)

> Status: **Implementation plan** — kickoff for the implementation session.
> Created: 2026-10-09. Supersedes §2b of `SqlTunnelProvider-Design-Draft.md` (which remains the
> decision record incl. security model and evaluated alternatives).
> Decision context: browser WASM has no raw TCP sockets; `Microsoft.Data.SqlClient` refuses to
> run on browser. We fork SqlClient, add a WebSocket transport for the browser TFM, and relay
> the raw TDS bytes to SQL Server through a gateway hosted by an iPlus ACComponent.

## 0. Current State (2026-10-09, updated after baseline rework)

- SqlClient fork cloned: `/home/damir/SHARED/Devel/iPlusGit/V5/SqlClient`
  - Remote: `git@github.com:iplus-framework/SqlClient.git`
  - Branch: `iplus/browser-transport` — **rebased onto `origin/release/7.1`** (v7.1.1 + 2 commits)
    per decision: iPlus already ran SqlClient 7.0.0-preview3, so the 7.x line is the correct
    baseline. Baseline package version: **`7.1.1-iplus.0`**.
  - 7.1 repo layout: single project `src/Microsoft.Data.SqlClient/src/Microsoft.Data.SqlClient.csproj`
    (TFMs `net462;net8.0;net9.0`), PNSE stubs in `src/Microsoft.Data.SqlClient/notsupported/`
    (lib/ = stubs, runtimes/<rid>/ = real impl), sibling packages
    `Microsoft.Data.SqlClient.Extensions.Abstractions` + `Microsoft.Data.SqlClient.Internal.Logging`
    (same version as SqlClient) and `Microsoft.SqlServer.Server` (own version).
  - Signing: `SigningKeyPath` msbuild property; fork key committed at `tools/iplus-fork.snk`
    (mono `sn -k`). The fork assembly therefore has a **new PublicKeyToken** (not the official one).
  - Local tooling: `dotnet tool restore` required (pwsh local tool); GenAPI must be built before
    the notsupported project (`dotnet build tools/GenAPI/Microsoft.DotNet.GenAPI/...csproj` and
    pass `-p:GenApiPath=`).
  - **Pack commands (all into `/home/damir/SHARED/Devel/iPlusGit/V5/packages-local`):**
    ```
    dotnet pack src/Microsoft.Data.SqlClient.Extensions/Abstractions/src/Abstractions.csproj -c Release -p:Version=7.1.1-iplus.0 -p:SigningKeyPath=$PWD/tools/iplus-fork.snk -o ../packages-local
    dotnet pack src/Microsoft.Data.SqlClient.Internal/Logging/src/Logging.csproj          -c Release -p:Version=7.1.1-iplus.0 -p:SigningKeyPath=$PWD/tools/iplus-fork.snk -o ../packages-local
    dotnet pack src/Microsoft.SqlServer.Server/Microsoft.SqlServer.Server.csproj          -c Release -p:SqlServerPackageVersion=1.1.0-preview1-iplus.0 -p:SigningKeyPath=$PWD/tools/iplus-fork.snk -o ../packages-local
    dotnet pack src/Microsoft.Data.SqlClient/src/Microsoft.Data.SqlClient.csproj          -c Release -p:SqlClientPackageVersion=7.1.1-iplus.0 -p:SqlServerPackageVersion=1.1.0-preview1-iplus.0 -p:SigningKeyPath=$PWD/tools/iplus-fork.snk -o ../packages-local
    ```
    (Do NOT pass `-p:Version=` to the main package — the dependency ranges are derived from
    `SqlClientPackageVersion`/`SqlServerPackageVersion`.)
  - Gotcha: the machine's NuGet global-packages store is redirected to
    `/home/damir/SHARED/Devel/iPlusGit/V5/packages` (not `~/.nuget/packages`) — purge stale
    fork versions there after repacking.
  - Gotcha: after rebasing 6.1 → 7.1, untracked 6.1-era leftovers (`src/Directory.Packages.props`)
    shadowed the 7.1 root CPM file and caused `NU1010`. If CPM errors appear in the fork, check
    `git status --untracked-files=all` for stale props files.
  - **Debugging (Option B):** iPlus can reference the fork as a *project* instead of the package:
    `dotnet build gip.iplus.client.avui.Desktop/... -p:UseSqlClientForkProjectReference=True`
    (switch + `SqlClientForkProject`/`SqlClientForkSigningKey` defined in iPlus
    `Directory.Build.props`, wiring in `Directory.Build.targets`: the SqlClient package assets
    are excluded via `ExcludeAssets=all` and a signed `ProjectReference` is added instead —
    token `9b460b8d8350bb47`, identical in both modes). Set `justMyCode: false` in the launch
    profile to step into fork source.
- EF fork: `/home/damir/SHARED/Devel/iPlusGit/V5/ef_main_iPlus` (branch `ef_main_iPlus`),
  pins `Microsoft.Data.SqlClient 7.1.1-iplus.0` in `Directory.Packages.props`;
  `packages-local` feed added to its `NuGet.config`. `EFCore.SqlServer` builds clean.
- iPlus: `/home/damir/SHARED/Devel/iPlusGit/V5/iPlus` (net10.0, consumes the EF fork as
  `iPlus.EntityFrameworkCore.*` **NuGet packages 10.0.5**, not project references).
  `Directory.Packages.props` pins `Microsoft.Data.SqlClient 7.1.1-iplus.0` (replacing the
  previous `7.0.0-preview3.25342.7` pin) + `Azure.Core 1.51.1` (7.1 SqlClient no longer flows
  Azure.* transitively; `gip.core.webservices` uses `Azure.Core`) and bumps
  `Microsoft.Bcl.AsyncInterfaces` to 10.0.2. `gip.iplus.client.avui.Desktop` builds clean and
  resolves `7.1.1-iplus.0`.
- GitHub Actions on the fork: **not yet disabled** (no `gh` CLI) — disable once in the web UI
  (repo → Actions → Disable actions) before the first push.

## 1. Architecture

```
+---------------- Browser (WASM) ------------------+
| Forked SqlClient (net10.0-browser TFM):          |
|   TDS parser (unchanged)                         |
|   transport: WebSocketStream (NEW) instead of    |
|   TcpClient/NetworkStream/SslStream              |
+---------------------|----------------------------+
                      | WebSocket (raw TDS bytes, TLS)
+---------------------v----------------------------+
| iPlus Application Server                         |
|   PASqlTunnelHost (NEW ACComponent, see §4)      |
|     - Kestrel WebSocket endpoint /sqltunnel      |
|     - 2-stage auth: OAuth + iPlus login          |
|     - after auth: dumb byte relay                |
|       WebSocket <-> TcpClient to SQL Server      |
+---------------------|----------------------------+
                  TCP (TDS)
               SQL Server (on-prem)
```

Key properties:
- **100% protocol fidelity:** real SqlClient runs in the browser — MARS, TVPs, bulk copy, every
  type work for free. **No EF changes at all.**
- **Credentials never reach the browser:** the relay resolves the connection string server-side
  per authenticated user (like today's fat client, but better — the client config no longer
  holds the password).
- **Desktop/Android/console/service are unaffected:** the fork's `net10.0` target keeps the
  stock TCP transport; only `net10.0-browser` gets the WebSocket transport.

## 2. Fork Work (`/home/damir/SHARED/Devel/iPlusGit/V5/SqlClient`)

### Step 1 — Clone & branch ✅ DONE 2026-10-09
Branch `iplus/browser-transport`, **rebased onto `origin/release/7.1`** (was release/6.1).

### Step 2 — Baseline build & drop-in verification ✅ DONE 2026-10-09 (build level)
- Packed `7.1.1-iplus.0` (+ Abstractions, Internal.Logging, SqlServer.Server siblings) into
  `packages-local`; see §0 for the exact commands.
- `EFCore.SqlServer` (EF fork) and `gip.iplus.client.avui.Desktop` (iPlus) build clean and
  resolve `7.1.1-iplus.0`.
- ⚠️ Runtime smoke test (login + BSO search against a real SQL Server) still pending.

### Step 3 — Add browser TFM ✅ DONE 2026-10-09
- `<TargetFrameworks>net462;net8.0;net9.0;net10.0-browser</TargetFrameworks>` in
  `src/Microsoft.Data.SqlClient/src/Microsoft.Data.SqlClient.csproj`.
- No PNSE guard needed to be removed: the browser ref assemblies keep unsupported APIs
  (attributed PNSE) instead of removing them, so the full source compiles unchanged for
  `net10.0-browser`. Only build change: `Microsoft.Data.SqlClient.SNI.runtime` is not
  referenced for the browser TFM (no native SNI on WASM); `System.Configuration.ConfigurationManager`
  is kept (pure managed, several config types are type-forwarded to it).
- All TFMs (`net462`, `net8.0`, `net9.0`, `net10.0-browser`) + `ref/` build clean, signed
  with `tools/iplus-fork.snk` (token `9b460b8d8350bb47`).

### Step 4 — Transport swap ✅ DONE 2026-10-09 (build level; spike pending)
- Transport seam already existed: `SniPacket.WriteToStream/ReadFromStream` operate on a
  plain `Stream`; the `Socket` in `SniTcpHandle` is only used for connect/timeout/poll.
- New `WebSocketStream : Stream` (`ManagedSni/WebSocketStream.netcore.cs`): full-duplex
  stream over `ClientWebSocket` (works on WASM). Writes = one binary WS message per call;
  reads coalesce received messages into a buffer. Sync + async + cancellation.
- New `SniWebSocketHandle : SniPhysicalHandle` (`ManagedSni/SniWebSocketHandle.netcore.cs`):
  Stream-only twin of `SniTcpHandle` — prelogin/TLS (`SslOverTdsStream`/`SniSslStream`) and
  packet IO unchanged; certificate validation still targets the SQL Server identity, not the
  relay. Known limitation: per-read receive timeouts are not honored (reads block until data
  or tunnel close); TDS-level attention/cancel still works.
- Selection is **keyword-driven, not TFM-driven** (works from any TFM, incl. desktop for
  testing): new connection-string keyword
  `Tunnel URL=<wss://host/sqltunnel>` (synonym `TunnelUrl`). When set,
  `ServerInfo` encodes `tunnel:<url>` into `ExtendedServerName`;
  `DataSource.ParseServerName` recognizes the `tunnel:` protocol and `SniProxy` creates
  `SniWebSocketHandle`. All other keywords (auth, encrypt, packet size, …) unchanged.
- Public API: `SqlConnectionStringBuilder.TunnelUrl` added in `src/` **and** `ref/`
  (+ doc snippet, `Strings.DbConnectionString_TunnelUrl` resource).
- Deviation from original plan: single `TunnelUrl` keyword instead of
  `Transport=WebSocket;TunnelUrl=...` — presence of `TunnelUrl` selects the WebSocket
  transport; no separate Transport keyword needed.

### Step 5 — Spike round-trip (go/no-go) ✅ **GO** 2026-10-10
- Dumb relay (see §4) + a minimal WASM test page (or the iPlus browser client itself).
- **Result:** Desktop spike (`iPlus/SqlTunnelSpike/`) round-tripped a full TDS prelogin +
  login + `SELECT @@VERSION` + `SELECT COUNT(*) FROM sys.databases` through the live
  `PASqlTunnelHost` relay (`ws://host:8751/sqltunnel` → SQL Server 2019). SQL auth
  (`user id`/`password`), `Encrypt=False`. Clean close handshake in both directions
  (client `WebSocketStream` sends a close frame on dispose; relay pumps treat abrupt
  disconnects as normal session end). No exceptions on either side.
- WASM round-trip still pending (browser client integration, task 7) — the transport is
  keyword-driven, so the same connection string applies.
- If SSPI/TLS corner cases stall the spike → fall back to Option C
  (see `SqlTunnelProvider-Design-Draft.md` §3).

### Step 6 — Hardening (after go)
- Complete TDS corners the test parser doesn't exercise: SSPI/NTLM login paths (prefer
  `user id`/`password` SQL auth in the tunnel first — simplest), TDS 8.0 TLS handshake order,
  INSERT BULK, TVPs.
- Reconnect/resume semantics: WebSocket drop mid-transaction → server rolls back, client
  surfaces the error (same as a dropped SQL connection today).
- Idle keep-alive (WebSocket ping) so firewalls don't kill idle sessions.

## 3. Client Integration (iPlus)

- `gip.core.datamodel` (`Database.ConnectionString`, `EntityObjectExtension.DbContextOptions`)
  needs **no changes** — the forked SqlClient is a drop-in; the browser client's
  `settingsiPlus.config` connection string just gains the `Transport=WebSocket;TunnelUrl=...`
  keywords (entered in the existing SettingsGrid).
- The tunnel URL can default to the same host the client already knows (the iPlus server from
  `VBUserInstance`), so users only configure it once.
- WCF client↔server communication (proxy components) is a **separate track** — see §8
  (Browser WCF Transport, decided 2026-10-10). The plain-HTTP-WCF-binding idea was dropped:
  WSDualHttpBinding does not exist in .NET Core WCF (dotnet/wcf#4614) and the thread-based
  `WCFClientChannel` cannot run on the browser main thread.

## 4. Server Side: `PASqlTunnelHost` (NEW ACComponent)

**Design decision (2026-10-09):** the relay is *database access*, unrelated to
`WCFServiceManager` (which handles client-proxy ↔ server-ACComponent communication). Therefore
it is built as a **configurable ACComponent in `gip.core.webservices`**, modeled after
`PAMcpServerHost` — the user configures *which application project* instantiates it.

### 4.1 Component shape (mirror of `PAMcpServerHost`)

**Status 2026-10-09: implemented** — `gip.core.webservices/Components/SqlTunnel/PASqlTunnelHost.cs`,
builds clean (and iPlus Desktop regression OK). v1 scope: dumb relay only —
config `IsEnabled`, `ServicePort` (inherited), `SqlServer`, `SqlPort`;
`GET /sqltunnel` → WebSocket accept → `TcpClient` to `SqlServer:SqlPort` →
order-preserving byte pump both directions (`ArrayPool` 16 KB buffers) →
close both on either side closing. **No auth stages yet** (v1 spike; the tunnel endpoint
must not be exposed unauthenticated to the internet — hardening task below).

```csharp
// gip.core.webservices/Components/SqlTunnel/PASqlTunnelHost.cs
[ACClassInfo(Const.PackName_VarioSystem, "en{'SQL Tunnel Host'}de{'SQL Tunnel Host'}",
    Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
public class PASqlTunnelHost : PAWebServiceBase
{
    // Config properties (ACPropertyConfig, like PAMcpServerHost):
    //   IsEnabled        : bool   — start the relay
    //   ServicePort      : int    — WebSocket listen port (or reuse ServicePortHTTP host?)
    //   AllowedDatabases : string — which connection strings/databases may be tunneled
    //   RequireOAuth     : bool   — stage-1 auth switch
    //   IdleTimeoutMin   : int    — session idle timeout

    public override void StartService()  // called from ACPostInit, guarded by IsEnabledStartService
    {
        // WebApplication.CreateBuilder() -> MapWebSocket("/sqltunnel", ...) -> RunAsync(uri)
        // identical lifecycle to PAMcpServerHost.StartService():
        //   StopService() first, build host, Task.Run(RunAsync), alarm on error
    }

    public override bool IsEnabledStartService() => _Host == null && IsEnabled && ServicePort > 0;
    public override void StopService() { /* cancel token, wait task, dispose — like PAMcpServerHost */ }
    public override bool IsEnabledStopService() => _Host != null;
}
```

Lifecycle (verified from the `PAMcpServerHost` stack trace):
- Instantiated via `ACActivator.CreateInstance` when the ApplicationManager starts the
  application project's components (`ACComponent.StartComponent` → ctor → `ACInit` →
  `ACPostInit` → `StartService()`).
- Stopped via `ACDeInit` → `StopService()`.
- Because it derives from `PAWebServiceBase`, it gets `ServicePort`, alarm infrastructure
  (`IsMcpServerAlarm`-style binding source → `IsTunnelAlarm`), and `AcknowledgeAlarms()`.

### 4.2 Per-connection flow

```
1. WebSocket connect to /sqltunnel
2. Stage 1 auth: OAuth token in handshake (ValidateToken) — reject if invalid
3. Stage 2 auth: client sends iPlus credentials (or session ticket from the WCF login);
   server validates via iPlus login (ACStartUpRoot/Database) and resolves the
   connection string for the user's application project — server-side only
4. Open TcpClient to the resolved SQL Server (from the resolved connection string)
5. Blind byte relay: WebSocket <-> TcpClient, both directions, until either side closes
6. On close/timeout: dispose TcpClient (SQL Server rolls back any open transaction)
```

Notes:
- **The relay never parses TDS** (except optionally the prelogin packet to route to the right
  database — v1 can avoid this by resolving the full connection string server-side and
  rewriting nothing; the client's TDS login then just works).
- **Two-stage auth** (OAuth + iPlus login) closes the unauthenticated-tunnel hole; see the
  security model in `SqlTunnelProvider-Design-Draft.md` §2b for the residual-risk table.
- **Multiple server instances** work as with WCF: the browser client connects to the instance
  whose application projects are assigned to the user. Internet access = existing gateway +
  port redirection (not recommended).

### 4.3 Hosting options

| Option | When |
|---|---|
| Own Kestrel `WebApplication` in `PASqlTunnelHost` (like `PAMcpServerHost`) | Default — independent of `ServiceAppEnbledHTTP`; works even on net.tcp-only servers |
| Attach the `/sqltunnel` endpoint to the `WCFServiceManager`'s ASP.NET host | Later optimization if a shared port is wanted |

## 5. Task List (AI-assisted estimates)

| # | Task | Depends on | Est. |
|---|------|-----------|------|
| 1 | Clone & branch fork | – | ✅ done |
| 2 | Baseline pack `6.1.7-iplus.0` + drop-in verification (EF fork + iPlus Desktop) | 1 | 0.5–1 d |
| 3 | Browser TFM + remove guard | 2 | 0.5–1 d |
| 4 | `WebSocketStream` + transport seam + `Transport=WebSocket` keyword | 3 | 2–3 d |
| 5 | `PASqlTunnelHost` ACComponent (config, lifecycle, alarm) | – | 1 d |
| 6 | Relay loop (auth stages, TcpClient pipe, cleanup) | 5 | 1–2 d |
| 7 | Spike round-trip from WASM (go/no-go) | 4, 6 | 1 d |
| 8 | iPlus browser client integration (connection string in SettingsGrid) | 7 | 0.5–1 d |
| 9 | Hardening: SSPI/TDS 8.0 TLS, INSERT BULK, TVP, reconnect, keep-alive | 7 | 3–5 d |

**Total: ~2–3 weeks AI-assisted** (human alone: 4–6 weeks). Go/no-go gate is task 7.

### Subagent split
- **Agent A (SqlClient fork):** tasks 2–4 — needs the fork checkout + local SQL Server.
- **Agent B (server component):** tasks 5–6 — pure iPlus server-side C#.
- **Agent C (integration):** tasks 7–8 — browser client + Playwright verification
  (tooling from the 2026-10-08 debugging session).

## 6. Open Questions

1. OAuth: which IdP / token validation library? (Or v1: iPlus session ticket only, OAuth later.)
2. Should `PASqlTunnelHost` reuse `ServicePortHTTP` or get its own port? (Own port recommended —
   independent of `ServiceAppEnbledHTTP`.)
3. Connection-string resolution: per user (`VBUserInstance`) or per application project config?
   (Proposal: per application project, matching where the component is instantiated.)
4. Does the browser client need *multiple* simultaneous tunnels (multiple DbContexts)? v1: one
   WebSocket per `SqlConnection` — SqlClient pools connections, so expect several sockets.

## 7. Browser Transport Fix: Sync-XHR HTTP Shim — **DECIDED 2026-10-10 (Option A)**

### 7.1 Why the WebSocket transport alone cannot work in the browser

The Step-5 browser test failed with:

```
SqlException: ... (provider: TCP Provider, error: 35 - An internal exception was caught)
  inner: PlatformNotSupportedException: Cannot wait on monitors on this runtime.
```

- The PNSE was thrown inside `SniWebSocketHandle`'s constructor (the only place that wraps
  exceptions into `SqlException` via `ReportTunnelSNIError`; the tunnel handle reports
  `TCP_PROV`, hence "TCP Provider").
- Culprit: `webSocket.ConnectAsync(...).GetAwaiter().GetResult()`. On the browser main thread
  there are no real threads; blocking on an incomplete `Task` falls back to `Monitor.Wait`,
  which single-threaded WASM rejects (documented dotnet/runtime limitation).
- This hits **every** sync TDS read (`WebSocketStream.Read` → `ReceiveAsync(...).GetResult()`),
  not just the connect — and iPlus's data layer is sync EF Core, so the whole path is sync.
- A WebSocket message is a JS macrotask: it can only be dispatched when the JS stack is empty.
  Therefore **no spin/sleep trick can ever observe an incoming frame** — sync-over-async
  network I/O is fundamentally impossible on the browser main thread.

Evaluated alternatives (details in `SqlTunnelProvider-Design-Draft.md` §8): Web Worker +
SharedArrayBuffer (B), WASM threads (C), async-everywhere (D — infeasible). **Decision: A.**

### 7.2 Design: HTTP shim endpoints on `PASqlTunnelHost`

The relay keeps the existing `/sqltunnel` WebSocket endpoint (desktop). Additive HTTP
endpoints for the browser:

```
POST   /sqltunnel-sync/session              -> create session, open TcpClient to SQL Server
   -> 200 {"sessionId": "..."} | 502 cannot reach SQL Server
POST   /sqltunnel-sync/{id}/send            -> write body bytes to the SQL socket
   body: TDS bytes (raw binary) -> 200 | 410 session dead
POST   /sqltunnel-sync/{id}/recv            -> long-poll next chunk from the SQL socket
   server holds max ~1 s; -> 200 body: base64 text | 204 no-data (client retries) | 410 dead
DELETE /sqltunnel-sync/{id}                 -> close SQL socket, drop session
```

> **Note (implemented 2026-10-10):** `/recv` returns **base64 text**, not raw bytes —
> synchronous XHR in window context only supports the `''` and `'text'` response types
> (no `arraybuffer`). `/send` stays raw binary (sync XHR *send* accepts typed arrays).

- **Client side (fork):** `#if IPLUS_BROWSER` variant of the transport — a `Stream` over
  **synchronous `XMLHttpRequest`** (via `JSImport` shim). The browser performs the wait
  internally on its own threads: no `Monitor.Wait`, no event-loop re-entrancy. Desktop keeps
  the WebSocket stream.
- **Sync XHR deprecation warning:** cosmetic console warning only; sync XHR is fully supported
  in Chrome/Firefox/Safari/Edge and underpins the whole SQLite-WASM ecosystem. Not a blocker.
- **Long queries:** the UI freeze is inherent to *any* sync DB call on the main thread, not to
  this transport. Design rule: `/recv` never holds longer than ~1 s — a 5-minute query becomes
  ~300 short `204`-retry round-trips, never one long blocked request (no XHR timeouts).
- **Risk — Chrome "Page unresponsive" dialog:** if the main thread blocks continuously for
  ~30–45 s *while the user interacts*, Chrome may show the dialog. Mitigations: busy indicator
  rendered before entering the sync call; long-running reports/mass updates via iPlus's async
  paths or batching. Typical BSO work (login, navigation, `NavTakeCount` searches, saves) is
  well under the threshold.
- **CORS:** relay sends permissive CORS headers for the browser origin (it already handles the
  WebSocket upgrade); auth token via header, same two-factor gate as §4.2.
- **Upgrade path:** if long-query UI responsiveness later becomes a product requirement, move
  to WASM threads (worker) — the session-based relay and protocol are reused unchanged.

### 7.3 Implementation steps

1. `PASqlTunnelHost`: add the four HTTP endpoints (session map `sessionId -> TcpClient`,
   `ArrayPool` buffers, 1 s recv hold, cleanup on DELETE/idle timeout).
2. Fork: browser `Stream` implementation over sync XHR (`JSImport` to a small JS helper that
   wraps `XMLHttpRequest` synchronously); wire into `SniProxy` for the browser TFM
   (replace/augment `SniWebSocketHandle` selection — e.g. `Transport=HttpSync` keyword or
   automatic on `net10.0-browser`).
3. Re-run the browser probe: prelogin + login + `SELECT @@VERSION` from the WASM client.
4. Desktop regression: WebSocket path untouched.

## 8. Browser WCF Transport — **DECIDED 2026-10-10**

### 8.1 Constraints (why not WCF-over-HTTP as-is)

- **WSDualHttpBinding does not exist** in .NET Core WCF (dotnet/wcf#4614) — server→client
  push over HTTP-duplex is unavailable on any .NET Core platform, not just WASM.
- The thread-based `WCFClientChannel`/`WCFServiceChannel` architecture (`new Thread`,
  `EventWaitHandle.WaitOne`, `Thread.Sleep(100)` in the dispatch loops) **cannot run on the
  browser main thread** (PNSE / UI freeze) — see §7.1.

### 8.2 Design: async-native browser channel reusing `WCFMessage` contracts

```
Browser (WASM)                          iPlus App Server
--------------                          ----------------
ACBSONav / proxy components
   -> WCFClientChannel (browser variant)
        async message pump (no threads):
        - SendQueue drained by an async loop (await-based, System.Threading.Channels)
        - incoming messages arrive via WebSocket push
        -> WebSocket endpoint on the app server
                                           -> same dispatch semantics as WCFService
                                           -> real server-side ACComponents
```

- **Reuse unchanged:** `WCFMessage`, `ACMethodInvocation`, `ACPropertyValueMessage`,
  `ACSubscriptionMessage` data contracts; serialize over binary JSON (or the existing
  DataContract serializer).
- **Server push:** WebSocket replaces the WSDualHttp callback (works async on WASM).
  `EnqeueReceivedMessageFromPeer` remains the single entry point — only its caller changes.
- **Client:** browser variant of `WCFClientChannel` with producer/consumer threads replaced by
  async pumps (`Channel<T>` + `await foreach`), selected via platform check.
  `SendACMessageToServer` stays fire-and-enqueue.
- **Hosting:** endpoint added to the `WebApplication` host that `WCFServiceManager` already
  builds when `ServiceAppEnbledHTTP` is set (net.tcp-only servers need HTTP enabled for
  browser clients). Auth: same iPlus login/token gate as the SQL tunnel (§4.2).
- **Desktop untouched:** existing net.tcp/HTTP WCF channels remain for desktop/Android.

### 8.3 Implementation steps

1. Server: WebSocket endpoint (e.g. `/wcfbrowser`) in the `WCFServiceManager` ASP.NET host
   (or a sibling ACComponent modeled on `PASqlTunnelHost`); per-connection dispatch into the
   existing `WCFService` message handling; push outgoing messages over the socket.
2. Fork/iPlus client: browser `WCFClientChannel` variant — async pumps, WebSocket transport,
   same public surface (`SendACMessageToServer`, `EnqeueReceivedMessageFromPeer`,
   subscription APIs).
3. Wire-up via platform check where the channel is created; desktop path unchanged.
4. Integration test: browser login → proxy component method invoke → property-value broadcast
   from server → client.

### 8.4 Task list additions

| # | Task | Depends on | Est. |
|---|------|-----------|------|
| 10 | `PASqlTunnelHost` HTTP shim endpoints (§7.2) — **implemented 2026-10-10** (`PASqlTunnelHost.cs`: `/sqltunnel-sync/session|send|recv|DELETE`, per-session `TcpClient` + persistent pending-read buffer, 1 s recv hold → 204, 410 on dead socket, 5 min idle cleanup, CORS preflight) | 6 | 1 d |
| 11 | Fork: browser sync-XHR `Stream` + transport selection — **implemented 2026-10-10** (`SyncHttpTunnelStream.netcore.cs`: sync XHR via `JSImport` to `globalThis.sqlTunnelSync`, self-installed JS shim, coalescing read buffer; `SniWebSocketHandle` generalized to accept a pre-connected transport; `SniProxy.CreateTunnelHandle` selects it on `net10.0-browser` when `Tunnel URL` is `http(s)://`) | 3 | 1–2 d |
| 12 | Browser probe re-test (prelogin/login/query via shim) | 10, 11 | 0.5 d |
| 13 | Server: browser WCF WebSocket endpoint + dispatch | – | 2–3 d |
| 14 | Client: browser `WCFClientChannel` variant (async pumps) | 13 | 2–3 d |
| 15 | Integration: browser login + proxy invoke + broadcast | 12, 14 | 1–2 d |

## 9. SESSION MEMORY — SQL Tunnel browser test setup (2026-10-10)

> **Read this first when continuing in a new session.** Everything needed to resume the
> browser end-to-end test without repeating mistakes.

### 10.1 How iPlus consumes the SqlClient fork — **ProjectReference mode, no packages**

- `iPlus/Directory.Build.props` sets `UseSqlClientForkProjectReference=True` (default) and
  `SqlClientForkProject=..\SqlClient\src\Microsoft.Data.SqlClient\src\Microsoft.Data.SqlClient.csproj`.
  Every project that would reference the `Microsoft.Data.SqlClient` **package** gets a direct
  **ProjectReference** to the fork checkout instead (same assembly name/version 7.1.0.0, signed
  with `SqlClient/tools/iplus-fork.snk`). **Do NOT repack NuGet packages for local testing** —
  just rebuild. (The `packages-local` feed + `7.1.1-iplus.0` package exists but is stale and
  its pack flow currently produces a nupkg without `net10.0` libs — ignore it for now.)
- Fork branch: `iplus/browser-transport`, TFMs `net10.0;net10.0-browser`, `IPLUS_BROWSER` define
  for browser-only code.

### 10.2 Local end-to-end test environment (all on this machine, `gipsoft-dlmhp-26`)

| Piece | How to run | Notes |
|---|---|---|
| Server (relay + web services) | `cd bin/Debug/net10.0 && dotnet gip.iplus.console.dll < /dev/null > /tmp/iplus-console.log 2>&1` (async/background) | Headless console; DB `gipDLVmSQL1.incus/iPlusV5` (reachable). Wait for `Service is ready for requests.` Relay auto-starts: log line `SQL Tunnel relay started on port 8751 -> gipDLVmSQL1.incus:1433` in `bin/Debug/net10.0/Logs/`. |
| Browser client | `cd iPlus && ASPNETCORE_URLS=http://localhost:5235 dotnet run --project gip.iplus.client.avui.Browser --no-launch-profile > /tmp/iplus-browser.log 2>&1` (async) | **Must be plain HTTP** — the VS Code browser tools refuse the dev self-signed HTTPS cert. `--urls` CLI arg does NOT work (gets passed to the WASM app); use `ASPNETCORE_URLS`. WASM build takes ~2–4 min; poll `curl http://localhost:5235/` for 200. The actual URL is printed as `App url:` in the log. |
| Shim smoke test | `curl -X POST http://localhost:8751/sqltunnel-sync/session` → `{"sessionId":"..."}` | Relay endpoints: `/sqltunnel-sync/session|{id}/send|{id}/recv|DELETE`. `/recv` returns **base64 text** (sync XHR cannot use `responseType=arraybuffer`); `/send` is raw binary. |

### 10.3 Auto-login bypass (temporary, in `LoginView.axaml.cs`)

- Avalonia renders to canvas → the agent browser tools **cannot type into textboxes** (clicks
  and checkbox toggles work, text input does not land). WASM settings also don't persist.
- Therefore `LoginView` has a **temporary bypass** (`AutoLoginBypassEnabled = true` const):
  on `OnLoaded` it injects `DatabaseSource=sqltunnel`, `DatabaseName=iPlusV5`,
  `DatabaseUser=gip`, `DatabasePassword=netspirit`,
  `DatabaseTunnelUrl=http://localhost:8751/sqltunnel-sync` into `CommandLineHelper.Settings`,
  sets `UserSettings.UserName/Password` (currently `superuser`/`netspirit` — **adjust if wrong**),
  and calls `ButtonLogin_Click` automatically. **Remove once login works end-to-end.**
- Tunnel URL must be `http://…/sqltunnel-sync` (HTTP sync shim), **never `ws://`** on browser —
  the fork now fails fast with a clear error for `ws(s)://` on `net10.0-browser`
  (`SniProxy.CreateTunnelHandle`).

### 9.4 Fixed / discovered on 2026-10-10 (this session)

1. **Relay HTTP sync shim** (`PASqlTunnelHost.cs`): `/sqltunnel-sync/session|send|recv|DELETE`,
   per-session `TcpClient`, persistent pending-read (no lost bytes across 204 retries), 1 s
   recv hold, 5 min idle cleanup, CORS preflight, diagnostic logging (iPlus log source
   `SyncShim`: session created / sent N bytes + first-bytes hex / received N bytes + TDS
   header hex). TEMPORARY diagnostics still in place — remove once stable.
2. **Fork browser transport** (`SyncHttpTunnelStream.netcore.cs`): sync-XHR `Stream` via
   `JSImport` to `globalThis.sqlTunnelSync` (self-installed JS shim via eval); coalescing
   read buffer; `SniWebSocketHandle` generalized to accept a pre-connected stream;
   `SniProxy.CreateTunnelHandle` selects it on browser for `http(s)://` TunnelUrl. Has
   TEMPORARY `Console.WriteLine` tracing (`SyncHttpTunnelStream: Read/Write ...`) — remove
   once stable.
3. **MARS Debug.Assert crash** (`TdsParserStateObject.ReadAsyncCallback`): a MARS
   receive-error broadcast to a session with no pending read (`source == null`,
   `error != 0`) aborted the whole server process ("AsyncResult null on callback", exit 134).
   Fixed: ignore the benign closed-connection race when `_parser.MARSOn`.
4. **ROOT CAUSE of the original browser stall: MARS.** With
   `multipleactiveresultsets=True` the browser client completed prelogin (94→54 bytes) and
   login7 (433→477 bytes, complete TDS packet) and then stalled forever in recv polling —
   the MARS demux path deadlocks on the single-threaded WASM main thread. Desktop spike
   over the same relay works with MARS (async) and without (sync). **MARS is now removed
   from the browser connection string** (LoginView.ApplyConnectionSettingsFromHelper —
   TEMPORARY, re-add only after a WASM-safe MARS design exists).
5. **Second root cause: `Integrated Security=True` + `user id`/`password`.** SqlClient
   ignores the SQL credentials and attempts SSPI/Windows auth → on WASM (no SSPI) the login
   stalls after the server's SSPI token. Fixed in `ApplyConnectionSettingsFromHelper`:
   `Integrated Security=False` when dbUser/dbPass are set, else `Integrated Security=True`.

### 9.5 Current state / next steps (as of 2026-10-10 ~20:05)

- **VERIFIED WORKING:** browser WASM client → HTTP sync shim → relay → SQL Server 2019.
  With MARS off + SQL auth, the full EF login traffic flows through the shim (large queries
  both directions, e.g. `LEFT JOIN [a4].[ACClass]`, 8 KB packet splits handled correctly by
  the coalescing read buffer). The SQL Tunnel Option A transport is **proven**.
- **CURRENT BLOCKER (next session's first task):** `gip.core.datamodel.Database..cctor`
  throws `NullReferenceException` AFTER its EF query succeeded (traffic visible). The cctor
  does: `GetOrCreateContext<Database>` → `ACClass` query (OK) →
  `ACQueryDefinition.CreateRootQueryDefinition(acClass)` → `_GlobalDatabase.InitTypeDictFromSystemNamespace()`.
  The NRE is in one of the last two — debug on browser (probe stack trace in LoginView
  already logs it: "probe failed ... ---> NullReferenceException" at
  `ApplyConnectionSettingsFromHelper` → `Database.ConnectionString`).
- **Then:** the iPlus user login (`superuser`/`netspirit` in the bypass — password unverified,
  adjust `BypassLoginPassword` const if wrong) has not been reached yet.
- **Test loop:** restart server (`pkill -f gip.iplus.console`, then nohup run from
  `bin/Debug/net10.0`), restart browser app (`fuser -k 5235/tcp`, then nohup
  `ASPNETCORE_URLS=http://localhost:5235 dotnet run --project gip.iplus.client.avui.Browser
  --no-launch-profile`), open `http://localhost:5235/` in the agent browser — the bypass
  auto-logs-in. Server-side shim trace: `grep -aE SyncShim bin/Debug/net10.0/Logs/*.txt`;
  Kestrel request log in `/tmp/iplus-console.log`. Browser console via playwright
  `page.on('console')` + reload `waitUntil:'commit'` (a stalled tab blocks reload — open a
  NEW page instead).
- **Cleanup backlog:** remove SyncShim hex-dump logging + SyncHttpTunnelStream tracing +
  auto-login bypass once stable; decide MARS-on-browser design; relay two-stage auth;
  wss:// for production; fix fork pack flow (net10.0 libs missing from nupkg) before any
  package-mode use; disable fork GitHub Actions before push.

### 9.6 Debugging the WASM client with VS Code + Chrome (correct order — 2026-10-10)

This is the reliable way to get red (bindable) breakpoints in the WASM app — the agent
browser tools cannot do this, so this is a **manual** step for the developer:

1. Start Chrome with remote debugging and open the debug helper page:
   `google-chrome --remote-debugging-port=9222 --user-data-dir=/tmp/blazor-chrome-debug http://localhost:5235/_framework/debug`
2. Start the app: `dotnet run --project gip.iplus.client.avui.Browser`
3. Open a **second tab** in the same Chrome with `http://localhost:5235`
4. In VS Code: attach the debugger (`.NET: Attach to Browser (Chrome/Chromium)` or the
   browser debug config). Wait ~10 seconds — then the breakpoints turn red/bound.

### 9.7 Manual connection settings for the browser client (SettingsGrid)

When the auto-login bypass is disabled (`AutoLoginBypassEnabled = false`), enter these
values manually in the SettingsGrid:

| Field | Value |
|---|---|
| **Database source** | `sqltunnel` (placeholder — the relay config decides the real SQL target) |
| **Database name** | `iPlusV5` |
| **Database user** | `gip` |
| **Database password** | `netspirit` |
| **Tunnel Url** | `http://gipsoft-dlmhp-26:8751/sqltunnel-sync` (**must** be the host name `gipsoft-dlmhp-26`, not `localhost` — the browser runs on the same machine here, but the shim must be reachable; use the HTTP sync shim, **never `ws://`**) |

### 9.8 OPEN ISSUE: `CommandLineHelper.ConfigCurrentDir` on Browser/WASM

Observed with the VS Code debugger: execution reaches
`LoginView.ApplyConnectionSettingsFromHelper` line 728
(`var config = CommandLineHelper.ConfigCurrentDir;`) and **stops there** — the getter does
not return normally. On Android the same `CommandLineHelper` works (persist + read
settings). Not a cookie problem — `ConfigCurrentDir` is pure file-based
`System.Configuration`:

- `OpenMappedExeConfiguration` for `<CurrentDirectory>/vbiplus.config` — on WASM
  `Environment.CurrentDirectory` is the Emscripten in-memory FS root; the file does not
  exist until `ApplyConnectionSettingsFromHelper` writes it, and `config.HasFile` is false.
- Fallback `OpenExeConfiguration` — there is no exe config on WASM; this path likely
  throws (caught) or misbehaves, so `_ConfigCurrentDir` stays null.
- **Next steps:** add step-by-step `Console.WriteLine` tracing inside the
  `ConfigCurrentDir` getter to see which call throws/hangs on WASM; consider a WASM-specific
  branch that skips file-based config entirely (register the connection string directly in
  `ConfigurationManager.ConnectionStrings`, which `Database.ConnectionString` already falls
  back to) and persists `settingsiPlus.config` via localStorage/IndexedDB instead of the
  virtual FS (the virtual FS is wiped on every reload — that is why settings don't persist,
  not cookies).

### 9.9 DIAGNOSED (2026-10-10 ~21:35): the `Database..cctor` NRE — nested query without MARS

Debugging chain (via Console.WriteLine tracing, NO debugger — the WASM debugger itself
crashes the runtime, see below):

1. `ConfigCurrentDir` is **fine** on WASM: returns a config (`HasFile=False` both paths,
   falls through to `OpenExeConfiguration`, returns non-null). NOT the problem.
2. `Database..cctor` NRE happens at `ACQueryDefinition.CreateRootQueryDefinition` line 83
   (`qryACQueryDefinition.ACColumns = acColumns;`) because
   `ACActivator.CreateInstance(...)` **returned null**.
3. `CreateInstance` swallowed the real exception in its catch block (and `Database.Root`
   is null during the cctor, so nothing was logged). With tracing:
   **`InvalidOperationException: There is already an open DataReader associated with this
   Connection which must be closed first.`** — thrown inside `ACInit` (nested query on the
   same connection while another reader is open).
4. This nested-query pattern is what **MARS** is for — and MARS is disabled on browser
   (it deadlocks the WASM main thread, §9.4). So the browser cannot run the iPlus
   `Database..cctor` as-is.

**Consequence:** MARS-on-browser is not optional — iPlus core code (ACActivator/ACInit and
likely many other places) issues nested queries on the shared connection. Options:
- (a) Make MARS work on the browser transport (the demux must not depend on the JS event
  loop being free — hard, this is the real fix), or
- (b) restructure the offending nested queries (open scope: unknown how many places), or
- (c) run the browser TDS transport on a Web Worker so the main thread stays free
  (sync XHR inside worker + Atomics.wait — bigger change but keeps sync semantics).

**TEMPORARY tracing added (remove when done):** `CommandLineHelper.ConfigCurrentDir`
getter, `ACActivator.CreateInstance` (null paths + catch), `ACQueryDefinition.CreateRootQueryDefinition`.

### 9.10 WASM debugger crashes the runtime (do not rely on it)

Attaching the VS Code/Chrome debugger and stepping/inspecting kills the WASM runtime:
`mono_process_dbg_packet → mono_class_from_mono_type_internal → * Assertion: should not be
reached at class.c:2310` → program exits with status 1 (keepRuntimeAlive). This is a Mono
WASM debugger bug (type decoding for breakpoint/locals), **not** an app bug — the earlier
"stops at line 728" was this crash, not `ConfigCurrentDir` hanging. Debug via
`Console.WriteLine` + browser console capture instead.

### 9.11 STAGE 1 SUCCESS (2026-10-10 ~23:30): MARS works on the browser — pull-based demux

Stage 1 (make MARS work on the main-thread transport) **succeeded**. The MARS demux was
rewritten to a **pull-based model** for synchronous transports (fork changes, all marked
"iPlus fork fix (browser WASM)"):

1. `SniPacket.ReadFromStreamAsync`: if the stream's ReadAsync completes **synchronously**
   (browser sync-XHR), run the demux continuation **inline** instead of
   `ContinueWith(TaskScheduler.Default)` — WASM has no thread pool, so the queued
   continuation never ran (this was the original browser stall).
2. `SniHandle.SupportsSynchronousReceive` (new internal virtual): true only for
   `SyncHttpTunnelStream` (browser) or `WebSocketStream` with the `IPLUS_SIMULATE_SYNC_READS=1`
   test hook. Gates all pull-based behavior — desktop async transports are 100% unchanged.
3. `SniMarsConnection.StartReceive`: NO standing read for sync transports (it would block the
   main thread forever — the server only sends after the client sends).
4. `SniMarsConnection.PullPacketBlocking` (new): posts one receive; with the inline fix the
   demux runs synchronously and routes the packet to the target session queue.
5. `SniMarsHandle.Receive` (sync): when the queue is empty, PULL instead of
   `_packetEvent.Wait` (nobody would ever set it). Desktop keeps the event wait.
6. `SniMarsHandle.Send` (sync): when the send window is full (`_sequenceNumber >=
   _sendHighwater`, initial window 4), PULL the ack instead of `_ackEvent.Wait()` forever.
7. `SniMarsConnection.HandleReceiveComplete`: after dispatching a packet, do NOT read ahead
   on sync transports (would block the main thread); the next session pull fetches more.

**Verification:**
- Desktop repro of the browser condition: `IPLUS_SIMULATE_SYNC_READS=1 SPIKE_MARS=1
  SPIKE_SYNC=1 dotnet run --project SqlTunnelSpike -- ws://localhost:8751/sqltunnel ...`
  hung before the fix (stack: `StartReceive → ReceiveAsync → ReadAsync` blocking), **SPIKE OK**
  after. Regressions: MARS+async OK, no-MARS+sync OK.
- **Browser: `LoginView: probe connection OPENED successfully` with
  `multipleactiveresultsets=True`** — full iPlus login traffic flows through the shim with
  MARS: SMUX session setup, interleaved 16-byte ACKs and multi-packet 8 KB/16 KB reads.
  The `Database..cctor` NRE (§9.9, nested query without MARS) is therefore also resolved.

**Debugging tips that cracked it:** `dotnet-stack report -p <pid>` on a hung desktop spike
(install: `dotnet tool install -g dotnet-stack`) gives the exact managed stack of the stall;
the `IPLUS_SIMULATE_SYNC_READS=1` env var reproduces the single-threaded-WASM condition on
desktop where real debuggers work.

**Next:** verify the iPlus user login completes and the main UI loads (was black at last
screenshot — check whether that is just rendering or a real failure); then cleanup backlog
(§9.5). Note: `AutoLoginBypassEnabled` is currently `true` again.

### 9.12 MD5 on WASM — login password hashing fixed (2026-10-11)

The iPlus user login failed on the browser with
`User superuser doesn't exist or wrong password! // Cryptography_UnknownHashAlgorithm, MD5`:
`Microsoft.Bcl.Cryptography` on the browser WASM runtime does not support `MD5.Create()`,
and `ACCrypt.GetMd5Hash` (used by `VBUser` password verification) needs it.

**Fix** (`gip.core.datamodel/ACHelperclasses/Cryptography/ACCrypt.cs`): new
`ACCrypt.ComputeMd5(byte[])` — tries the platform `MD5`, falls back to a pure managed
RFC-1321 implementation (`ACCrypt.ManagedMd5`) on `CryptographicException`. Verified against
canonical vectors (`abc` -> `900150983cd24fb0d6963f7d28e17f72` etc.). `GetMd5Hash` and
`DBInfo.GetCheckSum` now use it. Other `MD5.Create()` sites (digest client, installer) are
not on the login path.

**The login itself is now retestable** (password `superuser`); the WCF browser transport
below is separate.

### 9.13 Browser WCF transport (§8.3) — IMPLEMENTED, compilable, NOT yet tested (2026-10-11)

Implemented per §8.2/§8.3; **awaiting go-ahead before testing**. All projects build
(gip.core.autocomponent all TFMs, browser client, console server).

**Server side (gip.core.autocomponent):**
- `IWCFServicePeer` (new, `Communications/IWCFServicePeer.cs`): transport-agnostic peer of a
  `WCFServiceChannel` — `InvokeRemote`, `DisconnectClient`, `RemoteAddress`, `ClosingConnection`.
- `WCFService` implements it (ClosingConnection setter made public).
- `WCFServiceChannel`: `_serviceOfPeer` is now `IWCFServicePeer`; ACClassConstructorInfo
  param type updated. `ACClass.GetACParameter` assigns positionally, so passing a
  `WCFBrowserService` works without DB changes.
- `WCFBrowserService` (new, `Communications/Service/WCFBrowserService.cs`, `#if !ANDROID`):
  per-connection peer over a plain WebSocket. `Invoke` -> channel receive queue;
  `InvokeRemote` -> serialize + send frame; `RunReceiveLoopAsync` reads frames until close
  and stops the channel (like WCFService.Channel_Closed).
- `WCFBrowserSerializer` (new): `WCFMessage` <-> byte[] via DataContractSerializer with
  `ACKnownTypes.GetKnownType()` + `ACConvert.MyDataContractResolver` + PreserveObjectReferences
  (same stack as the WCF bindings). One binary WebSocket message = one WCFMessage.
- `WCFServiceManager` HTTP host: `app.UseWebSockets()` + `Map("/wcfbrowser")` — accepts the
  socket, starts a `WCFServiceChannel` per connection, runs the receive loop.

**Client side (`WCFClientChannel`):**
- `OperatingSystem.IsBrowser()` branch in `ACInit`: no ACThread loops, no blocking
  `_waitOnConnectionAtInit.WaitOne` — instead `StartBrowserTransport()` starts 4 async
  Task loops (connection+receive, send, property dispatch, point dispatch) over a
  `ClientWebSocket` to `ws://<authority>:<ServicePortHTTP>/wcfbrowser` (`EndpointUri`
  browser branch). Reconnect with growing 2s..58s delay, same as desktop.
- Same `_SendQueue`/`_ReceiveQueue`/`SyncQueueEvents` and `ProcessACMessage` pipeline;
  `EnqeueMessageForPeer` additionally releases a `SemaphoreSlim` wake-up for the send loop.
- Connection guards unified into `TransportAvailable` (browser: `_browserConnected`;
  desktop: unchanged WCF state checks).
- `ACDeInit` browser branch: cancel loops, dispose socket.

**Test plan (when given go-ahead):** server with `ServiceAppEnbledHTTP` user instance
(HTTP host must run for /wcfbrowser to exist) + browser client with the VBUserInstance
pointing at the server; verify ACConnect -> proxy method invoke -> property broadcast.
Note: the browser client currently reaches this only after a successful login (§9.12).

### 9.14 Hybrid operation: net.tcp + /wcfbrowser simultaneously (2026-10-11)

`WCFServiceManager.ACInit` previously built **either** the net.tcp host **or** the HTTP host
(depending on `VBUserInstance.ServiceAppEnbledHTTP`) — a hybrid desktop+browser deployment
could not serve both client types. Now ONE `WebApplication` host carries BOTH transports
unconditionally:

- net.tcp service endpoint (`IWCFService`, NetTcpBinding or CustomBinding as before) for
  desktop/Android clients — port from `EndpointUri` (`ServicePortTCP`).
- `/wcfbrowser` WebSocket endpoint for browser/WASM clients — HTTP port from
  `ASPNETCORE_URLS`/config.
- `ConfigureAllServiceHostBase` now guards against hosts without endpoints.

`ServiceAppEnbledHTTP` no longer switches the host type (the HTTP-mode WCF endpoint was never
functional anyway — WSDualHttpBinding does not exist in CoreWCF and the client's HTTP path
is unimplemented). Both client kinds connect per their own transport; the
`WCFServiceChannel` per connection works identically for `WCFService` (WCF session) and
`WCFBrowserService` (WebSocket) peers via `IWCFServicePeer`.

### 9.15 Correction: transport selection via ServiceAppEnabledTCP / ServiceAppEnbledHTTP (2026-10-11)

§9.14 hosted both transports unconditionally — wrong. The VBUserInstance flags control it
independently (one or both):

- **`ServiceAppEnabledTCP`** (existed but was never consumed before): hosts the net.tcp
  `IWCFService` endpoint (desktop clients) and selects net.tcp in `WCFClientChannel.EndpointUri`.
- **`ServiceAppEnbledHTTP`** (typo is the DB column name): hosts the Kestrel HTTP host incl.
  `/wcfbrowser` WebSocket (browser clients) and selects http on the client.
- Neither flag set → legacy fallback: net.tcp only (old databases keep working).
- If neither is enabled, no host is built (`_ASPHost` stays null; `ACDeInit` guards it).

Server `EndpointUri` is now always `net.tcp://…:ServicePortTCP` (it is only used for the
net.tcp endpoint; the HTTP port comes from ASPNETCORE_URLS). Client: http is chosen only
when `ServiceAppEnbledHTTP && !ServiceAppEnabledTCP`; browser always uses
`ws://…:ServicePortHTTP/wcfbrowser`.

### 9.16 Client-side dual option: WebSocket transport on desktop for HTTP-only (2026-10-11)

Desktop client with `ServiceAppEnbledHTTP=true` (TCP off) crashed: `NewConnectionInstance`'s
HTTP branch was empty (WSDualHttpBinding does not exist in .NET Core), so `_serviceOfPeer`
stayed null -> NRE at the DataContract-behavior loop.

**Fix:** the WebSocket transport is not browser-specific — `ClientWebSocket` works on desktop
too. `_IsBrowserTransport` (WebSocket transport selection) is now
`OperatingSystem.IsBrowser() || (ServiceAppEnbledHTTP && !ServiceAppEnabledTCP)`:

| ServiceAppEnabledTCP | ServiceAppEnbledHTTP | Desktop client | Browser client |
|---|---|---|---|
| true | false | net.tcp duplex WCF | ws /wcfbrowser |
| false | true | **ws /wcfbrowser** (was: crash) | ws /wcfbrowser |
| true | true | net.tcp (TCP preferred) | ws /wcfbrowser |
| false | false | net.tcp (legacy fallback) | ws /wcfbrowser |

Also: `NewConnectionInstance` now guards against `_serviceOfPeer == null` (logs instead of
NRE) if an unsupported flag combination ever reaches it.

### 9.17 Debug instrumentation memo — where to remove it later (2026-10-11)

The debug instrumentation is **intentionally kept for now** (may be needed for the NuGet
package test). Remove it once the tunnel is stable:

**iPlus repo:**
| File | What |
|---|---|
| `gip.core.webservices/Components/SqlTunnel/PASqlTunnelHost.cs` (~line 513) | `// TEMPORARY diagnostics:` TDS packet header hex-dump logging (SyncShim log source) |
| `gip.core.datamodel/ACHelperclasses/CommandLineHelper.cs` | `ConfigCurrentDir` getter step-by-step `Console.WriteLine` tracing (§9.9) |
| `gip.core.datamodel/ACHelperclasses/Runtime/ACActivator.cs` | `CreateInstance` null-path/catch tracing (3 `Console.WriteLine`) |
| `gip.core.datamodel/ACQueryDefinition/ACQueryDefinition.cs` | `CreateRootQueryDefinition` tracing (1 `Console.WriteLine`) |
| `gip.iplus.client.avui/Views/LoginView.axaml.cs` (~line 146) | Auto-login bypass block (`AutoLoginBypassEnabled` const + `ApplyAutoLoginBypass` + `SetBypassSetting`) — currently `false` |
| `gip.iplus.client.avui/Views/LoginView.axaml.cs` | `ApplyConnectionSettingsFromHelper`: MARS removal + `Integrated Security=False` override (TEMPORARY, §9.4) |

**SqlClient fork repo (`iplus/browser-transport`):**
| File | What |
|---|---|
| `src/.../ManagedSni/SyncHttpTunnelStream.netcore.cs` | `SyncHttpTunnelStream: Read/Write ...` `Console.WriteLine` tracing (2) |
| `src/.../ManagedSni/SniMarsConnection.netcore.cs` | pull-based demux tracing `Console.WriteLine` (2) |

### 9.18 Current state (2026-10-11, first commit)

- **Working end-to-end:** browser WASM client → sync-XHR shim → relay → SQL Server 2019,
  with MARS (pull-based demux, §9.11) and iPlus user login (managed-MD5 fallback, §9.12).
  Browser WCF transport (`/wcfbrowser`) implemented per §9.13–9.16, not yet tested.
- **Browser settings persistence:** client-local settings (connection settings +
  app `usersettings.json`) persist via `localStorage` (`BrowserSettingsStorage`,
  `window.iPlusSettings` JS bridge in `index.html`); DB-backed settings always worked.
- **Consumption mode:** iPlus currently consumes the fork via **ProjectReference**
  (`UseSqlClientForkProjectReference=True` in `Directory.Build.props`). Next step: pack
  `7.1.1-iplus.1` NuGet packages and switch iPlus to package mode to verify restore.
- **Commits:** first local commits of both repos (iPlus `main`, fork `iplus/browser-transport`);
  push deferred until the NuGet round-trip (pack → restore → runtime test) succeeds.
