# iPlus Licensing & Packaging — Developer Guide

This guide explains how **licensing** and **packaging** work in iPlus V5: how a license
file is issued, verified and enforced at runtime, how a package declares its license
(GPL vs. commercial), and how the NuGet / binary distribution pipelines are wired.

It is written for developers (and AI agents) who need to **add a commercial product
package**, **issue licenses**, or **change the build/publish pipeline** without
re-reading the whole framework source.

Reference implementations used throughout this document:
[License.cs](../../gip.core.datamodel/ACHelperclasses/ACLicense/License.cs),
[License.FileLicensing.cs](../../gip.core.datamodel/ACHelperclasses/ACLicense/License.FileLicensing.cs)
and [gip.core.licensing/](../../gip.core.licensing/readme.md).

---

## 0. Cheat sheet (TL;DR)

```text
1. FOUR license kinds: DeveloperIssuer (10), DeveloperEndUser (20), Product (30),
   FrameworkRuntime (40). FrameworkRuntime is the commercial iPlus edition, sold per
   concurrent user (MaxUsers). A Product license is only effective when a
   FrameworkRuntime license is present.
2. License files are *.gip: "IPLUS-LICENSE" + base64 RSA-SHA256 signature + JSON payload.
   The signature covers the exact UTF-8 bytes of the JSON — no canonicalization.
3. Verification is OFFLINE and stateless: LicenseVerifier.Verify(text, context).
   Framework key resolution: env IPLUS_LICENSE_PUBLIC_KEY -> embedded resource -> file.
4. Two-level PKI chain: gip signs DeveloperIssuer (embeds the developer's public key +
   IssuerKeyId); the developer signs Product with the matching private key. A developer
   cannot issue a DeveloperIssuer license for itself.
5. Runtime bridge: License.FileLicensing.cs. It scans <appbase>/Licenses/*.gip AND
   re-verifies rows activated into VBSystem (SystemName "VBP" = product/runtime,
   "VBD" = issuer). Dedupe key is LicenseId, NOT CustomerName. Any number of product
   licenses may coexist (one per developer); only ONE FrameworkRuntime is effective.
   <appbase> = AppContext.BaseDirectory (the folder holding the executable). The
   Licenses folder is NOT created by the framework and NOT shipped by the installer;
   a missing folder is silently treated as "no license files".
6. Enforcement points: ACPackage.IsLicensed -> ACClass.IsLicensed -> ACComponent.ACUrlCommand
   (_HasLicense) and ACBSO.OnSave -> VerifyLicense/VerifyMaxSessions.
7. Package license metadata: AssemblyMetadata("iPlusLicense", "<SPDX expression>").
   Read once in ACClassManager.RegisterAssemblyTypes -> PackageLicenseRegistry.
8. <GenerateAssemblyInfo>false</GenerateAssemblyInfo> is set in ALL projects, so the SDK's
   <AssemblyAttribute> item DOES NOT WORK. Use the GenerateIPlusLicenseAttribute target.
9. Mixing rule: a package may run under a commercial runtime license only if its
   expression contains "Commercial" or is permissive (MIT/Apache-2.0/BSD-3-Clause).
   Pure GPL-3 packages are blocked under a commercial runtime; commercial-only packages
   are blocked under the open runtime. A developer installation (DeveloperIssuer /
   DeveloperEndUser license file) is exempt: everything may run there.
10. Grace period: 30 min client warning, then ACUrlCommand blocked after 1.5 days.
    Tracked by LicenseGracePeriod (gip.core.licensing), persisted in
    <LicenseFolder>/.licensegrace so a restart does not reset the clock.
11. Pack: build/pack-local.sh -> ./local-packages (PackageId = iPlus.<ProjectName>).
    Publish: build/publish-release.sh -> ./artifacts/release (binaries, not packages).
    Platforms: desktop-win, desktop-linux, wpf-win, browser, android, service, console,
    setup, all. "all" also produces iplus-<ver>-all.zip (one folder per platform at the
    archive root).
12. Installer: gip.tool.installerAndUpdater (Avalonia, net10.0, Windows/Linux/macOS).
    Installs from a GitHub Release, a zip or a folder; writes ConnectionStrings.config,
    creates the DB, applies initial-install-*.sql, creates a shortcut. NO auto-update.
13. Code signing: SIGNING_MODE=none|self-signed|azure (CI) or build/sign-release.sh
    (local). Authenticode is the ONLY proof of who built a binary — GIP-SNKey.snk is
    public and the iPlusLicense metadata is a plain string.
14. Publisher verification: AuthenticodeVerifier + TrustedPublisherStore, called from
    ACClassManager.RegisterAssemblyTypes. Modes off/warn/enforce, default off, and
    FAIL-OPEN (empty list, non-Windows, or an error all allow the assembly).
15. Commercial module conventions (no new framework code needed):
    private repo, PackageId iPlus.Commercial.<Module>,
    PackageLicenseExpression LicenseRef-Commercial, ACPackageName gip.Commercial.<Module>,
    assembly gip.commercial.<module>.dll (matched by *.commercial.*.dll).
16. Android: sideloading needs NO keystore (debug key is auto-generated). Updates DO need
    a stable keystore (ANDROID_KEYSTORE_BASE64), otherwise every CI build has a different
    signature and Android refuses to install over the previous version. iOS is blocked by
    macOS + Xcode + Apple provisioning, not by an SDK install.
```

---

## 1. Background — the three-division model

V4 used a hardware dongle (TDi Matrix32/Matrix64, driven through `Mxapi.cs`). V5 replaces it
with **offline signed license files** while keeping the same three-division semantics. The
dongle code path has been **removed entirely** — `Mxapi.cs`, `Mxtea.cs`, `matrix.ini`, the
`#region Dongle-Operations` in `License.cs`, and the `gip.tool.devLicense` /
`gip.tool.devLicenseProvider` projects are gone. `License.Initialize()` no longer probes for
hardware, which is why the runtime now starts on Linux/macOS.

```mermaid
flowchart LR
    GIP["gip (framework vendor)"] -->|"DeveloperIssuer license<br/>(signs with gip private key)"| DEV["Developer / integrator"]
    GIP -->|"FrameworkRuntime license<br/>(per concurrent user)"| EU["End customer"]
    DEV -->|"Product license<br/>(signs with developer private key)"| EU
    DEV -->|"sells own product packages"| EU
```

| Division | Who | What they get |
|---|---|---|
| gip | framework vendor | Signs `DeveloperIssuer` and `FrameworkRuntime` licenses |
| developer / integrator | builds product packages | Holds a `DeveloperIssuer` license → may issue `Product` licenses for **their own** packages |
| end customer | runs the installation | Holds a `Product` license per developer product **and** a `FrameworkRuntime` license per concurrent user |

**Key business rule:** a product license is worthless without a framework runtime
license. The runtime license is what the end customer buys from gip; the product
license is what they buy from the integrator.

---

## 2. The licensing library — `gip.core.licensing`

Deliberately **dependency-free** (BCL only) so it can be consumed by
`gip.core.datamodel`, the issuer CLI and third-party commercial modules without
dragging in EF Core / Avalonia / SqlClient.

| Type | Purpose |
|---|---|
| `LicenseKind` | `Unknown=0`, `DeveloperIssuer=10`, `DeveloperEndUser=20`, `Product=30`, `FrameworkRuntime=40` |
| `LicensePayload` | The signed data: `FormatVersion`, `LicenseId`, `Kind`, `CustomerName`, `ProjectNo`, `IssuerName`, `IssuedUtc`, `ExpiresUtc?`, `MaxUsers?`, `Packages`, `DatabaseFingerprint`, `MachineFingerprint`, `Remarks`, `IssuerPublicKey`, `IssuerKeyId` |
| `LicenseFile` | Serialize / parse / read / write the `.gip` container |
| `LicenseIssuer` | Signs a payload with an RSA private key |
| `LicenseVerifier` | Verifies a payload against an RSA public key + `LicenseBindingContext` |
| `LicenseKeys` | Key pair creation, PEM/legacy-XML import/export |
| `FrameworkLicenseKeys` | Resolves the **framework** public key (env → embedded → file) |
| `DatabaseFingerprint` | `Compute(createDate, serviceBrokerGuid, databaseGuid)`; `FromLegacyChecksumCode(string)` for V4 checksums |
| `MachineFingerprint` | `ComputeCurrent()` |
| `LicenseFormat` | `CurrentVersion = 1`, `FileExtension = ".gip"`, `Magic = "IPLUS-LICENSE"` |
| `LicenseValidationResult` | `IsValid`, `Payload`, `Error`, `Message` |
| `PackageLicenseExpression` | SPDX classifier + the GPL/commercial mixing rule (`LicenseCompatibility`) |
| `LicenseGracePeriod` | Two-stage grace-period tracker (`Start`, `Clear`, `Restore`, `IsWarningDue`, `IsBlockDue`) — dependency-free and unit tested |

