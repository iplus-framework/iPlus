using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.communication.ISOonTCP
{
    public class PLC : INotifyPropertyChanged
    {
        public const int RFC1006Port = 102;
        public const int PDUMaxSize = 240;  //A typical PDU size is 240 Byte, limiting read calls to 222 byte result length
        public const int PDUMaxDataSize = 222;  //A typical PDU size is 240 Byte, limiting read calls to 222 byte result length

        public class Result
        {
            public Result()
            {
                ErrorCode = ErrorCodeEnum.NoError;
                RWErrorCode = ReadWriteErrorCodeEnum.Success;
                PLCErrorCode = ParameterErrorCodeEnum.NoError;
            }

            public Result(ErrorCodeEnum error, string errorText)
            {
                ErrorCode = error;
                RWErrorCode = ReadWriteErrorCodeEnum.Success;
                PLCErrorCode = ParameterErrorCodeEnum.NoError;
                ErrorText = errorText;
            }

            public Result(ErrorCodeEnum error, ParameterErrorCodeEnum paramError, string errorText)
            {
                ErrorCode = error;
                RWErrorCode = ReadWriteErrorCodeEnum.Success;
                PLCErrorCode = paramError;
                ErrorText = errorText;
            }

            public Result(ErrorCodeEnum error, ReadWriteErrorCodeEnum rwError, string errorText)
            {
                ErrorCode = error;
                RWErrorCode = rwError;
                PLCErrorCode = ParameterErrorCodeEnum.NoError;
                ErrorText = errorText;
            }

            public bool IsSucceeded
            {
                get
                {
                    return     ErrorCode == ErrorCodeEnum.NoError 
                            && RWErrorCode == ReadWriteErrorCodeEnum.Success 
                            && PLCErrorCode == ParameterErrorCodeEnum.NoError;
                }
            }

            public bool IsPLCError
            {
                get
                {
                    return     ErrorCode == ErrorCodeEnum.UnknownPLCError
                            || PLCErrorCode != ParameterErrorCodeEnum.NoError;
                }
            }

            public ErrorCodeEnum ErrorCode
            {
                get; private set;
            }

            public ReadWriteErrorCodeEnum RWErrorCode
            {
                get; private set;
            }

            public ParameterErrorCodeEnum PLCErrorCode
            {
                get; private set;
            }

            public string ErrorText
            {
                get; private set;
            }

            public override string ToString()
            {
                return String.Format("Error: {0}; PLC-Error: {1}; Read/Write-Error: {2}; Message: {3}", ErrorCode, PLCErrorCode, RWErrorCode, ErrorText);
            }
        }

        #region Properties
        public string IP
        { get; set; }

        public CPU_Type CPU
        { get; set; }

        public Int16 Rack
        { get; set; }

        public Int16 Slot
        { get; set; }

        public string Name
        { get; set; }

        public object Tag
        { get; set; }

        public bool UsePingForConnectTest
        {
            get;set;
        }

        int _ReceiveTimeout = 1000;
        public int ReceiveTimeout
        { 
            get 
            { 
                return _ReceiveTimeout; 
            } 
            set 
            { 
                _ReceiveTimeout = value; 
            } 
        }

        int _SendTimeout = 1000;
        public int SendTimeout
        { 
            get 
            { 
                return _SendTimeout; 
            } 
            set 
            { 
                _SendTimeout = value; 
            } 
        }


        private EndianessEnum _Endianess = EndianessEnum.BigEndian;
        public EndianessEnum Endianess
        {
            get
            {
                return _Endianess;
            }
            set
            {
                _Endianess = value;
            }
        }

        public bool IsAvailable
        {
            get
            {
                Ping ping = new Ping();
                PingReply result = ping.Send(IP);
                if (result.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
        }

        //protected ErrorCodeEnum _LastErrorCode = 0;
        //public ErrorCodeEnum LastErrorCode
        //{
        //    get
        //    {
        //        return _LastErrorCode;
        //    }
        //}

        //protected string _LastErrorString = "";
        //public string LastErrorString
        //{
        //    get
        //    {
        //        return _LastErrorString;
        //    }
        //}

        private bool _IsConnected = false;
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
            set
            {
                bool changed = _IsConnected != value;
                _IsConnected = value;
                if (changed)
                    OnPropertyChanged("IsConnected");
            }
        }

        private bool _ReadError = false;
        public bool ReadError
        {
            get
            {
                return _ReadError;
            }
            set
            {
                bool changed = _ReadError != value;
                _ReadError = value;
                if (changed)
                    OnPropertyChanged("ReadError");
            }
        }


        private bool _WriteError = false;
        public bool WriteError
        {
            get
            {
                return _WriteError;
            }
            set
            {
                bool changed = _WriteError != value;
                _WriteError = value;
                if (changed)
                    OnPropertyChanged("WriteError");
            }
        }


        public int LastReadTime = 0;
        public int LastWriteTime = 0;

        // Privates
        private Socket mSocket;
        public readonly ACMonitorObject _11900_SocketLockObj = new ACMonitorObject(11900);

        IPAddress _LocalhostIP;
        private IPAddress LocalhostIP
        {
            get
            {
                if (_LocalhostIP != null)
                    return _LocalhostIP;
                try
                {
                    IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    if (ips != null)
                        _LocalhostIP = ips.Where(c => c.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ISOonTCP.PLC", "LocalhostIP", msg);
                }
                return _LocalhostIP;
            }
        }
        #endregion

        // construction
        public PLC()
        {
            IP = "localhost";
            CPU = CPU_Type.S7400;
            Rack = 0;
            Slot = 2;
            UsePingForConnectTest = false;
        }

        public PLC(CPU_Type cpu, string ip, Int16 rack, Int16 slot, EndianessEnum endianess, bool usePingForConnectTest = true)
        {
            IP = ip;
            CPU = cpu;
            Rack = rack;
            Slot = slot;
            Endianess = endianess;
            UsePingForConnectTest = usePingForConnectTest;
        }

        public PLC(CPU_Type cpu, string ip, Int16 rack, Int16 slot, string name, object tag, EndianessEnum endianess, bool usePingForConnectTest = true)
        {
            IP = ip;
            CPU = cpu;
            Rack = rack;
            Slot = slot;
            Name = name;
            Tag = tag;
            Endianess = endianess;
            UsePingForConnectTest = usePingForConnectTest;
        }

        #region Connection (Open, Close)
        public Result Open()
	    {
		    byte[] bReceive = new byte[256];

            bool succeeded = true;
            if (UsePingForConnectTest)
            {
                try
                {
                    // check if available
                    Ping p = new Ping();
                    PingReply pingReplay = p.Send(IP);
                    if (pingReplay.Status != IPStatus.Success)
                        //throw new Exception();
                        succeeded = false;
                }
                catch (Exception e)
                {
                    return new Result(ErrorCodeEnum.IPAdressNotAvailable, "Destination IP-Address '" + IP + "' is not available!" + e.Message);
                }
            }
            if (!succeeded)
                return new Result(ErrorCodeEnum.IPAdressNotAvailable, "Destination IP-Address '" + IP + "' is not available!");

            try
            {
                // open the channel

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
                    mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SendTimeout);

                    IPEndPoint _server = new IPEndPoint(new IPAddress(IPToByteArray(IP)), RFC1006Port);
                    IPEndPoint _local = new IPEndPoint(LocalhostIP, RFC1006Port);
                    mSocket.Connect(_server);
                }
            }
            catch (SocketException sockEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, sockEx.Message);
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, dispEx.Message);
            }
            catch (Exception ex)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.Exception, ex.Message);
            }

            try
            {
                byte[] bSend1 = { 3, 0, 0, 22, 17, 224, 0, 0, 0, 46,
                0, 193, 2, 1, 0, 194, 2, 3, 0, 192,
                1, 9 };
                switch (CPU)
                {
                    case CPU_Type.S7200:
                        //S7200: Chr(193) & Chr(2) & Chr(16) & Chr(0) 'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 16;
                        bSend1[14] = 0;
                        //S7200: Chr(194) & Chr(2) & Chr(16) & Chr(0) 'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 16;
                        bSend1[18] = 0;
                        break;
                    case CPU_Type.S7300:
                        //S7300: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7300: Chr(194) & Chr(2) & Chr(3) & Chr(2)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    case CPU_Type.S7400:
                    case CPU_Type.S71500:
                        //S7400: Chr(193) & Chr(2) & Chr(1) & Chr(0)  'Eigener Tsap
                        bSend1[11] = 193;
                        bSend1[12] = 2;
                        bSend1[13] = 1;
                        bSend1[14] = 0;
                        //S7400: Chr(194) & Chr(2) & Chr(3) & Chr(3)  'Fremder Tsap
                        bSend1[15] = 194;
                        bSend1[16] = 2;
                        bSend1[17] = 3;
                        bSend1[18] = (byte)(Rack * 2 * 16 + Slot);
                        break;
                    default:
                        return new Result(ErrorCodeEnum.WrongCPU_Type, "ErrorCodeEnum.WrongCPU_Type");
                }


                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(bSend1, 22, SocketFlags.None);

                    if (mSocket.Receive(bReceive, 22, SocketFlags.None) != 22)
                        throw new Exception(ErrorCodeEnum.WrongNumberReceivedBytes.ToString());
                }

                byte[] bsend2 = { 3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
                0, 255, 255, 0, 8, 0, 0, 240, 0, 0,
                3, 0, 3, 1, 0 };

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(bsend2, 25, SocketFlags.None);

                    if (mSocket.Receive(bReceive, 27, SocketFlags.None) != 27)
                        throw new Exception(ErrorCodeEnum.WrongNumberReceivedBytes.ToString());
                }
                IsConnected = true;
            }
            catch (Exception ex)
            {
                return new Result(ErrorCodeEnum.ConnectionError, "Couldn't establish the connection: " + ex.Message);
            }

		    return null;
	    }

	    public void Close()
	    {

            using (ACMonitor.Lock(_11900_SocketLockObj))
            {
                if (mSocket.Connected)
                {
                    mSocket.Close();
                }
            }
            IsConnected = false;
        }

        private byte[] IPToByteArray(string ip)
        {
            IPAddress ipAddrToConnect = null;
            if (ip == "localhost" || ip == "127.0.0.1")
            {
                ipAddrToConnect = LocalhostIP;
                if (ipAddrToConnect == null)
                    ip = "127.0.0.1";
            }

            if (ipAddrToConnect == null)
            {
                try
                {
                    ipAddrToConnect = IPAddress.Parse(ip);
                }
                catch (Exception e)
                {
                    ipAddrToConnect = IPAddress.Parse("127.0.0.1");
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ISOonTCP.PLC", "IPToByteArray", msg);
                }
            }
            return ipAddrToConnect.GetAddressBytes();

        }
        #endregion

        /*  
         *  https://github.com/S7NetPlus/s7netplus
         *  Aufbau eines Data TPDUs

            Byte Description 
            0-3 ISO-TCP header 
            4   length (in byte) of TPDU header 
                (without this byte and possible user data) 
            5   DT code (1111) & credit (always 0000)   Fixed Part 
            6   TPDU number & EOT                       Fixed Part 
            7-... PDU                                   User Data 

            Aufbau der PDUs (Packet Data Units)

            PDUs sind der zentrale Bestandteil der S7 Kommunikation. Da das dazu gehörige Protokoll ein gut behütetes Geheimnis der Siemens AG ist, basieren viele der folgenden Informationen auf Reverse Engeneering und sollten daher immer kritisch betrachtet werden. Ein Teil der Beschreibung wurde aus dem Sourceforge Projekt libnodave übernommen.
            Der grundlegende Aufbau ist bei allen PDUs gleich:

            Byte Description 

            7 0x32 (unknown)            PDU Header 
            8 PDU type (1,2,3 or 7)     PDU Header 
                                         PDU Typ 1:
                                        •10 Byte Header (ohne Error Code) 
                                        •Vorkommen: Request (Open S7 Connection, Read, Write)
                                        PDU Typ 2:
                                        •12 Byte Header (mit Error Code) 
                                        •Vorkommen: unbekannt
                                        PDU Typ 3:
                                        •12 Byte Header (mit Error Code) 
                                        •Vorkommen: Response (Open S7 Connection, Read, Write)
                                        PDU Typ 7:
                                        •10 Byte Header (ohne Error Code) 
                                        •Vorkommen: Request (SZL Diagnose), Response (SZL Diagnose)
            9-10 reserved               PDU Header 
            11-12 sequence number       PDU Header 
            13-14 parameter length (in byte) PDU Header 
            15-16 data length (in byte) PDU Header 
            17-18 error code (only available in PDU type 2 and 3) PDU Header 
            ...-... paramater part 
            ...-... data part 
        */

        #region Read
        public Result ReadBytes(DataTypeEnum DataType, int dbNo, int startByteAddr, int count, out byte[] bytes)
        {
            bytes = new byte[count];

            try
            {
                // first create the header
                int packageSize = 31;
                ByteArray package = new ByteArray(packageSize);

                //ISO-TCP Header:
                //    Byte    Description 
                //    0       version number (for S7 communication always 0x03) 
                //    1       reserved (always 0x00) 
                //    2-3     size of ISO-TCP message (including this header) in byte
                package.Add(new byte[] { 0x03, 0x00, 0x00 });
                package.Add((byte)packageSize);

                //  4       length (in byte) of TPDU header 
                //          (without this byte and possible user data) 
                //  5       DT code (1111) & credit (always 0000)           Fixed Part 
                //  6       TPDU number & EOT                               Fixed Part 
                //  7       0x32 (unknown)                                  PDU Header 
                //  8       PDU type (1,2,3 or 7)                           PDU Header 
                //  9-10    reserved                                        PDU Header 
                //  11-12   sequence number                                 PDU Header 
                //  13-14   parameter length (in byte)                      PDU Header 
                //  15-16   data length (in byte)                           PDU Header 
                //  17-18   error code (only available in PDU type 2 and 3) PDU Header

                package.Add(new byte[] { 0x02, 0xf0, 0x80, 0x32, 0x01, 0x00, 0x00, 0x00, 
                                         0x00, 0x00, 0x0e, 0x00, 0x00});
                
                // 19... Parameter-Request: 
                    // Mittels eines Read Requests ist es dem Client möglich, Variablenbelegungen aus einem Datenbaustein auszulesen. Direkt nach dem Read Code folgt die Anzahl, sowie eine Liste der angeforderten Variablen. Ein Element der Liste besteht aus 12 Bytes (siehe unten).

                    //Byte Description 
                    //0    0x04 (read) 
                    //1    number of data requests 
                    //2    0x12 (unknown)               first data request 
                    //3    0x0A (10 bytes following)    first data request 
                    //4    0x10 (unknown)               first data request 
                    //5    data type                    first data request 
                    //6-7  length                       first data request 
                    //8-9  DB number                    first data request 
                    //10   area code                    first data request                
                            //Area Code Description 
                            //0x00 raw memory 
                            //0x03 system info of 200 family 
                            //0x05 system flags of 200 family 
                            //0x06 analog inputs of 200 family 
                            //0x07 analog outputs of 200 family 
                            //0x80 direct peripheral access 
                            //0x81 inputs 
                            //0x82 outputs 
                            //0x83 flags 
                            //0x84 data blocks 
                            //0x85 instance data blocks 
                            //0x86 system data area ? 
                            //0x87 unknown 
                            //0x1C S7 counters 
                            //0x1D S7 timers 
                            //0x1E IEC counters (200 family) 
                            //0x1F IEC timers (200 family) 

                    //11-13  start address in bits      first data request 
                    //14  0x12 (unknown)                second data request 
                    //15  0x0A (10 bytes following)     second data request 
                    //16  0x10 (unknown)                second data request 
                    //17-18  length                     second data request 
                    //19-20  DB number                  second data request 
                    //21  data type                     second data request 
                    //22  area code                     second data request 
                    //23-25  start address in bits      second data request 
                    //26-37  ...                        third data request 
                    //...-... ...                       following requests 
                    //Read Requests haben keinen Datenteil. 
                
                package.Add(new byte[] { (byte) FunctionCodeEnum.Read, 0x01, 0x12, 
                                         0x0a, 0x10});
                // data type:
                switch (DataType)
                {
                    case DataTypeEnum.Timer:
                    case DataTypeEnum.Counter:
                        package.Add((byte)DataType);
                        break;
                    default:
                        package.Add(0x02);
                        break;
                }

                // length
                package.Add(Types.Word.ToByteArray((ushort)(count), EndianessEnum.BigEndian));
                package.Add(Types.Word.ToByteArray((ushort)(dbNo), EndianessEnum.BigEndian));

                package.Add((byte)DataType);

                // Area-Code

                switch (DataType)
                {
                    case DataTypeEnum.Timer:
                    case DataTypeEnum.Counter:
                        package.Add((byte)0);
                        package.Add(Types.Word.ToByteArray((ushort)(startByteAddr), EndianessEnum.BigEndian));
                        break;
                    default:
                        package.Add((byte)((startByteAddr * 8) / 0x10000));
                        package.Add(Types.Word.ToByteArray((ushort)((startByteAddr) * 8), EndianessEnum.BigEndian));
                        break;
                }

                //byte[] bReceive = new byte[512];
                int readSize = count + 100;
                byte[] bReceive = new byte[readSize];
                int numReceived = 0;

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(package.array, package.array.Length, SocketFlags.None);
                    numReceived = mSocket.Receive(bReceive, readSize, SocketFlags.None);
                }

                Result validationResult = ValidateResponseHeader(ref bReceive, ErrorCodeEnum.ReadData, dbNo, startByteAddr, count);
                if (validationResult == null || validationResult.ErrorCode == ErrorCodeEnum.NoError)
                {
                    for (int cnt = 0; cnt < count; cnt++)
                        bytes[cnt] = bReceive[cnt + 25];
                }
                return validationResult;
            }
            catch (SocketException sockEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, sockEx.Message);
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, dispEx.Message);
            }
            catch (Exception e)
            {
                ReadError = true;
                return new Result(ErrorCodeEnum.Exception, e.Message);
            }
        }

        public Result Read(DataTypeEnum DataType, int DB, int StartByteAdr, VarTypeEnum VarType, int VarCount, out object readResult)
        {
            byte[] bytes = null;
            int cntBytes = 0;

            Result result;
            switch (VarType)
            {
                case VarTypeEnum.Byte:
                    cntBytes = VarCount;
                    if (cntBytes < 1) 
                        cntBytes = 1;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = bytes[0];
                    else
                        readResult = bytes;
                    break;
                case VarTypeEnum.Word:
                    cntBytes = VarCount * 2;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.Word.FromByteArray(bytes, Endianess);
                    else
                        readResult = Types.Word.ToArray(bytes, Endianess);
                    break;
                case VarTypeEnum.Int:
                    cntBytes = VarCount * 2;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.Int.FromByteArray(bytes, Endianess);
                    else
                        readResult = Types.Int.ToArray(bytes, Endianess);
                    break;
                case VarTypeEnum.DWord:
                    cntBytes = VarCount * 4;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.DWord.FromByteArray(bytes, Endianess);
                    else
                        readResult = Types.DWord.ToArray(bytes, Endianess);
                    break;
                case VarTypeEnum.DInt:
                    cntBytes = VarCount * 4;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.DInt.FromByteArray(bytes, Endianess);
                    else
                        readResult = Types.DInt.ToArray(bytes, Endianess);
                    break;
                case VarTypeEnum.Real:
                    cntBytes = VarCount * 4;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.Real.FromByteArray(bytes, Endianess);
                    else
                        readResult = Types.Real.ToArray(bytes, Endianess);
                    break;
                case VarTypeEnum.String:
                    cntBytes = VarCount;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    readResult = Types.String.FromByteArray(bytes);
                    break;
                case VarTypeEnum.Array:
                    cntBytes = VarCount;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    readResult = bytes;
                    break;
                case VarTypeEnum.Timer:
                    cntBytes = VarCount * 2;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.Timer.FromByteArray(bytes);
                    else
                        readResult = Types.Timer.ToArray(bytes);
                    break;
                case VarTypeEnum.Counter:
                    cntBytes = VarCount * 2;
                    result = ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes);
                    if (result != null && !result.IsSucceeded)
                    {
                        readResult = null;
                        return result;
                    }
                    if (bytes == null)
                    {
                        readResult = null;
                        return new Result(ErrorCodeEnum.EmptyResult, "bytes == null");
                    }
                    if (VarCount == 1)
                        readResult = Types.Counter.FromByteArray(bytes);
                    else
                        readResult = Types.Counter.ToArray(bytes);
                    break;
                default:
                    readResult = null;
                    return new Result(ErrorCodeEnum.EmptyResult, "Unsupported VarTypeEnum");
            }
            return null;
        }

        #endregion

        #region Write

        public Result WriteBytes(DataTypeEnum DataType, int dbNo, int startByteAddr, ref byte[] value)
        {
            byte[] bReceive = new byte[513];
            int varCount = 0;

            try
            {
                varCount = value.Length;
                // first create the header
                int packageSize = 35 + value.Length;
                ByteArray package = new ByteArray(packageSize);
                    
                package.Add(new byte[] { 3, 0, 0 });
                package.Add((byte)packageSize);
                package.Add(new byte[] { 2, 0xf0, 0x80, 0x32, 1, 0, 0 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount - 1), EndianessEnum.BigEndian));
                package.Add(new byte[] { 0, 0x0e });
                package.Add(Types.Word.ToByteArray((ushort)(varCount + 4), EndianessEnum.BigEndian));
                package.Add(new byte[] { (byte) FunctionCodeEnum.Write, 0x01, 0x12, 0x0a, 0x10, 0x02 }); // Reserved, Max AMQ Caller 2B, MAX AMQ Callee 2 B
                package.Add(Types.Word.ToByteArray((ushort)varCount, EndianessEnum.BigEndian)); // PDU Length
                package.Add(Types.Word.ToByteArray((ushort)(dbNo), EndianessEnum.BigEndian)); // DB No
                package.Add((byte)DataType);
                package.Add((byte)((startByteAddr * 8) / 0x10000));
                package.Add(Types.Word.ToByteArray((ushort)(startByteAddr * 8), EndianessEnum.BigEndian));
                package.Add(new byte[] { 0, 4 });
                package.Add(Types.Word.ToByteArray((ushort)(varCount * 8), EndianessEnum.BigEndian));

                // now join the header and the data
                package.Add(value);

                int numReceived = 0;
                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(package.array, package.array.Length, SocketFlags.None);
                    numReceived = mSocket.Receive(bReceive, 512, SocketFlags.None);
                }

                //21 data error code 
                //Error Code Description 
                //0x00ff no error 
                //0x0001 there is no peripheral at given address 
                //0x0003 a piece of data is not available, e.g. when trying to read a bit block (with a length other than 1) or a non existing DB (200 family) 
                //0x0005 the data address exceeds the address range 
                //0x0006 can not read a bit block with a length other than 1 
                //0x0007 write data size does not fit item size 
                //0x000A a piece of data is not available, e.g. when trying to read a non existing DB 

                Result validationResult = ValidateResponseHeader(ref bReceive, ErrorCodeEnum.ReadData, dbNo, startByteAddr, package.array.Length);
                return validationResult;
            }
            catch (SocketException sockEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, sockEx.Message);
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                return new Result(ErrorCodeEnum.ConnectionError, dispEx.Message);
            }
            catch (Exception e)
            {
                WriteError = true;
                return new Result(ErrorCodeEnum.Exception, e.Message);
            }
        }

        
        public object Write(DataTypeEnum DataType, int DB, int StartByteAdr, object value)
        {
            byte[] package = null;

            switch (value.GetType().Name)
            {
                case Const.TNameByte:
                    package = Types.Byte.ToByteArray((byte)value);
                    break;
                case Const.TNameInt16:
                    package = Types.Int.ToByteArray((Int16)value, Endianess);
                    break;
                case Const.TNameUInt16:
                    package = Types.Word.ToByteArray((UInt16)value, Endianess);
                    break;
                case Const.TNameInt32:
                    package = Types.DInt.ToByteArray((Int32)value, Endianess);
                    break;
                case Const.TNameUInt32:
                    package = Types.DWord.ToByteArray((UInt32)value, Endianess);
                    break;
                case Const.TNameDouble:
                    package = Types.Real.ToByteArray(Convert.ToSingle((Double)value), Endianess);
                    break;
                case Const.TNameSingle:
                    package = Types.Real.ToByteArray((Single)value, Endianess);
                    break;
                case "Byte[]":
                    package = (byte[])value;
                    break;
                case "Int16[]":
                    package = Types.Int.ToByteArray((Int16[])value, Endianess);
                    break;
                case "UInt16[]":
                    package = Types.Word.ToByteArray((UInt16[])value, Endianess);
                    break;
                case "Int32[]":
                    package = Types.DInt.ToByteArray((Int32[])value, Endianess);
                    break;
                case "UInt32[]":
                    package = Types.DWord.ToByteArray((UInt32[])value, Endianess);
                    break;
                case "Double[]":

                    int count = ((double[])value).Length;
                    float[] arr = new float[count];
                    int i = 0;
                    foreach (double val in (double[])value)
                    {
                        arr[i] = Convert.ToSingle(val);
                        i++;
                    }
                    package = Types.Real.ToByteArray(arr, Endianess);
                    break;
                case "Single[]":
                    package = Types.Real.ToByteArray((Single[])value, Endianess);
                    break;
                case Const.TNameString:
                    String strVal = value as string;
                    package = Types.String.ToByteArray(strVal, System.Convert.ToByte(strVal.Length), strVal.Length+2);
                    break;
                default:
                    return ErrorCodeEnum.WrongVarFormat;
            }
            return WriteBytes(DataType, DB, StartByteAdr, ref package);
        }

