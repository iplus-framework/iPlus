// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using gip.ext.designer.avui.Extensions;

namespace gip.ext.designer.avui.Services
{
    public abstract class DrawingTool : IDrawingTool
    {
        static DrawingTool()
        {
        }

        public virtual Cursor Cursor
        {
            get { return Cursors.Pen; }
        }

        public void Activate(IDesignPanel designPanel)
        {
            designPanel.MouseDown += OnMouseDown;
            designPanel.PreviewMouseMove += OnPreviewMouseMove;
            designPanel.DragOver += OnMouseDragOver;
            designPanel.Drop += OnMouseDrop;
            designPanel.DragLeave += OnMouseDragLeave;
            if (designPanel is DesignPanel)
            {
                DesignPanel dp = designPanel as DesignPanel;
                ZoomControl parentControl = dp.Parent as ZoomControl;
                if (parentControl != null)
                {
                    parentControl.KeyDown += parentControl_KeyDown;
                    parentControl.KeyUp += parentControl_KeyUp;
                }
            }

            //if (!_RegisteredToEvent)
            //{
                designPanel.Context.Services.GetRequiredService<UndoService>().TransactionExecuted += new UndoServiceTransactionExecuted(DrawShapesAdornerBase_TransactionExecuted);
                //_RegisteredToEvent = true;
            //}
                OnActivated(designPanel);
        }

        protected virtual void parentControl_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        protected virtual void parentControl_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        protected virtual void OnActivated(IDesignPanel designPanel)
        {
        }

        public void Deactivate(IDesignPanel designPanel)
        {
            designPanel.MouseDown -= OnMouseDown;
            designPanel.PreviewMouseMove -= OnPreviewMouseMove;
            designPanel.DragOver -= OnMouseDragOver;
            designPanel.Drop -= OnMouseDrop;
            designPanel.DragLeave -= OnMouseDragLeave;
            if (designPanel is DesignPanel)
            {
                DesignPanel dp = designPanel as DesignPanel;
                ZoomControl parentControl = dp.Parent as ZoomControl;
                if (parentControl != null)
                {
                    parentControl.KeyDown -= parentControl_KeyDown;
                    parentControl.KeyUp -= parentControl_KeyUp;
                }
            }
            designPanel.Context.Services.GetRequiredService<UndoService>().TransactionExecuted -= DrawShapesAdornerBase_TransactionExecuted;
            OnDeactivated(designPanel);
        }

        protected virtual void OnDeactivated(IDesignPanel designPanel)
        {
        }

        void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            IDesignPanel designPanel = (IDesignPanel)sender;
            DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
            if (result.ModelHit != null)
            {
                ISelectionService selectionService = designPanel.Context.Services.DrawingService;
                if (!selectionService.IsComponentSelected(result.ModelHit))
                {
                    selectionService.SetSelectedComponents(new DesignItem[] { result.ModelHit }, SelectionTypes.Auto, this);
                }
            }
            else
            {
                ISelectionService selectionService = designPanel.Context.Services.DrawingService;
                selectionService.SetSelectedComponents(null);
            }
        }

        public virtual void OnMouseDown(object sender, PointerEventArgs e)
        {
            IDesignPanel designPanel = (IDesignPanel)sender;
            DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
            if (result.ModelHit != null)
            {
                IHandleDrawToolMouseDown b = result.ModelHit.GetBehavior<IHandleDrawToolMouseDown>();
                if (b != null)
                {
                    b.HandleStartDrawingOnMouseDown(designPanel, e, result, this);
                }
                //if (!e.Handled) {
                //    if (e.ChangedButton == MouseButton.Left && MouseGestureBase.IsOnlyButtonPressed(e, MouseButton.Left)) {
                //        e.Handled = true;
                //        ISelectionService selectionService = designPanel.Context.Services.HoverSelection;
                //        selectionService.SetSelectedComponents(new DesignItem[] { result.ModelHit }, SelectionTypes.Auto);
                //        if (selectionService.IsComponentSelected(result.ModelHit)) {
                //            new DragMoveMouseGesture(result.ModelHit, e.ClickCount == 2).Start(designPanel, e);
                //        }
                //    }
                //}
            }
        }

        protected virtual void OnMouseDragOver(object sender, DragEventArgs e)
        {
        }

        protected virtual void OnMouseDrop(object sender, DragEventArgs e)
        {
        }

        protected virtual void OnMouseDragLeave(object sender, DragEventArgs e)
        {
        }

