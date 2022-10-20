using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetEventBase'}de{'ACPointNetEventBase'}", Global.ACKinds.TACClass)]
    public abstract class ACPointNetEventBase<T> : ACPointNetStorableEventBase<T, ACPointEventWrap<T>>, IACPointEvent<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetEventBase()
            : this(null, null, 0)
        {
            _base = new ACPointServiceReal<T, ACPointEventWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetEventBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceReal<T, ACPointEventWrap<T>>(this);
        }
        #endregion

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointEventWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointEventWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceReal<T, ACPointEventWrap<T>>(this);
                return _base;
            }
        }
        #endregion

        #region Override Member
        
        protected override void OnLocalStorageListChanged()
        {
            // Broadcast zu den Clients
            Base.OnLocalStorageListChanged();
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
        }

        protected override ACPointEventWrap<T> OnGetNewWrapInstance(T refObject, ACPointNetEventDelegate asyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry)
        {
            if (clientEntry == null)
                return new ACPointEventWrap<T>((T)refObject, this, asyncCallbackDelegate);
            else
                return new ACPointEventWrap<T>((T)refObject, this, asyncCallbackDelegate, clientEntry);
        }

        protected override ACPointEventWrap<T> OnGetNewWrapInstance(T refObject, string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry)
        {
            if (clientEntry == null)
                return new ACPointEventWrap<T>((T)refObject, this, asyncCallbackDelegateName);
            else
                return new ACPointEventWrap<T>((T)refObject, this, asyncCallbackDelegateName, clientEntry);
        }

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Subscription-Points (IACPointEventSubscr)
        /// </summary>
        [DataMember]
        public override IEnumerable<ACPointEventWrap<T>> ConnectionList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                // Geht nur bei Proxies
                //Base.ConnectionList = value;
            }
        }

        [IgnoreDataMember]
        public new IEnumerable<T> RefObjectList
        {
            get
            {
                return Base.RefObjectList;
            }
        }

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }
        #endregion

        #region IACPointRefNetEvent<T> Member

        /// <summary>
        /// Subscribes this Event, for CallbackMethods, which are defined by script
        /// </summary>
        /// <param name="asyncCallbackDelegate"></param>
        /// <returns></returns>
        public ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate asyncCallbackDelegate)
        {
            return base.SubscribeEvent(asyncCallbackDelegate, null);
        }

        /// <summary>
        /// Subscribes an Event, for CallbackMethods, which are defined by script
        /// </summary>
        /// <param name="fromACComponent"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <returns></returns>
        public ACPointEventWrap<T> SubscribeEvent(IACComponent fromACComponent, string asyncCallbackDelegateName)
        {
            return base.SubscribeEvent(fromACComponent, asyncCallbackDelegateName, null);
        }

        /// <summary>
        /// Subscribes this Event by Adding an Event-Handler-Delegate "of the Client-Object". 
        /// This Method is Called over the IACPointEventSubscr-Interface.
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <param name="clientEntry"></param>
        /// <returns></returns>
        public new ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry)
        {
            return base.SubscribeEvent(AsyncCallbackDelegate, clientEntry);
        }

        /// <summary>
        /// Subscribes this Event, for CallbackMethods, which are defined by script
        /// This Method is Called over the IACPointEventSubscr-Interface.
        /// </summary>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="clientEntry"></param>
        /// <returns></returns>
        public ACPointEventWrap<T> SubscribeEvent(string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry)
        {
            return base.SubscribeEvent(clientEntry.ParentACObject as IACComponent, asyncCallbackDelegateName, clientEntry);
        }

        /// <summary>
        /// Unsubscribes this Event by Removing ACObject over the Event-Handler-Delegate
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        public new bool UnSubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            return base.UnSubscribeEvent(AsyncCallbackDelegate);
        }
        //void operator - (ACPointRefNetEventDelegate AsyncCallbackDelegate);

        /// <summary>
        /// Unsubscribes a ACObject from this Event
        /// </summary>
        /// <param name="fromACComponent"></param>
        /// <returns></returns>
        public new bool UnSubscribeEvent(IACComponent fromACComponent)
        {
            return base.UnSubscribeEvent(fromACComponent);
        }

        /// <summary>
        /// Check if ACObject is subscribed at this Event
        /// </summary>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        public new bool Contains(string acUrl)
        {
            return base.Contains(acUrl);
        }

        public new bool Contains(ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            return base.Contains(AsyncCallbackDelegate);
        }

        public void Raise(ACEventArgs eventArgs)
        {
            // Raise Events locally
            if (RaisingStarted != null)
                RaisingStarted(this, new EventArgs());

            ACPointEventWrap<T>[] callbacks = null;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                callbacks = LocalStorage.ToArray();
            }

            if (callbacks != null && callbacks.Any())
            {
                foreach (ACPointEventWrap<T> eventWrap in callbacks)
                {
                    if (eventWrap.AsyncCallbackDelegate != null)
                    {
                        eventWrap.AsyncCallbackDelegate(this, eventArgs, eventWrap);
                    }
                    else if (!String.IsNullOrEmpty(eventWrap.AsyncCallbackDelegateName))
                    {
                        ACComponent refACObject = null;
                        refACObject = (ACComponent)(IACComponent)eventWrap.ValueT;
                        if (refACObject != null)
                            refACObject.ExecuteMethod(eventWrap.AsyncCallbackDelegateName, new object[] { (IACPointNetBase)this, (ACEventArgs)eventArgs, (IACObject)eventWrap });
                    }
                }
            }
            if (RaisingCompleted != null)
                RaisingCompleted(this, new EventArgs());

            // Raise Events Remote
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent != null)
            {
                bool marked = false;
                List<ACPSubscrObjService> SubscribedProxyObjects = parentACComponent.SubscribedProxyObjects;
                foreach (ACPSubscrObjService proxyObj in SubscribedProxyObjects)
                {
                    ACPointNetEventProxy<T> cpProxy = (ACPointNetEventProxy<T>)proxyObj.GetConnectionPoint(this.ACIdentifier);
                    if (cpProxy != null)
                    {
                        cpProxy.EventArgs = eventArgs;
                        cpProxy.RaiseCounter++;
                        cpProxy.PointChangedForBroadcast = true;
                        marked = true;
                    }
                }
                if (marked)
                    Base.MarkACObjectOnChangedPoint(parentACComponent);
            }
        }

#endregion


#region IACPointRefNetFrom<T,ACPointEventWrap<T>> Member

        [IgnoreDataMember]
        public IEnumerable<ACPointEventWrap<T>> ConnectionListLocal
        {
            get 
            { 
                return Base.ConnectionListLocal; 
            }
        }

        [IgnoreDataMember]
        public IEnumerable<T> RefObjectListLocal
        {
            get
            {
                return Base.RefObjectListLocal;
            }
        }

        /// <summary>
        /// Reference to Extension-Class, which implements this interface (workaround for multiple inheritance)
        /// </summary>
        [IgnoreDataMember]
        public ACPointServiceBase<T, ACPointEventWrap<T>> ServicePointHelper
        {
            get
            {
                return Base;
            }
        }

        public void InvokeSetMethod(IACPointNetBase point)
        {
            if (this.SetMethod != null)
                this.SetMethod(point);
            if (point == this)
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    RemoveAllRejected(false);
                    CopyDataOfLocalStorageToClientPoints(false);
                }
            }
        }

#endregion


        public event EventHandler RaisingStarted;

        public event EventHandler RaisingCompleted;
    }
}

