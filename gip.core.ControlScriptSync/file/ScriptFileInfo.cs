// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.ControlScriptSync.VBSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace gip.core.ControlScriptSync.file
{
    /// <summary>
    /// Information about update file stored into VBControlScripts folder
    /// Parses file name and extract all important information: VersionTime, Author
    /// </summary>
    public class ScriptFileInfo
    {
        public ScriptFileInfo(string fileName, string containerFolder)
        {
            FileName = fileName;
            FolderContainer = containerFolder;
            string datePart = "";
            // Exctracting author name from file name:
            if (DirectoryName.Contains('_'))
            {
                string[] nameParts = DirectoryName.Split('_');
                datePart = nameParts[0];
                Author = nameParts[1];
            }
            else
            {
                datePart = DirectoryName;
            }
            // Parsing VersionTime:
            Version = DateTime.ParseExact(datePart, ControlSyncSettings.DateFileFormat, CultureInfo.InvariantCulture);
        }

        public DateTime Version { get; set; }

        public string Author { get; set; }

        public string FileName { get; set; }

        public string FolderContainer { get; set; }

        public string DirectoryName
        {
            get
            {
                return FileName.Replace(".zip", "");
            }
        }

        public string FullDirectoryName
        {
            get
            {
                return System.IO.Path.GetTempPath() + DirectoryName;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Author))
            {
                return Version.ToString(ControlSyncSettings.DateFileFormat);
            }
            else
            {
                return Version.ToString(ControlSyncSettings.DateFileFormat) + "_" + Author;
            }
        }

        public void ExtractToFolder()
        {
            if (Directory.Exists(FullDirectoryName))
            {
                DeleteFolder();
            }
            ZipFile.ExtractToDirectory(FolderContainer + @"\" + FileName, System.IO.Path.GetTempPath());
        }

        public void DeleteFolder()
        {
            if (Directory.Exists(FullDirectoryName))
                Directory.Delete(FullDirectoryName, true);
        }
    }
}
