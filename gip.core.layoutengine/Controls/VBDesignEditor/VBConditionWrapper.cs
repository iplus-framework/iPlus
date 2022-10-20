using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using gip.ext.designer.OutlineView;
using gip.ext.design;
using gip.ext.design.PropertyGrid;
using gip.ext.designer.PropertyGrid;
using System.Linq;
using System.Windows.Markup;

namespace gip.core.layoutengine
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
