using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNet<T, W> : IACPoint<W>, IACPointNetBase
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
        IEnumerable<T> RefObjectList { get; }

        /// <summary>
        /// Returns a "WrapObject". LocalStorage is search by two steps:
        /// 1. Lookup by comparing Object-Instance (Pointer). If Not succeed, step 2:
        /// 2. Comparing is done over IComparable-Interface by comparing Properties
        /// </summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns>Returns a "WrapObject"</returns>
        W GetWrapObject(W cloneOrOriginal);

        /// <summary>Checks if a "wrapObject" exists in the List
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.</summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool Contains(W cloneOrOriginal);

        /// <summary>
        /// Removes a "wrapObject" from the list including it's wrapped "refObject" 
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="cloneOrOriginal">A wrapObject which is the Original Instance or a clone with same Properties</param>
        /// <returns>returns true if object existed and was removed</returns>
        bool Remove(W cloneOrOriginal);

        bool MaxCapacityReached { get; }
    }
}

