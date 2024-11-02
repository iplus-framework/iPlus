// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    ///   <br />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    internal class ACPointServiceObjectProxy<T> : ACPointNetStorableObjectBase<T, ACPointNetWrapObject<T>>, IACPointNetServiceObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>Constructor for Deserializer</summary>
        public ACPointServiceObjectProxy()
            : this(null, null, 0)
        {
            _base = new ACPointServiceProxy<T, ACPointNetWrapObject<T>>(this, true);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointServiceObjectProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _base = new ACPointServiceProxy<T, ACPointNetWrapObject<T>>(this, false);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointNetWrapObject<T>> _base;
        [IgnoreDataMember]
        protected ACPointServiceProxy<T, ACPointNetWrapObject<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointServiceProxy<T, ACPointNetWrapObject<T>>(this, true);
                return _base;
            }
        }
        #endregion

        #region Override Member
        public override void Subscribe(bool force = true)
        {
            Base.Subscribe(force);
        }

        public override void ReSubscribe()
        {
            Base.Subscribe(false);
        }

        public override void UnSubscribe()
        {
            Base.UnSubscribe();
        }

        protected override void OnLocalStorageListChanged()
        {
            // Trage im Dispatcher zum Versand ein.
            Base.OnLocalStorageListChanged();
        }

        public override void RebuildAfterDeserialization(object parentSubscrObject)
        {
            Base.RebuildAfterDeserialization(parentSubscrObject);
        }

        /// <summary>
        /// List of ACPointNetWrapObject-relations to other Client-Points (IACPointNetClientObject)
        /// </summary>
        public override IEnumerable<ACPointNetWrapObject<T>> ConnectionList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                Base.ConnectionList = value;
            }
        }


        public override string ConnectionListInfo
        {
            get
            {
                Subscribe(false); // Bindings to Tooltips invokes cyclic this Method. To reduce the serverside cpu-usage the subscription-Request should only take one time.
                return base.ConnectionListInfo;
            }
        }

        /// <summary>
        /// Member für Serialisierung, weil ConnectionList nicht serialisiert werden kann,
        /// weil das Interface nur eine get-Anweisung enhält
        /// </summary>
        [DataMember]
        private IEnumerable<ACPointNetWrapObject<T>> SerConnList
        {
            get
            {
                return Base.ConnectionList;
            }
            set
            {
                Base.ConnectionList = value;
            }
        }

        [IgnoreDataMember]
        public new IEnumerable<T> RefObjectList
        {
            get
            {
                return Base.RefObjectList;
            }
        }

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint"></param>
        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }

        #endregion

        #region IACPointRefNetFrom<T,ACPointRefNetWrapObject<T>> Member

        [IgnoreDataMember]
        public IEnumerable<ACPointNetWrapObject<T>> ConnectionListLocal
        {
            get { return Base.ConnectionListLocal; }
        }

        [IgnoreDataMember]
        public IEnumerable<T> RefObjectListLocal
        {
            get
            {
                return Base.RefObjectListLocal;
            }
        }

        /// <summary>
        /// Reference to Extension-Class, which implements this interface (workaround for multiple inheritance)
        /// </summary>
        [IgnoreDataMember]
        public ACPointServiceBase<T, ACPointNetWrapObject<T>> ServicePointHelper
        {
            get
            {
                return Base;
            }
        }

        public void InvokeSetMethod(IACPointNetBase point)
        {
            if (this.SetMethod != null)
                this.SetMethod(point);
        }

        #endregion
    }
}

