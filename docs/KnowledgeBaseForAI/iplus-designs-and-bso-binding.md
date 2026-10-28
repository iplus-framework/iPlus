# iPlus Designs (XAML) and BSO Binding — Developer Guide

How the **UI of a business object (BSO)** is defined in iPlus, how a design is bound to the
BSO's properties and methods, where designs are stored, and the pitfalls that cost the most time.
Written for developers and AI agents who need to add or change a screen for **WPF** and
**Avalonia** without searching the whole code base.

Reference implementation (all examples come from it):
[BSOMedia.cs](../../gip.core.media/BSO/BSOMedia.cs) and the design script
`scripts/sql-scripts/BSOMedia_RedesignAvalonia.sql` (separate git repo `V5/scripts`).

Related: [iplus-client-server-components.md](./iplus-client-server-components.md)
(services, proxies, `IsEnabled` mechanism in section 11).

---

## 0. Cheat sheet

```text
1. A design is NOT a .xaml file. It is a row in table ACClassDesign (per ACClass, i.e. per BSO).
     XMLDesign  = WPF XAML        (NOT NULL)
     XMLDesign2 = Avalonia XAML   (nullable), XMLDesign2UpdateDate
2. A BSO's designs are looked up by name. "*Name" in VBContent of a VBDesign means
   "embed the design 'Name' of the BSO that is the current DataContext".
3. Bindings are NOT {Binding}. Controls have VBContent="..." (an ACUrl relative to the BSO):
     VBContent="PropertyName"              property of the BSO
     VBContent="Selected\Child"            property "Child" of the object in property "Selected"
     VBContent="!MethodName"               VBButton -> ACMethod of the BSO
     VBContent="*DesignName"               VBDesign -> embedded design
4. A VBDataGrid finds its list ONLY via the ACGroup of the [ACPropertySelected]/[ACPropertyList]
   pair, or via VBSource="ListPropertyName". No group + no VBSource = empty grid, columns without header.
5. Avalonia (and WPF) only re-read a List<T> when a NEW list instance is assigned and
   OnPropertyChanged is raised. Never Add()/Remove() on the existing list and expect a refresh.
6. Whenever a property changes that other bindings depend on, raise OnPropertyChanged for ALL of
   them (selection, list, generic aliases). The UI does not know your dependencies.
7. Plain {Binding X.Y} (e.g. for an Image) is allowed, but it only refreshes if X raises
   PropertyChanged. Prefer VBContent for everything that has an ACPropertyInfo.
8. WPF and Avalonia designs are separate rows/columns with different xmlns and slightly different
   controls/converters (section 6). Keep both in the same SQL script.
9. Changing designs = SQL script (re-runnable) + restart of the client (designs are cached).
```

---

## 1. Where designs live

| Item | Value |
|---|---|
| Table | `ACClassDesign` (FK `ACClassID` -> `ACClass`) |
| Name | `ACIdentifier` (unique per class), referenced as `*Name` |
| WPF XAML | `XMLDesign` (text, NOT NULL) |
| Avalonia XAML | `XMLDesign2` (text, nullable) + `XMLDesign2UpdateDate` |
| Kind / usage | `ACKindIndex = 12010`, `ACUsageIndex = 4210` for normal BSO designs (copy from an existing row) |
| Other | `ValueTypeACClassID` (copy from an existing row), `SortIndex`, `IsDefault`, `IsRightmanagement`, `BranchNo`, `InsertName/Date`, `UpdateName/Date` |

Special design names: `Mainlayout` (main design of a BSO), `Messages`, `MediaDialog`-style names
used by `ShowDialogAsync(this, "DesignName")` (see BSO code: `DesignNameProgressBar` etc.).

Property metadata of a BSO also lives in the DB: `ACClassProperty` (ACIdentifier, `ACGroup`,
`ACPropUsageIndex`, `ValueTypeACClassID`, `GenericType`). It appears to be synchronised from the
attributes in code (UpdateDate of rows changes after code changes), so after adding or changing
property attributes **restart the client and check the DB row** before assuming a design can bind it. Property usage index: 10 Current, 20 Selected, 30 List,
40 Info (plain `[ACPropertyInfo]`).

### Inspecting existing designs (read-only)

