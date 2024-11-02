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
// <copyright file="IACComponentPWGroup.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for workflow-nodes that contains child-nodes. PWProcessFunction and PWGroup implements this interface.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACComponentPWGroup'}de{'IACComponentPWGroup'}", Global.ACKinds.TACInterface)]
    public interface IACComponentPWGroup : IACComponentPWNode
    {
        /// <summary>
        /// Completes this node by setting the CurrentACState-Property to ACStateEnum.SMCompleted.
        /// </summary>
        void GroupComplete();
    }
}
