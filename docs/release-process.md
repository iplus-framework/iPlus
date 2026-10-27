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
| `iplus-<ver>-desktop-win-x64.zip` | Windows desktop (Avalonia) | `publish-release.sh --platform desktop-win` |
| `iplus-<ver>-desktop-linux-x64.zip` | Linux desktop (Avalonia) | `publish-release.sh --platform desktop-linux` |
| `iplus-<ver>-wpf-win-x64.zip` | Windows desktop (WPF) | `publish-release.sh --platform wpf-win` |
| `iplus-<ver>-browser.zip` | Browser (WASM, static files) | `publish-release.sh --platform browser` |
| `iplus-<ver>-android.apk` | Android | `publish-release.sh --platform android` |
| `iplus-<ver>-service-linux-x64.zip` | Server service (Linux) | `publish-release.sh --platform service` |
| `iplus-<ver>-console-linux-x64.zip` | Console host (Linux) | `publish-release.sh --platform console` |
| `iplus-<ver>-setup-linux-x64.zip` | Installer/updater (Linux) | `publish-release.sh --platform setup` |
| `iplus-<ver>-setup-win-x64.zip` | Installer/updater (Windows) | `publish-release.sh --platform setup` |
| `iplus-<ver>-all.zip` | **Combined bundle** — one folder per platform | `publish-release.sh --platform all` |
| `initial-install-iplus.sql` | Database (framework schema + seed data) | `gip.tool.dbscript` from **iPlusV5** |
| `initial-install-mes.sql` | Database (framework + MES schema + seed data) | `gip.tool.dbscript` from **iPlusMESV5** |
| `SHA256SUMS` | Checksums for all of the above | `publish-release.sh` |

### Combined bundle layout

`iplus-<ver>-all.zip` contains **one folder per platform at the archive root** — no extra
wrapper directory:

```
iplus-<ver>-all.zip
├── iplus-<ver>-desktop-linux-x64/…
├── iplus-<ver>-desktop-win-x64/…
├── iplus-<ver>-wpf-win-x64/…
├── iplus-<ver>-browser/…
├── iplus-<ver>-service-linux-x64/…
├── iplus-<ver>-console-linux-x64/…
└── iplus-<ver>-setup-linux-x64/…
```

The installer resolves the folder matching the current OS and architecture by suffix
(`-<arch>`), so the version prefix in the folder name does not matter.

### Installer / updater

`gip.tool.installerAndUpdater` is a cross-platform Avalonia application (Windows, Linux,
macOS) that replaces the V4 WPF installer. It can install or update a product from:

- a **GitHub Release** (downloads the `-all` asset, or the platform asset if present),
- a **local zip**,
- an already **extracted folder**.

It then writes `ConnectionStrings.config`, optionally creates the SQL Server database and
applies `initial-install-*.sql`, creates a desktop shortcut and starts the application.
User-owned files (`ConnectionStrings.config`, `UserFiles/`, `Logs/`) are preserved on
update.

There is **no automatic update mechanism** — enterprises want to control exactly when an
update is applied, so the installer is run deliberately by an administrator.

The iPlus-MES repository publishes its own release with the same layout, prefixed
`iplus-mes-<ver>-…`, plus `initial-install-mes.sql`.

## Local release build

