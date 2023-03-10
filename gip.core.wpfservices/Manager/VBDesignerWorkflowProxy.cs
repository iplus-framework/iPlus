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
    public class VBDesignerWorkflowProxy : VBDesignerProxy
    {
        public VBDesignerWorkflowProxy(IACComponent component) : base(component)
        {
        }

        public override void UpdateVisual()
        {
            VBDesignerWorkflow vbDesigner = Designer<VBDesignerWorkflow>();
            if (vbDesigner == null)
                return;
        }

        public DesignItem CreateVBEdgeDesignItem(object visualInfo, DesignContext designContext)
        {
            DesignItem vbCanvas = designContext.RootItem.ContentProperty.Value;

            VBDesigner.VisualInfo vsInfo = visualInfo as VBDesigner.VisualInfo;
            String fromXName = vsInfo.ACUrl;
            String toXName = vsInfo.ACUrl2;

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

            return item;
        }

    }
}
