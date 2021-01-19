// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.Extensions;
using System.Windows.Controls;
using System.Windows;
using gip.ext.designer.Controls;
using System.Diagnostics;
using gip.ext.xamldom;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using gip.ext.designer.Services;
using gip.ext.design;
using gip.ext.graphics.shapes;
using System.Windows.Shapes;

namespace gip.ext.designer.Extensions
{
    [ExtensionFor(typeof(Polygon))]
    [ExtensionFor(typeof(Polyline))]
    [ExtensionFor(typeof(Line))]
    [ExtensionFor(typeof(ArrowLineBase))]
    public class ShapeEditPointsHandler : BehaviorExtension, IHandleDrawToolMouseDown
    {
        public ShapeEditPointsHandler()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ExtendedItem.AddBehavior(typeof(ShapeEditPointsHandler), this);
        }

        public void HandleStartDrawingOnMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result, IDrawingTool tool)
        {
            DrawingToolEditPoints DrawingTool = designPanel.Context.Services.Tool.CurrentTool as DrawingToolEditPoints;
            if (DrawShapesGesture.IsGestureActive)
                return;
            if (result.AdornerHit == null || result.AdornerHit.AdornedDesignItem == null)
                return;

            var ancestors = (result.AdornerHit.AdornedDesignItem.View as DependencyObject).GetVisualAncestors();
            var queryPanels = ancestors.OfType<Panel>();
            if (!queryPanels.Any())
                return;
            foreach (Panel panel in queryPanels)
            {
                DesignItem containerForShape = designPanel.Context.Services.Component.GetDesignItem(panel);
                if (containerForShape != null)
                {
                    foreach (UIElement adorner in result.AdornerHit.Children)
                    {
                        if (adorner is ShapePointAdorner)
                        {
                            FrameworkElement source = e.OriginalSource as FrameworkElement;
                            if ((source != null) && (source.TemplatedParent != null) && (source.TemplatedParent == adorner))
                            {
                                new EditPointsGesture(result.AdornerHit.AdornedDesignItem, containerForShape, (adorner as ShapePointAdorner)._pointToAdorn).Start(designPanel, e);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }

    [CLSCompliant(false)]
    sealed public class EditPointsGesture : MouseMoveAndDrawGestureBase
    {
        public static bool IsGestureActive = false;

        private Point _pointToEdit;

        public EditPointsGesture(DesignItem shapeToEdit, DesignItem containerForShape, Point pointToEdit)
            : base(shapeToEdit, containerForShape, false)
        {
            this.positionRelativeTo = shapeToEdit.View;
            _pointToEdit = pointToEdit;
        }

        public override DrawShapesAdornerBase GenerateShapeDrawer(MouseEventArgs e)
        {
            if ((ContainerOfStartPoint.View is ArrowLine) || (ContainerOfStartPoint.View is Line))
            {
                return new DrawLineEditPointAdorner(ContainerOfStartPoint, ContainerForShape, _pointToEdit);
            }
            else if ((ContainerOfStartPoint.View is ArrowPolyline) || (ContainerOfStartPoint.View is Polyline))
            {
                return new DrawPolylineEditPointAdorner(ContainerOfStartPoint, ContainerForShape, _pointToEdit);
            }
            else if (ContainerOfStartPoint.View is Polygon)
            {
                return new DrawPolygonEditPointAdorner(ContainerOfStartPoint, ContainerForShape, _pointToEdit);
            }
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

        protected override void StopOnClickEvent(object sender, MouseButtonEventArgs e)
        {
            if (_HasDragStarted == false)
            {
                //services.Selection.SetSelectedComponents(new DesignItem [] { _Container }, SelectionTypes.Auto);
            }
            else
            {
                DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
                if (result.VisualHit != null)
                {
                    if (ShapeDrawer is DrawShapesEditPointAdornerBase)
                    {
                        ChangeGroup changeGroup = ContainerOfStartPoint.OpenGroup("Drop Control");
                        Rect position;
                        (ShapeDrawer as DrawShapesEditPointAdornerBase).StopEditingPoint(e.GetPosition(ShapeDrawer.ContainerForShape.View), out position, ContainerOfStartPoint, ContainerForShape);
                        DesignItem[] extendedItemArray = new DesignItem[1];
                        extendedItemArray[0] = ContainerOfStartPoint;
                        IPlacementBehavior resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
                        if (resizeBehavior != null)
                        {
                            PlacementOperation operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
                            ModelTools.Resize(ContainerOfStartPoint, position.Width, position.Height);

                            if (operation != null)
                            {
                                var info = operation.PlacedItems[0];
                                var position2 = info.OriginalBounds;

                                info.Bounds = position.Round();
                                operation.CurrentContainerBehavior.BeforeSetPosition(operation);
                                operation.CurrentContainerBehavior.SetPosition(info);
                                operation.Commit();
                            }
                        }

                        changeGroup.Commit();
                    }
                }
            }
            Stop();
        }

    }
}
