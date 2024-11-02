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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointStorableBase'}de{'ACPointStorableBase'}", Global.ACKinds.TACClass)]
    public abstract class ACPointStorableBase<T> : ACPointBase<T> where T : IACObject 
    {
        #region c'tors
        public ACPointStorableBase(IACComponent parent, string acPropertyName, uint maxCapacity)
            : base(parent, acPropertyName, maxCapacity)
        {
            InitLockConnectionList();
        }

        /// <summary>
        /// Constructor for automatic Instantiation over Reflection in ACInitACPoint()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointStorableBase(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            InitLockConnectionList();
        }
        #endregion

        #region Own Member

        /// <summary>
        /// List of relations to other objects.
        /// It's use to describe the relationships to other objects.
        /// </summary>
        public override IEnumerable<T> ConnectionList
        {
            get
            {
                using (ACMonitor.Lock(LockConnectionList_20030))
                {
                    return UnsafeConnectionList.ToArray();
                }
            }
        }

        protected List<T> _ConnectionList;
        protected List<T> UnsafeConnectionList
        {
            get
            {
                if (_ConnectionList == null)
                    _ConnectionList = new List<T>();
                return _ConnectionList;
            }
        }

        private ACMonitorObject _20030_LockConnectionList = null;
        /// <summary>
        /// Lock for manipulation and querying of connectionlist
        /// </summary>
        protected ACMonitorObject LockConnectionList_20030
        {
            get
            {
                if (_20030_LockConnectionList != null)
                    return _20030_LockConnectionList;
                InitLockConnectionList();
                return _20030_LockConnectionList;
            }
        }

        private void InitLockConnectionList()
        {
            if (_20030_LockConnectionList == null)
                _20030_LockConnectionList = new ACMonitorObject(20030);
        }


        public virtual bool Contains(T item)
        {

            using (ACMonitor.Lock(LockConnectionList_20030))
            {
                return UnsafeConnectionList.Contains(item);
            }
        }

        public virtual void Add(T item)
        {

            using (ACMonitor.Lock(LockConnectionList_20030))
            {
                if (!UnsafeConnectionList.Contains(item))
                {
                    UnsafeConnectionList.Add(item);
                }
            }
        }

        public virtual bool Remove(T item)
        {

            using (ACMonitor.Lock(LockConnectionList_20030))
            {
                if (UnsafeConnectionList == null)
                    return false;
                return UnsafeConnectionList.Remove(item);
            }
        }

#endregion

    }
}
