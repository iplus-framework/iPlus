using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.timeline;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents an item in <see cref="VBTreeListView"/> control.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Element in <see cref="VBTreeListView"/> control dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTreeListViewItem'}de{'VBTreeListViewItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTreeListViewItem : VBTreeViewItem
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBTreeListViewItem.
        /// </summary>
        public VBTreeListViewItem()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of VBTreeListViewItem.
        /// </summary>
        /// <param name="parentACElement">The parent ACElement parameter.</param>
        /// <param name="acComponent">The acComponent parameter.</param>
        public VBTreeListViewItem(IACInteractiveObject parentACElement, IACObject acComponent)
            : base()
        {
        }
        #endregion

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        bool _IsInitialized = false;

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public void InitVBControl()
        {
            if (_IsInitialized)
                return;
            _IsInitialized = true;

            Binding binding = new Binding();
            binding.Source = this.DataContext;
            binding.Path = "IsCollapsed";
            binding.Mode = BindingMode.OneWayToSource;
            this.Bind(VBTreeListViewItem.IsExpandedProperty, binding);

            if (VBTimelineView == null)
                return;

            int index = VBTimelineView.PART_TimelineChart.Items.IndexWhere(c => c == this.DataContext);
            //TimelineItemsPresenter itemsPresenter = WpfUtility.FindVisualChild<TimelineItemsPresenter>(VBGanttView.PART_GanttChart);
            ItemsControl itemsPresenter = VBTimelineView.PART_TimelineChart._ItemsPresenter as ItemsControl;
            if (itemsPresenter == null || index < 0)
                return;
            TimelineItemMap = itemsPresenter.ContainerFromItem(VBTimelineView.PART_TimelineChart.Items[index]) as TimelineItemBase;
            if (TimelineItemMap == null)
                return;

            TimelineItemMap.VBTreeListViewItemMap = this;

            if (TimelineItemMap.VBTreeListViewItemMap == null)
                TimelineItemMap.VBTreeListViewItemMap = this;
            
            if (this.IsVisible)
                TimelineItemMap.IsCollapsed = !IsVisible;
            if (this.IsSelected)
                TimelineItemMap.IsSelected = this.IsSelected;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            this.ClearAllBindings();
            TimelineItemMap = null;
            _IsInitialized = false;
        }


        #region Properties
        ///// <summary>
        ///// Item's hierarchy in the tree
        ///// </summary>
        //private int _level = -1;
        ///// <summary>
        ///// Gets or sets the item's hierarchy in the tree.
        ///// </summary>
        //public int Level
        //{
        //    get
        //    {
        //        if (_level == -1)
        //        {
        //            VBTreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as VBTreeListViewItem;
        //            _level = (parent != null) ? parent.Level + 1 : 0;
        //        }
        //        return _level;
        //    }
        //}

        internal TimelineItemBase TimelineItemMap;
        private VBTimelineViewBase _VBTimelineView;

        /// <summary>
        /// Gets the VBGanttChartView.
        /// </summary>
        public VBTimelineViewBase VBTimelineView
        {
            get 
            {
                if (_VBTimelineView == null)
                    _VBTimelineView = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineViewBase)) as VBTimelineViewBase;
                return _VBTimelineView;
            }
        }

        #endregion

        #region Methods

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new VBTreeListViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBTreeListViewItem>(item, out recycleKey);
        }


        protected override void PrepareContainerForItemOverride(Control container, object item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);

            VBTreeListView parent = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTreeListView)) as VBTreeListView;
            TreeViewItem child = container as TreeViewItem;
            if (parent != null && child != null)
            {
                //parent.ApplySorting(child.Items);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsVisibleProperty)
            {
                TimelineItemMap.IsCollapsed = !IsVisible;
            }
            else if (change.Property == IsSelectedProperty)
            {
                if (TimelineItemMap != null)
                {
                    TimelineItemMap.IsSelected = this.IsSelected;
                }
            }
            base.OnPropertyChanged(change);
        }

        #endregion
    }
}
