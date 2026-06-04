using Avalonia;
using Avalonia.Controls;
using System;

namespace gip.core.layoutengine.avui
{
    public class VBContentRemoteControlAdorner : Adorner
    {
        private readonly Control _popupContent;

        public VBContentRemoteControlAdorner(Control adornedElement, Control popupContent) : base(adornedElement)
        {
            ArgumentNullException.ThrowIfNull(popupContent);

            _popupContent = popupContent;
            ((ISetLogicalParent)_popupContent).SetParent(this);
            LogicalChildren.Add(_popupContent);
            VisualChildren.Add(_popupContent);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var desiredSize = _popupContent.DesiredSize;

            if (desiredSize.Width <= 0 || desiredSize.Height <= 0)
            {
                _popupContent.Measure(finalSize);
                desiredSize = _popupContent.DesiredSize;
            }

            if (desiredSize.Width <= 0 || desiredSize.Height <= 0)
            {
                return finalSize;
            }

            double height = desiredSize.Height;
            if (height < 10)
                height = 10;
            else if (height > 120)
                height = 120;

            double scale = desiredSize.Height > 0 ? height / desiredSize.Height : 1;
            var arrangedSize = new Size(desiredSize.Width * scale, height);

            double x = finalSize.Width - arrangedSize.Width - 20;
            if (x < 0)
                x = 0;

            var point = new Point(x, 0);
            _popupContent.Arrange(new Rect(point, arrangedSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _popupContent.Measure(constraint);
            return base.MeasureOverride(constraint);
        }
    }

}
