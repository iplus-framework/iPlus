// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;
using gip.ext.design;
using gip.core.layoutengine;
using gip.ext.designer.Controls;
using gip.core.manager;
using static gip.core.manager.VBDesigner;
using System.Windows.Media;
using System.ComponentModel.Design;
using gip.ext.designer.Services;

namespace gip.core.wpfservices
{
    public class VBDesignerWorkflowMethodProxy : VBDesignerWorkflowProxy, IVBComponentDesignManagerWorkflowMethod
    {
        public VBDesignerWorkflowMethodProxy(IACComponent component) : base(component)
        {
        }

        public override DesignItem CreateVBVisualDesignItem(VisualInfo visualInfo, IACWorkflowNode acVisualWF, DesignContext designContext, out DesignItem designItemParent)
        {
            DesignItem item = base.CreateVBVisualDesignItem(visualInfo, acVisualWF, designContext, out designItemParent);

            VBDesignerWorkflowMethod vbDesigner = Designer<VBDesignerWorkflowMethod>();
            if (vbDesigner == null)
                return null;

            if (vbDesigner.UseAutoLayoutElements)
                vbDesigner.WFLayoutCalculatorProxy.WFLayoutGroup((short)visualInfo.LayoutAction, designContext, designItemParent, item);
            else
            {
                //TODO: place on the edge, between elements
            }

            return item;
        }

        public override void UpdateVisual()
        {
            VBDesignerWorkflowMethod vbDesigner = Designer<VBDesignerWorkflowMethod>();
            if (vbDesigner == null)
                return;

            base.UpdateVisual();
            if (vbDesigner.VBDesignEditor is VBDesignEditor)
            {
                ((VBDesignEditor)vbDesigner.VBDesignEditor).SaveToXAML();
                ((VBDesignEditor)vbDesigner.VBDesignEditor).RefreshViewFromXAML();
            }
        }

        public override IEnumerable<IACObject> GetAvailableTools()
        {
            VBDesignerWorkflowMethod vbDesigner = Designer<VBDesignerWorkflowMethod>();
            if (vbDesigner == null)
                return null;

            ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Pointer"), PointerTool.Instance, "DesignPointer"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Connector"), new ConnectTool(vbDesigner), "DesignConnector"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "EditPoints"), new DrawingToolEditPoints(), "DesignEditPoints"));
            return objectLayoutEntrys;
        }

        public void AddToVisualChangeListRectParam(IACObject visualObject, short layoutAction, double width, double height, string acUrl = "", string acUrl2 = "")
        {
            LayoutActionType layoutActionType = TransformShortToLayoutAction(layoutAction);
            AddToVisualChangeListRect(visualObject, layoutActionType, "", "", new Rect(0, 0, width, height));
        }

        protected override void CreateWFEdge(VBEdge newVBEdge, VBConnector targetConnector)
        {
            // TODO: - Moved from VBDesignerWorkflowMethod and method was not implemented
        }

        public bool DoInsertRoot(IACWorkflowDesignContext vbWorkflow, ACClass methodACClass)
        {
            ClearVisualChangeList();

            VBDesignerWorkflowMethod vbDesigner = Designer<VBDesignerWorkflowMethod>();
            if (vbDesigner == null)
                return false;

            ACClassMethod rootACClassMethod = vbWorkflow as ACClassMethod;

            if (rootACClassMethod.RootWFNode != null)
                return true;

            ACClassMethod acClassMethod = vbWorkflow as ACClassMethod;
            ACClassWF rootMethodWF = vbDesigner.CreateGroupClass(rootACClassMethod, methodACClass, rootACClassMethod.ACClass, null, null);

            string vbContent;
            string controlName;
            double top = 0;
            double left = 0;

            string xmlDesign = "<vb:VBCanvas Enabled=\"true\" Width=\"1024\" Height=\"768\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\">\n";

            foreach (var visualInfo in VisualChangeList.Where(c => c.LayoutAction == LayoutActionType.Insert))
            {
                IACWorkflowNode acVisualWF = visualInfo.VisualObject as IACWorkflowNode;
                switch (acVisualWF.PWACClass.ACKind)
                {
                    case Global.ACKinds.TPWMethod:
                        vbContent = Const.VBPresenter_SelectedRootWFNode;
                        controlName = Const.VBVisualGroup_ClassName;
                        break;
                    case Global.ACKinds.TPWNodeStart:
                        vbContent = acVisualWF.ACIdentifier;
                        controlName = Const.VBVisual_ClassName;
                        break;
                    case Global.ACKinds.TPWNodeEnd:
                    default:
                        vbContent = acVisualWF.ACIdentifier;
                        controlName = Const.VBVisual_ClassName;
                        break;
                }
                xmlDesign += string.Format("<vb:{6} VBContent=\"{0}\" Height=\"{1}\" Width=\"{2}\" Canvas.Top=\"{3}\" Canvas.Left=\"{4}\" Name=\"{5}\">\n",
                    vbContent,       // VBContent
                    visualInfo.Position.Height,    // Height
                    visualInfo.Position.Width,     // Width
                    top,       // Top
                    left,      // Left
                    acVisualWF.XName,
                    controlName);

                switch (acVisualWF.PWACClass.ACKind)
                {
                    case Global.ACKinds.TPWMethod:
                        xmlDesign += "<vb:VBCanvasInGroup>\n";
                        top = WFLayoutCalculator.TopSpace;
                        left = WFLayoutCalculator.LeftSpace;
                        break;
                    case Global.ACKinds.TPWNodeStart:
                        top += visualInfo.Position.Height + WFLayoutCalculator.VertSpace;
                        xmlDesign += "</vb:" + Const.VBVisual_ClassName + ">\n";
                        break;
                    default:
                        xmlDesign += "</vb:" + Const.VBVisual_ClassName + ">\n";
                        break;
                }
            }

            xmlDesign += "</vb:VBCanvasInGroup>\n";
            xmlDesign += "</vb:" + Const.VBVisualGroup_ClassName + ">\n";

            foreach (var acClassWFEdge in acClassMethod.ACClassWFEdge_ACClassMethod)
            {
                xmlDesign += string.Format("<vb:VBEdge VBContent=\"{0}\" VBConnectorSource=\"{1}\" VBConnectorTarget=\"{2}\" x:Name=\"{3}\" {4}></vb:VBEdge>\n",
                    Const.VBPresenter_SelectedWFContext + "\\" + acClassWFEdge.GetACUrl(acClassMethod),                            // VBContent
                    acClassWFEdge.SourceACConnector,                      // VBConnectorSource
                    acClassWFEdge.TargetACConnector,                      // VBConnectorTarget
                    acClassWFEdge.XName,
                     "Stroke=\"#FFFFFFFF\" StrokeThickness=\"1.5\" StrokeDashCap=\"Flat\" StrokeDashOffset=\"0\" StrokeEndLineCap=\"Flat\" StrokeLineJoin=\"Miter\" StrokeMiterLimit=\"10\" StrokeStartLineCap=\"Flat\"");
            }

            xmlDesign += "</vb:VBCanvas>\n";

            acClassMethod.XMLDesign = xmlDesign;
            return true;
        }
    }
}
