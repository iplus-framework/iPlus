// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
    public abstract class DrawPolyEditPointAdornerBase : DrawShapesEditPointAdornerBase
    {
        #region c'tors
        public DrawPolyEditPointAdornerBase(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(shapeToEdit, containerForShape, pointToEdit)
        {
            _PointToEdit = pointToEdit;
            Shape polyShape = shapeToEdit.View as Shape;
            PointCollection points = GetPointCollection(polyShape);

            if (points != null)
            {
                int i = 0;
                foreach (Point point in points)
                {
                    Point pointRelToShapeCont = polyShape.TranslatePoint(point, containerForShape.View);
                    _PointCollectionRelToShapeCont.Add(pointRelToShapeCont);
                    if (point.X == _PointToEdit.X && point.Y == _PointToEdit.Y)
                    {
                        _PointToEditIndex = i;
                        _EditingPointRelToShapeCont = pointRelToShapeCont;
                    }
                    i++;
                }
               
                if (DesignPanel != null && DesignPanel.IsRasterOn)
                    this._StartPointRelativeToShapeContainer = SnapPointToRaster(_EditingPointRelToShapeCont, DesignPanel.RasterSize);
                else
                    this._StartPointRelativeToShapeContainer = _EditingPointRelToShapeCont;
            }
        }

        #endregion

        protected Point _PointToEdit; // Relative Coordinates to Shape
        protected Point _EditingPointRelToShapeCont; // Relative Coordinates to Shape
        protected int _PointToEditIndex = 0;

        protected PointCollection _PointCollectionRelToShapeCont = new PointCollection();

        public override Pen DrawingPen
        {
            get
            {
                return _PenOfShapeToEdit;
            }
        }

        public override bool StopEditingPoint(Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container)
        {
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                endPoint = SnapPointToRaster(endPoint, DesignPanel.RasterSize);
            _EditingPointRelToShapeCont = endPoint;
            _PointCollectionRelToShapeCont[_PointToEditIndex] = _EditingPointRelToShapeCont;
            Point startPoint;
            PointCollection translatedPoints = TranslatePointsToBounds(_PointCollectionRelToShapeCont, out startPoint, out endPoint);

            UpdatePointsProperty(myCreatedItem, translatedPoints);

            DrawPolylineAdorner.GetRectForPlacementOperation(startPoint, endPoint, out result);
            return true;
        }

        protected abstract void UpdatePointsProperty(DesignItem myCreatedItem, PointCollection translatedPoints);
        protected abstract PointCollection GetPointCollection(Shape polyShape);
    }
}
