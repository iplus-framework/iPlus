using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
