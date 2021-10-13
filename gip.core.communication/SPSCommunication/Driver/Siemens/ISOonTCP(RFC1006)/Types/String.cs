using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.ISOonTCP.Types
{
    public static class String
    {
        ///S5- und S7-Strings sind unterschiedlich aufgebaut. 
        ///Während S5-Strings keine Längenangaben enthalten, 
        ///sind in einem S7-String die ersten beiden Bytes mit entsprechenden Angaben belegt:
        ///enthält im 1. Byte maximale Länge und im 2. Byte tatsächliche Länge

        // publics
        #region ToByteArray
        public static byte[] ToByteArray(string value, byte stringLen, int itemLen, bool asBase64 = false)
        {
            if (itemLen < 2)
                return null;
            if (value == null)
                value = "";
            byte[] bytes = new byte[itemLen];
            bytes[0] = stringLen;
            if (asBase64)
            {
                byte[] ca = Convert.FromBase64String(value);
                int valueLen = ca.Length;
                if (value.Length > (itemLen - 2))
                    valueLen = itemLen - 2;
                bytes[1] = System.Convert.ToByte(valueLen);
                for (int i = 0; i < (itemLen - 2); i++)
                {
                    if (i < valueLen)
                        bytes[i + 2] = ca[i];
                    else
                        bytes[i + 2] = 0;
                }
            }
            else
            {
                char[] ca = value.ToCharArray();
                int valueLen = value.Length;
                if (value.Length > (itemLen - 2))
                    valueLen = itemLen - 2;
                bytes[1] = System.Convert.ToByte(valueLen);
                for (int i = 0; i < (itemLen - 2); i++)
                {
                    if (i < valueLen)
                        bytes[i + 2] = (byte)Asc(ca[i].ToString());
                    else
                        bytes[i + 2] = 0;
                }
            }
            return bytes;
        }
        #endregion

        #region FromByteArray
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startindex"></param>
        /// <param name="totalStringLen">totalStringLen of S7-String is String-Length + 2</param>
        /// <returns></returns>
        public static string FromByteArray(byte[] bytes, int startindex, int totalStringLen, bool asBase64 = false)
        {
            byte[] sub = new byte[totalStringLen];
            Array.Copy(bytes, startindex, sub, 0, totalStringLen);
            return FromByteArray(sub, asBase64);
        }

        public static string FromByteArray(byte[] bytes, bool asBase64 = false)
        {
            if (bytes.Length <= 2)
                return "";
            if (bytes[1] <= 0)
                return "";
            int stringlength = bytes[1];
            if ((bytes.Length - 2) < stringlength)
                stringlength = bytes.Length - 2;
            byte[] valuebytes = new byte[stringlength];

            for (int i = 0; i < stringlength; i++)
            {
                valuebytes[i] = bytes[i+2];
            }
            if (asBase64)
                return Convert.ToBase64String(valuebytes);
            else
                return System.Text.Encoding.ASCII.GetString(valuebytes);
        }
        #endregion

        // privates
        private static int Asc(string s, bool asBase64 = false)
        {
            byte[] b;
            if (asBase64)
                b = Convert.FromBase64String(s);
            else
                b = System.Text.Encoding.ASCII.GetBytes(s);
            if (b.Length > 0)
                return b[0];
            return 0;
        }
    }
}
