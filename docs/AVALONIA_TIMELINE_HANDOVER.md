# Avalonia Timeline and Designer Handover

## Scope

This handover covers work in the iPlus Avalonia port related to the WYSIWYG designer, `VBTimelineView`, tooltips, timeline item rendering, date-axis grid lines, and time-range drag interaction.

Primary workspace: `/home/damir/SHARED/Devel/iPlusGit/V5/iPlus`

## Current Status

### Resolved: Designer binding failure for `MultiBinding`

The designer originally threw:

```text
Cannot find an ILogical to get a visual ancestor.
```

A `DataTriggerBehavior` can be the target of a `MultiBinding` containing a `RelativeSource FindAncestor` child binding. The target is not a `Visual`, so Avalonia needs the parser's visual anchor.

`gip.ext.xamldom.avui/src/XamlPropertyInfo.cs` was updated so `TrySetBindingDefaultAnchor` and `TrySetBindingNameScope` recursively process every child `BindingBase` inside a `MultiBinding`. Avalonia's `MultiBindingExpression.StartCore` creates its child bindings with a null anchor, therefore the needed anchor must be stored on each child `ReflectionBinding` through its internal `DefaultAnchor` property.

Result: the designer now loads, and the design is visible.

### Resolved: Designer infinite layout loop from null TreeList headers

In the WYSIWYG designer the BSO (`VBBSOPropertyLogPresenter`) is not instantiated, so the TreeList column `VBContent` values cannot be resolved to AC metadata/captions. Its `GridViewColumn.Header` values remain null.

The problematic loop was:

1. `VBTreeListView.TryInitializeColumnHeaders` scans generated headers.
2. Each header has `Content == null`, so no header is considered hooked.
3. `ScheduleHeaderRetry` marks `GridViewHeaderRowPresenter.NeedUpdateVisualTree = true`, calls `InvalidateMeasure`, then retries on `LayoutUpdated`.
4. `GridViewHeaderRowPresenter.UpdateVisualTree` recreates / refreshes headers.
5. The headers remain null and the loop repeats.

Logs identified this pattern:

```text
[HDR] UpdateVisualTree: Columns=5, children=8
[HDR] CreateAndInsertHeader: ... header=NULL
[TLV] No headers hooked after UpdateLayout ... GridViewColumnHeader:null-content
[Layout]Layout cycle detected ... AdornerLayer
```

The current `VBTreeListView` code contains design-mode behavior and bounded retry logic intended to prevent this endless loop. Verify it remains present before changing related code.

Relevant files:

- `gip.core.layoutengine.avui/Controls/VBTreeListView/VBTreeListView.cs`
- `gip.core.layoutengine.avui/Controls/VBGridView/GridViewHeaderRowPresenter.cs`

## Tooltip Visual-Parent Exception

### Original exception

```text
The control StackPanel already has a visual parent ContentPresenter
(Name = PART_ContentPresenter, Host = ToolTip)
while trying to add it as a child of ContentPresenter ...
```

Avalonia correctly prohibits a `Visual` from being added to two visual parents. The check is in Avalonia `Visual.cs` (`IAvaloniaListItemValidator<Visual>.Validate`).

### Cause

`VBTimelineChartBase` owns a single shared tooltip panel:

```csharp
internal StackPanel container = new StackPanel();
```

File:

- `gip.core.layoutengine.avui/Controls/VBTimeline/Base/VBTimelineChartBase.cs`

Every `TimelineItem` had assigned that same panel as its tooltip content:

```csharp
ToolTip.SetTip(this, VBTimelineChart.container);
```

A `ToolTip` is cached per control by Avalonia. When hovering item A, the shared panel is adopted by A's tooltip popup presenter. On hover of item B, Avalonia creates B's tooltip and its `ContentPresenter` attempts to adopt the same shared `StackPanel`, causing the exception.

### Applied fix

`TimelineItemBase` now has a `SetSharedToolTip` helper and its existing `TimelineItem_ToolTipOpening` handler releases the shared container from its existing `ContentPresenter` before a new tooltip is opened.

Files changed:

