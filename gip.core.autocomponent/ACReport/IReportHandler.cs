using System.Collections.Generic;
using System.Data;
using gip.core.datamodel;
using System.Linq;

namespace gip.core.autocomponent
{
    public interface IReportHandler : IACComponent
    {
        Msg Print(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies = 1, bool skipPrinterCheck = true);
        void Preview(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data);
        void Design(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data);
    }
}
