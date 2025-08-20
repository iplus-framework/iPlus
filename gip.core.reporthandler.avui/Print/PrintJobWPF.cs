using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using gip.core.reporthandler;

namespace gip.core.reporthandler.avui
{
    public class PrintJobWPF : PrintJob
    {
        #region ctor's
        public PrintJobWPF() : base()
        {
            PrintFormats = new List<PrintFormat>();
        }
        #endregion

        public FlowDocument FlowDocument { get; set; }

        public List<PrintFormat> PrintFormats { get; set; }


        public PrintFormat GetDefaultPrintFormat()
        {
            PrintFormat printFormat = new PrintFormat();
            if (PrintFormats != null)
            {
                foreach (var item in PrintFormats)
                {
                    if (item.FontSize != null)
                        printFormat.FontSize = item.FontSize;
                    if (item.FontWeight != null)
                        printFormat.FontWeight = item.FontWeight;
                    if (item.TextAlignment != null)
                        printFormat.TextAlignment = item.TextAlignment;
                }
            }
            return printFormat;
        }
    }
}
