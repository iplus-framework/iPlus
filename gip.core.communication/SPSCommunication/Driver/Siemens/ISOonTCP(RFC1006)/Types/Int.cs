using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.ISOonTCP.Types
{
    public static class Int
    {
        public static Int16 FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.Int16FromByteArray(bytes, startindex, endianess);
        }

        public static Int16 FromByteArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.Int16FromByteArray(bytes, endianess);
        }


        public static byte[] ToByteArray(Int16 value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.Int16ToByteArray(value, endianess);
        }


        public static byte[] ToByteArray(Int16[] value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.Int16ToByteArray(value, endianess);
        }


        public static Int16[] ToArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.Int16ToArray(bytes, endianess);
        }

        public const int Length = 2;
    }
}
