// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-17-2013
// ***********************************************************************
// <copyright file="Msg.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Transactions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Enum eMsgLevel
    /// </summary>
#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'eMsgLevel'}de{'eMsgLevel'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, true, false)]
#endif
    [DataContract]
    public enum eMsgLevel : short
    {
        /// <summary>
        /// Not specified in more detail.
        /// </summary>
        [EnumMember(Value = "D0")]
        Default = 0,

        /// <summary>
        /// A debug message. Messages that are used to track a problem.
        /// </summary>
        [EnumMember(Value = "D1")]
        Debug = 1,

        /// <summary>
        /// A informational message.
        /// </summary>
        [EnumMember(Value = "I2")]
        Info = 2,

        /// <summary>
        /// A warning message. It could indicate a problem that needs to be fixed.
        /// </summary>
        [EnumMember(Value = "W3")]
        Warning = 3,

        /// <summary>
        /// A failure message. An failure occurred that could be successful if you try again.
        /// </summary>
        [EnumMember(Value = "F4")]
        Failure = 4,

        /// <summary>
        /// A error message. There is an error that needs to be fixed.
        /// </summary>
        [EnumMember(Value = "E5")]
        Error = 5,

        /// <summary>
        /// A exception message. It could indicate a technical problem that may recur and the cause should be investigated.
        /// </summary>
        [EnumMember(Value = "E6")]
        Exception = 6,

        /// <summary>
        /// A question message. An action or process is waiting for a human response.
        /// </summary>
        [EnumMember(Value = "Q7")]
        Question = 7,

        /// <summary>
        /// A question message. An action or process is waiting for a human response to enter string value.
        /// </summary>
        [EnumMember(Value = "Q8")]
        QuestionPrompt = 8,
    };
}
