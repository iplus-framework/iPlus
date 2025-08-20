using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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
        private System.Windows.Media.Color _color;

        /// <summary>
        /// Creates a new instance of VBDesignSelectionAdorner.
        /// </summary>
        /// <param name="adornedElement"></param>
        /// <param name="color"></param>
        public VBDesignSelectionAdorner(UIElement adornedElement, System.Windows.Media.Color color)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _color = color;
            //ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles the OnRender.
        /// </summary>
        /// <param name="drawingContext">The drawing context parameter.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            System.Windows.Size desiredSize = this.AdornedElement.DesiredSize;
            desiredSize.Width += 4;
            desiredSize.Height += 4;

            Rect adornedElementRect = new Rect(desiredSize);
            adornedElementRect.X -= 2;
            adornedElementRect.Y -= 2;

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.White);
            renderBrush.Opacity = 0.05;
            System.Windows.Media.Pen renderPen = new System.Windows.Media.Pen(new SolidColorBrush(_color), 1.5d);
            renderPen.DashStyle = DashStyles.Dot;
            drawingContext.DrawRoundedRectangle(renderBrush, renderPen, adornedElementRect, 3, 3);
        }
    }

}
