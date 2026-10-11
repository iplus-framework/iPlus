# Design Draft: SQL Tunnel Provider for Browser (WASM) Clients

> Status: **Draft / Discussion** — to be implemented in a separate session (possibly with subagents).
> Date: 2026-10-08
> **Decision (2026-10-08):** Preferred approach is **Option B′** (§2b — fork SqlClient's TDS
> parser, WebSocket transport, dumb relay gateway), gated by a go/no-go spike (Task 0).
> **Option C** (§3 — ADO.NET tunnel provider) is the fallback.
> **Update (2026-10-09):** SqlClient fork created (`iplus-framework/SqlClient`, branch
> `release/6.1`); concrete fork setup + implementation steps added to §2b. Kick off the work in
> a new session from there.

## 1. Problem Statement

The iPlus Browser client (Avalonia, `net10.0-browser`, WebAssembly) is a **fat client**:
Business objects (`ACBSONav` and derived) run **in the browser** and use Entity Framework Core
with the SQL Server provider (`Microsoft.Data.SqlClient`) to materialize and save entities
client-side (see `ACAccessNav<T>`, `EntityObjectExtension.DbContextOptions<iPlusV5Context>(ConnectionString)`).

**Constraint:** Browser WebAssembly has **no raw TCP sockets**. `Microsoft.Data.SqlClient`
explicitly refuses to run on browser WASM:

```
PlatformNotSupportedException: Microsoft.Data.SqlClient is not supported on this platform.
```

(Verified empirically 2026-10-08 via a probe connection in the running browser client.)

**Goal:** Let the unchanged fat-client programming model (EF Core + SqlClient types) work in the
browser by tunneling SQL execution over WebSocket/HTTP to an iPlus application server, which
executes it against the real SQL Server.

**Non-goals:**
- No TDS protocol reimplementation *from scratch* (see §2b for the fork-based variant that reuses
  Microsoft's own TDS parser).
- No moving BSOs server-side ("thin client") — that changes the product.
- Future cloud backends (Cosmos DB, Snowflake) do **not** need this tunnel: they have official
  EF Core / ADO.NET providers that work over HTTP and hence on WASM.

## 2. Architecture

```
+------------------- Browser (WASM) -------------------+
|  BSOs (ACBSONav...)                                  |
|    -> ACAccessNav<T> -> EF Core                      |
|    -> SqlServer EF provider (unchanged)              |
|    -> TunnelDbConnection : DbConnection   (NEW)      |
|         serializes commands/params/results           |
|         over WebSocket (System.Net.WebSockets        |
|         ClientWebSocket works on WASM via fetch)     |
+-------------------------|----------------------------+
                          | WebSocket (binary frames)
+-------------------------v----------------------------+
|  iPlus Application Server (WCFServiceManager host)   |
|    SqlTunnelRelay (NEW, ASP.NET Core endpoint)      |
|      - authenticates user (reuse iPlus auth)         |
|      - pins ONE SqlConnection per client session     |
|      - injects credentials server-side               |
|      - executes commands, streams results back       |
+-------------------------|----------------------------+
                          | TCP (TDS)
                    SQL Server (on-prem)
```

Key properties:
- **Session pinning:** transactions (`BeginTransaction`) require a sticky server-side connection
  → WebSocket (not stateless HTTP) is the transport. One `SqlConnection` per tunnel session.
- **Credentials never reach the browser.** The relay resolves the connection string per
  authenticated user (per `VBUserInstance` / project assignment). This is *more* secure than
  today's fat client.
- **Multiple server instances** work as with WCF today: the client connects to the instance
  whose application projects are assigned to the user. Internet access = existing gateway +
  port redirection (not recommended, but possible).

## 2b. Option B′ — SqlClient TDS fork (PREFERRED, spike first)

Microsoft's SqlClient repo contains a TDS parser under
`src/Microsoft.Data.SqlClient/tests/tools/TDS/` (`TDSClientParser.cs` etc.). It is **test
infrastructure** (mocks a SQL Server endpoint for the SqlClient test suite), not a shipped API —
but it implements the TDS client protocol stack and can be forked and vendored.

### Architecture

```
+---------------- Browser (WASM) -----------------+
| Forked SqlClient:                               |
|   TDS parser (unchanged)                        |
|   transport swapped: TcpClient/SslStream        |
|     -> Stream wrapper over ClientWebSocket      |
+---------------------|---------------------------+
                      | WebSocket (raw TDS bytes)
+---------------------v---------------------------+
| Gateway (dumb byte relay, in app server or      |
| standalone): WebSocket <-> TcpClient to SQL     |
| Server. No TDS parsing. Optionally terminates   |
| TDS 8.0 TLS for inspection/authorization.       |
+---------------------|---------------------------+
                  TCP (TDS)
               SQL Server
```

- TDS is a stream protocol: as long as the transport delivers the same ordered bytes, the parser
  above it is agnostic. The gateway is a blind pipe (or a TLS-terminating authorizing proxy).
- **No EF work at all** — real SqlClient runs unchanged (MARS, TVPs, bulk copy, every type:
  all work for free).

### Advantages over Option C
- 100% protocol fidelity — zero per-feature work, ever, once the transport swap is done.
- Task 5 (EF provider subclassing) disappears entirely.

### Disadvantages / risks
- The TDS parser is test tooling: not production-hardened, no stability guarantee. Corners the
  tests don't exercise (SSPI/NTLM/Kerberos login, TDS 8.0 TLS handshake order, MARS, INSERT BULK,
  federated auth) must be completed by us.
