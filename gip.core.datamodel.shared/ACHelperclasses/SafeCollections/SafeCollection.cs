using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [Serializable]
    //[System.Runtime.InteropServices.ComVisible(false)]
    //[DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    //[DebuggerDisplay("Count = {Count}")]
    public class SafeCollection<T> : ISafeCollection<T>, ICollection
    {
        private readonly string _ExSource = "gip.core.datamodel.SafeCollection<" + typeof(T).Name + ">.";

        protected List<T> _Items;

        [NonSerialized]
        private ReaderWriterLockSlim _RWLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        protected virtual ReaderWriterLockSlim RWLock
        {
            get
            {
                return _RWLock;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class that is empty.
        /// </summary>
        public SafeCollection()
        {
            _Items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the class as a wrapper for the specified list.
        /// </summary>
        /// <param name="items">The list that is wrapped by the new collection.</param>
        public SafeCollection(IEnumerable<T> items)
        {
            _Items = new List<T>(items);
        }

        #endregion

        #region ICollection

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread safe).
        /// </summary>
        /// <returns>True if access to the collection is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        /// <returns>An object that can be used to synchronize access to the collection.</returns>
        public object SyncRoot
        {
            get { return RWLock; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <returns>True if the collection is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return (_Items as IList).IsReadOnly; }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <returns>The number of elements contained in the collection.</returns>
        public int Count
        {
            get
            {
                RWLock.EnterReadLock();
                try
                {
                    return _Items.Count;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
        }

        public void Add(T item)
        {
            this.Add(item, -1);
        }

        public bool Remove(T item)
        {
            return this.Remove(item, -1);
        }

        public void Clear()
        {
            this.Clear(-1);
        }


        public bool Contains(T item)
        {
            RWLock.EnterReadLock();
            try
            {
                return _Items.Contains(item);
            }
            finally
            {
                RWLock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            RWLock.EnterReadLock();
            try
            {
                _Items.CopyTo(array, index);
            }
            finally
            {
                RWLock.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeCollection<T>.Enumerator(this);
        }

        #region Private

        void ICollection.CopyTo(Array array, int index)
        {
            //this.CopyTo((T[])Array, ArrayIndex); 
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("Arg_RankMultiDimNotSupported");
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Arg_NonZeroLowerBound");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            }

            T[] tArray = array as T[];
            if (tArray != null)
            {
                CopyTo(tArray, index);
            }
            else
            {
                // 
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though. 
                // For example, if the element type of the Array is derived from T, 
                // we can't figure out if we can successfully copy the element beforehand.
                // 
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                {
                    throw new ArgumentException("Argument_InvalidArrayType");
                }

                // 
                // We can't cast array of value type to object[], so we don't support
                // widening of primitive types here. 
                //
                object[] objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException("Argument_InvalidArrayType");
                }

                RWLock.EnterReadLock();
                try
                {
                    int count = _Items.Count;
                    for (int i = 0; i < count; i++)
                    {
                        objects[index++] = _Items[i];
                    }
                }
                catch (ArrayTypeMismatchException e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

#if NETFRAMEWORK
                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("SafeCollection", "ICollection.CopyTo", msg);
#endif

                    throw new ArgumentException("Argument_InvalidArrayType");
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

#endregion

#endregion

#region ISafeCollection

        public virtual int Add(T item, int timeout)
        {
            if ((_Items as IList).IsReadOnly)
                throw new NotSupportedException("NotSupported_ReadOnlyCollection");

            if (!RWLock.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "InsertItem" };
            int index = 0;
            try
            {
                index = _Items.Count;
                InsertItem(index, item, timeout, false);
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
            return index;
        }

        public virtual bool Remove(T item, int timeout)
        {
            if ((_Items as IList).IsReadOnly)
                throw new NotSupportedException("NotSupported_ReadOnlyCollection");

            if (!RWLock.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "InsertItem" };
            int index = -1;
            try
            {
                index = _Items.IndexOf(item);
                RemoveItem(index, timeout, false);
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

            return index >= 0;
        }

        public virtual void Clear(int timeout)
        {
            if ((_Items as IList).IsReadOnly)
                throw new NotSupportedException("NotSupported_ReadOnlyCollection");

            ClearItems();
        }

#endregion

#region protected
        protected virtual void ClearItems()
        {
            if (!RWLock.TryEnterWriteLock(-1))
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "ClearItems" };
            try
            {
                _Items.Clear();
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        protected virtual void RemoveItem(int index, int timeout, bool withlock = true)
        {
            if (withlock && !RWLock.TryEnterWriteLock(timeout)) 
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "RemoveItem" };
            try
            {
                _Items.RemoveAt(index);
            }
            finally
            {
                if (withlock)
                    RWLock.ExitWriteLock();
            }
        }

        protected virtual void InsertItem(int index, T item, int timeout, bool withlock = true)
        {
            if (withlock && !RWLock.TryEnterWriteLock(timeout)) 
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "InsertItem" };
            try
            {
                _Items.Insert(index, item);
            }
            finally
            {
                if (withlock)
                    RWLock.ExitWriteLock();
            }
        }

        protected virtual void SetItem(int index, T item, int timeout)
        {
            if (!RWLock.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "RemoveItem" };
            try
            {
                _Items[index] = item;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }


#endregion

#region Enumerator

        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {
            private int _Index;
            private T _Current;
            private SafeCollection<T> _Host;

            public T Current
            {
                get { return _Current; }
            }

            internal Enumerator(SafeCollection<T> host)
            {
                _Index = 0;
                _Current = default(T);
                _Host = host;

                EnterReadLock();
            }

            public bool MoveNext()
            {
                EnterReadLock();
                if (_Index < _Host._Items.Count)
                {
                    _Current = _Host._Items[_Index];
                    _Index++;

                    return true;
                }
                else
                {
                    _Current = default(T);
                    ExitReadLock();
                    return false;
                }
            }

            public void Reset()
            {
                _Index = 0;
                _Current = default(T);
                ExitReadLock();
            }

            public void Dispose()
            {
                ExitReadLock();
            }

#region Private

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            private void EnterReadLock()
            {
                if (!_Host.RWLock.IsReadLockHeld)
                    _Host.RWLock.EnterReadLock();
            }

            private void ExitReadLock()
            {
                if (_Host.RWLock.IsReadLockHeld)
                {
                    try
                    {
                        _Host.RWLock.ExitReadLock();
                    }
                    catch (SynchronizationLockException e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

#if NETFRAMEWORK
                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("SafeCollection", "ExitReadLock", msg);
#endif
                    }
                }
            }

#endregion
        }

#endregion

    }
}
