using System;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Text.Json;
using System.Runtime.Serialization.Json;

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
    public abstract class PAJSONDocImporterBase : PADocImporterBase
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
        [ACPropertyInfo(true, 204, "Configuration", "en{'Default Parser'}de{'Voreingesteller Parser'}", "", true)]
        public PAJSONDocImportParserType DefaultParserType
        {
            get;
            set;
        }

        #endregion

        #region abstract/virtual
        public virtual PAJSONDocImportParserType ParserType
        {
            get
            {
                return DefaultParserType;
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

        #endregion

    }
}