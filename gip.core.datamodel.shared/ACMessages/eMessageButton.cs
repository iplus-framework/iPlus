// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Enum eMsgButton
    /// </summary>
#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'eMsgButton'}de{'eMsgButton'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, true, false)]
#endif
    [DataContract]
    public enum eMsgButton : short
    {
        /// <summary>
        /// The OK
        /// </summary>
        [EnumMember(Value = "O")]
        OK = 0,
        /// <summary>
        /// The OK cancel
        /// </summary>
        [EnumMember(Value = "OC")]
        OKCancel = 1,
        /// <summary>
        /// The yes no cancel
        /// </summary>
        [EnumMember(Value = "YNC")]
        YesNoCancel = 3,
        /// <summary>
        /// The yes no
        /// </summary>
        [EnumMember(Value = "YN")]
        YesNo = 4,
        /// <summary>
        /// Only closing window
        /// </summary>
        [EnumMember(Value = "C")]
        Cancel = 5
    }
}
