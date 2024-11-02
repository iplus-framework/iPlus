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
// <copyright file="IVBDataCheckbox.cs" company="gip mbh, Oftersheim, Germany">
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
    // TODO: Kann man die Checkbox im TreeView flexibler machen ?
    /// <summary>
    /// Interface IVBDataCheckbox
    /// </summary>
    public interface IVBDataCheckbox
    {
        /// <summary>
        /// Gets the data content check box.
        /// </summary>
        /// <value>The data content check box.</value>
        string DataContentCheckBox
        {
            get;
            //set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        bool IsChecked
        {
            get;
            set;
        }

        bool IsEnabled
        {
            get;
            set;
        }

    }
}
