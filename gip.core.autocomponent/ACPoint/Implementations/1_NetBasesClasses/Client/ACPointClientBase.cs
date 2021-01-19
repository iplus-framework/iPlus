using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class for implementing Real- or Proxy-Implementations which holds the "wrapObjects"(Wrapper) in a local List.
    /// All in ACPointRefNetBase declared abstract methods operates on this local storage list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ACPointClientBase<T, W>
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointClientBase(ACPointNetStorableBase<T, W> storablePoint)
        {
            _this = storablePoint;
        }   
        #endregion

        #region Protected Member
        protected ACPointNetStorableBase<T, W> _this = null;

        internal abstract void MarkACObjectOnChangedPoint(ACComponent acComponent = null);

        #endregion

        #region IACConnectionPointBase Member
        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public virtual void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
        }

        #endregion
    }
}
