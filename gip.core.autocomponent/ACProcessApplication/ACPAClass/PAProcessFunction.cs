using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Threading;

namespace gip.core.autocomponent
{
    /// <summary>
    ///   <para>
    /// A PAProcessFunction handles a ACMethod-Call asynchronously and makes a callback when it reaches the Completed-State.
    /// Instances of PAProcessFunction can only be childs of a PAProcessModule. Don't use it for other classes.
    /// PAProcessFunction is abstract class. Derive it for your own Implementation.
    /// The state-machine is based on the ISA-S88-Standard
    ///   </para>
    ///   <para>Methoden for controlling the state via UI:
    /// <br /> -Start(acMethod)    Start/Initialize an asnychronous Process
    /// <br /> -Run()              Activate the Process
    /// <br /> -Pause()            Pause the Process (short break)
    /// <br /> -Resume()           Resume "Paused-State"
    /// <br /> -Hold()             Hold the Process (long-term break)
    /// <br /> -Restart()          Resume the "Hold-State"
    /// <br /> -Abort()            Cancel the Process
    /// <br /> -Stop()             Stop the Process
    /// <br /> -Reset()            Reset brings the process back to the Idle-State
    /// </para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAProcessFunction'}de{'PAProcessFunction'}", Global.ACKinds.TPAProcessFunction, Global.ACStorableTypes.Optional, false, "", true)]
    public abstract class PAProcessFunction : PABase, IACComponentProcessFunction
    {
        ///   ACState       | Auto-Next-State  |                |                                               TRANSITION-METHODS
        ///             	|   (No Method)    |   Start()	    |   Run()	    |   Pause()	    |   Resume()	|   Hold()	    |   Restart()	|   Abort()	    |   Stopp()	    |   Reset()
        /// ___________________________________________________________________________________________________________________________________________________________________________________
        /// SMIdle		    |           		|  SMStarting   |				|               |               |               |               |               |               |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMStarting	    |       			|	    		|	SMRunning	|	            |               |               |               |               |               |   SMResetting
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMRunning	    | SMCompleted		|               |               | SMPausing		|               | SMHolding	    |	            |  SMAborting   |	SMStopping	|
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMCompleted	    |					|               |               |               |               |               |               |               |               |	SMResetting
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMResetting	    | SMIdle			|               |               |               |               |               |               |               |               |
        /// ___________________________________________________________________________________________________________________________________________________________________________________
        /// SMPausing	    | SMPaused			|               |               |               |               |               |               |               |               |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMPaused		|                   |               |               |               |  SMResuming	|               |               |  SMAborting	|   SMStopping  |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMResuming	    | SMRunning			|               |               |               |               |               |               |               |               |
        /// ___________________________________________________________________________________________________________________________________________________________________________________
        /// SMHolding	    | SMHeld			|               |               |               |               |               |               |               |               |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMHeld			|			        |               |               |               |               |               |SMRestarting	|   SMAborting	|   SMStopping	|
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMRestarting	| SMRunning			|               |               |               |               |               |               |               |               |
        /// ___________________________________________________________________________________________________________________________________________________________________________________
        /// SMAborting	    | SMAborted			|               |               |               |               |               |               |               |               |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMAborted		|					|               |               |               |               |               |               |               |               |	SMResetting
        /// ___________________________________________________________________________________________________________________________________________________________________________________
        /// SMStopping	    | SMStopped			|               |               |               |               |               |               |               |               |
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// SMStopped		|					|               |               |               |               |               |               |               |               |	SMResetting
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        #region c´tors

        static PAProcessFunction()
        {
            RegisterExecuteHandler(typeof(PAProcessFunction), HandleExecuteACMethod_PAProcessFunction);
        }

        public PAProcessFunction(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn1 = new PAPoint(this, Const.PAPointMatIn1);
            _PAPointMatOut1 = new PAPoint(this, Const.PAPointMatOut1);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            Malfunction.PropertyChanged += Malfunction_PropertyChanged;
            _FuncConvMode = new ACPropertyConfigValue<short>(this, "FuncConvMode", 0);
            return true;
        }

        public override bool ACPostInit()
        {
            short functConvMode = FuncConvMode;
            bool result = base.ACPostInit();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            Malfunction.PropertyChanged -= Malfunction_PropertyChanged;
            bool result = base.ACDeInit(deleteACClassTask);
            CurrentACMethod.ValueT = null;

            SetCurrentProgramLog(null, false);  // Beim herunterfahren ist detaching unnötige last deleteACClassTask);
            SetCurrentACProgram(null, false);

            _CurrentTask = null;
            _SavedACMethod = null;
            _ACStateConverter = null;
            return result;
        }

        #endregion


        #region Points

        private PAPoint _PAPointMatIn1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatIn1
        {
            get { return _PAPointMatIn1; }
        }

        private PAPoint _PAPointMatOut1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatOut1
        {
            get { return _PAPointMatOut1; }
        }

        #endregion


        #region Properties