- Maintenance coupling: every SqlClient update touching the TDS layer requires re-porting the fork.
- Weaker security model: the gateway is a blind proxy — per-user authorization happens only via
  TDS login, so SQL credentials flow through the tunnel (or the gateway must rewrite login
  packets = parsing TDS after all). Mitigation: TLS on the WebSocket + gateway-side IP/port
  allow-listing + iPlus-level auth on the WebSocket handshake before relaying.

### Security model (with two-factor gateway auth)

The naive blind-relay weakness is *unauthenticated tunnel access*. Closing it with a two-stage
gate makes B′ roughly as safe as today's fat client — arguably safer:

1. **Transport:** TLS on the gRPC/WebSocket session. Nobody on the network sees the TDS bytes
   (including the SQL login inside them) — same guarantee as the fat client's direct
   TCP + TDS encryption today.
2. **Stage 1 — OAuth:** validates the client identity on the WebSocket handshake (token expiry,
   revocation, IdP MFA come for free).
3. **Stage 2 — iPlus login:** validates the human and determines *which* database/instance the
   relay connects to. The gateway resolves the connection string **server-side** — the browser
   never holds SQL credentials (an improvement over today's fat client, where the connection
   string with password sits in the client config).
4. Only after both stages succeed does the relay open the `TcpClient` to SQL Server. Before
   that, the socket is a dead end.

**Residual risks & mitigations:**

