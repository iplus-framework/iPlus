namespace gip.core.reporthandler
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
