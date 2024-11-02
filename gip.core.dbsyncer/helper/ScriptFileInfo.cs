// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.dbsyncer.model;

namespace gip.core.dbsyncer.helper
{
    public class ScriptFileInfo
    {
        public int Version { get; set; }
        public string FileName{ get; set; }

        public DbSyncerInfoContext Context { get; set; }

        public string GetSqlContent()
        {
            return File.ReadAllText(FilePath());
        }

        public string FilePath()
        {
            return DbSyncerSettings.GetScriptFolderPath(Context.DbSyncerInfoContextID) + @"\" + FileName;
        }

        public DbSyncerInfo GetDbInfo()
        {
            FileInfo fi = new FileInfo(FilePath());
            List<string> fileNameParts = FileName.Split('-').ToList();
            string author = fileNameParts[fileNameParts.Count-1].Replace(".sql","");
            return new DbSyncerInfo()
            {
                DbSyncerInfoContextID = Context.DbSyncerInfoContextID,
                Version = Version,
                ScriptDate = fi.LastWriteTime,
                UpdateDate = DateTime.Now,
                UpdateAuthor = author 
            };
        }

    }
}
