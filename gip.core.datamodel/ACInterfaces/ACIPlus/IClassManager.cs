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
