// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.designer.avui.OutlineView
{
    public class DragListener
    {
        public DragListener(Control target)
        {
            this.target = target;
            //target.AddHandler(Control.PointerPressedEvent, new EventHandler<PointerPressedEventArgs>(PointerPressed), RoutingStrategies.Direct | RoutingStrategies.Bubble, true);
            target.PointerPressed += Target_PointerPressed;
            target.PointerMoved += Target_PointerMoved;
            target.PointerReleased += Target_PointerReleased;
        }

        public event EventHandler<PointerEventArgs> DragStarted;

        Control target;
        Point startPoint;
        bool ready;
        PointerEventArgs args;
        private IPointer capturedPointer;

        void Target_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(target).Properties.IsLeftButtonPressed && capturedPointer == null)
            {
                ready = true;
                startPoint = e.GetPosition(target);
                args = e;
                capturedPointer = e.Pointer;
                e.Pointer.Capture(target);
            }
        }

        void Target_PointerMoved(object sender, PointerEventArgs e)
        {
            if (ready)
            {
                var currentPoint = e.GetPosition(target);
                if (Math.Abs(currentPoint.X - startPoint.X) >= gip.ext.designer.avui.Controls.DragListener.MinimumDragDistance ||
                    Math.Abs(currentPoint.Y - startPoint.Y) >= gip.ext.designer.avui.Controls.DragListener.MinimumDragDistance)
                {
                    ready = false;
                    if (DragStarted != null)
                    {
                        DragStarted(this, args);
                    }
                }
            }
        }

        void Target_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ready = false;
            if (capturedPointer != null)
            {
                capturedPointer.Capture(null);
                capturedPointer = null;
            }
        }
    }
}
