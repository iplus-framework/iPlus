// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Xaml;
using gip.ext.design.avui;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;

namespace gip.ext.designer.avui.Controls
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
                (shape.Stroke as SolidColorBrush).ToImmutable();
                _PenOfShapeToEdit = new Pen((shape.Stroke as SolidColorBrush).Color.ToUInt32(), shape.StrokeThickness, new DashStyle(shape.StrokeDashArray, shape.StrokeDashOffset), shape.StrokeLineCap);
                _PenOfShapeToEdit.LineJoin = shape.StrokeJoin;
                _PenOfShapeToEdit.LineCap = shape.StrokeLineCap;
                //_PenOfShapeToEdit.MiterLimit = shape;
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
        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, PointerEventArgs e = null)
        {
            return null;
        }

        public abstract bool StopEditingPoint(Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container);
        #endregion
    }
}
