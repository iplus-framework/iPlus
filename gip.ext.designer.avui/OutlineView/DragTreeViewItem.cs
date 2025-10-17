// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.designer.avui.OutlineView
{
    public class DragTreeViewItem : TreeViewItem
    {
        static DragTreeViewItem()
        {
            // In AvaloniaUI, we don't override metadata for DefaultStyleKey in the same way
            // The styling is handled through XAML styles
        }

        public DragTreeViewItem()
        {
            Loaded += DragTreeViewItem_Loaded;
            Unloaded += DragTreeViewItem_Unloaded;
        }

        void DragTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ParentTree = this.GetVisualAncestors().OfType<DragTreeView>().FirstOrDefault();
            if (ParentTree != null)
            {
                ParentTree.ItemAttached(this);
            }
        }

        void DragTreeViewItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ParentTree != null)
            {
                ParentTree.ItemDetached(this);
            }
            ParentTree = null;
        }

        public new static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<DragTreeViewItem, bool>(nameof(IsSelected));

        public new bool IsSelected
        {
            get { return GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly StyledProperty<bool> IsDragHoverProperty =
            AvaloniaProperty.Register<DragTreeViewItem, bool>(nameof(IsDragHover));

        public bool IsDragHover
        {
            get { return GetValue(IsDragHoverProperty); }
            set { SetValue(IsDragHoverProperty, value); }
        }

        //internal ContentPresenter HeaderPresenter
        //{
        //    get
        //    {
        //        // In AvaloniaUI, we use FindNameScope and Find instead of Template.FindName
        //        var nameScope = NameScope.GetNameScope(this);
        //        return nameScope?.Find<ContentPresenter>("PART_Header");
        //    }
        //}

        public static readonly StyledProperty<int> LevelDragTreeProperty =
            AvaloniaProperty.Register<DragTreeViewItem, int>(nameof(LevelDragTree));

        public int LevelDragTree
        {
            get { return GetValue(LevelDragTreeProperty); }
            set { SetValue(LevelDragTreeProperty, value); }
        }

        public DragTreeView ParentTree { get; private set; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsSelectedProperty)
            {
                if (ParentTree != null)
                {
                    ParentTree.ItemIsSelectedChanged(this);
                }
            }
        }

        // In AvaloniaUI, we override CreateContainerForItemOverride instead
        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new DragTreeViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<DragTreeViewItem>(item, out recycleKey);
            //recycleKey = null;
            //return !(item is DragTreeViewItem);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            // Check if it's left button press
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (e.Source is ToggleButton || e.Source is ItemsPresenter) return;
                ParentTree?.ItemMouseDown(this);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            // Check if it was left button release
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                ParentTree?.ItemMouseUp(this);
            }
        }

        // Handle visual parent changes - AvaloniaUI equivalent
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            var parentItem = this.GetLogicalParent<DragTreeViewItem>();
            if (parentItem != null)
            {
                LevelDragTree = parentItem.Level + 1;
            }
        }
    }
}
