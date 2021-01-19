using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACPointEventSubscrWrap<T> : ACPointNetWrapObject<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for contructing the Wrapper-"wrapObject"
        /// </summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        public ACPointEventSubscrWrap(T refObject, IACPointNetBase owner, IACPointEvent<T> refEvent) 
            : base(refObject, owner)
        {
            _Event = refEvent;
            _RequestID = Guid.NewGuid();
        }

        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEventSubscrWrap()
        {
        }
        #endregion

        #region Own Member
        [IgnoreDataMember]
        private string _EventACIdentifier;
        /// <summary>
        /// Name of Event at Service-Object
        /// </summary>
        [DataMember]
        public string EventACIdentifier
        {
            get
            {
                if (_Event != null)
                    return _Event.ACIdentifier;
                return _EventACIdentifier;
            }
            set
            {
                _EventACIdentifier = value;
            }
        }

        [IgnoreDataMember]
        private IACPointEvent<T> _Event;
        [IgnoreDataMember]
        public IACPointEvent<T> Event
        {
            get
            {
                if (_Event == null && !String.IsNullOrEmpty(_EventACIdentifier) && IsObjLoaded)
                {
                    _Event = (ValueT as ACComponent).GetPointNet(_EventACIdentifier) as IACPointEvent<T>;
                }
                return _Event;
            }         
            set
            {
                _Event = value;
            }
        }

        [DataMember]
        public bool IsTriggerDirect
        {
            get;
            set;
        }

        /// <summary>
        /// Wenn die Message empfangen wurde, ist die IsActive = true
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        #endregion

        #region IComparable Member

        public override int CompareTo(object obj)
        {
            if (obj is ACPointEventSubscrWrap<T>)
            {
                int diff = this.GetACUrl().CompareTo((obj as ACPointEventSubscrWrap<T>).GetACUrl());
                if (diff != 0)
                    return diff;
                return this.EventACIdentifier.CompareTo((obj as ACPointEventSubscrWrap<T>).EventACIdentifier);
            }
            else if (obj is ACPointEventWrap<T>)
            {
                if (this.ParentACObject == null)
                    return -1;
                int diff = (this.ParentACObject as IACObject).GetACUrl().CompareTo((obj as ACPointEventWrap<T>).GetACUrl());
                if (diff != 0)
                    return diff;
                if ((obj as ACPointEventWrap<T>).Point != null)
                    diff = this.GetACUrl().CompareTo((obj as ACPointEventWrap<T>).Point.ACRef.GetACUrl());
                return diff;
                /*if (diff != 0)
                    return diff;
                return this.EventName.CompareTo((obj as ACPointEventWrap<T>).EventName);*/
            }
            else
                return -1;
        }

        public override void CopyDataOfWrapObject(ACPointNetWrapObject<T> obj)
        {
            base.CopyDataOfWrapObject(obj);
        }

        #endregion

    }
}