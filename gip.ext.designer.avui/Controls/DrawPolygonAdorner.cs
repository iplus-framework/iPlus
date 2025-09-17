// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;
using gip.ext.graphics.avui.shapes;
using gip.ext.designer.avui.Xaml;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Shapes;

namespace gip.ext.designer.avui.Controls
{
    [CLSCompliant(false)]
    public class DrawPolygonAdorner : DrawPolyAdornerBase
    {
        #region c'tors
        public DrawPolygonAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            _lastStartPoint = StartPointRelativeToShapeContainer;
            _lastEndPoint = pointRelativeToPathContainer;
            return GetGeometryForPoly(PointCollection);
        }

        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, PointerEventArgs e = null)
        {
            Polygon newInstance = (Polygon)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(Polygon), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);

                item.Properties[Polygon.PointsProperty].SetValue(PointCollectionRelativeToBounds);

            }
            return item;
        }

        public override bool MyGetRectForPlacementOperation(Point startPoint, Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container)
        {
            GetBounds(PointCollection, out startPoint, out endPoint);
            return GetRectForPlacementOperation(startPoint, endPoint, out result, myCreatedItem, container);
        }

        public override void RefreshMyPen(object sender, UndoServiceTransactionEventArgs e)
        {
            base.RefreshMyPen(sender, e);
            DrawPolygonAdorner.RefreshPen(sender, e);
        }

        public static new void RefreshPen(object sender, UndoServiceTransactionEventArgs e)
        {
            //var query = e.AffectedItem.AffectedElements.Where(c => c.Name == "Stroke");
            if (e.AffectedItem is XamlModelProperty.PropertyChangeAction)
            {
                XamlModelProperty.PropertyChangeAction changeAction = e.AffectedItem as XamlModelProperty.PropertyChangeAction;
                if ((changeAction.Property.DesignItem != null)
                    && (changeAction.Property.DesignItem.View != null)
                    && (changeAction.Property.DesignItem.View is Polygon))
                {
                    Polygon shape = changeAction.Property.DesignItem.View as Polygon;
                }
            }
        }

        protected override void ApplyDefaultPropertiesToItem(DesignItem item)
        {
            base.ApplyDefaultPropertiesToItem(item);
        }
    }
}
