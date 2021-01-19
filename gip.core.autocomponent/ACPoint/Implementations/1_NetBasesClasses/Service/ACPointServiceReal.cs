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
    public class ACPointServiceReal<T, W> : ACPointServiceBase<T, W> 
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointServiceReal(ACPointNetStorableBase<T, W> storablePoint)
            : base(storablePoint)
        {
            if (_this.ReStoreFromDB())
            {
                MarkACObjectOfSubscrProxiesOnChangedPoint();
            }
        }
        #endregion

        #region Protected Member

        public virtual void OnLocalStorageListChanged()
        {
            if (_this is IACPointNetService<T, W>)
            {
                (_this as IACPointNetService<T, W>).InvokeSetMethod(_this);
            }

            // Broadcast zu den Clients
            MarkACObjectOfSubscrProxiesOnChangedPoint();
            _this.Persist(true);
        }

        internal void MarkACObjectOfSubscrProxiesOnChangedPoint()
        {
            if (_this.ACRef == null)
                return;
            ACComponent parentACComponent = _this.ParentACComponent as ACComponent;
            if (parentACComponent != null)
            {
                bool marked = false;
                List<ACPSubscrObjService> SubscribedProxyObjects = parentACComponent.SubscribedProxyObjects;
                if (SubscribedProxyObjects != null)
                {
                    foreach (ACPSubscrObjService proxyObj in SubscribedProxyObjects)
                    {
                        IACPointNetBase cpProxy = proxyObj.GetConnectionPoint(_this.ACIdentifier);
                        if (cpProxy != null)
                        {
                            cpProxy.PointChangedForBroadcast = true;
                            marked = true;
                        }
                    }
                }
                if (marked)
                    MarkACObjectOnChangedPoint(parentACComponent);
            }
            _this.OnPropertyChangedLists();
        }

        internal override void MarkACObjectOnChangedPoint(ACComponent parentACComponent = null)
        {
            if (parentACComponent == null)
                parentACComponent = _this.ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return;
            parentACComponent.Root.MarkACObjectOnChangedPointForClient(parentACComponent);
        }


        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Client-Points (IACPointNetClientObject)
        /// </summary>
        public IEnumerable<W> ConnectionList
        {
            get
            {
                List<W> mergedList = new List<W>();
                if (_this.LocalStorage != null)
                {

                    using (ACMonitor.Lock(_this.LockLocalStorage_20033))
                    {
                        if (_this.LocalStorage.Any())
                        {
                            mergedList.AddRange(_this.LocalStorage);
                        }
                    }
                }
                ACComponent parentACComponent = _this.ParentACComponent as ACComponent;
                if (parentACComponent != null)
                {
                    List<ACPSubscrObjService> SubscribedProxyObjects = parentACComponent.SubscribedProxyObjects;
                    foreach (ACPSubscrObjService proxyObj in SubscribedProxyObjects)
                    {
                        IACPointNetBase cpProxy = proxyObj.GetConnectionPoint(_this.ACIdentifier);
                        if (cpProxy != null)
                        {
                            if (cpProxy is IACPointNetService<T,W>)
                            {
                                IEnumerable<W> proxyList = (IEnumerable<W>)(cpProxy as IACPointNetService<T, W>).ConnectionListLocal;
                                if ((proxyList != null) && (proxyList.Any()))
                                    mergedList.AddRange(proxyList);
                            }
                        }
                    }
                }
                return mergedList;
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
        }

#endregion
    }
}
