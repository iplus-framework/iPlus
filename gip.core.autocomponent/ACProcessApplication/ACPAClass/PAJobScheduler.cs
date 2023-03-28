using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;


namespace gip.core.autocomponent
{
    [ACSerializeableInfo(new Type[]{typeof(PAExportPeriodType), typeof(DayOfWeek)})]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Type of period'}de{'Typ des Zeitintervalls'}", Global.ACKinds.TACEnum)]
    public enum PAExportPeriodType : short
    {
        SeveralTimesADay = 0, // Mehrmals täglich
        Daily = 1, // Einmal Täglich
        Weekly = 2, // Wöchentlich
    }


    /// <summary>
    /// Exporter for Property-Logs (Root-Node)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Job sceduler'}de{'Aufgabenplaner'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class PAJobScheduler : PABase
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        #region Properties

        private Boolean _RunScheduler;
        [ACPropertyInfo(true, 201, "", "en{'Allow execution'}de{'Erlaube Ausführung'}", "", true, DefaultValue = false)]
        public Boolean RunScheduler
        {
            get
            {
                return _RunScheduler;
            }
            set
            {
                _RunScheduler = value;
                OnPropertyChanged("RunScheduler");
            }
        }

        [ACPropertyBindingSource(202, "", "en{'Scheduling started'}de{'Zeitplanung aktiviert'}", "", true, true)]
        public IACContainerTNet<Boolean> SchedulerActive { get; set; }


        private PAExportPeriodType _TypeOfPeriod;
        [ACPropertyInfo(true, 211, "Configuration", "en{'Type of period'}de{'Typ des Zeitintervals'}", "", true)]
        public PAExportPeriodType TypeOfPeriod
        {
            get
            {
                return _TypeOfPeriod;
            }
            set
            {
                _TypeOfPeriod = value;
                OnPropertyChanged("TypeOfPeriod");
            }
        }

        private DayOfWeek _Weekday;
        [ACPropertyInfo(true, 212, "Configuration", "en{'Weekday'}de{'Wochentag'}", "", true)]
        public DayOfWeek Weekday
        {
            get
            {
                return _Weekday;
            }
            set
            {
                _Weekday = value;
                OnPropertyChanged("Weekday");
            }
        }

        private TimeSpan _Period;
        [ACPropertyInfo(true, 213, "Configuration", "en{'Period'}de{'Zeitintervall'}", "", true)]
        public TimeSpan Period
        {
            get
            {
                return _Period;
            }
            set
            {
                _Period = value;
                OnPropertyChanged("Period");
            }
        }

