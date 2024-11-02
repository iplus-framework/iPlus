// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that stores information about the inserting-event.
    /// </summary>
    public interface IInsertInfo
    {
        /// <summary>
        /// Name of the user who added this record to the database.
        /// </summary>
        /// <value>
        /// Not null
        /// </value>
        string InsertName { get; set; }


        /// <summary>
        /// Date when this record was added to the database.
        /// </summary>
        /// <value>
        /// DateTime
        /// </value>
        DateTime InsertDate { get; set; }
    }
}
