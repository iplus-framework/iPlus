using System;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Runtime.Serialization;


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
        DataContractSerializer = 4
    }


    /// <summary>
    /// XML-Documents Importer Base
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'XML Importer'}de{'XML Importer'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAXMLDocImporterBase : PADocImporterBase
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
        [ACPropertyInfo(true, 204, "Configuration", "en{'Default Parser'}de{'Voreingesteller Parser'}", "", true)]
        public PAXMLDocImportParserType DefaultParserType
        {
            get;
            set;
        }

        #endregion

        #region abstract/virtual
        public virtual PAXMLDocImportParserType ParserType
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
                    case PAXMLDocImportParserType.DataContractSerializer:
                        parseSucc = ParseWithDCS(CurrentFileName);
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

        protected virtual bool ParseWithDCS(string fileName)
        {
            bool succ = true;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(TypeOfDeserialization);
                    object result = serializer.ReadObject(fs);
                    if (result == null)
                        return false;

                    succ = ProcessObject(result, serializer);
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.ParseWithDCS()", e.Message);
                return false;
            }


            return succ;
        }


        #endregion

        #region abstract Methods
        public abstract bool IsImporterForXMLDocType(ACEventArgs fileInfoArgs, XmlReader reader);
        public abstract bool ProcessObject(object xmlObj, object xmlParseObj);
        #endregion

        #endregion

        #region Event-Handler
        #endregion
    }
}