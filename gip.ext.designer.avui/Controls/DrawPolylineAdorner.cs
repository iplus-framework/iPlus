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

namespace gip.ext.designer.avui.Controls
{
    [CLSCompliant(false)]
    public class DrawPolylineAdorner : DrawPolyAdornerBase
    {
        #region c'tors
        public DrawPolylineAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            double arrowAngle = 0;
            if (s_ArrowAngle.HasValue)
                arrowAngle = s_ArrowAngle.Value;
            double arrowLength = 0;
            if (s_ArrowLength.HasValue)
                arrowLength = s_ArrowLength.Value;
            bool isArrowClosed = false;
            if (s_IsArrowClosed.HasValue)
                isArrowClosed = s_IsArrowClosed.Value;
            ArrowEnds arrowEnds = ArrowEnds.None;
            if (sArrowEnds.HasValue)
                arrowEnds = sArrowEnds.Value;
            if (arrowEnds != ArrowEnds.None)
            {
                if (arrowAngle < 0.001)
                    arrowAngle = (double)ArrowLineBase.ArrowAngleProperty.GetMetadata(typeof(ArrowLineBase)).DefaultValue;
                if (arrowLength < 0.001)
                    arrowLength = (double)ArrowLineBase.ArrowLengthProperty.GetMetadata(typeof(ArrowLineBase)).DefaultValue;
                if (!s_IsArrowClosed.HasValue)
                    isArrowClosed = (bool)ArrowLineBase.IsArrowClosedProperty.GetMetadata(typeof(ArrowLineBase)).DefaultValue;
            }
            _lastStartPoint = StartPointRelativeToShapeContainer;
            _lastEndPoint = pointRelativeToPathContainer;
            //System.Diagnostics.Debug.WriteLine("GetGeometry: " + PointCollection.ToString());
            return ArrowPolyline.GetPathGeometry(PointCollection, arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }

        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, PointerEventArgs e = null)
        {
            ArrowPolyline newInstance = (ArrowPolyline)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(ArrowPolyline), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);

                item.Properties[ArrowPolyline.PointsProperty].SetValue(PointCollectionRelativeToBounds);

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
            DrawPolylineAdorner.RefreshPen(sender, e);
        }

        protected static Nullable<double> s_ArrowAngle;
        protected static Nullable<double> s_ArrowLength;
        protected static Nullable<ArrowEnds> sArrowEnds;
        protected static Nullable<bool> s_IsArrowClosed;

        public static new void RefreshPen(object sender, UndoServiceTransactionEventArgs e)
        {
            //var query = e.AffectedItem.AffectedElements.Where(c => c.Name == "Stroke");
            if (e.AffectedItem is XamlModelProperty.PropertyChangeAction)
            {
                XamlModelProperty.PropertyChangeAction changeAction = e.AffectedItem as XamlModelProperty.PropertyChangeAction;
                if ((changeAction.Property.DesignItem != null)
                    && (changeAction.Property.DesignItem.View != null)
                    && (changeAction.Property.DesignItem.View is ArrowPolyline))
                {
                    ArrowPolyline shape = changeAction.Property.DesignItem.View as ArrowPolyline;
                    switch (changeAction.Property.Name)
                    {
                        case nameof(ArrowPolyline.ArrowAngle):
                            s_ArrowAngle = shape.ArrowAngle;
                            break;
                        case nameof(ArrowPolyline.ArrowLength):
                            s_ArrowLength = shape.ArrowLength;
                            break;
                        case nameof(ArrowPolyline.ArrowEnds):
                            sArrowEnds = shape.ArrowEnds;
                            break;
                        case nameof(ArrowPolyline.IsArrowClosed):
                            s_IsArrowClosed = shape.IsArrowClosed;
                            break;
                    }
                }
            }
        }

        protected override void ApplyDefaultPropertiesToItem(DesignItem item)
        {
            base.ApplyDefaultPropertiesToItem(item);
            if (s_ArrowAngle.HasValue)
                item.Properties[ArrowPolyline.ArrowAngleProperty].SetValue(s_ArrowAngle.Value);
            if (s_ArrowLength.HasValue)
                item.Properties[ArrowPolyline.ArrowLengthProperty].SetValue(s_ArrowLength.Value);
            if (sArrowEnds.HasValue)
                item.Properties[ArrowPolyline.ArrowEndsProperty].SetValue(sArrowEnds.Value);
            if (s_IsArrowClosed.HasValue)
                item.Properties[ArrowPolyline.IsArrowClosedProperty].SetValue(s_IsArrowClosed.Value);
        }
    }
}
