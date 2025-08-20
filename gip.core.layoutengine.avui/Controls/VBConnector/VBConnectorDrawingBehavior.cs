// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
using gip.ext.designer.avui.Xaml;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> for <see cref="VBVisual"/>.
    /// </summary>
    [ExtensionFor(typeof(VBVisual))]
    [ExtensionFor(typeof(VBVisualGroup))]
    [ExtensionFor(typeof(VBConnector))]
    [ExtensionFor(typeof(VBCanvas))]
    [ExtensionFor(typeof(VBEdge))]
    [ExtensionServer(typeof(DrawingExtensionServer))]
    public class VBConnectorDrawingBehavior : AdornerProvider
    {
        public sealed class VBConnectorPlacement : AdornerPlacement
        {
            readonly VBConnector _ConnectorPoint;
            public VBConnectorPlacement(VBConnector connectorPoint) 
            { 
                this._ConnectorPoint = connectorPoint; 
            }

            public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
            {
                if (adorner is VBConnectorAdorner)
                {
                    VBConnectorAdorner VBConnectorAdorner = adorner as VBConnectorAdorner;
                    Point relativePoint = _ConnectorPoint.TranslatePoint(new Point(0, 0), VBConnectorAdorner._vbVisualConnectorItem.View);
                    adorner.Arrange(new Rect(relativePoint, new Size(_ConnectorPoint.ActualWidth,_ConnectorPoint.ActualHeight)));
                }
                else
                    adorner.Arrange(new Rect(adornedElementSize));
            }

            public VBConnector ConnectorPoint
            {
                get
                {
                    return _ConnectorPoint;
                }
            }
        }


        AdornerPanel adornerPanel = new AdornerPanel();

        public VBConnectorDrawingBehavior()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Adorners.Add(adornerPanel);
            if (this.ExtendedItem.View != null)
                if (this.ExtendedItem.View is VBEdge)
                    DecorateVBConnectors(((FrameworkElement)this.ExtendedItem.View).Parent);
                else
                    DecorateVBConnectors(this.ExtendedItem.View);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
        }

        private void DecorateVBConnectors(DependencyObject obj)
        {
            if (Context.Services.Tool.CurrentTool.GetType().Name != "ConnectTool")
                return;
            if (obj is VBConnector)
            {
                if (!String.IsNullOrEmpty((obj as VBConnector).VBContent))
                {
                    var acPropUsage = (obj as VBConnector).ACPropUsage;
                    IACComponentDesignManager designManager = DrawVBEdgeAdorner._DesignManager;
                    var vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(Context.RootItem.View, typeof(VBDesign)) as VBDesign;
                    if (vbDesign != null)
                        designManager = vbDesign.GetDesignManager();
                    if (designManager == null)
                    {
                        designManager = DrawVBEdgeAdorner._DesignManager;
                        if (designManager == null)
                            return;
                    }
                    switch (acPropUsage)
                    {
                        case Global.ACPropUsages.ConnectionPoint:
                            //if (DrawVBEdgeAdorner._SourceConnector == null)
                            //    return;
                            if (!designManager.IsEnabledCreateEdge(DrawVBEdgeAdorner._SourceConnector, obj as VBConnector))
                                return;
                            break;
                        case Global.ACPropUsages.EventPoint:
                            if (ClickOrDragMouseGesture._HasDragStarted)
                                return;
                            break;
                        case Global.ACPropUsages.EventPointSubscr:
                            if (!ClickOrDragMouseGesture._HasDragStarted)
                                return;
                            if (DrawVBEdgeAdorner._SourceConnector == null)
                                return;
                            if (!designManager.IsEnabledCreateEdge(DrawVBEdgeAdorner._SourceConnector, obj as VBConnector))
                                return;
                            break;
                        default:
                            break; // Wegen Connector-Tool in Visu
                    }

                    VBConnectorAdorner adorner = new VBConnectorAdorner(this.ExtendedItem);
                    AdornerPanel.SetPlacement(adorner, new VBConnectorPlacement(obj as VBConnector));
                    adornerPanel.Children.Add(adorner);
                }
                return;
            }
            else
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null)
                        DecorateVBConnectors(child);
                }
            }
        }
    }

}
