using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.timeline;
using gip.ext.graphics.avui.shapes;
using System;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// The ruler line control.
    /// </summary>
    public class RulerLine : ArrowLine
    {
        public RulerLine() : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsVisibleProperty)
            {
                CalculateDateTime();
            }
            base.OnPropertyChanged(change);
        }

        Point anchorPoint;
        Point currentPoint;
        bool _IsInDrag = false;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                anchorPoint = e.GetPosition(TemplatedParent as Control);
                e.Pointer.Capture(this);
                _IsInDrag = true;
                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && _IsInDrag)
            {
                if (e.Pointer.Captured == this)
                    e.Pointer.Capture(null);
                _IsInDrag = false;
                e.Handled = true;
            }
            base.OnPointerReleased(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (_IsInDrag)
            {
                // In Avalonia, simply clear the capture by setting it to null
                // The control will automatically release any captured pointers
                _IsInDrag = false;
            }
            base.OnLostFocus(e);
        }

        private VBTimelineChartBase _VBTimelineChart
        {
            get { return TemplatedParent as VBTimelineChartBase; }
        }

        private TimelineRulerControl _TimelineRuler;
        private TimelineRulerControl _TimelineRulerControl
        {
            get
            {
                if (_TimelineRuler == null)
                    _TimelineRuler = VBVisualTreeHelper.FindChildObjects<TimelineRulerControl>(_VBTimelineChart)?.FirstOrDefault();
                return _TimelineRuler;
            }
        }

        private double RulerBoundMin
        {
            get
            {
                return (_VBTimelineChart.Bounds.Width / 2 - 5) * -1;
            }
        }

        private double RulerBoundMax
        {
            get
            {
                if (_VBTimelineChart.ScrollViewer.VerticalScrollBarVisibility == Avalonia.Controls.Primitives.ScrollBarVisibility.Visible)
                    return _VBTimelineChart.Bounds.Width / 2 - 20;
                return _VBTimelineChart.Bounds.Width / 2 - 5;
            }
        }

        private TranslateTransform transform = new TranslateTransform();

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_IsInDrag)
            {
                if (transform.X >= RulerBoundMin && transform.X <= RulerBoundMax)
                {
                    currentPoint = e.GetPosition(TemplatedParent as Control);
                    transform.X += currentPoint.X - anchorPoint.X;
                    if (this.RenderTransform != transform)
                        this.RenderTransform = transform;
                    CalculateDateTime();
                    anchorPoint = currentPoint;
                }
                else if (transform.X < 0)
                {
                    transform.X = RulerBoundMin;
                    var offset = _VBTimelineChart.ScrollViewer.Offset;
                    var newOffset = new Vector(
                        offset.X + (transform.X / 4),
                        offset.Y
                    );
                    _VBTimelineChart.ScrollViewer.SetCurrentValue(
                        ScrollViewer.OffsetProperty,
                        newOffset
                    );
                }
                else
                {
                    transform.X = RulerBoundMax;
                    var offset = _VBTimelineChart.ScrollViewer.Offset;
                    var newOffset = new Vector(
                        offset.X + (transform.X / 4),
                        offset.Y
                    );
                    _VBTimelineChart.ScrollViewer.SetCurrentValue(
                        ScrollViewer.OffsetProperty,
                        newOffset
                    );
                }
            }
            base.OnPointerMoved(e);
        }

        public static readonly StyledProperty<DateTime> TimeProperty
            = AvaloniaProperty.Register<RulerLine, DateTime>(nameof(Time));
        public DateTime Time
        {
            get
            {
                return GetValue(TimeProperty);
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }

        public void CalculateDateTime()
        {
            if (IsVisible && _TimelineRulerControl != null)
            {
                var transformedPoint = this.TransformToVisual(_TimelineRulerControl);
                if (transformedPoint != null)
                {
                    Point currentPos = transformedPoint.Value.Transform(new Point());
                    Time = Timeline.OffsetToDate(currentPos.X, _TimelineRulerControl);
                }
            }
        }

        public void ArrangeLine()
        {
            if (transform != null && IsVisible)
                transform.X = 0;
        }
    }
}
