﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACRMemoryDTEvent
    {
        [FieldOffset(0)]
        public uint InstanceID;

        [FieldOffset(4)]
        public ushort PropertyID;

        //convert from seconds, start date is 01.01.1970.
        [FieldOffset(8)]
        public int Value;
    }
}
