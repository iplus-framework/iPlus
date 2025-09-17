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
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace gip.ext.designer.avui.Controls
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
            IList<Point> points = GetPointCollection(polyShape);

            if (points != null)
            {
                int i = 0;
                foreach (Point point in points)
                {
                    Point pointRelToShapeCont = polyShape.TranslatePoint(point, containerForShape.View as Visual).Value;
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

        protected IList<Point> _PointCollectionRelToShapeCont = new List<Point>();

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
            IList<Point> translatedPoints = TranslatePointsToBounds(_PointCollectionRelToShapeCont, out startPoint, out endPoint);

            UpdatePointsProperty(myCreatedItem, translatedPoints);

            DrawPolylineAdorner.GetRectForPlacementOperation(startPoint, endPoint, out result);
            return true;
        }

        protected abstract void UpdatePointsProperty(DesignItem myCreatedItem, IList<Point> translatedPoints);
        protected abstract IList<Point> GetPointCollection(Shape polyShape);
    }
}
