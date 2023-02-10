using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;


namespace gip.core.communication
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAAlarmChangeState'}de{'PAAlarmChangeState'}", Global.ACKinds.TACEnum)]
    public enum PAXMLDocImportParserType : short
    {
        XMLReader = 0, // Vorwärtsgerichtetes Knotenweises Lesen  (schnell)
        XMLDocument = 1, // Zugriff per XPath (DOM-Parser, langsam)
        XDocument = 2, // Zugriff per LINQ
        XMLSerializer = 3, // Erzeugnung eines Objektbaums anhand von XSD per XMLReader
    }


    /// <summary>
    /// XML-Documents Importer Base
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'XML Importer'}de{'XML Importer'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAXMLDocImporterBase : PAClassAlarmingBase
    {
        #region c´tors
        public PAXMLDocImporterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties

        #region Configuration
        [ACPropertyInfo(true, 201, "Configuration", "en{'Directory to archive'}de{'Archivierungsverzeichnis'}", "", true)]
        public string ArchiveDir
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 202, "Configuration", "en{'Archiving on'}de{'Archivierung aktivieren'}", "", true)]
        public bool ArchivingOn
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 203, "Configuration", "en{'Trash directory'}de{'Mülleimer Verzeichnis'}", "", true)]
        public string TrashDir
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 204, "Configuration", "en{'Default Parser'}de{'Voreingesteller Parser'}", "", true)]
        public PAXMLDocImportParserType DefaultParserType
        {
            get;
            set;
        }


        [ACPropertyInfo(true, 206, "Configuration", "en{'Create subdirectory per day'}de{'Pro Tag ein Unterverzeichnis erstellen'}", "", false)]
        public bool CreateSubDirPerDay
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 207, "Configuration", "en{'Forward to directory'}de{'Weiterleiten in Verzeichnis'}", "", true)]
        public string ForwardDir
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 208, DefaultValue = 10000)]
        public int PerfTimeoutStackTrace { get; set; }

        #endregion

        #region abstract/virtual
        public virtual PAXMLDocImportParserType ParserType
        {
            get
            {
                return DefaultParserType;
            }
        }

        public abstract Type TypeOfDeserialization
        {
            get;
        }
        #endregion

        #region Alarm
        [ACPropertyBindingSource(210, "Error", "en{'Import Alarm'}de{'Import Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsImportAlarm { get; set; }

        [ACPropertyBindingSource(211, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }
        #endregion

        #region protected
        string _CurrentFileName;
        public string CurrentFileName
        {
            get
            {
                return _CurrentFileName;
            }
            set
            {
                _CurrentFileName = value;
            }
        }
        #endregion

        #endregion

        #region Points
        #endregion

        #region Methods

        #region public
        public virtual bool DoImportAndArchive(ACEventArgs fileInfoArgs)
        {
            PerformanceEvent perfEvent = null;
            var vbDump = Root.VBDump;
            if (vbDump != null)
                perfEvent = vbDump.PerfLoggerStart(this.GetACUrl() + "!" + nameof(DoImportAndArchive), 100);

            try
            {
                bool parseSucc = false;
                _CurrentFileName = fileInfoArgs["FullPath"] as string;
                switch (ParserType)
                {
                    case PAXMLDocImportParserType.XMLReader:
                        parseSucc = ParseWithXMLReader(CurrentFileName);
                        break;
                    case PAXMLDocImportParserType.XMLSerializer:
                        parseSucc = ParseWithXMLSerializer(CurrentFileName);
                        break;
                    case PAXMLDocImportParserType.XMLDocument:
                        parseSucc = ParseWithXMLDocument(CurrentFileName);
                        break;
                    case PAXMLDocImportParserType.XDocument:
                        parseSucc = ParseWithXDocument(CurrentFileName);
                        break;
                }

                if (!String.IsNullOrWhiteSpace(ForwardDir))
                    ForwardFile(CurrentFileName, ForwardDir);

                // Archiviere falls nötig
                if (parseSucc)
                {
                    string movePath = FindAndCreateArchivePath(CurrentFileName);
                    MoveOrDeleteFile(ArchivingOn, movePath, CurrentFileName);
                }
                // Verschiebe in Mülleimer
                else
                {
                    string movePath = FindAndCreateTrashPath(CurrentFileName);
                    MoveOrDeleteFile(true, movePath, CurrentFileName);
                }
            }
            finally
            {
                if (vbDump != null && perfEvent != null)
                {
                    vbDump.PerfLoggerStop(this.GetACUrl() + "!" + nameof(DoImportAndArchive), 100, perfEvent, PerfTimeoutStackTrace);
                    if (perfEvent.IsTimedOut)
                        Messages.LogDebug(this.GetACUrl(), "DoImportAndArchive(Duration)", CurrentFileName);
                }
            }

            return true;
        }
        #endregion

        #region protected
        protected virtual bool ParseWithXMLReader(string fileName)
        {
            bool succ = true;
            XmlTextReader xmlReader = null;
            try
            {
                xmlReader = new XmlTextReader(fileName);
                succ = ProcessObject(xmlReader, xmlReader);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.ParseWithXMLReader()", e.Message);
                return false;
            }
            finally
            {
                xmlReader.Close();
                xmlReader.Dispose();
                xmlReader = null;
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            }
            return succ;
        }

        protected virtual bool ParseWithXMLSerializer(string fileName)
        {
            if (TypeOfDeserialization == null)
                return false;
            bool succ = true;
            XmlSerializer xmlSerializer = null;
            XmlReader xmlReader = null;
            bool xmlReaderClosed = false;
            FileStream fs = null;
            bool fsClosed = false;
            try
            {
                xmlSerializer = new XmlSerializer(TypeOfDeserialization);
                fs = new FileStream(fileName, FileMode.Open);
                xmlReader = XmlReader.Create(fs);
                object deserializedObj = xmlSerializer.Deserialize(xmlReader);
                if (xmlReader != null)
                {
                    xmlReader.Close();
                    xmlReaderClosed = true;
                }
                if (fs != null)
                {
                    fs.Close();
                    fsClosed = true;
                }

                succ = ProcessObject(deserializedObj, xmlSerializer);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXMLSerializer()", 1000, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXMLSerializer()", 1010, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.ParseWithXMLSerializer()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
                if (xmlReader != null)
                {
                    if (!xmlReaderClosed)
                        xmlReader.Close();
                    xmlReader.Dispose();
                    xmlReader = null;
                }
                if (fs != null)
                {
                    if (!fsClosed)
                        fs.Close();
                    fs.Dispose();
                    fs = null;
                }
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            }
            return succ;
        }

        protected virtual bool ParseWithXMLDocument(string fileName)
        {
            bool succ = true;
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(fileName);
                succ = ProcessObject(xmlDocument, xmlDocument);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXMLDocument()", 1020, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXMLDocument()", 1030, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.ParseWithXMLDocument()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            } 
            xmlDocument = null;
            return succ;
        }

        protected virtual bool ParseWithXDocument(string fileName)
        {
            bool succ = true;
            XDocument xDocument = null;
            try
            {
                xDocument = XDocument.Load(fileName);
                succ = ProcessObject(xDocument, xDocument);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXDocument()", 1040, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAXMLDocImporterBase", "ParseWithXDocument()", 1050, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.ParseWithXDocument()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            }
            xDocument = null;
            return succ;
        }

        #endregion

        #region abstract Methods
        public abstract bool IsImporterForXMLDocType(ACEventArgs fileInfoArgs, XmlReader reader);
        public abstract bool ProcessObject(object xmlObj, object xmlParseObj);
        #endregion


        #region Private Methods
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsImportAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsImportAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsImportAlarm);
            }
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if (IsImportAlarm.ValueT != PANotifyState.Off)
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }

        public void MoveOrDeleteFile(bool moveElseDelete, string movePath, string fromPath)
        {
            if (moveElseDelete)
            {
                if (!String.IsNullOrEmpty(movePath))
                {
                    try
                    {
                        if (File.Exists(movePath))
                            File.Delete(movePath);
                        File.Move(fromPath, movePath);
                        return;
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.MoveOrDeleteFile(0)", e.Message);
                    }
                }
            }
            try
            {
                File.Delete(fromPath);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.MoveOrDeleteFile(1)", e.Message);
            }
        }

        public string FindAndCreateTrashPath(string fileName)
        {
            if (String.IsNullOrEmpty(TrashDir))
                return "";
            return CreateArchivePath(TrashDir, fileName);
        }

        public string FindAndCreateArchivePath(string fileName)
        {
            if (String.IsNullOrEmpty(ArchiveDir))
                return "";
            return CreateArchivePath(ArchiveDir, fileName);
        }

        protected string CreateArchivePath(string inDirectory, string fileName)
        {
            string archivePath;
            char last = inDirectory.Last();
            if (last == '\\')
                archivePath = inDirectory.Substring(0, inDirectory.Length - 1);
            else
                archivePath = inDirectory;

            if (!Directory.Exists(archivePath))
                return "";

            // Archivierung erfolgt in Monatsordnern, alle Unterverzeichnisse werden automatisch generiert
            // inDirectory
            //      |
            //      ---- stamp.year
            //               |
            //               ----- stamp.month

            DateTime stampNow = DateTime.Now;
            archivePath = String.Format("{0}\\{1}", archivePath, stampNow.Year);
            if (!Directory.Exists(archivePath))
            {
                try
                {
                    DirectoryInfo dirInfo = Directory.CreateDirectory(archivePath);
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.FindAndCreateArchivePath(0)", e.Message);
                    return "";
                }
            }

            archivePath = String.Format("{0}\\{1:00}", archivePath, stampNow.Month);
            if (!Directory.Exists(archivePath))
            {
                try
                {
                    DirectoryInfo dirInfo = Directory.CreateDirectory(archivePath);
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.FindAndCreateArchivePath(1)", e.Message);
                    return "";
                }
            }


            if (CreateSubDirPerDay)
            {
                archivePath = String.Format("{0}\\{1:00}", archivePath, stampNow.Day);
                if (!Directory.Exists(archivePath))
                {
                    try
                    {
                        DirectoryInfo dirInfo = Directory.CreateDirectory(archivePath);
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.FindAndCreateArchivePath(1)", e.Message);
                        return "";
                    }
                }
            }

            if (!String.IsNullOrEmpty(fileName))
            {
                string fileNameWithoutPath = ExtractFileName(fileName);
                if (!String.IsNullOrEmpty(fileNameWithoutPath))
                    archivePath = String.Format("{0}\\{1}", archivePath, fileNameWithoutPath);
            }

            return archivePath;
        }

        private string ExtractFileName(string fileNameWithPath)
        {
            if (String.IsNullOrEmpty(fileNameWithPath))
                return fileNameWithPath;
            return Path.GetFileName(fileNameWithPath);
            //string fileNameWithoutPath = fileNameWithPath;
            //char last = fileNameWithPath.Last();
            //if (last == '\\')
            //    fileNameWithoutPath = fileNameWithPath.Substring(0, fileNameWithPath.Length - 1);
            //else
            //    fileNameWithoutPath = fileNameWithPath;
            //int indexOfBackSlash = fileNameWithoutPath.LastIndexOf("\\");
            //if (indexOfBackSlash >= 0)
            //    fileNameWithoutPath = fileNameWithoutPath.Substring(indexOfBackSlash + 1);
            //return fileNameWithoutPath;
        }


        public bool ForwardFile(string file, string destinationDirPath)
        {
            try
            {
                DirectoryInfo targetDir = new DirectoryInfo(destinationDirPath);
                if (!targetDir.Exists)
                    return false;
                FileInfo fileInfo = new FileInfo(file);
                string destinationFilePath = Path.Combine(destinationDirPath, fileInfo.Name);
                File.Copy(file, destinationFilePath);
                return true;
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.MoveOrDeleteFile(0)", e);
            }
            return false;
        }

        #endregion

        #endregion

        #region Event-Handler
        #endregion
    }
}