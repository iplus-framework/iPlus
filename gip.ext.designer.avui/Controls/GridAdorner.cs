// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using gip.ext.xamldom.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// Adorner that displays the blue bar next to grids that can be used to create new rows/column.
    /// </summary>
    public class GridRailAdorner : Control
    {
        static GridRailAdorner()
        {
            bgBrush = new SolidColorBrush(Color.FromArgb(0x35, 0x1E, 0x90, 0xff));
            // Note: Avalonia doesn't have Freeze() method, immutable brushes are handled differently
        }

        readonly DesignItem gridItem;
        readonly Grid grid;
        readonly AdornerPanel adornerPanel;
        readonly GridSplitterAdorner previewAdorner;
        readonly Orientation orientation;
        readonly GridUnitSelector unitSelector;

        static readonly SolidColorBrush bgBrush;

        public const double RailSize = 10;
        public const double RailDistance = 6;
        public const double SplitterWidth = 10;

        bool displayUnitSelector; // Indicates whether Grid UnitSeletor should be displayed.

        public GridRailAdorner(DesignItem gridItem, AdornerPanel adornerPanel, Orientation orientation)
        {
            Debug.Assert(gridItem != null);
            Debug.Assert(adornerPanel != null);

            this.gridItem = gridItem;
            this.grid = (Grid)gridItem.Component;
            this.adornerPanel = adornerPanel;
            this.orientation = orientation;
            this.displayUnitSelector = false;
            this.unitSelector = new GridUnitSelector(this);
            adornerPanel.Children.Add(unitSelector);

            if (orientation == Orientation.Horizontal)
            {
                this.Height = RailSize;
                previewAdorner = new GridColumnSplitterAdorner(this, gridItem, null, null);
            }
            else
            { // vertical
                this.Width = RailSize;
                previewAdorner = new GridRowSplitterAdorner(this, gridItem, null, null);
            }
            unitSelector.Orientation = orientation;
            previewAdorner.IsPreview = true;
            previewAdorner.IsHitTestVisible = false;
            unitSelector.IsVisible = false;
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);

            if (orientation == Orientation.Horizontal)
            {
                Rect bgRect = new Rect(0, 0, grid.Bounds.Width, RailSize);
                drawingContext.DrawRectangle(bgBrush, null, bgRect);

                DesignItemProperty colCollection = gridItem.Properties["ColumnDefinitions"];
                double currentOffset = 0;
                foreach (var colItem in colCollection.CollectionElements)
                {
                    ColumnDefinition column = colItem.Component as ColumnDefinition;
                    if (column.ActualWidth < 0) continue;
                    GridLength len = (GridLength)column.GetValue(ColumnDefinition.WidthProperty);

                    // Create FormattedText for Avalonia
                    var formattedText = new FormattedText(
                        GridLengthToText(len),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        10,
                        Brushes.Black
                    );

                    // Center the text - use our calculated offset instead of non-existent Offset property
                    // In Avalonia FormattedText, use Width and Height properties directly
                    var centerX = currentOffset + column.ActualWidth / 2 - formattedText.Width / 2;
                    drawingContext.DrawText(formattedText, new Point(centerX, 0));

                    currentOffset += column.ActualWidth;
                }
            }
            else
            {
                Rect bgRect = new Rect(0, 0, RailSize, grid.Bounds.Height);
                drawingContext.DrawRectangle(bgBrush, null, bgRect);

                DesignItemProperty rowCollection = gridItem.Properties["RowDefinitions"];
                double currentOffset = 0;
                foreach (var rowItem in rowCollection.CollectionElements)
                {
                    RowDefinition row = rowItem.Component as RowDefinition;
                    if (row.ActualHeight < 0) continue;
                    GridLength len = (GridLength)row.GetValue(RowDefinition.HeightProperty);

                    // Create FormattedText for Avalonia
                    var formattedText = new FormattedText(
                        GridLengthToText(len),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        10,
                        Brushes.Black
                    );

                    // Rotate and position text
                    using (drawingContext.PushTransform(Matrix.CreateRotation(-Math.PI / 2)))
                    {
                        var centerY = (currentOffset + row.ActualHeight / 2) * -1 - formattedText.Width / 2;
                        drawingContext.DrawText(formattedText, new Point(centerY, 0));
                    }

                    currentOffset += row.ActualHeight;
                }
            }
        }

        // Helper method to get row/column offset
        private double GetRowOffset(int rowIndex)
        {
            double offset = 0;
            for (int i = 0; i < rowIndex && i < grid.RowDefinitions.Count; i++)
            {
                offset += grid.RowDefinitions[i].ActualHeight;
            }
            return offset;
        }

        private double GetColumnOffset(int columnIndex)
        {
            double offset = 0;
            for (int i = 0; i < columnIndex && i < grid.ColumnDefinitions.Count; i++)
            {
                offset += grid.ColumnDefinitions[i].ActualWidth;
            }
            return offset;
        }

        private int GetRowIndexAtPosition(double position)
        {
            double currentOffset = 0;
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                if (position >= currentOffset && position <= (currentOffset + grid.RowDefinitions[i].ActualHeight))
                    return i;
                currentOffset += grid.RowDefinitions[i].ActualHeight;
            }
            return -1;
        }

        private int GetColumnIndexAtPosition(double position)
        {
            double currentOffset = 0;
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                if (position >= currentOffset && position <= (currentOffset + grid.ColumnDefinitions[i].ActualWidth))
                    return i;
                currentOffset += grid.ColumnDefinitions[i].ActualWidth;
            }
            return -1;
        }

        #region Handle mouse events to add a new row/column
        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            this.Cursor = new Cursor(StandardCursorType.Cross);
            RelativePlacement rpUnitSelector = new RelativePlacement();
            if (orientation == Orientation.Vertical)
            {
                double insertionPosition = e.GetPosition(this).Y;
                int rowIndex = GetRowIndexAtPosition(insertionPosition);
                if (rowIndex >= 0 && rowIndex < grid.RowDefinitions.Count)
                {
                    RowDefinition current = grid.RowDefinitions[rowIndex];
                    double rowOffset = GetRowOffset(rowIndex);
                    DesignItem component = this.gridItem.Services.Component.GetDesignItem(current);
                    rpUnitSelector.XOffset = -(RailSize + RailDistance) * 2.75;
                    rpUnitSelector.WidthOffset = RailSize + RailDistance;
                    rpUnitSelector.WidthRelativeToContentWidth = 1;
                    rpUnitSelector.HeightOffset = 55;
                    rpUnitSelector.YOffset = rowOffset + current.ActualHeight / 2 - 25;
                    unitSelector.SelectedItem = component;
                    unitSelector.Unit = ((GridLength)component.Properties[RowDefinition.HeightProperty].ValueOnInstance).GridUnitType;
                    displayUnitSelector = true;
                }
                else
                {
                    displayUnitSelector = false;
                }
            }
            else
            {
                double insertionPosition = e.GetPosition(this).X;
                int columnIndex = GetColumnIndexAtPosition(insertionPosition);
                if (columnIndex >= 0 && columnIndex < grid.ColumnDefinitions.Count)
                {
                    ColumnDefinition current = grid.ColumnDefinitions[columnIndex];
                    double columnOffset = GetColumnOffset(columnIndex);
                    DesignItem component = this.gridItem.Services.Component.GetDesignItem(current);
                    Debug.Assert(component != null);
                    rpUnitSelector.YOffset = -(RailSize + RailDistance) * 2.20;
                    rpUnitSelector.HeightOffset = RailSize + RailDistance;
                    rpUnitSelector.HeightRelativeToContentHeight = 1;
                    rpUnitSelector.WidthOffset = 75;
                    rpUnitSelector.XOffset = columnOffset + current.ActualWidth / 2 - 35;
                    unitSelector.SelectedItem = component;
                    unitSelector.Unit = ((GridLength)component.Properties[ColumnDefinition.WidthProperty].ValueOnInstance).GridUnitType;
                    displayUnitSelector = true;
                }
                else
                {
                    displayUnitSelector = false;
                }
            }
            if (displayUnitSelector)
                unitSelector.IsVisible = true;
            if (!adornerPanel.Children.Contains(previewAdorner))
                adornerPanel.Children.Add(previewAdorner);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            RelativePlacement rp = new RelativePlacement();
            RelativePlacement rpUnitSelector = new RelativePlacement();
            if (orientation == Orientation.Vertical)
            {
                double insertionPosition = e.GetPosition(this).Y;
                int rowIndex = GetRowIndexAtPosition(insertionPosition);

                rp.XOffset = -(RailSize + RailDistance);
                rp.WidthOffset = RailSize + RailDistance;
                rp.WidthRelativeToContentWidth = 1;
                rp.HeightOffset = SplitterWidth;
                rp.YOffset = e.GetPosition(this).Y - SplitterWidth / 2;
                if (rowIndex >= 0 && rowIndex < grid.RowDefinitions.Count)
                {
                    RowDefinition current = grid.RowDefinitions[rowIndex];
                    double rowOffset = GetRowOffset(rowIndex);
                    DesignItem component = this.gridItem.Services.Component.GetDesignItem(current);
                    rpUnitSelector.XOffset = -(RailSize + RailDistance) * 2.75;
                    rpUnitSelector.WidthOffset = RailSize + RailDistance;
                    rpUnitSelector.WidthRelativeToContentWidth = 1;
                    rpUnitSelector.HeightOffset = 75;
                    rpUnitSelector.YOffset = rowOffset + current.ActualHeight / 2 - 25;
                    unitSelector.SelectedItem = component;
                    unitSelector.Unit = ((GridLength)component.Properties[RowDefinition.HeightProperty].ValueOnInstance).GridUnitType;
                    displayUnitSelector = true;
                }
                else
                {
                    displayUnitSelector = false;
                }
            }
            else
            {
                double insertionPosition = e.GetPosition(this).X;
                int columnIndex = GetColumnIndexAtPosition(insertionPosition);

                rp.YOffset = -(RailSize + RailDistance);
                rp.HeightOffset = RailSize + RailDistance;
                rp.HeightRelativeToContentHeight = 1;
                rp.WidthOffset = SplitterWidth;
                rp.XOffset = e.GetPosition(this).X - SplitterWidth / 2;

                if (columnIndex >= 0 && columnIndex < grid.ColumnDefinitions.Count)
                {
                    ColumnDefinition current = grid.ColumnDefinitions[columnIndex];
                    double columnOffset = GetColumnOffset(columnIndex);
                    DesignItem component = this.gridItem.Services.Component.GetDesignItem(current);
                    Debug.Assert(component != null);
                    rpUnitSelector.YOffset = -(RailSize + RailDistance) * 2.20;
                    rpUnitSelector.HeightOffset = RailSize + RailDistance;
                    rpUnitSelector.HeightRelativeToContentHeight = 1;
                    rpUnitSelector.WidthOffset = 95;
                    rpUnitSelector.XOffset = columnOffset + current.ActualWidth / 2 - 35;
                    unitSelector.SelectedItem = component;
                    unitSelector.Unit = ((GridLength)component.Properties[ColumnDefinition.WidthProperty].ValueOnInstance).GridUnitType;
                    displayUnitSelector = true;
                }
                else
                {
                    displayUnitSelector = false;
                }
            }
            AdornerPanel.SetPlacement(previewAdorner, rp);
            if (displayUnitSelector)
                AdornerPanel.SetPlacement(unitSelector, rpUnitSelector);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            // In Avalonia, cursor is managed differently - typically through CSS-like styling
            if (!unitSelector.IsPointerOver)
            {
                unitSelector.IsVisible = false;
                displayUnitSelector = false;
            }
            if (adornerPanel.Children.Contains(previewAdorner))
                adornerPanel.Children.Remove(previewAdorner);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                Focus();
                adornerPanel.Children.Remove(previewAdorner);
                if (orientation == Orientation.Vertical)
                {
                    double insertionPosition = e.GetPosition(this).Y;
                    DesignItemProperty rowCollection = gridItem.Properties["RowDefinitions"];

                    DesignItem currentRow = null;

                    using (ChangeGroup changeGroup = gridItem.OpenGroup("Split grid row"))
                    {
                        if (rowCollection.CollectionElements.Count == 0)
                        {
                            DesignItem firstRow = gridItem.Services.Component.RegisterComponentForDesigner(new RowDefinition());
                            rowCollection.CollectionElements.Add(firstRow);
                            grid.InvalidateArrange(); // Avalonia equivalent of UpdateLayout

                            currentRow = firstRow;
                        }
                        else
                        {
                            int rowIndex = GetRowIndexAtPosition(insertionPosition);
                            if (rowIndex >= 0)
                                currentRow = gridItem.Services.Component.GetDesignItem(grid.RowDefinitions[rowIndex]);
                        }

                        if (currentRow == null)
                            currentRow = gridItem.Services.Component.GetDesignItem(grid.RowDefinitions.Last());

                        unitSelector.SelectedItem = currentRow;
                        for (int i = 0; i < grid.RowDefinitions.Count; i++)
                        {
                            RowDefinition row = grid.RowDefinitions[i];
                            double rowOffset = GetRowOffset(i);
                            if (rowOffset > insertionPosition) continue;
                            if (rowOffset + row.ActualHeight < insertionPosition) continue;

                            // split row
                            GridLength oldLength = (GridLength)row.GetValue(RowDefinition.HeightProperty);
                            GridLength newLength1, newLength2;
                            SplitLength(oldLength, insertionPosition - rowOffset, row.ActualHeight, out newLength1, out newLength2);
                            DesignItem newRowDefinition = gridItem.Services.Component.RegisterComponentForDesigner(new RowDefinition());
                            rowCollection.CollectionElements.Insert(i + 1, newRowDefinition);
                            rowCollection.CollectionElements[i].Properties[RowDefinition.HeightProperty].SetValue(newLength1);
                            newRowDefinition.Properties[RowDefinition.HeightProperty].SetValue(newLength2);
                            grid.InvalidateArrange();
                            FixIndicesAfterSplit(i, Grid.RowProperty, Grid.RowSpanProperty, insertionPosition);
                            grid.InvalidateArrange();
                            changeGroup.Commit();
                            break;
                        }
                    }
                }
                else
                {
                    double insertionPosition = e.GetPosition(this).X;
                    DesignItemProperty columnCollection = gridItem.Properties["ColumnDefinitions"];

                    DesignItem currentColumn = null;

                    using (ChangeGroup changeGroup = gridItem.OpenGroup("Split grid column"))
                    {
                        if (columnCollection.CollectionElements.Count == 0)
                        {
                            DesignItem firstColumn = gridItem.Services.Component.RegisterComponentForDesigner(new ColumnDefinition());
                            columnCollection.CollectionElements.Add(firstColumn);
                            grid.InvalidateArrange(); // Avalonia equivalent of UpdateLayout

                            currentColumn = firstColumn;
                        }
                        else
                        {
                            int columnIndex = GetColumnIndexAtPosition(insertionPosition);
                            if (columnIndex >= 0)
                                currentColumn = gridItem.Services.Component.GetDesignItem(grid.ColumnDefinitions[columnIndex]);
                        }

                        if (currentColumn == null)
                            currentColumn = gridItem.Services.Component.GetDesignItem(grid.ColumnDefinitions.Last());

                        unitSelector.SelectedItem = currentColumn;
                        for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                        {
                            ColumnDefinition column = grid.ColumnDefinitions[i];
                            double columnOffset = GetColumnOffset(i);
                            if (columnOffset > insertionPosition) continue;
                            if (columnOffset + column.ActualWidth < insertionPosition) continue;

                            // split column
                            GridLength oldLength = (GridLength)column.GetValue(ColumnDefinition.WidthProperty);
                            GridLength newLength1, newLength2;
                            SplitLength(oldLength, insertionPosition - columnOffset, column.ActualWidth, out newLength1, out newLength2);
                            DesignItem newColumnDefinition = gridItem.Services.Component.RegisterComponentForDesigner(new ColumnDefinition());
                            columnCollection.CollectionElements.Insert(i + 1, newColumnDefinition);
                            columnCollection.CollectionElements[i].Properties[ColumnDefinition.WidthProperty].SetValue(newLength1);
                            newColumnDefinition.Properties[ColumnDefinition.WidthProperty].SetValue(newLength2);
                            grid.InvalidateArrange();
                            FixIndicesAfterSplit(i, Grid.ColumnProperty, Grid.ColumnSpanProperty, insertionPosition);
                            changeGroup.Commit();
                            grid.InvalidateArrange();
                            break;
                        }
                    }
                }
                InvalidateVisual();
            }
        }

        private void FixIndicesAfterSplit(int splitIndex, AvaloniaProperty idxProperty, AvaloniaProperty spanProperty, double insertionPostion)
        {
            if (orientation == Orientation.Horizontal)
            {
                // increment ColSpan of all controls in the split column, increment Column of all controls in later columns:
                foreach (DesignItem child in gridItem.Properties["Children"].CollectionElements)
                {
                    Point topLeft = new Point(0, 0);
                    var childView = child.View as Control;
                    if (childView != null)
                    {
                        topLeft = childView.TranslatePoint(new Point(0, 0), grid) ?? new Point(0, 0);
                    }
                    var margin = (Thickness)child.Properties[Control.MarginProperty].ValueOnInstance;
                    var start = (int)child.Properties.GetAttachedProperty(idxProperty).ValueOnInstance;
                    var span = (int)child.Properties.GetAttachedProperty(spanProperty).ValueOnInstance;
                    if (start <= splitIndex && splitIndex < start + span)
                    {
                        var width = (double)child.Properties[Control.WidthProperty].ValueOnInstance;
                        if (insertionPostion >= topLeft.X + width)
                        {
                            continue;
                        }
                        if (insertionPostion > topLeft.X)
                            child.Properties.GetAttachedProperty(spanProperty).SetValue(span + 1);
                        else
                        {
                            child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                            margin = new Thickness(topLeft.X - insertionPostion, margin.Top, margin.Right, margin.Bottom);
                            child.Properties[Control.MarginProperty].SetValue(margin);
                        }
                    }
                    else if (start > splitIndex)
                    {
                        child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                    }
                }
            }
            else
            {
                foreach (DesignItem child in gridItem.Properties["Children"].CollectionElements)
                {
                    Point topLeft = new Point(0, 0);
                    var childView = child.View as Control;
                    if (childView != null)
                    {
                        topLeft = childView.TranslatePoint(new Point(0, 0), grid) ?? new Point(0, 0);
                    }
                    var margin = (Thickness)child.Properties[Control.MarginProperty].ValueOnInstance;
                    var start = (int)child.Properties.GetAttachedProperty(idxProperty).ValueOnInstance;
                    var span = (int)child.Properties.GetAttachedProperty(spanProperty).ValueOnInstance;
                    if (start <= splitIndex && splitIndex < start + span)
                    {
                        var height = (double)child.Properties[Control.HeightProperty].ValueOnInstance;
                        if (insertionPostion >= topLeft.Y + height)
                            continue;
                        if (insertionPostion > topLeft.Y)
                            child.Properties.GetAttachedProperty(spanProperty).SetValue(span + 1);
                        else
                        {
                            child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                            margin = new Thickness(margin.Left, topLeft.Y - insertionPostion, margin.Right, margin.Bottom);
                            child.Properties[Control.MarginProperty].SetValue(margin);
                        }
                    }
                    else if (start > splitIndex)
                    {
                        child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                    }
                }
            }
        }

        static void SplitLength(GridLength oldLength, double insertionPosition, double oldActualValue,
                                out GridLength newLength1, out GridLength newLength2)
        {
            if (oldLength.IsAuto)
            {
                oldLength = new GridLength(oldActualValue);
            }
            double percentage = insertionPosition / oldActualValue;
            newLength1 = new GridLength(oldLength.Value * percentage, oldLength.GridUnitType);
            newLength2 = new GridLength(oldLength.Value - newLength1.Value, oldLength.GridUnitType);
        }
        #endregion

        string GridLengthToText(GridLength len)
        {
            switch (len.GridUnitType)
            {
                case GridUnitType.Auto:
                    return "Auto";
                case GridUnitType.Star:
                    return len.Value == 1 ? "*" : Math.Round(len.Value, 2) + "*";
                case GridUnitType.Pixel:
                    return Math.Round(len.Value, 2) + "px";
            }
            return string.Empty;
        }

        public void SetGridLengthUnit(GridUnitType unit)
        {
            DesignItem item = unitSelector.SelectedItem;
            grid.InvalidateArrange();

            Debug.Assert(item != null);

            if (orientation == Orientation.Vertical)
            {
                SetGridLengthUnit(unit, item, RowDefinition.HeightProperty);
            }
            else
            {
                SetGridLengthUnit(unit, item, ColumnDefinition.WidthProperty);
            }
            grid.InvalidateArrange();
            InvalidateVisual();
        }

        void SetGridLengthUnit(GridUnitType unit, DesignItem item, AvaloniaProperty property)
        {
            DesignItemProperty itemProperty = item.Properties[property];
            GridLength oldValue = (GridLength)itemProperty.ValueOnInstance;
            GridLength value = GetNewGridLength(unit, oldValue);

            if (value != oldValue)
            {
                itemProperty.SetValue(value);
            }
        }

        GridLength GetNewGridLength(GridUnitType unit, GridLength oldValue)
        {
            if (unit == GridUnitType.Auto)
            {
                return GridLength.Auto;
            }
            return new GridLength(oldValue.Value, unit);
        }
    }

    public abstract class GridSplitterAdorner : TemplatedControl
    {
        public static readonly StyledProperty<bool> IsPreviewProperty =
            AvaloniaProperty.Register<GridSplitterAdorner, bool>(nameof(IsPreview), false);

        protected readonly Grid grid;
        protected readonly DesignItem gridItem;
        protected readonly DesignItem firstRow, secondRow; // can also be columns
        protected readonly GridRailAdorner rail;

        internal GridSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow, DesignItem secondRow)
        {
            Debug.Assert(gridItem != null);
            this.grid = (Grid)gridItem.Component;
            this.gridItem = gridItem;
            this.firstRow = firstRow;
            this.secondRow = secondRow;
            this.rail = rail;
        }

        public bool IsPreview
        {
            get { return GetValue(IsPreviewProperty); }
            set { SetValue(IsPreviewProperty, value); }
        }

        ChangeGroup activeChangeGroup;
        double mouseStartPos;
        bool mouseIsDown;
        IPointer capturedPointer;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Handled = true;
            capturedPointer = e.Pointer;
            // In Avalonia, Capture returns void and we store the pointer reference
            e.Pointer.Capture(this);
            Focus();
            mouseStartPos = GetCoordinate(e.GetPosition(grid));
            mouseIsDown = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (mouseIsDown)
            {
                double mousePos = GetCoordinate(e.GetPosition(grid));
                if (activeChangeGroup == null)
                {
                    // Note: SystemParameters doesn't exist in Avalonia, using hardcoded values
                    if (Math.Abs(mousePos - mouseStartPos) >= 4)
                    {
                        activeChangeGroup = gridItem.OpenGroup("Change grid row/column size");
                        RememberOriginalSize();
                    }
                }
                if (activeChangeGroup != null)
                {
                    ChangeSize(mousePos - mouseStartPos);
                }
            }
        }

        protected GridLength original1, original2;
        protected double originalPixelSize1, originalPixelSize2;

        protected abstract double GetCoordinate(Point point);
        protected abstract void RememberOriginalSize();
        protected abstract AvaloniaProperty RowColumnSizeProperty { get; }

        void ChangeSize(double delta)
        {
            // delta = difference in pixels

            if (delta < -originalPixelSize1) delta = -originalPixelSize1;
            if (delta > originalPixelSize2) delta = originalPixelSize2;

            // replace Auto lengths with absolute lengths if necessary
            if (original1.IsAuto) original1 = new GridLength(originalPixelSize1);
            if (original2.IsAuto) original2 = new GridLength(originalPixelSize2);

            GridLength new1;
            if (original1.IsStar && originalPixelSize1 > 0)
                new1 = new GridLength(original1.Value * (originalPixelSize1 + delta) / originalPixelSize1, GridUnitType.Star);
            else
                new1 = new GridLength(originalPixelSize1 + delta);
            GridLength new2;
            if (original2.IsStar && originalPixelSize2 > 0)
                new2 = new GridLength(original2.Value * (originalPixelSize2 - delta) / originalPixelSize2, GridUnitType.Star);
            else
                new2 = new GridLength(originalPixelSize2 - delta);
            firstRow.Properties[RowColumnSizeProperty].SetValue(new1);
            secondRow.Properties[RowColumnSizeProperty].SetValue(new2);
            Control e = this.GetVisualParent<Control>();
            e?.InvalidateArrange();
            rail.InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (activeChangeGroup != null)
            {
                activeChangeGroup.Commit();
                activeChangeGroup = null;
            }
            Stop();
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            Stop();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Stop();
            }
        }

        protected void Stop()
        {
            if (capturedPointer != null)
            {
                capturedPointer.Capture(null);
                capturedPointer = null;
            }
            mouseIsDown = false;
            if (activeChangeGroup != null)
            {
                activeChangeGroup.Abort();
                activeChangeGroup = null;
            }
        }
    }

    public class GridRowSplitterAdorner : GridSplitterAdorner
    {
        static GridRowSplitterAdorner()
        {
            // Note: Avalonia uses different styling approach, no DefaultStyleKeyProperty override needed here
            CursorProperty.OverrideDefaultValue<GridRowSplitterAdorner>(new Cursor(StandardCursorType.SizeNorthSouth));
        }

        public GridRowSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow, DesignItem secondRow)
            : base(rail, gridItem, firstRow, secondRow)
        {
        }

        protected override double GetCoordinate(Point point)
        {
            return point.Y;
        }

        protected override void RememberOriginalSize()
        {
            RowDefinition r1 = (RowDefinition)firstRow.Component;
            RowDefinition r2 = (RowDefinition)secondRow.Component;
            original1 = (GridLength)r1.GetValue(RowDefinition.HeightProperty);
            original2 = (GridLength)r2.GetValue(RowDefinition.HeightProperty);
            originalPixelSize1 = r1.ActualHeight;
            originalPixelSize2 = r2.ActualHeight;
        }

        protected override AvaloniaProperty RowColumnSizeProperty
        {
            get { return RowDefinition.HeightProperty; }
        }
    }

    public sealed class GridColumnSplitterAdorner : GridSplitterAdorner
    {
        static GridColumnSplitterAdorner()
        {
            // Note: Avalonia uses different styling approach, no DefaultStyleKeyProperty override needed here
            CursorProperty.OverrideDefaultValue<GridColumnSplitterAdorner>(new Cursor(StandardCursorType.SizeWestEast));
        }

        internal GridColumnSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow, DesignItem secondRow)
            : base(rail, gridItem, firstRow, secondRow)
        {
        }

        protected override double GetCoordinate(Point point)
        {
            return point.X;
        }

        protected override void RememberOriginalSize()
        {
            ColumnDefinition r1 = (ColumnDefinition)firstRow.Component;
            ColumnDefinition r2 = (ColumnDefinition)secondRow.Component;
            original1 = (GridLength)r1.GetValue(ColumnDefinition.WidthProperty);
            original2 = (GridLength)r2.GetValue(ColumnDefinition.WidthProperty);
            originalPixelSize1 = r1.ActualWidth;
            originalPixelSize2 = r2.ActualWidth;
        }

        protected override AvaloniaProperty RowColumnSizeProperty
        {
            get { return ColumnDefinition.WidthProperty; }
        }
    }
}
