using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace gip.core.layoutengine
{
    public class VBContentRemoteControlAdorner : Adorner
    {
        private readonly UIElement _popupContent;
        private readonly VisualCollection _children;

        public VBContentRemoteControlAdorner(UIElement adornedElement, UIElement popupContent) : base(adornedElement)
        {
            _popupContent = popupContent;
            _children = new VisualCollection(this);
            _children.Add(_popupContent);
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Position the popup above the text box
            var desiredSize = _popupContent.DesiredSize;
            double height = desiredSize.Height; // finalSize.Height > 0.0001 ? finalSize.Height * 4 : desiredSize.Height;
            double width = desiredSize.Height; // Uniform
            if (height < 10)
            {
                height = 10; // Minimum height
                double factor = 1;
                if (desiredSize.Height > 0.0001)
                    factor = height / desiredSize.Height;
                width = factor * desiredSize.Width > 0.0001 ? desiredSize.Width : height;
                desiredSize.Height = height;
                desiredSize.Width = width;
            }
            else if (height > 120)
            {
                height = 120; // Minimum height
                double factor = 1;
                if (desiredSize.Height > 0.0001)
                    factor = height / desiredSize.Height;
                width = factor * desiredSize.Width > 0.0001 ? desiredSize.Width : height;
                desiredSize.Height = height;
                desiredSize.Width = width;
            }
            //desiredSize.Height = height;
            //desiredSize.Width = width;
            double x = finalSize.Width - desiredSize.Width - 20;
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
