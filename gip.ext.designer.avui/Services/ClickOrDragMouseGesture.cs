// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Input;
using System;
using System.Diagnostics;

namespace gip.ext.designer.avui.Services
{
    /// <summary>
    /// Base class for mouse gestures that should start dragging only after a minimum drag distance.
    /// </summary>
    public abstract class ClickOrDragMouseGesture : MouseGestureBase
    {
        protected Point startPoint;
        static public bool _HasDragStarted;
        protected IInputElement positionRelativeTo;

        const double MinimumDragDistance = 3;

        protected override void OnStarted(PointerEventArgs e)
        {
            Debug.Assert(positionRelativeTo != null);
            _HasDragStarted = false;
            Visual visual = positionRelativeTo as Visual;
            if (visual != null)
                startPoint = e.GetPosition(visual);
        }

        protected override void OnMouseMove(object sender, PointerEventArgs e)
        {
            if (!_HasDragStarted)
            {
                Visual visual = positionRelativeTo as Visual;
                if (visual != null)
                {
                    Vector v = e.GetPosition(visual) - startPoint;
                    if (Math.Abs(v.X) >= gip.ext.designer.avui.Controls.DragListener.MinimumDragDistance
                        || Math.Abs(v.Y) >= gip.ext.designer.avui.Controls.DragListener.MinimumDragDistance)
                    {
                        _HasDragStarted = true;
                        OnDragStarted(e);
                    }
                }
            }
        }

        protected override void OnStopped()
        {
            _HasDragStarted = false;
        }

        protected virtual void OnDragStarted(PointerEventArgs e) { }
    }
}