```bash
# 1. Binaries (default: desktop-win, desktop-linux, browser, service)
./build/publish-release.sh --version 5.0.0

# 2. Everything, including the combined bundle
./build/publish-release.sh --version 5.0.0 --platform all

# 3. A single platform
./build/publish-release.sh --version 5.0.0 --platform setup
./build/publish-release.sh --version 5.0.0 --platform wpf-win
./build/publish-release.sh --version 5.0.0 --platform console

# 4. Android (needs the android workload)
./build/publish-release.sh --version 5.0.0 --platform android

# 5. Sign the artifacts (Windows only; no-op elsewhere)
./build/sign-release.sh --mode self-signed --pfx my.pfx --password '***'

# 6. Database scripts (needs sqlpackage + the reference databases)
dotnet tool install -g microsoft.sqlpackage

# Framework script from iPlusV5
dotnet run --project gip.tool.dbscript -c Release -- \
  generate \
  --connection-string "data source=...;initial catalog=iPlusV5;user id=...;******" \
  --output ./artifacts/release/initial-install-iplus.sql \
  --seed-config ./gip.tool.dbscript/seed-tables.json \
  --suite-version 5.0.0

# MES script from iPlusMESV5
dotnet run --project gip.tool.dbscript -c Release -- \
  generate \
  --connection-string "data source=...;initial catalog=iPlusMESV5;user id=...;******" \
  --output ./artifacts/release/initial-install-mes.sql \
  --seed-config ./gip.tool.dbscript/seed-tables-mes.json \
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

Both repositories are public and each hosts its own release:

| Repository | Workflow | Artifacts |
|---|---|---|
| `iplus-framework/iPlus` | [.github/workflows/release.yml](../.github/workflows/release.yml) | framework binaries + `initial-install-iplus.sql` |
| `iplus-framework/iPlusMES` | [iPlusMES/.github/workflows/release.yml](../../iPlusMES/.github/workflows/release.yml) | MES binaries + `initial-install-mes.sql` |

The iPlus workflow runs on a `v*` tag or manual dispatch:

1. `resolve-version` — derives the version from the tag or `build/iPlus.Package.props`.
2. `build` — matrix job per platform (Windows runner for `desktop-win`, `wpf-win` and
   `setup`; Ubuntu for the rest), each uploading its artifacts. When `SIGNING_MODE` is
   `self-signed` or `azure`, the Windows jobs sign the binaries before uploading.
3. `database-script` — runs only when the repository variable
   `IPLUS_REFERENCE_DB_CONNECTION` is set; generates `initial-install-iplus.sql` from
   **iPlusV5**. When `IPLUSMES_REFERENCE_DB_CONNECTION` is also set it additionally
   generates `initial-install-mes.sql` from **iPlusMESV5**.
4. `release` — merges the artifacts, recomputes `SHA256SUMS` and creates a **draft**
   GitHub Release.

Review the draft, then publish it.

The iPlus-MES workflow is identical but checks out **both** repositories side-by-side
(the MES projects reference `..\..\iPlus\…` directly) and publishes the MES heads:
`gip.mes.client.avui.Desktop`, `.Browser`, `.Android` and `gip.mes.console` as the
server host (there is no `gip.mes.service` project).

### Repository configuration

**iPlus repository:**

| Kind | Name | Purpose |
|---|---|---|
| Variable | `IPLUS_REFERENCE_DB_CONNECTION` | Connection string of the **iPlusV5** reference database. Leave unset to skip the DB script job. |
| Variable | `IPLUSMES_REFERENCE_DB_CONNECTION` | Connection string of the **iPlusMESV5** reference database. Leave unset to skip the MES script. |
| Variable | `SIGNING_MODE` | `none` (default), `self-signed` or `azure`. See [Code signing](#code-signing). |
| Secret (optional) | `WINDOWS_CERTIFICATE_BASE64` | Base64 `.pfx` for `SIGNING_MODE=self-signed`. |
| Secret (optional) | `WINDOWS_CERTIFICATE_PASSWORD` | Password of that `.pfx`. |
| Secret (optional) | `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET` | Service principal for `SIGNING_MODE=azure`. |
| Secret (optional) | `AZURE_SIGNING_ENDPOINT`, `AZURE_SIGNING_ACCOUNT`, `AZURE_SIGNING_PROFILE` | Azure Artifact Signing account and certificate profile. |
| Secret (optional) | `ANDROID_KEYSTORE_BASE64` | Base64 `.jks` for a stable APK signature. Without it the per-machine debug key is used. |
| Secret (optional) | `ANDROID_KEYSTORE_PASSWORD` | Keystore password. |
| Secret (optional) | `ANDROID_KEY_ALIAS` | Key alias inside the keystore. |
| Secret (optional) | `ANDROID_KEY_PASSWORD` | Key password (defaults to the keystore password). |

**iPlusMES repository:**

| Kind | Name | Purpose |
|---|---|---|
| Variable | `IPLUSMES_REFERENCE_DB_CONNECTION` | Connection string of the **iPlusMESV5** reference database. |

`IPCDrivesV5` is a demo/example project for playing, experimenting and demonstration —
it is **never** used to generate a distribution script.

## Database provisioning

The distribution ships **no `.mdf` files**. Instead:

1. **Fresh install:** apply `initial-install-iplus.sql` (framework) or
   `initial-install-mes.sql` (framework + MES) — schema + seed/master data.
2. **Upgrade:** skip step 1 and run the incremental `DbScripts` updates
   (`gip.core.dbsyncer` / Ctrl-login model sync).
3. **Design updates:** import the zipped files from `VBControlScripts`.

The scripts are generated from the reference databases by
[gip.tool.dbscript](../gip.tool.dbscript/readme.md). Which tables are exported is
controlled by two configurations:

| Script | Reference database | Seed configuration |
|---|---|---|
| `initial-install-iplus.sql` | iPlusV5 | [seed-tables.json](../gip.tool.dbscript/seed-tables.json) |
| `initial-install-mes.sql` | iPlusMESV5 | [seed-tables-mes.json](../gip.tool.dbscript/seed-tables-mes.json) |

Review both whenever the EF model gains a new master-data table.

**SQL Server baseline: 2019.** `gip.tool.dbscript` extracts with
`/TargetServerVersion:Sql150`, and the docker bundle uses
`mcr.microsoft.com/mssql/server:2019-latest`.

## Evaluation bundle (docker-compose)

[deploy/docker/docker-compose.yml](../deploy/docker/docker-compose.yml) starts
SQL Server 2019 + the iPlus service + the browser client:

```bash
cp deploy/docker/.env.example deploy/docker/.env    # edit the SA password
./build/publish-release.sh --platform browser,service --keep-intermediate
docker compose -f deploy/docker/docker-compose.yml --env-file deploy/docker/.env up -d
```

Then open <http://localhost:8080>.

The `mssql-init` sidecar applies `initial-install-iplus.sql` exactly once (marker file in
the `mssql-init-state` volume) and skips it when the database already exists. The
service renders `ConnectionStrings.config` from environment variables at start-up,
so no password is baked into an image or committed to the repository.

This bundle is for **evaluation and development**. For production use a managed SQL
Server, TLS in front of the browser client, and a signed license file.

## Code signing

Signing is controlled by a single switch, `SIGNING_MODE`:

| Mode | What happens | Secrets |
|---|---|---|
| `none` (default) | Artifacts ship unsigned | — |
| `self-signed` | `signtool sign` with a `.pfx` | `WINDOWS_CERTIFICATE_BASE64`, `WINDOWS_CERTIFICATE_PASSWORD` |
| `azure` | `azure/trusted-signing-action` | `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_SIGNING_ENDPOINT`, `AZURE_SIGNING_ACCOUNT`, `AZURE_SIGNING_PROFILE` |

The steps run only on Windows runners. Locally the same modes are available through
[build/sign-release.sh](../build/sign-release.sh):

```bash
./build/sign-release.sh --mode self-signed --pfx my.pfx --password '***'
./build/sign-release.sh --mode azure   # reads AZURE_SIGNING_* from the environment
```

On Linux/macOS the script reports that Authenticode is not applicable and exits
successfully.

### Why Authenticode and not strong names

`GIP-SNKey.snk` is committed to the public repository, so anyone can produce a valid
strong name. The `iPlusLicense` assembly metadata is a plain string and equally
forgeable. **Authenticode is the only mechanism that proves who built a binary.**

### Publisher verification at runtime

The runtime verifies the signer of every assembly it registers:

- [`AuthenticodeVerifier`](../gip.core.licensing/AuthenticodeVerifier.cs) — `WinVerifyTrust`
  P/Invoke; no-op on Linux/macOS.
- [`TrustedPublisherStore`](../gip.core.licensing/TrustedPublisherStore.cs) — reads
  `Licenses/TrustedPublishers.txt` (one thumbprint per line, optional `mode=` line).
  `IPLUS_PUBLISHER_VERIFICATION` overrides the file.
- [`ACClassManager.RegisterAssemblyTypes`](../gip.core.autocomponent/ACClassManager/ACClassManager.cs)
  calls `IsPublisherAllowed` before registering any type.

```text
# Licenses/TrustedPublishers.txt
mode=warn
A1B2C3D4E5F6...
0123456789AB...
```

| Mode | Behaviour |
|---|---|
| `off` (default) | No verification — nothing changes for existing installs |
| `warn` | Log a warning, still register the types |
| `enforce` | Skip registration of assemblies whose signer is not trusted |

The check is **fail-open**: an empty thumbprint list, a non-Windows host, or a
verification error all allow the assembly. A misconfigured trust store must never lock a
customer out of their own runtime.

### Getting a certificate

**Azure Artifact Signing** (formerly Azure Trusted Signing) is the cheapest route:
Basic ~9.99 USD/month for 5,000 signatures, Premium ~99.99 USD/month for 100,000,
overage 0.005 USD. It requires a paid Azure subscription and **three years of verifiable
business history** — plan one to two weeks for onboarding.

A classic OV certificate (DigiCert, Sectigo, GlobalSign, Certum, SSL.com) costs
~200–500 €/year and is exportable as a `.pfx`, which is what `SIGNING_MODE=self-signed`
expects. EV certificates (~400–900 €/year) give immediate SmartScreen trust but come on a
hardware token or cloud HSM and cannot be exported as a plain `.pfx`.

**Recommended path:** self-signed certificate first to exercise the pipeline, then Azure
Artifact Signing once the business verification is through.

### Android APK signing

Android is **not** Authenticode — it uses its own APK signature scheme. Two things matter:

1. **Sideloading needs no keystore.** `dotnet workload install android` is the only
   prerequisite; the build auto-signs with the per-machine debug key
   (`META-INF/ANDROIDD.RSA`). Verified: `./build/publish-release.sh --platform android`
   produces a working 302 MB APK with no signing configuration at all.
2. **Updates need a stable keystore.** The debug key is generated per machine, so every
   CI run produces a different signature and Android refuses to install the new APK over
   the old one ("App not installed"). The user has to uninstall first.

To get a stable signature, create one keystore and store it as secrets:

```bash
keytool -genkeypair -v -keystore iplus-release.jks -alias iplus \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -dname "CN=gipSoft d.o.o., O=gipSoft, C=AT"

