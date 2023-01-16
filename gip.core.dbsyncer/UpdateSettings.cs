using gip.core.dbsyncer.Command;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace gip.core.dbsyncer
{
    public class UpdateSettings
    {

        #region Settings

        public string StartVersion = @"1.0.0.0";
        public string RegistryAppName = @"iPlus";
        public string RegistryAppVerson = @"1.0";
        public string RegistryVersionKeyName = @"dbsyncer-version";
        public string RegistryVersionDefValue = @"1.0.0.0";
        public string RootFolder { get; private set; }
        #endregion

        #region ctor's

        public UpdateSettings(string rootFolder)
        {
            RootFolder = rootFolder;
        }

        public UpdateSettings()
        {
            RootFolder = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }

        #endregion

        public Dictionary<string, string> GetMissingVersions(DbContext db)
        {
            bool match = false;
            Dictionary<string, string> missingVersion = new Dictionary<string, string>();
            string currentVersion = DBSyncerVersionCommand.GetLatestVersion(db);
            if (string.IsNullOrEmpty(currentVersion))
                throw new Exception("Error operating with DBSyncerVersion table!");

            foreach (var item in Update.Versions)
            {
                if (match)
                {
                    missingVersion.Add(item.Key, item.Value);
                }
                if (!match)
                {
                    match = item.Key == currentVersion;
                }
            }
            return missingVersion;
        }

    }
}
