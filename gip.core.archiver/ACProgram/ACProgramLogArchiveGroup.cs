// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using gip.core.communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using gip.core.autocomponent;
using Microsoft.EntityFrameworkCore;

namespace gip.core.archiver
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACProgramLog archive'}de{'ACProgramLog archive'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ACProgramLogArchiveGroup : PAFileCyclicGroupBase
    {
        #region c'tors

        public ACProgramLogArchiveGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _DelegateQueue = new ACDelegateQueue(this.GetACUrl());
            _DelegateQueue.StartWorkerThread();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_DelegateQueue != null)
            {
                _DelegateQueue.StopWorkerThread();
                _DelegateQueue = null;
            }
            return base.ACDeInit(deleteACClassTask);
        }


        #endregion

        #region Const
        public const string ClassName = nameof(ACProgramLogArchiveGroup);
        public const string PropNameExportAlarm = nameof(IsExportingAlarm);

        private const string C_RemoveRefFromPropertyLog =
                            "UPDATE proplog"
                            + "SET proplog.ACProgramLogID = null"
                            + "FROM ACPropertyLog AS proplog"
                            + "INNER JOIN ACProgramLog proglog ON proglog.ACProgramLogID = proplog.ACProgramLogID"
                            + "INNER JOIN ACProgram prog ON prog.ACProgramID = proglog.ACProgramID"
                            + "WHERE prog.ACProgramID = '{0}'";

        private const string C_RemoveParentRefsOfProgramLog =
                            "UPDATE ACProgramLog SET ParentACProgramLogID = null WHERE ACProgramID = {0}";

        private const string C_DeleteProgramLogs =
                    "DELETE FROM ACProgramLog WHERE ACProgramID = {0}";

        #endregion

        #region Properties

        private int _ArchiveAfterDays;
        [ACPropertyInfo(true, 301, "Configuration", "en{'Archive logs older than -X- days'}de{'Archiviere Protokolle älter als -X- Tage'}", "", true, DefaultValue = 30)]
        public int ArchiveAfterDays
        {
            get
            {
                return _ArchiveAfterDays;
            }
            set
            {
                _ArchiveAfterDays = value;
                OnPropertyChanged("ArchiveAfterDays");
            }
        }

        private bool _IsArchvingActive = false;
        protected bool IsArchivingActive
        {
            get
            {
                return _IsArchvingActive;
            }
            set
            {
                _IsArchvingActive = value;
            }
        }

        protected ACMonitorObject IsArchivingActiveLock = new ACMonitorObject(50100);

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {
                return _DelegateQueue;
            }
        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RestoreArchivedProgramLog):
                    result = RestoreArchivedProgramLog(acParameter[0] as String);
                    return true;
                    case nameof(RestoreArchivedProgramLogs):
                    result = RestoreArchivedProgramLogs((DateTime)acParameter[0]);
                    return true;
                case nameof(ArchiveProgramLogManual):
                    result = ArchiveProgramLogManual((DateTime)acParameter[0], acParameter[1] as String);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Methods

        public override datamodel.Msg DoExport(string exportPath, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(exportPath))
            {
                //Error50129:Export path is empty!
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "DoExport", 121, "Error50129");
                AddAlarm(PropNameExportAlarm, msg);
                return null;
            }

            Messages.LogInfo(ClassName, "DoExport", "ACProgramLogArchiveGroup - export is invoked.");


            using (ACMonitor.Lock(IsArchivingActiveLock))
            {
                // Check if automatic archiving is now running.
                if (IsArchivingActive)
                    return null;

            }

            DelegateQueue.Add(() => ArchivePrograms(exportPath));

            return null;
        }

        public void ArchivePrograms(string exportPath)
        {
            List<Guid> programsList;


            using (ACMonitor.Lock(IsArchivingActiveLock))
                IsArchivingActive = true;

            try
            {
                Messages.LogInfo(ClassName, "ArchivePrograms", "Automatic archiving is started.");
                using (Database db = new Database())
                {
                    DateTime endDate = DateTime.Now.Subtract(TimeSpan.FromDays(ArchiveAfterDays));
                    programsList = db.ACProgram.Where(c => c.ACProgramLog_ACProgram.Any(x => x.ACProgramLog1_ParentACProgramLog == null
                                                                              && x.EndDate.HasValue && x.EndDate.Value < endDate)
                                                                              && !c.ACClassTask_ACProgram.Any()).Select(x => x.ACProgramID).ToList();
                }
                foreach (Guid acProgramID in programsList)
                {
                    using (Database db = new Database())
                    {
                        ACProgram acProgram = db.ACProgram.FirstOrDefault(c => c.ACProgramID == acProgramID);
                        ArchiveProgram(exportPath, acProgram, db);
                        MsgWithDetails msg = db.ACSaveChanges();
                        if (msg != null)
                        {
                            AddAlarm(PropNameExportAlarm, msg);
                            Messages.LogError(GetACUrl(), "ArchivePrograms(10)", msg.Message);
                            Messages.LogError(GetACUrl(), "ArchivePrograms(11)", msg.InnerMessage);
                        }
                    }
                }
                Messages.LogInfo(ClassName, "ArchivePrograms", "Automatic archiving is finished.");
            }
            catch (Exception e)
            {
                Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "", 181);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msgEc += " Inner:" + e.InnerException.Message;

                Messages.LogException(ClassName, "DoExport(10)", msgEc);
            }
            finally
            {
                using (ACMonitor.Lock(IsArchivingActiveLock))
                    IsArchivingActive = false;
            }
        }

        public void ArchiveProgram(string exportPath, ACProgram acProgram, Database db)
        {
            string acProgramFolderName = OnProgramLogArchive(acProgram, exportPath);
            if (acProgramFolderName == null)
                return;

            ArchiveMsgAlarmLog(acProgram, acProgramFolderName, db);

            ArchiveProgramLog(acProgramFolderName, acProgram, db);
            if (!File.Exists(acProgramFolderName + ".zip"))
                ZipFile.CreateFromDirectory(acProgramFolderName, acProgramFolderName + ".zip");
            if (Directory.Exists(acProgramFolderName) && exportPath != acProgramFolderName)
                Directory.Delete(acProgramFolderName, true);
        }

        private void ArchiveProgramLog(string exportPath, ACProgram acProgram, Database db)
        {
            ACProgramLog parentLog = acProgram.ACProgramLog_ACProgram.FirstOrDefault(c => c.ACProgramLog1_ParentACProgramLog == null);
            if (parentLog == null)
            {
                //Error50130:Can't find the root ACProgramLog!
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveProgramLog", 220, "Error50130");
                AddAlarm(PropNameExportAlarm, msg);
                return;
            }
            string programLogUrl = parentLog.ACUrl.Replace("\\", "_");

            string filePath = string.Format("{0}\\ACProgramLog({1}).xml", exportPath, programLogUrl);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                //AddAlarm("ArchivePrograms(1)", string.Format("File {0} already exist!!!", filePath));
                //return;
            }

            List<ACProgramLog> acProgramLogList = acProgram.ACProgramLog_ACProgram.ToList();
            try
            {
                acProgramLogList.ForEach(c => c.GetObjectContext().Detach(c));
            }
            catch (Exception ec)
            {
                //Error50131: Detaching ACProgramLog from database context is fail! Error message:{0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveProgramLog", 242, "Error50131", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ArchiveProgramLog(10)", msgEc);

                return;
            }
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<ACProgramLog>));

            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                try
                {
                    serializer.WriteObject(fs, acProgramLogList);
                }
                catch (Exception ec)
                {
                    //Error50132: Program log serialization is fail! Error message:{0} 
                    Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveProgramLog", 264, "Error50132", ec.Message);
                    AddAlarm(PropNameExportAlarm, msg);

                    string msgEc = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msgEc += " Inner:" + ec.InnerException.Message;

                    Messages.LogException(ClassName, "ArchiveProgramLog(20)", msgEc);

                    return;
                }
            }

            try
            {
                //db.ExecuteStoreCommand(C_RemoveRefFromPropertyLog, acProgram.ACProgramID);
                db.Database.ExecuteSqlRaw(C_RemoveParentRefsOfProgramLog, acProgram.ACProgramID);
                db.Database.ExecuteSqlRaw(C_DeleteProgramLogs, acProgram.ACProgramID);
            }
            catch (Exception ec)
            {
                //Error50133: Program log archive is fail! Error message:{0} 
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveProgramLog", 285, "Error50133", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ArchiveProgramLog(30)", msgEc);

                return;
            }
        }

        protected virtual string OnProgramLogArchive(ACProgram acProgram, string exportPath)
        {
            return CreateACProgramDirectory(acProgram, exportPath);
        }

        protected virtual string CreateACProgramDirectory(IACObject acProgram, string exportPath)
        {
            string exportPathWithDate = exportPath;
            ACProgram acProg = acProgram as ACProgram;
            if (acProg == null)
            {
                //Error50134:Can't determine ACProgram folder name!
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "CreateACProgramDirectory", 310, "Error50134");
                AddAlarm(PropNameExportAlarm, msg);
                return null;
            }

            exportPathWithDate = exportPathWithDate + "\\" + acProg.InsertDate.Year;
            if (!Directory.Exists(exportPathWithDate))
                Directory.CreateDirectory(exportPathWithDate);

            exportPathWithDate = string.Format("{0}\\{1:00}", exportPathWithDate, acProg.InsertDate.Month);
            if (!Directory.Exists(exportPathWithDate))
                Directory.CreateDirectory(exportPathWithDate);

            string acProgramFolderName = exportPathWithDate + "\\ACProgram(" + acProg.ProgramNo + ")";
            if (File.Exists(acProgramFolderName + ".zip"))
            {
                File.Delete(acProgramFolderName + ".zip");
            }
            if (!Directory.Exists(acProgramFolderName))
                Directory.CreateDirectory(acProgramFolderName);
            return acProgramFolderName;
        }

        protected void DeleteProgramLog(ACProgram acProgram, Database db)
        {
            try
            {
                //db.ExecuteStoreCommand(C_RemoveRefFromPropertyLog, acProgram.ACProgramID);
                db.Database.ExecuteSqlRaw(C_RemoveParentRefsOfProgramLog, acProgram.ACProgramID);
                db.Database.ExecuteSqlRaw(C_DeleteProgramLogs, acProgram.ACProgramID);
                MsgWithDetails msg = db.ACSaveChanges();
                if (msg != null)
                {
                    AddAlarm(PropNameExportAlarm, msg);
                    Messages.LogError(GetACUrl(), "DeleteProgramLog(10)", msg.Message);
                    Messages.LogError(GetACUrl(), "DeleteProgramLog(11)", msg.InnerMessage);
                }
            }
            catch (Exception ec)
            {
                //Error50133: Program log archive is fail! Error message:{0} 
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "DeleteProgramLog", 346, "Error50133", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "DeleteProgramLog(10)", msgEc);

                return;
            }
        }

        [ACMethodInfo("", "en{'Restore program log from archive'}de{'Programmablaufprotokoll-Wiederherstellung aus dem Archiv'}", 400)]
        public string RestoreArchivedProgramLog(string programNo)
        {
            bool _isActive = false;

            using (ACMonitor.Lock(IsArchivingActiveLock))
                _isActive = IsArchivingActive;

            if (_isActive)
                return "Warning50018";

            DelegateQueue.Add(() => RestoreProgramLog(programNo, null));
            
            return null;
        }

        [ACMethodInfo("", "en{'Restore program log from archive'}de{'Programmablaufprotokoll-Wiederherstellung aus dem Archiv'}", 400)]
        public string RestoreArchivedProgramLogs(DateTime acProgramInsertMonth)
        {
            bool _isActive = false;

            using (ACMonitor.Lock(IsArchivingActiveLock))
                _isActive = IsArchivingActive;

            if (_isActive)
                return "Warning50018";

            DelegateQueue.Add(() => RestoreProgramLog(null, acProgramInsertMonth));

            return null;
        }

        private void RestoreProgramLog(string programNo, DateTime? acProgramInsertMonth)
        {
            PAFileCyclicExport paFileCyclicExport = ParentACComponent as PAFileCyclicExport;
            if (paFileCyclicExport == null)
                return;

            if(!string.IsNullOrEmpty(programNo))
            {
                string programName = string.Format("ACProgram({0})", programNo);
                string programNameZip = string.Format("{0}.zip", programName);
                string programPath = Directory.GetFiles(paFileCyclicExport.Path, programNameZip, SearchOption.AllDirectories).FirstOrDefault();
                if(!File.Exists(programPath))
                {
                    //Error50212: Restore ACProgram: Can't find the file with path: {0}
                    Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "FindProgramLogInArchive", 411, "Error50212", programPath);
                    AddAlarm(PropNameExportAlarm, msg);
                    return;
                }

                ProcessACProgramArchiveFile(programPath);
                CleanUpAfterRestore(programPath, paFileCyclicExport.Path);
            
            }
            else if(acProgramInsertMonth.HasValue)
            {
                string pathWithDate = string.Format("{0}\\{1}\\{2:00}", paFileCyclicExport.Path, acProgramInsertMonth.Value.Year, acProgramInsertMonth.Value.Month);
                if (!Directory.Exists(pathWithDate))
                {
                    //Error50213: Restore ACProgram: Can't find the directory with path: {0}
                    Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "FindProgramLogInArchive", 426, "Error50213", pathWithDate);
                    AddAlarm(PropNameExportAlarm, msg);
                    return;
                }

                string[] targetFilePaths = Directory.GetFiles(pathWithDate, "ACProgram(*).zip");
                foreach (var targetFilePath in targetFilePaths)
                {
                    ProcessACProgramArchiveFile(targetFilePath);
                    CleanUpAfterRestore(targetFilePath, paFileCyclicExport.Path);
                }
            }
        }

        protected void ProcessACProgramArchiveFile(string acProgramFilePath)
        {
            string acProgramDirPath = acProgramFilePath.Replace(".zip", "");
            try
            {
                if (!Directory.Exists(acProgramDirPath))
                {
                    Directory.CreateDirectory(acProgramDirPath);
                    ZipFile.ExtractToDirectory(acProgramFilePath, acProgramDirPath);
                }

                string[] archive = Directory.GetFiles(acProgramDirPath);
                string programLogPath = archive.FirstOrDefault(c => c.Contains(nameof(ACProgramLog)));

                if (string.IsNullOrEmpty(programLogPath))
                {
                    //Error50210:Restore ACProgram: Can't find ACProgramLog path!
                    Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ProcessACProgramArchiveFile", 457, "Error50210");
                    AddAlarm(PropNameExportAlarm, msg);
                    return;
                }

                DataContractSerializer serializer = new DataContractSerializer(typeof(List<ACProgramLog>));
                using (FileStream fs = File.Open(programLogPath, FileMode.Open))
                {
                    List<ACProgramLog> programs = serializer.ReadObject(fs) as List<ACProgramLog>;
                    using (Database db = new Database())
                    {
                        foreach (ACProgramLog log in programs)
                        {
                            if (!db.ACProgramLog.Any(c => c.ACProgramLogID == log.ACProgramLogID))
                            {
                                if (String.IsNullOrEmpty(log.XMLConfig))
                                    log.ACProperties.Serialize();
                                db.ACProgramLog.Add(log);
                            }
                        }
                        MsgWithDetails msg = db.ACSaveChanges();
                        if (msg != null)
                        {
                            AddAlarm(PropNameExportAlarm, msg);
                            Messages.LogError(GetACUrl(), "ProcessACProgramArchiveFile(9)", msg.Message);
                            Messages.LogError(GetACUrl(), "ProcessACProgramArchiveFile(10)", msg.InnerMessage);
                            return;
                        }
                    }
                }

                RestoreMsgAlarmLogFromArchive(acProgramDirPath);
            }
            catch (Exception ec)
            {
                //Error50211: Program log deserialization is fail! Error message: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ProcessACProgramArchiveFile", 485, "Error50211", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ProcessACProgramArchiveFile(10)", msgEc);

                return;
            }
        }

        protected void CleanUpAfterRestore(string acProgramFilePath, string defaultArchivePath)
        {
            string folderPath = acProgramFilePath.Replace(".zip", "");

            if (Directory.Exists(folderPath) && folderPath != defaultArchivePath)
                Directory.Delete(folderPath, true);
            if (File.Exists(acProgramFilePath) && acProgramFilePath != defaultArchivePath)
                File.Delete(acProgramFilePath);
        }

        [ACMethodInfo("", "en{'Archive program log'}de{'Archiviere Programmablaufprotokoll'}", 5100)]
        public string ArchiveProgramLogManual(DateTime acProgramInsertDate, string acProgramNo)
        {
            bool _isActive = false;

            using (ACMonitor.Lock(IsArchivingActiveLock))
                _isActive = IsArchivingActive;

            if (_isActive)
                return "Warning50018";

            bool isArchiveable = false;
            using (var db = new Database())
            {
                isArchiveable = db.ACProgram.Where(c => c.ProgramNo == acProgramNo && !c.ACClassTask_ACProgram.Any()).Any();
            }
            if (!isArchiveable)
                return "Warning50019";

            DelegateQueue.Add(() => ArchiveProgramLog(acProgramInsertDate, acProgramNo));
            return null;

        }

        private void ArchiveProgramLog(DateTime acProgramInsertDate, string acProgramNo)
        {
            PAFileCyclicExport paFileCyclicExport = ParentACComponent as PAFileCyclicExport;
            if (paFileCyclicExport == null)
                return;

            string pathWithDate = String.Format("{0}\\{1}\\{2:00}", paFileCyclicExport.Path, acProgramInsertDate.Year, acProgramInsertDate.Month);
            ACProgram acProgram = null;

            using (Database db = new datamodel.Database())
            {
                acProgram = db.ACProgram.FirstOrDefault(c => c.ProgramNo == acProgramNo);
                if (acProgram == null)
                {
                    //Error50214: Archive ProgramLog: Can't find a ACProgram!
                    Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "AcrhiveProgramLogManual", 550, "Error50214");
                    AddAlarm(PropNameExportAlarm, msg);
                    return;
                }
            
                ArchiveProgram(paFileCyclicExport.Path, acProgram, db);
                MsgWithDetails msg1 = db.ACSaveChanges();
                if (msg1 != null)
                {
                    AddAlarm(PropNameExportAlarm, msg1);
                    Messages.LogError(GetACUrl(), "ArchiveProgramLog(10)", msg1.Message);
                    Messages.LogError(GetACUrl(), "ArchiveProgramLog(11)", msg1.InnerMessage);
                }
            }
        }

        protected void AddAlarm(string name, Msg msg)
        {
            IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
            OnNewAlarmOccurred(name, msg);
        }

        private void ArchiveMsgAlarmLog(ACProgram acProgram, string exportPath, Database db)
        {
            if (exportPath == null)
                return;

            List<MsgAlarmLog> alarmLogList = db.MsgAlarmLog.Where(c => c.ACProgramLog != null && c.ACProgramLog.ACProgramID == acProgram.ACProgramID).ToList();
            if (!alarmLogList.Any())
                return;

            string filePath = string.Format("{0}\\MsgAlarmLog.xml", exportPath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            try
            {
                foreach (var item in alarmLogList)
                    db.Detach(item);
            }
            catch (Exception ec)
            {
                //Error50220: Detaching MsgAlarmLog from database context is fail!!! Error message: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveMsgAlarmLog", 164, "Error50220", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ArchiveOrderLog(0)", msgEc);

                return;
            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(List<MsgAlarmLog>));
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Create))
                {
                    serializer.WriteObject(fs, alarmLogList);
                }
            }
            catch (Exception ec)
            {
                //Error50221: MsgAlarmLog serialization is fail! Error message: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveMsgAlarmLog", 187, "Error50221", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ArchiveMsgAlarmLogLog(10)", msgEc);

                return;
            }

            try
            {
                db.Database.ExecuteSqlRaw("delete MsgAlarmLog from MsgAlarmLog al inner join ACProgramLog pl on pl.ACProgramLogID = al.ACProgramLogID inner join ACProgram p on p.ACProgramID = pl.ACProgramID where pl.ACProgramID = {0}", acProgram.ACProgramID);
            }
            catch (Exception ec)
            {
                //Error50222: MsgAlarmLog archive is fail! Error message: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "ArchiveMsgAlarmLog", 206, "Error50222", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "ArchiveMsgAlarmLog(20)", msgEc);

                return;
            }
        }

        private void RestoreMsgAlarmLogFromArchive(string acProgramFilePath)
        {
            string msgAlarmLogFilePath = string.Format("{0}\\MsgAlarmLog.xml", acProgramFilePath);

            if (!File.Exists(msgAlarmLogFilePath))
                return;

            try
            {
                DataContractSerializer serializerOrder = new DataContractSerializer(typeof(List<MsgAlarmLog>));
                using (FileStream fs = File.Open(msgAlarmLogFilePath, FileMode.Open))
                {
                    List<MsgAlarmLog> logs = serializerOrder.ReadObject(fs) as List<MsgAlarmLog>;
                    using (Database db = new Database())
                    {
                        foreach (MsgAlarmLog log in logs)
                        {
                            if (!db.MsgAlarmLog.Any(c => c.ACProgramLogID == log.ACProgramLogID))
                                db.MsgAlarmLog.Add(log);
                        }
                        Msg msg = db.ACSaveChanges();
                        if (msg != null)
                        {
                            AddAlarm(PropNameExportAlarm, msg);
                            return;
                        }
                    }
                }
            }
            catch (Exception ec)
            {
                //Error50223: MsgAlarmLog deserialization is fail! Error message: {0}
                Msg msg = new Msg(this, eMsgLevel.Error, ClassName, "FindProgramLogInArchive", 314, "Error50223", ec.Message);
                AddAlarm(PropNameExportAlarm, msg);

                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException(ClassName, "FindProgramLogInArchive(10)", msgEc);

                return;
            }
        }

        #endregion
    }
}
