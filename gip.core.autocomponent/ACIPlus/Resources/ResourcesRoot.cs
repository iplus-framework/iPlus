// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gip.core.autocomponent
{
    public class ResourcesRoot : Resources
    {

        #region ctor's

        public ResourcesRoot()
            : base()
        {
        }

        #endregion

        #region Dir

        public override ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = false)
        {
            int pos = path.LastIndexOf("\\");
            ACFSItem rootACObjectItem = new ACFSItem(this, container, null, Path.GetDirectoryName(path), ResourceTypeEnum.Folder, "\\Root\\" + path);
            var directoriesEnumeration = Directory.EnumerateDirectories(path);


            if (withFiles)
            {
                var filesEnumeration = Directory.EnumerateFiles(path);
                foreach (var file in filesEnumeration)
                {
                    if (ResourceFactory.IsResourceZip(file) || ResourceFactory.IsResourceXML(file))
                    {
                        IResources handlingResource = ResourceFactory.Factory(file, Worker);
                        handlingResource.VBProgress = VBProgress;
                        ACFSItem childItem = handlingResource.Dir(db, container, file, recursive, withFiles);
                        rootACObjectItem.Add(childItem);
                    }
                    else if (file.EndsWith(Const.ACQueryExportFileType))
                    {
                        ACEntitySerializer serializer = new ACEntitySerializer();
                        // serializer.VBProgress = this.VBProgress;
                        try
                        {
                            XElement xDoc = XElement.Parse(File.ReadAllText(file));
                            serializer.DeserializeXML(this, db, rootACObjectItem, xDoc, null, "\\Resources\\" + file);
                          
                            if(Worker != null)
                            {
                                foreach (Msg msg in serializer.MsgList)
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
                                    Message = string.Format(@"ResourcesRoot({0}): Unable to deserialize file: {1}! Exception: {2}", path, file, ec.Message)
                                });
                            }
                        }
                    }
                }
            }

            SetupProperties(rootACObjectItem);

            foreach (var folder in directoriesEnumeration)
            {
                pos = folder.LastIndexOf("\\");
                string folderName = folder.Substring(pos + 1);
                if (folderName.StartsWith(ACProject.ClassName))
                {
                    IResources basicResourceHt = ResourceFactory.Factory(folder, Worker);
                    basicResourceHt.VBProgress = VBProgress;
                    ACFSItem childItem = basicResourceHt.Dir(db, container, folder, recursive, withFiles);
                    rootACObjectItem.Add(childItem);
                }
                else
                {
                    ACFSItem childItem = Dir(db, container, folder, recursive, withFiles);
                    rootACObjectItem.Add(childItem);
                }
            }

            return rootACObjectItem;
        }

#endregion

    }
}
