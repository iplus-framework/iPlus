using gip.ext.design.avui;
using gip.ext.designer.avui.OutlineView;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a editor for multi triggers.
    /// </summary>
    public class VBMultiTriggerEditor : MultiTriggerEditor
    {
        protected override ConditionWrapper OnCreateConditionWrapper(DesignItem designObjectCondition)
        {
            return new VBConditionWrapper(designObjectCondition, _NodeMultiTrigger);
        }

        protected override ConditionEditor CreateConditionEditor()
        {
            return new VBConditionEditor();
        }

    }
}
