// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Collections.Generic;
using System.Data;
using gip.core.datamodel;
using System.Linq;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface IReportHandler : IACComponent
    {
        Msg Print(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies = 1, int maxPrintJobsInSpooler = 0);
        Task Preview(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data);
        Task Design(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data);
    }
}
