// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="IACComponentPWNode.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Base interface for all workflow-classes (real WF-instances, PWProxy or PWOfflineNode)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACComponentPWNode'}de{'IACComponentPWNode'}", Global.ACKinds.TACInterface)]
    public interface IACComponentPWNode : IACComponent, IACObjectDesign, IACConfigURL, IACConfigStoreSelection, IACConfigMethodHierarchy
    {
        /// <summary>
        /// Root-Workflownode
        /// </summary>
        /// <value>Root-Workflownode</value>
        IACComponentPWNode ParentRootWFNode { get; }


        /// <summary>
        /// Returns the Workflow-Context (ACClassMethod, ACProgram, Partslist or MaterialWF) for reading and saving the configuration-data of a workflow.
        /// </summary>
        /// <value>The Workflow-Context</value>
        IACWorkflowContext WFContext { get; }


        /// <summary>Reference to the definition of this Workflownode.</summary>
        /// <value>Reference to the definition of this Workflownode.</value>
        ACClassWF ContentACClassWF { get;  }

        bool IsOnlineNode { get; }
    }
}