```sql
-- list designs of a BSO
SELECT d.ACIdentifier, d.XMLDesign2UpdateDate, datalength(d.XMLDesign2) AS len2
FROM ACClassDesign d JOIN ACClass c ON c.ACClassID = d.ACClassID
WHERE c.ACIdentifier = 'BSOMedia' ORDER BY d.ACIdentifier;

-- read one (text columns: use datalength, not len; cast to nvarchar(max))
SELECT CAST(XMLDesign2 AS nvarchar(max)) FROM ACClassDesign
WHERE ACClassID = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOMedia')
  AND ACIdentifier = 'MediaForm';

-- who hosts a design? (find "*Name" or "VBContent=\"*Name\"" in other BSO designs)
SELECT c.ACIdentifier, d.ACIdentifier FROM ACClassDesign d JOIN ACClass c ON c.ACClassID = d.ACClassID
WHERE CAST(d.XMLDesign2 AS nvarchar(max)) LIKE '%*Media%';
```

---

## 2. Writing / changing a design: the SQL script pattern

Rules that make a script **re-runnable** and safe:

1. Put a header comment: which designs, what is replaced, WPF vs Avalonia.
2. Create missing rows with `IF NOT EXISTS ... INSERT ... SELECT` **copying** the not-null/foreign
   columns (`ValueTypeACClassID`, `ACKindIndex`, `ACUsageIndex`, ...) from an existing sibling row.
   `XMLDesign` is NOT NULL, so a new row needs a WPF XAML at insert time (copy one, replace later).
3. Replace content with `UPDATE ... SET XMLDesign2 = @xaml, XMLDesign2UpdateDate = GETDATE() ...`
   (Avalonia) and `UPDATE ... SET XMLDesign = @xaml ...` (WPF).
4. Each block is its own batch (`GO`) and re-declares `@ClassID` and `@xaml`.
5. XAML is inside an `N'...'` literal: **no single quotes inside the XAML** (use `"`), and no `GO`.
6. Validate the XAML before handing it over (e.g. parse the extracted literals with an XML parser).
7. Never run write statements against a shared DB yourself unless asked: deliver the script,
   tell the user to run it **and restart the client**.

Skeleton:

```sql
DECLARE @ClassID uniqueidentifier = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOMedia');

IF NOT EXISTS (SELECT 1 FROM ACClassDesign WHERE ACClassID = @ClassID AND ACIdentifier = 'MediaList')
    INSERT INTO ACClassDesign (ACClassDesignID, ACClassID, ACIdentifier, ACCaptionTranslation, XMLDesign, ValueTypeACClassID,
                               ACKindIndex, ACUsageIndex, SortIndex, IsRightmanagement, IsDefault, IsResourceStyle,
                               VisualHeight, VisualWidth, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate)
    SELECT NEWID(), ACClassID, 'MediaList', 'de{''Medienliste''}', XMLDesign, ValueTypeACClassID,
           ACKindIndex, ACUsageIndex, SortIndex, IsRightmanagement, IsDefault, IsResourceStyle,
           VisualHeight, VisualWidth, BranchNo, 'GIP', GETDATE(), 'GIP', GETDATE()
    FROM ACClassDesign WHERE ACClassID = @ClassID AND ACIdentifier = 'ImageList';

DECLARE @xaml nvarchar(max) = N'<?xml version="1.0" encoding="utf-8"?>
<vb:VBGrid ...>...</vb:VBGrid>';

UPDATE ACClassDesign
SET XMLDesign2 = @xaml, XMLDesign2UpdateDate = GETDATE(), UpdateName = 'GIP', UpdateDate = GETDATE()
WHERE ACClassID = @ClassID AND ACIdentifier = 'MediaList';
GO
```

---

## 3. XAML skeleton and namespaces

Avalonia (`XMLDesign2`):

```xml
<vb:VBGrid xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:vbs="http://www.iplus-framework.com/shapes/axaml"
           xmlns:vb="http://www.iplus-framework.com/axaml"
           xmlns:vbm="http://www.iplus-framework.com/manager/xaml"
           xmlns:vbr="http://www.iplus-framework.com/report/axaml"
           xmlns:vbv="http://www.iplus-framework.com/visual/axaml"
           xmlns:media="clr-namespace:gip.core.media;assembly=gip.core.media">   <!-- own enums/types -->
```

