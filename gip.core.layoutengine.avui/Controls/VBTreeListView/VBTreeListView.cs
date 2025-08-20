using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.ganttchart;
using System.Dynamic;
using System.Reflection;
using System.Windows.Media;
using gip.core.layoutengine.avui.timeline;
using System.Windows.Shapes;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control element for displaying hierarchies.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Hierarchien.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTreeListView'}de{'VBTreeListView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTreeListView : VBTreeView
    {
        #region c'tors

        private static List<CustomControlStyleInfo> _styleInfoList2 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TreeListViewStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTreeListView/Themes/TreeListViewStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TreeListViewStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTreeListView/Themes/TreeListViewStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of a custom styles.
        /// </summary>
        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList2;
            }
        }

        static VBTreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTreeListView), new FrameworkPropertyMetadata(typeof(VBTreeListView)));
        }

        /// <summary>
        /// Creates a new instance of the VBTreeListView.
        /// </summary>
        public VBTreeListView()
            : base()
        {
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        bool _themeApplied = false;
        new bool _Initialized = false;

        /// <summary>
        /// Actualizes the theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public new void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }

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

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public new void InitVBControl()
        {
            if (_Initialized || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;
            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            _PropertyInfoOfACPropertySelected = null; // Selected Type Info
            object sourceOfBindingForSelItm = null; // Für das Binding der SelectedItem-Property, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            string pathOfBindingForSelItm = ""; // Für das Binding der SelectedItem-Property, der Pfad relativ zum Source-Objekt
            Global.ControlModes rightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref _PropertyInfoOfACPropertySelected, ref sourceOfBindingForSelItm, ref pathOfBindingForSelItm, ref rightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTreeListView", VBContent);
                return;
            }

            string acAccess = "";
            ACClassProperty sourceProperty = null;
            _ACURLOfPropertyForItemsSource = _PropertyInfoOfACPropertySelected is ACClassProperty ? (_PropertyInfoOfACPropertySelected as ACClassProperty).GetACSource(_ACURLOfPropertyForItemsSource, out acAccess, out sourceProperty) : _ACURLOfPropertyForItemsSource;
            _ItemsSourceACTypeInfo = null;
            object sourceOfBindingForItmSrc = null; // Für das Binding der ItemsSource, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            string pathOfBindingForItmSrc = ""; // Für das Binding der ItemsSource, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(_ACURLOfPropertyForItemsSource, ref _ItemsSourceACTypeInfo, ref sourceOfBindingForItmSrc, ref pathOfBindingForItmSrc, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBTreeListView", _ACURLOfPropertyForItemsSource + " " + VBContent);
                return;
            }

            Binding binding2 = new Binding();
            binding2.Source = sourceOfBindingForItmSrc;
            binding2.Path = new PropertyPath(pathOfBindingForItmSrc);
            SetBinding(VBTreeListView.ItemsSourceProperty, binding2);

            if (_VBTimelineView != null && _VBTimelineView.TreeListViewColumns.Count == 0 && !string.IsNullOrEmpty(VBShowColumns))
            {
                List<ACColumnItem> vbShowColumns = ACQueryDefinition.BuildACColumnsFromVBSource(_VBTimelineView.VBShowColumns);
                Columns.Clear();
                foreach (ACColumnItem dataShowColumn in vbShowColumns)
                {
                    IACType dsColACTypeInfo;
                    object dsColSource = null;
                    string dsColPath = "";
                    Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
                    if (!ResolveColumnItem(dataShowColumn, out dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                    {
                        this.Root().Messages.LogDebug("Error00005", "VBTreeListView", dataShowColumn.PropertyName + " " + VBContent);
                        continue;
                    }
                    GridViewColumn col = new GridViewColumn();
                    Columns.Add(col);
                    Binding binding = new Binding();
                    binding.Path = new PropertyPath(dsColPath);
                    binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
                    col.DisplayMemberBinding = binding;
                    col.Header = dsColACTypeInfo.ACCaption;
                }
            }
            else
            {
                foreach (var col in this.Columns)
                {
                    if (col is VBGridViewColumn)
                    {
                        VBGridViewColumn column = col as VBGridViewColumn;
                        if (string.IsNullOrEmpty(column.VBContent))
                            continue;

                        IACType dsColACTypeInfo = ItemsSourceACTypeInfo;
                        object dsColSource = null;
                        string dsColPath = "";
                        Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
                        if (!ItemsSourceACTypeInfo.ACUrlBinding(column.VBContent, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                        {
                            this.Root().Messages.LogDebug("Error00005", "VBTreeListView", column.VBContent + " " + VBContent);
                        }
                        if (column.CellTemplate == null)
                        {
                            Binding binding = new Binding();
                            binding.Path = new PropertyPath(dsColPath);
                            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
                            if (column.StringFormat != null)
                                binding.StringFormat = column.StringFormat;
                            column.DisplayMemberBinding = binding;
                        }
                        column.Header = dsColACTypeInfo.ACCaption;
                    }
                }
            }

            this.Loaded += VBTreeListView_Loaded;
        }

        private void VBTreeListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsColumnHeadersInitialized)
                return;

            GridViewHeaderRowPresenter presenter = Helperclasses.VBVisualTreeHelper.FindChildObjectInVisualTree(this, typeof(GridViewHeaderRowPresenter)) as GridViewHeaderRowPresenter;
            if (presenter != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(presenter);
                for(int i=0; i< childrenCount; i++)
                {
                    GridViewColumnHeader header = VisualTreeHelper.GetChild(presenter, i) as GridViewColumnHeader;
                    if (header != null && header.Content != null)
                    {
                        Path sortIcon = Helperclasses.VBVisualTreeHelper.FindChildObjectInVisualTree(header, "SortArrow") as Path;
                        if (sortIcon != null)
                        {
                            header.Click += Header_Click;
                            _ColumnHeaders.Add(header, sortIcon);
                        }
                    }
                }
                _IsColumnHeadersInitialized = true;
            }
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            if(header != null)
            {
                ListSortDirection sortDirection = ListSortDirection.Ascending;
                string propertyName = header.Content != null ? header.Content.ToString() : "";

                if(Items.SortDescriptions.Count > 0 && _CurrentSortColumnHeader == header)
                {
                    SortDescription sortDesc = Items.SortDescriptions[0];
                    if (sortDesc.Direction == sortDirection)
                        sortDirection = ListSortDirection.Descending;
                }

                int index = Columns.IndexWhere(c => c.Header.ToString() == propertyName);
                VBGridViewColumn col = Columns[index] as VBGridViewColumn;
                if (col == null || !col.IsSortEnabled)
                    return;

                propertyName = col.VBContent;
                if(_CurrentSortColumnHeader != null)
                {
                    Path sortIcon = null;
                    if (_ColumnHeaders.TryGetValue(_CurrentSortColumnHeader, out sortIcon))
                    {
                        sortIcon.Visibility = Visibility.Hidden;
                        sortIcon.RenderTransform = null;
                    }
                }

                _CurrentSortColumnHeader = header;
                if (_CurrentSortColumnHeader != null)
                {
                    Path sortIcon = null;
                    if (_ColumnHeaders.TryGetValue(_CurrentSortColumnHeader, out sortIcon))
                    {
                        sortIcon.Visibility = Visibility.Visible;
                        if (sortDirection == ListSortDirection.Ascending)
                            sortIcon.RenderTransform = new RotateTransform(180);
                    }
                }

                var items = CollectionViewSource.GetDefaultView(Items);
                items.SortDescriptions.Clear();
                items.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
                if(_VBTimelineView != null && !string.IsNullOrEmpty(_VBTimelineView.BSOUpdateDisplayOrderMethodName))
                    BSOACComponent.ExecuteMethod(_VBTimelineView.BSOUpdateDisplayOrderMethodName.StartsWith("!") ? _VBTimelineView.BSOUpdateDisplayOrderMethodName : "!"+_VBTimelineView.BSOUpdateDisplayOrderMethodName, items);
                items.Refresh();
            }
        }

        internal bool ResolveColumnItem(ACColumnItem dataShowColumn, out IACType dsColACTypeInfo, ref object dsColSource, ref string dsColPath, ref Global.ControlModes dsColRightControlMode)
        {
            dsColACTypeInfo = ItemsSourceACTypeInfo;
            if (!ItemsSourceACTypeInfo.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00005", "VBDataGrid", dataShowColumn.PropertyName + " " + VBContent);
                return false;
            }
            return true;
        }

        #endregion

        #region Properties

        IACType _PropertyInfoOfACPropertySelected = null;
        string _ACURLOfPropertyForItemsSource;

        private IACType _ItemsSourceACTypeInfo;
        /// <summary>
        /// ACClassProperty die, die Liste beschreibt die an die ItemsSource-Property des DataGrids gebunden wird
        /// Es handelt sich dabei um die Properties im BSO, die mit dem Attribut [ACPropertyList(..,..)] versehen sind
        /// </summary>
        internal IACType ItemsSourceACTypeInfo
        {
            get
            {
                return _ItemsSourceACTypeInfo;
            }
        }

        private VBTimelineViewBase _VBTimelineView
        {
            get 
            {
                return TemplatedParent as VBTimelineViewBase;
            }
        }

        private Dictionary<GridViewColumnHeader, Path> _ColumnHeaders = new Dictionary<GridViewColumnHeader, Path>();

        private bool _IsColumnHeadersInitialized = false;

        private GridViewColumnHeader _CurrentSortColumnHeader;

        #region Columns
        private GridViewColumnCollection _columns;
        /// <summary>
        /// Gets or sets the GridView columns.
        /// </summary>
        public GridViewColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection();
                }

                return _columns;
            }
            set
            {
                _columns = value;
            }
        }

        #endregion

        #region SortDescriptions

        /// <summary>
        /// Represents the dependency property for SortDescriptions.
        /// </summary>
        public static readonly DependencyProperty SortDescriptionsProperty =
                DependencyProperty.Register("SortDescriptions", typeof(IEnumerable<SortDescription>), typeof(VBTreeListView), new FrameworkPropertyMetadata(null, SortDescriptionsChanged));

        /// <summary>
        /// Gets or sets the SortDescriptions.
        /// </summary>
        public IEnumerable<SortDescription> SortDescriptions
        {
            get { return (IEnumerable<SortDescription>)GetValue(SortDescriptionsProperty); }
            set { SetValue(SortDescriptionsProperty, value); }
        }

        private static void SortDescriptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VBTreeListView)d;
            self.SortDescriptionsChanged(e);
        }

        private void SortDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplySorting(Items);
        }
        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            BindingOperations.ClearAllBindings(this);

            if (_ColumnHeaders != null)
            {
                foreach (var colHeader in _ColumnHeaders)
                    colHeader.Key.Click -= Header_Click;
            }

            foreach (GridViewColumn col in Columns)
            {
                BindingOperations.ClearAllBindings(col);
                col.DisplayMemberBinding = null;
            }

            Columns.Clear();
            _CurrentSortColumnHeader = null;
            _ColumnHeaders = null;

            this.Loaded -= VBTreeListView_Loaded;
            _IsColumnHeadersInitialized = false;
            _Initialized = false;
            base.DeInitVBControl(bso);
        }

        /// <summary>
        /// Gets the container for item override.
        /// </summary>
        /// <returns>The new instance of VBTreeListView.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBTreeListViewItem();
        }

        /// <summary>
        /// Determines is item overrides it's own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if item overrides, otherwise false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is VBTreeListViewItem;
        }

        /// <summary>
        /// Handles the OnItemsSourceChanged.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if(_CurrentSortColumnHeader != null)
            {
                Items.SortDescriptions.Clear();
                Path sortIcon = null;
                if (_ColumnHeaders.TryGetValue(_CurrentSortColumnHeader, out sortIcon))
                    sortIcon.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Handles the OnItemsChanged.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(_VBTimelineView != null)
                _VBTimelineView.ExpandCollapseAll = false;
            base.OnItemsChanged(e);
        }


        internal void ApplySorting(ItemCollection items)
        {
            // Set sort descriptions for the current list.
            items.SortDescriptions.Clear();

            if (SortDescriptions != null)
            {
                foreach (var sortDesc in SortDescriptions)
                {
                    items.SortDescriptions.Add(sortDesc);
                }
            }

            // Set sort description for child lists
            foreach (var item in items)
            {
                TreeViewItem tvi =
                    ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi != null)
                {
                    ApplySorting(tvi.Items);
                }
            }
        }

        /// <summary>
        /// Prepares container for the item override.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="item">The item to prepare.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            TreeViewItem tvi = element as TreeViewItem;
            if (tvi != null)
            {
                ApplySorting(tvi.Items);
            }
        }

        /// <summary>
        /// Expands all tree view items.
        /// </summary>
        /// <param name="items">The items parameter.</param>
        public static void ExpandAll(Visual items)
        {
            if (items == null)
                return;

            var frameworkElement = items as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.ApplyTemplate();
            }

            Visual child = null;
            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(items); i < count; i++)
            {
                child = VisualTreeHelper.GetChild(items, i) as Visual;

                var treeViewItem = child as TreeViewItem;
                if (treeViewItem != null)
                {
                    if (!treeViewItem.IsExpanded)
                    {
                        treeViewItem.IsExpanded = true;
                        treeViewItem.UpdateLayout();
                    }
                }
                ExpandAll(child);
            }
        }

        /// <summary>
        /// Collapses all tree view items.
        /// </summary>
        /// <param name="items">The items parameter.</param>
        public static void CollapseAll(ItemsControl items)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    CollapseAll(childControl);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if (item != null)
                    item.IsExpanded = false;
            }
        }

        // This method was replaced by TreeViewExtensions which support select of
        // top-level item as well as items path (hierarchical item).
        //public bool SetSelectedItem(object item)
        //{
        //    if (item == null) return false;

        //    VBTreeListViewItem container =
        //      (VBTreeListViewItem)ItemContainerGenerator.ContainerFromItem(item);
        //    if (container != null)
        //    {
        //        container.IsSelected = true;
        //        return true;
        //    }

        //    return false;
        //}

        #endregion
    }
}