- `gip.core.layoutengine.avui/Controls/VBTimeline/Base/TimelineItemBase.cs`
- `gip.core.layoutengine.avui/Controls/VBTimeline/Timeline/TimelineItem.cs`
- `gip.core.layoutengine.avui/Controls/VBGanttChart/Timeline/TimelineGanttItem.cs`

`TimelineItem_ToolTipOpening` is registered in `TimelineItemBase.OnInitialized` with:

```csharp
ToolTip.AddToolTipOpeningHandler(this, TimelineItem_ToolTipOpening);
```

The critical timing detail: it must detach the shared panel in `ToolTipOpening`, not only in initialization. Avalonia raises this event before it creates / opens the new tooltip.

Result: the exception no longer appears, and hovering between items is possible.

### Remaining tooltip defects

1. Tooltip always displays the data of the first / previous item rather than the currently hovered item.
2. Tooltip content intermittently disappears. Once missing for an item, it can remain absent for future hovers.

Likely root cause: tooltip content is still one shared `StackPanel` and may contain a single `ContentControl` whose binding source is created only once. Moving the visual between tooltips does not reliably update the binding / `DataContext` to the newly hovered `TimelineItem`.

Relevant code:

- `TimelineItem.cs`, `TimelineItem.InitVBControl()`:
  - assigns `ToolTipContent = VBTimelineView.ToolTipItems`
  - creates a `ContentControl` bound to `this.ToolTipContent`
  - adds it to `VBTimelineChart.container`
- `TimelineItemBase.cs`, `OnLoaded()`:
  - contains overlapping legacy tooltip content setup logic and an instance field named `contentControl`.
- `VBTimelineViewBase.cs`, `InitVBControl()`:
  - moves the controls from `ItemToolTip` into one shared `ToolTipItems` `StackPanel`:

```csharp
ItemToolTip?.ForEach(c => ToolTipItems.Children.Add(c));
```

This design cannot safely provide per-item data context with a single visual tree.

### Recommended tooltip redesign

Prefer one tooltip content tree per opened / per timeline item rather than reparenting one `StackPanel`.

Possible approaches, in preferred order:

1. Make `ItemToolTip` a `IDataTemplate` / `DataTemplate` and build it for each `TimelineItem`. Set each item's tooltip to the new control tree and use the current item (or its model) as `DataContext`.
2. Retain a shared `ToolTip` shell but rebuild its child tree every `ToolTipOpening`, explicitly set the child `DataContext` to the hovered item / `ContextACObject`, and ensure the old child is removed before adding the new one.
3. At minimum, update the existing shared `ContentControl.DataContext` / binding source during `TimelineItem_ToolTipOpening`. This is less robust because the same visual still moves between popup trees and is likely to retain stale state.

Do not share `Control` instances from `ItemToolTip` between tooltip popups. A visual can have one parent only and a control tree may retain bindings, templated state, and popup state from its prior host.

## Original Consumer XAML Context

The design includes:

```xml
<vb:VBTimelineView
    VBContent="ACPropertyLogsRoot"
    VBSource="SelectedPropertyLog"
    GanttStart="StartDate"
    GanttEnd="EndDate"
    SearchProperty="SearchText"
    BSOUpdateDisplayOrderMethodName="UpdateDisplayOrder"
    TimeFrameFrom="TimeFrameFrom"
    TimeFrameTo="TimeFrameTo">
```

Tooltip content is supplied using the legacy attached / collection property:

```xml
<vb:VBTimelineView.ItemToolTip>
  <vb:VBTextBox VBContent="StartDate" Margin="0,0,0,2" />
  <vb:VBTextBox VBContent="EndDate" Margin="0,0,0,2" />
  <vb:VBTextBox VBContent="Duration" Margin="0,0,0,2" />
  <vb:VBTextBox VBContent="ACCaption" Margin="0,0,0,2" />
  <vb:VBTextBox VBContent="PropertyValue" Margin="0,0,0,2" />
</vb:VBTimelineView.ItemToolTip>
```

The BSO type is `VBBSOPropertyLogPresenter`; normal runtime has it, but WYSIWYG design mode does not.

## Remaining WPF-Parity Work

These are functional gaps compared to the WPF version and remain unresolved.

### 1. Per-item timeline rectangle / border is missing

WPF displayed a bordered rectangle for each timeline item. Avalonia currently renders only text.

Investigate:

