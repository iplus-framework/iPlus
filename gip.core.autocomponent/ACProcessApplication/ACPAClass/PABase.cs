using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for immaterial objects like functions or workflow-classes
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PAClassAlarmingBase" />
    /// <seealso cref="gip.core.autocomponent.IACWorkCycleWithACState" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PABase'}de{'PABase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Optional, false, false)]
    public abstract class PABase : PAClassAlarmingBase, IACWorkCycleWithACState
    {
        #region c´tors
        public PABase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            (ACState as IACPropertyNetServer).ValueUpdatedOnReceival += ACState_PropertyChanged;
            return true;
        }

        public override bool ACPostInit()
        {
            ACProgramLog programLog = CurrentProgramLog;
            if (programLog != null)
                TimeInfo.ValueT = new PATimeInfo(programLog);
            else
                TimeInfo.ValueT = new PATimeInfo();

            if (_ACStateMethod == null && CurrentACState == ACStateEnum.SMIdle && !ACState.InRestorePhase)
                _ACStateMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == ACStateConst.SMIdle);

            bool result = base.ACPostInit();

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            UnSubscribeToProjectWorkCycle();
            (ACState as IACPropertyNetServer).ValueUpdatedOnReceival -= ACState_PropertyChanged;
            bool result = base.ACDeInit(deleteACClassTask);
            if (deleteACClassTask)
            {
                if (TimeInfo.ValueT != null)
                    TimeInfo.ValueT.Reset();
                if (ACState.ValueT != ACStateEnum.SMIdle)
                    ACState.ValueT = ACStateEnum.SMIdle;
            }
            _ACStateMethod = null;
            _LastACStateMethod = null;
            return result;
        }

        #endregion

        #region Properties Range 200

        #region Common Properties
        protected readonly ACMonitorObject _20200_LockExecuteACStateMethod = new ACMonitorObject(20200);
        /// <summary>
        /// Lock for accessing local fields that are entity framework objects and were materialized from ACClassTaskQueue.TaskQueue.Context
        /// Use this whenever you want to read a entity framework object
        /// </summary>
        /// <value>
        /// The context lock for ac class wf.
        /// </value>
        public ACMonitorObject ContextLockForACClassWF
        {
            get
            {
                return (Content == null || Content is ACClassWF) ?
                      gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000
                    : ACClassTaskQueue.TaskQueue.Context.QueryLock_1X000;
            }
        }

        #endregion

        #region ACState


        /// <summary>
        /// The state of a function or workflownode
        /// </summary>
        [ACPropertyBindingSource(200, "", "en{'State general'}de{'Zustand Allgemein'}", "", true, true, DefaultValue = ACStateEnum.SMIdle, RemotePropID = 13)]
        public IACContainerTNet<ACStateEnum> ACState { get; set; }

        /// <summary>
        /// Called when ACState changes
        /// </summary>
        /// <param name="valueEvent">The value event.</param>
        public virtual void OnSetACState(IACPropertyNetValueEvent valueEvent)
        {
            ACPropertyValueEvent<ACStateEnum> valueEventT = valueEvent as ACPropertyValueEvent<ACStateEnum>;
            if (valueEventT.Sender == EventRaiser.Target)
            {
                valueEventT.Handled = true;
                return;
            }
            if (   (_ACStateMethod == null) 
                || (ACStateConst.GetEnum(_ACStateMethod.ACIdentifier) != valueEventT.Value))
            {
                if (ACState.InRestorePhase && !Root.Initialized)
                {
                    AddToPostInitQueue(delegate
                    {
                        SyncACStateWithACStateMethod(valueEventT.Value, valueEventT);
                    });
                    // Änderung am 21.11.2013: Restore von ACState wurde nicht durchgeführt. TODO: Check im Zusammenhang mit Workflows
                    //valueEventT.Handled = true;
                    valueEventT.Handled = false;
                    return;
                }
                string acStateString = ACStateConst.ToString(valueEventT.Value);
                ACClassMethod acClassMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == acStateString);
                if (acClassMethod == null)
                {
                    if (acClassMethod == null)
                    {
                        Messages.LogError(this.GetACUrl(), "OnSetACState(1)", String.Format("Method {0} not found", valueEventT.Value));
                        return;
                    }
                    valueEventT.Handled = true;
                    return;
                }
                // Muss immer ein [ACMethodState(..)] sein
                if (acClassMethod.ACGroup != Const.ACState)
                {
                    valueEventT.Handled = true;
                    return;
                }
                if (acClassMethod == null)
                {
                    valueEventT.Handled = true;
                    return;
                }
                if (!IsEnabledExecuteACMethod(ACStateConst.ToString(valueEventT.Value), null))
                {
                    valueEventT.Handled = true;
                    return;
                }
            }
        }


        /// <summary>
        /// Eventhandler for the ValueUpdatedOnReceival-Event of the ACState-Property
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ACPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <param name="phase">The phase.</param>
        protected virtual void ACState_PropertyChanged(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.BeforeBroadcast || ACState.InRestorePhase)
                return;
            ACPropertyValueEvent<ACStateEnum> valueEventT = e.ValueEvent as ACPropertyValueEvent<ACStateEnum>;
            if (valueEventT.Sender == EventRaiser.Target)
                return;
           
            //(ACState as IACPropertyNetSource).LogACStateChange(41, ACStateConst.ToString(valueEventT.Value));

            bool inconsistentStateButForce = valueEventT.Value != ACState.ValueT;
            if (inconsistentStateButForce)
            {
                // Added 20.07.2017
                Messages.LogError(this.GetACUrl(),
                                            "ACState_PropertyChanged(1)",
                                            String.Format("valueEventT.Value {0} was changed in the meantime to ACState.ValueT {1}", valueEventT.Value, ACState.ValueT));
            }
            SyncACStateWithACStateMethod(valueEventT.Value, valueEventT, inconsistentStateButForce);
        }


        /// <summary>
        /// Synchonizes the ACState-Property with the corresponding State-Method, that should be invoked when ACState has changed
        /// </summary>
        /// <param name="acStateValue"></param>
        /// <param name="valueEvent"></param>
        /// <param name="inconsistentStateButForce"></param>
        private void SyncACStateWithACStateMethod(ACStateEnum acStateValue, ACPropertyValueEvent<ACStateEnum> valueEvent, bool inconsistentStateButForce = false)
        {
            //if (string.IsNullOrEmpty(acStateValue))
            //    return;

            string acStateString = ACStateConst.ToString(acStateValue);
            ACClassMethod acClassMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == acStateString);
            if (acClassMethod == null)
            {
                Messages.LogError(this.GetACUrl(), "SyncACStateWithACStateMethod(2)", String.Format("Method {0} not found", acStateValue));
                return;
            }

            //(ACState as IACPropertyNetSource).LogACStateChange(42, ACStateConst.ToString(acStateValue));

            // Falls InvokerInfo nicht null, dann ist der Aufruf vom SPS-Thread gemacht worden => delegiere auf anderen Thread wegen besserer Performance
            if (valueEvent != null && valueEvent.InvokerInfo != null)
            {
                // Falls sich der Status in der Zwischenzeit geändert hat weil ein anderer Thread diesem zuvorgekommen ist,
                // dann führe Methode nicht aus
                // Changed 20.07.2017
                if (ACState.ValueT != acStateValue && !inconsistentStateButForce)
                {
                    Messages.LogError(this.GetACUrl(),
                                                "SyncACStateWithACStateMethod(1)",
                                                String.Format("Method {0} not executed because ACState was changed in the meantime to {1}", acStateValue, ACState.ValueT));
                    return;
                }

                this.ApplicationManager.ApplicationQueue.Add(() =>
                {
                    DelegateInvokeACStateMethod(acStateValue, valueEvent, acClassMethod);
                    //try
                    //{
                    //}
                    //catch (Exception qEx)
                    //{
                    //    Messages.LogException(this.GetACUrl(), "SyncACStateWithACStateMethod(3)", qEx.Message);
                    //    if (qEx.InnerException != null)
                    //        Messages.LogException(this.GetACUrl(), "SyncACStateWithACStateMethod(3)", qEx.InnerException.Message);
                    //}
                });
            }
            else
            {
                DelegateInvokeACStateMethod(acStateValue, valueEvent, acClassMethod);
            }
        }

        private void DelegateInvokeACStateMethod(ACStateEnum acStateValue, ACPropertyValueEvent<ACStateEnum> valueEvent, ACClassMethod acClassMethod)
        {
            using (ACMonitor.Lock(_20200_LockExecuteACStateMethod))
            {
                _LastACStateMethod = _ACStateMethod;
                _ACStateMethod = acClassMethod;
                _InconsistentACState = 0;

                //(ACState as IACPropertyNetSource).LogACStateChange(43, ACStateConst.ToString(acStateValue));

                var vbDump = Root.VBDump;
                Regex rgx = new Regex("\\((.*?)\\)");
                string loggerInstance = String.Format("{0}_{1}", rgx.Replace(this.GetACUrl(), ""), _ACStateMethod.ACIdentifier);
                PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(loggerInstance, 101) : null;

                InvokeACStateMethod(acClassMethod);

                if (perfEvent != null)
                    vbDump.PerfLoggerStop(loggerInstance, 101, perfEvent);
            }
        }


        ACClassMethod _ACStateMethod = null;
        /// <summary>
        /// Name of the State-Method, that is currently active accordinng to the ACState-Property
        /// </summary>
        /// <value>
        /// The current ac state method.
        /// </value>
        [ACPropertyInfo(9999)]
        public string CurrentACStateMethod
        {
            get
            {
                if (_ACStateMethod == null)
                    return null;
                return _ACStateMethod.ACIdentifier;
            }
        }

        ACClassMethod _LastACStateMethod = null;
        /// <summary>
        /// Previous ACState
        /// </summary>
        [ACPropertyInfo(9999)]
        public ACStateEnum LastACState
        {
            get
            {
                if (_LastACStateMethod == null)
                    return ACStateEnum.SMIdle;
                return ACStateConst.GetEnum(_LastACStateMethod.ACIdentifier);
            }
        }

        /// <summary>
        /// Curent value of the ACState-Property
        /// </summary>
        /// <value>
        /// The state of the current ac.
        /// </value>
        public ACStateEnum CurrentACState
        {
            get
            {
                return ACState.ValueT;
            }
            protected set
            {
                ACState.ValueT = value;
            }
        }


        /// <summary>
        /// Override this method if you want to react before the corresponding State-Method is invoked.
        /// </summary>
        /// <param name="acClassMethod"></param>
        public virtual void InvokeACStateMethod(ACClassMethod acClassMethod)
        {
            ExecuteMethod(acClassMethod.ACIdentifier, null);
        }

        public enum ACStateCompare : short
        {
            Consistent = 0,
            FallbackToPrevState = 1,
            Different = 2,
            WrongACStateMethod = 3
        }

        protected ACStateCompare IsACStateMethodConsistent(ACStateEnum invokingACMethod)
        {
            ACStateEnum currentACState = CurrentACState;
            if (currentACState == invokingACMethod || _ACStateMethod == null)
                return ACStateCompare.Consistent;
            if (ACStateConst.GetEnum(_ACStateMethod.ACIdentifier) == invokingACMethod)
            {
                if (_LastACStateMethod == null || ACStateConst.GetEnum(_LastACStateMethod.ACIdentifier) == currentACState)
                {
                    Messages.LogError(this.GetACUrl(), "IsACStateMethodConsistent(FallbackToPrevState)",
                                                String.Format("invokingACMethod is {0}, currentACState is {1}, _ACStateMethod is {2}, _LastACStateMethod {3}",
                                                invokingACMethod,
                                                ACStateConst.ToString(currentACState), 
                                                _ACStateMethod == null ? "null" : _ACStateMethod.ACIdentifier, 
                                                _LastACStateMethod == null ? "null" : _LastACStateMethod.ACIdentifier));
                    return ACStateCompare.FallbackToPrevState;
                }
                else
                {
                    Messages.LogError(this.GetACUrl(), "IsACStateMethodConsistent(Different)",
                                                String.Format("invokingACMethod is {0}, currentACState is {1}, _ACStateMethod is {2}, _LastACStateMethod {3}",
                                                invokingACMethod,
                                                ACStateConst.ToString(currentACState),
                                                _ACStateMethod == null ? "null" : _ACStateMethod.ACIdentifier,
                                                _LastACStateMethod == null ? "null" : _LastACStateMethod.ACIdentifier));
                    return ACStateCompare.Different;
                }
            }
            Messages.LogError(this.GetACUrl(), "IsACStateMethodConsistent(WrongACStateMethod)",
                                        String.Format("invokingACMethod is {0}, currentACState is {1}, _ACStateMethod is {2}, _LastACStateMethod {3}",
                                        invokingACMethod,
                                        ACStateConst.ToString(currentACState),
                                        _ACStateMethod == null ? "null" : _ACStateMethod.ACIdentifier,
                                        _LastACStateMethod == null ? "null" : _LastACStateMethod.ACIdentifier));
            return ACStateCompare.WrongACStateMethod;
        }

