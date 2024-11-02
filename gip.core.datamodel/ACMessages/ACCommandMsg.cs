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
// <copyright file="MsgInfo.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ACCommandMsg
    /// </summary>
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCommandMsg'}de{'ACCommandMsg'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACCommandMsg", "en{'ACCommandMsg'}de{'ACCommandMsg'}", typeof(ACCommandMsg), "ACCommandMsg", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACCommandMsg : ACCommand
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACCommandMsg"/> class.
        /// </summary>
        /// <param name="acUrlInfo">The ac URL info.</param>
        /// <param name="acComment">The ac comment.</param>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="parameterList">The parameter list.</param>
        public ACCommandMsg(string acUrlInfo, string acComment, string acCaption, string acUrl, ACValueList parameterList)
            : base(acCaption, acUrl, parameterList)
        {
            ACUrlInfo = acUrlInfo;
            ACComment = acComment;
        }
        #endregion

        /// <summary>
        /// The _ AC URL info
        /// </summary>
        string _ACUrlInfo;
        /// <summary>
        /// Gets or sets the AC URL info.
        /// </summary>
        /// <value>The AC URL info.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public string ACUrlInfo
        {
            get
            {
                return _ACUrlInfo;
            }
            set
            {
                _ACUrlInfo = value;
                OnPropertyChanged("ACUrlInfo");
            }
        }

        /// <summary>
        /// The _ AC comment
        /// </summary>
        string _ACComment;
        /// <summary>
        /// Gets or sets the AC comment.
        /// </summary>
        /// <value>The AC comment.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public string ACComment
        {
            get
            {
                return _ACComment;
            }
            set
            {
                _ACComment = value;
                OnPropertyChanged("ACComment");
            }
        }
    }

    /// <summary>
    /// Class ACCommandMsgList
    /// </summary>
    [ACSerializeableInfo]
    public class ACCommandMsgList : List<ACCommandMsg>
    {
    }
}
