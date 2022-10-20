using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.modbus.Types
{
    public static class Word
    {
        public static UInt16 FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.UInt16FromByteArray(bytes, startindex, endianess);
        }
        
        
        public static UInt16 FromByteArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.UInt16FromByteArray(bytes, endianess);
        }


        public static byte[] ToByteArray(UInt16 value, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.UInt16ToByteArray(value, endianess);
        }


        public static byte[] ToByteArray(UInt16[] value, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.UInt16ToByteArray(value, endianess);
        }


        public static UInt16[] ToArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.MixedEndian)
        {
            return EndianessHelper.UInt16ToArray(bytes, endianess);
        }

        public const int Length = 2;
    }
}
