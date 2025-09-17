using System;
using System.Collections.Generic;
using gip.core.layoutengine.avui.VisualControlAnalyser;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using gip.ext.designer.avui.Controls;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Draws a adorner around <see cref="VBEdge"/>.
    /// </summary>
    public class DrawVBEdgeAdorner : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawVBEdgeAdorner(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape, VBConnector sourceConnector, IACComponentDesignManager designManager)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
            _SourceConnector = sourceConnector;
            _DesignManager = designManager;
        }
        #endregion

        public static VBConnector _SourceConnector;
        public VBConnector SourceConnector
        {
            get { return _SourceConnector; }
        }


        private VBConnector _HitConnector;
        public VBConnector HitConnector
        {
            get { return _HitConnector; }
            set { _HitConnector = value; }
        }

        public static IACComponentDesignManager _DesignManager;
        public IACComponentDesignManager DesignManager
        {
            get
            {
                return _DesignManager;
            }
        }

        public override void DrawPath(DependencyObject hitObject, PointerEventArgs e)
        {
            if ((hitObject != null) && !(hitObject is VBConnector))
            {
                DependencyObject hitConnectorObject = VBVisualTreeHelper.FindParentObjectInVisualTree(hitObject, typeof(VBConnector));
                if ((hitConnectorObject != null) && (hitConnectorObject is VBConnector))
                    HitConnector = hitObject as VBConnector;
                else
                    HitConnector = null;
            }
            else if ((hitObject != null) && (hitObject is VBConnector))
                HitConnector = hitObject as VBConnector;
            else
                HitConnector = null;
            base.DrawPath(hitObject, e);
        }

        public override Geometry GetGeometry(Point pointRelativeToPathContainer)
        {
            return VBEdge.GetLineGeometry(StartPointRelativeToShapeContainer, pointRelativeToPathContainer);
        }

        /// <summary>
        /// Erzeugen einer VBEdge für eine mit dem Designer hinzugefügte Edge
        /// Es wird DesignItem und VBEdge-Datensatz erzeugt
        /// </summary>
        /// <param name="hitTest"></param>
        /// <returns></returns>
        public override DesignItem CreateShapeInstanceForDesigner(DesignPanelHitTestResult hitTest, MouseButtonEventArgs e = null)
        {
            DependencyObject vbConnector = null;
            if (e != null && hitTest.VisualHit is VBEdge)
            {
                VBEdge vbEdge = hitTest.VisualHit as VBEdge;
                Point hitPoint = e.GetPosition(DesignPanel);
                Point targetPoint = vbEdge.Target.TransformToAncestor(vbEdge.Parent as UIElement).Transform(new Point(0, 0));
                if (Math.Abs(hitPoint.X - targetPoint.X) < 5 && Math.Abs(hitPoint.Y - targetPoint.Y) < 5)
                    vbConnector = vbEdge.Target;
            }
            else
                vbConnector = VBVisualTreeHelper.FindParentObjectInVisualTree(hitTest.VisualHit, typeof(VBConnector));
            if (vbConnector == null)
            {
                _SourceConnector = null;
                return null;
            }
            HitConnector = (vbConnector as VBConnector);
            if (HitConnector == this.SourceConnector)
            {
                _SourceConnector = null;
                return null;
            }
            if (this.Geometry == null)
            {
                _SourceConnector = null;
                return null;
            }
            if (string.IsNullOrEmpty(HitConnector.Name))
            {
                _SourceConnector = null;
                return null;
            }

            if (DesignManager != null)
            {
                if (!DesignManager.IsEnabledCreateEdge(SourceConnector, HitConnector))
                    return null;
            }
            String fromXName = VBVisualTreeHelper.GetVBContentsAsXName(this.SourceConnector, this.ContainerForShape.View);
            String toXName = VBVisualTreeHelper.GetVBContentsAsXName(this.HitConnector, this.ContainerForShape.View);
            if (String.IsNullOrEmpty(fromXName) || String.IsNullOrEmpty(toXName))
            {
                _SourceConnector = null;
                return null;
            }

            Geometry transformedGeometry = VBEdge.TransformGeometryToZeroCoord(this.Geometry);
            
            if (transformedGeometry == null)
            {
                _SourceConnector = null;
                return null;
            }

            if (DesignManager != null)
            {
                //if (DesignManager.CanManagerCreateEdges(SourceConnector, HitConnector))
                if (DesignManager.CanManagerCreateEdges())
                {
                    ITool currentTool = ((IToolService)DesignManager.ToolService).CurrentTool;
                    var s = SourceConnector.ParentVBControl;
                    var t = HitConnector.ParentVBControl;
                    DesignManager.CreateEdge(SourceConnector, HitConnector);
                    ((IToolService)DesignManager.ToolService).CurrentTool = currentTool;
                    return null;
                }
            }

            VBEdge newInstance = (VBEdge)ContainerForShape.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBEdge), null);
            DesignItem item = ContainerForShape.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                ApplyDefaultPropertiesToItem(item);
                item.Properties[VBEdge.ACName1Property].SetValue(fromXName);
                item.Properties[VBEdge.ACName2Property].SetValue(toXName);
                item.Properties[VBEdge.StrokeProperty].SetValue("#FFFFFF");
                item.Properties[VBEdge.StrokeThicknessProperty].SetValue(1.5);

                //DesignItemProperty dataPropertyItem = item.Properties[VBEdge.DataProperty];
                //dataPropertyItem.SetValue(VBEdge.GetGeometryAsString(transformedGeometry));

                /*dataPropertyItem = item.Properties[VBConnectPath.StrokeProperty];
                dataPropertyItem.SetValue(DrawingPen.Brush);

                dataPropertyItem = item.Properties[VBConnectPath.StrokeThicknessProperty];
                dataPropertyItem.SetValue(DrawingPen.Thickness);*/

                //DesignItem firstRow = gridItem.Services.Component.RegisterComponentForDesigner(new LineGeometry());
                //rowCollection.CollectionElements.Add(firstRow);
                //item.Properties[VBConnectPath.DataProperty].SetValue(this.Geometry);
            }

            _SourceConnector = null;
            return item;

        }

    }
}
