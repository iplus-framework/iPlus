# Handover: NU5050 duplicate icon on Android pack

**Date:** 2026-10-08
**Status:** Not critical — only affects `gip.iplus.client.avui.Android`. All other packages pack and publish fine.

## Context

We renamed all NuGet package IDs from `gip.*` to `iPlus.gip.*` (nuget.org has the
`gip` prefix reserved by another owner) and added package icons via
`Directory.Build.targets`:

```xml
<PackageIcon>icon.png</PackageIcon>  <!-- set when icon.png exists -->
<None Include="icon.png" Pack="true" PackagePath="\" />
```

This works for all projects **except** `gip.iplus.client.avui.Android`, which fails with:

```
error NU5050: Attempted to pack multiple files into the same location(s).
The following destinations were used multiple times: /icon.png
```

## What we found so far

- The Android csproj contains `<AndroidResource Include="Icon.png">` (capital I,
  app icon for the APK) **and** our copied lowercase `icon.png`.
- Without the lowercase `icon.png`, the pack **succeeds** but the package has **no
  icon at all** (the `AndroidResource` is NOT packed into the nupkg).
- Dumping `_PackageFiles` right before `GenerateNuspec` shows only **one** entry
  for `icon.png` (TargetPath='') — yet NU5050 claims `/icon.png` is used twice.
- The generated nuspec in `obj/` contains only ONE icon file entry.
- `PackageBuilder.ValidateFilesUnique` compares paths **case-sensitively**
  (Ordinal), so `/icon.png` vs `/Icon.png` would NOT trigger it — the duplicate
  must be literally `\icon.png` twice.
- Not a multi-ABI issue (fails with `-p:AndroidAbis=arm64-v8a` too).
- No icon auto-add found in `PackTaskLogic`, `PackCommandRunner`,
  `MSBuildProjectFactory` (NuGet.Client source reviewed).

## Open question

Where does the second `/icon.png` come from? Next step would be dumping
`_PackageFiles` with `AfterTargets="_GetPackageFiles"` (file
`/tmp/dumppack5.targets` already prepared) to see the final list, and/or
checking whether some Android target marks items with
`TfmSpecificPackageFile="true"` (see `_IncludeAarInNuGetPackage` in
`Microsoft.Android.Sdk.AndroidLibraries.targets`, which pulls
`@(None->WithMetadataValue('TfmSpecificPackageFile','true'))` into the package).

## Workaround until fixed

**Yes — remove the lowercase icon from the Android project only** (it was copied
there by us; the project has its own `Icon.png` for the APK):

```bash
rm /home/damir/SHARED/Devel/iPlusGit/V5/iPlus/gip.iplus.client.avui.Android/icon.png
```

Then `iPlus.gip.iplus.client.avui.Android` packs without an icon (same as
preview.1). All other packages keep their icon. Alternatively skip packing the
Android client — it is in `IPlusPackExclude` anyway and was only packed
manually.

## Related fixes already done (keep!)

- `Directory.Build.targets`: `IncludeSymbols=false` for `gip.ext.design*`,
  `gip.ext.widgets*`, `gip.ext.xamldom*` (they produce no PDBs; empty .snupkg
  files get rejected by nuget.org with 400 and abort `dotnet nuget push`).
- `build/pack-local.sh` (iPlus + iPlusMES): deletes empty `.snupkg` files after
  packing (safety net).
- `iOS` client cannot be packed on Linux (NETSDK1178) — pack on Windows/macOS.
- Remember: already-published versions are immutable; icon only appears with
  the next version bump.
