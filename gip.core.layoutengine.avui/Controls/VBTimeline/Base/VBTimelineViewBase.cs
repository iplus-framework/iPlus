using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace gip.core.layoutengine.avui.timeline
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTimelineViewBase'}de{'VBTimelineViewBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class VBTimelineViewBase : Control, IVBContent, IVBSource, IACObject
    {
        private ScrollViewerSyncer syncer;

        #region Loaded-Event

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        bool _IsInitialized = false;

        private StackPanel _ToolTipItems;
        /// <summary>
        /// Gets the items for ToolTip.
        /// </summary>
        public StackPanel ToolTipItems
        {
            get
            {
                if (_ToolTipItems == null)
                    _ToolTipItems = new StackPanel();
                return _ToolTipItems;
            }
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public virtual void InitVBControl()
        {
            if (_IsInitialized || string.IsNullOrEmpty(VBContent) || string.IsNullOrEmpty(VBSource) || string.IsNullOrEmpty(GanttStart) || string.IsNullOrEmpty(GanttEnd))
                return;

            //ZoomListFill();

            if (string.IsNullOrEmpty(SearchProperty))
                PART_TextBoxSearch.Visibility = System.Windows.Visibility.Collapsed;
            else
                PART_TextBoxSearch.VBContent = SearchProperty;

            if (PART_TopContentControl != null && TreeViewTopRightControl != null)
                PART_TopContentControl.Content = TreeViewTopRightControl;

            //if (!string.IsNullOrEmpty(FilterProperty) && PART_TopContentControl != null)
            //{
            //    VBComboBox comboBox = PART_TopContentControl.TryFindResource("PART_ComboBoxFilter") as VBComboBox;
            //    if (comboBox != null)
            //    {
            //        comboBox.VBContent = FilterProperty;
            //        PART_TopContentControl.Content = comboBox;
            //    }
            //}
            //    PART_ComboBoxFilter.Visibility = System.Windows.Visibility.Collapsed;
            //else
            //    PART_ComboBoxFilter.VBContent = FilterProperty;

            //PART_CheckBoxRuler.VBContent = RulerProperty;
            //PART_DateTimeRuler.VBContent = RulerDateTimeProperty;

            PART_TimelineChart.VBContent = this.VBSource;
            PART_TimelineChart.ApplyTemplate();

            PART_TreeListView.VBContent = this.VBContent;
            PART_TreeListView.Columns = TreeListViewColumns;
            PART_TreeListView.ApplyTemplate();

            ItemToolTip.ForEach(c => ToolTipItems.Children.Add(c));

            SyncScrollViewers();

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                binding.Mode = BindingMode.OneWay;
                SetBinding(ACUrlCmdMessageProperty, binding);
            }

            if (!string.IsNullOrEmpty(TimeFrameFrom) && !string.IsNullOrEmpty(TimeFrameTo))
            {
                Binding binding = new Binding();
                binding.Source = this.ContextACObject;
                binding.Path = new PropertyPath(TimeFrameFrom);
                binding.Mode = BindingMode.OneWayToSource;
                PART_TimelineChart.SetBinding(VBTimelineChartBase.TimeFrameFromProperty, binding);

                Binding binding2 = new Binding();
                binding2.Source = this.ContextACObject;
                binding2.Path = new PropertyPath(TimeFrameTo);
                binding2.Mode = BindingMode.OneWayToSource;
                PART_TimelineChart.SetBinding(VBTimelineChartBase.TimeFrameToProperty, binding2);
            }

            _IsInitialized = true;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_TreeListView = Template.FindName("PART_TreeListView", this) as VBTreeListView;
            PART_TimelineChart = Template.FindName("PART_TimelineChart", this) as VBTimelineChartBase;
            PART_CheckBoxRuler = Template.FindName("PART_CheckBoxRuler", this) as VBCheckBox;
            PART_TextBoxSearch = Template.FindName("PART_TextBoxSearch", this) as VBTextBox;
            PART_TopContentControl = Template.FindName("PART_TopContentControl", this) as ContentControl;
            //PART_ComboBoxFilter = Template.FindName("PART_ComboBoxFilter", this) as VBComboBox;
            PART_DateTimeRuler = Template.FindName("PART_DateTimeRuler", this) as VBDateTimePicker;

            InitVBControl();
        }

        #endregion

        #region PARTs
        private VBTreeListView _PART_TreeListView;
        /// <summary>
        /// Gets or sets the PART_TreeListView.
        /// </summary>
        public VBTreeListView PART_TreeListView
        {
            get
            {
                return _PART_TreeListView;
            }
            set
            {
                _PART_TreeListView = value;
            }
        }

        private VBTimelineChartBase _PART_TimelineChart;

        public VBTimelineChartBase PART_TimelineChart
        {
            get => _PART_TimelineChart;
            set => _PART_TimelineChart = value;
        }

        internal VBCheckBox PART_CheckBoxRuler;
        internal VBTextBox PART_TextBoxSearch;
        internal ContentControl PART_TopContentControl;
        internal VBDateTimePicker PART_DateTimeRuler;

        #endregion

        #region DependncyProp

        /// <summary>
        /// Represents the dependency property for GanttStart.
        /// </summary>
        public static readonly DependencyProperty GanttStartProperty
            = DependencyProperty.Register("GanttStart", typeof(string), typeof(VBTimelineViewBase));

        /// <summary>
        /// Gets or sets the start date and time for Gantt chart.
        /// </summary>
        [Category("VBControl")]
        public string GanttStart
        {
            get { return (string)GetValue(GanttStartProperty); }
            set { SetValue(GanttStartProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for GanttEnd.
        /// </summary>
        public static readonly DependencyProperty GanttEndProperty
            = DependencyProperty.Register("GanttEnd", typeof(string), typeof(VBTimelineViewBase));

        /// <summary>
        /// Gets or sets the end date and time for Gantt chart.
        /// </summary>
        [Category("VBControl")]
        public string GanttEnd
        {
            get { return (string)GetValue(GanttEndProperty); }
            set { SetValue(GanttEndProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ItemToolTip.
        /// </summary>
        public static readonly DependencyProperty ItemToolTipProperty
            = DependencyProperty.RegisterAttached("ItemToolTip", typeof(List<FrameworkElement>), typeof(VBTimelineViewBase));

        /// <summary>
        /// Gets or sets the items(controls) which will be placed in ToolTip.
        /// </summary>
        public List<FrameworkElement> ItemToolTip
        {
            get { return (List<FrameworkElement>)GetValue(ItemToolTipProperty); }
            set { SetValue(ItemToolTipProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for TreeListViewColumns.
        /// </summary>
        [Category("VBControl")]
        public static readonly DependencyProperty TreeListViewColumnsProperty
            = DependencyProperty.Register("TreeListViewColumns", typeof(GridViewColumnCollection), typeof(VBTimelineViewBase));

        /// <summary>
        /// Gets or sets the columns which will be shown in TreeListView.
        /// </summary>
        [Category("VBControl")]
        public GridViewColumnCollection TreeListViewColumns
        {
            get { return (GridViewColumnCollection)GetValue(TreeListViewColumnsProperty); }
            set { SetValue(TreeListViewColumnsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for SearchProperty.
        /// </summary>
        public static readonly DependencyProperty SearchPropertyProperty
            = DependencyProperty.Register("SearchProperty", typeof(string), typeof(VBTimelineViewBase));

        /// <summary>
        /// Gets or sets the name of property which is responsible for a search keyword.
        /// </summary>
        [Category("VBControl")]
        public string SearchProperty
        {
            get { return (string)GetValue(SearchPropertyProperty); }
            set { SetValue(SearchPropertyProperty, value); }
        }


        [Category("VBControl")]
        public FrameworkElement TreeViewTopRightControl
        {
            get { return (FrameworkElement)GetValue(TreeViewTopRightControlProperty); }
            set { SetValue(TreeViewTopRightControlProperty, value); }
        }

        public static readonly DependencyProperty TreeViewTopRightControlProperty =
            DependencyProperty.Register("TreeViewTopRightControl", typeof(FrameworkElement), typeof(VBTimelineChartBase), new PropertyMetadata(null));


        ///// <summary>
        ///// Represents the dependency property for RulerProperty.
        ///// </summary>
        //public static readonly DependencyProperty RulerPropertyProperty
        //    = DependencyProperty.Register("RulerProperty", typeof(string), typeof(VBTimelineViewBase));

        ///// <summary>
        ///// Gets or sets the name of property which is responsible for a ruler visibility.
        ///// </summary>
        //[Category("VBControl")]
        //public string RulerProperty
        //{
        //    get { return (string)GetValue(RulerPropertyProperty); }
        //    set { SetValue(RulerPropertyProperty, value); }
        //}

        ///// <summary>
        ///// Represents the dependency property for RulerDateTimeProperty.
        ///// </summary>
        //public static readonly DependencyProperty RulerDateTimePropertyProperty
        //    = DependencyProperty.Register("RulerDateTimeProperty", typeof(string), typeof(VBTimelineViewBase));

        ///// <summary>
        ///// Gets or sets the name of property which is responsible for date and time on ruler.
        ///// </summary>
        //[Category("VBControl")]
        //public string RulerDateTimeProperty
        //{
        //    get { return (string)GetValue(RulerDateTimePropertyProperty); }
        //    set { SetValue(RulerDateTimePropertyProperty, value); }
        //}

        ///// <summary>
        ///// Represents the dependency property for FilterProperty.
        ///// </summary>
        //public static readonly DependencyProperty FilterPropertyProperty
        //    = DependencyProperty.Register("FilterProperty", typeof(string), typeof(VBTimelineViewBase));

        ///// <summary>
        ///// Gets or sets the name of property which is responsible for filter.
        ///// </summary>
        //[Category("VBControl")]
        //public string FilterProperty
        //{
        //    get { return (string)GetValue(FilterPropertyProperty); }
        //    set { SetValue(FilterPropertyProperty, value); }
        //}

        /// <summary>
        /// Represents the dependency property for ExpandCollapseAll.
        /// </summary>
        public static readonly DependencyProperty ExpandCollapseAllProperty =
            DependencyProperty.Register("ExpandCollapseAll", typeof(bool), typeof(VBTimelineViewBase), new PropertyMetadata(new PropertyChangedCallback(OnExpandCollapseAll)));

        /// <summary>
        /// Gets or sets expand or collapse all items in tree view.
        /// </summary>
        public bool ExpandCollapseAll
        {
            get { return (bool)GetValue(ExpandCollapseAllProperty); }
            set { SetValue(ExpandCollapseAllProperty, value); }
        }

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ACUrlCmdMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage", typeof(ACUrlCmdMessage), typeof(VBTimelineViewBase), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTimelineViewBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineViewBase thisControl = d as VBTimelineViewBase;
            if(thisControl != null)
            {
                if(e.Property == ACUrlCmdMessageProperty)
                {
                    ACUrlCmdMessage msg = e.NewValue as ACUrlCmdMessage;
                    if(msg != null && msg.ACUrl == "!ScrollToValue" && msg.ACParameter.Any() && msg.ACParameter[0] is DateTime)
                    {
                        DateTime dt = (DateTime)msg.ACParameter[0];
                        thisControl.ScrollToValue(dt);
                    }
                }
            }
        }

        public DataTemplate TimelineItemTemplate
        {
            get { return (DataTemplate)GetValue(TimelineItemTemplateProperty); }
            set { SetValue(TimelineItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimelineItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineItemTemplateProperty =
            DependencyProperty.RegisterAttached("TimelineItemTemplate", typeof(DataTemplate), typeof(VBTimelineViewBase));


        public double TimelineItemHeight
        {
            get { return (double)GetValue(TimelineItemHeightProperty); }
            set { SetValue(TimelineItemHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineItemHeightProperty =
            DependencyProperty.Register("TimelineItemHeight", typeof(double), typeof(VBTimelineViewBase), new PropertyMetadata(18D));


        public double TimelineItemVerticalMargin
        {
            get { return (double)GetValue(TimelineItemVerticalMarginProperty); }
            set { SetValue(TimelineItemVerticalMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowVerticalMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineItemVerticalMarginProperty =
            DependencyProperty.Register("TimelineItemVerticalMargin", typeof(double), typeof(VBTimelineViewBase), new PropertyMetadata(4D));

        /// <summary>
        /// Returns the sum of TimelineItemHeight and TimelineItemVerticalMargin (TimelineItemHeight + TimelineItemVerticalMargin)
        /// </summary>
        public double TimelineItemTotalHeight
        {
            get => TimelineItemHeight + TimelineItemVerticalMargin;
        }

        public GridLength TreeListViewWidth
        {
            get { return (GridLength)GetValue(TreeListViewWidthProperty); }
            set { SetValue(TreeListViewWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TreeListViewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TreeListViewWidthProperty =
            DependencyProperty.Register("TreeListViewWidth", typeof(GridLength), typeof(VBTimelineViewBase), new PropertyMetadata(new GridLength(1.5, GridUnitType.Star)));



        public GridLength TimelineViewWidth
        {
            get { return (GridLength)GetValue(TimelineViewWidthProperty); }
            set { SetValue(TimelineViewWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimelineViewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineViewWidthProperty =
            DependencyProperty.Register("TimelineViewWidth", typeof(GridLength), typeof(VBTimelineViewBase), new PropertyMetadata(new GridLength(3, GridUnitType.Star)));


        #endregion

        #region Properties

        public string BSOUpdateDisplayOrderMethodName
        {
            get;
            set;
        }

        public string TimeFrameFrom
        {
            get;
            set;
        }

        public string TimeFrameTo
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Expands or collapses all items in tree view.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public static void OnExpandCollapseAll(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineViewBase thisControl = o as VBTimelineViewBase;
            if ((bool)e.NewValue)
            {
                // TODO Avalonia:
                using (new WaitCursor())
                {
                    VBTreeListView.ExpandAll(thisControl.PART_TreeListView);
                }
            }
            else
                VBTreeListView.CollapseAll(thisControl.PART_TreeListView);
        }

        private void SyncScrollViewers()
        {
            PART_TreeListView.ApplyTemplate();
            PART_TimelineChart.ApplyTemplate();
            ScrollViewer treeSV = WpfUtility.FindVisualChild<ScrollViewer>(PART_TreeListView);
            ScrollViewer timelineSV = WpfUtility.FindVisualChild<ScrollViewer>(PART_TimelineChart);

            if (treeSV != null && timelineSV != null)
            {
                syncer = new ScrollViewerSyncer(treeSV, timelineSV);
            }
        }

        [ACMethodInfo("","",999)]
        public void ScrollToValue(DateTime valueDateTime)
        {
            PART_TimelineChart.GoToDate(valueDateTime);
        }

        #endregion

        #region IVBContent members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
          = DependencyProperty.Register("VBContent", typeof(string), typeof(VBGanttChartView));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.(Not implemented.)
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get { return Global.ControlModes.Enabled; }
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"(Not implemented.)
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        public string DisabledModes
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            TreeListViewColumns.Clear();
            this.ClearAllBindings();
            syncer?.DeInitControl();
            _IsInitialized = false;
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get { return DataContext as IACObject; }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return Name; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return ACIdentifier;
            }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion

        #region IVBSource members

        /// <summary>
        /// Represents the dependency property for VBSource.
        /// </summary>
        public static readonly DependencyProperty VBSourceProperty
           = DependencyProperty.Register("VBSource", typeof(string), typeof(VBGanttChartView));

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get { return (string)GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBShowColumns.
        /// </summary>
        public static readonly DependencyProperty VBShowColumnsProperty
            = DependencyProperty.Register("VBShowColumns", typeof(string), typeof(VBGanttChartView));

        /// <summary>
        /// Gets or sets the VBShowColumns.
        /// </summary>
        public string VBShowColumns
        {
            get { return (string)GetValue(VBShowColumnsProperty); }
            set { SetValue(VBShowColumnsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the VBDisabledColumns.
        /// </summary>
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// (Not implemented.)
        /// </summary>
        public string VBChilds
        {
            get
            {
                return "";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// (Not implemented.)
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
