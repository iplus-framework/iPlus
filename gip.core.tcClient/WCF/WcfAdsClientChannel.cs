﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gip.core.tcShared.ACVariobatch;
using gip.core.tcShared.WCF;
using gip.core.tcShared;

namespace gip.core.tcClient
{
    public class WcfAdsClientChannel
    {
        #region Private Members

        InstanceContext _instanceContext = null;
        WcfAdsClient _wcfAdsClient = null;
        EndpointAddress _endPoint = null;
        bool _ConnectionOn = false; // gibt an dass Verbindung aufrecht erhalten werden soll
        TimeSpan _reconnectTimeSpan = new TimeSpan(0, 0, 2);
        TCSession _tcSession = null;

        // Producer-Consumer for Sending
        SyncQueueEvents _syncSend;
        Thread _sendThread;

        // Producer-Consumer for Receiving
        SyncQueueEvents _syncReceive;
        Thread _receiveThread;

        // Reconnection-Handling
        Thread _ReconnectThread = null;
        protected ManualResetEvent _ReconnectShutdownEvent;
        Thread _ConnectionThread = null;
        private EventWaitHandle _waitOnDisconnection = null;
        private EventWaitHandle _waitOnConnectionAtInit = null;

        #endregion

        #region c'tors

        public WcfAdsClientChannel(TCSession manager)
        {
            _tcSession = manager;
            _syncSend = new SyncQueueEvents();
            _syncReceive = new SyncQueueEvents();
            InitWcfAdsClientChannel();
        }

        private void InitWcfAdsClientChannel()
        {
            _sendThread = new Thread(SendMessage);
            _sendThread.Start();
            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.Start();

            _waitOnConnectionAtInit = new EventWaitHandle(false, EventResetMode.AutoReset);
            InitConnection();
        }

        void InitConnection()
        {
            if (_ConnectionThread != null)
                return;

            _ConnectionOn = true;

            if (_wcfAdsClient == null)
            {
                if (EndpointUri == null)
                    return;
                _ConnectionThread = new Thread(NewConnectionInstance);
                _ConnectionThread.Start();
            }
        }

        public void DeinitWcfAdsClientChannel()
        {
            if (_ConnectionThread != null)
            {
                Disconnect();
                if (_waitOnDisconnection != null)
                    _waitOnDisconnection.Close();
                if (_waitOnConnectionAtInit != null)
                    _waitOnConnectionAtInit.Close();
                if (_ConnectionThread != null)
                    _ConnectionThread.Join();
                _ConnectionThread = null;
                DeleteServiceOfClient();
            }

            if (_receiveThread != null)
            {
                _syncReceive.TerminateThread();
                _receiveThread.Join();
                _receiveThread = null;
            }

            if (_sendThread != null)
            {
                _syncSend.TerminateThread();
                _sendThread.Join();
                _sendThread = null;
            }

            if (_ReconnectThread != null)
            {
                if (_ReconnectShutdownEvent != null && _ReconnectShutdownEvent.SafeWaitHandle != null && !_ReconnectShutdownEvent.SafeWaitHandle.IsClosed)
                    _ReconnectShutdownEvent.Set();
                if (!_ReconnectThread.Join(1000))
                {
                    try
                    {
                        _ReconnectThread.Abort();
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                       && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                            core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "DeinitWcfAdsClientChannel", msg);
                    }
                }
                _ReconnectShutdownEvent = null;
                _ReconnectThread = null;
            }

            _endpointUri = null;
            //_IsConnected = true;
            _CountReconnects = 0;
            if (_SendQueue != null)
                _SendQueue.Clear();
            if (_ReceiveQueue != null)
                _ReceiveQueue.Clear();
            _instanceContext = null;
            _wcfAdsClient = null;
            _endPoint = null;
            _ConnectionOn = false;
        }

        #endregion

        #region Properties

        private Uri _endpointUri = null;
        public Uri EndpointUri
        {
            get
            {
                if (_endpointUri != null)
                    return _endpointUri;

                // Schema / protocol
                string scheme = "net.tcp";

                // Authority
                string authority = _tcSession.IPAddress;

                authority += ":" + _tcSession.TcpPort.ToString();

                // Address
                _endpointUri = new Uri(String.Format("{0}://{1}/", scheme, authority));
                return _endpointUri;
            }
        }

