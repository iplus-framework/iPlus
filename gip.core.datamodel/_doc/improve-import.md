# Import Process Improvements Plan

## 1) Current State Analysis

### Scope of the profiled workflow
The measured workflow is the GUI-triggered import inspection path:

- `gip.bso.iplus.BSOiPlusImport.DoInspectImport(...)`
- `gip.core.autocomponent.ResourcesRoot.Dir(...)`
- `gip.core.autocomponent.Resources.SetupProperties(...)`
- `gip.core.datamodel.ACFSItem.SetupProperties(...)`
- `gip.core.datamodel.ACObjectSerialHelper.SetIACObjectProperties(...)`

### CPU profile summary (current state)
Profiling of a real execution (not startup-only) showed:

- `DoInspectImport(...)` total CPU ~61.69%
- `ResourcesRoot.Dir(...)` total CPU ~57.7%
- `Resources.SetupProperties(...)` total CPU ~43.1% (hot)
- `ACEntitySerializer.DeserializeXML(...)` total CPU ~14.5% (hot)

Additional expensive operations observed in the same call graph:

- `System.Linq.Enumerable.ToArray<T>(...)` (high self CPU)
- `System.Data.Objects.DataClasses.RelationshipManager.GetRelatedReference<T>(...)`
- `ACClass.BuildInheritedPropertyList(...)`

### Cost drivers and root causes

1. **Reflection-heavy per-object property processing**
   - In `ACObjectSerialHelper.GetPropertyList(...)`, each object repeatedly executes reflection and attribute checks:
	 - `GetType().GetProperties()`
	 - `GetCustomAttributes(...)`
	 - repeated filtering/grouping
   - This cost grows linearly with imported objects and properties and causes many allocations.

2. **Deep recursive traversal for property setup**
   - `ACFSItem.SetupProperties(...)` recursively walks the whole tree.
   - Every node with `ACObject` + XML/outer source triggers `SetIACObjectProperties(...)`.

3. **LINQ/iteration overhead in progress reporting loops**
   - `DeserializeXML(...)` and `DeserializeXMLChildren(...)` use `IndexOf(...)` inside `foreach` for progress.
   - This can degrade to O(n²) loop behavior on larger lists.

4. **Very frequent progress reporting**
   - Progress updates are sent per item/node, increasing overhead on high-volume imports.

5. **Minor avoidable allocations / dead work**
   - In `SetProperty(...)` IACObject branch, a temporary `ToList()` value is computed and not used.


## 2) Improvement Goals

1. Reduce CPU time of import inspection without changing behavior.
2. Reduce allocations and repeated reflection work.
3. Preserve current import semantics and error reporting.
4. Keep changes safe for .NET Framework 4.8 / C# 7.3.


## 3) Proposed Changes (Execution Plan)

### Change A — Cache serializable property metadata by CLR type
**Target:** `gip.core.datamodel/ACSerialization/ACObjectSerialHelper.cs`

Implement a static, thread-safe cache for per-type metadata used by import property mapping.

#### What to cache
- Filtered property set currently built with `!PropertyForIgnore(pi)`.
- Property handling type (`Primitive`, `String`, `DateTime`, `IACObject`, `ACClassDesignByte`).
- Optional split for key-members vs non-key-members (if needed for current `setupKeyACIdentifier` modes).

#### Suggested shape
- `ConcurrentDictionary<Type, CachedPropertyMetadata>`
- `CachedPropertyMetadata` contains precomputed arrays/lists grouped by handling type.

#### Impact
- Removes repeated reflection and attribute checks for objects of the same type.
- Expected to significantly reduce hotspot pressure around `SetIACObjectProperties(...)`.


### Change B — Replace O(n²) progress loops with indexed iteration
**Target:** `gip.core.datamodel/ACSerialization/ACEntitySerializer.cs`

In these loops:

- `DeserializeXML(...)` list processing
- `DeserializeXMLChildren(...)` child node processing

replace `foreach + IndexOf(...)` with indexed loops (`for`) or explicit incrementing counters.

#### Impact
- Eliminates repeated lookup scans.
- Improves throughput for larger XML lists/child sets.


### Change C — Throttle progress reporting
**Targets:**
- `gip.core.datamodel/ACSerialization/ACEntitySerializer.cs`
- optionally `gip.core.autocomponent/ACIPlus/Resources/Resources.cs`

Report progress every N items (for example every 25/50/100), plus final item.

#### Notes
- Keep existing messages and final completion reports.
- Make N configurable constant for tuning.

#### Impact
- Reduces overhead from very high-frequency status updates.
- Often meaningful in background-worker-heavy import flows.


### Change D — Remove dead allocation work in SetProperty IACObject branch
**Target:** `gip.core.datamodel/ACSerialization/ACObjectSerialHelper.cs`

Remove unused temporary list creation (`...Select(...).ToList()`) in `SetProperty(...)` where `entity == null` path is executed.

#### Impact
- Small but safe micro-optimization.


## 4) Implementation Order

1. **Implement Change A (metadata cache)** — largest expected gain.
2. **Implement Change B (indexed loops)** — low risk, immediate win.
3. **Implement Change C (progress throttling)** — medium impact, low risk.
4. **Implement Change D (dead work cleanup)** — quick cleanup.
5. Build and run import inspection scenario for regression check.


## 5) Validation Plan

### Functional validation
- Run the same import source tree used in profiling.
- Confirm:
  - same imported object count
  - same validation messages
  - same resulting object relationships
  - cancellation behavior unchanged

### Performance validation
- Re-profile `DoInspectImport(...)` with the same dataset and workflow.
- Compare before/after:
  - total duration
  - CPU % in `DoInspectImport`, `ResourcesRoot.Dir`, `SetupProperties`, `DeserializeXML`
  - call counts and self CPU in reflection-heavy helpers

### Safety checks
- Full build of affected solutions/projects.
- Check for threading issues in static caches.
- Ensure no behavior change with mixed entity types and null properties.


## 6) Risks and Mitigations

1. **Cache correctness risk**
   - Risk: cached metadata mismatches dynamic runtime assumptions.
   - Mitigation: cache key strictly by CLR `Type`; keep behavior identical to current filters.

2. **Progress UX changes**
   - Risk: less granular live progress feedback.
   - Mitigation: configurable throttle; always send final progress update.

3. **Subtle import behavior regressions**
   - Risk: property ordering/handling side effects.
   - Mitigation: preserve existing ordering semantics and run scenario parity tests.


## 7) Expected Outcome

With no functional change, the import inspection flow should spend less CPU in reflection and list traversal overhead, and overall execution of `DoInspectImport(...)` should improve measurably on large import trees.
