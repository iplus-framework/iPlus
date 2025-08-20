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
using gip.ext.graphics.avui.shapes;
using gip.ext.designer.avui.Xaml;

namespace gip.ext.designer.avui.Controls
{
    [CLSCompliant(false)]
    public class DrawPolylineEditPointAdorner : DrawPolyEditPointAdornerBase
    {
        #region c'tors
        public DrawPolylineEditPointAdorner(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(shapeToEdit, containerForShape, pointToEdit)
        {
        }

        #endregion

        #region methods
        protected override PointCollection GetPointCollection(Shape polyShape)
        {
            if (polyShape is Polyline)
            {
                return (polyShape as Polyline).Points;
            }
            else if (polyShape is ArrowPolyline)
            {
                return (polyShape as ArrowPolyline).Points;
            }
            return null;
        }


        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            ArrowPolyline line = ContainerOfStartPoint.View as ArrowPolyline;

            _EditingPointRelToShapeCont = pointRelativeToPathContainer;
            _PointCollectionRelToShapeCont[_PointToEditIndex] = _EditingPointRelToShapeCont;

            if (ContainerOfStartPoint.View is Polyline)
                return DrawPolyAdornerBase.GetGeometryForPoly(_PointCollectionRelToShapeCont);
            else if (ContainerOfStartPoint.View is ArrowPolyline)
                return ArrowPolyline.GetPathGeometry(_PointCollectionRelToShapeCont, line.ArrowEnds, line.ArrowLength, line.ArrowAngle, line.IsArrowClosed);
            return new PathGeometry();
        }

        protected override void UpdatePointsProperty(DesignItem myCreatedItem, PointCollection translatedPoints)
        {
            if (ContainerOfStartPoint.View is Polyline)
            {
                Polyline line = ContainerOfStartPoint.View as Polyline;
                if (line != null)
                    myCreatedItem.Properties[Polyline.PointsProperty].SetValue(translatedPoints);
            }
            else if (ContainerOfStartPoint.View is ArrowPolyline)
            {
                ArrowPolyline line = ContainerOfStartPoint.View as ArrowPolyline;
                if (line != null)
                    myCreatedItem.Properties[ArrowPolyline.PointsProperty].SetValue(translatedPoints);
            }
        }
        #endregion
    }
}
