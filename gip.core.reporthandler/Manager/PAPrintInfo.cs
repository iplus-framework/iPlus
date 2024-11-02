// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandler
{
    public class PAPrintInfo
    {
        public PAPrintInfo(string bsoACUrl, PrinterInfo printerInfo, string reportACIdentifier)
        {
            BSOACUrl = bsoACUrl;
            PrinterInfo = printerInfo;
            ReportACIdentifier = reportACIdentifier;
        }

        public string BSOACUrl { get; private set; }

        public string ReportACIdentifier { get; private set; }

        public PrinterInfo PrinterInfo { get; private set; }
    }
}
