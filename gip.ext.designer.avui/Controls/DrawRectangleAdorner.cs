// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.avui.Services;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design.avui;
using gip.ext.designer.avui.Xaml;

namespace gip.ext.designer.avui.Controls
{
    public class DrawRectangleAdorner : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawRectangleAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            RectangleGeometry rect = new RectangleGeometry(new Rect(StartPointRelativeToShapeContainer, pointRelativeToPathContainer));
            if (s_ShapeRadiusX.HasValue)
                rect.RadiusX = s_ShapeRadiusX.Value;
            if (s_ShapeRadiusY.HasValue)
                rect.RadiusY = s_ShapeRadiusY.Value;
            return rect;
        }

        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null)
        {
            Rectangle newInstance = (Rectangle)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(Rectangle), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);
                item.Properties[Rectangle.WidthProperty].SetValue((this.Geometry as RectangleGeometry).Rect.Width);
                item.Properties[Rectangle.HeightProperty].SetValue((this.Geometry as RectangleGeometry).Rect.Height);
                if ((this.Geometry as RectangleGeometry).RadiusX > 0)
                    item.Properties[Rectangle.RadiusXProperty].SetValue((this.Geometry as RectangleGeometry).RadiusX);
                if ((this.Geometry as RectangleGeometry).RadiusY > 0)
                    item.Properties[Rectangle.RadiusYProperty].SetValue((this.Geometry as RectangleGeometry).RadiusY);
            }
            return item;
        }

        public override void RefreshMyPen(object sender, UndoServiceTransactionEventArgs e)
        {
            base.RefreshMyPen(sender, e);
            DrawRectangleAdorner.RefreshPen(sender, e);
        }

        protected static Nullable<double> s_ShapeRadiusX;
        protected static Nullable<double> s_ShapeRadiusY;

        public static new void RefreshPen(object sender, UndoServiceTransactionEventArgs e)
        {
            if (e.AffectedItem is XamlModelProperty.PropertyChangeAction)
            {
                XamlModelProperty.PropertyChangeAction changeAction = e.AffectedItem as XamlModelProperty.PropertyChangeAction;
                if ((changeAction.Property.DesignItem != null)
                    && (changeAction.Property.DesignItem.View != null)
                    && (changeAction.Property.DesignItem.View is Rectangle))
                {
                    Rectangle rect = changeAction.Property.DesignItem.View as Rectangle;
                    switch (changeAction.Property.Name)
                    {
                        case "RadiusX":
                            s_ShapeRadiusX = rect.RadiusX;
                            break;
                        case "RadiusY":
                            s_ShapeRadiusY = rect.RadiusY;
                            break;
                    }
                }
            }
        }

        protected override void ApplyDefaultPropertiesToItem(DesignItem item)
        {
            base.ApplyDefaultPropertiesToItem(item);
            if (s_ShapeRadiusX.HasValue)
                item.Properties[Rectangle.RadiusXProperty].SetValue(s_ShapeRadiusX.Value);
            if (s_ShapeRadiusY.HasValue)
                item.Properties[Rectangle.RadiusYProperty].SetValue(s_ShapeRadiusY.Value);
        }
    }
}
