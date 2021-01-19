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
    public class ACPointClientProxy<T, W> : ACPointClientBase<T, W> 
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointClientProxy(ACPointNetStorableBase<T, W> storablePoint)
            : base(storablePoint)
        {
        }
        #endregion

        #region Protected Member
        public virtual void OnLocalStorageListChanged()
        {
            // Client-Seitige Änderungen sind nicht möglich
        }

        internal override void MarkACObjectOnChangedPoint(ACComponent acComponent = null)
        {
            if (acComponent == null)
                acComponent = _this.ParentACComponent as ACComponent;
            if (acComponent == null)
                return;
            _this.PointChangedForBroadcast = true;
            if (IsInstanceOnServerSide)
            {
                acComponent.Root.MarkACObjectOnChangedPointForClient(acComponent);
            }
        }

        private bool _IsSubscribed = false;
        internal void Subscribe(bool force)
        {
            if (IsInstanceOnServerSide)
                return;
            if ((_this.ACRef == null) || !_this.ACRef.IsObjLoaded || (_this.ACRef.ValueT.Root == null))
                return;
            if (force || (!force && !_IsSubscribed))
            {
                //if (force)
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
            W[] storageCopy = null;

            using (ACMonitor.Lock(_this.LockLocalStorage_20033))
            {
                storageCopy = _this.LocalStorage.ToArray();
            }
            if (parentSubscrObject != null)
            {
                if (parentSubscrObject is ACPSubscrObjService)
                    _this.ParentSubscrObject = (ACPSubscrObjService)parentSubscrObject;
                if (storageCopy != null && storageCopy.Any())
                {
                    foreach (var w in storageCopy)
                    {
                        w.Point = _this;
                    }
                }
            }
            // Called on Client-Side
            else
            {
                if (storageCopy != null && storageCopy.Any())
                {
                    foreach (var w in storageCopy)
                    {
                        w.Point = _this;
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

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Service-Points (IACPointNetServiceObject)
        /// </summary>
        public IEnumerable<W> ConnectionList
        {
            get
            {
                // Falls Point, der Serverseitig am ACPSubscrObjService hängt
                if (IsInstanceOnServerSide)
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
                // Falls Point, der am Proxy-Objekt (Client-Seite) hängt
                else
                {
                    return _this.LocalStorage;
                }
                return new List<W>();
            }
            set
            {
                _this.LocalStorage = value.ToList();
            }
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
                _this.OnPropertyChangedLists();
            }
            // Sonst Point, der Serverseitig am ACPSubscrObjService hängt
            // Aufgerufen von ACPSubscrObjService.UpdateConnectionPoints()
            else
            {
                // Aktualisierungen vom Proxy-Client sind nicht zulässig

                // Sende aktuelle Daten zurück zum Client
                MarkACObjectOnChangedPoint();
            }
        }

#endregion
    }
}
