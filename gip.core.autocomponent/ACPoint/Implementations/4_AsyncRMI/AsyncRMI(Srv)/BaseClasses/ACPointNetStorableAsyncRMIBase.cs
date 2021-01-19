﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableAsyncRMIBase'}de{'ACPointNetStorableAsyncRMIBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableAsyncRMIBase<T, W> : ACPointNetStorableBase<T, W>
        where W : ACPointAsyncRMIWrap<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableAsyncRMIBase()
            : this(null, null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointNetStorableAsyncRMIBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _LocalStorage = new List<W>();
        }
        #endregion

        #region Own Member
        protected abstract ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointNetEventDelegate asyncCallbackDelegate, ACMethod acMethod);
        protected abstract ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, string asyncCallbackDelegateName, ACMethod acMethod);
        protected abstract ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointAsyncRMISubscrWrap<T> clientEntry, ACPointNetEventDelegate asyncCallbackDelegate, ACMethod acMethod);
        protected abstract ACPointAsyncRMIWrap<T> OnGetNewWrapInstance(T refObject, ACPointAsyncRMISubscrWrap<T> clientEntry, string asyncCallbackDelegateName, ACMethod acMethod);

        public ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, ACPointNetEventDelegate AsyncCallbackDelegate, ACMethod acMethod)
        {
            if ((AsyncCallbackDelegate == null) || (acMethod == null) || !acMethod.IsValid())
                return null;
            if (!ACRef.IsObjLoaded || (ACRef.ValueT.ComponentClass.GetMethod(acMethod.ACIdentifier) == null))
                return null;
            ACPointAsyncRMIWrap<T> wAsync = null;
            if (clientEntry == null)
            {
                wAsync = OnGetNewWrapInstance((T)AsyncCallbackDelegate.Target, AsyncCallbackDelegate, acMethod);
                if (!acMethod.AutoRemove)
                    acMethod.AutoRemove = true;
            }
            else
            {
                wAsync = OnGetNewWrapInstance((T)AsyncCallbackDelegate.Target, clientEntry, AsyncCallbackDelegate, acMethod);
                if (!clientEntry.Point.IsPersistable && !acMethod.AutoRemove)
                    acMethod.AutoRemove = true;
            }
            ACPointAsyncRMIWrap<T> wAsyncExist = base.GetWrapObject((W)wAsync);
            if (wAsyncExist != null)
                return wAsyncExist;
            if (!AddToList((W)wAsync))
                return null;
            OnLocalStorageListChanged();
            if ((wAsync.State == PointProcessingState.Rejected) || (wAsync.State == PointProcessingState.Deleted))
            {
                if (!Contains(wAsync))
                    wAsync = null;
            }
            return wAsync;
        }

        public ACPointAsyncRMIWrap<T> InvokeAsyncMethod(ACPointAsyncRMISubscrWrap<T> clientEntry, IACComponent fromObject, string asyncCallbackDelegateName, ACMethod acMethod)
        {
            if ((fromObject == null) || String.IsNullOrEmpty(asyncCallbackDelegateName) || (acMethod == null) || !acMethod.IsValid())
                return null;
            if (fromObject.IsProxy)
                return null;
            if (fromObject.ComponentClass.GetMethod(asyncCallbackDelegateName) == null)
                return null;
            if (!ACRef.IsObjLoaded || (ACRef.ValueT.ComponentClass.GetMethod(acMethod.ACIdentifier) == null))
                return null;
            ACPointAsyncRMIWrap<T> wAsync = null;
            if (clientEntry == null)
            {
                wAsync = OnGetNewWrapInstance((T)fromObject, asyncCallbackDelegateName, acMethod);
                if (!acMethod.AutoRemove)
                    acMethod.AutoRemove = true;
            }
            else
            {
                wAsync = OnGetNewWrapInstance((T)fromObject, clientEntry, asyncCallbackDelegateName, acMethod);
                if (!clientEntry.Point.IsPersistable && !acMethod.AutoRemove)
                    acMethod.AutoRemove = true;
            }
            ACPointAsyncRMIWrap<T> wAsyncExist = base.GetWrapObject((W)wAsync);
            if (wAsyncExist != null)
                return wAsyncExist;
            if (!AddToList((W)wAsync))
                return null;
            OnLocalStorageListChanged();
            if ((wAsync.State == PointProcessingState.Rejected) || (wAsync.State == PointProcessingState.Deleted))
            {
                if (!Contains(wAsync))
                    wAsync = null;
            }
            return wAsync;
        }

        #endregion

        #region IACConnectionPoint<ACPointRefNetWrapAsyncRMI<T>> Member

        /// <summary>
        /// List of ACPointAsyncRMIWrap-relations to other Subscription-Points (IACPointAsyncRMISubscr)
        /// </summary>
        public virtual new IEnumerable<ACPointAsyncRMIWrap<T>> ConnectionList
        {
            get 
            { 
                return (IEnumerable<ACPointAsyncRMIWrap<T>>)_LocalStorage; 
            }

            set
            {
            }
        }

        #endregion
    }

}

