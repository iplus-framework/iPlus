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
// <copyright file="IClassManager.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface IClassManager
    /// </summary>
    public interface IClassManager
    {
        /// <summary>
        /// Defaults the values AC object.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        void DefaultValuesACObject(IACObject acObject);

        /// <summary>
        /// Checks the AC object.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        void CheckACObject(IACObject acObject);
    }
}
