// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.UIExtensions;
using gip.ext.design.avui;
using gip.ext.designer.avui.Controls;
using Avalonia.Controls;
using Avalonia;

namespace gip.ext.designer.avui.Extensions
{
    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> behavior for <see cref="Canvas"/>.
    /// </summary>
    [ExtensionFor(typeof(Canvas), OverrideExtension = typeof(DefaultPlacementBehavior))]
    public class CanvasPlacementSupport : SnaplinePlacementBehavior
    {
        GrayOutDesignerExceptActiveArea grayOut;
        Control extendedComponent;
        Control extendedView;

        static double GetCanvasProperty(Control element, AvaloniaProperty d)
        {
            double v = (double)element.GetValue(d);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        static bool IsPropertySet(Control element, AvaloniaProperty d)
        {
            return element.IsSet(d);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            extendedComponent = (Control)ExtendedItem.Component;
            extendedView = (Control)this.ExtendedItem.View;
        }

        static protected double GetLeft(Control element)
        {
            double v = (double)element.GetValue(Canvas.LeftProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        static protected double GetTop(Control element)
        {
            double v = (double)element.GetValue(Canvas.TopProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        public override Rect GetPosition(PlacementOperation operation, DesignItem item, bool verifyAndCorrectPosition)
        {
            Control child = item.View;

            if (child == null)
                return RectExtensions.Empty();

            double x, y;

            if (IsPropertySet(child, Canvas.LeftProperty) || !IsPropertySet(child, Canvas.RightProperty))
            {
                x = GetCanvasProperty(child, Canvas.LeftProperty);
            }
            else
            {
                x = extendedComponent.Bounds.Width - GetCanvasProperty(child, Canvas.RightProperty) - PlacementOperation.GetRealElementSize(child).Width;
            }


            if (IsPropertySet(child, Canvas.TopProperty) || !IsPropertySet(child, Canvas.BottomProperty))
            {
                y = GetCanvasProperty(child, Canvas.TopProperty);
            }
            else
            {
                y = extendedComponent.Bounds.Height - GetCanvasProperty(child, Canvas.BottomProperty) - PlacementOperation.GetRealElementSize(child).Height;
            }

            var p = new Point(x, y);
            //Fixes, Empty Image Resized to 0
            //return new Rect(p, child.RenderSize);
            return new Rect(p, PlacementOperation.GetRealElementSize(item.View));
        }

        public override void SetPosition(PlacementInformation info)
        {
            base.SetPosition(info);
            info.Item.Properties[Control.MarginProperty].Reset();

            Control child = info.Item.View;
            Rect newPosition = info.Bounds;

            if (IsPropertySet(child, Canvas.LeftProperty) || !IsPropertySet(child, Canvas.RightProperty))
            {
                if (newPosition.Left != GetCanvasProperty(child, Canvas.LeftProperty))
                {
                    info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(newPosition.Left);
                }
            }
            else
            {
                var newR = extendedComponent.Bounds.Width - newPosition.Right;
                if (newR != GetCanvasProperty(child, Canvas.RightProperty))
                    info.Item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(newR);
            }


            if (IsPropertySet(child, Canvas.TopProperty) || !IsPropertySet(child, Canvas.BottomProperty))
            {
                if (newPosition.Top != GetCanvasProperty(child, Canvas.TopProperty))
                {
                    info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(newPosition.Top);
                }
            }
            else
            {
                var newB = extendedComponent.Bounds.Height - newPosition.Bottom;
                if (newB != GetCanvasProperty(child, Canvas.BottomProperty))
                    info.Item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(newB);
            }

            if (info.Item == Services.Selection.PrimarySelection)
            {
                var b = new Rect(0, 0, extendedView.Bounds.Width, extendedView.Bounds.Height);
                // only for primary selection:
                if (grayOut != null)
                {
                    grayOut.AnimateActiveAreaRectTo(b);
                }
                else
                {
                    GrayOutDesignerExceptActiveArea.Start(ref grayOut, this.Services, this.ExtendedItem.View, b);
                }
            }

        }

        public override void LeaveContainer(PlacementOperation operation)
        {
            GrayOutDesignerExceptActiveArea.Stop(ref grayOut);

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
                info.Item.Properties[Control.HorizontalAlignmentProperty].Reset();
                info.Item.Properties[Control.VerticalAlignmentProperty].Reset();
                info.Item.Properties[Control.MarginProperty].Reset();

                if (operation.Type == PlacementType.PasteItem)
                {
                    if (!double.IsNaN(info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).GetConvertedValueOnInstance<double>()))
                    {
                        info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty)
                            .SetValue((info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).GetConvertedValueOnInstance<double>()) +
                                      PlacementOperation.PasteOffset);
                    }

                    if (!double.IsNaN(info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).GetConvertedValueOnInstance<double>()))
                    {
                        info.Item.Properties.GetAttachedProperty(Canvas.TopProperty)
                            .SetValue((info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).GetConvertedValueOnInstance<double>()) +
                                      PlacementOperation.PasteOffset);
                    }
                }
            }
        }

        public override void EndPlacement(PlacementOperation operation)
        {
            GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
            base.EndPlacement(operation);
        }
    }
}
