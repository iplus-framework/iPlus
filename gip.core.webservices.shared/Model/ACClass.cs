// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Runtime.Serialization;
using System.Reflection;
using gip.core.datamodel;
using System.Collections.Generic;

namespace gip.core.webservices
{
    [DataContract]
    public class ACClass
    {
        [DataMember(Name = "ID")]
        public Guid ACClassID
        {
            get; set;
        }

        [DataMember(Name = "FQN")]
        public string FullQName
        {
            get; set;
        }

        [DataMember(Name = "ACI")]
        public string ACIdentifier
        {
            get; set;
        }

        [DataMember(Name = "CT")]
        public string ACCaptionTranslation
        {
            get; set;
        }

        [DataMember(Name = "ACUrlC")]
        public string ACUrlComponent
        {
            get; set;
        }

        [DataMember(Name = "ixACCP")]
        public List<ACClassProperty> Properties
        {
            get;set;
        }

        [IgnoreDataMember]
        public string ACCaption
        {
            get
            {
                return Translator.GetTranslation(ACCaptionTranslation);
            }
        }

    }
}
