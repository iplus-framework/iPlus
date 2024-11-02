// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gip.core.datamodel.ACContainer
{
    [DataContract]
    [XmlRoot("ACMenuItem")]
    [XmlType("ACMenuItem")]
    public class ACMenuItemLight
    {
        public ACMenuItemLight()
        {

        }

        public string ACCaption { get; set; }
        public string ACCaptionTranslation { get; set; }
        public string ACUrl { get; set; }
        public List<ACMenuItemLight> Items { get; set; }
    }
}
