using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace gip.core.layoutengine.avui.timeline
{
    public class DateTimeAxesPanel : Canvas, INotifyPropertyChanged
    {
        #region c'tors

        static DateTimeAxesPanel()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(DateTimeAxesPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(DateTimeAxesPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(DateTimeAxesPanel), new FrameworkPropertyMetadata(Timeline.TickTimeSpanDefaultValue, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, TicksTimeSpanChanged, TicksTimeSpanCoerce));
        }

        public DateTimeAxesPanel()
        {

        }

        internal void InitControl(bool forceInit = false)
        {
            if (!MinimumDate.HasValue || !MaximumDate.HasValue || this.ActualWidth <= 0)
                return;

            if (!forceInit && _LastWidth == this.ActualWidth && Children.Count > 0)
                return;

            Children.Clear();
            _TimeframeDateTimes = new List<DateTime>();

            int numberOfAxes = (int)this.ActualWidth / _MinAxisOffset;

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
                if(lastDate.Date != calcDate.Date)
                {
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;
                }

                DateTimeAxis axis = new DateTimeAxis(this) { AxisHeight = this.ActualHeight };
                _TimeframeDateTimes.Add(calcDate);
                axis.CurrentDateTime = calcDate;
                axis.DateTimeTextBlock.Foreground = dtAxisForeground;
                lastDate = calcDate;

                if (defaultOffset < -(TextBlockWidth / 2))
                    defaultOffset = this.ActualWidth;

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
                TimelinePanel timelinePanel = WpfUtility.FindVisualChild<TimelinePanel>(TimelineChart.itemsPresenter);
                if (timelinePanel != null)
                    timelinePanel.MaximumDate = MaximumDate;
                _IsFirstInit = false;
            }

            _LastWidth = this.ActualWidth;
        }

        internal void ClearControl()
        {
            Children.Clear();
        }

        public void DeInitControl()
        {
            BindingOperations.ClearAllBindings(this);
            foreach (DateTimeAxis axis in Children)
                axis.DeInitControl();
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

        static SolidColorBrush _dtAxisColor1 = new SolidColorBrush(ControlManager.WpfTheme == eWpfTheme.Gip ? Colors.White : Colors.Black);

        static SolidColorBrush _dtAxisColor2= new SolidColorBrush(ControlManager.WpfTheme == eWpfTheme.Gip ? Colors.Orange : Colors.Blue);

        //temp fix 
        private double _TempZoomOffset = 0;

        #endregion

        #region Properties

        public static readonly DependencyProperty MaximumDateProperty;
        public static readonly DependencyProperty MinimumDateProperty;
        public static readonly DependencyProperty TickTimeSpanProperty;

        public Nullable<DateTime> MaximumDate
        {
            get { return (Nullable<DateTime>)GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        public Nullable<DateTime> MinimumDate
        {
            get { return (Nullable<DateTime>)GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        public TimeSpan TickTimeSpan
        {
            get { return (TimeSpan)GetValue(TickTimeSpanProperty); }
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

        public double AxesOffset
        {
            get;
            set;
        }

        public double ScrollViewerHorizontalOffset
        {
            get { return (double)GetValue(ScrollViewerHorizontalOffsetProperty); }
            set { SetValue(ScrollViewerHorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerHorizontalOffsetProperty =
            DependencyProperty.Register("ScrollViewerHorizontalOffset", typeof(double), typeof(DateTimeAxesPanel), new PropertyMetadata(0.0, new PropertyChangedCallback(OnOffsetChanged)));

        private List<TimeSpan> _LogicalDateTimeSpans;
        private List<TimeSpan> LogicalDateTimeSpans
        {
            get
            {
                if(_LogicalDateTimeSpans == null)
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

        public Thickness ZoomBorderMargin
        {
            get;
            set;
        }

        public double ZoomRectWidth
        {
            get;
            set;
        }

        public HorizontalAlignment ZoomBorderAlignment
        {
            get;
            set;
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InitControl();
        }

        private static void TimeframeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object TicksTimeSpanCoerce(DependencyObject d, object baseValue)
        {
            TimeSpan ts = (TimeSpan)baseValue;
            if (ts.Ticks <= 0)
            {
                return TimeSpan.FromMinutes(5);
            }
            else
            {
                return baseValue;
            }
        }

        private static void TicksTimeSpanChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        private DateTime FindNearestLogicalDateTime(DateTime currentDT)
        {
            return _TimeframeDateTimes.FirstOrDefault(c => c >= currentDT);
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimeAxesPanel thisControl = d as DateTimeAxesPanel;

            var newOffset = (double)e.NewValue - thisControl._LastOffset;
            if (newOffset == 0)
                return;

            if (thisControl._IsScrollFromZoom)
            {
                thisControl._IsScrollFromZoom = false;
                thisControl._LastOffset = (double)e.NewValue;

                if (thisControl._TempZoomOffset != thisControl.TimelineChart.ScrollViewer.HorizontalOffset)
                    thisControl.TimelineChart.ScrollViewer.ScrollToHorizontalOffset(thisControl._TempZoomOffset);

                return;
            }

            var newPosition = Canvas.GetLeft(thisControl.StartAxis) - newOffset;

            if (newPosition - thisControl.StartAxis.DefaultPosition >= thisControl._MinAxisOffset-1 || newPosition - thisControl.StartAxis.DefaultPosition <= -thisControl._MinAxisOffset+1)
            {
                DateTime currentDT = Timeline.OffsetToDate((double)e.NewValue, thisControl.TimelineChart.itemsPresenter);
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
                if(reverse && dtAxis != null && startDT.Date != dtAxis.CurrentDateTime.Date)
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;

                foreach (DateTimeAxis axis in thisControl.Children)
                {
                    axis.SetPosition(axis.DefaultPosition + offsetCorrection);

                    if (indexOfStartDT < maxTFDT)
                    {
                        DateTime current = thisControl._TimeframeDateTimes[indexOfStartDT];
                        if(lastDate.HasValue && lastDate.Value.Date != current.Date)
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
            }
            else
            {
                foreach (DateTimeAxis axis in thisControl.Children)
                    axis.SetPosition(Canvas.GetLeft(axis) - newOffset);
            }

            thisControl._LastOffset = (double)e.NewValue;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Methods => TimeSpan

        private TimeSpan DetermineDateTimeSpan(int maxAxis, DateTime start, DateTime end)
        {
            TimeSpan timeframe = end - start;
            
            foreach(TimeSpan logicalTS in LogicalDateTimeSpans)
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

        internal void OnZoomStart(MouseButtonEventArgs e)
        {
            _IsZoomCaptured = true;
            lastMousePos = e.GetPosition(this).X + 25;
            startMousePos = e.GetPosition(TimelineChart.itemsPresenter).X + 25;
            ZoomRectWidth = 0;
            OnPropertyChanged("ZoomRectWidth");
            (this.Parent as FrameworkElement).Cursor = Cursors.SizeWE;
        }

        internal void OnZoomMove(MouseEventArgs e)
        {
            if (!_IsZoomCaptured)
                return;

            double newPosition = e.GetPosition(this).X + 25;
            double offset = newPosition - lastMousePos;

            if (offset > 0)
            {
                if (ZoomBorderAlignment != HorizontalAlignment.Left)
                {
                    ZoomBorderAlignment = HorizontalAlignment.Left;
                    OnPropertyChanged("ZoomBorderAlignment");
                }

                ZoomBorderMargin = new Thickness(lastMousePos, 20, 0, 0);
                OnPropertyChanged("ZoomBorderMargin");
            }
            else if(offset < 0)
            {
                if (ZoomBorderAlignment != HorizontalAlignment.Right)
                {
                    ZoomBorderAlignment = HorizontalAlignment.Right;
                    OnPropertyChanged("ZoomBorderAlignment");
                }

                ZoomBorderMargin = new Thickness(0, 20, this.ActualWidth - lastMousePos, 0);
                OnPropertyChanged("ZoomBorderMargin");
            }
            
            ZoomRectWidth = Math.Abs(offset);
            OnPropertyChanged("ZoomRectWidth");
        }

        internal void OnZoomEnd(MouseButtonEventArgs e)
        {
            if (_IsZoomCaptured)
            {
                ZoomRectWidth = 0;
                OnPropertyChanged("ZoomRectWidth");
                (this.Parent as FrameworkElement).Cursor = Cursors.Arrow;
                var pos = e.GetPosition(TimelineChart.itemsPresenter).X;
                DoZoom(startMousePos, pos);
                _IsZoomCaptured = false;
            }
        }

        internal void OnZoomOut(MouseButtonEventArgs e)
        {
            startMousePos = e.GetPosition(TimelineChart.itemsPresenter).X + 25;
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

            var temp = date1;
            if (date1 > date2)
            {
                date1 = date2;
                date2 = temp;
            }

            if (date2 - date1 <= TimeSpan.FromSeconds(1))
                return;

            double? diff = null, diff2 = null;

            foreach(DateTime dt in _TimeframeDateTimes)
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

            TimeSpan ts = DetermineDateTimeSpan((int)this.ActualWidth / _MinAxisOffset + 1, date1, date2);

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

            _TimeframeDateTimes = GenerateTimeframeDateTimes(_StartDateTime, MaximumDate.Value, _CurrentTimeSpan);

            UpdateDateTimeAxes(date1, date2, _CurrentTimeSpan);
        }

        private List<DateTime> GenerateTimeframeDateTimes(DateTime start, DateTime end, TimeSpan timeSpan)
        {
            List<DateTime> datetimes = new List<DateTime>();
            DateTime current = start;
            for(; ; )
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

            _TempZoomOffset = Timeline.DateToOffset(displayStart, TimelineChart.itemsPresenter);
            TimelineChart.ScrollViewer.ScrollToHorizontalOffset(_TempZoomOffset);
            _LastOffset = _TempZoomOffset;

            DateTime current = displayStart;
            double offset = 0;
            DateTime lastDate = _TimeframeDateTimes.FirstOrDefault();
            SolidColorBrush dtAxisForeground = _dtAxisColor1;

            if (Children[0] is DateTimeAxis startAxis)
                offset = Timeline.DateToOffset(current, TimelineChart.itemsPresenter) - _TempZoomOffset - (TextBlockWidth / 2);

            foreach (DateTimeAxis dtAxis in Children)
            {
                if (lastDate.Date != current.Date)
                {
                    dtAxisForeground = dtAxisForeground == _dtAxisColor1 ? _dtAxisColor2 : _dtAxisColor1;
                }
                dtAxis.CurrentDateTime = current;
                dtAxis.DateTimeTextBlock.Foreground = dtAxisForeground;
                Canvas.SetLeft(dtAxis, offset);

                lastDate = current;
                current += timeSpan;
                offset += _MinAxisOffset;
            }

            _IsScrollFromZoom = true;

            TimelineChart.TimeFrameFrom = displayStart;
            TimelineChart.TimeFrameTo = displayEnd;
        }

        #endregion

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
