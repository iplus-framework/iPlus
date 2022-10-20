using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface ISafeCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Tries to add an item to the collection within specified timeout.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        int Add(T item, int timeout);

        /// <summary>
        /// Tries to remove an item from the collection within specified timeout.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        bool Remove(T item, int timeout);

        /// <summary>
        /// Tries to clear the collection within specified timeout.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait for write lock acquisition.</param>
        /// <exception cref="System.TimeoutException">Write lock not acquired before specified timeout expired.</exception>
        void Clear(int timeout);
    }
}
