# iPlus V5 — Docker evaluation bundle

One command to a running iPlus system: SQL Server 2022 + the iPlus service + the
browser client. No local SQL Server install, no `.mdf` files.

## Quick start

```bash
# 1. Configure (edit the SA password!)
cp deploy/docker/.env.example deploy/docker/.env

# 2. Build the artifacts the images consume
./build/publish-release.sh --platform browser,service --keep-intermediate

# 3. Generate the initial install script (needs sqlpackage + a reference DB)
dotnet run --project gip.tool.dbscript -c Release -- \
  generate -c "<reference-db-connection-string>" \
  -o ./artifacts/release/initial-install.sql --suite-version 5.0.0

# 4. Start
docker compose -f deploy/docker/docker-compose.yml --env-file deploy/docker/.env up -d

# 5. Open http://localhost:8080
```

## What runs

| Service | Image | Purpose |
|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | Database |
| `mssql-init` | same | Applies `initial-install.sql` once, then exits |
| `iplus-service` | built from `Dockerfile.service` | iPlus server (WCF + SQL Tunnel) |
| `iplus-browser` | built from `Dockerfile.browser` | nginx serving the WASM client |

## Why the images do not build iPlus

The images consume the output of `build/publish-release.sh`, so the container and
the GitHub Release artifact are byte-identical. Building inside the image would
require the Avalonia fork checkout and produce a second, divergent build.

`--keep-intermediate` is required: it keeps the raw publish folders in
`artifacts/release/.work/`, which the Dockerfiles copy from.

## Database initialisation

`mssql-init` runs [docker-init-db.sh](./docker-init-db.sh):

1. waits for SQL Server to accept connections,
2. skips everything when the database already exists (existing installations only
   run the incremental `DbScripts` updates),
3. otherwise creates the database and applies `initial-install.sql`,
4. writes a marker file into the `mssql-init-state` volume so the script is never
   applied twice.

## Configuration

The service renders `/app/ConnectionStrings.config` from environment variables at
start-up ([entrypoint-service.sh](./entrypoint-service.sh)), so the database
password is never baked into an image or committed.

| Variable | Default | Meaning |
|---|---|---|
| `MSSQL_SA_PASSWORD` | — (required) | SQL Server SA password |
| `IPLUS_DB_NAME` | `iPlusV5` | Database name |
| `IPLUS_VERSION` | `5.0.0` | Must match the artifacts from `publish-release.sh` |
| `IPLUS_HTTP_PORT` | `8080` | Published browser client port |
| `IPLUS_TUNNEL_PORT` | `8751` | Published SQL Tunnel port |
| `TZ` | `UTC` | Container timezone |

## Licenses

Put `*.gip` license files into `deploy/docker/licenses/`; the directory is mounted
read-only at `/app/licenses`. Without a license the system runs in evaluation mode.

## Production notes

This bundle is for **evaluation and development**:

- Use a managed SQL Server instead of the container for production.
- Put a TLS-terminating reverse proxy in front of the browser client.
- Do not expose the SQL Server port publicly.
- Replace the default `superuser` password created by the initial install script.
