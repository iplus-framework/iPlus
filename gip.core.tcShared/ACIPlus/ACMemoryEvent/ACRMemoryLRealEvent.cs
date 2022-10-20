using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACRMemoryLRealEvent
    {
        [FieldOffset(0)]
        public uint InstanceID;

        [FieldOffset(4)]
        public ushort PropertyID;

        [FieldOffset(8)]
        public double Value;

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