        private int _CountReconnects = 0;
        public int CountReconnects
        {
            get
            {
                return _CountReconnects;
            }

            set
            {
                _CountReconnects = value;
            }
        }

        #endregion

        #region Methods

        void NewConnectionInstance()
        {
            _endPoint = new EndpointAddress(EndpointUri);
            _instanceContext = new InstanceContext(new WcfAdsClientServiceCallback(this));

            NetTcpBinding netTcpBinding = new NetTcpBinding();
            if (netTcpBinding.ReaderQuotas != null)
                netTcpBinding.ReaderQuotas.MaxStringContentLength = WcfAdsServiceManager.MaxStringLength;
            netTcpBinding.MaxBufferSize = WcfAdsServiceManager.MaxBufferSize;
            netTcpBinding.MaxReceivedMessageSize = WcfAdsServiceManager.MaxBufferSize;
            //netTcpBinding.ConnectionBufferSize = WCFServiceManager.MaxBufferSize / 8;
            netTcpBinding.MaxBufferPoolSize = WcfAdsServiceManager.MaxBufferSize;
            netTcpBinding.ReceiveTimeout = WcfAdsServiceManager.ReceiveTimeout;
            netTcpBinding.Security.Mode = SecurityMode.None;

            _wcfAdsClient = new WcfAdsClient(_instanceContext, netTcpBinding, _endPoint);

            foreach (OperationDescription op in _wcfAdsClient.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = WcfAdsServiceManager.MaxItemsInObjectGraph;
                    //dataContractBehavior.DataContractResolver = ACConvert.MyDataContractResolver;
                }
            }

