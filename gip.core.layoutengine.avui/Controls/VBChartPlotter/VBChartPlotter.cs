using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using OxyPlot;
using OxyPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a plotter control which shows chart.
    /// </summary>
    /// <summary>
    /// Steuerelement zum plotten.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBChartPlotter'}de{'VBChartPlotter'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBChartPlotter : Plot, IACInteractiveObject, IACObject, IVBChart
    {
        /// <summary>
        /// This class map PropertyLogRing and FastLineRenderableSeries.
        /// </summary>
        /// 
        private abstract class LineMap
        {
            public LineMap(PropertyLogListInfo logListInfo, VBChartItem chartItem, LineSeries line)
            {
                _ChartItem = chartItem;
                _LogListInfo = logListInfo;
                _TupleValues = null;
                Line = line;
            }

            public LineMap(VBChartItem chartItem, IEnumerable<IVBChartTuple> tupleValues, LineSeries line)
            {
                _ChartItem = chartItem;
                _TupleValues = tupleValues;
                _LogListInfo = null;
                Line = line;
            }

            private VBChartItem _ChartItem;
            public VBChartItem ChartItem
            {
                get
                {
                    return _ChartItem;
                }
            }

            private PropertyLogListInfo _LogListInfo;
            public PropertyLogListInfo LogListInfo
            {
                get
                {
                    return _LogListInfo;
                }
            }

            public PropertyLogRing PropertyLogRing
            {
                get
                {
                    return LogListInfo.LiveLogList;
                }
            }

            IEnumerable<IVBChartTuple> _TupleValues;
            public IEnumerable<IVBChartTuple> TupleValues
            {
                get
                {
                    return _TupleValues;
                }
                set
                {
                    _TupleValues = value;
                }
            }

            public LineSeries Line
            {
                get;
                set;
            }

            public bool IsInterpolationMode
            {
                get;
                set;
            }


            public abstract void Append(object valueX, object valueY);

            //public void ClearDataSeries()
            //{
            //    Line.DataSeries.Clear();
            //}
        }

        private class LineMapT<TX, TY> : LineMap
            where TX : global::System.IComparable
            where TY : global::System.IComparable
        {
            public LineMapT(PropertyLogListInfo logListInfo, VBChartItem chartItem, LineSeries line)
                : base(logListInfo, chartItem, line)
            {
            }

            public LineMapT(VBChartItem chartItem, IEnumerable<IVBChartTuple> tupleValues, LineSeries line)
                : base(chartItem, tupleValues, line)
            {
            }

            public override void Append(object valueX, object valueY)
            {
                //XyDataSeries<TX, TY> dataSeries = Line.DataSeries as XyDataSeries<TX, TY>;
                //if (dataSeries == null)
                //    return;
                //if (valueY is Boolean)
                //    valueY = Convert.ToByte((Boolean)valueY);
                //if (valueX is TX && valueY is TY)
                //    dataSeries.Append((TX)valueX, (TY)valueY);
                //else
                //    dataSeries.Append((TX)Convert.ChangeType(valueX, typeof(TX)), (TY)Convert.ChangeType(valueY, typeof(TY)));
            }

            //public override void Append(DateTime dt, object value)
            //{
            //    XyDataSeries<DateTime, TY> dataSeries = Line.DataSeries as XyDataSeries<DateTime, TY>;
            //    if (dataSeries == null)
            //        return;
            //    if (value is Boolean)
            //        value = Convert.ToByte((Boolean)value);
            //    dataSeries.Append(dt, (TY)value);
            //}
        }


        public VBChartPlotter()
        {
            PropertyLogItems = new ObservableCollection<IVBChartItem>();
        }


        VBPropertyLogChart _VBPropertyLogChart = null;

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

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBChartPlotter>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }


        private ObservableCollection<IVBChartItem> _PropertyLogItems;
        public ObservableCollection<IVBChartItem> PropertyLogItems
        {
            get { return _PropertyLogItems; }
            set { _PropertyLogItems = value; }
        }

        public static readonly StyledProperty<VBChartItemDisplayMode> DisplayModeProperty = AvaloniaProperty.Register<VBCheckBox, VBChartItemDisplayMode>(nameof(DisplayMode));
        
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public VBChartItemDisplayMode DisplayMode
        {
            get { return (VBChartItemDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
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

        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return null;
            }
        }

        public Global.ControlModes RightControlMode
        {
            get
            {
                return Global.ControlModes.Collapsed;
            }
        }

        public string DisabledModes
        {
            get
            {
                return null;
            }
        }

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                return null;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return this.Name;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return null;
            }
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

        List<LineMap> _LineMapList = new List<LineMap>();

        /// <summary>
        /// This method initialize chart. Add lines to chart and fill lines with dataSeries. 
        /// </summary>
        protected bool _Initialized = false;
        private void InitVBControl()
        {
            if (PropertyLogItems == null || _Initialized)
                return;
            _Initialized = true;
            foreach (VBChartItem chartItem in PropertyLogItems)
            {
                chartItem.SetVBBindings(this.DataContext, this.BSOACComponent);
                chartItem.InitVBControl(ContextACObject);
                VBPropertyLogChartItem logChartItem = chartItem as VBPropertyLogChartItem;
                if (logChartItem == null && chartItem.ACProperty != null)
                    chartItem.ACProperty.PropertyChanged += ACProperty_PropertyChanged;
                InitLine(chartItem);
            }
            InitializeAutoRangeOfAxes();
        }

        private void ACProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                Avalonia.Threading.Dispatcher.UIThread.Invoke(new Action(delegate { ACProperty_PropertyChanged(sender, e); }), DispatcherPriority.Background);
            }
            else
            {
                LineMap lineMap = _LineMapList.Where(c => c.ChartItem.ACProperty == sender).FirstOrDefault();
                if (lineMap == null)
                {
                    if (PropertyLogItems == null || !PropertyLogItems.Any())
                        return;
                    VBChartItem chartItem = PropertyLogItems.Where(c => c.ACProperty == sender).FirstOrDefault() as VBChartItem;
                    if (chartItem != null)
                        InitLine(chartItem);
                }
                //else
                //{
                //    IEnumerable<IVBChartTuple> chartSeries = lineMap.ChartItem.DataSeries;
                //    if (chartSeries != null)
                //    {
                //        if (chartSeries == lineMap.TupleValues)
                //            return;
                //    }

                //    INotifyCollectionChanged notifyCollectionChangedCollection = lineMap.TupleValues as INotifyCollectionChanged;
                //    if (notifyCollectionChangedCollection != null)
                //        notifyCollectionChangedCollection.CollectionChanged -= NotifyableCollectionChanged;

                //    lineMap.ClearDataSeries();
                //    if (chartSeries != null)
                //    {
                //        lineMap.TupleValues = chartSeries;

                //        if (chartSeries.Any())
                //        {
                //            foreach (var tupleValue in chartSeries)
                //            {
                //                if (lineMap.ChartItem.DisplayMode == VBChartItemDisplayMode.MapTupleValue2ToX)
                //                    lineMap.Append(tupleValue.Value2, tupleValue.Value1);
                //                else
                //                    lineMap.Append(tupleValue.Value1, tupleValue.Value2);
                //            }
                //        }
                //        notifyCollectionChangedCollection = chartSeries as INotifyCollectionChanged;
                //        if (notifyCollectionChangedCollection != null)
                //            notifyCollectionChangedCollection.CollectionChanged += NotifyableCollectionChanged;
                //    }
                //    AutoZoomExtents();
                //}
            }
        }

        private void InitLine(VBChartItem chartItem)
        {
            //_CountAutoExtents = 0;
            VBPropertyLogChartItem logChartItem = chartItem as VBPropertyLogChartItem;

            Type typeOfYAxis = null;
            Type typeOfXAxis = null;
            IEnumerable<IVBChartTuple> chartSeries = null;
            PropertyLogRing propertyLogRing = null;
            PropertyLogListInfo logListInfo = null;
            // X-Axis is DateTime
            if (logChartItem != null && chartItem.ACProperty != null)
            {
                typeOfXAxis = typeof(DateTime);
                typeOfYAxis = chartItem.ACProperty.ACType.ObjectType;
                if (!typeOfYAxis.IsValueType || !typeof(IComparable).IsAssignableFrom(typeOfYAxis))
                    return;
                if (typeof(Boolean).IsAssignableFrom(typeOfYAxis))
                    typeOfYAxis = typeof(byte);
                else if (typeOfYAxis.IsEnum)
                    typeOfYAxis = Enum.GetUnderlyingType(typeOfYAxis);
                logListInfo = logChartItem.GetLiveLogList();

                IEnumerable<PropertyLogItem> propertyLog = logListInfo.PropertyLogList;
                if (propertyLog == null)
                    return;

                propertyLogRing = logListInfo.LiveLogList;
            }
            else
            {
                chartSeries = chartItem.DataSeries;
                if (chartSeries != null)
                {
                    IVBChartTuple tuple = chartSeries.FirstOrDefault();
                    if (tuple != null)
                    {
                        typeOfXAxis = tuple.Value1.GetType();
                        typeOfYAxis = tuple.Value2.GetType();
                    }
                }
                else if (chartItem.ACProperty != null)
                {
                    typeOfYAxis = chartItem.ACProperty.ACType.ObjectFullType;
                    Type typeOfChartTuple = typeof(IEnumerable<>).MakeGenericType(typeof(IVBChartTuple));
                    if (!typeOfChartTuple.IsAssignableFrom(typeOfYAxis))
                        return;
                    typeOfXAxis = typeof(Double);
                    typeOfYAxis = typeof(Double);
                }
                if (chartItem.DisplayMode == VBChartItemDisplayMode.MapTupleValue2ToX)
                {
                    Type tmp = typeOfXAxis;
                    typeOfXAxis = typeOfYAxis;
                    typeOfYAxis = tmp;
                }
            }
            if (typeOfXAxis == null || typeOfYAxis == null)
                return;

            if (logListInfo == null && chartSeries == null)
                return;

            bool bExists = false;
            LineSeries line = CreateOrGetLine(chartItem, out bExists);
            if (line == null)
                return;

            if (logListInfo != null)
            {
                line.Mapping = item => new DataPoint(((PropertyLogItem)item).Time.Ticks, Convert.ToDouble(((PropertyLogItem)item).Value));
                Binding binding = new Binding();
                binding.Source = logListInfo;
                binding.Path = nameof(PropertyLogListInfo.PropertyLogList);
                binding.Mode = BindingMode.OneWay;
                line.Bind(ItemsControl.ItemsSourceProperty, binding);
            }
            else if (chartSeries != null)
            {
                line.ItemsSource = chartSeries;
                if (chartItem.DisplayMode == VBChartItemDisplayMode.MapTupleValue1ToX)
                    line.Mapping = item => new DataPoint(Convert.ToDouble(((IVBChartTuple)item).Value1), Convert.ToDouble(((IVBChartTuple)item).Value2));
                else
                    line.Mapping = item => new DataPoint(Convert.ToDouble(((IVBChartTuple)item).Value2), Convert.ToDouble(((IVBChartTuple)item).Value1));

                Binding binding = new Binding();
                binding.Source = chartItem;
                binding.Path = nameof(VBChartItem.DataSeries);
                binding.Mode = BindingMode.OneWay;
                line.Bind(ItemsControl.ItemsSourceProperty, binding);
            }


            Type typeOfLineMap = typeof(LineMapT<,>).MakeGenericType(typeOfXAxis, typeOfYAxis);
            LineMap lineMap = null;

            if (logListInfo != null)
                lineMap = (LineMap)Activator.CreateInstance(typeOfLineMap, logListInfo, chartItem, line);
            else if (chartSeries != null)
                lineMap = (LineMap)Activator.CreateInstance(typeOfLineMap, chartItem, chartSeries, line);

            if (lineMap == null)
                return;
            _LineMapList.Add(lineMap);

            if (propertyLogRing != null)
            {
                propertyLogRing.CollectionChanged += NotifyableCollectionChanged;
                if ((chartItem.ACProperty != null) && (chartItem.ACProperty.LiveLog != null))
                    chartItem.ACProperty.LiveLog.PropertyChanged += LiveLog_PropertyChanged;
            }
            else if (chartSeries != null)
            {
                INotifyCollectionChanged notifyCollectionChangedCollection = chartSeries as INotifyCollectionChanged;
                if (notifyCollectionChangedCollection != null)
                    notifyCollectionChangedCollection.CollectionChanged += NotifyableCollectionChanged;
            }

            if (!bExists)
            {
                this.Series.Add(line);
            }
        }

        bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (!DisplayAsArchive)
                InitVBControl();
            if (_Loaded)
                return;
            _VBPropertyLogChart = this.FindLogicalAncestorOfType<VBPropertyLogChart>();
            //_VBPropertyLogChart = this.FindControl<VBPropertyLogChart>("ucChart") as VBPropertyLogChart;
            if (_VBPropertyLogChart != null)
                _VBPropertyLogChart.RefreshLiveConsole();
            _Loaded = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
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

            foreach (VBChartItem chartItem in PropertyLogItems)
            {
                if (chartItem.ACProperty == null)
                    continue;

                VBPropertyLogChartItem logChartItem = chartItem as VBPropertyLogChartItem;
                if (logChartItem != null)
                {
                    if (logChartItem.ACProperty != null && logChartItem.ACProperty.LiveLog != null && logChartItem.ACProperty.LiveLog.LiveLogList != null)
                        logChartItem.ACProperty.LiveLog.LiveLogList.CollectionChanged -= NotifyableCollectionChanged;
                    if (chartItem.ACProperty != null && chartItem.ACProperty.LiveLog != null)
                        chartItem.ACProperty.LiveLog.PropertyChanged -= LiveLog_PropertyChanged;
                }
                else
                {
                    if (chartItem.ACProperty != null)
                        chartItem.ACProperty.PropertyChanged -= ACProperty_PropertyChanged;
                    INotifyCollectionChanged chartSeries = chartItem.DataSeries as INotifyCollectionChanged;
                    if (chartSeries != null)
                        chartSeries.CollectionChanged -= NotifyableCollectionChanged;
                }

                chartItem.DeInitVBControl(bso);
            }
        }


        /// <summary>
        /// Refresh live console when live log changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The Property changed event arguments.</param>
        void LiveLog_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogState")
            {
                if (_VBPropertyLogChart != null)
                    _VBPropertyLogChart.RefreshLiveConsole();
            }
        }

        /// <summary>
        /// This method initialize chart when is in archive mode. Add lines to chart and fill lines with dataSeries from archive.
        /// </summary>
        /// <param name="from">Show archive from date.</param>
        /// <param name="to">Show archive to date.</param>
        public void InitializeChartArchive(DateTime from, DateTime to, Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay = null)
        {
            if (!DisplayAsArchive || PropertyLogItems == null)
                return;

            _Initialized = true;

            bool bForceRedraw = false;
            foreach (var dataSeries in this.Series.Where(c => c is LineSeries && String.IsNullOrEmpty((c as LineSeries).Name)).ToArray())
            {
                bForceRedraw = true;
                this.Series.Remove(dataSeries);
            }

            bool bAnyLines = false;
            foreach (VBPropertyLogChartItem propertylogitem in PropertyLogItems)
            {
                if (propertylogitem == null)
                    continue;
                propertylogitem.SetVBBindings(this.DataContext, this.BSOACComponent);
                propertylogitem.InitVBControl(ContextACObject);

                Type typeOfValue = null;
                if (propertylogitem.ACProperty != null)
                {
                    typeOfValue = propertylogitem.ACProperty.ACType.ObjectType;
                    if (!typeOfValue.IsValueType || !typeof(IComparable).IsAssignableFrom(typeOfValue))
                        continue;
                    if (typeof(Boolean).IsAssignableFrom(typeOfValue))
                        typeOfValue = typeof(byte);
                    else if (typeOfValue.IsEnum)
                        typeOfValue = Enum.GetUnderlyingType(typeOfValue);
                }
                else
                    continue;

                bool bExists = false;
                LineSeries line = CreateOrGetLine(propertylogitem, out bExists);
                if (line == null)
                    continue;
                if (bExists && line.ItemsSource != null)
                {
                    line.ClearBinding(ItemsControl.ItemsSourceProperty);
                    line.ClearValue(ItemsControl.ItemsSourceProperty);
                }

                PropertyLogListInfo logListInfo = propertylogitem.GetArchiveLogList(from, to);
                if (logListInfo != null)
                {
                    if (interpolation != Global.InterpolationMethod.None && logListInfo != null)
                    {
                        logListInfo.SetInterpolationParams(interpolation, range, decay);
                        logListInfo.Interpolate();
                    }

                    line.Mapping = item => new DataPoint(((PropertyLogItem)item).Time.Ticks, Convert.ToDouble(((PropertyLogItem)item).Value));
                    Binding binding = new Binding();
                    binding.Source = logListInfo;
                    binding.Path = nameof(PropertyLogListInfo.PropertyLogList);
                    binding.Mode = BindingMode.OneWay;
                    line.Bind(ItemsControl.ItemsSourceProperty, binding);
                }

                if (!bExists)
                    Series.Add(line);
                bAnyLines = true;
            }
            if (bAnyLines)
                AutoZoomExtents(bForceRedraw);
        }


        protected IEnumerable<Axis> YAxes
        {
            get
            {
                return this.Axes.Where(c => c.Position == OxyPlot.Axes.AxisPosition.Left || c.Position == OxyPlot.Axes.AxisPosition.Right);
            }
        }

        protected IEnumerable<Axis> XAxes
        {
            get
            {
                return this.Axes.Where(c => c.Position == OxyPlot.Axes.AxisPosition.Bottom || c.Position == OxyPlot.Axes.AxisPosition.Top);
            }
        }

        /// <summary>
        /// Create line from VBPropertyLogChartItem.
        /// </summary>
        /// <param name="vbChartItem">The vbPropertyLogChartItem.</param>
        /// <returns></returns>
        private LineSeries CreateOrGetLine(VBChartItem vbChartItem, out bool bExists)
        {
            bExists = false;
            if (vbChartItem == null || !YAxes.Any())
                return null;
            LineSeries line = null;
            if (!String.IsNullOrEmpty(vbChartItem.LineId))
            {
                line = Series.Where(c => c is LineSeries && (c as LineSeries).YAxisKey == vbChartItem.LineId).FirstOrDefault() as LineSeries;
                if (line != null)
                {
                    bExists = true;
                    return line;
                }
            }
            if (line == null)
            {
                line = new LineSeries();
                //line.IsDigitalLine = vbChartItem.IsDigitalLine;
            }

            if (string.IsNullOrEmpty(vbChartItem.AxisId) || string.IsNullOrWhiteSpace(vbChartItem.AxisId) || !YAxes.Any(c => c.Key == vbChartItem.AxisId))
                line.YAxisKey = YAxes.FirstOrDefault().Key;
            else
                line.YAxisKey = vbChartItem.AxisId;
            if (bExists)
                return line;

            line.StrokeThickness = 1;
            //(line as FastLineRenderableSeries).Name = vbPropertyLogChartItem.VBContent;
            line.Color = vbChartItem.GetLineColor();
            return line;
        }


        /// <summary>
        /// Add data or clear chart data in runtime.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The collection changed event arguments.</param>
        void NotifyableCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                Avalonia.Threading.Dispatcher.UIThread.Invoke(new Action(delegate { NotifyableCollectionChanged(sender, e); }), DispatcherPriority.Background);
            }
            else
            {
                PropertyLogRing propertyLogRing = sender as PropertyLogRing;
                if (propertyLogRing != null)
                {
                    if (e.NewItems != null && e.NewItems[0] != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        PropertyLogItem newItem = e.NewItems[0] as PropertyLogItem;
                        LineMap lineMap = _LineMapList.FirstOrDefault(k => k.PropertyLogRing == propertyLogRing);
                        if (lineMap == null)
                            return;
                        bool isInterpolationMode = lineMap.LogListInfo.IsInterpolationOn;
                        if (isInterpolationMode
                            || !isInterpolationMode && lineMap.IsInterpolationMode)
                        {
                            lineMap.IsInterpolationMode = isInterpolationMode;
                            //lineMap.ClearDataSeries();
                            if (isInterpolationMode)
                                lineMap.LogListInfo.Interpolate();
                            AutoZoomExtents();
                        }
                        else
                        {
                            lineMap.Append(newItem.Time, newItem.Value);
                            AutoZoomExtents();
                        }
                    }
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    {
                        LineMap lineMap = _LineMapList.FirstOrDefault(k => k.PropertyLogRing == propertyLogRing);
                        if (lineMap == null)
                            return;
                        //lineMap.ClearDataSeries();
                        AutoZoomExtents();
                    }
                }
                //else
                //{
                //    IEnumerable<IVBChartTuple> chartSeries = sender as IEnumerable<IVBChartTuple>;
                //    if (chartSeries != null)
                //    {
                //        if (e.NewItems != null && e.NewItems[0] != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                //        {
                //            IVBChartTuple newItem = e.NewItems[0] as IVBChartTuple;
                //            LineMap lineMap = _LineMapList.FirstOrDefault(k => k.TupleValues == chartSeries);
                //            if (lineMap == null)
                //                return;
                //            bool isInterpolationMode = lineMap.LogListInfo.IsInterpolationOn;
                //            if (isInterpolationMode
                //                || !isInterpolationMode && lineMap.IsInterpolationMode)
                //            {
                //                lineMap.IsInterpolationMode = isInterpolationMode;
                //                AutoZoomExtents();
                //            }
                //        }
                //        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                //        {
                //            LineMap lineMap = _LineMapList.FirstOrDefault(k => k.TupleValues == chartSeries);
                //            if (lineMap == null)
                //                return;
                //            //lineMap.ClearDataSeries();
                //            AutoZoomExtents();
                //        }
                //    }
                //}
            }
        }

        //Brush _NormalModeBrush = null;
        private bool _IsPrintingMode = false;
        /// <summary>
        /// Switch to normal mode or print mode.
        /// </summary>
        public void SwitchNormalPrintingMode()
        {
            if (!_IsPrintingMode)
            {
                //this.ActualThemeVariant;
                //if (_NormalModeBrush == null)
                //    _NormalModeBrush = ((GridLinesPanel)GridLinesPanel).Background;
                //((GridLinesPanel)GridLinesPanel).Background = Brushes.White;
                //Background = Brushes.White;
                //ThemeManager.SetTheme(this, "BrightSpark");
                _IsPrintingMode = true;
            }
            else
            {
                //((GridLinesPanel)GridLinesPanel).Background = _NormalModeBrush;
                //Background = _NormalModeBrush;
                //ThemeManager.SetTheme(this, "");
                _IsPrintingMode = false;
            }
        }

        /// <summary>
        /// This method create bitmap source from chart.
        /// </summary>
        /// <returns>Created bitmap source.</returns>
        public Bitmap CreatePrintableBitmap()
        {
            return this.ToBitmap();
        }


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
            return null;
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return false;
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

        //public AutoRange CanAutoExtendXAxis
        //{
        //    get
        //    {
        //        if (this.XAxis != null)
        //            return this.XAxis.AutoRange;
        //        else if (this.XAxes != null)
        //        {
        //            if (this.XAxes.Where(c => c.AutoRange == AutoRange.Always).Any())
        //                return AutoRange.Always;
        //            else if (this.XAxes.Where(c => c.AutoRange == AutoRange.Once).Any())
        //                return this.DisplayAsArchive ? AutoRange.Once : AutoRange.Always;
        //        }
        //        return AutoRange.Never;
        //    }
        //}

        //public AutoRange CanAutoExtendYAxis
        //{
        //    get
        //    {
        //        if (this.YAxis != null)
        //            return this.YAxis.AutoRange;
        //        else if (this.YAxes != null)
        //        {
        //            if (this.YAxes.Where(c => c.AutoRange == AutoRange.Always).Any())
        //                return AutoRange.Always;
        //            else if (this.YAxes.Where(c => c.AutoRange == AutoRange.Once).Any())
        //                return this.DisplayAsArchive ? AutoRange.Once : AutoRange.Always;
        //        }
        //        return AutoRange.Never;
        //    }
        //}

        protected void InitializeAutoRangeOfAxes()
        {
            //if (this.XAxis != null)
            //    InitializeAutoRangeOfAxis(this.XAxis);
            if (this.XAxes != null)
            {
                foreach (Axis axis in this.XAxes)
                {
                    InitializeAutoRangeOfAxis(axis);
                }
            }

            //if (this.YAxis != null)
            //    InitializeAutoRangeOfAxis(this.YAxis);
            if (this.YAxes != null)
            {
                foreach (Axis axis in this.YAxes)
                {
                    InitializeAutoRangeOfAxis(axis);
                }
            }
        }

        protected void InitializeAutoRangeOfAxis(Axis axis)
        {
            //axis.MaximumRange = double.NaN;
            //axis.MinimumRange = double.NaN;
            //AxisBase axisBase = axis as AxisBase;
            //if (axisBase != null)
            //{
            //    ValueSource valueSource = DependencyPropertyHelper.GetValueSource(axisBase, AxisBase.AutoRangeProperty);
            //    if ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))
            //    {
            //        axis.AutoRange = AutoRange.Always; // this.DisplayAsArchive ? AutoRange.Once : AutoRange.Always;
            //    }
            //}
        }

        //private int _CountAutoExtents = 0;
        public void AutoZoomExtents(bool forceRedraw = false)
        {
            this.UpdateModel();
            //AutoRange autoExtendXAxis = CanAutoExtendXAxis;
            //AutoRange autoExtendYAxis = CanAutoExtendYAxis;

            //bool extendX = autoExtendXAxis == AutoRange.Always || autoExtendXAxis == AutoRange.Once && _CountAutoExtents == 0;
            //bool extendY = autoExtendYAxis == AutoRange.Always || autoExtendYAxis == AutoRange.Once && _CountAutoExtents == 0;

            //if ((extendX && extendY) || forceRedraw)
            //{
            //    ZoomExtents();
            //    _CountAutoExtents++;
            //}
            //else if (extendX)
            //{
            //    ZoomExtentsX();
            //    _CountAutoExtents++;
            //}
            //else if (extendY)
            //{
            //    ZoomExtentsY();
            //    _CountAutoExtents++;
            //}
        }

        Bitmap IVBChart.CreatePrintableBitmap()
        {
            throw new NotImplementedException();
        }
    }
}
