// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
    /// <summary>
    /// Base class for classes handling pointer gestures on the design surface.
    /// </summary>
    public abstract class MouseGestureBase
    {
        /// <summary>
        /// Checks if <paramref name="button"/> is the only button that is currently pressed.
        /// </summary>
        public static bool IsOnlyButtonPressed(PointerEventArgs e, RawPointerEventType button)
        {
            var properties = e.Properties;
            return properties.IsLeftButtonPressed == (button == RawPointerEventType.LeftButtonDown)
                && properties.IsMiddleButtonPressed == (button == RawPointerEventType.MiddleButtonDown)
                && properties.IsRightButtonPressed == (button == RawPointerEventType.RightButtonDown)
                && properties.IsXButton1Pressed == (button == RawPointerEventType.XButton1Down)
                && properties.IsXButton2Pressed == (button == RawPointerEventType.XButton2Down);
        }

        protected IDesignPanel designPanel;
        protected ServiceContainer services;
        protected bool canAbortWithEscape = true;
        bool isStarted;

        public void Start(IDesignPanel designPanel, PointerEventArgs e)
        {
            if (designPanel == null)
                throw new ArgumentNullException("designPanel");
            if (e == null)
                throw new ArgumentNullException("e");
            if (isStarted)
                throw new InvalidOperationException("Gesture already was started");

            isStarted = true;
            this.designPanel = designPanel;
            this.services = designPanel.Context.Services;
            e.Pointer.Capture(designPanel);
            RegisterEvents();
            OnStarted(e);

            //if (designPanel.CapturePointer(e.Pointer))
            //{
            //    RegisterEvents();
            //    OnStarted(e);
            //}
            //else
            //{
            //    Stop();
            //}
        }

        void RegisterEvents()
        {
            if (designPanel is InputElement)
                (designPanel as InputElement).PointerCaptureLost += OnPointerCaptureLost;
            designPanel.PointerPressed += OnPointerPressed;
            designPanel.PointerMoved += OnPointerMoved;
            designPanel.PointerReleased += OnPointerReleased;
            designPanel.KeyDown += OnKeyDown;
        }

        void UnRegisterEvents()
        {
            if (designPanel is InputElement)
                (designPanel as InputElement).PointerCaptureLost -= OnPointerCaptureLost;
            designPanel.PointerPressed -= OnPointerPressed;
            designPanel.PointerMoved -= OnPointerMoved;
            designPanel.PointerReleased -= OnPointerReleased;
            designPanel.KeyDown -= OnKeyDown;
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (canAbortWithEscape && e.Key == Key.Escape)
            {
                e.Handled = true;
                Stop(e);
            }
        }

        void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            Stop(e);
        }

        //protected virtual void OnTapped(object sender, TappedEventArgs e)
        //{ }

        //protected virtual void OnDoubleTapped(object sender, TappedEventArgs e)
        //{ }

        protected virtual void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                if (e.Route == Avalonia.Interactivity.RoutingStrategies.Tunnel)
                    OnPreviewMouseLeftButtonDown(sender, e);
            }
            else if (e.Properties.IsRightButtonPressed)
            {
                if (e.Route == Avalonia.Interactivity.RoutingStrategies.Tunnel)
                    OnPreviewMouseLeftButtonDown(sender, e);
            }
            OnMouseDown(sender, e);
        }

        protected virtual void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                if (e.Route == Avalonia.Interactivity.RoutingStrategies.Tunnel)
                    OnPreviewMouseLeftButtonUp(sender, e);
            }
            else if (e.Properties.IsRightButtonPressed)
            {
                if (e.Route == Avalonia.Interactivity.RoutingStrategies.Tunnel)
                    OnPreviewMouseLeftButtonUp(sender, e);
            }
            OnMouseUp(sender, e);
        }

        protected virtual void OnPointerMoved(object sender, PointerEventArgs e)
        {
            OnMouseMove(sender, e);
        }


        protected virtual void OnPreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            //if (PointerHelper.IsDoubleClick(sender, e))
            if (e.ClickCount == 2)
                OnMouseDoubleClick(sender, e);
        }

        protected virtual void OnPreviewMouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
        { 
        }

        protected virtual void OnPreviewMouseRightButtonDown(object sender, PointerPressedEventArgs e)
        { 
        }

        protected virtual void OnPreviewMouseRightButtonUp(object sender, PointerReleasedEventArgs e)
        {
        }

        protected virtual void OnMouseDoubleClick(object sender, PointerPressedEventArgs e)
        { 
        }

        protected virtual void OnMouseDown(object sender, PointerPressedEventArgs e)
        { 
        }

        protected virtual void OnMouseMove(object sender, PointerEventArgs e)
        {
        }

        protected virtual void OnMouseUp(object sender, PointerReleasedEventArgs e)
        {
            Stop(e);
        }

        //private void OnPointerPressed2(object sender, PointerPressedEventArgs e)
        //{
        //    var point = e.GetCurrentPoint(null);
        //    if (point.Properties.IsLeftButtonPressed)
        //    {
        //        if (e.ClickCount == 2)
        //        {
        //            OnDoubleTapped(sender, new TappedEventArgs(null, e));
        //        }
        //    }
        //}

        protected void Stop(RoutedEventArgs e)
        {
            if (!isStarted) 
                return;
            isStarted = false;
            //PointerEventArgs pe = e as PointerEventArgs;
            //if (pe != null)
            //    designPanel.ReleasePointerCapture();
            UnRegisterEvents();
            OnStopped();
        }

        protected virtual void OnStarted(PointerEventArgs e) { }
        protected virtual void OnStopped() { }

        static class PointerHelper
        {
            private const double k_MaxMoveDistance = 10;
            private static long _LastClickTicks = 0;
            private static Point _LastPosition;
            private static WeakReference _LastSender;

            internal static bool IsDoubleClick(object sender, PointerEventArgs e)
            {
                Point position = e.GetPosition(null);
                long clickTicks = DateTime.Now.Ticks;
                long elapsedTicks = clickTicks - _LastClickTicks;
                long elapsedTime = elapsedTicks / TimeSpan.TicksPerMillisecond;
                
                // Use system double-click time (typically 500ms)
                bool quickClick = (elapsedTime <= 500);
                bool senderMatch = (_LastSender != null && sender.Equals(_LastSender.Target));

                if (senderMatch && quickClick && Distance(position, _LastPosition) <= k_MaxMoveDistance)
                {
                    // Double click!
                    _LastClickTicks = 0;
                    _LastSender = null;
                    return true;
                }

                // Not a double click
                _LastClickTicks = clickTicks;
                _LastPosition = position;
                if (!quickClick)
                    _LastSender = new WeakReference(sender);
                return false;
            }

            private static double Distance(Point pointA, Point pointB)
            {
                double x = pointA.X - pointB.X;
                double y = pointA.Y - pointB.Y;
                return Math.Sqrt(x * x + y * y);
            }
        }

    }
}
