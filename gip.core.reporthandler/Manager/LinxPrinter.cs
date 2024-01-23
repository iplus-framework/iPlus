using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Documents;

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

            DataSets = LoadDataSets();

            return true;
        }

        public virtual List<LinxDataSetData> LoadDataSets()
        {
            return new List<LinxDataSetData>()
            {
                new LinxDataSetData()
                {
                    DataSetName = "5 High Caps",
                    Height= 5,
                    Width = 5,
                    InterCharacterSpace = 1
                },
                new LinxDataSetData()
                {
                    DataSetName = "6 High Full",
                    Height= 6,
                    Width = 5,
                    InterCharacterSpace = 1
                },
                new LinxDataSetData()
                {
                    DataSetName = "7 High Full",
                    Height= 7,
                    Width = 5,
                    InterCharacterSpace = 1
                },
                new LinxDataSetData()
                {
                    DataSetName = "9 High Caps",
                    Height= 9,
                    Width = 7,
                    InterCharacterSpace = 1
                },
                new LinxDataSetData()
                {
                    DataSetName = "9 High Full",
                    Height= 9,
                    Width = 5,
                    InterCharacterSpace = 1
                },
                new LinxDataSetData()
                {
                    DataSetName = "15 High Full",
                    Height= 15,
                    Width = 10,
                    InterCharacterSpace = 2
                },
                new LinxDataSetData()
                {
                    DataSetName = "15 High Caps",
                    Height= 15,
                    Width = 10,
                    InterCharacterSpace = 2
                },
                new LinxDataSetData()
                {
                    DataSetName = "23 High Caps",
                    Height= 23,
                    Width = 16,
                    InterCharacterSpace = 2
                }
                ,
                new LinxDataSetData()
                {
                    DataSetName = "32 High Caps",
                    Height= 32,
                    Width = 24,
                    InterCharacterSpace = 3
                }
            };
        }

        public virtual byte[] GetFieldLengthInRasters(LinxDataSetData dataSetData, short numberOfCharacters)
        {
            // field length in rasters can be calculated by multiplying the number of characters in the field by the width of the character in rasters (including the inter-character space), minus one inter-character space.
            int length = (numberOfCharacters * dataSetData.Width - dataSetData.InterCharacterSpace);
            return BitConverter.GetBytes((short)length);
        }

        public override bool ACPostInit()
        {
            bool basePostInit = base.ACPostInit();


            _ShutdownEvent = new ManualResetEvent(false);
            _PollThread = new ACThread(Poll);
            _PollThread.Name = "ACUrl:" + this.GetACUrl() + ";Poll();";
            //_PollThread.ApartmentState = ApartmentState.STA;
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

            DataSets = null;

            return acDeinit;
        }



        #endregion

        #region Settings

        [ACPropertyInfo(true, 200, DefaultValue = false)]
        public bool UseRemoteReport { get; set; }

        public List<LinxDataSetData> DataSets { get; set; }

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
                    using (ACMonitor.Lock(this._61000_LockPort))
                    {
                        if (LinxPrintJobs.Any())
                        {
                            linxPrintJob = LinxPrintJobs.Dequeue();
                        }
                        if (linxPrintJob != null)
                        {
                            jobInfo = linxPrintJob.GetJobInfo();
                            ProcessJob(linxPrintJob);
                            Messages.LogInfo(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(Poll)}(10)", $"Job {jobInfo} processed!");
                        }
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

        public bool IsEnabledCheckStatus()
        {
            return IsConnected.ValueT || IsEnabledOpenPort() || IsEnabledClosePort();
        }

        [ACPropertyBindingSource(730, "Error", "en{'Printer (complete) status'}de{'Druckerstatus (abgeschlossen).'}", "", false, false)]
        public IACContainerTNet<LinxPrinterCompleteStatusResponse> PrinterCompleteStatus
        {
            get;
            set;
        }



        [ACMethodInteraction(nameof(LinxPrinter), "en{'Start Print'}de{'Drucker Start'}", 201, true)]
        public void StartPrint()
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.Print;
            StartPrintCommand(linxPrintJob);
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(StartPrint), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        public bool IsEnabledStartPrint()
        {
            return IsConnected.ValueT || IsEnabledOpenPort() || IsEnabledClosePort();
        }

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Stop Print'}de{'Drucker Stopp'}", 202, true)]
        public void StopPrint()
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.Print;
            StartPrintCommand(linxPrintJob, true);
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(StopPrint), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        public bool IsEnabledStopPrint()
        {
            return IsConnected.ValueT || IsEnabledOpenPort() || IsEnabledClosePort();
        }

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Start Jet'}de{'Druckkopf Start'}", 203, true)]
        public void StartJet()
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.Print;
            StartJetCommand(linxPrintJob);
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(StartJet), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        public bool IsEnabledStartJet()
        {
            return IsConnected.ValueT || IsEnabledOpenPort() || IsEnabledClosePort();
        }

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Stop Jet'}de{'Druckkopf Stopp'}", 203, true)]
        public void StopJet()
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            linxPrintJob.LinxPrintJobType = LinxPrintJobTypeEnum.Print;
            StartJetCommand(linxPrintJob, true);
            using (ACMonitor.Lock(_61000_LockPort))
            {
                Messages.LogMessage(eMsgLevel.Info, GetACUrl(), nameof(StopJet), $"Add LinxPrintJob:{linxPrintJob.PrintJobID} to queue...");
                LinxPrintJobs.Enqueue(linxPrintJob);
            }
        }

        public bool IsEnabledStopJet()
        {
            return IsConnected.ValueT || IsEnabledOpenPort() || IsEnabledClosePort();
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
                                        if (numberOfBytesRead > 0)
                                            result.AddRange(myReadBuffer.Take(numberOfBytesRead));
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
                            result.AddRange(myReadBuffer.Take(numberOfBytesRead));

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
            List<byte> result = new List<byte>();
            List<byte> checkSumList = new List<byte>();

            result.Add((byte)LinxASCIControlCharacterEnum.ESC);
            result.Add((byte)LinxASCIControlCharacterEnum.STX);
            checkSumList.Add((byte)LinxASCIControlCharacterEnum.STX);

            result.Add((byte)commandCode);
            checkSumList.Add((byte)commandCode);

            if (data != null)
            {
                result.AddRange(data);
                checkSumList.AddRange(data);
            }

            result.Add((byte)LinxASCIControlCharacterEnum.ESC);
            result.Add((byte)LinxASCIControlCharacterEnum.ETX);
            checkSumList.Add((byte)LinxASCIControlCharacterEnum.ETX);

            byte[] checkSumArr = checkSumList.ToArray();
            byte checkSum = LinxHelper.GetCheckSum(checkSumArr);
            result.Add(checkSum);

            return result.ToArray();
        }

        #endregion

        #region ACPrintServerBase

        public override PrintJob GetPrintJob(string reportName, FlowDocument flowDocument)
        {
            LinxPrintJob linxPrintJob = new LinxPrintJob();
            Encoding encoder = Encoding.Unicode;
            linxPrintJob.FlowDocument = flowDocument;
            linxPrintJob.Name = string.IsNullOrEmpty(flowDocument.Name) ? reportName : flowDocument.Name;
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
                                (MsgWithDetails msgWithDetails, LinxPrinterCompleteStatusResponse response) = LinxMapping<LinxPrinterCompleteStatusResponse>.Map(responseData);
                                if (msgWithDetails != null && !msgWithDetails.IsSucceded())
                                {
                                    LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                                    if (IsAlarmActive(nameof(LinxPrinterAlarm), msgWithDetails.DetailsAsText) == null)
                                    {
                                        Messages.LogError(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(120)", msgWithDetails.DetailsAsText);
                                    }
                                    OnNewAlarmOccurred(LinxPrinterAlarm, msgWithDetails.DetailsAsText, true);
                                }
                                else
                                {
                                    PrinterCompleteStatus.ValueT = response;
                                }
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
                                    (MsgWithDetails msgWithDetails, LinxPrinterStatusResponse response) = LinxMapping<LinxPrinterStatusResponse>.Map(responseData);
                                    if (msgWithDetails != null && !msgWithDetails.IsSucceded())
                                    {
                                        LinxPrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                                        if (IsAlarmActive(nameof(LinxPrinterAlarm), msgWithDetails.DetailsAsText) == null)
                                        {
                                            Messages.LogError(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(ProcessJob)}(130)", msgWithDetails.DetailsAsText);
                                        }
                                        OnNewAlarmOccurred(LinxPrinterAlarm, msgWithDetails.DetailsAsText, true);
                                    }
                                    else
                                    {
                                        if (response != null && (response.P_Status > 0 || response.C_Status > 0))
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
                            else
                            {
                                using (ACMonitor.Lock(_61000_LockPort))
                                {
                                    linxPrintJob.State = PrintJobStateEnum.InAlarm;
                                }
                                break;
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

            if (!UseRemoteReport)
            {
                string rasterName = "16 GEN STD";
                int msgLengthInBytes = linxPrintJob.LinxFields.Sum(c => BitConverter.ToInt32(c.Header.FieldLenghtInBytes, 0)) + LinxMessageHeader.DefaultHeaderLength;
                int msgLengthInRasters = linxPrintJob.LinxFields.Sum(c => BitConverter.ToInt32(c.Header.FieldLengthInRasters, 0)) + LinxMessageHeader.DefaultHeaderLength;
                LinxMessageHeader linxMessageHeader = GetLinxMessageHeader(linxPrintJob.Encoding, linxPrintJob.Name, rasterName, 1, (short)msgLengthInBytes, (short)msgLengthInRasters);

                List<byte> downloadData = new List<byte>(); 
                downloadData.AddRange(linxMessageHeader.GetBytes());
                foreach(LinxField linxField in linxPrintJob.LinxFields)
                {
                    downloadData.AddRange(linxField.GetBytes());
                }
            }
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
                DownloadTextValue(linxPrintJob, inlineDocumentValue.AggregateGroup, inlineDocumentValue.Text);
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
                DownloadTextValue(linxPrintJob, inlineACMethodValue.AggregateGroup, inlineACMethodValue.Text);
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
                DownloadTextValue(linxPrintJob, inlineTableCellValue.AggregateGroup, inlineTableCellValue.Text);
            }
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            string barcodeValue = "";

            BarcodeType[] ianBarcodeTypes = new BarcodeType[] { BarcodeType.CODE128, BarcodeType.CODE128A, BarcodeType.CODE128B, BarcodeType.CODE128C };
            bool isIanCodeType = ianBarcodeTypes.Contains(inlineBarcode.BarcodeType);
            Dictionary<string, string> barcodeValues = new Dictionary<string, string>();

            if (isIanCodeType)
            {
                if (inlineBarcode.BarcodeValues != null && inlineBarcode.BarcodeValues.Any())
                {
                    foreach (BarcodeValue tmpBarcodeValue in inlineBarcode.BarcodeValues)
                    {
                        try
                        {
                            string ai = tmpBarcodeValue.AI;
                            string tmpValue = tmpBarcodeValue.Value?.ToString();
                            if (tmpValue != null)
                            {
                                barcodeValues.Add(ai, tmpValue);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                barcodeValue = GS1.Generate(barcodeValues, isIanCodeType);
            }
            else
            {
                barcodeValue = inlineBarcode.Value.ToString();
            }

            if (UseRemoteReport)
            {
                SetRemoteValue(linxPrintJob, inlineBarcode.Name, barcodeValue);
            }
            else
            {
                DownloadBarcodeValue(linxPrintJob, inlineBarcode.AggregateGroup, barcodeValue);
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
                DownloadTextValue(linxPrintJob, inlineBoolValue.AggregateGroup, inlineBoolValue.Value.ToString());
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

        #region Methods -> Linx Printer Methods -> Communicate

        /// <summary>
        /// Add remote field value for post processing to LinxPrintJob feed
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        private void SetRemoteValue(LinxPrintJob linxPrintJob, string name, string text)
        {
            byte[] textByte = linxPrintJob.Encoding.GetBytes(text);
            linxPrintJob.RemoteFieldValues.Add(textByte);
        }


        /// <summary>
        /// add to print queue
        /// text data
        /// DownloadTextValue == sending text to printer -> printer downloads text
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="text"></param>
        private void DownloadTextValue(LinxPrintJob linxPrintJob, string aggregateGroup, string text)
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
            LinxDataSetData dataSet = DataSets.Where(c => c.DataSetName == aggregateGroup).FirstOrDefault();
            if (dataSet == null)
            {
                dataSet = DataSets.FirstOrDefault();
            }
            LinxField linxField = GetLinxField(dataSet, linxPrintJob.Encoding, text);

        }

        /// <summary>
        /// add to print queue
        /// barcode data
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="inlineBarcode"></param>
        private void DownloadBarcodeValue(LinxPrintJob linxPrintJob, string aggregateGroup, string barcodeValue)
        {
            // TODO: @aagincic LinxPrinter DownloadBarcodeValue

            /*
                7C	;Command ID - Download Message Data
                01	;Number of messages
                79 00	;Message length in bytes
                5C 01	;Message length in rasters
                00	;EHT setting - set to null
                00 00	;Inter-raster width
                10 00	;Print delay
                6D 65 73 73 61 67 65 37	;Message name - message7.pat
                2E 70 61 74 00 00 00 00
                35 30 30 20 48 69 67 68	;Raster name - 500 High
                00 00 00 00 00 00 00 00
                1C	;Field header
                C0	;Field type - Text field (not rendered)
                2C 00	;Field length in bytes
                FA FF	;Y position
                00 00	;X position
                40 01	;Field length in rasters
                2D 00	;Field height in drops
                00	;Format 3
                01	;Bold multiplier
                07	;String length (excluding null)
                00	;Format 1
                00	;Format 2
                01	;Linkage - points to Bar Code
                00	;Reserved - set to null
                00	;Reserved - set to null
                34 35 20 48 69 20 42 61	;Data set name - 45 Hi Barcode
                72 63 6F 64 65 00 00 00
                31 32 33 34 35 36 37 00	;Data - 1234567
 
                1C	;Field header
                46	;Field type - Bar code field
                24 00	;Field length in bytes
                00 00	;Y position
                00 00	;X position
                5C 01	;Field length in rasters
                E5 00	;Field height in drops
                00	;Format 3
                04	;Bold multiplier
                00	;Text length (excluding null)
                00	;Format 1
                03	;Format 2 - Checksum on, attached text; on
                00	;Linkage
                00	;Reserved
                00	;Reserved
                45 41 4E 2D 38 20 20 20	;Data set name - EAN-8
                20 20 20 20 20 20 20 00

         */
            LinxDataSetData dataSet = DataSets.Where(c => c.DataSetName == aggregateGroup).FirstOrDefault();
            if (dataSet == null)
            {
                dataSet = DataSets.FirstOrDefault();
            }
            LinxField linxField = GetLinxField(dataSet, linxPrintJob.Encoding, barcodeValue, 0x46);
        }


        /// <summary>
        /// Loading remote layout
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void LoadPrintMessage(LinxPrintJob linxPrintJob, FlowDocument flowDoc)
        {
            string reportName = flowDoc.Name;
            byte[] reportNameByte = linxPrintJob.Encoding.GetBytes(reportName);
            // LinxASCIControlCharacterEnum.RS == 0x1E
            byte[] data = GetData(LinxASCIControlCharacterEnum.RS, reportNameByte);
            linxPrintJob.PacketsForPrint.Add(data);
        }

        private byte[] GetMessageName(string reportName)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(reportName);

            byte[] result = new byte[15];
            Array.Copy(bytes, result, Math.Min(bytes.Length, 15));

            // If the name is shorter than 15 bytes, the rest of the array will be zeros.
            return result;
        }

        #endregion

        #region Methods -> Linx Printer Methods -> Start printing


        /// <summary>
        /// add to print queue
        /// starting jet command
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void StartJetCommand(LinxPrintJob linxPrintJob, bool bStop = false)
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
            byte[] data = GetData(bStop ? LinxASCIControlCharacterEnum.DLE : LinxASCIControlCharacterEnum.SI, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }


        /// <summary>
        /// Add to message queue
        /// starting print command
        /// </summary>
        /// <param name="linxPrintJob"></param>
        private void StartPrintCommand(LinxPrintJob linxPrintJob, bool bStop = false)
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
            byte[] data = GetData(bStop ? LinxASCIControlCharacterEnum.DC2 : LinxASCIControlCharacterEnum.DC1, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }


        #endregion

        #region Methods -> Linx Printer Methods -> Communicate

        public void StopPrint(LinxPrintJob linxPrintJob)
        {
            //1B 02; ESC STX sequence
            //12; Command ID -Stop Print
            //1B 03; ESC ETX sequence
            //E9; Checksum

            //12; Command ID -Stop Print
            byte[] data = GetData(LinxASCIControlCharacterEnum.DC2, null);
            linxPrintJob.PacketsForPrint.Add(data);
        }

        /// <summary>
        /// Doc name:
        /// Delete Message Data
        /// </summary>
        /// <param name="linxPrintJob"></param>
        /// <param name="reportName"></param>
        public void ClearRemoteReportData(LinxPrintJob linxPrintJob)
        {

            //1B 02; ESC STX sequence
            //1B 1B; Command ID -Delete Message
            //01; Number of messages
            //4C 49 4E 58 20 54 45 53
            //54 00 00 00 00 00 00 00; Message name -LINX TEST
            //1B 03; ESC ETX sequence
            //44; Checksum

            List<byte[]> dataArr = new List<byte[]>();

            //01; Number of messages
            dataArr.Add(new byte[] { 0x01 });

            //  Message name -LINX TEST
            byte[] reportNameBytes = linxPrintJob.Encoding.GetBytes(linxPrintJob.Name);
            dataArr.Add(reportNameBytes);


            //1B 1B; Command ID -Delete Message
            byte[] data = GetData(LinxASCIControlCharacterEnum.ESC, new byte[] { (byte)LinxASCIControlCharacterEnum.ESC });
            linxPrintJob.PacketsForPrint.Add(data);
        }

        #endregion

        #region Methods -> Prepare Fields

        public virtual LinxField GetLinxField(LinxDataSetData dataSet, Encoding encoding, string value, byte fieldType = 0x00)
        {
            LinxField field = new LinxField();
            field.Header = GetLinxFieldHeader(dataSet, encoding, value, fieldType);
            field.Value = value;
            return field;
        }

        public virtual LinxFieldHeader GetLinxFieldHeader(LinxDataSetData dataSet, Encoding encoding, string value, byte fieldType = 0x00)
        {
            LinxFieldHeader linxFieldHeader = new LinxFieldHeader();
            linxFieldHeader.FieldType = fieldType;
            linxFieldHeader.FieldLenghtInBytes = BitConverter.GetBytes((short)value.Length);
            linxFieldHeader.FieldLengthInRasters = GetFieldLengthInRasters(dataSet, (short)value.Length);
            linxFieldHeader.TextLenght = (byte)value.Length;

            // TODO: check this: DataSetName length
            byte[] test = Encoding.Unicode.GetBytes("6 High Full"); // meni je dužina polja (datasetname) ovdje 22 a njima 15 bytes + null*

            linxFieldHeader.DataSetName = encoding.GetBytes(dataSet.DataSetName);
            return linxFieldHeader;
        }

        #endregion

        #region Methods -> Prepare Header

        public virtual LinxMessageHeader GetLinxMessageHeader(Encoding encoding, string messageName, string rasterName, short numOfMessages, short msgLengthInBytes, short msgLengthInRasters)
        {
            LinxMessageHeader header = new LinxMessageHeader();

            // TODO: check this: MessageName length
            byte[] test = Encoding.Unicode.GetBytes("LINX TEST"); // meni je dužina polja ovdje 18 a njima 16
            
            header.MessageName = encoding.GetBytes(messageName);

            // TODO: check this: RasterName length
            byte[] test1 = Encoding.Unicode.GetBytes("16 GEN STD"); // meni je dužina polja ovdje 20 a njima 16


            header.RasterName = encoding.GetBytes(rasterName);
            header.NumberOfMessages = (byte)numOfMessages;
            header.MessageLengthInBytes = BitConverter.GetBytes(msgLengthInBytes);
            header.MessageLengthInRasters = BitConverter.GetBytes(msgLengthInRasters);
            return header;
        }

        #endregion

        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(CheckStatus):
                    CheckStatus();
                    return true;
                case nameof(IsEnabledCheckStatus):
                    result = IsEnabledCheckStatus();
                    return true;
                case nameof(StartPrint):
                    StartPrint();
                    return true;
                case nameof(IsEnabledStartPrint):
                    result = IsEnabledStartPrint();
                    return true;
                case nameof(StopPrint):
                    StopPrint();
                    return true;
                case nameof(IsEnabledStopPrint):
                    result = IsEnabledStopPrint();
                    return true;
                case nameof(StartJet):
                    StartJet();
                    return true;
                case nameof(IsEnabledStartJet):
                    result = IsEnabledStartJet();
                    return true;
                case nameof(StopJet):
                    StopJet();
                    return true;
                case nameof(IsEnabledStopJet):
                    result = IsEnabledStopJet();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion
    }
}
