using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Basisklasse für die Root-Klasse bei Anwendungen/Anwendungsdefinitionen
    /// 
    /// In der konkreten Implementierung werden hier Funktionalitäten bereit
    /// gestellt, welche die gesamte Anwendung betreffen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'APP-Manager'}de{'APP-Manager'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class ApplicationManager : ACComponentManager, IACComponentTaskExec, IAppManager 
    {
        #region private Members

        protected ManualResetEvent _ShutdownEvent;
        private ACThread _WorkCycleThread;
        private ACDelegateQueue _ApplicationQueue;

        #endregion

        public ApplicationManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ShutdownEvent = new ManualResetEvent(false);
            _WorkCycleThread = new ACThread(RunWorkCycle);
            _ACUrlRoutingService = new ACPropertyConfigValue<string>(this, "ACUrlRoutingService", ACRoutingService.DefaultServiceACUrl);
        }

        #region Initialisierung
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _TaskInvocationPoint = new ACPointTask(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint.SetMethod = OnSetTaskInvocationPoint;
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _ApplicationQueue = new ACDelegateQueue(this.GetACUrl() + ";AppQueue");
                _ApplicationQueue.StartWorkerThread();
                _WorkCycleThread.Name = "ACUrl:" + this.GetACUrl() + ";RunWorkCycle();";
                _WorkCycleThread.Start();
            }
            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();

            if (AutoRestoreUnboundTargetProp)
            {
                RestoreTargetProp();
            }
            if (_RoutingService == null)
                _RoutingService = ACRoutingService.ACRefToServiceInstance(this);

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_Simulator != null)
            {
                if (_Simulator.IsAttached)
                {
                    _Simulator.Detach();
                    _Simulator.ValueT = null;
                }
            }
            ACRoutingService.DetachACRefFromServiceInstance(this, _RoutingService);
            _RoutingService = null;

            bool result = base.ACDeInit(deleteACClassTask);

            if (_ApplicationQueue != null)
            {
                _ApplicationQueue.StopWorkerThread();

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ApplicationQueue = null;
                }
            }

            if (_WorkCycleThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_WorkCycleThread.Join(10000))
                    _WorkCycleThread.Abort();


                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _WorkCycleThread = null;
                    _ShutdownEvent = null;
                }
            }
            //if (AppContextQueue != null)
            //    AppContextQueue.Work();

            this.ACCompTypeDict.DetachAll();

            using (ACMonitor.Lock(_20015_LockValue))
            {
                this._ACCompTypeDict = null;
            }

            return result;
        }
        #endregion

        #region Points

        [ACPropertyInfo(9999)]
        public string ArchivePath { get; set; }

        protected ACPointTask _TaskInvocationPoint;
        [ACPropertyAsyncMethodPoint(9999, true, 0)]
        public ACPointTask TaskInvocationPoint
        {
            get
            {
                return _TaskInvocationPoint;
            }
        }

        public bool OnSetTaskInvocationPoint(IACPointNetBase point)
        {
            //TaskInvocationPoint.DeQueueInvocationList();
            TaskInvocationPoint.ActivateAllNewInvocations();

            return true;
        }

        #endregion

        #region Properties

        int _LastWorkflowInstanceNo = 0;
        [ACPropertyInfo(true, 999, "ACConfig", "en{'LastWorkflowIntanceNo'}de{'LastWorkflowIntanceNo'}", "", true)]
        public int LastWorkflowInstanceNo
        {
            get
            {
                return _LastWorkflowInstanceNo;
            }
            set
            {
                _LastWorkflowInstanceNo = value;
                OnPropertyChanged("LastWorkflowInstanceNo");
            }
        }

        [ACPropertyBindingSource(201, "Statistics", "en{'Count alarms'}de{'Anzahl Alarme'}", "", true, false)]
        public IACContainerTNet<Int32> CurrentAlarmsInProject { get; set; }

        [ACPropertyInfo(9999, DefaultValue = true)]
        public Boolean AutoRestoreUnboundTargetProp
        {
            get;
            set;
        }

        /// <summary>
        /// Overriden: Returns the Database-Context for Application-Managers (RootDbOpQueue.AppContext)
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                return RootDbOpQueue.AppContext;
            }
        }

        public virtual ACEntityOpQueue<IACEntityObjectContext> AppContextQueue
        {
            get
            {
                return RootDbOpQueue.AppContextQueue;
            }
        }

        public ACDelegateQueue ApplicationQueue
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _ApplicationQueue;
                }
            }
        }

        private ACRef<PAClassSimulator> _Simulator = null;
        public PAClassSimulator Simulator
        {
            get
            {
                if (_Simulator != null)
                    return _Simulator.ValueT;
                PAClassSimulator simulator = this.FindChildComponents<PAClassSimulator>(c => c is PAClassSimulator, null, 1).FirstOrDefault();
                if (simulator == null && !this.Root.Initialized)
                    return null;
                if (simulator != null)
                    _Simulator = new ACRef<PAClassSimulator>(simulator, this);
                else
                    _Simulator = new ACRef<PAClassSimulator>(simulator, null);
                return _Simulator.ValueT;
            }
        }

        public bool IsSimulationOn
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Live)
                    return true;
                PAClassSimulator simulator = Simulator;
                if (simulator == null)
                    return false;
                return simulator.RunSimulation.ValueT;
            }
        }

        public bool IsManualSimulation
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Live)
                    return true;
                PAClassSimulator simulator = Simulator;
                if (simulator == null)
                    return false;
                return simulator.ManualSimulationMode.ValueT;
            }
        }

        [ACPropertyBindingSource(303, "Info", "en{'Planned orders'}de{'Geplante Aufträge'}", "", false, false)]
        public IACContainerTNet<String> PlannedOrdersInfo { get; set; }

        private ACCompTypeDictionary _ACCompTypeDict = new ACCompTypeDictionary();
        public ACCompTypeDictionary ACCompTypeDict
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACCompTypeDict == null)
                        _ACCompTypeDict = new ACCompTypeDictionary();
                    return _ACCompTypeDict;
                }
            }
        }

        private ACCompUrlDictionary _ACCompUrlDict = new ACCompUrlDictionary();
        public ACCompUrlDictionary ACCompUrlDict
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACCompUrlDict == null)
                        _ACCompUrlDict = new ACCompUrlDictionary();
                    return _ACCompUrlDict;
                }
            }
        }

        bool _RoutingServiceTriedToConnect = false;
        protected ACRef<ACComponent> _RoutingService = null;
        public ACComponent RoutingServiceInstance
        {
            get
            {
                if (_RoutingService == null)
                {
                    if (!_RoutingServiceTriedToConnect)
                    {
                        _RoutingServiceTriedToConnect = true;
                        _RoutingService = ACRoutingService.ACRefToServiceInstance(this);
                    }
                    return _RoutingService != null ? _RoutingService.ValueT : null;
                }
                return _RoutingService.ValueT;
            }
        }
        #endregion

        #region Events
        public event EventHandler ProjectWorkCycleR100ms;
        public event EventHandler ProjectWorkCycleR200ms;
        public event EventHandler ProjectWorkCycleR500ms;
        public event EventHandler ProjectWorkCycleR1sec;
        public event EventHandler ProjectWorkCycleR2sec;
        public event EventHandler ProjectWorkCycleR5sec;
        public event EventHandler ProjectWorkCycleR10sec;
        public event EventHandler ProjectWorkCycleR20sec;
        public event EventHandler ProjectWorkCycleR1min;
        public event EventHandler ProjectWorkCycleR2min;
        public event EventHandler ProjectWorkCycleR5min;
        public event EventHandler ProjectWorkCycleR10min;
        public event EventHandler ProjectWorkCycleR20min;
        public event EventHandler ProjectWorkCycleHourly;
        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "InitializeRouteAndConfig":
                    InitializeRouteAndConfig();
                    return true;
                case "RunStateValidation":
                    RunStateValidation();
                    return true;
                case "CheckAndRemoveAllDeadTasks":
                    CheckAndRemoveAllDeadTasks();
                    return true;
                case "ReloadConfig":
                    ReloadConfig((Guid)acParameter[0]);
                    return true;
                case MN_FindMatchingUrls:
                    result = FindMatchingUrls(acParameter[0] as FindMatchingUrlsParam);
                    return true;
                case MN_FindMatchingUrlsSerial:
                    result = FindMatchingUrlsSerial(acParameter[0] as byte[]);
                    return true;
                case MN_GetACComponentACMemberValues:
                    result = GetACComponentACMemberValues(acParameter[0] as Dictionary<string, string>);
                    return true;
                case Const.IsEnabledPrefix + "InitializeRouteAndConfig":
                    result = IsEnabledInitializeRouteAndConfig();
                    return true;
                case Const.IsEnabledPrefix + "RunStateValidation":
                    result = IsEnabledRunStateValidation();
                    return true;
                case Const.IsEnabledPrefix + "CheckAndRemoveAllDeadTasks":
                    result = IsEnabledCheckAndRemoveAllDeadTasks();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Cycle
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
            //try
            //{
            //    StringBuilder desc = new StringBuilder();
            //    StackTrace stackTrace = new StackTrace(_WorkCycleThread.Thread, true);
            //    for (int i = 0; i < stackTrace.FrameCount; i++)
            //    {
            //        StackFrame sf = stackTrace.GetFrame(i);
            //        desc.AppendFormat(" Method: {0}", sf.GetMethod());
            //        desc.AppendFormat(" File: {0}", sf.GetFileName());
            //        desc.AppendFormat(" Line Number: {0}", sf.GetFileLineNumber());
            //        desc.AppendLine();
            //    }

            //    string stackDesc = desc.ToString();
            //    Messages.LogException("App.CurrentDomain_UnhandledException", "Stacktrace", stackDesc);
            //}
            //catch (Exception)
            //{
            //}
        }

        private DateTime? _LastWakeupTime = null;
        private TimeSpan _WakeupAlarm = new TimeSpan(0, 0, 2);
        private int _WakeupCounter = 0;
        internal void ProjectThreadWakedUpAfter100ms()
        {
            if (Root == null || Root.InitState == ACInitState.Destructing || Root.InitState == ACInitState.Destructed)
                return;
            _WakeupCounter++;
            DateTime stampStart = DateTime.Now;
            if (_LastWakeupTime.HasValue && (DateTime.Now > (_LastWakeupTime.Value + _WakeupAlarm)))
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Wakeuptime took longer than 2 seconds since {0}", _LastWakeupTime.Value));
            if (ProjectWorkCycleR100ms != null)
                ProjectWorkCycleR100ms(this, new EventArgs());
            if (_WakeupCounter % 2 == 0)
            {
                if (ProjectWorkCycleR200ms != null)
                    ProjectWorkCycleR200ms(this, new EventArgs());
            }
            if (_WakeupCounter % 5 == 0)
            {
                if (ProjectWorkCycleR500ms != null)
                    ProjectWorkCycleR500ms(this, new EventArgs());
            }
            if (_WakeupCounter % 10 == 0)
            {
                if (ProjectWorkCycleR1sec != null)
                    ProjectWorkCycleR1sec(this, new EventArgs());
            }
            if (_WakeupCounter % 20 == 0)
            {
                if (ProjectWorkCycleR2sec != null)
                    ProjectWorkCycleR2sec(this, new EventArgs());
                //if (this.AppContextQueue != null)
                //    AppContextQueue.Work();
            }
            if (_WakeupCounter % 50 == 0)
            {
                if (ProjectWorkCycleR5sec != null)
                    ProjectWorkCycleR5sec(this, new EventArgs());
            }
            if (_WakeupCounter % 100 == 0)
            {
                if (ProjectWorkCycleR10sec != null)
                    ProjectWorkCycleR10sec(this, new EventArgs());
                ObserveACState();
            }
            if (_WakeupCounter % 200 == 0)
            {
                if (ProjectWorkCycleR20sec != null)
                    ProjectWorkCycleR20sec(this, new EventArgs());
            }
            if (_WakeupCounter % 600 == 0)
            {
                if (ProjectWorkCycleR1min != null)
                    ProjectWorkCycleR1min(this, new EventArgs());
            }
            if (_WakeupCounter % 1200 == 0)
            {
                if (ProjectWorkCycleR2min != null)
                    ProjectWorkCycleR2min(this, new EventArgs());
            }
            if (_WakeupCounter % 3000 == 0)
            {
                if (ProjectWorkCycleR5min != null)
                    ProjectWorkCycleR5min(this, new EventArgs());
            }
            if (_WakeupCounter % 6000 == 0)
            {
                if (ProjectWorkCycleR10min != null)
                    ProjectWorkCycleR10min(this, new EventArgs());
            }
            if (_WakeupCounter % 12000 == 0)
            {
                if (ProjectWorkCycleR20min != null)
                    ProjectWorkCycleR20min(this, new EventArgs());
            }
            if (_WakeupCounter % 36000 == 0)
            {
                if (ProjectWorkCycleHourly != null)
                    ProjectWorkCycleHourly(this, new EventArgs());
                _WakeupCounter = 0;
            }
            DateTime stampFinished = DateTime.Now;
            TimeSpan diff = stampFinished - stampStart;
            if (diff.TotalSeconds >= 5)
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Child-Components consumed more than {0}", diff));
            _LastWakeupTime = stampFinished;
        }
        #endregion

        #region ACState Observing
        [ACPropertyInfo(true, 201, DefaultValue = false)]
        public bool ACStateObservingOn { get; set; }

        public void ObserveACState()
        {
            if (!ACStateObservingOn || !Root.Initialized)
                return;
            this.ApplicationQueue.Add(() => 
            {
                try
                {
                    List<IACWorkCycleWithACState> query = FindChildComponents<IACWorkCycleWithACState>(c => c is IACWorkCycleWithACState && !(c as IACWorkCycleWithACState).IsSubscribedToWorkCycle && (c as IACWorkCycleWithACState).IsACStateInconsistent);
                    if (query.Any())
                    {
                        Messages.LogError(this.GetACUrl(),
                                                    "ObserveACState()",
                                                    String.Format("ACState-Correction initiated for {0} instances", query.Count));
                        query.ForEach(c => c.SubscribeToProjectWorkCycle());
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("ApplicationManager", "ObserveACState", msg);
                }
            });
        }
        #endregion

        #region Misc
        public int GetNextWorkflowInstanceNo()
        {
            LastWorkflowInstanceNo++;
            if (LastWorkflowInstanceNo > 9999)
                LastWorkflowInstanceNo = 0;
            return LastWorkflowInstanceNo;
        }

        protected override void OnSubAlarmChanged(ACEventArgs events, bool childHasAlarms)
        {
            base.OnSubAlarmChanged(events, childHasAlarms);
            CurrentAlarmsInProject.ValueT = CountSubAlarms;
            if(CurrentAlarmsInProject.ValueT > 0)
                RefreshHasAlarms();
        }

        public const string MN_FindMatchingUrlsSerial = "FindMatchingUrlsSerial";
        [ACMethodInfo("", "en{'Find components serialized'}de{'Finde Komponenten serialisiert'}", 301, false)]
        public string[] FindMatchingUrlsSerial(byte[] queryData)
        {
            if (queryData == null)
                return null;
            FindMatchingUrlsParam deserializedQueryParam;
            using (var stream = new MemoryStream(queryData))
            {
                BinaryFormatter bf = new BinaryFormatter();
                deserializedQueryParam = (FindMatchingUrlsParam)bf.Deserialize(stream);
            }
            return FindMatchingUrls(deserializedQueryParam);
        }

        public const string MN_FindMatchingUrls = "FindMatchingUrls";
        [ACMethodInfo("", "en{'Find components'}de{'Finde Komponenten'}", 300, false)]
        public string[] FindMatchingUrls(FindMatchingUrlsParam queryParam)
        {
            if (queryParam == null || queryParam.Query == null) return null;
            return ACCompUrlDict.Where(c => queryParam.Query(c.Value.ValueT)).Select(c => c.Key).ToArray();
        }

        public const string MN_GetACComponentACMemberValues = "GetACComponentACMemberValues";
        [ACMethodInfo("", "en{'Query Propertyvalues from components'}de{'Abfrage Eigenschaftswerte von Komponenten'}", 302, false)]
        public Dictionary<string, object> GetACComponentACMemberValues(Dictionary<string, string> acUrl_AcMemberIdentifiers)
        {
            List<ACComponent> components = ACCompUrlDict.Where(c => acUrl_AcMemberIdentifiers.Keys.Contains(c.Key))
                .Select(c => c.Value.ValueT).ToList();

            return components.Join(acUrl_AcMemberIdentifiers, comp => comp.ACUrl, p => p.Key, (comp, p) => new { comp = comp, p = p })
            .ToDictionary(key => key.comp.ACUrl, val => val.comp.ACMemberList.Any(ml=>ml.ACIdentifier == val.p.Value) ? val.comp.ACMemberList.FirstOrDefault(ml => ml.ACIdentifier == val.p.Value).Value : null);
        }

        #endregion

        #region Routes and Config
        [ACMethodInteraction("xxx", "en{'Initialize Routes and configuration'}de{'Initialisiere Routen und Konfiguration'}", 101, true)]
        public void InitializeRouteAndConfig()
        {
            FindChildComponents<PAProcessModule>(c => c is PAProcessModule).Select(c => c.RouteItemID).ToArray();
            using (Database dbIPlus = new Database())
            {
                var list = FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction).ToArray();
                FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction).ForEach(c => c.InitializeRouteAndConfig(dbIPlus));
            }
        }

        public bool IsEnabledInitializeRouteAndConfig()
        {
            var user = Root.CurrentInvokingUser;
            if (user == null)
                return false;
            return user.IsSuperuser;
        }

        [ACMethodInfo("", "en{'Refresh configuration'}de{'Aktualisiere Konfiguration'}", 102, false)]
        public void ReloadConfig(Guid acMethodID)
        {
            var query = FindChildComponents<PWProcessFunction>(c => c is PWProcessFunction
                                                                && (c as PWProcessFunction).ContentACClassWF != null
                                                                && (c as PWProcessFunction).ContentACClassWF.ACClassMethodID == acMethodID
                                                                , null, 3);
            if (query.Any())
            {
                //ACMonitorObject contextLockForACClassWF = null;
                //List<ACClassWF> allLoadedWFs = new List<ACClassWF>();
                foreach (PWProcessFunction pwFunction in query)
                {
                    pwFunction.ReloadConfig();
                    //if (pwFunction.WFDictionary != null)
                    //{
                    //    foreach (ACClassWF wfClass in pwFunction.WFDictionary.Keys.ToArray())
                    //    {
                    //        if (contextLockForACClassWF == null && pwFunction.ContextLockForACClassWF != null)
                    //            contextLockForACClassWF = pwFunction.ContextLockForACClassWF;
                    //        if (!allLoadedWFs.Contains(wfClass))
                    //            allLoadedWFs.Add(wfClass);
                    //    }
                    //}
                }

                //if (contextLockForACClassWF != null && allLoadedWFs.Any())
                //{
                //    using (ACMonitor.Lock(contextLockForACClassWF))
                //    {
                //        foreach (ACClassWF wfClass in allLoadedWFs)
                //        {
                //            wfClass.AutoRefresh();
                //        }
                //    }
                //}
            }

            ACClassMethod acClassMethodWF = ACClassMethods.FirstOrDefault(c => c.ACClassMethodID == acMethodID);
            if (acClassMethodWF != null)
            {
                using (ACMonitor.Lock(acClassMethodWF.Database.QueryLock_1X000))
                {
                    acClassMethodWF.AutoRefresh();
                }
            }
        }

        [ACMethodInteraction("xxx", "en{'Run State Validation'}de{'Führe Zustandsvalidaierung durch'}", 103, true)]
        public void RunStateValidation()
        {
            FindChildComponents<PAProcessModule>(c => c is PAProcessModule).ForEach(c => c.RunStateValidation());
        }

        public bool IsEnabledRunStateValidation()
        {
            var user = Root.CurrentInvokingUser;
            if (user == null)
                return false;
            return user.IsSuperuser;
        }

        #endregion

        #region IACComponentTaskExec
        public bool ActivateTask(ACMethod acMethod, bool executeMethod, IACComponent executingInstance)
        {
            return ACPointAsyncRMIHelper.ActivateTask(this, acMethod, executeMethod, executingInstance);
        }

        public bool CallbackTask(ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state)
        {
            return ACPointAsyncRMIHelper.CallbackTask(this, acMethod, result, state);
        }

        public bool CallbackTask(IACTask task, ACMethodEventArgs result, PointProcessingState state)
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

        //[ACMethodInfo("", "", 999)]
        //public ACCompUrlDictionary GetACCompUrlDict(Func<KeyValuePair<string, ACRef<ACComponent>>, bool> fnTest)
        //{
        //    // ACCompUrlDictionary ACCompUrlDict
        //    ACCompUrlDictionary dict = new ACCompUrlDictionary();
        //    var query = ACCompUrlDict.Where(c => fnTest(c));
        //    foreach (var item in query)
        //        dict.Add(item.Key, item.Value);
        //    return dict;
        //}

        [ACMethodInteraction("xxx", "en{'Remove all inactive Taskinvocations'}de{'Entferne alle inaktive Taskaufrufe'}", 101, true)]
        public void CheckAndRemoveAllDeadTasks()
        {
            var taskexecutors = FindChildComponents<IACComponentTaskExec>(c => c is IACComponentTaskExec);
            foreach (var taskexec in taskexecutors)
            {
                taskexec.TaskInvocationPoint.CheckAndRemoveAllDeadTasks();
            }
        }

        public bool IsEnabledCheckAndRemoveAllDeadTasks()
        {
            var user = Root.CurrentInvokingUser;
            if (user == null)
                return false;
            return user.IsSuperuser;
        }

        #endregion

        #endregion

        #region Config
        private ACPropertyConfigValue<string> _ACUrlRoutingService;
        [ACPropertyConfig("en{'Routing service ACUrl'}de{'Routing service ACUrl'}")]
        public virtual string ACUrlRoutingService
        {
            get
            {
                return _ACUrlRoutingService.ValueT;
            }
        }

        public string RoutingServiceACUrl { get { return ACUrlRoutingService; } }

        #endregion
    }
}
