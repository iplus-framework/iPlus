using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace gip.core.layoutengine.avui
{
    class VBDockingSplitter : Border
    {
        VBDockSplitOrientation _split;
        Control _prevControl;
        Control _nextControl;

        public VBDockingSplitter() : base()
        {
        }

        public VBDockingSplitter(Control prevControl, Control nextControl, VBDockSplitOrientation split) : this()
        {
            _prevControl = prevControl;
            _nextControl = nextControl;
            _split = split;
            Background = new SolidColorBrush(Colors.Transparent);

            if (_split == VBDockSplitOrientation.Vertical)
            {
                Cursor = new Cursor(StandardCursorType.SizeWestEast);
                Width = 5;
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                Height = 5;
            }
        }

        Point ptStartDrag;
        Point AdjustControls(Point delta)
        {
            double x = delta.X;
            double y = delta.Y;
            if (_split == VBDockSplitOrientation.Vertical)
            {
                if (x > 0 && _nextControl != null)
                {
                    if (_nextControl.Bounds.Width - x < _nextControl.MinWidth)
                        x = _nextControl.Bounds.Width - _nextControl.MinWidth;
                }
                else if (x < 0 && _prevControl != null)
                {
                    if (_prevControl.Bounds.Width + x < _prevControl.MinWidth)
                        x = _prevControl.MinWidth - _prevControl.Bounds.Width;
                }

                if (_prevControl!=null)
                    _prevControl.Width += x;
                if (_nextControl!=null)
                    _nextControl.Width -= x;
            }
            else
            {
                if (y > 0 && _nextControl!=null)
                {
                    if (_nextControl.Bounds.Height - y < _nextControl.MinHeight)
                        y = _nextControl.Bounds.Height - _nextControl.MinHeight;
                }
                else if (y < 0 &&_prevControl != null)
                {
                    if (_prevControl.Bounds.Height + y < _prevControl.MinHeight)
                        y = _prevControl.MinHeight - _prevControl.Bounds.Height;

                }
                if (_prevControl != null)
                    _prevControl.Height += y;
                if (_nextControl != null)
                    _nextControl.Height -= y;
            }

            return new Point(x,y);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!Focusable)
            {
                ptStartDrag = e.GetPosition(Parent as Visual);
                e.Pointer.Capture(this);
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (Focusable)
            {
                Point ptCurrontRelative = e.GetPosition(this);

                Point ptCurrent = e.GetPosition(Parent as Visual);
                Point delta = new Point(ptCurrent.X - ptStartDrag.X, ptCurrent.Y - ptStartDrag.Y);

                delta = AdjustControls(delta);
                        
                ptStartDrag = new Point(ptStartDrag.X+delta.X, ptStartDrag.Y+delta.Y);
               
            }
            base.OnPointerMoved(e);
        }


        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (Focusable)
            {
                Point ptCurrent = e.GetPosition(Parent as Visual);
                Point delta = new Point(ptCurrent.X - ptStartDrag.X , ptCurrent.Y - ptStartDrag.Y);
                AdjustControls(delta);
                if (e.Pointer.Captured == this)
                    e.Pointer.Capture(null);
            }
            base.OnPointerReleased(e);

        }
    }
}
