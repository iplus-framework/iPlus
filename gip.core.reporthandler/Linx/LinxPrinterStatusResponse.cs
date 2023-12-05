namespace gip.core.reporthandler
{
    public class LinxPrinterStatusResponse
    {


        [LinxByteMapping(Order = 1, Length = 2)]
        public LinxASCIControlCharacterEnum StartCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 2, Length = 2)]
        public LinxASCIControlCharacterEnum StartCode02 { get; set; } = LinxASCIControlCharacterEnum.ACK;

        [LinxByteMapping(Order = 3, Length = 2)]
        public LinxPrinterFaultCodeEnum P_Status { get; set; }

        [LinxByteMapping(Order = 4, Length = 2)]
        public LinxCommandStatusCodeEnum C_Status { get; set; }

        [LinxByteMapping(Order = 5, Length = 2)]
        public LinxPrinterCommandCodeEnum CommandID { get; set; }

        [LinxByteMapping(Order = 6, Length = 2)]
        public LinxJetStateEnum JetState { get; set; }

        [LinxByteMapping(Order = 7, Length = 2)]
        public LinxPrintStateEnum PrintState { get; set; }

        [LinxByteMapping(Order = 8, Length = 4)]
        public int ErrorMask { get; set; }

        [LinxByteMapping(Order = 9, Length = 2)]
        public LinxASCIControlCharacterEnum EndCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 10, Length = 2)]
        public LinxASCIControlCharacterEnum EndCode02 { get; set; } = LinxASCIControlCharacterEnum.ETX;

    }
}
