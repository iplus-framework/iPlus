// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMethodDescriptor.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACMethodDescriptor
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMethod'}de{'ACMethod'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMethodDescriptor : ICloneable
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodDescriptor"/> class.
        /// </summary>
        public ACMethodDescriptor()
        {
            //_RequestID = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodDescriptor"/> class.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public ACMethodDescriptor(string methodName)
            : this()
        {
            ACIdentifier = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodDescriptor"/> class.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="requestID">The request ID.</param>
        public ACMethodDescriptor(string methodName, Guid requestID)
        {
            ACIdentifier = methodName;
            _RequestID = requestID;
        }

        #endregion

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Name'}de{'Name'}")]
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// The _ request ID
        /// </summary>
        [IgnoreDataMember]
        protected Guid _RequestID;
        /// <summary>
        /// 1. Methodenaufrufe
        /// Je Methodenaufruf eindeutig generiert.
        /// Zum Erzeugen eines neuen ACMethodJob immer die Methode AClassMethod.NewJobACMethod() verwenden,
        /// welche eine neue ACRequestID erzeugt.
        /// 2. Rückgabe
        /// Immer die ACRequestID des vorangegangenen Methodenaufrufs
        /// </summary>
        /// <value>The AC request ID.</value>
        [ACPropertyInfo(9999, "", "en{'ACRequestID'}de{'ACRequestID'}")]
        [DataMember]
        public Guid ACRequestID
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

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid()
        {
            if (String.IsNullOrEmpty(ACIdentifier))
                return false;
            if (_RequestID == Guid.Empty)
                return false;
            return true;
        }

        /// <summary>
        /// Copies from if different.
        /// </summary>
        /// <param name="from">From.</param>
        public virtual void CopyFromIfDifferent(ACMethodDescriptor from)
        {
            if (from == null)
                return;
            if (this.ACIdentifier != from.ACIdentifier)
                this.ACIdentifier = from.ACIdentifier;
            if (this._RequestID != from.ACRequestID)
                this._RequestID = from.ACRequestID;
        }

        public virtual void CopyFrom(ACMethodDescriptor from)
        {
            if (from == null)
                return;
            this.ACIdentifier = from.ACIdentifier;
            this._RequestID = from.ACRequestID;
        }

        public virtual object Clone()
        {
            ACMethodDescriptor clone = new ACMethodDescriptor();
            clone.CopyFrom(this);
            return clone;
        }
    }
}