base64 -w0 iplus-release.jks   # → ANDROID_KEYSTORE_BASE64
```

| Secret | Purpose |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | Base64 `.jks` |
| `ANDROID_KEYSTORE_PASSWORD` | Keystore password |
| `ANDROID_KEY_ALIAS` | Key alias (`iplus` above) |
| `ANDROID_KEY_PASSWORD` | Key password (defaults to the keystore password) |

When `ANDROID_KEYSTORE_BASE64` is set, `publish-release.sh` passes
`-p:AndroidKeyStore=true -p:AndroidSigningKeyStore=… -p:AndroidSigningKeyAlias=…` and the
APK is signed with that key instead of the debug key. Verified end-to-end: the resulting
APK contains `META-INF/IPLUS.RSA` with subject `CN=iPlus Test, O=gipSoft` rather than
`META-INF/ANDROIDD.RSA`.

**Keep the keystore safe.** Losing it means you can never publish an update to an
already-installed app — Android identifies apps by signing key, not by package name alone.

### iOS

iOS is **not** covered by the pipeline, and not because of a missing SDK:

- `dotnet workload install ios` only exists on macOS, and the build needs Xcode.
- iOS refuses to run *any* unsigned app, even sideloaded.
- A free Apple ID gives 7-day provisioning profiles (re-sign weekly, max 3 apps).
- A paid Apple Developer account (99 USD/year) gives 1-year profiles and ad-hoc install
  on registered device UDIDs.

Deferred until a customer asks for it.

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

## NuGet publishing

All packages are published on **nuget.org** under the
[iplus-framework organisation profile](https://www.nuget.org/profiles/iplus-framework).
There is no private feed — commercial modules are separate packages that carry the
`LicenseRef-Commercial` expression.

```bash
# 1. Pack (writes to ./local-packages)
./build/pack-local.sh Release