        public virtual DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawGeneralShape; }
        }

        internal DrawShapesAdornerBase ShapeDrawer
        {
            get;
            set;
        }

        //private bool _RegisteredToEvent = false;
        static void DrawShapesAdornerBase_TransactionExecuted(object sender, UndoServiceTransactionEventArgs e)
        {
            DrawShapesAdornerBase.RefreshPen(sender, e);
            DrawLineAdorner.RefreshPen(sender, e);
            DrawRectangleAdorner.RefreshPen(sender, e);
            DrawPolygonAdorner.RefreshPen(sender, e);
            DrawPolylineAdorner.RefreshPen(sender, e);
        }


        public void RaiseToolEvent(IDesignPanel designPanel, ToolEventArgs eventArgs)
        {
            if (designPanel == null)
                return;
            designPanel.Context.Services.Tool.RaiseToolEvent(this, eventArgs);
        }
    }


    public class DrawingToolForLine : DrawingTool
    {
        public static readonly DrawingToolForLine Instance = new DrawingToolForLine();

        public override DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawLine; }
        }
    }


    public class DrawingToolForRectangle : DrawingTool
    {
        public static readonly DrawingToolForRectangle Instance = new DrawingToolForRectangle();

        public override DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawRectangle; }
        }
    }

    public class DrawingToolForEllipse : DrawingTool
    {
        public static readonly DrawingToolForEllipse Instance = new DrawingToolForEllipse();

        public override DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawEllipse; }
        }
    }

    public class DrawingToolForPolyline : DrawingTool
    {
        public static readonly DrawingToolForPolyline Instance = new DrawingToolForPolyline();

        public override DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawPolyline; }
        }
    }

    public class DrawingToolForPolygon : DrawingTool
    {
        public static readonly DrawingToolForPolygon Instance = new DrawingToolForPolygon();

        public override DrawShapeType IsToolForShape
        {
            get { return DrawShapeType.DrawPolygon; }
        }
    }

    /// <summary>
    /// Node-Manipulation-Tool
    /// </summary>
    public class DrawingToolEditPoints : DrawingTool
    {
        public static readonly DrawingToolEditPoints Instance = new DrawingToolEditPoints();

        public DrawingToolEditPoints()
        {
        }

        private Cursor _Cursor = ZoomControl.GetCursor("Images/CursorEditNode.cur");
        public override Cursor Cursor
        {
            get 
            {
                return _Cursor;
            }
        }

        public override void OnMouseDown(object sender, PointerEventArgs e)
        {
            IDesignPanel designPanel = (IDesignPanel)sender;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                DesignPanelHitTestResult result1 = designPanel.HitTest(e.GetPosition(designPanel), false, true);
                if (result1.ModelHit != null && result1.VisualHit != null && result1.VisualHit is gip.ext.graphics.avui.shapes.ArrowPolyline)
                {
                    var polyline = result1.VisualHit as gip.ext.graphics.avui.shapes.ArrowPolyline;
                    if (polyline != null)
                        polyline.InsertPoint(e.GetPosition(polyline));

                    gip.ext.design.avui.Adorners.AdornerPanel adornerPanel = null;
                    foreach (var item in designPanel.Adorners)
                        if (item.AdornedElement == result1.VisualHit)
                            adornerPanel = item;

                    if (adornerPanel != null)
                    {
                        adornerPanel.Children.Clear();
                        foreach (Point point in polyline.Points)
                        {
                            ShapePointAdorner adorner = new ShapePointAdorner(result1.ModelHit, point);
                            gip.ext.design.avui.Adorners.AdornerPanel.SetPlacement(adorner, new ShapePointsAdornerProvider.ShapePointsAdornerPlacement(polyline as Shape, point));
                            adornerPanel.Children.Add(adorner);
                        }
                    }
                }
            }
            else
            {
                DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), true, true);
                if ((result.AdornerHit != null) && (result.AdornerHit.AdornedDesignItem != null) && (result.AdornerHit.AdornedElement != null))
                {
                    IHandleDrawToolMouseDown b = result.AdornerHit.AdornedDesignItem.GetBehavior<ShapeEditPointsHandler>();
                    if (b != null)
                    {
                        b.HandleStartDrawingOnMouseDown(designPanel, e, result, this);
                    }
                }
            }
        }

        protected override void parentControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                _Cursor = ZoomControl.GetCursor("Images/CursorInsertPoint.cur");
                Mouse.UpdateCursor();
            }
        }

        protected override void parentControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                _Cursor = ZoomControl.GetCursor("Images/CursorEditNode.cur");
                Mouse.UpdateCursor();
            }
        }

        //public override DrawShapeType IsToolForShape
        //{
        //    get { return DrawShapeType.DrawLine; }
        //}

    }

}
