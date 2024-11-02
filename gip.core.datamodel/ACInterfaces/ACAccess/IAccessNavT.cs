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
// <copyright file="IAccess.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>The generic form of IAccessNav</summary>
    /// <typeparam name="T">Type of the objects in the NavList</typeparam>
    /// <seealso cref="gip.core.datamodel.IAccessT{T}" />
    /// <seealso cref="gip.core.datamodel.IAccessNav" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IAccessNavT'}de{'IAccessNavT'}", Global.ACKinds.TACInterface)]
    public interface IAccessNavT<T> : IAccessT<T>, IAccessNav where T : class
    {
        /// <summary>
        /// The "Selected"-Property points to a EF-Object in the NavOList-Collection that is highlighted in a Items-Control on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The selected object of type T</value>
        [ACPropertyInfo(9999)]
        T Selected { get; set; }


        /// <summary>
        /// The "Current"-Property points to a EF-Object in the NavList-Collection that is displayed on a Form-View on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The current object of type T</value>
        [ACPropertyInfo(9999)]
        T Current { get; set; }
    }

}