#endregion

#region TimeInfo
        [ACPropertyBindingSource(201, "", "en{'Time information'}de{'Zeitinformation'}", "", true, false)]
        public IACContainerTNet<PATimeInfo> TimeInfo { get; set; }
        #endregion

        #region ACProgram        
        /// <summary>
        /// The current program log of this node
        /// </summary>
        /// <value>
        /// The current program log.
        /// </value>
        public abstract ACProgramLog CurrentProgramLog { get; }

        /// <summary>
        /// Restores the CurrentProgramLog-Property from the database.
        /// </summary>
        /// <param name="attach">if set to <c>true</c> [attach].</param>
        /// <returns></returns>
        protected abstract ACProgramLog GetCurrentProgramLog(bool attach);

        /// <summary>
        /// Sets or replaces the CurrentProgramLog-Property with the passed ACProgramLog. 
        /// If there was a previous ProgramLog it will be removed from the ProgramLog-Cache (ACClassTaskQueue.TaskQueue.ProgramCache)
        /// The new ProgramLog will be added to the ProgramLog-Cache.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="detach">if set to <c>true</c> [detach].</param>
        protected abstract void SetCurrentProgramLog(ACProgramLog value, bool detach);


        internal virtual string DumpReasonCurrentPLIsNull()
        {
            return String.Format("CurrentACState: {0} @ {1}", this.CurrentACState, this.GetACUrl());
        }


        /// <summary>
        /// Reads the previous program logs of this workflow-instance
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public abstract IEnumerable<ACProgramLog> PreviousProgramLogs { get; }


        /// <summary>
        /// Gets the parent program log.
        /// </summary>
        /// <value>
        /// The parent program log.
        /// </value>
        public abstract ACProgramLog ParentProgramLog { get; }


        /// <summary>
        /// Reads the previous PARENT program logs of this workflow-instance
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public abstract ACProgramLog PreviousParentProgramLog { get; }
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
                case "SubscribeToProjectWorkCycle":
                    SubscribeToProjectWorkCycle();
                    return true;
                case "UnSubscribeToProjectWorkCycle":
                    UnSubscribeToProjectWorkCycle();
                    return true;
                case "DoWork":
                    DoWork();
                    return true;
                case "SimulateGoToNextState":
                    SimulateGoToNextState();
                    return true;
                case Const.IsEnabledPrefix + "SubscribeToProjectWorkCycle":
                    result = IsEnabledSubscribeToProjectWorkCycle();
                    return true;
                case Const.IsEnabledPrefix + "UnSubscribeToProjectWorkCycle":
                    result = IsEnabledUnSubscribeToProjectWorkCycle();
                    return true;
                case Const.IsEnabledPrefix + "DoWork":
                    result = IsEnabledDoWork();
                    return true;
                case Const.IsEnabledPrefix + "SimulateGoToNextState":
                    result = IsEnabledSimulateGoToNextState();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        /// <summary>
        /// This method is called when ACState goes back to the Idle-State. 
        /// Override this method to reset all necessary private fiels or properties in your class.
        /// </summary>
        [ACMethodState("en{'Idle'}de{'Leerlauf'}", 10, true)]
        public virtual void SMIdle()
        {
            RecalcTimeInfo(); // Reset TimeInfo 
            _SimTriggerGoToNextState = false;
            _SimulationWait = 0;
        }
