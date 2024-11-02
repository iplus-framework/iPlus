// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that supports soft-deleting.
    /// </summary>
    public interface IDeleteInfo
    {
        /// <summary>
        /// Database-Field, that contains the username if the record was deleted.
        /// </summary>
        /// <value>
        /// Is not null if record was deleted.
        /// </value>
        string DeleteName { get; set; }

        /// <summary>
        /// Database-Field, that contains the Deletion-Date if the record was deleted.
        /// </summary>record was deleted.
        /// <value>
        /// Is not null if 
        /// </value>
        DateTime? DeleteDate { get; set; }
    }
}
