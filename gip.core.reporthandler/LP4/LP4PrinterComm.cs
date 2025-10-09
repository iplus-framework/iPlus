using DocumentFormat.OpenXml.Spreadsheet;
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace gip.core.reporthandler
{
    public partial class LP4Printer
    {
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

        #region Communication -> Open / Close port

        [ACMethodInteraction(nameof(OpenPort), "en{'Open Connection'}de{'Öffne Verbindung'}", 200, true)]
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
                    LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                    if (IsAlarmActive(nameof(LP4PrinterAlarm), e.Message) == null)
                    {
                        Messages.LogException(GetACUrl(), $"{nameof(OpenPort)}.{nameof(OpenPort)}(05)", e);
                    }
                    OnNewAlarmOccurred(LP4PrinterAlarm, e.Message, true);
                    ClosePort();
                }
                UpdateIsConnectedState();
            }
            return success;
        }

        public bool IsEnabledOpenPort()
        {
            if (!TCPCommEnabled || ACOperationMode != ACOperationModes.Live)
                return false;
            if (TCPCommEnabled)
            {
                var client = TcpClient;
                if (client == null)
                    return !String.IsNullOrEmpty(IPAddress) || !String.IsNullOrEmpty(ServerDNSName);
                return !client.Connected;
            }
            return false;
        }

        [ACMethodInteraction(nameof(ClosePort), "en{'Close Connection'}de{'Schliesse Verbindung'}", 201, true)]
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
            }
            IsConnected.ValueT = false;
        }

        public bool IsEnabledClosePort()
        {
            var client = TcpClient;
            if (client != null)
                return true;
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
            IsConnected.ValueT = false;
        }

        #endregion


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

                    LP4PrintJob lp4PrintJob = null;
                    using (ACMonitor.Lock(this._61000_LockPort))
                    {
                        if (LP4PrintJobs.Any())
                        {
                            lp4PrintJob = LP4PrintJobs.Dequeue();
                        }
                        if (lp4PrintJob != null)
                        {
                            jobInfo = lp4PrintJob.GetJobInfo();
                            ProcessJob(lp4PrintJob);
                            Messages.LogInfo(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(Poll)}(10)", $"Job {jobInfo} processed!");
                        }
                    }
                    _PollThread.StopReportingExeTime();
                }
            }
            catch (ThreadAbortException ec)
            {
                string message = $"Exception processing job {jobInfo}! Exception: {ec.Message}";
                LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LP4PrinterAlarm), message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(Poll)}(20)", ec);
                }
                OnNewAlarmOccurred(LP4PrinterAlarm, message, true);
            }
        }


        public bool Request(byte[] packet)
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
                            stream.Write(packet, 0, packet.Length);
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                if (IsAlarmActive(nameof(LP4PrinterAlarm), ex.Message) == null)
                {
                    Messages.LogException(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(Request)}(40)", ex);
                }
                OnNewAlarmOccurred(LP4PrinterAlarm, ex.Message, true);
            }

            return success;
        }


        public (bool, byte[]) Response(LP4PrintJob lp4PrintJob)
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
                            LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                            if (IsAlarmActive(nameof(LP4PrinterAlarm), ex.Message) == null)
                            {
                                Messages.LogException(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(Response)}(50)", ex);
                            }
                            OnNewAlarmOccurred(LP4PrinterAlarm, ex.Message, true);
                        }

                    }
                }
            }

            if (success)
            {
                //if (telegram.LinxPrintJobType != LinxPrintJobTypeEnum.RasterData
                //    && telegram.LinxPrintJobType != LinxPrintJobTypeEnum.DeleteReport)
                //    success = LinxHelper.ValidateChecksum(result.ToArray());
                //if (!success)
                //{
                //    string message = $"Bad checksum for response: {linxPrintJob.GetJobInfo()}";
                //    LP4PrinterAlarm.ValueT = PANotifyState.AlarmOrFault;
                //    Messages.LogError(GetACUrl(), $"{nameof(LP4Printer)}.{nameof(Request)}(70)", message);
                //    OnNewAlarmOccurred(LP4PrinterAlarm, message, true);
                //}
            }
            return (success, result.ToArray());
        }

    }
}
