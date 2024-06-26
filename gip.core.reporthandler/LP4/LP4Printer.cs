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

            responseString = responseString.Trim();

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
                        ProcessEnumPrinters(responseString, lp4PrintJob);
                        break;
                    }
                case LP4PrinterCommands.C_EnumLayouts:
                    {
                        ProcessEnumLayouts(responseString, lp4PrintJob);
                        break;
                    }
                case LP4PrinterCommands.C_EnumLayoutVariables:
                    {
                        ProcessEnumLayoutVariables(responseString, lp4PrintJob);
                        break;
                    }
                case LP4PrinterCommands.C_PrinterStatus:
                    {
                        ProcessPrinterStatus(responseString, lp4PrintJob);
                        break;
                    }
                case LP4PrinterCommands.C_ResetCommand:
                    {
                        ProcessPrinterStatus(responseString, lp4PrintJob);
                        break;
                    }
                case LP4PrinterCommands.C_PrintCommand:
                    {
                        ProcessPrintCommand(responseString, lp4PrintJob);
                        break;
                    }
            }
        }

        private void ProcessEnumPrinters(string response, LP4PrintJob lp4PrintJob)
        {
            //<STX>E:<CR>Druckername1<TAB>Typ1<TAB>Comm1<CR>Druckername2<TAB>Typ2<TAB>Comm2<CR>…<ETX>

            if (string.IsNullOrEmpty(response))
                return;

            string[] printers = response.Split(SeparatorCharachterCR);

            string command = printers.FirstOrDefault();
            if (command == string.Format("{0}:", CurrentCommands.EnumPrinters))
            {
                List<string> printerQueryResult = new List<string>();

                string[] printerInfos = printers.Skip(1).ToArray();

                foreach (string printerInfo in printerInfos)
                {
                    string printer = "";
                    string[] printerInfoParts = printerInfo.Split(SeparatorCharacterTab);
                    if (printerInfoParts.Count() > 2)
                    {
                        printer = printerInfoParts[0];

                        short driverType;
                        if (short.TryParse(printerInfoParts[1], out driverType))
                        {
                            LP4PrinterType printerType = (LP4PrinterType)driverType;
                            printer += string.Format(" {0}", printerType);
                        }

                        short commType;
                        if (short.TryParse(printerInfoParts[2], out commType))
                        {
                            LP4CommType printerCommType = (LP4CommType)commType;
                            printer += string.Format(" {0}", printerCommType);
                        }
                    }

                    printerQueryResult.Add(printer);
                }

                PrinterResponse.ValueT = printerQueryResult.ToString();
            }
        }

        private void ProcessEnumLayouts(string response, LP4PrintJob lp4PrintJob)
        {
            //<STX>L:<CR>Layoutname1<CR>Layoutname2<CR>…<ETX>

            if (string.IsNullOrEmpty(response))
                return;

            string[] layouts = response.Split(SeparatorCharachterCR);

            string command = layouts.FirstOrDefault();
            if (command == string.Format("{0}:", CurrentCommands.EnumLayouts))
            {
                layouts = layouts.Skip(1).ToArray();
                PrinterResponse.ValueT = layouts.ToString();
            }
        }

        private void ProcessEnumLayoutVariables(string response, LP4PrintJob lp4PrintJob)
        {
            //<STX>V:Etikettenlayout<CR>Varname1<CR>Varname2<CR>…<ETX>

            if (string.IsNullOrEmpty(response))
                return;

            string[] variables = response.Split(SeparatorCharachterCR);

            string command = variables.FirstOrDefault().Take(2).ToString();
            if (command == CurrentCommands.EnumLayoutVariables)
            {
                PrinterResponse.ValueT = variables.ToString();
            }
       }

        private void ProcessPrinterStatus(string response, LP4PrintJob lp4PrintJob)
        {
            //TODO
            PrinterResponse.ValueT = response;
        }

        private void ProcessResetCommand(string response, LP4PrintJob lp4PrintJob)
        {
            PrinterResponse.ValueT = response;
        }

        private void ProcessPrintCommand(string response, LP4PrintJob lp4PrintJob)
        {
            //<STX>P:Druckername<TAB>OK<ETX>
            //<STX>P:Druckername<TAB>ERROR<TAB>Fehlernummer<ETX>

            if (string.IsNullOrEmpty(response))
                return;

            string[] parts = response.Split(SeparatorCharacterTab);

            string command = parts.FirstOrDefault().Take(2).ToString();
            if (command == CurrentCommands.PrintCommand)
            {
                if (parts.Count() > 2)
                {
                    PrinterResponse.ValueT = parts.ToString();
                }
            }
        }

        #endregion
    }
}
