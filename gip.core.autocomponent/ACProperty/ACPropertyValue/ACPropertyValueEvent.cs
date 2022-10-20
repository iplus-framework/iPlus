using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    internal interface IACPropertyNetValueEventExt
    {
        void SetACUrl(string acUrl = "");
        IACPropertyNetValueEvent Clone();
        IACPropertyNetValueEvent OriginalEventIfClone { get; }
    }

    //[DataContract(Name = "ACPVE")]
    [DataContract]
    //[KnownType("GetKnownType")]
    public class ACPropertyValueEvent<T> : ACPropertyValueBase<T>, IACPropertyNetValueEvent, IACPropertyNetValueEventExt
    {
        // Default-Konstruktor nur für DataContractSerializer!!!
        public ACPropertyValueEvent()
        {
        }

        public ACPropertyValueEvent(EventTypes eventType, EventRaiser sender, IACComponent forACComponent, IACType acTypeInfo, object invokerInfo=null)
            : this(null, eventType, sender, forACComponent, acTypeInfo, invokerInfo)
        {
        }

        public ACPropertyValueEvent(ACPropertyValueEvent<T> forRequest, EventTypes eventType, EventRaiser sender, IACComponent forACComponent, object invokerInfo=null)
            : this(forRequest, eventType, sender, forACComponent, null, invokerInfo)
        {
        }

        public ACPropertyValueEvent(ACPropertyValueEvent<T> forRequest, EventTypes eventType, EventRaiser sender, IACComponent forACComponent, IACType acTypeInfo, object invokerInfo=null)
            : base()
        {
            // Kann nur einmal vom Ersteller gesetzt werden und darf von niemanden verfälscht werden.
            _Sender = sender;
            _EventType = eventType;
            //IsWorkflowEvent = false;
            if (forACComponent != null)
            {
                _ACUrl = forACComponent.ACUrl;
                /*if ((forACComponent.ACClass != null) && forACComponent.ACClass.IsWorkflowType)
                    IsWorkflowEvent = true;*/
            }
            if (acTypeInfo != null)
                _ACName = acTypeInfo.ACIdentifier;
            if (forRequest != null)
            {
                _RequestID = forRequest.RequestID;
                Message = forRequest.Message;
                _Handled = forRequest.Handled;
                if (acTypeInfo == null)
                    _ACName = forRequest.ACIdentifier;
            }
            else
                _RequestID = Guid.NewGuid();
            SubscriptionSendCount = 0;
            EventBroadcasted = ACPropertyBroadcastState.Unsent;
            if (invokerInfo != null)
                _InvokerInfo = invokerInfo;
            else if (forRequest != null)
                _InvokerInfo = forRequest.InvokerInfo;
        }

        internal void CopyFrom(IACPropertyNetValueEvent from)
        {
            if (from == null)
                return;
            _Handled = from.Handled;
            Message = from.Message;
            _RequestID = from.RequestID;
            _ACUrl = from.ACUrl;
            _ACName = from.ACIdentifier;
            SubscriptionSendCount = from.SubscriptionSendCount;
            EventBroadcasted = from.EventBroadcasted;
            _IsRequestedValueStillValid = from.IsRequestedValueStillValid;
            _EventType = from.EventType;
            _Sender = from.Sender;
            _ForceBroadcast = from.ForceBroadcast;
            if (from is ACPropertyValueEvent<T>)
                _value = (from as ACPropertyValueEvent<T>).Value;
        }

        private bool _Handled = false;
        /// <summary>
        /// If set to true, the Event will not be forwarded to the next property (on the route to the endpoint).
        /// A new IACPropertyNetValueEvent will be created and the EventType will be set to RefusedRequest and broadcasted back to the originator.
        /// </summary>
        [DataMember(Name = "H")]
        public bool Handled
        {
            get
            {
                return _Handled;
            }

            set
            {
                _Handled = value;
            }
        }


        [IgnoreDataMember]
        private bool _IsRequestedValueStillValid = true;
        /// <summary>
        /// Is true if requested value change is still valid.
        /// Is false if another Proxy has also created another request at the same time or a internal change in the source-proterty has taken place.
        /// In this case the value is deprecated.
        /// </summary>
        [IgnoreDataMember]
        public bool IsRequestedValueStillValid
        {
            get
            {
                return _IsRequestedValueStillValid;
            }
        }

        public void InvalidateRequestedValue()
        {
            if (_IsRequestedValueStillValid == true)
                _IsRequestedValueStillValid = false;
        }


        /// <summary>
        /// Message, which can be set from the refusing property to inform the originator about the reason.
        /// </summary>
        [DataMember(Name = "M")]
        public string Message { get; set; }


        [DataMember(Name = "RID")]
        private Guid _RequestID;
        /// <summary>
        /// Unique ID generated by the caller/originator (first property, where the value change is generated). <para />
        /// The ValueEvent is then passed along the entire transport route to the end point with the same RequestID. <para />
        /// If the value change at the endpoint is successful, a new event is generated and distributed across the network (broadcast). <para />
        /// Through the distribution, the caller can then wait in its ACPropertyChangedEventHandler on its RequestID to evaluate what happened with the change request. <para />
        /// </summary>
        [IgnoreDataMember]
        public Guid RequestID
        {
            get
            {
                return _RequestID;
            }
        }


        [DataMember(Name = "S")]
        private EventRaiser _Sender = EventRaiser.Proxy;

        /// <summary>
        /// Describes, who was the originator who have changed a Property-Value
        /// </summary>
        [IgnoreDataMember]
        public EventRaiser Sender
        {
            get
            {
                return _Sender;
            }
        }


        [DataMember(Name = "E")]
        private EventTypes _EventType = EventTypes.Request;
        /// <summary>
        /// Reason why Event was generated
        /// </summary>
        [IgnoreDataMember]
        public EventTypes EventType
        {
            get
            {
                return _EventType;
            }
        }


        [DataMember(Name = "ACU")]
        private string _ACUrl;
        /// <summary>
        /// Address of the ACComponent
        /// </summary>
        [IgnoreDataMember]
        public string ACUrl
        {
            get
            {
                return _ACUrl;
            }
        }


        [DataMember(Name = "PID")]
        private String _ACName;
        /// <summary>
        /// Name/ID of Property which has created this Event
        /// </summary>
        [IgnoreDataMember]
        public String ACIdentifier
        {
            get
            {
                return _ACName;
            }
        }


        [IgnoreDataMember]
        public int SubscriptionSendCount { get; set; }
        /// <summary>
        /// Broadcasting state
        /// </summary>
        [IgnoreDataMember]
        public ACPropertyBroadcastState EventBroadcasted { get; set; }


        /// <summary>
        /// New value shich shuld be changed in property or was changed
        /// </summary>
        [IgnoreDataMember]
        public object ChangedValue
        {
            get
            {
                return this.Value;
            }
        }


        [IgnoreDataMember]
        object _InvokerInfo;
        /// <summary>
        /// Contains additional informations from driver specific objects (e.g. ValueQT from OPC, or S7TCPItem...) which has changed the value
        /// </summary>
        [IgnoreDataMember]
        public object InvokerInfo
        {
            get
            {
                return this._InvokerInfo;
            }
        }


        [DataMember(Name = "FB")]
        private bool _ForceBroadcast;
        /// <summary>
        /// Is true, if value wasn't changed but it should be transported to the Endpoint (Forced)
        /// </summary>
        [IgnoreDataMember]
        public bool ForceBroadcast
        {
            get
            {
                return _ForceBroadcast;
            }
            set
            {
                _ForceBroadcast = value;
            }
        }


        public void SetACUrl(string acUrl = "")
        {
            this._ACUrl = acUrl;
        }

        private IACPropertyNetValueEvent _OriginalEventIfClone = null;
        public IACPropertyNetValueEvent OriginalEventIfClone
        {
            get
            {
                return _OriginalEventIfClone;
            }
        }

        public IACPropertyNetValueEvent Clone()
        {
            ACPropertyValueEvent<T> clone = new ACPropertyValueEvent<T>();
            clone.CopyFrom(this);
            clone._OriginalEventIfClone = this;
            return clone;
        }
    }
}
