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
// <copyright file="IACObjectDesign.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for objects that provide XAML-Code for presentation.
    /// (ACClassDesign, ACClassMethod, Workflows) 
    /// </summary>
    public interface IACObjectDesign : IACObject
    {
        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        String XAMLDesign { get; set; }
    }
}






