﻿using System;
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
using System.Threading;

namespace gip.core.communication
{
    /// <summary>
    /// XML-Documents Exporter Base
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'XML Exporter'}de{'XML Exporter'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAXMLDocExporterBase : PAFileCyclicGroupBase, IACComponentTaskExec
    {
        #region c´tors
        public PAXMLDocExporterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            //_TaskInvocationPoint = new ACPointAsyncRMI(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint = new ACPointTask(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint.SetMethod = OnSetTaskInvocationPoint;
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

        [ACPropertyInfo(true, 203, "Configuration", "en{'Export directory'}de{'Export Verzeichnis'}", "", true)]
        public string ExportDir
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 204, "Configuration", "en{'Export deactivated'}de{'Export Deaktiviert'}", "", true)]
        public bool ExportOff
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 205, "Configuration", "en{'Move to Networkfolder'}de{'Auf Netzlaufwerk verschieben'}", "", true)]
        public string MoveToNetDir
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

        [ACPropertyInfo(true, 207, "Configuration", "en{'Nr of retries sending XML'}de{'Probenummber für XMLSchickung'}", "", false)]
        public int NrOfRetriesSendingXML
        {
            get;
            set;
        }


        #endregion

        #region abstract/virtual
        protected abstract string XMLFileName { get; }
        #endregion

        #endregion

        #region Points
        protected ACPointTask _TaskInvocationPoint;
        [ACPropertyAsyncMethodPoint(9999, false, 0)]
        public ACPointTask TaskInvocationPoint
        {
            get
            {
                return _TaskInvocationPoint;
            }
        }

        public bool OnSetTaskInvocationPoint(IACPointNetBase point)
        {
            TaskInvocationPoint.DeQueueInvocationList();
            return true;
        }
        public bool ActivateTask(ACMethod acMethod, bool executeMethod, IACComponent executingInstance = null)
        {
            return ACPointAsyncRMIHelper.ActivateTask(this, acMethod, executeMethod, executingInstance);
        }

        public bool CallbackTask(ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted)
        {
            return ACPointAsyncRMIHelper.CallbackTask(this, acMethod, result, state);
        }

        public bool CallbackTask(IACTask task, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted)
        {
            return ACPointAsyncRMIHelper.CallbackTask(this, task, result, state);
        }

        public IACTask GetTaskOfACMethod(ACMethod acMethod)
        {
            return ACPointAsyncRMIHelper.GetTaskOfACMethod(this, acMethod);
        }

        public bool CallbackCurrentTask(ACMethodEventArgs result)
        {
            return ACPointAsyncRMIHelper.CallbackCurrentTask(this, result);
        }

        #endregion

        #region Methods

        #region public
        public virtual Exception WriteXMLWithRetry<T>(string archivFileName, string exportFileName, T rootNode) where T : class
        {
            int tmpNrOfRetries = 0;
            Exception exception = new Exception("?");
            while (tmpNrOfRetries < NrOfRetriesSendingXML && exception != null)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    TextWriter writer = new StreamWriter(archivFileName);
                    serializer.Serialize(writer, rootNode);
                    writer.Close();
                    MoveOrArchiveFile(ArchivingOn, exportFileName, archivFileName);
                    exception = null;
                }
                catch (Exception ec)
                {
                    exception = ec;
                }
                Thread.Sleep(10);
                tmpNrOfRetries++;
            }
            return exception;
        }

        public static string SerializeToXML<T>(T rootNode) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, rootNode);
                return writer.ToString();
            }
        }

        public virtual void SerializeToXMLEncoded<T>(T rootNode, string fileName) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(EncodingName))
            {
                encoding = Encoding.GetEncoding(EncodingName);
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Encoding = encoding;
            settings.Indent = true;
            StreamWriter sw = new StreamWriter(fileName, false, encoding);
            XmlWriter writer = XmlWriter.Create(sw, settings);
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
            xmlns.Add(string.Empty, string.Empty);
            serializer.Serialize(writer, rootNode, xmlns);
            writer.Close();
        }

        public virtual bool CheckPath(string className, out string xmlFileName, out string archivFileName, out string exportFileName)
        {
            xmlFileName = XMLFileName;
            archivFileName = FindAndCreateArchivePath(xmlFileName);
            exportFileName = "";
            if (String.IsNullOrEmpty(archivFileName))
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                //Error50195: Invalid archive path.
                Msg msg = new Msg(this, eMsgLevel.Error, className, "CheckPath()", 1010, "Error50195");
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), string.Format("{0}.CheckPath()", className), ErrorText.ValueT);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }

            exportFileName = FindAndCreateExportPath(xmlFileName);
            if (String.IsNullOrEmpty(exportFileName))
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                //Error50196: Invalid export path.
                Msg msg = new Msg(this, eMsgLevel.Error, className, "CheckPath(10)", 1020, "Error50196");
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), string.Format("{0}.CheckPath()", className), ErrorText.ValueT);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }
            return true;
        }

        public ACMethodEventArgs AddExportTaskToQueue(ACDelegateQueue exporterQueue, string className, string methodName, ACMethod acMethod, Func<bool> isEnabledFunction, Action action)
        {
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
            ACValue succValue = result.GetACValue("Succeeded");
            if (succValue == null)
            {
                succValue = new ACValue("Succeeded", typeof(Boolean), false);
                result.Add(succValue);
            }

            if (!isEnabledFunction())
                return result;

            ACPointAsyncRMIWrap<ACComponent> currentAsyncRMI = TaskInvocationPoint.CurrentAsyncRMI;

            if (exporterQueue != null)
            {
                exporterQueue.Add(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                        Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, className, methodName, 1010);
                        ErrorText.ValueT = msg.Message;
                        Messages.LogException(this.GetACUrl(), string.Format("{0}.{1}()", className, methodName), e.Message);
                        OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    }
                    finally
                    {
                        if (currentAsyncRMI != null && !currentAsyncRMI.CallbackIsPending)
                        {
                            TaskInvocationPoint.InvokeCallbackDelegate(new ACMethodEventArgs(currentAsyncRMI.RequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                        }
                        else
                        {
                            CallbackTask(acMethod, new ACMethodEventArgs(acMethod.ACRequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                        }
                    }
                });
            }

            result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
            return result;
        }

        #endregion

        #region protected

        #endregion

        #region abstract Methods
        #endregion

        #region Private Methods
        public void MoveOrArchiveFile(bool archivingOn, string movePath, string fromPath)
        {
            if (!archivingOn)
            {
                if (!String.IsNullOrEmpty(movePath))
                {
                    try
                    {
                        File.Move(fromPath, movePath);
                        return;
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PAXMLDocExporterBase.MoveOrDeleteFile(0)", e.Message);
                    }
                }
            }
            try
            {
                if (!String.IsNullOrEmpty(movePath))
                {
                    try
                    {
                        if (File.Exists(movePath))
                            File.Delete(movePath);
                        File.Copy(fromPath, movePath);
                        return;
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PAXMLDocExporterBase.MoveOrDeleteFile(1)", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocExporterBase.MoveOrDeleteFile(2)", e.Message);
            }
        }

        public string FindAndCreateExportPath(string fileName)
        {
            if (String.IsNullOrEmpty(ExportDir))
                return "";
            string exportPath;
            char last = ExportDir.Last();
            if (last == '\\')
                exportPath = ExportDir.Substring(0, ExportDir.Length - 1);
            else
                exportPath = ExportDir;

            if (!String.IsNullOrEmpty(fileName))
                exportPath = String.Format("{0}\\{1}", exportPath, fileName);

            return exportPath;
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
                    Messages.LogException(this.GetACUrl(), "PAXMLDocExporterBase.FindAndCreateArchivePath(0)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PAXMLDocExporterBase.FindAndCreateArchivePath(1)", e.Message);
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
                archivePath = String.Format("{0}\\{1}", archivePath, fileName);
            }

            return archivePath;
        }

        #endregion

        #endregion

        #region Event-Handler
        #endregion
    }
}
