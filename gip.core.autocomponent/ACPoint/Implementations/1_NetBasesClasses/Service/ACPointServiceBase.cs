// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
    /// <typeparam name="W"></typeparam>
    public abstract class ACPointServiceBase<T, W>
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
       
        #region c'tors
        public ACPointServiceBase(ACPointNetStorableBase<T, W> storablePoint)
        {
            _this = storablePoint;
        }   
        #endregion

        #region Protected Member
        protected ACPointNetStorableBase<T, W> _this = null;

        internal abstract void MarkACObjectOnChangedPoint(ACComponent parentACComponent = null);

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

        #region IACPointNetService<T,W> Member

        public IEnumerable<W> ConnectionListLocal
        {
            get { return _this.LocalStorage; }
        }

        public IEnumerable<T> RefObjectListLocal
        {
            get
            {
                if (_this.LocalStorage == null)
                    return null;
                return _this.LocalStorage.Select(c => c.ValueT);
            }
        }

        #endregion
    }
}
