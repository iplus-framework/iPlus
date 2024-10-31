// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace gip.ext.designer.Services
{
    /// <summary>
    /// Base class for mouse gestures that should start dragging only after a minimum drag distance.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class ClickOrDragMouseGesture : MouseGestureBase
    {
        protected Point startPoint;
        static public bool _HasDragStarted;
        protected IInputElement positionRelativeTo;

        protected override void OnStarted(MouseButtonEventArgs e)
        {
            Debug.Assert(positionRelativeTo != null);
            _HasDragStarted = false;
            startPoint = e.GetPosition(positionRelativeTo);
        }

        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_HasDragStarted)
            {
                Vector v = e.GetPosition(positionRelativeTo) - startPoint;
                if (Math.Abs(v.X) >= SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(v.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    _HasDragStarted = true;
                    OnDragStarted(e);
                }
            }
        }

        protected override void OnStopped()
        {
            _HasDragStarted = false;
        }

        protected virtual void OnDragStarted(MouseEventArgs e) { }
    }
}
