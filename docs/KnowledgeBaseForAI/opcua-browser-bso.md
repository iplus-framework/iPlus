# OPC UA Browser BSO — Concept & Implementation Guide

Status: **in progress** — this document is the continuation guide if an agent session is lost.
Target workspace: `V5/iPlus` (framework) — used by iPlus and iPlusMES clients (WPF **and** Avalonia).

---

## 1. Goal / UX

The user configures OPC UA variables for an `OPCUAClientACSubscr` instance (e.g. `TagsPA481`)
in **BSOiPlusStudio**. Today he must press *New Configuration* (`NewConfigACClassProperty`) and
type the UA NodeId manually into `OPCItemConfig.OPCAddr`.

New workflow:

1. User drags an `OPCUAClientACSubscr` (e.g. `TagsPA481`) into the project tree and selects it.
2. User presses the new button **"OPC UA Browser"** in the *TabConfigList* design of `BSOiPlusStudio`.
3. A new global BSO **`BSOOPCUABrowser`** opens, connects to the UA server (endpoint taken from the
   session component that owns the subscription) and shows the address space as a **checkbox tree**
   (checking a node auto-checks all children — done by `VBTreeView` itself).
4. The tree is **lazily loaded** on node expand (`VBTreeViewExpandMethod="!OnNodeExpand"`).
5. User selects variables and presses **"Add to selected Subscription"** → for every checked
   variable an `ACClassProperty` row is created **exactly like `NewConfigACClassProperty()` does**
   (ConfigACClass = `OPCItemConfig`, parent = the `OPCProperties` ConfigPointProperty,
   `XMLConfig` contains the serialized `OPCItemConfig` with `OPCAddr`), plus
   `ValueTypeACClass` is set from the UA data type.

## 2. Key framework facts (verified)

| Fact | Detail |
|---|---|
| Config point property | `OPCClientACSubscr.OPCProperties` has `[ACPropertyPointProperty(9999,"",typeof(OPCItemConfig))]` → `ACPropUsage = ConfigPointProperty (68)`, `ConfigACClass = OPCItemConfig`. Row lives on abstract base `OPCClientACSubscr` (ACKind 10010). |
| Variable rows | `ACClassProperty` with `ACKind = PSPropertyExt (10020)`, `ACPropUsage = Property (40)`, `ParentACClassPropertyID → OPCProperties`, `ConfigACClassID → OPCItemConfig`, `ValueTypeACClassID` = .NET type of the UA variable (e.g. `Boolean`), `XMLConfig` = `ArrayOfACPropertyExt` with one `ACPropertyExt` (`ACIdentifier=OPCItemConfig`, `TypeString=gip.core.communication.OPCItemConfig`, `XMLValue` = serialized `<OPCItemConfig><OPCAddr>ns=2;s=...</OPCAddr></OPCItemConfig>`). |
| Creating a row | Copy of `BSOiPlusStudio_4_Config.NewConfigACClassProperty()` (lines 243–270): `ACClassProperty.NewACObject(db, ownerClass)` → set `ConfigACClass` + `ACClassProperty1_ParentACClassProperty` → invoke static `InitProperty` on config class → `db.ACClassProperty.Add(...)` → **touch `ConfigValue`** (materializes the `ACPropertyExt`). |
| Writing OPCAddr | `acClassProperty["OPCItemConfig"] = new OPCItemConfig { OPCAddr = nodeId };` — the indexer setter → `ACPropertyManager.SetACPropertyExtValue` → **calls `Serialize()`** which writes the `XMLConfig` column immediately (verified ACPropertyManager.cs ~line 371). |
| Class registration | New `[ACClassInfo(..., TACBSOGlobal, ...)]` classes are **auto-registered** at client start by `ACClassManager.RegisterAndUpdateACObjects` (assembly SHA1 hash change triggers re-reflection). `GetManagerACClass` sets `ParentACClass = Businessobjects` automatically for TACBSO/TACBSOGlobal (ACClassManager.cs ~986, ~1394). **No manual SQL for the class row.** |
| BSO start by name | `Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + "BSOOPCUABrowser", acMethod.ParameterValueList)` — pattern of `BSOiPlusStudio.ModifyACClass()` (BSOiPlusStudio.cs ~2477). Constructor params declared via `[ACClassConstructorInfo]`, read in `ACInit` via `ParameterValue("...")`. |
| Checkbox tree | Model items implement `IVBDataCheckbox` (`bool IsChecked`, `string DataContentCheckBox`). `VBTreeView` renders the checkbox when the root's `CheckHandler.IsCheckboxVisible = true` (set via `SetPresentationOptions(showCaptionInTree, visibilityFilter, checkHandler)`). **Cascade to children is done by the control** (`checkChilds` on click, unless Ctrl held). Collect checked items via `root.CallOnAllItems(item => { if (item.IsChecked) ... })`. Reference: `BSOiPlusExport` + design `ExportTree`. |
| Lazy expand | `VBTreeView.VBTreeViewExpandMethod="!OnNodeExpand"`: on first expand the control calls `BSO.ACUrlCommand("!OnNodeExpand", itemModel)` then re-reads the `VBChilds` property (`VisibleItemsT`) via `FillChildsOnItemExpand`. Reference: `BSOComponentSelector.OnTreeViewItemExpand` (gip.bso.iplus/Solution/BSOComponentSelector.cs ~455). Trick: give expandable nodes a dummy child so the expander arrow shows; on expand remove dummy, browse, add real children. |
| Method dispatch | Every `[ACMethodInfo]` method needs a `case nameof(X):` in `HandleExecuteACMethod` **and** a `case nameof(X): case nameof(IsEnabledX):` in `GetPropsToObserveForIsEnabled` of its BSO (see BSOiPlusStudio.cs ~4017 / ~4304). |
| UA browse API | `Session.BrowseAsync(null, null, maxRefs, browseDescriptionCollection, ct)` with `BrowseDescription { NodeId, BrowseDirection.Forward, ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences, IncludeSubtypes = true, NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable), ResultMask = (uint)BrowseResultMask.All }`. Variable metadata: `VariableNode node = (VariableNode)await session.ReadNodeAsync(id, ct)` → `node.DataType`, `node.ValueRank`. Map to .NET: `TypeInfo.GetBuiltInType(node.DataType)` + `TypeInfo.GetSystemType(builtIn, node.ValueRank)`. `NodeId.Parse("ns=2;s=...")`. |
| UA packages | `OPCFoundation.NetStandard.Opc.Ua.*` 1.5.378.145, referenced **only** by `gip.core.communication` (Directory.Packages.props 196–200). |
| Assembly refs | `gip.core.communication` references `gip.bso.iplus` → **`gip.bso.iplus` must NOT reference `gip.core.communication`** (circular). Therefore the browser BSO lives **in `gip.core.communication`** and `BSOiPlusStudio` starts it **by name (string)** — no compile-time ref needed. |

