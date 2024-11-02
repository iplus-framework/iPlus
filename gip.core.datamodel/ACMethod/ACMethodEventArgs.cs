// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMethodEventArgs.cs" company="gip mbh, Oftersheim, Germany">
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
    /// <summary>A wrapper for the result of a asynchronous method. It's a response for passed ACMethod.</summary>
    [CollectionDataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMethodEventArgs'}de{'ACMethodEventArgs'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMethodEventArgs : ACEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodEventArgs"/> class.
        /// </summary>
        public ACMethodEventArgs() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodEventArgs"/> class.
        /// </summary>
        /// <param name="acMethod">The ac method.</param>
        /// <param name="resultState">State of the result.</param>
        public ACMethodEventArgs(ACMethod acMethod, Global.ACMethodResultState resultState)
            : base(acMethod.ResultValueList)
        {
            this.ParentACMethod = acMethod;
            ACRequestID = acMethod.ACRequestID;
            ResultState = resultState;
            if (this._ValidMessage == null)
                _ValidMessage = acMethod.ValidMessage;
            else
            {
                if (this._ValidMessage.MsgDetailsCount <= 0)
                    _ValidMessage = acMethod.ValidMessage;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethodEventArgs"/> class.
        /// </summary>
        /// <param name="requestID">The request ID.</param>
        /// <param name="resultList">The result list.</param>
        /// <param name="resultState">State of the result.</param>
        public ACMethodEventArgs(Guid requestID, ACValueList resultList, Global.ACMethodResultState resultState)
            : base(resultList)
        {
            ACRequestID = requestID;
            ResultState = resultState;
        }

        /// <summary>
        /// The result or current state of the asynchronus invocation.
        /// </summary>
        /// <value>The state of the result.</value>
        [DataMember]
        public Global.ACMethodResultState ResultState
        {
            get;
            internal set;
        }

        /// <summary>
        /// The RequestID is the same like the original RequestID of the ACMethod that was passed when the asynchronous method was started.
        /// </summary>
        /// <value>The AC request ID.</value>
        [DataMember]
        public Guid ACRequestID
        {
            get;
            internal set;
        }
    }
}
