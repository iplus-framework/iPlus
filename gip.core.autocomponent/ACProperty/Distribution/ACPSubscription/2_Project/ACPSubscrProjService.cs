using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// ACPSubscrProjService verwaltet eine Liste von dem Client abbonierten ACObjects
    /// </summary>
    [DataContract]
    [ACSerializeableInfo]
    public class ACPSubscrProjService : ACPSubscrProjBase
    {
        #region c'tors
        public ACPSubscrProjService(string acName, ACPSubscrService parent)
            : base(acName)
        {
            _Parent = parent;
        }
        #endregion

        #region Members
        ACPSubscrService _Parent = null;
        public ACPSubscrService Parent
        {
            get
            {
                return _Parent;
            }
        }

        public WCFServiceChannel WCFServiceChannel
        {
            get
            {
                if (Parent == null)
                    return null;
                return Parent.WCFServiceChannel;
            }
        }
        #endregion

        #region Methods
        protected override ACPSubscrObjBase CreateNewSubscriptionObject(string ACUrl)
        {
            ACPSubscrObjService subscrObject = new ACPSubscrObjService(ACUrl, this);
            //if (subscrObject.Obj != null)
            //subscrObject.Obj.SubscribeProxy(subscrObject);
            return subscrObject;
        }

        protected override void OnRemoveSubscriptionObject(ACPSubscrObjBase subscrObj)
        {
            //ACPSubscrObjService subscrObject = (ACPSubscrObjService) subscrObj;
            //if (subscrObject.Obj != null)
            //subscrObject.Obj.UnsubscribeProxy(subscrObject);
        }

        /// <summary>Updates the Subscription</summary>
        /// <param name="update"></param>
        /// <param name="broadcastList"></param>
        public void Subscribe(ACPSubscrProjDispClient update, List<Tuple<IACPointNetBase, IACPointNetBase>> broadcastList)
        {
            if (update == null)
                return;

            try
            {
                foreach (ACPSubscrObjDispClient subscrOrUpdatedObject in update.SubscribedACObjects)
                {
                    ACPSubscrObjService subscrServiceObject = (ACPSubscrObjService)this.Subscribe(subscrOrUpdatedObject.GetACUrl());
                    subscrServiceObject.UpdateConnectionPoints(subscrOrUpdatedObject, broadcastList);
                    ACPSubscrService.SubscribeACObjectOverAllProxies(subscrServiceObject);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjService", "Subscribe", msg);
            }
        }

        /// <summary>
        /// Updates the Subscription
        /// </summary>
        /// <param name="update"></param>
        public void UnSubscribe(ACPSubscrProjDispClient update)
        {
            if (update == null)
                return;
            try
            {
                foreach (ACPSubscrObjDispClient subscribedObject in update.SubscribedACObjects)
                {
                    ACPSubscrObjService subscrServiceObject = (ACPSubscrObjService)GetACObject(subscribedObject.GetACUrl());
                    if (subscrServiceObject != null)
                    {
                        subscrServiceObject.UnSubscribe();
                        OnRemoveSubscriptionObject(subscrServiceObject);
                        this._SubscribedACObjectsDict.Remove(subscrServiceObject.ACUrl);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjService", "UnSubscribe", msg);
            }
        }

        internal void UnSubscribeAll()
        {
            try
            {
                foreach (ACPSubscrObjService subscrServiceObject in this.SubscribedACObjects.ToArray())
                {
                    subscrServiceObject.UnSubscribe();
                    OnRemoveSubscriptionObject(subscrServiceObject);
                    this._SubscribedACObjectsDict.Remove(subscrServiceObject.ACUrl);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjService", "UnSubscribeAll", msg);
            }
        }

        #endregion

        #region ValueEvents
        internal IEnumerable<IACPropertyNetValueEvent> GetPropertyValuesOfNewSubscribedObjects(IRoot root, WCFServiceChannel forChannel)
        {
            if (root == null)
                return null;
            List<IACPropertyNetValueEvent> valueEventList = new List<IACPropertyNetValueEvent>();
            try
            {
                IEnumerable<ACPSubscrObjBase> objectsToFetch = SubscribedACObjects.Where(c => c.FetchAllProperties == true);
                if (objectsToFetch != null && objectsToFetch.Any())
                {
                    foreach (ACPSubscrObjBase objectToFetch in objectsToFetch)
                    {
                        object resObj = root.ACUrlCommand(objectToFetch.GetACUrl());
                        if (resObj != null)
                        {
                            if (resObj is ACComponent)
                            {
                                IEnumerable<IACPropertyNetValueEvent> eventList = (resObj as ACComponent).GetPropertyValuesAsEvents(forChannel);
                                if (eventList != null && eventList.Any())
                                    valueEventList.AddRange(eventList);
                            }
                        }
                        objectToFetch.FetchAllProperties = false;
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrProjService", "GetPropertyValuesOfNewSubscribedObjects", msg);
            }
            return valueEventList;
        }
        #endregion
    }
}
