using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    ///   <br />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointClientObject'}de{'ACPointClientObject'}", Global.ACKinds.TACClass)]
    public class ACPointClientObject<T> : ACPointNetStorableObjectBase<T, ACPointNetWrapObject<T>>, IACPointNetClientObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointClientObject()
            : this(null, (ACClassProperty)null, 0)
        {
            _base = new ACPointClientReal<T, ACPointNetWrapObject<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointClientObject(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointClientReal<T, ACPointNetWrapObject<T>>(this);
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointClientObject{T}" /> class.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        public ACPointClientObject(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
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

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointNetWrapObject<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientReal<T, ACPointNetWrapObject<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientReal<T, ACPointNetWrapObject<T>>(this);
                return _base;
            }
        }
        #endregion


        #region IACPointRefNetTo Member

        public IACPointNetBase GetServicePoint(IACObject fromACObject, string fromPointName)
        {
            ACComponent acObject = (ACComponent)fromACObject;
            IACPointNetBase cp = acObject.GetPointNet(fromPointName);
            if (cp == null)
                return null;
            if (!(cp is IACPointNetService<T, ACPointNetWrapObject<T>>))
                return null;
            if (!(cp is IACPointNetServiceObject<T>))
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

        #region IACPointRefNetToObject<T> Member

        public ACPointNetWrapObject<T> AddToServicePoint(IACComponent acObject, string pointName)
        {
            if ((acObject == null) || (ACRef == null) || !ACRef.IsObjLoaded)
                return null;
            if (!(acObject is ACComponent))
                return null;

            ACPointNetBase<T, ACPointNetWrapObject<T>> point = (ACPointNetBase<T, ACPointNetWrapObject<T>>)((ACComponent)acObject).GetPointNet(pointName);
            if (point == null)
                return null;

            return AddToServicePoint((T)acObject, point);
        }

        public ACPointNetWrapObject<T> AddToServicePoint(T acObject, ACPointNetBase<T, ACPointNetWrapObject<T>> point)
        {
            if ((acObject == null) || (ACRef == null) || !ACRef.IsObjLoaded)
                return null;
            if (!(acObject is ACComponent))
                return null;
            if (point == null)
                return null;
            if (!(point is IACPointNetServiceObject<T>))
                return null;

            if (MaxCapacityReached || point.MaxCapacityReached)
                return null;

            ACPointNetWrapObject<T> wrapObject = new ACPointNetWrapObject<T>((T)ACRef.ValueT, point);
            if (wrapObject == null)
                return null;
            wrapObject.ClientPointName = this.ACIdentifier;
            wrapObject.RequestID = Guid.NewGuid();

            _base.PrepareRequestIfSynchronousMode(point);

            try
            {
                if (!((IACPointNetServiceObject<T>)point).Add(wrapObject))
                    return null;

                ACPointNetWrapObject<T> cloneWrapObject = new ACPointNetWrapObject<T>(acObject, this);
                cloneWrapObject.CopyDataOfWrapObject(wrapObject);
                // Broadcast durch OnLocalStorageListChanged():
                if (!this.Add(cloneWrapObject))
                    return null;

                if (_base.WaitOnRequestIfSynchronousMode(point))
                {
                    if (!point.Contains(wrapObject))
                        return null;
                }
            }
            finally
            {
                _base.RemoveRequestIfSynchronousMode(point);
            }
            return wrapObject;
        }

        public bool RemoveFromServicePoint(IACComponent acObject, string pointName)
        {
            if ((acObject == null) || (ACRef == null) || !ACRef.IsObjLoaded)
                return false;
            if (!(acObject is ACComponent))
                return false;

            IACPointNetBase point = ((ACComponent)acObject).GetPointNet(pointName);
            if (point == null)
                return false;
            return RemoveFromServicePoint(acObject, point);
        }

        public bool RemoveFromServicePoint(IACComponent acObject, IACPointNetBase point)
        {
            if ((acObject == null) || (ACRef == null) || !ACRef.IsObjLoaded)
                return false;
            if (!(acObject is ACComponent))
                return false;
            if (point == null)
                return false;
            if (!(point is IACPointNetServiceObject<T>))
                return false;
            ACPointNetWrapObject<T> wrapObject = ((IACPointNetServiceObject<T>)point).GetWrapObject((T)ACRef.ValueT);
            if (wrapObject != null)
            {
                this.Remove(wrapObject);
                return ((IACPointNetServiceObject<T>)point).Remove(wrapObject);
            }
            return false;
        }

        #endregion
    }
}

