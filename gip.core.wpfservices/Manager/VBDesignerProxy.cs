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
using gip.ext.designer;

namespace gip.core.wpfservices
{
    public abstract class VBDesignerProxy : IVBComponentDesignManagerProxy
    {
        public VBDesignerProxy(IACComponent component)
        {
            _DesignerComp = component;
        }

        #region properties

        private IACComponent _DesignerComp;
        public T Designer<T>() where T : VBDesigner
        {
            if (_DesignerComp == null)
                return null;
            return _DesignerComp as T;
        }

        #endregion

        public abstract void UpdateVisual();

        public IACInteractiveObject GetVBDesignEditor(IACComponent component)
        {
            VBDesigner vbDesigner = component as VBDesigner;
            if (vbDesigner == null)
                return null;
            if (vbDesigner.VBDesign != null)
            {
                if ((vbDesigner.VBDesign.Content != null) && (vbDesigner.VBDesign.Content is VBDesignEditor))
                    return (vbDesigner.VBDesign.Content as VBDesignEditor);
            }
            if (vbDesigner.ParentACComponent != null && vbDesigner.ParentACComponent.ReferencePoint != null)
                return vbDesigner.ParentACComponent.ReferencePoint.ConnectionList.Where(c => c is VBDesignEditor).FirstOrDefault() as IACInteractiveObject;
            return null;
        }

        internal static bool AddItemWithDefaultSize(DesignItem container, DesignItem createdItem, Rect position)
        {
            //Rect position = new Rect();
            PlacementOperation operation = PlacementOperation.TryStartInsertNewComponents(
                container,
                new DesignItem[] { createdItem },
                new Rect[] { position },
                PlacementType.AddItem
            );
            if (operation != null)
            {
                container.Services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
                operation.Commit();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
