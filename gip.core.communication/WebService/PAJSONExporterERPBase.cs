using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using System.Linq;
using System.Text;

namespace gip.core.communication
{

    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAJSONDocSerializerType'}de{'PAJSONDocSerializerType'}", Global.ACKinds.TACEnum)]
    public enum PAJSONDocSerializerType : short
    {
        SystemTextJson = 0,
        CustomSerializer = 1,
    }


    //TODO Ivan: add message(alarm) translation
    /// <summary>
    /// Represents the base class for ERP's web service invoker(exporter) and archiver using JSON format.
    /// The component which inherits this class must be installed as child-component of PAExportERPGroup component or their descendants. 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'JSON WebService Exporter'}de{'JSON Web Service Exporter'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAJSONExporterERPBase : PAJSONDocExporterBase, IExporterToExternalSystem
    {
        #region c'tors

        public PAJSONExporterERPBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            ObjectToSendCache = null;
            return await base.ACDeInit(deleteACClassTask);
        }

        public const string ClassName = "PAJSONExporterBase";

        #endregion

        #region Properties

        public const string IsExportingAlarmPropName = "IsExportingAlarm";

        /// <summary>
        /// Override with a type which will be sended on a invoke web service's method. This type must be serializable.
        /// </summary>
        public abstract Type SendObjectType
        {
            get;
        }

        protected virtual PAJSONDocSerializerType SerializerType
        {
            get
            {
                return PAJSONDocSerializerType.SystemTextJson;
            }
        }