WPF (`XMLDesign`): `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"`, `vb` =
`http://www.iplus-framework.com/xaml`, `vbs` = `.../shapes/xaml`, `vbr` = `.../report/xaml`,
`vbv` = `.../visual/xaml`.

Use `vb:VBGrid` as root for a form (a `Grid` with iPlus binding support). `Grid.RowDefinitions`
etc. are used without prefix.

---

## 4. Controls and VBContent

| Control | VBContent | Notes |
|---|---|---|
| `vb:VBTextBox` | `Prop` or `Selected\Prop` | `ShowCaption="true"` shows the caption from `[ACPropertyInfo]`; `DisabledModes="Disabled"` = read-only look |
| `vb:VBCheckBox` | bool property | row height 30, not `Auto` (checkbox grows too high) |
| `vb:VBButton` | `!MethodName` | calls the BSO `[ACMethodInfo]`; enabled state comes from `IsEnabled<Method>()` (see client-server guide, section 11) |
| `vb:VBDataGrid` | selected property | needs list (section 5) |
| `vb:VBDataGridTextColumn` / `...CheckBoxColumn` / `...DateTimeColumn` | property of the **row** type | header from the row type's ACCaption |
| `vb:VBDataGridTemplateColumn` | - | `Header` is plain text, content is a normal `DataTemplate` (use `{Binding Prop}` of the row object) |
| `vb:VBDesign` | `*DesignName` | embeds another design of the current BSO; same `DataContext` |
| `vb:VBWebBrowser` | path/URL property | needs a **local path or URL**; byte[] is not supported |
| `vb:VBTabControl` / `vb:VBTabItem` | `VBContent="Name"` on the item | fires `TabItemActivated` (section 7) |
| `vb:VBDockingManager` | - | WPF/Avalonia docking container with `VBDesign` children (Container/DockState/DockPosition attached props) |

Combo boxes, trees, etc. follow the same Selected/List rule as the DataGrid.

### Properties that bindings can find

```csharp
[ACPropertySelected(9999, "Image", "en{'Selected'}de{'Ausgewählt'}")]   // ACGroup = "Image"
public MediaItemPresentation SelectedImage { get; set; }

[ACPropertyList(9999, "Image")]                                          // same ACGroup
public List<MediaItemPresentation> ImageList { get; set; }

[ACPropertyInfo(81, "SelectedMediaItemPresentation", "en{'...'}de{'...'}")]  // plain info property
public MediaItemPresentation SelectedMediaItemPresentation { get; set; }
```

A path like `SelectedImage\EditFilePath` is resolved on the **runtime type** of the property
(its `ACClass` / `ValueTypeACClass`). Properties of the row type (here `MediaItemPresentation`)
must carry `[ACPropertyInfo]` to be bindable and to have a caption.

---

## 5. DataGrid: how it finds its list (most common source of an empty grid)

`VBDataGrid.InitVBControl` does, in order:

1. Resolve `VBContent` (selected item property). Failure -> debug log `Error00003`.
2. Resolve the **ItemsSource URL**: `VBSource` if set, else
   `ACClassProperty.GetACSource` = the property with `ACPropUsage == List` and the **same
   `ACGroup`** as the selected property.
3. Resolve that URL (failure -> log `Error00004`) and bind `ItemsSource` + `SelectedItem`
   (TwoWay if the selected property is an input).
4. Resolve every column's `VBContent` against the **row type** of the ItemsSource.

If step 2 yields nothing, the grid stays empty **and the VBContent columns show no header** (the
type is unknown). Symptom seen: only the template column ("Preview") has a header.

Fix options:

```xml
<!-- A: generic selected/list pair without common ACGroup -->
<vb:VBDataGrid VBContent="SelectedMediaItemPresentation" VBSource="MediaItemPresentationList" ...>

<!-- B: declare the pair with [ACPropertySelected]/[ACPropertyList] and the same group name -->
```

### Refresh rules (Avalonia)

