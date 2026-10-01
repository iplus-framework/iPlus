using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Linq;

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
            // Support both the WPF-style name and Avalonia's default part name.
            PART_scp = e.NameScope.Find("PART_ScrollContentPresenter") as ScrollContentPresenter
                       ?? e.NameScope.Find("PART_ContentPresenter") as ScrollContentPresenter;
            if (PART_scp != null)
                PART_scp.SizeChanged += PART_scp_SizeChanged;
            base.OnApplyTemplate(e);

            // Diagnostics: listen at the visual ROOT (tunneling) to see whether
            // wheel events occur at all and where they are targeted.
            if (VisualRoot is Avalonia.Interactivity.Interactive rootInteractive)
            {
                rootInteractive.AddHandler(InputElement.PointerWheelChangedEvent,
                    Root_PointWheelTunnel, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            }
        }

        private void Root_PointWheelTunnel(object sender, PointerWheelEventArgs e)
        {
            // Let Ctrl+wheel pass through untouched (used for zoom elsewhere).
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                return;

            // Only act when the pointer is over THIS viewer. The hit-tested Source
            // may lie OUTSIDE our subtree (e.g. an overlay Border/VBDockPanel over
            // empty chart areas), so we rely on geometry, not e.Source.
            Point pos = e.GetPosition(this);
            if (pos.X < 0 || pos.Y < 0 || pos.X > Bounds.Width || pos.Y > Bounds.Height)
                return;

            // The outer viewer often has no scrollable extent of its own - the real
            // scrollable content lives in the INNER ScrollViewers of the nested
            // ItemsControls. Pick the inner ScrollViewer whose bounds contain the
            // pointer; fall back to this viewer.
            ScrollViewer target = FindScrollViewerAt(pos) ?? this;
            double delta = e.Delta.Y * 50;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                // Shift+wheel: horizontal scroll.
                target.SetCurrentValue(ScrollViewer.OffsetProperty,
                    new Vector(target.Offset.X + delta, target.Offset.Y));
            }
            else
            {
                // Plain wheel: vertical scroll.
                target.SetCurrentValue(ScrollViewer.OffsetProperty,
                    new Vector(target.Offset.X, target.Offset.Y - delta));
            }
            e.Handled = true;
        }

        /// <summary>
        /// Finds the innermost ScrollViewer below this viewer whose bounds contain
        /// the given point (in this viewer's coordinates).
        /// </summary>
        private ScrollViewer FindScrollViewerAt(Point posLocal)
        {
            ScrollViewer best = null;
            foreach (var sv in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(this).OfType<ScrollViewer>())
            {
                if (ReferenceEquals(sv, this))
                    continue;
                Matrix? transform = this.TransformToVisual(sv);
                if (transform == null)
                    continue;
                Point p = posLocal * transform.Value;
                if (sv.Bounds.Contains(p))
                {
                    // Prefer the deepest (innermost) match.
                    if (best == null || IsDescendantOf(sv, best))
                        best = sv;
                }
            }
            return best;
        }

        private static bool IsDescendantOf(Avalonia.Visual node, Avalonia.Visual ancestor)
        {
            while (node != null)
            {
                if (ReferenceEquals(node, ancestor))
                    return true;
                node = Avalonia.VisualTree.VisualExtensions.GetVisualParent(node);
            }
            return false;
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
                // Do NOT steal the pointer when the press landed on a timeline item -
                // capturing here redirects all subsequent events (incl. the release)
                // to the ScrollViewer, so the item's OnPointerReleased never fires
                // and click-selection breaks. Drag-scrolling still works everywhere
                // else (empty areas, tree, etc.).
                // e.Source is the deepest hit element (e.g. the template's Border),
                // so walk up the visual tree to detect a timeline item.
                bool pressedOnTimelineItem = e.Source is TimelineItemBase ||
                    (e.Source is Avalonia.Visual v &&
                     gip.core.layoutengine.avui.Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(v, typeof(TimelineItemBase)) != null);
                if (!pressedOnTimelineItem)
                {
                    _ShouldAutoScroll = false;
                    _isPointerCaptured = true;
                    _AnimationTimer.Start();   // Restart idle-stopped timer.
                    // Save starting point, used later when determining how much to scroll.
                    _ScrollStartPoint = e.GetPosition(this);
                    _ScrollStartOffset = new Point(Offset.X, Offset.Y);
                    // Update the cursor if can scroll or not.
                    Cursor = (Extent.Width > Viewport.Width) ||
                        (Extent.Height > Viewport.Height) ?
                        new Cursor(StandardCursorType.SizeAll) : new Cursor(StandardCursorType.Arrow);
                    e.Pointer.Capture(this);
                }
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
                // Ctrl+wheel: horizontal zoom-scroll.
                var newOffset = new Vector(Offset.X + e.Delta.Y * 50, Offset.Y);
                SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                // Shift+wheel: horizontal scroll.
                var newOffset = new Vector(Offset.X + e.Delta.Y * 50, Offset.Y);
                SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
            }
            else
            {
                // Plain wheel: vertical scroll (handled explicitly because the
                // base implementation does not react in this template setup).
                var newOffset = new Vector(Offset.X, Offset.Y - e.Delta.Y * 50);
                SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
                e.Handled = true;
            }
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
                    }
                }

                // Only force re-render while an animation is actually running.
                // Unconditional InvalidateVisual() every 10ms starves the UI thread
                // on large charts (thousands of items after Expand-All).
                if (_ShouldAutoScroll || _Velocity.Length > 1)
                    InvalidateVisual();
                else
                    _AnimationTimer.Stop();   // Idle: no 100Hz ticking overhead.
            }
        }
        #endregion

        public Point AutoScrollTarget
        {
            set
            {
                _AutoScrollTarget = value;
                _ShouldAutoScroll = true;
                _AnimationTimer.Start();   // Restart idle-stopped timer.
            }
        }


        public void ScrollToCenterTarget(Point target)
        {
            var newOffset = new Vector(target.X - Viewport.Width / 2.0, target.Y - Viewport.Height / 2.0);
            SetCurrentValue(ScrollViewer.OffsetProperty, newOffset);
        }
    }
}
