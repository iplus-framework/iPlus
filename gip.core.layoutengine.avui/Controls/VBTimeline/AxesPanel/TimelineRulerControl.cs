using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace gip.core.layoutengine.avui.timeline
{

    /// <summary>
    /// The Timeline Ruler control that displays date and time ruler.
    /// </summary>
    public class TimelineRulerControl : TemplatedControl
    {
        #region c'tors

        static TimelineRulerControl()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner<TimelineRulerControl>();
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner<TimelineRulerControl>();
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner<TimelineRulerControl>();
        }


        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ScrollViewer viewer = this._headerSV;
            this._headerSV = base.Parent as ScrollViewer;
            if (viewer != this._headerSV)
            {
                if (viewer != null)
                {
                    viewer.ScrollChanged -= OnHeaderScrollChanged;
                }
                if (this._headerSV != null)
                {
                    this._headerSV.ScrollChanged += OnHeaderScrollChanged;
                }
            }
            ScrollViewer viewer2 = this._mainSV;
            this._mainSV = base.TemplatedParent as ScrollViewer;
            if (viewer2 != this._mainSV)
            {
                if (viewer2 != null)
                {
                    viewer2.ScrollChanged -= OnMasterScrollChanged;
                }
                if (this._mainSV != null)
                {
                    this._mainSV.ScrollChanged += OnMasterScrollChanged;
                }
            }

        }
        #endregion

        #region StyledProperties

        /// <summary>
        /// Represents the styled property for MaximumDate.
        /// </summary>
        public static readonly StyledProperty<DateTime?> MaximumDateProperty =
            AvaloniaProperty.Register<TimelineRulerControl, DateTime?>(nameof(MaximumDate));

        /// <summary>
        /// Represents the styled property for MinimumDate.
        /// </summary>
        public static readonly StyledProperty<DateTime?> MinimumDateProperty =
            AvaloniaProperty.Register<TimelineRulerControl, DateTime?>(nameof(MinimumDate));

        /// <summary>
        /// Represents the styled property for TickTimeSpan.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> TickTimeSpanProperty =
            AvaloniaProperty.Register<TimelineRulerControl, TimeSpan>(nameof(TickTimeSpan));

        /// <summary>
        /// Represents the styled property for BlockTimeSpan.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> BlockTimeSpanProperty =
            AvaloniaProperty.Register<TimelineRulerControl, TimeSpan>(nameof(BlockTimeSpan), TimeSpan.FromDays(1));

        private static readonly StyledProperty<IList<RulerBlockItem>> RulerBlocksPropertyKey =
            AvaloniaProperty.Register<TimelineRulerControl, IList<RulerBlockItem>>(nameof(RulerBlocks));

        public static readonly StyledProperty<IList<RulerBlockItem>> RulerBlocksProperty = RulerBlocksPropertyKey;

        private static readonly StyledProperty<double> DpiPerBlockPropertyKey =
            AvaloniaProperty.Register<TimelineRulerControl, double>(nameof(DpiPerBlock), 1D);

        public static readonly StyledProperty<double> DpiPerBlockProperty = DpiPerBlockPropertyKey;

        /// <summary>
        /// Represents the styled property for TimeLinesContent.
        /// </summary>
        public static readonly StyledProperty<Canvas> TimeLinesContentProperty =
            AvaloniaProperty.Register<TimelineRulerControl, Canvas>(nameof(TimeLinesContent));

        #endregion

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

        private VBTimelineChartBase _vbTimelineChartBase;
        private VBTimelineChartBase _VBTimelineChartBase
        {
            get
            {
                if (_vbTimelineChartBase == null)
                    _vbTimelineChartBase = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineChartBase)) as VBTimelineChartBase;
                return _vbTimelineChartBase;
            }
        }

        public TimeSpan BlockTimeSpan
        {
            get { return GetValue(BlockTimeSpanProperty); }
            set { SetValue(BlockTimeSpanProperty, value); }
        }

        public IList<RulerBlockItem> RulerBlocks
        {
            get { return GetValue(RulerBlocksProperty); }
            private set { SetValue(RulerBlocksPropertyKey, value); }
        }

        public double DpiPerBlock
        {
            get { return GetValue(DpiPerBlockProperty); }
            private set { SetValue(DpiPerBlockPropertyKey, value); }
        }

        public TimelineRulerControl()
        {

        }

        private bool blockUpdatePending = false;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == MaximumDateProperty || change.Property == MinimumDateProperty)
            {
                blockUpdatePending = true;
                //UpdateRulerBlocks();
            }
            else if (change.Property == TickTimeSpanProperty)
            {
                TimeSpan ts = (TimeSpan)change.NewValue;
                if (ts.Ticks <= 0)
                {
                    SetValue(TickTimeSpanProperty, TimeSpan.FromMinutes(5));
                }
                updateDpiPerBlock = true;
            }
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
            if ((this._mainSV != null) && (this._headerSV == e.Source))
            {
                // Avalonia TODO: @Ivan - does this make sense?
                //this._mainSV.ScrollToHorizontalOffset(e.HorizontalOffset);
                _VBTimelineChartBase._PART_Line.CalculateDateTime();
            }
        }

        private void OnMasterScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((this._headerSV != null) && (this._mainSV == e.Source))
            {
                // Avalonia TODO: @Ivan - does this make sense?
                //this._headerSV.ScrollToHorizontalOffset(e.HorizontalOffset);

                _VBTimelineChartBase._PART_Line.CalculateDateTime();
            }
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
                    block.Span = EffectiveBlockTimeSpan;
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
            get { return GetValue(TimeLinesContentProperty); }
            set { SetValue(TimeLinesContentProperty, value); }
        }

        private int _RulerBlockCounter = 0;

        public void UpdateTimeLines(RulerBlockItem rbItem)
        {
            if (RulerBlocks.Contains(rbItem))
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
                    Line line = new Line() { StartPoint = new Point(0, 0), Fill = Brushes.White, Stroke = Brushes.White, Stretch = Stretch.Fill };

                    Binding binding = new Binding("Bounds.Height");
                    binding.Source = canvas;
                    MultiBinding multiBinding = new MultiBinding();
                    multiBinding.Bindings.Add(binding);
                    Binding staticBinding = new Binding();
                    staticBinding.Source = 0;
                    multiBinding.Bindings.Add(staticBinding); // X - Value
                    multiBinding.Bindings.Add(binding); // Y - Value
                    PointConverter pointConverter = new PointConverter();
                    multiBinding.Converter = pointConverter;
                    line.Bind(Line.EndPointProperty, multiBinding);

                    Canvas.SetLeft(line, offset);
                    canvas.Children.Add(line);
                }
            }

            TimeLinesContent = canvas;
        }

        #endregion

    }

    public class RulerBlockSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null
                || values.Count < 2
                || values[0] == AvaloniaProperty.UnsetValue
                || values[1] == AvaloniaProperty.UnsetValue)
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
