## gip.iplus.console

Headless console host for the iplus-framework. It starts the full iPlus runtime
(database connection, AC root, web services) without any UI — intended for
servers, automation and **AI agent sessions**.

## Quick start

```bash
# from the build output directory (bin/Debug/net10.0 or bin/Release/net10.0)
./gip.iplus.console
```

- Default login is user `00` / password `00` (no arguments needed).
- The service stays running until it is asked to stop (see below).
- On startup it listens on the configured HTTP ports (e.g. 8750, 8751, 8001).

### Command line arguments

| Argument | Meaning |
|---|---|
| `/U<name>` | Login user name (default: `00`) |
| `/P<password>` | Login password (default: `00`) |
| `/WCFOff` | **Do not start the WCF/web services.** Only use this if you are *not* testing anything with clients. Normally leave it off so clients can connect. |
| `/Simulation` | Run in simulation mode |

### Stopping the service

| Mode | How to stop |
|---|---|
| Interactive terminal | Press **Enter** or **Ctrl+C** |
| Headless (stdin redirected/closed, e.g. agents, CI) | Send **SIGTERM** (`kill <pid>`, `timeout`, ...) or **Ctrl+C** |

The service always shuts down gracefully (`ACDeInit`) — never `kill -9` unless
it is hung.

## Logging

Logging is configured in `gip.iplus.console/App.config` (section
`Logging/LoggingConfiguration`, same mechanism as the Desktop client).
Log files are written to `./Logs` relative to the working directory:

```
Logs/iPlusConsoleDebugLog_<yyyyMMdd>.<n>.txt
```

There are two ways to instrument/debug:

1. **Console output** — startup and status messages are printed to stdout.
   Redirect to a file if needed: `./gip.iplus.console > console.log 2>&1`
2. **Log files** — configured via `App.config`. The default configuration logs
   *all* message levels (`MessageType="Default"`, `Source="*"`) to the DebugLog.
   Adjust `LoggingTypes` to filter by `MessageType` (`Debug`, `Info`, `Warning`,
   `Failure`, `Error`, `Exception`, `Command`) and/or `Source`.

## Notes for AI agents

- Start the console **without** `/WCFOff` unless the task explicitly does not
  involve client connectivity — `/WCFOff` disables the web services that
  clients connect to.
- Run it with stdin redirected (`< /dev/null` or equivalent) so it runs
  headless; it will keep running until you send SIGTERM or Ctrl+C.
- Wait for the line `Service is ready for requests.` before assuming startup
  is complete. On login failure you will see `Login Error.` plus the reason
  (e.g. wrong credentials, DB connection failed) and the process exits.
- Default credentials are `00`/`00`; override with `/U<user> /P<password>`.
- For debugging, prefer the log files in `./Logs` (see above) — they contain
  the full message stream including exceptions, not just the console output.
- The database connection string is in `App.config` (`iPlusV5_Entities`).

