using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace gip.core.layoutengine.timeline
{

    /// <summary>
    /// The Timeline Ruler control that displays date and time ruler.
    /// </summary>
    public class TimelineRulerControl : Control
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TimelineRulerControlStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/TimelineRulerControlStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TimelineRulerControlStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/TimelineRulerControlStyleAero.xaml" },
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

        static TimelineRulerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineRulerControl),
              new FrameworkPropertyMetadata(typeof(TimelineRulerControl)));

            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(Timeline.TickTimeSpanDefaultValue, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, TicksTimeSpanChanged, TicksTimeSpanCoerce));
        }

        bool _themeApplied = false;
        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);

            ScrollViewer viewer = this._headerSV;
            this._headerSV = base.Parent as ScrollViewer;
            if (viewer != this._headerSV)
            {
                if (viewer != null)
                {
                    viewer.ScrollChanged -= new ScrollChangedEventHandler(this.OnHeaderScrollChanged);
                }
                if (this._headerSV != null)
                {
                    this._headerSV.ScrollChanged += new ScrollChangedEventHandler(this.OnHeaderScrollChanged);
                }
            }
            ScrollViewer viewer2 = this._mainSV;
            this._mainSV = base.TemplatedParent as ScrollViewer;
            if (viewer2 != this._mainSV)
            {
                if (viewer2 != null)
                {
                    viewer2.ScrollChanged -= new ScrollChangedEventHandler(this.OnMasterScrollChanged);
                }
                if (this._mainSV != null)
                {
                    this._mainSV.ScrollChanged += new ScrollChangedEventHandler(this.OnMasterScrollChanged);
                }
            }

        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #endregion

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

        private VBTimelineChartBase _vbTimelineChartBase;
        private VBTimelineChartBase _VBTimelineChartBase
        {
            get 
            {
                if (_vbTimelineChartBase == null)
                    _vbTimelineChartBase = WpfUtility.FindVisualParent<VBTimelineChartBase>(this) as VBTimelineChartBase;
                return _vbTimelineChartBase;
            }
        }

        public TimeSpan BlockTimeSpan
        {
            get { return (TimeSpan)GetValue(BlockTimeSpanProperty); }
            set { SetValue(BlockTimeSpanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockTimeSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockTimeSpanProperty =
          DependencyProperty.Register("BlockTimeSpan", typeof(TimeSpan), typeof(TimelineRulerControl), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));


        private static readonly DependencyPropertyKey RulerBlocksPropertyKey =
        DependencyProperty.RegisterReadOnly("RulerBlocks", typeof(IList<RulerBlockItem>),
        typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty RulerBlocksProperty =
        RulerBlocksPropertyKey.DependencyProperty;

        public IList<RulerBlockItem> RulerBlocks
        {
            get { return (IList<RulerBlockItem>)GetValue(RulerBlocksProperty); }
            private set { SetValue(RulerBlocksPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey DpiPerBlockPropertyKey =
        DependencyProperty.RegisterReadOnly("DpiPerBlock", typeof(double),
        typeof(TimelineRulerControl), new FrameworkPropertyMetadata(1D));

        public static readonly DependencyProperty DpiPerBlockProperty =
        DpiPerBlockPropertyKey.DependencyProperty;

        public double DpiPerBlock
        {
            get { return (double)GetValue(DpiPerBlockProperty); }
            private set { SetValue(DpiPerBlockPropertyKey, value); }
        }

        public TimelineRulerControl()
        {

        }

        private bool blockUpdatePending = false;

        private static void TimeframeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimelineRulerControl ruler = (TimelineRulerControl)o;
            ruler.blockUpdatePending = true;
            //ruler.UpdateRulerBlocks();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            MeasureEffectiveBlockSpan();

            if (blockUpdatePending)
            {
                blockUpdatePending = false;
                UpdateRulerBlocks();
            }

            return base.MeasureOverride(constraint);
        }

        private ScrollViewer _headerSV;
        private ScrollViewer _mainSV;

        private void OnHeaderScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((this._mainSV != null) && (this._headerSV == e.OriginalSource))
            {
                this._mainSV.ScrollToHorizontalOffset(e.HorizontalOffset);

                _VBTimelineChartBase._PART_Line.CalculateDateTime();
            }
        }

        private void OnMasterScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((this._headerSV != null) && (this._mainSV == e.OriginalSource))
            {
                this._headerSV.ScrollToHorizontalOffset(e.HorizontalOffset);

                _VBTimelineChartBase._PART_Line.CalculateDateTime();
            }
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
            TimelineRulerControl ruler = (TimelineRulerControl)o;
            ruler.updateDpiPerBlock = true;
        }

        private Nullable<TimeSpan> effectiveBlockSpan;
        private bool updateDpiPerBlock = false;

        private void MeasureEffectiveBlockSpan()
        {
            if (updateDpiPerBlock)
            {
                updateDpiPerBlock = false;
                effectiveBlockSpan = null;

                double pixelPerTick = 1D / TickTimeSpan.Ticks;
                DpiPerBlock = pixelPerTick * EffectiveBlockTimeSpan.Ticks;

                UpdateRulerBlocks();
            }
        }

        private TimeSpan EffectiveBlockTimeSpan
        {
            get
            {
                if (effectiveBlockSpan == null)
                {
                    double pixelPerTick = 1D / TickTimeSpan.Ticks;
                    long ticks = (long)(150 / pixelPerTick);
                    effectiveBlockSpan = TimeSpan.FromTicks(ticks);

                    // NACHON: Added to show nicer timelineRuler                     
                    foreach (var blockFactor in BlockFactors())
                    {
                        if (TickTimeSpan <= TimeSpan.FromSeconds(blockFactor.TotalMinutes))
                        {
                            effectiveBlockSpan = blockFactor;
                            break;
                        }
                    }

                    // End NACHON
                }

                return effectiveBlockSpan.Value;
            }
        }

        private IEnumerable<TimeSpan> BlockFactors()
        {
            yield return TimeSpan.FromMinutes(15);
            yield return TimeSpan.FromMinutes(30);
            yield return TimeSpan.FromMinutes(45);
            yield return TimeSpan.FromHours(1);
            yield return TimeSpan.FromMinutes(90);
            yield return TimeSpan.FromHours(2);
            yield return TimeSpan.FromHours(4);
            yield return TimeSpan.FromHours(8);
            yield return TimeSpan.FromHours(16);
            yield return TimeSpan.FromDays(1);
            yield return TimeSpan.FromDays(2);
            yield return TimeSpan.FromDays(3);
            yield return TimeSpan.FromDays(4);
            yield return TimeSpan.FromDays(5);
            yield return TimeSpan.FromDays(6);
            yield return TimeSpan.FromDays(7);
            yield return TimeSpan.FromDays(30);
        }

        private readonly List<RulerBlockItem> EmptyRulerBlockList = new List<RulerBlockItem>();

        private void UpdateRulerBlocks()
        {
            if (MinimumDate == null || MaximumDate == null)
            {
                // Clear all block
                RulerBlocks = EmptyRulerBlockList;
            }
            else
            {
                TimeSpan timeframe = MaximumDate.Value - MinimumDate.Value;
                int totalBlocks = (int)Math.Ceiling((double)(timeframe.Ticks / EffectiveBlockTimeSpan.Ticks));
                totalBlocks++;

                if (totalBlocks > 2000)
                {
#if DEBUG
                    Debug.WriteLine("Because we do not support virtualization for TimelineRulerControl yet the number of blocks was limit to 2000");
#endif
                    totalBlocks = 2000;
                }

                List<RulerBlockItem> blocks = new List<RulerBlockItem>();

                TimeSpan spanFromStart = EffectiveBlockTimeSpan;
                DateTime prev = MinimumDate.Value;

                for (int blockIdx = 0; blockIdx < totalBlocks; blockIdx++)
                {
                    DateTime current = MinimumDate.Value.Add(spanFromStart);

                    RulerBlockItem block = new RulerBlockItem(this, blockIdx);
                    block.Start = prev;
                    block.Span =  EffectiveBlockTimeSpan;
                    //block.Text = prev.ToString();
                    blocks.Add(block);

                    prev = current;
                    spanFromStart = spanFromStart.Add(EffectiveBlockTimeSpan);
                }

                _VBTimelineChartBase._PART_Line.CalculateDateTime();

                if (RulerBlocks != null)
                    DeInitRulerBlocks();

                RulerBlocks = blocks;
                _RulerBlockCounter = 0;
                TimeLinesContent = null;
            }
        }

        private void DeInitRulerBlocks()
        {
            foreach (RulerBlockItem rbi in RulerBlocks)
                rbi.DeInitControl();
        }


        #region RulerBlockTimeLines

        public Canvas TimeLinesContent
        {
            get { return (Canvas)GetValue(TimeLinesContentProperty); }
            set { SetValue(TimeLinesContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeLinesContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeLinesContentProperty =
            DependencyProperty.Register("TimeLinesContent", typeof(Canvas), typeof(TimelineRulerControl));

        private int _RulerBlockCounter = 0;

        public void UpdateTimeLines(RulerBlockItem rbItem)
        {
            if(RulerBlocks.Contains(rbItem))
                _RulerBlockCounter++;

            if (RulerBlocks.Count != _RulerBlockCounter)
                return; ;

            var canvas = new Canvas();
            foreach (RulerBlockItem rbi in RulerBlocks)
            {
                if (rbi.RulerBlockTimeItemsOffsets == null)
                    continue;

                foreach (var offset in rbi.RulerBlockTimeItemsOffsets)
                {
                    Line line = new Line() { Y1 = 0, Fill = Brushes.White, Stroke = Brushes.White, Stretch = Stretch.Fill };

                    Binding binding = new Binding();
                    binding.Source = canvas;
                    binding.Path = new PropertyPath("ActualHeight");
                    line.SetBinding(Line.Y2Property, binding);

                    Canvas.SetLeft(line, offset);
                    canvas.Children.Add(line);
                }
            }

            TimeLinesContent = canvas;
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(TimeLinesContentProperty, null, canvas));
        }

        #endregion

    }

    public class RulerBlockSizeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2 ||
              values[0] == DependencyProperty.UnsetValue ||
              values[1] == DependencyProperty.UnsetValue)
            {
                return 0.0d;
            }

            TimeSpan tickTimeSpan = (TimeSpan)values[0];
            double pixelPerTick = 1D / tickTimeSpan.Ticks;
            TimeSpan span = (TimeSpan)values[1];

            double width = span.Ticks * pixelPerTick;

            return width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