### 2.0 The four license kinds

| Kind | Value | Issued by | Bound to | Grants |
|---|---|---|---|---|
| `DeveloperIssuer` | 10 | gip | nothing / machine | Development environment + right to issue `Product` licenses. **Embeds the developer's public key.** |
| `DeveloperEndUser` | 20 | gip | machine | Development/engineering features only |
| `Product` | 30 | developer or gip | database (+ machine) | The listed packages. **Only effective when a `FrameworkRuntime` license is present.** |
| `FrameworkRuntime` | 40 | gip | database | The commercial iPlus edition, per concurrent user (`MaxUsers`). |

`CoversPackage()` returns `true` for any developer kind, `false` for
`FrameworkRuntime`, else a case-insensitive list lookup.

### 2.0.1 Two-level PKI chain

```mermaid
flowchart LR
    G["gip private key"] -->|signs| DI["DeveloperIssuer license<br/>IssuerPublicKey + IssuerKeyId"]
    DI -->|embeds| DP["developer public key"]
    DP -->|verifies| PL["Product license<br/>IssuerKeyId"]
    DK["developer private key"] -->|signs| PL
    G -->|signs| FR["FrameworkRuntime license"]
```

`LicenseVerifier.VerifySignature` tries the **framework** key first. Only for a
`Product` license with a non-empty `IssuerKeyId` **and** a configured
`issuerKeyResolver` does it try the developer key. A developer therefore cannot
issue a `DeveloperIssuer` license for itself — that needs gip's private key.

`LicenseKeys.ComputeKeyId(RSA)` = first 16 lowercase hex chars of SHA-256 over
`ExportSubjectPublicKeyInfo()`. `ComputeKeyIdFromText(string)` does the same from
PEM/XML text.

`FrameworkLicenseKeys.CreateVerifier(Func<string, RSA?>? issuerKeyResolver)` is the
overload that enables the chain. `License.FileLicensing.CreateVerifierWithIssuerChain()`
builds the resolver from (a) `DeveloperIssuer` licenses in the license folder and in
`VBSystem`, and (b) `<LicenseFolder>/Issuers/*.pem`.

### 2.1 Payload helpers

```csharp
payload.CoversPackage("ACME.Mes")   // true for any developer kind; else list lookup (case-insensitive)
payload.IsTimeValid(DateTimeOffset.UtcNow)  // false when ExpiresUtc has passed
```

### 2.2 Verification order

`LicenseVerifier.Verify(text, context, nowUtc)` is **offline, stateless and
thread-safe**. It fails fast in this order:

| # | Check | `LicenseValidationError` |
|---|---|---|
| 1 | Parse header / signature / payload | `Malformed` (10) |
| 2 | `FormatVersion == LicenseFormat.CurrentVersion` | `UnsupportedFormatVersion` (20) |
| 3 | RSA-SHA256 PKCS1 signature over the payload bytes | `InvalidSignature` (30) |
| 4 | `IsTimeValid(now)` | `Expired` (40) |
| 5 | `DatabaseFingerprint` matches context | `DatabaseMismatch` (50) |
| 6 | `MachineFingerprint` matches context | `MachineMismatch` (60) |
| 7 | `context.RequiredPackage` is covered | `PackageNotLicensed` (70) |
| 8 | `context.RequiredKind` matches | `WrongLicenseKind` (80) |
| 9 | issuer key is trusted (reserved) | `UntrustedIssuer` (90) |

Empty `DatabaseFingerprint` / `MachineFingerprint` in the payload means "not bound" —
the check is skipped. That is how developer licenses stay machine-independent.

### 2.3 Framework key resolution

`FrameworkLicenseKeys.TryGetPublicKey()` tries, in order:

1. Environment variable `IPLUS_LICENSE_PUBLIC_KEY` — a **path** to a PEM/XML file
   (intended for development and for commercial builds that embed their own key)
2. Embedded resource whose name ends with `FrameworkPublicKey.pem`
3. A file named `FrameworkPublicKey.pem` next to the assembly

The result is cached; call `FrameworkLicenseKeys.ResetCache()` in tests.

`LicenseFormat.FrameworkPublicKeyResourceName` is
`gip.core.licensing.FrameworkPublicKey.pem` — it must match the `LogicalName` in
`gip.core.licensing.csproj` (it was `.xml` before and had to be fixed).

---

## 3. License file format

```text
IPLUS-LICENSE
<base64 RSA-SHA256 signature>
{ ...json payload... }
```

The signature covers the **exact UTF-8 bytes of the JSON payload** that follow the
second line. Because the payload bytes are stored verbatim there is no
canonicalization problem: verification re-reads the same bytes that were signed.

JSON options: `WriteIndented`, `camelCase`, `IgnoreNull`, case-insensitive on read.

---

## 4. Runtime integration — `License.FileLicensing.cs`

`License` is a `partial` class; the V4 engine lives in `License.cs`, the new
file-based bridge in `License.FileLicensing.cs`. The V4 public API surface is
preserved so existing developer tooling keeps compiling.

### 4.1 Where licenses are found

`IsPackageLicensedByFile(acPackage)` consults, in order:

1. `Const.PackName_VarioSystem` / `Const.PackName_System` → always `true` (framework core)
2. `FileLicenses` — every `*.gip` in `LicenseFolder` (default `<appbase>/Licenses`)
   that carries a valid signature **and** passes the binding context
3. `IsPackageLicensedByActivatedLicense(packageName)` — rows in `VBSystem` with
   `SystemName == "VBP"`, re-verified from the stored raw text

Invalid files are skipped and reported through `LastLicenseErrors` — a corrupt file
never breaks startup.

#### 4.1.1 The `Licenses` folder — where it is and who creates it

`LicenseFolder` is **not** a database table and **not** a config entry. It is a plain
directory next to the binaries:

```csharp
_LicenseFolder = Path.Combine(AppContext.BaseDirectory, "Licenses");
```

`AppContext.BaseDirectory` is the folder that contains the running executable — i.e.
`bin/Debug/net10.0/` in development, or the install directory in production. So for
`gip.iplus.console` the folder is `bin/Debug/net10.0/Licenses/`.

Layout:

```text
<appbase>/Licenses/
├── *.gip                  signed license files (DeveloperIssuer, FrameworkRuntime, Product)
├── Issuers/*.pem          extra trusted developer public keys (optional)
├── TrustedPublishers.txt  Authenticode thumbprints + optional "mode=" line
└── .licensegrace          grace-period start time (written by the runtime)
```

**The framework does not create the folder.** `LoadFileLicenses()` returns early when
`Directory.Exists(folder)` is `false`, so a missing folder is silently treated as
"no license files" — no crash, no error. The only place the runtime creates it is
`WritePersistedGraceStart()`, which does `Directory.CreateDirectory(folder)` when the
grace period starts. In practice that means the folder appears on its own only after
a licensing problem has already been detected.

Consequences:

| Situation | Result |
|---|---|
| Folder missing, licenses activated into `VBSystem` | Works — `IsPackageLicensedByActivatedLicense` reads the DB rows |
| Folder missing, license only dropped in as a file | File is ignored |
| Folder missing, `TrustedPublishers.txt` expected | `TrustedPublisherStore` finds nothing → verification stays fail-open |

`LicenseFolder` has a public setter, so a host can redirect it, but there is **no
environment variable** for it (unlike `IPLUS_LICENSE_PUBLIC_KEY` and
`IPLUS_PUBLISHER_VERIFICATION`). Neither `build/publish-release.sh` nor
`gip.tool.installerAndUpdater` creates the folder — an administrator has to create it
by hand when they want to pre-place a `DeveloperIssuer` or `FrameworkRuntime` file
instead of activating it through `BSOLicenseActivation`.

> **Rule:** activating a license through the UI writes to the database and needs no
> folder. Dropping a `.gip` file into `Licenses/` is the alternative path for
> unattended installs — and that path requires the folder to exist.

### 4.2 Activation

`ActivateLicenseFile(licenseText, out error)`:

1. Verifies against `CreateBindingContext()` (DB + machine fingerprint) using
   `CreateVerifierWithIssuerChain()` (so developer-signed product licenses work)
