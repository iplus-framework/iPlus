using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.ISOonTCP.Types
{
    public static class DWord
    {
        public static UInt32 FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.UInt32FromByteArray(bytes, startindex, endianess);
        }
        

        public static UInt32 FromByteArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.UInt32FromByteArray(bytes, endianess);
        }


        public static byte[] ToByteArray(UInt32 value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.UInt32ToByteArray(value, endianess);
        }


        public static byte[] ToByteArray(UInt32[] value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.UInt32ToByteArray(value, endianess);
        }


        public static UInt32[] ToArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.UInt32ToArray(bytes, endianess);
        }

        public const int Length = 4;
    }
}