## 3. Architecture

```
gip.core.communication/
  BSO/
    BSOOPCUABrowser.cs        <- new: [ACClassInfo(TACBSOGlobal)] BSO + OPCUANodeItem tree model
gip.bso.iplus/
  BSOiPlus/
    BSOiPlusStudio_5_OPCUA.cs <- new: partial with StartOPCUABrowser()/IsEnabledStartOPCUABrowser()
    BSOiPlusStudio.cs         <- edit: 2 switch cases (HandleExecuteACMethod + GetPropsToObserveForIsEnabled)
docs/sql-scripts/BSOOPCUABrowser_Designs.sql  <- SQL: ACClassDesign rows (Mainlayout/Browser/Messages)
                                                  for BSOOPCUABrowser + button in BSOiPlusStudio
                                                  TabConfigList (XMLDesign + XMLDesign2)
```

### 3.1 `BSOOPCUABrowser` (in `gip.core.communication`, namespace `gip.core.communication`)

- `[ACClassInfo(Const.PackName_VarioSystem, "en{'OPC UA Browser'}de{'OPC UA Browser'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]`
- `[ACClassConstructorInfo]` params: `AutoLoad` (ACUrl of the subscription `ACClass`, optional).
- Properties (`[ACPropertyCurrent]`/`[ACPropertyList]`):
  - `CurrentSubscriptionClass` (`ACClass`) — target class; resolved in `ACInit` from `AutoLoad`.
  - `SessionList` (`IEnumerable<OPCUAClientACSession>`) — `Root.FindChildComponents<OPCUAClientACSession>()`.
  - `CurrentSession` (`OPCUAClientACSession`) — auto-selected: session owning a child subscription
    instance whose `ACType == CurrentSubscriptionClass`; else first in list.
  - `BrowseTreeRoot` (`OPCUANodeItem`), `CurrentBrowseItem` (`OPCUANodeItem`).
  - `CurrentMsg`/`MsgList` for user feedback (pattern: BSOiPlusExport).
- Methods: `Browse()` (build root: Objects folder), `OnNodeExpand(OPCUANodeItem)` (lazy browse),
  `AddToSubscription()`, `RefreshSessionList()`, each with `IsEnabledX()` and switch cases.
- Session usage:
  1. If `CurrentSession.UASession != null && !Disposed && Connected` → browse on it.
  2. Else create a **temporary session**: `CoreClientUtils.SelectEndpointAsync(appConfig, endpointUrl, useSecurity, 15000, null, default)` → `OPCUAClientACSession.CreateSession(appConfig, endpoint, false, false)` → `OpenAsync(..., new UserIdentity(new AnonymousIdentityToken()), ...)` → browse → `CloseAsync()`/`Dispose()`. `appConfig` comes from `CurrentSession.AppInstance.ApplicationConfiguration` (parent `OPCUAClientACService`); if `AppInstance == null` → error message "start the OPC UA client service first".
