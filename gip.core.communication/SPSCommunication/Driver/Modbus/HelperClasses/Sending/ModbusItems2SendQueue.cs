using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.modbus;

namespace gip.core.communication
{
    public class ModbusItems2SendQueue
    {
        #region c'tors
        public ModbusItems2SendQueue()
        {
        }
        #endregion

        #region Members
        protected readonly ACMonitorObject _30140_LockSendList = new ACMonitorObject(30140);
        protected List<ModbusItems2SendEntry> _SendList = new List<ModbusItems2SendEntry>();
        protected List<ModbusItems2SendEntry> SendList
        {
            get
            {
                return _SendList;
            }
        }
        #endregion

        #region Methods

        public bool Enqueue(ModbusItem item, bool writeChronologial)
        {
            if (item == null)
                return false;

            bool succeeded = false;
            if (TryEnterLockSendList())
            {
                try
                {
                    ModbusItems2SendEntry sendEntry = null;
                    if (!writeChronologial)
                        sendEntry = GetEntry(item, false);
                    if (sendEntry == null)
                    {
                        sendEntry = new ModbusItems2SendEntry(item, _SendList.Count);
                        _SendList.Add(sendEntry);
                        succeeded = true;
                    }
                }
                finally
                {
                    ExitLockSendList();
                }
            }
            return succeeded;
        }

        public void RemoveAllEntrys()
        {
            EnterLockSendList();
            try
            {
                _SendList = new List<ModbusItems2SendEntry>();
            }
            finally
            {
                ExitLockSendList();
            }
        }

        public List<ModbusItems2SendEntry> GetAllEntrys(bool AutoRemove)
        {
            List<ModbusItems2SendEntry> eventList = new List<ModbusItems2SendEntry>();
            if (TryEnterLockSendList())
            {
                try
                {
                    eventList = _SendList.ToList();
                    if (AutoRemove)
                        _SendList = new List<ModbusItems2SendEntry>();
                }
                finally
                {
                    ExitLockSendList();
                }
            }
            return eventList;
        }


        internal ModbusItems2SendEntry GetEntry(ModbusItem item, bool withCS)
        {
            ModbusItems2SendEntry pollingPlanEntry = null;
            if (withCS)
                EnterLockSendList();
            try
            {
                pollingPlanEntry = _SendList.Where(c => c.Item == item).FirstOrDefault();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ModbusItem2SendQueue", "GetEntry", msg);
            }
            finally
            {
                if (withCS)
                    ExitLockSendList();
            }
            return pollingPlanEntry;
        }

        /// <summary>
        /// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        /// </summary>
        protected bool TryEnterLockSendList()
        {
            int tries = 0;
            while (tries < 100)
            {
                if (ACMonitor.TryEnter(_30140_LockSendList, 10))
                    break;
                tries++;
            }
            return tries < 100;
        }

        protected void EnterLockSendList()
        {
            ACMonitor.Enter(_30140_LockSendList);
        }

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        protected void ExitLockSendList()
        {
            ACMonitor.Exit(_30140_LockSendList);
        }

        #endregion
    }
}
