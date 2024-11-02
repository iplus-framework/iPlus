// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Text;
using System.Collections.Generic;

namespace gip.core.reporthandlerwpf
{
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxPrinterRasterDataResponse'}de{'LinxPrinterRasterDataResponse'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public class LinxPrinterRasterDataResponse
    {

        /*


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
        public byte NumberOfHeaders { get; set; }

        //[LinxByteMapping(Order = 11, Length = 1)]
        //public byte Checksum { get; set; }

        //public static byte[] GetDemoData()
        //{
        //    return new byte[]
        //    {
        //        0x1B, 0x06,
        //        0x00,
        //        0x00,
        //        0x14,
        //        0x03,
        //        0x02,
        //        0x00, 0x00, 0x00, 0x00,
        //        0x1B, 0x03,
        //        0xDE
        //    };
        //}

        private List<RasterData> _Rasters = new List<RasterData>();
        public List<RasterData> Rasters
        {
            get
            {
                return _Rasters;
            }
        }

        public class RasterData
        {
            public RasterData(byte[] segment)
            {
                ParseData(segment);
            }

            private byte _NumberOfPrintedDrops;
            public byte NumberOfPrintedDrops
            {
                get
                {
                    return _NumberOfPrintedDrops;
                }
            }

            private byte _TotalNumberDrops;
            public byte TotalNumberDrops
            {
                get
                {
                    return _TotalNumberDrops;
                }
            }

            private byte _RasterType;
            public string RasterType
            {
                get
                {
                    if (_RasterType == 0)
                        return "Null";
                    char[] chars = Encoding.ASCII.GetChars(new byte[] { _RasterType });
                    switch (chars[0])
                    {
                        case 'G':
                            return "G - General Purpose";
                        case 'H':
                            return "H - High Speed";
                        case 'B':
                            return "B - Barcode";
                        case 'D':
                            return "IJ600 printer raster type";
                        case 'S':
                            return "S Stichjed";
                        default:
                            return chars[0].ToString();
                    }
                }
            }

            private byte[] _RasterIDName;
            public string RasterIDName
            {
                get
                {
                    if (_RasterIDName == null)
                        return "Null";
                    return Encoding.ASCII.GetString(_RasterIDName);
                }
            }

            private byte[] _RasterName;
            public string RasterName
            {
                get
                {
                    if (_RasterName == null)
                        return "Null";
                    return Encoding.ASCII.GetString(_RasterName);
                }
            }

            public void ParseData(byte[] segment)
            {
                if (segment == null || segment.Length < C_SegmentLength)
                    return;

                int i = 0;
                _NumberOfPrintedDrops = segment[i]; i++;
                _RasterType = segment[1]; i++;

                _RasterIDName = new byte[16];
                Array.Copy(segment, i, _RasterIDName, 0, 16); i += 16;
                _RasterIDName = LinxHelper.RemoveZeros(_RasterIDName);

                _RasterName = new byte[16];
                Array.Copy(segment, i, _RasterName, 0, 16); i += 16;
                _RasterName = LinxHelper.RemoveZeros(_RasterName);

                _TotalNumberDrops = segment[i];
            }

            public override string ToString()
            {
                return String.Format("NoOfPrDrop:{0}, TotalDrop:{1}, RasterType:{2}, IDName:{3}, Name:{4}", NumberOfPrintedDrops, TotalNumberDrops, RasterType, RasterIDName, RasterName);
            }

            public const int C_SegmentLength = 35;
        }

        public void ParseData(byte[] responseData)
        {
            if (NumberOfHeaders == 0)
            {
                return;
            }

            for (int i = 0; i < NumberOfHeaders; i++)
            {
                int start = (i * RasterData.C_SegmentLength) + 6;
                if (start + RasterData.C_SegmentLength > responseData.Length - 1)
                    break;
                byte[] rasterBlock = new byte[RasterData.C_SegmentLength];
                Array.Copy(responseData, start, rasterBlock, 0, RasterData.C_SegmentLength);
                RasterData rasterData = new RasterData(rasterBlock);
                if (rasterData.RasterIDName == "Null")
                    continue;
                _Rasters.Add(new RasterData(rasterBlock));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (RasterData rasterData in _Rasters)
            {
                sb.AppendLine(rasterData.ToString());
            }
            return sb.ToString();
        }
    }

}
