using System;
using System.Collections.Generic;
using System.Text;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication.ISOonTCP.Types
{
    public static class Real
    {
        public static double FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleFromByteArray(bytes, startindex, endianess);
        }

        public static Single FromByteArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleFromByteArray(bytes, endianess);
        }

        public static byte[] ToByteArray(Double value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleToByteArray(Convert.ToSingle(value), endianess);
        }

        public static byte[] ToByteArray(Single value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleToByteArray(value, endianess);
        }

        public static byte[] ToByteArray(Single[] value, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleToByteArray(value, endianess);
        }

        public static Single[] ToArray(byte[] bytes, EndianessEnum endianess = EndianessEnum.BigEndian)
        {
            return EndianessHelper.SingleToArray(bytes, endianess);
        }

        public const int Length = 4;
    }
}
