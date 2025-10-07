using DocumentFormat.OpenXml.Vml;
using gip.core.layoutengine;
using System;
using System.Collections.Generic;
using System.IO;

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

        public void WriteTelegramsToFiles(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("Root path cannot be null or empty", nameof(rootPath));

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            for (int i = 0; i < PacketsForPrint.Count; i++)
            {
                var telegram = PacketsForPrint[i];
                string filename = $"{timestamp}_{telegram.LinxPrintJobType}_{i:D3}.bin";
                string filePath = System.IO.Path.Combine(rootPath, filename);

                File.WriteAllBytes(filePath, telegram.Packet);
            }
        }

        #endregion
    }
}
