using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.communication
{
    public class ModbusPollingPlan
    {
        #region c'tors
        public ModbusPollingPlan()
        {
        }
        #endregion

        #region Members
        protected readonly ACMonitorObject _30180_LockPollingPlanList = new ACMonitorObject(30180);
        protected List<ModbusPollingPlanEntry> _PollingPlanList = new List<ModbusPollingPlanEntry>();
        protected List<ModbusPollingPlanEntry> PollingPlanList
        {
            get
            {
                return _PollingPlanList;
            }
        }
        #endregion

        #region Methods

        public bool Enqueue(ModbusSubscr subscr, int dbNo = -10)
        {
            if (subscr == null)
                return false;

            bool succeeded = false;
            if (TryEnterLockPollingPlanList())
            {
                try
                {
                    ModbusPollingPlanEntry pollingPlanEntry = GetEntry(subscr, dbNo, false);
                    if (pollingPlanEntry == null)
                    {
                        pollingPlanEntry = new ModbusPollingPlanEntry(subscr, dbNo);
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

            using (ACMonitor.Lock(_30180_LockPollingPlanList))
            {
                _PollingPlanList = new List<ModbusPollingPlanEntry>();
            }
        }

        public List<ModbusPollingPlanEntry> GetAllEntrys(bool AutoRemove)
        {
            List<ModbusPollingPlanEntry> eventList = new List<ModbusPollingPlanEntry>();
            if (TryEnterLockPollingPlanList())
            {
                try
                {
                    eventList = _PollingPlanList.ToList();
                    if (AutoRemove)
                        _PollingPlanList = new List<ModbusPollingPlanEntry>();
                }
                finally
                {
                    ExitLockPollingPlanList();
                }
            }
            return eventList;
        }


        internal ModbusPollingPlanEntry GetEntry(ModbusSubscr subscr, int dbNo, bool withCS)
        {
            ModbusPollingPlanEntry pollingPlanEntry = null;
            if (withCS)
                EnterLockPollingPlanList();
            try
            {
                IEnumerable<ModbusPollingPlanEntry> query = null;
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
                    datamodel.Database.Root.Messages.LogException("ModbusPollingPlan", "GetEntry", msg);
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
