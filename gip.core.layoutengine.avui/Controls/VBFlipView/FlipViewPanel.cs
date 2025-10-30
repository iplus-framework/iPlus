using Avalonia;
using Avalonia.Controls;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the flip view panel.
    /// </summary>
    public class FlipViewPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (!Double.IsInfinity(availableSize.Width) && !Double.IsInfinity(availableSize.Height))
            {
                foreach (Control child in Children)
                {
                    if (child == null)
                        continue;
                    child.Measure(availableSize);
                }
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (Control child in Children)
            {
                double top = Canvas.GetTop(child);
                double left = Canvas.GetLeft(child);

                left = Double.IsNaN(left) ? 0.0 : left;
                top = Double.IsNaN(top) ? 0.0 : top;

                child.Arrange(new Rect(left, top, finalSize.Width, finalSize.Height));
            }
            return finalSize;
        }
    }
}
