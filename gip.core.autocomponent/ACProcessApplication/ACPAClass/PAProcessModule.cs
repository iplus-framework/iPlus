using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// According to the composition principle, modules can be recursively combined to form larger assemblies. 
    /// Larger assemblies such as a machine are not just physical objects, they have a purpose. 
    /// They provide functions so that production processes or processes in general can be carried out. 
    /// These are therefore called "process modules " and the base class is PAProcessModule. 
    /// Process modules consist of a collection of process functions ( PAProcessFunction ) 
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PAClassPhysicalBase" />
    /// <seealso cref="gip.core.autocomponent.IACComponentTaskExec" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAProcessModule'}de{'PAProcessModule'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, PWGroup.PWClassName, true)]
    public abstract class PAProcessModule : PAClassPhysicalBase, IACComponentTaskExec, IACAttachedAlarmHandler, IRouteItemIDProvider
    {
        #region c'tors

        public const string SelRuleID_ProcessModule = "PAProcessModule";
        public const string SelRuleID_ProcessModule_Deselector = "PAProcessModule.Deselector";

        public const string ClassName = "PAProcessModule";

        static PAProcessModule()
        {
            RegisterExecuteHandler(typeof(PAProcessModule), HandleExecuteACMethod_PAProcessModule);
            ACRoutingService.RegisterSelectionQuery(SelRuleID_ProcessModule, (c, p) => c.Component.ValueT is PAProcessModule, null);
            ACRoutingService.RegisterSelectionQuery(SelRuleID_ProcessModule_Deselector, null, (c, p) => c.Component.ValueT is PAProcessModule);
        }

        public PAProcessModule(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _RouteItemID = new ACPropertyConfigValue<string>(this, "RouteItemID", "0");
            _AllocationExternal = new ACPropertyConfigValue<bool>(this, "AllocationExternal", false);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _TaskInvocationPoint = new ACPointTask(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint.SetMethod = OnSetTaskInvocationPoint; 
            _Semaphore = new ACPointServiceACObject(this, "Semaphore", OnGetSemaphoreCapacity());

            if (!base.ACInit(startChildMode))
                return false;

            if (IsDisplayingOrderInfo)
                _OrderInfoManager = PAShowDlgManagerBase.ACRefToServiceInstance(this);

            AttachedAlarms = new SafeList<Msg>();

            _ = AllocationExternal;
            _ = RouteItemID;
            return true;
        }

        public virtual uint OnGetSemaphoreCapacity()
        {
            return 1;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            RefreshOrderInfo();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_OrderInfoManager != null)
                PAShowDlgManagerBase.DetachACRefFromServiceInstance(this, _OrderInfoManager);
            _OrderInfoManager = null;

            AttachedAlarms = null;

            bool init = base.ACDeInit(deleteACClassTask);
            return init;
        }
        //public override Global.ACStorableTypes ACStorableType
        //{
        //    get
        //    {
        //        return Global.ACStorableTypes.Required;
        //    }
        //}
        #endregion

        #region Points

        protected ACPointServiceACObject _Semaphore;
        [ACPropertyPoint(true,1)]
        public ACPointServiceACObject Semaphore
        {
            get
            {
                return _Semaphore;
            }
        }

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
            //RefreshOrderInfo();
            return true;
        }
        #endregion

        #region Properties

        [ACPropertyBindingSource(300, "Configuration", "en{'Max. weight capacity [kg]'}de{'Max. Gewicht [kg]'}", "", true, true)]
        public IACContainerTNet<Double> MaxWeightCapacity { get; set; }

        [ACPropertyBindingSource(301, "Configuration", "en{'Max. volume [dm³]'}de{'Max. Volumen [dm³]'}", "", true, true)]
        public IACContainerTNet<Double> MaxVolumeCapacity { get; set; }

        [ACPropertyBindingSource(302, "Info", "en{'Order-Info'}de{'Auftragsinformation'}", "", false, false)]
        public IACContainerTNet<String> OrderInfo { get; set; }

        [ACPropertyBindingSource(303, "Info", "en{'Reservation-Info'}de{'Reservierungsinformation'}", "", false, false)]
        public IACContainerTNet<String> ReservationInfo { get; set; }

        [ACPropertyBindingSource(304, "Info", "en{'Active WF-Nodes'}de{'Active Workflowknoten'}", "", false, false)]
        public IACContainerTNet<List<ACChildInstanceInfo>> WFNodes { get; set; }

        [ACPropertyBindingSource(305, "Info", "en{'Last occupation date'}de{'Letzte Belegung am'}", "", false, true)]
        public IACContainerTNet<DateTime> LastOccupation { get; set; }

        [ACPropertyInfo(350, "Configuration", "en{''Max. repeats of interdis.'}de{'Max. Zwischenentleerungswiederholungen'}", "", true)]
        public short MaxCapacityRepeat
        {
            get;
            set;
        }

        [ACPropertyBindingTarget(403, "Read from PLC", "en{'Allocated'}de{'Belegt'}", "", false, false, RemotePropID = 16)]
        public IACContainerTNet<Boolean> Allocated { get; set; }

        protected ACPropertyConfigValue<string> _RouteItemID;
        [ACPropertyConfig("en{'ID/Number in PLC'}de{'ID/Nummer in SPS'}")]
        public virtual string RouteItemID
        {
            get
            {
                return _RouteItemID.ValueT;
            }
            //set
            //{
            //    this["RouteItemID"] = value;
            //}
        }

        protected Nullable<int> _RouteItemIDAsNum;
        public virtual int RouteItemIDAsNum
        {
            get
            {
                if (_RouteItemIDAsNum.HasValue)
                    return _RouteItemIDAsNum.Value;
                _RouteItemIDAsNum = -1;
                if (!String.IsNullOrEmpty(RouteItemID))
                {
                    try { _RouteItemIDAsNum = System.Convert.ToInt32(RouteItemID); }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("PAProcessModule", "RouteItemIDAsNum", msg);
                    }
                }
                return _RouteItemIDAsNum.Value;
            }
        }

        private ACPropertyConfigValue<bool> _AllocationExternal;
        [ACPropertyConfig("en{'Allocation from external System'}de{'Belegung von externem System'}")]
        public bool AllocationExternal
        {
            get
            {
                return _AllocationExternal.ValueT;
            }
            set
            {
                _AllocationExternal.ValueT = value;
            }
        }

        protected ACRef<PAShowDlgManagerBase> _OrderInfoManager = null;
        public PAShowDlgManagerBase OrderInfoManager
        {
            get
            {
                if (_OrderInfoManager == null)
                    return null;
                return _OrderInfoManager.ValueT;
            }
        }

        protected virtual bool IsDisplayingOrderInfo
        {
            get
            {
                return true;
            }
        }

        Guid[] _CacheModuleDestinations = null;
        public Guid[] ModuleDestinations
        {
            get
            {
                if (_CacheModuleDestinations != null)
                    return _CacheModuleDestinations;

                using (Database db = new datamodel.Database())
                {
                    ACRoutingParameters routingParameters = new ACRoutingParameters()
                    {
                        RoutingService = this.RoutingService,
                        Database = db,
                        AttachRouteItemsToContext = RoutingService != null && RoutingService.IsProxy,
                        SelectionRuleID = PAProcessModule.SelRuleID_ProcessModule,
                        Direction = RouteDirections.Forwards,
                        DBSelector = (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                        DBDeSelector = (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                        MaxRouteAlternativesInLoop = 1,
                        IncludeReserved = true,
                        IncludeAllocated = true,
                    };

                    RoutingResult rResult = ACRoutingService.FindSuccessors(this.GetACUrl(), routingParameters);
                    if (rResult.Routes != null && rResult.Routes.Any())
                        _CacheModuleDestinations = rResult.Routes.Select(c => c.LastOrDefault().Target.ACClassID).ToArray();
                    else
                        _CacheModuleDestinations = new Guid[] { };
                }
                return _CacheModuleDestinations;
            }
        }

        [ACPropertyBindingTarget]
        public IACContainerTNet<bool> HasAttachedAlarm
        {
            get;
            set;
        }

        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.TMReset:
                    Reset();
                    return true;
                case "SemaphoreAccessedFrom":
                    result = SemaphoreAccessedFrom();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMReset:
                    result = IsEnabledReset();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PAProcessModule(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "WorkflowDialogOn":
                    WorkflowDialogOn(acComponent);
                    return true;
                case Const.IsEnabledPrefix + "WorkflowDialogOn":
                    result = IsEnabledWorkflowDialogOn(acComponent);
                    return true;
            }
            return HandleExecuteACMethod_PAClassPhysicalBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        public virtual bool IsEnabledTransition(string transitionMethod, PAProcessFunction childFunction)
        {
            return true;
        }

        /// <summary>
        /// Stellt die Verknüpfung zum ACProgramLog her für Alarme die geloggt werden solllen
        /// </summary>
        /// <param name="newLog"></param>
        protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        {
            RelateAlarmLogWithProgramLog(newLog);
        }

        public void RelateAlarmLogWithProgramLog(MsgAlarmLog newLog)
        {
            if (CurrentProgramLog != null)
                newLog.ACProgramLogID = CurrentProgramLog.ACProgramLogID;
        }

        public ACProgramLog CurrentProgramLog
        {
            get
            {
                if (Semaphore.ConnectionListCount <= 0)
                    return null;
                var wrapPWGroup = Semaphore.ConnectionList.FirstOrDefault();
                if (wrapPWGroup == null)
                    return null;
                PWGroup pwGroup = wrapPWGroup.ValueT as PWGroup;
                if (pwGroup != null)
                    return pwGroup.CurrentProgramLog;
                return null;
            }
        }

        public SafeList<Msg> AttachedAlarms 
        {
            get;
            set; 
        }

        public TResult GetExecutingFunction<TResult>(Guid acMethodRequestID) where TResult : PAProcessFunction
        {
            return FindChildComponents<TResult>(c => c is PAProcessFunction
                && (c as PAProcessFunction).CurrentACMethod.ValueT != null 
                && (c as PAProcessFunction).CurrentACMethod.ValueT.ACRequestID == acMethodRequestID, 
                null, 1)
                .FirstOrDefault();
        }

        public ACStateEnum? GetACStateOfFunction(string acIdentifierOfVirtMethod, out PAProcessFunction childProcessFunction)
        {
            string acMethodName1;
            int pos = acIdentifierOfVirtMethod.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acIdentifierOfVirtMethod.Substring(1);
            else
                acMethodName1 = acIdentifierOfVirtMethod;

            childProcessFunction = null;
            if (ComponentClass == null)
                return null;

            ACClassMethod acClassMethod = ACClassMethods.FirstOrDefault(c => c.ACIdentifier == acMethodName1);
            if (acClassMethod == null)
                acClassMethod = ComponentClass.GetMethod(acMethodName1, true);
            if (acClassMethod == null || acClassMethod.ACKind != Global.ACKinds.MSMethodFunction)
                return null;
            if (acClassMethod.ACClassMethod1_ParentACClassMethod != null)
            {
                IEnumerable<PAProcessFunction> childFunctions = FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction, null, 1)
                                                            .Where(c => c.ComponentClass.MethodsCached.Contains(acClassMethod.BasedOnACClassMethod));
                if (childFunctions.Count() == 1)
                    childProcessFunction = childFunctions.FirstOrDefault();
                else if (acClassMethod.AttachedFromACClassID.HasValue)
                {
                    childProcessFunction = childFunctions.Where(c => c.ComponentClass.ACClassID == acClassMethod.AttachedFromACClassID.Value
                                                                    || c.ComponentClass.IsDerivedClassFrom(acClassMethod.AttachedFromACClass)).FirstOrDefault();
                }
                //childProcessFunction = FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction
                //    && (c as PAProcessFunction).ComponentClass.MethodsCached.Contains(acClassMethod.ACClassMethod1_ParentACClassMethod),
                //    null, 1)
                //    .FirstOrDefault();
            }
            if (childProcessFunction == null)
                return ACStateEnum.SMIdle;
            return childProcessFunction.CurrentACState;
        }

        [ACMethodInfo("Function", "en{'SemaphoreAccessedFrom'}de{'SemaphoreAccessedFrom'}", 9999)]
        public virtual string[] SemaphoreAccessedFrom()
        {
            if (Semaphore == null || Semaphore.ConnectionListCount <= 0)
                return null;
            return Semaphore.ConnectionList.Select(c => c.ACUrl).ToArray();
        }

        public virtual void RefreshOrderInfo()
        {
            if (OrderInfoManager != null)
                OrderInfoManager.BuildAndSetOrderInfo(this);
            else
            {
                string[] accessArr = SemaphoreAccessedFrom();
                this.OrderInfo.ValueT = accessArr != null && accessArr.Any() ? String.Join(System.Environment.NewLine, accessArr) : "";
            }
            IACPropertyNetTarget allocTarget = Allocated as IACPropertyNetTarget;
            if (!(allocTarget != null && allocTarget.Source != null && AllocationExternal))
                Allocated.ValueT = !String.IsNullOrEmpty(this.OrderInfo.ValueT);
            RefreshPWNodeInfo();
            FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction, null, 1).ForEach(c => c.OnOrderInfoRefreshed());
        }

        public virtual void RefreshPWNodeInfo()
        {
            if (String.IsNullOrEmpty(this.OrderInfo.ValueT) || !Semaphore.ConnectionList.Any())
                WFNodes.ValueT = null;
            else
            {
                List<ACChildInstanceInfo> childInstanceInfos = new List<ACChildInstanceInfo>();
                foreach (var entry in Semaphore.ConnectionList)
                {
                    PWBase pwNode = entry.ValueT as PWBase;
                    if (pwNode != null)
                    {
                        childInstanceInfos.AddRange(pwNode.FindChildComponents<PWBaseExecutable>(c => (c is PWBaseNodeProcess || c is PWNodeDecisionFunc)
                                                            && !(c is PWNodeProcessMethod)
                                                            && !(c is PWNodeProcessWorkflow)
                                                            && (c as PWBaseExecutable).CurrentACState != ACStateEnum.SMIdle
                                                            && (c as PWBaseExecutable).CurrentACState != ACStateEnum.SMBreakPoint
                                                            && (c as PWBaseExecutable).CurrentACState != ACStateEnum.SMBreakPointStart,
                                                 null, 1)
                                                .Select(c => new ACChildInstanceInfo(c)));
                    }
                }
                WFNodes.ValueT = childInstanceInfos;
            }
        }

        public virtual void RunStateValidation()
        {
        }

        public override void RefreshHasAlarms(bool? childHasAlarms = null, bool resetIfNoUnackAlarms = false)
        {
            base.RefreshHasAlarms(childHasAlarms, resetIfNoUnackAlarms);

            if (!String.IsNullOrEmpty(this.OrderInfo.ValueT) && Semaphore.ConnectionList.Any())
            {
                PWGroup pwGroup = Semaphore.ConnectionList.FirstOrDefault().ValueT as PWGroup;
                if (pwGroup != null && pwGroup.RootPW != null)
                {
                    pwGroup.RootPW.OnHasAlarmChangedInPhysicalModel(this);
                }
            }
        }

        public virtual void OnProcessModuleOccupied(PWGroup pwGroup)
        {
        }

        public virtual void OnProcessModuleReleased(PWGroup pwGroup)
        {
        }

        #region IACComponentTaskExec
        public virtual bool ActivateTask(ACMethod acMethod, bool executeMethod, IACComponent executingInstance)
        {
            return ACPointAsyncRMIHelper.ActivateTask(this, acMethod, executeMethod, executingInstance);
        }

        public virtual bool CallbackTask(ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state)
        {
            return ACPointAsyncRMIHelper.CallbackTask(this, acMethod, result, state);
        }

        public virtual bool CallbackTask(IACTask task, ACMethodEventArgs result, PointProcessingState state)
        {
            return ACPointAsyncRMIHelper.CallbackTask(this, task, result, state);
        }

        public IACTask GetTaskOfACMethod(ACMethod acMethod)
        {
            return ACPointAsyncRMIHelper.GetTaskOfACMethod(this, acMethod);
        }

        public virtual bool CallbackCurrentTask(ACMethodEventArgs result)
        {
            return ACPointAsyncRMIHelper.CallbackCurrentTask(this, result);
        }
        #endregion

        #region Client-Methods

        #region Operating-Mode
        public override void SwitchToAutomatic()
        {
            if (!IsEnabledSwitchToAutomatic())
                return;
            OperatingMode.ValueT = Global.OperatingMode.Automatic;
        }

        public override void SwitchToManual()
        {
            if (!IsEnabledSwitchToManual())
                return;
            OperatingMode.ValueT = Global.OperatingMode.Manual;
        }

        public override void SwitchToMaintenance()
        {
            if (!IsEnabledSwitchToMaintenance())
                return;
            OperatingMode.ValueT = Global.OperatingMode.Maintenance;
        }
        #endregion


        #region Reset
        [ACMethodInteraction("", "en{'Reset'}de{'Reset'}", 302, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Reset()
        {
            //Semaphore

            using (ACMonitor.Lock(Semaphore.LockLocalStorage_20033))
            {
                if (Semaphore.LocalStorage.Any())
                {
                    Semaphore.LocalStorage.Clear();
                    Semaphore.Persist(false);
                }
            }

            var invocations = this.TaskInvocationPoint.ConnectionList.ToList();
            foreach (var invocation in invocations)
            {
                if (invocation.ACMethod != null)
                    this.TaskInvocationPoint.InvokeCallbackDelegate(new ACMethodEventArgs(invocation.ACMethod, Global.ACMethodResultState.Failed));
            }
        }

        public virtual bool IsEnabledReset()
        {
            return true;
            //if (Root.CurrentInvokingUser != null && Root.CurrentInvokingUser.IsSuperuser)
            //    return true;
            //return true;
        }

        public static bool AskUserReset(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            return acComponent.Messages.Question(acComponent, "Question50037", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }
#endregion


#region Workflow and Alarms
        [ACMethodInteractionClient("", "en{'Workflowdialog'}de{'Workflow-dialog'}", (short)MISort.ComponentExplorer-1, false)]
        public static void WorkflowDialogOn(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            if (_this.CurrentInvokingACCommand == null)
                return;

            IACBSO bso = _this as IACBSO;
            if (bso == null && _this.CurrentInvokingACCommand.BSO != null)
                bso = _this.CurrentInvokingACCommand.BSO;
            if (bso == null)
                return;
            VBBSOSelectionDependentDialog dialog = null;
            dialog = (bso as ACComponent).GetChildComponent("VBBSOWorkflowDialog(CurrentDesign)") as VBBSOSelectionDependentDialog;
            if (dialog == null)
                dialog = bso.StartComponent("VBBSOWorkflowDialog(CurrentDesign)", null, null) as VBBSOSelectionDependentDialog;
            if (dialog != null)
                dialog.ShowDialogForComponent(_this);
        }

        public static bool IsEnabledWorkflowDialogOn(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            if (acComponent.ComponentClass.GetDesign(_this, Global.ACUsages.DULayout, Global.ACKinds.DSDesignLayout, "WorkflowDialog") != null)
                return true;
            return false;
        }

        public override MsgList GetAlarms(bool thisAlarms, bool subAlarms, bool onlyUnackAlarms)
        {
            var msgList = base.GetAlarms(thisAlarms, subAlarms, onlyUnackAlarms);
            if (!String.IsNullOrEmpty(this.OrderInfo.ValueT) && Semaphore.ConnectionList.Any())
            {
                PWBase pwNode = Semaphore.ConnectionList.FirstOrDefault().ValueT as PWBase;
                if (pwNode != null)
                {
                    var msgListWF = pwNode.GetAlarms(thisAlarms,subAlarms,onlyUnackAlarms);
                    msgList.AddRange(msgListWF);
                }
            }
            return msgList;
        }

        public void AddAttachedAlarm(Msg msg)
        {
            if (AttachedAlarms == null)
                return;

            AttachedAlarms.Add(msg);
            HasAttachedAlarm.ValueT = true;
        }

        [ACMethodInfo("", "", 9999)]
        public void AckAttachedAlarm(Guid msgID)
        {
            if (AttachedAlarms == null)
                return;

            Msg alarmMsg = AttachedAlarms.FirstOrDefault(c => c.MsgId == msgID);
            if (alarmMsg != null)
                AttachedAlarms.Remove(alarmMsg);

            if (!AttachedAlarms.Any())
                HasAttachedAlarm.ValueT = false;
        }

        [ACMethodInfo("", "", 9999)]
        public void AckAllAttachedAlarms()
        {
            if (AttachedAlarms == null)
                return;

            AttachedAlarms.Clear();
            HasAttachedAlarm.ValueT = false;
        }

        [ACMethodInfo("","",9999)]
        public MsgList GetAttachedAlarms()
        {
            MsgList result = new MsgList();
            if (AttachedAlarms != null)
                result.AddRange(AttachedAlarms);
            return result;
        }

        #endregion

        #endregion

        #endregion


        // Methods, Range: 300

    }
}
