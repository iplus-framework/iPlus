using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// This interface is for live ACComponents, that needs Configurations via IACConfig
    /// In most cases this are Worfkflow-Components, that are created dynamicaly to be able to read their configuration data
    /// </summary>
    public interface IACConfigURL
    {
        /// <summary>
        /// ACUrl for Accessing IACConfig-Entries from a IACConfigProvider
        /// </summary>
        /// <value>The config AC URL.</value>
        string ConfigACUrl { get; }


        /// <summary>
        /// Calling subworkflows is similar to calling a subprogram. 
        /// When calling subprograms, parameters must be passed for which only one call is valid for this one. 
        /// If the same subprogram is called from another program, other parameters must be passed. 
        /// This is also the case with workflows. 
        /// A IACConfigProvider neds this Url for providing the right Configuraton data.
        /// Workflow-Nodes returns here the PreValueACUrl of it's root-Node.
        /// </summary>
        /// <value>ACUrl of the parent Workflow, that invoked this Workflow</value>
        string PreValueACUrl { get; }


        /// <summary>
        /// Refreshes the Routing-Rules in a node.
        /// </summary>
        void RefreshRuleStates();
    }
}
