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

            // Einf�gen von WFEdge (Vor WF, da diese f�r die Layoutberechnung ben�tigt werden
            foreach (var change in vbDesigner.VisualChangeList)
            {
                if (change.LayoutAction != VBDesigner.LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = vbDesigner.DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                CreateVBEdgeDesignItem(change, vbDesigner.DesignSurface.DesignContext);
            }

            if (changeGroup != null)
                changeGroup.Commit();
        }

        public void CreateVBEdgeDesignItem(VisualInfo visualInfo, DesignContext designContext)
        {
            DesignItem vbCanvas = designContext.RootItem.ContentProperty.Value;

            String fromXName = visualInfo.ACUrl;
            String toXName = visualInfo.ACUrl2;

            VBEdge newInstance = (VBEdge)vbCanvas.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBEdge), null);
            DesignItem item = vbCanvas.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                DrawShapesAdornerBase.ApplyDefaultPropertiesToItemS(item);
                item.Properties[VBEdge.ACName1Property].SetValue(fromXName);
                item.Properties[VBEdge.ACName2Property].SetValue(toXName);
            }
            //item.Properties[VBEdge.NameProperty].SetValue(acVisualEdge.ACIdentifier);
            //            item.Properties[VBEdge.VBContentProperty].SetValue(RootACUrl + "\\" + acVisualEdge.GetACUrl(acVisualEdge.ParentACObject));

            AddItemWithDefaultSize(vbCanvas, item, new Rect());
        }
    }
}
