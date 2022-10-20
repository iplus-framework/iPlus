using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ACRMemoryMetaProp
    {
        [FieldOffset(0)]
        public uint _Offset;

        [FieldOffset(4)]
        public ushort _Length;
    }

}
