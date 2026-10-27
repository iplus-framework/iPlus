# gip.tool.dbscript

Cross-platform command line tool that generates the **initial install script**
for a fresh iPlus database from a reference database.

The script contains the complete schema plus the seed/master data, so a new
installation can be created without restoring a backup or shipping `.mdf` files.

Two scripts are produced for the two products:

| Script | Reference database | Seed configuration |
|---|---|---|
| `initial-install-iplus.sql` | **iPlusV5** (framework only) | `seed-tables.json` |
| `initial-install-mes.sql` | **iPlusMESV5** (framework + MES) | `seed-tables-mes.json` |

`IPCDrivesV5` is a demo/example project for playing, experimenting and
demonstration — it is never used to generate a distribution script.

## Why

V4 shipped database backups. That leaks customer data, is version-bound and does
not work for containers. This tool produces a reviewable, versioned SQL script
that is attached to the same GitHub Release as the binaries.

## Pipeline

```
reference database ──► sqlpackage /Action:Extract ──► reference.dacpac
                   └─► sqlpackage /Action:Script  ──► reference-schema.sql
                   └─► SELECT from seed tables    ──► seed INSERTs
                                                        │
                                                        ▼
                                              initial-install-*.sql
```

The installer / first-run flow then applies:

1. `initial-install-iplus.sql` / `initial-install-mes.sql` — **fresh installs only**
2. `DbScripts` incremental updates (`gip.core.dbsyncer` / Ctrl-login model sync)
3. `VBControlScripts` design imports

Existing installations skip step 1 and only run the incremental parts.

## Requirements

- .NET 10 runtime
- `sqlpackage` on `PATH` (or `--sqlpackage <path>`):
  `dotnet tool install -g microsoft.sqlpackage`
- Read access to the reference database

## Usage

```bash
# Framework script (schema + seed data) from iPlusV5
gip.tool.dbscript generate \
  -c "data source=localhost;Initial catalog=iPlusV5;Trusted_Connection=False;Encrypt=False;user id=sa;******;" \
  -o ./out/initial-install-iplus.sql \
  --seed-config ./seed-tables.json \
  --suite-version 5.0.0

# MES script (framework + MES schema and master data) from iPlusMESV5
gip.tool.dbscript generate \
  -c "data source=localhost;Initial catalog=iPlusMESV5;Trusted_Connection=False;Encrypt=False;user id=sa;******;" \
  -o ./out/initial-install-mes.sql \
  --seed-config ./seed-tables-mes.json \
  --suite-version 5.0.0

# Seed data only (schema is applied by dbsyncer)
gip.tool.dbscript generate -c "<cs>" -o ./out/seed-only.sql --seed-only

# Print the license fingerprint of a database
gip.tool.dbscript fingerprint -c "<cs>"
```

### Options

| Option | Description |
|---|---|
| `-c, --connection-string` | Reference database connection string (same format as `iPlusModelUpdate.sh`) |
| `-o, --output` | Output path (default `initial-install.sql`) |
| `--seed-config` | Seed table configuration (default `seed-tables.json` next to the tool) |
| `--suite-version` | Version stamped into the script header |
| `--sqlpackage` | Path of the `sqlpackage` executable |
| `--seed-only` | Emit only the seed data section |
| `--keep-dacpac` | Keep the intermediate `.dacpac` / schema script |

## Seed configuration

The seed configuration decides **which** tables are exported. Only master data
is listed; customer data (orders, postings, logs, runtime state) is never
exported.

- `seed-tables.json` — framework tables (`AC*`, `VB*`, dbsyncer bookkeeping).
- `seed-tables-mes.json` — the same framework tables **plus** all `MD*` MES
  master-data tables. Use this one for a MES database.

```jsonc
{
  "defaultSchema": "dbo",
  "tables": [
    { "name": "ACClass" },
    { "name": "MD*" },                                  // prefix wildcard
    { "name": "VBUser", "filter": "IsSuperuser = 1" },  // row filter
    { "name": "VBConfig", "excludeColumns": ["XMLConfig"] }
  ],
  "excludeTables": [ "dbo.ACChangeLog" ]
}
```

Per table:

| Property | Default | Description |
|---|---|---|
| `name` | — | Table name; a trailing `*` is a prefix wildcard |
| `schema` | `defaultSchema` | Schema override |
| `filter` | none | `WHERE` clause (without the keyword) |
| `excludeColumns` | none | Columns that must not be exported |
| `insertOnlyWhenEmpty` | `true` | Wrap the INSERTs in `IF NOT EXISTS (...)` |

**Review both configurations whenever the EF model gains a new master-data
table.** A table that is missing here is silently absent from the distribution.

## Output guarantees

- Idempotent: every seed table is guarded by `IF NOT EXISTS (SELECT 1 FROM ...)`.
- `SET IDENTITY_INSERT` is emitted for identity columns and switched off again.
- All values are rendered as properly escaped T-SQL literals (no parameters),
  so the script is standalone.
- `GO` separators are never emitted inside a `BEGIN...END` block.
- The header states the suite version and that the script must only be applied
  to a fresh database.

## Notes

- `sqlpackage` is invoked as an external process on purpose: referencing DacFx
  would pin `Microsoft.Data.SqlClient` 5.x, which conflicts with the iPlus
  SqlClient fork (same assembly identity, different version).
- The generated script is **not** a backup. It contains no customer data and no
  file-stream / BLOB content beyond what the seed tables hold.
- The DACPAC is scripted with `/TargetServerVersion:Sql150`, i.e. the generated
  schema targets **SQL Server 2019** (the supported baseline).
