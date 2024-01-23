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

        #region Properies

        public LinxPrintJobTypeEnum LinxPrintJobType { get; set; }

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


        private List<byte[]> _RemoteFieldValues = new List<byte[]>();
        public List<byte[]> RemoteFieldValues
        {
            get
            {
                return _RemoteFieldValues;
            }
        }

        public List<LinxField> LinxFields { get; set; } = new List<LinxField>();

        #endregion

        #region Methods

        public string GetJobInfo()
        {
            return $"PrintJobID: {PrintJobID} Name:{Name}";
        }

        #endregion
    }
}
