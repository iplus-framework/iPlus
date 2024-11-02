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
// <copyright file="IACContainerRef.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>Type-Neutral interface for ACRef</summary>
    /// <seealso cref="gip.core.datamodel.IACContainer" />
    /// <seealso cref="gip.core.datamodel.IACObject" />
    /// <seealso cref="gip.core.datamodel.IACAttach" />
    public interface IACContainerRef : IACContainer, IACObject, IACAttach
    {
        /// <summary>
        /// A weak reference is a reference that does not protect the referenced object from collection by the IPlus-GarbageCollector
        /// </summary>
        bool IsWeakReference { get; }
    }
}
