using DocumentFormat.OpenXml.Drawing.Diagrams;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
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

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Settings

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

        [ACPropertyBindingSource(9999, "Error", "en{'Communication alarm'}de{'Communication-Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> CommAlarm { get; set; }
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
            bool requestSuccess = Request(data);
            if (requestSuccess)
            {
                System.Threading.Thread.Sleep(ReceiveTimeout);
                (bool responseSuccess, byte[] responseData) = Response();
                if (responseSuccess && responseData != null)
                {
                    ParsePrinterStatusFull(responseData);
                }
            }
        }

        private (bool, LinxPrinterStatusResponse) ParsePrinterStatusFull(byte[] responseData)
        {
            bool success = false;
            LinxPrinterStatusResponse response = new LinxPrinterStatusResponse();

            bool isCheckSumValid = ValidateChecksum(responseData);
            if (isCheckSumValid)
            {
                response = LinxMapping<LinxPrinterStatusResponse>.Map(responseData);
            }
            else
            {
                //CommAlarm.ValueT = PANotifyState.AlarmOrFault;
                //if (IsAlarmActive("CommAlarm", e.Message) == null)
                //    Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(OpenPort)}(10)", e);
                OnNewAlarmOccurred(CommAlarm, $"Response checksum is not valid!", true);
                ClosePort();
            }


            return (success, response);
        }

        #endregion

        #region Communication -> Open / Close port

        [ACMethodInteraction(nameof(LinxPrinter), "en{'Open Connection'}de{'Öffne Verbindung'}", 200, true)]
        public void OpenPort()
        {
            if (!IsEnabledOpenPort())
            {
                UpdateIsConnectedState();
                return;
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
                }
                catch (Exception e)
                {
                    CommAlarm.ValueT = PANotifyState.AlarmOrFault;
                    if (IsAlarmActive("CommAlarm", e.Message) == null)
                        Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(OpenPort)}(10)", e);
                    OnNewAlarmOccurred(CommAlarm, e.Message, true);
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
                            _serialPort.Open();
                    }
                }
                catch (Exception e)
                {
                    CommAlarm.ValueT = PANotifyState.AlarmOrFault;
                    if (IsAlarmActive("CommAlarm", e.Message) == null)
                        Messages.LogException(GetACUrl(), $"{nameof(LinxPrinter)}.{nameof(OpenPort)}(20)", e);
                    OnNewAlarmOccurred(CommAlarm, e.Message, true);
                }
                UpdateIsConnectedState();
            }
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
            bool success = false;
            OpenPort();
            if (TCPCommEnabled)
            {
                if (TcpClient == null || !TcpClient.Connected)
                    return false;
                NetworkStream stream = TcpClient.GetStream();
                if (stream == null || !stream.CanWrite)
                    return false;
                stream.Write(data, 0, data.Length);
            }
            else if (SerialCommEnabled)
            {
                if (!SerialPort.IsOpen)
                    return false;
                try
                {
                    SerialPort.Write(data, 0, data.Length);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return success;
        }


        public (bool, byte[]) Response()
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

                                success = result != null;

                                if (success)
                                {
                                    success = ValidateChecksum(result.ToArray());
                                    if (!success)
                                    {

                                    }
                                }
                            }
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
                            success = true;
                        }
                        catch (TimeoutException)
                        {
                        }
                    }
                }
            }
            return (success, result.ToArray());
        }

        #endregion

        #endregion

        #region Byte operations

        public byte[] GetData(LinxPrinterCommandCodeEnum commandCode, byte[] data)
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
                byte[] checkSum = GetCheckSum(tempArr);
                ms.Write(checkSum, 0, checkSum.Length);

                return ms.ToArray();
            }
        }

        public void ReciveData(byte[] input)
        {

        }

        public byte[] GetCheckSum(byte[] input)
        {
            int checkSum = BitConverter.ToInt32(input, 0) & 0x0FF;
            checkSum = 0x100 - checkSum;
            return BitConverter.GetBytes(checkSum);
        }

        public bool ValidateChecksum(byte[] dataWithCheckSum)
        {
            bool isValid = false;
            if (dataWithCheckSum != null && dataWithCheckSum.Length > 2)
            {
                byte[] data = new byte[dataWithCheckSum.Length - 2];
                byte[] inputChecksum = new byte[2];

                Array.Copy(dataWithCheckSum, 0, data, 0, dataWithCheckSum.Length - 2);
                Array.Copy(dataWithCheckSum, 0, inputChecksum, dataWithCheckSum.Length - 2 - 1, 2);

                byte[] calcCheckSum = GetCheckSum(data);

                isValid = inputChecksum == calcCheckSum;
            }
            return isValid;
        }

        #endregion


        #region ACPrintServerBase

        public override void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionTableHeader(PrintContext printContext, Table table)
        {
        }

        public override void OnRenderSectionTableFooter(PrintContext printContext, Table table)
        {
        }

        public override void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn)
        {
        }

        public override void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
        }

        public override void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
        }

        public override void OnRenderParagraphHeader(PrintContext printContext, Paragraph paragraph)
        {
        }

        public override void OnRenderParagraphFooter(PrintContext printContext, Paragraph paragraph)
        {
        }

        public override void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
        }

        public override void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
        }

        public override void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
        }

        public override void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {
        }

        public override void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
        }

        public override void OnRenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue)
        {
        }

        public override void OnRenderRun(PrintContext printContext, Run run)
        {
        }

        public override void OnRenderLineBreak(PrintContext printContext, LineBreak lineBreak)
        {
        }

        #endregion

    }
}
