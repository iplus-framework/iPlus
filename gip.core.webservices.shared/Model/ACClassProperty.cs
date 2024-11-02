// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Runtime.Serialization;
using System.Reflection;
using gip.core.datamodel;

namespace gip.core.webservices
{
    [DataContract(Name ="ACCP")]
    public class ACClassProperty
    {
        [DataMember(Name = "ID")]
        public Guid ACClassPropertyID
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