        #region ACProgram
        protected ACProgram _CurrentACProgram;
        /// <summary>
        /// If this Function was invoked by Workflow-Instance, then the ACProgram of the Workflow-Root is returned here.
        /// Otherwise null will be returned.
        /// </summary>
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
                if (currentACProgram != null)
                    ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(currentACProgram.ACProgramID);
            }
            else if (CurrentTask != null)
            {
                ACValue acValue = CurrentTask.ACMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (acValue != null)
                {
                    Guid acProgramID = (Guid)acValue.Value;
                    if (acProgramID != Guid.Empty)
                    {
                        currentACProgram = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                    }
                }
                if (currentACProgram == null)
                    currentACProgram = CurrentTask.WorkflowContext as ACProgram;
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
        #endregion


        #region ACProgramLog
        protected ACProgramLog _CurrentProgramLog;
        /// <summary>
        /// Gets the current program log.
        /// </summary>
        /// <value>
        /// The current program log.
        /// </value>
        public override ACProgramLog CurrentProgramLog
        {
            get
            {
                return GetCurrentProgramLog(CurrentACState != ACStateEnum.SMIdle);
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
                if (parentProgramLog != null)
                {
                    string acUrl = this.GetACUrl();
                    currentProgramLog = ACClassTaskQueue.TaskQueue.ProgramCache.GetCurrentProgramLog(parentProgramLog, acUrl);
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
                sb.AppendLine(String.Format("=> PAProcessFunction._CurrentProgramLog is null at {0}", this.GetACUrl()));
                ACProgramLog parentProgramLog = ParentProgramLog;
                if (parentProgramLog == null)
                    sb.AppendLine(String.Format("=> ParentProgramLog is null at {0}", this.GetACUrl()));
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
                    parentProgramLog = PreviousParentProgramLog; // NO-CHANGE-TRACKING!!!

                string acUrl = this.GetACUrl();
                if (parentProgramLog != null)
                {
                    previousLogs = ACClassTaskQueue.TaskQueue.ProgramCache.GetPreviousLogsFromParentLog(parentProgramLog.ACProgramLogID, acUrl);
                }

                return previousLogs;
            }
        }


        public override ACProgramLog ParentProgramLog
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

        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override ACProgramLog PreviousParentProgramLog
        {
            get
            {
                if (CurrentTask == null || !CurrentTask.IsObjLoaded)
                    return null;
                PABase paBase = CurrentTask.ValueT as PABase;
                if (paBase == null)
                    return null;
                return paBase.PreviousProgramLogs.LastOrDefault();
            }
        }

        #endregion


        #region Task and ACMethod

        /// <summary>The virtual method that this function is currently processing.</summary>
        /// <value>ACMethod</value>
        [ACPropertyBindingSource(999, "ACConfig", "en{'Current Method'}de{'Aktuelle Methode'}", "", false, false)]
        public IACContainerTNet<ACMethod> CurrentACMethod { get; set; }

        /// <summary>
        /// Internal method, that is called when CurrentACMethod is changed. Don't use it!
        /// </summary>
        /// <param name="valueEvent">The value event.</param>
        public void OnSetCurrentACMethod(IACPropertyNetValueEvent valueEvent)
        {
            // Verify that changes on ACMethod are only allowed, if Function is not completed. Otherwise the Result-Values from ReceiveACMethodResult will eventually be overriden through concurrency problems.
            if (valueEvent.Sender == EventRaiser.Proxy && valueEvent.EventType == EventTypes.Request)
            {
                if (!IsEnabledSendChangedACMethod())
                {
                    valueEvent.Handled = true;
                    return;
                }
            }
        }

        ACPointAsyncRMIWrap<ACComponent> _CurrentTask = null;
        /// <summary>
        /// Current Task that is set when this function was started asynchronously from a Workflow-Instance.
        /// In this case the CurrentACMethod ist set from CurrentTask.ACMethod!
        /// </summary>
        /// <value>
        /// The current task.
        /// </value>
        [ACPropertyInfo(true, 999, "ACConfig", "en{'Current Task'}de{'Aktueller Task'}", "", true)]
        public ACPointAsyncRMIWrap<ACComponent> CurrentTask
        {
            get
            {
                return _CurrentTask;
            }
            set
            {
                /// Falls die Funktion durch einen Workflowschritt gestartet worden ist,
                /// dann sollen die gültigen Parameter (ACMethod) indirekt über diese Task-Property gespeichert werden
                /// Beim anschliessenden Hochfahren, wird der setter durch die Restore-Funktion aufgerufen und dadurch die CurrentACMethod gesetzt
                /// In diesem Fall ist SavedACMethod = null
                _CurrentTask = value;
                if (_CurrentTask != null)
                {
                    if (CurrentACMethod.ValueT != _CurrentTask.ACMethod)
                    {
                        CurrentACMethod.ValueT = _CurrentTask.ACMethod;
                        if (CurrentACMethod.ValueT == null && ACStateConverter != null && ACStateConverter.LoggingEnabled)
                            Messages.LogDebug(this.GetACUrl(), "CurrentTask(1)", "CurrentACMethod.ValueT = null");
                    }
                }
                else if (SavedACMethod == null)
                {
                    CurrentACMethod.ValueT = null;
                    if (ACStateConverter != null && ACStateConverter.LoggingEnabled)
                        Messages.LogDebug(this.GetACUrl(), "CurrentTask(2)", "CurrentACMethod.ValueT = null");
                }
                OnPropertyChanged("CurrentTask");
            }
        }

        ACMethod _SavedACMethod = null;
        /// <summary>
        /// If this function has been started synchronously via GUI on a client then the CurrentTask-Property is null.
        /// In this case the persistable network property CurrentACMethod ist used to store the parameters that are passed in the Start()-Method.
        /// </summary>
        /// <value>
        /// The saved ac method.
        /// </value>
        [ACPropertyInfo(true, 999, "ACConfig", "en{'Saved ACMethod'}de{'Saved acMethod'}", "", true)]
        public ACMethod SavedACMethod
        {
            get
            {
                return _SavedACMethod;
            }
            set
            {
                /// Falls die Funktion durch einen manuellen Start von der Client-Oberfläche gestartet worden ist,
                /// dann sollen die gültigen Parameter (ACMethod) über diese lokale Property persisitiert werden
                /// Beim anschliessenden Hochfahren, wird der setter durch die Restore-Funktion aufgerufen und dadurch die CurrentACMethod gesetzt
                /// In diesem Fall ist CurrentTask = null
                _SavedACMethod = value;
                if (_SavedACMethod != null)
                {
                    if (CurrentACMethod.ValueT != _SavedACMethod)
                    {
                        CurrentACMethod.ValueT = value;
                        if (CurrentACMethod.ValueT == null && ACStateConverter != null && ACStateConverter.LoggingEnabled)
                            Messages.LogDebug(this.GetACUrl(), "SavedACMethod(1)", "CurrentACMethod.ValueT = null");
                    }
                }
                else if (CurrentTask == null)
                {
                    CurrentACMethod.ValueT = null;
                    if (ACStateConverter != null && ACStateConverter.LoggingEnabled)
                        Messages.LogDebug(this.GetACUrl(), "SavedACMethod(2)", "CurrentACMethod.ValueT = null");
                }
                OnPropertyChanged("SavedACMethod");
            }
        }

        /// <summary>
        /// Parent Processmodule, that attaches this function as a method that can be invoked to start this process function.
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
        #endregion


        #region Converter

        private PAFuncStateConvBase _ACStateConverter;
        /// <summary>
        /// Returns a State-Converter-Instance if configured as a child.
        /// </summary>
        /// <value>
        /// The ac state converter.
        /// </value>
        public PAFuncStateConvBase ACStateConverter
        {
            get
            {
                if (_ACStateConverter != null)
                    return _ACStateConverter;
                return FindChildComponents<PAFuncStateConvBase>(c => c is PAFuncStateConvBase).FirstOrDefault();
            }
            set
            {
                _ACStateConverter = value;
                OnPropertyChanged("ACStateConverter");
            }
        }


        /// <summary>
        /// Enum for controlling the behavior of a Function an it's Converter
        /// </summary>
        public enum FuncConverterMode
        {
            /// <summary>
            /// Normal Function, which is started from Workflow-Nodes
            /// </summary>
            ControlledByIPlus = 0,

            /// <summary>
            /// Backgroundfunction, which is not controlled by iPlus.
            /// PLC needs this function but switches the states selfly.
            /// The Standard-Logic in iPlus must be disabled.
            /// Example: PLC needs a Way-Function in background
            /// </summary>
            OnlyOnPLCSide = 1
        }

        private ACPropertyConfigValue<short> _FuncConvMode;
        /// <summary>
        /// Enum for controlling the behavior of a Function an it's Converter
        /// </summary>
        [ACPropertyConfig("en{'Converter mode'}de{'Converter-Modus'}")]
        public short FuncConvMode
        {
            get
            {
                return _FuncConvMode.ValueT;
            }
            set
            {
                _FuncConvMode.ValueT = value;
            }
        }

        #endregion


        #region Malfunction State
        [ACPropertyBindingTarget(634, "Read from PLC", "en{'Malfunction'}de{'Störung allgemein'}", "", false, false, RemotePropID = 14)]
        public IACContainerTNet<PANotifyState> Malfunction { get; set; }
        /// <summary>
        /// For internal usage only: Its invoked when Malfunction changes. Don't use it!
        /// </summary>
        /// <param name="valueEvent">The value event.</param>
        public void OnSetMalfunction(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (AckMalfunction.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                AckMalfunction.ValueT = false;
            if (newSensorState != Malfunction.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _MalfunctionAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _MalfunctionAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _MalfunctionAlarmChanged = PAAlarmChangeState.NoChange;
        void Malfunction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_MalfunctionAlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(Malfunction);
                if (_MalfunctionAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(Malfunction);
                else
                    OnAlarmDisappeared(Malfunction);
                _MalfunctionAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        [ACPropertyBindingTarget(653, "Write to PLC", "en{'Fault acknowledge'}de{'Störungsquittung'}", "", true, false, RemotePropID = 15)]
        public IACContainerTNet<bool> AckMalfunction { get; set; }
        #endregion


        #region Error and Repeat
        [ACPropertyBindingSource(210, "Error", "en{'Function error'}de{'Funktionsfehler'}", "", false, false)]
        public IACContainerTNet<PANotifyState> FunctionError { get; set; }
        public const string PropNameFunctionError = "FunctionError";

        private bool _IsMethodChangedFromClient = false;
        /// <summary>
        /// SendChangedACMethod() was invoked from Client
        /// </summary>
        protected bool IsMethodChangedFromClient
        {
            get
            {
                // return Root.CurrentInvokingUser.VBUserID != Root.Environment.User.VBUserID
                return _IsMethodChangedFromClient;
            }
        }


        private bool _RepeatWFAfterReset = false;
        /// <summary>
        /// Should WF-Node execute this Function again after manual resetting
        /// </summary>
        protected bool RepeatWFAfterReset
        {
            get
            {
                return _RepeatWFAfterReset;
            }
        }
        #endregion


        #region IACConfigURL

        public virtual string ConfigACUrl
        {
            get
            {
                if (CurrentTask == null)
                    return null;
                ACClassWF currentWfNode = null;
                IACComponentPWNode pwNode = CurrentTask.ValueT as IACComponentPWNode;
                if (pwNode != null)
                    currentWfNode = pwNode.ContentACClassWF;
                if (currentWfNode != null)
                {
                    if (this.ParentACObject is IACConfigURL)
                    {
                        return (ParentACObject as IACConfigURL).ConfigACUrl + "\\" + currentWfNode.ACIdentifier;
                    }
                    else
                        return currentWfNode.ACIdentifier;
                }
                return "";
            }
        }

        public virtual string PreValueACUrl
        {
            get
            {
                PAProcessFunction paProcessFunction = this;
                ACClassWF currentWfNode = null;
                if (CurrentTask != null)
                {
                    IACComponentPWNode pwNode = CurrentTask.ValueT as IACComponentPWNode;
                    if (pwNode != null)
                        currentWfNode = pwNode.ContentACClassWF;
                }

                while (paProcessFunction != null)
                {
                    IACWorkflowObject rootACVisual = null;
                    ACClassMethod rootMethod = null;
                    if (currentWfNode != null)
                    {

                        using (ACMonitor.Lock(this.ContextLockForACClassWF))
                        {
                            rootMethod = currentWfNode.ACClassMethod;
                            if (rootMethod != null)
                                rootACVisual = rootMethod.RootWFNode;
                        }
                    }

                    if (rootACVisual != null && rootACVisual == currentWfNode)
                    {
                        string preACUrl = "";
                        PAProcessFunction paProcessFunction2 = paProcessFunction.ParentACObject as PAProcessFunction;
                        while (paProcessFunction2 != null)
                        {
                            preACUrl = paProcessFunction2.ACIdentifier + "\\" + preACUrl;
                            paProcessFunction2 = paProcessFunction2.ParentACObject as PAProcessFunction;
                        }
                        return preACUrl;
                    }
                    paProcessFunction = paProcessFunction.ParentACObject as PAProcessFunction;
                }
                return "";
            }
        }

        public virtual void RefreshRuleStates()
        {
        }

        #endregion

        #endregion


        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.SMStarting:
                    SMStarting();
                    return true;
                case ACStateConst.SMRunning:
                    SMRunning();
                    return true;
                case ACStateConst.SMCompleted:
                    SMCompleted();
                    return true;
                case ACStateConst.SMResetting:
                    SMResetting();
                    return true;
                case ACStateConst.SMPausing:
                    SMPausing();
                    return true;
                case ACStateConst.SMPaused:
                    SMPaused();
                    return true;
                case ACStateConst.SMResuming:
                    SMResuming();
                    return true;
                case ACStateConst.SMHolding:
                    SMHolding();
                    return true;
                case ACStateConst.SMHeld:
                    SMHeld();
                    return true;
                case ACStateConst.SMRestarting:
                    SMRestarting();
                    return true;
                case ACStateConst.SMAborting:
                    SMAborting();
                    return true;
                case ACStateConst.SMAborted:
                    SMAborted();
                    return true;
                case ACStateConst.SMStopping:
                    SMStopping();
                    return true;
                case ACStateConst.SMStopped:
                    SMStopped();
                    return true;
                case ACStateConst.TMPause:
                    Pause();
                    return true;
                case ACStateConst.TMResume:
                    Resume();
                    return true;
                case ACStateConst.TMHold:
                    Hold();
                    return true;
                case ACStateConst.TMRestart:
                    Restart();
                    return true;
                case ACStateConst.TMAbort:
                    Abort();
                    return true;
                case ACStateConst.TMStopp:
                    Stopp();
                    return true;
                case ACStateConst.TMReset:
                    Reset();
                    return true;
                case "Reset2Repeat":
                    Reset2Repeat();
                    return true;
                case "SendChangedACMethod":
                    SendChangedACMethod();
                    return true;
                case ACStateConst.TMStart:
                    result = Start(acParameter[0] as ACMethod);
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMRun:
                    result = IsEnabledRun();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMPause:
                    result = IsEnabledPause();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMResume:
                    result = IsEnabledResume();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMHold:
                    result = IsEnabledHold();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMRestart:
                    result = IsEnabledRestart();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMAbort:
                    result = IsEnabledAbort();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMStopp:
                    result = IsEnabledStopp();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMReset:
                    result = IsEnabledReset();
                    return true;
                case Const.IsEnabledPrefix + "Reset2Repeat":
                    result = IsEnabledReset2Repeat();
                    return true;
                case Const.IsEnabledPrefix + "SendChangedACMethod":
                    result = IsEnabledSendChangedACMethod();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMStart:
                    result = IsEnabledStart(acParameter != null && acParameter.Count() > 0 ? acParameter[0] as ACMethod : null);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PAProcessFunction(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case Const.AskUserPrefix + ACStateConst.TMAbort:
                    result = AskUserAbort(acComponent);
                    return true;
                case Const.AskUserPrefix + ACStateConst.TMStopp:
                    result = AskUserStopp(acComponent);
                    return true;
                case Const.AskUserPrefix + ACStateConst.TMReset:
                    result = AskUserReset(acComponent);
                    return true;
                case Const.AskUserPrefix + "Reset2Repeat":
                    result = AskUserReset2Repeat(acComponent);
                    return true;
                case Const.AskUserPrefix + "SendChangedACMethod":
                    result = AskUserSendChangedACMethod(acComponent);
                    return true;
            }
            return false;
        }
        #endregion


        #region Virtual

        /// <summary>
        /// Before a Function can switch to the running state parameters has to be send to a external system (e.g. PLC).
        /// Before sending this method is called. Override this Method if you want to manipulate the parameters inside ACMethod before they are sended by ACStateConverter.SendACMethod().
        /// This method is also invoked, when a user changes the parameterlist on the GUI on client-side.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <returns></returns>
        protected virtual MsgWithDetails CompleteACMethodOnSMStarting(ACMethod acMethod)
        {
            return null;
        }


        /// <summary>
        /// This method is invoked when the function completes and ACStateConverter.ReceiveACMethodResult() has been called to get result-values from a external system (e.g. PLC).
        /// Override this method if you want to manipulate the ResultValue-List before the callback to the invoking workflow-node takes place.
        /// With returning the enum CompleteResult you can control if the callback should be done or not.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="completeResult">The complete result.</param>
        /// <returns></returns>
        protected virtual PAProcessFunction.CompleteResult AnalyzeACMethodResult(ACMethod acMethod, out MsgWithDetails msg, CompleteResult completeResult)
        {
            msg = null;
            return CompleteResult.Succeeded;
        }


        /// <summary>
        /// This method is invoked when ApplicationManager.InitializeRouteAndConfig() is invoked from a user.
        /// Override this method if your function has to inititialize some Standard-Parameters for the Route-Configuration.
        /// </summary>
        /// <param name="dbIPlus">The database i plus.</param>
        public virtual void InitializeRouteAndConfig(Database dbIPlus)
        {
        }


        protected virtual string OnGetVMethodNameForRouteInitialization(string suggestedVMethodname)
        {
            return suggestedVMethodname;
        }

        #endregion


        #region ACState

        #region State-Methods

        #region Main-States
        public override void SMIdle()
        {
            base.SMIdle();
            _IsMethodChangedFromClient = false;
            try
            {
                using (ACMonitor.Lock(_20200_LockExecuteACStateMethod))
                {
                    // Maybe state is not in SMIdle any more because this method is called through delegated invocation of DelegateInvokeACStateMethod() from Application-Queue
                    // an another thread has started this function again in the meantime
                    if (CurrentACState == ACStateEnum.SMIdle)
                    {
                        // If State-Converter has switche illegaly to this state and a Task is active, empty first TaskPoint
                        if (CurrentTask != null && FuncConvMode == (short)FuncConverterMode.ControlledByIPlus)
                        {
                            CallbackTaskBeforeResetting(_RepeatWFAfterReset ? Global.ACMethodResultState.FailedAndRepeat : Global.ACMethodResultState.Failed);

                            if (CurrentTask != null)
                            {
                                if (!ParentTaskExecComp.TaskInvocationPoint.ConnectionList.Any())
                                    CurrentTask = null;
                                else
                                    Messages.LogError(this.GetACUrl(), "SMIdle()", "CallbackTaskBeforeResetting doesn't succeeded in SMIdle()");
                            }
                        }
                        if (SavedACMethod != null)
                        {
                            SavedACMethod = null;
                        }
                        base.SMIdle();
                    }
                }
                // Maybe state is not in SMIdle any more because this method is called through delegated invocation of DelegateInvokeACStateMethod() from Application-Queue
                // an another thread has started this function again in the meantime
                if (CurrentACState == ACStateEnum.SMIdle)
                    UnSubscribeToProjectWorkCycle();
            }
            finally
            {
                _CallbackTries = 0;
                _RepeatWFAfterReset = false;
            }
        }

        /// <summary>
        ///   <para>
        /// If method "Start(ACMethod acMethod)" was sucessful, than the state-machine switches to SMStarting and invokes this cyclic method.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMRunning: If ReSendACMethod() succeeds and parameters are send to a external system.
        /// <br />SMIdle: If something went wrong and ACStateConverter has cancelled the process.
        /// </para>
        /// </summary>
        [ACMethodState("en{'Starting'}de{'Startend'}", 20, true)]
        public virtual void SMStarting()
        {

            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (!CallbackOnSMStarting())
                return;

            RecalcTimeInfo();
            if (CurrentTask != null && CreateNewProgramLog(CurrentACMethod.ValueT) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                return;

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                bool wasSubscribed = IsSubscribedToWorkCycle;
                if (!IsEnabledRun())
                {
                    // Falls Converter in IsEnabledRun() entschieden hat, dass ein erneutes Starten nicht mehr nötig ist, da bereits alles getan
                    if (wasSubscribed && !IsSubscribedToWorkCycle)
                        return;
                    SubscribeToProjectWorkCycle();
                    return;
                }

                MsgWithDetails msgError = ReSendACMethod();
                if (msgError != null)
                {
                    SubscribeToProjectWorkCycle();
                    return;
                }
                else
                    UnSubscribeToProjectWorkCycle();

                _CallbackTries = 0;
                if (ACOperationMode == ACOperationModes.Live)
                {
                    ACStateEnum nextACState;
                    if (ACStateConverter != null)
                        nextACState = ACStateConverter.GetNextACState(this, ACStateConst.TMRun);
                    else
                        nextACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMRun);
                    if (nextACState != CurrentACState)
                        CurrentACState = nextACState;
                    //if (nextACState == ACStateEnum.SMStarting)
                    //    SubscribeToProjectWorkCycle();
                }
                else
                    CurrentACState = ACStateEnum.SMCompleted;
            }
        }


        /// <summary>
        /// Callbacks the the invoking workflow-node and informs, that the task was either started/accepted or cancelled.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CallbackOnSMStarting()
        {
            //  Falls der Schritt sich noch im SMStarting-Modus befand nach dem letzten herunterfahren, dann während der Hochfahrphase kein erneuter Callback 
            if (!Root.Initialized)
                return true;

            // Parameter müssen gültig sein und der ACMethodState muss auf Planned stehen
            if (CurrentTask != null && !CurrentTask.ACMethod.IsValid())
            {
                if (ParentTaskExecComp != null)
                {
                    if (ParentTaskExecComp.CallbackTask(CurrentTask, CreateNewMethodEventArgs(CurrentTask.ACMethod, Global.ACMethodResultState.Failed)))
                        CurrentTask = null;
                }
                if (CurrentACState == ACStateEnum.SMStarting) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                    CurrentACState = ACStateEnum.SMIdle;
                return false;
            }
            else if (CurrentACMethod.ValueT != null && !CurrentACMethod.ValueT.IsValid())
            {
                if (ParentTaskExecComp != null)
                    ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.Failed));
                if (CurrentACState == ACStateEnum.SMStarting) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                    CurrentACState = ACStateEnum.SMIdle;
                return false;
            }
            return true;
        }


        /// <summary>
        ///   <para>
        /// Comes from SMStarting-State an signals that the process is running.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMCompleted
        /// <br />SMAborted
        /// </para>
        /// </summary>
        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public virtual void SMRunning()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ParentTaskExecComp != null && CurrentACMethod.ValueT != null && LastACState != CurrentACState && LastACState == ACStateEnum.SMStarting)
                ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.InProcess), PointProcessingState.Accepted);

            if (CyclicWaitIfSimulationOn())
                return;

            if (IsSimulationOn)
            {
                if (ACStateConverter != null)
                    CurrentACState = ACStateConverter.GetNextACState(this);
                else
                    CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
            }
        }


        /// <summary>
        ///   <para>
        /// Comes from SMRunning-State an signals that the process has completed. Reset() will be called inside this method if callback to the invoking workflow-node has been sucessful.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMResetting
        /// </para>
        /// </summary>
        [ACMethodState("en{'Completed'}de{'Fertiggestellt'}", 40, true)]
        public virtual void SMCompleted()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            CompleteResult completeResult = CompleteResultAndCallback(Global.ACMethodResultState.Succeeded);
            if (completeResult == CompleteResult.FailedAndWait)
            {
                SubscribeToProjectWorkCycle();
                return;
            }
            UnSubscribeToProjectWorkCycle();

            if (IsACStateMethodConsistent(ACStateEnum.SMCompleted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
            }
        }



        /// <summary>
        ///   <para>
        /// Comes from SMCompleted or SMStopped or SMAborted-State an signals that the callback to the invoking workflow-node has been done
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMIdle
        /// </para>
        /// </summary>
        [ACMethodState("en{'Resetting'}de{'Zurücksetzen'}", 40, true)]
        public virtual void SMResetting()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }
        #endregion


        #region Sub-States Pausing and Resuming
        /// <summary>
        ///   <para>
        /// Comes from SMRunning-State when user has invoked the Pause()-Method.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMPaused
        /// </para>
        /// </summary>
        [ACMethodState("en{'Pausing'}de{'Pausieren'}", 50, true)]
        public virtual void SMPausing()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }


        /// <summary>
        ///   <para>
        /// Comes from SMPausing-State. Resume() must be invoked from user to come back to the Running-State.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMResuming
        /// </para>
        /// </summary>
        [ACMethodState("en{'Paused'}de{'Pausiert'}", 60, true)]
        public virtual void SMPaused()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ParentTaskExecComp != null && CurrentACMethod.ValueT != null && LastACState != CurrentACState && LastACState == ACStateEnum.SMStarting)
                ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.InProcess), PointProcessingState.Accepted);
        }


        /// <summary>
        ///   <para>
        /// Comes from SMPaused-State.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMRunning
        /// </para>
        /// </summary>
        [ACMethodState("en{'Resuming'}de{'In fortsetzung'}", 60, true)]
        public virtual void SMResuming()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }
        #endregion


        #region Sub-States Holding and Restarting
        /// <summary>
        ///   <para>
        /// Comes from SMRunning-State when user has invoked the Hold()-Method.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMPaused
        /// </para>
        /// </summary>
        [ACMethodState("en{'Holding'}de{'Anhalten'}", 50, true)]
        public virtual void SMHolding()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }


        /// <summary>
        ///   <para>
        /// Comes from SMHolding-State. Restart() must be invoked from user to come back to the Running-State.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMResuming
        /// </para>
        /// </summary>
        [ACMethodState("en{'Held'}de{'Angehalten'}", 60, true)]
        public virtual void SMHeld()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ParentTaskExecComp != null && CurrentACMethod.ValueT != null && LastACState != CurrentACState && LastACState == ACStateEnum.SMStarting)
                ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, Global.ACMethodResultState.InProcess), PointProcessingState.Accepted);
        }

        /// <summary>
        ///   <para>
        /// Comes from SMHeld-State.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMRunning
        /// </para>
        /// </summary>
        [ACMethodState("en{'Restarting'}de{'Neustarten'}", 70, true)]
        public virtual void SMRestarting()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }
        #endregion


        #region Sub-States Aborting
        /// <summary>
        ///   <para>
        /// Comes from SMRunning-State when user has invoked the Abort()-Method.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMAborted
        /// </para>
        /// </summary>
        [ACMethodState("en{'Aborting'}de{'Abbrechen'}", 90, true)]
        public virtual void SMAborting()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }


        /// <summary>
        ///   <para>
        /// Comes from SMAborting-State. Reset() will be called inside this method if callback to the invoking workflow-node has been sucessful.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMResetting
        /// </para>
        /// </summary>
        [ACMethodState("en{'Aborted'}de{'Abgebrochen'}", 100, true)]
        public virtual void SMAborted()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            CompleteResult completeResult = CompleteResultAndCallback(Global.ACMethodResultState.Failed);
            if (completeResult == CompleteResult.FailedAndWait)
            {
                SubscribeToProjectWorkCycle();
                return;
            }
            UnSubscribeToProjectWorkCycle();

            if (IsACStateMethodConsistent(ACStateEnum.SMAborted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
            }
        }
        #endregion


        #region Sub-States Stopping
        /// <summary>
        ///   <para>
        /// Comes from SMRunning-State when user has invoked the Stopp()-Method.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMStopped
        /// </para>
        /// </summary>
        [ACMethodState("en{'Stopping'}de{'Stoppen'}", 90, true)]
        public virtual void SMStopping()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState);
        }


        /// <summary>
        ///   <para>
        /// Comes from SMStopping-State. Reset() will be called inside this method if callback to the invoking workflow-node has been sucessful.
        /// </para>
        ///   <para>
        /// Possible follow-up states: 
        /// <br />SMResetting
        /// </para>
        /// </summary>
        [ACMethodState("en{'Stopped'}de{'Gestoppt'}", 100, true)]
        public virtual void SMStopped()
        {
            if (FuncConvMode == (short)FuncConverterMode.OnlyOnPLCSide)
            {
                UnSubscribeToProjectWorkCycle();
                return;
            }

            CompleteResult completeResult = CompleteResultAndCallback(Global.ACMethodResultState.Succeeded);
            if (completeResult == CompleteResult.FailedAndWait)
            {
                SubscribeToProjectWorkCycle();
                return;
            }
            UnSubscribeToProjectWorkCycle();

            if (IsACStateMethodConsistent(ACStateEnum.SMStopped) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
            }
        }
        #endregion

        #endregion

        #region Transition-Methods

        #region Starting        
        /// <summary>
        /// Starts a new asynchronous process.
        /// IsEnabledStart() is called first to validate the parameters in the passed ACMethod-Object.
        /// If IsEnabledStart() fails, then ACMethodEventArgs ist returned with Global.ACMethodResultState.Failed or Global.ACMethodResultState.FailedAndRepeat.
        /// Else Global.ACMethodResultState.InProcess is returned and the invoking Workflow-Node is called back.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <returns></returns>
        [ACMethodAsync("Process", "en{'Start'}de{'Start'}", (short)MISort.Start, false)]
        public virtual ACMethodEventArgs Start(ACMethod acMethod)
        {
            if (!IsEnabledStart(acMethod))
                return CreateNewMethodEventArgs(acMethod, this.IsACStateInconsistent ? Global.ACMethodResultState.FailedAndRepeat : Global.ACMethodResultState.Failed);
            using (ACMonitor.Lock(_20200_LockExecuteACStateMethod))
            {
                if (ParentTaskExecComp != null)
                {
                    // Falls Funktion von einem Workflowschritt aufgerufen worden ist, dann ermittle CurrentTask um die Task zu persistieren, damit im Falle des Neustartens die aktuelle CurrentACMethod.ValueT wiederhergestellt wird
                    ACPointAsyncRMIWrap<ACComponent> taskEntry = ParentTaskExecComp.GetTaskOfACMethod(acMethod) as ACPointAsyncRMIWrap<ACComponent>;
                    if (taskEntry != null)
                    {
                        CurrentTask = taskEntry;
                        SavedACMethod = null;
                        taskEntry.SetExecutingInstance(this);
                    }
                    // Sonst wurde Funktion manuell von einem Client aus gestartet, dann persisitiere die acMethod in die SavedACMethod-PRoperty, damit im Falle des Neustartens die aktuelle CurrentACMethod.ValueT wiederhergestellt wird
                    else
                    {
                        CurrentTask = null;
                        SavedACMethod = acMethod;
                    }
                }
                else
                {
                    CurrentTask = null;
                    SavedACMethod = acMethod;
                }

                // Hole ACProgram vom ACClassTask-Datenbankkontext
                ACValue acValue = acMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (acValue != null && acValue.Value != null)
                {
                    Guid acProgramID = (Guid)acValue.Value;
                    if (acProgramID != Guid.Empty)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(
                                () => { _CurrentACProgram = ACClassTaskQueue.TaskQueue.Context.ACProgram.Where(c => c.ACProgramID == acProgramID).FirstOrDefault(); }
                            );
                    }

                    //_CurrentACProgram = value.EntityT<ACProgram>(ApplicationManager.Database.ContextIPlus);
                }

                OnPropertyChanged("CurrentACMethod");
                OnPropertyChanged("CurrentACProgram");

                CurrentACState = ACStateEnum.SMStarting;
            }
            return CreateNewMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
        }

        /// <summary>
        /// Validates the passed ACMethod and calls ACMethod.IsValid().
        /// Override this method if you wan't to implement some further validations.
        /// Alway call base.IsEnabledStart() first.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <returns>
        ///   <c>true</c> if [is enabled start] [the specified ac method]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEnabledStart(ACMethod acMethod)
        {
            if (CurrentTask != null)
                return false;
            bool isEnabledStart = false;
            if (ACStateConverter != null)
                isEnabledStart = ACStateConverter.IsEnabledTransition(this, ACStateConst.TMStart);
            else
                isEnabledStart = PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMStart, this);
            if (!isEnabledStart)
                return false;
            else if (IsACStateInconsistent)
                return false;

            return acMethod != null ? acMethod.IsValid() : true;
        }


        /// <summary>
        /// This method is called inside SMStarting() to check if a remote system (e.g. PLC) is ready to start this Function.
        /// Only if this method returns true, ReSendACMethod() will be called.
        /// You can override this method if you want to implement some further checks.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is enabled run]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEnabledRun()
        {
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMRun);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMRun, this);
        }
        #endregion


        #region Pausing and Resuming
        /// <summary>
        /// Brings the process from Running-State to Paused-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Pause'}de{'Pause'}", (short)MISort.Pause, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Pause()
        {
            if (!IsEnabledPause())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMPause);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMPause);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Pause()", user);
        }

        public virtual bool IsEnabledPause()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMPause, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMPause);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMPause, this);
        }


        /// <summary>
        /// Brings the process from Paused-State to Running-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Resume'}de{'Fortsetzen'}", (short)MISort.Resume, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Resume()
        {
            if (!IsEnabledResume())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMResume);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMResume);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Resume()", user);
        }

        public virtual bool IsEnabledResume()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMResume, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMResume);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMResume, this);
        }
        #endregion


        #region Holding and Restarting
        /// <summary>
        /// Brings the process from Running-State to Hold-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Hold'}de{'Anhalten'}", (short)MISort.Pause, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Hold()
        {
            if (!IsEnabledHold())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMHold);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMHold);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Hold()", user);
        }

        public virtual bool IsEnabledHold()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMHold, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMHold);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMHold, this);
        }


        /// <summary>
        /// Brings the process from Hold-State to Running-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Restart'}de{'Neustarten'}", (short)MISort.Resume, true)]
        public virtual void Restart()
        {
            if (!IsEnabledRestart())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMRestart);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMRestart);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Restart()", user);
        }

        public virtual bool IsEnabledRestart()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMRestart, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMRestart);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMRestart, this);
        }
        #endregion


        #region Aborting
        /// <summary>
        /// Brings the process from any state to Aborted-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Abort'}de{'Abbrechen'}", (short)MISort.Abort, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Abort()
        {
            if (!IsEnabledAbort())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMAbort);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMAbort);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Abort()", user);
        }

        public virtual bool IsEnabledAbort()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMAbort, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMAbort);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMAbort, this);
        }

        public static bool AskUserAbort(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50016", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }
        #endregion


        #region Stopping
        /// <summary>
        /// Brings the process from any state to Stopped-State
        /// Method is named "Stopp" (StopProcess) because Stop()-MEthod already exists in ACComponent
        /// </summary>
        [ACMethodInteraction("Process", "en{'Stop'}de{'Stoppen'}", (short)MISort.Stop, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Stopp()
        {
            if (!IsEnabledStopp())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMStopp);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMStopp);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
                user = Root.CurrentInvokingUser.VBUserName;
            Messages.LogDebug(this.GetACUrl(), "Stopp()", user);
        }

        public virtual bool IsEnabledStopp()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMStopp, this))
                    return false;
            }
            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMStopp);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMStopp, this);
        }

        public static bool AskUserStopp(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50015", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }

        #endregion


        #region Resetting
        /// <summary>
        /// Brings the process from Stopped- oder Aborted- or Completed-State to Idle-State
        /// </summary>
        [ACMethodInteraction("Process", "en{'Reset'}de{'Reset'}", 302, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Reset()
        {
            if (!IsEnabledReset())
                return;
            if (ACStateConverter != null)
                CurrentACState = ACStateConverter.GetNextACState(this, ACStateConst.TMReset);
            else
                CurrentACState = PAFuncStateConvBase.GetDefaultNextACState(CurrentACState, ACStateConst.TMReset);
            SetCurrentProgramLog(null, true);
            string user = "Service";
            if (Root.CurrentInvokingUser != null)
            {
                user = Root.CurrentInvokingUser.VBUserName;
                // If Reset invoke from Client, then Reset everything
                if (CurrentACState == ACStateEnum.SMIdle
                    && Root.CurrentInvokingUser.IsSuperuser
                    && Root.Environment.User != null
                    && Root.CurrentInvokingUser.VBUserID != Root.Environment.User.VBUserID)
                {
                    if (CurrentTask != null)
                        CurrentTask = null;
                    if (SavedACMethod != null)
                        SavedACMethod = null;
                }
            }
            Messages.LogDebug(this.GetACUrl(), "Reset()", user);
        }

        public virtual bool IsEnabledReset()
        {
            PAProcessModule parentModule = (ParentACComponent as PAProcessModule);
            if (parentModule != null)
            {
                if (!parentModule.IsEnabledTransition(ACStateConst.TMReset, this))
                    return false;
            }

            if (ACStateConverter != null)
                return ACStateConverter.IsEnabledTransition(this, ACStateConst.TMReset);
            else
                return PAFuncStateConvBase.IsEnabledTransitionDefault(CurrentACState, ACStateConst.TMReset, this);
        }

        public static bool AskUserReset(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50017", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }


        [ACMethodInteraction("Process", "en{'Reset and repeat'}de{'Reset und wiederholen'}", 303, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Reset2Repeat()
        {
            if (!IsEnabledReset2Repeat())
                return;
            _RepeatWFAfterReset = true;
            Reset();
        }

        public virtual bool IsEnabledReset2Repeat()
        {
            if (!IsEnabledReset())
                return false;
            return CurrentTask != null;
        }

        public static bool AskUserReset2Repeat(IACComponent acComponent)
        {
            return AskUserReset(acComponent);
        }

        #endregion


        #region Changing and Sending parameters
        /// <summary>
        /// Sends the parameters of CurrentACMethod to an external system (e.g. PLC)
        /// </summary>
        [ACMethodCommand("Process", "en{'Send changed Parameters'}de{'Sende geänderte Paramter'}", (short)MISort.Save, true)]
        public virtual void SendChangedACMethod()
        {
            if (!IsEnabledSendChangedACMethod())
                return;
            try
            {
                _IsMethodChangedFromClient = true;

                if (ReSendACMethod() == null)
                {
                    if (CurrentTask != null)
                    {
                        CurrentTask.ACMethod = CurrentACMethod.ValueT;
                        OnPropertyChanged("CurrentTask"); // Persist
                        ACPointAsyncRMIWrap<ACComponent> taskClone = ParentTaskExecComp.TaskInvocationPoint.ConnectionList.Where(c => c.RequestID == CurrentTask.ACMethod.ACRequestID).FirstOrDefault();
                        if (taskClone != null && taskClone != CurrentTask)
                        {
                            taskClone.ACMethod = CurrentTask.ACMethod;
                            ParentTaskExecComp.TaskInvocationPoint.Persist(true);
                        }
                    }
                }
            }
            finally
            {
                _IsMethodChangedFromClient = false;
            }
        }

        public virtual bool IsEnabledSendChangedACMethod()
        {
            if ((CurrentACState == ACStateEnum.SMRunning
                || CurrentACState == ACStateEnum.SMPaused
                || CurrentACState == ACStateEnum.SMHeld
                || CurrentACState == ACStateEnum.SMStopping
                || CurrentACState == ACStateEnum.SMStarting)
                && CurrentACMethod.ValueT != null)
            {
                return true;
            }
            return false;
        }

        public static bool AskUserSendChangedACMethod(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            IACPropertyNetBase netProperty = acComponent.GetMember("CurrentACMethod") as IACPropertyNetBase;
            if (netProperty == null)
                return false;
            netProperty.OnMemberChanged();
            return acComponent.Messages.Question(acComponent, "Question50023", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }
        #endregion

        #endregion

        #region internal methods

        #region Task-Handling
        protected bool _IsCallbackLocked = false;
        private int _CallbackTries = 0;
        /// <summary>
        /// Return value for AnalyzeACMethodResult to control if a callback to the invoking workflow-node should be done or not.
        /// </summary>
        public enum CompleteResult
        {
            /// <summary>
            /// Some values in the result are not valid. The user must decide what should happen. No callback is done and the function waits for a response of the user.
            /// </summary>
            FailedAndWait = 0,

            /// <summary>
            /// All result values are valid => Callback the invoking workflow-node.
            /// </summary>
            Succeeded = 1,

            /// <summary>
            /// Some values in the result are not valid. "ACStateConverter.ReceiveACMethodResult()" should be called again in the next cycle to get valid values from the external system (e.g. PLC).
            /// </summary>
            FailedAndRepeat = 2
        }


        /// <summary>
        /// This method is invoked from SMCompleted(), SMAborted() or SMStopped() to read the results from an external system and invoke the calback-method of the workwflow-node which started this asynchronous process.
        /// <br /> 1. Reads the result from a external system via ACStateConverter.ReceiveACMethodResult().
        /// <br /> 2. Checks if ReceiveACMethodResult() was sucessful by calling HandleFunctionErrorOnCallback().
        /// <br /> 3. Analyzes the result by calling AnalyzeACMethodResult().
        /// <br /> 4. Checks if AnalyzeACMethodResult() was sucessful by calling HandleFunctionErrorOnCallback() again.
        /// <br /> 5. Invokes CallbackTaskBeforeResetting().
        /// </summary>
        /// <param name="resultState"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        protected virtual CompleteResult CompleteResultAndCallback(Global.ACMethodResultState resultState)
        {
            using (ACMonitor.Lock(_20200_LockExecuteACStateMethod))
            {
                if (_IsCallbackLocked)
                    return CompleteResult.FailedAndRepeat;
                _IsCallbackLocked = true;
            }

            try
            {
                CompleteResult completeResult = CompleteResult.Succeeded;
                MsgWithDetails msgError = null;
                if (ACStateConverter != null)
                {
                    completeResult = ACStateConverter.ReceiveACMethodResult(this, CurrentACMethod.ValueT, out msgError);
                    completeResult = HandleFunctionErrorOnCallback(msgError, completeResult);
                    if (completeResult == CompleteResult.FailedAndWait)
                        return completeResult;
                }
                completeResult = AnalyzeACMethodResult(CurrentACMethod.ValueT, out msgError, completeResult);
                completeResult = HandleFunctionErrorOnCallback(msgError, completeResult);
                if (completeResult == CompleteResult.FailedAndWait)
                    return completeResult;
                else if (completeResult == CompleteResult.FailedAndRepeat)
                    resultState = Global.ACMethodResultState.FailedAndRepeat;

                CallbackTaskBeforeResetting(resultState);
                CurrentACMethod.ForceBroadcast = true;
                CurrentACMethod.ValueT = CurrentACMethod.ValueT;
                CurrentACMethod.ForceBroadcast = false;
            }
            catch (Exception e)
            {
                _CallbackTries++;
                Messages.LogException(this.GetACUrl(),
                                                        String.Format("CompleteResultAndCallback({0})", CurrentACState),
                                                        String.Format("Tries: {0}, Message: {1}, Trace: {2}", _CallbackTries, e.Message, e.StackTrace)
                                                     );
                if (_CallbackTries <= 3)
                    return CompleteResult.FailedAndRepeat;
            }
            finally
            {
                using (ACMonitor.Lock(_20200_LockExecuteACStateMethod))
                {
                    _IsCallbackLocked = false;
                }
            }
            return CompleteResult.Succeeded;
        }


        /// <summary>
        /// Handles the CompleteResult-State when ACStateConverter.ReceiveACMethodResult() and AnalyzeACMethodResult() was called.
        /// </summary>
        /// <param name="msgError"></param>
        /// <param name="completeResult"></param>
        /// <returns></returns>
        protected CompleteResult HandleFunctionErrorOnCallback(MsgWithDetails msgError, CompleteResult completeResult)
        {
            if (completeResult != CompleteResult.Succeeded)
            {
                if (msgError != null)
                {
                    if (FunctionError.ValueT == PANotifyState.Off && (IsAlarmActive(FunctionError, msgError.Message) == null))
                    {
                        Messages.LogError(this.GetACUrl(), "HandleFunctionErrorOnCallback(" + CurrentACState + ")", msgError.Message + " " + msgError.InnerMessage);
                        if (ACStateConverter != null && ACStateConverter.LoggingEnabled && CurrentACState == ACStateEnum.SMResetting)
                        {
                            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
                            Messages.LogException(this.GetACUrl(), "HandleFunctionErrorOnCallback(" + CurrentACState + ")", stackTrace.ToString());
                        }
                    }
                }
                FunctionError.ValueT = PANotifyState.AlarmOrFault;
                if (msgError != null)
                    OnNewAlarmOccurred(FunctionError, new Msg(msgError.Message, this, eMsgLevel.Exception, "PAProcessFunction", "HandleFunctionErrorOnCallback", 1000), true);
                return completeResult;
            }
            else if (FunctionError.ValueT == PANotifyState.AlarmOrFault)
                FunctionError.ValueT = PANotifyState.Off;
            return completeResult;
        }


        /// <summary>
        /// Invokes the callback-method of the workflow-node which started this asnychronous process.
        /// </summary>
        /// <param name="resultState"></param>
        protected virtual void CallbackTaskBeforeResetting(Global.ACMethodResultState resultState)
        {
            RecalcTimeInfo();
            if (CurrentTask != null)
            {
                FinishProgramLog(CurrentTask.ACMethod);
                if (ParentTaskExecComp != null)
                {
                    if (CurrentTask != null && CurrentACMethod.ValueT != null)
                    {
                        if (ParentTaskExecComp.CallbackTask(CurrentTask, CreateNewMethodEventArgs(CurrentTask.ACMethod, resultState)))
                            CurrentTask = null;
                    }
                    else if (CurrentACMethod.ValueT != null)
                    {
                        ParentTaskExecComp.CallbackTask(CurrentACMethod.ValueT, CreateNewMethodEventArgs(CurrentACMethod.ValueT, resultState));
                    }
                }
            }
        }
        #endregion


        #region Alarm-Handling
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (Malfunction.ValueT == PANotifyState.AlarmOrFault)
                AckMalfunction.ValueT = true;
            if (FunctionError.ValueT == PANotifyState.AlarmOrFault)
                FunctionError.ValueT = PANotifyState.Off;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if ((Malfunction.ValueT == PANotifyState.AlarmOrFault && !AckMalfunction.ValueT)
                || FunctionError.ValueT == PANotifyState.AlarmOrFault)
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }
        #endregion


        #region ACMethod-Handling
        /// <summary>
        /// Sends the parameters of CurrentACMethod to an external system (e.g. PLC)
        /// </summary>
        public MsgWithDetails ReSendACMethod(ACMethod newParameters = null)
        {
            if (newParameters != null)
            {
                OnChangingCurrentACMethod(CurrentACMethod.ValueT, newParameters);
                CurrentACMethod.ValueT = newParameters;
            }
            MsgWithDetails msgError = null;
            if (CurrentACMethod.ValueT == null)
            {
                if (CurrentTask != null)
                    CurrentACMethod.ValueT = CurrentTask.ACMethod;
                else if (SavedACMethod != null)
                    CurrentACMethod.ValueT = SavedACMethod;
            }

            if (CurrentACMethod.ValueT == null)
            {
                msgError = new MsgWithDetails() { Message = "CurrentACMethod is null" };
                Messages.LogError(this.GetACUrl(), "ReSendACMethod(0)", msgError.Message);
            }
            else
                msgError = CompleteACMethodOnSMStarting(CurrentACMethod.ValueT);
            if (msgError != null)
            {
                if (FunctionError.ValueT == PANotifyState.Off)
                    Messages.LogException(this.GetACUrl(), "ReSendACMethod(1)", msgError.Message + " " + msgError.InnerMessage);
                FunctionError.ValueT = PANotifyState.AlarmOrFault;
                OnNewAlarmOccurred(FunctionError, new Msg(msgError.Message, this, eMsgLevel.Exception, "PAProcessFunction", "ReSendACMethod", 1010), true);
                return msgError;
            }

            if (ACStateConverter != null)
            {
                msgError = ACStateConverter.SendACMethod(this, CurrentACMethod.ValueT);
                if (msgError != null && !IsSimulationOn)
                {
                    if (FunctionError.ValueT == PANotifyState.Off)
                        Messages.LogException(this.GetACUrl(), "ReSendACMethod(2)", msgError.Message + " " + msgError.InnerMessage);
                    FunctionError.ValueT = PANotifyState.AlarmOrFault;
                    OnNewAlarmOccurred(FunctionError, new Msg(msgError.Message, this, eMsgLevel.Exception, "PAProcessFunction", "ReSendACMethod", 1020), true);
                    return msgError;
                }
            }

            PersistCurrentACMethod();

            if (FunctionError.ValueT == PANotifyState.AlarmOrFault)
                FunctionError.ValueT = PANotifyState.Off;
            return null;
        }


        /// <summary>
        /// Informs derivations that ReSendACMethod() was called.
        /// </summary>
        /// <param name="currentACMethod"></param>
        /// <param name="newACMethod"></param>
        protected virtual void OnChangingCurrentACMethod(ACMethod currentACMethod, ACMethod newACMethod)
        {
        }


        /// <summary>
        /// Persists the CurrentACMethod-Property
        /// </summary>
        protected void PersistCurrentACMethod()
        {
            if (SavedACMethod != null)
                SavedACMethod = CurrentACMethod.ValueT;
            else if (CurrentTask != null)
            {
                if (CurrentACMethod.ValueT != null && CurrentTask.ACMethod != CurrentACMethod.ValueT)
                    CurrentTask.ACMethod = CurrentACMethod.ValueT;
                var prop = GetProperty("CurrentTask");
                if (prop != null)
                    prop.Persist();
            }
        }
        #endregion

        #endregion

        #endregion


        #region Planning and Testing
        public virtual ACMethodEventArgs CreateNewMethodEventArgs(ACMethod acMethod, Global.ACMethodResultState state)
        {
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, state);
            RecalcTimeInfo();
            ACValue durationValue = result.GetACValue("TimeInfo");
            if (durationValue == null)
            {
                durationValue = new ACValue("TimeInfo", typeof(PATimeInfo));
                try
                {
                    result.Add(durationValue);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("PAProcessFunction", "CreateNewMethodEventArgs", msg);
                }
            }

            durationValue.Value = TimeInfo.ValueT;

            return result;
        }
        #endregion

        #endregion
    }
}
