// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Xaml;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Collections;
using Avalonia.Input;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.Controls
{
    public abstract class DrawShapesAdornerBase : Control
    {
        static DrawShapesAdornerBase()
        {
            AffectsRender<DrawShapesAdornerBase>(GeometryProperty);
        }
        #region c'tors
        public DrawShapesAdornerBase(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base()
        {
            this._ContainerForShape = containerForShape;
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                this._StartPointRelativeToShapeContainer = SnapPointToRaster(startPointRelativeToShapeContainer, DesignPanel.RasterSize);
            else
                this._StartPointRelativeToShapeContainer = startPointRelativeToShapeContainer;
            this._ContainerOfStartPoint = containerOfStartPoint;
        }

        #endregion

        #region Properties

        public static readonly StyledProperty<Geometry> GeometryProperty = AvaloniaProperty.Register<DrawShapesAdornerBase, Geometry>(nameof(Geometry));

        public Geometry Geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        public static IBrush s_ShapeStroke = Brushes.LightSlateGray;
        public static double s_ShapeStrokeThickness = 1;
        public static IBrush s_ShapeFill;
        public static IList<double> s_ShapeStrokeDashArray;
        public static Nullable<double> s_ShapeStrokeDashOffset;// = 0;
        //public static Nullable<PenLineCap> s_ShapeStrokeEndLineCap;// = PenLineCap.Flat;
        public static Nullable<PenLineJoin> s_ShapeStrokeLineJoin;// = PenLineJoin.Miter;
        public static Nullable<double> s_ShapeStrokeMiterLimit;// = 10;
        public static Nullable<PenLineCap> s_ShapeStrokeStartLineCap;// = PenLineCap.Flat;
        public static Nullable<Stretch> s_ShapeStretch = Stretch.None;

        protected static Pen DefaultDrawingPen
        {
            get
            {
                Pen pen = new Pen(s_ShapeStroke, s_ShapeStrokeThickness);
                if (s_ShapeStrokeLineJoin.HasValue)
                    pen.LineJoin = s_ShapeStrokeLineJoin.Value;
                //if (s_ShapeStrokeDashCap.HasValue)
                //    pen.DashCap = s_ShapeStrokeDashCap.Value;
                if (s_ShapeStrokeDashArray != null && s_ShapeStrokeDashOffset.HasValue)
                    pen.DashStyle = new DashStyle(s_ShapeStrokeDashArray, s_ShapeStrokeDashOffset.Value);
                //if (s_ShapeStrokeEndLineCap.HasValue)
                //    pen.EndLineCap = s_ShapeStrokeEndLineCap.Value;
                if (s_ShapeStrokeMiterLimit.HasValue)
                    pen.MiterLimit = s_ShapeStrokeMiterLimit.Value;
                if (s_ShapeStrokeStartLineCap.HasValue)
                    pen.LineCap = s_ShapeStrokeStartLineCap.Value;
                return pen;
            }
        }

        public virtual Pen DrawingPen
        {
            get
            {
                return DefaultDrawingPen;
            }
        }

        public virtual Brush Fill
        {
            get
            {
                return s_ShapeFill as Brush;
            }
        }

        protected Point _StartPointRelativeToShapeContainer;
        public Point StartPointRelativeToShapeContainer
        {
            get
            {
                return _StartPointRelativeToShapeContainer;
            }
        }


        private DesignItem _ContainerOfStartPoint;
        public DesignItem ContainerOfStartPoint
        {
            get
            {
                return _ContainerOfStartPoint;
            }
        }

        DesignItem _ContainerForShape;
        public DesignItem ContainerForShape
        {
            get
            {
                return _ContainerForShape;
            }
        }

        protected IDesignPanel DesignPanel
        {
            get
            {
                if (ContainerForShape == null)
                    return null;
                return ContainerForShape.Services.GetService<IDesignPanel>();
            }
        }

        #endregion

        #region Methods
        public virtual void DrawPath(AvaloniaObject hitObject, PointerEventArgs e)
        {
            if (ContainerForShape == null)
                return;
            if (ContainerForShape.View == null)
                return;
            Point pointRelativeToPathContainer = e.GetPosition(ContainerForShape.View);
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                this.Geometry = GetGeometry(SnapPointToRaster(pointRelativeToPathContainer, DesignPanel.RasterSize));
            else
                this.Geometry = GetGeometry(pointRelativeToPathContainer);
            //this.InvalidateVisual();
        }

        public virtual void OnIntermediateClick(AvaloniaObject hitObject, PointerEventArgs e)
        {
            if (ContainerForShape == null)
                return;
            if (ContainerForShape.View == null)
                return;
            Point pointRelativeToPathContainer = e.GetPosition(ContainerForShape.View);
            if (DesignPanel != null && DesignPanel.IsRasterOn)
                OnIntermediateClick(SnapPointToRaster(pointRelativeToPathContainer, DesignPanel.RasterSize), hitObject, e);
            else
                OnIntermediateClick(pointRelativeToPathContainer, hitObject, e);
        }

        public virtual void OnIntermediateClick(Point pointRelativeToPathContainer, AvaloniaObject hitObject, PointerEventArgs e)
        {
        }

        public override void Render(DrawingContext dc)
        {
            base.Render(dc);
            dc.DrawGeometry(Fill, DrawingPen, this.Geometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the VBEdgeAdorner does.
            //dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }

        public abstract Geometry GetGeometry(Point pointRelativeToPathContainer);

        public abstract DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, PointerEventArgs e = null);

        public virtual bool MyGetRectForPlacementOperation(Point startPoint, Point endPoint, out Rect result, DesignItem myCreatedItem, DesignItem container)
        {
            return GetRectForPlacementOperation(startPoint, endPoint, out result, myCreatedItem, container);
        }

        public static bool GetRectForPlacementOperation(Point startPoint, Point endPoint, out Rect result, DesignItem myCreatedItem = null, DesignItem container = null)
        {
            if (container != null)
            {
                IDesignPanel designPanel = container.Services.GetService<IDesignPanel>();
                if (designPanel != null && designPanel.IsRasterOn)
                {
                    endPoint = SnapPointToRaster(endPoint, designPanel.RasterSize);
                }
            }

            double width = endPoint.X - startPoint.X;
            double height = endPoint.Y - startPoint.Y;
            if ((width == 0) && (height == 0))
            {
                result = RectExtensions.Empty();
                return false;
            }
            if (width == 0)
                width = 1;
            if (height == 0)
                height = 1;
            Point position;
            // Rechteck nach rechts unten
            if ((width > 0) && (height > 0))
            {
                position = startPoint;
            }
            // Rechteck nach rechts oben
            else if ((width > 0) && (height < 0))
            {
                position = new Point(startPoint.X, startPoint.Y + height);
                height *= -1;
            }
            // Rechteck nach links unten
            else if ((width < 0) && (height > 0))
            {
                position = new Point(startPoint.X + width, startPoint.Y);
                width *= -1;
            }
            // Rechteck nach links oben
            else //if ((width < 0) && (height < 0))
            {
                position = new Point(startPoint.X + width, startPoint.Y + height);
                height *= -1;
                width *= -1;
            }
            result = new Rect(position, new Size(width, height)).Round();
            return true;
        }


        public static IList<Point> TranslatePointsToBounds(IList<Point> pointsToTranslate, out Point startPoint, out Point endPoint)
        {
            GetBounds(pointsToTranslate, out startPoint, out endPoint);
            IList<Point> pointCollection = new List<Point>();
            foreach (Point p in pointsToTranslate)
            {
                pointCollection.Add(new Point(p.X - startPoint.X, p.Y - startPoint.Y));
            }
            return pointCollection;
        }

        public static void GetBounds(IList<Point> pCol, out Point startPoint, out Point endPoint)
        {
            double mostLeft = 99999;
            double mostRight = -99999;
            double mostTop = 99999;
            double mostDown = -99999;
            int count = 0;
            foreach (Point p in pCol)
            {
                if (p.X > mostRight)
                    mostRight = p.X;
                if (p.X < mostLeft)
                    mostLeft = p.X;
                if (p.Y < mostTop)
                    mostTop = p.Y;
                if (p.Y > mostDown)
                    mostDown = p.Y;
                count++;
            }

            startPoint = new Point(mostLeft, mostTop);
            endPoint = new Point(mostRight, mostDown);
        }


        public virtual void RefreshMyPen(object sender, UndoServiceTransactionEventArgs e)
        {
            DrawShapesAdornerBase.RefreshPen(sender, e);
        }

        public static void RefreshPen(object sender, UndoServiceTransactionEventArgs e)
        {
            //var query = e.AffectedItem.AffectedElements.Where(c => c.Name == "Stroke");
            if (e.AffectedItem is XamlModelProperty.PropertyChangeAction)
            {
                XamlModelProperty.PropertyChangeAction changeAction = e.AffectedItem as XamlModelProperty.PropertyChangeAction;
                if ((changeAction.Property.DesignItem != null)
                    && (changeAction.Property.DesignItem.View != null)
                    && (changeAction.Property.DesignItem.View is Shape))
                {
                    Shape shape = changeAction.Property.DesignItem.View as Shape;
                    switch (changeAction.Property.Name)
                    {
                        case nameof(Shape.Stroke):
                            if (shape.Stroke != null)
                                s_ShapeStroke = shape.Stroke.Clone();
                            break;
                        case nameof(Shape.StrokeThickness):
                            s_ShapeStrokeThickness = shape.StrokeThickness;
                            break;
                        case nameof(Shape.Fill):
                            if (shape.Fill != null)
                                s_ShapeFill = shape.Fill.Clone();
                            break;
                        case nameof(Shape.StrokeDashArray):
                            if (shape.StrokeDashArray != null)
                                s_ShapeStrokeDashArray = shape.StrokeDashArray.ToList();
                            break;
                        case nameof(Shape.StrokeDashOffset):
                            s_ShapeStrokeDashOffset = shape.StrokeDashOffset;
                            break;
                        //case nameof(Shape.StrokeLineCap):
                        //    s_ShapeStrokeEndLineCap = shape.StrokeLineCap;
                        //    break;
                        case nameof(Shape.StrokeJoin):
                            s_ShapeStrokeLineJoin = shape.StrokeJoin;
                            break;
                        //case "StrokeMiterLimit":
                        //    s_ShapeStrokeMiterLimit = shape.StrokeMiterLimit;
                        //    break;
                        //case "StrokeStartLineCap":
                        //    s_ShapeStrokeStartLineCap = shape.StrokeStartLineCap;
                        //    break;
                        case nameof(Shape.Stretch):
                            s_ShapeStretch = shape.Stretch;
                            break;
                    }
                    IPen pen = shape.GetPen();
                    if (pen != null)
                    {
                        s_ShapeStrokeMiterLimit = pen.MiterLimit;
                        s_ShapeStrokeStartLineCap = pen.LineCap;
                    }
                }
            }
        }

        protected virtual void ApplyDefaultPropertiesToItem(DesignItem item)
        {
            ApplyDefaultPropertiesToItemS(item);
        }

        public static void ApplyDefaultPropertiesToItemS(DesignItem item)
        {
            if (s_ShapeStroke != null)
                item.Properties[Shape.StrokeProperty].SetValue(s_ShapeStroke.Clone());
            item.Properties[Shape.StrokeThicknessProperty].SetValue(s_ShapeStrokeThickness);
            if (s_ShapeFill != null)
                item.Properties[Shape.FillProperty].SetValue(s_ShapeFill.Clone());
            if (s_ShapeStrokeDashArray != null)
                item.Properties[Shape.StrokeDashArrayProperty].SetValue(new AvaloniaList<double>(s_ShapeStrokeDashArray));
            //if (s_ShapeStrokeDashCap.HasValue)
            //    item.Properties[Shape.StrokeDashCapProperty].SetValue(s_ShapeStrokeDashCap.Value);
            if (s_ShapeStrokeDashOffset.HasValue)
                item.Properties[Shape.StrokeDashOffsetProperty].SetValue(s_ShapeStrokeDashOffset.Value);
            //if (s_ShapeStrokeEndLineCap.HasValue)
            //    item.Properties[Shape.StrokeEndLineCapProperty].SetValue(s_ShapeStrokeEndLineCap.Value);
            if (s_ShapeStrokeLineJoin.HasValue)
                item.Properties[Shape.StrokeJoinProperty].SetValue(s_ShapeStrokeLineJoin.Value);
            // Pen-Property in Avalonia:
            //if (s_ShapeStrokeMiterLimit.HasValue)
            //    item.Properties[Shape.StrokeMiterLimitProperty].SetValue(s_ShapeStrokeMiterLimit.Value);
            if (s_ShapeStrokeStartLineCap.HasValue)
                item.Properties[Shape.StrokeLineCapProperty].SetValue(s_ShapeStrokeStartLineCap.Value);
            if (s_ShapeStretch.HasValue)
                item.Properties[Shape.StretchProperty].SetValue(s_ShapeStretch.Value);
        }

        public static Point SnapPointToRaster(Point point, double rasterSize)
        {
            double pointRasterX;
            double pointRasterY;
            double modX = point.X % rasterSize;
            double modY = point.Y % rasterSize;
            double halfRasterSize = rasterSize * 0.5;
            if (modX > halfRasterSize)
            {
                pointRasterX = point.X + rasterSize - modX;
            }
            else
            {
                pointRasterX = point.X - modX;
            }
            if (modY > halfRasterSize)
            {
                pointRasterY = point.Y + rasterSize - modY;
            }
            else
            {
                pointRasterY = point.Y - modY;
            }
            return new Point(pointRasterX, pointRasterY);
        }
        #endregion
    }
}
