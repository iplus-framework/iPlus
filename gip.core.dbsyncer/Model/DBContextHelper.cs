// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.dbsyncer.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace gip.core.dbsyncer.model
{
    /// <summary>
    /// Providing informations about context status and needed updates
    /// </summary>
    public class DBContextHelper
    {
        #region ctor's

        /// <summary>
        /// Doing job to define should be and what will be updated in context
        /// </summary>
        /// <param name="dbInfoContext"></param>
        public DBContextHelper(gip.core.datamodel.Database db, gip.core.datamodel.DbSyncerInfoContext dbInfoContext, string rootFolder)
        {
            FileAvailableVersions = DbSyncerInfoCommand.FileAvailableVersions(dbInfoContext, rootFolder);
            try
            {
                DatabaseMaxScriptDate = DbSyncerInfoCommand.DatabaseMaxScriptDate(db, dbInfoContext);
            }
            catch (InvalidOperationException ex)
            {
                // If new database registered and has no entries, than sql-server returns with a null message:
                if (ex.HResult != -2146233079)
                {
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
            if (FileAvailableVersions != null && FileAvailableVersions.Any())
            {
                FileMaxScriptDate = FileAvailableVersions.Max(c => c.ScriptDate);
                if (DatabaseMaxScriptDate == null || DatabaseMaxScriptDate < FileMaxScriptDate)
                {
                    IsUpdateNeeded = true;
                    UpdateNeededFiles =
                        FileAvailableVersions
                        .Where(x => x.ScriptDate > DatabaseMaxScriptDate || DatabaseMaxScriptDate == null)
                        .OrderBy(c => c.ScriptDate)
                        .ToList();
                }

            }
        }

        #endregion

        #region properties
        /// <summary>
        /// All available files for context in file system
        /// </summary>
        public List<ScriptFileInfo> FileAvailableVersions { get; set; }

        /// <summary>
        /// List of files in file system not executed during db udpate
        /// </summary>
        public List<ScriptFileInfo> UpdateNeededFiles { get; set; }

        /// <summary>
        /// oldest time of file system update files
        /// </summary>
        public DateTime? FileMaxScriptDate { get; set; }

        /// <summary>
        /// oldest time of update stored in database for contexts
        /// </summary>
        public DateTime? DatabaseMaxScriptDate { get; set; }

        /// <summary>
        /// Flag is update needed for context
        /// </summary>
        public bool IsUpdateNeeded { get; set; }
        #endregion

        #region overrides

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(@"[IsUpdateNeeded={0}][cnt={1}]", IsUpdateNeeded, UpdateNeededFiles != null ? UpdateNeededFiles.Count : 0);
        }

        #endregion
    }
}
