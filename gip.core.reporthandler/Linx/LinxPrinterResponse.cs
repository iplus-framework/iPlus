namespace gip.core.reporthandler
{
    public class LinxPrinterResponse
    {
        public LinxPrinterFaultCodeEnum FaultCode { get; set; }

        public LinxCommandStatusCodeEnum CommandStatusCode { get; set; }

        public LinxPrinterCommandCodeEnum CommandCode { get; set; }
    }
}
