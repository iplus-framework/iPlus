using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Linq;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Manager zum Verwalten des Verzeichnissen, Dateien und externen Anwendungen
    /// </summary>
    public class Resources : IResources
    {
        #region c´tors

        public Resources()
        {
        }

        #endregion

        #region IResources Member

        #region Filesystem


        /// <summary>Liefert Direktory</summary>
        /// <param name="db"></param>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        /// <param name="withFiles"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("Directory", "en{'Directory'}de{'Verzeichnis'}", 9999)]
        public virtual ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = true)
        {
            if (string.IsNullOrEmpty(initialPath))
                initialPath = path;
            int pos = path.LastIndexOf("\\");
            string folderName = path.Substring(pos + 1);
            string taskName = string.Format(@"Resources.Dir(""{0}"")", folderName);

            var directoriesEnumeration = Directory.EnumerateDirectories(path);
            var filesEnumeration = Directory.EnumerateFiles(path).Where(x => x.EndsWith(Const.ACQueryExportFileType)).OrderBy(x => x).ToList();
            int totalItemsForProcess = directoriesEnumeration.Count() + (withFiles ? filesEnumeration.Count() : 0);

            if (VBProgress != null)
                VBProgress.AddSubTask(taskName, 0, totalItemsForProcess);

            ACFSItem rootACObjectItem = new ACFSItem(this, container, null, folderName, ResourceTypeEnum.Folder, "\\Resources\\" + path);
            int index = 0;



            if (withFiles)
            {
                ACEntitySerializer serializer = new ACEntitySerializer();
                // serializer.VBProgress = this.VBProgress;
                foreach (var file in filesEnumeration)
                {
                    try
                    {
                        XElement xDoc = XElement.Parse(File.ReadAllText(file));
                        serializer.DeserializeXML(this, db, rootACObjectItem, xDoc, null, "\\Resources\\" + file);
                        if (System.Windows.Application.Current != null)
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                if (serializer.MsgList.Any() && MsgObserver != null)
                                    serializer.MsgList.ForEach(x => MsgObserver.SendMessage(x));
                            }));
                    }
                    catch (Exception ec)
                    {
                        if (MsgObserver != null)
                        {
                            MsgObserver.SendMessage(new Msg()
                            {
                                MessageLevel = eMsgLevel.Error,
                                Message = string.Format(@"Resources({0}): Unable to deserialize file: {1}! Exception: {2}", path, file, ec.Message)
                            });
                        }
                    }
                    index++;
                    if (VBProgress != null)
                        VBProgress.ReportProgress(taskName, index, string.Format("{0} processed.", file));
                }
            }

            foreach (var folder in directoriesEnumeration)
            {
                ACFSItem childItem = Dir(db, container, folder, recursive, withFiles);
                rootACObjectItem.Add(childItem);
                index++;
                if (VBProgress != null)
                    VBProgress.ReportProgress(taskName, index, string.Format("{0} processed.", folder));
            }

            if (VBProgress != null)
                VBProgress.ReportProgress(taskName, totalItemsForProcess, "Directory processed!");

            if (path == initialPath)
                SetupProperties(rootACObjectItem);
            return rootACObjectItem;
        }

        [ACMethodInfo("File", "en{'Read Textfile'}de{'Textdatei lesen'}", 9999)]
        public virtual string ReadText(string filename)
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "ReadText", msg);
                return null;
            }
        }

        [ACMethodInfo("File", "en{'Read Textfile'}de{'Textdatei lesen'}", 9999)]
        public virtual string ReadTextEncoding(string filename, Encoding encoding)
        {
            try
            {
                return File.ReadAllText(filename, encoding);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "ReadTextEncoding", msg);
                return null;
            }
        }


        [ACMethodInfo("File", "en{'Read Lines'}de{'Textzeilen lesen'}", 9999)]
        public virtual string[] ReadLinesEncoding(string filename, Encoding encoding)
        {
            try
            {
                return File.ReadAllLines(filename, encoding);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "ReadLinesEncoding", msg);
                return null;
            }
        }

        [ACMethodInfo("File", "en{'Write Textfile'}de{'Textdatei schreiben'}", 9999)]
        public virtual bool WriteText(string filename, string text)
        {
            try
            {
                if (!CheckOrCreateDir(filename))
                    return false;
                File.WriteAllText(filename, text);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "WriteText", msg);
                return false;
            }
        }

        [ACMethodInfo("File", "en{'Read Binaryfile'}de{'Binärdatei lesen'}", 9999)]
        public virtual Byte[] ReadBinary(string filename)
        {
            if (!File.Exists(filename)) return null;
            try
            {
                return File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "ReadBinary", msg);
                return null;
            }
        }

        [ACMethodInfo("File", "en{'Write Binaryfile'}de{'Binärdatei schreiben'}", 9999)]
        public virtual bool WriteBinary(string filename, Byte[] text)
        {
            try
            {
                if (!CheckOrCreateDir(filename))
                    return false;
                File.WriteAllBytes(filename, text);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "WriteBinary", msg);
                return false;
            }
        }

        [ACMethodInfo("Directory", "en{'Create Directory'}de{'Verzeichnis erstellen'}", 9999)]
        public virtual bool CreateDir(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "CreateDir", msg);
                // TODO: Fehlerbehandlung
                return false;
            }
        }

        [ACMethodInfo("Directory", "en{'Check/Create Directory'}de{'Verzeichnis prüfen/erstellen'}", 9999)]
        public virtual bool CheckOrCreateDir(string filename)
        {
            try
            {
                int pos = filename.LastIndexOf("\\");
                int pos2 = filename.Substring(pos).LastIndexOf(".");

                if (pos2 != -1)
                {
                    filename = filename.Substring(0, pos);
                }
                if (!Directory.Exists(filename))
                    Directory.CreateDirectory(filename);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "CheckOrCreateDir", msg);
                return false;
            }
            return true;
        }

        [ACMethodInfo("Directory", "en{'Delete Directory'}de{'Verzeichnis löschen'}", 9999)]
        public virtual bool DeleteDir(string path, bool recursive = false)
        {
            try
            {
                Directory.Delete(path, recursive);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "DeleteDir", msg);
                // TODO: Fehlerbehandlung
                return false;
            }
        }

        [ACMethodInfo("File", "en{'Copy File'}de{'Datei kopieren'}", 9999)]
        public virtual bool CopyFile(string sourceFilename, string destFilename, bool overwrite = false)
        {
            try
            {
                if (!CheckOrCreateDir(destFilename))
                    return false;
                try
                {
                    File.Copy(sourceFilename, destFilename, overwrite);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("Resources", "CopyFile", msg);
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "CopyFile(10)", msg);
                // TODO: Fehlerbehandlung
                return false;
            }
        }

        [ACMethodInfo("File", "en{'Move File'}de{'Datei verschieben'}", 9999)]
        public virtual bool MoveFile(string sourceFilename, string destFilename)
        {
            try
            {
                if (!CheckOrCreateDir(destFilename))
                    return false;
                File.Move(sourceFilename, destFilename);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "MoveFile", msg);
                return false;
            }
        }

        [ACMethodInfo("File", "en{'Delete File'}de{'Datei löschen'}", 9999)]
        public virtual bool DeleteFile(string filename)
        {
            try
            {
                File.Delete(filename);
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Resources", "DeleteFile", msg);
                return false;
            }
        }
        #endregion


        #endregion

        #region Progress and messages

        public IMsgObserver MsgObserver { get; set; }

        public IVBProgress VBProgress { get; set; }

        #endregion

        #region Helper methods

        public virtual void SetupProperties(ACFSItem rootACObjectItem)
        {
            List<Msg> tmpMsgList = new List<Msg>();
            rootACObjectItem.SetupProperties(tmpMsgList);
            if (MsgObserver != null)
            {
                if (System.Windows.Application.Current != null)
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        foreach (Msg msg in tmpMsgList)
                            MsgObserver.SendMessage(msg);
                    }));
            }
        }

        #endregion

        #region private members

        private string initialPath;
        #endregion

    }
}
