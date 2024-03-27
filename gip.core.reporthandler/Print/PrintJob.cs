using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    public class PrintJob
    {
        public Guid PrintJobID { get;set; }
        public string Name { get;set; }
        public DateTime InsertDate { get;set; }

        public PrintJobStateEnum State { get;set; }

        #region ctor's
        public PrintJob()
        {
            PrintFormats = new List<PrintFormat>();
            PrintJobID = Guid.NewGuid();
            InsertDate = DateTime.Now;
            State = PrintJobStateEnum.New;
        }
        #endregion

        public byte[] Main { get; set; }

        public FlowDocument FlowDocument { get; set; }

        public Encoding Encoding { get; set; }

        public int ColumnMultiplier { get; set; }
        public int ColumnDivisor { get; set; }


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
