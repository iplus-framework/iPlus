using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Controls.Templates;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui.timeline
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTimelineViewBase'}de{'VBTimelineViewBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class VBTimelineViewBase : TemplatedControl, IVBContent, IVBSource, IACObject
    {
        private ScrollViewerSyncer _Syncer;

        #region Loaded-Event

        protected override void OnInitialized()
        {
            base.OnInitialized();
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
                PART_TextBoxSearch.IsVisible = false;
            else
                PART_TextBoxSearch.VBContent = SearchProperty;

            if (PART_TopContentControl != null && TreeViewTopRightControl != null)
                PART_TopContentControl.Content = TreeViewTopRightControl;

            PART_TimelineChart.VBContent = this.VBSource;
            PART_TimelineChart.ApplyTemplate();

            PART_TreeListView.VBContent = this.VBContent;
            PART_TreeListView.Columns = TreeListViewColumns;
            PART_TreeListView.ApplyTemplate();

            ItemToolTip?.ForEach(c => ToolTipItems.Children.Add(c));

            SyncScrollViewers();

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.ACUrlCmdMessage;
                binding.Mode = BindingMode.OneWay;
                this.Bind(ACUrlCmdMessageProperty, binding);
            }

            if (!string.IsNullOrEmpty(TimeFrameFrom) && !string.IsNullOrEmpty(TimeFrameTo))
            {
                Binding binding = new Binding();
                binding.Source = this.ContextACObject;
                binding.Path = TimeFrameFrom;
                binding.Mode = BindingMode.OneWayToSource;
                PART_TimelineChart.Bind(VBTimelineChartBase.TimeFrameFromProperty, binding);

                Binding binding2 = new Binding();
                binding2.Source = this.ContextACObject;
                binding2.Path = TimeFrameTo;
                binding2.Mode = BindingMode.OneWayToSource;
                PART_TimelineChart.Bind(VBTimelineChartBase.TimeFrameToProperty, binding2);
            }

            _IsInitialized = true;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_TreeListView = e.NameScope.Find("PART_TreeListView") as VBTreeListView;
            PART_TimelineChart = e.NameScope.Find("PART_TimelineChart") as VBTimelineChartBase;
            PART_CheckBoxRuler = e.NameScope.Find("PART_CheckBoxRuler") as VBCheckBox;
            PART_TextBoxSearch = e.NameScope.Find("PART_TextBoxSearch") as VBTextBox;
            PART_TopContentControl = e.NameScope.Find("PART_TopContentControl") as ContentControl;
            PART_DateTimeRuler = e.NameScope.Find("PART_DateTimeRuler") as VBDateTimePicker;

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

        #region StyledProperties

        /// <summary>
        /// Represents the styled property for GanttStart.
        /// </summary>
        public static readonly StyledProperty<string> GanttStartProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(GanttStart));

        /// <summary>
        /// Gets or sets the start date and time for Gantt chart.
        /// </summary>
        [Category("VBControl")]
        public string GanttStart
        {
            get { return GetValue(GanttStartProperty); }
            set { SetValue(GanttStartProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for GanttEnd.
        /// </summary>
        public static readonly StyledProperty<string> GanttEndProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(GanttEnd));

        /// <summary>
        /// Gets or sets the end date and time for Gantt chart.
        /// </summary>
        [Category("VBControl")]
        public string GanttEnd
        {
            get { return GetValue(GanttEndProperty); }
            set { SetValue(GanttEndProperty, value); }
        }

        /// <summary>
        /// Represents the attached property for ItemToolTip.
        /// </summary>
        public static readonly AttachedProperty<List<Control>> ItemToolTipProperty =
            AvaloniaProperty.RegisterAttached<VBTimelineViewBase, Control, List<Control>>("ItemToolTip");

        /// <summary>
        /// Gets or sets the items(controls) which will be placed in ToolTip.
        /// </summary>
        public List<Control> ItemToolTip
        {
            get { return GetValue(ItemToolTipProperty); }
            set { SetValue(ItemToolTipProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for TreeListViewColumns.
        /// </summary>
        [Category("VBControl")]
        public static readonly StyledProperty<GridViewColumnCollection> TreeListViewColumnsProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, GridViewColumnCollection>(nameof(TreeListViewColumns));

        /// <summary>
        /// Gets or sets the columns which will be shown in TreeListView.
        /// </summary>
        [Category("VBControl")]
        public GridViewColumnCollection TreeListViewColumns
        {
            get { return GetValue(TreeListViewColumnsProperty); }
            set { SetValue(TreeListViewColumnsProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for SearchProperty.
        /// </summary>
        public static readonly StyledProperty<string> SearchPropertyProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(SearchProperty));

        /// <summary>
        /// Gets or sets the name of property which is responsible for a search keyword.
        /// </summary>
        [Category("VBControl")]
        public string SearchProperty
        {
            get { return GetValue(SearchPropertyProperty); }
            set { SetValue(SearchPropertyProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for TreeViewTopRightControl.
        /// </summary>
        public static readonly StyledProperty<Control> TreeViewTopRightControlProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, Control>(nameof(TreeViewTopRightControl));

        [Category("VBControl")]
        public Control TreeViewTopRightControl
        {
            get { return GetValue(TreeViewTopRightControlProperty); }
            set { SetValue(TreeViewTopRightControlProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ExpandCollapseAll.
        /// </summary>
        public static readonly StyledProperty<bool> ExpandCollapseAllProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, bool>(nameof(ExpandCollapseAll));

        /// <summary>
        /// Gets or sets expand or collapse all items in tree view.
        /// </summary>
        public bool ExpandCollapseAll
        {
            get { return GetValue(ExpandCollapseAllProperty); }
            set { SetValue(ExpandCollapseAllProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTimelineViewBase>();
        
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ACUrlCmdMessageProperty)
            {
                ACUrlCmdMessage msg = change.NewValue as ACUrlCmdMessage;
                if (msg != null && msg.ACUrl == "!ScrollToValue" && msg.ACParameter.Any() && msg.ACParameter[0] is DateTime)
                {
                    DateTime dt = (DateTime)msg.ACParameter[0];
                    ScrollToValue(dt);
                }
            }
            else if (change.Property == ExpandCollapseAllProperty)
            {
                if ((bool)change.NewValue)
                {
                    // TODO Avalonia:
                    using (new WaitCursor())
                    {
                        VBTreeListView.ExpandAll(PART_TreeListView);
                    }
                }
                else
                    VBTreeListView.CollapseAll(PART_TreeListView);
            }
            base.OnPropertyChanged(change);
        }

        /// <summary>
        /// Represents the styled property for TimelineItemTemplate.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> TimelineItemTemplateProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, IDataTemplate>(nameof(TimelineItemTemplate));

        public IDataTemplate TimelineItemTemplate
        {
            get { return GetValue(TimelineItemTemplateProperty); }
            set { SetValue(TimelineItemTemplateProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for TimelineItemHeight.
        /// </summary>
        public static readonly StyledProperty<double> TimelineItemHeightProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, double>(nameof(TimelineItemHeight), defaultValue: 18.0);

        public double TimelineItemHeight
        {
            get { return GetValue(TimelineItemHeightProperty); }
            set { SetValue(TimelineItemHeightProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for TimelineItemVerticalMargin.
        /// </summary>
        public static readonly StyledProperty<double> TimelineItemVerticalMarginProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, double>(nameof(TimelineItemVerticalMargin), defaultValue: 4.0);

        public double TimelineItemVerticalMargin
        {
            get { return GetValue(TimelineItemVerticalMarginProperty); }
            set { SetValue(TimelineItemVerticalMarginProperty, value); }
        }

        /// <summary>
        /// Returns the sum of TimelineItemHeight and TimelineItemVerticalMargin (TimelineItemHeight + TimelineItemVerticalMargin)
        /// </summary>
        public double TimelineItemTotalHeight
        {
            get => TimelineItemHeight + TimelineItemVerticalMargin;
        }

        /// <summary>
        /// Represents the styled property for TreeListViewWidth.
        /// </summary>
        public static readonly StyledProperty<GridLength> TreeListViewWidthProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, GridLength>(nameof(TreeListViewWidth), 
                defaultValue: new GridLength(1.5, GridUnitType.Star));

        public GridLength TreeListViewWidth
        {
            get { return GetValue(TreeListViewWidthProperty); }
            set { SetValue(TreeListViewWidthProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for TimelineViewWidth.
        /// </summary>
        public static readonly StyledProperty<GridLength> TimelineViewWidthProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, GridLength>(nameof(TimelineViewWidth), 
                defaultValue: new GridLength(3, GridUnitType.Star));

        public GridLength TimelineViewWidth
        {
            get { return GetValue(TimelineViewWidthProperty); }
            set { SetValue(TimelineViewWidthProperty, value); }
        }

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


        private void SyncScrollViewers()
        {
            PART_TreeListView.ApplyTemplate();
            PART_TimelineChart.ApplyTemplate();
            ScrollViewer treeSV = VBVisualTreeHelper.FindChildObjects<ScrollViewer>(PART_TreeListView)?.FirstOrDefault();
            ScrollViewer timelineSV = VBVisualTreeHelper.FindChildObjects<ScrollViewer>(PART_TimelineChart)?.FirstOrDefault();
            if (treeSV != null && timelineSV != null)
            {
                _Syncer = new ScrollViewerSyncer(treeSV, timelineSV);
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
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
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
            TreeListViewColumns?.GetType().GetMethod("Clear")?.Invoke(TreeListViewColumns, null);
            this.ClearAllBindings();
            _Syncer?.DeInitControl();
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
        /// Represents the styled property for VBSource.
        /// </summary>
        public static readonly StyledProperty<string> VBSourceProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(VBSource));

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get { return GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for VBShowColumns.
        /// </summary>
        public static readonly StyledProperty<string> VBShowColumnsProperty =
            AvaloniaProperty.Register<VBTimelineViewBase, string>(nameof(VBShowColumns));

        /// <summary>
        /// Gets or sets the VBShowColumns.
        /// </summary>
        public string VBShowColumns
        {
            get { return GetValue(VBShowColumnsProperty); }
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
