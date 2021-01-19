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
    public class DrawPolygonEditPointAdorner : DrawPolyEditPointAdornerBase
    {
        #region c'tors
        public DrawPolygonEditPointAdorner(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(shapeToEdit, containerForShape, pointToEdit)
        {
        }

        #endregion

        #region methods
        protected override PointCollection GetPointCollection(Shape polyShape)
        {
            if (polyShape is Polygon)
            {
                return (polyShape as Polygon).Points;
            }
            return null;
        }


        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            _EditingPointRelToShapeCont = pointRelativeToPathContainer;
            _PointCollectionRelToShapeCont[_PointToEditIndex] = _EditingPointRelToShapeCont;
            return DrawPolyAdornerBase.GetGeometryForPoly(_PointCollectionRelToShapeCont);
        }

        protected override void UpdatePointsProperty(DesignItem myCreatedItem, PointCollection translatedPoints)
        {
            Polygon line = ContainerOfStartPoint.View as Polygon;
            if (line != null)
                myCreatedItem.Properties[Polygon.PointsProperty].SetValue(translatedPoints);
        }
        #endregion
    }
}
