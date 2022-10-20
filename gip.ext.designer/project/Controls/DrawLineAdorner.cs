// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.Services;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design;
using gip.ext.graphics.shapes;
using gip.ext.designer.Xaml;

namespace gip.ext.designer.Controls
{
    [CLSCompliant(false)]
    public class DrawLineAdorner : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawLineAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        private Nullable<Point> _lastStartPoint;
        private Nullable<Point> _lastEndPoint;
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
            ArrowEnds arrowEnds = graphics.shapes.ArrowEnds.None;
            if (sArrowEnds.HasValue)
                arrowEnds = sArrowEnds.Value;
            if (arrowEnds != ArrowEnds.None)
            {
                if (arrowAngle < 0.001)
                    arrowAngle = (double) ArrowLineBase.ArrowAngleProperty.DefaultMetadata.DefaultValue;
                if (arrowLength < 0.001)
                    arrowLength = (double) ArrowLineBase.ArrowLengthProperty.DefaultMetadata.DefaultValue;
                if (!s_IsArrowClosed.HasValue)
                    isArrowClosed = (bool) ArrowLineBase.IsArrowClosedProperty.DefaultMetadata.DefaultValue;
            }
            _lastStartPoint = StartPointRelativeToShapeContainer;
            _lastEndPoint = pointRelativeToPathContainer;
            return ArrowLine.GetPathGeometry(StartPointRelativeToShapeContainer, pointRelativeToPathContainer, arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }

        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null)
        {
            ArrowLine newInstance = (ArrowLine)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(ArrowLine), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);

                double width = 0;
                double height = 0;
                if ((_lastStartPoint.HasValue) && (_lastEndPoint.HasValue))
                {
                    width = _lastEndPoint.Value.X - _lastStartPoint.Value.X;
                    height = _lastEndPoint.Value.Y - _lastStartPoint.Value.Y;

                    PointCollection _PointCollectionRelToShapeCont = new PointCollection();
                    _PointCollectionRelToShapeCont.Add(_lastStartPoint.Value);
                    _PointCollectionRelToShapeCont.Add(_lastEndPoint.Value);

                    Point startPoint;
                    Point endPoint;
                    PointCollection points = TranslatePointsToBounds(_PointCollectionRelToShapeCont, out startPoint, out endPoint);

                    item.Properties[ArrowLine.X1Property].SetValue(points[0].X);
                    item.Properties[ArrowLine.Y1Property].SetValue(points[0].Y);
                    item.Properties[ArrowLine.X2Property].SetValue(points[1].X);
                    item.Properties[ArrowLine.Y2Property].SetValue(points[1].Y);

                    _lastEndPoint = null;
                    _lastStartPoint = null;

                }

            }
            return item;
        }

        public override bool MyGetRectForPlacementOperation(Point startPoint, Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container)
        {
            return GetRectForPlacementOperation(startPoint, endPoint, out result, myCreatedItem, container);
        }

        public override void RefreshMyPen(object sender, UndoServiceTransactionEventArgs e)
        {
            base.RefreshMyPen(sender, e);
            DrawLineAdorner.RefreshPen(sender, e);
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
                    && (changeAction.Property.DesignItem.View is ArrowLine))
                {
                    ArrowLine shape = changeAction.Property.DesignItem.View as ArrowLine;
                    switch (changeAction.Property.Name)
                    {
                        case "ArrowAngle":
                            s_ArrowAngle = shape.ArrowAngle;
                            break;
                        case "ArrowLength":
                            s_ArrowLength = shape.ArrowLength;
                            break;
                        case "ArrowEnds":
                            sArrowEnds = shape.ArrowEnds;
                            break;
                        case "IsArrowClosed":
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
                item.Properties[ArrowLine.ArrowAngleProperty].SetValue(s_ArrowAngle.Value);
            if (s_ArrowLength.HasValue)
                item.Properties[ArrowLine.ArrowLengthProperty].SetValue(s_ArrowLength.Value);
            if (sArrowEnds.HasValue)
                item.Properties[ArrowLine.ArrowEndsProperty].SetValue(sArrowEnds.Value);
            if (s_IsArrowClosed.HasValue)
                item.Properties[ArrowLine.IsArrowClosedProperty].SetValue(s_IsArrowClosed.Value);
        }
    }
}
