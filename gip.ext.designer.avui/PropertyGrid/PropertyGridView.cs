// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;
using gip.ext.design.avui.PropertyGrid;
using System.Diagnostics;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace gip.ext.designer.avui.PropertyGrid
{
    [TemplatePart(Name = "PART_Thumb", Type = typeof(Control))]
    [TemplatePart(Name = "PART_clearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NameTextBox", Type = typeof(TextBox))]
    public class PropertyGridView : TemplatedControl
    {
        static PropertyGridView()
        {
            // In Avalonia, we use the DefaultStyleKeyProperty differently
        }

        public PropertyGridView()
        {
            PropertyGrid = OnCreatePropertyGrid();
            DataContext = PropertyGrid;
        }

        protected virtual PropertyGrid OnCreatePropertyGrid()
        {
            return new PropertyGrid();
        }

        Thumb thumb;
        Button clearButton;
        public TextBox NameTextBox { get; set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            this.PropertyGrid.DependencyPropFilter = this.DependencyPropFilter;

            thumb = e.NameScope.Find("PART_Thumb") as Thumb;
            if (thumb != null)
                thumb.DragDelta += thumb_DragDelta;

            clearButton = e.NameScope.Find("PART_clearButton") as Button;
            if (clearButton != null)
                clearButton.Click += clearButton_Click;

            NameTextBox = e.NameScope.Find("PART_NameTextBox") as TextBox;
        }

        static PropertyContextMenu propertyContextMenu = new PropertyContextMenu();

        public PropertyGrid PropertyGrid { get; protected set; }

        public static readonly StyledProperty<double> FirstColumnWidthProperty =
            AvaloniaProperty.Register<PropertyGridView, double>(nameof(FirstColumnWidth), 120.0);

        public double FirstColumnWidth
        {
            get { return GetValue(FirstColumnWidthProperty); }
            set { SetValue(FirstColumnWidthProperty, value); }
        }

        public static readonly StyledProperty<IEnumerable<DesignItem>> SelectedItemsProperty =
            AvaloniaProperty.Register<PropertyGridView, IEnumerable<DesignItem>>(nameof(SelectedItems));

        public IEnumerable<DesignItem> SelectedItems
        {
            get { return GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public enum ShowAsMode
        {
            AsGrid,
            AsListbox,
            OnlyPropertyGrid
        }

        public static readonly StyledProperty<ShowAsMode> ShowAsProperty =
            AvaloniaProperty.Register<PropertyGridView, ShowAsMode>(nameof(ShowAs), ShowAsMode.AsGrid);

        public ShowAsMode ShowAs
        {
            get { return GetValue(ShowAsProperty); }
            set { SetValue(ShowAsProperty, value); }
        }

        public enum EnumBarMode
        {
            ShowBoth,
            OnlyProperties,
            OnlyEvents
        }

        public static readonly StyledProperty<bool> FilterVisibilityProperty =
            AvaloniaProperty.Register<PropertyGridView, bool>(nameof(FilterVisibility), true);

        public bool FilterVisibility
        {
            get { return GetValue(FilterVisibilityProperty); }
            set { SetValue(FilterVisibilityProperty, value); }
        }

        public static readonly StyledProperty<EnumBarMode> ShowEnumBarProperty =
            AvaloniaProperty.Register<PropertyGridView, EnumBarMode>(nameof(ShowEnumBar), EnumBarMode.ShowBoth);

        public EnumBarMode ShowEnumBar
        {
            get { return GetValue(ShowEnumBarProperty); }
            set { SetValue(ShowEnumBarProperty, value); }
        }

        public static readonly StyledProperty<bool> DependencyPropFilterProperty =
            AvaloniaProperty.Register<PropertyGridView, bool>(nameof(DependencyPropFilter), false);

        public bool DependencyPropFilter
        {
            get { return GetValue(DependencyPropFilterProperty); }
            set { SetValue(DependencyPropFilterProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == SelectedItemsProperty)
            {
                PropertyGrid.SelectedItems = SelectedItems;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
#if DEBUG
                Point x = e.GetPosition(this);
                var result = this.InputHitTest(x);
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine("OnPointerReleased() P:" + x.ToString() + " Type:" + result.GetType().FullName);
                }
#endif
                Border row;
                PropertyNode node = GetNodeOverVisualPos(e.Source as Visual, out row);
                //if (node.IsEvent) return;

                PropertyContextMenu contextMenu = new PropertyContextMenu();
                contextMenu.DataContext = node;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.HorizontalOffset = -30;
                contextMenu.PlacementTarget = row;
                // In Avalonia, use Open method to display the context menu
                if (row != null)
                {
                    contextMenu.Open(row);
                }
                else
                    contextMenu.Open();
            }
            base.OnPointerReleased(e);
        }

        protected PropertyNode GetNodeOverVisualPos(Visual visual, out Border row)
        {
            var ancestors = visual?.GetVisualAncestors();
            row = ancestors?.OfType<Border>().Where(b => b.Name == "uxPropertyNodeRow").FirstOrDefault();
            if (row == null)
                return null;

            if (!(row.DataContext is PropertyNode))
                return null;
            PropertyNode node = row.DataContext as PropertyNode;
            return node;
        }

        void clearButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyGrid.ClearFilter();
        }

        void thumb_DragDelta(object sender, VectorEventArgs e)
        {
            FirstColumnWidth = Math.Max(0, FirstColumnWidth + e.Vector.X);
        }
    }
}
