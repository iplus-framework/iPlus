using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication
{
    public class S7TCPItems2SendQueue
    {
        #region c'tors
        public S7TCPItems2SendQueue()
        {
        }
        #endregion

        #region Members
        protected readonly ACMonitorObject _30140_LockSendList = new ACMonitorObject(30140);
        protected List<S7TCPItems2SendEntry> _SendList = new List<S7TCPItems2SendEntry>();
        protected List<S7TCPItems2SendEntry> SendList
        {
            get
            {
                return _SendList;
            }
        }

        int _MoreItemsThanLastTimeCounter = 0;
        int _LastEmptyingCount = 0;
        #endregion

        #region Methods

        public bool Enqueue(S7TCPItem item, bool writeChronologial)
        {
            if (item == null)
                return false;

            bool succeeded = false;
            if (TryEnterLockSendList())
            {
                try
                {
                    S7TCPItems2SendEntry sendEntry = null;
                    if (!writeChronologial)
                        sendEntry = GetEntry(item, false);
                    if (sendEntry == null)
                    {
                        sendEntry = new S7TCPItems2SendEntry(item, _SendList.Count);
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
                _SendList = new List<S7TCPItems2SendEntry>();
            }
            finally
            {
                ExitLockSendList();
            }
        }

        public List<S7TCPItems2SendEntry> GetAllEntrys(bool AutoRemove, out bool overCrowdingThreat)
        {
            overCrowdingThreat = false;
            List<S7TCPItems2SendEntry> eventList = new List<S7TCPItems2SendEntry>();
            if (TryEnterLockSendList())
            {
                try
                {
                    eventList = _SendList.ToList();
                    if (AutoRemove)
                    {
                        int newItemCount = eventList.Count;
                        if (newItemCount > _LastEmptyingCount)
                        {
                            _MoreItemsThanLastTimeCounter++;
                            if (_MoreItemsThanLastTimeCounter > 5)
                                overCrowdingThreat = true;
                        }
                        else
                            _MoreItemsThanLastTimeCounter = 0;
                        _LastEmptyingCount = newItemCount;

                        _SendList = new List<S7TCPItems2SendEntry>();
                    }
                }
                finally
                {
                    ExitLockSendList();
                }
            }
            return eventList;
        }


        internal S7TCPItems2SendEntry GetEntry(S7TCPItem item, bool withCS)
        {
            S7TCPItems2SendEntry pollingPlanEntry = null;
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
                    datamodel.Database.Root.Messages.LogException("S7TCPItems2SendQueue", "GetEntry", msg);
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
