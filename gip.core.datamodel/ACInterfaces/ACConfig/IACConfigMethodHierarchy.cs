using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Workflows can invoke other sub-workflows.
    /// This interface helps to return the information about the current the Workflow-"Stack".
    /// Every Workfow-Node instance (on server-side) or a proxy (on cient side) or a offline node (during configuration) mus implement this interface.
    /// </summary>
    public interface IACConfigMethodHierarchy
    {
        /// <summary>
        /// Since IACComponentPWNode inherits from the IACConfigURL interface, the workflow instance itself knows from which "parent workflow" the "subworkflow", to which the workflow instance belongs, was called. 
        /// The "call stack" - that is the call sequence of workflows - provides the property ACConfigMethodHierarchy from this interface.
        /// </summary>
        /// <value>
        /// The ac configuration method hierarchy.
        /// </value>
        List<ACClassMethod> ACConfigMethodHierarchy { get; }
    }
}
