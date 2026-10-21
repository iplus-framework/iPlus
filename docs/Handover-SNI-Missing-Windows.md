# HANDOVER: DllNotFoundException 'Microsoft.Data.SqlClient.SNI.dll' on Windows (fork package mode)

> Give this file to a fresh VS Code chat on the Windows machine. It contains everything
> needed to diagnose and fix the problem without re-doing the analysis.

## 1. Symptom

Launching the iPlus client on Windows (built in **package mode**, i.e. consuming the
`iPlus.*` NuGet packages from nuget.org) throws at startup:

```
System.TypeInitializationException in Microsoft.EntityFrameworkCore.Relational.dll:
  'The type initializer for 'Microsoft.Data.SqlClient.TdsParser' threw an exception.'
Innermost: System.DllNotFoundException: Unable to load DLL 'Microsoft.Data.SqlClient.SNI.dll'
  or one of its dependencies (0x8007007E)
   at Interop.Windows.Sni.SniNativeMethods.SNIInitialize(IntPtr pmo)
   at Microsoft.Data.SqlClient.SniNativeWrapper.SniInitialize()
   at Microsoft.Data.SqlClient.SNILoadHandle..cctor()
```

Works fine on Linux (managed SNI is the default there) — the failure is Windows-only.

## 2. ROOT CAUSE (already confirmed by inspecting the nupkg)

The fork package `iPlus.Microsoft.Data.SqlClient 7.1.1-iplus.1` was hand-rewritten
(`src/Microsoft.Data.SqlClient/src/Microsoft.Data.SqlClient.nuspec` in the SqlClient fork,
branch `iplus/browser-transport`) and the dependency groups were **pruned to only
net10.0 + net10.0-browser1.0**. In that rewrite the **`Microsoft.Data.SqlClient.SNI.runtime`
dependency was dropped** and no `runtimes/` folder is packed.

Verified against the published nupkg (local cache
`/home/damir/SHARED/Devel/iPlusGit/V5/packages/iplus.microsoft.data.sqlclient/7.1.1-iplus.1/`):

- `lib/` contains only `net10.0` and `net10.0-browser1.0` (correct).
- nuspec `<dependencies>` for `net10.0` lists Abstractions, Internal.Logging,
  SqlServer.Server, Bcl.Cryptography, Caching.Memory, IdentityModel.*, ConfigurationManager,
  Pkcs, RateLimiting — **but NOT `Microsoft.Data.SqlClient.SNI.runtime`**.
- No `runtimes/` entries in the package at all.

The official `Microsoft.Data.SqlClient` net8.0/net9.0 group declares:

```xml
<dependency id="Microsoft.Data.SqlClient.SNI.runtime" version="x.y.z" />
```

That package carries `runtimes/win-x64/native/Microsoft.Data.SqlClient.SNI.dll` (+ arm64/x86),
which the fork's `SniProxy`/`SniNativeWrapper` P/Invokes on Windows when the **native** SNI
is selected (default on Windows for net10.0).

## 3. Fix options (pick one)

### Option A (recommended): add the SNI.runtime dependency back to the fork nuspec

In `SqlClient/src/Microsoft.Data.SqlClient/src/Microsoft.Data.SqlClient.nuspec`, add to the
**net10.0 group only** (NOT net10.0-browser1.0 — WASM must not pull native SNI):

```xml
<dependency id="Microsoft.Data.SqlClient.SNI.runtime" version="6.0.2" />
```

- Use the same version the upstream 7.1 release used — check
  `Directory.Packages.props` in the SqlClient fork (`MicrosoftDataSqlClientSniPackageVersion`
  or similar) or the official `Microsoft.Data.SqlClient` 7.1.1 nuspec on nuget.org.
- The fork csproj already conditionally references `Microsoft.Data.SqlClient.SNI.runtime`
  for net8.0/net9.0 — check whether the `net10.0` TFM is included in that conditional
  `ItemGroup` (it was added for net8/9 only; net10.0 may need to be added so the build
  also works in project-reference mode).
- Then repack `7.1.1-iplus.2` (see pack commands in
  `docs/SqlTunnel-OptionB-Implementation.md` §0), upload to nuget.org, bump
  `iPlus/Directory.Packages.props` (`iPlus.Microsoft.Data.SqlClient` version) and retest.

### Option B: reference the official SNI.runtime package directly in iPlus

Add to `iPlus/Directory.Packages.props` + the client csproj:

```xml
<PackageReference Include="Microsoft.Data.SqlClient.SNI.runtime" Version="6.0.2" />
```

This restores the native DLL without republishing the fork. Works, but it is a hidden
requirement every consumer must remember — Option A is the correct long-term fix.

### Option C: force managed SNI on Windows

Connection string / AppContext switch
`Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows=true`. NOT recommended —
untested on Windows in this fork, and changes behavior vs. the official driver.

## 4. Repack + test procedure (after fix)

Pack commands are documented in `docs/SqlTunnel-OptionB-Implementation.md` §0 (pack into
`../packages-local`, purge the redirected NuGet cache
`/home/damir/SHARED/Devel/iPlusGit/V5/packages/iplus.microsoft.data.sqlclient*` before
re-restoring). Bump to `7.1.1-iplus.2`, verify the new nupkg contains the dependency
(`unzip -p *.nupkg *.nuspec | grep SNI`), then on Windows:

1. Update package version in `iPlus/Directory.Packages.props`.
2. `dotnet restore` + run the client — the exception must be gone.
3. Verify `Microsoft.Data.SqlClient.SNI.dll` lands in `bin/Debug/net10.0/runtimes/win-x64/native/`
   (or is copied to the output root).

## 5. Context: how iPlus consumes the fork

- Package mode (default): `UseSqlClientForkProjectReference=False` in
  `iPlus/Directory.Build.props`; `Directory.Build.targets` does
  `<PackageReference Update="Microsoft.Data.SqlClient" ExcludeAssets="all" />` to suppress
  the *official* SqlClient flowing transitively from `iPlus.EntityFrameworkCore.*`, and
  `gip.core.datamodel`/`gip.bso.iplus` reference `iPlus.Microsoft.Data.SqlClient` directly.
- Project mode (fork debugging): `-p:UseSqlClientForkProjectReference=True` adds a signed
  ProjectReference to `../SqlClient/src/...` (token `9b460b8d8350bb47`). In this mode the
  SNI.runtime package flows via the fork csproj's own PackageReference — check that the
  net10.0 TFM is covered there too (see Option A note).
- Fork repo: `git@github.com:iplus-framework/SqlClient.git`, branch `iplus/browser-transport`.
- Fork TFMs: `net10.0;net10.0-browser` (browser TFM = sync-XHR tunnel transport, `IPLUS_BROWSER`
  define). The browser TFM must NOT get the SNI dependency (no native code on WASM).

## 6. Related gotchas (do not reintroduce)

- Browser TFM in the nuspec must be written `net10.0-browser1.0` (platform version required),
  both in dependency groups and `<file target="lib\net10.0-browser1.0\">`.
- Do NOT pass `-p:Version=` to the main pack command — dependency ranges derive from
  `SqlClientPackageVersion`/`SqlServerPackageVersion`.
- NuGet global cache on the dev machine is redirected to
  `/home/damir/SHARED/Devel/iPlusGit/V5/packages` — purge stale fork versions there after
  repacking, otherwise restore picks the old package.
