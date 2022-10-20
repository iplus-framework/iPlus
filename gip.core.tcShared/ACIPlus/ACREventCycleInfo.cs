using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACREventCycleInfo
    {
        [FieldOffset(0)]
        public uint MemReadCycle;

        [FieldOffset(4)]
        public uint MemWriteCycle;

        [FieldOffset(8)]
        public int NextByteEventIndex;

        [FieldOffset(12)]
        public int NextUIntEventIndex;

        [FieldOffset(16)]
        public int NextIntEventIndex;

        [FieldOffset(20)]
        public int NextUDIntEventIndex;

        [FieldOffset(24)]
        public int NextDIntEventIndex;

        [FieldOffset(28)]
        public int NextRealEventIndex;

        [FieldOffset(32)]
        public int NextLRealEventIndex;

        [FieldOffset(36)]
        public int NextStringEventIndex;

        [FieldOffset(40)]
        public int NextTimeEventIndex;

        [FieldOffset(44)]
        public int NextDTEventIndex;
    }
}
