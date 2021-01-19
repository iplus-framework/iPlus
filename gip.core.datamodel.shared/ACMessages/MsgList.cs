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
    /// Class MsgList
    /// </summary>
#if NETFRAMEWORK
    [ACSerializeableInfo]
#endif
    public class MsgList : List<Msg>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsgList"/> class.
        /// </summary>
        public MsgList()
        {
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Msg msg in this)
            {
                string message = msg.Message;
#if NETFRAMEWORK
                if (String.IsNullOrEmpty(message))
                    message = msg.ACCaption;
#endif
                if (String.IsNullOrEmpty(message))
                    message = msg.ACIdentifier;
                builder.AppendLine(message);
            }
            return builder.ToString();
        }
    }

}