2. Writes a `VBSystem` row:
   - `SystemName` = `"VBP"` for `Product` **and** `FrameworkRuntime`, `"VBD"` for
     developer kinds
   - `SystemInternal` = raw signed file bytes (so it can be re-verified later)
   - `SystemInternal2` = `LicenseId`
   - `SystemCommonPublic` = `DatabaseFingerprint`
3. Sets `NeedRestart = true` and calls `ReloadFileLicenses()`

**Dedupe key is `LicenseId` (`SystemInternal2`), not `CustomerName`.** Two different
developers may license the same customer; keying on the customer name made them
overwrite each other. Re-activating the same file replaces its row.

### 4.2.1 Framework runtime license

| Member | Meaning |
|---|---|
| `FrameworkRuntimeLicense` | The valid `FrameworkRuntime` payload (license folder first, then activated `VBSystem` rows) |
| `HasFrameworkRuntimeLicense` | `FrameworkRuntimeLicense != null` |
| `IsCommercialRuntime` | Same as `HasFrameworkRuntimeLicense` — the runtime license **is** the commercial edition |
| `LicensedMaxUsers` | `FrameworkRuntimeLicense.MaxUsers`, `0` = unlimited |
| `ActivatedLicensePayloads` | Every activated `VBSystem` row that still verifies |

`ACStartUpRoot.LoginUser` reads `LicensedMaxUsers` right after
`new License(user, Database)` and calls
`Environment.ApplyLicensedMaxUsers(...)`, which sets `MaxLicensedWinSessions`.
`IsMaxWinSessionsExceeded` then compares against `CurrentWinSessionCount`.

### 4.2.2 Grace period

The two-stage grace period lives in a dependency-free, unit-tested class:
[LicenseGracePeriod.cs](../../gip.core.licensing/LicenseGracePeriod.cs). `License`
exposes it as `GracePeriod` and delegates all grace logic to it.

| Member | Default | Meaning |
|---|---|---|
| `LicenseGracePeriod.DefaultWarningPeriod` | 30 min | After this the client shows `Warning50025` once |
| `LicenseGracePeriod.DefaultBlockPeriod` | 1.5 days | After this `IsComponentLicensed` returns `False` → `ACUrlCommand` returns `null` |
| `License.WarningGracePeriod` / `BlockGracePeriod` | 30 min / 1.5 days | Overridable per installation |
| `License.GracePeriod` | — | The tracker instance (`Start`, `Clear`, `Restore`, `IsWarningDue`, `IsBlockDue`) |
| `License.LicenseGraceStartUtc` | — | When the problem was first detected (`StartLicenseGracePeriod()`) |
| `License.IsLicenseWarningDue` | — | Stage 1 reached |
| `License.IsLicenseBlockDue` | — | Stage 2 reached |

`IsPackageLicensed` calls `StartLicenseGracePeriod()` when a package is blocked and
returns `!IsLicenseBlockDue` — so the package keeps working during the grace window.
`ClearLicenseGracePeriod()` resets it once the license is fixed; `ActivateLicenseFile`
also clears it, so a freshly installed license gets a full grace window if it still
does not resolve the problem.

**Persisted across restarts:** the start time is written to
`<LicenseFolder>/.licensegrace` (UTC ticks). `License.GracePeriod` restores it lazily on
first access, so restarting the service does not reset the clock. A start time in the
future (clock set back) is ignored. `GracePeriodFilePath` is overridable.

> `LicenseGracePeriod` is deliberately free of database/UI dependencies so it can be
> unit tested — see `gip.core.licensing.unit.test/LicenseGracePeriodTests.cs` (14 tests).

### 4.3 Issuing (developer side)

`IssueProductLicenseFile(privateKeyText, customerName, projectNo, packages, outputPath, out error)`
requires `IsDeveloperWithIssuingRightsByFile` (i.e. an active `DeveloperIssuer`
license), builds a `Product` payload bound to the current DB fingerprint and signs it
with the developer's private key.

#### 4.3.1 The customer-database fingerprint trap

A `Product` license is **issued on the developer's installation** but **activated on
the customer's database**. The original overload bound the payload to
`CreateBindingContext().DatabaseFingerprint` — i.e. the *developer's own* database —
so the customer's `ActivateLicenseFile` always failed with `DatabaseMismatch`. The
license was structurally unusable.

The fix is the overload that takes the fingerprint explicitly:

```csharp
public bool IssueProductLicenseFile(
    string privateKeyText, string customerName, string projectNo,
    IEnumerable<string> packages, string outputPath,
    string databaseFingerprint, out string error)
```

The original overload delegates to it with `databaseFingerprint: null`, which falls
back to the local database — preserving the behaviour of `gip.tool.licenseIssuer
issue-product` (no `--database-fingerprint`) and the unit tests.

**Where the customer fingerprint comes from:** `BSOLicenseActivation` exports the
activation data file with the V4 identification code:

```xml
<xml><companyName>…</companyName><db>…</db><ds>…</ds><code>177-117-56-…</code></xml>
```

`code` is `DBInfo.GetCheckSumCode()` — the dash-separated MD5 byte list of
`service_broker_guid + create_date + database_guid`. `BSOiPlusLicenseManager`
converts it with `DatabaseFingerprint.FromLegacyChecksumCode(code)` and stores the
result in `CustomerDatabaseFingerprint`. **Never** compute the fingerprint from the
developer's own database when issuing for a customer.

The CLI does the same: `issue-runtime` / `issue-product` accept `--activation-file`
(`-a`) instead of `--database-fingerprint`, and `fingerprint -a <file>` just prints it.

> `DatabaseFingerprint.Compute(...)` and `FromLegacyChecksumCode(...)` both return
> **uppercase** SHA-256 hex. `LicenseVerifier` compares with `OrdinalIgnoreCase`, so
> casing is not a correctness risk, but keep it consistent in logs and CLI output.

#### 4.3.2 End-to-end workflow (V5)

```mermaid
sequenceDiagram
    participant GIP as gip (framework vendor)
    participant DEV as Developer / integrator
    participant CUS as End customer

    GIP->>GIP: ./gip.tool.licenseIssuer create-keypair --name framework
    Note over GIP: framework-private.pem stays offline
    DEV->>DEV: ./gip.tool.licenseIssuer create-keypair --name acme
    DEV->>GIP: send acme-public.pem
    GIP->>DEV: issue-developer --developer-public-key acme-public.pem<br/>→ ACME-developer.gip (DeveloperIssuer)
    DEV->>DEV: install ACME-developer.gip into Licenses/
    CUS->>CUS: BSOLicenseActivation → GetActivationData<br/>→ ExportActivationData → "Contoso-activation data.gip"
    CUS->>DEV: send activation data file
    DEV->>DEV: BSOiPlusLicenseManager → ImportActivationData<br/>(captures db, ds, code → CustomerDatabaseFingerprint)
    DEV->>DEV: assign packages, set DeveloperPrivateKeyPath = acme-private.pem
    DEV->>DEV: GenerateLicenseFile → ACME-license.gip (Product, bound to customer DB)
    DEV->>CUS: send ACME-license.gip
    CUS->>CUS: BSOLicenseActivation → ActivateProduct → restart
```

The `DeveloperIssuer` license is what makes `IsDeveloperWithIssuingRightsByFile`
true; without it `GenerateLicenseFile` refuses to run. The developer's private key
never leaves their machine — `gip.tool.licenseIssuer` and
`BSOiPlusLicenseManager.GenerateLicenseFile` both sign locally, no HTTP call.

#### 4.3.3 `BSOiPlusLicenseManager` V5 properties

| Property | Index | Purpose |
|---|---|---|
| `DeveloperPrivateKeyPath` | 514 | Path to the developer's `*-private.pem`; enables the V5 signing path |
| `CustomerDatabaseName` | 516 | Read-only, filled from the activation data `<db>` |
| `CustomerDatabaseServerName` | 517 | Read-only, filled from the activation data `<ds>` |
| `CustomerDatabaseFingerprint` | 518 | Derived from `<code>`; passed to `IssueProductLicenseFile` |

`IsEnabledGenerateLicenseFile` returns `false` when a private key is configured but
no customer fingerprint has been imported — this prevents issuing a license that can
never be activated.

**Removed in V5:** `SignLicenseFile` (510), `IsEnabledSignLicenseFile`,
`BrowseLicenseFile` (511) and the `LicenseFilePath` property (511). They implemented
the V4 flow "generate an unsigned XML license, then sign it in a second step". In V5
signing is part of `GenerateLicenseFile`, so the whole `Methods -> SignFile` region
became dead code and was deleted, together with `BgWorkerDoWork`,
`SignLicenseLocally`, the `infoSignLicenseStart`/`infoSignLicenseComplete` captions
and the `ACClassMethod`/`ACClassProperty` rows in both databases. Only
`BrowseDeveloperPrivateKey` (515) survived, in the renamed
`Methods -> Developer key` region.

