using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;
using System.Threading;

namespace gip.core.autocomponent
{
    /// <summary>
    /// A PWProcessFunction represents the root node of a loaded workflow. 
    /// The ACIdentifier of the instance is supplemented with a aequential instance number for each new loaded workflow that is dynamically added to the application tree. 
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBase" />
    /// <seealso cref="gip.core.datamodel.IACComponentPWGroup" />
    /// <seealso cref="gip.core.datamodel.IACComponentProcessFunction" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Root of a Workflow'}de{'Wurzel eines Workflows'}", Global.ACKinds.TPAProcessFunction, Global.ACStorableTypes.Optional, true, true)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] {ACProgram.ClassName, Global.ParamOption.Required, typeof(Guid)},
            new object[] {ACProgramLog.ClassName, Global.ParamOption.Optional, typeof(Guid)}
        }
    )]
    public abstract class PWProcessFunction : PWBase, IACComponentPWGroup, IACComponentProcessFunction
    {
        public const string PWClassName = "PWProcessFunction";

        #region c´tors

        static PWProcessFunction()
        {
            RegisterExecuteHandler(typeof(PWProcessFunction), HandleExecuteACMethod_PWProcessFunction);
        }

        public PWProcessFunction(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override void Construct(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            _WFDictionary = new WFDictionary();
            base.Construct(acType, content, parentACObject, parameter, acIdentifier);
        }

        protected override void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
            if (ContentACClassWF != null && WFInitPhase == WFInstantiatonPhase.NewWF_Creating && ContentTask == null && ApplicationManager != null)
            {
                //int suggestedWFINo = 0;
                //suggestedWFINo = ApplicationManager.GetNextWorkflowInstanceNo();
                int minInstanceNo = -1;
                int maxInstanceNo = -1;
                GetMaxInstanceNoOfChilds(ContentACClassWF.ACIdentifier, out minInstanceNo, out maxInstanceNo);
                if (maxInstanceNo < 0)
                    maxInstanceNo = 0;
                //if (minInstanceNo < 0)
                //    minInstanceNo = 0;

                using (ACMonitor.Lock(_20016_NewInstancesLock))
                {
                    List<NewPWInstance> newInstances = null;
                    if (!_NewInstancesCreating.TryGetValue(ParentACComponent, out newInstances))
                    {
                        newInstances = new List<NewPWInstance>();
                        _NewInstancesCreating.Add(ParentACComponent, newInstances);
                    }
                    if (newInstances.Any())
                    {
                        foreach (var newInst in newInstances.ToArray())
                        {
                            if (newInst.IsExpired)
                            {
                                newInstances.Remove(newInst);
                                continue;
                            }
                            if (maxInstanceNo <= -1)
                                maxInstanceNo = newInst._NewInstanceNo;
                            else if (newInst._NewInstanceNo > maxInstanceNo)
                                maxInstanceNo = newInst._NewInstanceNo;
                            if (minInstanceNo <= -1)
                                minInstanceNo = newInst._NewInstanceNo;
                            else if (newInst._NewInstanceNo < minInstanceNo)
                                minInstanceNo = newInst._NewInstanceNo;
                        }
                    }
                    maxInstanceNo++;
                    acIdentifier = ContentACClassWF.ACIdentifier + "(" + maxInstanceNo + ")";
                    NewPWInstance newInstance = new NewPWInstance() { _HashCode = this.GetHashCode(), _NewInstanceNo = maxInstanceNo };
                    newInstances.Add(newInstance);
                }
            }
            else if (ContentTask != null)
            {
                acIdentifier = ContentTask.ACIdentifier;
            }
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (WFDictionary != null)
                PWContentTaskHelper.InitWFChilds(this, WFDictionary);

            if (ConsistencyCheckWF && WFInitPhase >= WFInstantiatonPhase.NewWF_Creating)
            {
                int retries = 0;
                while (ACClassTaskQueue.TaskQueue.IsBusy && retries < 100)
                {
                    Thread.Sleep(ACClassTaskQueue.TaskQueue.WorkerInterval_ms);
                    retries++;
                }
                // Prüfe ob Speicherung erfolgreich
                if (!IsWorkflowConsistent)
                {
                    Messages.LogError(this.GetACUrl(), "ACInit(IsWorkflowConsistent)", "Workflow not consistent");
                    return false;
                }
            }

            return true;
        }

        public override bool ACPostInit()
        {
            RemoveMeFromNewInstances();
            return base.ACPostInit();
        }

        public override void OnInitFailed(Exception reason)
        {
            RemoveMeFromNewInstances();
            base.OnInitFailed(reason);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            RemoveMeFromNewInstances();

            if (WFDictionary != null)
            {
                WFDictionary.DetachAll();
                _WFDictionary = null;
            }
            var parentProgramLog = _ParentProgramLog;
            _ParentProgramLog = null;
            bool baseResult = base.ACDeInit(deleteACClassTask);
            if (baseResult)
                PWContentTaskHelper.DeInitContent(this, deleteACClassTask);
            CurrentACMethod.ValueT = null;

            SetCurrentProgramLog(null, deleteACClassTask);
            // Detach CurrentACProgram only if ParentWorkflow isn't active, because when an new Batch will be started, then an ne ACProgramLog will be generated and automatically ACProgram added as a new object
            // Afterwards ACSavesChanged will come back with a PrimaryKey-Error 'PK_ACProgram', that the ACProgram exists already
            SetCurrentACProgram(null, deleteACClassTask && parentProgramLog == null);

            _CurrentTask = null;
            _DelegateStartingToWorkCycle = false;

            using (ACMonitor.Lock(_20015_LockStoreList))
            {
                _MandatoryConfigStores = null;
            }

            return baseResult;
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            using (ACMonitor.Lock(_20015_LockStoreList))
            {
                _MandatoryConfigStores = null;
            }
            base.Recycle(content, parentACObject, parameter, acIdentifier);
        }

        private static bool? _ConsistencyCheckWF;
        public static bool ConsistencyCheckWF
        {
            get
            {
                if (!_ConsistencyCheckWF.HasValue)
                    return true;
                return _ConsistencyCheckWF.Value;
            }
            set
            {
                if (_ConsistencyCheckWF.HasValue)
                    return;
                _ConsistencyCheckWF = value;
            }
        }
        #endregion

        #region ConfigPoint
        public ACProgramConfig Applicationdata
        {
            get
            {
                if (CurrentACProgram == null)
                    return null;
                return CurrentACProgram.Applicationdata;
            }
        }

        #endregion


        #region Properties

        #region Program, Node

        ACProgram _CurrentACProgram;
        /// <summary>
        /// Gets the current program.
        /// </summary>
        /// <value>
        /// The current ac program.
        /// </value>
        [ACPropertyInfo(9999, "ACConfig")]
        public ACProgram CurrentACProgram
        {
            get
            {
                return GetCurrentACProgram(true);
            }
        }


        /// <summary>
        /// Only for internal usage! Use property CurrentACProgram.
        /// </summary>
        /// <param name="attach">if set to <c>true</c> [attach].</param>
        /// <returns></returns>
        internal ACProgram GetCurrentACProgram(bool attach)
        {
            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentACProgram != null || !attach)
                    return _CurrentACProgram;
            }

            ACProgram currentACProgram = null;
            if (ContentTask != null)
            {
                ACClassTaskQueue.TaskQueue.ProcessAction(() => { currentACProgram = ContentTask.ACProgram; });
                ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(currentACProgram.ACProgramID);
            }
            if (currentACProgram == null && (CurrentTask != null || CurrentACMethod.ValueT != null))
            {
                ACValue acValue = null;
                // Falls asynchron aufgerufen
                if (CurrentTask != null)
                    acValue = CurrentTask.ACMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                // Sons synchron aufgerufen
                else
                    acValue = CurrentACMethod.ValueT.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (acValue != null && acValue.Value != null)
                {
                    Guid acProgramID = (Guid)acValue.Value;
                    if (acProgramID != Guid.Empty)
                    {
                        currentACProgram = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                    }
                }
                if (currentACProgram == null)
                {
                    acValue = CurrentTask.Parameter.GetACValue(ACProgram.ClassName);
                    if (acValue != null && acValue.Value != null)
                    {
                        Guid acProgramID = (Guid)acValue.Value;
                        if (acProgramID != Guid.Empty)
                        {
                            currentACProgram = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                        }
                    }
                }
            }

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentACProgram == null && currentACProgram != null)
                    _CurrentACProgram = currentACProgram;
                return _CurrentACProgram;
            }
        }


        /// <summary>
        /// Only for internal usage! Use property CurrentACProgram.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="detach">if set to <c>true</c> [detach].</param>
        private void SetCurrentACProgram(ACProgram value, bool detach)
        {
            ACProgram currentACProgram;

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                currentACProgram = _CurrentACProgram;
            }

            if (detach && currentACProgram != null && currentACProgram != value)
            {
                ACClassTaskQueue.TaskQueue.ProgramCache.RemoveProgram(currentACProgram);
            }


            using (ACMonitor.Lock(this._20015_LockValue))
            {
                _CurrentACProgram = value;
            }
        }


        /// <summary>The virtual method that this function is currently processing.</summary>
        /// <value>ACMethod</value>
        [ACPropertyBindingSource(999, "ACConfig", "en{'Current Method'}de{'Aktuelle Methode'}", "", false, true)]
        public IACContainerTNet<ACMethod> CurrentACMethod { get; set; }

        ACPointAsyncRMIWrap<ACComponent> _CurrentTask = null;
        /// <summary>
        /// CurrentTask is set if the Workflow was asynchronously started by AddTask. AddTask calls the Start-Method where this Property is set.
        /// </summary>
        [ACPropertyInfo(true, 999, "ACConfig", "en{'Current Task'}de{'Aktueller Task'}", "", true)]
        public ACPointAsyncRMIWrap<ACComponent> CurrentTask
        {
            get
            {
                return _CurrentTask;
            }
            set
            {
                _CurrentTask = value;
                if (_CurrentTask != null && CurrentACMethod.ValueT != _CurrentTask.ACMethod)
                    CurrentACMethod.ValueT = _CurrentTask.ACMethod;
                OnPropertyChanged("CurrentTask");
            }
        }

        /// <summary>
        /// Gets the ParentACComponent as IACComponentTaskExec
        /// </summary>
        /// <value>
        /// The parent task execute comp.
        /// </value>
        public IACComponentTaskExec ParentTaskExecComp
        {
            get
            {
                return ParentACComponent as IACComponentTaskExec;
            }
        }

        /// <summary>
        /// Start-Node that the functions starts to start the processing of this workflow
        /// </summary>
        public PWNodeStart PWNodeStart
        {
            get
            {
                var query = this.ACComponentChilds.Where(c => c is PWNodeStart);
                if (query.Count() != 1)
                    return null;
                return query.First() as PWNodeStart;
            }
        }

        #endregion

        #region Private members

        private class NewPWInstance
        {
            private DateTime _ExpiresAt = DateTime.Now.AddSeconds(30);
            public int _HashCode;
            public int _NewInstanceNo;
            public bool IsExpired
            {
                get
                {
                    return DateTime.Now > _ExpiresAt;
                }
            }
        }

        private static ACMonitorObject _20016_NewInstancesLock = new ACMonitorObject(20016);

        private static Dictionary<IACComponent, List<NewPWInstance>> _NewInstancesCreating = new Dictionary<IACComponent, List<NewPWInstance>>();
        private void RemoveMeFromNewInstances()
        {
            using (ACMonitor.Lock(_20016_NewInstancesLock))
            {
                List<NewPWInstance> newInstances = null;
                var component = ParentACComponent;
                if (component != null)
                {
                    if (_NewInstancesCreating.TryGetValue(component, out newInstances))
                    {
                        int thisHashCode = GetHashCode();
                        var thisNewInstance = newInstances.Where(c => c._HashCode == thisHashCode).FirstOrDefault();
                        if (thisNewInstance != null)
                            newInstances.Remove(thisNewInstance);
                        if (!newInstances.Any())
                            _NewInstancesCreating.Remove(component);
                    }
                }
            }
        }

        private WFDictionary _WFDictionary = new WFDictionary();
        /// <summary>
        /// Dictionary for all Child-Workflow-Instances that are stored under the ACClassWF as key.
        /// </summary>
        public WFDictionary WFDictionary
        {
            get
            {
                return _WFDictionary;
            }
        }
        private bool _DelegateStartingToWorkCycle = false;

        #endregion

        #region ACProgram
        public override ACProgramLog CurrentProgramLog
        {
            get
            {
                return GetCurrentProgramLog(CurrentACState != ACStateEnum.SMIdle && !_DelegateStartingToWorkCycle);
            }
        }

        protected override ACProgramLog GetCurrentProgramLog(bool attach)
        {
            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentProgramLog != null)
                    return _CurrentProgramLog;
            }

            ACProgramLog currentProgramLog = null;
            if (attach)
            {
                ACProgramLog parentProgramLog = ParentProgramLog;
                // Falls Workflow durch anderen Workflow gestartet, dann existiert parentProgramLog
                if (parentProgramLog != null)
                {
                    string acUrl = this.GetACUrl();
                    currentProgramLog = ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(parentProgramLog, acUrl);
                }
                // Sonst Workflow is Root
                else if (CurrentACProgram != null)
                {
                    string acUrl = this.GetACUrl();
                    currentProgramLog = ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(CurrentACProgram, acUrl);
                }
            }

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                if (_CurrentProgramLog == null && currentProgramLog != null)
                    _CurrentProgramLog = currentProgramLog;
                return _CurrentProgramLog;
            }
        }


        protected override void SetCurrentProgramLog(ACProgramLog value, bool detach)
        {
            ACProgramLog programLog;

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                programLog = _CurrentProgramLog;
            }


            if (detach && programLog != null && programLog != value)
            {
                ACClassTaskQueue.TaskQueue.ProgramCache.RemoveProgramLog(programLog);
            }

            if (value != null)
                ACClassTaskQueue.TaskQueue.ProgramCache.AddProgramLog(value, programLog == null);
            else if (detach && programLog == null)
            {
                var acProgram = GetCurrentACProgram(false);
                if (acProgram != null)
                {
                    string acUrl = GetACUrl();
                    if (!String.IsNullOrEmpty(acUrl))
                        ACClassTaskQueue.TaskQueue.ProgramCache.RemoveProgramLog(acProgram, acUrl);
                }
            }

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                _CurrentProgramLog = value;
            }
        }

        internal override string DumpReasonCurrentPLIsNull()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("CurrentACState: {0} @ {1}", this.CurrentACState, this.GetACUrl()));
            if (_CurrentProgramLog == null)
            {
                sb.AppendLine(String.Format("=> PWProcessFunction._CurrentProgramLog is null at {0}", this.GetACUrl()));
                ACProgramLog parentProgramLog = ParentProgramLog;
                if (parentProgramLog == null)
                {
                    sb.AppendLine(String.Format("=> ParentProgramLog is null at {0}", this.GetACUrl()));
                    if (CurrentACProgram == null)
                        sb.AppendLine(String.Format("=> CurrentACProgram is null at {0}", this.GetACUrl()));
                    else
                        sb.AppendLine("ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(CurrentACProgram) not succeeded");
                }
                else
                    sb.AppendLine("ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(parentProgramLog) not succeeded");
            }
            return sb.ToString();
        }

        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override IEnumerable<ACProgramLog> PreviousProgramLogs
        {
            get
            {
                IEnumerable<ACProgramLog> previousLogs = new ACProgramLog[] { };
                ACProgramLog parentProgramLog = ParentProgramLog;
                if (parentProgramLog == null)
                    parentProgramLog = PreviousParentProgramLog; // NO-CHANGE-TRACKING!

                string acUrl = this.GetACUrl();
                if (parentProgramLog != null)
                {
                    previousLogs = ACClassTaskQueue.TaskQueue.ProgramCache.GetPreviousLogsFromParentLog(parentProgramLog.ACProgramLogID, acUrl);
                }
                // Sonst Workflow is Root
                else if (CurrentACProgram != null)
                {
                    previousLogs = ACClassTaskQueue.TaskQueue.ProgramCache.GetPreviousLogsFromProgram(CurrentACProgram.ACProgramID, acUrl);
                }

                return previousLogs;
            }
        }


        private ACProgramLog _ParentProgramLog;
        public override ACProgramLog ParentProgramLog
        {
            get
            {
                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    if (_ParentProgramLog != null)
                        return _ParentProgramLog;
                }

                ACProgramLog parentProgramLog = ParentProgramLogFromPABase;

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    if (_ParentProgramLog == null && parentProgramLog != null)
                        _ParentProgramLog = parentProgramLog;
                    if (_ParentProgramLog != null)
                        return _ParentProgramLog;
                }

                if (CurrentACMethod.ValueT == null)
                    return null;
                // Hole ProgramLog vom ACClassTask-Datenbankkontext
                ACValue acValue = CurrentACMethod.ValueT.ParameterValueList.GetACValue(ACProgramLog.ClassName);
                if (acValue != null && acValue.Value != null)
                {
                    Guid acProgramLogID = (Guid)acValue.Value;
                    if (acProgramLogID != Guid.Empty)
                    {
                        parentProgramLog = ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLogByLogID(acProgramLogID);
                    }
                }

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    if (_ParentProgramLog == null && parentProgramLog != null)
                        _ParentProgramLog = parentProgramLog;
                    return _ParentProgramLog;
                }
            }
        }

        public override ACProgramLog PreviousParentProgramLog
        {
            get
            {
                return null;
            }
        }

        internal ACProgramLog ParentProgramLogFromPABase
        {
            get
            {
                if (CurrentTask == null || !CurrentTask.IsObjLoaded)
                    return null;
                PABase paBase = CurrentTask.ValueT as PABase;
                if (paBase == null)
                    return null;
                return paBase.CurrentProgramLog;
            }
        }
        #endregion

        #region Substate

        /// <summary>
        /// A state property in order to be able to save further sub-states in addition to the ACState property.
        /// </summary>
        [ACPropertyBindingSource(9999, "", "en{'Substate of a process object'}de{'Unterzustand eines Prozessobjekts'}", "", false, true)]
        public IACContainerTNet<uint> ACSubState { get; set; }

        /// <summary>
        /// A state property in order to be able to save further sub-states in addition to the ACState property.
        /// </summary>
        public uint CurrentACSubState
        {
            get
            {
                if (ACSubState == null)
                    return 0;
                return ACSubState.ValueT;
            }
            set
            {
                ACSubState.ValueT = value;
            }
        }
        #endregion

        #region IACConfigMethodHierarchy
        public override string PreValueACUrl
        {
            get
            {
                IACConfigURL invokingWorkflow = InvokingWorkflow;
                if (invokingWorkflow != null)
                {
                    if (!String.IsNullOrEmpty(invokingWorkflow.PreValueACUrl))
                        return invokingWorkflow.PreValueACUrl + invokingWorkflow.ConfigACUrl + "\\";
                    else
                        return invokingWorkflow.ConfigACUrl + "\\";
                }

                return "";
            }
        }

        public IACConfigURL InvokingWorkflow
        {
            get
            {
                if (CurrentTask != null)
                {
                    return CurrentTask.ValueT as IACConfigURL;
                }
                return null;
            }
        }

        public override List<ACClassMethod> ACConfigMethodHierarchy
        {
            get
            {
                List<ACClassMethod> methods = new List<ACClassMethod>();
                IACComponentPWNode invoker = null;
                if (this.ContentACClassWF != null)
                {
                    ACClassMethod acClassMethod = null;
                    using (ACMonitor.Lock(this.ContextLockForACClassWF))
                    {
                        acClassMethod = ContentACClassWF.ACClassMethod;
                    }

                    methods.Add(acClassMethod);
                    if (CurrentTask != null && CurrentTask.ValueT != null)
                    {
                        invoker = CurrentTask.ValueT as IACComponentPWNode;
                        if (invoker != null && invoker.ContentACClassWF != null)
                        {
                            PWProcessFunction pwFunction = invoker.ParentRootWFNode as PWProcessFunction;
                            if (pwFunction != null)
                            {
                                var parentHierarchy = pwFunction.ACConfigMethodHierarchy;
                                if (parentHierarchy != null && parentHierarchy.Any())
                                    methods.AddRange(parentHierarchy);
                            }
                            //acClassMethod = null;
                            //using (ACMonitor.Lock(this.ContextLockForACClassWF))
                            //{
                            //    acClassMethod = invoker.ContentACClassWF.ACClassMethod;
                            //}
                            //methods.Add(acClassMethod);
                        }
                    }
                }
                int i = 1;
                foreach (ACClassMethod method in methods)
                {
                    method.OverridingOrder = i;
                    i++;
                }
                return methods;
            }
        }
        #endregion

        #region IACConfigStoreSelection

        public readonly ACMonitorObject _20015_LockStoreList = new ACMonitorObject(20015);
        protected List<IACConfigStore> _MandatoryConfigStores;
        public override List<IACConfigStore> MandatoryConfigStores
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockStoreList))
                {
                    if (_MandatoryConfigStores != null)
                        return _MandatoryConfigStores.ToList();
                }
                var mandatoryConfigStores = new List<IACConfigStore>();

                mandatoryConfigStores.AddRange(ACConfigMethodHierarchy.Select(x => x as IACConfigStore));

                IACComponentPWNode invoker = null;
                if (this.ContentACClassWF != null)
                {
                    if (RootPW != null && CurrentTask != null && CurrentTask.ValueT != null)
                    {
                        invoker = CurrentTask.ValueT as IACComponentPWNode;
                    }
                }

                if (CurrentACProgram != null)
                    mandatoryConfigStores.Add(CurrentACProgram);

                using (ACMonitor.Lock(_20015_LockStoreList))
                {
                    _MandatoryConfigStores = mandatoryConfigStores;
                    return _MandatoryConfigStores.ToList();
                }
            }
        }

        [ACMethodInteraction("Process", "en{'Reload Configuration'}de{'Aktualisiere Konfiguration'}", 310, true)]
        public virtual void ReloadConfig()
        {
            using (ACMonitor.Lock(_20015_LockStoreList))
            {
                var configStores = MandatoryConfigStores;
                if (configStores != null)
                    configStores.ForEach(c => c.ClearCacheOfConfigurationEntries());
                _MandatoryConfigStores = null;
            }
            // Read cache again
            MandatoryConfigStores.ToArray();
            this.HasRules.ValueT = 0;
            FindChildComponents<IACMyConfigCache>(c => c is IACMyConfigCache).ForEach(c => c.ClearMyConfiguration());
        }

        #endregion

        #region Mapping
        public IEnumerable<PAProcessModule> AllPossibleModules
        {
            get
            {
                return FindChildComponents<PWGroup>(c => c is PWGroup).SelectMany(c => c.PossibleModuleList).Distinct();
            }
        }
        #endregion

        #endregion

        #region Methods

        #region ACState

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.SMIdle:
                    SMIdle();
                    return true;
                case ACStateConst.SMStarting:
                    SMStarting();
                    return true;
                case ACStateConst.SMRunning:
                    SMRunning();
                    return true;
                case ACStateConst.SMCompleted:
                    SMCompleted();
                    return true;
                case ACStateConst.SMPausing:
                    SMPausing();
                    return true;
                case ACStateConst.SMPaused:
                    SMPaused();
                    return true;
                case ACStateConst.SMAborting:
                    SMAborting();
                    return true;
                case ACStateConst.TMReset:
                    Reset();
                    return true;
                case ACStateConst.TMPause:
                    Pause();
                    return true;
                case ACStateConst.TMResume:
                    Resume();
                    return true;
                case ACStateConst.TMAbort:
                    Abort();
                    return true;
                case "ReStart":
                    ReStart();
                    return true;
                case "ResetSubState":
                    ResetSubState();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMReset:
                    result = IsEnabledReset();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMPause:
                    result = IsEnabledPause();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMResume:
                    result = IsEnabledResume();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMAbort:
                    result = IsEnabledAbort();
                    return true;
                case Const.IsEnabledPrefix + "ReStart":
                    result = IsEnabledReStart();
                    return true;
                case Const.IsEnabledPrefix + "ResetSubState":
                    result = IsEnabledResetSubState();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWProcessFunction(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        public override void Reset()
        {
            if (!IsEnabledReset())
                return;

            var lastState = CurrentACState;
            ACSubState.ValueT = 0;

            base.Reset();

            if (lastState == ACStateEnum.SMIdle)
                UnloadWorkflow();
        }

        /// <summary>
        /// StateMode: Starten des Prozesses  (aktiv)
        /// 
        /// Mögliche Folge-ACState:
        /// -SMRunning   Wenn Parametrisierung okay, den Prozess starten
        /// -SMIdle      Wenn Parametrisierung nicht okay, ACMethod.MethodState auf "Rejected" beendet wird
        /// </summary>
        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public virtual void SMStarting()
        {
            //if (!PreExecute(PABaseState.SMStarting))
            //    return;

            RecalcTimeInfo();
            var alreadyCreated = GetCurrentProgramLog(false);
            if (CreateNewProgramLog(CurrentACMethod.ValueT, alreadyCreated == null) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                return;

            // Entkoppelung der Aufrufe bze. Reduktion des Callstack, damit nicht Funktionen beliebig Unterfunktionen im selen Thread aufrufen
            if (_DelegateStartingToWorkCycle)
            {
                SubscribeToProjectWorkCycle();
                _DelegateStartingToWorkCycle = false;
                return;
            }
            if (!CanRunWorkflow())
                return;
            UnSubscribeToProjectWorkCycle();

            PWNodeStart.Start();
            //base.SMStarting();
            //if (!CallbackOnSMStarting())
            //return;

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                CurrentACState = ACStateEnum.SMRunning;
                //PostExecute(PABaseState.SMStarting);
            }

            ////if (!PreExecute(PABaseState.SMStarting))
            ////    return;

            //if (!CallbackOnSMStarting())
            //    return;

            //RecalcTimeInfo();
            //CreateNewProgramLog(CurrentACMethod.ValueT);

            //if (CurrentACState == PABaseState.SMStarting) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            //{
            //    if (ACOperationMode == ACOperationModes.Live)
            //        CurrentACState = PABaseState.SMRunning;
            //    else
            //        CurrentACState = PABaseState.SMCompleted;
            //    //    PostExecute(PABaseState.SMStarting);
            //}
        }

        protected virtual bool CanRunWorkflow()
        {
            return true;
        }

        protected virtual bool CallbackOnSMStarting()
        {
            // Parameter müssen gültig sein und der ACMethodState muss auf Planned stehen
            if (CurrentTask != null && !CurrentTask.ACMethod.IsValid())
            {
                if (ParentTaskExecComp != null)
                {
                    if (ParentTaskExecComp.CallbackTask(CurrentTask, CreateNewMethodEventArgs(CurrentTask.ACMethod, Global.ACMethodResultState.Failed)))
                        CurrentTask = null;
                }
                if (CurrentACState == ACStateEnum.SMStarting) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                {
                    Reset();
                    //CurrentACState = PABaseState.SMIdle;
                }
                return false;
            }
            else if (CurrentACMethod.ValueT != null && !CurrentACMethod.ValueT.IsValid())
            {
                if (ParentTaskExecComp != null)
                    ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.Failed));
                if (CurrentACState == ACStateEnum.SMStarting) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                {
                    Reset();
                    //CurrentACState = PABaseState.SMIdle;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// StateMode: Laufender Prozess  (aktiv)
        /// 
        /// Mögliche Folge-ACState:
        /// -SMCompleted    Prozess erfolgreich beendet
        /// -SMAborted      Prozess abgebrochen
        /// </summary>
        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public virtual void SMRunning()
        {
            ////if (!PreExecute(PABaseState.SMRunning))
            ////    return;

            //if (CyclicWaitIfSimulationOn())
            //    return;

            //CurrentACState = PABaseState.SMCompleted;
            ////PostExecute(PABaseState.SMRunning);

            //if (!PreExecute(PABaseState.SMRunning))
            //    return;
            //PostExecute(PABaseState.SMRunning);
            //ACState = PABaseState.SMCompleted;  Wird vom PWNodeEnd gesetzt
        }

        /// <summary>
        /// StateMode: Prozess erfolgreich beendet  (aktiv)
        /// </summary>
        [ACMethodState("en{'Completed'}de{'Beendet'}", 40, true)]
        public virtual void SMCompleted()
        {
            //if (!PreExecute(PABaseStateConst.SMCompleted))
            //    return;
            RecalcTimeInfo();
            if (CurrentACMethod.ValueT != null)
                FinishProgramLog(CurrentACMethod.ValueT);
            if (ParentTaskExecComp != null)
            {
                if (CurrentTask != null)
                {
                    if (ParentTaskExecComp.CallbackTask(CurrentTask, CreateNewMethodEventArgs(CurrentTask.ACMethod, Global.ACMethodResultState.Succeeded)))
                        CurrentTask = null;
                }
                else if (CurrentACMethod.ValueT != null)
                {
                    ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.Succeeded));
                }
            }
            if (IsACStateMethodConsistent(ACStateEnum.SMCompleted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
                //CurrentACState = PABaseState.SMIdle;
                //PostExecute(PABaseState.SMCompleted);
            }
        }

        /// <summary>
        /// StateMode: Prozess in Pausezustand versetzten  (aktiv)
        /// </summary>
        [ACMethodState("en{'Pausing'}de{'Pausieren'}", 50, true)]
        public virtual void SMPausing()
        {
            //if (!PreExecute(PABaseState.SMPausing))
            //  return;
            CurrentACState = ACStateEnum.SMPaused;
            //PostExecute(PABaseState.SMPausing);
        }

        /// <summary>
        /// StateMode: Prozess ist in Pausezustand (inaktiv)
        /// </summary>
        [ACMethodState("en{'Paused'}de{'Pausiert'}", 60)]
        public virtual void SMPaused()
        {
            //if (!PreExecute(PABaseState.SMPaused))
            //  return;
            //PostExecute(PABaseState.SMPaused);
            // Fortsetzen mit Methode "Resume"
        }

        /// <summary>
        /// StateMode: Prozess vom Pausezustand wieder laufenden Status setzten (aktiv)
        /// </summary>
        [ACMethodState("en{'Resume'}de{'Fortsetzen'}", 70, true)]
        public virtual void SMResume()
        {
            //if (!PreExecute(Const.SMResume))
            //  return;
            CurrentACState = ACStateEnum.SMRunning;
            //PostExecute(Const.SMResume);
        }

        /// <summary>
        /// StateMode: Prozess durch den Benutzer abbrechen (aktiv)
        /// </summary>
        [ACMethodState("en{'Aborting'}de{'Abbrechen'}", 90, true)]
        public virtual void SMAborting()
        {
            //if (!PreExecute(PABaseState.SMAborting))
            //  return;
            CurrentACState = ACStateEnum.SMAborted;
            //PostExecute(PABaseState.SMAborting);
        }

        /// <summary>
        /// StateMode: Prozess abgebrochen (inaktiv)
        /// </summary>
        [ACMethodState("en{'Aborted'}de{'Abgebrochen'}", 100)]
        public virtual void SMAborted()
        {
            //if (!PreExecute(PABaseState.SMAborted))
            //  return;
            RecalcTimeInfo();
            if (CurrentACMethod.ValueT != null)
                FinishProgramLog(CurrentACMethod.ValueT);
            if (ParentTaskExecComp != null)
            {
                if (CurrentTask != null)
                {
                    if (ParentTaskExecComp.CallbackTask(CurrentTask, CreateNewMethodEventArgs(CurrentTask.ACMethod, Global.ACMethodResultState.Failed)))
                        CurrentTask = null;
                }
                else if (CurrentACMethod.ValueT != null)
                {
                    ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.Failed));
                }
            }
            if (IsACStateMethodConsistent(ACStateEnum.SMAborted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
                //CurrentACState = PABaseState.SMIdle;
                //PostExecute(PABaseState.SMAborted);
            }
        }

        /// <summary>
        /// Starten eines asynchronen Prozesses
        /// </summary>
        /// <param name="acMethod"></param>
        /// <returns>
        /// false: Wenn schon ein Prozess schon läuft oder der acMethod nicht den Status "Planned" hat
        /// true: Process wird gestartet
        /// </returns>
        [ACMethodAsync("Process", "en{'Start'}de{'Start'}", (short)MISort.Start, false)]
        public virtual ACMethodEventArgs Start(ACMethod acMethod)
        {
            if (!IsEnabledStart(acMethod))
                return CreateNewMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);

            try
            {
                if (ParentTaskExecComp != null)
                {
                    ACPointAsyncRMIWrap<ACComponent> taskEntry = ParentTaskExecComp.GetTaskOfACMethod(acMethod) as ACPointAsyncRMIWrap<ACComponent>;
                    if (taskEntry != null)
                    {
                        CurrentTask = taskEntry;
                        taskEntry.SetExecutingInstance(this);
                    }
                    else
                    {
                        CurrentTask = null;
                        CurrentACMethod.ValueT = acMethod;
                    }
                }
                else
                {
                    CurrentTask = null;
                    CurrentACMethod.ValueT = acMethod;
                }

                // Hole ACProgram vom ACClassTask-Datenbankkontext
                ACValue acValue = acMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (acValue != null && acValue.Value != null)
                {
                    Guid acProgramID = (Guid)acValue.Value;
                    if (acProgramID != Guid.Empty)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(
                                () => { _CurrentACProgram = ACClassTaskQueue.s_cQry_ACProgram(ACClassTaskQueue.TaskQueue.Context, acProgramID); }
                            );
                    }
                }

                acValue = CurrentACMethod.ValueT.ParameterValueList.GetACValue(ACProgramLog.ClassName);
                if (acValue != null && acValue.Value != null)
                {
                    Guid acProgramLogID = (Guid)acValue.Value;
                    if (acProgramLogID != Guid.Empty)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(
                                () => { _ParentProgramLog = ACClassTaskQueue.s_cQry_ACProgramLog(ACClassTaskQueue.TaskQueue.Context, acProgramLogID); }
                            );
                    }
                }

                OnPropertyChanged("CurrentACMethod");
                OnPropertyChanged("CurrentACProgram");

                _DelegateStartingToWorkCycle = true;
                ReloadConfig();
                FindChildComponents<PWBaseExecutable>(c => c is PWBaseExecutable).ForEach(c => c.OnRootPWStarted());
                CurrentACState = ACStateEnum.SMStarting;
                _DelegateStartingToWorkCycle = false;
            }
            catch (Exception e)
            {
                //Exception50000: {0}\n{1}\n{2}
                Msg msg = new Msg(this, eMsgLevel.Exception, PWClassName, "Start", 1000, "Exception50000",
                                    e.Message, e.InnerException != null ? e.InnerException.Message : "", e.StackTrace);
                OnNewAlarmOccurred(ProcessAlarm, msg, true);
                Messages.LogException(this.GetACUrl(), "Start(0)", msg.Message);
                throw e;
            }
            return CreateNewMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
        }

        public virtual bool IsEnabledStart(ACMethod acMethod)
        {
            bool isEnabledStart = CurrentACState == ACStateEnum.SMIdle && acMethod.IsValid();
            if (!isEnabledStart && CurrentACState == ACStateEnum.SMIdle)
            {
                UnloadWorkflow();
            }
            return isEnabledStart;
        }


        [ACMethodInteraction("Process", "en{'Pause'}de{'Pause'}", 300, true)]
        public virtual void Pause()
        {
            CurrentACState = ACStateEnum.SMPausing;
        }

        public virtual bool IsEnabledPause()
        {
            return CurrentACState == ACStateEnum.SMRunning;
        }

        [ACMethodInteraction("Process", "en{'Resume'}de{'Fortsetzen'}", 300, true)]
        public virtual void Resume()
        {
            CurrentACState = ACStateEnum.SMRunning;
        }

        public virtual bool IsEnabledResume()
        {
            return CurrentACState == ACStateEnum.SMPaused;
        }

        [ACMethodInteraction("Process", "en{'Abort'}de{'Abbrechen'}", (short)MISort.Abort, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Abort()
        {
            CurrentACState = ACStateEnum.SMAborting;
        }

        public virtual bool IsEnabledAbort()
        {
            return CurrentACState == ACStateEnum.SMRunning || CurrentACState == ACStateEnum.SMHolding || CurrentACState == ACStateEnum.SMHeld ||
                CurrentACState == ACStateEnum.SMRestarting || CurrentACState == ACStateEnum.SMPausing || CurrentACState == ACStateEnum.SMPaused ||
                CurrentACState == ACStateEnum.SMStopping;
        }

        public override void InvokeACStateMethod(ACClassMethod acClassMethod)
        {
            if ((LastACState == ACStateEnum.SMCompleted || LastACState == ACStateEnum.SMAborted)
                && CurrentACState == ACStateEnum.SMIdle)
            {
                UnloadWorkflow();
            }
            base.InvokeACStateMethod(acClassMethod);
        }


        /// <summary>
        /// If somebody has set the Workflow to Idle-State this mathod can Restart the Workflow again.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Start'}de{'Start'}", 301, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ReStart()
        {
            if (IsEnabledReStart())
                CurrentACState = ACStateEnum.SMStarting;
        }

        public virtual bool IsEnabledReStart()
        {
            return CurrentACState == ACStateEnum.SMIdle
                && CurrentACMethod.ValueT != null && CurrentACMethod.ValueT.IsValid()
                && CurrentACProgram != null
                && InitState == ACInitState.Initialized;
        }

        /// <summary>
        /// Completes this node by setting the CurrentACState-Property to ACStateEnum.SMCompleted.
        /// </summary>
        public void GroupComplete()
        {
            CurrentACState = ACStateEnum.SMCompleted;
        }
        #endregion

        #region User-Methods
        [ACMethodInteraction("", "en{'Reset substate'}de{'Unterstatus zurücksetzen'}", 803, true)]
        public virtual void ResetSubState()
        {
            if (!IsEnabledResetSubState())
                return;
            CurrentACSubState = 0;
        }

        public virtual bool IsEnabledResetSubState()
        {
            return CurrentACSubState != 0;
        }
        #endregion

        #region UnloadWorkflow

        public virtual void UnloadWorkflow()
        {
#if DEBUG
            Messages.LogDebug(this.GetACUrl(), "UnloadWorkflow(0)", "Starting unloading workflow");
#endif
            if (_UnloadCounter > 0)
                return;
            if (ApplicationManager == null)
                return;
            _UnloadCounter++;
            ApplicationManager.ProjectWorkCycleR1sec += new EventHandler(ProjectWorkCycleR1sec_TryUnloadWF);
        }

        private int _UnloadCounter = 0;
        private int _RetryUnloadCountDown = 3;
        void ProjectWorkCycleR1sec_TryUnloadWF(object sender, EventArgs e)
        {
            if (_UnloadCounter < 10)
            {
                if (FindChildComponents<PWBase>(c => c is PWBase).Where(c => c.CurrentACState != ACStateEnum.SMIdle).Any())
                {
                    _UnloadCounter++;
                    return;
                }
            }

            _UnloadCounter = 0;
            ApplicationManager objectManager = sender as ApplicationManager;
            if (objectManager != null)
            {
                if (!OnWorkflowUnloading(_RetryUnloadCountDown))
                {
                    _RetryUnloadCountDown--;
                    if (_RetryUnloadCountDown > 0)
                        return;
                }
#if DEBUG
                Messages.LogDebug(this.GetACUrl(), "UnloadWorkflow(1)", "Started Stopping workflow components");
#endif
                ParentACComponent.StopComponent(this, true);
                objectManager.ProjectWorkCycleR1sec -= ProjectWorkCycleR1sec_TryUnloadWF;
            }
        }

        protected virtual bool OnWorkflowUnloading(int retryUnloadCountDown)
        {
            return true;
        }
        #endregion

        #region Planning and Testing
        protected override TimeSpan GetPlannedDuration()
        {
            return base.GetPlannedDuration();
        }

        protected override DateTime GetPlannedStartTime()
        {
            return base.GetPlannedStartTime();
        }

        public virtual ACMethodEventArgs CreateNewMethodEventArgs(ACMethod acMethod, Global.ACMethodResultState state)
        {
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, state);
            RecalcTimeInfo();
            ACValue durationValue = result.GetACValue("TimeInfo");
            if (durationValue == null)
            {
                durationValue = new ACValue("TimeInfo", typeof(PATimeInfo));
                result.Add(durationValue);
            }

            durationValue.Value = TimeInfo.ValueT;

            return result;
        }

        #endregion

        #endregion

    }
}
