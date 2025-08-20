// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.Extensions;
using System.Windows.Controls;
using System.Windows;
using gip.ext.designer.avui.Controls;
using System.Diagnostics;
using gip.ext.xamldom.avui;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Extensions
{
    /// <summary>
    /// Handles selection multiple controls inside a Panel.
    /// </summary>
    [ExtensionFor(typeof(FrameworkElement))]
    public class ShapeDrawingHandler : BehaviorExtension, IHandleDrawToolMouseDown
    {
        public ShapeDrawingHandler()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ExtendedItem.AddBehavior(typeof(IHandleDrawToolMouseDown), this);
        }

        public void HandleStartDrawingOnMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result, IDrawingTool tool)
        {
            if ((designPanel.Context.Services.Tool.CurrentTool != null) && (designPanel.Context.Services.Tool.CurrentTool is IDrawingTool))
            {
                IDrawingTool DrawingTool = designPanel.Context.Services.Tool.CurrentTool as IDrawingTool;
                if (DrawingTool.IsToolForShape != DrawShapeType.DrawGeneralShape)
                {
                    if (DrawShapesGesture.IsGestureActive)
                        return;

                    var ancestors = (result.ModelHit.View as DependencyObject).GetVisualAncestors();
                    var queryPanels = ancestors.OfType<Panel>();
                    if (!queryPanels.Any())
                        return;
                    foreach (Panel panel in queryPanels)
                    {
                        DesignItem containerForShape = designPanel.Context.Services.Component.GetDesignItem(panel);
                        if (containerForShape != null)
                        {
                            //new DrawConnectionGesture(result.AdornerHit.AdornedDesignItem, containerForShape, placement.ConnectorPoint).Start(designPanel, e);
                            if ((DrawingTool.IsToolForShape == DrawShapeType.DrawPolyline) || (DrawingTool.IsToolForShape == DrawShapeType.DrawPolygon))
                                new DrawShapesGesture(result.ModelHit, containerForShape, DrawingTool.IsToolForShape, true).Start(designPanel, e);
                            else
                                new DrawShapesGesture(result.ModelHit, containerForShape, DrawingTool.IsToolForShape, false).Start(designPanel, e);
                            break;
                        }
                    }

                    //if ((DrawingTool.IsToolForShape == DrawShapeType.DrawPolyline) || (DrawingTool.IsToolForShape == DrawShapeType.DrawPolygon))
                    //    new DrawShapesGesture(result.ModelHit, result.ModelHit, DrawingTool.IsToolForShape,true).Start(designPanel, e);
                    //else
                    //    new DrawShapesGesture(result.ModelHit, result.ModelHit, DrawingTool.IsToolForShape,false).Start(designPanel, e);
                }
            }
        }
    }

    [CLSCompliant(false)]
    sealed public class DrawShapesGesture : MouseMoveAndDrawGestureBase
    {
        public static bool IsGestureActive = false;

        private DrawShapeType _ShapeType;
        public DrawShapesGesture(DesignItem containerOfStartPoint, DesignItem containerForShape, DrawShapeType shapeType, bool stopWithDblClick)
            : base(containerOfStartPoint, containerForShape, stopWithDblClick)
        {
            this.positionRelativeTo = containerOfStartPoint.View;
            _ShapeType = shapeType;
        }

        public override DrawShapesAdornerBase GenerateShapeDrawer(MouseEventArgs e)
        {
            Point startPointRelativeToShapeContainer = e.GetPosition(ContainerForShape.View);
            if (_ShapeType == DrawShapeType.DrawLine)
                return new DrawLineAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape);
            else if (_ShapeType == DrawShapeType.DrawRectangle)
                return new DrawRectangleAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape);
            else if (_ShapeType == DrawShapeType.DrawEllipse)
                return new DrawEllipseAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape);
            else if (_ShapeType == DrawShapeType.DrawPolyline)
                return new DrawPolylineAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape);
            else if (_ShapeType == DrawShapeType.DrawPolygon)
                return new DrawPolygonAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape);
            return null;
        }

        protected sealed override void OnStarted(MouseButtonEventArgs e)
        {
            base.OnDragStarted(e);
            IsGestureActive = true;
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            IsGestureActive = false;
        }
    }
}
