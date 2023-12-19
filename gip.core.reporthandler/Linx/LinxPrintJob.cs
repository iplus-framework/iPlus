using System.Collections.Generic;

namespace gip.core.reporthandler
{
    public class LinxPrintJob : PrintJob
    {

        #region ctor's

        public LinxPrintJob() : base()
        {

        }

        #endregion
        
        private List<byte[]> _PacketsForPrint;
        public List<byte[]> PacketsForPrint
        {
            get
            {
                if (_PacketsForPrint == null)
                {
                    _PacketsForPrint = new List<byte[]>();
                }
                return _PacketsForPrint;
            }
        }
    }
}
