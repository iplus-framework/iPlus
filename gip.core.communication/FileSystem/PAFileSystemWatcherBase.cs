using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace gip.core.communication
{
    /// <summary>
    /// File-System-Watcher
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Filesystem watcher'}de{'Dateisystem Überwacher'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class PAFileSystemWatcherBase : PAClassAlarmingBase, IACWorkCycle
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

#region Properties

        private bool _SubscribedToWorkCycle = false;
        public bool IsSubscribedToWorkCycle
        {
            get
            {
                return _SubscribedToWorkCycle;
            }
        }

        private bool _WithWatcher = false;
        private FileSystemWatcher _Watcher;

        protected SafeList<string> _FilesInProcess = new SafeList<string>();

        [ACPropertyInfo(true, 201, "Configuration", "en{'Path to watch'}de{'Überwachungspfad'}", "", true)]
        public string Path
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 202, "Configuration", "en{'Filter'}de{'Filter'}", "", true)]
        //[DefaultValueAttribute("*.txt")]
        public string Filter
        {
            get;
            set;
        }

        [ACPropertyBindingSource(203, "", "en{'Watching is on'}de{'Überwachung eingeschaltet'}", "", true, true, DefaultValue = true)]
        public IACContainerTNet<Boolean> RunWatching { get; set; }

        [ACPropertyInfo(true, 204, "Configuration", "en{'Trash directory'}de{'Mülleimer Verzeichnis'}", "", true)]
        public string TrashDir
        {
            get;
            set;
        }

        protected string _NetUseArgsSucc = "";
        [ACPropertyInfo(true, 210, "Configuration", "en{'Net use arguments connect'}de{'Netzlaufwerk argumente verbinden'}", "", true)]
        public string NetUseArguments
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 211, "Configuration", "en{'Net use arguments disconnect'}de{'Netzlaufwerk argumente trennen'}", "", true)]
        public string NetUseDeleteArguments
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 212, "Configuration", "en{'Forward to directory'}de{'Weiterleiten in Verzeichnis'}", "", true)]
        public string ForwardDir
        {
            get;
            set;
        }

        [ACPropertyBindingSource(203, "Error", "en{'Watching Alarm'}de{'Überwachungs Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsWatchingAlarm { get; set; }


        [ACPropertyBindingSource(204, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }

        public static new Dictionary<string, ACEventArgs> SVirtualEventArgs
        {
            get { return _SVirtualEventArgs; }
        }

        public override Dictionary<string, ACEventArgs> VirtualEventArgs
        {
            get
            {
                return SVirtualEventArgs;
            }
        }

        protected ManualResetEvent _ShutdownEvent;
        private ACThread _WorkCycleThread;

        #endregion

        #region Constructors

        static PAFileSystemWatcherBase()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PAClassAlarmingBase.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("FullPath", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("Name", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("OldFullPath", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("OldName", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("FileContentInfo", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("WatcherChangeType", typeof(int), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("FileChangedEvent", TMP);
        }

        public PAFileSystemWatcherBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ShutdownEvent = new ManualResetEvent(false);
            _WorkCycleThread = new ACThread(RunWorkCycle);
            _FileChangedEvent = new ACPointEvent(this, "FileChangedEvent", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _WorkCycleThread.Name = "ACUrl:" + this.GetACUrl() + ";RunWorkCycle();";
            _WorkCycleThread.Start();
            return result;
        }

        public override bool ACPostInit()
        {
            StartWatching();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopWatching();

            bool result = base.ACDeInit(deleteACClassTask);

            if (_WorkCycleThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_WorkCycleThread.Join(10000))
                    _WorkCycleThread.Abort();

                _WorkCycleThread = null;
                _ShutdownEvent = null;
            }

            return result;
        }

#endregion

#region Points

        protected ACPointEvent _FileChangedEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent FileChangedEvent
        {
            get
            {
                return _FileChangedEvent;
            }
            set
            {
                _FileChangedEvent = value;
            }
        }

        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "StartWatching":
                    StartWatching();
                    return true;
                case "IsEnabledSubscribeToProjectWorkCycle":
                    result = IsEnabledSubscribeToProjectWorkCycle();
                    return true;
                case "IsEnabledUnSubscribeToProjectWorkCycle":
                    result = IsEnabledUnSubscribeToProjectWorkCycle();
                    return true;
                case "IsEnabledStartWatching":
                    result = IsEnabledStartWatching();
                    return true;
                case "StopWatching":
                    StopWatching();
                    return true;
                case "IsEnabledStopWatching":
                    result = IsEnabledStopWatching();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #region public methods

        [ACMethodInteraction("Watching", "en{'Start Directory-watching'}de{'Starte Verzeichnis-Überwachung'}", 200, true)]
        public void StartWatching()
        {
            StopWatching();
            if (!IsEnabledStartWatching())
                return;

            if (String.IsNullOrEmpty(_NetUseArgsSucc) && !String.IsNullOrEmpty(NetUseArguments))
            {
                try
                {
                    Process p = Process.Start("net.exe", "use " + NetUseArguments);
                    p.WaitForExit();
                    _NetUseArgsSucc = NetUseArguments;
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "ACFileSystemWatcherBase.StartWatching(0):", e.Message);
                }
            }

            // Der File-Systemwatcher wird nicht aktiv, wenn bereits Dateien vorlagen als er instanzieert worden ist,
            // daher muss das Verzeichnis das erste mal durchsucht werden
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(Path))
                {
                    Messages.LogError(this.GetACUrl(), "ACFileSystemWatcherBase.StartWatching(0):", String.Format("Directory {0} doesn't exist.", Path));
                    return;
                }

                if (_WithWatcher)
                {
                    _Watcher = new FileSystemWatcher(Path);
                    if (!String.IsNullOrEmpty(Filter))
                        _Watcher.Filter = Filter;
                    _Watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
                    _Watcher.Created += new FileSystemEventHandler(OnWatcherChanged);
                    _Watcher.Renamed += new RenamedEventHandler(OnWatcherRenamed);
                    _Watcher.Error += new ErrorEventHandler(OnWatcher_Error);
                    _Watcher.EnableRaisingEvents = true;
                }
                else if (IsEnabledSubscribeToProjectWorkCycle())
                {
                    SubscribeToProjectWorkCycle();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAFileSystemWatcherBase", "StartWatching", msg);
            }
            finally 
            { 
            }

        }

        public void SubscribeToProjectWorkCycle()
        {
            if (!IsEnabledSubscribeToProjectWorkCycle())
                return;
            ProjectWorkCycleR5sec += ApplicationManager_ProjectWorkCycleR5sec;
            _SubscribedToWorkCycle = true;
        }

        public bool IsEnabledSubscribeToProjectWorkCycle()
        {
            if (_SubscribedToWorkCycle)
                return false;
            return true;
        }


        public void UnSubscribeToProjectWorkCycle()
        {
            if (!IsEnabledUnSubscribeToProjectWorkCycle())
                return;
            ProjectWorkCycleR5sec -= ApplicationManager_ProjectWorkCycleR5sec;
            _SubscribedToWorkCycle = false;
        }

        public bool IsEnabledUnSubscribeToProjectWorkCycle()
        {
            if (!_SubscribedToWorkCycle)
                return false;
            return true;
        }

        public bool IsEnabledStartWatching()
        {
            if (_WithWatcher && _Watcher != null)
                return false;
            else if (_SubscribedToWorkCycle)
                return false;
            if (String.IsNullOrEmpty(Path))
                return false;
            if (!RunWatching.ValueT)
                return false;
            return true;
        }

        [ACMethodInteraction("Watching", "en{'Stop Directory-watching'}de{'Stoppe Verzeichnis-Überwachung'}", 200, true)]
        public void StopWatching()
        {
            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Created -= OnWatcherChanged;
                _Watcher.Renamed -= OnWatcherRenamed;
            }
            _Watcher = null;
            if (IsEnabledUnSubscribeToProjectWorkCycle())
                UnSubscribeToProjectWorkCycle();

            if (!String.IsNullOrEmpty(_NetUseArgsSucc) && !String.IsNullOrEmpty(NetUseDeleteArguments) && (_NetUseArgsSucc != NetUseArguments))
            {
                try
                {
                    Process p = Process.Start("net.exe", "use " + NetUseDeleteArguments);
                    p.WaitForExit();
                    _NetUseArgsSucc = "";
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "PAFileSystemWatcherBase.StopWatching(0):", e.Message);
                }
            }
        }

        public bool IsEnabledStopWatching()
        {
            if (_WithWatcher && _Watcher == null)
                return false;
            else if (!_SubscribedToWorkCycle)
                return false;
            return true;
        }

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsWatchingAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsWatchingAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsWatchingAlarm);
            }
            base.AcknowledgeAlarms();
        }

        //protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        //{
        //    if (String.IsNullOrEmpty(newLog.Message))
        //        newLog.Message = ErrorText.ValueT;
        //    base.OnNewMsgAlarmLogCreated(newLog);
        //}

        public void ImporterHasProcessed(string fileName)
        {
            _FilesInProcess.Remove(fileName);
        }
        #endregion

        #region protected methods
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
                        Messages.LogException(this.GetACUrl(), "PAFileSystemWatcherBase.MoveOrDeleteFile(0)", e.Message);
                    }
                }
            }
            try
            {
                File.Delete(fromPath);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAFileSystemWatcherBase.MoveOrDeleteFile(1)", e.Message);
            }
        }

        public string FindAndCreateTrashPath(string fileName)
        {
            if (String.IsNullOrEmpty(TrashDir))
                return "";
            return CreateArchivePath(TrashDir, fileName);
        }

        public string CreateArchivePath(string inDirectory, string fileName)
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
                    Messages.LogException(this.GetACUrl(), "PAFileSystemWatcherBase.FindAndCreateArchivePath(0)", e.Message);
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
                    Messages.LogException(this.GetACUrl(), "PAFileSystemWatcherBase.FindAndCreateArchivePath(1)", e.Message);
                    return "";
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
            return System.IO.Path.GetFileName(fileNameWithPath);
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
                string destinationFilePath = System.IO.Path.Combine(destinationDirPath, fileInfo.Name);
                File.Copy(file, destinationFilePath);
                return true;
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "PAXMLDocImporterBase.MoveOrDeleteFile(0)", e);
            }
            return false;
        }

        public event EventHandler ProjectWorkCycleR5sec;

        private void RunWorkCycle()
        {
            try
            {
                while (!_ShutdownEvent.WaitOne(100, false))
                {
                    _WorkCycleThread.StartReportingExeTime();
                    ProjectThreadWakedUpAfter100ms();
                    _WorkCycleThread.StopReportingExeTime();
                }
            }
            catch (ThreadAbortException e)
            {
                Messages.LogException(this.GetACUrl(), "RunWorkCycle()", "Thread abort exception. Thread TERMINATED!!!!");
                if (!String.IsNullOrEmpty(e.Message))
                {
                    Messages.LogException(this.GetACUrl(), "RunWorkCycle()", e.Message);
                    if (e.InnerException != null && !String.IsNullOrEmpty(e.InnerException.Message))
                    {
                        Messages.LogException(this.GetACUrl(), "RunWorkCycle()", e.InnerException.Message);
                    }
                }
            }
        }

        private DateTime? _LastWakeupTime = null;
        private TimeSpan _WakeupAlarm = new TimeSpan(0, 0, 2);
        private int _WakeupCounter = 0;
        internal void ProjectThreadWakedUpAfter100ms()
        {
            _WakeupCounter++;
            DateTime stampStart = DateTime.Now;
            if (_LastWakeupTime.HasValue && (DateTime.Now > (_LastWakeupTime.Value + _WakeupAlarm)))
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Wakeuptime took longer than 2 seconds since {0}", _LastWakeupTime.Value));
            if (_WakeupCounter % 50 == 0)
            {
                if (ProjectWorkCycleR5sec != null)
                    ProjectWorkCycleR5sec(this, new EventArgs());
            }
            DateTime stampFinished = DateTime.Now;
            TimeSpan diff = stampFinished - stampStart;
            if (diff.TotalSeconds >= 5)
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Child-Components consumed more than {0}", diff));
            _LastWakeupTime = stampFinished;
        }

        #endregion

        #region abstract Methods
        protected abstract bool AnalyzeContentBeforeRaising(ACEventArgs eventArgs);

        protected virtual String[] OnSortFilesBeforeProcessing(String[] files)
        {
            try
            {
                String[] files2 = ACUrlCommand("!SortFilesBeforeProcessing", new object[] { files }) as String[];
                if (files2 == null)
                    return files;
                return files2;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAFileSystemWatcherBase", "OnSortFilesBeforeProcessing", msg);
            }
            return files;
        }
        #endregion

        #endregion

        #region Event-Handler
        void OnWatcherChanged(object sender, FileSystemEventArgs e)
        {
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("FileChangedEvent", VirtualEventArgs);

            eventArgs.GetACValue("FullPath").Value = e.FullPath;
            eventArgs.GetACValue("Name").Value = e.Name;
            eventArgs.GetACValue("WatcherChangeType").Value = (int)e.ChangeType;

            Messages.LogDebug(this.GetACUrl(), "ACFileSystemXMLWatcher.OnWatcherChanged()", e.FullPath);
            if (!AnalyzeContentBeforeRaising(eventArgs))
                return;
            FileChangedEvent.Raise(eventArgs);
        }

        void OnWatcherRenamed(object sender, RenamedEventArgs e)
        {
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("FileChangedEvent", VirtualEventArgs);

            eventArgs.GetACValue("FullPath").Value = e.FullPath;
            eventArgs.GetACValue("Name").Value = e.Name;
            eventArgs.GetACValue("OldFullPath").Value = e.OldFullPath;
            eventArgs.GetACValue("OldName").Value = e.OldName;
            eventArgs.GetACValue("WatcherChangeType").Value = (int)e.ChangeType;

            Messages.LogDebug(this.GetACUrl(), "ACFileSystemXMLWatcher.OnWatcherRenamed()", e.FullPath);
            if (!AnalyzeContentBeforeRaising(eventArgs))
                return;
            FileChangedEvent.Raise(eventArgs);
        }

        void OnWatcher_Error(object sender, ErrorEventArgs e)
        {
            IsWatchingAlarm.ValueT = PANotifyState.AlarmOrFault;
            Msg msg = new Msg(e.ToString(), this, eMsgLevel.Error, "PAFileSystemWatcherBase", "OnWatcher_Error", 1000);
            ErrorText.ValueT = msg.Message;
            OnNewAlarmOccurred(IsWatchingAlarm, msg, true);
        }

        // M.Tartsch:  changed to virtual for Josera to allow overwrites with pre, post and idle processing
        protected virtual void ApplicationManager_ProjectWorkCycleR5sec(object sender, EventArgs e)
        {
            try
            {
                if (!this.Root.Initialized)
                    return;

                string filter = Filter;
                if (String.IsNullOrEmpty(filter))
                    filter = "*.*";

                string[] files = Directory.GetFiles(Path, filter);

                if (files.Any())
                {
                    files = OnSortFilesBeforeProcessing(files);
                    foreach (string fileName in files)
                    {
                        if (File.Exists(fileName))
                        {
                            if (_FilesInProcess.Contains(fileName))
                                continue;
                            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("FileChangedEvent", VirtualEventArgs);

                            eventArgs.GetACValue("FullPath").Value = fileName;
                            eventArgs.GetACValue("Name").Value = System.IO.Path.GetFileName(fileName);
                            eventArgs.GetACValue("WatcherChangeType").Value = WatcherChangeTypes.Created;

                            if (!AnalyzeContentBeforeRaising(eventArgs))
                                return;
                            FileChangedEvent.Raise(eventArgs);
                        }
                    }
                }
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                Messages.LogException("PAFileSystemWatcherBase", "ApplicationManager_ProjectWorkCycleR5sec", msg);
            }
        }

        #endregion
    }
}
