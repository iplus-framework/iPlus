using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    public class VBContentRemoteControlAdorner : Adorner
    {
        private readonly Control _popupContent;
        private readonly List<Visual> _children;

        public VBContentRemoteControlAdorner(Control adornedElement, Control popupContent) : base(adornedElement)
        {
            _popupContent = popupContent;
            _children = new List<Visual>();
            _children.Add(_popupContent);
        }

        protected int VisualChildrenCount => _children.Count;


        //protected override Visual GetVisualChild(int index)
        //{
        //    return _children[index];
        //}

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Position the popup above the text box
            var desiredSize = _popupContent.DesiredSize;
            double height = desiredSize.Height; // finalSize.Height > 0.0001 ? finalSize.Height * 4 : desiredSize.Height;
            double width = desiredSize.Height; // Uniform
            double newheight = desiredSize.Height; // finalSize.Height > 0.0001 ? finalSize.Height * 4 : desiredSize.Height;
            double newwidth = desiredSize.Width; // Uniform
            if (height < 10)
            {
                height = 10; // Minimum height
                double factor = 1;
                if (desiredSize.Height > 0.0001)
                    factor = height / desiredSize.Height;
                width = factor * desiredSize.Width > 0.0001 ? desiredSize.Width : height;
                newheight = height;
                newwidth = width;
            }
            else if (height > 120)
            {
                height = 120; // Minimum height
                double factor = 1;
                if (desiredSize.Height > 0.0001)
                    factor = height / desiredSize.Height;
                width = factor * desiredSize.Width > 0.0001 ? desiredSize.Width : height;
                newheight = height;
                newwidth = width;
            }
            //desiredSize.Height = height;
            //desiredSize.Width = width;
            double x = finalSize.Width - newwidth - 20;
            if (x < 0)
                x = 0; 
            var point = new Point(x, 0);
            //var point = new Point(-20, 0);
            _popupContent.Arrange(new Rect(point, desiredSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _popupContent.Measure(constraint);
            return base.MeasureOverride(constraint);
        }
    }

}
