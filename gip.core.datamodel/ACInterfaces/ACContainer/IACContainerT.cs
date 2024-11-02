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
// <copyright file="IACContainerT.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>The Generic version of IACContainer. A Container is a object that encapsulates a value which is from type IACObject.</summary>
    /// <typeparam name="T">Generic type for ValueT-Property</typeparam>
    /// <seealso cref="gip.core.datamodel.IACContainer" />
    public interface IACContainerT<T> : IACContainer
    {
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        T ValueT { get; set; }
    }
}
