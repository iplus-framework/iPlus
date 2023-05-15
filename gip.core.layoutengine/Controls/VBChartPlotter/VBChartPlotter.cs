using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using gip.ext.chart;
using gip.ext.chart.Common.Auxiliary;
using gip.core.datamodel;
using System.Transactions;
using gip.ext.chart.DataSources;
using gip.ext.chart.Charts;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a plotter control which shows chart.
    /// </summary>
    /// <summary>
    /// Steuerelement zum plotten.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBChartPlotter'}de{'VBChartPlotter'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBChartPlotter : ChartPlotter, IACInteractiveObject, IACObject, IVBChart, IVBSource
    {
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ChartPlotterStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBChartPlotter/Themes/ChartPlotterStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ChartPlotterStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBChartPlotter/Themes/ChartPlotterStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBChartPlotter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBChartPlotter), new FrameworkPropertyMetadata(typeof(VBChartPlotter)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBChartPlotter.
        /// </summary>
        public VBChartPlotter()
        {
            PropertyLogItems = new ObservableCollection<IVBChartItem>();
        }

        /// <summary>
        /// Updates UI parts.
        /// </summary>
        protected override void UpdateUIParts()
        {
            ResourceDictionary dict = new ResourceDictionary
            {
                Source = new Uri(StyleInfoList[0].styleUri, UriKind.Relative)
            };

            Style = (Style)dict[StyleInfoList[0].styleName];

            //ControlTemplate template = (ControlTemplate)dict[Plotter.templateKey];
            //Template = template;
            ApplyTemplate();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            this.Loaded += VBChartPlotter_Loaded;
            this.Unloaded += VBChartPlotter_Unloaded;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        VBPropertyLogChart _VBPropertyLogChart = null;

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        /// <summary>
        /// This method initialize chart and add horizontal date time axis.
        /// </summary>
        private void InitVBControl()
        {
            if (_Initialized)
                return;
            _Initialized = true;
            _VBPropertyLogChart = FindName("ucChart") as VBPropertyLogChart;

            if (ContextACObject != null)
            {
                // Falls Nutzung per Dependency-Property (ACComponent gibt an was auf dem Plotter dargestellt werden soll) 
                if (!String.IsNullOrEmpty(VBContent))
                {
                    // Seal Property
                    if (!_DisplayAsArchive.HasValue)
                        DisplayAsArchive = false;

                    Binding binding = new Binding();
                    binding.Source = ContextACObject;
                    binding.Path = new PropertyPath(VBContent);
                    binding.Mode = BindingMode.OneWay;
                    binding.NotifyOnSourceUpdated = true;
                    binding.NotifyOnTargetUpdated = true;
                    this.SetBinding(VBChartPlotter.BindableChartItemsProperty, binding);
                }
                else if (!DisplayAsArchive)
                    RefreshPlotterWithStaticItems();
            }

        }

        bool _Loaded = false;
        void VBChartPlotter_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;
            _Loaded = true;
        }

        void VBChartPlotter_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;
            _Loaded = false;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;

            if (PropertyLogItems != null)
            {
                foreach (IVBChartItem chartItem in PropertyLogItems)
                {
                    IPropertyLogChartItem logChartItem = chartItem as IPropertyLogChartItem;
                    if (logChartItem != null && !DisplayAsArchive)
                    {
                        if ((chartItem.ACProperty != null) && (chartItem.ACProperty.LiveLog != null))
                            chartItem.ACProperty.LiveLog.PropertyChanged -= LiveLog_PropertyChanged;
                    }
                    if (chartItem.ACProperty != null)
                        chartItem.DeInitVBControl(bso);
                }
            }
            BindingOperations.ClearBinding(this, VBChartPlotter.BindableChartItemsProperty);
            BindingOperations.ClearAllBindings(this);

            this.Loaded -= VBChartPlotter_Loaded;
            this.Unloaded -= VBChartPlotter_Unloaded;
        }


        /// <summary>
        /// Clear lines in graph.
        /// </summary>
        public void RemoveUserLineGraphElements()
        {
            List<IPlotterElement> items = Children.ToList();
            var query = items.Where(c => c is LineGraph);
            if (query.Any())
            {
                foreach (IPlotterElement item in query)
                {
                    Children.Remove(item);
                }
            }
        }

        #region IACInteractiveObject Member
        public IACType VBContentValueType
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBChartPlotter));

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
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBChartPlotter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
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
        #endregion

        #region Screenshots & copy to clipboard

        public static readonly DependencyProperty InPrintingModeProperty
            = DependencyProperty.Register("InPrintingMode", typeof(bool), typeof(VBChartPlotter));

        [Category("VBControl")]
        public bool InPrintingMode
        {
            get
            {
                return (bool)GetValue(InPrintingModeProperty);
            }
            set
            {
                SetValue(InPrintingModeProperty, value);
            }
        }

        public override BitmapSource CreateScreenshot()
        {
            BitmapSource result = base.CreateScreenshot();
            return result;
        }

        public virtual BitmapSource CreatePrintableBitmap()
        {
            UIElement parent = (UIElement)Parent;

            Rect renderBounds = new Rect(RenderSize);

            Point p1 = renderBounds.TopLeft;
            Point p2 = renderBounds.BottomRight;

            if (parent != null)
            {
                p1 = TranslatePoint(p1, parent);
                p2 = TranslatePoint(p2, parent);
            }

            Int32Rect rect = new Rect(p1, p2).ToInt32Rect();

            return ScreenshotHelper.CreatePrintableBitmap(this, rect, 300);
        }

        /// <summary>Saves screenshot to file.</summary>
        /// <param name="filePath">File path.</param>
        public override void SaveScreenshot(string filePath)
        {
            base.SaveScreenshot(filePath);
        }

        /// <summary>
        /// Saves screenshot to stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fileExtension">The file type extension.</param>
        public override void SaveScreenshotToStream(Stream stream, string fileExtension)
        {
            base.SaveScreenshotToStream(stream, fileExtension);
        }

        /// <summary>Copies the screenshot to clipboard.</summary>
        public override void CopyScreenshotToClipboard()
        {
            base.CopyScreenshotToClipboard();
        }

        #endregion

        #region IACObject
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
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
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
        #endregion

        #region IACObject Member

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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name;  }
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

        #endregion

        public static readonly DependencyProperty BindableChartItemsProperty
            = DependencyProperty.Register("BindableChartItems", typeof(IEnumerable<IVBChartItem>), typeof(VBChartPlotter), new PropertyMetadata(new PropertyChangedCallback(BindableChartItems_Changed)));

        /// <summary>
        /// PropertyLogs which shoud be displayed
        /// DisplayMode must be set to PropertyLog
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public IEnumerable<IVBChartItem> BindableChartItems
        {
            get
            {
                return (IEnumerable<IVBChartItem>)GetValue(BindableChartItemsProperty);
            }
            set
            {
                SetValue(BindableChartItemsProperty, value);
            }
        }

        private static void BindableChartItems_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBChartPlotter)
            {
                VBChartPlotter vbChartPlotter = d as VBChartPlotter;
                vbChartPlotter.RefreshPlotterWithBindableItems();
            }
        }

        public static readonly DependencyProperty DisplayModeProperty
            = DependencyProperty.Register("DisplayMode", typeof(VBChartItemDisplayMode), typeof(VBChartPlotter), new PropertyMetadata(VBChartItemDisplayMode.PropertyLog));

        /// <summary>
        /// Defines if PropertyLogItems or BindableChartSeries are used for displaying the chart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public VBChartItemDisplayMode DisplayMode
        {
            get { return (VBChartItemDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }



        [Category("VBControl")]
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        private ObservableCollection<IVBChartItem> _PropertyLogItems;
        public ObservableCollection<IVBChartItem> PropertyLogItems
        {
            get
            {
                return _PropertyLogItems;
            }
            set
            {
                _PropertyLogItems = value;
            }
        }

        //private bool _InternalCollectionLock = false;
        private Nullable<bool> _DisplayAsArchive;
        public bool DisplayAsArchive
        {
            get
            {
                if (_DisplayAsArchive.HasValue)
                    return _DisplayAsArchive.Value;
                return false;
            }
            set
            {
                if (_DisplayAsArchive.HasValue)
                    return;
                _DisplayAsArchive = value;
            }
        }

        private void RefreshPlotterWithStaticItems()
        {
            // Sonst Nutzung des Controls per XAML-Deklaration
            if ((PropertyLogItems.Count > 0) || !String.IsNullOrEmpty(VBShowColumns))
            {
                RemoveUserLineGraphElements();
                // Seal Property
                if (!_DisplayAsArchive.HasValue)
                    DisplayAsArchive = false;

                if (PropertyLogItems.Count > 0)
                {
                    foreach (VBChartItem chartItem in PropertyLogItems)
                    {
                        chartItem.SetVBBindings(this.DataContext, this.BSOACComponent);
                        chartItem.InitVBControl(ContextACObject);
                        try
                        {
                            if (BSOACComponent != null)
                                BSOACComponent.AddWPFRef(chartItem.GetHashCode(), chartItem.ACProperty);
                        }
                        catch (Exception exw)
                        {
                            this.Root().Messages.LogDebug("VBCharPlotter", "RemoveWPFRef", exw.Message);
                        }
                    }
                }
                else
                {
                    //_InternalCollectionLock = true;
                    string[] acURLs = VBShowColumns.Split(',');
                    foreach (string acURL in acURLs)
                    {
                        VBChartItem logChartItem = DisplayMode == VBChartItemDisplayMode.PropertyLog ? new VBPropertyLogChartItem() : new VBChartItem();
                        logChartItem.VBContent = acURL;
                        logChartItem.SetVBBindings(this.DataContext, this.BSOACComponent);
                        logChartItem.InitVBControl(ContextACObject);
                        PropertyLogItems.Add(logChartItem);
                    }
                    //_InternalCollectionLock = false;
                }

                if (!DisplayAsArchive)
                {
                    foreach (IVBChartItem chartItem in PropertyLogItems)
                    {
                        if (chartItem is IPropertyLogChartItem && chartItem.ACProperty != null && chartItem.ACProperty.LiveLog != null)
                            chartItem.ACProperty.LiveLog.PropertyChanged += LiveLog_PropertyChanged;
                    }
                }

                PlotLogs(PropertyLogItems);
                if (_VBPropertyLogChart != null)
                    _VBPropertyLogChart.RefreshLiveConsole();
            }
        }

        private void RefreshPlotterWithBindableItems()
        {
            foreach (IVBChartItem chartItem in PropertyLogItems)
            {
                if (BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(chartItem.GetHashCode());
                IPropertyLogChartItem logChartItem = chartItem as IPropertyLogChartItem;
                if (logChartItem != null && !DisplayAsArchive)
                {
                    if ((chartItem.ACProperty != null) && (chartItem.ACProperty.LiveLog != null))
                        chartItem.ACProperty.LiveLog.PropertyChanged -= LiveLog_PropertyChanged;
                }
            }

            this.RemoveUserLineGraphElements();

            int countLogs = BindableChartItems.Count();
            if (countLogs <= 0)
                return;
            //_InternalCollectionLock = true;
            foreach (IVBChartItem chartItem in BindableChartItems)
            {
                chartItem.InitVBControl(ContextACObject);
                try
                {
                    if (BSOACComponent != null)
                        BSOACComponent.AddWPFRef(chartItem.GetHashCode(), chartItem.ACProperty);
                    PropertyLogItems.Add(chartItem);
                    IPropertyLogChartItem logChartItem = chartItem as IPropertyLogChartItem;
                    if (logChartItem != null && !DisplayAsArchive)
                    {
                        if (chartItem.ACProperty != null && chartItem.ACProperty.LiveLog != null)
                            chartItem.ACProperty.LiveLog.PropertyChanged += new PropertyChangedEventHandler(LiveLog_PropertyChanged);
                    }
                }
                catch (Exception exw)
                {
                    this.Root().Messages.LogDebug("VBCharPlotter", "RemoveWPFRef", exw.Message);
                }
            }
            //_InternalCollectionLock = false;
            PlotLogs(PropertyLogItems);
        }

        private void PlotLogs(IEnumerable<IVBChartItem> chartItems)
        {
            if (chartItems == null)
                return;

            if (chartItems.Where(c => c is IPropertyLogChartItem).Any() && !(this.HorizontalAxis is HorizontalDateTimeAxis))
            {
                HorizontalDateTimeAxis horizontalDTAxis = new HorizontalDateTimeAxis();
                horizontalDTAxis.Name = "X-Axis";
                this.HorizontalAxis = horizontalDTAxis;
            }

            // TODO: Start/Stop: Logging Live-Wert
            int countLogs = chartItems.Count();
            if (countLogs <= 0)
                return;
            foreach (IVBChartItem chartItem in chartItems)
            {
                if (chartItem.ACProperty == null)
                    continue;

                IPropertyLogChartItem logChartItem = chartItem as IPropertyLogChartItem;
                if (logChartItem != null)
                {
                    PropertyLogListInfo logListInfo = logChartItem.GetLiveLogList();
                    IEnumerable<PropertyLogItem> propertyLog = logListInfo.PropertyLogList;

                    if (propertyLog == null)
                        continue;
                    EnumerableDataSource<PropertyLogItem> ds = CreateChartDataSource(propertyLog);
                    this.AddLineGraph(ds, chartItem.GetLineColor(), chartItem.LineThickness, chartItem.ACCaption);
                }
                else
                {
                    IEnumerable<IVBChartTuple> chartSeries = chartItem.DataSeries;
                    if (chartSeries != null)
                    {
                        EnumerableDataSource<IVBChartTuple> ds = CreateChartDataSource(chartSeries, chartItem.DisplayMode);
                        this.AddLineGraph(ds, chartItem.GetLineColor(), chartItem.LineThickness, chartItem.ACCaption);
                    }
                }
            }
        }

        private void PlotLogs(IEnumerable<IVBChartItem> chartItems, DateTime from, DateTime to, Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay = null)
        {
            if (chartItems == null)
                return;

            _Initialized = true;

            // TODO: Start/Stop: Logging Live-Wert
            int countLogs = chartItems.Count();
            if (countLogs <= 0)
                return;
            foreach (IVBChartItem chartItem in chartItems)
            {
                VBPropertyLogChartItem propertylogitem = chartItem as VBPropertyLogChartItem;
                if (propertylogitem == null)
                    continue;
                if (propertylogitem != null)
                {
                    propertylogitem.SetVBBindings(this.DataContext, this.BSOACComponent);
                }
                chartItem.InitVBControl(ContextACObject);

                if (chartItem.ACProperty == null)
                    continue;
                PropertyLogListInfo logListInfo = null;
                // ArchiveLog
                if (DisplayAsArchive)
                    logListInfo = propertylogitem.GetArchiveLogList(from, to);
                // LiveLog
                else
                    logListInfo = propertylogitem.GetLiveLogList();
                if (logListInfo == null)
                    continue;
                if (interpolation != Global.InterpolationMethod.None && logListInfo != null)
                {
                    logListInfo.SetInterpolationParams(interpolation, range, decay);
                    logListInfo.Interpolate();
                }
                IEnumerable<PropertyLogItem> propertyLog = logListInfo.PropertyLogList;
                if (propertyLog == null)
                    continue;
                EnumerableDataSource<PropertyLogItem> ds = CreateChartDataSource(propertyLog);
                this.AddLineGraph(ds, chartItem.GetLineColor(), chartItem.LineThickness, chartItem.ACCaption);
            }
        }

        /// <summary>
        /// Initialize and fill chart when is in archive mode.
        /// </summary>
        /// <param name="from">Show archive from date.</param>
        /// <param name="to">Show archive to date.</param>
        /// <param name="interpolation">The interpolation method.</param>
        /// <param name="range">The range parameter.</param>
        /// <param name="decay">The decay parameter.</param>
        public void InitializeChartArchive(DateTime from, DateTime to, Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay = null)
        {
            RemoveUserLineGraphElements();
            PlotLogs(PropertyLogItems, from, to, interpolation, range, decay);
        }

        private EnumerableDataSource<PropertyLogItem> CreateChartDataSource(IEnumerable<PropertyLogItem> propertyLog)
        {
            if (propertyLog == null)
                return null;
            var ds = new EnumerableDataSource<PropertyLogItem>(propertyLog);
            ds.SetXMapping(pi => ((HorizontalDateTimeAxis)HorizontalAxis).ConvertToDouble(pi.Time));
            ds.SetYMapping(pi => Convert.ToDouble(pi.Value));
            return ds;
        }

        private EnumerableDataSource<IVBChartTuple> CreateChartDataSource(IEnumerable<IVBChartTuple> chartSeries, VBChartItemDisplayMode displayMode)
        {
            if (chartSeries == null)
                return null;
            var ds = new EnumerableDataSource<IVBChartTuple>(chartSeries);
            if (DisplayMode == VBChartItemDisplayMode.MapTupleValue2ToX)
            {
                ds.SetXMapping(pi => Convert.ToDouble(pi.Value2));
                ds.SetYMapping(pi => Convert.ToDouble(pi.Value1));
            }
            else
            {
                ds.SetXMapping(pi => Convert.ToDouble(pi.Value1));
                ds.SetYMapping(pi => Convert.ToDouble(pi.Value2));
            }
            return ds;
        }

        void LiveLog_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogState")
                if (_VBPropertyLogChart != null)
                    _VBPropertyLogChart.RefreshLiveConsole();
        }

        /// <summary>
        /// Switch chart to normal mode or to printing mode.
        /// </summary>
        public void SwitchNormalPrintingMode()
        {
            InPrintingMode = !InPrintingMode;
        }

        #region IVBContent members
        IACType _VBContentPropertyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { return _VBContentPropertyInfo as ACClassProperty; }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }
        #endregion

        #region IVBSource members

        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        public string VBDisabledColumns
        {
            get;
            set;
        }

        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolation"></param>
        /// <param name="range"></param>
        /// <param name="decay"></param>
        public void InterpolationParamsChangedInView(Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay = null)
        {
            if (PropertyLogItems == null || DisplayAsArchive)
                return;
            foreach (VBPropertyLogChartItem propertylogitem in PropertyLogItems)
            {
                PropertyLogListInfo listInfo = propertylogitem.GetLiveLogList();
                if (listInfo != null)
                    listInfo.SetInterpolationParams(interpolation, range, decay);
            }
        }
    }
}
