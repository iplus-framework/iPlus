// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.reporthandler;
using gip.core.reporthandler.avui.Flowdoc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using static gip.core.reporthandler.avui.LinxPrintJob;

namespace gip.core.reporthandler.avui
{
    public partial class LinxPrinter
    {

        #region Properties
        private Queue<LinxPrintJob> _LinxPrintJobs;
        public Queue<LinxPrintJob> LinxPrintJobs
        {
            get
            {
                if (_LinxPrintJobs == null)
                {
                    _LinxPrintJobs = new Queue<LinxPrintJob>();
                }
                return _LinxPrintJobs;
            }
        }
        #endregion

        #region Methods
        public void EnqueueJob(LinxPrintJob linxPrintJob, string name)
        {
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(StartPrint), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        public override PrintJob GetPrintJob(string reportName, FlowDocument flowDocument)
        {
            Encoding encoder = Encoding.Unicode;
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

            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.FlowDocument = flowDocument;
            linxPrintJob.Name = string.IsNullOrEmpty(flowDocument.Name) ? reportName : flowDocument.Name;
            linxPrintJob.Encoding = encoder;
            linxPrintJob.ColumnMultiplier = 1;
            linxPrintJob.ColumnDivisor = 1;
            OnRenderFlowDocument(linxPrintJob, linxPrintJob.FlowDocument);
            return linxPrintJob;
        }

        public override bool SendDataToPrinter(PrintJob printJob)
        {
            bool success = false;

            if (printJob != null)
            {
                LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;

                if (linxPrintJob != null)
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(SendDataToPrinter) + "(100)", $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                        LinxPrintJobs.Enqueue(linxPrintJob);
                    }
                }
            }

            return success;
        }


        public bool ProcessJob(LinxPrintJob linxPrintJob)
        {
            bool success = false;
            try
            {
                if (linxPrintJob.State == PrintJobStateEnum.New)
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        linxPrintJob.State = PrintJobStateEnum.InProcess;
                    }

                    foreach (Telegram telegram in linxPrintJob.PacketsForPrint)
                    {
                        bool requestSuccess = Request(telegram);
                        if (requestSuccess)
                        {
                            Thread.Sleep(ReceiveTimeout);
                            (bool responseSuccess, byte[] responseData) = Response(linxPrintJob, telegram);
                            // TODO: Linxmapping according Type of Telegram!! Temporary workaround for DeleteReport
                            if (   telegram.LinxPrintJobType != LinxPrintJobTypeEnum.DeleteReport
                                && responseSuccess
                                && responseData != null)
                            {
                                if (telegram.LinxPrintJobType == LinxPrintJobTypeEnum.CheckStatus)
                                {
                                    (MsgWithDetails msgWithDetails, LinxPrinterCompleteStatusResponse response) = LinxMapping<LinxPrinterCompleteStatusResponse>.Map(responseData);
                                    if (ValidateMessage(msgWithDetails) && response != null)
                                        PrinterCompleteStatus.ValueT = response;
                                }
                                else if (telegram.LinxPrintJobType == LinxPrintJobTypeEnum.RasterData)
                                {
                                    (MsgWithDetails msgWithDetails, LinxPrinterRasterDataResponse response) = LinxMapping<LinxPrinterRasterDataResponse>.Map(responseData);
                                    if (ValidateMessage(msgWithDetails) && response != null)
                                    {
                                        response.ParseData(responseData);
                                        string dumpedResult = response.ToString();
                                        Messages.LogInfo(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(120)", dumpedResult);
                                        OnNewAlarmOccurred(LinxPrinterAlarm, dumpedResult, true);
                                    }
                                }
                                else
                                {
                                    (MsgWithDetails msgWithDetails, LinxPrinterStatusResponse response) = LinxMapping<LinxPrinterStatusResponse>.Map(responseData);
                                    if (ValidateMessage(msgWithDetails) && response != null)
                                    {
                                        if (response.P_Status > 0 || response.C_Status > 0)
                                        {
                                            using (ACMonitor.Lock(_61000_LockPort))
                                            {
                                                linxPrintJob.State = PrintJobStateEnum.InAlarm;
                                            }

                                            string message = $"JobID: {linxPrintJob.PrintJobID}| Printer return P_Status: {response.P_Status}; C_Status: {response.C_Status}";

                                            if (response.P_Status > 0)
                                            {
                                                LinxPrintErrorEnum printErrorEnum = LinxPrintErrorEnum.Remote_alarm;
                                                if (Enum.TryParse<LinxPrintErrorEnum>(response.P_Status.ToString(), out printErrorEnum))
                                                {
                                                    message += System.Environment.NewLine;
                                                    message += "Printer error code: " + printErrorEnum.ToString();
                                                }
                                            }

                                            if (response.C_Status > 0)
                                            {
                                                LinxCommandStatusCodeEnum commandStatusCode = LinxCommandStatusCodeEnum.Data_overrun;
                                                if (Enum.TryParse<LinxCommandStatusCodeEnum>(response.C_Status.ToString(), out commandStatusCode))
                                                {
                                                    message += System.Environment.NewLine;
                                                    message += "Command status code: " + commandStatusCode.ToString();
                                                }
                                            }

                                            LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                                            if (IsAlarmActive(nameof(LinxPrinterAlarm), message) == null)
                                            {
                                                Messages.LogError(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(140)", message);
                                            }
                                            OnNewAlarmOccurred(LinxPrinterAlarm, message, true);
                                            if (PrinterCompleteStatus.ValueT != null)
                                            {
                                                PrinterCompleteStatus.ValueT.P_Status = response.P_Status;
                                                PrinterCompleteStatus.ValueT.C_Status = response.C_Status;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (ACMonitor.Lock(_61000_LockPort))
                            {
                                linxPrintJob.State = PrintJobStateEnum.InAlarm;
                            }
                            break;
                        }
                    }


                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        linxPrintJob.State = PrintJobStateEnum.Done;
                    }
                }
            }
            catch (Exception ex)
            {
                LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LinxPrinterAlarm), ex.Message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(70)", ex);
                }
                OnNewAlarmOccurred(LinxPrinterAlarm, ex.Message, true);
            }

            ClosePort();
            return success;
        }


        protected bool ValidateMessage(MsgWithDetails msgWithDetails)
        {
            if (msgWithDetails == null)
                return false;
            if (!msgWithDetails.IsSucceded())
            {
                LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LinxPrinterAlarm), msgWithDetails.DetailsAsText) == null)
                {
                    Messages.LogError(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(120)", msgWithDetails.DetailsAsText);
                }
                OnNewAlarmOccurred(LinxPrinterAlarm, msgWithDetails.DetailsAsText, true);
                return false;
            }
            return true;
        }
        #endregion

    }
}
