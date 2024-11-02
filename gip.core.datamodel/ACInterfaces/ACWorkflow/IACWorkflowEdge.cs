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
// <copyright file="IACWorkflowEdge.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface for Entity-Classes that represents a Workflow-Edge.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACWorkflowEdge'}de{'IACWorkflowEdge'}", Global.ACKinds.TACInterface)]
    public interface IACWorkflowEdge : IACWorkflowObject
    {
        /// <summary>
        /// Reference to the From-Node (Source)
        /// </summary>
        /// <value>Reference to the From-Node (Source)</value>
        IACWorkflowNode FromWFNode { get; set; }


        /// <summary>
        /// Reference to the To-Node (Target)
        /// </summary>
        /// <value>Reference to the To-Node (Target)</value>
        IACWorkflowNode ToWFNode { get; set; }


        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>The type of the connection.</value>
        Global.ConnectionTypes ConnectionType { get; set; }


        /// <summary>
        /// ACIdentifier of the FromWFNode
        /// </summary>
        /// <value>ACIdentifier of the FromWFNode</value>
        string SourceACName { get; }


        /// <summary>
        /// Connection-Point of the FromWFNode
        /// </summary>
        /// <value>Connection-Point of the FromWFNode</value>
        ACClassProperty SourceACClassProperty { get; set; }


        /// <summary>
        /// WPF-x:Name of the FromWFNode + \\ + SourceACClassProperty.ACIdentifier
        /// </summary>
        /// <value>WPF-x:Name of the FromWFNode + \\ + SourceACClassProperty.ACIdentifier</value>
        string SourceACConnector { get; }


        /// <summary>
        /// ACIdentifier of the ToWFNode
        /// </summary>
        /// <value>ACIdentifier of the ToWFNode</value>
        string TargetACName { get; }


        /// <summary>
        /// Connection-Point of the ToWFNode
        /// </summary>
        /// <value>Connection-Point of the ToWFNode</value>
        ACClassProperty TargetACClassProperty { get; set; }


        /// <summary>
        /// WPF-x:Name of the ToWFNode + \\ + TargetACClassProperty.ACIdentifier
        /// </summary>
        /// <value>WPF-x:Name of the ToWFNode + \\ + TargetACClassProperty.ACIdentifier</value>
        string TargetACConnector { get; }
    }
}