        private DateTime _LastRunDateSaved;
        [ACPropertyInfo(true, 213, "Configuration", "en{'date of last run'}de{'Datum letzte Ausführung'}", "", true)]
        public DateTime LastRunDateSaved
        {
            get
            {
                return _LastRunDateSaved;
            }
            set
            {
                _LastRunDateSaved = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyBindingSource(202, "", "en{'date of last run'}de{'Datum letzte Ausführung'}", "", true, false)]
        public IACContainerTNet<DateTime> LastRunDate { get; set; }

        [ACPropertyBindingSource(203, "", "en{'date of next run'}de{'Datum nächste Ausführung'}", "", true, false)]
        public IACContainerTNet<DateTime> NextRunDate { get; set; }

        private int _CycleCounter = 0;

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

        #endregion

        #region Constructors

        static PAJobScheduler()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PABase.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("LastRunDate", typeof(DateTime), DateTime.MinValue, Global.ParamOption.Required));
            TMP.Add(new ACValue("RunDate", typeof(DateTime), DateTime.MinValue, Global.ParamOption.Required));
            TMP.Add(new ACValue("NextRunDate", typeof(DateTime), DateTime.MinValue, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("RunJobEvent", TMP);
        }

        public PAJobScheduler(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _RunJobEvent = new ACPointEvent(this, "RunJobEvent", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            if (LastRunDateSaved > DateTime.MinValue)
                LastRunDate.ValueT = LastRunDateSaved;
            else
                LastRunDate.ValueT = DateTime.Now;
            StartScheduling();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopScheduling();
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

#endregion

#region Points

        protected ACPointEvent _RunJobEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent RunJobEvent
        {
            get
            {
                return _RunJobEvent;
            }
            set
            {
                _RunJobEvent = value;
            }
        }
        #endregion

        #region Public

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "StartScheduling":
                    StartScheduling();
                    return true;
                case "IsEnabledStartScheduling":
                    result = IsEnabledStartScheduling();
                    return true;
                case "StopScheduling":
                    StopScheduling();
                    return true;
                case "IsEnabledStopScheduling":
                    result = IsEnabledStopScheduling();
                    return true;
                //case "IsEnabledSubscribeToProjectWorkCycle10":
                //    result = IsEnabledSubscribeToProjectWorkCycle10();
                //    return true;
                //case "IsEnabledUnSubscribeToProjectWorkCycle10":
                //    result = IsEnabledUnSubscribeToProjectWorkCycle10();
                //    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInteraction("Scheduling", "en{'Activate scheduling'}de{'Aktiviere Zeitplanung'}", 200, true)]
        public void StartScheduling()
        {
            StopScheduling();
            if (!IsEnabledStartScheduling())
                return;
            OnStartScheduling();
            NextRunDate.ValueT = CalculateNextStartTime(TypeOfPeriod, Weekday, Period);
            SubscribeToProjectWorkCycle10();
        }

        protected virtual void OnStartScheduling()
        {
        }

        public virtual bool IsEnabledStartScheduling()
        {
            if (!RunScheduler)
                return false;
            if (_SubscribedToProjectWorkCycle10)
                return false;
            return true;
        }

        [ACMethodInteraction("Watching", "en{'Stop scheduling'}de{'Stoppe Zeitplanung'}", 201, true)]
        public void StopScheduling()
        {
            LastRunDateSaved = LastRunDate.ValueT;
            UnSubscribeToProjectWorkCycle10();
            OnStopScheduling();
        }

        protected virtual void OnStopScheduling()
        {
        }

        public virtual bool IsEnabledStopScheduling()
        {
            if (!_SubscribedToProjectWorkCycle10)
                return false;
            return true;
        }

        private bool _SubscribedToProjectWorkCycle10 = false;
        public bool IsSubscribedToProjectWorkCycle10
        {
            get
            {
                return _SubscribedToProjectWorkCycle10;
            }
        }

        public override ACProgramLog CurrentProgramLog
        {
            get
            {
                return null;
            }
        }

        protected override ACProgramLog GetCurrentProgramLog(bool attach, bool lookupOnlyInCache = false)
        {
            return null;
        }

        protected override void SetCurrentProgramLog(ACProgramLog value, bool detach)
        {
        }

        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override IEnumerable<ACProgramLog> PreviousProgramLogs
        {
            get
            {
                return null;
            }
        }

        public override ACProgramLog ParentProgramLog
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// NO-CHANGE-TRACKING!
        /// </summary>
        public override ACProgramLog PreviousParentProgramLog
        {
            get
            {
                return null;
            }
        }


        public void SubscribeToProjectWorkCycle10()
        {
            if (!IsEnabledSubscribeToProjectWorkCycle10())
                return;
            ApplicationManager.ProjectWorkCycleR10sec += objectManager_ProjectWorkCycle10;
            _SubscribedToProjectWorkCycle10 = true;
            SchedulerActive.ValueT = true;
        }

        public bool IsEnabledSubscribeToProjectWorkCycle10()
        {
            if (_SubscribedToProjectWorkCycle10 || ApplicationManager == null)
                return false;
            return true;
        }


        public void UnSubscribeToProjectWorkCycle10()
        {
            if (!IsEnabledUnSubscribeToProjectWorkCycle10())
                return;
            ApplicationManager.ProjectWorkCycleR10sec -= objectManager_ProjectWorkCycle10;
            _SubscribedToProjectWorkCycle10 = false;
            SchedulerActive.ValueT = false;
        }

        public bool IsEnabledUnSubscribeToProjectWorkCycle10()
        {
            if (!_SubscribedToProjectWorkCycle10 || ApplicationManager == null)
                return false;
            return true;
        }

        protected virtual void objectManager_ProjectWorkCycle10(object sender, EventArgs e)
        {
            _CycleCounter++;
            DateTime now = DateTime.Now;
            if (now >= NextRunDate.ValueT)
            {
                RaiseJobEvent();
            }
            else
            {
                DateTime prevExportDate = CalculatePrevStartTime(NextRunDate.ValueT, TypeOfPeriod, Weekday, Period);
                // Letzter Export wurde nicht durchgeführt, weil Server längere Zeit beendet war
                if (LastRunDate.ValueT < prevExportDate)
                {
                    RaiseJobEvent();
                }
            }
            if (_CycleCounter > 60)
            {
                _CycleCounter = 0;
                LastRunDateSaved = LastRunDate.ValueT;
            }
        }

        private void RaiseJobEvent()
        {
            DateTime now = DateTime.Now;
            DateTime next = CalculateNextStartTime(TypeOfPeriod, Weekday, Period);

            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("RunJobEvent", VirtualEventArgs);

            eventArgs.GetACValue("LastRunDate").Value = LastRunDate.ValueT;
            eventArgs.GetACValue("RunDate").Value = now;
            eventArgs.GetACValue("NextRunDate").Value = next;
            
            RunJobEvent.Raise(eventArgs);

            RunJob(now, LastRunDate.ValueT, NextRunDate.ValueT);

            LastRunDate.ValueT = now;
            NextRunDate.ValueT = next;
        }

        protected virtual void RunJob(DateTime now, DateTime lastRun, DateTime nextRun)
        {
        }

        public static DateTime CalculateNextStartTime(PAExportPeriodType periodType, DayOfWeek weekDay, TimeSpan period)
        {
            // Zeitperiode von mindestens einer Minute
            if (period.Hours <= 0 && period.Minutes <= 0)
                period = new TimeSpan(0, 0, 10);

            if (periodType == PAExportPeriodType.SeveralTimesADay)
            {
                DateTime startTime = DateTime.Today;
                DateTime maxTime = startTime.AddDays(2);
                DateTime now = DateTime.Now;
                while (startTime < maxTime)
                {
                    startTime += period;
                    if (startTime >= now)
                    {
                        break;
                    }
                }
                return startTime;
            }
            else if (periodType == PAExportPeriodType.Daily)
            {
                DateTime startTime = DateTime.Today;
                startTime += period;
                if (startTime < DateTime.Now)
                    startTime = startTime.AddDays(1);
                return startTime;
            }
            else // Weekly
            {
                DateTime startTime = DateTime.Today;
                DateTime now = DateTime.Now;
                if (now.DayOfWeek == weekDay)
                {
                    startTime += period;
                    if (startTime > now)
                        return startTime;
                    else
                        startTime = DateTime.Today;
                }

                do
                {
                    startTime = startTime.AddDays(1);
                }
                while (startTime.DayOfWeek != weekDay);
                
                startTime += period;
                return startTime;
            }
        }

        public static DateTime CalculatePrevStartTime(DateTime nextStartTime, PAExportPeriodType periodType, DayOfWeek weekDay, TimeSpan period)
        {
            // Zeitperiode von mindestens einer Minute
            if (period.Hours <= 0 && period.Minutes <= 0)
                period = new TimeSpan(0, 0, 10);

            if (periodType == PAExportPeriodType.SeveralTimesADay)
            {
                DateTime prevStartTime = nextStartTime - period;
                return prevStartTime;
            }
            else if (periodType == PAExportPeriodType.Daily)
            {
                DateTime prevStartTime = nextStartTime.AddDays(-1);
                return prevStartTime;
            }
            else // Weekly
            {
                DateTime prevStartTime = nextStartTime.AddDays(-7) - period;
                return prevStartTime;
            }
        }


#endregion

        #region Event-Handler
        #endregion
    }
}
