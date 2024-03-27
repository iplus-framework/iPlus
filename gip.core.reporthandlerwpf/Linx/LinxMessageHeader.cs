using System.Collections.Generic;

namespace gip.core.reporthandlerwpf
{
    public class LinxMessageHeader
    {
        #region const
        public const int DefaultHeaderLength = 41;
        #endregion
        #region ctor's
        public LinxMessageHeader()
        {
            EHTSetting = 0x06;
            InterRasterWidth = new byte[] { 0x00, 0x00 };
            PrintDelay = new byte[] { 0x10, 0x00 };
        }
        #endregion

        #region Properties

        // *** Message Header - 41 bytes ***
        //Size of Message	2 bytes
        //Size of Pixel Image in Rasters	2 bytes
        //EHT Setting	1 byte
        //Print Width	2 bytes
        //Print Delay	2 bytes
        //Message Name	15 bytes + null
        //Raster Name	15 bytes + null


        // *** From sample ***
        //01	;Number of messages
        //ED 00	; Message length in bytes
        //CF 00	; Message length in rasters
        //06	;EHT setting
        //00 00	; Inter-raster width
        //10 00	; Print delay
        //4C 49 4E 58 20 54 45 53	;Message name - LINX TEST
        //54 00 00 00 00 00 00 00
        //31 36 20 47 45 4E 20 53	; Raster name - 16 GEN STD
        //54 44 00 00 00 00 00 00

        public byte NumberOfMessages { get; set; }
        public byte[] MessageLengthInBytes { get; } = new byte[2];
        public byte[] MessageLengthInRasters { get; } = new byte[2];
        public byte EHTSetting { get; set; }

        public byte[] InterRasterWidth { get; } = new byte[2];
        public byte[] PrintDelay { get; } = new byte[2];
        public byte[] MessageName { get; } = new byte[16];
        public byte[] RasterName { get; } = new byte[16];

        #endregion

        #region Methods

        public List<byte[]> GetBytes()
        {
            List<byte[]> bytes = new List<byte[]>();

            bytes.Add(new byte[] { NumberOfMessages });
            bytes.Add(MessageLengthInBytes);
            bytes.Add(MessageLengthInRasters);
            bytes.Add(new byte[] { EHTSetting });
            bytes.Add(InterRasterWidth);
            bytes.Add(PrintDelay);
            bytes.Add(MessageName);
            bytes.Add(RasterName);

            return bytes;
        }

        #endregion

    }
}
