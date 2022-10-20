﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACRMemoryStringEvent
    {
        [FieldOffset(0)]
        public uint InstanceID;

        [FieldOffset(4)]
        public ushort PropertyID;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string Value;
    }
}