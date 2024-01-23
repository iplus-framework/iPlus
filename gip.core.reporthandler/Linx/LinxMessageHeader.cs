using System.Collections.Generic;

namespace gip.core.reporthandler
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
        public byte[] MessageLengthInBytes { get; set; }
        public byte[] MessageLengthInRasters { get; set; }
        public byte EHTSetting { get; set; }

        public byte[] InterRasterWidth { get; set; }
        public byte[] PrintDelay { get; set; }
        public byte[] MessageName { get; set; }
        public byte[] RasterName { get; set; }

        #endregion

        #region Methods

        public List<byte> GetBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.Add(NumberOfMessages);
            bytes.AddRange(MessageLengthInBytes);
            bytes.AddRange(MessageLengthInRasters);
            bytes.Add(EHTSetting);
            bytes.AddRange(InterRasterWidth);
            bytes.AddRange(PrintDelay);
            bytes.AddRange(MessageName);
            bytes.AddRange(RasterName);

            return bytes;
        }

        #endregion

    }
}
