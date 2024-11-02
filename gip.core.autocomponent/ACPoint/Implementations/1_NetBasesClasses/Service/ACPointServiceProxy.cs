// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class for implementing Real- or Proxy-Implementations which holds the "wrapObjects"(Wrapper) in a local List.
    /// All in ACPointRefNetBase declared abstract methods operates on this local storage list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class ACPointServiceProxy<T, W> : ACPointServiceBase<T, W> 
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointServiceProxy(ACPointNetStorableBase<T, W> storablePoint, bool isDeserializedPoint)
            : base(storablePoint)
        {
            if (!IsInstanceOnServerSide && !isDeserializedPoint)
            {
                if (_this.ReStoreFromDB())
                {
                    if (_this.LocalStorage.Count > 0)
                        _this.PointChangedForBroadcast = true;
                }
            }
        }
        #endregion

        #region Protected Member
        public virtual void OnLocalStorageListChanged()
        {
            // Trage im Dispatcher zum Versand ein.
            MarkACObjectOnChangedPoint();
        }

        internal override void MarkACObjectOnChangedPoint(ACComponent parentACComponent = null)
        {
            if (parentACComponent == null)
                parentACComponent = _this.ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return;
            _this.PointChangedForBroadcast = true;
            if (IsInstanceOnServerSide)
            {
                parentACComponent.Root.MarkACObjectOnChangedPointForClient(parentACComponent);
            }
            else
            {
                // Variable isThisSyncCall ist false, wenn umgebende Funktion,
                // sprich eine Aufruf des Client-Points auch Synchron aufgerufen worden ist.
                // Oder false, wenn dieser Point nicht synchron aufgerufen werden soll.
                bool isThisSyncCall = PrepareRequestIfSynchronousMode();
                try
                {
                    parentACComponent.Root.MarkACObjectOnChangedPointForServer(parentACComponent);
                    _this.OnPropertyChangedLocalList();
                    _this.Persist(true);
                    if (isThisSyncCall)
                    {
                        WaitOnRequestIfSynchronousMode();
                    }
                }
                finally
                {
                    if (isThisSyncCall)
                        RemoveRequestIfSynchronousMode();
                }
            }
        }

        private bool PrepareRequestIfSynchronousMode()
        {
            if (!_this.SynchronousMode)
                return false;
            // Falls Synchroner Aufruf schon aktiviert durch umgebende Client-Funnktion
            if (_this._CurrentSyncRequest != null)
                return false;
            if (!(_this.ACRef.ValueT is ACComponentProxy))
                return false;
            _this._CurrentSyncRequest = (_this.ACRef.ValueT as ACComponentProxy).RMInvoker.NewSynchronousRequest();
            return (_this._CurrentSyncRequest != null);
        }

        private bool WaitOnRequestIfSynchronousMode()
        {
            if (!_this.SynchronousMode)
                return false;
            if (_this._CurrentSyncRequest == null)
                return false;
            if (!(_this.ACRef.ValueT is ACComponentProxy))
                return false;

            if ((_this.ACRef.ValueT as ACComponentProxy).RMInvoker.WaitOnSynchronousRequest(_this._CurrentSyncRequest))
            {
                _this._CurrentSyncRequest = null;
                return true;
            }
            _this._CurrentSyncRequest = null;
            return false;
        }

        internal void RemoveRequestIfSynchronousMode()
        {
            if (!_this.SynchronousMode)
                return;
            _this._CurrentSyncRequest = null;
        }

        private bool _IsSubscribed = false;
        internal void Subscribe(bool force)
        {
            if (IsInstanceOnServerSide)
                return;
            if ((_this.ACRef == null) || !_this.ACRef.IsObjLoaded || (_this.ACRef.ValueT.Root == null))
                return;
            if (   force
                || (!force && (_IsSubscribed || (_this.LocalStorage.Count > 0))))
            {
                if (force)
                    _IsSubscribed = true;
                _this.PointChangedForBroadcast = true;
                _this.ACRef.ValueT.Root.MarkACObjectOnChangedPointForServer(_this.ACRef.ValueT);
            }
        }

        internal void UnSubscribe()
        {
            if (IsInstanceOnServerSide)
            {
                _this.RemoveAll();
            }
        }

        public virtual void RebuildAfterDeserialization(object parentSubscrObject)
        {
            // If Called on Server-Side
            // TODO: Gemergte-Liste für Serialisierung ist das richtig so?
            if (parentSubscrObject != null)
            {
                if (parentSubscrObject is ACPSubscrObjService)
                    _this.ParentSubscrObject = (ACPSubscrObjService)parentSubscrObject;

                if (_this.LocalStorage == null)
                    return;
                W[] storageCopy = null;

                using (ACMonitor.Lock(_this.LockLocalStorage_20033))
                {
                    storageCopy = _this.LocalStorage.ToArray();
                }
                if (storageCopy != null && storageCopy.Any())
                {
                    foreach (var w in storageCopy)
                    {
                        w.Point = _this;
                    }
                    var query = storageCopy.Where(c => c.SequenceNo == 0);
                    if (query.Any())
                    {
                        if (_this.ACType == null)
                            return;
                        List<W> updateList = query.ToList();
                        updateList.ForEach(c => c.SequenceNo = ((ACClassProperty)_this.ACType).NextPointSeqNo);
                    }
                }
            }
            // Called on Client-Side
            else
            {
                if (_this.LocalStorage != null)
                {
                    W[] storageCopy = null;

                    using (ACMonitor.Lock(_this.LockLocalStorage_20033))
                    {
                        storageCopy = _this.LocalStorage.ToArray();
                    }
                    if (storageCopy != null && storageCopy.Any())
                    {
                        foreach (var w in storageCopy)
                        {
                            w.Point = _this;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If true, Instance is created by Deserializer and exists in Subscription (ACPSubscrACObjectServer-Instance) on Server-Side
        /// If false, Instance is created by Framework or Deserializer on Client-Side an exists ("hangs's") either on Real- or a Proxy-Object
        /// </summary>
        public virtual bool IsInstanceOnServerSide
        {
            get
            {
                return (_this.ParentSubscrObject != null);
            }
        }


        protected IEnumerable<W> _ConnectionList = null;

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Client-Points (IACPointNetClientObject)
        /// </summary>
        public IEnumerable<W> ConnectionList
        {
            get
            {
                // Falls Point, der Serverseitig am ACPSubscrObjService hängt
                if (IsInstanceOnServerSide)
                {
                    if (_this.ACRef != null)
                    {
                        ACComponent acObject = (ACComponent)_this.ACRef.ValueT;
                        if (acObject != null)
                        {
                            IACPointNetBase cpPoint = acObject.GetPointNet(_this.ACIdentifier);
                            if (cpPoint != null)
                            {
                                if (cpPoint is IACPointNet<T, W>)
                                {
                                    IACPointNet<T, W> servicePoint = (IACPointNet<T, W>)cpPoint;
                                    return servicePoint.ConnectionList;
                                }
                            }
                        }
                    }
                }
                // Falls Point, der am Proxy-Objekt (Client-Seite) hängt
                else
                {
                    // Auskommentiert, weil der Subscribe-Befehl dazu führt, dass erneut eine Invocation auf Server-Seite erfolgt. Ist 2013 von mir hinzugefügt worden aber warum?
                    if (_ConnectionList == null)
                        Subscribe(true);
                    return _ConnectionList;
                }
                return new List<W>();
            }
            set
            {
                _ConnectionList = value;
            }
        }

        public IEnumerable<W> GetConnectionListForSerialization()
        {
            return _ConnectionList;
        }

        public IEnumerable<T> RefObjectList
        {
            get
            {
                if (this.ConnectionList == null)
                    return null;
                return this.ConnectionList.Select(c => c.ValueT);
            }
        }
#endregion

#region IACConnectionPointBase Member
        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            // Falls Point, der am Proxy-Objekt (Client-Seite) hängt
            if (!IsInstanceOnServerSide)
            {
                // Kopiere Lokale Liste
                _this.UpdateLocalStorage(receivedPoint);
                _this.OnPropertyChangedLocalList();
                _this.Persist(true);

                // Kopiere Globale Liste
                if (receivedPoint is IACPointNet<T, W>)
                {
                    //IACPointNet<T, W> receivedServiceProxyPoint = (IACPointNet<T, W>)receivedPoint;
                    //_ConnectionList = receivedServiceProxyPoint.ConnectionList;
                    IACPointNetService<T, W> receivedServiceProxyPoint = (IACPointNetService<T, W>)receivedPoint;
                    _ConnectionList = receivedServiceProxyPoint.ConnectionListLocal;
                    _this.OnPropertyChangedGlobalList();
                }

                // Lösche verworfene Requests

                using (ACMonitor.Lock(_this.LockLocalStorage_20033))
                {
                    _this.RemoveAllRejected(false);
                    _this.CopyDataOfLocalStorageToClientPoints(false);
                }

                if (_this._CurrentSyncRequest != null)
                {
                    if (receivedPoint is ACPointNetBase<T, W>)
                    {
                        ACPointNetBase<T, W> pointOfRequest = (ACPointNetBase<T, W>)receivedPoint;
                        if (pointOfRequest.SyncRequestID == _this._CurrentSyncRequest.RequestID)
                            _this._CurrentSyncRequest.Set();
                    }

                }
            }
            // Sonst Point, der Serverseitig am ACPSubscrObjService hängt
            // Aufgerufen von ACPSubscrObjService.UpdateConnectionPoints()
            else
            {
                // Falls Point schon existierte, dann aktualisiere Liste
                if (receivedPoint != this)
                {
                    _this.UpdateLocalStorage(receivedPoint);
                    if (receivedPoint is ACPointNetBase<T, W>)
                        _this.SyncRequestID = ((ACPointNetBase<T, W>)receivedPoint).SyncRequestID;
                }

                // Callback auf "Real-Point" von Real-Server-ACObjekt 
                ACComponent parentACComponent = _this.ParentACComponent as ACComponent;
                if (parentACComponent != null)
                {
                    IACPointNetBase realPointOfACObj = parentACComponent.GetPointNet(receivedPoint.ACIdentifier);
                    if (realPointOfACObj != null)
                    {
                        if (realPointOfACObj is IACPointNetService<T, W>)
                        {
                            IACPointNetService<T, W> realServicePoint = realPointOfACObj as IACPointNetService<T, W>;
                            realServicePoint.InvokeSetMethod(_this);
                            //_this.Persist(false);

                            ACPointServiceBase<T, W> spHelper = realServicePoint.ServicePointHelper;
                            if (spHelper != null)
                            {
                                if (spHelper is ACPointServiceReal<T, W>)
                                {
                                    (spHelper as ACPointServiceReal<T, W>).MarkACObjectOfSubscrProxiesOnChangedPoint();
                                }
                            }
                        }
                    }
                }
            }
        }

#endregion
    }
}
