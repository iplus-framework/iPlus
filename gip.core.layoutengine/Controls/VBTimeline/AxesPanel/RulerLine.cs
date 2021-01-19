using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using gip.ext.graphics.shapes;
using System.Windows;
using System.Windows.Media;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The ruler line control.
    /// </summary>
    public class RulerLine : ArrowLine 
    {
        public RulerLine() : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            this.IsVisibleChanged += RulerLine_IsVisibleChanged;
            base.OnInitialized(e);
        }

        void RulerLine_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CalculateDateTime();
        }

        Point anchorPoint;
        Point currentPoint;
        bool _IsInDrag = false;

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            anchorPoint = e.GetPosition(TemplatedParent as FrameworkElement);
            this.CaptureMouse();
            _IsInDrag = true;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_IsInDrag)
            {
                this.ReleaseMouseCapture();
                _IsInDrag = false;
                e.Handled = true;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            this.ReleaseMouseCapture();
            _IsInDrag = false;
            base.OnLostFocus(e);
        }

        public void DeInitVBControl()
        {
            this.IsVisibleChanged -= RulerLine_IsVisibleChanged;
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
                if(_TimelineRuler == null)
                    _TimelineRuler = WpfUtility.FindVisualChild<TimelineRulerControl>(_VBTimelineChart);
                return _TimelineRuler;
            }
        }

        private double _RulerBoundMin
        {
            get
            {
                return (_VBTimelineChart.ActualWidth / 2 - 5)*-1;
            }
        }

        private double _RulerBoundMax
        {
            get
            {
                if(_VBTimelineChart.ScrollViewer.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
                    return _VBTimelineChart.ActualWidth / 2 - 20;
                return _VBTimelineChart.ActualWidth / 2 - 5;
            }
        }

        private TranslateTransform transform = new TranslateTransform();
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (_IsInDrag)
            {
                if (transform.X >= _RulerBoundMin && transform.X <= _RulerBoundMax)
                {
                    currentPoint = e.GetPosition(TemplatedParent as FrameworkElement);
                    transform.X += currentPoint.X - anchorPoint.X;
                    if(this.RenderTransform != transform)
                        this.RenderTransform = transform;
                    CalculateDateTime();
                    anchorPoint = currentPoint;
                }
                else if (transform.X < 0)
                {
                    transform.X = _RulerBoundMin;
                    _VBTimelineChart.scrollViewer.ScrollToHorizontalOffset(_VBTimelineChart.scrollViewer.HorizontalOffset + (transform.X/4));
                }

                else
                {
                    transform.X = _RulerBoundMax;
                    _VBTimelineChart.scrollViewer.ScrollToHorizontalOffset(_VBTimelineChart.scrollViewer.HorizontalOffset + (transform.X/4));
                }
                    
            }
        }

        public static readonly DependencyProperty TimeProperty
            = DependencyProperty.Register("Time", typeof(DateTime), typeof(RulerLine));
        public DateTime Time
        {
            get
            {
                return (DateTime)GetValue(TimeProperty);
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }

        public void CalculateDateTime()
        {
            if (Visibility == System.Windows.Visibility.Visible && _TimelineRulerControl != null)
            {
                Point currentPos = this.TransformToVisual(_TimelineRulerControl).Transform(new Point());
                Time = Timeline.OffsetToDate(currentPos.X, _TimelineRulerControl);
            }
        }

        public void ArrangeLine()
        {
            if (transform != null && IsVisible)
                transform.X = 0;
        }
    }
}