            AddServiceOfClient();
            SendConnectMessageToOpenChannel();
            _waitOnConnectionAtInit.Set();
        }

        void AddServiceOfClient()
        {
            if (_wcfAdsClient == null)
                return;
            if (_wcfAdsClient.InnerDuplexChannel != null)
            {
                _wcfAdsClient.InnerDuplexChannel.Opened += Channel_Opened;
                _wcfAdsClient.InnerDuplexChannel.Faulted += Channel_Faulted;
                _wcfAdsClient.InnerDuplexChannel.Closing += Channel_Closing;
            }
        }

        void SendConnectMessageToOpenChannel()
        {
            WcfAdsMessage acMessage = new WcfAdsMessage() { ConnectionState = (byte)ConnectionState.Connect };
            EnqeueMessageForService(acMessage);
        }

        public void Disconnect()
        {
            _ConnectionOn = false;

            if ((_wcfAdsClient != null) && (_wcfAdsClient.InnerDuplexChannel != null))
            {
                if (_wcfAdsClient.State != CommunicationState.Closed)
                {
                    _waitOnDisconnection = new EventWaitHandle(false, EventResetMode.AutoReset);
                    SendDisconnectMessageToCloseChannel();
                    _waitOnDisconnection.WaitOne();
                }
            }
        }

        private void SendDisconnectMessageToCloseChannel()
        {
            WcfAdsMessage msg = new WcfAdsMessage() { ConnectionState = (byte)ConnectionState.Disconnect };
            EnqeueMessageForService(msg);
        }

        void DeleteServiceOfClient()
        {
            if (_wcfAdsClient == null)
                return;
            if (_wcfAdsClient.InnerDuplexChannel != null)
            {
                _wcfAdsClient.InnerDuplexChannel.Opened -= Channel_Opened;
                _wcfAdsClient.InnerDuplexChannel.Faulted -= Channel_Faulted;
                _wcfAdsClient.InnerDuplexChannel.Closing -= Channel_Closing;
            }
            _wcfAdsClient = null;
            _instanceContext = null;
        }

        private void IncreaseReconnectTimeout()
        {
            if (_reconnectTimeSpan.TotalSeconds <= 20)
                _reconnectTimeSpan = _reconnectTimeSpan.Add(new TimeSpan(0, 0, 2));
        }

        private void ResetReconnectTimeout()
        {
            _reconnectTimeSpan = new TimeSpan(0, 0, 2);
        }

        public void ResetEndpointUri()
        {
            _endpointUri = null;
        }

        #endregion

        #region Reconnection-Handling

        private void StartReconnect()
        {
            if (_ReconnectThread != null)
                return;
            if (_ConnectionThread != null)
            {
                if (!_ConnectionThread.Join(1000))
                {
                    try
                    {
                        _ConnectionThread.Abort();
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                       && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                            core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "StartReconnect", msg);
                    }
                }
                _ConnectionThread = null;
                DeleteServiceOfClient();
            }

            //DisconnectedSince = DateTime.Now;
            _tcSession.IsConnected.ValueT = false;
            _tcSession.IsPlcConnected = false;
            _tcSession.IsReadyForWriting = false;
            _ReconnectShutdownEvent = new ManualResetEvent(false);
            _ReconnectThread = new Thread(Reconnect);
            _ReconnectThread.Start();
        }

        private void Reconnect()
        {
            while (!_ReconnectShutdownEvent.WaitOne(_reconnectTimeSpan, false))
            {
                IncreaseReconnectTimeout();

                if (_ConnectionThread == null)
                    InitConnection();

                else if (_wcfAdsClient != null)
                {
                    if (_wcfAdsClient.State == CommunicationState.Faulted)
                    {
                        try
                        {
                            _wcfAdsClient.Abort();
                            _instanceContext.Abort();
                        }
                        catch (Exception ec)
                        {
                            string msg = ec.Message;
                            if (ec.InnerException != null && ec.InnerException.Message != null)
                                msg += " Inner:" + ec.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null
                                                                          && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "Reconnect", msg);
                        }
                    }

                    if (_wcfAdsClient.State == CommunicationState.Closed)
                    {
                        if (_ConnectionThread != null)
                        {
                            if (!_ConnectionThread.Join(1000))
                            {
                                try
                                {
                                    _ConnectionThread.Abort();
                                }
                                catch (Exception ec)
                                {
                                    string msg = ec.Message;
                                    if (ec.InnerException != null && ec.InnerException.Message != null)
                                        msg += " Inner:" + ec.InnerException.Message;

                                    if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                                   && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                        core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "Reconnect", msg);
                                }
                            }
                            _ConnectionThread = null;
                            DeleteServiceOfClient();
                        }
                    }
                    else
                        SendConnectMessageToOpenChannel();
                }
            }
        }

        #endregion

        #region Event handling (Closing, Faulted, Opened)

        private void Channel_Closing(object sender, EventArgs e)
        {
            if (_ConnectionOn == true)
            {
                StartReconnect();
            }
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            if (_ConnectionOn == true)
            {
                if ((_wcfAdsClient != null) && (_wcfAdsClient.InnerDuplexChannel != null))
                {
                    StartReconnect();
                }
            }
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            ResetReconnectTimeout();
            if (_ReconnectShutdownEvent != null && _ReconnectShutdownEvent.SafeWaitHandle != null && !_ReconnectShutdownEvent.SafeWaitHandle.IsClosed)
                _ReconnectShutdownEvent.Set();
            if (_ReconnectThread != null && !_ReconnectThread.Join(100))
            {
                try
                {
                    _ReconnectThread.Abort();
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                   && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                        core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "Channel_Opened", msg);
                }
            }
            _ReconnectShutdownEvent = null;
            _ReconnectThread = null;
            CountReconnects++;
            _tcSession.IsConnected.ValueT = true;
            _tcSession.IsReadyForWriting = true;
        }

        #endregion

        #region Sending

        private Queue<WcfAdsMessage> _SendQueue = new Queue<WcfAdsMessage>();
        internal Queue<WcfAdsMessage> SendQueue
        {
            get { return _SendQueue; }
            set { _SendQueue = value; }
        }

        public void EnqeueMessageForService(WcfAdsMessage message)
        {
            if (!_syncSend.NewItemsEnqueueable)
                return;
            lock (_syncSend._20010_QueueSyncLock)
            {
                this.SendQueue.Enqueue(message);
            }
            // Signalisiere Thread, dass neue Message ansteht
            _syncSend.NewItemEvent.Set();
        }

        void SendMessage()
        {
            while (!_syncSend.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncSend.NewItemEvent.WaitOne();

                while (SendQueue.Count > 0)
                {
                    WcfAdsMessage message = null;
                    lock (_syncSend._20010_QueueSyncLock)
                    {
                        message = SendQueue.Dequeue();
                    }
                    if (message != null & _wcfAdsClient != null)
                    {
                        try
                        {
                            _wcfAdsClient.Invoke(message);
                        }
                        catch (Exception ec)
                        {
                            string msg = ec.Message;
                            if (ec.InnerException != null && ec.InnerException.Message != null)
                                msg += " Inner:" + ec.InnerException.Message;

                            if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                           && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "SendMessage", msg);
                        }
                        if (message.ConnectionState != null && ((ConnectionState)Enum.ToObject(typeof(ConnectionState), message.ConnectionState) == ConnectionState.Disconnect))
                        {
                            try
                            {
                                if (_wcfAdsClient != null)
                                    _wcfAdsClient.Abort();
                                if (_instanceContext != null)
                                    _instanceContext.Abort();
                            }
                            catch (CommunicationException ec)
                            {
                                string msg = ec.Message;
                                if (ec.InnerException != null && ec.InnerException.Message != null)
                                    msg += " Inner:" + ec.InnerException.Message;

                                if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                              && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                    core.datamodel.Database.Root.Messages.LogException("WcfAdsClientChannel", "SendMessage", msg);
                            }
                            _waitOnDisconnection.Set();
                        }
                    }
                }
            }
            _syncSend.ThreadTerminated();
        }

        #endregion

        #region Receiving

        private Queue<WcfAdsMessage> _ReceiveQueue = new Queue<WcfAdsMessage>();
        internal Queue<WcfAdsMessage> ReceiveQueue
        {
            get { return _ReceiveQueue; }
            set { _ReceiveQueue = value; }
        }


        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueReceivedMessageFromService(WcfAdsMessage message)
        {
            if (!_syncReceive.NewItemsEnqueueable)
                return;

            lock (this._syncReceive._20010_QueueSyncLock)
            {
                this.ReceiveQueue.Enqueue(message);
            }

            // Signalisiere Thread, dass neue Message ansteht
            _syncReceive.NewItemEvent.Set();
        }

        void ReceiveMessage()
        {
            while (!_syncReceive.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncReceive.NewItemEvent.WaitOne();

                while (ReceiveQueue.Count > 0)
                {
                    WcfAdsMessage message = null;
                    //dequeue a message from the send queue
                    lock (this._syncReceive._20010_QueueSyncLock)
                    {
                        if (ReceiveQueue.Count <= 0)
                            break;
                        message = ReceiveQueue.Dequeue();
                    }
                    if (message != null)
                        ProcessMessage(message);
                }
            }
            _syncReceive.ThreadTerminated();
        }

        static ACRMemoryMetaObj[] metaData = new ACRMemoryMetaObj[10000];

        void ProcessMessage(WcfAdsMessage message)
        {
            if (_tcSession.IsPropertiesMapped && _tcSession.IsMemoryInitialized && message.Metadata == null 
                                                 && message.Memory == null && message.ConnectionState == null && message.Result == null)
            {
                if (message.ByteEvents != null)
                    _tcSession.RaiseEventByte(message.ByteEvents);

                if (message.UIntEvents != null)
                    _tcSession.RaiseEventUInt(message.UIntEvents);

                if (message.IntEvents != null)
                    _tcSession.RaiseEventInt(message.IntEvents);

                if (message.DIntEvents != null)
                    _tcSession.RaiseEventDInt(message.DIntEvents);

                if (message.UDIntEvents != null)
                    _tcSession.RaiseEventUDInt(message.UDIntEvents);

                if (message.RealEvents != null)
                    _tcSession.RaiseEventReal(message.RealEvents);

                if (message.LRealEvents != null)
                    _tcSession.RaiseEventLReal(message.LRealEvents);

                if (message.StringEvents != null)
                    _tcSession.RaiseEventString(message.StringEvents);

                if (message.TimeEvents != null)
                    _tcSession.RaiseEventTime(message.TimeEvents);

                if (message.DTEvents != null)
                    _tcSession.RaiseEventDT(message.DTEvents);
            }
            else if (message.Memory != null)
            {
                _tcSession.Memory = message.Memory;
            }
            else if (message.Metadata != null)
            {
                _tcSession.IsSessionConnected.ValueT = true;
                _tcSession.IsPlcConnected = true;
                _tcSession.Metadata = message.Metadata;
            }
            else if(message.ConnectionState != null && (ConnectionState)Enum.ToObject(typeof(ConnectionState), message.ConnectionState) == ConnectionState.DisconnectPLC)
            {
                _tcSession.IsPlcConnected = false;
            }
            else if(message.Result != null)
            {
                _tcSession.OnResultRead(message.Result);
            }
        }

        #endregion
    }
}