#endregion

        #region Error-Handling
        protected Result ValidateResponseHeader(ref byte[] bReceive, ErrorCodeEnum defaultErrorCode, int dbNo, int startByteAddr, int count)
        {
            if (bReceive[17] != (byte)HeaderErrorClassEnum.NoError || bReceive[18] != (byte)ParameterErrorCodeEnum.NoError)
            {
                string errorTxt = "";
                try
                {
                    MessageTypeEnum messageType = (MessageTypeEnum)bReceive[8];
                    HeaderErrorClassEnum? errorClassEnum = BuildHeaderErrorClass(bReceive[17]);
                    ParameterErrorCodeEnum? errorCodeEnum = BuildParamErrorCode(bReceive[17], bReceive[18]);
                    errorTxt = BuildErrorMessage(messageType, bReceive[17], bReceive[18]);
                    if (errorCodeEnum.HasValue)
                        return new Result(defaultErrorCode, errorCodeEnum.Value, errorTxt);
                    else
                        return new Result(ErrorCodeEnum.UnknownPLCError, errorTxt);
                }
                catch (Exception ex)
                {
                    errorTxt = String.Format("HeaderErrorClass : {0}, ParameterErrorCode: {1}, Exception: {2}", bReceive[17], bReceive[18], ex.Message);
                    return new Result(ErrorCodeEnum.UnknownPLCError, errorTxt);
                }
            }

            //21 data error code 
            //Error Code Description 
            //0x00ff no error 
            //0x0001 there is no peripheral at given address 
            //0x0003 a piece of data is not available, e.g. when trying to read a bit block (with a length other than 1) or a non existing DB (200 family) 
            //0x0005 the data address exceeds the address range 
            //0x0006 can not read a bit block with a length other than 1 
            //0x0007 write data size does not fit item size 
            //0x000A a piece of data is not available, e.g. when trying to read a non existing DB 
            if (bReceive[21] == (byte)ReadWriteErrorCodeEnum.Success)
                return null;
            else if (bReceive[21] == (byte)ReadWriteErrorCodeEnum.AddressOutOfRange)
                return new Result(ErrorCodeEnum.DBRangeToSmall, String.Format("DB-Size in PLC to small! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, count, startByteAddr + count));
            else if (bReceive[21] == (byte)ReadWriteErrorCodeEnum.ObjectDoesNotExist)
                return new Result(ErrorCodeEnum.DBNotExist, String.Format("DB does not exist in PLC! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, count, startByteAddr + count));
            else
            {
                ReadWriteErrorCodeEnum? rwError = BuildReadWriteErrorCode(bReceive[21]);
                if (rwError.HasValue)
                    return new Result(defaultErrorCode, rwError.Value, rwError.Value.ToString());
                else
                    return new Result(defaultErrorCode, String.Format("{Unkown RW-Result 0:X}", bReceive[21]));
            }
        }

        static string BuildErrorMessage(MessageTypeEnum messageType, byte errorClass, byte errorCode)
        {
            var sb = new StringBuilder("BuildErrorMessage (10):");

            sb.Append("Message type: ").Append(messageType);

            sb.Append("; Error class hex: ").AppendFormat("{0:X}", errorClass);
            HeaderErrorClassEnum? errorClassEnum = BuildHeaderErrorClass(errorClass);
            if (errorClassEnum.HasValue)
                sb.AppendFormat(", Name: {0}", errorClassEnum.Value.ToString());

            sb.Append("; Param error code: 0x").AppendFormat("{0:X}", errorCode);
            ParameterErrorCodeEnum? errorCodeEnum = BuildParamErrorCode(errorClass, errorCode);
            if (errorCodeEnum.HasValue)
                sb.AppendFormat(", Name: {0}", errorCodeEnum.Value.ToString());

            return sb.ToString();
        }

        static ParameterErrorCodeEnum? BuildParamErrorCode(byte errorClass, byte errorCode)
        {
            ushort combinedErrorCode = (ushort)(((int)errorClass << 8) | errorCode);
            if (Enum.IsDefined(typeof(ParameterErrorCodeEnum), combinedErrorCode))
                return (ParameterErrorCodeEnum)combinedErrorCode;
            return null;
        }

        static HeaderErrorClassEnum? BuildHeaderErrorClass(byte errorClass)
        {
            if (Enum.IsDefined(typeof(HeaderErrorClassEnum), errorClass))
                return (HeaderErrorClassEnum)errorClass;
            return null;
        }

        static ReadWriteErrorCodeEnum? BuildReadWriteErrorCode(byte value)
        {
            if (Enum.IsDefined(typeof(ReadWriteErrorCodeEnum), value))
                return (ReadWriteErrorCodeEnum)value;
            return null;
        }
        #endregion


        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#endregion
    }
}