> **The framework never deletes orphaned `ACClassMethod`/`ACClassProperty` rows.**
> `ACClassManager.InsertOrUpdateMethodParameters` only inserts and updates. Removing
> a method from code leaves its row behind, and the design can still bind to it. When
> you delete a method, also delete its `ACClassMethod` row (plus `VBGroupRight`,
> `ACClassMethodConfig`, `ACClassWF`, `ACClassWFEdge` references) and its
> `ACClassProperty` row.

**Removing a license (customer dialog).** `BSOLicenseActivation.RemoveLicense` asks for
confirmation and calls `License.RemoveActivatedLicense(VBSystem, out error)`, which deletes the
`VBSystem` row and sets `NeedRestart`. Removing the `FrameworkRuntime` license returns the
installation to the open edition. The button (`!RemoveLicense`) lives in the
`LicenceActivation` design (database only) and the `ACClassMethod` row had to be inserted
into every database by hand. `LicenseCompanyList` and `LicensedPackages` are stable
instances on purpose: replacing a ListBox `ItemsSource` during a selection change throws
in Avalonia (`LicensedPackages` must stay a `List<string>`, the registered property type).

**Package scope of a developer (`PackageNamespaces`).** `License.GetAvailablePackages()`
fills the "Available packages" list. In V4 it compared the first segment of every
`ACPackageName` with the developer's *company name* — in V5 the company name
("gipSoft d.o.o.") no longer equals the package prefix ("gip"), so the list was empty.
V5 reads `LicensePayload.PackageNamespaces` of the `DeveloperIssuer` license instead
(`issue-developer --package-namespace acme`); empty = all packages (minus
`VarioDevelopment`/`VarioLicense`/`VarioSystem`/`System`). The V4 company-name rule
remains as fallback only when no signed `DeveloperIssuer` payload exists.

**No V4 fallback in `GenerateLicenseFile`.** It used to fall back to the V4 XML license
when issuing failed, producing a file the V5 verifier rejects. It now requires the
private key and fingerprint (`IsEnabledGenerateLicenseFile`), validates that
`DeveloperPrivateKeyPath` is a private key (not a `.gip` license file) and shows the
error text in a warning.

**Self-contained product licenses (`LicensePayload.IssuerLicense`).** A product license is
signed by the developer key, but the customer installation knows only gip's framework key.
The issuing side therefore embeds the developer's gip-signed `DeveloperIssuer` license in the
product payload (`License.IssueProductLicenseFile` finds it by key id in the license folder /
`VBD` rows; `issue-product --issuer-license` does the same in the CLI). `LicenseVerifier`
verifies the embedded license with the framework key, checks that its public key matches
`IssuerKeyId`, and verifies the product signature with that key. The customer installs
**one** `.gip` file, no `Issuers/*.pem` and no developer license. Without the embedded
license the verifier falls back to the resolver (`Issuers/*.pem`). Issuing fails with a clear
error when the private key does not belong to any installed `DeveloperIssuer` license.
`BSOLicenseActivation.ActivateProduct` no longer has a V4 fallback and shows the
`ActivateLicenseFile` error text.

**Customer dialog states (`BSOLicenseActivation`).** `ACInit` shows the "activated" view when
a runtime/developer license is active **or** at least one product license (`VBP` row) is installed.
A product license without a `FrameworkRuntime` license is ineffective (open edition); the status text
says so explicitly ("Product license installed, but no framework runtime license…"). Every
`[ACMethodInfo]` button needs its own `IsEnabled<Method>` **and** a case for it in
`HandleExecuteACMethod`; without them the button stays disabled.

**Unissued records.** `BSOiPlusLicenseManager.New()` creates an incomplete record
(`VBLicense.PackageSystem` is `NOT NULL`, so it is initialised with an empty array).
`IsLicenseIssued` is `false` until `GenerateLicenseFile` succeeds; Save is disabled
meanwhile, and closing the dialog discards the record (`ACSaveOrUndoChanges` override)
instead of offering to save it.

#### 4.3.4 Several developers licensing the same customer

A customer routinely needs packages from more than one integrator. Nothing in the
model limits the number of installed product licenses:

| Concern | Behaviour |
|---|---|
| Storage | Every activation writes its own `VBSystem` row (`SystemName = "VBP"`). `ActivateLicenseFile` dedupes on `LicenseId` (`SystemInternal2`), **not** on `CustomerName`, so ACME's and Beta's licenses coexist. Re-activating the same file replaces its row. |
| Trusted issuer keys | `CreateVerifierWithIssuerChain()` collects **all** developer public keys: every `DeveloperIssuer` payload found in the license folder **and** in activated `VBD` rows, plus every `*.pem` in `<LicenseFolder>/Issuers/`. N developers → N trusted keys. |
| Package check | `IsPackageLicensedByFile` and `IsPackageLicensedByActivatedLicense` iterate **all** licenses; a package is licensed if **any** license covers it. |
| UI | `BSOLicenseActivation.LicenseCompanyList` = `License.SystemList` = all `VBP` rows. `AddNewLicense` (405) re-runs `GetActivationData` and shows the activation panel again, so the customer installs the second/third `.gip` the same way as the first. |
| Runtime license | Only **one** `FrameworkRuntime` license is effective — `FrameworkRuntimeLicense` returns the first match (folder first, then activated rows). |

```mermaid
flowchart LR
    subgraph Customer["Customer database (IPCDrivesV5)"]
        R1["VBP row<br/>ACME Product license"]
        R2["VBP row<br/>Beta Product license"]
        R3["VBP row<br/>gip FrameworkRuntime"]
    end
    R1 --> V["LicenseVerifier<br/>framework key + issuer chain"]
    R2 --> V
    R3 --> V
    V --> P["IsPackageLicensed('ACME.Custom') → true<br/>IsPackageLicensed('Beta.Quality') → true"]
```

**Do not** parse `VBSystem.SystemInternal` as a comma-separated package list. In V5
that column holds the complete signed `.gip` text. `BSOLicenseActivation` used to do
exactly that (a V4 leftover) and showed garbage in the package list; it now calls
`LicenseFile.TryParse` + `LicenseFile.DeserializePayload` and falls back to the
comma-separated form only for legacy rows.

---

## 5. Enforcement points

```mermaid
flowchart TD
    A["ACPackage.IsLicensed"] --> B["License.IsPackageLicensed(acPackage)"]
    C["ACClass.IsLicensed"] --> D{"ACClassInfo.ACPackageName<br/>== DB ACPackageName?"}
    D -->|"no"| E["false (DB manipulated)"]
    D -->|"yes"| A
    F["ACComponent.ACUrlCommand"] --> G["_HasLicense =<br/>License.IsComponentLicensed(ACPackageID)"]
    G -->|"ComponentLicense.False"| H["return null"]
    I["ACBSO.OnSave"] --> J["VerifyLicense()"]
    I --> K["VerifyMaxSessions()"]
```

| Point | File | Behaviour |
|---|---|---|
| `ACPackage.IsLicensed` | [ACPackage.cs](../../gip.core.datamodel/EntityACClass/ACPackage.cs) | `Database.Root.Environment.License.IsPackageLicensed(this)`; `true` while the root is not initialized |
| `ACClass.IsLicensed` | [ACClass.cs](../../gip.core.datamodel/EntityACClass/ACClass.cs) | Compares `ACClassInfo.ACPackageName` with the DB row (detects DB manipulation), then delegates to `ACPackage.IsLicensed`; caches in `_IsPackageLicensed` |
| `ACComponent.ACUrlCommand` | [ACComponent.cs](../../gip.core.autocomponent/ACComponent/ACComponent.cs) | `_HasLicense` starts as `ComponentLicense.TimeCheck`; on first call it is resolved and `null` is returned when `False`. Re-evaluated while `LicenseGraceStartUtc.HasValue`, so the block takes effect without a restart. This is the **server-side block** |
| `ACBSO.OnSave` | [ACBSO.cs](../../gip.core.autocomponent/ACComponent/ACBSO.cs) | `VerifyLicense()` shows `Warning50025` once when `IsLicenseWarningDue`; `VerifyMaxSessions()` opens `BSOiPlusHelp` when `IsMaxWinSessionsExceeded` (throttled to 4 h) |
| `Environment.MaxLicensedWinSessions` | [Environment.cs](../../gip.core.autocomponent/ACIPlus/Environment/Environment.cs) | `ACPropertyConfigValue<int>`, default `0` = unlimited; `IsMaxWinSessionsExceeded` returns `false` when `<= 0` |

