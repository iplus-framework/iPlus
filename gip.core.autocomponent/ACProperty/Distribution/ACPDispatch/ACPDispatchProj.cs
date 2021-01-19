using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{

    public class ACPDispatchProj
    {
        #region c'tors
        public ACPDispatchProj(string acName)
        {
            _ACIdentifier = acName;
        }
        #endregion

        #region Members
        private string _ACIdentifier;
        /// <summary>
        /// ProjectID / ModelID
        /// String anstatt Referenz auf Objekt, damit Object entladen und geladen werden können!
        /// </summary>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
        }

        protected SafeList<IACPropertyNetValueEvent> _ValueEventDispatchList = new SafeList<IACPropertyNetValueEvent>();
        public SafeList<IACPropertyNetValueEvent> ValueEventDispatchList
        {
            get
            {
                return _ValueEventDispatchList;
            }
        }
        #endregion

        #region Methods
        //private string _LastEnqeuedUrl = "";

        public bool Enqueue(IACPropertyNetValueEvent eventArgs)
        {
            _ValueEventDispatchList.Add(eventArgs);
            return true;
        }

        internal bool RemoveAllSent(bool withAging = false)
        {
            try
            {
                if (withAging)
                {
                    // Falls nach 3 maligem versuchen zu senden nicht verteilt worden ist, dann als gesendet kennzeichnen
                    foreach (var valueEvent in ValueEventDispatchList)
                    {
                        if (valueEvent.EventBroadcasted == ACPropertyBroadcastState.ProcessedInLoop)
                        {
                            if (valueEvent.SubscriptionSendCount > 3)
                                valueEvent.EventBroadcasted = ACPropertyBroadcastState.Sent;
                            else
                                valueEvent.SubscriptionSendCount++;
                        }
                    }
                }
                ValueEventDispatchList.RemoveAll(x => x.EventBroadcasted == ACPropertyBroadcastState.Sent);
                //_LastEnqeuedUrl = "";
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPDispatchProj", "RemoveAllSent", msg);
                return false;
            }
            return true;
        }

        internal bool RemoveAllTriedToSend()
        {
            try
            {
                ValueEventDispatchList.RemoveAll(x => x.SubscriptionSendCount > 0);
                //ValueEventDispatchList.Clear();
                //_LastEnqeuedUrl = "";
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPDispatchProj", "RemoveAllTriedToSend", msg);
                return false;
            }
            return true;
        }

        internal bool RemoveAllEvents()
        {
            try
            {
                //ValueEventDispatchList.RemoveAll(x => x.SubscriptionSendCount > 0);
                ValueEventDispatchList.Clear();
                //_LastEnqeuedUrl = "";
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPDispatchProj", "RemoveAllEvents", msg);
                return false;
            }
            return true;
        }
        #endregion

    }

}
