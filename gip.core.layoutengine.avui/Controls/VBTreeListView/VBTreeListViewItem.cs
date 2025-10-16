using System;
using System.Collections.Generic;
using System.Text;
using gip.core.datamodel;
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
        private static List<CustomControlStyleInfo> _styleInfoList3 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TreeListViewItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBVBTreeListView/Themes/TreeListViewItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TreeListViewItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBVBTreeListView/Themes/TreeListViewItemStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of a custom styles.
        /// </summary>
        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList3;
            }
        }

        static VBTreeListViewItem()
        {
            
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTreeListViewItem), new FrameworkPropertyMetadata(typeof(VBTreeListViewItem)));
        }

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

        bool _themeApplied = false;
        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
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
            binding.Path = new PropertyPath("IsCollapsed");
            binding.Mode = BindingMode.OneWayToSource;
            SetBinding(VBTreeListViewItem.IsExpandedProperty, binding);
            IsVisibleChanged += VBTreeListViewItem_IsVisibleChanged;

            if (VBTimelineView == null)
                return;

            int index = VBTimelineView.PART_TimelineChart.Items.IndexWhere(c => c == this.DataContext);
            //TimelineItemsPresenter itemsPresenter = WpfUtility.FindVisualChild<TimelineItemsPresenter>(VBGanttView.PART_GanttChart);
            ItemsControl itemsPresenter = VBTimelineView.PART_TimelineChart.itemsPresenter as ItemsControl;
            if (itemsPresenter == null || index < 0)
                return;
            TimelineItemMap = itemsPresenter.ItemContainerGenerator.ContainerFromItem(VBTimelineView.PART_TimelineChart.Items[index]) as TimelineItemBase;
            if (TimelineItemMap == null)
                return;

            TimelineItemMap.VBTreeListViewItemMap = this;

            if (TimelineItemMap.VBTreeListViewItemMap == null)
                TimelineItemMap.VBTreeListViewItemMap = this;
            
            if (this.IsVisible)
            {
                TimelineItemMap.IsCollapsed = !IsVisible;
                TimelineItemMap.OnPropertyChanged("IsCollapsed");
            }
            if (this.IsSelected)
            {
                TimelineItemMap.IsSelected = this.IsSelected;
                TimelineItemMap.OnPropertyChanged("IsSelected");
            }
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
            IsVisibleChanged -= VBTreeListViewItem_IsVisibleChanged;
            this.ClearAllBindings();
            TimelineItemMap = null;
            _IsInitialized = false;
        }

        /// <summary>
        /// Actualizes the theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public override void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }

        #region Properties
        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        private int _level = -1;
        /// <summary>
        /// Gets or sets the item's hierarchy in the tree.
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    VBTreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as VBTreeListViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }

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
                    _VBTimelineView = WpfUtility.FindVisualParent<VBTimelineViewBase>(this);
                return _VBTimelineView;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the container for the item override.
        /// </summary>
        /// <returns>The new instance of VBTreeListViewItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBTreeListViewItem();
        }

        /// <summary>
        /// Determines is item overrides it's own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if is overrides, otherwise false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is VBTreeListViewItem;
        }

        /// <summary>
        /// Prepares the container for item override.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="item">The item to prepare.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            VBTreeListView parent = WpfUtility.FindVisualParent<VBTreeListView>(this);
            TreeViewItem child = element as TreeViewItem;
            if (parent != null && child != null)
            {
                parent.ApplySorting(child.Items);
            }
        }

        /// <summary>
        /// Handles the OnSelected event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnSelected(RoutedEventArgs e)
        {
            if (TimelineItemMap != null)
            {
                TimelineItemMap.IsSelected = true;
                TimelineItemMap.OnPropertyChanged("IsSelected");
            }

            base.OnSelected(e);
        }

        /// <summary>
        /// Handles the OnUnselected event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnUnselected(RoutedEventArgs e)
        {
            if (TimelineItemMap != null)
            {
                TimelineItemMap.IsSelected = false;
                TimelineItemMap.OnPropertyChanged("IsSelected");
            }
            base.OnUnselected(e);
        }

        void VBTreeListViewItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TimelineItemMap != null)
            {
                TimelineItemMap.IsCollapsed = !IsVisible;
                TimelineItemMap.OnPropertyChanged("IsCollapsed");
            }
        }
        #endregion
    }
}
