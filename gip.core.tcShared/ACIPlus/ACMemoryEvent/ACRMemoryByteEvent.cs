// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACRMemoryByteEvent
    {
        [FieldOffset(0)]
        public uint InstanceID;

        [FieldOffset(4)]
        public ushort PropertyID;

        [FieldOffset(8)]
        public byte Value;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)InstanceID;
                hash = hash * 23 + (short)PropertyID;
                return hash;
            }
        }
    }
}