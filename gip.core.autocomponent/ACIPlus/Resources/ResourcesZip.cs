// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gip.core.autocomponent
{
    public class ResourcesZip : Resources
    {
        #region ctor's

        public ResourcesZip()
            : base()
        {

        }

        #endregion

        #region Dir

        public override ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = true)
        {
            int pos = path.LastIndexOf("\\");
            string zipName = path.Substring(pos + 1);
            string taskName = string.Format(@"ResourceZip.Dir(""{0}"")", zipName);
            ACFSItem rootACObjectItem = new ACFSItem(this, container, null, zipName, ResourceTypeEnum.Zip, "\\ZIP\\" + path);

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                ACEntitySerializer serializer = new ACEntitySerializer();
                // serializer.VBProgress = this.VBProgress;
                if (VBProgress != null)
                    VBProgress.AddSubTask(taskName, 0, zip.Entries.Count());

                foreach (ZipArchiveEntry item in zip.Entries.OrderBy(x => x.FullName))
                {
                    if (string.IsNullOrEmpty(item.Name) || !item.Name.EndsWith(Const.ACQueryExportFileType)) continue;

                    string fileContent = new string(
                                    (GetStreamReader(path + "\\" + item.FullName, Encoding.UTF8)
                                     .ReadToEnd())
                                     .ToArray());

                    string fullName = item.FullName;
                    fullName = fullName.Replace('/', '\\');
                    int lastIndex = fullName.LastIndexOf('\\');
                    string subPath = fullName.Substring(0, lastIndex);
                    ACFSItem folderRootFSFolderItem = rootACObjectItem.GetChildFolderItem(this, subPath, true);

                    try
                    {
                        XElement xDoc = XElement.Parse(fileContent);
                        serializer.DeserializeXML(this, db, folderRootFSFolderItem, xDoc, null, "\\Resources\\" + path + @"\" + item.FullName);
                        if (Worker != null && serializer.MsgList.Any())
                        {
                            foreach(Msg msg in serializer.MsgList)
                            {
                                Worker.ReportProgress(0, msg);
                            }
                        }     
                    }
                    catch (Exception ec)
                    {
                        if (Worker != null)
                        {
                            Worker.ReportProgress(0, new Msg()
                            {
                                MessageLevel = eMsgLevel.Error,
                                Message = string.Format(@"ResourcesZip({0}): Unable to deserialize XML in item {1}! Exception: {2}", path, item.FullName, ec.Message)
                            });
                        }
                    }

                    if (VBProgress != null)
                        VBProgress.ReportProgress(taskName, zip.Entries.IndexOf(item), "Extract item " + item.Name);
                }
            }

            SetupProperties(rootACObjectItem);
            return rootACObjectItem;
        }

#endregion

#region Content Operations

#region Content Operations -> Read

        public override string ReadText(string filename)
        {
            return ReadTextEncoding(filename, Encoding.UTF8);
        }

        public override string ReadTextEncoding(string filename, Encoding encoding)
        {
            StreamReader streamReader = GetStreamReader(filename, encoding);
            if (streamReader == null) return null;
            return new string(streamReader.ReadToEnd().ToArray());
        }

        public override string[] ReadLinesEncoding(string filename, Encoding encoding)
        {
            StreamReader streamReader = GetStreamReader(filename, encoding);
            if (streamReader == null) return null;
            List<string> result = new List<string>();
            string line = null;
            while (!string.IsNullOrEmpty(line = streamReader.ReadLine()))
            {
                result.Add(line);
            }
            return result.ToArray();
        }

        public override byte[] ReadBinary(string filename)
        {
            int indexOfZip = filename.LastIndexOf(".zip");
            string zipFile = filename.Substring(0, indexOfZip + ".zip".Length);
            if (!File.Exists(zipFile)) return null;
            string entryFullName = filename.Substring(indexOfZip + ".zip".Length + 1, (filename.Length - indexOfZip - ".zip".Length - 1));
            using (BinaryReader binaryReader = new System.IO.BinaryReader(
                    System.IO.Compression.ZipFile.OpenRead(zipFile)
                    .Entries.Where(x => x.FullName.Equals(entryFullName, StringComparison.InvariantCulture))
                    .FirstOrDefault()
                    .Open()))
            {
                using (var ms = new MemoryStream())
                {
                    binaryReader.BaseStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            //return null;
        }

#endregion

#region Content Operations -> Write

        public override bool WriteText(string filename, string text)
        {
            Tuple<string, string> dissPath = DisassemblyPath(filename);
            using (ZipArchive zipArchive = ZipFile.Open(dissPath.Item1, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry = zipArchive.Entries.FirstOrDefault(x => x.FullName == dissPath.Item2);
                entry.Delete();
                string tmpFileName = Path.GetTempPath() + @"\test_" + Guid.NewGuid().ToString() + ".txt";

                File.WriteAllText(tmpFileName, text, Encoding.UTF8);
                zipArchive.CreateEntryFromFile(tmpFileName, dissPath.Item2);
                File.Delete(tmpFileName);
            }
            return false;
        }

        public override bool WriteBinary(string filename, byte[] text)
        {
            int indexOfZip = filename.LastIndexOf(".zip");
            string zipFile = filename.Substring(0, indexOfZip + ".zip".Length);
            if (!File.Exists(zipFile)) return false;
            string entryFullName = filename.Substring(indexOfZip + ".zip".Length + 1, (filename.Length - indexOfZip - ".zip".Length - 1));
            using (BinaryWriter binaryWriter = new System.IO.BinaryWriter(
                    System.IO.Compression.ZipFile.OpenRead(zipFile)
                    .Entries.Where(x => x.FullName.Equals(entryFullName, StringComparison.InvariantCulture))
                    .FirstOrDefault()
                    .Open()))
            {
                binaryWriter.Write(text);
            }
            return true;
        }

#endregion

#region Content Operations -> Folder operations

        public override bool CreateDir(string path)
        {
            throw new NotImplementedException();
        }

        public override bool CheckOrCreateDir(string filename)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteDir(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

#endregion

#region Content Operations -> File operations

        public override bool CopyFile(string sourceFilename, string destFilename, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public override bool MoveFile(string sourceFilename, string destFilename)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteFile(string filename)
        {
            throw new NotImplementedException();
        }

#endregion

#endregion

#region Helper methods

        public StreamReader GetStreamReader(string filename, Encoding encoding)
        {
            Tuple<string, string> dissPath = DisassemblyPath(filename);
            return new System.IO.StreamReader(
                    ZipFile.OpenRead(dissPath.Item1)
                    .Entries.Where(x => x.FullName.Equals(dissPath.Item2, StringComparison.InvariantCulture))
                    .FirstOrDefault()
                    .Open(), encoding);
        }

        public Tuple<string, string> DisassemblyPath(string filename)
        {
            int indexOfZip = filename.LastIndexOf(".zip");
            string zipFile = filename.Substring(0, indexOfZip + ".zip".Length);
            string entryFullName = filename.Substring(indexOfZip + ".zip".Length + 1, (filename.Length - indexOfZip - ".zip".Length - 1));
            return new Tuple<string, string>(zipFile, entryFullName);
        }

#endregion
    }
}
