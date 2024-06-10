using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'LP4Printer'}de{'LP4Printer'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public partial class LP4Printer : ACPrintServerBase
    {
        #region c'tors

        public LP4Printer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);

            return result;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();

            CurrentCommands = new LP4PrinterCommands(StartCharacter, EndCharacter, SeparatorCharacterTab, SeparatorCharachterCR);

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        protected readonly ACMonitorObject _61000_LockPort = new ACMonitorObject(61000);

        [ACPropertyBindingSource(9999, "Error", "en{'LP4 printer alarm'}de{'LP4 Drucker Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> LP4PrinterAlarm { get; set; }

        //todo config property
        public string PrinterName
        {
            get;
            set;
        }

        public LP4PrinterCommands CurrentCommands
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        #region Methods => Commands

        public void EnumeratePrinters()
        {

        }

        public void EnumerateLayouts()
        {

        }

        public void EnumerateLayoutVariables()
        {

        }

        #endregion

        public override PrintJob GetPrintJob(string reportName, FlowDocument flowDocument)
        {
            Encoding encoder = Encoding.ASCII;
            VBFlowDocument vBFlowDocument = flowDocument as VBFlowDocument;

            int? codePage = null;

            if (vBFlowDocument != null && vBFlowDocument.CodePage > 0)
            {
                codePage = vBFlowDocument.CodePage;
            }
            else if (CodePage > 0)
            {
                codePage = CodePage;
            }

            if (codePage != null)
            {
                try
                {
                    encoder = Encoding.GetEncoding(codePage.Value);
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), nameof(GetPrintJob), ex);
                }
            }

            LP4PrintJob printJob = new LP4PrintJob(CurrentCommands, PrinterName, reportName);
            printJob.FlowDocument = flowDocument;
            printJob.Encoding = encoder;
            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
            printJob.LayoutName = reportName;
            printJob.PrinterName = this.ACIdentifier;

            if (!string.IsNullOrEmpty(PrinterName))
                printJob.PrinterName = PrinterName;

            OnRenderFlowDocument(printJob, printJob.FlowDocument);
            return printJob;
        }

        #endregion
    }
}
