using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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

        /// <summary>
        /// Handles the OnRender.
        /// </summary>
        /// <param name="drawingContext">The drawing context parameter.</param>
        public override void Render(DrawingContext drawingContext)
        {
            Size desiredSize = this.AdornedElement.DesiredSize;
            Rect adornedElementRect = new Rect(-2, -2, desiredSize.Width + 4, desiredSize.Height + 4);
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.White);
            renderBrush.Opacity = 0.05;
            Pen renderPen = new Pen(new SolidColorBrush(_color), 1.5d);
            renderPen.DashStyle = DashStyle.Dot;
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect, 3, 3);
            base.Render(drawingContext);
        }
    }

}
