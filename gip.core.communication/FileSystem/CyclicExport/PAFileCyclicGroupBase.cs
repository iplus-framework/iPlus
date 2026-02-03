// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;

namespace gip.core.communication
{
    /// <summary>
    /// Group for PAFilePropertyLogExport
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Cyclic export group'}de{'Zyklischer Export Gruppe'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAFileCyclicGroupBase : PAClassAlarmingBase
    {
        #region c´tors
        public PAFileCyclicGroupBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool result = await base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties

        #region Configuration
        private Boolean _CreateSubfolder;
        [ACPropertyInfo(true, 203, "Configuration", "en{'Create and export to subfolder'}de{'Erzeuge und exportiere in Unterordner'}", "", true)]
        public Boolean CreateSubfolder
        {
            get
            {
                return _CreateSubfolder;
            }
            set
            {
                _CreateSubfolder = value;
                OnPropertyChanged("CreateSubfolder");
            }
        }

        private string _FilenameFormat;
        [ACPropertyInfo(true, 204, "Configuration", "en{'Format of filename'}de{'Format des Dateinamens'}", "", true, DefaultValue = "{0}_{1:yyyyMMdd_HHmmss}.txt")]
        public string FilenameFormat
        {
            get
            {
                return _FilenameFormat;
            }
            set
            {
                _FilenameFormat = value;
                OnPropertyChanged("FilenameFormat");
            }
        }

        private Boolean _FileAppend;
        [ACPropertyInfo(true, 205, "Configuration", "en{'Append to existing file'}de{'Erweitere existierende Datei'}", "", true)]
        public Boolean FileAppend
        {
            get
            {
                return _FileAppend;
            }
            set
            {
                _FileAppend = value;
                OnPropertyChanged("FileAppend");
            }
        }

        private string _EncodingName;
        [ACPropertyInfo(true, 206, "Configuration", "en{'Name of encoding/codepage'}de{'Name des encodings/codepage'}", "", true)]
        public string EncodingName
        {
            get
            {
                return _EncodingName;
            }
            set
            {
                _EncodingName = value;
                OnPropertyChanged("EncodingName");
            }
        }

        [ACPropertyBindingSource(210, "Error", "en{'Export Alarm'}de{'Export Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsExportingAlarm { get; set; }

        [ACPropertyBindingSource(211, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }

        protected StreamWriter _StreamWriter;
        private bool _IsAppendingToStream = false;
        protected bool IsAppendingToStream
        {
            get
            {
                return _IsAppendingToStream;
            }
        }
        #endregion

        #region abstract and virtual
        // Property to avoid unnecessary calls to script methods if they doesn't exist
        protected bool? _HasScriptDefaultFileNameMethod = null;
        protected virtual string DefaultFileName
        {
            get
            {
                string fileName = "";
                if (!_HasScriptDefaultFileNameMethod.HasValue)
                    _HasScriptDefaultFileNameMethod = ACClassMethods.Where(c => c.ACIdentifier == "ScriptDefaultFileName").Any();
                if (_HasScriptDefaultFileNameMethod.Value)
                    fileName = ACUrlCommand("!ScriptDefaultFileName") as string;

                if (!String.IsNullOrEmpty(fileName))
                    return fileName;

                string fileNameFormat = FilenameFormat;
                if (String.IsNullOrEmpty(fileNameFormat))
                    fileNameFormat = "{0}_{1:yyyyMMdd_HHmmss}.txt";
                fileName = "";
                try
                {
                    fileName = String.Format(fileNameFormat, this.ACIdentifier, DateTime.Now);
                }
                catch (Exception e)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAFileCyclicGroupBase", "DefaultFileName", 1000);
                    ErrorText.ValueT = msg.Message;

                    Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.DefaultFileName(Format)", e.Message);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                }
                return fileName;
            }
        }

        // Property to avoid unnecessary calls to script methods if they doesn't exist
        protected bool? _HasScriptDefaultSubFolderNameMethod = null;
        protected virtual string DefaultSubFolderName
        {
            get
            {
                string folderName = "";
                if (!_HasScriptDefaultSubFolderNameMethod.HasValue)
                    _HasScriptDefaultSubFolderNameMethod = ACClassMethods.Where(c => c.ACIdentifier == "ScriptDefaultSubFolderName").Any();
                if (_HasScriptDefaultSubFolderNameMethod.Value)
                    folderName = ACUrlCommand("!ScriptDefaultSubFolderName") as string;
                if (!String.IsNullOrEmpty(folderName))
                    return folderName;

                return this.ACIdentifier;
            }
        }
        #endregion

        #endregion

        #region Methods

        #region public methods
        public bool OpenFile(string exportPath, string fileName)
        {
            if (_StreamWriter != null)
                return true;

            Msg msg = null;
            if (String.IsNullOrEmpty(fileName))
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                //Error50199:fileName is empty.
                msg = new Msg(this, eMsgLevel.Error, "PAFileCyclicGroupBase", "OpenFile()", 1010, "Error50199");
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(fileName)", ErrorText.ValueT);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }

            if (String.IsNullOrEmpty(exportPath))
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                //Error50200:Export-Path is empty.
                msg = new Msg(this, eMsgLevel.Error, "PAFileCyclicGroupBase", "OpenFile(10)", 1020, "Error50200");
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(exportPath)", ErrorText.ValueT);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }

            if (exportPath.Last() == '\\')
                exportPath = exportPath.Substring(0,exportPath.Length - 1);

            if (!Directory.Exists(exportPath))
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                //Error50201:Export-Path {0} doesn't exist!.
                msg = new Msg(this, eMsgLevel.Error, "PAFileCyclicGroupBase", "OpenFile(20)", 1030, "Error50201", exportPath);
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(exportPath)", ErrorText.ValueT);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }

            string subPath = exportPath;
            if (this.CreateSubfolder)
            {
                string folderName = DefaultSubFolderName;
                subPath = exportPath + "\\" + folderName;
                if (!Directory.Exists(subPath))
                {
                    try
                    {
                        Directory.CreateDirectory(subPath);
                    }
                    catch (Exception e)
                    {
                        IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                        msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAFileCyclicGroupBase", "OpenFile", 1040);
                        ErrorText.ValueT = msg.Message;

                        Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(Directory.CreateDirectory)", e.Message);
                        OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                        return false;
                    }
                }
            }

            string filePath = subPath + "\\" + fileName;

            Encoding encoding = null;
            if (!String.IsNullOrEmpty(this.EncodingName))
            {
                try
                {
                    encoding = Encoding.GetEncoding(this.EncodingName);
                }
                catch (Exception e)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAFileCyclicGroupBase", "OpenFile", 1050);
                    ErrorText.ValueT = msg.Message;
                    Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(Encoding.GetEncoding)", e.Message);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    return false;
                }
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    _IsAppendingToStream = false;
                    if (encoding == null)
                        _StreamWriter = new StreamWriter(filePath);
                    else
                        _StreamWriter = new StreamWriter(filePath, false, encoding);
                }
                else
                {
                    if (FileAppend)
                    {
                        _IsAppendingToStream = true;
                        if (encoding == null)
                            _StreamWriter = new StreamWriter(filePath, true);
                        else
                            _StreamWriter = new StreamWriter(filePath, true, encoding);
                    }
                    else
                    {
                        IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                        //Error50202:File {0} already exists. Turn on Property FileAppend to allow to append to file or remove file manually.
                        msg = new Msg(this, eMsgLevel.Exception, "PAFileCyclicGroupBase", "OpenFile(30)", 1060, "Error50202", filePath);
                        ErrorText.ValueT = msg.Message;
                        Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(Encoding.GetEncoding)", ErrorText.ValueT);
                        OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAFileCyclicGroupBase", "OpenFile", 1070);
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "PAFileCyclicGroupBase.OpenFile(new StreamWriter)", e.Message);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                return false;
            }

            return true;
        }

        public bool CloseFile()
        {
            if (_StreamWriter == null)
                return true;
            try
            {
                _StreamWriter.Flush();
                _StreamWriter.Close();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAFileCyclicGroupBase", "CloseFile", msg);
            }
            _IsAppendingToStream = false;
            _StreamWriter = null;
            return true;
        }
        #endregion

        #region Abstract
        public abstract Msg DoExport(string exportPath, DateTime fromDate, DateTime toDate);
        #endregion

        #region AlarmHandling
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsExportingAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsExportingAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsExportingAlarm);
            }
            base.AcknowledgeAlarms();
        }

        //protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        //{
        //    if (String.IsNullOrEmpty(newLog.Message))
        //        newLog.Message = ErrorText.ValueT;
        //    base.OnNewMsgAlarmLogCreated(newLog);
        //}
        #endregion

        #endregion
    }
}
