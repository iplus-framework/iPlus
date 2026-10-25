# iPlus V5 — Release & Distribution Process

This document describes how iPlus V5 is built, packaged and published under the
dual-licensing model. It is the operational counterpart of
[HANDOVER-dual-licensing-distribution.md](./HANDOVER-dual-licensing-distribution.md).

## Two channels, two audiences

| Channel | Content | Audience | Tooling |
|---|---|---|---|
| **Source / developer** | Source code + `iPlus.*` NuGet packages | Developers, open-source license | `build/pack-local.sh`, nuget.org |
| **Binary / end user** | Ready-to-run per-platform artifacts | End users, both editions | `build/publish-release.sh`, GitHub Releases |

One artifact must not serve both audiences. NuGet packages are for developers
composing their own launch projects; end users need published, self-contained
outputs.

## Artifacts per release

| Artifact | Platform | Produced by |
|---|---|---|
| `iplus-<ver>-desktop-win-x64.zip` | Windows desktop | `publish-release.sh --platform desktop-win` |
| `iplus-<ver>-desktop-linux-x64.zip` | Linux desktop | `publish-release.sh --platform desktop-linux` |
| `iplus-<ver>-browser.zip` | Browser (WASM, static files) | `publish-release.sh --platform browser` |
| `iplus-<ver>-android.apk` | Android | `publish-release.sh --platform android` |
| `iplus-<ver>-service-linux-x64.zip` | Server service (Linux) | `publish-release.sh --platform service` |
| `initial-install.sql` | Database (schema + seed data) | `gip.tool.dbscript` |
| `SHA256SUMS` | Checksums for all of the above | `publish-release.sh` |

## Local release build

```bash
# 1. Binaries (default: desktop-win, desktop-linux, browser, service)
./build/publish-release.sh --version 5.0.0

# 2. Android (needs the android workload)
./build/publish-release.sh --version 5.0.0 --platform android

# 3. Database script (needs sqlpackage + a reference database)
dotnet tool install -g microsoft.sqlpackage
dotnet run --project gip.tool.dbscript -c Release -- \
  generate \
  --connection-string "data source=...;initial catalog=iPlusMESV5;user id=...;password=..." \
  --output ./artifacts/release/initial-install.sql \
  --suite-version 5.0.0
```

Output lands in `artifacts/release/` (git-ignored).

### Environment switches

| Variable | Default | Meaning |
|---|---|---|
| `IPLUS_USE_AVALONIA_FORK` | `false` | `false` consumes the `iPlus.*` Avalonia packages from nuget.org (required for CI and fresh clones). `true` uses the local fork checkout. |
| `IPLUS_BUILD_ANDROID_TFM` | `false` | `gip.core.autocomponent` adds an `-android` TFM on Linux, which breaks non-Android publishes. The `android` platform enables it automatically. |

### Known publish quirk

`gip.core.autocomponent` and `gip.core.communication` both ship a
`VBControlScripts/2026-04-13 08-58_dlisak.zip` (different content, same name).
Without `-p:ErrorOnDuplicatePublishOutputFiles=false` the publish fails with
`NETSDK1152`. `publish-release.sh` sets the switch; the last copy wins, which is
acceptable because the design-import step tolerates duplicates.

### Browser publish: Roslyn build host

`Microsoft.CodeAnalysis.Workspaces.MSBuild` (transitive via `gip.core.datamodel`)
ships `BuildHost-net472/` and `BuildHost-netcore/` content folders. Both contain a
`Microsoft.Build.Locator.dll` with the *same* assembly identity but *different*
content. The WASM `ManagedToNativeGenerator` scans every assembly in
`WasmAssembliesToBundle` and aborts with:

```
error MSB4018: The assembly 'Microsoft.Build.Locator, Version=1.0.0.0, ...'
has already loaded been loaded into this MetadataLoadContext.
```

The build host is a desktop-only Roslyn helper that is never used in the browser.
[gip.iplus.client.avui.Browser.csproj](../gip.iplus.client.avui.Browser/gip.iplus.client.avui.Browser.csproj)
removes those entries from `ResolvedFileToPublish` in the
`RemoveRoslynBuildHostFromPublish` target, which both fixes the generator and keeps
~460 KB of dead weight out of the artifact.

## CI release (GitHub Actions)

[.github/workflows/release.yml](../.github/workflows/release.yml) runs on a `v*`
tag or manual dispatch:

1. `resolve-version` — derives the version from the tag or `build/iPlus.Package.props`.
2. `build` — matrix job per platform (Windows runner for `desktop-win`, Ubuntu for
   the rest), each uploading its artifacts.
3. `database-script` — runs only when the repository variable
   `IPLUS_REFERENCE_DB_CONNECTION` is set; generates `initial-install.sql`.
4. `release` — merges the artifacts, recomputes `SHA256SUMS` and creates a **draft**
   GitHub Release.

Review the draft, then publish it.

### Repository configuration

| Kind | Name | Purpose |
|---|---|---|
| Variable | `IPLUS_REFERENCE_DB_CONNECTION` | Connection string of the authoritative reference database. Leave unset to skip the DB script job. |
| Secret (optional) | `WINDOWS_CERTIFICATE_BASE64` | Code-signing certificate for the Windows desktop build. |
| Secret (optional) | `WINDOWS_CERTIFICATE_PASSWORD` | Password of that certificate. |

## Database provisioning

The distribution ships **no `.mdf` files**. Instead:

