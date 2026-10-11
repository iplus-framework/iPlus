# Handover: RoslynPad → Morgania Migration (VBScriptEditor / iPlus)

> **Status:** Memo for a future migration. Read this fully before touching anything.
> **Written:** 2026-10-02, after fixing the `IDeferredQuickInfoContentProvider` composition failure.
> **Trigger for migration:** when the roslynpad fork ships its NuGet packages for .NET 11 (or whenever we decide to leave the frozen `d6c079` binaries).

---

## 1. Background — why we are currently pinned

The iPlus `VBScriptEditor` (`gip.core.layoutengine.avui/Controls/VBScriptEditor/VBScriptEditor.cs`)
is written against the **old** RoslynPad editor API:

- `RoslynCodeEditor` (AvalonEdit-based `CodeTextEditor`)
- `RoslynHost` with a **MEF v2** (`System.Composition.Hosting`) container
- Platform assemblies `RoslynPad.Roslyn.Avalonia` / `RoslynPad.Editor.Avalonia`

The fork was restructured (merge commit `1a34aa1`, upstream `77d4e7a`):

- `RoslynPad.Roslyn`, `RoslynPad.Editor.Avalonia`, `RoslynPad.Roslyn.Avalonia`,
  `RoslynPad.Editor.Windows`, `RoslynPad.Roslyn.Windows`, `RoslynPad.Avalonia`,
  `RoslynPad.Common.UI`, `RoslynPad.Build` were **dissolved**.
- The editor is now **Morgania** (vendored `vs-editor-api` reimplemented on Avalonia).
- Composition is now **VS-MEF** (`Microsoft.VisualStudio.Composition`) mixing MEF v1 + v2 parts,
  built by `EditorComposition.CreateConfiguration()` in
  `src/Morgania.CodeAnalysis.Editor/EditorComposition.cs`.
- The app-facing editor control is now `CodeEditorView` (`src/RoslynPad/CodeEditorView.cs`).

**Consequence:** building the fork at `main` no longer produces the DLL set that
`iPlus/build/AvaloniaRoslynPadInclude.props` references via `HintPath`. Worse, rebuilding
`RoslynPad.Roslyn.Avalonia` at `main` produces an **empty shim** without
`DeferredQuickInfoContentProvider`, which caused:

```
System.Composition.Hosting.CompositionFailedException:
  No export was found for the contract 'IDeferredQuickInfoContentProvider'
  -> required by import 'contentProvider' of part 'QuickInfoProvider'
```

## 2. Current setup (the pin)

- **Compat worktree:** `V5/roslynpad-d6c079` — a git worktree of
  `iplus-framework/roslynpad` at commit `d6c079` (old layout). It is the single source for:
  - the `HintPath` DLLs (`RoslynPad.Roslyn.dll`, `RoslynPad.Editor.Avalonia.dll`,
    `RoslynPad.Roslyn.Avalonia.dll` with `DeferredQuickInfoContentProvider`)
  - any future `iPlus.RoslynPad.*` NuGet packs (pack from here, **not** from `main`)
- **Batch build:** `iPlus/build/build-avalonia-projects.sh` builds
  `../../roslynpad-d6c079/iPlusAvalonia.slnx` (entry renamed
  "Roslyn Pad (d6c079 compat fork)") with `-p:TreatWarningsAsErrors=false`
  (MessagePack NU1902/NU1903 advisories in the old tree).
- **Do not delete `roslynpad-d6c079`** while `VBScriptEditor` targets the old API.
- **Do not rebuild** `roslynpad/src/RoslynPad.Roslyn.Avalonia` at `main` — it would
  overwrite the HintPath DLL with the empty shim and reintroduce the composition crash.
  If that happens, re-copy from `roslynpad-d6c079/src/RoslynPad.Roslyn.Avalonia/bin/Debug/net10.0/`.

### Fresh-clone requirements (colleagues)

1. Folder layout must be siblings (relative imports!):
   ```
   V5/
   ├── iPlus/       ← provides build/Avalonia.Build.props (imported by roslynpad)
   ├── Avalonia/    ← our Avalonia fork, must be BUILT first (HintPath DLLs)
   └── roslynpad/   ← fork clone (checkout d6c079 for the compat set)
   ```
