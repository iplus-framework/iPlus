// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACWorkflowObject.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Base-Interface for IACWorkflowEdge and IACWorkflowNode.
    /// Entity-Classes that persist workflows has to implement both interfaces.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACWorkflowObject'}de{'IACWorkflowObject'}", Global.ACKinds.TACInterface)]
    public interface IACWorkflowObject : IACObject
    {
        /// <summary>
        /// Unique ID of the Workflow Node or Edge
        /// </summary>
        /// <value>Guid of the WF-Node</value>
        Guid WFObjectID { get; }

        /// <summary>
        /// Reference to the parent Workflow-Node that groups more child-nodes together
        /// </summary>
        /// <value>Parent Workflow-Node (Group)</value>
        IACWorkflowNode WFGroup { get; set; }

        /// <summary>
        /// WPF's x:Name to indentify this instance in the Logical-Tree
        /// </summary>
        /// <value>x:Name (WPF)</value>
        string XName { get; }
    }
}
