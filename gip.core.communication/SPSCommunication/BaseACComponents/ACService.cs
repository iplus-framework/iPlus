using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Threading;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACService'}de{'ACService'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class ACService : ACComponent
    {
        #region c´tors
        public ACService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ShutdownEvent = new ManualResetEvent(false);
            _WorkCycleThread = new ACThread(RunWorkCycle);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _IsReadyForWriting = false;
            if (!base.ACInit(startChildMode))
                return false;
            _WorkCycleThread.Name = "ACUrl:" + this.GetACUrl() + ";RunWorkCycle();";
            _WorkCycleThread.Start();
            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is ACSession)
                {
                    ACSession acSession = child as ACSession;
                    acSession.PropertyChanged += OnSession_PropertyChanged;
                }
            }
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is ACSession)
                {
                    ACSession acSession = child as ACSession;
                    acSession.PropertyChanged -= OnSession_PropertyChanged;
                }
            }
            bool result = base.ACDeInit(deleteACClassTask);

            if (_WorkCycleThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_WorkCycleThread.Join(10000))
                    _WorkCycleThread.Abort();

                _WorkCycleThread = null;
                _ShutdownEvent = null;
            }

            return result;
        }
        #endregion

        #region Events
        public event EventHandler ProjectWorkCycleR100ms;
        public event EventHandler ProjectWorkCycleR200ms;
        public event EventHandler ProjectWorkCycleR500ms;
        public event EventHandler ProjectWorkCycleR1sec;
        public event EventHandler ProjectWorkCycleR2sec;
        public event EventHandler ProjectWorkCycleR5sec;
        public event EventHandler ProjectWorkCycleR10sec;
        public event EventHandler ProjectWorkCycleR20sec;
        public event EventHandler ProjectWorkCycleR1min;
        public event EventHandler ProjectWorkCycleR2min;
        public event EventHandler ProjectWorkCycleR5min;
        public event EventHandler ProjectWorkCycleR10min;
        public event EventHandler ProjectWorkCycleR20min;
        public event EventHandler ProjectWorkCycleHourly;
        #endregion

        #region Properties
        protected ManualResetEvent _ShutdownEvent;
        private ACThread _WorkCycleThread;

        private bool _IsReadyForWriting = false;
        [ACPropertyBindingSource()]
        public bool IsReadyForWriting
        {
            get
            {
                return _IsReadyForWriting;
            }
            set
            {
                _IsReadyForWriting = value;
                OnPropertyChanged("IsReadyForWriting");
            }
        }
        #endregion

        #region Event-Handler
        protected void OnSession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsReadyForWriting")
            {
                bool bAllSubscrReady = true;
                foreach (IACComponent child in this.ACComponentChilds)
                {
                    if (child is ACSession)
                    {
                        ACSession acSession = child as ACSession;
                        if (!acSession.IsReadyForWriting)
                        {
                            bAllSubscrReady = false;
                            break;
                        }
                    }
                }
                if (IsReadyForWriting != bAllSubscrReady)
                    IsReadyForWriting = bAllSubscrReady;
            }
        }
        #endregion

        #region Cycle
        private void RunWorkCycle()
        {
            try
            {
                while (!_ShutdownEvent.WaitOne(100, false))
                {
                    _WorkCycleThread.StartReportingExeTime();
                    ProjectThreadWakedUpAfter100ms();
                    _WorkCycleThread.StopReportingExeTime();
                }
            }
            catch (ThreadAbortException e)
            {
                Messages.LogException(this.GetACUrl(), "RunWorkCycle()", "Thread abort exception. Thread TERMINATED!!!!");
                if (!String.IsNullOrEmpty(e.Message))
                {
                    Messages.LogException(this.GetACUrl(), "RunWorkCycle()", e.Message);
                    if (e.InnerException != null && !String.IsNullOrEmpty(e.InnerException.Message))
                    {
                        Messages.LogException(this.GetACUrl(), "RunWorkCycle()", e.InnerException.Message);
                    }
                }
            }
        }

        private DateTime? _LastWakeupTime = null;
        private TimeSpan _WakeupAlarm = new TimeSpan(0, 0, 2);
        private int _WakeupCounter = 0;
        internal void ProjectThreadWakedUpAfter100ms()
        {
            _WakeupCounter++;
            DateTime stampStart = DateTime.Now;
            if (_LastWakeupTime.HasValue && (DateTime.Now > (_LastWakeupTime.Value + _WakeupAlarm)))
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Wakeuptime took longer than 2 seconds since {0}", _LastWakeupTime.Value));
            if (ProjectWorkCycleR100ms != null)
                ProjectWorkCycleR100ms(this, new EventArgs());
            if (_WakeupCounter % 2 == 0)
            {
                if (ProjectWorkCycleR200ms != null)
                    ProjectWorkCycleR200ms(this, new EventArgs());
            }
            if (_WakeupCounter % 5 == 0)
            {
                if (ProjectWorkCycleR500ms != null)
                    ProjectWorkCycleR500ms(this, new EventArgs());
            }
            if (_WakeupCounter % 10 == 0)
            {
                if (ProjectWorkCycleR1sec != null)
                    ProjectWorkCycleR1sec(this, new EventArgs());
            }
            if (_WakeupCounter % 20 == 0)
            {
                if (ProjectWorkCycleR2sec != null)
                    ProjectWorkCycleR2sec(this, new EventArgs());
            }
            if (_WakeupCounter % 50 == 0)
            {
                if (ProjectWorkCycleR5sec != null)
                    ProjectWorkCycleR5sec(this, new EventArgs());
            }
            if (_WakeupCounter % 100 == 0)
            {
                if (ProjectWorkCycleR10sec != null)
                    ProjectWorkCycleR10sec(this, new EventArgs());
            }
            if (_WakeupCounter % 200 == 0)
            {
                if (ProjectWorkCycleR20sec != null)
                    ProjectWorkCycleR20sec(this, new EventArgs());
            }
            if (_WakeupCounter % 600 == 0)
            {
                if (ProjectWorkCycleR1min != null)
                    ProjectWorkCycleR1min(this, new EventArgs());
            }
            if (_WakeupCounter % 1200 == 0)
            {
                if (ProjectWorkCycleR2min != null)
                    ProjectWorkCycleR2min(this, new EventArgs());
            }
            if (_WakeupCounter % 3000 == 0)
            {
                if (ProjectWorkCycleR5min != null)
                    ProjectWorkCycleR5min(this, new EventArgs());
            }
            if (_WakeupCounter % 6000 == 0)
            {
                if (ProjectWorkCycleR10min != null)
                    ProjectWorkCycleR10min(this, new EventArgs());
            }
            if (_WakeupCounter % 12000 == 0)
            {
                if (ProjectWorkCycleR20min != null)
                    ProjectWorkCycleR20min(this, new EventArgs());
            }
            if (_WakeupCounter % 36000 == 0)
            {
                if (ProjectWorkCycleHourly != null)
                    ProjectWorkCycleHourly(this, new EventArgs());
                _WakeupCounter = 0;
            }
            DateTime stampFinished = DateTime.Now;
            TimeSpan diff = stampFinished - stampStart;
            if (diff.TotalSeconds >= 5)
                Messages.LogWarning(this.GetACUrl(), "ProjectThreadWakedUpAfter100ms()", String.Format("Child-Components consumed more than {0}", diff));
            _LastWakeupTime = stampFinished;
        }
        #endregion


    }
}
