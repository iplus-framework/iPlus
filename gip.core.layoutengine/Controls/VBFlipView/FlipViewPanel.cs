using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Transactions;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the flip view panel.
    /// </summary>
    public class FlipViewPanel : Panel
    {
        protected override Size MeasureOverride(System.Windows.Size availableSize)
        {
            if (!Double.IsInfinity(availableSize.Width) && !Double.IsInfinity(availableSize.Height))
            {
                foreach (UIElement child in InternalChildren)
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
            foreach (UIElement child in InternalChildren)
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
