using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Process-Knoten zur implementierung eines Workflowinternen Processes
    /// 
    /// Methoden zur Steuerung von außen: 
    /// -Start()    Starten des Processes
    ///
    /// Mögliche ACState:
    /// SMIdle      (Definiert in ACComponent)
    /// SMStarting (Definiert in PWNode, Überschrieben in PWMethodCall)
    /// SMRunning   (Definiert in PWMethodCall)
    /// SMCompleted (Definiert in PWMethodCall)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Wait'}de{'Warten'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeWait : PWBaseNodeProcess
    {
        public const string PWClassName = "PWNodeWait";

        protected bool _SubscribedToTimerCycle = false;


        #region Properties

        /// <summary>
        /// Starttime without daylight saving (Always winter time)
        /// </summary>
        [ACPropertyBindingSource(700, "ACConfig", "en{'Start time'}de{'Startzeit'}", "", false, true)]
        public IACContainerTNet<DateTime> StartTime { get; set; }

        /// <summary>
        /// Endtime without daylight saving (Always winter time)
        /// </summary>
        [ACPropertyBindingSource(701, "ACConfig", "en{'End time'}de{'Endezeit'}", "", false, true)]
        public IACContainerTNet<DateTime> EndTime { get; set; }

        [ACPropertyBindingSource(702, "ACConfig", "en{'Waiting time'}de{'Wartezeit'}", "", false, true)]
        public IACContainerTNet<TimeSpan> WaitingTime { get; set; }

        [ACPropertyBindingSource(703, "ACConfig", "en{'Remaining time'}de{'Restzeit'}", "", false, false)]
        public IACContainerTNet<TimeSpan> RemainingTime { get; set; }

        [ACPropertyBindingSource(704, "ACConfig", "en{'Elapsed time'}de{'Abgelaufene Zeit'}", "", false, false)]
        public IACContainerTNet<TimeSpan> ElapsedTime { get; set; }

        [ACPropertyBindingSource(705, "ACConfig", "en{'Last Pause'}de{'Zuletzt pausiert'}", "", false, true)]
        public IACContainerTNet<DateTime> LastPauseTime { get; set; }

        [ACPropertyBindingSource(702, "ACConfig", "en{'Sum of all Pauses'}de{'Summe aller Pausen'}", "", false, true)]
        public IACContainerTNet<TimeSpan> SumPausingTimes { get; set; }

        /// <summary>
        /// Starttime according daylightsaving  (winter/summertime)
        /// </summary>
        [ACPropertyBindingSource(708, "", "en{'Start time for view'}de{'Startzeit zur Anzeige'}", "", false, false)]
        public IACContainerTNet<DateTime> StartTimeView { get; set; }

        /// <summary>
        /// Endtime according daylightsaving  (winter/summertime)
        /// </summary>
        [ACPropertyBindingSource(706, "", "en{'End time for view'}de{'End zur Anzeige'}", "", false, false)]
        public IACContainerTNet<DateTime> EndTimeView { get; set; }

        #endregion

        #region Constructors

        static PWNodeWait()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("Duration", typeof(TimeSpan), TimeSpan.Zero, Global.ParamOption.Required));
            paramTranslation.Add("Duration", "en{'Waitingtime'}de{'Wartezeit'}");
            method.ParameterValueList.Add(new ACValue("ACUrlCmdOnEnd", typeof(string), null, Global.ParamOption.Optional));
            paramTranslation.Add("ACUrlCmdOnEnd", "en{'ACUrlCommand on end'}de{'ACUrlCommand am Ende'}");
            var wrapper = new ACMethodWrapper(method, "en{'Wait'}de{'Warten'}", typeof(PWNodeWait), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodeWait), ACStateConst.SMStarting, wrapper);
            //ACMethod.RegisterVirtualMethod(typeof(PWNodeWait), PABaseState.SMStarting, method, "en{'Wait'}de{'Warten'}", null);
            RegisterExecuteHandler(typeof(PWNodeWait), HandleExecuteACMethod_PWNodeWait);
        }

        public PWNodeWait(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            UnSubscribeToTimerCycle();
            if (StartTime != null)
                (StartTime as IACPropertyNetServer).ValueUpdatedOnReceival -= PWNodeWait_ValueUpdatedOnReceival;
            if (EndTime != null)
                (EndTime as IACPropertyNetServer).ValueUpdatedOnReceival -= PWNodeWait_ValueUpdatedOnReceival;
            if (ApplicationManager != null)
                ApplicationManager.DaylightSavingTimeSwitched -= ApplicationManager_DaylightSavingTimeSwitched;
            if (deleteACClassTask)
                ResetTimeProperties(true);
            return base.ACDeInit(deleteACClassTask);
        }


        public override bool ACPostInit()
        {
            if (StartTime != null)
                (StartTime as IACPropertyNetServer).ValueUpdatedOnReceival += PWNodeWait_ValueUpdatedOnReceival;
            if (EndTime != null)
                (EndTime as IACPropertyNetServer).ValueUpdatedOnReceival += PWNodeWait_ValueUpdatedOnReceival;

            UpdateViewTimes();
            if (ApplicationManager != null)
                ApplicationManager.DaylightSavingTimeSwitched += ApplicationManager_DaylightSavingTimeSwitched;
            return base.ACPostInit();
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            base.Recycle(content, parentACObject, parameter, acIdentifier);
            ResetTimeProperties(true);
        }

        private void ResetTimeProperties(bool resetPersistedStartEnd = false)
        {
            if (resetPersistedStartEnd)
            {
                if (StartTime != null)
                    StartTime.ValueT = DateTime.MinValue;
                if (EndTime != null)
                    EndTime.ValueT = DateTime.MinValue;
                if (StartTimeView != null)
                    StartTimeView.ValueT = DateTime.MinValue;
                if (EndTimeView != null)
                    EndTimeView.ValueT = DateTime.MinValue;
            }
            WaitingTime.ValueT = TimeSpan.Zero;
            LastPauseTime.ValueT = DateTime.MinValue;
            ElapsedTime.ValueT = TimeSpan.Zero;
            RemainingTime.ValueT = TimeSpan.Zero;
            SumPausingTimes.ValueT = TimeSpan.Zero;
        }

        #endregion

        #region Properties
        public TimeSpan Duration
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("Duration");
                    if (acValue != null)
                    {
                        TimeSpan duration = acValue.ParamAsTimeSpan;
                        if (duration < TimeSpan.Zero)
                            duration = TimeSpan.Zero;
                        return duration;
                    }
                }
                return TimeSpan.Zero;
            }
        }

        public string ACUrlCmdOnEnd
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ACUrlCmdOnEnd");
                    if (acValue != null)
                        return acValue.ParamAsString;
                }
                return null;
            }
        }


        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }

        public virtual bool ActsAsAlarmClock
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Public
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(CancelWaiting):
                    CancelWaiting();
                    return true;
                case nameof(IsEnabledCancelWaiting):
                    result = IsEnabledCancelWaiting();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWNodeWait(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        public override void Reset()
        {
            UnSubscribeToTimerCycle();
            base.Reset();
        }

        public override void SMIdle()
        {
            base.SMIdle();
            UnSubscribeToTimerCycle();
            ResetTimeProperties(false);
            if (ParentPWGroup != null)
            {
                var processModule = ParentPWGroup.AccessedProcessModule;
                if (processModule != null)
                    processModule.RefreshPWNodeInfo();
            }
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            if (!CheckParentGroupAndHandleSkipMode())
                return;

            //if (!PreExecute(PABaseState.SMStarting))
            //  return;
            var newMethod = NewACMethodWithConfiguration();
            CreateNewProgramLog(newMethod, true);
            RecalcTimeInfo();

            WaitingTime.ValueT = GetPlannedDuration();
            DateTime dtNow = DateTimeUtils.NowDST;
            if (ActsAsAlarmClock)
            {
                if (EndTime.ValueT <= DateTime.MinValue)
                    EndTime.ValueT = dtNow + WaitingTime.ValueT;
                StartTime.ValueT = dtNow;
            }
            else
            {
                StartTime.ValueT = dtNow;
                EndTime.ValueT = dtNow + WaitingTime.ValueT;
            }

            RemainingTime.ValueT = EndTime.ValueT - dtNow;
            ElapsedTime.ValueT = TimeSpan.Zero;

            if (ACOperationMode == ACOperationModes.Live)
            {
                if (ParentPWGroup != null)
                {
                    var processModule = ParentPWGroup.AccessedProcessModule;
                    if (processModule != null)
                        processModule.RefreshPWNodeInfo();
                }
                CurrentACState = ACStateEnum.SMRunning;
            }
            else
                CurrentACState = ACStateEnum.SMCompleted;
            //PostExecute(PABaseState.SMStarting);
        }

        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public override void SMRunning()
        {
            SubscribeToTimerCycle();
        }

        [ACMethodState("en{'Completed'}de{'Beendet'}", 40, true)]
        public override void SMCompleted()
        {
            UnSubscribeToTimerCycle();
            OnACUrlCmdOnEnd();
            base.SMCompleted();
        }

        protected virtual void OnACUrlCmdOnEnd()
        {
            string cmd = ACUrlCmdOnEnd;
            if (!String.IsNullOrEmpty(cmd))
                ACUrlCommand(cmd);
        }

        public override void Pause()
        {
            base.Pause();
            if (CurrentACState == ACStateEnum.SMPaused)
            {
                LastPauseTime.ValueT = DateTimeUtils.NowDST;
            }
        }

        public override void Resume()
        {
            base.Resume();
            if (CurrentACState == ACStateEnum.SMRunning)
            {
                if (!ActsAsAlarmClock)
                {
                    if (LastPauseTime.ValueT != null && LastPauseTime.ValueT > DateTime.MinValue)
                    {
                        TimeSpan pauseDuration = DateTimeUtils.NowDST - LastPauseTime.ValueT;
                        SumPausingTimes.ValueT = SumPausingTimes.ValueT + pauseDuration;
                        EndTime.ValueT = EndTime.ValueT + pauseDuration;
                    }
                    LastPauseTime.ValueT = DateTime.MinValue;
                }
            }
        }

        [ACMethodInteraction("Process", "en{'Cancel waiting time'}de{'Wartezeit abbrechen'}", 299, true)]
        public void CancelWaiting()
        {
            if (IsEnabledCancelWaiting())
                CurrentACState = ACStateEnum.SMCompleted;
        }

        public bool IsEnabledCancelWaiting()
        {
            return CurrentACState == ACStateEnum.SMRunning;
        }

#endregion

#region Protected

        protected virtual void SubscribeToTimerCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (_SubscribedToTimerCycle
                    || InitState != ACInitState.Initialized)
                    return;

                // Access to ApplicationManager not in OR-Condition above to avoid Attachment to ACRef of ParentComponent!
                if (ApplicationManager == null)
                    return;

                ApplicationManager.ProjectWorkCycleR200ms += objectManager_ProjectTimerCycle200ms;
                _SubscribedToTimerCycle = true;
            }
        }

        protected virtual void UnSubscribeToTimerCycle()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (!_SubscribedToTimerCycle)
                    return;
                if (this.ACRef == null)
                    return;

                bool wasDetached = !this.ACRef.IsAttached;
                if (ApplicationManager != null)
                    ApplicationManager.ProjectWorkCycleR200ms -= objectManager_ProjectTimerCycle200ms;
                _SubscribedToTimerCycle = false;
                if (wasDetached)
                    this.ACRef.Detach();
            }
        }

        private void PWNodeWait_ValueUpdatedOnReceival(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.BeforeBroadcast)
                return;
            UpdateViewTimes();
        }

        private void ApplicationManager_DaylightSavingTimeSwitched(object sender, EventArgs e)
        {
            UpdateViewTimes();
        }

        protected void UpdateViewTimes()
        {
            DateTime start = StartTime.ValueT;
            DateTime end = EndTime.ValueT;
            DateTime now = DateTime.Now;
            if (TimeZoneInfo.Local.SupportsDaylightSavingTime && now.IsDaylightSavingTime())
            {
                start = start.AddHours(1);
                end = end.AddHours(1);
            }
            bool viewTimeChanged = false;
            if (start != StartTimeView.ValueT)
            {
                StartTimeView.ValueT = start;
                viewTimeChanged = true;
            }
            if (end != EndTimeView.ValueT)
            {
                EndTimeView.ValueT = end;
                viewTimeChanged = true;
            }
            if (viewTimeChanged)
                OnViewTimeChanged();
        }

        protected virtual void OnViewTimeChanged()
        {
        }

        protected virtual void objectManager_ProjectTimerCycle200ms(object sender, EventArgs e)
        {
            if (this.InitState == ACInitState.Destructed || this.InitState == ACInitState.DisposingToPool || this.InitState == ACInitState.DisposedToPool)
            {
                gip.core.datamodel.Database.Root.Messages.LogError("PWNodeWait", "objectManager_ProjectTimerCycle200ms(1)", String.Format("Unsubcribed from Workcycle. Init-State is {0}, _SubscribedToTimerCycle is {1}, at Type {2}. Ensure that you unsubscribe from Work-Cycle in ACDeinit().", this.InitState, _SubscribedToTimerCycle, this.GetType().AssemblyQualifiedName));

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    (sender as ApplicationManager).ProjectWorkCycleR1sec -= objectManager_ProjectTimerCycle200ms;
                    _SubscribedToTimerCycle = false;
                }
                return;
            }

            //UpdateViewTimes();

            if (ActsAsAlarmClock)
            {
                DateTime nowDST = DateTimeUtils.NowDST;
                DateTime dt = EndTime.ValueT;
                RemainingTime.ValueT = dt - nowDST;
                ElapsedTime.ValueT = nowDST - StartTime.ValueT;
                if ((dt < nowDST || dt == DateTime.MinValue)
                     && CurrentACState >= ACStateEnum.SMStarting
                     && CurrentACState <= ACStateEnum.SMCompleted)
                {
                    CurrentACState = ACStateEnum.SMCompleted;
                }
            }
            else
            {
                if (CurrentACState == ACStateEnum.SMPaused && LastPauseTime.ValueT != null && LastPauseTime.ValueT > DateTime.MinValue)
                {
                    RemainingTime.ValueT = EndTime.ValueT - LastPauseTime.ValueT;
                    if (SumPausingTimes.ValueT != null && SumPausingTimes.ValueT > TimeSpan.Zero)
                        ElapsedTime.ValueT = LastPauseTime.ValueT - StartTime.ValueT - SumPausingTimes.ValueT;
                    else
                        ElapsedTime.ValueT = LastPauseTime.ValueT - StartTime.ValueT;
                }
                else if (CurrentACState >= ACStateEnum.SMRunning && CurrentACState <= ACStateEnum.SMCompleted)
                {
                    DateTime nowDST = DateTimeUtils.NowDST;
                    RemainingTime.ValueT = EndTime.ValueT - nowDST;
                    if (SumPausingTimes.ValueT != null && SumPausingTimes.ValueT > TimeSpan.Zero)
                        ElapsedTime.ValueT = nowDST - StartTime.ValueT - SumPausingTimes.ValueT;
                    else
                        ElapsedTime.ValueT = nowDST - StartTime.ValueT;
                }

                if (RemainingTime.ValueT < TimeSpan.Zero
                     && CurrentACState >= ACStateEnum.SMStarting
                     && CurrentACState <= ACStateEnum.SMCompleted)
                {
                    CurrentACState = ACStateEnum.SMCompleted;
                }
            }
        }

        protected override TimeSpan GetPlannedDuration()
        {
            return Duration;
        }

        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement xmlProperty = xmlACPropertyList["_SubscribedToTimerCycle"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("_SubscribedToTimerCycle");
                if (xmlProperty != null)
                    xmlProperty.InnerText = _SubscribedToTimerCycle.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

            xmlProperty = xmlACPropertyList["Duration"];
            if (xmlProperty == null)
            {
                xmlProperty = doc.CreateElement("Duration");
                if (xmlProperty != null)
                    xmlProperty.InnerText = Duration.ToString();
                xmlACPropertyList.AppendChild(xmlProperty);
            }

        }

        #endregion

    }
}
