using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public class ACPointAsyncRMISubscrWrap<T> : ACPointNetWrapObject<T>
        where T : ACComponent 
    {
        #region c'tors
        /// <summary>
        /// Constructor for contructing the Wrapper-"wrapObject"
        /// </summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        public ACPointAsyncRMISubscrWrap(T refObject, IACPointNetBase owner, IACPointAsyncRMI<T> asyncRMI, string acNameIdentifier, Guid requestID)
            : base(refObject, owner)
        {
            AsyncRMI = asyncRMI;
            _ACMethodDescriptor = new ACMethodDescriptor(acNameIdentifier, requestID);
        }

        /// <summary>
        /// Constructor for contructing the Wrapper-"wrapObject"
        /// </summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        public ACPointAsyncRMISubscrWrap(T refObject, IACPointNetBase owner, IACPointAsyncRMI<T> asyncRMI, ACMethodDescriptor asyncMethodToCall)
            : base(refObject, owner)
        {
            AsyncRMI = asyncRMI;
            _ACMethodDescriptor = asyncMethodToCall;
        }

        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointAsyncRMISubscrWrap()
        {
        }
        #endregion

        #region Own Member

        private ACMethodDescriptor _ACMethodDescriptor;
        [DataMember]
        public ACMethodDescriptor ACMethodDescriptor
        {
            get
            {
                return _ACMethodDescriptor;
            }
            set
            {
                _ACMethodDescriptor = value;
            }
        }


        [IgnoreDataMember]
        public override Guid RequestID
        {
            get
            {
                if (ACMethodDescriptor == null)
                    return Guid.Empty;
                return ACMethodDescriptor.ACRequestID;
            }

            internal set
            {
            }
        }

        /// <summary>
        /// Name of Method, which will be called on the Server-Object
        /// </summary>
        [IgnoreDataMember]
        public string MethodACIdentifier
        {
            get
            {
                if (ACMethodDescriptor == null)
                    return "";
                return ACMethodDescriptor.ACIdentifier;
            }
        }

        [IgnoreDataMember]
        private IACPointAsyncRMI<T> _AsyncRMI;
        [IgnoreDataMember]
        public IACPointAsyncRMI<T> AsyncRMI
        {
            get
            {
                return _AsyncRMI;
            }
            set
            {
                _AsyncRMI = value;
                if (_AsyncRMI != null)
                    RMIPointName = _AsyncRMI.ACIdentifier;
            }
        }

        [IgnoreDataMember]
        protected string _RMIPointName;
        /// <summary>
        /// Name of Client-Point which has a Realtion to this ServerPoint
        /// </summary>
        // Heißt beim ACMethod "ClientJobPoint"
        [DataMember]
        public string RMIPointName
        {
            get
            {
                return _RMIPointName;
            }
            set
            {
                _RMIPointName = value;
            }
        }

        #endregion

        #region IComparable Member

        public override int CompareTo(object obj)
        {
            if (obj is ACPointAsyncRMISubscrWrap<T>)
            {
                ACPointAsyncRMISubscrWrap<T> objRMI = obj as ACPointAsyncRMISubscrWrap<T>;
                int diff = this.GetACUrl().CompareTo(objRMI.GetACUrl());
                if (diff != 0)
                    return diff;
                diff = this.MethodACIdentifier.CompareTo(objRMI.MethodACIdentifier);
                if (diff != 0)
                    return diff;
                return this.RequestID.CompareTo(objRMI.RequestID);
            }
            else if (obj is ACPointAsyncRMIWrap<T>)
            {
                int diff = 0;
                ACPointAsyncRMIWrap<T> objRMI = obj as ACPointAsyncRMIWrap<T>;
                if (this.ParentACObject != null)
                {
                    diff = (this.ParentACObject as IACObject).GetACUrl().CompareTo(objRMI.GetACUrl());
                    if (diff != 0)
                        return diff;
                }
                diff = this.MethodACIdentifier.CompareTo(objRMI.MethodACIdentifier);
                if (diff != 0)
                    return diff;
                return this.RequestID.CompareTo(objRMI.RequestID);
            }
            else
                return -1;
        }

        public override void CopyDataOfWrapObject(ACPointNetWrapObject<T> obj)
        {
            ACPointAsyncRMISubscrWrap<T> objSubscr = obj as ACPointAsyncRMISubscrWrap<T>;
            if (objSubscr != null)
                this.RMIPointName = objSubscr.RMIPointName;
            base.CopyDataOfWrapObject(obj);
        }

        #endregion

    }
}