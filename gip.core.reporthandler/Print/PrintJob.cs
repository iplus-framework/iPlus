using System;
using System.Collections.Generic;
using System.Text;

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
            PrintJobID = Guid.NewGuid();
            InsertDate = DateTime.Now;
            State = PrintJobStateEnum.New;
        }
        #endregion

        public byte[] Main { get; set; }


        public Encoding Encoding { get; set; }

        public int ColumnMultiplier { get; set; }
        public int ColumnDivisor { get; set; }
    }
}
