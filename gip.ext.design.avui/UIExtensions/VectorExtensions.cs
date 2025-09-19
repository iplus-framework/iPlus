using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace gip.ext.design.avui.UIExtensions
{

    /// <summary>
    /// Extension methods for brush classes.
    /// </summary>
    public static class VectorExtensions
    {
        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            double y = vector1.X * vector2.Y - vector2.X * vector1.Y;
            double x = vector1.X * vector2.X + vector1.Y * vector2.Y;
            return Math.Atan2(y, x) * (180.0 / Math.PI);
        }
    }
}
