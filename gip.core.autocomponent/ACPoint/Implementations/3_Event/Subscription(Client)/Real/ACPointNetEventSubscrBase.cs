using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetEventSubscrBase'}de{'ACPointNetEventSubscrBase'}", Global.ACKinds.TACClass)]
    public abstract class ACPointNetEventSubscrBase<T> : ACPointNetStorableEventSubscrBase<T, ACPointEventSubscrWrap<T>>, IACPointEventSubscr<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetEventSubscrBase()
            : this(null, null, 0)
        {
            _base = new ACPointClientReal<T, ACPointEventSubscrWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetEventSubscrBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointClientReal<T, ACPointEventSubscrWrap<T>>(this);
        }
        #endregion

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointEventSubscrWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointEventSubscrWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientReal<T, ACPointEventSubscrWrap<T>>(this);
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

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
        }

        #endregion

        #region IACPointRefNetTo Member

        public IACPointNetBase GetServicePoint(IACObject fromACComponent, string fromPointName)
        {
            ACComponent acObject = (ACComponent)fromACComponent;
            IACPointNetBase cp = acObject.GetPointNet(fromPointName);
            if (cp == null)
                return null;
            if (!(cp is IACPointNetService<T,ACPointEventWrap<T>>))
                return null;
            if (!(cp is IACPointEvent<T>))
                return null;
            return cp;
        }

        public void InvokeSetMethod(IACPointNetBase point)
        {
            if (this.SetMethod != null)
                this.SetMethod(point);
            RemoveAllRejected(true);
        }

        #endregion

        #region IACPointRefNetToEvent<T> Member

        /// <summary>
        /// Subscribes an Event at atACObject, for CallbackMethods, which are defined in a assembly
        /// </summary>
        /// <param name="atACComponent">ACObject which publish events</param>
        /// <param name="eventName">Name of event</param>
        /// <param name="asyncCallbackDelegate">Event-Handler-CallBack-Delegate of this</param>
        /// <returns></returns>
        public ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, ACPointNetEventDelegate asyncCallbackDelegate)
        {
            if ((atACComponent == null) || String.IsNullOrEmpty(eventName) || (asyncCallbackDelegate == null))
                return null;
            if (this.ACRef.ValueT != asyncCallbackDelegate.Target)
                return null;
            ACComponent acObject = (ACComponent)atACComponent;
            IACPointEvent<T> iEvent = (IACPointEvent<T>) GetServicePoint(atACComponent, eventName);
            if (iEvent == null)
                return null;
            return Subscribe(atACComponent, iEvent, asyncCallbackDelegate);
        }

        /// <summary>
        /// Subscribes an Event from atACObject, for CallbackMethods, which are defined by script   
        /// </summary>
        /// <param name="atACComponent">ACObject which publish events</param>
        /// <param name="eventName">Name of even</param>
        /// <param name="asyncCallbackDelegateName">Name of Event-Handler-CallBack-Delegate of this</param>
        /// <returns></returns>
        public ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, string asyncCallbackDelegateName)
        {
            if ((atACComponent == null) || String.IsNullOrEmpty(eventName) || String.IsNullOrEmpty(asyncCallbackDelegateName))
                return null;
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return null;
            ACPointEventSubscrWrap<T> wEventSubscr = null;
            if (!parentACComponent.IsProxy && parentACComponent.ACType != null)
            {
                if (parentACComponent.ComponentClass.GetMethod(asyncCallbackDelegateName) != null)
                {
                    ACComponent acObject = (ACComponent)atACComponent;
                    IACPointEvent<T> iEvent = (IACPointEvent<T>)GetServicePoint(atACComponent, eventName);
                    if (iEvent != null)
                    {
                        wEventSubscr = Subscribe(acObject, iEvent, asyncCallbackDelegateName);
                        /*wEventSubscr = new ACPointEventSubscrWrap<T>((T)atACObject, this, iEvent);
                        ACPointEventSubscrWrap<T> wEventSubscrExist = base.GetWrapObject(wEventSubscr);
                        if (wEventSubscrExist == null)
                        {
                            ACPointEventWrap<T> wEvent = iEvent.SubscribeEvent(this.ACRef.Obj, asyncCallbackDelegateName, this.PropertyName);
                            if (wEvent != null)
                            {
                                if (!AddToList(wEventSubscr))
                                {
                                    wEventSubscr = null;
                                }
                                else
                                    OnLocalStorageListChanged();
                            }
                            else
                                wEventSubscr = null;
                        }
                        else
                            wEventSubscr = wEventSubscrExist;*/
                    }
                }
            }
            return wEventSubscr;
        }


        /// <summary>
        /// Subscribes all events of atACObject
        /// </summary>
        /// <param name="atACComponent"></param>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        public bool SubscribeAllEvents(IACComponent atACComponent, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            if ((atACComponent == null) || (AsyncCallbackDelegate == null))
                return false;
            if (!ACRef.IsObjLoaded || (this.ACRef.ValueT != AsyncCallbackDelegate.Target))
                return false;
            ACComponent acObject = (ACComponent)atACComponent;
            bool isSubscribed = false;
            if (acObject.Events != null)
            {
                foreach (IACPointEvent<T> iEvent in acObject.Events)
                {
                    if (Subscribe(atACComponent, iEvent, AsyncCallbackDelegate) != null)
                        isSubscribed = true;
                }
            }
            return isSubscribed;
        }

        public ACPointEventSubscrWrap<T> Subscribe(IACObject atACComponent, IACPointEvent<T> iEvent, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            if (MaxCapacityReached || iEvent.MaxCapacityReached)
                return null;
            ACPointEventSubscrWrap<T> wEventSubscr = new ACPointEventSubscrWrap<T>((T)atACComponent, this, iEvent);
            ACPointEventSubscrWrap<T> wEventSubscrExist = base.GetWrapObject(wEventSubscr);
            if (wEventSubscrExist != null)
                return null;
            ACPointEventWrap<T> wEvent = iEvent.SubscribeEvent(AsyncCallbackDelegate, wEventSubscr);
            if (wEvent != null)
            {
                if (!AddToList(wEventSubscr))
                    return null;
#if DEBUG
                if (!this.ParentACComponent.Root.Initialized && this.IsPersistable)
                {
                    string acUrlSubscr = this.ParentACComponent.GetACUrl();
                    string acUrlEvent = iEvent.ParentACComponent.GetACUrl();
                    this.ParentACComponent.Messages.LogError(acUrlSubscr, this.ACIdentifier, String.Format("***Point*** Resubscribed at Init: {0}/{1}. Count this {2}, Count Event {3} ", acUrlEvent, iEvent.ACIdentifier, this.ConnectionList.Count(), iEvent.ConnectionList.Count()));
                    //if (System.Diagnostics.Debugger.IsAttached)
                    //{
                    //    System.Diagnostics.Debugger.Break();
                    //}
                }
#endif

                OnLocalStorageListChanged();
                return wEventSubscr;
            }
            return null;
        }

        public ACPointEventSubscrWrap<T> Subscribe(IACObject atACComponent, IACPointEvent<T> iEvent, string asyncCallbackDelegateName)
        {
            if (MaxCapacityReached || iEvent.MaxCapacityReached)
                return null;
            ACPointEventSubscrWrap<T> wEventSubscr = new ACPointEventSubscrWrap<T>((T)atACComponent, this, iEvent);
            ACPointEventSubscrWrap<T> wEventSubscrExist = base.GetWrapObject(wEventSubscr);
            if (wEventSubscrExist != null)
                return null;
            ACPointEventWrap<T> wEvent = iEvent.SubscribeEvent(asyncCallbackDelegateName, wEventSubscr);
            if (wEvent != null)
            {
                if (!AddToList(wEventSubscr))
                    return null;
                OnLocalStorageListChanged();
                return wEventSubscr;
            }
            return null;
        }

        /// <summary>
        /// Unsubscribes this Event-Subscription at the Event (eventName) at a ACObject (atACUrl)
        /// (Removes automatically the corresponding EventHandler-Delegate at IACPointEvent)
        /// </summary>
        /// <param name="atACComponent"></param>
        /// <param name="eventName">Name of event</param>
        /// <returns></returns>
        public bool UnSubscribeEvent(IACComponent atACComponent, string eventName)
        {
            if ((atACComponent == null) || String.IsNullOrEmpty(eventName))
                return false;

            ACComponent acObject = (ACComponent)atACComponent;
            IACPointEvent<T> iEvent = (IACPointEvent<T>)GetServicePoint(atACComponent, eventName);
            if (iEvent == null)
                return false;
            return RemoveEvent(atACComponent, iEvent);
        }

        /// <summary>
        /// Unsubscribes all subscribed Events of this Event-Subscription
        /// at a ACObject (removes automatically all EventHandler-Delegates at IACPointEvent)
        /// </summary>
        /// <param name="atACComponent"></param>
        /// <returns></returns>
        public bool UnSubscribeAllEvents(IACComponent atACComponent)
        {
            if (atACComponent == null)
                return false;

            ACComponent acObject = (ACComponent)atACComponent;
            bool isUnSubscribed = true;
            if (acObject.Events != null)
            {
                foreach (IACPointEvent<T> iEvent in acObject.Events)
                {
                    if (!RemoveEvent(atACComponent, iEvent))
                        isUnSubscribed = false;
                }
            }
            return isUnSubscribed;
        }

        public bool UnSubscribeAllEvents()
        {
            bool isUnSubscribed = true;
            foreach (ACPointEventSubscrWrap<T> subscrWrapObj in _LocalStorage.ToList())
            {
                if (!RemoveEvent(subscrWrapObj.ValueT, subscrWrapObj.Event))
                    isUnSubscribed = false;
            }
            return isUnSubscribed;
        }


        private bool RemoveEvent(IACComponent atACComponent, IACPointEvent<T> iEvent)
        {
            if (iEvent == null)
                return true;
            ACPointEventSubscrWrap<T> wCloneEventSubscr = new ACPointEventSubscrWrap<T>((T)atACComponent, this, iEvent);
            bool removed = false;
            if (this.ACRef.ValueT != null)
                removed = iEvent.UnSubscribeEvent(this.ACRef.ValueT);
            //if (removed)
            if (Remove(wCloneEventSubscr))
                removed = true;
            return removed;
        }

        /// <summary>
        /// Check if this Event-Subscription subscribes the Event (eventName) at a ACObject (atACUrl)
        /// </summary>
        /// <param name="atACUrl"></param>
        /// <param name="acIdentifier">Name des Event-Properties</param>
        /// <returns></returns>
        public bool Contains(string atACUrl, string acIdentifier)
        {
            if (String.IsNullOrEmpty(atACUrl) || String.IsNullOrEmpty(acIdentifier))
                return false;
            ACPointEventSubscrWrap<T> wCloneEventSubscr = new ACPointEventSubscrWrap<T>();
            wCloneEventSubscr.ACUrl = atACUrl;
            wCloneEventSubscr.EventACIdentifier = acIdentifier;
            return Contains(wCloneEventSubscr);
        }

        /// <summary>
        /// Check if this Event-Subscription subscribes the Event for this EventHandler-Delegate
        /// </summary>
        /// <param name="atACComponent"></param>
        /// <param name="asyncCallbackDelegate"></param>
        /// <returns></returns>
        public bool Contains(IACComponent atACComponent, ACPointNetEventDelegate asyncCallbackDelegate)
        {
            if (asyncCallbackDelegate == null)
                return false;

            ACComponent acObject = (ACComponent)atACComponent;
            if (acObject.Events != null)
            {
                foreach (IACPointEvent<T> iEvent in acObject.Events)
                {
                    if (iEvent.Contains(asyncCallbackDelegate))
                        return true;
                }
            }
            return false;
        }

        #endregion
    }
}

