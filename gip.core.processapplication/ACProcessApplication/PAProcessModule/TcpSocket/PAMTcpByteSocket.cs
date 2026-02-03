// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TCP Byte Socket'}de{'TCP Byte Socket'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, "", true)]
    public class PAMTcpByteSocket : PAProcessModule
    {
        #region c´tors

        static PAMTcpByteSocket()
        {
            RegisterExecuteHandler(typeof(PAMTcpByteSocket), HandleExecuteACMethod_PAMTcpByteSocket);
        }

        public PAMTcpByteSocket(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _TCPBuffer = new byte[1024];
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (IsClient)
            {
                TCPConnect();
            }
            else
            {
                TCPServerStart();
            }
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (IsClient)
            {
                TCPDisconnect();
            }
            else
            {
                TCPServerStop();
            }

            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        //TCP Protocol
        private Socket _Socket;
        private byte[] _TCPBuffer;

        private TcpListener _TCPListener;
        private Thread _ListenThread;
        private TcpClient _TCPClient;

        string _IPAddressV4;
        [ACPropertyInfo(1)]
        [DefaultValueAttribute("127.0.0.1")]
        public string IPAddressV4
        {
            get
            {
                return _IPAddressV4;
            }
            set
            {
                _IPAddressV4 = value;
                OnPropertyChanged("IPAddressV4");
            }
        }

        int _Port;
        [ACPropertyInfo(2)]
        [DefaultValueAttribute(8080)]
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
                OnPropertyChanged("Port");
            }
        }

        bool _IsServer = false;
        [ACPropertyInfo(3)]
        bool IsServer
        {
            get
            {
                return _IsServer;
            }
            set
            {
                _IsServer = value;
                OnPropertyChanged("IsServer");
                OnPropertyChanged("IsClient");
            }
        }

        [ACPropertyInfo(4)]
        bool IsClient
        {
            get
            {
                return !_IsServer;
            }
            set
            {
                _IsServer = !value;
                OnPropertyChanged("IsServer");
                OnPropertyChanged("IsClient");
            }
        }

        bool _IsConnected = false;
        [ACPropertyInfo(5)]
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
            set
            {
                _IsConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }
        #endregion

        #region Methods
        #region Client
        public virtual bool TCPConnect()
        {
            try
            {
                IPAddress ipo = IPAddress.Parse(IPAddressV4);
                IPEndPoint ipEo = new IPEndPoint(ipo, Port);

                _Socket = new Socket(AddressFamily.InterNetwork,
                                  SocketType.Stream,
                                  ProtocolType.Tcp);
                _Socket.ReceiveTimeout = 4000;
                _Socket.Connect(ipEo);
                _Socket.BeginReceive(_TCPBuffer, 0, 1024, 0, new AsyncCallback(TCPReadCallback), this);
                IsConnected = true;
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0} Error code: {1}.", Ex.Message, Ex.GetType());
                IsConnected = false;

                string msg = Ex.Message;
                if (Ex.InnerException != null && Ex.InnerException.Message != null)
                    msg += " Inner:" + Ex.InnerException.Message;

                Messages.LogException("PAMTcpByteSocket", "TCPConnect", msg);

                return false;
            }
        }

        public virtual bool TCPDisconnect()
        {
            try
            {
                if (_Socket.Connected)
                    _Socket.Disconnect(true);
                _Socket = null;
                IsConnected = false;
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0} Error code: {1}.", Ex.Message, Ex.GetType());
                IsConnected = false;

                string msg = Ex.Message;
                if (Ex.InnerException != null && Ex.InnerException.Message != null)
                    msg += " Inner:" + Ex.InnerException.Message;

                Messages.LogException("PAMTcpByteSocket", "TCPDisconnect", msg);

                return true;
            }
        }
        #endregion

        #region Server
        public virtual void TCPServerStart()
        {
            IPAddress ipAddress = IPAddress.Parse(IPAddressV4);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, Port);

            this._TCPListener = new TcpListener(ipEndPoint);
            this._ListenThread = new Thread(new ThreadStart(ListenForClients));
            this._ListenThread.Start();
        }

        public virtual void TCPServerStop()
        {
            _ListenThread.Join();
        }
        #endregion

        public void TCPSend(byte[] sendBytes)
        {
            if (IsClient)
            {
                if (_Socket != null && _Socket.Connected)
                {
                    try
                    {
                        _Socket.Send(sendBytes);
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine("{0} Error code: {1}.", Ex.Message, Ex.GetType());
                        string msg = Ex.Message;
                        if (Ex.InnerException != null && Ex.InnerException.Message != null)
                            msg += " Inner:" + Ex.InnerException.Message;

                        Messages.LogException("PAMTcpByteSocket", "TCPSend", msg);
                    }
                }
            }
            else
            {
                // TODO: Server Send
                NetworkStream clientStream = _TCPClient.GetStream();

                clientStream.Write(sendBytes, 0, sendBytes.Length);
                clientStream.Flush();
            }
        }

        protected virtual void TCPReceive(byte[] receiveBytes)
        {
        }
        #endregion

        #region private Methods
        #region Client
        private static void TCPReadCallback(IAsyncResult ar)
        {
            PAMTcpByteSocket This = (PAMTcpByteSocket)ar.AsyncState;
            if (This != null)
            {
                Socket sock = This._Socket;
                if (sock != null)
                {
                    try
                    {
                        int read = sock.EndReceive(ar);
                        if (read > 0)
                        {
                            This.TCPReceive(This._TCPBuffer);
                        }
                        sock.BeginReceive(This._TCPBuffer, 0, 1024, 0, new AsyncCallback(TCPReadCallback), This);
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine("{0} Error code: {1}.", Ex.Message, Ex.GetType());
                        string msg = Ex.Message;
                        if (Ex.InnerException != null && Ex.InnerException.Message != null)
                            msg += " Inner:" + Ex.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("PAMTcpByteSocket", "TCPReadCallback", msg);
                    }
                }
            }
        }
        #endregion

        #region Server
        private void ListenForClients()
        {
            this._TCPListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this._TCPListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            _TCPClient = (TcpClient)client;
            NetworkStream clientStream = _TCPClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (Exception Ex)
                {
                    //a socket error has occured
                    string msg = Ex.Message;
                    if (Ex.InnerException != null && Ex.InnerException.Message != null)
                        msg += " Inner:" + Ex.InnerException.Message;

                    Messages.LogException("PAMTcpByteSocket", "HandleClientComm", msg);
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                TCPReceive(message);
            }

            _TCPClient.Close();
            _TCPClient = null;
        }
        #endregion
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAMTcpByteSocket(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAProcessModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
