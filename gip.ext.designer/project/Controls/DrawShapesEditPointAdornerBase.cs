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
using gip.ext.designer.Xaml;
using System.Windows.Shapes;
using gip.ext.design;

namespace gip.ext.designer.Controls
{
    public abstract class DrawShapesEditPointAdornerBase : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawShapesEditPointAdornerBase(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(pointToEdit, shapeToEdit, containerForShape)
        {
            Shape shape = shapeToEdit.View as Shape;
            if (shape != null)
            {
                _PenOfShapeToEdit = new Pen(shape.Stroke.Clone(), shape.StrokeThickness);
                if (shape.Fill != null)
                    _ShapeFill = shape.Fill.Clone();
                _PenOfShapeToEdit.LineJoin = shape.StrokeLineJoin;
                _PenOfShapeToEdit.DashCap = shape.StrokeDashCap;
                _PenOfShapeToEdit.DashStyle = new DashStyle(shape.StrokeDashArray, shape.StrokeDashOffset);
                _PenOfShapeToEdit.EndLineCap = shape.StrokeEndLineCap;
                _PenOfShapeToEdit.StartLineCap = shape.StrokeStartLineCap;
                _PenOfShapeToEdit.MiterLimit = shape.StrokeMiterLimit;
            }
        }

        #endregion

        #region Properties
        protected Pen _PenOfShapeToEdit;
        protected Brush _ShapeFill;

        public override Pen DrawingPen
        {
            get
            {
                return _PenOfShapeToEdit;
            }
        }

        public override Brush Fill
        {
            get
            {
                return _ShapeFill;
            }
        }


        #endregion

        #region Methods
        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null)
        {
            return null;
        }

        public abstract bool StopEditingPoint(Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container);
        #endregion
    }
}
