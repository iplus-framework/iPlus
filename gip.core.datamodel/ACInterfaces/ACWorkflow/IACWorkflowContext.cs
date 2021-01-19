// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="IACWorkflowContext.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// The workflow-context is a Database-Entity that is reponsible for the persistance of the workflow-state and it's configuration data (IACConfigStore).
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACWorkflowContext'}de{'IACWorkflowContext'}", Global.ACKinds.TACInterface)]
    public interface IACWorkflowContext : IACConfigStore, IACObjectDesign
    {
        /// <summary>
        /// Returns the Root-Node (ACClassWF) of this Workflow-Method
        /// </summary>
        IACWorkflowNode RootWFNode { get; }


        /// <summary>
        /// Returns the Type (ACClass) of the Root-Workflownode
        /// </summary>
        [ACPropertyInfo(9999)]
        ACClass WorkflowTypeACClass { get; }
    }
}
