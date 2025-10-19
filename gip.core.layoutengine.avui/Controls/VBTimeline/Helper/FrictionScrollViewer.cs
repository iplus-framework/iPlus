using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using gip.core.layoutengine.avui.Helperclasses;
using System;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// Provides a scrollable ScrollViewer which
    /// allows user to apply friction, which in turn
    /// animates the ScrollViewer position, giving it
    /// the appearance of sliding into position
    /// 
    /// Code from the address http://www.codeproject.com/KB/WPF/SpiderControl.aspx
    /// </summary>
    public class FrictionScrollViewer : VBScrollViewer
    {
        #region Data

        // Used when manually scrolling.
        private DispatcherTimer _AnimationTimer = new DispatcherTimer();
        private Point _PreviousPoint;
        private Point _ScrollStartOffset;
        private Point _ScrollStartPoint;
        private Point _ScrollTarget;
        private Vector _Velocity;
        private Point _AutoScrollTarget;
        private bool _ShouldAutoScroll = false;
        private bool _isPointerCaptured = false;
        #endregion

        #region Ctor

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            PART_scp = e.NameScope.Find("PART_ScrollContentPresenter") as ScrollContentPresenter;
            if (PART_scp != null)
                PART_scp.SizeChanged += PART_scp_SizeChanged;
            base.OnApplyTemplate(e);
        }

        void PART_scp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VBTimelineChartBase tc = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineChartBase)) as VBTimelineChartBase;
            if (tc != null && PART_scp.Bounds.Height > 30)
            {
                tc.LineY2 = PART_scp.Bounds.Height;
                tc.LineXMax = PART_scp.Bounds.Width - 28;
            }
        }

        internal ScrollContentPresenter PART_scp;

        /// <summary>
        /// Initialises all friction related variables
        /// </summary>
        public FrictionScrollViewer()
        {
            Friction = 0.7;
            _AnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            _AnimationTimer.Tick += HandleWorldTimerTick;
            _AnimationTimer.Start();
        }
        #endregion

        #region StyledProperties
        /// <summary>
        /// The ammount of friction to use. Use the Friction property to set a 
        /// value between 0 and 1, 0 being no friction 1 is full friction 
        /// meaning the panel won't "auto-scroll".
        /// </summary>
        public double Friction
        {
            get { return GetValue(FrictionProperty); }
            set { SetValue(FrictionProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Friction.
        /// </summary>
        public static readonly StyledProperty<double> FrictionProperty =
            AvaloniaProperty.Register<FrictionScrollViewer, double>(nameof(Friction), 0.0);
        #endregion

        #region overrides
        /// <summary>
        /// Get position and capture pointer
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed && !e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                _ShouldAutoScroll = false;
                _isPointerCaptured = true;
                // Save starting point, used later when determining how much to scroll.
                _ScrollStartPoint = e.GetPosition(this);
                _ScrollStartOffset = new Point(Offset.X, Offset.Y);
                // Update the cursor if can scroll or not. 
                Cursor = (Extent.Width > Viewport.Width) ||
                    (Extent.Height > Viewport.Height) ?
                    new Cursor(StandardCursorType.SizeAll) : new Cursor(StandardCursorType.Arrow);
                e.Pointer.Capture(this);
            }
            base.OnPointerPressed(e);
        }


        /// <summary>
        /// If pointer is captured scroll to correct position. 
        /// Where position is updated by animation timer
        /// </summary>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_isPointerCaptured && e.Pointer.Captured == this)
            {
                _ShouldAutoScroll = false;
                Point currentPoint = e.GetPosition(this);
                // Determine the new amount to scroll.
                Point delta = new Point(_ScrollStartPoint.X -
                    currentPoint.X, _ScrollStartPoint.Y - currentPoint.Y);
                _ScrollTarget = new Point(_ScrollStartOffset.X + delta.X, _ScrollStartOffset.Y + delta.Y);
                // Scroll to the new position.
                var newOffset = new Vector(_ScrollTarget.X, _ScrollTarget.Y);
                SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
            }
            base.OnPointerMoved(e);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var newOffset = new Vector(Offset.X + e.Delta.Y * 50, Offset.Y);
                SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
            }
            else
                base.OnPointerWheelChanged(e);
        }


        /// <summary>
        /// Release pointer capture if its captured
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_isPointerCaptured)
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                if (e.Pointer.Captured == this)
                    e.Pointer.Capture(null);
                _isPointerCaptured = false;
            }
            base.OnPointerReleased(e);
        }
        #endregion

        #region Animation timer Tick
        /// <summary>
        /// Animation timer tick, used to move the scrollviewer incrementally
        /// to the desired position. This also uses the friction setting
        /// when determining how much to move the scrollviewer
        /// </summary>
        private void HandleWorldTimerTick(object sender, EventArgs e)
        {
            if (_isPointerCaptured)
            {
                // Note: In Avalonia, we don't have a direct equivalent to Mouse.GetPosition(this)
                // The pointer position tracking is handled in the pointer event handlers
                _Velocity = _PreviousPoint - _ScrollStartPoint;
                _PreviousPoint = _ScrollStartPoint;
            }
            else
            {
                if (_ShouldAutoScroll)
                {
                    Point currentScroll = new Point(Offset.X + Viewport.Width / 2.0, Offset.Y + Viewport.Height / 2.0);
                    Vector offset = _AutoScrollTarget - currentScroll;
                    _ShouldAutoScroll = offset.Length > 2.0;

                    // FIXME: 10.0 here is the scroll speed factor, a higher value means slower auto-scroll, 1 means no animation
                    var newOffset = new Vector(Offset.X + offset.X / 10, Offset.Y + offset.Y / 10);
                    SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
                }
                else
                {
                    if (_Velocity.Length > 1)
                    {
                        var newOffset = new Vector(_ScrollTarget.X, _ScrollTarget.Y);
                        SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
                        _ScrollTarget = new Point(_Velocity.X, _Velocity.Y);
                        _Velocity *= Friction;
                        //System.Diagnostics.Debug.WriteLine("Scroll @ " + Offset.X + ", " + Offset.Y);
                    }
                }

                InvalidateVisual();
            }
        }
        #endregion

        public Point AutoScrollTarget
        {
            set
            {
                _AutoScrollTarget = value;
                _ShouldAutoScroll = true;
            }
        }


        public void ScrollToCenterTarget(Point target)
        {
            var newOffset = new Vector(target.X - Viewport.Width / 2.0, target.Y - Viewport.Height / 2.0);
            SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
        }
    }
}
