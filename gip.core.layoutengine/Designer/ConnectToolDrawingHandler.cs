// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using gip.ext.design;
using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.designer;
using gip.ext.designer.Controls;
using gip.ext.designer.Services;
using gip.ext.designer.Extensions;
using gip.core.layoutengine;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;


namespace gip.core.layoutengine
{
    /// <summary>
    /// Handles selection multiple controls inside a Panel.
    /// </summary>
    [ExtensionFor(typeof(VBVisual))]
    [ExtensionFor(typeof(VBVisualGroup))]
    [ExtensionFor(typeof(VBConnector))]
    [ExtensionFor(typeof(VBEdge))]
    public class ConnectToolDrawingHandler : BehaviorExtension, IHandleDrawToolMouseDown
    {
        public ConnectToolDrawingHandler()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ExtendedItem.AddBehavior(typeof(ConnectToolDrawingHandler), this);
        }

        public void HandleStartDrawingOnMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result, IDrawingTool tool)
        {
            DependencyObject vbConnectorAdorner = VBVisualTreeHelper.FindParentObjectInVisualTree(result.VisualHit, typeof(VBConnectorAdorner));
            if (vbConnectorAdorner == null)
                return;
            VBConnectorDrawingBehavior.VBConnectorPlacement placement = AdornerPanel.GetPlacement(vbConnectorAdorner as UIElement) as VBConnectorDrawingBehavior.VBConnectorPlacement;
            if (placement == null)
                return;

            if ((result.AdornerHit != null) && (result.AdornerHit.AdornedDesignItem != null)
                && (placement.ConnectorPoint != null) && e.ChangedButton == MouseButton.Left && MouseGestureBase.IsOnlyButtonPressed(e, MouseButton.Left))
            {
                if (String.IsNullOrEmpty(placement.ConnectorPoint.Name))
                    return;
                tool.RaiseToolEvent(designPanel, new ToolEventArgs("VBConnectorClicked", new object[] { result, placement }));
                e.Handled = true;

                var ancestors = (result.AdornerHit.AdornedDesignItem.View as DependencyObject).GetVisualAncestors();
                var queryVBCanvases = ancestors.OfType<VBCanvas>();
                if (!queryVBCanvases.Any())
                    return;
                foreach (VBCanvas vbCanvas in queryVBCanvases)
                {
                    DesignItem containerForShape = designPanel.Context.Services.Component.GetDesignItem(vbCanvas);
                    if (containerForShape != null)
                    {
                        switch (placement.ConnectorPoint.ACPropUsage)
                        {
                            case Global.ACPropUsages.EventPoint:
                                break;
                            case Global.ACPropUsages.ConnectionPoint:
                                break;
                            case Global.ACPropUsages.EventPointSubscr:  // Kann nicht als Startpunkt verwendet werden
                            default:
                                return;
                        }
                        new DrawConnectionGesture(result.AdornerHit.AdornedDesignItem, containerForShape, placement.ConnectorPoint, (tool as ConnectTool).DesignManager).Start(designPanel, e);
                        break;
                    }
                }

                //DesignItem containerForShape;
                //if (result.ModelHit.View is VBCanvas)
                //    containerForShape = result.ModelHit;
                //else if ((result.ModelHit.Parent != null) && (result.ModelHit.Parent.View is VBCanvas))
                //    containerForShape = result.ModelHit.Parent;
                //else
                //    containerForShape = result.ModelHit.Context.RootItem;

                //new DrawConnectionGesture(result.ModelHit, containerForShape, vbConnector as VBConnector).Start(designPanel, e);
            }
        }
    }

    sealed public class DrawConnectionGesture : MouseMoveAndDrawGestureBase
    {
        VBConnector _sourceConnector;

        public DrawConnectionGesture(DesignItem containerOfStartPoint, DesignItem containerForShape, VBConnector vbConnector, IACComponentDesignManager designManager)
            : base(containerOfStartPoint, containerForShape, false)
        {
            this.positionRelativeTo = containerOfStartPoint.View;
            this._sourceConnector = vbConnector;
            DesignManager = designManager;
        }

        public override DrawShapesAdornerBase GenerateShapeDrawer(MouseEventArgs e)
        {
            Point startPointRelativeToShapeContainer = e.GetPosition(ContainerForShape.View);
            return new DrawVBEdgeAdorner(startPointRelativeToShapeContainer, ContainerOfStartPoint, ContainerForShape, _sourceConnector, DesignManager);
        }

        public IACComponentDesignManager DesignManager
        {
            get;
            set;
        }

    }
}
