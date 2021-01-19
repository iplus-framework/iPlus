﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared.ACVariobatch;

namespace gip.core.tcShared.WCF
{
    [DataContract]
    public class WcfAdsMessage
    {
        [DataMember(EmitDefaultValue = false, Name="A")]
        public byte? ConnectionState { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "B")]
        public ACRMemoryMetaObj[] Metadata { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "C")]
        public byte[] Memory { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "D")]
        public byte[] Parameters { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "E")]
        public byte[] Result { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "F")]
        public VBEvent VBEvent { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "G")]
        public ACRMemoryByteEvent[] ByteEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "H")]
        public ACRMemoryUIntEvent[] UIntEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "I")]
        public ACRMemoryIntEvent[] IntEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "J")]
        public ACRMemoryDIntEvent[] DIntEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "K")]
        public ACRMemoryUDIntEvent[] UDIntEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "L")]
        public ACRMemoryRealEvent[] RealEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "M")]
        public ACRMemoryLRealEvent[] LRealEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "N")]
        public ACRMemoryStringEvent[] StringEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "O")]
        public ACRMemoryTimeEvent[] TimeEvents { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "P")]
        public ACRMemoryDTEvent[] DTEvents { get; set; }
    }
}