`ComponentLicense`: `TimeCheck = 0`, `True = 10`, `False = 20`.
`LicenseType`: `Trial = 0`, `Developer_EndUser = 10`, `Developer_Issuer = 20`,
`RemoteDeveloper = 30`, `User = 40`.

`License.CurrentLicenseType` derives from the license files
(`Developer_Issuer` → `Developer_EndUser` → `RemoteDeveloper` → `User`).
`License.IsOpenEdition` is `!HasFrameworkRuntimeLicense` and is what the client title
bar shows as "Open Edition".

---

## 6. Package license metadata (GPL vs. commercial)

**Requirement:** a GPL-3 package must not run inside a commercially licensed iPlus
installation, and a commercial package must not run without a commercial runtime
license. Mixing must be impossible.

### 6.1 Where the metadata lives

| Location | Visible to | Enforceable at runtime |
|---|---|---|
| NuGet `.nuspec` (`PackageLicenseExpression`) | nuget.org, `dotnet restore` | ❌ never loaded by the app |
| **Assembly attribute** (`AssemblyMetadata`) | `Assembly.GetCustomAttributes()` | ✅ compiled into the DLL |
| Database row (`ACPackage`) | UI, SQL | ✅ but needs an EF schema change |

**Decision:** the `.nuspec` value is the legal/human declaration; the **assembly
attribute is the machine-readable copy the runtime trusts**. No DB schema change —
`ACPackage` gets a `[NotMapped]` property backed by an in-memory registry.

### 6.2 Vocabulary — reuse SPDX

