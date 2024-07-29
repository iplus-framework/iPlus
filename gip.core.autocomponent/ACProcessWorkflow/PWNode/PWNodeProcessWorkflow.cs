using System;
using System.Collections.Generic;
using System.Linq;
using gip.core.datamodel;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Process-Knoten zur implementierung eines (asynchronen) Workflow-ACClassMethod-Aufruf auf die Model-Welt
    /// 
    /// Methoden zur Steuerung von außen: 
    /// -Start()    Starten des Processes
    ///
    /// Mögliche ACState:
    /// SMIdle      (Definiert in ACComponent)
    /// SMStarting (Definiert in PWNode)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Start subworkflow'}de{'Unterworkflow aufrufen'}", Global.ACKinds.TPWNodeWorkflow, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeProcessWorkflow : PWBaseNodeProcess
    {
        public const string PWClassName = "PWNodeProcessWorkflow";

        #region c´tors
        static PWNodeProcessWorkflow()
        {
            RegisterExecuteHandler(typeof(PWNodeProcessWorkflow), HandleExecuteACMethod_PWNodeProcessWorkflow);
        }

        public PWNodeProcessWorkflow(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Returns a List of Real- or Proxy-Instances which implements IACComponentTaskExec
        /// </summary>
        public virtual List<ACComponent> InvokableTaskExecutors
        {
            get
            {
                if (ContentACClassWF == null)
                    return new List<ACComponent>();
                ACClass refPAACClass = RefACClassOfContentWF;
                if (refPAACClass == null)
                    return new List<ACComponent>();
                IEnumerable<IACComponent> queryProject = null;
                // Kopiere ComponentClass, damit nicht QueryLock_1X000 ausserhalb von _20025_LockValue liegt und ein potentieller deadlock entsteht
                var childs = (this.Root as ACComponent).ACComponentChilds.Select(c => new { Child = c, CompClass = c.ComponentClass }).ToArray();

                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    queryProject = childs.Where(c => c.CompClass != null
                                                && c.CompClass.ACProject != null
                                                && c.CompClass.ACProject.ACProject1_BasedOnACProject != null
                                                && c.CompClass.ACProject.ACProject1_BasedOnACProject.ACProjectID == refPAACClass.ACProjectID)
                                                .Select(c => c.Child)
                                                .ToArray();
                }
                if (!queryProject.Any())
                    return new List<ACComponent>();
                List<ACComponent> list = new List<ACComponent>();
                foreach (ACComponent projComp in queryProject)
                {
                    List<ACComponent> listSub = projComp.FindChildComponents(refPAACClass, 0).Select(c => c as ACComponent).ToList();
                    if (projComp.ComponentClass.IsDerivedClassFrom(refPAACClass))
                        listSub.Add(projComp);
                    if (listSub.Count > 0)
                        list.AddRange(listSub);
                }

                if (list.Any())
                {
                    RuleValueList ruleValueList = null;
                    //using (IACEntityObjectContext db = (IACEntityObjectContext)Activator.CreateInstance(ParentACComponent.Database.GetType()))
                    //{
                    ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);
                    if (serviceInstance != null)
                    {
                        var mandantoryConfigStores = MandatoryConfigStores;
                        if (!ValidateExpectedConfigStores())
                            return new List<ACComponent>();
                        ruleValueList = serviceInstance.GetRuleValueList(mandantoryConfigStores, PreValueACUrl, ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Allowed_instances.ToString());
                    }
                    //}
                    if (ruleValueList != null)
                    {
                        IEnumerable<ACClass> selectedClasses = null;

                        using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                        {
                            selectedClasses = ruleValueList.GetSelectedClasses(ACClassWFRuleTypes.Allowed_instances, gip.core.datamodel.Database.GlobalDatabase);
                        }
                        if (selectedClasses != null && selectedClasses.Any())
                        {
                            var allowedComponents = selectedClasses.Select(c => c.GetACUrlComponent());
                            var filteredList = list.Where(c => allowedComponents.Contains(c.GetACUrl())).ToList();
                            return filteredList;
                        }
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Returns an Real- or Proxy-Instance which implements IACComponentTaskExec
        /// </summary>
        protected virtual ACComponent FirstInvokableTaskExecutor
        {
            get
            {
                List<ACComponent> list = InvokableTaskExecutors;
                if (!list.Any())
                    return null;
                return list.First();
            }
        }

        protected virtual ACMethod CurrentInvokedACMethod
        {
            get
            {
                if (TaskSubscriptionPoint == null)
                    return null;

                using (ACMonitor.Lock(TaskSubscriptionPoint.LockConnectionList_20040))
                {
                    if (!TaskSubscriptionPoint.ConnectionList.Any())
                        return null;
                    ACPointAsyncRMISubscrWrap<ACComponent> methodInvokeSubscr = TaskSubscriptionPoint.ConnectionList.First();
                    return methodInvokeSubscr.ACMethodDescriptor as ACMethod;
                }
            }
        }

        public bool HasActiveSubworkflows
        {
            get
            {
                if (this.TaskSubscriptionPoint.LocalStorage == null)
                    return false;
                ACPointAsyncRMISubscrWrap<ACComponent>[] activeInvocations = null;
                // Kann nicht beednet werden falls noch Sub-Workflows aktiv sind

                using (ACMonitor.Lock(this.TaskSubscriptionPoint.LockLocalStorage_20033))
                {
                    activeInvocations = this.TaskSubscriptionPoint.LocalStorage.Where(c => c.State <= PointProcessingState.Accepted).ToArray();
                }
                if (activeInvocations == null || !activeInvocations.Any())
                    return false;
                return true;
            }
        }

        public int CountActiveSubworkflows
        {
            get
            {
                if (this.TaskSubscriptionPoint.LocalStorage == null)
                    return -1;
                ACPointAsyncRMISubscrWrap<ACComponent>[] activeInvocations = null;
                // Kann nicht beednet werden falls noch Sub-Workflows aktiv sind

                using (ACMonitor.Lock(this.TaskSubscriptionPoint.LockLocalStorage_20033))
                {
                    activeInvocations = this.TaskSubscriptionPoint.LocalStorage.Where(c => c.State <= PointProcessingState.Accepted).ToArray();
                }
                if (activeInvocations == null || !activeInvocations.Any())
                    return 0;
                return activeInvocations.Count();
            }
        }

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }

        [ACPropertyBindingSource(211, nameof(HasPlanning), "en{'Has planning'}de{'Hat Planung'}", "", false, false)]
        public IACContainerTNet<short> HasPlanning { get; set; }

        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        public static bool HandleExecuteACMethod_PWNodeProcessWorkflow(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        #region ACState
        public override void Reset()
        {
            this.TaskSubscriptionPoint.UnSubscribe();
            base.Reset();
        }

        public override void SMStarting()
        {
            //if (!PreExecute(PABaseState.SMStarting))
            //  return;

            AddNewTaskToApplication();

            // Falls module.AddTask synchron ausgeführt wurde, dann ist der Status schon weiter
            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod)
            {
                CurrentACState = ACStateEnum.SMRunning;
                //PostExecute(PABaseState.SMStarting);
            }

            HasPlanning.ValueT = 0;
            ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);
            if (serviceInstance != null && ContentACClassWF != null)
            {
                IACConfigStore currentConfigStore = MandatoryConfigStores.OrderBy(c => c.OverridingOrder).FirstOrDefault();
                HasPlanning.ValueT = (short)(serviceInstance.HasPlanning(Database, currentConfigStore, ContentACClassWF.ACClassWFID) ? 1 : 0);
            }
        }

        public override void SMRunning()
        {
            //if (!PreExecute(PABaseState.SMRunning))
            //  return;
            //CurrentACState = PABaseState.SMCompleted;
            //PostExecute(PABaseState.SMRunning);
        }

        public override void SMCompleted()
        {
            if (HasActiveSubworkflows)
            {
                CurrentACState = ACStateEnum.SMRunning;
                return;
            }

            base.SMCompleted();
        }

        protected virtual bool AddNewTaskToApplication()
        {
            ACClassMethod refPAACClassMethod = RefACClassMethodOfContentWF;
            if (refPAACClassMethod != null)
            {
                ACComponent pFunction = FirstInvokableTaskExecutor;
                if (pFunction == null)
                {
                    string messageText = "FirstInvokableTaskExecutor is null!";
                    Msg msg = new Msg(messageText, this, eMsgLevel.Error, PWNodeProcessWorkflow.PWClassName, "AddNewTaskToApplication()", 1000);
                    if (IsAlarmActive(ProcessAlarm, msg.Message) == null)
                    {
                        Messages.LogMessageMsg(msg);
                        OnNewAlarmOccurred(ProcessAlarm, msg, true);
                    }
                    return false;
                }

                ACMethod paramMethod = refPAACClassMethod.TypeACSignature();
                if (paramMethod.ACIdentifier != refPAACClassMethod.ACIdentifier)
                {
                    paramMethod.ACIdentifier = refPAACClassMethod.ACIdentifier;
                }
                if (!(bool)ExecuteMethod(nameof(GetConfigForACMethod), paramMethod, true))
                    return false;

                RecalcTimeInfo();
                if (CreateNewProgramLog(paramMethod) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                    return false;

                ACValue acValue = paramMethod.ParameterValueList.GetACValue(ACProgramLog.ClassName);
                var currentProgramLog = CurrentProgramLog;
                if (currentProgramLog != null)
                {
                    if (acValue == null)
                    {
                        acValue = new ACValue() { ACIdentifier = ACProgramLog.ClassName, Value = currentProgramLog.ACProgramLogID, ValueTypeACClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(Guid)), Option = Global.ParamOption.Required };
                        paramMethod.ParameterValueList.Add(acValue);
                    }
                    else
                        acValue.Value = CurrentProgramLog.ACProgramLogID;
                }

                acValue = paramMethod.ParameterValueList.GetACValue(ACProgram.ClassName);
                if (acValue == null)
                {
                    acValue = new ACValue() { ACIdentifier = ACProgram.ClassName, Value = RootPW.CurrentACProgram.ACProgramID, ValueTypeACClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(Guid)), Option = Global.ParamOption.Required };
                    paramMethod.ParameterValueList.Add(acValue);
                }
                else
                    acValue.Value = RootPW.CurrentACProgram.ACProgramID;

                acValue = paramMethod.ParameterValueList.GetACValue(PWProcessFunction.C_InvocationCount);
                if (acValue == null)
                {
                    acValue = new ACValue() { ACIdentifier = PWProcessFunction.C_InvocationCount, Value = this.IterationCount.ValueT, ValueTypeACClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(int)), Option = Global.ParamOption.Optional };
                    paramMethod.ParameterValueList.Add(acValue);
                }
                else
                    acValue.Value = this.IterationCount.ValueT;

                IACPointAsyncRMI rmiInvocationPoint = pFunction.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
                if (rmiInvocationPoint != null)
                {
                    IACPointEntry task = rmiInvocationPoint.AddTask(paramMethod, this);
                    bool wfSucceeded = IsTaskStarted(task);
                    if (!wfSucceeded)
                    {
                        //Error50145: Generation and starting of a new subworkflow failed.
                        Msg msg = new Msg(this, eMsgLevel.Error, PWClassName, "AddNewTaskToApplication", 1000, "Error50145");
                        OnNewAlarmOccurred(ProcessAlarm, msg, true);
                    }
                    return wfSucceeded;
                }
                return true;
            }
            return false;
        }

        public virtual Guid[] GetCachedDestinations(bool refreshCache, out short errorCode, out string errorMsg)
        {
            errorCode = -1;
            errorMsg = null;
            return new Guid[] { };
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

        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlChild = xmlACPropertyList["HasActiveSubworkflows"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("HasActiveSubworkflows");
                if (xmlChild != null)
                    xmlChild.InnerText = HasActiveSubworkflows.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }

            xmlChild = xmlACPropertyList["CountActiveSubworkflows"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("CountActiveSubworkflows");
                if (xmlChild != null)
                    xmlChild.InnerText = CountActiveSubworkflows.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }
        }

        #endregion

        #endregion
    }
}
