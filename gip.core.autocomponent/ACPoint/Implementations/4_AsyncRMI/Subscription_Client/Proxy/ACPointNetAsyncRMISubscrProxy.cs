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
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetAsyncRMISubscrProxy'}de{'ACPointNetAsyncRMISubscrProxy'}", Global.ACKinds.TACAbstractClass)]
    internal abstract class ACPointNetAsyncRMISubscrProxy<T> : ACPointNetStorableAsyncRMISubscrBase<T, ACPointAsyncRMISubscrWrap<T>>, IACPointAsyncRMISubscr<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetAsyncRMISubscrProxy()
        {
            _base = new ACPointClientProxy<T, ACPointAsyncRMISubscrWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetAsyncRMISubscrProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base (parent, acClassProperty, maxCapacity)
        {
            _base = new ACPointClientProxy<T, ACPointAsyncRMISubscrWrap<T>>(this);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointAsyncRMISubscrWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointAsyncRMISubscrWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientProxy<T, ACPointAsyncRMISubscrWrap<T>>(this);
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

        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            Base.OnPointReceivedRemote(receivedPoint);
        }

        /// <summary>
        /// List of ACPointAsyncRMISubscrWrap-relations to other RMI-Points (IACPointAsyncRMI)
        /// </summary>
        [IgnoreDataMember]
        public override IEnumerable<ACPointAsyncRMISubscrWrap<T>> ConnectionList
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
        private IEnumerable<ACPointAsyncRMISubscrWrap<T>> SerConnList
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

        #endregion

        #region IACPointAsyncRMISubscr<T> Member

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            return InvokeAsyncMethod(atACComponent, asyncRMIPointName, acMethod, AsyncCallbackDelegate, !PropertyInfo.IsPersistable);
        }

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, ACPointNetEventDelegate AsyncCallbackDelegate, bool AutoRemove)
        {
            // Nur in Realen-Objekten erlaubt
            throw new NotImplementedException();
        }

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, string asyncCallbackDelegateName)
        {
            return InvokeAsyncMethod(atACComponent, asyncRMIPointName, acMethod, asyncCallbackDelegateName, !PropertyInfo.IsPersistable);
        }

        public ACPointAsyncRMISubscrWrap<T> InvokeAsyncMethod(IACComponent atACComponent, string asyncRMIPointName, ACMethod acMethod, string asyncCallbackDelegateName, bool AutoRemove)
        {
            // Nur in Realen-Objekten erlaubt
            throw new NotImplementedException();
        }

        #endregion

        #region IACPointNetClient Member

        public IACPointNetBase GetServicePoint(IACObject fromACComponent, string fromPointName)
        {
            // Nur in Realen-Objekten erlaubt
            throw new NotImplementedException();
        }

        public void InvokeSetMethod(IACPointNetBase point)
        {
            if (this.SetMethod != null)
                this.SetMethod(point);
        }
        #endregion
    }
}

