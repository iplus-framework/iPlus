// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;
using System.Collections.Specialized;
using System.Collections;

namespace gip.ext.designer.avui.OutlineView
{
    // limitations:
    // - Do not use ItemsSource (use Root)
    // - Do not use Items (use Root)
    public class DragTreeView : TreeView, IDataObject
    {
        static DragTreeView()
        {
            // In AvaloniaUI, styling is handled through XAML styles rather than metadata override
        }

        public DragTreeView()
        {
            // In AvaloniaUI, we set up drag/drop event handlers differently
            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);

            DragDrop.SetAllowDrop(this, true);
            // TODO: Implement drag listener equivalent for AvaloniaUI
            new DragListener(this).DragStarted += DragTreeView_DragStarted;
        }

        DragTreeViewItem dropTarget;
        DragTreeViewItem treeItem;
        DragTreeViewItem dropAfter;
        int part;
        bool dropInside;
        bool dropCopy;
        bool canDrop;

        Border insertLine;

        public static readonly StyledProperty<object> RootProperty =
            AvaloniaProperty.Register<DragTreeView, object>(nameof(Root));

        public object Root
        {
            get { return GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        //public object[] SelectedItems
        //{
        //    get { return Selection.Select(item => item.DataContext).ToArray(); }
        //}

        #region Filtering

        public static readonly StyledProperty<string> FilterProperty =
            AvaloniaProperty.Register<DragTreeView, string>(nameof(Filter), null,
                coerce: null, defaultBindingMode: BindingMode.TwoWay);

        public string Filter
        {
            get { return GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == FilterProperty)
            {
                var ev = FilterChanged;
                if (ev != null)
                    ev(Filter);
            }
            else if (change.Property == RootProperty)
            {
                ItemsSource = new[] { Root };
            }
        }

        public event Action<string> FilterChanged;

        public virtual bool ShouldItemBeVisible(DragTreeViewItem dragTreeViewitem)
        {
            return true;
        }

        #endregion

        void DragTreeView_DragStarted(object sender, PointerEventArgs e)
        {
            // TODO: Implement drag drop for AvaloniaUI
            DragDrop.DoDragDrop(e, this, DragDropEffects.Copy);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            insertLine = e.NameScope.Find<Border>("PART_InsertLine");
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new DragTreeViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            recycleKey = null;
            return !(item is DragTreeViewItem);
        }

        void OnDragEnter(object sender, DragEventArgs e)
        {
            ProcessDrag(e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            ProcessDrag(e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            ProcessDrop(e);
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            HideDropMarker();
        }

        void PrepareDropInfo(DragEventArgs e)
        {
            dropTarget = null;
            dropAfter = null;
            treeItem = (e.Source as Visual)?.GetVisualAncestors().OfType<DragTreeViewItem>().FirstOrDefault();

            if (treeItem != null)
            {
                var parent = treeItem.GetLogicalParent<DragTreeViewItem>();
                ContentPresenter header = treeItem.HeaderPresenter;
                if (header != null)
                {
                    Point p = e.GetPosition(header);
                    part = (int)(p.Y / (header.Bounds.Height / 3));
                    dropCopy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                    dropInside = false;

                    if (part == 1 || parent == null)
                    {
                        dropTarget = treeItem;
                        dropInside = true;
                        if (treeItem.ItemCount > 0)
                        {
                            // TODO: Get last item container - need to find AvaloniaUI equivalent
                            // dropAfter = treeItem.ItemContainerGenerator.ContainerFromIndex(treeItem.Items.Count - 1) as DragTreeViewItem;
                        }
                    }
                    else if (part == 0)
                    {
                        dropTarget = parent;
                        // TODO: Implement index-based container lookup for AvaloniaUI
                        // var index = dropTarget.ItemContainerGenerator.IndexFromContainer(treeItem);
                        // if (index > 0) {
                        //     dropAfter = dropTarget.ItemContainerGenerator.ContainerFromIndex(index - 1) as DragTreeViewItem;
                        // }
                    }
                    else
                    {
                        dropTarget = parent;
                        dropAfter = treeItem;
                    }
                }
            }
        }

        void ProcessDrag(DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
            canDrop = false;

            // TODO: Implement proper data check for AvaloniaUI
            // if (e.Data.GetData(GetType()) != this) return;

            HideDropMarker();
            PrepareDropInfo(e);

            if (dropTarget != null && CanInsertInternal())
            {
                canDrop = true;
                e.DragEffects = dropCopy ? DragDropEffects.Copy : DragDropEffects.Move;
                DrawDropMarker();
            }
        }

        void ProcessDrop(DragEventArgs e)
        {
            HideDropMarker();

            if (canDrop)
            {
                InsertInternal();
            }
        }

        void DrawDropMarker()
        {
            if (dropInside)
            {
                dropTarget.IsDragHover = true;
            }
            else if (treeItem?.HeaderPresenter != null)
            {
                var header = treeItem.HeaderPresenter;
                // TODO: Implement coordinate transformation for AvaloniaUI
                // var p = header.TransformToVisual(this).Transform(
                //     new Point(0, part == 0 ? 0 : header.Bounds.Height));

                if (insertLine != null)
                {
                    insertLine.IsVisible = true;
                    // insertLine.Margin = new Thickness(p.X, p.Y, 0, 0);
                }
            }
        }

        void HideDropMarker()
        {
            if (insertLine != null)
            {
                insertLine.IsVisible = false;
            }
            if (dropTarget != null)
            {
                dropTarget.IsDragHover = false;
            }
        }

        internal HashSet<DragTreeViewItem> Selection = new HashSet<DragTreeViewItem>();
        DragTreeViewItem upSelection;

        internal void ItemMouseDown(DragTreeViewItem item)
        {
            upSelection = null;
            bool control = false; // TODO: Get control key state in AvaloniaUI
                                  // bool control = Keyboard.IsKeyDown(Key.LeftCtrl);

            if (Selection.Contains(item))
            {
                if (control)
                {
                    Unselect(item);
                }
                else
                {
                    upSelection = item;
                }
            }
            else
            {
                if (control)
                {
                    Select(item);
                }
                else
                {
                    SelectOnly(item);
                }
            }
        }

        internal void ItemMouseUp(DragTreeViewItem item)
        {
            if (upSelection == item)
            {
                SelectOnly(item);
            }
            upSelection = null;
        }

        internal void ItemAttached(DragTreeViewItem item)
        {
            if (item.IsSelected) Selection.Add(item);
        }

        internal void ItemDetached(DragTreeViewItem item)
        {
            if (item.IsSelected) Selection.Remove(item);
        }

        internal void ItemIsSelectedChanged(DragTreeViewItem item)
        {
            if (item.IsSelected)
            {
                Selection.Add(item);
            }
            else
            {
                Selection.Remove(item);
            }
        }

        void Select(DragTreeViewItem item)
        {
            Selection.Add(item);
            item.IsSelected = true;
            OnSelectionChanged();
        }

        void Unselect(DragTreeViewItem item)
        {
            Selection.Remove(item);
            item.IsSelected = false;
            OnSelectionChanged();
        }

        protected virtual void SelectOnly(DragTreeViewItem item)
        {
            ClearSelection();
            Select(item);
            OnSelectionChanged();
        }

        void ClearSelection()
        {
            foreach (var treeItem in Selection.ToArray())
            {
                treeItem.IsSelected = false;
            }
            Selection.Clear();
            OnSelectionChanged();
        }

        void OnSelectionChanged()
        {
        }

        bool CanInsertInternal()
        {
            if (!dropCopy)
            {
                var item = dropTarget;
                while (true)
                {
                    if (Selection.Contains(item)) return false;
                    item = item?.GetLogicalParent<DragTreeViewItem>();
                    if (item == null) break;
                }

                if (Selection.Contains(dropAfter)) return false;
            }

            return CanInsert(dropTarget, Selection.ToArray(), dropAfter, dropCopy);
        }

        void InsertInternal()
        {
            var selection = Selection.ToArray();

            if (!dropCopy)
            {
                foreach (var item in Selection.ToArray())
                {
                    var parent = item.GetLogicalParent<DragTreeViewItem>();
                    //TODO
                    if (parent != null)
                    {
                        Remove(parent, item);
                    }
                }
            }
            Insert(dropTarget, selection, dropAfter, dropCopy);
        }

        protected virtual bool CanInsert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after, bool copy)
        {
            return true;
        }

        protected virtual void Insert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after, bool copy)
        {
        }

        protected virtual void Remove(DragTreeViewItem target, DragTreeViewItem item)
        {
        }

        public IEnumerable<string> GetDataFormats()
        {
            return null;
        }

        public bool Contains(string dataFormat)
        {
            return false;
        }

        public object Get(string dataFormat)
        {
            return "";
        }
    }
}
