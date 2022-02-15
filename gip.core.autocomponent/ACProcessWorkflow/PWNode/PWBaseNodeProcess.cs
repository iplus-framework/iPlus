using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    ///   <para>PWBaseNodeProcess extends PWBaseExecutable by a more extensive  state machine. It will be the
    ///   <br />- SMPaused state supported, so that a running process can be paused and the
    ///   <br />- SMCompleted state supported, so that an opened ProgramLog can be completed.
    ///   <br />The SMCompleted status is either set directly in the derivatives or by the callback method "TaskCallback()".
    ///   <br />PWBaseNodeProcess gets this method through the implementation of the IACComponentTaskSubscr interface. 
    ///   IACComponentTaskSubscr is the counterpart to the IACComponentTaskExec interface in  order to  provide the necessary subscription point for asynchronous calls .</para>
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBaseExecutable" />
    /// <seealso cref="gip.core.autocomponent.IACComponentTaskSubscr" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWBaseNodeProcess'}de{'PWBaseNodeProcess'}", Global.ACKinds.TPWNode, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, false)]
    public abstract class PWBaseNodeProcess : PWBaseExecutable, IACComponentTaskSubscr
    {
        #region c´tors
        protected ACEventArgs _LastCallbackResult = null;

        public PWBaseNodeProcess(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PWPointRunning = new ACPointEvent(this, "PWPointRunning", 0);
            _TaskSubscriptionPoint = new ACPointAsyncRMISubscr(this, Const.TaskSubscriptionPoint, 0);
        }

        static PWBaseNodeProcess()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string, ACEventArgs>(PWBaseExecutable.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("TimeInfo", typeof(PATimeInfo), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("PWPointRunning", TMP);

            RegisterExecuteHandler(typeof(PWBaseNodeProcess), HandleExecuteACMethod_PWBaseNodeProcess);
        }

        public override bool ACPreDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACPreDeInit(deleteACClassTask);
            if (!result)
                return result;
            if (deleteACClassTask)
                TaskSubscriptionPoint.UnSubscribe();
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _LastCallbackResult = null;
            _InCallback = false;
            _RepeatAfterCompleted = false;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion


        #region Points
        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;
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


        protected ACPointEvent _PWPointRunning;
        [ACPropertyEventPoint(9999, true)]
        public ACPointEvent PWPointRunning
        {
            get
            {
                return _PWPointRunning;
            }
        }


        #region IACComponentTaskSubscr
        protected ACPointAsyncRMISubscr _TaskSubscriptionPoint;
        [ACPropertyAsyncMethodPointSubscr(9999, true, 0, "TaskCallback")]
        public ACPointAsyncRMISubscr TaskSubscriptionPoint
        {
            get
            {
                return _TaskSubscriptionPoint;
            }
        }

        public ACPointNetEventDelegate TaskCallbackDelegate
        {
            get
            {
                return TaskCallback;
            }
        }

        protected bool _RepeatAfterCompleted = false;
        public bool RepeatAfterCompleted
        {
            get
            {
                return _RepeatAfterCompleted;
            }
        }

        public bool IsTaskStarted(IACPointEntry acPointEntry)
        {
            if (acPointEntry == null)
                return false;
            if (acPointEntry.State == PointProcessingState.Rejected)
                return false;
            return true;
        }

        #endregion

        #endregion


        #region Properties
        protected bool _InCallback = false;
        protected ACMethodEventArgs _CurrentMethodEventArgs = null;
        #endregion


        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.SMRunning:
                    SMRunning();
                    return true;
                case ACStateConst.SMPaused:
                    SMPaused();
                    return true;
                case ACStateConst.SMCompleted:
                    SMCompleted();
                    return true;
                case "RaiseRunningEvent":
                    RaiseRunningEvent();
                    return true;
                case ACStateConst.TMPause:
                    Pause();
                    return true;
                case ACStateConst.TMResume:
                    Resume();
                    return true;
                case "TaskCallback":
                    TaskCallback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
                case Const.IsEnabledPrefix + "RaiseRunningEvent":
                    result = IsEnabledRaiseRunningEvent();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMPause:
                    result = IsEnabledPause();
                    return true;
                case Const.IsEnabledPrefix + ACStateConst.TMResume:
                    result = IsEnabledResume();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWBaseNodeProcess(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseExecutable(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        #region ACState

        public override void SMIdle()
        {
            _RepeatAfterCompleted = false;
            base.SMIdle();
        }

        protected override bool IsEnabledStartInternal()
        {
            return CurrentACState == ACStateEnum.SMIdle 
                || (CurrentACState == ACStateEnum.SMCompleted && RepeatAfterCompleted);
        }

        public override void SMStarting()
        {
            _RepeatAfterCompleted = false;
            RecalcTimeInfo();
            // Falls durch tiefere Callstacks der Status schon weitergeschaltet worden ist, dann schalte Status nicht weiter
            if (CurrentACState == ACStateEnum.SMStarting)
                CurrentACState = ACStateEnum.SMRunning;
        }


        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public virtual void SMRunning()
        {
            if (CyclicWaitIfSimulationOn())
                return;

            // Falls durch tiefere Callstacks der Status schon weitergeschaltet worden ist, dann schalte Status nicht weiter
            if (CurrentACState == ACStateEnum.SMRunning)
                CurrentACState = ACStateEnum.SMCompleted;
        }

        [ACMethodState("en{'Paused'}de{'Pausiert'}", 35, true)]
        public virtual void SMPaused()
        {
        }


        [ACMethodState("en{'Completed'}de{'Beendet'}", 40, true)]
        public virtual void SMCompleted()
        {
            if (_RepeatAfterCompleted)
            {
                try
                {
                    UnSubscribeToProjectWorkCycle();
                    Start();
                }
                finally
                {
                    _RepeatAfterCompleted = false;
                }
                return;
            }

            RecalcTimeInfo();
            ACMethod acMethod = ExecutingACMethod;
            ACMethod configMethod = MyConfiguration;
            if (    configMethod != null
                && (acMethod == null || acMethod.ACIdentifier != configMethod.ACIdentifier))
            {
                acMethod = configMethod;
            }
            
            FinishProgramLog(acMethod);
            IterationCount.ValueT++;

            ACMethodEventArgs lastMethodResult = null;
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(Const.PWPointOut, VirtualEventArgs);
            if (_LastCallbackResult != null)
            {
                lastMethodResult = _LastCallbackResult as ACMethodEventArgs;
                ACValue durationValue = _LastCallbackResult.GetACValue("TimeInfo");
                if (durationValue != null)
                    eventArgs.GetACValue("TimeInfo").Value = durationValue.Value;
                else
                    eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();
                _LastCallbackResult = null;
            }
            else
                eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();

            if (IsACStateMethodConsistent(ACStateEnum.SMCompleted) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
            {
                if (lastMethodResult != null && lastMethodResult.ResultState == Global.ACMethodResultState.FailedAndRepeat)
                {
                    _RepeatAfterCompleted = true;
                    SubscribeToProjectWorkCycle();
                    return;
                }
                else
                {
                    Reset();
                }
            }
            RaiseOutPoint(eventArgs);
        }


        protected virtual void RaiseOutPoint(ACEventArgs eventArgs)
        {
            PWPointOut.Raise(eventArgs);
        }


        [ACMethodInteraction("Process", "en{'Raise running event'}de{'Läuft-Ereignis auslösen'}", 2001, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void RaiseRunningEvent()
        {
            if (!IsEnabledRaiseRunningEvent())
                return;
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointRunning", VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();
            PWPointRunning.Raise(eventArgs);
        }


        public virtual bool IsEnabledRaiseRunningEvent()
        {
            return true;
        }


        protected override void DumpPointList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPointList(doc, xmlACPropertyList);

            XmlElement wfInfos = xmlACPropertyList["PWPointRunningInfo"];
            if (wfInfos == null && ContentACClassWF != null)
            {
                wfInfos = doc.CreateElement("PWPointRunningInfo");
                if (wfInfos != null)
                {
                    wfInfos.InnerText = PWPointRunning.DumpStateInfo();
                }
                xmlACPropertyList.AppendChild(wfInfos);
            }

        }

        #region User
        [ACMethodInteraction("Process", "en{'Pause'}de{'Pause'}", 300, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Pause()
        {
            if (!IsEnabledPause())
                return;
            CurrentACState = ACStateEnum.SMPaused;
        }

        public virtual bool IsEnabledPause()
        {
            return CurrentACState == ACStateEnum.SMRunning;
        }

        [ACMethodInteraction("Process", "en{'Resume'}de{'Fortsetzen'}", 300, true,"", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void Resume()
        {
            if (!IsEnabledResume())
                return;
            CurrentACState = ACStateEnum.SMRunning;
        }

        public virtual bool IsEnabledResume()
        {
            return CurrentACState == ACStateEnum.SMPaused;
        }
        #endregion

        #endregion


        #region Misc
        public override PAOrderInfo GetPAOrderInfo()
        {
            PWGroup group = ParentACComponent as PWGroup;
            if (group != null)
                return group.GetPAOrderInfo();
            return base.GetPAOrderInfo();
        }

        public T CurrentExecutingFunction<T>() where T : PAProcessFunction
        {
            IEnumerable<ACPointAsyncRMISubscrWrap<ACComponent>> taskEntries = null;

            using (ACMonitor.Lock(TaskSubscriptionPoint.LockLocalStorage_20033))
            {
                taskEntries = this.TaskSubscriptionPoint.ConnectionList.ToArray();
            }
            // Falls Dosierung zur Zeit aktiv ist, dann gibt es auch einen Eintrag in der TaskSubscriptionListe
            if (taskEntries != null && taskEntries.Any())
            {
                foreach (var entry in taskEntries)
                {
                    T currentExecutingFunction = ParentPWGroup.GetExecutingFunction<T>(entry.RequestID);
                    if (currentExecutingFunction != null)
                        return currentExecutingFunction;
                }
            }
            return null;
        }
        #endregion


        #region Callbacks
        [ACMethodInfo("Function", "en{'TaskCallback'}de{'TaskCallback'}", 9999)]
        public virtual void TaskCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            _InCallback = true;
            try
            {
                if (e != null)
                {
                    IACTask taskEntry = wrapObject as IACTask;
                    ACMethodEventArgs eM = e as ACMethodEventArgs;
                    _CurrentMethodEventArgs = eM;
                    if (taskEntry.State == PointProcessingState.Deleted /*&& taskEntry.InProcess*/)
                    {
                        _LastCallbackResult = e;
                        CurrentACState = ACStateEnum.SMCompleted;
                    }
                    else if (PWPointRunning != null && eM != null && eM.ResultState == Global.ACMethodResultState.InProcess && taskEntry.State == PointProcessingState.Accepted)
                    {
                        PAProcessModule module = sender.ParentACComponent as PAProcessModule;
                        if (module != null)
                        {
                            PAProcessFunction function = module.GetExecutingFunction<PAProcessFunction>(eM.ACRequestID);
                            if (function != null)
                            {
                                if (function.CurrentACState == ACStateEnum.SMRunning 
                                    || function.CurrentACState == ACStateEnum.SMPaused || function.CurrentACState == ACStateEnum.SMPausing
                                    || function.CurrentACState == ACStateEnum.SMHeld || function.CurrentACState == ACStateEnum.SMHolding)
                                {
                                    ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointRunning", VirtualEventArgs);
                                    eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();
                                    PWPointRunning.Raise(eventArgs);
                                }
                            }
                        }
                    }
                    // Starting of a Method failed
                    else if (taskEntry.State == PointProcessingState.Rejected)
                    {
                    }
                }
            }
            finally
            {
                _InCallback = false;
            }
        }
        #endregion
        
        #endregion
    }
}
