using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace gip.ext.design.avui.UIExtensions
{

    public static class ShapeExtensions
    {
        public static IPen GetPen(this Shape shape)
        {
            var field = shape.GetType().GetField("_strokePen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(shape) as IPen;
        }
    }

    public static class RectExtensions
    {
        private static readonly Rect s_empty = CreateEmptyRect();
        private static Rect CreateEmptyRect()
        {
            return new Rect(new Point(double.PositiveInfinity, double.PositiveInfinity), new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        public static bool IsEmpty(this Rect rect)
        {
            return rect == s_empty;
        }

        public static Rect Empty()
        {
            return s_empty;
        }

        public static Rect Inflate(this Rect rect, double width, double height)
        {
            if (rect.IsEmpty())
            {
                throw new InvalidOperationException("rect.IsEmpty()");
            }

            double _x = rect.X;
            double _y = rect.Y;
            double _width = rect.Width;
            double _height = rect.Height;

            _x -= width;
            _y -= height;
            _width += width;
            _width += width;
            _height += height;
            _height += height;
            if (!(_width >= 0.0) || !(_height >= 0.0))
            {
                return Empty();
            }
            return new Rect(_x, _y, _width, _height);
        }
    }
}
