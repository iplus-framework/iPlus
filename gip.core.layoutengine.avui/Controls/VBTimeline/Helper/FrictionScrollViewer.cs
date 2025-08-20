using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;

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
        private DispatcherTimer animationTimer = new DispatcherTimer();
        private Point previousPoint;
        private Point scrollStartOffset;
        private Point scrollStartPoint;
        private Point scrollTarget;
        private Vector velocity;
        private Point autoScrollTarget;
        private bool shouldAutoScroll = false;
        #endregion

        #region Ctor
        /// <summary>
        /// Overrides metadata
        /// </summary>
        static FrictionScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FrictionScrollViewer),
            new FrameworkPropertyMetadata(typeof(FrictionScrollViewer)));
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            PART_scp = Template.FindName("PART_ScrollContentPresenter",this) as ScrollContentPresenter;
            PART_scp.SizeChanged += PART_scp_SizeChanged;
            base.OnApplyTemplate();
        }

        void PART_scp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           VBTimelineChartBase tc = WpfUtility.FindVisualParent<VBTimelineChartBase>(this);
            if (tc != null && PART_scp.ActualHeight > 30)
            {
                tc.LineY2 = PART_scp.ActualHeight;
                tc.LineXMax = PART_scp.ActualWidth -28;
            }
        }

        internal ScrollContentPresenter PART_scp;

        /// <summary>
        /// Initialises all friction related variables
        /// </summary>
        public FrictionScrollViewer()
        {
            Friction = 0.7;
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            animationTimer.Tick += HandleWorldTimerTick;
            animationTimer.Start();
        }
        #endregion

        #region DPs
        /// <summary>
        /// The ammount of friction to use. Use the Friction property to set a 
        /// value between 0 and 1, 0 being no friction 1 is full friction 
        /// meaning the panel won’t "auto-scroll".
        /// </summary>
        public double Friction
        {
            get { return (double)GetValue(FrictionProperty); }
            set { SetValue(FrictionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Friction.  
        public static readonly DependencyProperty FrictionProperty =
            DependencyProperty.Register("Friction", typeof(double),
            typeof(FrictionScrollViewer), new UIPropertyMetadata(0.0));
        #endregion

        #region overrides
        /// <summary>
        /// Get position and CaptureMouse
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (IsMouseOver && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                shouldAutoScroll = false;
                // Save starting point, used later when determining how much to scroll.
                scrollStartPoint = e.GetPosition(this);
                scrollStartOffset.X = HorizontalOffset;
                scrollStartOffset.Y = VerticalOffset;
                // Update the cursor if can scroll or not. 
                Cursor = (ExtentWidth > ViewportWidth) ||
                    (ExtentHeight > ViewportHeight) ?
                    Cursors.ScrollAll : Cursors.Arrow;
                CaptureMouse();
            }
            base.OnMouseDown(e);
        }


        /// <summary>
        /// If IsMouseCaptured scroll to correct position. 
        /// Where position is updated by animation timer
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                shouldAutoScroll = false;
                Point currentPoint = e.GetPosition(this);
                // Determine the new amount to scroll.
                Point delta = new Point(scrollStartPoint.X -
                    currentPoint.X, scrollStartPoint.Y - currentPoint.Y);
                scrollTarget.X = scrollStartOffset.X + delta.X;
                scrollTarget.Y = scrollStartOffset.Y + delta.Y;
                // Scroll to the new position.
                ScrollToHorizontalOffset(scrollTarget.X);
                ScrollToVerticalOffset(scrollTarget.Y);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                ScrollToHorizontalOffset(this.HorizontalOffset + e.Delta);
            else
                base.OnMouseWheel(e);
        }


        /// <summary>
        /// Release MouseCapture if its captured
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Cursor = Cursors.Arrow;
                ReleaseMouseCapture();
            }
            base.OnMouseUp(e);
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
            if (IsMouseCaptured)
            {
                Point currentPoint = Mouse.GetPosition(this);
                velocity = previousPoint - currentPoint;
                previousPoint = currentPoint;
            }
            else
            {
                if (shouldAutoScroll)
                {
                    Point currentScroll = new Point(ScrollInfo.HorizontalOffset + ScrollInfo.ViewportWidth / 2.0, ScrollInfo.VerticalOffset + ScrollInfo.ViewportHeight / 2.0);
                    Vector offset = autoScrollTarget - currentScroll;
                    shouldAutoScroll = offset.Length > 2.0;

                    // FIXME: 10.0 here is the scroll speed factor, a higher value means slower auto-scroll, 1 means no animation
                    ScrollToHorizontalOffset(HorizontalOffset + offset.X / 10);
                    ScrollToVerticalOffset(VerticalOffset + offset.Y / 10);
                }
                else
                {
                    if (velocity.Length > 1)
                    {
                        ScrollToHorizontalOffset(scrollTarget.X);
                        ScrollToVerticalOffset(scrollTarget.Y);
                        scrollTarget.X += velocity.X;
                        scrollTarget.Y += velocity.Y;
                        velocity *= Friction;
                        //System.Diagnostics.Debug.WriteLine("Scroll @ " + ScrollInfo.HorizontalOffset + ", " + ScrollInfo.VerticalOffset);
                    }
                }

                InvalidateScrollInfo();
                InvalidateVisual();
            }
        }
        #endregion

        public Point AutoScrollTarget
        {
            set 
            {
                autoScrollTarget = value;
                shouldAutoScroll = true;
            }
        }


        public void ScrollToCenterTarget(Point target)
        {
            ScrollToHorizontalOffset(target.X - ScrollInfo.ViewportWidth / 2.0);
            ScrollToVerticalOffset(target.Y - ScrollInfo.ViewportHeight / 2.0);
        }


    }
}
