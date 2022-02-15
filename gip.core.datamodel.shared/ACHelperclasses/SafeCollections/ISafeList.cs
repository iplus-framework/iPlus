using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface ISafeList : IList
    {
        /// <summary>
        /// Tries to get or set item at specified index within specified timeout. 
        /// </summary>
        /// <param name="index">The zero-based index of the item to get or set.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        /// <remarks>Timeout applies only when item is set.</remarks>
        /// <returns>The item at specified index.</returns>
        object this[int index, int timeout] { get; set; }

        /// <summary>
        /// Tries to add an item to the list within specified timeout.
        /// </summary>
        /// <param name="item">Item to add to collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        int Add(object item, int timeout);

        /// <summary>
        /// Tries to insert an item into the list at the specified index within specified timeout.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The item to add to the collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void Insert(int index, object item, int timeout);

        /// <summary>
        /// Tries to remove an item from the list within specified timeout.
        /// </summary>
        /// <param name="item">Item to remove from collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void Remove(object item, int timeout);

        /// <summary>
        /// Tries to remove an item at the specified index within specified timeout.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void RemoveAt(int index, int timeout);

        /// <summary>
        /// Tries to clear collection within specified timeout.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void Clear(int timeout);
    }

    public interface ISafeList<T> : IList<T>, ISafeCollection<T>
    {
        /// <summary>
        /// Tries to get or set item at specified index within specified timeout. 
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        /// <remarks>Timeout applies only when item is set.</remarks>
        /// <returns>The item at specified index.</returns>
        T this[int index, int timeout] { get; set; }

        /// <summary>
        /// Tries to insert an item into the list at the specified index within specified timeout.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The item to add to the collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void Insert(int index, T item, int timeout);

        /// <summary>
        /// Tries to remove an item at the specified index within specified timeout.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void RemoveAt(int index, int timeout);
    }
}
