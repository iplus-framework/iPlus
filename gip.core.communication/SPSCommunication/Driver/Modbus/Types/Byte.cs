using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace gip.core.communication.modbus.Types
{
    public static class Byte
    {
        // publics
        #region ToByteArray
        public static byte[] ToByteArray(byte value)
        {
            byte[] bytes = new byte[] { value};
            return bytes;
        }
        #endregion

        #region FromByteArray
        public static byte FromByteArray(byte[] bytes)
        {
            return bytes[0];
        }
        #endregion

        public static bool[] Convert(byte[] source, short? removeBitsFromStart, short? removeBitsFromEnd)
        {
            bool[] dest = new bool[source.Length * 8];
            int i = 0;
            foreach (byte val in source)
            {
                dest[i * 8 + 0] = System.Convert.ToBoolean(val & 0x01);
                dest[i * 8 + 1] = System.Convert.ToBoolean(val & 0x02);
                dest[i * 8 + 2] = System.Convert.ToBoolean(val & 0x04);
                dest[i * 8 + 3] = System.Convert.ToBoolean(val & 0x08);
                dest[i * 8 + 4] = System.Convert.ToBoolean(val & 0x10);
                dest[i * 8 + 5] = System.Convert.ToBoolean(val & 0x20);
                dest[i * 8 + 6] = System.Convert.ToBoolean(val & 0x40);
                dest[i * 8 + 7] = System.Convert.ToBoolean(val & 0x80);
                i++;
            }
            if (removeBitsFromStart.HasValue || removeBitsFromEnd.HasValue)
            {
                var list = dest.ToList();
                if (removeBitsFromStart.HasValue)
                {
                    for (int j = 0; j < removeBitsFromStart.Value; j++)
                    {
                        list.RemoveAt(0);
                    }
                }
                if (removeBitsFromEnd.HasValue)
                {
                    for (int j = 0; j < removeBitsFromEnd.Value; j++)
                    {
                        list.RemoveAt(list.Count-1);
                    }
                }
                return list.ToArray();
            }
            return dest;
        }

        public static byte[] Convert(bool[] source)
        {
            int size = 0;
            if (source.Length <= 8)
                size = 1;
            else
            {
                size = source.Length / 8;
                if (source.Length % 8 != 0)
                    size++;
            }
            byte[] dest = new byte[size];
            int byteIndex = 0, bitIndex = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i])
                {
                    byte newBitMask = (byte)(1 << bitIndex);
                    //dest[byteIndex] |= (byte)(1 << (7 - bitIndex));
                    dest[byteIndex] |= newBitMask;
                }
                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return dest;
        }

        public const int Length = 1;
    }
}
