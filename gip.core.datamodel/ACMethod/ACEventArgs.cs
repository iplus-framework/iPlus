// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACEventArgs.cs" company="gip mbh, Oftersheim, Germany">
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
    /// ACEventArgs
    /// </summary>
    //[ACSerializeableInfo]
    [CollectionDataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACEventArgs'}de{'ACEventArgs'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACEventArgs : ACValueList
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACEventArgs"/> class.
        /// </summary>
        public ACEventArgs() 
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACEventArgs"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public ACEventArgs(ACValueList collection)
            : base(collection)
        {
        }

        /// <summary>
        /// The _ valid message
        /// </summary>
        protected MsgWithDetails _ValidMessage;
        /// <summary>
        /// Gets or sets the valid message.
        /// </summary>
        /// <value>The valid message.</value>
        public MsgWithDetails ValidMessage
        {
            get
            {
                if (_ValidMessage == null)
                {
                    _ValidMessage = new MsgWithDetails();
                }
                return _ValidMessage;
            }
            set
            {
                _ValidMessage = value;
            }
        }

        public override object Clone()
        {
            ACEventArgs clone = new ACEventArgs();
            clone.CloneValues(this);
            return clone;
        }
        #endregion

        public static ACEventArgs GetVirtualEventArgs(string EventName, Dictionary<string, ACEventArgs> virtualEventArgs)
        {
            ACEventArgs args = null;

            if (virtualEventArgs != null)
                virtualEventArgs.TryGetValue(EventName, out args);

            if (args == null)
                return new ACEventArgs();
            else
                return (ACEventArgs)args.Clone();
        }
    }

}
