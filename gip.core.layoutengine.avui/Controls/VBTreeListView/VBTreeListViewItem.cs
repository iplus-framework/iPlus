using System;
using System.Collections.Generic;
using System.Linq;
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
            
            // Sync the timeline row with the EFFECTIVE visibility (considering
            // collapsed ancestors), not just this item's own IsVisible.
            TimelineItemMap.IsCollapsed = !IsEffectivelyVisible();
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
            // Child containers are created by THIS item (it is the ItemsControl of
            // its own children). Like VBTreeListView.CreateContainerForItemOverride,
            // the container must receive Header/HeaderTemplate/ContentACObject,
            // otherwise the TreeDataTemplate's child-selector never runs for the
            // grandchildren and expanded rows stay empty.
            VBTreeListViewItem container = new VBTreeListViewItem();

            if (this.HeaderTemplate != null && item is IACObject acObject)
            {
                container.ContentACObject = acObject;
                container.HeaderTemplate = this.HeaderTemplate;
                container.Header = acObject;
                container.DataContext = acObject;
                container.PrepareItemContainerForParent(this);
            }

            return container;
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBTreeListViewItem>(item, out recycleKey);
        }


        protected override void PrepareContainerForItemOverride(Control container, object item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);

            // The TreeDataTemplate (ItemsSource="{Binding Items}") binds the child
            // items via HeaderedItemsControl.PrepareItemContainer -> BindChildren.
            // That path relies on the template being resolvable at prepare time;
            // to be robust, bind ItemsSource explicitly here for items that expose
            // their children through an "Items" property (e.g. ProgramLogWrapper).
            if (container is VBTreeListViewItem child && child.Header != null
                && child.Header.GetType().GetProperty("Items") != null)
            {
                Binding itemsBinding = new Binding("Items");
                itemsBinding.Source = child.Header;
                itemsBinding.Mode = BindingMode.OneWay;
                child.Bind(ItemsSourceProperty, itemsBinding);
            }

            VBTreeListView parent = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTreeListView)) as VBTreeListView;
            TreeViewItem childItem = container as TreeViewItem;
            if (parent != null && childItem != null)
            {
                //parent.ApplySorting(child.Items);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsVisibleProperty)
            {
                // IsVisible alone is not enough: it stays true for children of a
                // COLLAPSED ancestor (they are merely not realized/laid out). Compute
                // the effective visibility over the whole ancestor chain so the
                // timeline rows stay in sync with what the TreeListView actually
                // displays (only expanded branches).
                if (TimelineItemMap != null)
                    TimelineItemMap.IsCollapsed = !IsEffectivelyVisible();
            }
            else if (change.Property == IsExpandedProperty)
            {
                // Expanding/collapsing this item changes the effective visibility of
                // all realized descendants - update their timeline rows as well.
                UpdateDescendantTimelineRows();
            }
            else if (change.Property == IsSelectedProperty)
            {
                if (this.IsSelected)
                {
                    // Enforce single selection: the tree items live in nested
                    // ItemsControls, so the TreeView does not deselect the
                    // previously selected container automatically.
                    DeselectOtherTreeItems();
                }
                if (TimelineItemMap != null)
                {
                    TimelineItemMap.IsSelected = this.IsSelected;
                }
            }
            base.OnPropertyChanged(change);
        }

        private void UpdateDescendantTimelineRows()
        {
            foreach (var container in GetRealizedDescendantContainers())
            {
                if (container.TimelineItemMap != null)
                    container.TimelineItemMap.IsCollapsed = !container.IsEffectivelyVisible();
                container.UpdateDescendantTimelineRows();
            }
        }

        /// <summary>
        /// Deselects every other selected VBTreeListViewItem in the whole tree.
        /// Needed because the containers are spread over nested ItemsControls,
        /// where no automatic single-selection exists.
        /// NOTE: ContainerFromItem only resolves TOP-LEVEL items (children live
        /// in nested ItemsControls), therefore the visual tree is searched.
        /// </summary>
        private void DeselectOtherTreeItems()
        {
            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(this);
            while (owner is VBTreeListViewItem parentItem)
                owner = ItemsControl.ItemsControlFromItemContainer(parentItem);
            if (owner == null)
                return;
            foreach (var container in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(owner).OfType<VBTreeListViewItem>())
            {
                if (!ReferenceEquals(container, this) && container.IsSelected)
                    container.SetCurrentValue(IsSelectedProperty, false);
            }
        }

        private IEnumerable<VBTreeListViewItem> GetRealizedDescendantContainers()
        {
            foreach (object item in Items)
            {
                if (ContainerFromItem(item) is VBTreeListViewItem child)
                    yield return child;
            }
        }

        /// <summary>
        /// True if this item AND all its ancestor items are effectively visible
        /// (i.e. no collapsed TreeListViewItem in the chain hides this row).
        /// </summary>
        public bool IsEffectivelyVisible()
        {
            if (!IsVisible)
                return false;
            var parent = ItemsControl.ItemsControlFromItemContainer(this) as VBTreeListViewItem;
            while (parent != null)
            {
                if (!parent.IsVisible || !parent.IsExpanded)
                    return false;
                parent = ItemsControl.ItemsControlFromItemContainer(parent) as VBTreeListViewItem;
            }
            return true;
        }

        #endregion
    }
}
