using gip.core.datamodel;

namespace gip.core.reporthandler
{

    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxPrinterStatusResponse'}de{'LinxPrinterStatusResponse'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public class LinxPrinterStatusResponse
    {

        /*
            Printer Reply:
            1B 06	;ESC ACK sequence
            00	;P-Status - No printer errors
            00	;C-Status - No command errors
            19	;Command ID sent
            1B 03	;ESC ETX sequence
            DE	;Checksum
         */

        [LinxByteMapping(Order = 1, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ESC)]
        public LinxASCIControlCharacterEnum StartCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 2, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ACK)]
        public LinxASCIControlCharacterEnum StartCode02 { get; set; } = LinxASCIControlCharacterEnum.ACK;

        [LinxByteMapping(Order = 3, Length = 1)]
        public LinxPrinterFaultCodeEnum P_Status { get; set; }

        [LinxByteMapping(Order = 4, Length = 1)]
        public LinxCommandStatusCodeEnum C_Status { get; set; }

        [LinxByteMapping(Order = 5, Length = 1)]
        public LinxPrinterCommandCodeEnum CommandID { get; set; }

        [LinxByteMapping(Order = 6, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ESC)]
        public LinxASCIControlCharacterEnum EndCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 7, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ETX)]
        public LinxASCIControlCharacterEnum EndCode02 { get; set; } = LinxASCIControlCharacterEnum.ETX;

        [LinxByteMapping(Order = 8, Length = 1)]
        public byte Checksum { get; set; }


        public static byte[] GetDemoData()
        {
            return new byte[] 
            { 
                0x1B, 0x06, 
                0x00, 
                0x00, 
                0x19, 
                0x1B, 0x03, 
                0xDE 
            };
        }

    }
}
