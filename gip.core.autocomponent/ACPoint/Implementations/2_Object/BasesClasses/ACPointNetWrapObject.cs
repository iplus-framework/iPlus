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
    /// ACPointRefNetObject wrapps Objects of different types. The synonym of a ACPointRefNetObject is "wrapObject"
    /// Instead of serializing the wrapped object, the IACUrl-Adress of the object will be determined on the follwoing ways:
    ///     - If Object is a IACUrl-Implementation, then der ACUrl-address is the ACUrl of that object itself
    ///     - If Object is a Entity-Object (IACObjectEntity), then the ACUrl-address will be built over the Typename, Entity-GUID and the ACUrl-Adress of the Database-Converter-ACObject
    ///     - If Object is something else, this Class must be derived.
    /// The ACUrl-String will be transfered over the network.
    /// </summary>
    /// <typeparam name="T">T is wrapped content. The synonym of T is "refObject". T can be Types of Instances of IACUrl, ACObject, IACObjectEntity.</typeparam>

    [DataContract]
    public class ACPointNetWrapObject<T> : ACRef<T>, IACPointEntry, IComparable
        where T : IACObject 
    {
        #region c'tors
        /// <summary>Constructor for contructing the Wrapper-"wrapObject"</summary>
        /// <param name="refObject">A "refObject" muss be passed. It will be wrapped from this "wrapObject"</param>
        /// <param name="owner"></param>
        public ACPointNetWrapObject(T refObject, IACPointNetBase owner)
            : base(refObject,owner,true)
        {
            _Point = owner;
            _RequestID = Guid.Empty;
            _State = PointProcessingState.NewEntry;
            _SequenceNo = 0;
        }

        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetWrapObject()
        {
            _IsWeakReference = true;
            //_State = ProcessingState.NewEntry;
            //_SequenceNo = 0;
            //_RefACObject = new RefACObject();
        }
        #endregion

        #region DataMember

        [IgnoreDataMember]
        private IACPointNetBase _Point = null;
        [IgnoreDataMember]
        public IACPointNetBase Point
        {
            get
            {
                return _Point;
            }
            set
            {
                _Point = value;
                OnPointReceivedRemote();
            }
        }

        protected virtual void OnPointReceivedRemote()
        {
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [IgnoreDataMember]
        public override IACObject ParentACObject
        {
            get
            {
                if (Point == null)
                    return null;
                return Point.ACRef.ValueT;
            }
        }

        [IgnoreDataMember]
        protected string _ClientPointName;
        /// <summary>
        /// Name of Client-Point which has a Realtion to this ServerPoint
        /// </summary>
        // Heißt beim ACMethod "ClientJobPoint"
        [DataMember]
        public string ClientPointName
        {
            get
            {
                return _ClientPointName;
            }
            set
            {
                _ClientPointName = value;
            }
        }

        
        [DataMember]
        private PointProcessingState _State;
        [IgnoreDataMember]
        public PointProcessingState State
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
            }
        }

        [DataMember]
        private long _SequenceNo;
        [IgnoreDataMember]
        public long SequenceNo
        {
            get
            {
                return _SequenceNo;
            }

            set
            {
                _SequenceNo = value;
            }
        }

        [DataMember]
        protected Guid _RequestID;
        [IgnoreDataMember]
        public virtual Guid RequestID
        {
            get
            {
                return _RequestID;
            }

            internal set
            {
                _RequestID = value;
            }
        }

        [IgnoreDataMember]
        public bool DataHasChanged
        {
            get;
            internal set;
        }

        #endregion

        #region IACUrl Member

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public override object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            // Falls Objekt im Speicherraum schonmal instanziert worden ist
            if (IsAttached)
            {
                base.ACUrlCommand(acUrl, acParameter);
            }
            // Sonst lebt Objekt im Netzwerk: Aufruf ohne Instanzierung
            else
            {
                if (Point != null)
                {
                    // ACPSubscrObjService ParentSubscrObject
                    if (Point is ACPointNetStorableBase<T, ACPointNetWrapObject<T>>)
                    {
                        ACPointNetStorableBase<T, ACPointNetWrapObject<T>> storablePoint = Point as ACPointNetStorableBase<T, ACPointNetWrapObject<T>>;
                        if (storablePoint.ParentSubscrObject != null)
                        {
#if !ANDROID
                            if (storablePoint.ParentSubscrObject.WCFServiceChannel != null)
                            {
                                storablePoint.ParentSubscrObject.WCFServiceChannel.BroadcastACMessageToClient(WCFMessage.NewACMessage(acUrl, acParameter));
                            }
#endif
                        }
                    }

                }
            }
            return null;
        }

        #endregion

        #region IComparable Member
        public virtual int CompareToAtRemove(object obj)
        {
            int diff = CompareTo(obj);
            if (diff == 0)
                this.Detach();
            return diff;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is ACPointNetWrapObject<T>)
            {
                return this.RequestID.CompareTo((obj as ACPointNetWrapObject<T>).RequestID);
                //return this.GetACUrl().CompareTo((obj as ACPointNetWrapObject<T>).GetACUrl());
            }
            else
                return -1;
        }

        public virtual void CompareChangedData(ACPointNetWrapObject<T> objUpdate)
        {
            if (objUpdate.State != this.State)
                this.DataHasChanged = true;
        }

        public virtual void CopyDataOfWrapObject(ACPointNetWrapObject<T> obj)
        {
            this.State = obj.State;
            this.SequenceNo = obj.SequenceNo;
            this._RequestID = obj._RequestID;
            this._ClientPointName = obj._ClientPointName;
        }

        #endregion
    }
}