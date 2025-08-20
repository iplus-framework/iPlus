// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;

namespace gip.core.reporthandler.avui
{
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxPrinterCompleteStatusResponse'}de{'LinxPrinterCompleteStatusResponse'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public class LinxPrinterCompleteStatusResponse
    {

        /*
            Printer Reply:
            1B 06	;ESC ACK sequence
            00	;P-Status - No printer errors
            00	;C-Status - No command errors
            14	;Command ID sent
            03	;Jet State - Jet stopped
            02	;Print State - Idle
            00 00 00 00	;32-bit Error Mask - No errors
            1B 03	;ESC ETX sequence
            DE	;Checksum

         */

        [LinxByteMapping(Order = 1, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ESC)]
        public LinxASCIControlCharacterEnum StartCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 2, Length = 1)]
        public LinxASCIControlCharacterEnum StartCode02 { get; set; } = LinxASCIControlCharacterEnum.ACK;

        [LinxByteMapping(Order = 3, Length = 1)]
        public LinxPrinterFaultCodeEnum P_Status { get; set; }

        [LinxByteMapping(Order = 4, Length = 1)]
        public LinxCommandStatusCodeEnum C_Status { get; set; }

        [LinxByteMapping(Order = 5, Length = 1)]
        public LinxPrinterCommandCodeEnum CommandID { get; set; }

        [LinxByteMapping(Order = 6, Length = 1)]
        public LinxJetStateEnum JetState { get; set; }

        [LinxByteMapping(Order = 7, Length = 1)]
        public LinxPrintStateEnum PrintState { get; set; }

        [LinxByteMapping(Order = 8, Length = 4)]
        public int ErrorMask { get; set; }

        [LinxByteMapping(Order = 9, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ESC)]
        public LinxASCIControlCharacterEnum EndCode01 { get; set; } = LinxASCIControlCharacterEnum.ESC;

        [LinxByteMapping(Order = 10, Length = 1, DefaultValue = (byte)LinxASCIControlCharacterEnum.ETX)]
        public LinxASCIControlCharacterEnum EndCode02 { get; set; } = LinxASCIControlCharacterEnum.ETX;

        [LinxByteMapping(Order = 11, Length = 1)]
        public byte Checksum { get; set; }

        public static byte[] GetDemoData()
        {
            return new byte[]
            {
                0x1B, 0x06,
                0x00,
                0x00,
                0x14,
                0x03,
                0x02,
                0x00, 0x00, 0x00, 0x00,
                0x1B, 0x03,
                0xDE
            };
        }

    }
}
