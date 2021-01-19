using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableAsyncRMISubscrBase'}de{'ACPointNetStorableAsyncRMISubscrBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableAsyncRMISubscrBase<T, W> : ACPointNetStorableBase<T, W>
        where W : ACPointAsyncRMISubscrWrap<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableAsyncRMISubscrBase()
            : this(null, null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointNetStorableAsyncRMISubscrBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _LocalStorage = new List<W>();
            ReStoreFromDB();
        }
        #endregion

        #region IACConnectionPoint<ACPointRefNetWrapAsyncRMISubscription<T>> Member

        /// <summary>
        /// List of ACPointAsyncRMISubscrWrap-relations to other RMI-Points (IACPointAsyncRMI)
        /// </summary>
        public virtual new IEnumerable<ACPointAsyncRMISubscrWrap<T>> ConnectionList
        {
            get 
            { 
                return (IEnumerable<ACPointAsyncRMISubscrWrap<T>>)_LocalStorage; 
            }
            set
            {
            }
        }

        #endregion
    }

}

