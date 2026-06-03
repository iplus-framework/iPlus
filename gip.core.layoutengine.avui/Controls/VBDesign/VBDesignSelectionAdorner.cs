using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a adorner that decorates selected design items.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt einen Dekorateur dar, der ausgewählte Designobjekte schmückt.
    /// </summary>
    public class VBDesignSelectionAdorner : Adorner
    {
        private Color _color;

        /// <summary>
        /// Creates a new instance of VBDesignSelectionAdorner.
        /// </summary>
        /// <param name="adornedElement"></param>
        /// <param name="color"></param>
        public VBDesignSelectionAdorner(Control adornedElement, Color color)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _color = color;
        }

        private static IDashStyle s_dot;
        private static IDashStyle Dot => s_dot ??= new ImmutableDashStyle(new double[] { 1, 1 }, 1);

        private static SolidColorBrush s_FillBrush;
        private static SolidColorBrush FillBrush => s_FillBrush ??= new SolidColorBrush(Colors.White) { Opacity = 0.05 };

        /// <summary>
        /// Handles the OnRender.
        /// </summary>
        /// <param name="drawingContext">The drawing context parameter.</param>
        public override void Render(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(FillBrush, 
                new Pen(new SolidColorBrush(_color), 6.0) { DashStyle = Dot }, 
                new RoundedRect(new Rect(0, 0, this.AdornedElement.DesiredSize.Width + 0, this.AdornedElement.DesiredSize.Height + 0), 10, 10));
            base.Render(drawingContext);
        }
    }

}
