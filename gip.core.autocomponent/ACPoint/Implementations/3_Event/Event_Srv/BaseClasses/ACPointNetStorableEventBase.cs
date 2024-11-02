// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableEventBase'}de{'ACPointNetStorableEventBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableEventBase<T, W> : ACPointNetStorableBase<T, W>
        where W : ACPointEventWrap<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableEventBase()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetStorableEventBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _LocalStorage = new List<W>();
        }
        #endregion

        #region Own Member
        /// <summary>
        /// Events are not Persistable
        /// </summary>
        /// <returns></returns>
        internal override bool ReStoreFromDB()
        {
            //return false;
            bool result = base.ReStoreFromDB();
            return result;
        }

        /// <summary>
        /// Events are not Persistable
        /// </summary>
        /// <returns></returns>
        public override bool Persist(bool withLock)
        {
            return base.Persist(withLock);
            //return false;
        }


        [IgnoreDataMember]
        private ACEventArgs _EventArgs;
        [DataMember]
        public ACEventArgs EventArgs
        {
            get
            {
                return _EventArgs;
            }
            set
            {
                _EventArgs = value;
            }
        }


        protected abstract ACPointEventWrap<T> OnGetNewWrapInstance(T refObject, ACPointNetEventDelegate asyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry);
        protected abstract ACPointEventWrap<T> OnGetNewWrapInstance(T refObject, string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry);

        public ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry)
        {
            if (AsyncCallbackDelegate == null)
                return null;
            if (MaxCapacityReached)
                return null;
            ACPointEventWrap<T> wEvent = OnGetNewWrapInstance((T)AsyncCallbackDelegate.Target, AsyncCallbackDelegate, clientEntry);
            ACPointEventWrap<T> wEventExist = base.GetWrapObject((W)wEvent);
            if (wEventExist != null)
                return wEventExist;
            if (!AddToList((W)wEvent))
                return null;
            OnLocalStorageListChanged();
            return wEvent;
        }

        /// <summary>
        /// Subscribes an Event, for CallbackMethods, which are defined by script
        /// </summary>
        /// <param name="fromObject"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="clientEntry"></param>
        /// <returns></returns>
        public ACPointEventWrap<T> SubscribeEvent(IACComponent fromObject, string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry)
        {
            if ((fromObject == null) || String.IsNullOrEmpty(asyncCallbackDelegateName))
                return null;
            if (fromObject.IsProxy)
                return null;
            if (MaxCapacityReached)
                return null;
            if (fromObject.ComponentClass.GetMethod(asyncCallbackDelegateName) == null)
                return null;
            ACPointEventWrap<T> wEvent = OnGetNewWrapInstance((T)fromObject, asyncCallbackDelegateName, clientEntry);
            ACPointEventWrap<T> wEventExist = base.GetWrapObject((W)wEvent);
            if (wEventExist != null)
                return wEventExist;
            if (!AddToList((W)wEvent))
                return null;
            OnLocalStorageListChanged();
            return wEvent;
        }

        /// <summary>
        /// Unsubscribes an Event
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        public bool UnSubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            if (AsyncCallbackDelegate == null)
                return false;
            ACPointEventWrap<T> wCloneEvent = new ACPointEventWrap<T>((T)AsyncCallbackDelegate.Target, this, AsyncCallbackDelegate);
            return Remove((W)wCloneEvent);
        }
        //void operator - (ACPointRefNetEventDelegate AsyncCallbackDelegate);

        /// <summary>
        /// Unsubscribes an Event
        /// </summary>
        /// <param name="fromObject"></param>
        /// <returns></returns>
        public bool UnSubscribeEvent(IACComponent fromObject)
        {
            if (fromObject == null)
                return false;
            ACPointEventWrap<T> wCloneEvent = new ACPointEventWrap<T>((T)fromObject, this, ""); // "" weil Vergleich nur auf ACUrl
            return Remove((W)wCloneEvent);
        }


        public bool Contains(string acUrl)
        {
            if (String.IsNullOrEmpty(acUrl))
                return false;
            ACPointEventWrap<T> wCloneEvent = new ACPointEventWrap<T>();
            wCloneEvent.ACUrl = acUrl;
            // wCloneEvent.AsyncCallbackDelegateName = ""; // "" weil Vergleich nur auf ACUrl
            return Contains((W)wCloneEvent);
        }


        public bool Contains(ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            if (AsyncCallbackDelegate == null)
                return false;
            ACPointEventWrap<T> wCloneEvent = new ACPointEventWrap<T>((T)AsyncCallbackDelegate.Target, this, AsyncCallbackDelegate);
            return Contains((W)wCloneEvent);
        }
        #endregion

        #region IACConnectionPoint<ACPointRefNetWrapEvent<T>> Member

        /// <summary>
        /// List of ACPointEventWrap-relations to other Subscription-Points (IACPointEventSubscr)
        /// </summary>
        public virtual new IEnumerable<ACPointEventWrap<T>> ConnectionList
        {
            get
            {
                return (IEnumerable<ACPointEventWrap<T>>)_LocalStorage;
            }

            set
            {
            }
        }

        //public ACEventArgs GetNewEventArgs()
        //{
        //    return (this.ACType as ACClassProperty).ACEventArgs;
        //}
        #endregion
    }
}

