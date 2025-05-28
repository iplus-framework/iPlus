using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWBaseExecutable extends PWBaseInOut so that a workflow node can be configured. 
    /// An ACMethod instance is generated and the parameters are put together by the overridable configuration stores. 
    /// The final configuration is provided in the CurrentACMethod and ExecutingACMethod properties.
    /// PWBaseExcecutable is also the base class for all workflow nodes that remain in the ACState states for a certain time until their task is completed. 
    /// Before a workflow node changes to the "Starting" state, it can be stopped by a breakpoint. 
    /// This class provides some methods for this. It also reads the configuration memory when it starts and sets the breakpoint by itself.
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBaseInOut" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWBaseExecutable'}de{'PWBaseExecutable'}", Global.ACKinds.TPWNode, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, false)]
    public abstract class PWBaseExecutable : PWBaseInOut, IACMyConfigCache
    {
        #region c´tors

        static PWBaseExecutable()
        {
            RegisterExecuteHandler(typeof(PWBaseExecutable), HandleExecuteACMethod_PWBaseExecutable);
        }

        public PWBaseExecutable(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACPostInit()
        {
            bool postInit = base.ACPostInit();
            UpdateCurrentACMethod();
            return postInit;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (deleteACClassTask)
            {
                IterationCount.ValueT = 0;
            }
            _ExecutingACMethod = null;
            _ACMethodSignature = null;
            ClearMyConfiguration();
            return base.ACDeInit(deleteACClassTask);
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            ClearMyConfiguration();
            base.Recycle(content, parentACObject, parameter, acIdentifier);
        }

        public virtual void OnRootPWStarted()
        {
            if (CurrentACState == ACStateEnum.SMIdle)
            {
                ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);
                if (serviceInstance != null)
                {
                    var ruleValueList = serviceInstance.GetRuleValueList(MandatoryConfigStores, PreValueACUrl, ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Breakpoint.ToString());
                    if (ruleValueList != null)
                    {
                        if (ruleValueList.IsBreakPointSet())
                            CurrentACState = ACStateEnum.SMBreakPoint;
                    }
                }
            }
        }
        #endregion

        #region Properties, Range: 500
        [ACPropertyBindingSource(500, "", "en{'Iteration count'}de{'Durchlauf Anzahl'}", "", false, true)]
        public IACContainerTNet<Int32> IterationCount { get; set; }


        protected ACMethod _ExecutingACMethod = null;
        /// <summary>
        /// Gets the active ACMethod, that is stored in CurrentProgramLog.XMLConfig
        /// </summary>
        /// <value>
        /// The executing ac method.
        /// </value>
        public virtual ACMethod ExecutingACMethod
        {
            get
            {
                if (_ExecutingACMethod != null)
                    return _ExecutingACMethod;
                var currentProgramLog = CurrentProgramLog;
                if (currentProgramLog == null)
                    return null;
                if (String.IsNullOrEmpty(currentProgramLog.XMLConfig))
                    return null;
                _ExecutingACMethod = ACConvert.XMLToObject<ACMethod>(currentProgramLog.XMLConfig, true, null);
                return _ExecutingACMethod;
            }
        }

        /// <summary>
        /// Returns ExecutingACMethod-Property in a network-property
        /// </summary>
        /// <value>
        /// The current ac method.
        /// </value>
        [ACPropertyBindingSource(999, "ACConfig", "en{'Current Method'}de{'Aktuelle Methode'}", "", false, false)]
        public IACContainerTNet<ACMethod> CurrentACMethod { get; set; }


        /// <summary>
        /// If this workflow-node is excuted a second time, than this property returns the previous ACMethod
        /// </summary>
        /// <value>
        /// The previous ac method.
        /// </value>
        public virtual ACMethod PreviousACMethod
        {
            get
            {
                var currentACMethod = ExecutingACMethod;
                if (currentACMethod != null)
                    return currentACMethod;
                var lastLog = PreviousProgramLogs.LastOrDefault();
                if (lastLog == null)
                    return null;
                if (String.IsNullOrEmpty(lastLog.XMLConfig))
                    return null;
                return ACConvert.XMLToObject<ACMethod>(lastLog.XMLConfig, true, null);
            }
        }

        private ACMethod _ACMethodSignature = null;
        /// <summary>
        /// Creates a new and empty ACMethod-instance. 
        /// Use NewACMethodWithConfiguration() to get a ACMethod-Instance with parameters that are read from the Config-Stores.
        /// </summary>
        /// <returns></returns>
        public virtual ACMethod NewACMethod()
        {
            if (_ACMethodSignature != null)
                return _ACMethodSignature.Clone() as ACMethod;
            ACClass pwACClass = PWACClassOfContentWF;
            if (pwACClass == null)
                return null;
            var acClassMethod = pwACClass.MethodsCached.Where(c => c.ACIdentifier == ACStateConst.SMStarting && c.ACGroup == Const.ACState).FirstOrDefault();
            if (acClassMethod == null)
                return null;
            _ACMethodSignature = acClassMethod.ACMethod;
            if (_ACMethodSignature == null)
                return null;
            return _ACMethodSignature.Clone() as ACMethod;
        }

        /// <summary>
        /// Returns a new ACMethod-instance and fills all parameters from Config-Store-Hierarchy by calling PWBase.GetConfigForACMethod()
        /// </summary>
        /// <returns></returns>
        public virtual ACMethod NewACMethodWithConfiguration()
        {
            if (InitState < ACInitState.Reloading
                || InitState >= ACInitState.Destructing)
            {
                Messages.LogError(this.GetACUrl(), "NewACMethodWithConfiguration(10)", "Access to early: InitState is not Initialized");
                Messages.LogError(this.GetACUrl(), "NewACMethodWithConfiguration(11)", System.Environment.StackTrace);
                return null;
            }
            ACMethod acMethod = NewACMethod();
            if (acMethod != null)
            {
                ExecuteMethod(nameof(GetConfigForACMethod), acMethod, false);
            }
            return acMethod;
        }

        protected void UpdateCurrentACMethod()
        {
            this.CurrentACMethod.ValueT = ExecutingACMethod;
        }

        [ACMethodCommand("Process", "en{'Save changed Parameters'}de{'Speichere geänderte Paramter'}", (short)MISort.Save, true)]
        public Msg SaveChangedACMethod()
        {
            ACMethod thisACMethod = this.CurrentACMethod.ValueT;
            Msg msg = OnValidateChangedACMethod(this.CurrentACMethod.ValueT);
            if (msg != null)
                return msg;
            if (   _ExecutingACMethod == null 
                || _ExecutingACMethod.ACIdentifier == thisACMethod.ACIdentifier)
            {
                _ExecutingACMethod = thisACMethod;
                FinishProgramLog(thisACMethod);
            }
            return null;
        }

        public virtual bool IsEnabledSaveChangedACMethod()
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

        public static bool AskUserSaveChangedACMethod(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            IACPropertyNetBase netProperty = acComponent.GetMember("CurrentACMethod") as IACPropertyNetBase;
            if (netProperty == null)
                return false;
            netProperty.OnMemberChanged();
            return acComponent.Messages.Question(acComponent, "Question50081", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }

        protected virtual Msg OnValidateChangedACMethod(ACMethod acMethod)
        {
            return null;
        }

        public bool IsConfigurationLoaded
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _MyConfiguration != null;
                }
            }
        }

        protected ACMethod _MyConfiguration;
        [ACPropertyInfo(999)]
        public ACMethod MyConfiguration
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_MyConfiguration != null)
                        return _MyConfiguration;
                }
                var myNewConfig = NewACMethodWithConfiguration();
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_MyConfiguration == null)
                        _MyConfiguration = myNewConfig;
                }
                return myNewConfig;
            }
        }

        public virtual void ClearMyConfiguration()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _MyConfiguration = null;
            }
            this.HasRules.ValueT = 0;
        }

        public bool IsMyConfigurationLoaded
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _MyConfiguration != null;
                }
            }
        }

        public virtual bool IsSkippable
        {
            get
            {
                return true;
            }
        }

        public abstract bool MustBeInsidePWGroup { get; }

        /// <summary>
        /// Checks if Workflow or parent group is set to skipping mode.
        /// If Node is allowed to skip, then the state is automatically se to SMCompleted.
        /// </summary>
        /// <returns>Returns true if parent PWGroup exists and node can continue with it's logic.</returns>
        public virtual bool CheckParentGroupAndHandleSkipMode(bool autoSetCompleted = true)
        {
            var pwGroup = ParentPWGroup;
            if (pwGroup == null) // Is null when Service-Application is shutting down
            {
                if (MustBeInsidePWGroup)
                {
                    if (this.InitState == ACInitState.Initialized)
                        Messages.LogError(this.GetACUrl(), "SMStarting()", "ParentPWGroup is null");
                    return false;
                }
            }
            if (!IsSkippable)
                return true;
            if (   (pwGroup != null && pwGroup.IsInSkippingMode)
                || (RootPW != null && RootPW.IsInSkippingMode))
            {
                UnSubscribeToProjectWorkCycle();
                if (autoSetCompleted)
                {
                    // Falls durch tiefere Callstacks der Status schon weitergeschaltet worden ist, dann schalte Status nicht weiter
                    if (CurrentACState == ACStateEnum.SMStarting)
                        CurrentACState = ACStateEnum.SMCompleted;
                }
                return false;
            }
            return true;
        }

        #endregion

        #region Methods

        #region ACState

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Start):
                    Start();
                    return true;
                case nameof(SMStarting):
                    SMStarting();
                    return true;
                case nameof(SMBreakPoint):
                    SMBreakPoint();
                    return true;
                case nameof(SMBreakPointStart):
                    SMBreakPointStart();
                    return true;
                case nameof(SetBreakPoint):
                    SetBreakPoint();
                    return true;
                case nameof(RemoveBreakPoint):
                    RemoveBreakPoint();
                    return true;
                case nameof(ResetAndComplete):
                    ResetAndComplete();
                    return true;
                case nameof(SaveChangedACMethod):
                    result = SaveChangedACMethod();
                    return true;
                case Const.IsEnabledPrefix + nameof(Start):
                    result = IsEnabledStart();
                    return true;
                case Const.IsEnabledPrefix + nameof(SetBreakPoint):
                    result = IsEnabledSetBreakPoint();
                    return true;
                case Const.IsEnabledPrefix + nameof(RemoveBreakPoint):
                    result = IsEnabledRemoveBreakPoint();
                    return true;
                case Const.IsEnabledPrefix + nameof(ResetAndComplete):
                    result = IsEnabledResetAndComplete();
                    return true;
                case Const.IsEnabledPrefix + nameof(SaveChangedACMethod):
                    result = IsEnabledSaveChangedACMethod();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWBaseExecutable(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(ShowOrderDialog):
                    ShowOrderDialog(acComponent);
                    return true;
                case Const.IsEnabledPrefix + nameof(ShowOrderDialog):
                    result = IsEnabledShowOrderDialog(acComponent);
                    return true;
                case nameof(AskUserStart):
                    result = AskUserStart(acComponent);
                    return true;
                case Const.AskUserPrefix + nameof(SaveChangedACMethod):
                    result = AskUserSaveChangedACMethod(acComponent);
                    return true;
            }
            return HandleExecuteACMethod_PWBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        public override void Reset()
        {
            if (!IsEnabledReset())
                return;

            base.Reset();
            _ExecutingACMethod = null;
        }

        [ACMethodInteraction("Process", "en{'Start'}de{'Start'}", 301, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Start()
        {
            if (IsEnabledStartInternal())
                CurrentACState = ACStateEnum.SMStarting;
            else if (CurrentACState == ACStateEnum.SMBreakPoint)
                CurrentACState = ACStateEnum.SMBreakPointStart;
        }

        protected virtual bool IsEnabledStartInternal()
        {
            return CurrentACState == ACStateEnum.SMIdle;
        }


        public virtual bool IsEnabledStart()
        {
            if (Root.CurrentInvokingUser != null && Root.CurrentInvokingUser.IsSuperuser)
                return true;
            return IsEnabledStartInternal();
        }

        public static bool AskUserStart(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50038", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }

        [ACMethodInteraction("Process", "en{'Set breakpoint'}de{'Haltepunkt setzen'}", (short)201, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void SetBreakPoint()
        {
            if (IsEnabledSetBreakPoint())
            {
                CurrentACState = ACStateEnum.SMBreakPoint;
            }
        }

        public virtual bool IsEnabledSetBreakPoint()
        {
            return IsEnabledStartInternal();
        }

        [ACMethodInteraction("Process", "en{'Remove breakpoint'}de{'Haltepunkt entfernen'}", (short)202, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void RemoveBreakPoint()
        {
            if (IsEnabledRemoveBreakPoint())
            {
                ACStateEnum lastState = CurrentACState;
                Reset();
                if (lastState == ACStateEnum.SMBreakPointStart)
                    Start();
            }
        }

        public virtual bool IsEnabledRemoveBreakPoint()
        {
            return CurrentACState == ACStateEnum.SMBreakPoint || CurrentACState == ACStateEnum.SMBreakPointStart;
        }

        [ACMethodInteraction("Process", "en{'Reset and complete'}de{'Reset und Beenden'}", (short)203, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ResetAndComplete()
        {
            if (!IsEnabledResetAndComplete())
                return;
            Reset();
            RaiseOutEvent();
        }

        public virtual bool IsEnabledResetAndComplete()
        {
            return IsEnabledReset();
        }

        public override void SMIdle()
        {
            base.SMIdle();
            UnSubscribeToProjectWorkCycle();
        }

        [ACMethodState("en{'Breakpoint set'}de{'Haltepunkt gesetzt'}", 10, true)]
        public virtual void SMBreakPoint()
        {
        }

        [ACMethodState("en{'Breakpoint set with start'}de{'Haltepunkt gesetzt mit Start'}", 10, true)]
        public virtual void SMBreakPointStart()
        {
        }

        [ACMethodState("en{'Starting'}de{'Startend'}", 20, true)]
        public virtual void SMStarting()
        {
            RecalcTimeInfo();
            if (CreateNewProgramLog(NewACMethodWithConfiguration()) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                return;

            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(Const.PWPointOut, VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = TimeInfo.ValueT;
            PWPointOut.Raise(eventArgs);

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                Reset();
            }
        }

        #endregion

        #region Callbacks
        [ACMethodInfo("Function", "en{'PWPointInCallback'}de{'PWPointInCallback'}", 9999)]
        public override void PWPointInCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Status so setzen, das Event als empfangen gekennzeichnet ist
                PWPointIn.UpdateActiveState(wrapObject);
                // Wenn alle Vorgänger ihre Events gefeuert haben, dann kann der 
                // Class aktiviert werden
                if (PWPointIn.IsActive)
                {
                    PWPointIn.ResetActiveStates();
                    Start();
                }
            }
        }

        #endregion


        #region Client-Methods
        [ACMethodInteractionClient("", "en{'View order'}de{'Auftrag anschauen'}", 450, false, "", false, Global.ContextMenuCategory.ProdPlanLog)]
        public static void ShowOrderDialog(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            if (!IsEnabledShowOrderDialog(_this))
                return;

            PAShowDlgManagerBase serviceInstance = PAShowDlgManagerBase.GetServiceInstance(acComponent.Root as ACComponent);
            if (serviceInstance == null)
                return;
            serviceInstance.ShowDialogOrder(acComponent);
        }

        public static bool IsEnabledShowOrderDialog(IACComponent acComponent)
        {
            PAShowDlgManagerBase serviceInstance = PAShowDlgManagerBase.GetServiceInstance(acComponent.Root as ACComponent) as PAShowDlgManagerBase;
            if (serviceInstance == null)
                return false;
            return serviceInstance.IsEnabledShowDialogOrder(acComponent);
        }
        #endregion

        #endregion
    }
}
