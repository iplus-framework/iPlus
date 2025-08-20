// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.
//#define POLY

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Controls;

namespace gip.core.layoutengine.avui
{
    //[ExtensionServer(typeof(DependentDrawingsExtensionServer))]
    [ExtensionServer(typeof(DefaultExtensionServer.Permanent))]
    public class RelocationExtension : DefaultExtension, IDependentDrawingsBehavior
    {
        public virtual void BeginPlacement(PlacementOperation operation)
        {
        }

        public virtual void SetPosition(PlacementInformation info)
        {
        }

        public virtual void EndPlacement(PlacementOperation operation)
        {
        }

        public virtual void PlacementAborted(PlacementOperation operation)
        {
        }
    }


    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> for <see cref="VBVisual"/>.
    /// </summary>
    [ExtensionFor(typeof(VBVisual), OverrideExtension = typeof(RelocationExtension))]
    [ExtensionFor(typeof(VBConnector), OverrideExtension = typeof(RelocationExtension))]
    public class VBConnectorRelocationBehavior : RelocationExtension
    {
        public VBConnectorRelocationBehavior()
            : base()
        {
        }
        /*sealed class VBConnectorRelocationPlacement : AdornerPlacement
        {
            readonly VBConnector _ConnectorPoint;
            public VBConnectorRelocationPlacement(VBConnector connectorPoint) { this._ConnectorPoint = connectorPoint; }

            public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
            {
                if (adorner is VBConnectorAdorner)
                {
                    VBConnectorAdorner VBConnectorAdorner = adorner as VBConnectorAdorner;
                    Point relativePoint = _ConnectorPoint.TranslatePoint(new Point(0, 0), VBConnectorAdorner._VBConnectorRelocationItem.View);
                    adorner.Arrange(new Rect(relativePoint, new Size(_ConnectorPoint.ActualWidth, _ConnectorPoint.ActualHeight)));
                }
                else
                    adorner.Arrange(new Rect(adornedElementSize));
            }
        }


        AdornerPanel adornerPanel = new AdornerPanel();*/

        PlacementOperation operationOnEdges;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            IDependentDrawingsBehavior behavior = ExtendedItem.GetBehavior<IDependentDrawingsBehavior>();
            if (behavior == null)
                ExtendedItem.AddBehavior(typeof(IDependentDrawingsBehavior), this);
        }

        public override void BeginPlacement(PlacementOperation operation)
        {
            if (this.ExtendedItem.View != null)
            {
                if (operationOnEdges != null)
                {
                    operationOnEdges.Abort();
                    operationOnEdges = null;
                }
                List<VBEdge> edges = new List<VBEdge>();
                AttachToVBConnectorRelocationEvent(this.ExtendedItem.View, false, edges);
                if (edges.Count > 0)
                {
                    List<DesignItem> items = new List<DesignItem>();
                    foreach (VBEdge edge in edges)
                    {
                        DesignItem item = this.ExtendedItem.Services.Component.GetDesignItem(edge);
                        if (item != null)
                            items.Add(item);
                    }
                    if (items.Count > 0)
                    {
                        operationOnEdges = PlacementOperation.Start(items, PlacementType.MoveDependentItems);
                    }
                }
            }
        }

        public override void SetPosition(PlacementInformation info)
        {
        }

        public override void EndPlacement(PlacementOperation operation)
        {
            if (this.ExtendedItem.View != null)
            {
                if (operationOnEdges != null)
                {
                    operationOnEdges.Commit();
                    operationOnEdges = null;
                }
                List<VBEdge> edges = new List<VBEdge>();
                AttachToVBConnectorRelocationEvent(this.ExtendedItem.View, true, edges);
            }
        }

        public override void PlacementAborted(PlacementOperation operation)
        {
            if (this.ExtendedItem.View != null)
            {
                if (operationOnEdges != null)
                {
                    operationOnEdges.Abort();
                    operationOnEdges = null;
                }
                List<VBEdge> edges = new List<VBEdge>();
                AttachToVBConnectorRelocationEvent(this.ExtendedItem.View, true, edges);
            }
        }


        protected override void OnRemove()
        {
            base.OnRemove();
        }

        private void AttachToVBConnectorRelocationEvent(DependencyObject obj, bool removeEventHandler, List<VBEdge> edges)
        {
            if (obj is VBConnector)
            {
                if (!String.IsNullOrEmpty((obj as VBConnector).Name))
                {
                    /*VBConnectorAdorner adorner = new VBConnectorAdorner(this.ExtendedItem);
                    AdornerPanel.SetPlacement(adorner, new VBConnectorRelocationPlacement(obj as VBVisualConnecto));
                    adornerPanel.Children.Add(adorner);*/
                    foreach (VBEdge edge in (obj as VBConnector).ConnectedEdges)
                    {
                        if (removeEventHandler)
                            edge.VBEdgeRedraw -= OnVBEdgeRedrawed;
                        else
                            edge.VBEdgeRedraw += OnVBEdgeRedrawed;
                        edges.Add(edge);
                    }
                }
                return;
            }
            else
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null)
                        AttachToVBConnectorRelocationEvent(child, removeEventHandler, edges);
                }
            }
        }

        void OnVBEdgeRedrawed(object sender, VBEdgeRedrawEventArgs e)
        {
            if (operationOnEdges == null)
                return;
            Rect position;
            DesignItem itemEdge = null;
            if (this.ExtendedItem != null)
                itemEdge = this.ExtendedItem.Services.Component.GetDesignItem(sender);

            // TODO: DrawLineAdorner ist falsche Instanz bei Routed Edges
            if (!DrawLineAdorner.GetRectForPlacementOperation(e.NewSourcePosition, e.NewTargetPosition, out position, itemEdge))
                return;

            if (itemEdge != null)
            {
                var query = operationOnEdges.PlacedItems.Where(c => c.Item == itemEdge);
                if (query.Any())
                {
#if POLY 
#else
                    PlacementInformation info = query.First();
                    info.Bounds = position;
                    operationOnEdges.CurrentContainerBehavior.SetPosition(info);
#endif
                    //itemEdge.Properties[VBEdge.DataProperty].SetValue(VBEdge.GetGeometryAsString(e.NewGeometry));
                }
            }
            /*Geometry transformedGeometry = VBConnectPath.TransformGeometryToZeroCoord(this.Geometry);
            // TODO:
            if ((transformedGeometry == null) || !(transformedGeometry is LineGeometry))
                return null;*/

        }
    }

}
