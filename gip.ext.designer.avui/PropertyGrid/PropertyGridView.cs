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
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.PropertyGrid
{
    [TemplatePart(Name = "PART_Thumb", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_clearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NameTextBox", Type = typeof(TextBox))]
    public class PropertyGridView : Control
    {
        static PropertyGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridView), new FrameworkPropertyMetadata(typeof(PropertyGridView)));
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PropertyGrid.DependencyPropFilter = this.DependencyPropFilter;

            thumb = Template.FindName("PART_Thumb", this) as Thumb;
            if (thumb != null)
                thumb.DragDelta += new DragDeltaEventHandler(thumb_DragDelta);

            clearButton = Template.FindName("PART_clearButton", this) as Button;
            if (clearButton != null)
                clearButton.Click += new RoutedEventHandler(clearButton_Click);

            NameTextBox = Template.FindName("PART_NameTextBox", this) as TextBox;
        }


        static PropertyContextMenu propertyContextMenu = new PropertyContextMenu();

        public PropertyGrid PropertyGrid { get; protected set; }

        public static readonly DependencyProperty FirstColumnWidthProperty =
            DependencyProperty.Register("FirstColumnWidth", typeof(double), typeof(PropertyGridView),
            new PropertyMetadata(120.0));

        public double FirstColumnWidth
        {
            get { return (double)GetValue(FirstColumnWidthProperty); }
            set { SetValue(FirstColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable<DesignItem>), typeof(PropertyGridView));

        public IEnumerable<DesignItem> SelectedItems
        {
            get { return (IEnumerable<DesignItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }


        public enum ShowAsMode
        {
            AsGrid,
            AsListbox,
            OnlyPropertyGrid
        }

        public static readonly DependencyProperty ShowAsProperty =
            DependencyProperty.Register("ShowAs", typeof(ShowAsMode), typeof(PropertyGridView), new PropertyMetadata(ShowAsMode.AsGrid));

        public ShowAsMode ShowAs
        {
            get { return (ShowAsMode)GetValue(ShowAsProperty); }
            set { SetValue(ShowAsProperty, value); }
        }


        public enum EnumBarMode
        {
            ShowBoth,
            OnlyProperties,
            OnlyEvents
        }

        public static readonly DependencyProperty FilterVisibilityProperty =
            DependencyProperty.Register("FilterVisibility", typeof(Visibility), typeof(PropertyGridView), new PropertyMetadata(Visibility.Visible));

        public Visibility FilterVisibility
        {
            get { return (Visibility)GetValue(FilterVisibilityProperty); }
            set { SetValue(FilterVisibilityProperty, value); }
        }


        public static readonly DependencyProperty ShowEnumBarProperty =
            DependencyProperty.Register("ShowEnumBar", typeof(EnumBarMode), typeof(PropertyGridView), new PropertyMetadata(EnumBarMode.ShowBoth));

        public EnumBarMode ShowEnumBar
        {
            get { return (EnumBarMode)GetValue(ShowEnumBarProperty); }
            set { SetValue(ShowEnumBarProperty, value); }
        }


        public static readonly DependencyProperty DependencyPropFilterProperty =
            DependencyProperty.Register("DependencyPropFilter", typeof(bool), typeof(PropertyGridView),
            new PropertyMetadata(false));

        public bool DependencyPropFilter
        {
            get { return (bool)GetValue(DependencyPropFilterProperty); }
            set { SetValue(DependencyPropFilterProperty, value); }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SelectedItemsProperty)
            {
                PropertyGrid.SelectedItems = SelectedItems;
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
#if DEBUG
            Point x = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, x);
            if ((result != null) && (result.VisualHit != null))
            {
                System.Diagnostics.Debug.WriteLine("OnMouseRightButtonUp() P:" + x.ToString() + " Type:" + result.VisualHit.GetType().FullName);
            }
#endif
            Border row;
            PropertyNode node = GetNodeOverVisualPos(e.OriginalSource as DependencyObject, out row);
            //if (node.IsEvent) return;

            PropertyContextMenu contextMenu = new PropertyContextMenu();
            contextMenu.DataContext = node;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.HorizontalOffset = -30;
            contextMenu.PlacementTarget = row;
            contextMenu.IsOpen = true;
        }

        protected PropertyNode GetNodeOverVisualPos(DependencyObject visual, out Border row)
        {
            var ancestors = (visual as DependencyObject).GetVisualAncestors();
            row = ancestors.OfType<Border>().Where(b => b.Name == "uxPropertyNodeRow").FirstOrDefault();
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

        void thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            FirstColumnWidth = Math.Max(0, FirstColumnWidth + e.HorizontalChange);
        }
    }
}
