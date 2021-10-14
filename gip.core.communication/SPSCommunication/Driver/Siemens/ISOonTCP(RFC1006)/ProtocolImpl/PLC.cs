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

        public ErrorCode lastErrorCode = 0;
        public string lastErrorString = "";

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
        public ErrorCode Open()
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
                    lastErrorCode = ErrorCode.IPAdressNotAvailable;
                    lastErrorString = "Destination IP-Address '" + IP + "' is not available!" + e.Message;
                    return lastErrorCode;
                }
            }
            if (!succeeded)
            {
                lastErrorCode = ErrorCode.IPAdressNotAvailable;
                lastErrorString = "Destination IP-Address '" + IP + "' is not available!";
                return lastErrorCode;
            }

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
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = sockEx.Message;
                return ErrorCode.ConnectionError;
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = dispEx.Message;
                return ErrorCode.ConnectionError;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = ex.Message;
                return ErrorCode.ConnectionError;
            }

		    try {
			    byte[] bSend1 = { 3, 0, 0, 22, 17, 224, 0, 0, 0, 46, 
			    0, 193, 2, 1, 0, 194, 2, 3, 0, 192, 
			    1, 9 };
			    switch (CPU) {
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
					    return ErrorCode.WrongCPU_Type;
			    }


                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(bSend1, 22, SocketFlags.None);

                    if (mSocket.Receive(bReceive, 22, SocketFlags.None) != 22)
                        throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }

			    byte[] bsend2 = { 3, 0, 0, 25, 2, 240, 128, 50, 1, 0, 
			    0, 255, 255, 0, 8, 0, 0, 240, 0, 0, 
			    3, 0, 3, 1, 0 };

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    mSocket.Send(bsend2, 25, SocketFlags.None);

                    if (mSocket.Receive(bReceive, 27, SocketFlags.None) != 27)
                        throw new Exception(ErrorCode.WrongNumberReceivedBytes.ToString());
                }
			    IsConnected = true;
		    }
		    catch (Exception ex)
            {
			    lastErrorCode = ErrorCode.ConnectionError;
			    lastErrorString = "Couldn't establish the connection: " + ex.Message;
			    IsConnected = false;
			    return ErrorCode.ConnectionError;
		    }

		    return ErrorCode.NoError;
		    // ok
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

        /*  Aufbau eines Data TPDUs

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
        public ErrorCode ReadBytes(DataType DataType, int dbNo, int startByteAddr, int count, out byte[] bytes)
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
                
                package.Add(new byte[] { 0x04, 0x01, 0x12, 
                                         0x0a, 0x10});
                // data type:
                switch (DataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
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
                package.Add((byte)0);

                switch (DataType)
                {
                    case DataType.Timer:
                    case DataType.Counter:
                        package.Add(Types.Word.ToByteArray((ushort)(startByteAddr), EndianessEnum.BigEndian));
                        break;
                    default:
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

                
                //21 data error code 
                    //Error Code Description 
                    //0x00ff no error 
                    //0x0001 there is no peripheral at given address 
                    //0x0003 a piece of data is not available, e.g. when trying to read a bit block (with a length other than 1) or a non existing DB (200 family) 
                    //0x0005 the data address exceeds the address range 
                    //0x0006 can not read a bit block with a length other than 1 
                    //0x0007 write data size does not fit item size 
                    //0x000A a piece of data is not available, e.g. when trying to read a non existing DB 

                if (bReceive[21] == 0xff)
                {
                    for (int cnt = 0; cnt < count; cnt++)
                        bytes[cnt] = bReceive[cnt + 25];
                    lastErrorCode = ErrorCode.NoError;
                    lastErrorString = "";
                    return lastErrorCode;
                }
                else if (bReceive[21] == 0x05)
                {
                    lastErrorCode = ErrorCode.DBRangeToSmall;
                    lastErrorString = String.Format("DB-Size in PLC to small! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, count, startByteAddr + count);
                    return lastErrorCode;
                }
                else if (bReceive[21] == 0x0A)
                {
                    lastErrorCode = ErrorCode.DBNotExist;
                    lastErrorString = String.Format("DB does not exist in PLC! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, count, startByteAddr + count);
                    return lastErrorCode;
                }

                return ErrorCode.ReadData;
            }
            catch (SocketException sockEx)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = sockEx.Message;
                return lastErrorCode;
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = dispEx.Message;
                return lastErrorCode;
            }
            catch (Exception e)
            {
                ReadError = true;
                lastErrorCode = ErrorCode.Exception;
                lastErrorString = e.Message;
                return lastErrorCode;
            }
        }

        public object Read(DataType DataType, int DB, int StartByteAdr, VarType VarType, int VarCount)
        {
            byte[] bytes = null;
            int cntBytes = 0;

            switch (VarType)
            {
                case VarType.Byte:
                    cntBytes = VarCount;
                    if (cntBytes < 1) cntBytes = 1;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return bytes[0];
                    else
                        return bytes;
                case VarType.Word:
                    cntBytes = VarCount * 2;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.Word.FromByteArray(bytes, Endianess);
                    else
                        return Types.Word.ToArray(bytes, Endianess);
                case VarType.Int:
                    cntBytes = VarCount * 2;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null)
                        return null;
                    if (VarCount == 1)
                        return Types.Int.FromByteArray(bytes, Endianess);
                    else
                        return Types.Int.ToArray(bytes, Endianess);
                case VarType.DWord:
                    cntBytes = VarCount * 4;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.DWord.FromByteArray(bytes, Endianess);
                    else
                        return Types.DWord.ToArray(bytes, Endianess);
                case VarType.DInt:
                    cntBytes = VarCount * 4;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.DInt.FromByteArray(bytes, Endianess);
                    else
                        return Types.DInt.ToArray(bytes, Endianess);
                case VarType.Real:
                    cntBytes = VarCount * 4;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.Real.FromByteArray(bytes, Endianess);
                    else
                        return Types.Real.ToArray(bytes, Endianess);
                case VarType.String:
                    cntBytes = VarCount;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    return Types.String.FromByteArray(bytes);
                case VarType.Array:
                    cntBytes = VarCount;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null)
                        return null;
                    return bytes;
                case VarType.Timer:
                    cntBytes = VarCount * 2;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.Timer.FromByteArray(bytes);
                    else
                        return Types.Timer.ToArray(bytes);
                case VarType.Counter:
                    cntBytes = VarCount * 2;
                    if (ReadBytes(DataType, DB, StartByteAdr, cntBytes, out bytes) != ErrorCode.NoError)
                        return null;
                    if (bytes == null) 
                        return null;
                    if (VarCount == 1)
                        return Types.Counter.FromByteArray(bytes);
                    else
                        return Types.Counter.ToArray(bytes);
            }
            return null;
        }

        public object Read(string variable)
        {
            DataType mDataType;
            VarType mVarType;
            int mDB;
            int mByte;
            short mBit;
            int mLength;

            bool resolved = ItemSyntaxResolver.Resolve(variable, out mDataType, out mVarType, out mDB, out mByte, out mLength, out mBit);
            if (!resolved)
            {
                lastErrorCode = ErrorCode.WrongVarFormat;
                lastErrorString = "The Variable '" + variable + "' coudn't be decoded!";
                return null;
            }

            object readResult = Read(mDataType, mDB, mByte, mVarType, 1);
            if (readResult == null)
            {
                lastErrorCode = ErrorCode.WrongVarFormat;
                lastErrorString = "The Variable '" + variable + "' coudn't be read!";
                return null;
            }

            switch (mVarType)
            {
                case VarType.Bit:
                    bool[] objBoolArray = (bool[])readResult;
                    return objBoolArray[mBit];
                default:
                    return readResult;
            }

            //return readResult;


            //DataType mDataType;
            //int mDB;
            //int mByte;
            //int mBit;

            //byte objByte;
            //UInt16 objUInt16;
            //UInt32 objUInt32;
            //double objDouble;
            //bool[] objBoolArray;

            //string txt = variable.ToUpper();
            //txt = txt.Replace(" ", "");     // remove spaces

            //try
            //{
            //    switch (txt.Substring(0, 2))
            //    {
            //        case "DB":
            //            string[] strings = txt.Split(new char[] { '.' });
            //            if (strings.Length < 2)
            //                throw new Exception();

            //            mDB = int.Parse(strings[0].Substring(2));
            //            mDataType = DataType.DataBlock;
            //            string dbType = strings[1].Substring(0, 3);
            //            int dbIndex = int.Parse(strings[1].Substring(3));

            //            switch (dbType)
            //            {
            //                case "DBB":
            //                    byte obj = (byte)Read(DataType.DataBlock, mDB, dbIndex, VarType.Byte, 1);
            //                    return obj;
            //                case "DBW":
            //                    UInt16 objI = (UInt16)Read(DataType.DataBlock, mDB, dbIndex, VarType.Word, 1);
            //                    return objI;
            //                case "DBD":
            //                    UInt32 objU = (UInt32)Read(DataType.DataBlock, mDB, dbIndex, VarType.DWord, 1);
            //                    return objU;
            //                case "DBX":
            //                    mByte = dbIndex;
            //                    mBit = int.Parse(strings[2]);
            //                    if (mBit > 7) throw new Exception();
            //                    objBoolArray = (bool[])Read(DataType.DataBlock, mDB, mByte, VarType.Bit, 1);
            //                    return objBoolArray[mBit];
            //                default:
            //                    throw new Exception();
            //            }
            //        case "EB":
            //            // Eingangsbyte
            //            objByte = (byte)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
            //            return objByte;
            //        case "EW":
            //            // Eingangswort
            //            objUInt16 = (UInt16)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
            //            return objUInt16;
            //        case "ED":
            //            // Eingangsdoppelwort
            //            objUInt32 = (UInt32)Read(DataType.Input, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
            //            return objUInt32;
            //        case "AB":
            //            // Ausgangsbyte
            //            objByte = (byte)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
            //            return objByte;
            //        case "AW":
            //            // Ausgangswort
            //            objUInt16 = (UInt16)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
            //            return objUInt16;
            //        case "AD":
            //            // Ausgangsdoppelwort
            //            objUInt32 = (UInt32)Read(DataType.Output, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
            //            return objUInt32;
            //        case "MB":
            //            // Merkerbyte
            //            objByte = (byte)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.Byte, 1);
            //            return objByte;
            //        case "MW":
            //            // Merkerwort
            //            objUInt16 = (UInt16)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.Word, 1);
            //            return objUInt16;
            //        case "MD":
            //            // Merkerdoppelwort
            //            objUInt32 = (UInt32)Read(DataType.Marker, 0, int.Parse(txt.Substring(2)), VarType.DWord, 1);
            //            return objUInt32;
            //        default:
            //            switch (txt.Substring(0, 1))
            //            {
            //                case "E":
            //                case "I":
            //                    // Eingang
            //                    mDataType = DataType.Input;
            //                    break;
            //                case "A":
            //                case "O":
            //                    // Ausgang
            //                    mDataType = DataType.Output;
            //                    break;
            //                case "M":
            //                    // Merker
            //                    mDataType = DataType.Marker;
            //                    break;
            //                case "T":
            //                    // Timer
            //                    objDouble = (double)Read(DataType.Timer, 0, int.Parse(txt.Substring(1)), VarType.Timer, 1);
            //                    return objDouble;
            //                case "Z":
            //                case "C":
            //                    // Counter
            //                    objUInt16 = (UInt16)Read(DataType.Counter, 0, int.Parse(txt.Substring(1)), VarType.Counter, 1);
            //                    return objUInt16;
            //                default:
            //                    throw new Exception();
            //            }

            //            string txt2 = txt.Substring(1);
            //            if (txt2.IndexOf(".") == -1) throw new Exception();

            //            mByte = int.Parse(txt2.Substring(0, txt2.IndexOf(".")));
            //            mBit = int.Parse(txt2.Substring(txt2.IndexOf(".") + 1));
            //            if (mBit > 7) throw new Exception();
            //            objBoolArray = (bool[])Read(mDataType, 0, mByte, VarType.Bit, 1);
            //            return objBoolArray[mBit];
            //    }
            //}
            //catch
            //{
            //    lastErrorCode = ErrorCode.WrongVarFormat;
            //    lastErrorString = "Die Variable '" + variable + "' konnte nicht entschlüsselt werden!";
            //    return lastErrorCode;
            //}
        }

        public object ReadStruct(Type structType, int DB)
        {
            double numBytes = Types.Struct.GetStructSize(structType);
            // now read the package
            byte[] bytes = (byte[])Read(DataType.DataBlock, DB, 0, VarType.Byte, (int)numBytes);
            // and decode it
            return Types.Struct.FromBytes(structType, bytes, EndianessEnum.BigEndian);
        }
#endregion

#region Write

        public ErrorCode WriteBytes(DataType DataType, int dbNo, int startByteAddr, ref byte[] value)
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
                package.Add(new byte[] { 0x05, 0x01, 0x12, 0x0a, 0x10, 0x02 });
                package.Add(Types.Word.ToByteArray((ushort)varCount, EndianessEnum.BigEndian));
                package.Add(Types.Word.ToByteArray((ushort)(dbNo), EndianessEnum.BigEndian));
                package.Add((byte)DataType);
                package.Add((byte)0);
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

                if (bReceive[21] == 0xff)
                {
                    lastErrorCode = ErrorCode.NoError;
                    lastErrorString = "";
                    return lastErrorCode;
                }
                else if (bReceive[21] == 0x05)
                {
                    lastErrorCode = ErrorCode.DBRangeToSmall;
                    lastErrorString = String.Format("DB-Size in PLC to small! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, package.array.Length, startByteAddr + package.array.Length);
                    return lastErrorCode;
                }
                else if (bReceive[21] == 0x0A)
                {
                    lastErrorCode = ErrorCode.DBNotExist;
                    lastErrorString = String.Format("DB does not exist in PLC! (DB: {0}, StartAddr: {1}, Length: {2}, EndAddr: {3}", dbNo, startByteAddr, package.array.Length, startByteAddr + package.array.Length);
                    return lastErrorCode;
                }

                return ErrorCode.WriteData;
            }
            catch (SocketException sockEx)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = sockEx.Message;
                return lastErrorCode;
            }
            catch (ObjectDisposedException dispEx)
            {
                IsConnected = false;
                lastErrorCode = ErrorCode.ConnectionError;
                lastErrorString = dispEx.Message;
                return lastErrorCode;
            }
            catch (Exception e)
            {
                WriteError = true;
                lastErrorCode = ErrorCode.Exception;
                lastErrorString = e.Message;
                return lastErrorCode;
            }
        }

        
        public object Write(DataType DataType, int DB, int StartByteAdr, object value)
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
                    return ErrorCode.WrongVarFormat;
            }
            return WriteBytes(DataType, DB, StartByteAdr, ref package);
        }

        public object Write(string variable, object value)
        {
            DataType mDataType;
            VarType mVarType;
            int mDB;
            int mByte;
            short mBit;
            int mLength;
            object objValue = value;

            bool resolved = ItemSyntaxResolver.Resolve(variable, out mDataType, out mVarType, out mDB, out mByte, out mLength, out mBit);
            if (!resolved)
            {
                lastErrorCode = ErrorCode.WrongVarFormat;
                lastErrorString = "The Variable '" + variable + "' coudn't be decoded!";
                return null;
            }

            if (mDataType == DataType.DataBlock)
            {
                try
                {
                    switch (mVarType)
                    {
                        case VarType.Bit:
                            byte b = (byte)Read(mDataType, mDB, mByte, VarType.Byte, 1);
                            if ((int)value == 1)
                                b = (byte)(b | (byte)Math.Pow(2, mBit)); // Bit setzen
                            else
                                b = (byte)(b & (b ^ (byte)Math.Pow(2, mBit))); // Bit rücksetzen
                            objValue = (byte)b;
                            break;
                        case VarType.Byte:
                            objValue = Convert.ChangeType(value, typeof(byte));
                            break;
                        case VarType.Word:
                            objValue = Convert.ChangeType(value, typeof(UInt16));
                            break;
                        case VarType.DWord:
                            objValue = Convert.ChangeType(value, typeof(UInt32));
                            break;
                        case VarType.Int:
                            objValue = Convert.ChangeType(value, typeof(Int16));
                            break;
                        case VarType.DInt:
                            objValue = Convert.ChangeType(value, typeof(Int32));
                            break;
                        case VarType.Real:
                            objValue = Convert.ChangeType(value, typeof(Double));
                            break;
                        case VarType.String:
                            string strVal = (String)Convert.ChangeType(value, typeof(String));
                            if (strVal.Length > mLength)
                                strVal = strVal.Substring(0, mLength);
                            objValue = strVal;
                            break;
                        case VarType.Array:
                            objValue = (byte[])value;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    lastErrorCode = ErrorCode.WrongVarFormat;
                    lastErrorString = "Die Variable '" + variable + "' coudn't be decoded!";
                    return null;
                }
            }
            //else if (mDataType == DataType.Input)...
            //{
            //}
            return Write(mDataType, mDB, mByte, objValue);

            //return Write(mDataType, mDB, mByte, (byte)objValue);
            
            //DataType mDataType;
            //int mDB;
            //int mByte;
            //int mBit;

            //string txt2;
            //byte _byte;
            //object objValue;

            //string txt = variable.ToUpper();
            //txt = txt.Replace(" ", ""); // Leerzeichen entfernen 

            //try
            //{
            //    switch (txt.Substring(0, 2))
            //    {
            //        case "DB":
            //            string[] strings = txt.Split(new char[]{'.'});
            //            if (strings.Length < 2)
            //                throw new Exception();

            //            mDB = int.Parse(strings[0].Substring(2));
            //            mDataType = DataType.DataBlock;
            //            string dbType = strings[1].Substring(0, 3);
            //            int dbIndex = int.Parse(strings[1].Substring(3));                       
                       
            //            switch (dbType)
            //            {
            //                case "DBB":
            //                    objValue = Convert.ChangeType(value, typeof(byte));
            //                    return Write(DataType.DataBlock, mDB, dbIndex, (byte)objValue);
            //                case "DBW":
            //                    objValue = Convert.ChangeType(value, typeof(UInt16));
            //                    return Write(DataType.DataBlock, mDB, dbIndex, (UInt16)objValue);
            //                case "DBD":
            //                    objValue = Convert.ChangeType(value, typeof(UInt32));
            //                    return Write(DataType.DataBlock, mDB, dbIndex, (UInt32)objValue);
            //                case "DBX":
            //                    mByte = dbIndex;
            //                    mBit = int.Parse(strings[2]);
            //                    if (mBit > 7) throw new Exception();
            //                    byte b = (byte)Read(DataType.DataBlock, mDB, mByte, VarType.Byte, 1);
            //                    if ((int)value == 1)
            //                        b = (byte)(b | (byte)Math.Pow(2, mBit)); // Bit setzen
            //                    else
            //                        b = (byte)(b & (b ^ (byte)Math.Pow(2, mBit))); // Bit rücksetzen

            //                    return Write(DataType.DataBlock, mDB, mByte, (byte)b);
            //                case "DBS":
            //                    // DB-String
            //                    return Write(DataType.DataBlock, mDB, dbIndex, (string)value);
            //                default:
            //                    throw new Exception();
            //            }
            //        case "EB":
            //            // Eingangsbyte
            //            objValue = Convert.ChangeType(value, typeof(byte));
            //            return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (byte)objValue);
            //        case "EW":
            //            // Eingangswort
            //            objValue = Convert.ChangeType(value, typeof(UInt16));
            //            return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
            //        case "ED":
            //            // Eingangsdoppelwort
            //            objValue = Convert.ChangeType(value, typeof(UInt32));
            //            return Write(DataType.Input, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
            //        case "AB":
            //            // Ausgangsbyte
            //            objValue = Convert.ChangeType(value, typeof(byte));
            //            return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (byte)objValue);
            //        case "AW":
            //            // Ausgangswort
            //            objValue = Convert.ChangeType(value, typeof(UInt16));
            //            return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
            //        case "AD":
            //            // Ausgangsdoppelwort
            //            objValue = Convert.ChangeType(value, typeof(UInt32));
            //            return Write(DataType.Output, 0, int.Parse(txt.Substring(2)), (UInt32)objValue);
            //        case "MB":
            //            // Merkerbyte
            //            objValue = Convert.ChangeType(value, typeof(byte));
            //            return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), (byte)objValue);
            //        case "MW":
            //            // Merkerwort
            //            objValue = Convert.ChangeType(value, typeof(UInt16));
            //            return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), (UInt16)objValue);
            //        case "MD":
            //            // Merkerdoppelwort
            //            return Write(DataType.Marker, 0, int.Parse(txt.Substring(2)), value);
            //        default:
            //            switch (txt.Substring(0, 1))
            //            {
            //                case "E":
            //                case "I":
            //                    // Eingang
            //                    mDataType = DataType.Input;
            //                    break;
            //                case "A":
            //                case "O":
            //                    // Ausgang
            //                    mDataType = DataType.Output;
            //                    break;
            //                case "M":
            //                    // Merker
            //                    mDataType = DataType.Marker;
            //                    break;
            //                case "T":
            //                    // Timer
            //                    return Write(DataType.Timer, 0, int.Parse(txt.Substring(1)), (double)value);
            //                case "Z":
            //                case "C":
            //                    // Zähler
            //                    return Write(DataType.Counter, 0, int.Parse(txt.Substring(1)), (short)value);
            //                default:
            //                    throw new Exception("Unbekannte Variable");
            //            }

            //            txt2 = txt.Substring(1);
            //            if (txt2.IndexOf(".") == -1) throw new Exception("Unbekannte Variable");

            //            mByte = int.Parse(txt2.Substring(0, txt2.IndexOf(".")));
            //            mBit = int.Parse(txt2.Substring(txt2.IndexOf(".") + 1));
            //            if (mBit > 7) throw new Exception("Unbekannte Variable");
            //            _byte = (byte)Read(mDataType, 0, mByte, VarType.Byte, 1);
            //            if ((int)value == 1)
            //                _byte = (byte)(_byte | (byte)Math.Pow(2, mBit));      // Bit setzen
            //            else
            //                _byte = (byte)(_byte & (_byte ^ (byte)Math.Pow(2, mBit))); // Bit rücksetzen

            //            return Write(mDataType, 0, mByte, (byte)_byte);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string msg = ex.Message;
            //    lastErrorCode = ErrorCode.WrongVarFormat;
            //    lastErrorString = "Die Variable '" + variable + "' konnte nicht entschlüsselt werden!";
            //    return lastErrorCode;
            //}
        }

        
        public ErrorCode WriteStruct(object structValue, int DB)
        {
            try
            {
                byte[] bytes = Types.Struct.ToBytes(structValue, Endianess);
                ErrorCode errCode = WriteBytes(DataType.DataBlock, DB, 0, ref bytes);
                return errCode;
            }
            catch
            {
                lastErrorCode = ErrorCode.WriteData;
                lastErrorString = "Error while writing the Struct!";
                return lastErrorCode;
            }
        }
#endregion


#region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#endregion
    }
}
