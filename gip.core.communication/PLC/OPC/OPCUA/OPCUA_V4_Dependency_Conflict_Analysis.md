# V4 OPC UA Upgrade Dependency Conflict Analysis

## Scope
This note explains the dependency conflicts discovered when upgrading V4 OPC UA from `1.4.363.0` to `1.5.378.145`.

The analysis focuses on:
- Why the upgrade was attempted
- Why RoslynPad compatibility regressed
- Which assemblies conflict (build-time and runtime)
- Why `net472` OPC UA binaries do not solve it

## Why the upgrade was attempted
The older OPC UA stack (`1.4.363.0`) has certificate behavior that routes into `ncrypt.dll` in environments where this causes failures under Wine.

The newer OPC UA stack (`1.5.378.145`) avoids that problematic path in V5 and is therefore desirable for V4 Wine execution as well.

## Where the conflict happens in V4
The conflict is not isolated to one library. It occurs because one project graph combines:
- OPC UA references from `gip.core.communication`
- RoslynPad and Roslyn-related references from `gip.core.layoutengine` / `gip.core.reporthandler` / `gip.bso.iplus`

Key graph points:
- `gip.core.communication.csproj` references OPC UA 1.5.378.145 packages directly.
- `gip.core.communication.csproj` also references `gip.bso.iplus` and `gip.core.reporthandler`.
- RoslynPad dependencies are present in `gip.core.layoutengine`, `gip.core.reporthandler`, and `gip.bso.iplus`.

This places old Roslyn-era helper assemblies and newer OPC UA helper assembly requirements into the same process dependency closure.

## Version mismatch matrix

| Dependency | RoslynPad side | OPC UA 1.5.378.145 side | Observed pressure |
|---|---|---|---|
| System.Memory | 4.0.1.1 / 4.0.1.2 era | 4.0.5.0 | Upgrade pressure, redirect mismatch risk |
| System.Buffers | 4.0.3.0 | 4.0.5.0 | Explicit MSB3277 conflict |
| System.Runtime.CompilerServices.Unsafe | 4.0.6.0 | 6.0.3.0 | High mismatch, possible runtime risk |
| System.Text.Json | 4.0.1.2 | 10.0.0.8 (transitive path in graph) | Explicit MSB3277 conflict |
| System.Formats.Asn1 | 5.0.0.0 | 10.0.0.8 | Explicit MSB3277 conflict |
| Newtonsoft.Json | 12.0.0.0 | 13.0.0.0 (other graph branch) | Explicit MSB3277 conflict |

## Build evidence (Wine .NET Framework 4.8 build)
Build command used for diagnostics:

```bash
WINEPREFIX="$HOME/.wine-dotnet48-64" WINEDEBUG=-all wine /home/damir/.wine-dotnet48-64/drive_c/users/damir/.vscode/extensions/ms-dotnettools.csharp-1.24.4-win32-x64/.omnisharp/1.38.2/.msbuild/Current/Bin/MSBuild.exe /p:Configuration=Debug /p:UseProjectReferences=true /nr:false /m /verbosity:normal "D:\Devel\iPlusGit\V4\iPlus\gip.core.communication\gip.core.communication.csproj"
```

Result: build succeeds, but with unresolved conflict warnings (`MSB3277`).

Conflict headlines found in:
- `V4/iPlus/build-logs/msbuild-gip.core.communication.log`

Conflicts detected:
- `Newtonsoft.Json` 12.0.0.0 vs 13.0.0.0
- `System.Text.Json` 4.0.1.2 vs 10.0.0.8
- `System.Buffers` 4.0.3.0 vs 4.0.5.0
- `System.Formats.Asn1` 5.0.0.0 vs 10.0.0.8

## Runtime policy evidence
Generated runtime config in bin output:
- `V4/iPlus/bin/Debug/gip.iplus.client.exe.config`

Observed redirects include:
- `System.Memory` -> `4.0.1.2`
- `System.Buffers` -> `4.0.3.0`
- `System.Runtime.CompilerServices.Unsafe` -> `4.0.6.0`

At the same time, OPC UA 1.5.378.145 references newer versions (notably `System.Memory 4.0.5.0`, `System.Buffers 4.0.5.0`, `Unsafe 6.0.3.0`, `System.Formats.Asn1 10.0.0.8`).

This indicates a runtime policy that is still RoslynPad-era oriented while the OPC UA path expects newer baselines.

