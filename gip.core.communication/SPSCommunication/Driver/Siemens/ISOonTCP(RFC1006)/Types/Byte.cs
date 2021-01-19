using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.ISOonTCP.Types
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

        public static bool[] Convert(byte[] source)
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
            return dest;
        }

        public const int Length = 1;
    }
}