| Risk | Severity | Mitigation |
|---|---|---|
| Session hijack (stolen WebSocket session = DB access as that user until disconnect) | Medium | Short-lived OAuth tokens, re-auth on reconnect, idle timeout, bind session to client cert/IP where possible |
| No per-query authorization (blind pipe; SQL Server role permissions are the only limit) | Low — unchanged from today's fat client | Status quo; if per-query policy is ever wanted, that's when Option C's parsing relay wins |
| Client-generated SQL (compromised client can send arbitrary SQL as that user) | Low — unchanged from today | Identical to current fat client; not a regression |
| Gateway compromise (holds resolved connection strings) | Low | On-prem, same trust zone as WCF servers; harden like any server |
| Compromised client machine (malware with user's OAuth session) | Low — unchanged | Same as fat client today; IdP MFA helps |

**Bottom line:** the two-factor gate closes the one *new* hole (unauthenticated tunnel use);
everything else matches the existing fat-client risk profile, with one improvement (no SQL
credentials in the client) and one new operational duty (protect the gateway like a WCF server).

### Fork setup (DONE 2026-10-09)

- Fork: `git@github.com:iplus-framework/SqlClient.git` (all branches copied, GitHub Actions
  disabled on the fork).
- Source branch: **`release/6.1`** — matches the pinned `Microsoft.Data.SqlClient 6.1.1` in the
  EF fork's `Directory.Packages.props`. Working branch: `iplus/browser-transport`.
- Local clone: `/home/damir/SHARED/Devel/iPlusGit/V5/SqlClient` (sibling of `ef_main_iPlus`,
  same pattern as the EF fork referenced by `build/EntityFramework.Build.props`).
  **Cloned & branched 2026-10-09** — `release/6.1` is at 6.1.7+2, so the baseline package
  version is `6.1.7-iplus.0`.

### Implementation steps (start in a new session)

1. **Clone & branch** — ✅ DONE 2026-10-09:
   `/home/damir/SHARED/Devel/iPlusGit/V5/SqlClient`, branch `iplus/browser-transport`
   (tracking `origin/release/6.1`), `v6.1.1` confirmed ancestor of HEAD.
   **Note:** `release/6.1` is currently at 6.1.7+2 commits — baseline pack should be
   `6.1.7-iplus.0` (not `6.1.1-iplus.0`), and the EF fork pin updated to match.
   GitHub Actions could not be disabled via CLI (no `gh`) — disable once in the web UI
   (repo → Actions → Disable actions) before the first push.
2. **Baseline build (no changes!):** build `Microsoft.Data.SqlClient` for `net10.0`, pack it
   (e.g. version `6.1.1-iplus.0`), and point the EF fork + iPlus at it to prove the forked
   package is a drop-in replacement BEFORE any transport work:
   - `ef_main_iPlus/Directory.Packages.props`: `<PackageVersion Include="Microsoft.Data.SqlClient"
     Version="6.1.1-iplus.0" />` (restore from a local folder feed or push to the iPlus feed)
   - Rebuild `EFCore.SqlServer` + iPlus Desktop → run a login/search smoke test.
3. **Add browser TFM:** multi-target `net10.0;net10.0-browser` in `Microsoft.Data.SqlClient.csproj`.
   Remove the browser `PlatformNotSupportedException` guard for the browser TFM only; keep
   `net10.0` behavior byte-identical (desktop/console/service/Android consumers unaffected).
4. **Transport swap:** introduce an internal stream abstraction over the current
   `TcpClient`/`NetworkStream`/`SslStream` transport; implement `WebSocketStream` (full-duplex,
   `ClientWebSocket` backend) selected for the browser TFM. Use `tests/tools/TDS/` as the
   reference for what the parser expects from the transport.
5. **Spike round-trip (Task 0 success criteria):** dumb relay gateway (Kestrel WebSocket
   endpoint <-> `TcpClient` to SQL Server) → from a WASM test page: prelogin + login handshake +
   one query returns results. Go/no-go for B′ vs. Option C.

**Assembly identity decision:** keep `AssemblyName = Microsoft.Data.SqlClient` and the same
public key; version fork packages as `6.1.1-iplus.N`. This makes the fork a drop-in replacement
for *all* consumers (EF fork, `gip.core.datamodel`'s direct `SqlConnection` usage, Android
client) with zero source changes. Renaming would require touching every direct reference.

### Spike (Task 0, go/no-go, ~2–3 days)
1. Fork `TDS.EndPoint` client parser, vendor it pinned to a SqlClient version.
2. Write `WebSocketStream : Stream` (full-duplex, WASM-compatible `ClientWebSocket` backend).
3. Dumb relay: Kestrel WebSocket endpoint <-> `TcpClient` to a local SQL Server.
4. Success criteria: TDS prelogin + login handshake round-trips, one simple query executes and
   returns results through the relay, from a WASM test page.
5. If the spike drags into SSPI/TLS corner-cases → fall back to Option C (§3).

## 3. Components (Option C — fallback if the B′ spike fails)

### 3.1 Shared: Tunnel protocol (new project, e.g. `gip.core.sqltunnel.shared`)

Message shapes (binary, e.g. MessagePack or JSON for v1 — start with JSON, optimize later):

```
// Client -> Server
OpenSession      { userId, authToken, databaseKey }        // server resolves conn string
CloseSession     { }
BeginTransaction { isolationLevel }
Commit           { }
Rollback         { }
Execute          { commandText, parameters[], commandType, timeout }
                 -> returns header { rowsAffected, hasResultSet } then streamed rows
Cancel           { executionId }
Ping             { }

// Parameter serialization
Parameter        { name, sqlDbType, size, precision, scale, direction, value }
                 // SqlParameter is fully describable; no SqlClient types cross the wire

// Server -> Client
ResultHeader     { executionId, fieldCount, fields[] { name, sqlDbType, isNullable } }
ResultRows       { executionId, rows[][] }        // chunked, N rows per frame
ResultDone       { executionId, rowsAffected, outputParams[], error? }
Error            { code, message, innerMessage }
```

Notes:
- `Execute` is request/response but **result rows stream in chunks** so large result sets
  don't buffer fully on the server. `NavTakeCount` keeps typical BSO result sets small.
- Output/return parameters are returned in `ResultDone`.
- Every message carries a correlation id (executionId) to allow `Cancel`.

### 3.2 Client: ADO.NET shim (new project, e.g. `gip.core.sqltunnel.client`)

Implement the minimal ADO.NET surface EF Core needs:

- `TunnelDbConnection : DbConnection` — manages the WebSocket + session lifecycle.
- `TunnelDbCommand : DbCommand` — serializes command + `SqlParameter`s, streams `DbDataReader`.
- `TunnelDbDataReader : DbDataReader` — materializes chunked rows; typed getters map
  `SqlDbType` → CLR types.
- `TunnelDbTransaction : DbTransaction`.
- Reuse **`SqlParameter`/`SqlDbType` types directly** (they live in `Microsoft.Data.SqlClient`
  which *does* compile/run on WASM — only the *transport* is unsupported). This keeps the EF
  SqlServer provider's type mappings intact.

**EF integration:** subclass the EF SqlServer provider and override only the connection
factory to return `TunnelDbConnection` (or use `UseSqlServer(...)` with a custom
`IRelationalConnection`). Wire-up in `EntityObjectExtension.DbContextOptions` behind a
platform check:

```csharp
// pseudo
if (OperatingSystem.IsBrowser())
    options.UseSqlServerTunnel(tunnelEndpoint, authToken);
else
    options.UseSqlServer(ConnectionString);
```

### 3.3 Server: Relay (in `gip.core.autocomponent` / hosted by WCFServiceManager's ASP.NET host)

- ASP.NET Core WebSocket endpoint `/sqltunnel` (the `WCFServiceManager` already builds a
  `WebApplication` host when `ServiceAppEnbledHTTP` is set — add the endpoint there; for
  net.tcp-only servers, either force HTTP for tunnel or host a small Kestrel sidecar).
- Per WebSocket: authenticate (reuse iPlus login/token), resolve connection string
  server-side, open **one** `SqlConnection`, loop: read message → execute → stream chunks.
- Map `SqlException` → `Error` message (preserve number/severity for EF retry logic).
- Idle timeout + cleanup on socket close (dispose connection, rollback open transaction).

## 4. Implementation Plan (subagent-friendly)

| # | Task | Depends on | Est. (AI-assisted) |
|---|------|-----------|--------------------|
| 0 | **Spike Option B′:** fork TDS parser, WebSocketStream, dumb relay, login+query round-trip from WASM | – | 2–3 days |
| 1 | Spike: `ClientWebSocket` + streaming on browser WASM (echo test against local Kestrel) | – | 0.5 day |
| 2 | Shared protocol project + message serialization (Option C only) | – | 1 day |
| 3 | Server relay endpoint (auth, session pinning, execute/stream) (Option C only) | 1, 2 | 2–3 days |
| 4 | Client `TunnelDbConnection/Command/Reader/Transaction` (Option C only) | 2 | 3–4 days |
| 5 | EF SqlServer provider subclass + `EntityObjectExtension` wiring (browser only) (Option C only) | 4 | 1–2 days |
| 6 | Integration test: browser client login → `ACBSONav` search → navigate → save | 0 *or* 3,4,5 | 2–3 days |
| 7 | Hardening: cancellation, reconnect/resume, MARS check, TVP check, perf (binary serialization) | 6 | 3–5 days |
| 8 | (B′ only) Complete TDS parser corners: SSPI, TDS 8.0 TLS, INSERT BULK; gateway auth hook | 0 | 3–5 days |

**If the Task-0 spike succeeds (B′):** total ≈ 1.5–2.5 weeks (0, 6, 7, 8 — tasks 2–5 skipped).
**If it fails (C):** total ≈ 2–3 weeks (1–7).

**Total: roughly 2–3 weeks** — this estimate is for **AI-assisted development** (me/subagents
writing the code, you reviewing and testing). For a human developer working alone on this
codebase, expect **4–6 weeks**, mostly due to iPlus framework integration and testing against
real BSOs. The biggest schedule risks are task 0 (TDS corners) / task 5 (EF provider internals)
and 7 (edge cases).

### Subagent split suggestion
- **Agent A (spike B′):** task 0 (+8) — fork TDS parser, WebSocketStream, relay; needs a local
  SQL Server to test against. Fork already prepared (see "Fork setup" in §2b).
- **Agent B (protocol + server, Option C):** tasks 2, 3 — pure server-side C#, testable with a
  console client.
- **Agent C (client shim, Option C):** tasks 4, 5 — needs the Avalonia fork/browser build
  environment.
- **Agent D (integration):** task 6 — runs the browser client, Playwright-driven verification
  (same tooling as the 2026-10-08 session), reports failures back to A/B/C.

## 5. Risks / Open Questions

1. **EF provider subclassing surface:** how much of `SqlServerConnection` is sealed/overridable
   in the EF Core version in use → verify in task 5 spike. Fallback: full custom provider
   reusing `SqlParameter` types (more work, still bounded).
   **Plan B (if subclassing fails entirely):** switch the tunnel payload from *SQL text* to
   *serialized query expression trees* (Remote.Linq-style, but self-implemented — Remote.Linq
   itself is dormant). The server executes the tree against its own `DbContext` and returns
   entities; the client `Attach`es them so the change tracker still works for `SaveChanges`.
   Same wire protocol, different `Execute` payload. More work than the transport shim, but
   bounded — and it removes the "server executes client-generated SQL" trust question.
2. **MARS / multiple active readers:** EF sometimes holds two readers. Either disable MARS in
   tunnel v1 (serialize readers) or multiplex readers over the socket (v2).
3. **Table-valued parameters & SqlBulkCopy:** not needed by most BSOs; defer.
4. **Type fidelity:** `datetime`, `decimal` scale, `varbinary` streaming — cover with a
   type-mapping test matrix (task 4).
5. **Security:** tunnel endpoint must require the same auth as WCF; rate-limit; TLS via
   existing HTTPS setup (port 7169 dev / 8443 prod pattern).
6. **Reconnect semantics:** if the WebSocket drops mid-transaction, the server must roll back;
   client surfaces the error to the BSO (same as a dropped SQL connection today).

## 6. Decision Record

- **Chosen: Option B′ — fork SqlClient's TDS parser, swap transport to WebSocket, dumb byte
  relay gateway.** Spike (task 0) is the go/no-go gate; on success this is the cheapest *complete*
  solution (no EF work, full protocol fidelity).
- Fallback: **Option C — ADO.NET tunnel provider over WebSocket** (transport shim reusing
  SqlClient types + EF SqlServer provider) if the B′ spike fails.
- Rejected: TDS reimplementation from scratch (months of work, undocumented protocol).
- Rejected: thin client (BSOs must run client-side — product requirement).
- WCF client↔server communication: switch to HTTP binding for browser clients via
  `VBUserInstance` setting (separate track, independent of this tunnel).
  **Update (2026-10-10):** plain HTTP *WCF* binding is not viable on WASM either — see §9.
  The browser client gets an async-native WebSocket/HTTP remoting transport reusing the
  `WCFMessage` contracts instead (§9); desktop clients keep the existing WCF channels.
- **Update (2026-10-10):** the B′ WebSocket transport as implemented (sync `Stream` over
  `ClientWebSocket`) **cannot work on the browser main thread** — see §8. The transport must
  become sync-capable via the **sync-XHR HTTP shim (§8, Option A)** or the whole client must
  move to a threading-enabled WASM runtime (§8, Option B/C). Option A is preferred.

## 7. Evaluated Alternatives (2026-10-08)

- **OData endpoint** — rejected as the *primary* mechanism: server owns the `DbContext`, no
  client-side change tracking/`SaveChanges`, would force DTO-based rewrite of all BSOs
  (= thin client). Kept as a possible *additional* API surface for integrations later.
- **Remote.Linq** — rejected as a dependency (dormant project, no CUD/change-tracking story),
  but its *pattern* (expression trees over the wire) is adopted as **Plan B** in §5.1 if the
  EF provider subclassing spike fails.

## 8. Browser WASM Sync-Blocking Limitation (discovered 2026-10-10)

### 8.1 What happened

The Step-5 browser spike (desktop GO, see `SqlTunnel-OptionB-Implementation.md`) failed in the
browser client with:

```
SqlException: ... (provider: TCP Provider, error: 35 - An internal exception was caught)
  inner: PlatformNotSupportedException: Cannot wait on monitors on this runtime.
```

**Root cause:** the PNSE was thrown inside `SniWebSocketHandle`'s constructor — the only place
that wraps exceptions into a `SqlException` via `ReportTunnelSNIError` (which is also why the
provider string says "TCP Provider": the tunnel handle reports `TCP_PROV`). The culprit:

```csharp
webSocket.ConnectAsync(new Uri(tunnelUrl), cts.Token).GetAwaiter().GetResult();
```

On the browser main thread there are **no real threads**. Blocking on an incomplete `Task`
(`.GetAwaiter().GetResult()`, `Task.Wait`, `Task.Result`) falls back to `Monitor.Wait`, which
the single-threaded WASM runtime explicitly rejects. This is a documented dotnet/runtime
limitation (their own tests gate on `PlatformDetection.IsMultithreadingSupported` with the
comment "Browser PNSE: Cannot wait on monitors").

### 8.2 The general rule (applies to far more than the connect)

| API | Browser main thread (single-threaded WASM) |
|---|---|
| `async`/`await`, `Task` continuations | ✅ works (JS event loop) |
| `Monitor.Enter` / `lock` (uncontended) | ✅ works |
| `Monitor.Wait` / `Monitor.TryEnter(obj, timeout)` | ❌ PNSE |
| `EventWaitHandle.WaitOne`, `ManualResetEventSlim.Wait`, contended `SemaphoreSlim.Wait` | ❌ PNSE |
| `new Thread(...).Start()` | ❌ PNSE |
| `Thread.Sleep` | ⚠️ works but freezes the whole UI; **does not pump the JS event loop** |
| Raw TCP sockets | ❌ not available at all |
| WebSocket / HTTP (async) | ✅ works |

**Consequence:** a WebSocket message is a macrotask that can only be dispatched when the JS
stack is empty. Therefore **no spin/sleep trick can ever observe an incoming frame** —
sync-over-async for network I/O is *fundamentally impossible* on the browser main thread.
This hits every synchronous TDS read (`WebSocketStream.Read` → `ReceiveAsync(...).GetResult()`),
not just the connect. And iPlus's data layer is **sync EF Core**, so the whole login/query
path is sync.

### 8.3 Options

| Option | Effort | Description |
|---|---|---|
| **A. Sync-XHR HTTP shim (PREFERRED)** | Low–medium | Add HTTP endpoints to `PASqlTunnelHost`: `POST /sqltunnel-sync/{id}/send` (body = TDS bytes) and `POST /sqltunnel-sync/{id}/recv` (long-poll, returns bytes). The browser `WebSocketStream` (browser variant) reads/writes via **synchronous `XMLHttpRequest`** — the browser performs the wait internally on its own threads; no JS event loop re-entrancy, no `Monitor.Wait`. Sync XHR is deprecated but fully functional in Chrome/Firefox. The relay keeps one live SQL socket per session. iPlus stays 100% sync/unchanged. |
| **B. JS Web Worker + SharedArrayBuffer pipe** | Medium–high | Classic SQLite-WASM pattern. Needs COOP/COEP headers + ring-buffer plumbing. More moving parts than A. |
| **C. WASM threads (.NET worker thread)** | High | .NET 9/10 supports real threads on WASM (Web Workers + SharedArrayBuffer, threading-enabled build). `Monitor.Wait`/sync TDS then work — but only on worker threads; the main thread still cannot block, so UI calls must be marshaled. Avalonia support uncertain; build/packaging complexity high. |
| **D. Async all the way in iPlus** | Very high | Sync EF Core everywhere — not feasible. |

### 8.4 Option A design sketch (sync-XHR shim)

```
Browser (WASM)                         PASqlTunnelHost (app server)
--------------                         ----------------------------
POST /sqltunnel-sync/session           -> creates session id, opens TcpClient to SQL Server
   {server, port} -> {sessionId}
POST /sqltunnel-sync/{id}/send         -> writes body bytes to the SQL socket
   body: TDS bytes        -> 200
POST /sqltunnel-sync/{id}/recv         -> long-poll: reads next chunk from SQL socket
   (server holds request until data or timeout) -> 200 body: TDS bytes | 204 no-data-retry
DELETE /sqltunnel-sync/{id}            -> closes SQL socket, drops session
```

- Fork-side: `#if IPLUS_BROWSER` variant of the transport (`SqlHttpSyncHandle` or a
  `WebSocketStream` browser variant) implementing the same `Stream` contract with sync XHR
  via `JSImport`/`System.Net.Http` sync-over-fetch shim. Desktop keeps the WebSocket stream.
- The relay keeps the existing `/sqltunnel` WebSocket endpoint for desktop; the HTTP shim is
  additive.
- CORS: relay must send permissive CORS headers for the browser origin (it already handles
  the WebSocket upgrade).
- Security: same two-factor gate as §2b (auth on session create, then blind pipe). Sync XHR
  sends no credentials cross-origin unless `withCredentials`; use the same token header.
- Caveat: sync XHR blocks the UI thread for the duration of each request — acceptable for
  TDS chunk sizes (≤ packet size, typically < 8 KB) and LAN latencies; long-poll recv must
  return quickly (e.g. ≤ 1 s server-side hold) so the UI never freezes for long.

### 8.5 WCF note (why §9 exists)

The same limitation kills the thread-based `WCFClientChannel` architecture on the browser main
thread (`new Thread`, `EventWaitHandle.WaitOne`, `Thread.Sleep(100)` in the dispatch loops —
all PNSE or UI-freezing). WSDualHttpBinding additionally does not exist in .NET Core WCF at
all (dotnet/wcf#4614). See §9 for the browser WCF transport design.

## 9. Browser WCF Transport (sketch, separate track)

Goal: browser proxy-ACComponents talk to the real server-side instances without the thread-
based channel machinery, reusing the existing `WCFMessage` data contracts.

```
Browser (WASM)                          iPlus App Server
--------------                          ----------------
ACBSONav / proxy components
   -> WCFClientChannel (browser variant)
        async message pump (no threads):
        - SendQueue drained by an async loop (await-based)
        - incoming messages arrive via WebSocket push
        -> WebSocket / HTTP endpoint on WCFServiceManager's ASP.NET host
                                           -> WCFService (same IWCFService semantics)
                                           -> dispatch to real ACComponents
```

- **Reuse:** `WCFMessage`, `ACMethodInvocation`, `ACPropertyValueMessage`, `ACSubscriptionMessage`
  contracts unchanged; serialize over binary JSON (or the existing DataContract serializer).
- **Server push:** WebSocket (works async on WASM) replaces the WSDualHttp callback.
  `EnqeueReceivedMessageFromPeer` stays the single entry point — only its caller changes.
- **Client:** a browser variant of `WCFClientChannel` with the producer/consumer threads
  replaced by async pumps (`Channel<T>`/`System.Threading.Channels` + `await foreach`),
  selected via platform check. `SendACMessageToServer` becomes fire-and-enqueue as today.
- **Hosting:** add the endpoint to the `WebApplication` host that `WCFServiceManager` already
  builds when `ServiceAppEnbledHTTP` is set (net.tcp-only servers need HTTP enabled for
  browser clients).
- **Auth:** same iPlus login/token as the SQL tunnel gate (§2b security model).

*Decision pending — to be finalized, then recorded in `SqlTunnel-OptionB-Implementation.md`.*
