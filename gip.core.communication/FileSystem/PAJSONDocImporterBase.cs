using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace gip.core.communication
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAJSONDocImportParserType'}de{'PAJSONDocImportParserType'}", Global.ACKinds.TACEnum)]
    public enum PAJSONDocImportParserType : short
    {
        JsonDocument = 0, // System.Text.Json JsonDocument (fast, read-only)
        JsonSerializer = 1, // System.Text.Json JsonSerializer for object deserialization
        DataContractSerializer = 2, // DataContractJsonSerializer
        NewtonsoftJson = 3 // Newtonsoft.Json JsonConvert
    }

    /// <summary>
    /// JSON-Documents Importer Base
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'JSON Importer'}de{'JSON Importer'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAJSONDocImporterBase : PAClassAlarmingBase
    {
        #region c´tors
        public PAJSONDocImporterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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
        public PAJSONDocImportParserType DefaultParserType
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
        public virtual PAJSONDocImportParserType ParserType
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
                    case PAJSONDocImportParserType.JsonDocument:
                        parseSucc = ParseWithJsonDocument(CurrentFileName);
                        break;
                    case PAJSONDocImportParserType.JsonSerializer:
                        parseSucc = ParseWithJsonSerializer(CurrentFileName);
                        break;
                    case PAJSONDocImportParserType.DataContractSerializer:
                        parseSucc = ParseWithDataContractJsonSerializer(CurrentFileName);
                        break;
                    case PAJSONDocImportParserType.NewtonsoftJson:
                        parseSucc = ParseWithNewtonsoftJson(CurrentFileName);
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
        protected virtual bool ParseWithJsonDocument(string fileName)
        {
            bool succ = true;
            JsonDocument jsonDocument = null;
            try
            {
                string jsonContent = File.ReadAllText(fileName, Encoding.UTF8);
                jsonDocument = JsonDocument.Parse(jsonContent);
                succ = ProcessObject(jsonDocument, jsonDocument);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithJsonDocument()", 1020, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithJsonDocument()", 1030, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.ParseWithJsonDocument()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
                if (jsonDocument != null)
                {
                    jsonDocument.Dispose();
                    jsonDocument = null;
                }
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            }
            return succ;
        }

        protected virtual bool ParseWithJsonSerializer(string fileName)
        {
            if (TypeOfDeserialization == null)
                return false;
            bool succ = true;
            FileStream fs = null;
            bool fsClosed = false;
            try
            {
                string jsonContent = File.ReadAllText(fileName, Encoding.UTF8);
                // JSON > 4.7.0:
                //fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                //object deserializedObj = System.Text.Json.JsonSerializer.Deserialize(fs, TypeOfDeserialization);
                object deserializedObj = System.Text.Json.JsonSerializer.Deserialize(jsonContent, TypeOfDeserialization);
                if (fs != null)
                {
                    fs.Close();
                    fsClosed = true;
                }

                succ = ProcessObject(deserializedObj, null);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithJsonSerializer()", 1000, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithJsonSerializer()", 1010, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.ParseWithJsonSerializer()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
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

        protected virtual bool ParseWithDataContractJsonSerializer(string fileName)
        {
            bool succ = true;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(TypeOfDeserialization);
                    object result = serializer.ReadObject(fs);
                    if (result == null)
                        return false;

                    succ = ProcessObject(result, serializer);
                }
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithDataContractJsonSerializer()", 1040, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithDataContractJsonSerializer()", 1050, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.ParseWithDataContractJsonSerializer()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }

            return succ;
        }

        protected virtual bool ParseWithNewtonsoftJson(string fileName)
        {
            if (TypeOfDeserialization == null)
                return false;
            bool succ = true;
            
            try
            {
                string jsonContent = File.ReadAllText(fileName, Encoding.UTF8);
                object deserializedObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent, TypeOfDeserialization);
                
                succ = ProcessObject(deserializedObj, null);
            }
            catch (Exception e)
            {
                IsImportAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = null;

                //Error50193: Filename: {0}, Message: {1}
                if (e.InnerException == null)
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithNewtonsoftJson()", 1060, "Error50193", fileName, e.Message);
                //Error50194: Filename: {0}, Message: {1}, {2}
                else
                    msg = new Msg(this, eMsgLevel.Error, "PAJSONDocImporterBase", "ParseWithNewtonsoftJson()", 1070, "Error50194", fileName, e.Message, e.InnerException.Message);

                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.ParseWithNewtonsoftJson()", ErrorText.ValueT);
                OnNewAlarmOccurred(IsImportAlarm, msg, true);
                return false;
            }
            finally
            {
                PAFileSystemWatcherBase fileSystemWatcher = ParentACComponent as PAFileSystemWatcherBase;
                if (fileSystemWatcher != null)
                    fileSystemWatcher.ImporterHasProcessed(fileName);
            }
            return succ;
        }

        #endregion

        #region abstract Methods
        public abstract bool IsImporterForJSONDocType(ACEventArgs fileInfoArgs, JsonElement rootElement);
        public abstract bool ProcessObject(object jsonObj, object jsonParseObj);
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
                        Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.MoveOrDeleteFile(0)", e.Message);
                    }
                }
            }
            try
            {
                File.Delete(fromPath);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.MoveOrDeleteFile(1)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.FindAndCreateArchivePath(0)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.FindAndCreateArchivePath(1)", e.Message);
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
                        Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.FindAndCreateArchivePath(2)", e.Message);
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
                Messages.LogException(this.GetACUrl(), "PAJSONDocImporterBase.ForwardFile()", e);
            }
            return false;
        }

        #endregion

        #endregion

        #region Event-Handler
        #endregion
    }
}