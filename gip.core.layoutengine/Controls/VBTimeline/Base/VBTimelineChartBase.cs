using DocumentFormat.OpenXml.Spreadsheet;
using gip.core.datamodel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace gip.core.layoutengine.timeline
{
    public abstract class VBTimelineChartBase : System.Windows.Controls.Control, IVBContent, IVBSource
    {
        #region c'tors

        static VBTimelineChartBase()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null, MinimumMaximumDateChanged));
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null, MinimumMaximumDateChanged));
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(Timeline.TickTimeSpanDefaultValue, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TickTimeSpanChanged));
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            scrollViewer = Template.FindName("PART_ScrollViewer", this) as FrictionScrollViewer;
            _PART_Line = Template.FindName("PART_Line", this) as RulerLine;
            PART_AxesPanel = Template.FindName("PART_DateTimeAxesPanel", this) as DateTimeAxesPanel;

            InitVBControl();

            WireScrollViewer();
        }

        bool _IsInitialized = false;

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public virtual void InitVBControl()
        {
            if (_IsInitialized || ContextACObject == null)
                return;
            if (BSOACComponent != null)
            {
                Binding bindingACComp = new Binding();
                bindingACComp.Source = BSOACComponent;
                bindingACComp.Path = new PropertyPath(Const.InitState);
                bindingACComp.Mode = BindingMode.OneWay;
                SetBinding(VBTimelineChartBase.ACCompInitStateProperty, bindingACComp);
            }

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBGanttChart", VBContent);
                return;
            }

            string acAccess = "";
            ACClassProperty sourceProperty = null;
            VBSource = dcACTypeInfo is ACClassProperty ? (dcACTypeInfo as ACClassProperty).GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBGanttChart", VBSource + " " + VBContent);
                //this.Root().Messages.ErrorAsync(ContextACObject, "Error00004", "VBComboBox", VBSource, VBContent);
                return;
            }

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Mode = BindingMode.TwoWay;
            binding.Path = new PropertyPath(dcPath, null);
            SetBinding(VBTimelineChartBase.SelectedItemProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = dsSource;
            binding2.Path = new PropertyPath(dsPath, null);
            binding2.Mode = BindingMode.OneWay;
            SetBinding(VBTimelineChartBase.ItemsSourceProperty, binding2);


            _IsInitialized = true;
            ApplyTemplate();
        }

        #endregion

        #region PARTs

        internal ItemsControl itemsPresenter;
        internal FrictionScrollViewer scrollViewer;
        internal RulerLine _PART_Line;
        internal DateTimeAxesPanel PART_AxesPanel;

        /// <summary>
        /// Gets the ScrollViewer.
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get { return scrollViewer; }
        }

        #endregion

        #region Properties

        private VBTimelineViewBase _VBTimelineView
        {
            get
            {
                return TemplatedParent as VBTimelineViewBase;
            }
        }

        private bool setCurrentTimePending = false;

        internal StackPanel container = new StackPanel();

        /// <summary>
        /// The line X maximum.
        /// </summary>
        public double LineXMax = 100;

        private TimeSpan _defaultTimeTickSpanValue;

        #endregion

        #region DP

        /// <summary>
        /// Represents the dependency property key for Items.
        /// </summary>
        private static readonly DependencyPropertyKey ItemsPropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(Items), typeof(IList<IACTimeLog>), typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Represents the dependency property for Items.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty = ItemsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the Items.
        /// </summary>
        public IList<IACTimeLog> Items
        {
            get { return (IList<IACTimeLog>)GetValue(ItemsProperty); }
            internal set { SetValue(ItemsPropertyKey, value); }
        }

        /// <summary>
        /// Represents the dependency property for the ItemsSource.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null, ItemsSourceChanged));

        /// <summary>
        /// Gets or sets the ItemsSource.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineChartBase self = (VBTimelineChartBase)o;
            self.ItemsSourceChanged(e);
        }

        private void ItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Items.Clear();
            if (e.NewValue != null)
            {
                IEnumerable items = (IEnumerable)e.NewValue;
                AddItemsInternal(items);

                INotifyCollectionChanged oldCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (oldCollectionChanged != null)
                {
                    oldCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
                }

                INotifyCollectionChanged collectionChanged = e.NewValue as INotifyCollectionChanged;
                if (collectionChanged != null)
                {
                    collectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                }
            }
        }

        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddItemInternal(e.NewItems[0] as IACTimeLog);
                if (MinimumDate == null || MaximumDate == null)
                    SetMinMaxBounds();
                else
                    SetMinMaxBounds(e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveItemsInternal(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RemoveAllItemsInternal();
                MinimumDate = null;
                MaximumDate = null;
            }

        }

        /// <summary>
        /// Represents the dependency property for the SelectedItem.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty
            = DependencyProperty.Register("SelectedItem", typeof(object), typeof(VBTimelineChartBase));

        /// <summary>
        /// Gets or sets the SelectedItem.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Represents the dependecy property key for the Connections.
        /// </summary>
        private static readonly DependencyPropertyKey ConnectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("Connections", typeof(IList<object>), typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Represents the dependecy property for the ItemsPanel.
        /// </summary>
        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(VBTimelineChartBase), ItemsPanelMetadata());

        /// <summary>
        /// Gets or sets the ItemsPanel.
        /// </summary>
        public ItemsPanelTemplate ItemsPanel
        {
            get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        private static FrameworkPropertyMetadata ItemsPanelMetadata()
        {
            var defaultPanelTemplate =
              new ItemsPanelTemplate(
                new FrameworkElementFactory(typeof(TimelineItemPanel))
                );
            var md = new FrameworkPropertyMetadata(defaultPanelTemplate);
            return md;
        }

        /// <summary>
        /// Gets or sets the style selector of item container.
        /// </summary>
        public StyleSelector ItemContainerStyleSelector
        {
            get { return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty); }
            set { SetValue(ItemContainerStyleSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemContainerStyleSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
            DependencyProperty.Register("ItemContainerStyleSelector", typeof(StyleSelector), typeof(VBTimelineChartBase),
            new FrameworkPropertyMetadata(null));

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.RegisterAttached("ItemContainerStyle", typeof(Style), typeof(VBTimelineChartBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the MaximumDate.
        /// </summary>
        public Nullable<DateTime> MaximumDate
        {
            get { return (Nullable<DateTime>)GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumDateProperty;

        /// <summary>
        /// Gets or sets the MinimumDate.
        /// </summary>
        public Nullable<DateTime> MinimumDate
        {
            get { return (Nullable<DateTime>)GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumDateProperty;

        /// <summary>
        /// Gets or sets the Tick TimeSpan.
        /// </summary>
        public TimeSpan TickTimeSpan
        {
            get { return (TimeSpan)GetValue(TickTimeSpanProperty); }
            set { SetValue(TickTimeSpanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TickTimeSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TickTimeSpanProperty;

        private static void TickTimeSpanChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineChartBase self = (VBTimelineChartBase)o;
            self.TickTimeSpanChanged(e);
        }

        private void TickTimeSpanChanged(DependencyPropertyChangedEventArgs e)
        {
            setCurrentTimePending = true;
            UpdateDisplayTimeSpan();
        }

        /// <summary>
        /// Gets or sets the CurrentTime.
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(VBGanttChart), new FrameworkPropertyMetadata(DateTime.MinValue, CurrentTimeChanged));

        private static void CurrentTimeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineChartBase self = (VBTimelineChartBase)o;
            self.CurrentTimeChanged(e);
        }

        private bool ignoreCurrentChanged = false;

        private void CurrentTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ignoreCurrentChanged) return;

            GoToDate((DateTime)e.NewValue);
        }

        private static readonly DependencyPropertyKey MaximumTickTimeSpanPropertyKey =
          DependencyProperty.RegisterReadOnly("MaximumTickTimeSpan", typeof(TimeSpan),
          typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));

        /// <summary>
        /// Represents the dependency property for MaximumTickTimeSpan
        /// </summary>
        public static readonly DependencyProperty MaximumTickTimeSpanProperty =
          MaximumTickTimeSpanPropertyKey.DependencyProperty;

        /// <summary>
        /// Get the maxinimum tick time span allowed.
        /// This control the amount of zoom the control support.
        /// </summary>
        public TimeSpan MaximumTickTimeSpan
        {
            get { return (TimeSpan)GetValue(MaximumTickTimeSpanProperty); }
            internal set { SetValue(MaximumTickTimeSpanPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey MinimumTickTimeSpanPropertyKey =
          DependencyProperty.RegisterReadOnly("MinimumTickTimeSpan", typeof(TimeSpan),
          typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));

        /// <summary>
        /// Represents the dependency property for MaximumTickTimeSpan
        /// </summary>
        public static readonly DependencyProperty MinimumTickTimeSpanProperty =
          MinimumTickTimeSpanPropertyKey.DependencyProperty;

        /// <summary>
        /// Get the minimum tick time allowed.
        /// This control the amount of zoom the control support.
        /// </summary>
        public TimeSpan MinimumTickTimeSpan
        {
            get { return (TimeSpan)GetValue(MinimumTickTimeSpanProperty); }
            internal set { SetValue(MinimumTickTimeSpanPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey DisplayTimeSpanPropertyKey =
          DependencyProperty.RegisterReadOnly("DisplayTimeSpan", typeof(TimeSpan),
          typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));

        /// <summary>
        /// Represents the dependency property for DisplayTimeSpan.
        /// </summary>
        public static readonly DependencyProperty DisplayTimeSpanProperty =
          DisplayTimeSpanPropertyKey.DependencyProperty;

        /// <summary>
        /// Get the time span display by the timeline control.
        /// </summary>
        /// <remarks>
        /// This property is affected by the current TickTimeSpan and the actual size of the control.
        /// This property can be presented to the user to show how much actual "wall-clock" time is presented
        /// by the control.
        /// </remarks>
        public TimeSpan DisplayTimeSpan
        {
            get { return (TimeSpan)GetValue(DisplayTimeSpanProperty); }
            internal set { SetValue(DisplayTimeSpanPropertyKey, value); }
        }


        /// <summary>
        /// Get indication weather there are bounds to the timeline control or not.
        /// </summary>
        public bool IsNoBounds
        {
            get { return (bool)GetValue(IsNoBoundsProperty); }
            internal set { SetValue(IsNoBoundsPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for IsNoBounds.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey IsNoBoundsPropertyKey =
          DependencyProperty.RegisterReadOnly("IsNoBounds", typeof(bool), typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Represents the dependency property for IsNoBounds.
        /// </summary>
        public static readonly DependencyProperty IsNoBoundsProperty =
            IsNoBoundsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets NoBounds.
        /// </summary>
        public object NoBounds
        {
            get { return (object)GetValue(NoBoundsProperty); }
            set { SetValue(NoBoundsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoBoundsContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoBoundsProperty =
            DependencyProperty.Register("NoBounds", typeof(object), typeof(VBTimelineChartBase), new UIPropertyMetadata("No bounds are set"));

        /// <summary>
        /// Gets or sets the data template for NoBoundsContent
        /// </summary>
        public DataTemplate NoBoundsContentTemplate
        {
            get { return (DataTemplate)GetValue(NoBoundsContentTemplateProperty); }
            set { SetValue(NoBoundsContentTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoBoundsContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoBoundsContentTemplateProperty =
            DependencyProperty.Register("NoBoundsContentTemplate", typeof(DataTemplate), typeof(VBTimelineChartBase), new UIPropertyMetadata(null));


        /// <summary>
        /// Gets the data template selector for NooBoundsContent.
        /// </summary>
        public DataTemplateSelector NoBoundsContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(NoBoundsContentTemplateSelectorProperty); }
            set { SetValue(NoBoundsContentTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoBoundsContentTemplateSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoBoundsContentTemplateSelectorProperty =
            DependencyProperty.Register("NoBoundsContentTemplateSelector", typeof(DataTemplateSelector), typeof(VBTimelineChartBase), new UIPropertyMetadata(null));


        /// <summary>
        /// Gets or sets the string format for NoBounds.
        /// </summary>
        public string NoBoundsStringFormat
        {
            get { return (string)GetValue(NoBoundsStringFormatProperty); }
            set { SetValue(NoBoundsStringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoBoundsStringFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoBoundsStringFormatProperty =
            DependencyProperty.Register("NoBoundsStringFormat", typeof(string), typeof(VBGanttChart), new UIPropertyMetadata(null));

        /// <summary>
        /// Represents the dependency property for LineY2.
        /// </summary>
        public static readonly DependencyProperty LineY2Property
           = DependencyProperty.Register("LineY2", typeof(double), typeof(VBTimelineChartBase));

        /// <summary>
        /// Gets or sets the LineY2.
        /// </summary>
        public double LineY2
        {
            get { return (double)GetValue(LineY2Property); }
            set { SetValue(LineY2Property, value + 27); }
        }

        /// <summary>
        /// Represents the dependency property for Zoom.
        /// </summary>
        public static readonly DependencyProperty ZoomProperty
            = DependencyProperty.Register("Zoom", typeof(string), typeof(VBTimelineChartBase), new PropertyMetadata(new PropertyChangedCallback(ZoomChanged)));

        /// <summary>
        /// Gets or sets the Zoom.
        /// </summary>
        public string Zoom
        {
            get { return (string)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static void ZoomChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //if (e.NewValue == null)
            //    return;

            //VBTimelineChartBase timelineChart = o as VBTimelineChartBase;

            //switch (e.NewValue.ToString())
            //{
            //    case "Min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(1);
            //        break;

            //    case "30 min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(16);
            //        break;

            //    case "45 min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(31);
            //        break;

            //    case "60 min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(46);
            //        break;

            //    case "90 min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(61);
            //        break;

            //    case "120 min":
            //        timelineChart.TickTimeSpan = TimeSpan.FromSeconds(100);
            //        break;

            //    case "Default":
            //        timelineChart.TickTimeSpan = timelineChart.MaximumTickTimeSpan;
            //        break;
            //}
        }


        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
           DependencyProperty.Register("ACCompInitState",
               typeof(ACInitState), typeof(VBGanttChart),
               new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBTimelineChartBase thisControl = dependencyObject as VBTimelineChartBase;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }



        public DataTemplate TimelineItemTemplate
        {
            get { return (DataTemplate)GetValue(TimelineItemTemplateProperty); }
            set { SetValue(TimelineItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineItemTemplateProperty =
            DependencyProperty.RegisterAttached("TimelineItemTemplate", typeof(DataTemplate), typeof(VBTimelineChartBase), new PropertyMetadata(null));


        public DateTime? TimeFrameFrom
        {
            get { return (DateTime?)GetValue(TimeFrameFromProperty); }
            set { SetValue(TimeFrameFromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FromTimeFrame.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFrameFromProperty =
            DependencyProperty.Register("TimeFrameFrom", typeof(DateTime?), typeof(VBTimelineChartBase), new PropertyMetadata(null));


        public DateTime? TimeFrameTo
        {
            get { return (DateTime?)GetValue(TimeFrameToProperty); }
            set { SetValue(TimeFrameToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToDateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFrameToProperty =
            DependencyProperty.Register("TimeFrameTo", typeof(DateTime?), typeof(VBTimelineChartBase), new PropertyMetadata(null));

        #endregion

        #region Methods

        private void CalculateDefaultTickTimeSpan()
        {
            if (MinimumDate.HasValue && MaximumDate.HasValue)
            {
                TimeSpan diff = MaximumDate.Value - MinimumDate.Value;
                if (diff.TotalMinutes < 60)
                    _defaultTimeTickSpanValue = TimeSpan.FromSeconds(5);
                else
                    _defaultTimeTickSpanValue = TimeSpan.FromSeconds(60);
            }
            TickTimeSpan = _defaultTimeTickSpanValue;
        }

        private void AddItemsInternal(IEnumerable items)
        {
            foreach (IACTimeLog item in items)
            {
                AddItemInternal(item);
            }
            SetMinMaxBounds();
        }

        private void AddItemInternal(IACTimeLog item)
        {
            Items.Add(item);
        }

        private void RemoveItemsInternal(IEnumerable items)
        {
            foreach (IACTimeLog item in items)
            {
                RemoveItemInternal(item);
            }
        }

        private void RemoveItemInternal(IACTimeLog item)
        {
            Items.Remove(item);
        }

        private void RemoveAllItemsInternal()
        {
            Items.Clear();
        }

        private void UpdateDisplayTimeSpan()
        {
            if (ActualWidth > 0 && TickTimeSpan.Ticks > 0)
            {
                double pixelPerTick = Timeline.GetPixelsPerTick(this);
                DisplayTimeSpan = TimeSpan.FromTicks((long)(ActualWidth / pixelPerTick));
                //DisplayTimeSpan = TimeSpan.FromSeconds(1);
            }
            else
            {
                DisplayTimeSpan = TimeSpan.Zero;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (_PART_Line != null)
                _PART_Line.ArrangeLine();
            base.OnRenderSizeChanged(sizeInfo);
        }

        internal void UpdateDisplayOrder(ICollectionView items)
        {
            BSOACComponent.ExecuteMethod("UpdateDisplayOrder", items, Items);
            itemsPresenter.InvalidateMeasure();
        }

        /// <summary>
        /// Sets the minimum and maximum bounds.
        /// </summary>
        public void SetMinMaxBounds()
        {
            if (Items.Any())
            {
                DateTime maxDate = DateTime.Now;
                DateTime minDate = maxDate.AddDays(-1);
                var queryMin = this.Items.Where(c => c.StartDate.HasValue);
                if (queryMin.Any())
                    minDate = queryMin.Min(c => c.StartDate.Value);
                var queryMax = this.Items.Where(c => c.EndDate.HasValue);
                if (queryMax.Any())
                    maxDate = queryMax.Max(c => c.EndDate.Value).AddHours(1);

                MinimumDate = new DateTime(minDate.Year, minDate.Month, minDate.Day, minDate.Hour, 0, 0);
                MaximumDate = new DateTime(maxDate.Year, maxDate.Month, maxDate.Day, maxDate.Hour, 0, 0);

                PART_AxesPanel.InitControl(true);
            }
            else
                PART_AxesPanel.ClearControl();

        }

        /// <summary>
        /// Sets the minimum and maximum bounds from newItem.
        /// </summary>
        /// <param name="newItem">The new item parameter.</param>
        public void SetMinMaxBounds(object newItem)
        {
            if (newItem != null)
            {
                DateTime? tempStart = newItem.GetValue(_VBTimelineView.GanttStart) as DateTime?;
                if (tempStart != null && tempStart.Value < MinimumDate.Value.AddMinutes(20))
                {
                    var minDate = tempStart.Value.AddMinutes(-20);
                    MinimumDate = new DateTime(minDate.Year, minDate.Month, minDate.Day, minDate.Hour, minDate.Minute, 30);

                }

                DateTime? tempEnd = newItem.GetValue(_VBTimelineView.GanttEnd) as DateTime?;
                if (tempEnd != null && tempEnd.Value > MaximumDate.Value.AddMinutes(-20))
                {
                    var maxDate = tempEnd.Value.AddMinutes(20);
                    MaximumDate = new DateTime(maxDate.Year, maxDate.Month, maxDate.Day, maxDate.Hour, maxDate.Minute, 0);
                }
                PART_AxesPanel.InitControl(true);
            }
        }

        private void SetZoom()
        {
            //Zoom = "";
            //if (MaximumDate.HasValue && MinimumDate.HasValue)
            //{
            //    TimeSpan diff = MaximumDate.Value - MinimumDate.Value;
            //    if (diff.TotalHours <= 12)
            //        Zoom = "60 min";
            //    else if (diff.TotalHours <= 24)
            //        Zoom = "120 min";
            //    else
            //        Zoom = "Max";
            //}
            //else
            Zoom = "Default";
        }

        protected bool recalcMaxZoom = true;

        private static void MinimumMaximumDateChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VBTimelineChartBase self = (VBTimelineChartBase)sender;
            bool newIsNoBound = (self.MinimumDate == null && self.MaximumDate == null);
            bool isNoBoundChanged = self.IsNoBounds != newIsNoBound;
            self.IsNoBounds = newIsNoBound;
            //if the are no bounds, we leave the process as is
            if (isNoBoundChanged)
            {
                //self.TickTimeSpan = Timeline.TickTimeSpanDefaultValue;
            }
            self.recalcMaxZoom = true;
        }

        private void WireScrollViewer()
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (setCurrentTimePending)
            {
                setCurrentTimePending = false;
                GoToDate(CurrentTime);
            }
            else if (e.HorizontalChange != 0)
            {
                ignoreCurrentChanged = true;
                CurrentTime = Timeline.OffsetToDate(e.HorizontalOffset, this);
                ignoreCurrentChanged = false;
            }
        }

        /// <summary>
        /// Scrolls to given date and time.
        /// </summary>
        /// <param name="date"></param>
        public void GoToDate(DateTime date)
        {
            if (scrollViewer == null) return;

            double offset = Timeline.DateToOffset(date, this);
            if (offset >= 0)
            {
                scrollViewer.ScrollToHorizontalOffset(offset);
            }
        }

        // add to derivations
        //internal TimelineItem ContainerFromItem(object item)
        //{
        //    if (itemsPresenter == null) return null;

        //    return itemsPresenter.ContainerFromItem(item);
        //}

        protected Nullable<Size> lastActualBounds;

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size actual = base.ArrangeOverride(arrangeBounds);

            // If the maximum minimum date have changed (recalcMaxZoom is true)
            // or the actual bounds of the control have changed we recalc the maximum zoom factor.
            if (recalcMaxZoom || (lastActualBounds == null || lastActualBounds.Value != actual))
            {
                recalcMaxZoom = false;
                SetMaximumZoomFactor(actual);
            }

            lastActualBounds = actual;

            return actual;
        }

        private void SetMaximumZoomFactor(Size actualSize)
        {
            const double maxZoomMargin = 40;

            if (MaximumDate.HasValue && MinimumDate.HasValue)
            {
                TimeSpan timeframe = MaximumDate.Value - MinimumDate.Value;
                double tickPerTimeSpan = timeframe.Ticks / MathUtil.ReduceUntilOne(actualSize.Width, maxZoomMargin);
                MaximumTickTimeSpan = TimeSpan.FromTicks((long)(tickPerTimeSpan));
                //MinimumTickTimeSpan = TimeSpan.FromTicks((long)tickPerTimeSpan / 10);
            }
            else
            {
                MaximumTickTimeSpan = TimeSpan.Zero;
                MinimumTickTimeSpan = TimeSpan.Zero;
            }
            SetZoom();
        }

        //add to derivations
        ///// <summary>
        ///// Brings item into view.
        ///// </summary>
        ///// <param name="item">The item parameter.</param>
        //public void BringItemIntoView(object item)
        //{
        //    TimelineItem container = ContainerFromItem(item);
        //    if (container != null)
        //    {
        //        Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
        //        if (start.HasValue)
        //        {
        //            GoToDate(start.Value);
        //        }
        //    }
        //}

        // add to derivations
        ///// <summary>
        ///// Brings into view.
        ///// </summary>
        ///// <param name="mode">The mode parameter.</param>
        ///// <param name="dataItem">The data item parameter.</param>
        //public void BringIntoView(VBGanttChartBringIntoViewMode mode, object dataItem)
        //{
        //    TimelineItem container = ContainerFromItem(dataItem);
        //    if (container != null)
        //    {
        //        Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
        //        Nullable<DateTime> end = TimelineCompactPanel.GetEndDate(container);

        //        if (IsSetZoomToFit(mode) && start.HasValue && end.HasValue)
        //        {
        //            TimeSpan duration = end.Value - start.Value;
        //            double pixelPerTick = (ActualWidth / 2) / duration.Ticks;
        //            TimeSpan newTickTimeSpan = TimeSpan.FromTicks((long)(1D / pixelPerTick));

        //            if (newTickTimeSpan.TotalMinutes < 1)
        //            {
        //                newTickTimeSpan = TimeSpan.FromMinutes(1);
        //            }

        //            if (newTickTimeSpan < TickTimeSpan)
        //            {
        //                TickTimeSpan = newTickTimeSpan;
        //            }
        //            else
        //            {
        //                if (ActualWidth / 2 < duration.Ticks * Timeline.GetPixelsPerTick(this))
        //                {
        //                    TickTimeSpan = newTickTimeSpan;
        //                }
        //            }

        //            WpfUtility.WaitForPriority(DispatcherPriority.Background);
        //        }

        //        if (IsSetCurrentTime(mode))
        //        {
        //            if (start.HasValue) CurrentTime = start.Value;
        //            else if (end.HasValue) CurrentTime = end.Value;
        //        }
        //    }
        //}

        private static bool IsSetZoomToFit(VBGanttChartBringIntoViewMode mode)
        {
            return ((mode & VBGanttChartBringIntoViewMode.SetZoomToFit) != 0);
        }

        private static bool IsSetCurrentTime(VBGanttChartBringIntoViewMode mode)
        {
            return ((mode & VBGanttChartBringIntoViewMode.SetCurrentTime) != 0);
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        internal void SetTickTimeSpan(TimeSpan timeSpan)
        {
            this.TickTimeSpan = timeSpan;
        }

        #endregion

        #region IVBContent members

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get { return Global.ControlModes.Enabled; }
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        public string DisabledModes
        {
            get { return ""; }
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
            PART_AxesPanel.DeInitControl();
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            BindingOperations.ClearBinding(this, SelectedItemProperty);
            BindingOperations.ClearAllBindings(this);
            container.Children.Clear();
            container = null;
            _IsInitialized = false;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBGanttChart));

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
            get;
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
            get;
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
            return false;
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

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTimelineChartBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        #region IVBSource members

        /// <summary>
        /// Represents the dependency property for the VBSource.
        /// </summary>
        public static readonly DependencyProperty VBSourceProperty
            = DependencyProperty.Register("VBSource", typeof(string), typeof(VBGanttChart));

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        public string VBSource
        {
            get
            {
                return (string)GetValue(VBSourceProperty);
            }
            set
            {
                SetValue(VBSourceProperty, value);
            }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBShowColumns
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBDisabledColumns
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBChilds
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Mouse events - overrides

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl))
                PART_AxesPanel.OnZoomStart(e);
            else if (Keyboard.IsKeyDown(Key.LeftShift))
                PART_AxesPanel.OnZoomOut(e);

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl))
                PART_AxesPanel.OnZoomMove(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            PART_AxesPanel.OnZoomEnd(e);
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        #endregion
    }
}
