// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [Serializable]
    //[System.Runtime.InteropServices.ComVisible(false)]
    //[DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    //[DebuggerDisplay("Count = {Count}")]
    public class SafeList<T> : SafeCollection<T>, ISafeList<T>, ISafeList, IList<T>, IList
    {
        /// <summary>
        /// TODO Compare:
        /// http://reflector.webtropy.com/default.aspx/4@0/4@0/DEVDIV_TFS/Dev10/Releases/RTMRel/ndp/clr/src/BCL/System/Collections/ObjectModel/Collection@cs/1305376/Collection@cs
        /// </summary>
        #region Constructors

        private readonly string _ExSource = "gip.core.datamodel.SafeList<" + typeof(T).Name + ">.";

        public SafeList()
            : base()
        {
        }

        public SafeList(IEnumerable<T> Items)
            : base(Items)
        {
        }

        public SafeList(int Capacity)
        {
            _Items = new List<T>(Capacity);
        }

        #endregion

        #region IList

        public T this[int index]
        {
            get { return this[index, -1]; }
            set { this[index, -1] = value; }
        }

        public void Insert(int index, T item)
        {
            this.Insert(index, item, -1);
        }

        public void RemoveAt(int index)
        {
            this.RemoveAt(index, -1);
        }

        public int RemoveAll(Predicate<T> match)
        {
            return this.RemoveAll(match, -1);
        }

        public int IndexOf(T Item)
        {
            RWLock.EnterReadLock();
            try
            {
                return _Items.IndexOf(Item);
            }
            finally
            {
                RWLock.ExitReadLock();
            }
        }

        #region Private 

        bool IList.IsFixedSize
        {
            get
            {                 
                // There is no IList<t>.IsFixedSize, so we must assume that only 
                // readonly collections are fixed size, if our internal item 
                // collection does not implement IList.  Note that Array implements
                // IList, and therefore T[] and U[] will be fixed-size. 
                IList list = _Items as IList;
                if (list != null)
                {
                    return list.IsFixedSize;
                }
                return list.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get { return this[index, -1]; }
            set { this[index, -1] = (T)value; }
        }

        int IList.Add(object item)
        {
            return ((ISafeList)this).Add(item, -1);
        }

        void IList.Remove(object item)
        {
            if (IsCompatibleObject(item))
            {
                base.Remove((T)item, -1);
            }
        }

        void IList.Insert(int index, object item)
        {
            this.Insert(index, (T)item, -1);
        }

        int IList.IndexOf(object item)
        {
            if (IsCompatibleObject(item))
            {
                return this.IndexOf((T)item);
            }
            return -1;
        }

        bool IList.Contains(object item)
        {
            if (IsCompatibleObject(item))
            {
                return this.Contains((T)item);
            }
            return false;
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<u>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<u>. 
            return ((value is T) || (value == null && default(T) == null));
        } 
        #endregion

        #endregion

        #region ISafeList

        public virtual T this[int index, int timeout]
        {
            get
            {
                RWLock.EnterReadLock();
                try
                {
                    return _Items[index];
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
            set
            {
                if ((_Items as IList).IsReadOnly)
                    throw new NotSupportedException("NotSupported_ReadOnlyCollection");
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();

                SetItem(index, value, timeout);
            }
        }

        public virtual void Insert(int index, T item, int timeout)
        {
            if ((_Items as IList).IsReadOnly)
                throw new NotSupportedException("NotSupported_ReadOnlyCollection");
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();
            InsertItem(index, item, timeout);
        }

        public virtual void RemoveAt(int index, int timeout)
        {
            RemoveItem(index, timeout);
        }

        public virtual int RemoveAll(Predicate<T> match, int timeout)
        {
            if (!RWLock.TryEnterWriteLock(timeout)) 
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "RemoveAll" };
            try
            {
                return _Items.RemoveAll(match);
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        #region Private

        object ISafeList.this[int index, int timeout]
        {
            get { return this[index, timeout]; }
            set { this[index, timeout] = (T)value; }
        }

        int ISafeList.Add(object item, int timeout)
        {
            int index = Add((T)item, timeout);
            return index;
        }

        void ISafeList.Insert(int index, object item, int timeout)
        {
            this.Insert(index, (T)item, timeout);
        }

        void ISafeList.Remove(object item, int timeout)
        {
            this.Remove((T)item, timeout);
        }

        #endregion

        #endregion

    }
}
