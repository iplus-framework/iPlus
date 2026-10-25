# gip.tool.dbscript

Cross-platform command line tool that generates the **initial install script**
(`initial-install.sql`) for a fresh iPlus database from a reference database.

The script contains the complete schema plus the seed/master data, so a new
installation can be created without restoring a backup or shipping `.mdf` files.

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
                                              initial-install.sql
```

The installer / first-run flow then applies:

1. `initial-install.sql` — **fresh installs only**
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
# Full script (schema + seed data)
gip.tool.dbscript generate \
  -c "data source=localhost;Initial catalog=iPlusMESV5;Trusted_Connection=False;Encrypt=False;user id=sa;password=***;" \
  -o ./out/initial-install.sql \
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

`seed-tables.json` decides **which** tables are exported. Only master data is
listed; customer data (orders, postings, logs, runtime state) is never exported.

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

**Review `seed-tables.json` whenever the EF model gains a new master-data
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