#endregion

#region Cycle-Work-Methods
        private bool _SubscribedToWorkCycle = false;
        /// <summary>
        /// Gets a value indicating whether this instance is subscribed to work cycle.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is subscribed to work cycle; otherwise, <c>false</c>.
        /// </value>
        public bool IsSubscribedToWorkCycle
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _SubscribedToWorkCycle;
                }
            }
        }

        /// <summary>
        /// Subscribes to the work cycle (ProjectWorkCycleR1sec) of the application manager.
        /// The State-Methods will be called cyclic after subscription.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Cyclic processing ON'}de{'Zyklische Abarbeitung AN'}", (short)303, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void SubscribeToProjectWorkCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (!IsEnabledSubscribeToProjectWorkCycle())
                    return;
                ApplicationManager.ProjectWorkCycleR1sec += objectManager_ProjectWorkCycle;
                _SubscribedToWorkCycle = true;
            }
        }

        public virtual bool IsEnabledSubscribeToProjectWorkCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (   _SubscribedToWorkCycle 
                    || InitState < ACInitState.Reloading
                    || InitState >= ACInitState.Destructing)
                    return false;
                // Access to ApplicationManager not in OR-Condition above to avoid Attachment to ACRef of ParentComponent!
                if (ApplicationManager == null)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Unsubscribes from the work cycle (ProjectWorkCycleR1sec) of the application manager.
        /// The State-Methods won't be called cyclic any more.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Cyclic processing OFF'}de{'Zyklische Abarbeitung AUS'}", 303, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void UnSubscribeToProjectWorkCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (!IsEnabledUnSubscribeToProjectWorkCycle())
                    return;
                if (this.ACRef == null)
                    return;
                bool wasDetached = !this.ACRef.IsAttached;
                if (ApplicationManager != null)
                    ApplicationManager.ProjectWorkCycleR1sec -= objectManager_ProjectWorkCycle;
                _SubscribedToWorkCycle = false;
                if (wasDetached)
                    this.ACRef.Detach();
            }
        }

        public bool IsEnabledUnSubscribeToProjectWorkCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (!_SubscribedToWorkCycle)
                    return false;
                return true;
            }
        }

        protected virtual void objectManager_ProjectWorkCycle(object sender, EventArgs e)
        {
            if (this.InitState == ACInitState.Destructed || this.InitState == ACInitState.DisposingToPool || this.InitState == ACInitState.DisposedToPool)
            {
                gip.core.datamodel.Database.Root.Messages.LogError("PABase", "objectManager_ProjectWorkCycle(1)", String.Format("Unsubcribed from Workcycle. Init-State is {0}, _SubscribedToWorkCycle is {1}, at Type {2}. Ensure that you unsubscribe from Work-Cycle in ACDeinit().", this.InitState, _SubscribedToWorkCycle, this.GetType().AssemblyQualifiedName));

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    (sender as ApplicationManager).ProjectWorkCycleR1sec -= objectManager_ProjectWorkCycle;
                    _SubscribedToWorkCycle = false;
                }
                return;
            }
            DoWork();
        }

        private int _InconsistentACState = 0;
        public bool IsACStateInconsistent
        {
            get
            {
                return _ACStateMethod != null && ACStateConst.GetEnum(_ACStateMethod.ACIdentifier) != CurrentACState;
            }
        }


        /// <summary>
        /// Invokes the current state-methode.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Execute current state method'}de{'Führe aktuelle Statusmethode aus'}", (short)304, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public void DoWork()
        {
            if (_ACStateMethod == null || !_ACStateMethod.IsPeriodic)
                return;
            ACStateEnum paStateBeforeLock = ACState.ValueT;
            if (paStateBeforeLock != ACStateConst.GetEnum(_ACStateMethod.ACIdentifier))
            {
                //if (_InconsistentACState == 0)
                //{
                //    Messages.LogError(this.GetACUrl(),
                //                                "DoWork(1)",
                //                                String.Format("_ACStateMethod {0} is different to ACState.ValueT {1}", _ACStateMethod.ACIdentifier, ACState.ValueT));
                //}
                _InconsistentACState++;
                if (_InconsistentACState < 3)
                    return;
                Messages.LogError(this.GetACUrl(),
                                            "DoWork(1)",
                                            String.Format("_ACStateMethod {0} is different to ACState.ValueT {1}", _ACStateMethod.ACIdentifier, ACState.ValueT));
            }
            else if (_InconsistentACState != 0)
                _InconsistentACState = 0;
            bool lockGet = false;
            //short canEnterCS = 0;
            try
            {
                // Zyklische Methoden haben niederere Priorität als Aufrufe die durch Property-Changd-Ändeungen aus SyncACStateWithACStateMethod() ausgeführt worden sind
                lockGet = ACMonitor.TryEnter(_20200_LockExecuteACStateMethod, 1);
                if (lockGet)
                {
                    if (_InconsistentACState != 0)
                    {
                        string acStateBeforeLock = ACStateConst.ToString(paStateBeforeLock);
                        _InconsistentACState = 0;
                        ACClassMethod acClassMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == acStateBeforeLock);
                        if (acClassMethod != null)
                        {
                            Messages.LogError(this.GetACUrl(),
                                                        "DoWork(2)",
                                                        String.Format("_ACStateMethod corrected to {0}, wrong state was {1}, last state was {2}", acClassMethod.ACIdentifier, _ACStateMethod.ACIdentifier, _LastACStateMethod == null ? "" : _LastACStateMethod.ACIdentifier));
                            _ACStateMethod = acClassMethod;
                        }
                    }
                    // Falls sich der Status in der Zwischenzeit geändert hat weil ein anderer Thread diesem zuvorgekommen ist,
                    // dann führe Methode nicht aus
                    if (   ACState.ValueT == paStateBeforeLock 
                        && _ACStateMethod != null 
                        && ACStateConst.GetEnum(_ACStateMethod.ACIdentifier) == paStateBeforeLock
                        && IsSubscribedToWorkCycle)
                    {
                        var vbDump = Root.VBDump;
                        Regex rgx = new Regex("\\((.*?)\\)");
                        string loggerInstance = String.Format("{0}_{1}", rgx.Replace(this.GetACUrl(), ""), _ACStateMethod.ACIdentifier);
                        PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(loggerInstance, 100) : null;

                        _LastACStateMethod = _ACStateMethod;
                        InvokeACStateMethod(_ACStateMethod);

                        if (perfEvent != null)
                            vbDump.PerfLoggerStop(loggerInstance, 100, perfEvent);
                    }
                }
            }
            finally
            {
                if (lockGet)
                    ACMonitor.Exit(_20200_LockExecuteACStateMethod);
            }
        }

        public bool IsEnabledDoWork()
        {
            return IsEnabledSubscribeToProjectWorkCycle();
        }

        #endregion

        #region Planning and Testing

        /// <summary>
        /// Calculates scheduling times and current live times depending on the current ACState and ACOperationModes
        /// </summary>
        /// <returns>TimeInfo.ValueT</returns>
        [ACMethodInfo("", "", 9999)]
        protected virtual PATimeInfo RecalcTimeInfo(bool resetStartTime = false)
        {
            if (TimeInfo.ValueT == null)
            {
                var timeInfo = new PATimeInfo();
                timeInfo.PlannedTimes.ChangeTime(DateTime.Now, new TimeSpan(0, 1, 0));
                timeInfo.ActualTimes.ChangeTime(DateTime.Now, new TimeSpan(0, 1, 0));
                TimeInfo.ValueT = timeInfo;
                return timeInfo;
            }
            switch (CurrentACState)
            {
                case ACStateEnum.SMIdle:
                    if (TimeInfo.ValueT != null)
                        TimeInfo.ValueT.Reset();
                    break;
                case ACStateEnum.SMStarting:
                    if (TimeInfo.ValueT.PlannedTimes.IsNull || !TimeInfo.ValueT.PlannedTimes.StartTimeValue.HasValue)
                    {
                        DateTime plannedStartTime = GetPlannedStartTime();
                        TimeSpan plannedDuration = GetPlannedDuration();
                        TimeInfo.ValueT.PlannedTimes.ChangeTime(plannedStartTime, plannedDuration);
                    }

                    if (TimeInfo.ValueT.ActualTimes.IsNull || !TimeInfo.ValueT.ActualTimes.StartTimeValue.HasValue)
                    {
                        if (ACOperationMode == ACOperationModes.Live)
                            TimeInfo.ValueT.ActualTimes.ChangeTime(DateTime.Now, TimeSpan.Zero);
                        else
                            TimeInfo.ValueT.ActualTimes.ChangeTime(CurrentPointInTime, TimeInfo.ValueT.PlannedTimes.Duration);
                    }
                    break;
                case ACStateEnum.SMCompleted:
                case ACStateEnum.SMAborted:
                    if (ACOperationMode == ACOperationModes.Live)
                    {
                        TimeInfo.ValueT.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue ? DateTime.Now : TimeInfo.ValueT.ActualTimes.StartTime, DateTime.Now);
                    }
                    else
                    {
                        if (!TimeInfo.ValueT.ActualTimes.DurationValue.HasValue)
                        {
                            TimeInfo.ValueT.ActualTimes.ChangeTime(CurrentPointInTime, GetPlannedDuration());
                        }
                    }
                    break;
                default:
                    if (ACOperationMode == ACOperationModes.Live)
                    {
                        TimeInfo.ValueT.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue || resetStartTime ? DateTime.Now : TimeInfo.ValueT.ActualTimes.StartTime, DateTime.Now);
                    }
                    break;
            }
            return TimeInfo.ValueT;
        }

        protected virtual DateTime GetPlannedStartTime()
        {
            if (ACOperationMode != ACOperationModes.Live) // Falls Simulation
            {
                return CurrentPointInTime;
            }
            else
            {
                // TODO: DateTime.Now ersetzen durch zugriff auf Planungsdaten wenn Planung programmiert wird
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Must be overwritten in the derivatives and the duration calculated using ACMethod and other parameters (e.B. quantity per unit of time, temperature increase...)
        /// </summary>
        /// <returns></returns>
        protected virtual TimeSpan GetPlannedDuration()
        {
            return new TimeSpan(0, 0, new Random().Next(5, 15)); // Muss in den Ableitungen überschrieben werden
        }

        /// <summary>
        /// Returns the current time.
        /// If ACOperationModes.Live, then DateTime.Now will be returned
        /// If in Simulation, the simulated time from ACPointEventAbsorber will be returned.
        /// </summary>
        public DateTime CurrentPointInTime
        {
            get
            {
                DateTime pointInTime = DateTime.Now;
                if (ACOperationMode != ACOperationModes.Live)
                {
                    ACPointEventAbsorber eventAbsorber = (Root as ACRoot).EventAbsorber;
                    if (eventAbsorber != null)
                        pointInTime = eventAbsorber.CurrentSimulationTime;
                }
                return pointInTime;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance runs in simulation mode and the states will be changed automatically by the simulatorlogic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is simulation on; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSimulationOn
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Live)
                    return true;
                if (ApplicationManager == null)
                    return false;
                return ApplicationManager.IsSimulationOn;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance runs in simulation mode and the states will be changed manually by a operator.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is manual simulation; otherwise, <c>false</c>.
        /// </value>
        public bool IsManualSimulation
        {
            get
            {
                if (ACOperationMode != ACOperationModes.Live)
                    return true;
                if (ApplicationManager == null)
                    return false;
                return ApplicationManager.IsManualSimulation;
            }
        }

        protected int _SimulationWait = 0;
        protected bool _SimTriggerGoToNextState = false;
        protected virtual bool CyclicWaitIfSimulationOn()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (!IsSimulationOn)
            {
                _SimulationWait = 0;
                return false;
            }

            if (IsManualSimulation)
            {
                _SimulationWait = 0;
                if (_SimTriggerGoToNextState)
                {
                    _SimTriggerGoToNextState = false;
                    UnSubscribeToProjectWorkCycle();
                    return false;
                }
                else
                    return true;
            }
            else if (_SimulationWait <= 0)
                SubscribeToProjectWorkCycle();

            _SimTriggerGoToNextState = false;
            _SimulationWait++;
            Random random = new Random();
            int duration = random.Next(5, 15);
            //int duration = 300;
            if (_SimulationWait < duration)
                return true;
            UnSubscribeToProjectWorkCycle();
            _SimulationWait = 0;
            return false;
        }

        /// <summary>
        /// Switches to the next state if in manual simulation.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Simulate go to next state'}de{'Simulation: Nächster Zustand'}", 299, true)]
        public void SimulateGoToNextState()
        {
            if (!IsEnabledSimulateGoToNextState())
                return;
            _SimTriggerGoToNextState = true;
            SubscribeToProjectWorkCycle();
        }

        public bool IsEnabledSimulateGoToNextState()
        {
            return IsSimulationOn && IsManualSimulation && CurrentACState != ACStateEnum.SMIdle && !_SimTriggerGoToNextState;
        }

#endregion

#region Diagnostics and Dump
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement xmlSubscribedToWorkCycle = xmlACPropertyList["_SubscribedToWorkCycle"];
            if (xmlSubscribedToWorkCycle == null)
            {
                xmlSubscribedToWorkCycle = doc.CreateElement("_SubscribedToWorkCycle");
                if (xmlSubscribedToWorkCycle != null)
                    xmlSubscribedToWorkCycle.InnerText = IsSubscribedToWorkCycle.ToString();
                xmlACPropertyList.AppendChild(xmlSubscribedToWorkCycle);
            }

            XmlElement xmlSimulationWait = xmlACPropertyList["_SimulationWait"];
            if (xmlSimulationWait == null)
            {
                xmlSimulationWait = doc.CreateElement("_SimulationWait");
                if (xmlSimulationWait != null)
                    xmlSimulationWait.InnerText = _SimulationWait.ToString();
                xmlACPropertyList.AppendChild(xmlSimulationWait);
            }

            //XmlElement xmlACStateMethod = xmlACPropertyList["_ACStateMethod"];
            //if (xmlACStateMethod == null)
            //{
            //    xmlACStateMethod = doc.CreateElement("_ACStateMethod");
            //    if (xmlACStateMethod != null)
            //        xmlACStateMethod.InnerText = _ACStateMethod == null ? "null" : _ACStateMethod.ACIdentifier;
            //    xmlACPropertyList.AppendChild(xmlACStateMethod);
            //}

            //XmlElement xmlLastACStateMethod = xmlACPropertyList["_LastACStateMethod"];
            //if (xmlLastACStateMethod == null)
            //{
            //    xmlLastACStateMethod = doc.CreateElement("_LastACStateMethod");
            //    if (xmlLastACStateMethod != null)
            //        xmlLastACStateMethod.InnerText = _LastACStateMethod == null ? "null" : _LastACStateMethod.ACIdentifier;
            //    xmlACPropertyList.AppendChild(xmlLastACStateMethod);
            //}

        }
        #endregion

        #region ACProgram
        public enum CreateNewProgramLogResult
        {
            ErrorCurrentNoNull = -1,
            ErrorNoProgramFound = 0,
            NewOneCreated = 1,
            CurrentKept = 2,
        }

        /// <summary>
        /// Creates the new program log.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <param name="forceCreateNew">if set to <c>true</c> a new ACProgramLog will be created and the current replaced with the new one.</param>
        /// <param name="checkCurrentMustBeNull">if set to <c>true</c> checks if current alread exist. If yes ErrorCurrentNoNull will be returned.</param>
        /// <returns></returns>
        protected CreateNewProgramLogResult CreateNewProgramLog(ACMethod acMethod, bool forceCreateNew = false, bool checkCurrentMustBeNull = false)
        {
            string messageText = null;
            Msg msg = null;
            if (forceCreateNew)
                SetCurrentProgramLog(null, true);
            else
            {
                ACProgramLog currentLog = GetCurrentProgramLog(true);
                if (currentLog != null)
                {
                    if (checkCurrentMustBeNull)
                    {
                        messageText = "Can't create new Program-Log! GetCurrentProgramLog(false) is not null with value: " + currentLog.ACProgramLogID.ToString();
                        msg = new Msg(messageText, this, eMsgLevel.Error, PWNodeProcessWorkflow.PWClassName, "CreateNewProgramLog()", 1000);
                        if (IsAlarmActive(TimeInfo, msg.Message) == null)
                        {
                            Messages.LogMessageMsg(msg);
                            OnNewAlarmOccurred(TimeInfo, msg, true);
                        }
                        return CreateNewProgramLogResult.ErrorCurrentNoNull;
                    }
                    return CreateNewProgramLogResult.CurrentKept;
                }
            }
            ACProgramLog parentProgramLog = ParentProgramLog;
            ACProgram acProgram = null;
            // Falls Root-Knoten, dann is ParentProgramLog null => neuer ProgramLog muss über ACProgram angelegt werden
            if (parentProgramLog == null && this is PWProcessFunction)
            {
                PWProcessFunction pwFunction = this as PWProcessFunction;
                acProgram = pwFunction.CurrentACProgram;
            }
            else if (parentProgramLog != null)
            {
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                {
                    acProgram = parentProgramLog.ACProgram;
                });
            }

            if (acProgram != null)
            {
                ACProgramLog currentProgramLog = null;
                Guid componentClassID = this.ComponentClass.ACClassID;
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        currentProgramLog = ACProgramLog.NewACObject(ACClassTaskQueue.TaskQueue.Context, acProgram);
                        currentProgramLog.ACProgramLog1_ParentACProgramLog = parentProgramLog;
                        currentProgramLog.ACProgram = acProgram;
                        currentProgramLog.ACUrl = this.GetACUrl();
                        currentProgramLog.ACClassID = componentClassID;
                        if (acMethod != null)
                            currentProgramLog.XMLConfig = ACConvert.ObjectToXML(acMethod, true);
                        else
                            currentProgramLog.XMLConfig = "";
                        TimeInfo.ValueT.StoreToProgramLog(currentProgramLog);
                    }
                );

                SetCurrentProgramLog(currentProgramLog, false);
                ACClassTaskQueue.TaskQueue.ProgramCache.AddProgramLog(currentProgramLog);

                // Eintrag in Queue, Speicherung kann verzögert erfolgen.
                ACClassTaskQueue.TaskQueue.Add(() =>
                    {
                        acProgram.ACProgramLog_ACProgram.Add(currentProgramLog);
                        if (parentProgramLog != null)
                            parentProgramLog.ACProgramLog_ParentACProgramLog.Add(currentProgramLog);
                        OnNewProgramLogAddedToQueue(acMethod, currentProgramLog);
                    }
                );
                return CreateNewProgramLogResult.NewOneCreated;
            }

            messageText = "Can't create new Program-Log! ACProgram is null.";
            if (parentProgramLog == null)
                messageText += " acProgram is null.";
            msg = new Msg(messageText, this, eMsgLevel.Error, PWNodeProcessWorkflow.PWClassName, "CreateNewProgramLog()", 1100);
            if (IsAlarmActive(TimeInfo, msg.Message) == null)
            {
                Messages.LogMessageMsg(msg);
                OnNewAlarmOccurred(TimeInfo, msg, true);
            }
            return CreateNewProgramLogResult.ErrorNoProgramFound;
        }

        protected virtual void OnNewProgramLogAddedToQueue(ACMethod acMethod, ACProgramLog currentProgramLog)
        {
        }

        /// <summary>
        /// Finishes the program log.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <param name="refACClassID">The reference ac class identifier.</param>
        /// <returns></returns>
        protected bool FinishProgramLog(ACMethod acMethod, Guid? refACClassID = null)
        {
            ACProgramLog currentProgramLog = CurrentProgramLog; // Kopie in Stackvariable, weil durch UnloadWorkflow und verzögerter Speicherung, CurrentProgramLog null wird
            if (currentProgramLog != null)
            {
                PATimeInfo timeInfo = TimeInfo.ValueT.Clone();
                ACClassTaskQueue.TaskQueue.Add(() =>
                //ACClassTaskQueue.TaskQueue.ProcessAction(() => 
                    {
                        if (refACClassID.HasValue)
                            currentProgramLog.RefACClassID = refACClassID;
                        if (acMethod != null)
                            currentProgramLog.XMLConfig = ACConvert.ObjectToXML(acMethod, true);
                        timeInfo.StoreToProgramLog(currentProgramLog);
                        currentProgramLog.UpdateDate = DateTime.Now;
                        //ACClassTaskQueue.TaskQueue.Context.ACSaveChanges();
                    }
                );
                return true;
            }
            else
            {
                if (CurrentACState > ACStateEnum.SMStarting && LastACState > ACStateEnum.SMStarting)
                    Messages.LogError(this.GetACUrl(), "FinishProgramLog(0)", "CurrentProgramLog is null because: " + DumpReasonCurrentPLIsNull());
            }
            return false;
        }

        /// <summary>
        /// Informs subclasses, that a new program log is created.
        /// </summary>
        /// <param name="newLog"></param>
        protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        {
            ACProgramLog currentProgramLog = CurrentProgramLog;
            if (currentProgramLog != null)
            {
                newLog.ACProgramLogID = currentProgramLog.ACProgramLogID;
            }
        }

#endregion

#endregion
        
    }
}