* Assigning a **new `List<T>` instance** + `OnPropertyChanged(name)` refreshes the grid.
  Raising PropertyChanged for the same instance does nothing visible for non-observable lists.
  (`VBDataGrid` has a workaround for a changed notification on plain lists, but do not rely on it.)
* Pattern for Add/Delete:

```csharp
MediaItemPresentationList = new List<MediaItemPresentation>(MediaItemPresentationList) { item };   // Add
MediaItemPresentationList = MediaItemPresentationList.Where(c => c != item).ToList();              // Delete
```

* `ObservableCollection<T>` / `ACAccess` lists refresh by themselves; use them for large or
  frequently modified lists.
* The DataGrid may transiently set `SelectedItem = null` while it re-attaches; the framework
  ignores null in ConvertBack for the selected binding. Do not clear BSO state in the setter when
  you see a transient null; check that the item still belongs to the list.

---

## 6. WPF versus Avalonia differences

| Topic | WPF (`XMLDesign`) | Avalonia (`XMLDesign2`) |
|---|---|---|
| Visibility | `Visibility="{...}"` with `Visibility` converter | `IsVisible="{...}"` with bool/visible converter |
| Visibility by enum | `ObjectEqualsVisbilityConverter` has **no `.Current`**: declare a resource and use `{StaticResource}` | `Converter={x:Static vb:ObjectEqualsVisbilityConverter.Current}` |
| Binding extension | `{vb:VBBinding Converter=..., VBContent=Prop, ConverterParameter=...}` | same syntax |
| DataGrid columns | `<DataGrid.Columns>` / `<DataGridTemplateColumn.CellTemplate>` | `<vb:VBDataGrid.Columns>` / `<vb:VBDataGridTemplateColumn.CellTemplate>` |
| byte[] as image | `Source="{Binding Image}"` works (WPF `ImageSourceConverter` handles byte[]) | needs `Converter={x:Static vb:ConverterImagePath.Current}` (byte[] or path) |
| Splitter | `GridSplitter` | `GridSplitter` (needs `Width="4"`) |

WPF visibility by enum, example:

```xml
<vb:VBGrid.Resources>
  <vb:ObjectEqualsVisbilityConverter x:Key="ObjectEqualsVisibility" />
</vb:VBGrid.Resources>
...
<Image Visibility="{vb:VBBinding Converter={StaticResource ObjectEqualsVisibility}, VBContent=ActiveTab,
       ConverterParameter={x:Static media:MediaItemTypeEnum.Image}}" />
```

Avalonia:

```xml
<Image IsVisible="{vb:VBBinding Converter={x:Static vb:ObjectEqualsVisbilityConverter.Current},
       VBContent=ActiveTab, ConverterParameter={x:Static media:MediaItemTypeEnum.Image}}" />
```

An enum used as ConverterParameter must be reachable via `xmlns:...clr-namespace` and, if it is
sent over the wire, be `[ACSerializeableInfo]` (see client-server guide, section 6).

---

## 7. Hosting a child BSO inside a parent design

Parent design (excerpt of `BSOMaintOrder` -> `MaintOrderImages`):

```xml
<vb:VBDesign VBContent="*Images">
  <vb:VBInstanceInfo Key="BSOMedia_Child(1)" SetAsDataContext="True" SetAsBSOACComponet="True" AutoStart="True" />
</vb:VBDesign>
```

* `VBInstanceInfo` creates/starts the child BSO and makes it the DataContext of the embedded design.
* The embedded design name (`*Images`, `*Media`) is a design **of the child BSO**.
  Several parents can host the same child design; **changing that design changes all hosts**.
  Before editing, find every host (query in section 1) and check which design name each one uses.
* Parent code pushes data into the child, e.g. `BSOMaintOrder.ACAction` handles
  `TabItemActivated` and calls `BSOMedia_Child.Value.LoadMedia(SelectedMaintOrderTask)`.
  When the context changes, the child must reset its own selection and raise change notifications
  (otherwise the old details stay visible).

### Tab events

`VBTabItem` (and `VBDockingManager.FireTabItemAction`) call `ContextACObject.ACAction` with
`ElementAction == TabItemActivated`; `actionArgs.DropObject.VBContent` is the tab's VBContent.
Exceptions in the handler are swallowed. A BSO can use that to keep an "active tab" property,
but a design **without** tabs never fires it: do not derive state from tab events if the design
may be shown without them. Derive state from data (e.g. media type from the selected item).

