using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui.timeline
{
    public class DateTimeAxesPanel : Canvas, INotifyPropertyChanged
    {
        #region c'tors

        static DateTimeAxesPanel()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner<DateTimeAxesPanel>(new StyledPropertyMetadata<DateTime?>(null));
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner<DateTimeAxesPanel>(new StyledPropertyMetadata<DateTime?>(null));
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner<DateTimeAxesPanel>(new StyledPropertyMetadata<TimeSpan>(Timeline.TickTimeSpanDefaultValue));
            ScrollViewerHorizontalOffsetProperty.Changed.AddClassHandler<DateTimeAxesPanel>((x, e) => OnOffsetChanged(x, e));
        }

        public DateTimeAxesPanel()
        {

        }

        internal void InitControl(bool forceInit = false)
        {
            if (!MinimumDate.HasValue || !MaximumDate.HasValue || this.Bounds.Width <= 0)
                return;

            if (!forceInit && _LastWidth == this.Bounds.Width && Children.Count > 0)
                return;

            Children.Clear();
            _TimeframeDateTimes = new List<DateTime>();

            EnsureGridLinesOverlay();

            int numberOfAxes = (int)this.Bounds.Width / _MinAxisOffset;

            _DefaultTimeSpan = DetermineDateTimeSpan(numberOfAxes, TimelineChart.MinimumDate.Value, TimelineChart.MaximumDate.Value);
            _CurrentTimeSpan = _DefaultTimeSpan;
            TimelineChart.SetTickTimeSpan(_DefaultTimeSpan);

            DateTime calcDate = MinimumDate.Value;
            _StartDateTime = calcDate;

            DateTime lastDate = calcDate;
            SolidColorBrush dtAxisForeground = _dtAxisColor1;

            var defaultOffset = Timeline.DateToOffset(calcDate, this) - (TextBlockWidth / 2);

            for (int i = 0; i < numberOfAxes; i++)
            {
                if (lastDate.Date != calcDate.Date)
                {
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;
                }

                DateTimeAxis axis = new DateTimeAxis(this) { AxisHeight = this.Bounds.Height };
                _TimeframeDateTimes.Add(calcDate);
                axis.CurrentDateTime = calcDate;
                axis.DateTimeTextBlock.Foreground = dtAxisForeground;
                lastDate = calcDate;

                if (defaultOffset < -(TextBlockWidth / 2))
                    defaultOffset = this.Bounds.Width;

                axis.SetPosition(defaultOffset, true);
                Children.Add(axis);

                calcDate += _DefaultTimeSpan;
                defaultOffset += _MinAxisOffset;
            }

            StartAxis = Children[0] as DateTimeAxis;
            EndAxis = Children[numberOfAxes - 1] as DateTimeAxis;

            if (_IsFirstInit || forceInit)
            {
                MaximumDate = EndAxis.CurrentDateTime;
                TimelineChart.MaximumDate = MaximumDate;
                TimelinePanel timelinePanel = VBVisualTreeHelper.FindChildObjects<TimelinePanel>(TimelineChart._ItemsPresenter)?.FirstOrDefault();
                if (timelinePanel != null)
                    timelinePanel.MaximumDate = MaximumDate;
                _IsFirstInit = false;
            }

            _LastWidth = this.Bounds.Width;
            InvalidateGridLines();
        }

        internal void ClearControl()
        {
            Children.Clear();
        }

        public void DeInitControl()
        {
            this.ClearAllBindings();
            foreach (DateTimeAxis axis in Children)
            {
                axis.DeInitControl();
            }
        }

        #endregion

        #region Fields

        public static double TextBlockWidth = 60;

        private List<DateTime> _TimeframeDateTimes = new List<DateTime>();

        private DateTime _StartDateTime = new DateTime();

        internal bool _IsScrollFromZoom = false;

        private TimeSpan _DefaultTimeSpan;

        private TimeSpan _CurrentTimeSpan;

        private double _LastWidth = 0;

        private double _LastOffset = 0;

        private int _MinAxisOffset = 60;

        private bool _IsFirstInit = true;

        static SolidColorBrush _dtAxisColor1 = new SolidColorBrush(ControlManager.WpfTheme == eWpfTheme.Dark ? Colors.White : Colors.Black);

        static SolidColorBrush _dtAxisColor2 = new SolidColorBrush(ControlManager.WpfTheme == eWpfTheme.Dark ? Colors.Orange : Colors.Blue);

        //temp fix 
        private double _TempZoomOffset = 0;

        #endregion

        #region Properties

        public static readonly StyledProperty<DateTime?> MaximumDateProperty;
        public static readonly StyledProperty<DateTime?> MinimumDateProperty;
        public static readonly StyledProperty<TimeSpan> TickTimeSpanProperty;

        public Nullable<DateTime> MaximumDate
        {
            get { return GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        public Nullable<DateTime> MinimumDate
        {
            get { return GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        public TimeSpan TickTimeSpan
        {
            get { return GetValue(TickTimeSpanProperty); }
            set { SetValue(TickTimeSpanProperty, value); }
        }

        private DateTimeAxis _StartAxis;
        private DateTimeAxis StartAxis
        {
            get => _StartAxis;
            set
            {
                _StartAxis = value;
            }

        }

        private DateTimeAxis _EndAxis;
        private DateTimeAxis EndAxis
        {
            get => _EndAxis;
            set
            {
                _EndAxis = value;
            }
        }

        public static readonly StyledProperty<double> AxesOffsetProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, double>(nameof(AxesOffset), 0.0);

        public double AxesOffset
        {
            get { return GetValue(AxesOffsetProperty); }
            set { SetValue(AxesOffsetProperty, value); }
        }

        public static readonly StyledProperty<double> ScrollViewerHorizontalOffsetProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, double>(nameof(ScrollViewerHorizontalOffset), 0.0);

        public double ScrollViewerHorizontalOffset
        {
            get { return GetValue(ScrollViewerHorizontalOffsetProperty); }
            set { SetValue(ScrollViewerHorizontalOffsetProperty, value); }
        }

        private List<TimeSpan> _LogicalDateTimeSpans;
        private List<TimeSpan> LogicalDateTimeSpans
        {
            get
            {
                if (_LogicalDateTimeSpans == null)
                {
                    _LogicalDateTimeSpans = new List<TimeSpan>
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(10),
                        TimeSpan.FromMinutes(15),
                        TimeSpan.FromMinutes(30),
                        TimeSpan.FromMinutes(45),
                        TimeSpan.FromHours(1),
                        TimeSpan.FromHours(2),
                        TimeSpan.FromHours(4),
                        TimeSpan.FromHours(8),
                        TimeSpan.FromHours(16),
                        TimeSpan.FromDays(1),
                        TimeSpan.FromDays(2),
                        TimeSpan.FromDays(3),
                        TimeSpan.FromDays(4),
                        TimeSpan.FromDays(5),
                        TimeSpan.FromDays(10),
                        TimeSpan.FromDays(20),
                        TimeSpan.FromDays(30),
                        TimeSpan.FromDays(60),
                        TimeSpan.FromDays(180)
                    };
                }
                return _LogicalDateTimeSpans;
            }
        }

        public static readonly StyledProperty<Thickness> ZoomBorderMarginProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, Thickness>(nameof(ZoomBorderMargin), new Thickness());

        public Thickness ZoomBorderMargin
        {
            get { return GetValue(ZoomBorderMarginProperty); }
            set { SetValue(ZoomBorderMarginProperty, value); }
        }

        public static readonly StyledProperty<double> ZoomRectWidthProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, double>(nameof(ZoomRectWidth), 0.0);

        public double ZoomRectWidth
        {
            get { return GetValue(ZoomRectWidthProperty); }
            set { SetValue(ZoomRectWidthProperty, value); }
        }

        public static readonly StyledProperty<bool> IsZoomActiveProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, bool>(nameof(IsZoomActive), false);

        /// <summary>
        /// True while a zoom-range drag is in progress. Drives the visibility of
        /// the ZoomRectBorder in the chart template (a double cannot be checked
        /// for null, so the former IsNotNull-converter binding was always true).
        /// </summary>
        public bool IsZoomActive
        {
            get { return GetValue(IsZoomActiveProperty); }
            set { SetValue(IsZoomActiveProperty, value); }
        }

        public static readonly StyledProperty<HorizontalAlignment> ZoomBorderAlignmentProperty =
            AvaloniaProperty.Register<DateTimeAxesPanel, HorizontalAlignment>(nameof(ZoomBorderAlignment), HorizontalAlignment.Left);

        public HorizontalAlignment ZoomBorderAlignment
        {
            get { return GetValue(ZoomBorderAlignmentProperty); }
            set { SetValue(ZoomBorderAlignmentProperty, value); }
        }

        private VBTimelineChartBase TimelineChart
        {
            get
            {
                return TemplatedParent as VBTimelineChartBase;
            }
        }

        #endregion

        #region Methods

        // Overlay child that draws the vertical grid lines (Panel.Render is sealed
        // in Avalonia, so the lines cannot be drawn by the panel itself).
        private GridLinesOverlay _GridLinesOverlay;

        /// <summary>
        /// Ensures the grid-line overlay is present. IMPORTANT: appended as the
        /// LAST child - several places use index-based access (Children[0] must be
        /// the first DateTimeAxis, Children[numberOfAxes-1] the last one), and
        /// UpdateDateTimeAxes calibrates the axis offset via Children[0].
        /// </summary>
        private void EnsureGridLinesOverlay()
        {
            if (_GridLinesOverlay == null)
                _GridLinesOverlay = new GridLinesOverlay(this);
            if (!Children.Contains(_GridLinesOverlay))
                Children.Add(_GridLinesOverlay);
            _GridLinesOverlay.InvalidateVisual();
        }

        /// <summary>
        /// Draws a vertical grid line for each timestamp axis, spanning the full
        /// height of the panel (WPF-Parity: in WPF the DateTimeAxis-Line stretched
        /// over the whole chart height).
        /// </summary>
        internal void InvalidateGridLines()
        {
            _GridLinesOverlay?.InvalidateVisual();
        }

        /// <summary>
        /// Renders the vertical grid lines below each axis timestamp.
        /// </summary>
        private sealed class GridLinesOverlay : Control
        {
            private readonly DateTimeAxesPanel _Panel;
            private Pen _Pen;

            public GridLinesOverlay(DateTimeAxesPanel panel)
            {
                _Panel = panel;
                IsHitTestVisible = false;
            }

            public override void Render(DrawingContext context)
            {
                base.Render(context);

                double height = _Panel.Bounds.Height;
                if (height <= 0 || _Panel.Children.Count == 0)
                    return;

                // Draw beneath the axis text (~23px) down to the bottom of the panel
                const double lineTop = 23;
                if (height <= lineTop)
                    return;

                if (_Pen == null)
                {
                    // Match the axis line color of DateTimeAxis (Gray for light theme, Beige otherwise)
                    IBrush axisColor = Brushes.Beige;
                    if (ControlManager.WpfTheme == eWpfTheme.Light)
                        axisColor = Brushes.Gray;
                    _Pen = new Pen(axisColor, 1.0);
                }

                foreach (var axis in _Panel.Children.OfType<DateTimeAxis>())
                {
                    double x = Canvas.GetLeft(axis) + (DateTimeAxesPanel.TextBlockWidth / 2);
                    if (x < 0 || x > _Panel.Bounds.Width)
                        continue;
                    context.DrawLine(_Pen, new Point(x, lineTop), new Point(x, height));
                }
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty)
            {
                InitControl();
            }
        }

        private static void TimeframeChanged(AvaloniaObject o, AvaloniaPropertyChangedEventArgs e)
        {
        }

        private DateTime FindNearestLogicalDateTime(DateTime currentDT)
        {
            return _TimeframeDateTimes.FirstOrDefault(c => c >= currentDT);
        }

        private static void OnOffsetChanged(DateTimeAxesPanel thisControl, AvaloniaPropertyChangedEventArgs e)
        {
            var newOffset = (double)e.NewValue - thisControl._LastOffset;
            if (newOffset == 0)
                return;

            // Guard: the offset binding can fire before InitControl created the
            // axes (or after ClearControl) - StartAxis would be null here.
            if (thisControl.StartAxis == null || thisControl.EndAxis == null)
            {
                thisControl._LastOffset = (double)e.NewValue;
                return;
            }

            if (thisControl._IsScrollFromZoom)
            {
                thisControl._IsScrollFromZoom = false;
                thisControl._LastOffset = (double)e.NewValue;

                if (thisControl._TempZoomOffset != thisControl.TimelineChart.ScrollViewer.Offset.X)
                {
                    var offset = thisControl.TimelineChart.ScrollViewer.Offset;
                    var newOffsetVector = new Vector(thisControl._TempZoomOffset, offset.Y);
                    thisControl.TimelineChart.ScrollViewer.SetCurrentValue(
                        ScrollViewer.OffsetProperty,
                        newOffsetVector
                    );
                }

                return;
            }

            var newPosition = Canvas.GetLeft(thisControl.StartAxis) - newOffset;

            if (newPosition - thisControl.StartAxis.DefaultPosition >= thisControl._MinAxisOffset - 1 || newPosition - thisControl.StartAxis.DefaultPosition <= -thisControl._MinAxisOffset + 1)
            {
                DateTime currentDT = Timeline.OffsetToDate((double)e.NewValue, thisControl.TimelineChart._ItemsPresenter);
                DateTime startDT = thisControl.FindNearestLogicalDateTime(currentDT);

                double offset = Timeline.DateToOffset(startDT, thisControl);
                var offsetCorrection = offset - (double)e.NewValue;
                int indexOfStartDT = thisControl._TimeframeDateTimes.IndexOf(startDT);
                int maxTFDT = thisControl._TimeframeDateTimes.Count;
                DateTime? lastDate = startDT;
                bool reverse = false;
                DateTimeAxis dtAxis = thisControl.Children.OfType<DateTimeAxis>().FirstOrDefault(c => c.CurrentDateTime == startDT);
                if (dtAxis == null)
                {
                    dtAxis = thisControl.Children.OfType<DateTimeAxis>().FirstOrDefault();
                    reverse = true;
                }

                SolidColorBrush dtAxisForeground = dtAxis != null ? dtAxis.DateTimeTextBlock.Foreground as SolidColorBrush : _dtAxisColor1;
                if (reverse && dtAxis != null && startDT.Date != dtAxis.CurrentDateTime.Date)
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;

                foreach (DateTimeAxis axis in thisControl.Children)
                {
                    axis.SetPosition(axis.DefaultPosition + offsetCorrection);

                    if (indexOfStartDT < maxTFDT)
                    {
                        DateTime current = thisControl._TimeframeDateTimes[indexOfStartDT];
                        if (lastDate.HasValue && lastDate.Value.Date != current.Date)
                        {
                            dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;
                        }

                        axis.CurrentDateTime = current;
                        axis.DateTimeTextBlock.Foreground = dtAxisForeground;
                        lastDate = current;

                        indexOfStartDT++;
                    }
                    else
                        axis.CurrentDateTime = new DateTime();
                }

                thisControl.TimelineChart.TimeFrameFrom = startDT;
                thisControl.TimelineChart.TimeFrameTo = thisControl.Children.OfType<DateTimeAxis>().LastOrDefault()?.CurrentDateTime;
                thisControl.InvalidateGridLines();
            }
            else
            {
                foreach (DateTimeAxis axis in thisControl.Children)
                    axis.SetPosition(Canvas.GetLeft(axis) - newOffset);
                thisControl.InvalidateGridLines();
            }

            thisControl._LastOffset = (double)e.NewValue;
        }

        #region Methods => TimeSpan

        private TimeSpan DetermineDateTimeSpan(int maxAxis, DateTime start, DateTime end)
        {
            TimeSpan timeframe = end - start;

            foreach (TimeSpan logicalTS in LogicalDateTimeSpans)
            {
                var lin = (timeframe.TotalSeconds / logicalTS.TotalSeconds) + 1;
                if (lin < maxAxis)
                    return logicalTS;
            }

            return TimeSpan.FromHours(1);
        }

        #endregion

        #region Methods => Zoom

        double lastMousePos = 0;
        double startMousePos = 0;
        bool _IsZoomCaptured = false;

        internal void OnZoomStart(PointerPressedEventArgs e)
        {
            _IsZoomCaptured = true;
            IsZoomActive = true;
            lastMousePos = e.GetPosition(this).X + 25;
            startMousePos = e.GetPosition(TimelineChart._ItemsPresenter).X + 25;
            ZoomRectWidth = 0;
            //System.Diagnostics.Debug.WriteLine($"[ZOOM] Start: lastMousePos={lastMousePos}, startMousePos={startMousePos}, panelMin={MinimumDate}, panelMax={MaximumDate}, panelTick={TickTimeSpan}, chartTick={TimelineChart?.TickTimeSpan}");
            if (this.Parent is Control parentControl)
                parentControl.Cursor = new Cursor(StandardCursorType.SizeWestEast);
        }

        internal void OnZoomMove(PointerEventArgs e)
        {
            if (!_IsZoomCaptured)
                return;

            double newPosition = e.GetPosition(this).X + 25;
            double offset = newPosition - lastMousePos;

            if (offset > 0)
            {
                ZoomBorderAlignment = HorizontalAlignment.Left;
                ZoomBorderMargin = new Thickness(lastMousePos, 20, 0, 0);
            }
            else if (offset < 0)
            {
                ZoomBorderAlignment = HorizontalAlignment.Right;
                ZoomBorderMargin = new Thickness(0, 20, this.Bounds.Width - lastMousePos, 0);
            }

            ZoomRectWidth = Math.Abs(offset);
            //System.Diagnostics.Debug.WriteLine($"[ZOOM] Move: newPosition={newPosition}, offset={offset}, margin={ZoomBorderMargin}, width={ZoomRectWidth}");
        }

        internal void OnZoomEnd(PointerReleasedEventArgs e)
        {
            if (_IsZoomCaptured)
            {
                ZoomRectWidth = 0;
                IsZoomActive = false;
                if (this.Parent is Control parentControl)
                    parentControl.Cursor = new Cursor(StandardCursorType.Arrow);
                var pos = e.GetPosition(TimelineChart._ItemsPresenter).X;
                //System.Diagnostics.Debug.WriteLine($"[ZOOM] End: startPos={startMousePos}, endPos={pos}");
                DoZoom(startMousePos, pos);
                _IsZoomCaptured = false;
            }
        }

        /// <summary>
        /// Aborts an active zoom-range drag without applying the zoom
        /// (e.g. when pointer capture is lost mid-drag).
        /// </summary>
        internal void OnZoomCancelled()
        {
            if (_IsZoomCaptured)
            {
                ZoomRectWidth = 0;
                IsZoomActive = false;
                if (this.Parent is Control parentControl)
                    parentControl.Cursor = new Cursor(StandardCursorType.Arrow);
                _IsZoomCaptured = false;
            }
        }

        internal void OnZoomOut(PointerPressedEventArgs e)
        {
            startMousePos = e.GetPosition(TimelineChart._ItemsPresenter).X + 25;
            DateTime dt = Timeline.OffsetToDate(startMousePos, this);

            int indexOfLogicalTS = LogicalDateTimeSpans.IndexOf(_CurrentTimeSpan);
            if (indexOfLogicalTS >= LogicalDateTimeSpans.Count)
                return;

            TimeSpan zoomOutSpan = LogicalDateTimeSpans[indexOfLogicalTS + 1];
            if (zoomOutSpan > _DefaultTimeSpan)
                return;

            _CurrentTimeSpan = zoomOutSpan;
            _TimeframeDateTimes = GenerateTimeframeDateTimes(_TimeframeDateTimes.FirstOrDefault(), _TimeframeDateTimes.LastOrDefault(), _CurrentTimeSpan);
            TimelineChart.SetTickTimeSpan(_CurrentTimeSpan);

            dt = _TimeframeDateTimes.LastOrDefault(c => c <= dt);

            UpdateDateTimeAxes(dt, new DateTime(), _CurrentTimeSpan);
        }

        private void DoZoom(double pos1, double pos2)
        {
            DateTime date1 = Timeline.OffsetToDate(pos1, this);
            DateTime date2 = Timeline.OffsetToDate(pos2, this);
            //System.Diagnostics.Debug.WriteLine($"[ZOOM] DoZoom: pos1={pos1}, pos2={pos2}, date1={date1}, date2={date2}, panelMin={MinimumDate}, panelTick={TickTimeSpan}, tfCount={_TimeframeDateTimes.Count}");

            var temp = date1;
            if (date1 > date2)
            {
                date1 = date2;
                date2 = temp;
            }

            if (date2 - date1 <= TimeSpan.FromSeconds(1))
            {
                //System.Diagnostics.Debug.WriteLine("[ZOOM] DoZoom aborted: range <= 1s");
                return;
            }

            double? diff = null, diff2 = null;

            foreach (DateTime dt in _TimeframeDateTimes)
            {
                if (!diff2.HasValue)
                {
                    TimeSpan dtDiff = date1 - dt;
                    if (!(diff.HasValue) || Math.Abs(dtDiff.TotalSeconds) < diff)
                    {
                        diff = Math.Abs(dtDiff.TotalSeconds);
                        continue;
                    }
                    else
                    {
                        date1 = _TimeframeDateTimes[_TimeframeDateTimes.IndexOf(dt) - 1];
                    }
                }
                TimeSpan dtDiff2 = date2 - dt;
                if (!(diff2.HasValue) || Math.Abs(dtDiff2.TotalSeconds) < diff2)
                {
                    diff2 = Math.Abs(dtDiff2.TotalSeconds);

                }
                else
                {
                    date2 = _TimeframeDateTimes[_TimeframeDateTimes.IndexOf(dt) - 1];
                    break;
                }
            }

            TimeSpan ts = DetermineDateTimeSpan((int)this.Bounds.Width / _MinAxisOffset + 1, date1, date2);

            if (ts == _CurrentTimeSpan)
            {
                int iTS = _LogicalDateTimeSpans.IndexOf(ts);
                if (iTS > 0)
                    ts = _LogicalDateTimeSpans[iTS - 1];
                else
                    return;
            }

            _CurrentTimeSpan = ts;

            TimelineChart.SetTickTimeSpan(_CurrentTimeSpan);
            //System.Diagnostics.Debug.WriteLine($"[ZOOM] DoZoom applying: date1={date1}, date2={date2}, newTick={ts}");

            _TimeframeDateTimes = GenerateTimeframeDateTimes(_StartDateTime, MaximumDate.Value, _CurrentTimeSpan);

            UpdateDateTimeAxes(date1, date2, _CurrentTimeSpan);
        }

        private List<DateTime> GenerateTimeframeDateTimes(DateTime start, DateTime end, TimeSpan timeSpan)
        {
            List<DateTime> datetimes = new List<DateTime>();
            DateTime current = start;
            for (; ; )
            {
                if (current > end)
                    break;
                datetimes.Add(current);

                current += timeSpan;
            }
            return datetimes;
        }

        private void UpdateDateTimeAxes(DateTime displayStart, DateTime displayEnd, TimeSpan timeSpan)
        {
            if (_DefaultTimeSpan == _CurrentTimeSpan)
                displayStart = _TimeframeDateTimes.FirstOrDefault();

            // The TickTimeSpan was just changed on the chart. The inherited value
            // has not necessarily reached the item panel yet - force a layout pass
            // NOW, otherwise the offset calculations below use the OLD
            // pixels-per-tick ratio and the scroll position / item widths desync
            // from the axes (items "not re-rendered at the new zoom level").
            var itemsPresenter = TimelineChart?._ItemsPresenter;
            if (itemsPresenter != null)
            {
                itemsPresenter.InvalidateMeasure();
                itemsPresenter.UpdateLayout();
            }

            // Use the CHART as owner for DateToOffset: it is where the new
            // TickTimeSpan was set, so the conversion always uses the new
            // pixels-per-tick ratio (the presenter may still hold a stale value).
            _TempZoomOffset = Timeline.DateToOffset(displayStart, TimelineChart);

            var offset = TimelineChart.ScrollViewer.Offset;
            var newOffset = new Vector(_TempZoomOffset, offset.Y);
            TimelineChart.ScrollViewer.SetCurrentValue(
                ScrollViewer.OffsetProperty,
                newOffset
            );
            _LastOffset = _TempZoomOffset;

            DateTime current = displayStart;
            double axisOffset = 0;
            DateTime lastDate = _TimeframeDateTimes.FirstOrDefault();
            SolidColorBrush dtAxisForeground = _dtAxisColor1;

            var axes = Children.OfType<DateTimeAxis>().ToList();
            if (axes.Count == 0)
                return;

            axisOffset = Timeline.DateToOffset(current, TimelineChart) - _TempZoomOffset - (TextBlockWidth / 2);

            foreach (DateTimeAxis dtAxis in axes)
            {
                if (lastDate.Date != current.Date)
                {
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;
                }
                dtAxis.CurrentDateTime = current;
                dtAxis.DateTimeTextBlock.Foreground = dtAxisForeground;
                Canvas.SetLeft(dtAxis, axisOffset);

                lastDate = current;
                current += timeSpan;
                axisOffset += _MinAxisOffset;
            }

            _IsScrollFromZoom = true;

            TimelineChart.TimeFrameFrom = displayStart;
            TimelineChart.TimeFrameTo = displayEnd;
            InvalidateGridLines();
        }

        #endregion

        #endregion
    }
}
