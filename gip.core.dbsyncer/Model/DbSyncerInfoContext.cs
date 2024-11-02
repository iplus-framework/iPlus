// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.dbsyncer.model
{
    /// <summary>
    /// Mapped table DbSyncerInfoContext
    /// </summary>
    public class DbSyncerInfoContext
    {

        /// <summary>
        /// ID and primary key
        /// short unique name for context to
        /// </summary>
        public string DbSyncerInfoContextID { get; set; }

        /// <summary>
        /// Context (long) name
        /// </summary>
        public string Name {get;set;}

        /// <summary>
        /// Name of EF connection in .config file used for context
        /// </summary>
        public string ConnectionName {get;set;}

        /// <summary>
        /// Priority order of context
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DbSyncerInfoContextID;
        }
    }
}
