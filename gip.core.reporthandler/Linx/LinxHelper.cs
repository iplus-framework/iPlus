using System;
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
    }
}
