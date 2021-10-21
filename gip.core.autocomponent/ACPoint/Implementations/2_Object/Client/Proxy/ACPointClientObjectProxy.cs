using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    internal class ACPointClientObjectProxy<T> : ACPointNetStorableObjectBase<T, ACPointNetWrapObject<T>>, IACPointNetClientObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointClientObjectProxy()
        {
            _base = new ACPointClientProxy<T, ACPointNetWrapObject<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointClientObjectProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _base = new ACPointClientProxy<T, ACPointNetWrapObject<T>>(this);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointNetWrapObject<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointNetWrapObject<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientProxy<T, ACPointNetWrapObject<T>>(this);
                return _base;
            }
        }
        #endregion

        #region IACPointNetClientObject<T> Member

        public ACPointNetWrapObject<T> AddToServicePoint(IACComponent acObject, string pointName)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFromServicePoint(IACComponent acObject, string pointName)
        {
            throw new NotImplementedException();
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
        /// List of ACPointNetWrapObject-relations to other Service-Points (IACPointNetServiceObject)
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

        #endregion

        #region IACPointNetClient Member

        public IACPointNetBase GetServicePoint(IACObject fromACObject, string fromPointName)
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

        /// <summary>
        /// Removes a "refObject" from the local storage list including it's wrapping "wrapObject" 
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns>returns true if object existed and was removed</returns>
        public override bool Remove(T refObject)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the "wrapObject" from the local storage list including it's wrapped "refObject" 
        /// </summary>
        /// <param name="acURL">Addressing over acURL</param>
        /// <returns>returns true if object existed and was removed</returns>
        public override bool Remove(string acURL)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }


        /// <summary>
        /// Adds a "wrapObject" which contains a "refObject" to the List
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        public override bool Add(ACPointNetWrapObject<T> wrapObject)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a "refObject" to the List by creating a ACPointRefNetWrapObject-Instance and wrapping the "refObject".
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns></returns>
        public override ACPointNetWrapObject<T> Add(T refObject)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }

        /// <summary>
        /// Appends a List of "refObjects" to the local storage list by automatically creating ACPointRefNetWrapObject-Instances and wrapping the "refObjects".
        /// </summary>
        /// <param name="refObjects"></param>
        public override void Add(IEnumerable<T> refObjects)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces the local storage List with the passed List of "refObjects" by automatically creating ACPointRefNetWrapObject-Instances and wrapping the "refObjects".
        /// </summary>
        /// <param name="refObjects"></param>
        public override void Set(IEnumerable<T> refObjects)
        {
            // In Proxy-Objekten keine Änderungsmöglichkeiten vom Client
            throw new NotImplementedException();
        }
    }
}

