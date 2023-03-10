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
    public class VBDesignerXAMLProxy : VBDesignerProxy
    {
        public VBDesignerXAMLProxy(IACComponent component) : base(component)
        {
        }

        public override void UpdateVisual()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            List<DesignItem> changedItems = new List<DesignItem>();

            ChangeGroup changeGroup = null;

            // Einfügen von WFEdge (Vor WF, da diese für die Layoutberechnung benötigt werden
            foreach (var change in vbDesigner.VisualChangeList)
            {
                if (change.LayoutAction != VBDesigner.LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = vbDesigner.DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                // TODO Mario
                //DesignItem designItem = CreateVBEdgeDesignItem(change, vbDesigner.DesignSurface.DesignContext);
            }

            if (changeGroup != null)
                changeGroup.Commit();
        }
    }
}
