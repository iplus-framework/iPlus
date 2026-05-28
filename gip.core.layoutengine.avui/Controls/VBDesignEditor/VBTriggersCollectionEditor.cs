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
    public class VBTriggersCollectionEditor : TriggersCollectionEditor, IDesignEditorWindow
    {
        protected override Type StyleKeyOverride => typeof(VBTriggersCollectionEditor);

        protected override TriggerOutlineNodeBase CreateOutlineNode(DesignItem child, DesignItem designObject)
        {
            switch (DetermineTriggerKind(child))
            {
                case TriggerKind.Property:
                    return new VBPropertyTriggerOutlineNode(child, designObject);
                case TriggerKind.Data:
                    return new VBDataTriggerOutlineNode(child, designObject);
                case TriggerKind.Event:
                    return new VBEventTriggerOutlineNode(child, designObject);
                case TriggerKind.Multi:
                    return new VBMultiTriggerOutlineNode(child, designObject);
                case TriggerKind.MultiData:
                    return new VBMultiDataTriggerOutlineNode(child, designObject);
                default:
                    return null;
            }
        }

    }
}
