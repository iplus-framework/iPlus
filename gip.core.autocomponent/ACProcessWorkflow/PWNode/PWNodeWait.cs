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
    public class PWNodeWait : PWBaseNodeProcess, IACMyConfigCache
    {
        public const string PWClassName = "PWNodeWait";

        private bool _SubscribedToTimerCycle = false;


#region Properties

        [ACPropertyBindingSource(700, "ACConfig", "en{'Start time'}de{'Startzeit'}", "", false, true)]
        public IACContainerTNet<DateTime> StartTime { get; set; }

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

#endregion

#region Constructors

        static PWNodeWait()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("Duration", typeof(TimeSpan), TimeSpan.Zero, Global.ParamOption.Required));
            paramTranslation.Add("Duration", "en{'Waitingtime'}de{'Wartezeit'}");
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
            ClearMyConfiguration();
            UnSubscribeToTimerCycle();
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties
        private ACMethod _MyConfiguration;
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
                _MyConfiguration = myNewConfig;
                return myNewConfig;
            }
        }

        public void ClearMyConfiguration()
        {

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _MyConfiguration = null;
            }
            this.HasRules.ValueT = 0;
        }

        protected TimeSpan Duration
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

        #endregion

        #region Public
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "CancelWaiting":
                    CancelWaiting();
                    return true;
                case Const.IsEnabledPrefix + "CancelWaiting":
                    result = IsEnabledCancelWaiting();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWNodeWait(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }


        public override void SMIdle()
        {
            base.SMIdle();
            WaitingTime.ValueT = TimeSpan.Zero;
            LastPauseTime.ValueT = DateTime.MinValue;
            ElapsedTime.ValueT = TimeSpan.Zero;
            RemainingTime.ValueT = TimeSpan.Zero;
            SumPausingTimes.ValueT = TimeSpan.Zero;

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
            //if (!PreExecute(PABaseState.SMStarting))
            //  return;
            var newMethod = NewACMethodWithConfiguration();
            CreateNewProgramLog(newMethod, true);

            StartTime.ValueT = DateTime.Now;
            WaitingTime.ValueT = new TimeSpan(0, 0, new Random().Next(5, 15));

            RecalcTimeInfo();
            WaitingTime.ValueT = GetPlannedDuration();
            EndTime.ValueT = StartTime.ValueT + WaitingTime.ValueT;
            RemainingTime.ValueT = EndTime.ValueT - DateTime.Now;
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
            base.SMCompleted();
        }

        public override void Pause()
        {
            base.Pause();
            if (CurrentACState == ACStateEnum.SMPaused)
            {
                LastPauseTime.ValueT = DateTime.Now;
            }
        }

        public override void Resume()
        {
            base.Resume();
            if (CurrentACState == ACStateEnum.SMRunning)
            {
                if (LastPauseTime.ValueT != null && LastPauseTime.ValueT > DateTime.MinValue)
                {
                    TimeSpan pauseDuration = DateTime.Now - LastPauseTime.ValueT;
                    SumPausingTimes.ValueT = SumPausingTimes.ValueT + pauseDuration;
                    EndTime.ValueT = EndTime.ValueT + pauseDuration;
                }
                LastPauseTime.ValueT = DateTime.MinValue;
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
            if (_SubscribedToTimerCycle || ApplicationManager == null)
                return;
            ApplicationManager.ProjectWorkCycleR200ms += objectManager_ProjectTimerCycle200ms;
            _SubscribedToTimerCycle = true;
        }

        protected virtual void UnSubscribeToTimerCycle()
        {
            if (!_SubscribedToTimerCycle || ApplicationManager == null)
                return;
            ApplicationManager.ProjectWorkCycleR200ms -= objectManager_ProjectTimerCycle200ms;
            _SubscribedToTimerCycle = false;
        }

        protected virtual void objectManager_ProjectTimerCycle200ms(object sender, EventArgs e)
        {
            if (CurrentACState == ACStateEnum.SMPaused && LastPauseTime.ValueT != null && LastPauseTime.ValueT > DateTime.MinValue)
            {
                RemainingTime.ValueT = EndTime.ValueT - LastPauseTime.ValueT;
                if (SumPausingTimes.ValueT != null && SumPausingTimes.ValueT > TimeSpan.Zero)
                    ElapsedTime.ValueT = LastPauseTime.ValueT - StartTime.ValueT - SumPausingTimes.ValueT;
                else
                    ElapsedTime.ValueT = LastPauseTime.ValueT - StartTime.ValueT;
            }
            else
            {
                RemainingTime.ValueT = EndTime.ValueT - DateTime.Now;
                if (SumPausingTimes.ValueT != null && SumPausingTimes.ValueT > TimeSpan.Zero)
                    ElapsedTime.ValueT = DateTime.Now - StartTime.ValueT - SumPausingTimes.ValueT;
                else
                    ElapsedTime.ValueT = DateTime.Now - StartTime.ValueT;
            }

            if (RemainingTime.ValueT < TimeSpan.Zero)
            {
                CurrentACState = ACStateEnum.SMCompleted;
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