1. **Fresh install:** apply `initial-install.sql` (schema + seed/master data).
2. **Upgrade:** skip step 1 and run the incremental `DbScripts` updates
   (`gip.core.dbsyncer` / Ctrl-login model sync).
3. **Design updates:** import the zipped files from `VBControlScripts`.

`initial-install.sql` is generated from a reference database by
[gip.tool.dbscript](../gip.tool.dbscript/readme.md). Which tables are exported is
controlled by [seed-tables.json](../gip.tool.dbscript/seed-tables.json) — review it
whenever the EF model gains a new master-data table.

## Evaluation bundle (docker-compose)

[deploy/docker/docker-compose.yml](../deploy/docker/docker-compose.yml) starts
SQL Server 2022 + the iPlus service + the browser client:

```bash
cp deploy/docker/.env.example deploy/docker/.env    # edit the SA password
./build/publish-release.sh --platform browser,service --keep-intermediate
docker compose -f deploy/docker/docker-compose.yml --env-file deploy/docker/.env up -d
```

Then open <http://localhost:8080>.

The `mssql-init` sidecar applies `initial-install.sql` exactly once (marker file in
the `mssql-init-state` volume) and skips it when the database already exists. The
service renders `ConnectionStrings.config` from environment variables at start-up,
so no password is baked into an image or committed to the repository.

This bundle is for **evaluation and development**. For production use a managed SQL
Server, TLS in front of the browser client, and a signed license file.

## Licensing

Licenses are **offline signed files** (`*.gip`), validated with an embedded public
key — no USB dongle, no phone-home.

| Task | Tool |
|---|---|
| Create a key pair | `gip.tool.licenseIssuer create-keypair` |
| Issue a developer license | `gip.tool.licenseIssuer issue-developer` |
| Issue a framework runtime license | `gip.tool.licenseIssuer issue-runtime` |
| Issue a product license | `gip.tool.licenseIssuer issue-product` |
| Verify a license | `gip.tool.licenseIssuer verify` |
| Database fingerprint | `gip.tool.dbscript fingerprint -c "<cs>"` |

Four license kinds exist (`LicenseKind`):

| Kind | Issued by | Purpose |
|---|---|---|
| `DeveloperIssuer` (10) | gip | Development environment + right to issue product licenses. Embeds the developer's public key. |
| `DeveloperEndUser` (20) | gip | Development/engineering features only. |
| `Product` (30) | developer or gip | The listed packages on one customer database. Only effective when a `FrameworkRuntime` license is present. |
| `FrameworkRuntime` (40) | gip | The commercial iPlus runtime edition, per concurrent user (`MaxUsers`). |

Product licenses are signed with the **developer's own** private key. The matching
public key is embedded in the `DeveloperIssuer` license that gip signed for that
developer, so end customers verify offline — no central signing service.

See [gip.core.licensing/readme.md](../gip.core.licensing/readme.md) and
[gip.tool.licenseIssuer/readme.md](../gip.tool.licenseIssuer/readme.md).

**Key management:** the framework private key is never committed. It lives offline
with gip and is only used to sign developer licenses. The framework public key is
embedded in `gip.core.licensing` (`FrameworkPublicKey.pem`).

## Dual-licensing structure

- **Open edition:** core framework + datamodel + standard BSOs (everything packed to
  NuGet) under GPL-3.0-or-later; binaries on GitHub Releases.
- **Commercial edition:** the same binaries plus proprietary modules, shipped as
  separate NuGet packages or a plugin drop folder. License checks live **only** in
  the commercial assemblies; the open build contains no key or phone-home code.

### Package license metadata

Every gip package is **dual-licensed** and declares it in NuGet and in the
assembly:

```xml
<PackageLicenseExpression>GPL-3.0-or-later OR LicenseRef-Commercial</PackageLicenseExpression>
```

`Directory.Build.targets` derives `IPlusLicenseExpression` from that property and
generates an assembly attribute:

```csharp
[assembly: AssemblyMetadata("iPlusLicense", "GPL-3.0-or-later OR LicenseRef-Commercial")]
```

The SDK's built-in `<AssemblyAttribute>` item cannot be used here because all
projects set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`; the custom
`GenerateIPlusLicenseAttribute` target writes `iPlusLicenseInfo.g.cs` and adds it
to `Compile` instead.

At runtime `ACClassManager.RegisterAssemblyTypes` reads the attribute into
`PackageLicenseRegistry`, and `License.IsPackageLicensed` applies the mixing rule:

| Package expression | Commercial runtime | Open runtime |
|---|---|---|
| `GPL-3.0-or-later` | **blocked** | allowed |
| `GPL-3.0-or-later OR LicenseRef-Commercial` | allowed | allowed |
| `LicenseRef-Commercial` | needs a `Product` license | **blocked** |
| `MIT` / `Apache-2.0` / `BSD-3-Clause` | allowed | allowed |
| unknown / missing | allowed (fail-open) | allowed |

A blocked package does not stop the program immediately: the client warns after
30 minutes and `ACUrlCommand` is blocked after 1.5 days, so the end user has time
to resolve the license issue.

## Open questions

These are still undecided and must be answered before the first public release:

- Which repository hosts the releases (public vs. private commercial repo)?
- Code-signing certificate for Windows/Android artifacts?
- SQL Server version baseline for the docker image and the DACPAC?
- Where do commercial module binaries live (private NuGet feed vs. drop folder)?
- Who hosts the optional online activation service, and how are key pairs managed?
- Which reference database is authoritative, and which data is seed/master data?
