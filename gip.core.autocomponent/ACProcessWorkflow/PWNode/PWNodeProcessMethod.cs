// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeProcessMethod enables PAProcessFunction's to be started in the physical model (asynchronously). 
    /// The parameter list is transferred as a virtual method (ACMethod). 
    /// Which virtual method to be used is, in the property ContentACClassWF.RefPAACClassMethod determined that has been set by the workflow editor. 
    /// The parameters, as well as the configuration properties, are put together by the overwritable configuration stores (MandatoryConfigStores) by calling PWBase.GetConfigForACMethod() is called and an empty ACMethod (template) is passed. 
    /// This logic is implemented in the overwritten status method SMStarting(). 
    /// If the function was started successfully, the ACState changes to SMRunning.
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBaseNodeProcess" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWNodeProcessMethod'}de{'PWNodeProcessMethod'}", Global.ACKinds.TPWNodeMethod, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeProcessMethod : PWBaseNodeProcess
    {

        #region Constructors
        static PWNodeProcessMethod()
        {
            RegisterExecuteHandler(typeof(PWNodeProcessMethod), HandleExecuteACMethod_PWNodeProcessMethod);
        }

        public PWNodeProcessMethod(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Properties

        public override ACMethod ExecutingACMethod
        {
            get
            {
                if (TaskSubscriptionPoint == null)
                    return null;

                using (ACMonitor.Lock(TaskSubscriptionPoint.LockConnectionList_20040))
                {
                    if (TaskSubscriptionPoint.ConnectionList.Any())
                    {
                        ACPointAsyncRMISubscrWrap<ACComponent> methodInvokeSubscr = TaskSubscriptionPoint.ConnectionList.First();
                        return methodInvokeSubscr.ACMethodDescriptor as ACMethod;
                    }
                }
                return base.ExecutingACMethod;
            }
        }

        public TResult GetCurrentExecutingFunction<TResult>() where TResult : PAProcessFunction
        {
            ACPointAsyncRMISubscrWrap<ACComponent> taskEntry = null;

            using (ACMonitor.Lock(TaskSubscriptionPoint.LockLocalStorage_20033))
            {
                taskEntry = this.TaskSubscriptionPoint.ConnectionList.FirstOrDefault();
            }
            // Falls Dosierung zur Zeit aktiv ist, dann gibt es auch einen Eintrag in der TaskSubscriptionListe
            if (taskEntry != null && ParentPWGroup != null)
            {
                return ParentPWGroup.GetExecutingFunction<TResult>(taskEntry.RequestID);
            }
            return null;
        }

        public bool IsAnyTaskStarted
        {
            get
            {
                using (ACMonitor.Lock(TaskSubscriptionPoint.LockLocalStorage_20033))
                {
                    return this.TaskSubscriptionPoint.ConnectionList.Any();
                }
            }
        }

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return true;
            }
        }

        public virtual ACMethod NewACMethodPAFWithConfiguration()
        {
            if (InitState < ACInitState.Reloading
                || InitState >= ACInitState.Destructing)
            {
                Messages.LogError(this.GetACUrl(), "NewACMethodWithConfiguration(10)", "Access to early: InitState is not Initialized");
                Messages.LogError(this.GetACUrl(), "NewACMethodWithConfiguration(11)", System.Environment.StackTrace);
                return null;
            }

            core.datamodel.ACClassMethod refPAACClassMethod = RefACClassMethodOfContentWF;
            if (refPAACClassMethod == null)
                return null;

            ACMethod paramMethod = refPAACClassMethod.TypeACSignature();
            if (!(bool)ExecuteMethod(nameof(GetConfigForACMethod), paramMethod, true))
                return null;
            return paramMethod;
        }

        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        public static bool HandleExecuteACMethod_PWNodeProcessMethod(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Virtual
        public PAProcessFunction GetResponsibleProcessFunc()
        {
            core.datamodel.ACClassMethod refPAACClassMethod = RefACClassMethodOfContentWF;
            if (refPAACClassMethod == null)
                return null;
            ACMethod acMethod = refPAACClassMethod.TypeACSignature();
            if (acMethod == null)
                return null;
            PAProcessModule module = null;
            if (ParentPWGroup.NeedsAProcessModule && (ACOperationMode == ACOperationModes.Live || ACOperationMode == ACOperationModes.Simulation))
                module = ParentPWGroup.AccessedProcessModule;
            // Testmode
            else
                module = ParentPWGroup.ProcessModuleForTestmode;
            if (module == null)
                return null;

            return GetResponsibleProcessFunc(module, acMethod);
        }

        protected PAProcessFunction GetResponsibleProcessFunc(PAProcessModule module, ACMethod acMethod)
        {
            PAProcessFunction pAProcessFunction = null;
            if (module == null)
                return null;
            module.GetACStateOfFunction(acMethod.ACIdentifier, out pAProcessFunction);
            return pAProcessFunction;
        }

        protected virtual PAProcessFunction CanStartProcessFunc(PAProcessModule module, ACMethod acMethod, params object[] acParameter)
        {
            PAProcessFunction pAProcessFunction = null;
            if (module == null)
                return null;
            ACStateEnum? acStateOfFunction = module.GetACStateOfFunction(acMethod.ACIdentifier, out pAProcessFunction);
            if (!acStateOfFunction.HasValue || acStateOfFunction != ACStateEnum.SMIdle)
                return null;
            return pAProcessFunction;
        }

        #endregion

        #region ACState

        public override void Reset()
        {
            if (!_InCallback)
            {
                PAProcessModule module = null;
                if (ParentPWGroup != null)
                    module = ParentPWGroup.AccessedProcessModule;
                if (module != null)
                {
                    module.TaskInvocationPoint.ClearMyInvocations(this);
                }
            }

            base.Reset();
        }

        protected virtual bool RunWithoutInvokingFunction
        {
            get
            {
                return false;
            }
        }

        protected override bool CanRaiseRunningEvent
        {
            get
            {
                return (ParentPWGroup != null && ParentPWGroup.WithoutPM) || RunWithoutInvokingFunction;
            }
        }

        public override void SMStarting()
        {
            if (!CheckParentGroupAndHandleSkipMode())
                return;
            if (ParentPWGroup != null
                && this.ContentACClassWF != null)
            {
                ACClassMethod refPAACClassMethod = RefACClassMethodOfContentWF;
                if (refPAACClassMethod != null)
                {
                    ACMethod paramMethod = null;
                    PAProcessModule module = null;
                    if (   ParentPWGroup.NeedsAProcessModule 
                        && (ACOperationMode == ACOperationModes.Live || ACOperationMode == ACOperationModes.Simulation))
                        module = ParentPWGroup.AccessedProcessModule;
                    if (ParentPWGroup.WithoutPM || RunWithoutInvokingFunction)
                    {
                        paramMethod = MyConfiguration;
                        RecalcTimeInfo();
                        if (CreateNewProgramLog(paramMethod) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                            return;
                        _ExecutingACMethod = paramMethod;
                        UpdateCurrentACMethod();
                        base.SMStarting();
                        return;
                    }
                    // Testmode
                    else if (module == null)
                        module = ParentPWGroup.ProcessModuleForTestmode;

                    if (module == null)
                    {
                        // TODO: Meldung: Programmfehler, darf nicht vorkommen
                        return;
                    }
                    paramMethod = refPAACClassMethod.TypeACSignature();
                    if (!(bool)ExecuteMethod(nameof(GetConfigForACMethod), paramMethod, true))
                    {
                        // TODO: Meldung
                        return;
                    }

                    RecalcTimeInfo();
                    if (CreateNewProgramLog(paramMethod) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                        return;
                    _ExecutingACMethod = paramMethod;

                    module.TaskInvocationPoint.ClearMyInvocations(this);
                    _CurrentMethodEventArgs = null;
                    IACPointEntry task = module.TaskInvocationPoint.AddTask(paramMethod, this);
                    if (!IsTaskStarted(task))
                    {
                        ACMethodEventArgs eM = _CurrentMethodEventArgs;
                        if (eM == null || eM.ResultState != Global.ACMethodResultState.FailedAndRepeat)
                        {
                            string msg = "TaskInvocationPoint.AddTask failed";
                            if (IsAlarmActive(ProcessAlarm, msg) == null)
                                Messages.LogError(this.GetACUrl(), "SMStarting()", msg);
                            OnNewAlarmOccurred(ProcessAlarm, msg);
                        }
                        SubscribeToProjectWorkCycle();
                        return;
                    }
                    else
                    {
                        UnSubscribeToProjectWorkCycle();
                    }
                    UpdateCurrentACMethod();
                }
                else
                {
                    if (ParentPWGroup.WithoutPM || RunWithoutInvokingFunction)
                    {
                        base.SMStarting();
                        return;
                    }
                }
            }

            // Falls module.AddTask synchron ausgeführt wurde, dann ist der Status schon weiter
            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod)
            {
                if (CanRaiseRunningEvent)
                    RaiseRunningEvent();
                CurrentACState = ACStateEnum.SMRunning;
            }
        }

        public override void SMRunning()
        {
            // Muss überschrieben werden, damit Basis-Klasse den Status nicht sofort auf SMCompleted setzt!
        }

        public virtual bool ValidateRouteForFuncParam(Route route)
        {
            if (route == null)
                return false;
            if (ApplicationManager.RouteFindModeE == ApplicationManager.RouteFindModeEnum.None || ApplicationManager.RoutingServiceInstance == null)
                return true;

            (bool reserved, bool allocated) = route.GetReservedAndAllocated(ApplicationManager.RoutingServiceInstance);
            if ((allocated || reserved)
                && (ApplicationManager.RouteFindModeE == ApplicationManager.RouteFindModeEnum.AllocatedBlock
                    || ApplicationManager.RouteFindModeE == ApplicationManager.RouteFindModeEnum.AllocatedWarning))
            {
                //Warning50070: There are elements in the route that are either occupied or reserved!
                Msg msg = new Msg(this, eMsgLevel.Warning, nameof(PWNodeProcessMethod), "ValidateRouteForFuncParam(1)", 1000, "Warning50070");
                if (IsAlarmActive(ProcessAlarm, msg.Message) == null)
                    Messages.LogWarning(this.GetACUrl(), "ValidateRouteForFuncParam(2)", msg.InnerMessage);
                OnNewAlarmOccurred(ProcessAlarm, msg, true);
            }
            return !((allocated && ApplicationManager.RouteFindModeE == ApplicationManager.RouteFindModeEnum.AllocatedBlock)
                    || (allocated && ApplicationManager.RouteFindModeE == ApplicationManager.RouteFindModeEnum.ReservedBlock));
        }

        #endregion

        #region Planning and Testing

        protected override TimeSpan GetPlannedDuration()
        {
            //TODO Damir: Dieser Aufruf sollte eigentlich nie stattfinden, da durch Callback von PAProcessfunction 
            // die Zeit in _LastCallbackResult stehen sollte. Falls nicht, dann muss hier die Dauer errechnet werden, durch den Start- und Endzeitpunkt
            // der im ACProgram-Log steht => Warten bis ACProgram-Log programmiert wird
            //return base.GetPlannedDuration();
            return TimeSpan.Zero;
        }

        #endregion

        #endregion

    }
}
