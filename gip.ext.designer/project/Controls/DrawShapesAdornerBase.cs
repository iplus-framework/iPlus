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
using gip.ext.designer.Xaml;
using System.Windows.Shapes;
using gip.ext.design;

namespace gip.ext.designer.Controls
{
    public abstract class DrawShapesAdornerBase : FrameworkElement
    {
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

        public static readonly DependencyProperty GeometryProperty
            = DependencyProperty.Register("Geometry", typeof(Geometry), typeof(DrawShapesAdornerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Geometry Geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        public static Brush s_ShapeStroke = Brushes.LightSlateGray;
        public static double s_ShapeStrokeThickness = 1;
        public static Brush s_ShapeFill;
        public static DoubleCollection s_ShapeStrokeDashArray;
        public static Nullable<PenLineCap> s_ShapeStrokeDashCap; // = PenLineCap.Flat;
        public static Nullable<double> s_ShapeStrokeDashOffset;// = 0;
        public static Nullable<PenLineCap> s_ShapeStrokeEndLineCap;// = PenLineCap.Flat;
        public static Nullable<PenLineJoin> s_ShapeStrokeLineJoin;// = PenLineJoin.Miter;
        public static Nullable<double> s_ShapeStrokeMiterLimit;// = 10;
        public static Nullable<PenLineCap> s_ShapeStrokeStartLineCap;// = PenLineCap.Flat;

        protected static Pen DefaultDrawingPen
        {
            get
            {
                Pen pen = new Pen(s_ShapeStroke, s_ShapeStrokeThickness);
                if (s_ShapeStrokeLineJoin.HasValue)
                    pen.LineJoin = s_ShapeStrokeLineJoin.Value;
                if (s_ShapeStrokeDashCap.HasValue)
                    pen.DashCap = s_ShapeStrokeDashCap.Value;
                if (s_ShapeStrokeDashArray != null && s_ShapeStrokeDashOffset.HasValue)
                    pen.DashStyle = new DashStyle(s_ShapeStrokeDashArray, s_ShapeStrokeDashOffset.Value);
                if (s_ShapeStrokeEndLineCap.HasValue)
                    pen.EndLineCap = s_ShapeStrokeEndLineCap.Value;
                if (s_ShapeStrokeMiterLimit.HasValue)
                    pen.MiterLimit = s_ShapeStrokeMiterLimit.Value;
                if (s_ShapeStrokeStartLineCap.HasValue)
                    pen.StartLineCap = s_ShapeStrokeStartLineCap.Value;
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
                return s_ShapeFill;
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
        public virtual void DrawPath(DependencyObject hitObject, MouseEventArgs e)
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

        public virtual void OnIntermediateClick(DependencyObject hitObject, MouseEventArgs e)
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

        public virtual void OnIntermediateClick(Point pointRelativeToPathContainer, DependencyObject hitObject, MouseEventArgs e)
        {
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(Fill, DrawingPen, this.Geometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the VBEdgeAdorner does.
            //dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }

        public abstract Geometry GetGeometry(Point pointRelativeToPathContainer);

        public abstract DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null);

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
                result = new Rect();
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


        public static PointCollection TranslatePointsToBounds(PointCollection pointsToTranslate, out Point startPoint, out Point endPoint)
        {
            GetBounds(pointsToTranslate, out startPoint, out endPoint);
            PointCollection pointCollection = new PointCollection();
            foreach (Point p in pointsToTranslate)
            {
                pointCollection.Add(new Point(p.X - startPoint.X, p.Y - startPoint.Y));
            }
            return pointCollection;
        }

        public static void GetBounds(PointCollection pCol, out Point startPoint, out Point endPoint)
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
                        case "Stroke":
                            if (shape.Stroke != null)
                                s_ShapeStroke = shape.Stroke.Clone();
                            break;
                        case "StrokeThickness":
                            s_ShapeStrokeThickness = shape.StrokeThickness;
                            break;
                        case "Fill":
                            if (shape.Fill != null)
                                s_ShapeFill = shape.Fill.Clone();
                            break;
                        case "StrokeDashArray":
                            if (shape.StrokeDashArray != null)
                                s_ShapeStrokeDashArray = shape.StrokeDashArray.Clone();
                            break;
                        case "StrokeDashCap":
                            s_ShapeStrokeDashCap = shape.StrokeDashCap;
                            break;
                        case "StrokeDashOffset":
                            s_ShapeStrokeDashOffset = shape.StrokeDashOffset;
                            break;
                        case "StrokeEndLineCap":
                            s_ShapeStrokeEndLineCap = shape.StrokeEndLineCap;
                            break;
                        case "StrokeLineJoin":
                            s_ShapeStrokeLineJoin = shape.StrokeLineJoin;
                            break;
                        case "StrokeMiterLimit":
                            s_ShapeStrokeMiterLimit = shape.StrokeMiterLimit;
                            break;
                        case "StrokeStartLineCap":
                            s_ShapeStrokeStartLineCap = shape.StrokeStartLineCap;
                            break;
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
                item.Properties[Shape.StrokeDashArrayProperty].SetValue(s_ShapeStrokeDashArray.Clone());
            if (s_ShapeStrokeDashCap.HasValue)
                item.Properties[Shape.StrokeDashCapProperty].SetValue(s_ShapeStrokeDashCap.Value);
            if (s_ShapeStrokeDashOffset.HasValue)
                item.Properties[Shape.StrokeDashOffsetProperty].SetValue(s_ShapeStrokeDashOffset.Value);
            if (s_ShapeStrokeEndLineCap.HasValue)
                item.Properties[Shape.StrokeEndLineCapProperty].SetValue(s_ShapeStrokeEndLineCap.Value);
            if (s_ShapeStrokeLineJoin.HasValue)
                item.Properties[Shape.StrokeLineJoinProperty].SetValue(s_ShapeStrokeLineJoin.Value);
            if (s_ShapeStrokeMiterLimit.HasValue)
                item.Properties[Shape.StrokeMiterLimitProperty].SetValue(s_ShapeStrokeMiterLimit.Value);
            if (s_ShapeStrokeStartLineCap.HasValue)
                item.Properties[Shape.StrokeStartLineCapProperty].SetValue(s_ShapeStrokeStartLineCap.Value);
        }

        public static Point SnapPointToRaster(Point point, double rasterSize)
        {
            Point pointRaster = new Point();
            double modX = point.X % rasterSize;
            double modY = point.Y % rasterSize;
            double halfRasterSize = rasterSize * 0.5;
            if (modX > halfRasterSize)
            {
                pointRaster.X = point.X + rasterSize - modX;
            }
            else
            {
                pointRaster.X = point.X - modX;
            }
            if (modY > halfRasterSize)
            {
                pointRaster.Y = point.Y + rasterSize - modY;
            }
            else
            {
                pointRaster.Y = point.Y - modY;
            }
            return pointRaster;
        }
        #endregion
    }
}
