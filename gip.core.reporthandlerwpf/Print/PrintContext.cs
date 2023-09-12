using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Documents;
using gip.core.reporthandler;

namespace gip.core.reporthandlerwpf
{
    public class PrintContext
    {

        #region ctor's
        public PrintContext()
        {
            PrintFormats = new List<PrintFormat>();
        }
        #endregion

        public byte[] Main { get; set; }

        public FlowDocument FlowDocument { get; set; }
        public NetworkStream NetworkStream { get; set; }

        public TcpClient TcpClient { get; set; }

        public Encoding Encoding { get; set; }

        public int ColumnMultiplier { get; set; }
        public int ColumnDivisor { get; set; }


        public List<PrintFormat> PrintFormats { get; set; }


        public PrintFormat GetDefaultPrintFormat()
        {
            PrintFormat printFormat = new PrintFormat();
            if (PrintFormats != null)
                foreach (var item in PrintFormats)
                {
                    if (item.FontSize != null)
                        printFormat.FontSize = item.FontSize;
                    if (item.FontWeight != null)
                        printFormat.FontWeight = item.FontWeight;
                    if (item.TextAlignment != null)
                        printFormat.TextAlignment = item.TextAlignment;
                }
            return printFormat;
        }
    }
}
