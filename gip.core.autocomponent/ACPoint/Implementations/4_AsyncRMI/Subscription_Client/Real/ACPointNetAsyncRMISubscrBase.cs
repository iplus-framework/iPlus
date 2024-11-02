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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetAsyncRMISubscrBase'}de{'ACPointNetAsyncRMISubscrBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetAsyncRMISubscrBase<T> : ACPointNetStorableAsyncRMISubscrBase<T, ACPointAsyncRMISubscrWrap<T>>, IACPointAsyncRMISubscr<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetAsyncRMISubscrBase()
            : this(null, null, 0)
        {
            _base = new ACPointClientReal<T, ACPointAsyncRMISubscrWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetAsyncRMISubscrBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointClientReal<T, ACPointAsyncRMISubscrWrap<T>>(this);
        }
        #endregion

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointAsyncRMISubscrWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointAsyncRMISubscrWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientReal<T, ACPointAsyncRMISubscrWrap<T>>(this);
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
            //base.UpdateLocalStorage(receivedPoint);
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
            if (!(cp is IACPointNetService<T, ACPointAsyncRMIWrap<T>>))
                return null;
            if (!(cp is IACPointAsyncRMI<T>))
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

        #region IACPointAsyncRMISubscr<T> Member

        /// <summary>Invokes an AsyncMethod at atACObject. It registers a Callback-Method, which is defined in an Assembly-Class</summary>
        /// <param name="atACComponent">ACObject which publish an AsyncMethod</param>
        /// <param name="asyncRMIPointName">Name of Point for activating the AsyncMethod</param>
        /// <param name="acMethod"></param>
        /// <param name="AsyncCallbackDelegate">Event-Handler-CallBack-Delegate of this when Asyc-Method is Executed</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            return InvokeAsyncMethod(atACComponent, asyncRMIPointName, acMethod, AsyncCallbackDelegate, !PropertyInfo.IsPersistable);
        }

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName,
                                                       ACMethod acMethod,
                                                       ACPointNetEventDelegate AsyncCallbackDelegate,
                                                       bool AutoRemove)
        {
            if ((atACComponent == null) || String.IsNullOrEmpty(asyncRMIPointName) || (AsyncCallbackDelegate == null)
                || (acMethod == null) || !acMethod.IsValid())
                return null;
            if (acMethod.AutoRemove != AutoRemove)
                acMethod.AutoRemove = AutoRemove;
            if (!ACRef.IsObjLoaded || (ACRef.ValueT != AsyncCallbackDelegate.Target))
                return null;
            ACComponent acObject = (ACComponent)atACComponent;
            if (acObject.ComponentClass.GetMethod(acMethod.ACIdentifier) == null)
                return null;
            IACPointAsyncRMI<T> iAsyncRMI = (IACPointAsyncRMI<T>)GetServicePoint(atACComponent, asyncRMIPointName);
            if (iAsyncRMI == null)
                return null;
            if (MaxCapacityReached || iAsyncRMI.MaxCapacityReached)
                return null;

            ACPointAsyncRMISubscrWrap<T> wAsyncRMISubscr = new ACPointAsyncRMISubscrWrap<T>((T)atACComponent, this, iAsyncRMI, acMethod);
            // Immer Neuer Request ohne Vergleichs-Überprüfung
            /*ACPointAsyncRMISubscrWrap<T> wAsyncRMISubscrExist = base.GetWrapObject(wAsyncRMISubscr);
            if (wAsyncRMISubscrExist != null)
                return wAsyncRMISubscrExist;*/
            _base.PrepareRequestIfSynchronousMode(iAsyncRMI);

            try
            {
                ACPointAsyncRMIWrap<T> wAsyncRMI = iAsyncRMI.InvokeAsyncMethod(wAsyncRMISubscr, AsyncCallbackDelegate, acMethod);
                if (wAsyncRMI == null)
                    return null;
                wAsyncRMISubscr.State = wAsyncRMI.State;
                if (wAsyncRMISubscr.State < PointProcessingState.Rejected)
                {
                    if (!AddToList(wAsyncRMISubscr))
                    {
                        wAsyncRMISubscr.State = PointProcessingState.Rejected;
                        return wAsyncRMISubscr;
                    }

                    if (_base.WaitOnRequestIfSynchronousMode(iAsyncRMI))
                    {
                        if (!iAsyncRMI.Contains(wAsyncRMI))
                        {
                            wAsyncRMISubscr.State = PointProcessingState.Rejected;
                            return wAsyncRMISubscr;
                        }
                    }

                    OnLocalStorageListChanged();
                }
            }
            finally
            {
                _base.RemoveRequestIfSynchronousMode(iAsyncRMI);
            }

            return wAsyncRMISubscr;
        }

        /// <summary>Invokes an AsyncMethod at atACObject. It registers a Callback-Method, which is defined in an Assembly-Class</summary>
        /// <param name="atACComponent">ACObject which publish an AsyncMethod</param>
        /// <param name="asyncRMIPointName">Name of Point for activating the AsyncMethod</param>
        /// <param name="acMethod"></param>
        /// <param name="asyncCallbackDelegateName">Event-Handler-CallBack-Delegate of this when Asyc-Method is Executed</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, string asyncCallbackDelegateName)
        {
            return InvokeAsyncMethod(atACComponent, asyncRMIPointName, acMethod, asyncCallbackDelegateName, !PropertyInfo.IsPersistable);
        }

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, string asyncCallbackDelegateName,
                                                            bool AutoRemove)
        {
            if ((atACComponent == null) || String.IsNullOrEmpty(asyncRMIPointName) || String.IsNullOrEmpty(asyncCallbackDelegateName)
                || (acMethod == null) || !acMethod.IsValid())
                return null;
            if (acMethod.AutoRemove != AutoRemove)
                acMethod.AutoRemove = AutoRemove;
            ACPointAsyncRMISubscrWrap<T> wAsyncRMISubscr = null;
            ACComponent parentACComponent = ParentACComponent as ACComponent;
            if (parentACComponent != null && !parentACComponent.IsProxy && parentACComponent.ComponentClass != null)
            {
                if (parentACComponent.ComponentClass.GetMethod(asyncCallbackDelegateName) != null)
                {
                    ACComponent acObject = (ACComponent)atACComponent;
                    if (acObject.ComponentClass.GetMethod(acMethod.ACIdentifier) != null)
                    {
                        IACPointAsyncRMI<T> iAsyncRMI = (IACPointAsyncRMI<T>)GetServicePoint(atACComponent, asyncRMIPointName);
                        if (iAsyncRMI == null)
                            return null;
                        if (MaxCapacityReached || iAsyncRMI.MaxCapacityReached)
                            return null;
                        _base.PrepareRequestIfSynchronousMode(iAsyncRMI);

                        try
                        {
                            wAsyncRMISubscr = new ACPointAsyncRMISubscrWrap<T>((T)atACComponent, this, iAsyncRMI, acMethod);
                            // Immer Neuer Request ohne Vergleichs-Überprüfung
                            /*ACPointAsyncRMISubscrWrap<T> wAsyncRMISubscrExist = base.GetWrapObject(wAsyncRMISubscr);
                            if (wAsyncRMISubscrExist != null)
                                return wAsyncRMISubscrExist;*/
                            ACPointAsyncRMIWrap<T> wAsyncRMI = iAsyncRMI.InvokeAsyncMethod(wAsyncRMISubscr, asyncCallbackDelegateName, acMethod);
                            if (wAsyncRMI == null)
                                return null;
                            wAsyncRMISubscr.State = wAsyncRMI.State;
                            if (!AddToList(wAsyncRMISubscr))
                                return null;

                            if (_base.WaitOnRequestIfSynchronousMode(iAsyncRMI))
                            {
                                if (!iAsyncRMI.Contains(wAsyncRMI))
                                    return null;
                            }
                            OnLocalStorageListChanged();
                        }
                        finally
                        {
                            _base.RemoveRequestIfSynchronousMode(iAsyncRMI);
                        }
                    }
                }
            }
            return wAsyncRMISubscr;
        }

        #endregion
    }
}