2. .NET SDK ≥ 10.0.201 (pinned in `roslynpad/global.json`).
3. `git submodule update --init` in roslynpad (`vendor/roslyn`).
4. Build the Avalonia fork first — `AvaloniaForkBuildConfiguration` is probed from
   `Avalonia/src/Avalonia.Native/bin/<Debug|Release>/net10.0/Avalonia.dll`.

## 3. What Morgania is

Morgania is the fork's codename for the editor engine: the vendored
**`vs-editor-api` (Visual Studio editor API) reimplemented on Avalonia**.

| Project | Role |
|---|---|
| `Morgania.Editor.Abstractions` | Core `Microsoft.VisualStudio.Text.*` contracts (`ITextBuffer`, classification, …) |
| `Morgania.Editor` | The editor implementation (text view, taggers, completion, find/replace, folding) on Avalonia |
| `Morgania.CodeAnalysis.EditorFeatures` | Roslyn's `EditorFeatures` recompiled from `vendor/roslyn` against Morgania |
| `Morgania.CodeAnalysis.Editor` | Glue: VS-MEF composition (`EditorComposition`), squiggles, light bulb, glyphs, dialogs, key bridge |

The namespaces intentionally keep `Microsoft.VisualStudio.*` so Roslyn's editor features
compile unmodified. Per-feature design docs live in
`roslynpad/docs/architecture/editor-integration.md` — **read the relevant section before
working on a feature**.

## 4. Migration checklist (VBScriptEditor → Morgania)

### 4.1 Host construction

Old (`CustomRoslynHost` in `VBScriptEditor.cs`):

```csharp
_roslynHost = new CustomRoslynHost(_roslynAssembly, RoslynHostReferences.NamespaceDefault.With(references));
// MEF v2 container over DefaultCompositionAssemblies + additionalAssemblies
```

New: `RoslynHost` (`src/RoslynPad/Roslyn/RoslynHost.cs`) builds a single **VS-MEF** graph:

```csharp
ExportProvider = EditorComposition.CreateConfiguration(additionalAssemblies)
    .CreateExportProviderFactory().CreateExportProvider();
HostServices = MorganiaMefHostServices.Create(ExportProvider);
```

- `additionalAssemblies` still exists — pass iPlus assemblies there if they export MEF parts.
- `GetService<T>()` now goes through `ExportProvider.GetExportedValue<T>()`.
- Services like `IQuickInfoProvider` come from the recompiled EditorFeatures
  (`Morgania.CodeAnalysis.EditorFeatures`), not from `RoslynPad.Roslyn`.
- `RoslynHost` is now `IDisposable` (analyzer-config watchers) — dispose it in `DeInitVBControl`.

### 4.2 Editor control

- Replace `RoslynCodeEditor` base class with `CodeEditorView` (in `src/RoslynPad/`).
- `CodeEditorView` hosts Morgania's `ITextViewHost`; the `TextArea` is gone —
  anything using `TextArea.TextView`, `TextArea.Caret`, `FoldingManager.Install(TextArea)`
  must be re-mapped:
  - Folding: driven by Roslyn's structure tagger via
    `StructureOutliningTaggerProvider`; `Ctrl/Cmd+M` chord lives in `CommandingKeyBridge`.
    The custom `VBScriptEditorBraceFoldingStrategy` + `DispatcherTimer` folding loop can
    likely be **deleted** — Roslyn block structure covers it.
  - Find/replace: `FindReplacePanel` floats over the view; invocation is commanding
    (`InvokeFindReplace`). The `VBBSOFindAndReplace` handler needs rewiring — it currently
    wraps `TextArea`.
  - Clipboard: all paste entry points must route through `PasteFromClipboardAsync`
    (Avalonia clipboard is async-only; `IEditorOperations.Paste` is synchronous).
- `InitializeAsync(IRoslynHost, IClassificationHighlightColors, workingDirectory, text, sourceCodeKind)`
  has a similar shape on `CodeEditorView` — check its current signature at migration time.
- `DocumentView` creates the `ITextBuffer` (CSharp content type) and opens the Roslyn
  document over `buffer.AsTextContainer()`; workspace changes round-trip as minimal
  buffer edits. The `AvalonEditTextContainer` + `CreatingDocumentEventArgs` dance goes away.