- `gip.core.layoutengine.avui/Controls/VBTimeline/Timeline/TimelineItem.cs`
- `gip.core.layoutengine.avui/Controls/VBTimeline/Base/TimelineItemBase.cs`
- Timeline item themes / `.axaml` files under `gip.core.layoutengine.avui/Controls/VBTimeline/`
- The WPF implementation for equivalent `TimelineItem` templates and styles.

Likely needs a `Border` / appropriate template part around the timeline item's content in the Avalonia `ControlTheme` or `DataTemplate`, with correct width and position supplied by `TimelinePanel`.

### 2. Timestamp vertical lines in `PART_DateTimeAxesPanel` are missing

WPF rendered vertical grid lines for each timestamp shown by `PART_DateTimeAxesPanel`; Avalonia does not.

Investigate:

- the control exposing `PART_DateTimeAxesPanel`
- timeline axis panel rendering / measure / arrange logic
- WPF equivalent axis panel implementation, looking for line / grid rendering in `OnRender`, visual children, or generated `Border`/`Line` elements.

Prefer drawing repeated grid lines in `Render(DrawingContext)` when they are purely decorative and derived from timestamp positions. This avoids creating many visual children and reduces layout churn.

### 3. Ctrl + left mouse drag changes time range in `FrictionScrollViewer`

WPF behavior: hold left Ctrl, hold left mouse button, drag over the `FrictionScrollViewer` to alter the visible time range.

The Avalonia implementation does not currently provide this interaction.

Investigate:

- `FrictionScrollViewer` classes in `gip.core.layoutengine.avui`
- pointer event handlers (`PointerPressed`, `PointerMoved`, `PointerReleased`)
- WPF equivalent input logic.

Expected Avalonia mapping:

- `PointerPressed`: check `e.GetCurrentPoint(control).Properties.IsLeftButtonPressed` and `e.KeyModifiers.HasFlag(KeyModifiers.Control)`.
- capture pointer while range drag is active.
- `PointerMoved`: compute the drag delta and translate it to time span / viewport range.
- `PointerReleased` and lost capture: end range drag and release capture.
- Do not interfere with regular scrolling, panning, or selection when Ctrl is absent.

## Other Observed Warnings

These are separate from the tooltip parent exception and should be triaged independently:

- `EndpointNotFoundException` to `net.tcp://gipsoft-dlmhp-26:8002/`: external service endpoint refused connection.
- Dock binding messages where `Layout`, `DockCapabilityOverrides`, or `Owner.DockCapabilityPolicy` are null during initialization.
- `VBComboBox` template binds `$templatedParent.IsReadOnly`, but `VBComboBox` lacks that property.
- `ACQueryDefinition.TakeCount` null binding during initialization.
- `StaticResourceExtension` instances were sometimes assigned as raw values to properties such as `Theme`, `Background`, `ControlTheme`, or `IValueConverter`. This points to incomplete markup extension/resource resolution in the custom XAML DOM parser and is separate from the timeline tooltip problem.

## Validation Performed

After the tooltip parent fix, the focused build completed successfully:

```text
dotnet build gip.core.layoutengine.avui/gip.core.layoutengine.avui.csproj /p:Configuration=Debug
0 Error(s)
```

Manual runtime result reported by the user:

- Hovering different timeline items no longer throws the visual-parent exception.
- Tooltip opens, but content is stale and intermittently missing.

## Suggested Next-Chat Entry Point

Start by reading these files together:

1. `gip.core.layoutengine.avui/Controls/VBTimeline/Base/TimelineItemBase.cs`
2. `gip.core.layoutengine.avui/Controls/VBTimeline/Timeline/TimelineItem.cs`
3. `gip.core.layoutengine.avui/Controls/VBTimeline/Base/VBTimelineViewBase.cs`
4. `gip.core.layoutengine.avui/Controls/VBTimeline/Base/VBTimelineChartBase.cs`
5. The WPF counterparts for those timeline classes and their XAML / templates.

First local hypothesis to test: `ItemToolTip` must be instantiated separately for each timeline item (or rebuilt per tooltip opening) because the current shared controls keep the first item's binding context. Validate by logging / inspecting the active tooltip child `DataContext` while hovering item A then item B.
