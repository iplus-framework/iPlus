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
    public class VBDesignerWorkflowMethodProxy : VBDesignerWorkflowProxy
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
                vbDesigner.WFLayoutCalculator.WFLayoutGroup(visualInfo.LayoutAction, designContext, designItemParent, item);
            else
            {
                //TODO: place on the edge, between elements
            }

            return item;
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

        protected override void CreateWFEdge(VBEdge newVBEdge, VBConnector targetConnector)
        {
            // TODO: - Moved from VBDesignerWorkflowMethod and method was not implemented
        }
    }
}
