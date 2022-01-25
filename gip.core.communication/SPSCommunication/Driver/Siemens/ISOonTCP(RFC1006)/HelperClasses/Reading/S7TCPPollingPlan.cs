using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.communication
{
    public class S7TCPPollingPlan
    {
        #region c'tors
        public S7TCPPollingPlan()
        {
        }
        #endregion

        #region Members
        protected readonly ACMonitorObject _30180_LockPollingPlanList = new ACMonitorObject(30180);
        protected List<S7TCPPollingPlanEntry> _PollingPlanList = new List<S7TCPPollingPlanEntry>();
        protected List<S7TCPPollingPlanEntry> PollingPlanList
        {
            get
            {
                return _PollingPlanList;
            }
        }
        #endregion

        #region Methods

        public bool Enqueue(S7TCPSubscr subscr, int dbNo = -10)
        {
            if (subscr == null)
                return false;

            bool succeeded = false;
            if (TryEnterLockPollingPlanList())
            {
                try
                {
                    S7TCPPollingPlanEntry pollingPlanEntry = GetEntry(subscr, dbNo, false);
                    if (pollingPlanEntry == null)
                    {
                        pollingPlanEntry = new S7TCPPollingPlanEntry(subscr, dbNo);
                        _PollingPlanList.Add(pollingPlanEntry);
                        succeeded = true;
                    }
                }
                finally
                {
                    ExitLockPollingPlanList();
                }
            }
            return succeeded;
        }

        public void RemoveAllEntrys()
        {
            EnterLockPollingPlanList();
            try
            {
                _PollingPlanList = new List<S7TCPPollingPlanEntry>();
            }
            finally
            {
                ExitLockPollingPlanList();
            }
        }

        public List<S7TCPPollingPlanEntry> GetAllEntrys(bool AutoRemove)
        {
            List<S7TCPPollingPlanEntry> eventList = new List<S7TCPPollingPlanEntry>();
            if (TryEnterLockPollingPlanList())
            {
                try
                {
                    eventList = _PollingPlanList.ToList();
                    if (AutoRemove)
                        _PollingPlanList = new List<S7TCPPollingPlanEntry>();
                }
                finally
                {
                    ExitLockPollingPlanList();
                }
            }
            return eventList;
        }


        internal S7TCPPollingPlanEntry GetEntry(S7TCPSubscr subscr, int dbNo, bool withCS)
        {
            S7TCPPollingPlanEntry pollingPlanEntry = null;
            if (withCS)
                EnterLockPollingPlanList();
            try
            {
                IEnumerable<S7TCPPollingPlanEntry> query = null;
                if (dbNo <= -1)
                    query = _PollingPlanList.Where(c => c.Subscr == subscr);
                else
                    query = _PollingPlanList.Where(c => c.Subscr == subscr && c.DBNo == dbNo);
                pollingPlanEntry = query.FirstOrDefault();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("S7TCPPollingPlan", "GetEntry", msg);
            }
            finally
            {
                if (withCS)
                    ExitLockPollingPlanList();
            }
            return pollingPlanEntry;
        }

        /// <summary>
        /// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        /// </summary>
        protected bool TryEnterLockPollingPlanList()
        {
            int tries = 0;
            while (tries < 100)
            {
                if (ACMonitor.TryEnter(_30180_LockPollingPlanList, 10))
                    break;
                tries++;
            }
            return tries < 100;
        }

        protected void EnterLockPollingPlanList()
        {
            ACMonitor.Enter(_30180_LockPollingPlanList);
        }

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        protected void ExitLockPollingPlanList()
        {
            ACMonitor.Exit(_30180_LockPollingPlanList);
        }

        #endregion

    }
}
