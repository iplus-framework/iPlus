// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IACInteractiveObjectParent.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>Interface that extends IACInteractiveObject to be able to define a tree-structure beetween IACInteractiveObject's </summary>
    /// <seealso cref="gip.core.datamodel.IACInteractiveObject" />
    public interface IACInteractiveObjectParent : IACInteractiveObject
    {
        /// <summary>
        /// The parent IACInteractiveObject
        /// </summary>
        /// <value>The parent IACInteractiveObject</value>
        IACInteractiveObject ParentACElement { get; }
    }
}
