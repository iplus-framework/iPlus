// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.dbsyncer.model
{
    public class DBSyncerVersion
    {
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Time when update is realized
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}
