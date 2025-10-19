using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui.timeline
{
    public abstract class VBTimelineChartBase : TemplatedControl, IVBContent, IVBSource
    {
        #region c'tors

        static VBTimelineChartBase()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(VBTimelineChartBase), new StyledPropertyMetadata<DateTime?>(null));
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(VBTimelineChartBase), new StyledPropertyMetadata<DateTime?>(null));
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(VBTimelineChartBase), new StyledPropertyMetadata<TimeSpan>(Timeline.TickTimeSpanDefaultValue));
            
            MinimumDateProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => MinimumMaximumDateChanged(x, e));
            MaximumDateProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => MinimumMaximumDateChanged(x, e));
            TickTimeSpanProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => TickTimeSpanChanged(x, e));
            CurrentTimeProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => CurrentTimeChanged(x, e));
            ItemsSourceProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => ItemsSourceChanged(x, e));
            ZoomProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => ZoomChanged(x, e));
            ACCompInitStateProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => OnDepPropChanged(x, e));
            BSOACComponentProperty.Changed.AddClassHandler<VBTimelineChartBase>((x, e) => OnDepPropChanged(x, e));
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _ScrollViewer = e.NameScope.Find("PART_ScrollViewer") as FrictionScrollViewer;
            _PART_Line = e.NameScope.Find("PART_Line") as RulerLine;
            PART_AxesPanel = e.NameScope.Find("PART_DateTimeAxesPanel") as DateTimeAxesPanel;

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
                bindingACComp.Path = Const.InitState;
                bindingACComp.Mode = BindingMode.OneWay;
                this.Bind(VBTimelineChartBase.ACCompInitStateProperty, bindingACComp);
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
                return;
            }

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Mode = BindingMode.TwoWay;
            binding.Path = dcPath;
            this.Bind(VBTimelineChartBase.SelectedItemProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = dsSource;
            binding2.Path = dsPath;
            binding2.Mode = BindingMode.OneWay;
            this.Bind(VBTimelineChartBase.ItemsSourceProperty, binding2);

            _IsInitialized = true;
        }

        #endregion

        #region PARTs

        internal ItemsControl _ItemsPresenter;
        internal FrictionScrollViewer _ScrollViewer;
        internal RulerLine _PART_Line;
        internal DateTimeAxesPanel PART_AxesPanel;

        /// <summary>
        /// Gets the ScrollViewer.
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get { return _ScrollViewer; }
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

        #region Avalonia Properties

        /// <summary>
        /// Represents the styled property for Items.
        /// </summary>
        public static readonly DirectProperty<VBTimelineChartBase, IList<IACTimeLog>> ItemsProperty =
            AvaloniaProperty.RegisterDirect<VBTimelineChartBase, IList<IACTimeLog>>(
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);

        private IList<IACTimeLog> _items = new List<IACTimeLog>();

        /// <summary>
        /// Gets or sets the Items.
        /// </summary>
        public IList<IACTimeLog> Items
        {
            get { return _items; }
            internal set { SetAndRaise(ItemsProperty, ref _items, value); }
        }

        /// <summary>
        /// Represents the styled property for the ItemsSource.
        /// </summary>
        public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, IEnumerable>(nameof(ItemsSource));

        /// <summary>
        /// Gets or sets the ItemsSource.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(VBTimelineChartBase self, AvaloniaPropertyChangedEventArgs e)
        {
            self.ItemsSourceChanged(e);
        }

        private void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
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
        /// Represents the styled property for the SelectedItem.
        /// </summary>
        public static readonly StyledProperty<object> SelectedItemProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, object>(nameof(SelectedItem));

        /// <summary>
        /// Gets or sets the SelectedItem.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for the ItemsPanel.
        /// </summary>
        public static readonly StyledProperty<ITemplate<Panel>> ItemsPanelProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, ITemplate<Panel>>(nameof(ItemsPanel), ItemsPanelTemplate());

        /// <summary>
        /// Gets or sets the ItemsPanel.
        /// </summary>
        public ITemplate<Panel> ItemsPanel
        {
            get { return GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        private static ITemplate<Panel> ItemsPanelTemplate()
        {
            return new FuncTemplate<Panel>(() => new TimelineItemPanel());
        }

        /// <summary>
        /// Gets or sets the style selector of item container.
        /// </summary>
        public static readonly StyledProperty<Selector> ItemContainerStyleSelectorProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, Selector>(nameof(ItemContainerStyleSelector));

        public Selector ItemContainerStyleSelector
        {
            get { return GetValue(ItemContainerStyleSelectorProperty); }
            set { SetValue(ItemContainerStyleSelectorProperty, value); }
        }

        public static readonly StyledProperty<IStyle> ItemContainerStyleProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, IStyle>(nameof(ItemContainerStyle));

        public IStyle ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the MaximumDate.
        /// </summary>
        public static readonly StyledProperty<DateTime?> MaximumDateProperty;

        public Nullable<DateTime> MaximumDate
        {
            get { return GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the MinimumDate.
        /// </summary>
        public static readonly StyledProperty<DateTime?> MinimumDateProperty;

        public Nullable<DateTime> MinimumDate
        {
            get { return GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Tick TimeSpan.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> TickTimeSpanProperty;

        public TimeSpan TickTimeSpan
        {
            get { return GetValue(TickTimeSpanProperty); }
            set { SetValue(TickTimeSpanProperty, value); }
        }

        private static void TickTimeSpanChanged(VBTimelineChartBase self, AvaloniaPropertyChangedEventArgs e)
        {
            self.TickTimeSpanChanged(e);
        }

        private void TickTimeSpanChanged(AvaloniaPropertyChangedEventArgs e)
        {
            setCurrentTimePending = true;
            UpdateDisplayTimeSpan();
        }

        /// <summary>
        /// Gets or sets the CurrentTime.
        /// </summary>
        public static readonly StyledProperty<DateTime> CurrentTimeProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, DateTime>(nameof(CurrentTime), DateTime.MinValue);

        public DateTime CurrentTime
        {
            get { return GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        private static void CurrentTimeChanged(VBTimelineChartBase self, AvaloniaPropertyChangedEventArgs e)
        {
            self.CurrentTimeChanged(e);
        }

        private bool ignoreCurrentChanged = false;

        private void CurrentTimeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (ignoreCurrentChanged) return;

            GoToDate((DateTime)e.NewValue);
        }

        public static readonly DirectProperty<VBTimelineChartBase, TimeSpan> MaximumTickTimeSpanProperty =
            AvaloniaProperty.RegisterDirect<VBTimelineChartBase, TimeSpan>(
                nameof(MaximumTickTimeSpan),
                o => o.MaximumTickTimeSpan,
                (o, v) => o.MaximumTickTimeSpan = v,
                TimeSpan.FromDays(1));

        private TimeSpan _maximumTickTimeSpan = TimeSpan.FromDays(1);

        /// <summary>
        /// Get the maxinimum tick time span allowed.
        /// This control the amount of zoom the control support.
        /// </summary>
        public TimeSpan MaximumTickTimeSpan
        {
            get { return _maximumTickTimeSpan; }
            internal set { SetAndRaise(MaximumTickTimeSpanProperty, ref _maximumTickTimeSpan, value); }
        }

        public static readonly DirectProperty<VBTimelineChartBase, TimeSpan> MinimumTickTimeSpanProperty =
            AvaloniaProperty.RegisterDirect<VBTimelineChartBase, TimeSpan>(
                nameof(MinimumTickTimeSpan),
                o => o.MinimumTickTimeSpan,
                (o, v) => o.MinimumTickTimeSpan = v,
                TimeSpan.FromDays(1));

        private TimeSpan _minimumTickTimeSpan = TimeSpan.FromDays(1);

        /// <summary>
        /// Get the minimum tick time allowed.
        /// This control the amount of zoom the control support.
        /// </summary>
        public TimeSpan MinimumTickTimeSpan
        {
            get { return _minimumTickTimeSpan; }
            internal set { SetAndRaise(MinimumTickTimeSpanProperty, ref _minimumTickTimeSpan, value); }
        }

        public static readonly DirectProperty<VBTimelineChartBase, TimeSpan> DisplayTimeSpanProperty =
            AvaloniaProperty.RegisterDirect<VBTimelineChartBase, TimeSpan>(
                nameof(DisplayTimeSpan),
                o => o.DisplayTimeSpan,
                (o, v) => o.DisplayTimeSpan = v,
                TimeSpan.FromDays(1));

        private TimeSpan _displayTimeSpan = TimeSpan.FromDays(1);

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
            get { return _displayTimeSpan; }
            internal set { SetAndRaise(DisplayTimeSpanProperty, ref _displayTimeSpan, value); }
        }

        public static readonly DirectProperty<VBTimelineChartBase, bool> IsNoBoundsProperty =
            AvaloniaProperty.RegisterDirect<VBTimelineChartBase, bool>(
                nameof(IsNoBounds),
                o => o.IsNoBounds,
                (o, v) => o.IsNoBounds = v,
                true);

        private bool _isNoBounds = true;

        /// <summary>
        /// Get indication weather there are bounds to the timeline control or not.
        /// </summary>
        public bool IsNoBounds
        {
            get { return _isNoBounds; }
            internal set { SetAndRaise(IsNoBoundsProperty, ref _isNoBounds, value); }
        }

        /// <summary>
        /// Gets or sets NoBounds.
        /// </summary>
        public static readonly StyledProperty<object> NoBoundsProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, object>(nameof(NoBounds), "No bounds are set");

        public object NoBounds
        {
            get { return GetValue(NoBoundsProperty); }
            set { SetValue(NoBoundsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the data template for NoBoundsContent
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> NoBoundsContentTemplateProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, IDataTemplate>(nameof(NoBoundsContentTemplate));

        public IDataTemplate NoBoundsContentTemplate
        {
            get { return GetValue(NoBoundsContentTemplateProperty); }
            set { SetValue(NoBoundsContentTemplateProperty, value); }
        }

        /// <summary>
        /// Gets the data template selector for NooBoundsContent.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> NoBoundsContentTemplateSelectorProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, IDataTemplate>(nameof(NoBoundsContentTemplateSelector));

        public IDataTemplate NoBoundsContentTemplateSelector
        {
            get { return GetValue(NoBoundsContentTemplateSelectorProperty); }
            set { SetValue(NoBoundsContentTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the string format for NoBounds.
        /// </summary>
        public static readonly StyledProperty<string> NoBoundsStringFormatProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, string>(nameof(NoBoundsStringFormat));

        public string NoBoundsStringFormat
        {
            get { return GetValue(NoBoundsStringFormatProperty); }
            set { SetValue(NoBoundsStringFormatProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for LineY2.
        /// </summary>
        public static readonly StyledProperty<double> LineY2Property =
            AvaloniaProperty.Register<VBTimelineChartBase, double>(nameof(LineY2));

        /// <summary>
        /// Gets or sets the LineY2.
        /// </summary>
        public double LineY2
        {
            get { return GetValue(LineY2Property); }
            set { SetValue(LineY2Property, value + 27); }
        }

        /// <summary>
        /// Represents the styled property for Zoom.
        /// </summary>
        public static readonly StyledProperty<string> ZoomProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, string>(nameof(Zoom));

        /// <summary>
        /// Gets or sets the Zoom.
        /// </summary>
        public string Zoom
        {
            get { return GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static void ZoomChanged(VBTimelineChartBase timelineChart, AvaloniaPropertyChangedEventArgs e)
        {
            // Zoom change logic can be implemented here if needed
        }

        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(VBTimelineChartBase thisControl, AvaloniaPropertyChangedEventArgs args)
        {
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

        public static readonly StyledProperty<IDataTemplate> TimelineItemTemplateProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, IDataTemplate>(nameof(TimelineItemTemplate));

        public IDataTemplate TimelineItemTemplate
        {
            get { return GetValue(TimelineItemTemplateProperty); }
            set { SetValue(TimelineItemTemplateProperty, value); }
        }

        public static readonly StyledProperty<DateTime?> TimeFrameFromProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, DateTime?>(nameof(TimeFrameFrom));

        public DateTime? TimeFrameFrom
        {
            get { return GetValue(TimeFrameFromProperty); }
            set { SetValue(TimeFrameFromProperty, value); }
        }

        public static readonly StyledProperty<DateTime?> TimeFrameToProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, DateTime?>(nameof(TimeFrameTo));

        public DateTime? TimeFrameTo
        {
            get { return GetValue(TimeFrameToProperty); }
            set { SetValue(TimeFrameToProperty, value); }
        }

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
            if (Bounds.Width > 0 && TickTimeSpan.Ticks > 0)
            {
                double pixelPerTick = Timeline.GetPixelsPerTick(this);
                DisplayTimeSpan = TimeSpan.FromTicks((long)(Bounds.Width / pixelPerTick));
            }
            else
            {
                DisplayTimeSpan = TimeSpan.Zero;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == BoundsProperty)
            {
                if (_PART_Line != null)
                    _PART_Line.ArrangeLine();
                UpdateDisplayTimeSpan();
            }
        }

        internal void UpdateDisplayOrder(ICollectionView items)
        {
            BSOACComponent.ExecuteMethod("UpdateDisplayOrder", items, Items);
            _ItemsPresenter.InvalidateMeasure();
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

                PART_AxesPanel?.InitControl(true);
            }
            else
                PART_AxesPanel?.ClearControl();
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
                PART_AxesPanel?.InitControl(true);
            }
        }

        private void SetZoom()
        {
            Zoom = "Default";
        }

        protected bool recalcMaxZoom = true;

        private static void MinimumMaximumDateChanged(VBTimelineChartBase self, AvaloniaPropertyChangedEventArgs e)
        {
            bool newIsNoBound = (self.MinimumDate == null && self.MaximumDate == null);
            bool isNoBoundChanged = self.IsNoBounds != newIsNoBound;
            self.IsNoBounds = newIsNoBound;
            if (isNoBoundChanged)
            {
                // Handle no bounds change if needed
            }
            self.recalcMaxZoom = true;
        }

        private void WireScrollViewer()
        {
            if (_ScrollViewer != null)
            {
                _ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (setCurrentTimePending)
            {
                setCurrentTimePending = false;
                GoToDate(CurrentTime);
            }
            else if (e.OffsetDelta.X != 0)
            {
                ignoreCurrentChanged = true;
                CurrentTime = Timeline.OffsetToDate(e.OffsetDelta.X, this);
                ignoreCurrentChanged = false;
            }
        }

        /// <summary>
        /// Scrolls to given date and time.
        /// </summary>
        /// <param name="date"></param>
        public void GoToDate(DateTime date)
        {
            if (_ScrollViewer == null) return;

            double offset = Timeline.DateToOffset(date, this);
            if (offset >= 0)
            {
                var currentOffset = _ScrollViewer.Offset;
                var newOffset = new Vector(offset, currentOffset.Y);
                _ScrollViewer.SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
            }
        }

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
            }
            else
            {
                MaximumTickTimeSpan = TimeSpan.Zero;
                MinimumTickTimeSpan = TimeSpan.Zero;
            }
            SetZoom();
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
            PART_AxesPanel?.DeInitControl();
            this.ClearAllBindings();
            container?.Children.Clear();
            container = null;
            _IsInitialized = false;
        }

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, string>(nameof(VBContent));

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
        /// Represents the styled property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty =  ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTimelineChartBase>();

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        #region IVBSource members

        /// <summary>
        /// Represents the styled property for the VBSource.
        /// </summary>
        public static readonly StyledProperty<string> VBSourceProperty =
            AvaloniaProperty.Register<VBTimelineChartBase, string>(nameof(VBSource));

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        public string VBSource
        {
            get { return GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBShowColumns
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBDisabledColumns
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public string VBChilds
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// (Not implemented)
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Pointer events - overrides

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                PART_AxesPanel?.OnZoomStart(e);
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                PART_AxesPanel?.OnZoomOut(e);

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                PART_AxesPanel?.OnZoomMove(e);
            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            PART_AxesPanel?.OnZoomEnd(e);
            base.OnPointerReleased(e);
        }

        #endregion
    }
}
