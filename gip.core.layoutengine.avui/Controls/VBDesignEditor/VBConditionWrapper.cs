using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.OutlineView;

namespace gip.core.layoutengine.avui
{

    /// <summary>
    /// Represents a wrapper for conditions.
    /// </summary>
    public class VBConditionWrapper : ConditionWrapper
    {
        public VBConditionWrapper(DesignItem designObjectCondition, MultiTriggerNodeBase parentMultiWrapper)
            : base(designObjectCondition, parentMultiWrapper)
        {
        }

        protected override PropertyNode CreatePropertyNode()
        {
            return new VBPropertyNode();
        }
    }
}
