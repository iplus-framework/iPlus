using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.ACVariobatch
{
    [StructLayout(LayoutKind.Explicit)]
    public class ACRMemoryMetaObj
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GCL.cACUrlLEN)]
        public string _ACUrl;

        [FieldOffset(204)]
        public uint _OffsetInMemory;

        [FieldOffset(208)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = GCL.cMetaPropMAX)]
        public ACRMemoryMetaProp[] _MetaProp;

        [FieldOffset(1008)]
        public uint _MetaPropSize;

        [FieldOffset(1012)]
        public ushort _MaxPropertyID;

        [FieldOffset(1014)]
        [MarshalAs(UnmanagedType.I2)]
        public TypeOfACObject _TypeOfACObject;
    }
}
