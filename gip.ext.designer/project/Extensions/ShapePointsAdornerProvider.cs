// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using gip.ext.design.Adorners;
using gip.ext.designer.Controls;
using gip.ext.design.Extensions;
using gip.ext.design;
using gip.ext.graphics.shapes;
using System.Windows.Shapes;
using gip.ext.designer.Services;

namespace gip.ext.designer.Extensions
{
    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> for <see cref="VBVisual"/>.
    /// </summary>
    [ExtensionFor(typeof(Polygon))]
    [ExtensionFor(typeof(Polyline))]
    [ExtensionFor(typeof(Line))]
    [ExtensionFor(typeof(ArrowLineBase))]
    [ExtensionServer(typeof(DrawingExtensionServer))]
    public class ShapePointsAdornerProvider : AdornerProvider
    {
        public sealed class ShapePointsAdornerPlacement : AdornerPlacement
        {
            readonly Shape _Shape;
            readonly Point _PointToAdorn;
            public ShapePointsAdornerPlacement(Shape shape, Point pointToAdorn)
            {
                this._Shape = shape;
                this._PointToAdorn = pointToAdorn;
            }

            public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
            {
                if (adorner is ShapePointAdorner)
                {
                    ShapePointAdorner shapePointAdorner = adorner as ShapePointAdorner;
                    Point relativePoint = _Shape.TranslatePoint(_PointToAdorn, shapePointAdorner._shapeDesignItem.View);
                    relativePoint.X -= 3;
                    relativePoint.Y -= 3;
                    adorner.Arrange(new Rect(relativePoint, new Size(6, 6)));
                }
                else
                    adorner.Arrange(new Rect(adornedElementSize));
            }

            public Shape ConnectorPoint
            {
                get
                {
                    return _Shape;
                }
            }
        }


        AdornerPanel adornerPanel = new AdornerPanel();

        public ShapePointsAdornerProvider()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Adorners.Add(adornerPanel);
            if (this.ExtendedItem.View != null)
                DecorateShapes(this.ExtendedItem.View);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
        }

        private void DecorateShapes(DependencyObject obj)
        {
            if (!(Context.Services.Tool.CurrentTool is DrawingToolEditPoints))
                return;
            if (obj is Shape)
            {
                if (obj is ArrowLine)
                {
                    ArrowLine line = obj as ArrowLine;

                    Point point = new Point(line.X1, line.Y1);
                    ShapePointAdorner adorner = new ShapePointAdorner(this.ExtendedItem, point);
                    AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                    adornerPanel.Children.Add(adorner);

                    point = new Point(line.X2, line.Y2);
                    adorner = new ShapePointAdorner(this.ExtendedItem, point);
                    AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                    adornerPanel.Children.Add(adorner);
                }
                else if (obj is Line)
                {
                    Line line = obj as Line;

                    Point point = new Point(line.X1, line.Y1);
                    ShapePointAdorner adorner = new ShapePointAdorner(this.ExtendedItem, point);
                    AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                    adornerPanel.Children.Add(adorner);

                    point = new Point(line.X2, line.Y2);
                    adorner = new ShapePointAdorner(this.ExtendedItem, point);
                    AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                    adornerPanel.Children.Add(adorner);
                }
                else if (obj is ArrowPolyline)
                {
                    ArrowPolyline polyline = obj as ArrowPolyline;
                    foreach (Point point in polyline.Points)
                    {
                        ShapePointAdorner adorner = new ShapePointAdorner(this.ExtendedItem, point);
                        AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                        adornerPanel.Children.Add(adorner);
                    }
                }
                else if (obj is Polygon)
                {
                    Polygon polyline = obj as Polygon;
                    foreach (Point point in polyline.Points)
                    {
                        ShapePointAdorner adorner = new ShapePointAdorner(this.ExtendedItem, point);
                        AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerPlacement(obj as Shape, point));
                        adornerPanel.Children.Add(adorner);
                    }
                }
                return;
            }
        }
    }
}
