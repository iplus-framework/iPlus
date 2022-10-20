using gip.core.ControlScriptSync.file;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync.VBSettings
{
    /// <summary>
    /// Static settings for Control Script Syncer
    /// </summary>
    public class ControlSyncSettings
    {
        /// <summary>
        /// VersionTime format for file system
        /// </summary>
        public static string DateFileFormat = "yyyy-MM-dd HH-mm";
        /// <summary>
        /// SQL date format
        /// </summary>
        public static string DateSQLFormat = "yyyy-MM-dd HH:mm";


        public static FileLoadMode FileLoadMode = file.FileLoadMode.AllMissingInDatabase;
    }
}
