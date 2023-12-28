using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.reporthandler
{
    /// <summary>
    /// Helper class for build linx messages
    /// </summary>
    public static class LinxHelper
    {
        public static void Write(this MemoryStream ms, LinxASCIControlCharacterEnum command)
        {
            ms.Write(BitConverter.GetBytes((short)command), 0, BitConverter.GetBytes((short)command).Count());
        }

        public static void Write(this MemoryStream ms, LinxPrinterCommandCodeEnum commandCode)
        {
            ms.Write(BitConverter.GetBytes((short)commandCode), 0, BitConverter.GetBytes((short)commandCode).Count());
        }


        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static byte[] Combine(List<byte[]> arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