# 2. Verify the license metadata before pushing
for pkg in local-packages/*.nupkg; do
  unzip -p "$pkg" '*.nuspec' | grep -o '<license[^/]*/>' | head -1
done

# 3. Push (API key from https://www.nuget.org/account/apikeys)
dotnet nuget push "local-packages/*.nupkg" \
  --api-key "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

`--skip-duplicate` makes the push idempotent, so a re-run after a partial failure
does not abort on already-published versions. `build/pack-local.sh` removes empty
`.snupkg` files because nuget.org rejects them with HTTP 400.

The iPlusMES repository packs into its own `local-packages` folder with the MES
suite version (`IPlusMESPackageVersion`) and pushes to the same organisation.

## Open questions

All questions are answered; the answers are binding for the release pipeline.

| Question | Answer |
|---|---|
| Which repository hosts the releases? | Both are public and host their own: `iplus-framework/iPlus` and `iplus-framework/iPlusMES`. |
| Code-signing certificate for Windows/Android? | Not purchased yet. Recommended: OV certificate (exportable `.pfx`). Alternatives: EV (hardware token) or Azure Trusted Signing. See [HANDOVER §8](./HANDOVER-dual-licensing-distribution.md#8-code-signing). |
| SQL Server version baseline? | **SQL Server 2019** — `Sql150` DACPAC target, `mssql/server:2019-latest` docker image. |
| Where do commercial module binaries live? | nuget.org under the [iplus-framework organisation](https://www.nuget.org/profiles/iplus-framework). No private feed. |
| Who hosts the optional online activation service? | Deferred — offline signed license files are the baseline. |
| Which reference database is authoritative? | **iPlusV5** for the framework script, **iPlusMESV5** for the MES script. `IPCDrivesV5` is demo-only. |
