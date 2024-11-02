// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.ControlScriptSync.model;
using gip.core.ControlScriptSync.sql;
using gip.core.ControlScriptSync.VBSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync.file
{
    /// <summary>
    /// Prepare list of ScriptFileInfo object - represent information about available script updates (in file system)
    /// </summary>
    public class FileCommand
    {

        #region DI

        public string RootFolder { get; private set; }

        public VBSQLCommand VBSQLCommand { get; private set; }

        #endregion

        #region ctor's
        public FileCommand(string rootFolder, VBSQLCommand vBSQLCommand)
        {
            RootFolder = rootFolder;
            VBSQLCommand = vBSQLCommand;
        }
        #endregion

        string SqlScriptLocation = @"VBControlScripts";
        public string GetScriptFolderPath()
        {
            return RootFolder + @"\" + SqlScriptLocation;
        }


       
        public UpdateFiles LoadUpdateFileList()
        {
            UpdateFiles updateFiles = null;
            switch (ControlSyncSettings.FileLoadMode)
            {
                case FileLoadMode.AllMissingInDatabase:
                    updateFiles = LoadUpdateFileListAllMissingInDatabase();
                    break;
                case FileLoadMode.HigherAsYoungestInDatabase:
                    updateFiles = LoadUpdateFileListHigherAsYoungestInDatabase();
                    break;
            }
            return updateFiles;
        }

        private UpdateFiles LoadUpdateFileListHigherAsYoungestInDatabase()
        {
            UpdateFiles updateFiles = new UpdateFiles();
            datamodel.ControlScriptSyncInfo maxVersion = VBSQLCommand.MaxVersion();
            updateFiles.MaxVersion = string.Format(@"{0}_{1}.zip", maxVersion.VersionTime.ToString("yyyy-MM-dd HH-mm"), maxVersion.UpdateAuthor);
            string folder = GetScriptFolderPath();
            DirectoryInfo dir = new DirectoryInfo(folder);
            var query = dir.GetFiles().Where(x => x.Extension == ".zip").Select(x => new ScriptFileInfo(x.Name, folder));
            List<datamodel.ControlScriptSyncInfo> dbScriptList = VBSQLCommand.AllVersions();
            if (maxVersion != null)
            {
                var queryIncluded = query.Where(x => x.Version > maxVersion.VersionTime);

                var queryExcluded = query.Where(x => x.Version <= maxVersion.VersionTime);
                queryExcluded = queryExcluded.Where(c => !dbScriptList.Select(x => x.VersionTime).Contains(c.Version));

                updateFiles.Items = queryIncluded.OrderBy(x => x.Version).ToList();
                updateFiles.ExcludedItems = queryExcluded.OrderBy(x => x.Version).ToList();
            }
            else
                updateFiles.Items = query.OrderBy(x => x.Version).ToList();

            return updateFiles;
        }

        private UpdateFiles LoadUpdateFileListAllMissingInDatabase()
        {
            List<ScriptFileInfo>  updateFileList = new List<ScriptFileInfo>();
            List<datamodel.ControlScriptSyncInfo> dbScriptList = VBSQLCommand.AllVersions();
            string folder = GetScriptFolderPath();
            DirectoryInfo dir = new DirectoryInfo(folder);
            List<ScriptFileInfo> fileScriptList = dir.GetFiles().Where(x => x.Extension == ".zip").Select(x => new ScriptFileInfo(x.Name, folder)).ToList();
            if (dbScriptList == null || !dbScriptList.Any())
            {
                updateFileList = fileScriptList.OrderBy(x => x.Version).ToList();
            }
            else
            {
                updateFileList = fileScriptList.Where(x => !dbScriptList.Select(y => y.VersionTime).Contains(x.Version)).OrderBy(x => x.Version).ToList();
            }
            UpdateFiles updateFiles = new UpdateFiles();
            updateFiles.Items = updateFileList;
            return updateFiles;
        }
    }
}
