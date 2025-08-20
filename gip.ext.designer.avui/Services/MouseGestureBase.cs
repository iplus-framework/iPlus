﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Windows.Input;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
    /// <summary>
    /// Base class for classes handling mouse gestures on the design surface.
    /// </summary>
    public abstract class MouseGestureBase
    {
        /// <summary>
        /// Checks if <paramref name="button"/> is the only button that is currently pressed.
        /// </summary>
        public static bool IsOnlyButtonPressed(MouseEventArgs e, MouseButton button)
        {
            return e.LeftButton == (button == MouseButton.Left ? MouseButtonState.Pressed : MouseButtonState.Released)
                && e.MiddleButton == (button == MouseButton.Middle ? MouseButtonState.Pressed : MouseButtonState.Released)
                && e.RightButton == (button == MouseButton.Right ? MouseButtonState.Pressed : MouseButtonState.Released)
                && e.XButton1 == (button == MouseButton.XButton1 ? MouseButtonState.Pressed : MouseButtonState.Released)
                && e.XButton2 == (button == MouseButton.XButton2 ? MouseButtonState.Pressed : MouseButtonState.Released);
        }

        protected IDesignPanel designPanel;
        protected ServiceContainer services;
        protected bool canAbortWithEscape = true;
        bool isStarted;

        public void Start(IDesignPanel designPanel, MouseButtonEventArgs e)
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
            if (designPanel.CaptureMouse())
            {
                RegisterEvents();
                OnStarted(e);
            }
            else
            {
                Stop();
            }
        }

        void RegisterEvents()
        {
            designPanel.LostMouseCapture += OnLostMouseCapture;
            designPanel.MouseDown += OnMouseDown;
            designPanel.MouseDown += OnMouseDown2;
            designPanel.MouseMove += OnMouseMove;
            designPanel.MouseUp += OnMouseUp;
            designPanel.KeyDown += OnKeyDown;
        }

        void UnRegisterEvents()
        {
            designPanel.LostMouseCapture -= OnLostMouseCapture;
            designPanel.MouseDown -= OnMouseDown;
            designPanel.MouseDown -= OnMouseDown2;
            designPanel.MouseMove -= OnMouseMove;
            designPanel.MouseUp -= OnMouseUp;
            designPanel.KeyDown -= OnKeyDown;
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (canAbortWithEscape && e.Key == Key.Escape)
            {
                e.Handled = true;
                Stop();
            }
        }

        void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            Stop();
        }

        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Stop();
        }

        private void OnMouseDown2(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    OnMouseDoubleClick(sender, e);
                }
            }
        }

        protected virtual void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }


        protected void Stop()
        {
            if (!isStarted) return;
            isStarted = false;
            designPanel.ReleaseMouseCapture();
            UnRegisterEvents();
            OnStopped();
        }

        protected virtual void OnStarted(MouseButtonEventArgs e) { }
        protected virtual void OnStopped() { }
    }
}
