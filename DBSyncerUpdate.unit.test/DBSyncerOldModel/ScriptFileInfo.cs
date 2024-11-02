// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.dbsyncer.helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace DBSyncerUpdate.unit.test.DBSyncerOldModel
{
    public class ScriptFileInfo
    {
        private DateTime _LastWriteTime;
        public string RootFolder { get; set; }
        #region ctor's
        public ScriptFileInfo(DbSyncerInfoContext context, FileInfo fi, string rootFolder)
        {
            Context = context;
            FileName = fi.Name;
            _LastWriteTime = fi.LastWriteTime;

            // dbsync - 0001 - aagincic.sql
            // dbsync - 2018 - 08 - 23_10 - 08 - aagincic.sql

            List<string> fileNameParts = FileName.Split('-').ToList();
            Version = int.Parse(fileNameParts[1]);
            UpdateAuthor = fileNameParts[fileNameParts.Count - 1].Replace(".sql", "");
            RootFolder = rootFolder;
        }
        #endregion

        #region properties

        public int Version { get; set; }
        public string FileName { get; set; }

        public DbSyncerInfoContext Context { get; set; }

        #endregion

        #region hepler methods

        public string GetSqlContent()
        {
            return File.ReadAllText(FilePath());
        }

        public string FilePath()
        {
            return DbSyncerSettings.GetScriptFolderPath(Context.DbSyncerInfoContextID, RootFolder) + @"\" + FileName;
        }

        public string UpdateAuthor { get; set; }
        public DbSyncerInfo GetDbInfo()
        {
            FileInfo fi = new FileInfo(FilePath());
            return new DbSyncerInfo()
            {
                DbSyncerInfoContextID = Context.DbSyncerInfoContextID,
                Version = Version,
                ScriptDate = _LastWriteTime,
                UpdateDate = DateTime.Now,
                UpdateAuthor = UpdateAuthor
            };
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return string.Format(@"[{0}] {1}", Context.DbSyncerInfoContextID, FileName);
        }

        #endregion
    }
}