| `iPlusLicense` value | Meaning | Open runtime | Commercial runtime |
|---|---|---|---|
| `GPL-3.0-or-later` | pure GPL, third party | ✅ | 🔴 blocked |
| `GPL-3.0-or-later OR LicenseRef-Commercial` | dual-licensed (gip's own packages) | ✅ | ✅ |
| `Commercial` | proprietary, commercial only | 🔴 blocked | ✅ |
| `MIT` / `Apache-2.0` / `BSD-3-Clause` | permissive | ✅ | ✅ |

**Rule:** a package may run under a commercial runtime license only if its expression
contains `Commercial` or is permissive. A bare `GPL-3.0-or-later` cannot — a third
party cannot relicense their GPL code, so combining it with commercially licensed
iPlus would violate the GPL.

**Developer installations are exempt.** A `DeveloperIssuer` or `DeveloperEndUser`
license file marks a development environment, where everything may run so the developer
can build and test both GPL and commercial packages. The mixing rule only applies to
end-customer installations (`License.IsPackageAllowedByLicenseExpression` returns
`true` early when `IsDeveloperByFile`).

`LicenseRef-Commercial` is the SPDX-correct form for a non-standard license.
`dotnet pack` accepts both forms and emits
`<license type="expression">…</license>` into the `.nuspec`; NuGet does **not**
validate SPDX ids at pack time.

### 6.3 The `GenerateAssemblyInfo` gotcha

The SDK's built-in `<AssemblyAttribute>` item **does not work in this repo**: all
projects set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` (they use
hand-written `Properties/AssemblyInfo.cs`), and `GetAssemblyAttributes` in
`Microsoft.NET.GenerateAssemblyInfo.targets` is gated on
`'$(GenerateAssemblyInfo)' == 'true'`.

**Working mechanism** — a target in [Directory.Build.targets](../../Directory.Build.targets)
that writes a generated `.g.cs` and adds it to `Compile`:

```xml
<PropertyGroup>
  <!-- Derive from the existing NuGet license expression, so no csproj needs editing -->
  <IPlusLicenseExpression Condition="'$(IPlusLicenseExpression)' == '' and '$(PackageLicenseExpression)' != ''">$(PackageLicenseExpression)</IPlusLicenseExpression>
  <IPlusLicenseExpression Condition="'$(IPlusLicenseExpression)' == ''">GPL-3.0-or-later</IPlusLicenseExpression>
</PropertyGroup>

<Target Name="GenerateIPlusLicenseAttribute" BeforeTargets="BeforeCompile;CoreCompile">
  <PropertyGroup>
    <_IPlusLicenseFile>$(IntermediateOutputPath)iPlusLicenseInfo.g.cs</_IPlusLicenseFile>
  </PropertyGroup>
  <WriteLinesToFile File="$(_IPlusLicenseFile)" Overwrite="true"
                    Lines="[assembly: System.Reflection.AssemblyMetadata(&quot;iPlusLicense&quot;, &quot;$(IPlusLicenseExpression)&quot;)]" />
  <ItemGroup>
    <Compile Include="$(_IPlusLicenseFile)" />
  </ItemGroup>
</Target>
```

Why it is written this way:

- **Derives from `PackageLicenseExpression`** → existing csproj files need no edits
  (`gip.ext.opctoolbox` is MIT and stays MIT).
- **`Directory.Build.targets`, not `.props`** → evaluated after the csproj, so it sees
  the final `PackageLicenseExpression`. Same reason the packaging logic lives there.
- **`BeforeTargets="BeforeCompile;CoreCompile"`** → runs per-TFM inner build, so
  multi-targeted projects get their own file.
- **`AssemblyMetadataAttribute` is a BCL type** → `gip.core.licensing` (dependency-free)
  can read it without referencing anything. A custom attribute would force every
  package to reference a shared assembly — rejected for that reason.

A commercial integrator only writes:

```xml
<PropertyGroup>
  <PackageLicenseExpression>LicenseRef-Commercial</PackageLicenseExpression>
</PropertyGroup>
```

### 6.3.1 Commercial module conventions

A commercial module needs **no new framework code**. Six conventions are enough:

| # | Convention | Value |
|---|---|---|
| 1 | Source location | Separate **private** repository (public repo ⇒ GPL) |
| 2 | `PackageId` | `iPlus.Commercial.<Module>` |
| 3 | `PackageLicenseExpression` | `LicenseRef-Commercial` |
| 4 | `ACPackageName` | `gip.Commercial.<Module>` |
| 5 | Assembly name | `gip.commercial.<module>.dll` |
| 6 | Delivery | NuGet reference (bundled) or module-pack zip in the app base dir |

Convention 5 is what makes discovery work: `ACClassManager.AssemblyNameSearchPattern`
contains `*.commercial.*.dll`. Convention 3 flows automatically into the assembly
metadata via `GenerateIPlusLicenseAttribute`, and convention 4 is the key that
`PackageLicenseRegistry` uses to look the expression up.

**Classification gotcha (fixed):** `PackageLicenseExpression.Classify` used to return
`Both` for a pure `LicenseRef-Commercial` expression, because the
`commercial || permissive` branch ran before the commercial-only check. That made the
`CommercialOnly` branch in `License.IsPackageAllowedByLicenseExpression` unreachable, so
a commercial module would have run **without** a product license. The order is now:

```csharp
if (commercial && !copyleft && !permissive) return LicenseCompatibility.CommercialOnly;
if (commercial || permissive)               return LicenseCompatibility.Both;
if (copyleft)                               return LicenseCompatibility.OpenOnly;
return LicenseCompatibility.Unknown;
```

### 6.4 Runtime read path

```mermaid
flowchart TD
    A["ACClassManager.RegisterAssemblyTypes(assembly)"] --> B["Read AssemblyMetadata 'iPlusLicense'"]
    B --> C["PackageLicenseRegistry.RegisterFromAssembly(assembly, packageNames)"]
    C --> D["License.IsPackageLicensed(acPackage)"]
    D --> D0{"IsDeveloperByFile?"}
    D0 -->|"yes (developer installation)"| G["allow"]
    D0 -->|"no"| E{"IsCommercialRuntime?"}
    E -->|"no (open edition)"| F{"CommercialOnly?"}
    F -->|"yes"| H["block"]
    F -->|"no"| G
    E -->|"yes (commercial)"| I{"OpenOnly?"}
    I -->|"yes"| H
    I -->|"no"| J{"CommercialOnly?"}
    J -->|"yes"| K{"covered by a Product license?"}
    K -->|"yes"| G
    K -->|"no"| H
    J -->|"no (Both / Unknown)"| G
```

[ACClassManager.RegisterAssemblyTypes](../../gip.core.autocomponent/ACClassManager/ACClassManager.cs)
is the single choke point where every assembly is loaded and its types registered —
`RegisterPackageLicense(assembly, types)` reads the attribute once, collects the
distinct `ACClassInfo.ACPackageName` values and calls
`PackageLicenseRegistry.RegisterFromAssembly`. The registry lives in
`gip.core.datamodel` (which already references `gip.core.licensing`).

`ACPackage` exposes two `[NotMapped]` properties backed by the registry:
`LicenseExpression` and `LicenseCompatibility` — **no EF schema change**.

**Fail-open during startup:** an unknown expression is allowed. Enforcement kicks in at
`ACClass.IsLicensed` / `ACComponent.ACUrlCommand`, which run after registration. This
avoids breaking startup ordering.

`License.IsComponentLicensed(packageID)` caches the per-package result in
`_ComponentLicenseCache`; `ReloadFileLicenses()` calls
`InvalidateComponentLicenseCache()` so a newly activated license takes effect without a
restart.

### 6.5 Grace period

V4 behaviour to preserve: after **30 minutes** the client shows a message; after
**1–2 days** the server blocks `ACUrlCommand`, so the program stops working. This gives
the end user time to resolve the license issue.

| Stage | Trigger | Effect | Where |
|---|---|---|---|
| 1 — warning | `IsLicenseWarningDue` (30 min) | `Messages.WarningAsync(this, "Warning50025")` once | `ACBSO.VerifyLicense()` |
| 2 — block | `IsLicenseBlockDue` (1.5 days) | `IsComponentLicensed` → `False` → `ACUrlCommand` returns `null` | `License.IsComponentLicensed` |

**Soft fail, not hard fail:** the system keeps running; only the offending package's
components become unavailable, and the license manager shows why.

The clock is persisted in `<LicenseFolder>/.licensegrace`, so a service restart does not
reset it. `ACComponent.ACUrlCommand` re-evaluates `_HasLicense` whenever
`LicenseGraceStartUtc.HasValue`, so the block takes effect without a restart.

`ACBSO.VerifyLicense()` only fires when `RootPageWPF != null` (client side), so the
warning is a client concern while the block is enforced on the server. `VerifyLicense`
is called from `ACBSO.OnSave()`.

---

## 7. Packaging pipeline (NuGet)

Two separate channels — one artifact must not serve both audiences:

| Channel | Script | Output | Audience |
|---|---|---|---|
| Source / developer | `build/pack-local.sh` | `./local-packages/*.nupkg` | developers building on iPlus |
| Binary / end user | `build/publish-release.sh` | `./artifacts/release/*.zip` | end customers running iPlus |

### 7.1 Configuration files

| File | Contains |
|---|---|
| [build/iPlus.Package.props](../../build/iPlus.Package.props) | `IPlusPackageVersion` (`5.0.0-preview.4`), `IPlusMESPackageVersion`, `IPlusSourceRepoRoot`, `IPlusPackExclude` |
| [Directory.Build.props](../../Directory.Build.props) | imports `build/iPlus.Package.props` |
| [Directory.Build.targets](../../Directory.Build.targets) | the packable logic (must be `.targets`, see below) |

### 7.2 Why the packable logic lives in `Directory.Build.targets`

- The packable decision depends on the **final** `IPlusSourceRepoRoot`, which dependent
  repos (iPlusMES) override *after* importing iPlus's `Directory.Build.props`.
- Item fixups (`icon.png`, `readme.md`, `DbScripts`, `VBControlScripts`) must run after
  the default items have been globbed.
- Multi-targeted projects are packed from the **outer** (cross-targeting) evaluation,
  which does **not** glob default items — therefore `icon.png` / `readme.md` are
  `Remove`+`Include`'d (not `Update`'d) so they exist in both evaluations.

### 7.3 What the packable block sets

```xml
<IPlusIsPackable>true</IPlusIsPackable>          <!-- only inside an iPlus source repo -->
<IsPackable>true</IsPackable>                    <!-- re-asserted for WebAssembly SDK -->
<PackageId>iPlus.$(MSBuildProjectName)</PackageId>  <!-- nuget.org has "gip" reserved -->
<Version>$(IPlusSuitePackageVersion)</Version>
<PackageLicenseExpression>GPL-3.0-or-later OR LicenseRef-Commercial</PackageLicenseExpression>  <!-- fallback -->
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
<PackageOutputPath>$(IPlusSourceRepoRoot)local-packages</PackageOutputPath>
```

Assembly names, namespaces and project names stay unchanged — only the **package id**
gets the `iPlus.` prefix.

Legacy `gip.ext.design*`, `gip.ext.widgets*`, `gip.ext.xamldom*` produce no PDBs, so
`IncludeSymbols` is disabled for them (nuget.org rejects empty `.snupkg` with HTTP 400).

**Dual-licensing:** all 43 gip csproj files and all 24 iPlusMES csproj files set
`<PackageLicenseExpression>GPL-3.0-or-later OR LicenseRef-Commercial</PackageLicenseExpression>`.
`gip.ext.opctoolbox` stays `MIT`. The fallback in `Directory.Build.targets` is the same
dual expression, so a new project is dual-licensed by default.

### 7.4 Exclusions

`IPlusPackExclude` (in `build/iPlus.Package.props`) lists launch projects, tools and
tests that must not be packed. The trailing `;` is required for exact-name matching in
`Directory.Build.targets`. `build/pack-local.sh` keeps a **duplicate** list in its
`EXCLUDE` variable — keep both in sync.

---

## 8. Publish pipeline (binaries)

`build/publish-release.sh` publishes ready-to-run binaries per platform into
`./artifacts/release` and writes `SHA256SUMS`. `.github/workflows/release.yml` runs it
in CI and attaches the archives to a GitHub Release.

### 8.1 Flags that are not optional

| Flag | Why |
|---|---|
| `-p:BuildAndroidTargetFramework=false` | `gip.core.autocomponent` adds an `-android` TFM on Linux; that breaks every non-Android publish (`NU1102`). The `android` platform enables it automatically |
| `-p:ErrorOnDuplicatePublishOutputFiles=false` | two different `VBControlScripts/2026-04-13 08-58_dlisak.zip` files share a name |
| `IPLUS_USE_AVALONIA_FORK=false` | CI and fresh clones must consume the `iPlus.*` Avalonia packages from nuget.org, not the fork checkout |

### 8.2 The browser / Roslyn `BuildHost` problem

`Microsoft.CodeAnalysis.Workspaces.MSBuild` (transitive via `gip.core.datamodel` →
`gip.iplus.client.avui`) ships `BuildHost-net472/` and `BuildHost-netcore/` content
folders. Both contain a `Microsoft.Build.Locator.dll` with the **same assembly identity
but different content**. The WASM `ManagedToNativeGenerator` scans every assembly in
`WasmAssembliesToBundle` and aborts with:

```text
The assembly 'Microsoft.Build.Locator' has already been loaded into this MetadataLoadContext
```

The build host is a desktop-only Roslyn helper that is never used in the browser, so
[gip.iplus.client.avui.Browser.csproj](../../gip.iplus.client.avui.Browser/gip.iplus.client.avui.Browser.csproj)
drops it from the publish list:

```xml
<Target Name="RemoveRoslynBuildHostFromPublish" AfterTargets="ComputeFilesToPublish" BeforeTargets="PrepareInputsForWasmBuild">
  <ItemGroup>
    <ResolvedFileToPublish Remove="@(ResolvedFileToPublish)"
                           Condition="$([System.String]::Copy('%(ResolvedFileToPublish.RelativePath)').StartsWith('BuildHost-'))" />
  </ItemGroup>
</Target>
```

Verified result: browser publish produces 1503 files with **zero** `BuildHost` leakage.

### 8.3 Platforms and the combined bundle

`--platform` accepts `desktop-win`, `desktop-linux`, `wpf-win`, `browser`, `android`,
`service`, `console`, `setup` and `all`. `all` builds every platform and then produces
`iplus-<ver>-all.zip`.

**Bundle layout matters.** The combined archive must have **one folder per platform at
its root** — no extra `iplus-<ver>-all/` wrapper:

```text
iplus-5.0.0-all.zip
├── iplus-5.0.0-desktop-linux-x64/…
├── iplus-5.0.0-desktop-win-x64/…
└── iplus-5.0.0-setup-win-x64/…
```

The correct way to produce that is to zip the *contents* of the staging folder:

```bash
(cd "$stage" && zip -qr "$target" .)
```

Moving each platform folder's contents up one level instead collides on shared
subdirectories such as `BuildHost-net472/`.

Platform folder names carry the version prefix, so the installer matches them by
**suffix** (`-<arch>`), never by exact name.

### 8.4 The installer (`gip.tool.installerAndUpdater`)

Rewritten from the V4 WPF `net6.0-windows` app into a cross-platform Avalonia `net10.0`
application. It installs or updates from a GitHub Release, a local zip, or an extracted
folder, writes `ConnectionStrings.config`, optionally creates the database and applies
`initial-install-*.sql`, creates a shortcut and starts the app. There is **no
auto-update** — enterprises want to control when an update is applied.

**Build quirk:** the Avalonia fork's XAML compilation is gated on the project name
containing `.avui` ([build/AvaloniaBaseCompileInclude.props](../../build/AvaloniaBaseCompileInclude.props)).
`gip.tool.installerAndUpdater` does not match, so it needs a **project-local
`Directory.Build.props`** that sets `IsAvaloniaUIProject=true` *before* importing the root
props. Setting it in the `.csproj` body is too late — the root props are already
evaluated. Re-importing `AvaloniaBaseCompileInclude.props` triggers `MSB4011` and is
ignored.

Fork projects also need the **managed** `SkiaSharp` and `HarfBuzzSharp` packages, not
just the native-asset packages.

### 8.4.1 Android APK signing

Android does **not** use Authenticode. Two facts:

- **Sideloading needs no keystore.** `dotnet workload install android` is the only
  prerequisite; the build auto-signs with the per-machine debug key
  (`META-INF/ANDROIDD.RSA`).
- **Updates need a stable keystore.** The debug key is generated per machine, so every CI
  run yields a different signature and Android refuses to install the new APK over the old
  one ("App not installed").

`publish-release.sh` reads `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`,
`ANDROID_KEY_ALIAS` and `ANDROID_KEY_PASSWORD` and passes
`-p:AndroidKeyStore=true -p:AndroidSigningKeyStore=… -p:AndroidSigningKeyAlias=…`.
Verified: with a keystore the APK contains `META-INF/IPLUS.RSA` (subject
`CN=iPlus Test, O=gipSoft`) instead of `META-INF/ANDROIDD.RSA`.

**iOS** is blocked by macOS + Xcode + Apple provisioning, not by an SDK install. Deferred.

### 8.5 Code signing and publisher verification

Signing is driven by `SIGNING_MODE=none|self-signed|azure` in the workflow, and by
[build/sign-release.sh](../../build/sign-release.sh) locally.

**Why Authenticode and not strong names:** `GIP-SNKey.snk` is committed to the public
repository, so anyone can produce a valid strong name. The `iPlusLicense` assembly
metadata is a plain string and equally forgeable. Authenticode is the only mechanism that
proves *who built* a binary.

The runtime verifies the signer of every assembly it registers:

| Type | Role |
|---|---|
| [`AuthenticodeVerifier`](../../gip.core.licensing/AuthenticodeVerifier.cs) | `WinVerifyTrust` P/Invoke, extracts the signer thumbprint. No-op on Linux/macOS |
| [`TrustedPublisherStore`](../../gip.core.licensing/TrustedPublisherStore.cs) | Reads `Licenses/TrustedPublishers.txt` (thumbprints + `mode=`); `IPLUS_PUBLISHER_VERIFICATION` overrides |
| [`ACClassManager.RegisterAssemblyTypes`](../../gip.core.autocomponent/ACClassManager/ACClassManager.cs) | Calls `IsPublisherAllowed` before registering any type |

Modes: `off` (default), `warn`, `enforce`. The check is **fail-open** — an empty
thumbprint list, a non-Windows host, or a verification error all allow the assembly. A
misconfigured trust store must never lock a customer out of their own runtime.

---

## 9. Pitfalls checklist

- [ ] **`<AssemblyAttribute>` used for license metadata** → silently ignored, because `GenerateAssemblyInfo=false`. Use the `GenerateIPlusLicenseAttribute` target.
- [ ] **`IPlusLicenseExpression` set in `Directory.Build.props`** → evaluated before the csproj, so it cannot see `PackageLicenseExpression`. Put it in `.targets`.
- [ ] **Custom attribute instead of `AssemblyMetadata`** → forces every package to reference a shared assembly. Use `AssemblyMetadataAttribute` (BCL).
- [ ] **`PackageLicenseExpression` set per-csproj only** → the fallback in `Directory.Build.targets` never applies. Set it centrally.
- [ ] **`IPlusPackExclude` and `pack-local.sh` `EXCLUDE` out of sync** → a tool gets packed or a library is skipped.
- [ ] **`PackageId` without the `iPlus.` prefix** → nuget.org rejects it ("gip" is reserved).
- [ ] **`BuildAndroidTargetFramework` left `true`** on a desktop/browser/service publish → `NU1102`.
- [ ] **`ErrorOnDuplicatePublishOutputFiles` left `true`** → publish fails on the duplicate `VBControlScripts` zip.
- [ ] **`IPLUS_USE_AVALONIA_FORK=true` in CI** → packages bind to fork assemblies that do not exist on nuget.org.
- [ ] **`BuildHost-*` not removed for browser** → WASM build aborts on duplicate `Microsoft.Build.Locator`.
- [ ] **`ActivateLicenseFile` dedupe on `CustomerName`** → two developers licensing the same customer overwrite each other. Key on `LicenseId` (`SystemInternal2`). *Fixed.*
- [ ] **Combined bundle built by moving folder contents up a level** → collides on `BuildHost-net472/`. Zip the staging folder's *contents* instead.
- [ ] **Platform folder matched by exact name** → folder names carry the version prefix. Match by suffix (`-<arch>`).
- [ ] **`IsAvaloniaUIProject` set in the `.csproj`** → too late, the root props are already evaluated. Use a project-local `Directory.Build.props`.
- [ ] **Only `SkiaSharp.NativeAssets.*` referenced** → `DllNotFoundException` for the managed `SkiaSharp`/`HarfBuzzSharp` assemblies. Add the managed packages too.
- [ ] **Publisher verification defaulting to `enforce`** → locks every customer out on first start. Default must be `off`, and the check must be fail-open.
- [ ] **Android APK built in CI without a keystore** → each run gets a different debug signature, so updates cannot install over the previous version. Configure `ANDROID_KEYSTORE_BASE64`.
- [ ] **`Classify` checking `commercial || permissive` before commercial-only** → `LicenseRef-Commercial` classifies as `Both`, the `CommercialOnly` branch becomes unreachable, and commercial modules run without a product license. *Fixed.*
- [ ] **Commercial module assembly not named `gip.commercial.*.dll`** → `ACClassManager` never discovers it. The pattern is in `AssemblyNameSearchPattern`.
- [ ] **`FrameworkLicenseKeys.CreateVerifier()` used for developer-signed licenses** → always uses gip's key, so `Product` licenses fail with `InvalidSignature`. Use `CreateVerifierWithIssuerChain()` / `CreateVerifier(resolver)`. *Fixed.*
- [ ] **`MaxUsers` never read** → `Environment.MaxLicensedWinSessions` stays `0` (= unlimited), so the per-user runtime license is unenforceable. *Fixed* in `ACStartUpRoot.LoginUser`.
- [ ] **`LicenseFormat.FrameworkPublicKeyResourceName`** → must match the csproj `LogicalName` (`gip.core.licensing.FrameworkPublicKey.pem`). *Fixed.*
- [ ] **`License.IsPackageLicensed` returning `true`** → no enforcement at all. The real bodies were commented out under `//#if DEBUG`. *Fixed.*
- [ ] **`License.Initialize()` returning immediately** → license files were never loaded. It must call `ReloadFileLicenses()`. *Fixed.*
- [ ] **`License.CurrentLicenseType` hardcoded to `User`** → `IsDeveloperWithIssuingRights` was always `false`, so nobody could issue licenses. Derive it from `FileLicenses`. *Fixed.*
- [x] **`BSOiPlusLicenseManager.SignLicense()` POSTing to `iplus-framework.com`** → a phone-home dependency that breaks offline installs. *Fixed:* signing happens locally inside `GenerateLicenseFile` with the developer's private key. The obsolete two-step `SignLicenseFile`/`BrowseLicenseFile`/`LicenseFilePath` path was deleted entirely in V5.
- [ ] **`BSOLicenseActivation.ActivateProduct()` only reading V4 XML** → cannot read `.gip`. Try `ActivateLicenseFile` first, fall back to V4. *Fixed.*
- [x] **`BSOLicenseActivation.SelectedLicenseCompany` parsing `SystemInternal` as a comma-separated package list** → a V4 leftover. In V5 that column holds the complete signed `.gip` text, so the package list in the UI showed garbage. *Fixed:* `GetLicensedPackages` now calls `LicenseFile.TryParse` + `LicenseFile.DeserializePayload` and only falls back to the comma-separated form for legacy rows.
- [x] **`BSOLicenseActivation` not refreshing `LicenseCompanyList` after activation** → a second/third license was stored but the list stayed stale. *Fixed:* `ActivateProduct` raises `OnPropertyChanged("LicenseCompanyList")` and selects the newly added row; `ACInit` selects the last row too.
- [x] **`BSOLicenseActivation.AddNewLicense` opening a second `Database` connection** → it queried `VBSystem` through a fresh `new Database()` instead of the already-open one. *Fixed:* it reads `Root.Environment.License.SystemList`.
- [x] **`LicenseGraceStartUtc` in memory only** → a restart resets the grace clock. *Fixed:* persisted to `<LicenseFolder>/.licensegrace` and restored by `License.GracePeriod`.
- [x] **`gip.tool.devLicense` / `gip.tool.devLicenseProvider` still in the tree** → V4 dongle tooling. *Fixed:* both projects deleted, together with `Mxapi.cs`/`Mxtea.cs` and the dongle regions in `License.cs`.
- [x] **`License.SystemID` guard reading `SystemID.HasValue` instead of `_SystemID.HasValue`** → infinite recursion (`SystemID` → `IsComponentLicensed` → `ACUrlCommand` → `PARole.GetServiceInstance` → `PAMSilo.ACInit` → `ACUrlCommand` → `IsComponentLicensed` → `SystemID`), which crashed the MES console with `Stack overflow. Repeated 22705 times`. *Fixed.* **Rule:** a lazy-cached property must test its **backing field**, never itself. This is invisible in iPlus (no `System` package → `_SystemID` stays `null`) but fatal in iPlusMES.
- [x] **EF Core cannot translate `byte[].Length`** → `Translation of method 'Enumerable.Any' failed` at login. `VBSystem.SystemInternal` is a `byte[]`; EF rewrites `.Length > 0` to `.Any()`. *Fixed:* filter on `SystemName` server-side, then `.AsEnumerable()` before the length check. `VBSystem` holds only a handful of rows, so client evaluation is free.
- [ ] **Stale `ConnectionStrings.config` in a shared output folder** → `gip.mes.console` failed with `SQL Server ... error: 26 - Fehler beim Suchen des angegebenen Servers` because `iPlusMES/bin/Debug/net10.0/ConnectionStrings.config` was a leftover from an older build containing the Windows placeholder `data source=.\SQLEXP16`. All MES projects share `$(SolutionDir)bin\$(Configuration)`, so whichever project copies last wins, and `CopyToOutputDirectory=Always` only refreshes when that project is actually rebuilt. *Fix:* rebuild the owning project (`dotnet build gip.mes.console`) — the copy is then logged as `Die Datei wird von ... kopiert`. When a console host fails at "Connecting to database" while another host works, compare the **output-folder** config, not the source-tree one.
- [x] **`gip.tool.licenseIssuer: Befehl nicht gefunden`** → the tool is built into the shared output folder, which is **not** on `PATH`. A bare `gip.tool.licenseIssuer ...` therefore fails even when the file is right there. *Fix:* invoke it as `./gip.tool.licenseIssuer ...` from the output folder, with the full path from anywhere, or as `dotnet gip.tool.licenseIssuer.dll ...`. The readme examples were missing the `./` prefix and have been corrected.

---

## 10. Reference implementations in the framework

| Pattern | Class | Notes |
|---|---|---|
| License engine (V4 + file bridge) | [License.cs](../../gip.core.datamodel/ACHelperclasses/ACLicense/License.cs) | `partial`; `IsPackageLicensed`, `IsComponentLicensed`, `LicenseType`, `ComponentLicense` |
| File-based licensing | [License.FileLicensing.cs](../../gip.core.datamodel/ACHelperclasses/ACLicense/License.FileLicensing.cs) | `LicenseFolder`, `FileLicenses`, `ActivateLicenseFile`, `IssueProductLicenseFile` |
| Offline verification | [LicenseVerifier.cs](../../gip.core.licensing/LicenseVerifier.cs) | `Verify`, `VerifyFile`, `LicenseBindingContext` |
| Signing | [LicenseIssuer.cs](../../gip.core.licensing/LicenseIssuer.cs) | `Issue`, `IssueToFile`, `GetPublicKeyPem` |
| Framework key resolution | [FrameworkLicenseKeys.cs](../../gip.core.licensing/FrameworkLicenseKeys.cs) | env → embedded → file, cached |
| DB fingerprint | [DatabaseFingerprint.cs](../../gip.core.licensing/DatabaseFingerprint.cs) | mirrors V4 `DBInfo.GetCheckSum()` |
| Issuer CLI | [gip.tool.licenseIssuer/](../../gip.tool.licenseIssuer/readme.md) | `create-keypair`, `issue-developer`, `issue-runtime`, `issue-product`, `verify`, `show`. Lives in the output folder, which is not on `PATH` — invoke as `./gip.tool.licenseIssuer` |
| End-customer activation UI | [BSOLicenseActivation.cs](../../gip.bso.iplus/BSOiPlus/BSOLicenseActivation.cs) | `ActivateProduct()` — `ActivateLicenseFile` first, V4 XML fallback; `AddNewLicense()` installs a 2nd/3rd license; `GetLicensedPackages()` parses the `.gip` payload |
| Developer/integrator UI | [BSOiPlusLicenseManager.cs](../../gip.bso.iplus/BSOiPlus/BSOiPlusLicenseManager.cs) | `GenerateLicenseFile()` (local signing via `DeveloperPrivateKeyPath`, bound to `CustomerDatabaseFingerprint`), `ImportActivationData()` (captures `db`/`ds`/`code`) |
| SPDX classifier | [PackageLicenseExpression.cs](../../gip.core.licensing/PackageLicenseExpression.cs) | `Classify`, `IsAllowedUnderCommercialRuntime` |
| Package → expression registry | [PackageLicenseRegistry.cs](../../gip.core.datamodel/ACHelperclasses/ACLicense/PackageLicenseRegistry.cs) | `RegisterFromAssembly`, `GetExpression`, `GetCompatibility` |
| Assembly registration choke point | [ACClassManager.cs](../../gip.core.autocomponent/ACClassManager/ACClassManager.cs) | `RegisterAssemblyTypes` — where the license attribute is read |
| Package license check | [ACPackage.cs](../../gip.core.datamodel/EntityACClass/ACPackage.cs) | `IsLicensed` → `IsPackageLicensed` |
| Class license check | [ACClass.cs](../../gip.core.datamodel/EntityACClass/ACClass.cs) | `IsLicensed` — also detects DB manipulation |
| Server-side block | [ACComponent.cs](../../gip.core.autocomponent/ACComponent/ACComponent.cs) | `ACUrlCommand` + `_HasLicense` |
| Client-side warning | [ACBSO.cs](../../gip.core.autocomponent/ACComponent/ACBSO.cs) | `VerifyLicense`, `VerifyMaxSessions` |
| Session limit | [Environment.cs](../../gip.core.autocomponent/ACIPlus/Environment/Environment.cs) | `MaxLicensedWinSessions`, `IsMaxWinSessionsExceeded` |
| Packaging config | [build/iPlus.Package.props](../../build/iPlus.Package.props) | versions, `IPlusPackExclude` |
| Packable logic | [Directory.Build.targets](../../Directory.Build.targets) | `IPlusIsPackable`, `PackageId`, license fallback |
| NuGet packing | [build/pack-local.sh](../../build/pack-local.sh) | writes `./local-packages` |
| Binary publishing | [build/publish-release.sh](../../build/publish-release.sh) | writes `./artifacts/release` |
| Release workflow | [.github/workflows/release.yml](../../.github/workflows/release.yml) | CI matrix + GitHub Release |

---

## 11. Further reading

- [iplus-client-server-components.md](./iplus-client-server-components.md) — client-server
  programming, proxies, DTOs, `ACUrlCommand`
- [iplus-messages-workflow.md](../iplus-messages-workflow.md) — error messages and `ACClassMessage`
- [add-iplusV4-dbfield.md](../add-iplusV4-dbfield.md) — adding a DB field to an entity
- [docs/HANDOVER-dual-licensing-distribution.md](../HANDOVER-dual-licensing-distribution.md) —
  the full dual-licensing strategy, decisions and gap list
- [docs/release-process.md](../release-process.md) — operational release documentation
- [gip.core.licensing/readme.md](../../gip.core.licensing/readme.md) — library overview
- [gip.tool.licenseIssuer/readme.md](../../gip.tool.licenseIssuer/readme.md) — CLI usage
- Online documentation: <https://iplus-framework.com/en?section=Dual%20Licensing>
