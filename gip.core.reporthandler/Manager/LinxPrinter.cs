using DocumentFormat.OpenXml.ExtendedProperties;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using static ClosedXML.Excel.XLPredefinedFormat;
using System.Windows.Forms;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'LinxPrinter'}de{'LinxPrinter'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class LinxPrinter : ACPrintServerBase
    {

        #region ctor's
        public LinxPrinter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            _ = IPAddress;
            return true;
        }

        public override bool ACPostInit()
        {
            bool basePostInit = base.ACPostInit();

            _ShutdownEvent = new ManualResetEvent(false);
            _PollThread = new ACThread(Poll);
            _PollThread.Name = "ACUrl:" + this.GetACUrl() + ";Poll();";
            _PollThread.Start();

            return basePostInit;
        }


        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool acDeinit = base.ACDeInit(deleteACClassTask);

            if (_PollThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_PollThread.Join(5000))
                    _PollThread.Abort();
                _PollThread = null;
            }

            return acDeinit;
        }



        #endregion

        #region Settings

        [ACPropertyInfo(true, 200, DefaultValue = false)]
        public bool UseRemoteReport { get; set; }

        #endregion

        #region Thread
        protected ManualResetEvent _ShutdownEvent;
        ACThread _PollThread;
        private void Poll()
        {
            string jobInfo = "";
            try
            {
                while (!_ShutdownEvent.WaitOne(2000, false))
                {
                    _PollThread.StartReportingExeTime();

                    LinxPrintJob linxPrintJob = null;
                    //LinxPrintJob linxPrintJob = LinxPrintJobs.Where(c => c.State == PrintJobStateEnum.New).OrderBy(c => c.InsertDate).FirstOrDefault();
                    using (ACMonitor.Lock(this.LockMemberList_20020))
                    {
                        if (LinxPrintJobs.Count <= 0)
                            break;
                        linxPrintJob = LinxPrintJobs.Dequeue();
                    }
                    if (linxPrintJob != null)
                    {
                        jobInfo = linxPrintJob.GetJobInfo();
                        ProcessJob(linxPrintJob);
                        Messages.LogInfo(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Poll)}(10)", $"Job {jobInfo} processed!");
                    }
                    _PollThread.StopReportingExeTime();
                }
            }
            catch (ThreadAbortException ec)
            {
                string message = $"Exception processing job {jobInfo}! Exception: {ec.Message}";
                LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LinxPrinterAlarm), message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Poll)}(20)", ec);
                }
                OnNewAlarmOccurred(LinxPrinterAlarm, message, true);
            }
        }
        #endregion

        #region Serial Communication

        protected readonly ACMonitorObject _61000_LockPort = new ACMonitorObject(61000);

        [ACPropertyInfo(true, 411, "Config", "en{'Serial-Communication on'}de{'Serial-Kommunikation ein'}", DefaultValue = false)]
        public bool SerialCommEnabled { get; set; }


        protected SerialPort _serialPort = null;
        [ACPropertyInfo(9999)]
        public SerialPort SerialPort
        {
            get
            {
                using (ACMonitor.Lock(_61000_LockPort))
                {
                    return _serialPort;
                }
            }
        }

        /// <summary>
        /// PortName
        /// </summary>
        [ACPropertyInfo(true, 412, DefaultValue = "COM1")]
        public string PortName { get; set; }

        /// <summary>
        /// BaudRate
        /// </summary>
        [ACPropertyInfo(true, 413, DefaultValue = 9600)]
        public int BaudRate { get; set; }

        /// <summary>
        /// Parity
        /// </summary>
        [ACPropertyInfo(true, 414)]
        public Parity Parity { get; set; }

        /// <summary>
        /// DataBits
        /// </summary>
        [ACPropertyInfo(true, 415, DefaultValue = 8)]
        public int DataBits { get; set; }

        /// <summary>
        /// StopBits
        /// </summary>
        [ACPropertyInfo(true, 416)]
        public StopBits StopBits { get; set; }


        /// <summary>
        /// RtsEnable
        /// </summary>
        [ACPropertyInfo(true, 417, DefaultValue = false)]
        public bool RtsEnable { get; set; }

        /// <summary>
        /// Handshake
        /// </summary>
        [ACPropertyInfo(true, 418)]
        public Handshake Handshake { get; set; }

        #endregion

        #region TCP-Communication

        [ACPropertyInfo(true, 401, "Config", "en{'TCP-Communication on'}de{'TCP- Kommunikation ein'}", DefaultValue = false)]
        public bool TCPCommEnabled { get; set; }

        protected TcpClient _tcpClient = null;
        [ACPropertyInfo(9999)]
        public TcpClient TcpClient
        {
            get
            {
                using (ACMonitor.Lock(_61000_LockPort))
                {
                    return _tcpClient;
                }
            }
        }

        [ACPropertyInfo(true, 403, "Config", "en{'DNS-Name [ms]'}de{'DNS-Name [ms]'}")]
        public string ServerDNSName { get; set; }

        [ACPropertyInfo(false, 405, "Config", "en{'Trace read values'}de{'Ausgabe gelesene Werte'}", DefaultValue = false)]
        public bool TraceValues { get; set; }

        [ACPropertyInfo(false, 406, "Config", "en{'IgnoreInvalidTeleLength}de{'IgnoreInvalidTeleLength'}", DefaultValue = false)]
        public bool IgnoreInvalidTeleLength { get; set; }

        #endregion

        #region Broadcast-Properties

        [ACPropertyBindingSource(9999, "Error", "en{'Linx printer alarm'}de{'Linx Drucker Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> LinxPrinterAlarm { get; set; }
        #endregion

        #region Common configuration
        [ACPropertyInfo(true, 400, "Config", "en{'Polling [ms]'}de{'Abfragezyklus [ms]'}", DefaultValue = 200)]
        public int PollingInterval { get; set; }

        /// <summary>
        /// ReadTimeout
        /// </summary>
        [ACPropertyInfo(true, 405, "Config", "en{'Read-Timeout [ms]'}de{'Lese-Timeout [ms]'}", DefaultValue = 2000)]
        public int ReadTimeout { get; set; }

        /// <summary>
        /// WriteTimeout
        /// </summary>
        [ACPropertyInfo(true, 406, "Config", "en{'Write-Timeout [ms]'}de{'Schreibe-Timeout [ms]'}", DefaultValue = 2000)]
        public int WriteTimeout { get; set; }

        #endregion

        #region Communcation

        #region Communication -> Status

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Check status'}de{'Status überprüfen'}", 200, true)]
        public void CheckStatus()
        {
            byte[] data = GetData(LinxPrinterCommandCodeEnum.Printer_Status_Request, null);
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.CheckStatus;
            linxPrintJob.PacketsForPrint.Add(data);
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(SendDataToPrinter), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        #endregion

        #region Communication -> Open / Close port

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Open Connection'}de{'Öffne Verbindung'}", 200, true)]
        public bool OpenPort()
        {
            bool success = false;
            if (!IsEnabledOpenPort())
            {
                UpdateIsConnectedState();
                return false;
            }
            if (TCPCommEnabled)
            {
                try
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        if (_tcpClient == null)
                            _tcpClient = new TcpClient();
                        if (WriteTimeout > 0)
                            _tcpClient.SendTimeout = WriteTimeout;
                        if (ReadTimeout > 0)
                            _tcpClient.ReceiveTimeout = ReadTimeout;
                        if (!_tcpClient.Connected)
                        {
                            if (!String.IsNullOrEmpty(IPAddress))
                            {
                                IPAddress ipAddress = System.Net.IPAddress.Parse(IPAddress);
                                _tcpClient.Connect(ipAddress, Port);
                            }
                            else
                            {
                                _tcpClient.Connect(ServerDNSName, Port);
                            }
                        }
                    }
                    success = _tcpClient.Connected;
                }
                catch (Exception e)
                {
                    LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                    if (IsAlarmActive(nameof(LinxPrinterAlarm), e.Message) == null)
                    {
                        Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(OpenPort)}(05)", e);
                    }
                    OnNewAlarmOccurred(LinxPrinterAlarm, e.Message, true);
                    ClosePort();
                }
                UpdateIsConnectedState();
            }
            else if (SerialCommEnabled)
            {
                try
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        //_serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                        _serialPort = new SerialPort(PortName, BaudRate);
                        if (ReadTimeout > 0)
                            _serialPort.ReadTimeout = ReadTimeout;
                        else
                            _serialPort.ReadTimeout = 5000;
                        if (WriteTimeout > 0)
                            _serialPort.WriteTimeout = WriteTimeout;
                        if (RtsEnable == true)
                            _serialPort.RtsEnable = true;
                        if (Handshake != System.IO.Ports.Handshake.None)
                            _serialPort.Handshake = Handshake;
                        if (!_serialPort.IsOpen)
                        {
                            _serialPort.Open();
                        }
                    }
                    success = _serialPort.IsOpen;
                }
                catch (Exception e)
                {
                    LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                    if (IsAlarmActive(nameof(LinxPrinterAlarm), e.Message) == null)
                    {
                        Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(OpenPort)}(30)", e);
                    }
                    OnNewAlarmOccurred(LinxPrinterAlarm, e.Message, true);
                }
                UpdateIsConnectedState();
            }
            return success;
        }

        public bool IsEnabledOpenPort()
        {
            if ((!TCPCommEnabled && !SerialCommEnabled)
                || (ACOperationMode != ACOperationModes.Live))
                return false;
            if (TCPCommEnabled)
            {
                var client = TcpClient;
                if (client == null)
                    return !String.IsNullOrEmpty(IPAddress) || !String.IsNullOrEmpty(ServerDNSName);
                return !client.Connected;
            }
            else if (SerialCommEnabled)
            {
                var port = SerialPort;
                if (port == null)
                    return !String.IsNullOrEmpty(PortName);
                return !port.IsOpen;
            }
            return false;
        }

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Close Connection'}de{'Schliesse Verbindung'}", 201, true)]
        public void ClosePort()
        {
            if (!IsEnabledClosePort())
                return;
            using (ACMonitor.Lock(_61000_LockPort))
            {
                if (_tcpClient != null)
                {
                    if (_tcpClient.Connected)
                        _tcpClient.Close();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                        _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            IsConnected.ValueT = false;
        }
        public bool IsEnabledClosePort()
        {
            var client = TcpClient;
            if (client != null)
                return true;
            var port = SerialPort;
            if (port != null)
                return port.IsOpen;
            return false;
        }

        protected void UpdateIsConnectedState()
        {
            var client = TcpClient;
            if (client != null)
            {
                IsConnected.ValueT = client.Connected;
                return;
            }
            else
            {
                var port = SerialPort;
                if (port != null)
                {
                    IsConnected.ValueT = port.IsOpen;
                    return;
                }
            }
            IsConnected.ValueT = false;
        }

        #endregion

        #region Communication -> Request/Response
        public bool Request(byte[] data)
        {
            bool success = OpenPort();

            try
            {
                if (success)
                {
                    if (TCPCommEnabled)
                    {
                        if (TcpClient == null || !TcpClient.Connected)
                            return false;
                        NetworkStream stream = TcpClient.GetStream();
                        if (stream != null && stream.CanWrite)
                        {
                            stream.Write(data, 0, data.Length);
                            success = true;
                        }
                    }
                    else if (SerialCommEnabled)
                    {
                        if (SerialPort.IsOpen)
                        {
                            SerialPort.Write(data, 0, data.Length);
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LinxPrinterAlarm), ex.Message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Request)}(40)", ex);
                }
                OnNewAlarmOccurred(LinxPrinterAlarm, ex.Message, true);
            }

            return success;
        }


        public (bool, byte[]) Response(LinxPrintJob linxPrintJob)
        {
            bool success = false;
            List<byte> result = new List<byte>();
            OpenPort();
            if (TCPCommEnabled)
            {
                using (ACMonitor.Lock(_61000_LockPort))
                {
                    result = null;
                    if (TcpClient != null || TcpClient.Connected)
                    {
                        try
                        {
                            NetworkStream stream = TcpClient.GetStream();
                            if (stream != null)
                            {
                                if (stream.CanRead && stream.DataAvailable)
                                {
                                    byte[] myReadBuffer = new byte[1024];
                                    int numberOfBytesRead = 0;
                                    do
                                    {
                                        numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                                        result.AddRange(myReadBuffer);
                                    }
                                    while (stream.DataAvailable);

                                    success = result != null && result.Any();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                            if (IsAlarmActive(nameof(LinxPrinterAlarm), ex.Message) == null)
                            {
                                Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Response)}(50)", ex);
                            }
                            OnNewAlarmOccurred(LinxPrinterAlarm, ex.Message, true);
                        }

                    }
                }
            }
            else if (SerialCommEnabled)
            {
                if (SerialPort != null && SerialPort.IsOpen)
                {
                    using (ACMonitor.Lock(_61000_LockPort))
                    {
                        byte[] myReadBuffer = new byte[1024];
                        int numberOfBytesRead = 0;
                        try
                        {
                            numberOfBytesRead = SerialPort.Read(myReadBuffer, 0, 1024);
                            result.AddRange(myReadBuffer);

                            success = result != null && result.Any();
                        }
                        catch (Exception ex)
                        {
                            LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                            if (IsAlarmActive(nameof(LinxPrinterAlarm), ex.Message) == null)
                            {
                                Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Response)}(60)", ex);
                            }
                            OnNewAlarmOccurred(LinxPrinterAlarm, ex.Message, true);
                        }
                    }
                }
            }

            if (success)
            {
                success = LinxHelper.ValidateChecksum(result.ToArray());
                if (!success)
                {
                    string message = $"Bad checksum for response: {linxPrintJob.GetJobInfo()}";
                    LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                    Messages.LogError(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Request)}(70)", message);
                    OnNewAlarmOccurred(LinxPrinterAlarm, message, true);
                }
            }
            return (success, result.ToArray());
        }

        #endregion

        #endregion

        #region Print Job

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

        #region Byte operations

        public byte[] GetData(LinxPrinterCommandCodeEnum commandCode, byte[] data)
        {
            return GetData((byte)commandCode, data);
        }

        public byte[] GetData(LinxASCIControlCharacterEnum commandCode, byte[] data)
        {
            return GetData((byte)commandCode, data);
        }

        public byte[] GetData(byte commandCode, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (MemoryStream msForCheckSum = new MemoryStream())
            {
                ms.Write(LinxASCIControlCharacterEnum.ESC);
                ms.Write(LinxASCIControlCharacterEnum.STX);
                msForCheckSum.Write(LinxASCIControlCharacterEnum.STX);

                ms.Write(commandCode);
                msForCheckSum.Write(commandCode);

                if (data != null)
                {
                    ms.Write(data, 0, data.Length);
                    msForCheckSum.Write(data, 0, data.Length);
                }

                ms.Write(LinxASCIControlCharacterEnum.ESC);
                ms.Write(LinxASCIControlCharacterEnum.ETX);
                msForCheckSum.Write(LinxASCIControlCharacterEnum.ETX);

                byte[] tempArr = msForCheckSum.ToArray();
                byte checkSum = LinxHelper.GetCheckSum(tempArr);
                ms.Write(new byte[] { checkSum }, 0, 1);

                return ms.ToArray();
            }
        }

        #endregion

        #region ACPrintServerBase

        public override PrintJob GetPrintJob(FlowDocument flowDocument)
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            Encoding encoder = Encoding.ASCII;
            linxPrintJob.FlowDocument = flowDocument;
            linxPrintJob.Encoding = encoder;
            linxPrintJob.ColumnMultiplier = 1;
            linxPrintJob.ColumnDivisor = 1;
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.Print;
            if (UseRemoteReport)
            {
                linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.PrintRemote;
            }

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

                    if (linxPrintJob.LinxPrintJobType == LinxPrintJobTypeEnum.CheckStatus && linxPrintJob.PacketsForPrint.Count == 1)
                    {
                        bool requestSuccess = Request(linxPrintJob.PacketsForPrint[0]);
                        if (requestSuccess)
                        {
                            Thread.Sleep(ReceiveTimeout);
                            (bool responseSuccess, byte[] responseData) = Response(linxPrintJob);
                            if (responseSuccess && responseData != null)
                            {
                                (MsgWithDetails msgWithDetails, LinxPrinterFullStatusResponse response) = LinxMapping<LinxPrinterFullStatusResponse>.Map(responseData);
                                // TODO @aagincic: LinxPrinter parse LinxPrinterFullStatusResponse response
                            }
                        }
                    }
                    else
                    {
                        foreach (byte[] data in linxPrintJob.PacketsForPrint)
                        {
                            bool requestSuccess = Request(data);
                            if (requestSuccess)
                            {
                                Thread.Sleep(ReceiveTimeout);
                                (bool responseSuccess, byte[] responseData) = Response(linxPrintJob);
                                if (responseSuccess && responseData != null)
                                {
                                    // TODO: @aagincic LinxPrinter Parse printer data
                                    //LinxPrinterStatusResponse response = LinxMapping<LinxPrinterStatusResponse>.Map(responseData);
                                }
                            }
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

        public override void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;

            if (UseRemoteReport)
            {
                LoadPrintMessage(linxPrintJob, flowDoc);
            }

            base.OnRenderFlowDocument(printJob, flowDoc);

            if (UseRemoteReport)
            {
                // field length
                int dataLength = linxPrintJob.RemoteFieldValues.Sum(c => c.Length);
                byte[] dataLengthBy = BitConverter.GetBytes(dataLength);

                // prepare data
                List<byte[]> dataArr = linxPrintJob.RemoteFieldValues;
                dataArr.Insert(0, dataLengthBy);
                byte[] inputData = LinxHelper.Combine(dataArr);

                // generate request array and add to queue
                byte[] data = GetData(LinxASCIControlCharacterEnum.GS, inputData);
                linxPrintJob.PacketsForPrint.Add(data);
            }

            StartJetCommand(linxPrintJob);
            StartPrintCommand(linxPrintJob);
        }


        public override void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionTableHeader(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderSectionTableFooter(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn)
        {
        }

        public override void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow)
        {
        }

        public override void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow)
        {
        }

        public override void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph)
        {
        }

        public override void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph)
        {

        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
            if (UseRemoteReport)
            {

            }
            else
            {

            }
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                SetRemoteValue(linxPrintJob, inlineDocumentValue.Name, inlineDocumentValue.Text);
            }
            else
            {
                DownloadTextValue(linxPrintJob, inlineDocumentValue.Text);
            }
        }

        public override void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                SetRemoteValue(linxPrintJob, inlineACMethodValue.Name, inlineACMethodValue.Text);
            }
            else
            {
                DownloadTextValue(linxPrintJob, inlineACMethodValue.Text);
            }
        }

        public override void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                SetRemoteValue(linxPrintJob, inlineTableCellValue.Name, inlineTableCellValue.Text);
            }
            else
            {
                DownloadTextValue(linxPrintJob, inlineTableCellValue.Text);
            }
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                // Think don't must do something
            }
            else
            {
                DownloadBarcodeValue(linxPrintJob, inlineBarcode);
            }
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                SetRemoteValue(linxPrintJob, inlineBoolValue.Name, inlineBoolValue.Value.ToString());
            }
            else
            {
                DownloadTextValue(linxPrintJob, inlineBoolValue.Value.ToString());
            }
        }

        public override void OnRenderRun(PrintJob printJob, Run run)
        {
        }

        public override void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak)
        {
        }

        #endregion

        #region Methods -> Linx Printer Methods

        /// <summary>
        /// Add remote field value for post processing to LinxPrintJob feed
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        private void SetRemoteValue(LinxPrintJob linxPrintJob, string name, string text)
        {
            linxPrintJob.RemoteFieldValues.Add(Encoding.ASCII.GetBytes(text));
        }


        /// <summary>
        /// add to print queue
        /// text data
        /// DownloadTextValue == sending text to printer -> printer downloads text
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="text"></param>
        private void DownloadTextValue(LinxPrintJob linxPrintJob, string text)
        {
            /*
                19	;Command ID - Download Message
                01	;Number of messages
                4E 00	;Message length in bytes
                17 00	;Message length in rasters
                06	;EHT setting
                00 00	;Inter-raster width
                10 00	;Print Delay
                6D 65 73 73 61 67 65 31	;Message name - message1.pat
                2E 70 61 74 00 00 00 00
                31 36 20 47 45 4E 20 53	;Raster name - 16 GEN STD
                54 44 00 00 00 00 00 00
                1C	;Field header
                00	;Field type – Text field
                25 00	;Field length in bytes
                00	;Y position
                00 00	;X position
                17 00	;Field length in rasters
                07	;Field height in drops
                00	;Format 3
                01	;Bold multiplier
 
                04	;String length (excluding null)
                00	;Format 1 – set to null
                00	;Format 2
                00	;Linkage
                37 20 48 69 67 68 20 46	;Data set name - 7 High Full
                75 6C 6C 00 00 00 00 00
                4C 69 6E 78 00	;Data - Linx (note the null terminator)



                Printer Reply:
                1B 06	;ESC ACK sequence
                00	;P-Status - No printer errors
                00	;C-Status - No command errors
                19	;Command ID sent
                1B 03	;ESC ETX sequence
                DE	;Checksum

            */
            // TODO: @aagincic LinxPrinter DownloadTextValue

            byte[] textBytes = Encoding.ASCII.GetBytes(text);

            List<byte[]> dataArr = new List<byte[]>();

            //01; Number of messages
            dataArr.Add(new byte[] { 0x01 });

            //49 00; Message length in bytes
            dataArr.Add(BitConverter.GetBytes(textBytes.Length));

            //17 00; Message length in rasters
            dataArr.Add(new byte[] { 0x17, 0 });

            //06; EHT setting
            dataArr.Add(new byte[] { (byte)LinxASCIControlCharacterEnum.ACK });

            //00 00; Inter - raster width
            dataArr.Add(new byte[] { 0x00, 0x00 });

            //10 00; Print delay
            dataArr.Add(new byte[] { 0x10, 0x00 });

            //52 45 4D 4F 54 45 20 54; Message name -REMOTE TEST
            //45 53 54 00 00 00 00 00
            dataArr.Add(textBytes);

            //31 36 20 47 45 4E 20 53; Raster name -16 GEN STD
            //54 44 00 00 00 00 00 00
            dataArr.Add(new byte[] { 0x31, 0x36, 0x20, 0x47, 0x45, 0x4E, 0x20, 0x53, 0x54, 0x44, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            //1C; Field header
            dataArr.Add(new byte[] { (byte)LinxASCIControlCharacterEnum.FS });

            //07; Field type -Remote Field
            dataArr.Add(new byte[] { (byte)LinxASCIControlCharacterEnum.BEL });

            //20 00; Field length in bytes
            dataArr.Add(new byte[] { 0x20, 0x00 });

            //00; Y position
            dataArr.Add(new byte[] { 0x00 });

            //00 00; X position
            dataArr.Add(new byte[] { 0x00, 0x00 });

            //1D 00; Field length in rasters
            dataArr.Add(new byte[] { 0x1D, 0x00 });

            //07; Field height in drops
            dataArr.Add(new byte[] { 0x07 });

            //00; Format 3
            dataArr.Add(new byte[] { 0x00 });

            //01; Bold multiplier
            dataArr.Add(new byte[] { 0x00 });

            //05; String length(excluding null)
            dataArr.Add(new byte[] { (byte)text.Length });

            //00; Format 1
            dataArr.Add(new byte[] { 0x00 });

            //00; Format 2
            dataArr.Add(new byte[] { 0x00 });

            //00; Linkage
            dataArr.Add(new byte[] { 0x00 });

            //37 20 48 69 67 68 20 46; Character set name - 7 High Full
            //75 6C 6C 00 00 00 00 00
            dataArr.Add(new byte[] { 0x37, 0x20, 0x48, 0x69, 0x67, 0x68, 0x20, 0x46, 0x75, 0x6C, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00 });

            byte[] inputData = LinxHelper.Combine(dataArr);


            byte[] data = GetData(LinxASCIControlCharacterEnum.DC3, inputData);
            linxPrintJob.PacketsForPrint.Add(data);

        }

        /// <summary>
        /// add to print queue
        /// barcode data
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="inlineBarcode"></param>
        private void DownloadBarcodeValue(LinxPrintJob linxPrintJob, InlineBarcode inlineBarcode)
        {
            // TODO: @aagincic LinxPrinter DownloadBarcodeValue

            /*
                1B 02	;ESC STX sequence
                19	;Command ID - Download Message Data
                01	;Number of messages
                ED 00	;Message length in bytes
                CF 00	;Message length in rasters
                06	;EHT setting
                00 00	;Inter-raster width
                10 00	;Print delay
                4C 49 4E 58 20 54 45 53	;Message name - LINX TEST
                54 00 00 00 00 00 00 00
                31 36 20 47 45 4E 20 53	;Raster name - 16 GEN STD
                54 44 00 00 00 00 00 00
                1C	;Field header
                00	;Field type - Text field
                2A 00	;Field length in bytes
                00	;Y position
                00 00	;X position
                35 00	;Field length in rasters
                07	;Field height in drops
 
                00	;Format 3
                01	;Bold multiplier
                09	;String length (excluding null)
                00	;Format 1
                00	;Format 2
                00	;Linkage
                37 20 48 69 67 68 20 46	;Data set name - 7 High Full
                75 6C 6C 00 00 00 00 00
                54 65 73 74 20 54 65 78	;Data - Test Text
                74 00
                1C	;Field header
                05	;Field type - Date field
                32 00	;Field length in bytes
                09	;Y position
                00 00	;X position
                2F 00	;Field length in rasters
                07	;Field height in drops
                00	;Format 3
                01	;Bold multiplier
                08	;String length (excluding null)
                00	;Format 1
                00	;Format 2
                00	;Linkage
                37 20 48 69 67 68 20 46	;Data set name - 7 High Full
                75 6C 6C 00 00 00 00 00
                64 64 2E 6D 6D 2E 79 79	;Date format name - dd.mm.yy
                00 00 00 00 00 00 00 00
                00	00	;Date offset - 0
                1C	;Field header
                01	;Field type - Logo field
                20 00	;Field length in bytes
                00	;Y position
                3C 00	;X position
                36 00	;Field length in rasters
                10	;Field height in drops
                00	;Format 3
                01	;Bold multiplier
                00	;String length (excluding null)
                00	;Format 1
                00	;Format 2
                00	;Linkage
                45 78 70 2E 20 31 36 20	;Data set name - Exp. 16 (Arab)
                28 41 72 61 62 29 00 00
                1C	;Field header
                C0	;Field type - Bar code text (not rendered)
                28 00	;Field length in bytes
                00	;Y position
                00 00	;X position
                2F 00	;Field length in rasters
                07	;Field height in drops
                00	;Format 3
 
                01	;Bold multiplier
                07	;String length (excluding null)
                00	;Format 1
                00	;Format 2
                04	;Linkage – points to field 4
                37 20 48 69 67 68 20 46	;Data set name - 7 High Full
                75 6C 6C 00 00 00 00 00
                31 32 33 34 35 36 37 00	;Data - 1234567
                1C	;Field header
                46	;Field type - Bar code field
                20 00	;Field length in bytes
                00	;Y position
                78 00	;X position
                57 00	;Field length in rasters
                10	;Field height in drops
                00	;Format 3
                01	;Bold multiplier
                00	;String length (excluding null)
                00	;Format 1
                01	;Format 2 (Checksum on)
                03	;Linkage – points to field 3
                45 41 4E 2D 38 20 20 20	;Data set name - EAN-8
                20 20 20 20 20 20 20 00
                1B 03	;ESC ETX sequence
                D4	;Checksum

                Printer Reply
                1B 06	;ESC ACK sequence
                00	;P-Status - No printer errors
                00	;C-Status - No command errors
                19	;Command ID sent
                1B 03	;ESC ETX sequence
                DE	;Checksum
    */


            byte[] data = GetData(LinxASCIControlCharacterEnum.DC3, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }

        /// <summary>
        /// add to print queue
        /// starting jet command
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void StartJetCommand(LinxPrintJob linxPrintJob)
        {
            /*
                E.1.9	Start Jet Command
                1B 02	;ESC STX sequence
                0F	;Command ID
                1B 03	;ESC ETX sequence
                EC	;Checksum

                Printer Reply
                1B 06	;ESC ACK sequence
                00	;P-Status - No printer errors
                00	;C-Status - No command errors
                0F	;Command ID sent
                1B 03	;ESC ETX sequence
                E8	;Checksum
            */
            // LinxASCIControlCharacterEnum.SI == 0xF
            byte[] data = GetData(LinxASCIControlCharacterEnum.SI, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }


        /// <summary>
        /// Add to message queue
        /// starting print command
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void StartPrintCommand(LinxPrintJob linxPrintJob)
        {
            /*
                 E.1.10	Start Print Command
                 1B 02	;ESC STX sequence
                 11	;Command ID
                 1B 03	;ESC ETX sequence
                 EA	;Checksum

                 Printer Reply
                 1B 06	;ESC ACK sequence
                 00	;P-Status - No printer errors
                 00	;C-Status - No command errors
                 11	;Command ID sent
                 1B 03	;ESC ETX sequence
                 E6	;Checksum

             */
            // LinxASCIControlCharacterEnum.VT == 0xB == 11
            byte[] data = GetData(LinxASCIControlCharacterEnum.VT, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }


        /// <summary>
        /// Loading remote layout
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void LoadPrintMessage(LinxPrintJob linxPrintJob, FlowDocument flowDoc)
        {
            string reportName = flowDoc.Name;
            byte[] reportNameByte = Encoding.ASCII.GetBytes(reportName);
            // LinxASCIControlCharacterEnum.RS == 0x1E
            byte[] data = GetData(LinxASCIControlCharacterEnum.RS, reportNameByte);
            linxPrintJob.PacketsForPrint.Add(data);
        }


        #endregion



    }
}
