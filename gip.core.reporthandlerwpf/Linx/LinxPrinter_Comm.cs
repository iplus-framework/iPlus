using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.reporthandlerwpf.Flowdoc;
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
using static gip.core.reporthandlerwpf.LinxPrintJob;

namespace gip.core.reporthandlerwpf
{
    public partial class LinxPrinter
    {
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

        #region Common configuration
        [ACPropertyInfo(true, 400, "Config", "en{'Polling [ms]'}de{'Abfragezyklus [ms]'}", DefaultValue = 200)]
        public int PollingInterval { get; set; }

        /// <summary>
        /// ReadTimeout
        /// </summary>
        [ACPropertyInfo(true, 405, "Config", "en{'Read-Timeout [ms]'}de{'Lese-Timeout [ms]'}", DefaultValue = 5000)]
        public int ReadTimeout { get; set; }

        /// <summary>
        /// WriteTimeout
        /// </summary>
        [ACPropertyInfo(true, 406, "Config", "en{'Write-Timeout [ms]'}de{'Schreibe-Timeout [ms]'}", DefaultValue = 2000)]
        public int WriteTimeout { get; set; }

        #endregion

        #region Communication -> Open / Close port

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Open Connection'}de{'Öffne Verbindung'}", 200, true)]
        public bool OpenPort()
        {
            bool success = false;
            if (!IsEnabledOpenPort())
            {
                UpdateIsConnectedState();
                return IsConnected.ValueT;
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

        #region Send / Receive

        public bool Request(Telegram telegram)
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
                            stream.Write(telegram.Packet, 0, telegram.Packet.Length);
                            success = true;
                        }
                    }
                    else if (SerialCommEnabled)
                    {
                        if (SerialPort.IsOpen)
                        {
                            SerialPort.Write(telegram.Packet, 0, telegram.Packet.Length);
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


        public (bool, byte[]) Response(LinxPrintJob linxPrintJob, Telegram telegram)
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
                                if (stream.CanRead) // && stream.DataAvailable)
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
                if (telegram.LinxPrintJobType != LinxPrintJobTypeEnum.RasterData 
                    && telegram.LinxPrintJobType != LinxPrintJobTypeEnum.DeleteReport)
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

    }
}