        protected virtual JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    WriteIndented = JSONWriteIndented,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
            }
        }

        protected virtual string FileNameExtension
        {
            get
            {
                return ".json";
            }
        }


        /// <summary>
        /// Cache for last invoke call which isn't successfull.
        /// </summary>
        public virtual Tuple<ERPFileItem, object> ObjectToSendCache
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent component as a PAExportERPGroup.
        /// </summary>
        public PAExportERPGroup ParentExportGroup
        {
            get => ParentACComponent as PAExportERPGroup;
        }

        [ACPropertyInfo(true, 208, DefaultValue = 10000)]
        public int PerfTimeoutStackTrace { get; set; }

        #endregion

        #region Static methods

        /// <summary>
        /// Registers a virtual method with required parameters. It must be invoked from a static constructor.
        /// </summary>
        /// <param name="acMethod">The acMethod signature.</param>
        /// <param name="typeOfComponent">The type of component on which will be registered.</param>
        /// <param name="acMethodDescTranslation">The acMethod's description translation.</param>
        public static void RegisterSendACMethod(ACMethod acMethod, Type typeOfComponent, string acMethodDescTranslation)
        {
            ACMethod.RegisterVirtualMethod(typeOfComponent, "ExecuteBuildAndSend", acMethod, acMethodDescTranslation, null);
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Creates a object which will be sended to a web service.
        /// </summary>
        /// <param name="acMethod">The acMethod with parameters, that are needed to create a target object.</param>
        /// <returns>The created object which type must be equals to the overriden property SendObjectType.</returns>
        public abstract object CreateObject(ACMethod acMethod);

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="objectToSend">The object which will be sended in a method invoke. This object type must be equals to the overriden property SendObjectType</param>
        /// <returns>True if is invoke/send successfull, otherwise false.</returns>
        public abstract bool SendToWebService(object objectToSend);

        #endregion

        #region Virtual methods

        /// <summary>
        /// Invokes the BuildAndSend method in a delegate queue. This method MUST be overriden and decorated with ACMethodAsync attribute. Overriden method must return base.ExecuteBuildAndSend(acMethod);
        /// </summary>
        /// <param name="acMethod">The acMethod with parameters.</param>
        /// <returns>The ACMethod event arguments(RequestID and ACMethodResult)</returns>
        [ACMethodAsync("Send", "en{'Send Message asynchronous'}de{'Sende Nachricht asynchron'}", 201, false)]
        public virtual ACMethodEventArgs ExecuteBuildAndSend(ACMethod acMethod)
        {
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
            try
            {
                ACValue succValue = result.GetACValue("Succeeded");
                if (succValue == null)
                {
                    succValue = new ACValue("Succeeded", typeof(Boolean), false);
                    result.Add(succValue);
                }

                if (!IsEnabledExecuteBuildAndSend(acMethod))
                    return result;

                ACPointAsyncRMIWrap<ACComponent> currentAsyncRMI = TaskInvocationPoint.CurrentAsyncRMI;
                DateTime sendTime = DateTime.Now;

                ParentExportGroup.DelegateQueue.Add(() => BuildAndSend(acMethod, sendTime, currentAsyncRMI));
            }
            catch (Exception e)
            {
                Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "ExecuteBuildAndSend", 137);
                Messages.LogMessageMsg(msg);

                result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
                return result;
            }

            result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
            return result;
        }

        /// <summary>
        /// Determines is the BuildAndSend method invokable or not.
        /// </summary>
        /// <param name="acMethod">The acMethod with parameters.</param>
        /// <returns>True if is enabled to invoke, otherwise false.</returns>
        public virtual bool IsEnabledExecuteBuildAndSend(ACMethod acMethod)
        {
            if (ExportOff && !ArchivingOn)
                return false;

            Msg msg = null;

            if(string.IsNullOrEmpty(ExportDir))
                msg = new Msg("Temp directory (ExportDir) is not configured!", this, eMsgLevel.Error, "PAJSONExporterERPBase", "IsEnabledExecuteBuildAndSend(0)", 157);

            if (msg == null && ArchivingOn && string.IsNullOrEmpty(ArchiveDir))
                msg = new Msg("Archiving is enabled but Archive directory is not configured!", this, eMsgLevel.Error, "PAJSONExporterERPBase", "IsEnabledExecuteBuildAndSend(10)", 161);

            if(msg == null && ExportDir == ArchiveDir)
                msg = new Msg("Temp directory and Archive directory is same directory. This is not allowed. Please, change one of those two.", this, eMsgLevel.Error, "PAJSONExporterERPBase", "IsEnabledExecuteBuildAndSend(20)", 164);

            if (msg == null && ParentExportGroup == null)
                msg = new Msg("The parent component is not PAExportERPGroup!", this, eMsgLevel.Error, "PAJSONExporterERPBase", "IsEnabledExecuteBuildAndSend(30)", 157);

            if (msg == null && !acMethod.IsValid())
                msg = new Msg("acMethod is not valid", this, eMsgLevel.Error, "PAJSONExporterERPBase", "IsEnabledExecuteBuildAndSend(40)", 158);

            if (msg != null)
            {
                if (IsAlarmActive(IsExportingAlarm, msg.Message) == null)
                    Messages.LogMessageMsg(msg);
                AddAlarm(msg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Builds(create) object and tries to send it, over method SendToWebService.
        /// </summary>
        /// <param name="acMethod">The acMethod with parameters for object creation.</param>
        /// <param name="sendTime">The dateTime stamp when is operation added to delegate queue.</param>
        /// <param name="currentAsyncRMI">The current async remote invoker.</param>
        public virtual void BuildAndSend(ACMethod acMethod, DateTime sendTime, ACPointAsyncRMIWrap<ACComponent> currentAsyncRMI)
        {
            PerformanceEvent perfEvent = null;
            var vbDump = Root.VBDump;
            if (vbDump != null)
                perfEvent = vbDump.PerfLoggerStart(this.GetACUrl() + "!" + nameof(BuildAndSend), 100);
            try
            {
                object objectToSend = CreateObject(acMethod);
                if (objectToSend == null)
                    return;

                bool addToSendCache = false;

                if (ParentExportGroup.IsAvailableDirectSend)
                {
                    bool sended = false;

                    if (!ExportOff)
                    {
                        if (ParentExportGroup.IsWSSupportsMultiInvokesInSameTime)
                            sended = SendToWebService(objectToSend);
                        else
                        {
                            using (ACMonitor.Lock(ParentExportGroup.SendLockObject))
                            {
                                sended = SendToWebService(objectToSend);
                            }
                        }
                    }
                    else
                        sended = true;

                    if (sended)
                    {
                        if (ArchivingOn)
                        {
                            string archiveFilePath = FindAndCreateArchivePath(ERPFileItem.GenerateFileName(sendTime, ACIdentifier, FileNameExtension));

                            Msg msg = SerializeToFile(archiveFilePath, objectToSend, SendObjectType);
                            if (msg != null)
                            {
                                if (IsAlarmActive(IsExportingAlarm, msg.Message) == null)
                                {
                                    Messages.LogMessageMsg(msg);
                                }
                                AddAlarm(msg);
                            }
                        }

                        if (currentAsyncRMI != null && !currentAsyncRMI.CallbackIsPending)
                        {
                            TaskInvocationPoint.InvokeCallbackDelegate(new ACMethodEventArgs(currentAsyncRMI.RequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                        }
                        else
                        {
                            CallbackTask(acMethod, new ACMethodEventArgs(acMethod.ACRequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                        }
                    }
                    else
                    {
                        addToSendCache = true;
                    }
                }
                else if (!ExportOff)
                {
                    addToSendCache = true;
                }

                if (addToSendCache)
                {
                    ERPFileItem fileItem = new ERPFileItem(sendTime, ACIdentifier, ExportDir, FileNameExtension);
                    Msg msg = SerializeToFile(fileItem.FilePath, objectToSend, SendObjectType);
                    if (msg != null)
                    {
                        if (IsAlarmActive(IsExportingAlarm, msg.Message) == null)
                            Messages.LogMessageMsg(msg);
                        AddAlarm(msg);
                        return;
                    }
                    ParentExportGroup.AddObjectToSendCache(fileItem);
                }
            }
            catch (Exception e)
            {
                Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAJSONExporterERPBase", "BuildAndSend(10)", 30);
                if(IsAlarmActive(IsExportingAlarm, msg.Message) == null)
                    Messages.LogMessageMsg(msg);
                AddAlarm(msg);
            }
            finally
            {
                if (vbDump != null && perfEvent != null)
                {
                    vbDump.PerfLoggerStop(this.GetACUrl() + "!" + nameof(BuildAndSend), 100, perfEvent, PerfTimeoutStackTrace);
                    if (perfEvent.IsTimedOut)
                    {
                        string bpSerialized = ACConvert.ObjectToXML(acMethod, true);
                        Messages.LogDebug(this.GetACUrl(), "BuildAndSend(Duration)", bpSerialized);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes object to JSON file in a target directory.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="objectType">The type of object which will be serialized.</param>
        /// <returns>Null if is serialization successfull, otherwise false.</returns>
        public virtual Msg SerializeToFile(string filePath, object objectToSerialize, Type objectType)
        {
            if (string.IsNullOrEmpty(filePath))
                return new Msg("The parameter filePath is empty!", this, eMsgLevel.Error, ClassName, "SerializeToFile(10)", 188);

            if (objectToSerialize == null)
                return new Msg("The parameter objectToSerialize is null!", this, eMsgLevel.Error, ClassName, "SerializeToFile(20)", 191);

            try
            {
                if (SerializerType == PAJSONDocSerializerType.SystemTextJson)
                {
                    var options = JsonSerializerOptions;
                    string jsonString = JsonSerializer.Serialize(objectToSerialize, objectType, options);
                    File.WriteAllText(filePath, jsonString, Encoding.UTF8);
                }
                else if (SerializerType == PAJSONDocSerializerType.CustomSerializer)
                {
                    // For custom serializer implementation - can be overridden in derived classes
                    var options = JsonSerializerOptions;
                    string jsonString = JsonSerializer.Serialize(objectToSerialize, objectType, options);
                    File.WriteAllText(filePath, jsonString, Encoding.UTF8);
                }
                return null;
            }
            catch (Exception e)
            {
                return new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "SerializeToFile(30)", 204);
            }
        }

        /// <summary>
        /// Deserializes object from JSON file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="deserializedObject">Returns the deserialized object.</param>
        /// <returns>Null if is deserialization successfull, otherwise false.</returns>
        public virtual Msg DeserializeFromFile(string filePath, Type objectType, out object deserializedObject)
        {
            deserializedObject = null;

            if (string.IsNullOrEmpty(filePath))
                return new Msg("The parameter filePath is empty!", this, eMsgLevel.Error, ClassName, "DeserializeFromFile(10)", 213);

            try
            {
                if (SerializerType == PAJSONDocSerializerType.SystemTextJson)
                {
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var options = JsonSerializerOptions;
                    deserializedObject = JsonSerializer.Deserialize(jsonContent, objectType, options);
                }
                else if (SerializerType == PAJSONDocSerializerType.CustomSerializer)
                {
                    // For custom serializer implementation - can be overridden in derived classes
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var options = JsonSerializerOptions;
                    deserializedObject = JsonSerializer.Deserialize(jsonContent, objectType, options);
                }
                return null;
            }
            catch (Exception e)
            {
                return new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "DeserializeFromFile(20)", 226);
            }
        }

        /// <summary>
        /// Moves file from temporary send directory to archive directory.
        /// </summary>
        /// <param name="tempDirFilePath"></param>
        /// <returns></returns>
        public virtual Msg MoveFromTempDirToArchiveDir(string tempDirFilePath)
        {
            try
            {
                string fileName = Path.GetFileName(tempDirFilePath);
                string destFileName = FindAndCreateArchivePath(fileName);
                File.Move(tempDirFilePath, destFileName);
            }
            catch (Exception e)
            {
                return new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "MoveFromTempDirToArchiveDir", 238);
            }
            return null;
        }

        public virtual void ArchiveInvokeDirect(string methodName, object callParam, Type callParamType, object result, Type resultType)
        {
            string filePath = FindAndCreateArchivePath(ERPFileItem.GenerateFileName(DateTime.Now, ACIdentifier, FileNameExtension));
            try
            {
                var archiveData = new
                {
                    CallParameters = new
                    {
                        MethodName = methodName,
                        Timestamp = DateTime.Now,
                        Data = callParam
                    },
                    CallResult = result != null ? new
                    {
                        Timestamp = DateTime.Now,
                        Data = result
                    } : null
                };

                var options = JsonSerializerOptions;
                string jsonString = JsonSerializer.Serialize(archiveData, options);
                File.WriteAllText(filePath, jsonString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "ArchiveInvokeDirect", 184);
            }
        }

        /// <summary>
        /// Tries resend object from ERPFileItem or from object cache.
        /// </summary>
        /// <param name="fileItem">The ERPFile item. Represents the data about file which needs to be deserialized and sended to a web service.</param>
        /// <returns>True if resend is successfull, otherwise false.</returns>
        public virtual bool ReSendObject(ERPFileItem fileItem)
        {
            object objectToResend = null;

            if (ObjectToSendCache != null && ObjectToSendCache.Item1 == fileItem)
            {
                objectToResend = ObjectToSendCache.Item2;
            }

            if (objectToResend == null)
            {
                Msg msg = DeserializeFromFile(fileItem.FilePath, SendObjectType, out objectToResend);
                if (msg != null && objectToResend == null)
                {
                    if (IsAlarmActive(IsExportingAlarm, msg.Message) == null)
                    {
                        Messages.LogMessageMsg(msg);
                    }
                    AddAlarm(msg);

                    ParentExportGroup.RemoveFromSendCache(fileItem);
                    ObjectToSendCache = null;
                    return false;
                }
            }

            if (SendToWebService(objectToResend))
            {
                ObjectToSendCache = null;
                if (ArchivingOn)
                {
                    MoveFromTempDirToArchiveDir(fileItem.FilePath);
                }
                else
                {
                    File.Delete(fileItem.FilePath);
                }
                return true;
            }
            else
            {
                ObjectToSendCache = new Tuple<ERPFileItem, object>(fileItem, objectToResend);
            }

            return false;
        }

        #endregion

        public void AddAlarm(Msg msg, bool autoAck = true)
        {
            IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
            OnNewAlarmOccurred(IsExportingAlarm, msg, autoAck);
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch(acMethodName)
            {
                case "ExecuteBuildAndSend":
                    ExecuteBuildAndSend(acParameter[0] as ACMethod);
                    return true;

                case Const.IsEnabledPrefix + "ExecuteBuildAndSend":
                    result = IsEnabledExecuteBuildAndSend(acParameter[0] as ACMethod);
                    return true;
            }

            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
    }
}