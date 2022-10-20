using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACPointEventWrap<T> : ACPointNetWrapObject<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>Constructor for contructing the Wrapper-"wrapObject"</summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        /// <param name="owner"></param>
        /// <param name="asyncCallbackDelegate"></param>
        public ACPointEventWrap(T refObject, IACPointNetBase owner, ACPointNetEventDelegate asyncCallbackDelegate) 
            : base(refObject, owner)
        {
            _OriginalAsyncCallbackDelegate = asyncCallbackDelegate;
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointEventWrap{T}" /> class.</summary>
        /// <param name="refObject">The reference object.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="asyncCallbackDelegate">The asynchronous callback delegate.</param>
        /// <param name="clientEntry">The client entry.</param>
        public ACPointEventWrap(T refObject, IACPointNetBase owner, ACPointNetEventDelegate asyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry)
            : this(refObject, owner, asyncCallbackDelegate)
        {
            _ClientPointName = clientEntry.Point.ACIdentifier;
            _RequestID = clientEntry.RequestID;
        }

        /// <summary>Constructor for contructing the Wrapper-"wrapObject", if Callback-Method is defined by script</summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        /// <param name="owner"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        public ACPointEventWrap(T refObject, IACPointNetBase owner, string asyncCallbackDelegateName)
            : base(refObject, owner)
        {
            _AsyncCallbackDelegateName = asyncCallbackDelegateName;
            _OriginalAsyncCallbackDelegate = null;
        }

        public ACPointEventWrap(T refObject, IACPointNetBase owner, string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry)
            : this(refObject, owner, asyncCallbackDelegateName)
        {
            _ClientPointName = clientEntry.Point.ACIdentifier;
            _RequestID = clientEntry.RequestID;
        }

        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEventWrap()
        {
        }
        #endregion

        #region Own Member
        [IgnoreDataMember]
        private string _AsyncCallbackDelegateName;
        /// <summary>
        /// Name of Method, which will be called from Server, when Event occurs
        /// 
        /// TODO: Entspricht dem ACEvent.ClientPoint, wobei der ClienPoint + "Callback" immer der AsyncCallbackDelegateName entspricht
        /// </summary>
        [DataMember]
        public string AsyncCallbackDelegateName
        {
            get
            {
                if (_OriginalAsyncCallbackDelegate != null)
                    return _OriginalAsyncCallbackDelegate.Method.Name;
                return _AsyncCallbackDelegateName;
            }
            set
            {
                _AsyncCallbackDelegateName = value;
            }
        }

        [IgnoreDataMember]
        internal ACPointNetEventDelegate _RedirectCallbackDelegate;

        [IgnoreDataMember]
        private ACPointNetEventDelegate _OriginalAsyncCallbackDelegate;
        [IgnoreDataMember]
        public ACPointNetEventDelegate OriginalAsyncCallbackDelegate
        {
            get
            {
                return _OriginalAsyncCallbackDelegate;
            }
        }

        [IgnoreDataMember]
        public ACPointNetEventDelegate AsyncCallbackDelegate
        {
            get
            {
                if (_RedirectCallbackDelegate != null)
                    return _RedirectCallbackDelegate;
                return _OriginalAsyncCallbackDelegate;
            }         
        }
        #endregion

        #region IComparable Member

        public override int CompareTo(object obj)
        {
            if (obj is ACPointEventWrap<T>)
            {
                int diff = this.GetACUrl().CompareTo((obj as ACPointEventWrap<T>).GetACUrl());
                return diff;
                // Kein Vergleich auf AsyncCallbackDelegateName, 
                // weil ein abonniertes Event nur auf eine CallBack-Methode des Abonnierenden ACObjects verweisen darf
                /*if (diff != 0)
                    return diff;
                return this.AsyncCallbackDelegateName.CompareTo((obj as ACPointEventWrap<T>).AsyncCallbackDelegateName);*/
            }
            else if (obj is ACPointEventSubscrWrap<T>)
            {
                if (this.ParentACObject == null)
                    return -1;
                int diff = (this.ParentACObject as IACObject).GetACUrl().CompareTo((obj as ACPointEventSubscrWrap<T>).GetACUrl());
                if (diff != 0)
                    return diff;
                if ((obj as ACPointEventSubscrWrap<T>).Point != null)
                    diff = this.GetACUrl().CompareTo((obj as ACPointEventSubscrWrap<T>).Point.ACRef.GetACUrl());
                return diff;
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