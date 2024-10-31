// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Controls;
using gip.ext.design.Extensions;
using gip.ext.design;

namespace gip.ext.designer.Extensions
{
    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> behavior for <see cref="Canvas"/>.
    /// </summary>
    [ExtensionFor(typeof(Canvas), OverrideExtension = typeof(DefaultPlacementBehavior))]
    [CLSCompliant(false)]
    public class CanvasPlacementSupport : SnaplinePlacementBehavior
    {
        static protected double GetLeft(UIElement element)
        {
            double v = (double)element.GetValue(Canvas.LeftProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        static protected double GetTop(UIElement element)
        {
            double v = (double)element.GetValue(Canvas.TopProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        public override void SetPosition(PlacementInformation info)
        {
            base.SetPosition(info);
            info.Item.Properties[FrameworkElement.MarginProperty].Reset();

            UIElement child = info.Item.View;
            Rect newPosition = info.Bounds;

            if (newPosition.Left != GetLeft(child))
            {
                info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(Math.Round(newPosition.Left,3));
            }
            if (newPosition.Top != GetTop(child))
            {
                info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(Math.Round(newPosition.Top,3));
            }
        }

        public override void LeaveContainer(PlacementOperation operation)
        {
            base.LeaveContainer(operation);
            foreach (PlacementInformation info in operation.PlacedItems)
            {
                info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
                info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
            }
        }

        public override void EnterContainer(PlacementOperation operation)
        {
            base.EnterContainer(operation);
            foreach (PlacementInformation info in operation.PlacedItems)
            {
                info.Item.Properties[FrameworkElement.HorizontalAlignmentProperty].Reset();
                info.Item.Properties[FrameworkElement.VerticalAlignmentProperty].Reset();
                info.Item.Properties[FrameworkElement.MarginProperty].Reset();
            }
        }
    }
}
