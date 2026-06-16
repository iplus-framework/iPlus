using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Linq;
using System.IO;

namespace gip.core.communication
{
    public abstract class PADocImporterBase : PAClassAlarmingBase
    {

        #region c´tors
        public PADocImporterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

        #region Alarm
        [ACPropertyBindingSource(210, "Error", "en{'Import Alarm'}de{'Import Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsImportAlarm { get; set; }

        [ACPropertyBindingSource(211, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }
        #endregion

        #region protected
        protected string _CurrentFileName;
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

        public abstract Type TypeOfDeserialization
        {
            get;
        }

        #endregion

        #endregion

        #region Methods

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
                        Messages.LogException(this.GetACUrl(), "PADocImporterBase.MoveOrDeleteFile(0)", e.Message);
                    }
                }
            }
            try
            {
                File.Delete(fromPath);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PADocImporterBase.MoveOrDeleteFile(1)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PADocImporterBase.FindAndCreateArchivePath(0)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PADocImporterBase.FindAndCreateArchivePath(1)", e.Message);
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
                        Messages.LogException(this.GetACUrl(), "PADocImporterBase.FindAndCreateArchivePath(2)", e.Message);
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
                Messages.LogException(this.GetACUrl(), "PADocImporterBase.ForwardFile()", e);
            }
            return false;
        }

        #endregion

        #endregion
    }
}
