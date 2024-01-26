using DocumentFormat.OpenXml.Vml;
using gip.core.layoutengine;
using System.Collections.Generic;

namespace gip.core.reporthandler
{
    public class LinxPrintJob : PrintJob
    {

        #region ctor's
        public LinxPrintJob()
        {
        }


        #endregion

        #region Properies

        public string RasterName
        {
            get
            {
                string rasterName = null;
                VBFlowDocument vBFlowDocument = this.FlowDocument as VBFlowDocument;
                if (vBFlowDocument != null)
                    rasterName = vBFlowDocument.Custom01;
                if (string.IsNullOrEmpty(rasterName))
                    rasterName = "16 GEN STD";
                return rasterName;
            }
        }

        public int CharacterWidth
        {
            get
            {
                int val = -1;
                VBFlowDocument vBFlowDocument = this.FlowDocument as VBFlowDocument;
                if (vBFlowDocument != null)
                    val = vBFlowDocument.CustomInt01;
                return val;
            }
        }

        public bool IsOneLine
        {
            get
            {
                string value = null;
                VBFlowDocument vBFlowDocument = this.FlowDocument as VBFlowDocument;
                if (vBFlowDocument != null)
                    value = vBFlowDocument.Custom02;
                if (!string.IsNullOrEmpty(value) && value == "OneLine")
                    return true;
                return false;
            }
        }

        public int InterCharSpace
        {
            get
            {
                int val = -1;
                VBFlowDocument vBFlowDocument = this.FlowDocument as VBFlowDocument;
                if (vBFlowDocument != null)
                    val = vBFlowDocument.CustomInt02;
                return val;
            }
        }

        public int FieldHeightDrop
        {
            get
            {
                int val = -1;
                VBFlowDocument vBFlowDocument = this.FlowDocument as VBFlowDocument;
                if (vBFlowDocument != null)
                    val = vBFlowDocument.CustomInt03;
                return val;
            }
        }

        //public LinxPrintJobTypeEnum LinxPrintJobType { get; set; }

        public class Telegram
        {
            public Telegram(LinxPrintJobTypeEnum jobType, byte[] packet)
            {
                LinxPrintJobType = jobType;
                Packet = packet;
            }

            public LinxPrintJobTypeEnum LinxPrintJobType { get; set; }
            public byte[] Packet { get; set; }
        }

        private List<Telegram> _PacketsForPrint;
        public List<Telegram> PacketsForPrint
        {
            get
            {
                if (_PacketsForPrint == null)
                {
                    _PacketsForPrint = new List<Telegram>();
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
