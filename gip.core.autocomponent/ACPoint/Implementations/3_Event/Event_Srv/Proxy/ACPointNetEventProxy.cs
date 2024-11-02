// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    internal abstract class ACPointNetEventProxy<T> : ACPointNetStorableEventBase<T, ACPointEventWrap<T>>, IACPointEvent<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetEventProxy()
            : this(null, null, 0)
        {
            _base = new ACPointServiceProxy<T, ACPointEventWrap<T>>(this, true);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetEventProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceProxy<T, ACPointEventWrap<T>>(this, false);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointEventWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointEventWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceProxy<T, ACPointEventWrap<T>>(this, true);
                return _base;
            }
        }
        #endregion

        #region Override Member
        public override void Subscribe(bool force = true)
        {
            Base.Subscribe(force);
        }

        public override void ReSubscribe()
        {
            Base.Subscribe(false);
        }

        public override void UnSubscribe()
        {
            Base.UnSubscribe();
        }

        protected override void OnLocalStorageListChanged()
        {
            // Trage im Dispatcher zum Versand ein.
            Base.OnLocalStorageListChanged();
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
            Base.RebuildAfterDeserialization(parentSubscrObject);
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
        /// List of ACPointEventWrap-relations to other Subscription-Points (IACPointEventSubscr)
        /// </summary>
        [IgnoreDataMember]
        public override IEnumerable<ACPointEventWrap<T>> ConnectionList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                Base.ConnectionList = value;
            }
        }


        public override string ConnectionListInfo
        {
            get
            {
                Subscribe(false); // Bindings to Tooltips invokes cyclic this Method. To reduce the serverside cpu-usage the subscription-Request should only take one time.
                return base.ConnectionListInfo;
            }
        }

        /// <summary>
        /// Member für Serialisierung, weil ConnectionList nicht serialisiert werden kann,
        /// weil das Interface nur eine get-Anweisung enhält
        /// </summary>
        [DataMember]
        private IEnumerable<ACPointEventWrap<T>> SerConnList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                Base.ConnectionList = value;
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

        [IgnoreDataMember]
        private int _RaiseCounter = 0;

        [DataMember]
        public int RaiseCounter
        {
            get
            {
                return _RaiseCounter;
            }
            set
            {
                _RaiseCounter = value;
            }
        }

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            if (EventArgs != null)
                EventArgs.AttachTo(ACRef.ValueT);

            Base.OnPointReceivedRemote(receivedPoint);
            
            if (!Base.IsInstanceOnServerSide)
            {
                ACPointNetEventProxy<T> receivedProxyPoint = (ACPointNetEventProxy<T>)receivedPoint;
                this.EventArgs = receivedProxyPoint.EventArgs;
                if (receivedProxyPoint.RaiseCounter > this.RaiseCounter)
                {
                    this.RaiseCounter = receivedProxyPoint.RaiseCounter;
                    Raise(this.EventArgs);
                }
            }
        }


        #endregion

        #region IACPointRefNetEvent<T> Member

        /// <summary>
        /// Subscribes this Event by Adding an Event-Handler-Delegate "of the Client-Object"
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        public ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            return base.SubscribeEvent(AsyncCallbackDelegate, null);
        }

        /// <summary>
        /// Subscribes this Event, for CallbackMethods, which are defined by script
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
                    if (!String.IsNullOrEmpty(eventWrap.AsyncCallbackDelegateName))
                    {
                        ACComponent refACObject = (ACComponent)(IACComponent)eventWrap.ValueT;
                        if (refACObject != null)
                            refACObject.ExecuteMethod(eventWrap.AsyncCallbackDelegateName, new object[] { (IACPointNetBase)this, (ACEventArgs)eventArgs, (IACObject)eventWrap });
                    }
                }
            }
            if (RaisingCompleted != null)
                RaisingCompleted(this, new EventArgs());
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
        }

#endregion


        public event EventHandler RaisingStarted;

        public event EventHandler RaisingCompleted;
    }
}

