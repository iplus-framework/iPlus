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
    }
}