- `OPCUANodeItem : ACClassInfoWithItems` — adds `string UANodeId`, `NodeClass UANodeClass`,
  `string DataTypeName`, `bool IsVariable`, `bool IsExpandedOnce`. Root gets
  `SetPresentationOptions(true, new VisibilityFilters(), new CheckHandler { IsCheckboxVisible = true })`.
  Children exposed through inherited `ItemsT`/`VisibleItemsT`.

### 3.2 Adding the properties (`AddToSubscription`)

```csharp
ACClassProperty pointProperty = CurrentSubscriptionClass.Properties
    .Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty
             && c.ConfigACClass != null
             && c.ConfigACClass.ACIdentifier == nameof(OPCItemConfig))
    .FirstOrDefault() as ACClassProperty;   // verify .Properties includes inherited rows!

foreach (var node in checkedVariableNodes)
{
    string identifier = Sanitize(node.BrowseName);   // letters/digits/_, unique per class
    if (CurrentSubscriptionClass.ACClassProperty_ACClass.Any(c => c.ACIdentifier == identifier)) { skip; }
    ACClassProperty p = ACClassProperty.NewACObject(Database.ContextIPlus, CurrentSubscriptionClass);
    p.ConfigACClass = pointProperty.ConfigACClass;
    p.ACClassProperty1_ParentACClassProperty = pointProperty;
    if (node.SystemType != null)
        p.ValueTypeACClass = Database.ContextIPlus.GetACType(node.SystemType) as ACClass;
    // invoke static InitProperty like NewConfigACClassProperty
    Database.ContextIPlus.ACClassProperty.Add(p);
    p[nameof(OPCItemConfig)] = new OPCItemConfig { OPCAddr = node.UANodeId }; // writes XMLConfig
}
Database.ContextIPlus.SaveChanges(); // check how studio persists (maybe deferred)
OnPropertyChanged(...); PostExecute(...);
```

### 3.3 Designs (SQL script, re-runnable)

One design `Mainlayout` for `BSOOPCUABrowser` (WPF `XMLDesign` + Avalonia `XMLDesign2`):
`VBComboBox` (session), `VBButton !Browse`, `VBTreeView` (`VBContent="CurrentBrowseItem"`,
`VBSource="BrowseTreeRoot"`, `VBChilds="VisibleItemsT"`, `VBShowColumns="ItemText"`,
`VBTreeViewExpandMethod="!OnNodeExpand"`), `VBButton !AddToSubscription`, caption of target
subscription. Copy `ACKindIndex=12010`, `ACUsageIndex=4210`, `ValueTypeACClassID` from an existing
row (e.g. `BSOiPlusExport.ExportTree`). Plus `UPDATE` of `BSOiPlusStudio.TabConfigList`
(both XMLDesign and XMLDesign2) adding `<vb:VBButton VBContent="!StartOPCUABrowser" .../>`.

**Never run write-SQL against the shared DB without being asked — deliver script, user runs it and restarts the client.**

## 4. Status / TODO checklist

- [x] Research (data model, patterns, UA API) — see §2.
- [x] `BSOOPCUABrowser.cs` implemented + compiles (0 warnings/errors).
- [x] `BSOiPlusStudio_5_OPCUA.cs` + 2 switch cases in `BSOiPlusStudio.cs` (compiles).
- [x] SQL script written: `docs/sql-scripts/BSOOPCUABrowser_Designs.sql` (not executed; all REPLACE
      anchors verified byte-exact against the live `TabConfigList` XMLDesign/XMLDesign2 — both
      WPF and Avalonia use identical 8/4-space indentation).
- [x] Build `gip.core.communication` + `gip.bso.iplus` (`dotnet build` on the csproj, Debug).
- [ ] User: deploy new `gip.core.communication.dll` → start client once (auto-registers the
      `BSOOPCUABrowser` ACClass row) → run SQL script → restart client → test with `opc.tcp://...`
      server from `SessionA` config.

### Implementation notes (deviations from original plan)
- `ACClass.Properties` **does** return inherited properties (verified ACClass.cs ~1790) — the
  `OPCProperties` row from base `OPCClientACSubscr` is found.
- `Root.FindChildComponents<OPCUAClientACSession>()` works (ACComponent.cs:727, depth 0 = unlimited).
- `Database.ContextIPlus.GetACType(Type)` exists (Database.cs:1828).
- UA API used: `session.ManagedBrowseAsync(...)` (returns `(IList<ReferenceDescriptionCollection>,
  IList<ServiceResult>)`), `session.ReadNodeAsync(id, NodeClass.Variable, true, ct)`,
  static `ExpandedNodeId.ToNodeId(expandedNodeId, session.NamespaceUris)`.
- `SortIndex` is set manually via a `NextSortIndex` helper (NewACObject does not set it).
- `MISort` enum has no `Refresh` member — plain numbers used (499/502/510/511/520).
- Duplicate identifiers are skipped (OrdinalIgnoreCase against `CurrentSubscriptionClass.Properties`),
  folders resolved recursively (`CollectVariablesRecursive`, depth 32, max 10000 nodes).
- Temp session is closed in `ACDeInit` via `CloseTempSession()`.
