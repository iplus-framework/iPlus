using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.OutlineView;
using gip.ext.designer.avui;
using gip.ext.design.avui;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the editor for triggers collection.
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert den Editor für die Trigger-Sammlung.
    /// </summary>
    public class VBTriggersCollectionEditor : TriggersCollectionEditor
    {
        protected override TriggerOutlineNodeBase CreateOutlineNode(DesignItem child, DesignItem designObject)
        {
            if (child.ComponentType.Name.Contains("MultiDataTrigger"))
                return new VBMultiDataTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("MultiTrigger"))
                return new VBMultiTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("DataTrigger"))
                return new VBDataTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("EventTrigger"))
                return new VBEventTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("Trigger"))
                return new VBPropertyTriggerOutlineNode(child, designObject);
            return null;
        }

    }
}
