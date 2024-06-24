using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

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

        public string CurrentPrinterName
        {
            get
            {
                if (string.IsNullOrEmpty(PrinterName))
                    return ACIdentifier;

                return PrinterName;
            }
        }

        public LP4PrinterCommands CurrentCommands
        {
            get;
            private set;
        }

        private Queue<LP4PrintJob> _LP4PrintJobs;
        public Queue<LP4PrintJob> LP4PrintJobs
        {
            get
            {
                if (_LP4PrintJobs == null)
                {
                    _LP4PrintJobs = new Queue<LP4PrintJob>();
                }
                return _LP4PrintJobs;
            }
        }

        [ACPropertyBindingSource]
        public IACContainerTNet<string> PrinterResponse
        {
            get;
            set;
        }

        #endregion

        #region Methods

        #region Methods => Commands

        public void EnumeratePrinters()
        {
            LP4PrintJob lp4PrintJob = new LP4PrintJob(CurrentCommands, null);
            lp4PrintJob.PrinterTask = CurrentCommands.EnumPrinters;

            
        }

        public void EnumerateLayouts()
        {
            LP4PrintJob lp4PrintJob = new LP4PrintJob(CurrentCommands, null);
            lp4PrintJob.PrinterTask = CurrentCommands.EnumLayouts;
        }

        public void EnumerateLayoutVariables()
        {
            LP4PrintJob lp4PrintJob = new LP4PrintJob(CurrentCommands, null, "TODO");
            lp4PrintJob.PrinterTask = CurrentCommands.EnumLayoutVariables;
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

            LP4PrintJob printJob = new LP4PrintJob(CurrentCommands, CurrentPrinterName, reportName);
            printJob.FlowDocument = flowDocument;
            printJob.Encoding = encoder;
            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
            printJob.PrinterTask = CurrentCommands.PrintCommand;

            OnRenderFlowDocument(printJob, printJob.FlowDocument);
            return printJob;
        }

        public bool ProcessJob(LP4PrintJob lp4PrintJob)
        {
            bool success = false;
            try
            {
                if (lp4PrintJob.State == PrintJobStateEnum.New)
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        lp4PrintJob.State = PrintJobStateEnum.InProcess;
                    }

                    byte[] printerCommand = lp4PrintJob.GetPrinterCommand();
                    if (printerCommand == null && lp4PrintJob.PrintJobError != null)
                    {
                        //todo: error
                        return false;
                    }

                    bool requestSuccess = Request(printerCommand);
                    if (requestSuccess)
                    {
                        Thread.Sleep(ReceiveTimeout);
                        (bool responseSuccess, byte[] responseData) = Response(lp4PrintJob);

                        if (responseSuccess)
                        {
                            ProcessResponse(responseData, lp4PrintJob);
                        }
                        else
                        {
                            //TODO: error
                            return false;
                        }

                    }

                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        lp4PrintJob.State = PrintJobStateEnum.Done;
                    }
                }
            }
            catch (Exception ex)
            {
                LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LP4PrinterAlarm), ex.Message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(ProcessJob)}(70)", ex);
                }
                OnNewAlarmOccurred(LP4PrinterAlarm, ex.Message, true);
            }

            ClosePort();
            return success;
        }

        private void ProcessResponse(byte[] response, LP4PrintJob lp4PrintJob)
        {
            if (lp4PrintJob == null)
                return;

            if (response == null)
                return;

            string responseString = lp4PrintJob.Encoding.GetString(response);

            char start = responseString.FirstOrDefault();
            char end = responseString.LastOrDefault();

            if (start != StartCharacter)
            {
                //TODO:error
                return;
            }

            if (end != EndCharacter)
            {
                //TODO:error
                return;
            }

            responseString = responseString.Substring(1, responseString.Length - 1);

            switch (lp4PrintJob.PrinterTask)
            {
                case LP4PrinterCommands.C_EnumPrinters:
                    {
                        break;
                    }
                case LP4PrinterCommands.C_EnumLayouts:
                    {
                        break;
                    }
                case LP4PrinterCommands.C_EnumLayoutVariables:
                    {
                        break;
                    }
                case LP4PrinterCommands.C_PrinterStatus:
                    {
                        break;
                    }
                case LP4PrinterCommands.C_ResetCommand:
                    {
                        break;
                    }
                case LP4PrinterCommands.C_PrintCommand:
                    {
                        break;
                    }
            }
        }

        private void ProcessEnumPrinters(string response, LP4PrintJob lp4PrintJob)
        {
            
        }

        #endregion
    }
}
