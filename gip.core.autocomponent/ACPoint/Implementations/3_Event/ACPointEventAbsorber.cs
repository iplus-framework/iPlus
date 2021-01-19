using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Event-absorber'}de{'Event-Aufsauger'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACPointEventAbsorber : ACComponent
    {
        private class DelayedCallback
        {
            private static int _LastInsertNo = 0;
            public DelayedCallback(TimeSpan timeStart, IACPointNetBase sender, ACEventArgs e, ACPointEventWrap<ACComponent> wrapObject)
            {
                _TimeStart = timeStart;
                if (_TimeStart < TimeSpan.Zero)
                    _TimeStart = TimeSpan.Zero;
                _Sender = sender;
                _EventArgs = e;
                _WrapObject = wrapObject;
                _LastInsertNo++;
                InsertNo = _LastInsertNo;

                ACValue durationValue = _EventArgs.GetACValue("TimeInfo");
                if (durationValue != null && durationValue.Value != null && durationValue.Value is PATimeInfo)
                {
                    PATimeInfo timeInfo = durationValue.Value as PATimeInfo;
                    _Duration = timeInfo.PlannedTimes.Duration;
                    if (_Duration <= TimeSpan.Zero)
                        _Duration = new TimeSpan(0, 0, 0, 0, 1);
                }
            }

            private TimeSpan _TimeStart = TimeSpan.Zero;
            public TimeSpan TimeStart
            {
                get
                {
                    return _TimeStart;
                }
            }

            private TimeSpan _Duration = new TimeSpan(0, 0, 0, 0, 1);
            public TimeSpan Duration
            {
                get
                {
                    return _Duration;
                }
            }
            
            public TimeSpan TimeEnd
            {
                get
                {
                    return _TimeStart + _Duration;
                }
            }

            private IACPointNetBase _Sender;
            public IACPointNetBase Sender
            {
                get
                {
                    return _Sender;
                }
            }

            private ACEventArgs _EventArgs;
            public ACEventArgs EventArgs
            {
                get
                {
                    return _EventArgs;
                }
            }

            private ACPointEventWrap<ACComponent> _WrapObject;
            public ACPointEventWrap<ACComponent> WrapObject
            {
                get
                {
                    return _WrapObject;
                }
            }

            public int InsertNo
            {
                get;
                private set;
            }
        }

        private class DelayedCallbackComparer : IComparer<DelayedCallback>
        {
            public int Compare(DelayedCallback x, DelayedCallback y)
            {
                int comp = x.TimeEnd.CompareTo(y.TimeEnd);
                if (comp != 0)
                    return comp;
                else
                {
                    return x.InsertNo.CompareTo(y.InsertNo);
                }
            }
        }

        #region c'tors
        public ACPointEventAbsorber(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        private TimeSpan _LastEndTime = TimeSpan.Zero;
        private TimeSpan _NextEndTime = TimeSpan.Zero;
        private List<DelayedCallback> _DelayedCallbacks = new List<DelayedCallback>();
        private readonly ACMonitorObject _20058_LockDelayedCallbacks = new ACMonitorObject(20058);
        private Dictionary<IACPointEvent<ACComponent>, bool> _RaisingSubscribed = new Dictionary<IACPointEvent<ACComponent>, bool>();

        private DateTime _PointOfSimulationTime = DateTime.Now;
        public DateTime CurrentSimulationTime
        {
            get
            {
                return _PointOfSimulationTime + _LastEndTime;
            }
        }
        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Callback":
                    Callback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        #region public

        public void RedirectEventSubscr(ACPointEventSubscrWrap<ACComponent> eventSubscrEntry)
        {
            if (eventSubscrEntry.Event != null)
            {
                var query = eventSubscrEntry.Event.ConnectionList.Where(c => c.CompareTo(eventSubscrEntry) == 0);
                if (query.Any())
                {
                    ACPointEventWrap<ACComponent> eventEntry  = query.First();
                    if (eventEntry != null)
                    {
                        if (!_RaisingSubscribed.ContainsKey(eventSubscrEntry.Event))
                        {
                            eventSubscrEntry.Event.RaisingCompleted += Event_RaisingCompleted;
                            eventSubscrEntry.Event.AllEntriesRemoved += Event_AllEntriesRemoved;
                            _RaisingSubscribed.Add(eventSubscrEntry.Event, true);
                        }
                        eventEntry._RedirectCallbackDelegate = Callback;
                    }
                }
            }
        }

        #endregion

        #region Callbacks
        [ACMethodInfo("Function", "en{'PWPointInCallback'}de{'PWPointInCallback'}", 9999)]
        public virtual void Callback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            ACPointEventWrap<ACComponent> eventWrap = wrapObject as ACPointEventWrap<ACComponent>;
            if (e != null && eventWrap != null)
            {

                using (ACMonitor.Lock(_20058_LockDelayedCallbacks))
                {
                    DelayedCallback delayedCallback = new DelayedCallback(_LastEndTime, sender, e, eventWrap);
                    _DelayedCallbacks.Add(delayedCallback);
                    _DelayedCallbacks.Sort(new DelayedCallbackComparer());
                    _NextEndTime = delayedCallback.TimeEnd;
                }
            }
        }

        void Event_RaisingCompleted(object sender, EventArgs e)
        {
            while (_DelayedCallbacks.Count > 0)
            {
                DelayedCallback delayedCallback = null;

                using (ACMonitor.Lock(_20058_LockDelayedCallbacks))
                {
                    delayedCallback = _DelayedCallbacks.First();
                    _DelayedCallbacks.RemoveAt(0);
                    _LastEndTime = delayedCallback.TimeEnd;
                }

                ACPointEventWrap<ACComponent> eventWrap = delayedCallback.WrapObject as ACPointEventWrap<ACComponent>;
                if (eventWrap.OriginalAsyncCallbackDelegate != null)
                {
                    eventWrap.OriginalAsyncCallbackDelegate(delayedCallback.Sender, delayedCallback.EventArgs, eventWrap);
                }
                else if (!String.IsNullOrEmpty(eventWrap.AsyncCallbackDelegateName))
                {
                    ACComponent refACObject = null;
                    refACObject = (ACComponent)(IACComponent)eventWrap.ValueT;
                    if (refACObject != null)
                        refACObject.ExecuteMethod(eventWrap.AsyncCallbackDelegateName, new object[] { delayedCallback.Sender, delayedCallback.EventArgs, eventWrap });
                }
            }
        }

        void Event_AllEntriesRemoved(object sender, EventArgs e)
        {
            IACPointEvent<ACComponent> pointEvent = sender as IACPointEvent<ACComponent>;
            if (pointEvent != null)
            {
                pointEvent.RaisingCompleted -= Event_RaisingCompleted;
                pointEvent.AllEntriesRemoved -= Event_AllEntriesRemoved;
                try
                {
                    _RaisingSubscribed.Remove(pointEvent);
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    Messages.LogException("ACPointEventAbsorber", "Event_AllEntriesRemoved", msg);
                }
            }
        }


#endregion

#endregion

    }
}
