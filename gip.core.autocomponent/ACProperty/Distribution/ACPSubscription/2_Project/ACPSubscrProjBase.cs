using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// ACPSubscriptionACProject verwaltet eine Liste von dem Client abbonierten ACObjects
    /// </summary>
    [DataContract]
    public abstract class ACPSubscrProjBase
    {
        #region c'tors
        public ACPSubscrProjBase(string acName)
        {
            _ACIdentifier = acName;
        }
        #endregion

        #region Members
        [DataMember(Name = "ACId")]
        private string _ACIdentifier;
        /// <summary>
        /// ProjectID / ModelID
        /// String anstatt Referenz auf Objekt, damit Object entladen und geladen werden können!
        /// </summary>
        [IgnoreDataMember]
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
        }

        [DataMember(Name = "OSL")]
        protected Dictionary<string, ACPSubscrObjBase> _SubscribedACObjectsDict = new Dictionary<string, ACPSubscrObjBase>();

        [IgnoreDataMember]
        public IEnumerable<ACPSubscrObjBase> SubscribedACObjects
        {
            get
            {
                return _SubscribedACObjectsDict.Values;
            }
        }

        public Dictionary<string, ACPSubscrObjBase> SubscribedACObjectsDict
        {
            get
            {
                return _SubscribedACObjectsDict;
            }
        }

        #endregion

        #region Methods
        protected abstract ACPSubscrObjBase CreateNewSubscriptionObject(string ACUrl);
        protected abstract void OnRemoveSubscriptionObject(ACPSubscrObjBase subscrObj);

        public ACPSubscrObjBase Subscribe(string ACObjectIdentifier)
        {
            ACPSubscrObjBase subscrObject = this.GetACObject(ACObjectIdentifier);
            if (subscrObject == null)
            {
                subscrObject = CreateNewSubscriptionObject(ACObjectIdentifier);
                SubscribedACObjectsDict.Add(subscrObject.ACUrl, subscrObject);
            }
            return subscrObject;
        }


        public ACPSubscrObjBase GetACObject(string acUrl)
        {
            ACPSubscrObjBase subscrObject = null;
            try
            {
                if (_SubscribedACObjectsDict.TryGetValue(acUrl, out subscrObject))
                    return subscrObject;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjBase", "GetACObject", msg);
            }
            return null;
            //return subscrObject;
        }


        public int UnSubscribe(string ACObjectIdentifier)
        {
            ACPSubscrObjBase subscrObj = GetACObject(ACObjectIdentifier);
            if (subscrObj != null)
            {
                OnRemoveSubscriptionObject(subscrObj);
                try
                {
                    return Convert.ToInt32(_SubscribedACObjectsDict.Remove(ACObjectIdentifier));
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPSubscrProjBase", "UnSubscribe", msg);
                }
            }
            return 0;
        }

        public void Detach()
        {
            try
            {
                foreach (ACPSubscrObjBase obj in SubscribedACObjects)
                {
                    obj.Detach();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjBase", "Detach", msg);
            }
        }

        #endregion
    }
}
