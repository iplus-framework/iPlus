using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Für jeden Abbonenten im Netzwerk, wird auf Server-Seite eine oder mehrere Instanzen von einem Observer angelegt
    /// Mit der Subscription wird registriert, an welchen Objekten der Client interessiert ist
    /// In bestimmten Zyklen, muss die ACPropertyDispatchProject-Liste geleert werden, 
    /// indem alle Subscriptions mit dem zu versendenten Inhalt informiert werden
    /// </summary>
    //[DataContract(Name = "ACPS")]
    [DataContract]
    //[KnownType(typeof(ACPSubscriptionACProject))]
    [ACSerializeableInfo]
    public class ACPSubscrService : ACPSubscrBase
    {
        #region c'tors
        public ACPSubscrService(WCFServiceChannel wcfServiceChannel)
            : base()
        {
            _WCFServiceChannel = wcfServiceChannel;
        }
        #endregion

        #region Members
        WCFServiceChannel _WCFServiceChannel = null;
        public WCFServiceChannel WCFServiceChannel
        {
            get
            {
                return _WCFServiceChannel;
            }
        }

        /*protected override ACPSubscrProjBase CreateNewSubscriptionProject(string ACIdentifier)
        {
            return new ACPSubscrProjService(ACIdentifier);
        }*/


        [IgnoreDataMember]
        //private static List<ACPSubscriptionSharedACObject> _SubscribedACObjectsOverAllProxies = new List<ACPSubscriptionSharedACObject>();
        private static Dictionary<string, ACPSubscrObjServiceShared> _SubscribedACObjectsOverAllProxies = new Dictionary<string, ACPSubscrObjServiceShared>();
        private static readonly ACMonitorObject _20053_LockSubscrAllProxies = new ACMonitorObject(20053);

        [IgnoreDataMember]
        public static ICollection<ACPSubscrObjServiceShared> SubscribedACObjectsOverAllProxies
        {
            get
            {
                return (ICollection<ACPSubscrObjServiceShared>)_SubscribedACObjectsOverAllProxies.Values;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the Subscription
        /// </summary>
        /// <param name="update"></param>
        public void Subscribe(ACPSubscrDispClient update)
        {
            if (update == null)
                return;
            List<Tuple<IACPointNetBase, IACPointNetBase>> broadcastList = new List<Tuple<IACPointNetBase, IACPointNetBase>>();

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                foreach (ACPSubscrProjDispClient project in update.ProjectSubscriptionList)
                {
                    ACPSubscrProjService thisProject = (ACPSubscrProjService)GetProject(project.ACIdentifier, false);
                    if (thisProject == null)
                    {
                        thisProject = new ACPSubscrProjService(project.ACIdentifier, this);
                        _ProjectSubscriptionList.Add(thisProject);
                    }
                    // Update Projectlist
                    thisProject.Subscribe(project, broadcastList);
                }
            }
            broadcastList.ForEach(c => c.Item1.OnPointReceivedRemote(c.Item2));
        }

        /// <summary>
        /// Updates the Subscription
        /// </summary>
        /// <param name="update"></param>
        public void UnSubscribe(ACPSubscrDispClient update)
        {
            if (update == null)
                return;

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                foreach (ACPSubscrProjDispClient project in update.ProjectSubscriptionList)
                {
                    ACPSubscrProjService thisProject = (ACPSubscrProjService)GetProject(project.ACIdentifier, false);
                    if (thisProject != null)
                    {
                        // Update Projectlist
                        thisProject.UnSubscribe(project);
                    }
                }
            }
        }

        internal void UnSubscribeAll()
        {

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                foreach (ACPSubscrProjService thisProject in ProjectSubscriptionList)
                {
                    thisProject.UnSubscribeAll();
                }
            }
        }

        internal static void EmtpySubscrACObjectOverAllProxies()
        {
            try
            {

                using (ACMonitor.Lock(_20053_LockSubscrAllProxies))
                {
                    _SubscribedACObjectsOverAllProxies = new Dictionary<string, ACPSubscrObjServiceShared>();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrService", "EmptySubscrACObjectOverAllProxies", msg);
            }
        }

        internal static void SubscribeACObjectOverAllProxies(ACPSubscrObjService acObjectSubscription)
        {
            if (acObjectSubscription == null)
                return;
            try
            {

                using (ACMonitor.Lock(_20053_LockSubscrAllProxies))
                {
                    ACPSubscrObjServiceShared sharedObject = GetSharedACObject(acObjectSubscription.GetACUrl());
                    if (sharedObject == null)
                    {
                        sharedObject = new ACPSubscrObjServiceShared(acObjectSubscription.GetACUrl());
                        _SubscribedACObjectsOverAllProxies.Add(acObjectSubscription.GetACUrl(), sharedObject);
                    }
                    sharedObject.RefCounter++;
                    acObjectSubscription.SharedSubscriptionObject = sharedObject;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrService", "SubscribeACObjectOverAllProxies", msg);
            }
        }

        internal static void UnSubscribeACObjectOverAllProxies(ACPSubscrObjService acObjectSubscription)
        {
            if (acObjectSubscription == null)
                return;
            ACPSubscrObjServiceShared sharedObject = acObjectSubscription.SharedSubscriptionObject;
            acObjectSubscription.SharedSubscriptionObject = null;
            try
            {

                using (ACMonitor.Lock(_20053_LockSubscrAllProxies))
                {
                    if (sharedObject == null)
                        sharedObject = GetSharedACObject(acObjectSubscription.GetACUrl());
                    if (sharedObject != null)
                    {
                        sharedObject.RefCounter--;
                        if (sharedObject.RefCounter <= 0)
                            _SubscribedACObjectsOverAllProxies.Remove(acObjectSubscription.GetACUrl());
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPSubscrService", "UnSubscribeACObjectOverAllProxies", msg);
            }
        }

        internal static ACPSubscrObjServiceShared GetSharedACObject(string ACObjectIdentifier)
        {
            ACPSubscrObjServiceShared sharedObj = null;

            using (ACMonitor.Lock(_20053_LockSubscrAllProxies))
            {
                try
                {
                    if (_SubscribedACObjectsOverAllProxies.TryGetValue(ACObjectIdentifier, out sharedObj))
                        return sharedObj;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPSubscrService", "GetSharedACObject", msg);
                }
                return null;
            }
        }

#endregion

#region ValueEvents
        internal List<IACPropertyNetValueEvent> GetPropertyValuesOfNewSubscribedObjects(IRoot root, WCFServiceChannel forChannel)
        {
            if (root == null)
                return null;
            List<IACPropertyNetValueEvent> valueEventList = new List<IACPropertyNetValueEvent>();

            using (ACMonitor.Lock(_20054_LockProjectSubscriptionList))
            {
                foreach (ACPSubscrProjService project in ProjectSubscriptionList)
                {
                    IEnumerable<IACPropertyNetValueEvent> eventList = project.GetPropertyValuesOfNewSubscribedObjects(root, forChannel);
                    if (eventList != null && eventList.Any())
                        valueEventList.AddRange(eventList);
                }
            }
            return valueEventList;
        }
#endregion

    }

}
