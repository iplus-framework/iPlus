// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using gip.ext.design.Adorners;
using gip.ext.designer.Controls;
using gip.ext.design;

namespace gip.ext.designer.Services
{
    /// <summary>
    /// Gesture for Drawing Element by Click->Move->MouseUp
    /// iplus-Extension
    /// </summary>
    [CLSCompliant(false)]
    public abstract class MouseMoveAndDrawGestureBase : ClickOrDragMouseGesture
    {
        #region c'tors
        public MouseMoveAndDrawGestureBase(DesignItem containerOfStartPoint, DesignItem containerForShape, bool stopWithDblClick)
        {
            this._ContainerOfStartPoint = containerOfStartPoint;
            this._ContainerForShape = containerForShape;
            this.positionRelativeTo = containerForShape.View;
            _StopWithDblClick = stopWithDblClick;
        }
        #endregion

        #region Properties
        private bool _StopWithDblClick;

        DesignItem _ContainerOfStartPoint;
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


        AdornerPanel _AdornerPanel;
        DrawShapesAdornerBase _ShapeDrawer;

        public DrawShapesAdornerBase ShapeDrawer
        {
            get
            {
                return _ShapeDrawer;
            }
        }
        #endregion

        #region Methods
        public abstract DrawShapesAdornerBase GenerateShapeDrawer(MouseEventArgs e);

        protected override void OnDragStarted(MouseEventArgs e)
        {
            _ShapeDrawer = GenerateShapeDrawer(e);
            if (_ShapeDrawer == null)
                return;
            _AdornerPanel = new AdornerPanel();
            if (ContainerForShape.Services.Tool.CurrentTool is DrawingTool)
                (ContainerForShape.Services.Tool.CurrentTool as DrawingTool).ShapeDrawer = _ShapeDrawer;
            _AdornerPanel.SetAdornedElement(_ShapeDrawer.ContainerForShape.View, _ShapeDrawer.ContainerForShape);
            _AdornerPanel.Children.Add(_ShapeDrawer);
            designPanel.Adorners.Add(_AdornerPanel);
            System.Diagnostics.Debug.WriteLine("OnDragStarted");
        }

        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);
            if (_HasDragStarted)
            {
                DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
                SetPlacement(result, e);
            }
        }

        protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_StopWithDblClick)
            {
                StopOnClickEvent(sender, e);
            }
            else
            {
                //base.OnMouseUp(sender, e);
                if (_HasDragStarted && _ShapeDrawer != null)
                {
                    DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
                    _ShapeDrawer.OnIntermediateClick(result.VisualHit, e);
                    //RelativePlacement p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
                    //AdornerPanel.SetPlacement(_ShapeDrawer, p);
                }
            }
        }

        protected override void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_StopWithDblClick)
            {
                StopOnClickEvent(sender, e);
            }
        }

        protected virtual void StopOnClickEvent(object sender, MouseButtonEventArgs e)
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
                    //DesignItem item = _ShapeDrawer.CreateShapeInstanceForDesigner(result);
                    DesignItem item = _ShapeDrawer.CreateShapeInstanceForDesigner(result, e);
                    if (item != null)
                    {
                        ChangeGroup changeGroup = item.OpenGroup("Drop Control");
                        AddItemWithDefaultSize(_ShapeDrawer.ContainerForShape, item, _ShapeDrawer.StartPointRelativeToShapeContainer, e.GetPosition(_ShapeDrawer.ContainerForShape.View));
                        changeGroup.Commit();
                    }
                }
            }
            Stop();
        }

        protected virtual void SetPlacement(DesignPanelHitTestResult hitTest, MouseEventArgs e)
        {
            RelativePlacement p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
            AdornerPanel.SetPlacement(_ShapeDrawer, p);
            _ShapeDrawer.DrawPath(hitTest.VisualHit, e);
        }

        internal bool AddItemWithDefaultSize(DesignItem container, DesignItem createdItem, Point startPoint, Point endPoint)
        {
            Rect position;
            if (!_ShapeDrawer.MyGetRectForPlacementOperation(startPoint, endPoint, out position, createdItem, container))
                return false;
            PlacementOperation operation = PlacementOperation.TryStartInsertNewComponents(
                container,
                new DesignItem[] { createdItem },
                new Rect[] { position },
                PlacementType.AddItem
            );
            if (operation != null)
            {
                container.Services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
                operation.Commit();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnStopped()
        {
            if (_AdornerPanel != null)
            {
                designPanel.Adorners.Remove(_AdornerPanel);
                _AdornerPanel = null;
            }
            _ShapeDrawer = null;
            base.OnStopped();
        }
        #endregion
    }
}
