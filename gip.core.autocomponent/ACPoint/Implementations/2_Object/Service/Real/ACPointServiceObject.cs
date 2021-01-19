using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointServiceObject'}de{'ACPointServiceObject'}", Global.ACKinds.TACClass)]
    public class ACPointServiceObject<T> : ACPointNetStorableObjectBase<T, ACPointNetWrapObject<T>>, IACPointNetServiceObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointServiceObject()
            : this(null, (ACClassProperty)null, 0)
        {
            _base = new ACPointServiceReal<T,ACPointNetWrapObject<T>>(this);
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointServiceObject(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceReal<T, ACPointNetWrapObject<T>>(this);
        }


        public ACPointServiceObject(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        #region Protected Member
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointNetWrapObject<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceReal<T, ACPointNetWrapObject<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceReal<T, ACPointNetWrapObject<T>>(this);
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

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
            // Es gibt keine Serialisierung oder Deserialisierung von Service-Objekten
        }

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Client-Points (IACPointNetClientObject)
        /// </summary>
        [IgnoreDataMember]
        public override IEnumerable<ACPointNetWrapObject<T>> ConnectionList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                //Base.ConnectionList = value;
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

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }

        #endregion

        #region IACPointNetFrom<T,ACPointRefNetWrapObject<T>> Member

        [IgnoreDataMember]
        public IEnumerable<ACPointNetWrapObject<T>> ConnectionListLocal
        {
            get { return Base.ConnectionListLocal; }
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
        public ACPointServiceBase<T, ACPointNetWrapObject<T>> ServicePointHelper
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
            if (point == this)
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    RemoveAllRejected(false);
                    CopyDataOfLocalStorageToClientPoints(false);
                }
            }
        }

#endregion
    }
}

