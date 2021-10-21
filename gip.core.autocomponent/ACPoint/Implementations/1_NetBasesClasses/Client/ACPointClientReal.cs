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
    public class ACPointClientReal<T, W> : ACPointClientBase<T, W> 
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointClientReal(ACPointNetStorableBase<T, W> storablePoint)
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
            if (_this is IACPointNetClient)
            {
                (_this as IACPointNetClient).InvokeSetMethod(_this);
            }

            // Broadcast zu den Clients
            MarkACObjectOfSubscrProxiesOnChangedPoint();
            _this.Persist(true);
        }

        internal void MarkACObjectOfSubscrProxiesOnChangedPoint()
        {
            ACComponent acComponent = _this.ParentACComponent as ACComponent;
            if (acComponent != null)
            {
                bool marked = false;
                List<ACPSubscrObjService> SubscribedProxyObjects = acComponent.SubscribedProxyObjects;
                foreach (ACPSubscrObjService proxyObj in SubscribedProxyObjects)
                {
                    IACPointNetBase cpProxy = proxyObj.GetConnectionPoint(_this.ACIdentifier);
                    if (cpProxy != null)
                    {
                        cpProxy.PointChangedForBroadcast = true;
                        marked = true;
                    }
                }
                if (marked)
                    MarkACObjectOnChangedPoint(acComponent);
            }
            _this.OnPropertyChangedLists();
        }

        internal override void MarkACObjectOnChangedPoint(ACComponent acComponent = null)
        {
            if (acComponent == null)
                acComponent = _this.ParentACComponent as ACComponent;
            if (acComponent == null)
                return;
            acComponent.Root.MarkACObjectOnChangedPointForClient(acComponent);
        }

        internal bool PrepareRequestIfSynchronousMode(IACPointNetBase servicePoint)
        {
            if (servicePoint == null)
                return false;
            if (!_this.SynchronousMode)
                return false;
            if (!(servicePoint is ACPointNetBase<T, W>))
                return false;
            ACPointNetBase<T, W> point = servicePoint as ACPointNetBase<T, W>;
            if (!(point.ACRef.ValueT is ACComponentProxy))
                return false;
            point._CurrentSyncRequest = (point.ACRef.ValueT as ACComponentProxy).RMInvoker.NewSynchronousRequest();
            return (point._CurrentSyncRequest != null);
        }

        internal void RemoveRequestIfSynchronousMode(IACPointNetBase servicePoint)
        {
            if (servicePoint == null)
                return;
            if (!_this.SynchronousMode)
                return;
            if (!(servicePoint is ACPointNetBase<T, W>))
                return;
            ACPointNetBase<T, W> point = servicePoint as ACPointNetBase<T, W>;
            point._CurrentSyncRequest = null;
        }

        internal bool WaitOnRequestIfSynchronousMode(IACPointNetBase servicePoint)
        {
            if (servicePoint == null)
                return false;
            if (!_this.SynchronousMode)
                return false;
            if (!(servicePoint is ACPointNetBase<T, W>))
                return false;
            ACPointNetBase<T, W> point = servicePoint as ACPointNetBase<T, W>;
            if (point._CurrentSyncRequest == null)
                return false;
            if (!(point.ACRef.ValueT is ACComponentProxy))
                return false;

            if ((point.ACRef.ValueT as ACComponentProxy).RMInvoker.WaitOnSynchronousRequest(point._CurrentSyncRequest))
            {
                point._CurrentSyncRequest = null;
                return true;
            }
            point._CurrentSyncRequest = null;
            return false;
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
