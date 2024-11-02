// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that stores information about the updateting-event.
    /// </summary>
    public interface IUpdateInfo
    {
        /// <summary>
        /// Name of the user who manipulated this record the last time.
        /// </summary>
        /// <value>
        /// Not null
        /// </value>
        string UpdateName { get; set; }

        /// <summary>
        /// Date when this record was manipulated the last time.
        /// </summary>
        /// <value>
        /// DateTime
        /// </value>
        DateTime UpdateDate { get; set; }
    }
}
