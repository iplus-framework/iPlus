using gip.core.dbsyncer.Command;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

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
        public DBContextHelper(DbContext db, DbSyncerInfoContext dbInfoContext, string rootFolder)
        {
            FileAvailableVersions = DbSyncerInfoCommand.FileAvailableVersions(dbInfoContext, rootFolder);
            DatabaseMaxScriptDate = DbSyncerInfoCommand.DatabaseMaxScriptDate(db, dbInfoContext);
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