### 4.3 Theming

- `VBScriptEditor` picks `ClassificationHighlightColorsGip` (dark) / `ClassificationHighlightColors`.
  In the new world the **theme is authoritative**: `ThemeClassificationFormats` maps VS Code
  themes onto classifications; unthemed classifications get explicit formats *cleared*.
  Consider replacing the GIP colors with a theme mapping instead of overriding formats.

### 4.4 Packaging / build

- `AvaloniaRoslynPadInclude.props` must switch from `HintPath` references to the new
  package/project set: `RoslynPad.Roslyn` (host layer), `Morgania.*`, `RoslynPad.Themes`,
  `RoslynPad.Runtime` — or to the shipped `iPlus.RoslynPad.*` NuGet packages.
- The batch script entry must point back to `../../roslynpad/iPlusAvalonia.slnx`
  and the `-p:TreatWarningsAsErrors=false` extra arg can be dropped.
- Delete the `roslynpad-d6c079` worktree only after nothing references it:
  `cd roslynpad && git worktree remove ../roslynpad-d6c079`.

### 4.5 Known gotchas (from the fork's CLAUDE.md — re-read it at migration time)

- **Composition rejections are normal:** EditorFeatures parts with VS-only imports are
  rejected by design; `CompositionConfiguration.CompositionErrors` is advisory. Don't call
  `ThrowOnErrors`. Rejections are logged to `composition.log` in the app directory.
- **`IDocumentNavigationService` stub** must export at `ServiceLayer.Editor` (Roslyn ships
  a Features-layer default under the same contract).
- **Roslyn internal API access** relies on `IgnoresAccessChecksToGenerator`; Roslyn
  upgrades can break the build — expect compile fixes after version bumps.
- **Settings defaulting to `true`** need `[JsonIgnore(Condition = JsonIgnoreCondition.Never)]`
  in `ApplicationSettings.SerializableValues`.
- The trailing-expression `.Dump()` rewrite round-trips through text — not relevant for
  VBScriptEditor unless we adopt `ExecutionHost`.

## 5. Verification plan after migration

1. Open a form with a `VBScriptEditor` bound to a VBContent string property.
2. Verify: syntax coloring, completion, quick info (hover), squiggles, brace matching
   (Ctrl+], Ctrl+M chord), folding, find/replace (Ctrl+F/Ctrl+H), snippets.
3. Verify dark theme (`ControlManager.WpfTheme == eWpfTheme.Dark`) renders correctly.
4. Verify `VBText` two-way binding still syncs (watch the `_OnTargetUpdated` guard logic —
   it exists to prevent text duplication during init; re-check it against the new init path).
5. Check `composition.log` in the app directory for unexpected rejections.
6. Run the fork's headless smoke test as a sanity check:
   `dotnet run --project src/Morgania.Demo.EditorFeatures -- --smoke`

## 6. Quick reference

| Thing | Old (d6c079) | New (main / Morgania) |
|---|---|---|
| Editor control | `RoslynCodeEditor : CodeTextEditor` (AvalonEdit) | `CodeEditorView` (Morgania `ITextViewHost`) |
| Composition | MEF v2 `System.Composition.Hosting` | VS-MEF `Microsoft.VisualStudio.Composition` (v1+v2 mix) |
| Quick info | `RoslynPad.Roslyn.QuickInfo` + `RoslynPad.Roslyn.Avalonia` export | Recompiled Roslyn EditorFeatures in `Morgania.CodeAnalysis.EditorFeatures` |
| Diagnostics | `IDiagnosticsUpdater` via workspace service | Same, but filtered via `TrailingExpressionDiagnosticsFilter` at `ServiceLayer.Host` |
| Folding | `FoldingManager.Install(TextArea)` + custom strategy | `StructureOutliningTaggerProvider` (Roslyn structure tags) |
| Find/replace | `VBBSOFindAndReplace` handler over `TextArea` | `FindReplacePanel` + commanding (`InvokeFindReplace`) |
| Host disposal | none | `RoslynHost : IDisposable` |
| Build output | — | `BuildOutputView` over `BuildOutput` content type (optional to adopt) |