---

## 8. Change notification checklist for a BSO

```csharp
// 1) list changed
MediaItemPresentationList = newList;                 // setter raises OnPropertyChanged for list + aliases
// 2) selection changed: raise for every property the UI binds to
OnPropertyChanged(nameof(SelectedImage));
OnPropertyChanged(nameof(SelectedMediaItemPresentation));
// 3) an item's own properties changed: the item must implement INotifyPropertyChanged
//    (plain {Binding Sel.Image} only refreshes if Sel raised PropertyChanged for "Image")
// 4) buttons: IsEnabled<Method>() is re-evaluated when one of GetPropsToObserveForIsEnabled(method) changes,
//    so list selection AND anything the method depends on must be in that array.
```

Typical bugs and their cause:

| Symptom | Cause |
|---|---|
| Grid shows nothing, no column headers | no list resolved (section 5) |
| Grid keeps old rows after Add/Delete | same List instance re-notified |
| Details/preview keep old object after navigating to another parent object | selection not reset when the new list is empty |
| Selecting a row does not update the details | `Selected...` setter does not raise the generic `SelectedXxx` property |
| Button stays disabled/enabled | property missing in `GetPropsToObserveForIsEnabled` |
| Image control empty in Avalonia | byte[] bound without `ConverterImagePath`, or the path is not reachable from the client |
| WCF "TryResolveType" failure on an enum/DTO | type missing `[ACSerializeableInfo]` (+ `[ACClassInfo(..., TACEnum)]` for enums) |

---

## 9. Design with several data types in one list (pattern)

Instead of one design per type (ImageList, DocumentList, ...), use:

* **one** list property `MediaItemPresentationList` holding all item types,
* **one** selected property `SelectedMediaItemPresentation`,
* a discriminator on the item (`MediaType`) and a BSO property (`ActiveTab`) that always mirrors the
  type of the selected item, used only for visibility switches,
* the old per-type properties kept as filtered views (`ImageList => items where MediaType == Image`)
  and synchronised selections, so old WPF designs keep working,
* the type derived from the data (file extension), not from UI tabs.

Layout: `Media` = `*MediaList` | `GridSplitter` | `*MediaForm`, optionally wrapped in a docking manager for WPF.

---

## 10. Cross-platform restrictions to remember when designing

* Avalonia Browser/WASM, Android, iOS: no access to the server's file system and no free file
  system access on the client. Content must come over the service as `byte[]`; temp files are a
  desktop-only fallback (documents, "open", "show in folder").
* `VBWebBrowser` needs a file path or URL: on non-desktop platforms the document preview needs
  another approach.
* Service calls from a BSO must use the async variants on Browser/WASM (see client-server guide, 5.2).

---

## 11. Workflow when asked to "add/change a screen"

1. Find the BSO (`ACClass`) and query its designs and **hosting** designs (section 1).
2. Decide: new design row or change an existing one (who else hosts it?).
3. Make sure the BSO exposes what the design binds to: `[ACPropertyInfo]` on the row type,
   Selected/List pair with a common group (or use `VBSource`), `[ACMethodInfo]` + `IsEnabled...`.
4. Write the SQL script for **both** WPF (`XMLDesign`) and Avalonia (`XMLDesign2`).
5. Build the BSO project (`dotnet build <proj> --no-restore`, one project at a time).
6. Tell the user: run the script, **restart the client**, what to test (including navigation to
   another object, an empty list, and selection change).
7. If a grid is empty, check in this order: list found (section 5) -> properties raise
   notifications (section 8) -> DataContext of the embedded design (section 7) -> debug log
   `Error00003` / `Error00004`.

---

## 12. Open points (not verified)

* WPF `MediaList` (generic `SelectedMediaItemPresentation` + `VBSource`) showed no rows in a first test:
  the WPF `VBDataGrid` path (ItemsSource resolution, refresh for plain `List<T>`) was not yet
  analysed. Compare with the Avalonia implementation in `VBDataGrid.InitVBControl` before relying on
  section 5 for WPF.
* Document preview (`VBWebBrowser`) for Browser/Android is not solved.
