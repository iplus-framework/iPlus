// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace gip.tool.publish
{
    [DataContract]
    public class Version
    {
        [DataMember]
        public string SvnRevision
        {
            get;
            set;
        }

        [DataMember]
        public string ApplicationFileName
        {
            get;
            set;
        }

        [DataMember]
        public string DatabaseFileName
        {
            get;
            set;
        }

        [DataMember]
        public string ChangeLogEn
        {
            get;
            set;
        }

        [DataMember]
        public string ChangeLogDe
        {
            get;
            set;
        }

        [DataMember]
        public DateTime PublishDateTime
        {
            get;
            set;
        }
    }
}
