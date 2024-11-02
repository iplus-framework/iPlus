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
// <copyright file="ObjectSelected.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.datamodel
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACUrlCmdMessage'}de{'ACUrlCmdMessage'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACUrlCmdMessage
    {
        [DataMember]
        private string _ACUrl;
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        public string ACUrl
        {
            get
            {
                return _ACUrl;
            }
            set
            {
                _ACUrl = value;
            }
        }

        //[DataMember]
        private Object[] _acParameter;
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        public Object[] ACParameter
        {
            get
            {
                return _acParameter;
            }
            set
            {
                _acParameter = value;
            }
        }

        [DataMember]
        private string _TargetVBContent;
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        public string TargetVBContent
        {
            get
            {
                return _TargetVBContent;
            }
            set
            {
                _TargetVBContent = value;
            }
        }

    }
}
