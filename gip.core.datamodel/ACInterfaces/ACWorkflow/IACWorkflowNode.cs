// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACWorkflowNode.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface for Entity-Classes that represents a Workflow-Node.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACWorkflowNode'}de{'IACWorkflowNode'}", Global.ACKinds.TACInterface)]
    public interface IACWorkflowNode : IACWorkflowObject
    {
        /// <summary>All edges that starts from this node</summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        IEnumerable<IACWorkflowEdge> GetOutgoingWFEdges(IACWorkflowContext context);


        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all outgoing-edges belonging to this node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        IEnumerable<IACWorkflowEdge> GetOutgoingWFEdgesInGroup(IACWorkflowContext context);


        /// <summary>
        /// All edges that ends in this node
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        IEnumerable<IACWorkflowEdge> GetIncomingWFEdges(IACWorkflowContext context);


        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all incoming-edges belonging to this node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        IEnumerable<IACWorkflowEdge> GetIncomingWFEdgesInGroup(IACWorkflowContext context);


        /// <summary>
        /// Returns true if this Node is a Workflow-Group and is the most outer node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value><c>true</c> if this Node is a Workflow-Group and is the most outer node; otherwise, <c>false</c>.</value>
        bool IsRootWFNode(IACWorkflowContext context);


        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all nnodes that are inside of this group.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of nodes</value>
        IEnumerable<IACWorkflowNode> GetChildWFNodes(IACWorkflowContext context);


        /// <summary>
        /// Returns the ACClassProperty that reprensents a Connection-Point where Edges can be connected to.
        /// </summary>
        /// <param name="acPropertyName">Name of the property.</param>
        /// <returns>ACClassProperty.</returns>
        ACClassProperty GetConnector(string acPropertyName);


        /// <summary>
        /// Returns a ACUrl, to be able to find this instance in the WPF-Logical-Tree.
        /// </summary>
        /// <value>ACUrl as string</value>
        string VisualACUrl { get; }

        /// <summary>
        /// The Runtime-type of the Workflow-Class that will be instantiated when the Workflow is loaded.
        /// </summary>
        /// <value>Reference to a ACClass</value>
        ACClass PWACClass { get; }

    }
}
