using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.modbus.Types
{
    public static class DInt
    {
        public static Int32 FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.Int32FromByteArray(bytes, startindex, endianess);
        }
        
        
        public static Int32 FromByteArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.Int32FromByteArray(bytes, endianess);
        }


        public static byte[] ToByteArray(Int32 value, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.Int32ToByteArray(value, endianess);
        }


        public static byte[] ToByteArray(Int32[] value, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.Int32ToByteArray(value, endianess);
        }


        public static Int32[] ToArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.Int32ToArray(bytes, endianess);
        }

        public const int Length = 4;
    }
}