## Why net472 OPC UA binaries are not a real fix
Inspection of `net48` and `net472` OPC UA package assemblies for 1.5.378.145 showed:
- Different binary hashes
- Same OPC UA assembly versions
- Same key dependency pressure (System.Memory/System.Buffers/Unsafe/logging family)

Conclusion: switching from OPC UA `net48` binaries to OPC UA `net472` binaries does not remove the fundamental RoslynPad dependency clash.

## Practical conclusion for V4
If immediate V4 stability is the top priority, reverting OPC UA to `1.4.363.0` is pragmatic.

Tradeoff:
- You regain compatibility with existing RoslynPad-era dependency expectations.
- You reintroduce the old certificate/ncrypt-related constraints that motivated the upgrade for Wine.

## Suggested branch strategy
To avoid blocking delivery and still keep Wine progress:
1. Keep mainline V4 on OPC UA `1.4.363.0` for stability.
2. Maintain a separate Wine-focused branch with OPC UA `1.5.378.145`.
3. In that branch, isolate or refactor RoslynPad-coupled runtime paths so OPC UA and Roslyn dependencies no longer force incompatible versions into the same process closure.

## Reference files
- `V4/iPlus/gip.core.communication/gip.core.communication.csproj`
- `V4/iPlus/gip.core.layoutengine/gip.core.layoutengine.csproj`
- `V4/iPlus/gip.core.reporthandler/gip.core.reporthandler.csproj`
- `V4/iPlus/gip.bso.iplus/gip.bso.iplus.csproj`
- `V4/iPlus/build-logs/msbuild-gip.core.communication.log`
- `V4/iPlus/bin/Debug/gip.iplus.client.exe.config`

## Dependency detail cheat sheet (for quick recall)

### RoslynPad and Microsoft.CodeAnalysis
- In `gip.core.layoutengine.csproj`, the reference is declared as `Microsoft.CodeAnalysis, Version=3.0.0.0`, but the hinted RoslynPad DLL is actually `3.8.0.0`.
- The same applies to `Microsoft.CodeAnalysis.Workspaces`: declared as `3.0.0.0`, but RoslynPad-provided binary version is `3.8.0.0`.
- RoslynPad Roslyn binaries observed in `packages/RoslynPad` are built around the Roslyn `3.8.0.0` family.

### RoslynPad-side base dependency expectations
From RoslynPad CodeAnalysis assembly refs (metadata):
- `System.Memory` -> `4.0.1.1` (runtime often redirected to `4.0.1.2`)
- `System.Runtime.CompilerServices.Unsafe` -> `4.0.6.0`
- `System.Collections.Immutable` -> `5.0.0.0`
- `System.Reflection.Metadata` -> `5.0.0.0`

### OPC UA 1.5.378.145-side base dependency expectations
From OPC UA core/security assembly refs (metadata):
- `System.Memory` -> `4.0.5.0`
- `System.Buffers` -> `4.0.5.0`
- `System.Runtime.CompilerServices.Unsafe` -> `6.0.3.0`
- `System.Formats.Asn1` -> `10.0.0.8`

This is the core version-skew with RoslynPad-era dependencies.

### Runtime output actually seen in V4 bin/Debug
Observed in `V4/iPlus/bin/Debug` after build:
- `System.Memory.dll` -> `4.0.5.0`
- `System.Buffers.dll` -> `4.0.3.0`
- `System.Runtime.CompilerServices.Unsafe.dll` -> `4.0.6.0`
- `System.Text.Json.dll` -> `4.0.1.2`
- `System.Formats.Asn1.dll` -> `5.0.0.0`

This mixed output baseline is exactly why `MSB3277` appears even when build succeeds.

### Json and Google API dependency notes
- `gip.core.communication` still references `Newtonsoft.Json` `12.0.0.0`.
- Other branches in the same closure pull `Newtonsoft.Json` `13.0.0.0`.
- `gip.bso.iplus` includes multiple Google API packages (`Google.Api.Gax`, `Google.Apis`, `Google.Apis.Auth`, etc.), which contribute to the wider transitive graph and conflict pressure.
- Build log confirms unresolved Json conflicts:
	- `Newtonsoft.Json` `12.0.0.0` vs `13.0.0.0`
	- `System.Text.Json` `4.0.1.2` vs `10.0.0.8`

### One-line memory summary
`OPC UA 1.5.x wants newer System.* (Memory/Buffers/Unsafe/Asn1), while RoslynPad in V4 anchors older Roslyn-era System.*; both end up in one process graph, so conflicts are expected unless the graph is isolated or unified.`
