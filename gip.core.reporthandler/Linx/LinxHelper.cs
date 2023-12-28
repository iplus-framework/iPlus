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

        public static void Write(this MemoryStream ms, short command)
        {
            ms.Write(BitConverter.GetBytes(command), 0, BitConverter.GetBytes(command).Count());
        }

        public static void Write(this MemoryStream ms, LinxASCIControlCharacterEnum command)
        {
            ms.Write(BitConverter.GetBytes((byte)command), 0, BitConverter.GetBytes((byte)command).Count());
        }

        public static void Write(this MemoryStream ms, LinxPrinterCommandCodeEnum commandCode)
        {
            ms.Write(BitConverter.GetBytes((byte)commandCode), 0, BitConverter.GetBytes((byte)commandCode).Count());
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


        public static byte GetCheckSum(byte[] input)
        {
            // LinxASCIControlCharacterEnum.ESC not calculated
            int sumValues = input.Where(c => c != (byte)LinxASCIControlCharacterEnum.ESC).Sum(c => c);
            int checkSum = sumValues & 0x0FF;
            checkSum = 0x100 - checkSum;
            return (byte)checkSum;
        }

        public static bool ValidateChecksum(byte[] dataWithCheckSum)
        {
            bool isValid = false;
            if (dataWithCheckSum != null && dataWithCheckSum.Length > 2)
            {
                byte[] data = new byte[dataWithCheckSum.Length - 1];
                Array.Copy(dataWithCheckSum, 0, data, 0, dataWithCheckSum.Length - 1);

                byte inputChecksum = dataWithCheckSum[dataWithCheckSum.Length - 1];

                byte calcCheckSum = GetCheckSum(data);

                isValid = inputChecksum == calcCheckSum;
            }
            return isValid;
        }
    }
}
