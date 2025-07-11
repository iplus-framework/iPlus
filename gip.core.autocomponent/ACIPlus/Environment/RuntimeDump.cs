// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Use this component to investigate performance problems that you add as an instance in an application tree. RuntimeDump may only be instantiated once when an iPlus service is started. Therefore, we recommend adding the instance in the service project with the ACIdentifer "VBDump"
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Dump of iPlus runtime'}de{'Speicherabbild der iPlus Laufzeitumgebung'}", Global.ACKinds.TACRuntimeDump, Global.ACStorableTypes.Required, false, false)]
    public class RuntimeDump : ACComponent, IRuntimeDump
    {
        #region c´tors

        static RuntimeDump()
        {
            RegisterExecuteHandler(typeof(RuntimeDump), HandleExecuteACMethod_RuntimeDump);
        }

        public RuntimeDump(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _WorkCycleThread = new ACThread(RunWorkCycle);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _WorkCycleThread.Name = "ACUrl:" + this.GetACUrl() + ";RunWorkCycle();";
            _WorkCycleThread.Start();

            return true;
        }

        public override bool ACPostInit()
        {
            if (Root == gip.core.autocomponent.ACRoot.SRoot && ParentACComponent is Environment)
            {
                _FileSystemWatcher = new FileSystemWatcher(Root.Environment.Rootpath);
                _FileSystemWatcher.BeginInit();
                _FileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
                //_FileSystemWatcher.Filter = "*.dmp";
                _FileSystemWatcher.Renamed += new RenamedEventHandler(_FileSystemWatcher_Renamed);
                _FileSystemWatcher.EnableRaisingEvents = true;
                _FileSystemWatcher.EndInit();
            }
            //if (ACThread.PerfLogger.LogACState)
            //    ACStateLoggingOn();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (Root == gip.core.autocomponent.ACRoot.SRoot && ParentACComponent is Environment)
            {
                _FileSystemWatcher.EnableRaisingEvents = false;
                _FileSystemWatcher.Renamed -= _FileSystemWatcher_Renamed;
                _FileSystemWatcher = null;
            }

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

        #region Properties
        private FileSystemWatcher _FileSystemWatcher;
        private PerformanceLogger _PerfLogger = new PerformanceLogger("Code-Analysis");

        public PerformanceLogger PerfLogger
        {
            get
            {
                return _PerfLogger;
            }
        }

        private ManualResetEvent _ShutdownEvent = new ManualResetEvent(false);
        private ACThread _WorkCycleThread;


        [ACPropertyInfo(true, 200, DefaultValue = 0)]
        public int PerfTimeoutStackTrace { get; set; }

        [ACPropertyInfo(true, 201, DefaultValue = 0)]
        public int PerfMonitoringTimeout { get; set; }

        /// <summary>
        /// Interval when Performce-Logs shoud be dumped automatically in background.
        /// </summary>
        [ACPropertyInfo(true, 202, DefaultValue = 0)]
        public int PerfDumpIntervalMin { get; set; }

        [ACPropertyInfo(true, 204)]
        public DateTime PerfDumpLastRun { get; set; }

        private object _LockPerfMonLock = new object();
        private string[] _PerfMonIgnoreList = new string[] { };
        protected string[] PerfMonIgnoreList
        {
            get
            {
                lock (_LockPerfMonLock)
                {
                    return _PerfMonIgnoreList;
                }
            }
        }

        private string _PerfMonIgnore;
        [ACPropertyInfo(true, 202)]
        public string PerfMonIgnore 
        {
            get
            {
                return _PerfMonIgnore;
            }
            set
            {
                _PerfMonIgnore = value;
                if (String.IsNullOrEmpty(value))
                {
                    lock (_LockPerfMonLock)
                    {
                        _PerfMonIgnoreList = new string[] { };
                    }
                }
                else
                {
                    lock (_LockPerfMonLock)
                    {
                        try
                        {
                            _PerfMonIgnoreList = value.Split(ACUrlHelper.Delimiter_Translate);
                        }
                        catch (Exception ex)
                        {
                            Messages.LogException(this.GetACUrl(), "PerfMonIgnore.Split()", ex);
                            _PerfMonIgnoreList = new string[] { };
                        }
                    }
                }
            }
        }

        private string[] _PerfMonIncludeList = new string[] { };
        protected string[] PerfMonIncludeList
        {
            get
            {
                lock (_LockPerfMonLock)
                {
                    return _PerfMonIncludeList;
                }
            }
        }

        private string _PerfMonInclude;
        [ACPropertyInfo(true, 202)]
        public string PerfMonInclude
        {
            get
            {
                return _PerfMonInclude;
            }
            set
            {
                _PerfMonInclude = value;
                if (String.IsNullOrEmpty(value))
                {
                    lock (_LockPerfMonLock)
                    {
                        _PerfMonIncludeList = new string[] { };
                    }
                }
                else
                {
                    lock (_LockPerfMonLock)
                    {
                        try
                        {
                            _PerfMonIncludeList = value.Split(ACUrlHelper.Delimiter_Translate);
                        }
                        catch (Exception ex)
                        {
                            Messages.LogException(this.GetACUrl(), "PerfMonInclude.Split()", ex);
                            _PerfMonIncludeList = new string[] { };
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Dump):
                    Dump();
                    return true;
                case nameof(DumpCPU):
                    DumpCPU();
                    return true;
                case nameof(DumpPerfLog):
                    DumpPerfLog();
                    return true;
                //case nameof(ACStateLoggingOn):
                //    ACStateLoggingOn();
                //    return true;
                //case nameof(ACStateLoggingOff):
                //    ACStateLoggingOff();
                //    return true;
                case nameof(PerfLoggingOn):
                    PerfLoggingOn();
                    return true;
                case nameof(PerfLoggingOff):
                    PerfLoggingOff();
                    return true;
                case nameof(DumpDelegateQueues):
                    DumpDelegateQueues();
                    return true;
                case nameof(DumpSubscribedComponents):
                    DumpSubscribedComponents();
                    return true;
                case nameof(RestartDelegateQueue):
                    RestartDelegateQueue(acParameter[0] as string);
                    return true;
                case nameof(IsEnabledDumpCPU):
                    result = IsEnabledDumpCPU();
                    return true;
                case nameof(IsEnabledDumpPerfLog):
                    result = IsEnabledDumpPerfLog();
                    return true;
                //case Const.IsEnabledPrefix + "ACStateLoggingOn):
                //    result = IsEnabledACStateLoggingOn();
                //    return true;
                //case Const.IsEnabledPrefix + "ACStateLoggingOff):
                //    result = IsEnabledACStateLoggingOff();
                //    return true;
                case nameof(IsEnabledPerfLoggingOn):
                    result = IsEnabledPerfLoggingOn();
                    return true;
                case nameof(IsEnabledPerfLoggingOff):
                    result = IsEnabledPerfLoggingOff();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_RuntimeDump(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RestartDelegateQueueC):
                    RestartDelegateQueueC(acComponent);
                    return true;
            }
            return false;
        }
        #endregion


        #region public

        private void RunWorkCycle()
        {
            while (!_ShutdownEvent.WaitOne(500, false))
            {
                try
                {
                    _WorkCycleThread.StartReportingExeTime();
                    MonitorActivePerfEvents();
                    _WorkCycleThread.StopReportingExeTime();
                    CyclicDump();
                }
                catch (Exception e)
                {
                    //ThreadAbortException
                    Messages.LogException(this.GetACUrl(), "RunWorkCycle()", e.GetType().Name);
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
        }

        public PerformanceEvent PerfLoggerStart(string url, int id, bool checkCallStack = false)
        {
            if (PerfLogger == null)
                return null;
            return PerfLogger.Start(url, id, checkCallStack);

        }

        public bool? PerfLoggerStop(string url, int id, PerformanceEvent perfEvent, int perfTimeoutStackTrace = 0)
        {
            bool? bOk = null;
            if (PerfLogger != null)
                bOk = PerfLogger.Stop(url, id, perfEvent);
            else
            {
                perfEvent.Stop();
                bOk = true;
            }
            if (perfTimeoutStackTrace <= 0)
                perfTimeoutStackTrace = this.PerfTimeoutStackTrace;
            if (perfEvent.CalculateTimeout(perfTimeoutStackTrace))
            {
//#if DEBUG
//                if (System.Diagnostics.Debugger.IsAttached)
//                {
//                    System.Diagnostics.Debugger.Break();
//                }
//#endif
                string stackTrace = System.Environment.StackTrace;
                Messages.LogDebug(this.GetACUrl(), "Stop()", String.Format("{0}, Duration: {1}", url, perfEvent.ElapsedMilliseconds.ToString()));
                Messages.LogDebug(this.GetACUrl(), "Stop()", stackTrace);
                bOk = false;
            }
            return bOk;
        }

        private void MonitorActivePerfEvents()
        {
            if (PerfLogger == null || !PerfLogger.Active || PerfMonitoringTimeout <= 0)
                return;
            PerfLogger.MonitorActivePerfEvents(this.PerfMonitoringTimeout, this, PerfMonIgnoreList, PerfMonIncludeList);
        }

        private void CyclicDump()
        {
            if (PerfDumpIntervalMin <= 0)
                return;
            if (PerfDumpLastRun == DateTime.MinValue || DateTime.Now > PerfDumpLastRun.AddMinutes(PerfDumpIntervalMin))
            {
                DumpCPU();
                DumpPerfLog();
                PerfDumpLastRun = DateTime.Now;
            }
        }

#pragma warning disable CS0618
        /// <summary>
        ///   <para>
        /// Create an empty text file called "Invoke_Dump.txt" in the iPlus installation directory or in the directory where the iPlus service is executed.</para>
        ///   <para>The iPlus service starts to output all application trees including the root  tree in the temporary directory. An XML file with the name "RuntimeDump_ProcId_yyyyMMdd_HHmmss.xml" is created. The XML file contains all instances including all property values ​​and private fields as it is also displayed in the diagnostics dialog.</para>
        /// </summary>
        [ACMethodCommand("", "en{'Dump'}de{'Ausgabe Speicherinhalt'}", 9999)]
        public void Dump()
        {
            string xmlDumpFileName = string.Format("{0}RuntimeDump_{1}_{2:yyyyMMdd_HHmmss}.xml", Messages.LogFilePath, Process.GetCurrentProcess().Id.ToString(), DateTime.Now);

            (Root as ACComponent).DumpAsXMLDoc().Save(xmlDumpFileName);
            DumpStackTrace();
        }

        private const string C_IdleMethod = "Int32 WaitOneNative";

        public void DumpStackTrace(Thread ignoreThread = null, bool ignoreIdleThreads = false)
        {
            StackTrace st = new StackTrace(true);
            string trace = st.ToString();
            Messages.LogDebug(this.GetACUrl(), "RuntimeDump.Dump(StackTrace)", trace);

            //Solution https://github.com/dotnet/runtime/issues/80555#issuecomment-1386817390

            //foreach (ACThread thread in ACThread.ACThreadList)
            //{
            //    if (ignoreThread != null && thread.Thread == ignoreThread)
            //        continue;
            //    StringBuilder builder = new StringBuilder();
            //    thread.Suspend();
            //    st = new StackTrace(thread.Thread, true);
            //    //trace = st.ToString();
            //    string stackIndent = "";
            //    for (int i = 0; i < st.FrameCount; i++)
            //    {
            //        StackFrame sf = st.GetFrame(i);
            //        if (ignoreIdleThreads && i == 0)
            //        {
            //            string methodName = sf.GetMethod().ToString();
            //            if (!String.IsNullOrEmpty(methodName) && methodName.StartsWith(C_IdleMethod))
            //                break;
            //        }
            //        builder.AppendLine(stackIndent + "Method: " + sf.GetMethod());
            //        builder.AppendLine(stackIndent + "File: " + sf.GetFileName());
            //        builder.AppendLine(stackIndent + "Line: " + sf.GetFileLineNumber());
            //        stackIndent += "  ";
            //    }
            //    if (!String.IsNullOrEmpty(stackIndent))
            //        Messages.LogDebug(this.GetACUrl(), "RuntimeDump.Dump(StackTrace)", String.Format("Thread: {0}, Trace {1}", thread.Name, builder.ToString()));

            //    thread.Resume();
            //}
        }
#pragma warning restore CS0618

        /// <summary>
        ///   <para>
        ///  This command creates a file with the name "UsageCPU_YYYYMMDD_hhmmss.txt" in the temporary directory (%USERPROFILE%\AppData\Local\Temp) where the message log is also stored.</para>
        ///   <para>This file outputs statistics about all threads that were created with the "gip.core.datamodel.ACThread" class. Therefore, you should only use the ACThread class for thread programming.</para>
        /// </summary>
        [ACMethodInteraction("", "en{'Dump CPU usage'}de{'Dump CPU usage'}",100, true)]
        public void DumpCPU()
        {
            if (!ACThread.PerfLogger.Active)
                return;

            DateTime now = DateTime.Now;
            List<IPerfLogWriter> dumpWriter = FindChildComponents<IPerfLogWriter>();
            if (dumpWriter != null && dumpWriter.Any())
            {
                // First, get the performance data without resetting
                var perfData = ACThread.PerfLogger.GetPerformanceData();
                if (perfData != null)
                {
                    foreach (IPerfLogWriter writer in dumpWriter)
                    {
                        writer.WritePerformanceData(now, perfData, ACThread.PerfLogger);
                    }
                }
            }

            string dumpFilePath = string.Format("{0}UsageCPU_{1:yyyyMMdd_HHmmss}.txt", Messages.LogFilePath, now);
            string log = ACThread.DumpStatisticsAndReset();
            if (log != null)
                File.WriteAllText(dumpFilePath, log);
        }

        public bool IsEnabledDumpCPU()
        {
            return ACThread.PerfLogger.Active;
        }


        /// <summary>
        ///   <para>
        ///  This command creates a file with the name "PerfLog_YYYYMMDD_hhmmss.txt" in the temporary directory (%USERPROFILE%\AppData\Local\Temp) where the message log is also stored.</para>
        ///   <para>This file contains usage statistics for all instances that have status-dependent methods. Status-dependent methods are called cyclically by activation via "PABase .SubscribeToProjectWorkCycle()". The execution time of a status-dependent method is logged and output in this performance log.</para>
        /// </summary>
        [ACMethodInteraction("", "en{'Dump Performancelog'}de{'Dump Performancelog'}", 101, true)]
        public void DumpPerfLog()
        {
            if (!PerfLogger.Active)
                return;

            DateTime now = DateTime.Now;
            List<IPerfLogWriter> dumpWriter = FindChildComponents<IPerfLogWriter>();
            if (dumpWriter != null && dumpWriter.Any())
            {
                // First, get the performance data without resetting
                var perfData = PerfLogger.GetPerformanceData();
                if (perfData != null)
                {
                    foreach (IPerfLogWriter writer in dumpWriter)
                    {
                        writer.WritePerformanceData(now, perfData, PerfLogger);
                    }
                }
            }

            string dumpFilePath = string.Format("{0}PerfLog_{1:yyyyMMdd_HHmmss}.txt", Messages.LogFilePath, now);
            string log = PerfLogger.DumpAndReset();
            if (log != null)
                File.WriteAllText(dumpFilePath, log);
        }

        public bool IsEnabledDumpPerfLog()
        {
            return PerfLogger.Active;
        }


        /// <summary>This enables performance logging. To activate it permanently when the service starts,  add the PerfLogConfiguration section in the App.Config file.</summary>
        [ACMethodInteraction("", "en{'Enable Performance-logging'}de{'Enable Performance-logging'}", 102, true)]
        public void PerfLoggingOn()
        {
            ACThread.PerfLogger.Active = true;
            PerfLogger.Active = true;
        }

        public bool IsEnabledPerfLoggingOn()
        {
            return !ACThread.PerfLogger.Active;
        }

        /// <summary>This disables performance logging. To activate it permanently when the service starts,  add the PerfLogConfiguration section in the App.Config file.</summary>
        [ACMethodInteraction("", "en{'Disable Performance-logging'}de{'Disable Performance-logging'}", 103, true)]
        public void PerfLoggingOff()
        {
            ACThread.PerfLogger.Active = false;
            PerfLogger.Active = false;
        }

        public bool IsEnabledPerfLoggingOff()
        {
            return ACThread.PerfLogger.Active;
        }

        //[ACMethodInteraction("", "en{'Enable ACState logging'}de{'Enable ACState logging'}", 104, true)]
        //public void ACStateLoggingOn()
        //{
        //    ACProperty<ACStateEnum>.LogACState = true;
        //}

        //public bool IsEnabledACStateLoggingOn()
        //{
        //    return !ACProperty<ACStateEnum>.LogACState;
        //}

        //[ACMethodInteraction("", "en{'Disable ACState logging'}de{'Disable ACState logging'}", 105, true)]
        //public void ACStateLoggingOff()
        //{
        //    ACProperty<ACStateEnum>.LogACState = false;
        //}

        //public bool IsEnabledACStateLoggingOff()
        //{
        //    return ACProperty<ACStateEnum>.LogACState;
        //}


        /// <summary>
        ///   <para>
        ///  This command creates a file with the name "DelegateQueue_YYYYMMDD_hhmmss.txt" in the temporary directory (%USERPROFILE%\AppData\Local\Temp) where the message log is also stored. </para>
        ///   <para>This file contains statistics on the status and the degree of utilization of all delegate queues.</para>
        /// </summary>
        [ACMethodInteraction("", "en{'Dump Delegate-Queues'}de{'Dump Delegate-Queues'}", 110, true)]
        public void DumpDelegateQueues()
        {
            string dumpFilePath = string.Format("{0}DelegateQueue_{1}.txt", Messages.LogFilePath, DateTime.Now.ToString().Replace('.', '-').Replace(':', '_'));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("========== Delegate-Queue-Dump ==========");
            sb.AppendLine("");
            List<ACThread> activeQueues = new List<ACThread>();
            var queues = ACThread.ACThreadList.Where(c => c.ParentQueue != null);
            foreach (ACThread threadOfQueue in queues.Where(c => c.ParentQueue != null).OrderBy(c => c.Name))
            {
                int queueCount = 0;
                try
                {
                    queueCount = threadOfQueue.ParentQueue.UnsafeDelegateQueueCount;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("RuntimeDump", "DumpDelegateQueues", msg);
                }
                sb.AppendLine(String.Format("ID: {0}, State: {1}, Last run: {2}, Idle time: {3}, Elements in Queue: {4}",
                                            threadOfQueue.Name, threadOfQueue.ThreadState,
                                            threadOfQueue.ParentQueue.LastRun, threadOfQueue.ParentQueue.IdleTime, queueCount));
                if (queueCount > 0)
                {
                    activeQueues.Add(threadOfQueue);
                }
            }
            sb.AppendLine("");
            
            if (activeQueues.Any())
            {
                sb.AppendLine("========== Potentially deadlocked queues ==========");
                foreach (ACThread threadToCheckForDeadlock in activeQueues)
                {
                    if (threadToCheckForDeadlock.ParentQueue.IdleTime.TotalMilliseconds > (threadToCheckForDeadlock.ParentQueue.WorkerInterval_ms * 10))
                    {
                        sb.AppendLine(String.Format("ID: {0}, State: {1}, Last run: {2}, Idle time: {3}",
                                                    threadToCheckForDeadlock.Name, threadToCheckForDeadlock.ThreadState,
                                                    threadToCheckForDeadlock.ParentQueue.LastRun, threadToCheckForDeadlock.ParentQueue.IdleTime));
                    }
                }
            }
            File.WriteAllText(dumpFilePath, sb.ToString());
        }

        [ACMethodInfo("", "en{'Restart Delegate-Queue'}de{'Restart Delegate-Queue'}", 9999)]
        public void RestartDelegateQueue(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return;
            bool abortThread = id.StartsWith("(Abort)");
            if (abortThread)
                id = id.Replace("(Abort)", "");
            var threadOfQueue = ACThread.ACThreadList.Where(c => c.ParentQueue != null && c.Name == id).FirstOrDefault();
            if (threadOfQueue == null)
                return;
            threadOfQueue.ParentQueue.RestartQueue(abortThread);
        }

        #region Client-Methods
        [ACMethodInteractionClient("", "en{'Restart Delegate-Queue'}de{'Restart Delegate-Queue'}", 111, false, "", false)]
        public static void RestartDelegateQueueC(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            string id = _this.Messages.InputBox("Enter ID of Delegate-Queue for restarting", "");
            if (String.IsNullOrWhiteSpace(id))
                return;
            _this.ACUrlCommand("!RestartDelegateQueue", id);
        }
        #endregion


        /// <summary>
        /// This command creates a file with the name "SubscribedComp_YYYYMMDD_hhmmss.txt" in the temporary directory (%USERPROFILE%\AppData\Local\Temp) where the message log is also stored. This file lists all instances whose status-dependent methods are called cyclically because they were activated using "PABase .SubscribeToProjectWorkCycle()".
        /// </summary>
        [ACMethodInteraction("", "en{'Dump all subscribed components'}de{'Dump all subscribed components'}", 120, true)]
        public void DumpSubscribedComponents()
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    var appManagers = (this.Root as ACRoot).FindChildComponents<ApplicationManager>(c => c is ApplicationManager, null, 1);
                    foreach (var appManager in appManagers)
                    {
                        var query = appManager.FindChildComponents<IACWorkCycle>(c => c is IACWorkCycle && (c as IACWorkCycle).IsSubscribedToWorkCycle)
                            .Select(c => new { ACUrl = c.GetACUrl(), ACState = (c is IACWorkCycleWithACState) ? (c as IACWorkCycleWithACState).CurrentACState : ACStateEnum.SMIdle })
                            .ToList();
                        query.ForEach(c => sb.AppendLine(String.Format("{0}; {1};", c.ACUrl, c.ACState)));
                    }
                    string dumpFilePath = string.Format("{0}SubscribedComp_{1:yyyyMMdd_HHmmss}.txt", Messages.LogFilePath, DateTime.Now);
                    File.WriteAllText(dumpFilePath, sb.ToString());
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("RuntimeDump", "DumpSubscribedComponents", msg);
                }
            });
        }

#endregion

#region Event-Delegates
        void _FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.Name.EndsWith("Invoke_Dump.txt"))
                Dump();

            else if(e.Name.EndsWith("Invoke_Dump_CPU.txt"))
                DumpCPU();
        }

        void _FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith("Invoke_Dump.txt"))
                Dump();

            else if (e.Name.EndsWith("Invoke_Dump_CPU.txt"))
                DumpCPU();
        }

#endregion

#endregion

    }
}
