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
    internal abstract class ACPointNetEventSubscrProxy<T> : ACPointNetStorableEventSubscrBase<T, ACPointEventSubscrWrap<T>>, IACPointEventSubscr<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetEventSubscrProxy()
        {
            _base = new ACPointClientProxy<T, ACPointEventSubscrWrap<T>>(this);
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetEventSubscrProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
            _base = new ACPointClientProxy<T, ACPointEventSubscrWrap<T>>(this);
        }
        #endregion

        #region protected Member
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointEventSubscrWrap<T>> _base;
        [IgnoreDataMember]
        protected ACPointClientProxy<T, ACPointEventSubscrWrap<T>> Base
        {
            get
            {
                if (_base == null)
                    _base = new ACPointClientProxy<T, ACPointEventSubscrWrap<T>>(this);
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
        /// List of ACPointEventSubscrWrap-relations to other Event-Points (IACPointEvent)
        /// </summary>
        [IgnoreDataMember]
        public override IEnumerable<ACPointEventSubscrWrap<T>> ConnectionList
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
        private IEnumerable<ACPointEventSubscrWrap<T>> SerConnList
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


        #region IACPointEventSubscr<T> Member

        public ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, string asyncCallbackDelegateName)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool SubscribeAllEvents(IACComponent atACComponent, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool UnSubscribeEvent(IACComponent atACComponent, string eventName)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool UnSubscribeAllEvents(IACComponent atACComponent)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool UnSubscribeAllEvents()
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool Contains(string atACUrl, string eventName)
        {
            // Nur in Realen-Objekten können Events abboniert werden
            throw new NotImplementedException();
        }

        public bool Contains(IACComponent atACComponent, ACPointNetEventDelegate AsyncCallbackDelegate)
        {
            // Nur in Realen-Objekten können Events abboniert werden
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

