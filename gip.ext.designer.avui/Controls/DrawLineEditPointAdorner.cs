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
    public class DrawLineEditPointAdorner : DrawShapesEditPointAdornerBase
    {
        #region c'tors
        public DrawLineEditPointAdorner(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(shapeToEdit, containerForShape, pointToEdit)
        {
            _PointToEdit = pointToEdit;
            if (shapeToEdit.View is ArrowLine)
            {
                ArrowLine line = shapeToEdit.View as ArrowLine;
                if (line.X1 == _PointToEdit.X && line.Y1 == _PointToEdit.Y)
                {
                    _PointToEditIsX1Y1 = true;
                    _EditingPointRelToShapeCont = line.TranslatePoint(_PointToEdit, containerForShape.View);
                    _FixPointRelToShapeCont = line.TranslatePoint(new Point(line.X2, line.Y2), containerForShape.View);
                }
                else
                {
                    _PointToEditIsX1Y1 = false;
                    _EditingPointRelToShapeCont = line.TranslatePoint(_PointToEdit, containerForShape.View);
                    _FixPointRelToShapeCont = line.TranslatePoint(new Point(line.X1, line.Y1), containerForShape.View);
                }
            }
            else if (shapeToEdit.View is Line)
            {
                Line line = shapeToEdit.View as Line;
                if (line.X1 == _PointToEdit.X && line.Y1 == _PointToEdit.Y)
                {
                    _PointToEditIsX1Y1 = true;
                    _EditingPointRelToShapeCont = line.TranslatePoint(_PointToEdit, containerForShape.View);
                    _FixPointRelToShapeCont = line.TranslatePoint(new Point(line.X2, line.Y2), containerForShape.View);
                }
                else
                {
                    _PointToEditIsX1Y1 = false;
                    _EditingPointRelToShapeCont = line.TranslatePoint(_PointToEdit, containerForShape.View);
                    _FixPointRelToShapeCont = line.TranslatePoint(new Point(line.X1, line.Y1), containerForShape.View);
                }
            }
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                this._StartPointRelativeToShapeContainer = SnapPointToRaster(_FixPointRelToShapeCont, DesignPanel.RasterSize);
            else
                this._StartPointRelativeToShapeContainer = _FixPointRelToShapeCont;
        }

        #endregion

        private Point _PointToEdit; // Relative Coordinates to Shape
        private bool _PointToEditIsX1Y1 = false;

        private Point _FixPointRelToShapeCont;
        private Point _EditingPointRelToShapeCont;

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            _EditingPointRelToShapeCont = pointRelativeToPathContainer;
            if (ContainerOfStartPoint.View is ArrowLine)
            {
                ArrowLine line = ContainerOfStartPoint.View as ArrowLine;
                if (_PointToEditIsX1Y1)
                    return ArrowLine.GetPathGeometry(pointRelativeToPathContainer, _FixPointRelToShapeCont, line.ArrowEnds, line.ArrowLength, line.ArrowAngle, line.IsArrowClosed);
                else
                    return ArrowLine.GetPathGeometry(_FixPointRelToShapeCont, pointRelativeToPathContainer, line.ArrowEnds, line.ArrowLength, line.ArrowAngle, line.IsArrowClosed);
            }
            else if (ContainerOfStartPoint.View is Line)
            {
                Line line = ContainerOfStartPoint.View as Line;
                if (_PointToEditIsX1Y1)
                    return ArrowLine.GetPathGeometry(pointRelativeToPathContainer, _FixPointRelToShapeCont, ArrowEnds.None, 0, 0, false);
                else
                    return ArrowLine.GetPathGeometry(_FixPointRelToShapeCont, pointRelativeToPathContainer, ArrowEnds.None, 0, 0, false);
            }
            return new PathGeometry();
        }

        public override bool StopEditingPoint(Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container)
        {
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                endPoint = SnapPointToRaster(endPoint, DesignPanel.RasterSize);

            PointCollection _PointCollectionRelToShapeCont = new PointCollection();
            if (_PointToEditIsX1Y1)
            {
                _PointCollectionRelToShapeCont.Add(endPoint);
                _PointCollectionRelToShapeCont.Add(_FixPointRelToShapeCont);
            }
            else
            {
                _PointCollectionRelToShapeCont.Add(_FixPointRelToShapeCont);
                _PointCollectionRelToShapeCont.Add(endPoint);
            }

            Point startPoint;
            PointCollection points = TranslatePointsToBounds(_PointCollectionRelToShapeCont, out startPoint, out endPoint);

            if (myCreatedItem.ComponentType.IsAssignableFrom(typeof(ArrowLine)))
            {
                myCreatedItem.Properties[ArrowLine.X1Property].SetValue(points[0].X);
                myCreatedItem.Properties[ArrowLine.Y1Property].SetValue(points[0].Y);
                myCreatedItem.Properties[ArrowLine.X2Property].SetValue(points[1].X);
                myCreatedItem.Properties[ArrowLine.Y2Property].SetValue(points[1].Y);
            }
            else if (myCreatedItem.ComponentType.IsAssignableFrom(typeof(Line)))
            {
                myCreatedItem.Properties[Line.X1Property].SetValue(points[0].X);
                myCreatedItem.Properties[Line.Y1Property].SetValue(points[0].Y);
                myCreatedItem.Properties[Line.X1Property].SetValue(points[1].X);
                myCreatedItem.Properties[Line.Y1Property].SetValue(points[1].Y);
            }
            DrawPolylineAdorner.GetRectForPlacementOperation(startPoint, endPoint, out result);
            return true;
        }
    }
}
