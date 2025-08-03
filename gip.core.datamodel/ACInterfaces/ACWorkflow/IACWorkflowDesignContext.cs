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
// <copyright file="IACObjectDesignWF.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for objects that stores nodes and edges for workflows and its XAML-Design
    /// (ACClassMethod, MaterialWF) 
    /// </summary>
    public interface IACWorkflowDesignContext : IACWorkflowContext
    {
        /// <summary>Returns all Nodes in this Workflow</summary>
        IEnumerable<IACWorkflowNode> AllWFNodes { get; }


        /// <summary>Returns als Edges in this workflow</summary>
        IEnumerable<IACWorkflowEdge> AllWFEdges { get; }


        /// <summary>
        /// Adds a new node to this workflow
        /// </summary>
        void AddNode(IACWorkflowNode node);


        /// <summary>
        /// Removes a Workflow-Node from this Workflow
        /// </summary>
        /// <param name="database">Database-Context</param>
        /// <param name="node">Workflow-Node</param>
        /// <param name="configACUrl">Optional: Config-Url</param>
        void DeleteNode(IACEntityObjectContext database, IACWorkflowNode node, string configACUrl);


        /// <summary>
        /// Adds a new edge to this workflow
        /// </summary>
        /// <param name="edge"></param>
        void AddEdge(IACWorkflowEdge edge);


        /// <summary>
        /// Removes a Edge from this Workflow
        /// </summary>
        /// <param name="database">Database-Context</param>
        /// <param name="edge">Workflow-Edge</param>
        void DeleteEdge(IACEntityObjectContext database, IACWorkflowEdge edge, IACWorkflowNode node, bool nodeIsTargetElseSource);


        /// <summary>
        /// Creates a new edge
        /// </summary>
        /// <param name="database">Database-Context</param>
        /// <returns></returns>
        IACWorkflowEdge CreateNewEdge(IACEntityObjectContext database);

    }
}
