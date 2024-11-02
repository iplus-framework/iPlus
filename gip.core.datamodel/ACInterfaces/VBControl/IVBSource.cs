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
// <copyright file="IVBSource.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IVBSource
    /// </summary>
    public interface IVBSource
    {
        /// <summary>
        /// Datenquelle
        /// </summary>
        /// <value>The VB source.</value>
        string VBSource { get; set; }

        /// <summary>
        /// Angezeigte Spalten (kommasepariert)
        /// </summary>
        /// <value>The VB show columns.</value>
        string VBShowColumns { get; set; }

        /// <summary>
        /// Angezeigte Spalten (kommasepariert)
        /// </summary>
        /// <value>The VB disabled columns.</value>
        string VBDisabledColumns { get; set; }

        /// <summary>
        /// Unterelemente
        /// </summary>
        /// <value>The VB childs.</value>
        string VBChilds { get; set; }

        /// <summary>
        /// Gets or sets the AC query definition.
        /// </summary>
        /// <value>The AC query definition.</value>
        ACQueryDefinition ACQueryDefinition { get; }
    }
}
