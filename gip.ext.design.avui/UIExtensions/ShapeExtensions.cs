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
            return new Rect(new Point(double.PositiveInfinity, double.PositiveInfinity), new Size(double.NegativeInfinity, double.NegativeInfinity));
        }

        public static bool IsEmpty(this Rect rect)
        {
            return rect.Width < 0;
        }

        public static Rect Empty()
        {
            return s_empty;
        }

        public static Rect Union(this Rect rect, Rect other)
        {
            if (rect.IsEmpty())
            {
                return other;
            }
            if (other.IsEmpty())
            {
                return rect;
            }

            double x1 = Math.Min(rect.X, other.X);
            double y1 = Math.Min(rect.Y, other.Y);
            double x2 = Math.Max(rect.Right, other.Right);
            double y2 = Math.Max(rect.Bottom, other.Bottom);

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        public static Rect Inflate(this Rect rect, double width, double height)
        {
            if (rect.IsEmpty())
            {
                return Empty();
            }

            double _x = rect.X;
            double _y = rect.Y;
            double _width = rect.Width;
            double _height = rect.Height;

            _x -= width;
            _y -= height;
            _width += 2 * width;
            _height += 2 * height;

            if (_width < 0 || _height < 0)
            {
                return Empty();
            }
            return new Rect(_x, _y, _width, _height);
        }
    }
}
