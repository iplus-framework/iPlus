using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableEventSubscrBase'}de{'ACPointNetStorableEventSubscrBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableEventSubscrBase<T, W> : ACPointNetStorableBase<T, W>
        where W : ACPointEventSubscrWrap<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableEventSubscrBase()
            : this(null, null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointNetStorableEventSubscrBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _LocalStorage = new List<W>();
            ReStoreFromDB();
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
        #endregion

        #region IACConnectionPoint<ACPointRefNetWrapEventSubscription<T>> Member

        /// <summary>
        /// List of ACPointEventSubscrWrap-relations to other Event-Points (IACPointEvent)
        /// </summary>
        public virtual new IEnumerable<ACPointEventSubscrWrap<T>> ConnectionList
        {
            get 
            { 
                return (IEnumerable<ACPointEventSubscrWrap<T>>)_LocalStorage; 
            }

            set
            {
            }
        }

        #endregion
    }
}

