using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization;
using gip.core.datamodel;
using System.Transactions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WCFClientChannel'}de{'WCFClientChannel'}", Global.ACKinds.TACWCFClientChannel, Global.ACStorableTypes.NotStorable, true, false)]
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {"UserInstance", Global.ParamOption.Optional, typeof(VBUserInstance)}
        }
    )]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "WCFClientChannel", "en{'WCFClientChannel'}de{'WCFClientChannel'}", typeof(WCFClientChannel), "WCFClientChannel", "ConnectedSince", "ConnectedSince")]
    public class WCFClientChannel : ACComponent
    {
        #region private members
        bool _useHttpConnection = false;
        bool _useIPV6 = false;
        bool _useTextEncoding = false;
        bool _nameResolutionOn = false;
        bool _useNetTCPBinding = false;

        protected ACPDispatchClient _ACPDispatchToServer;
        private VBUserInstance _VBUserInstance = null;
        //private List<IACObjectWithBinding> _ACProjectsInstanceOnServerSide = new List<IACObjectWithBinding>();
        private List<string> _ACProjectsOnServerSide = new List<string>();

        InstanceContext _instanceContext = null;
        WCFClient _serviceOfPeer = null;
        EndpointAddress _endPoint = null;
        bool _ConnectionOn = false; // gibt an dass Verbindung aufrecht erhalten werden soll
        TimeSpan _reconnectTimeSpan = new TimeSpan(0, 0, 2);

        // Producer-Consumer for Sending
        SyncQueueEvents _syncSend;
        ACThread _sendThread;

        // Producer-Consumer for Receiving
        SyncQueueEvents _syncReceive;
        ACThread _receiveThread;

        // Producer-Consumer for PropertyValue-Dispatch
        SyncQueueEvents _syncDispatch;
        ACThread _ACPDispatchThread = null;

        // Producer-Consumer for ACPoint-Dispatch
        SyncQueueEvents _syncDispatchPoints;
        ACThread _ACPDispatchPointsThread = null;

        // Reconnection-Handling
        ACThread _ReconnectThread = null;
        protected ManualResetEvent _ReconnectShutdownEvent;
        ACThread _ConnectionThread = null;
        private EventWaitHandle _waitOnDisconnection = null;
        private EventWaitHandle _waitOnConnectionAtInit = null;

        private bool _resendSubscriptionInfo = false;
        #endregion

        #region c´tors

        public WCFClientChannel(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

            if (ParameterValue("UserInstance") != null)
            {
                _VBUserInstance = (VBUserInstance)ParameterValue("UserInstance");
            }

            _syncSend = new SyncQueueEvents();
            _syncReceive = new SyncQueueEvents();
            _syncDispatch = new SyncQueueEvents();
            _syncDispatchPoints = new SyncQueueEvents();
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            if (_VBUserInstance == null)
                return false;

            _ACProjectsOnServerSide = new List<string>();
            _ConnectedSince = DateTime.Now;
            _DisconnectedSince = new DateTime();
            _useHttpConnection = _VBUserInstance.ServiceAppEnbledHTTP;
            _useIPV6 = _VBUserInstance.UseIPV6;
            _useTextEncoding = _VBUserInstance.UseTextEncoding;
            if (_useHttpConnection && (_useTextEncoding == false))
                _useTextEncoding = true;
            _nameResolutionOn = _VBUserInstance.NameResolutionOn;

            _ACPDispatchToServer = new ACPDispatchClient();
            try
            {
                IEnumerable<VBUserACProject> projectsOnServerSide = from e in _VBUserInstance.VBUser.VBUserACProject_VBUser where e.IsServer select e;
                if (projectsOnServerSide != null)
                {
                    foreach (VBUserACProject acUserProject in projectsOnServerSide)
                    {
                        _ACProjectsOnServerSide.Add(acUserProject.ACProject.RootClass.ACIdentifier);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFClientChannel", "ACInit", msg);
            }

            IsConnected = true;

            Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)", ConnectionDetailXML);

            _sendThread = new ACThread(SendMessageToPeer);
            _sendThread.Name = "ACUrl:" + this.GetACUrl() + ";SendMessageToPeer();";
            _sendThread.Start();
            _receiveThread = new ACThread(ProcessMessageFromPeer);
            _receiveThread.Name = "ACUrl:" + this.GetACUrl() + ";ProcessMessageFromPeer();";
            _receiveThread.Start();
            _ACPDispatchThread = new ACThread(DispatchProperties);
            _ACPDispatchThread.Name = "ACUrl:" + this.GetACUrl() + ";DispatchProperties();";
            _ACPDispatchThread.Start();
            _ACPDispatchPointsThread = new ACThread(DispatchPoints);
            _ACPDispatchPointsThread.Name = "ACUrl:" + this.GetACUrl() + ";DispatchPoints();";
            _ACPDispatchPointsThread.Start();

            _waitOnConnectionAtInit = new EventWaitHandle(false, EventResetMode.AutoReset);
            InitializeConnection();
            // Warte auf ersten Verbindungsaufbau, damit erster Methodenaufruf nicht fehlschlägt,
            // indem auf das _serviceOfPeer-Objekt gewartet wird.
            _waitOnConnectionAtInit.WaitOne(2000);

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ACDeInit()", ConnectionDetailXML);

            if (_ACPDispatchThread != null)
            {
                _syncDispatch.TerminateThread();
                _ACPDispatchThread.Join();
                _ACPDispatchThread = null;
            }
            if (_ACPDispatchPointsThread != null)
            {
                _syncDispatchPoints.TerminateThread();
                _ACPDispatchPointsThread.Join();
                _ACPDispatchPointsThread = null;
            }
            _ACPDispatchToServer = null;

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
                DeleteServiceOfPeer();
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
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("WCFClientChannel", "ACDeInit", msg);
                    }
                }
                _ReconnectShutdownEvent = null;
                _ReconnectThread = null;
            }

            _endpointUri = null;
            _IsConnected = true;
            _CountReconnects = 0;
            if (_SendQueue != null)
                _SendQueue.Clear();
            if (_ReceiveQueue != null)
                _ReceiveQueue.Clear();
            _VBUserInstance = null;
            _instanceContext = null;
            _serviceOfPeer = null;
            _endPoint = null;
            _ConnectionOn = false;

            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region public members
        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        private Uri _endpointUri = null;
        [ACPropertyInfo(9999)]
        public Uri EndpointUri
        {
            get
            {
                if (_endpointUri != null)
                    return _endpointUri;

                if (_VBUserInstance == null)
                    return null;

                // Schema / protocol
                string scheme = "net.tcp";
                if (_useHttpConnection)
                    scheme = "http";

                // Authority
                string authority = _VBUserInstance.ServerIPV4;
                if (_useIPV6)
                    authority = _VBUserInstance.ServerIPV6;
                else if (_nameResolutionOn)
                    authority = _VBUserInstance.Hostname;

                if ((Root as ACRoot).WCFOff)
                    authority = "localhost";

                if (_useHttpConnection)
                    authority += ":" + _VBUserInstance.ServicePortHTTP;
                else
                    authority += ":" + _VBUserInstance.ServicePortTCP;

                // Address
                _endpointUri = new Uri(String.Format("{0}://{1}/", scheme, authority));
                return _endpointUri;
            }
        }

        [ACPropertyInfo(5)]
        public string EndpointIP
        {
            get
            {
                if (EndpointUri == null)
                    return "";
                return EndpointUri.OriginalString;
            }
        }

        public WCFClientManager WCFClientManager
        {
            get
            {
                return this.ParentACComponent as WCFClientManager;
            }
        }

        private bool _IsConnected = true;
        [ACPropertyInfo(1)]
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }

            set
            {
                bool DisconnectedEvent = false;
                if ((_IsConnected == true) && (value == false))
                    DisconnectedEvent = true;
                _IsConnected = value;
                OnPropertyChanged("IsConnected");
                if (WCFClientManager != null)
                    WCFClientManager.UpdateStatistic();
                if (DisconnectedEvent)
                    InformObjectsOnDisconnection();
            }
        }

        private int _CountReconnects = 0;
        [ACPropertyInfo(2)]
        public int CountReconnects
        {
            get
            {
                return _CountReconnects;
            }

            set
            {
                _CountReconnects = value;
                OnPropertyChanged("CountReconnects");
            }
        }

        private DateTime _ConnectedSince = DateTime.Now;
        [ACPropertyInfo(3)]
        public DateTime ConnectedSince
        {
            get
            {
                return _ConnectedSince;
            }

            set
            {
                _ConnectedSince = value;
                OnPropertyChanged("ConnectedSince");
            }
        }

        private DateTime _DisconnectedSince = new DateTime();
        [ACPropertyInfo(4)]
        public DateTime DisconnectedSince
        {
            get
            {
                return _DisconnectedSince;
            }

            set
            {
                _DisconnectedSince = value;
                OnPropertyChanged("DisconnectedSince");
            }
        }

        [ACPropertyInfo(9999)]
        public List<string> ACProjectsOnServerSide
        {
            get
            {
                return _ACProjectsOnServerSide;
            }
        }

        [ACPropertyInfo(9999)]
        public string ConnectionDetailXML
        {
            get
            {
                string connected = "";
                string connectedTime = "";
                if (IsConnected)
                {
                    connected = Root.Environment.TranslateText(this, "Connected since");
                    connectedTime = ConnectedSince.ToString();
                }
                else
                {
                    connected = Root.Environment.TranslateText(this, "Disconnected since");
                    connectedTime = DisconnectedSince.ToString();
                }

                string xaml = String.Format("<WCFClientChannel>" +
                    "<EndpointUri>{0}</EndpointUri>" +
                    "<ConnectedTime>{1}</ConnectedTime>" +
                    "<CountReconnects>{2}</CountReconnects>" +
                "</WCFClientChannel>",
                EndpointUri.OriginalString,
                connectedTime,
                CountReconnects
                );

                return xaml;
            }
        }

        [ACPropertyInfo(9999)]
        public VBUserInstance VBUserInstance
        {
            get
            {
                return _VBUserInstance;
            }
        }
        #endregion

        #region Connect/Disconnect

        #region New Connection

        /// <summary>
        /// Instanziert einen neuen Thread, der den WCF-Client-Channel erstellt
        /// </summary>
        [ACMethodInfo("Connection", "en{'Connection establishment'}de{'Verbindungsaufbau'}", 9999)]
        private void InitializeConnection()
        {
            if (_ConnectionThread != null)
                return;

            _ConnectionOn = true;

            if (_serviceOfPeer == null)
            {
                if (EndpointUri == null)
                    return;
                _ConnectionThread = new ACThread(NewConnectionInstance);
                _ConnectionThread.Name = "ACUrl:" + this.GetACUrl() + ";NewConnectionInstance();";
                _ConnectionThread.Start();
            }
        }


        /// <summary>
        /// Erstellt einen neuen WCF-Client-Channel
        /// </summary>
        private void NewConnectionInstance()
        {
            _endPoint = new EndpointAddress(EndpointUri);

            _instanceContext = new InstanceContext(new WCFClientServiceCallback(this));

            // Service Endpoint
            if (_useHttpConnection)
            {
                /*
                WSDualHttpBinding wsDualHttpBinding = new WSDualHttpBinding();
                wsDualHttpBinding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                _serviceOfPeer = new WCFClient(_instanceContext, wsDualHttpBinding, _endPoint);
                */
            }
            else
            {
                if (_useNetTCPBinding)
                {
                    NetTcpBinding netTcpBinding = new NetTcpBinding();
                    if (netTcpBinding.ReaderQuotas != null)
                        netTcpBinding.ReaderQuotas.MaxStringContentLength = WCFServiceManager.MaxStringLength;
                    netTcpBinding.MaxBufferSize = WCFServiceManager.MaxBufferSize;
                    netTcpBinding.MaxReceivedMessageSize = WCFServiceManager.MaxBufferSize;
                    //netTcpBinding.ConnectionBufferSize = WCFServiceManager.MaxBufferSize / 8;
                    netTcpBinding.MaxBufferPoolSize = WCFServiceManager.MaxBufferSize;
                    netTcpBinding.ReceiveTimeout = WCFServiceManager.ReceiveTimeout;

                    // When to use reliable session
                    // http://msdn.microsoft.com/en-us/library/ms733136(v=vs.110).aspx
                    //if (netTcpBinding.ReliableSession != null)
                    //    netTcpBinding.ReliableSession.Enabled = true;

                    _serviceOfPeer = new WCFClient(_instanceContext, netTcpBinding, _endPoint);
                }
                else
                {
                    CustomBinding netTcpBinding = new CustomBinding();
                    TcpTransportBindingElement tcpTransport = new TcpTransportBindingElement();

                    if (_useTextEncoding)
                    {
                        TextMessageEncodingBindingElement textMessageEncoding = new TextMessageEncodingBindingElement();
                        netTcpBinding.Elements.Add(textMessageEncoding);
                        if (textMessageEncoding.ReaderQuotas != null)
                            textMessageEncoding.ReaderQuotas.MaxStringContentLength = WCFServiceManager.MaxStringLength;
                    }
                    else
                    {
                        BinaryMessageEncodingBindingElement binaryMessageEncoding = new BinaryMessageEncodingBindingElement();
                        netTcpBinding.Elements.Add(binaryMessageEncoding);
                        if (binaryMessageEncoding.ReaderQuotas != null)
                            binaryMessageEncoding.ReaderQuotas.MaxStringContentLength = WCFServiceManager.MaxStringLength;
                    }

                    tcpTransport.MaxBufferSize = WCFServiceManager.MaxBufferSize;
                    tcpTransport.MaxReceivedMessageSize = WCFServiceManager.MaxBufferSize;
                    tcpTransport.ConnectionBufferSize = WCFServiceManager.MaxBufferSize / 8;
                    tcpTransport.MaxBufferPoolSize = WCFServiceManager.MaxBufferSize;
                    netTcpBinding.Elements.Add(tcpTransport);
                    netTcpBinding.ReceiveTimeout = WCFServiceManager.ReceiveTimeout;

                    _serviceOfPeer = new WCFClient(_instanceContext, netTcpBinding, _endPoint);
                }
            }

            foreach (OperationDescription op in _serviceOfPeer.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph;
                    dataContractBehavior.DataContractResolver = ACConvert.MyDataContractResolver;
                }
            }

            AddServiceOfPeer();
            SendConnectMessageToOpenChannel();
            // Signalisiere, dass Instanziierung ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic) forgesetzt werden kann um Methodenaufrufe durchzuführen
            _waitOnConnectionAtInit.Set();
        }

        #endregion

        #region Event-Handler für Opened, Faulted, Closing

        /// <summary>
        /// Event-Handler für Event _serviceOfPeer.InnerDuplexChannel.Opened
        /// Wird aufgerufen nachdem der erste Webservice-Aufruf auf Serverseite erfolgreich erfolgt ist
        /// durch WCFClient.Invoke(ACMessage acMessage)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Channel_Opened(object sender, EventArgs e)
        {
            ConnectedSince = DateTime.Now;
            IsConnected = true;
            if (_ReconnectThread == null)
            {
                if (_resendSubscriptionInfo == true)
                    SendSubscriptionInfoToServer();
                Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ConnectionIsEstablished()", "First Connect");
                return;
            }

            Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ConnectionIsEstablished()", "Reconnected");

            ResetReconnectTimeout();
            if (_ReconnectShutdownEvent != null && _ReconnectShutdownEvent.SafeWaitHandle != null && !_ReconnectShutdownEvent.SafeWaitHandle.IsClosed)
                _ReconnectShutdownEvent.Set();
            if (!_ReconnectThread.Join(100))
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

                    Messages.LogException("WCFClientChannel", "Channel_Opened", msg);
                }
            }
            _ReconnectShutdownEvent = null;
            _ReconnectThread = null;
            CountReconnects++;
            RebuildSubscription();
        }


        /// <summary>
        /// Event-Handler für Event _serviceOfPeer.InnerDuplexChannel.Faulted
        /// Wird aufgerufen nachdem der erste Webservice-Aufruf auf Serverseite nicht erfolgreich war
        /// sprich Channel nicht aufgebaut werden konnte
        /// durch WCFClient.Invoke(ACMessage acMessage)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Channel_Faulted(object sender, EventArgs e)
        {
            if (_ConnectionOn == true)
            {
                if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
                {
                    //System.Runtime.Remoting.Proxies.RealProxy realProxy = _serviceOfPeer.InnerDuplexChannel as System.Runtime.Remoting.Proxies.RealProxy;
                    //if (realProxy != null)
                    //{
                    //    realProxy.Target
                    //}
                    //_serviceOfPeer.InnerDuplexChannel.Extensions
                    Messages.LogFailure(this.GetACUrl(), "WCFClientChannel.Channel_Faulted()", "");
                    StartReconnect();
                }
            }
        }


        /// <summary>
        /// Event-Handler für Event _serviceOfPeer.InnerDuplexChannel.Closing
        /// Wird aufgerufen nachdem der erste Webservice-Aufruf auf Serverseite nicht erfolgreich war
        /// und durch _serviceOfPeer.Abort() die Verbindung geschlossen wurde
        /// durch WCFClient.Invoke(ACMessage acMessage)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Channel_Closing(object sender, EventArgs e)
        {
            if (_ConnectionOn == true)
            {
                Messages.LogFailure(this.GetACUrl(), "WCFClientChannel.Channel_Closing()", "");
                StartReconnect();
            }
        }

        #endregion

        #region Reconnection-Handling

        /// <summary>
        /// Instanziert einen neuen Thread, der für den Verbindungsneuaufbau zuständig ist
        /// StartReconnect wird durch den "Faulted-Event" oder "Closing-Event" aktiviert
        /// Der Reconnect-thread wird durch den "Opened-Event" wieder terminiert und der Connection-Thread lebt
        /// </summary>
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
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("WCFClientChannel", "StartReconnect", msg);
                    }
                }
                _ConnectionThread = null;
                DeleteServiceOfPeer();
            }

            DisconnectedSince = DateTime.Now;
            IsConnected = false;
            _ReconnectShutdownEvent = new ManualResetEvent(false);
            _ReconnectThread = new ACThread(Reconnect);
            _ReconnectThread.Name = "ACUrl:" + this.GetACUrl() + ";Reconnect();";
            Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.StartReconnect()", "ReconnectThread created");
            _ReconnectThread.Start();
        }

        /// <summary>
        /// Wird durch den Start des ReconnectThreads aktiviert
        /// Die Methode versucht ständig eine Message an den Server zu senden
        /// um den Verbindungsaufbau wiederherzustellen durch WCFClient.Invoke()
        /// Der Zeitabstand wird dabei nach nicht erfolreichem Aufbau wieder weiter verlängert
        /// </summary>
        private void Reconnect()
        {
            while (!_ReconnectShutdownEvent.WaitOne(_reconnectTimeSpan,false))
            {
                _ReconnectThread.StartReportingExeTime();
                //Thread.Sleep(_reconnectTimeSpan);
                IncreaseReconnectTimeout();

                if (_ConnectionThread == null)
                {
                    InitializeConnection();
                }
                else if (_serviceOfPeer != null)
                {
                    if (_serviceOfPeer.State == CommunicationState.Faulted)
                    {
                        try
                        {
                            _serviceOfPeer.Abort();
                            _instanceContext.Abort();
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Messages.LogException("WCFClientChannel", "Reconnect", msg);
                        }
                    }

                    if (_serviceOfPeer.State == CommunicationState.Closed)
                    {
                        if (_ConnectionThread != null)
                        {
                            if (!_ConnectionThread.Join(1000))
                            {
                                try
                                {
                                    _ConnectionThread.Abort();
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    Messages.LogException("WCFClientChannel", "Reconnect(10)", msg);
                                }
                            }
                            _ConnectionThread = null;
                            DeleteServiceOfPeer();
                        }
                    }
                    else
                    {
                        SendConnectMessageToOpenChannel();
                    }
                }
                _ReconnectThread.StopReportingExeTime();
            }
        }

        #endregion

        #region Disconnect

        /// <summary>
        /// Verbindungstrennung durch ACDeInit
        /// </summary>
        public void Disconnect()
        {
            _ConnectionOn = false;

            if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
            {
                if (_serviceOfPeer.State != CommunicationState.Closed)
                {
                    _waitOnDisconnection = new EventWaitHandle(false, EventResetMode.AutoReset);
                    SendDisconnectMessageToCloseChannel();
                    _waitOnDisconnection.WaitOne();
                }
            }
        }

        public void Connect()
        {
            _ConnectionOn = true;
        }

        #endregion

        #region Helper-Methods

        private void ResetReconnectTimeout()
        {
            _reconnectTimeSpan = new TimeSpan(0, 0, 2);
        }


        private void IncreaseReconnectTimeout()
        {
            if (_reconnectTimeSpan.TotalSeconds <= 58)
                _reconnectTimeSpan = _reconnectTimeSpan.Add(new TimeSpan(0, 0, 2));
        }


        private void SendConnectMessageToOpenChannel()
        {
            ACConnect message = new ACConnect();
            message.UserName = this.Root.Environment.User.VBUserName;

            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);
        }


        private void SendDisconnectMessageToCloseChannel()
        {
            ACDisconnect message = new ACDisconnect();
            message.UserName = this.Root.Environment.User.VBUserName;

            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);
        }


        private void DeleteServiceOfPeer()
        {
            if (_serviceOfPeer == null)
                return;
            if (_serviceOfPeer.InnerChannel != null)
            {
                //_serviceOfPeer.InnerDuplexChannel.Opening -= Channel_Opening;
                _serviceOfPeer.InnerChannel.Opened -= Channel_Opened;
                _serviceOfPeer.InnerChannel.Faulted -= Channel_Faulted;
                _serviceOfPeer.InnerChannel.Closing -= Channel_Closing;
                //_serviceOfPeer.InnerDuplexChannel.Closed -= Channel_Closed;
            }
            _serviceOfPeer = null;
            _instanceContext = null;
        }


        private void AddServiceOfPeer()
        {
            if (_serviceOfPeer == null)
                return;
            if (_serviceOfPeer.InnerChannel != null)
            {
                //_serviceOfPeer.InnerDuplexChannel.Opening += Channel_Opening;
                _serviceOfPeer.InnerChannel.Opened += Channel_Opened;
                _serviceOfPeer.InnerChannel.Faulted += Channel_Faulted;
                _serviceOfPeer.InnerChannel.Closing += Channel_Closing;
                //_serviceOfPeer.InnerDuplexChannel.Closed += Channel_Closed;
            }
        }

        #endregion

        #endregion

        #region public Methods

        /// <summary>
        /// Methode dieser Verbindungskanal für das entsprechend Projekt zuständig ist
        /// </summary>
        /// <param name="ProjectACIdentifier"></param>
        /// <returns></returns>
        public bool IsChannelForProject(string ProjectACIdentifier)
        {
            try
            {
                if (_ACProjectsOnServerSide.Where(c => c == ProjectACIdentifier).Any())
                    return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFClientChannel", "IsChannelForProject", msg);
            }
            return false;
        }


        /// <summary>Sendet eine Clientseitige Nachricht an den Server</summary>
        /// <param name="message"></param>
        /// <exception cref="gip.core.autocomponent.ACWCFException">Thrown when disconnected</exception>
        public void SendACMessageToServer(WCFMessage message)
        {
            // Falls Verbindung zu Server getrennt war und der gerade versucht wird die Verbindung neu aufzubauen, verwerfe Request
            if ((_ReconnectThread != null) || (_serviceOfPeer == null))
                throw new ACWCFException(Root.Environment.TranslateMessage(this, "Error00036"), ACWCFException.WCFErrorCode.Disconnected);
            else if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
            {
                if ((_serviceOfPeer.State != CommunicationState.Opened) && (_serviceOfPeer.State != CommunicationState.Created))
                    throw new ACWCFException(Root.Environment.TranslateMessage(this, "Error00036"), ACWCFException.WCFErrorCode.Disconnected);
            }
            Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendACMessageToServer()", message.ACUrl);
            EnqeueMessageForPeer(message);
        }


        /// <summary>Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side</summary>
        /// <param name="eventArgs"></param>
        /// <param name="ACProject"></param>
        public bool SendPropertyValueToServer(IACPropertyNetValueEvent eventArgs, IACComponent ACProject)
        {
            // Falls Verbindung zu Server getrennt war und der gerade versucht wird ddie Verbindung neu aufzubauen, verwerfe Request
            if ((_ReconnectThread != null) || (_serviceOfPeer == null))
                return false;
            else if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
            {
                if ((_serviceOfPeer.State != CommunicationState.Opened) && (_serviceOfPeer.State != CommunicationState.Created))
                    return false;
            }

            if (_ACPDispatchToServer == null)
                return false;
            if (!_syncDispatch.NewItemsEnqueueable)
                return false;

            bool enqueued = _ACPDispatchToServer.Enqueue(ACProject, eventArgs);

            // Signalisiere Thread, dass neues Event ansteht
            if (enqueued)
                _syncDispatch.NewItemEvent.Set();

            return enqueued;
        }


        /// <summary>Method subscribes an new generated ACObject for retrieving ValueEvents from the Server</summary>
        /// <param name="acComponentProject"></param>
        /// <param name="ChildNode"></param>
        public void SubscribeACObjectOnServer(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if (_ACPDispatchToServer == null)
                return;
            _ACPDispatchToServer.Subscribe(acComponentProject, ChildNode);
        }

        /// <summary>Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server</summary>
        /// <param name="acComponentProject"></param>
        /// <param name="ChildNode"></param>
        public void MarkACObjectOnChangedPoint(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if (_ACPDispatchToServer == null)
                return;
            bool enqueued = _ACPDispatchToServer.MarkACObjectOnChangedPoint(acComponentProject, ChildNode);
            if (!_ACPDispatchToServer.InACObjectInitPhase && enqueued)
                _syncDispatchPoints.NewItemEvent.Set();
        }


        /// <summary>Method unsubscribes an unloaded ACObject</summary>
        /// <param name="acComponentProject"></param>
        /// <param name="ChildNode"></param>
        public void UnSubscribeACObjectOnServer(IACComponent acComponentProject, IACComponent ChildNode)
        {
            if (_ACPDispatchToServer == null)
                return;
            _ACPDispatchToServer.UnSubscribe(acComponentProject, ChildNode);
        }

        public void EnqueueSendSubscriptionInfoToServer()
        {
            // Falls Verbindung zu Server getrennt war und der gerade versucht wird ddie Verbindung neu aufzubauen, verwerfe Request
            if ((_ReconnectThread != null) || (_serviceOfPeer == null))
                return;
            else if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
            {
                if ((_serviceOfPeer.State != CommunicationState.Opened) && (_serviceOfPeer.State != CommunicationState.Created))
                    return;
            }
            if (_ACPDispatchToServer == null)
                return;
            if (!_syncDispatchPoints.NewItemsEnqueueable)
                return;

            int countSubscr = 0;
            int countUnSubscr = 0;
            _ACPDispatchToServer.CountOfSubscriptions(out countSubscr, out countUnSubscr);

            if (countSubscr <= 0 && countUnSubscr <= 0)
                return;

            // Signalisiere Thread, dass neues Event ansteht
            _syncDispatchPoints.NewItemEvent.Set();
        }

        /// <summary>
        /// Activates Sending of Subscription to server.
        /// Method will be called, when a common set of Objects are generated
        /// </summary>
        public void SendSubscriptionInfoToServer()
        {
            if (_ACPDispatchToServer == null)
                return;

            int countSubscr = 0;
            int countUnSubscr = 0;
            _ACPDispatchToServer.CountOfSubscriptions(out countSubscr, out countUnSubscr);

            if (countSubscr > 0 || countUnSubscr > 0)
            {
                // Falls Verbindung zu Server getrennt war und der gerade versucht wird ddie Verbindung neu aufzubauen, verwerfe Request
                if ((_ReconnectThread != null) || (_serviceOfPeer == null))
                {
                    _resendSubscriptionInfo = true;
                    return;
                }
                else if ((_serviceOfPeer != null) && (_serviceOfPeer.InnerChannel != null))
                {
                    if ((_serviceOfPeer.State != CommunicationState.Opened) && (_serviceOfPeer.State != CommunicationState.Created))
                    {
                        _resendSubscriptionInfo = true;
                        return;
                    }
                }

                Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendSubscriptionInfoToServer()", String.Format("SubscribeRequest: {0}, UnSubscribeRequest: {1}", countSubscr, countUnSubscr));

                _resendSubscriptionInfo = false;

                ACSubscriptionMessage message = new ACSubscriptionMessage();
                ACPSubscrDispClient ACPSubscribe = null;
                ACPSubscrDispClient ACPUnSubscribe = null;
                if (_ACPDispatchToServer.GetCopyAndEmpty(ref ACPSubscribe, ref ACPUnSubscribe))
                {
                    message.ACPSubscribe = ACPSubscribe;
                    message.ACPUnSubscribe = ACPUnSubscribe;
                    WCFMessage acMessage = NewACMessage("", new Object[] { message });
                    EnqeueMessageForPeer(acMessage);
                }
            }
        }

        public void BroadcastShutdownAllClients()
        {
            ACDisconnect message = new ACDisconnect();
            message.UserName = this.Root.Environment.User.VBUserName;
            message.ShutdownClient = true;

            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);
        }
        #endregion

        #region internal Dispatching of Subscription and Property-Values

        /// <summary>
        /// Die Subscription wird bei Reconnect neu aufgebaut, 
        /// damit der Server nach dem Hochfahren weiß, welche Properties an den Client versendet werden müssen
        /// </summary>
        private void RebuildSubscription()
        {
            if (_ACPDispatchToServer == null)
                return;
            _ACPDispatchToServer.EmptySubscriptionRequests();
            // Durchlaufe Models + WFManager
            // Ermittle Rekursiv alle Proxy-Objekte und Fülle Subscription
            foreach (ACComponent acComponentProject in this.Root.ACComponentChilds)
            {
                if (acComponentProject == null)
                    continue;
                if (IsChannelForProject(acComponentProject.ACIdentifier))
                    acComponentProject.ReSubscribe();
            }
            SendSubscriptionInfoToServer();
        }

        private void InformObjectsOnDisconnection()
        {
            foreach (ACComponent acComponentProject in this.Root.ACComponentChilds)
            {
                if (acComponentProject == null)
                    continue;
                if (IsChannelForProject(acComponentProject.ACIdentifier))
                    acComponentProject.InformObjectsOnDisconnect();
            }
        }

        /// <summary>
        /// Producer-/Consumer für Versenden der angestauten Property-Values
        /// </summary>
        private void DispatchProperties()
        {
            while (!_syncDispatch.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncDispatch.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(100);

                if ((_ACPDispatchToServer == null) || (Root == null))
                    continue;
                _ACPDispatchThread.StartReportingExeTime();
                ACPropertyValueMessage message = new ACPropertyValueMessage();
                // Alle Events, da pro ObserverClient nur ein Modell im Dispatcher existiert
                message.PropertyValues = _ACPDispatchToServer.GetAllEvents(true);
                if (message.PropertyValues.Count > 0)
                {
                    WCFMessage acMessage = NewACMessage("", new Object[] { message });
                    EnqeueMessageForPeer(acMessage);
                }
                _ACPDispatchThread.StopReportingExeTime();
            }
            _syncDispatch.ThreadTerminated();
        }

        /// <summary>
        /// Producer-/Consumer für Versenden der angestauten Points
        /// </summary>
        private void DispatchPoints()
        {
            while (!_syncDispatchPoints.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncDispatchPoints.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(100);

                if ((_ACPDispatchToServer == null) || (Root == null) || _ACPDispatchToServer.InACObjectInitPhase)
                    continue;

                _ACPDispatchPointsThread.StartReportingExeTime();
                SendSubscriptionInfoToServer();
                _ACPDispatchPointsThread.StopReportingExeTime();
            }
            _syncDispatchPoints.ThreadTerminated();
        }

        #endregion

        #region Producer-Consumer for Sending

        private Queue<WCFMessage> _SendQueue = new Queue<WCFMessage>();
        internal Queue<WCFMessage> SendQueue
        {
            get { return _SendQueue; }
            set { _SendQueue = value; }
        }

        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueMessageForPeer(WCFMessage message)
        {
            if (!_syncSend.NewItemsEnqueueable)
                return;

            using (ACMonitor.Lock(this._syncSend._20010_QueueSyncLock))
                this.SendQueue.Enqueue(message);

            // Signalisiere Thread, dass neue Message ansteht
            _syncSend.NewItemEvent.Set();
        }

        /// <summary>
        /// Consumer-Method
        /// </summary>
        private void SendMessageToPeer()
        {
            while (!_syncSend.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncSend.NewItemEvent.WaitOne();

                _sendThread.StartReportingExeTime();
                while (SendQueue.Count > 0)
                {
                    WCFMessage acMessage = null;
                    //dequeue a message from the send queue

                    using (ACMonitor.Lock(this._syncSend._20010_QueueSyncLock))
                    {
                        if (SendQueue.Count <= 0)
                            break;
                        acMessage = SendQueue.Dequeue();
                    }
                    if (acMessage != null)
                    {
                        if (_serviceOfPeer != null)
                        {
                            try
                            {
                                _serviceOfPeer.Invoke(acMessage);
                                //ConnectionIsEstablished();
                            }
                            // Sonst Verbindung gestört, Reconnect wird durch Channel_Faulted-Event aktiviert
                            catch (CommunicationException e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("WCFClientChannel", "SendMessageToPeer", msg);
                            }

                            if (acMessage.MethodInvokeRequestID > 0)
                            {
                                Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendMessageToPeer()", String.Format("ACMethodInvocation: {0} {1}", acMessage.ACUrl, acMessage.MethodInvokeRequestID));
                            }

                            if (acMessage.ACParameter != null)
                            {
                                if ((acMessage.ACParameter.Any()) && (acMessage.ACParameter[0] != null))
                                {
                                    if (acMessage.ACParameter[0] is ACSubscriptionMessage)
                                    {
                                        Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendMessageToPeer()", "ACSubscriptionMessage sended");
                                        (acMessage.ACParameter[0] as ACSubscriptionMessage).Detach();
                                    }
                                    else if (acMessage.ACParameter[0] is ACPropertyValueMessage)
                                    {
#if DEBUG
                                        Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendMessageToPeer()", "ACPropertyValueMessage sended");
#endif
                                    }
                                    else if (acMessage.ACParameter[0] is ACDisconnect)
                                    {
                                        ACDisconnect acDisconnect = acMessage.ACParameter[0] as ACDisconnect;
                                        Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.SendMessageToPeer()", "ACDisconnect sended");
                                        if (acDisconnect.ShutdownClient)
                                        {
                                        }
                                        else
                                        {
                                            try
                                            {
                                                if (_serviceOfPeer != null)
                                                    _serviceOfPeer.Abort();
                                                if (_instanceContext != null)
                                                    _instanceContext.Abort();
                                            }
                                            catch (CommunicationException e)
                                            {
                                                string msg = e.Message;
                                                if (e.InnerException != null && e.InnerException.Message != null)
                                                    msg += " Inner:" + e.InnerException.Message;

                                                Messages.LogException("WCFClientChannel", "SendMessageToPeer(10)", msg);
                                            }
                                            _waitOnDisconnection.Set();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _sendThread.StopReportingExeTime();
            }
            _syncSend.ThreadTerminated();
        }
#endregion

#region Producer-Consumer for Receiving

        private Queue<WCFMessage> _ReceiveQueue = new Queue<WCFMessage>();
        internal Queue<WCFMessage> ReceiveQueue
        {
            get { return _ReceiveQueue; }
            set { _ReceiveQueue = value; }
        }


        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueReceivedMessageFromPeer(WCFMessage message)
        {
            if (!_syncReceive.NewItemsEnqueueable)
                return;

            using (ACMonitor.Lock(this._syncReceive._20010_QueueSyncLock))
                this.ReceiveQueue.Enqueue(message);

            // Signalisiere Thread, dass neue Message ansteht
            _syncReceive.NewItemEvent.Set();
        }


        /// <summary>
        /// Consumer-Method
        /// </summary>
        private void ProcessMessageFromPeer()
        {
            while (!_syncReceive.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncReceive.NewItemEvent.WaitOne();

                _receiveThread.StartReportingExeTime();
                while (ReceiveQueue.Count > 0)
                {
                    WCFMessage acMessage = null;
                    //dequeue a message from the send queue

                    using (ACMonitor.Lock(this._syncReceive._20010_QueueSyncLock))
                    {
                        if (ReceiveQueue.Count <= 0)
                            break;
                        acMessage = ReceiveQueue.Dequeue();
                    }
                    if (acMessage != null)
                        ProcessACMessage(acMessage);
                }
                _receiveThread.StopReportingExeTime();
            }
            _syncReceive.ThreadTerminated();
        }


        /// <summary>
        /// Analyzing and Processing Recieved Messages
        /// </summary>
        /// <param name="acMessage"></param>
        private void ProcessACMessage(WCFMessage acMessage)
        {
            if (acMessage == null)
                return;
            object acObj = null;

            if (acMessage.ACParameter[0] is ACConnect)
            {
                Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ProcessACMessage()", "ACConnect");
                // Alles ok!
                return;
            }
            else if (acMessage.ACParameter[0] is ACDisconnect)
            {
                Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ProcessACMessage()", "ACDisconnect");
                ACDisconnect disconnect = acMessage.ACParameter[0] as ACDisconnect;
                if (disconnect.ShutdownClient)
                {
                    if (Root.RootPageWPF != null)
                        Root.RootPageWPF.CloseWindowFromThread();
                }
                return;
            }
            else if (acMessage.ACParameter[0] is ACSubscriptionServiceMessage)
            {
                ACSubscriptionServiceMessage message = acMessage.ACParameter[0] as ACSubscriptionServiceMessage;

                //Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ProcessACMessage()", String.Format("SubscribeRequest: {0}", message.ACPSubscribe.ProjectSubscriptionList.Count()));

                if (message.ACPSubscribe.ProjectSubscriptionList.Count > 0)
                {
                    // Aktualisiere bzw. Benachrichtige Points der Proxy-Objekte
                    ThreadPool.QueueUserWorkItem((object state) =>
                    message.ACPSubscribe.UpdatePointsOnProxyObjects());
                }
                return;
            }
            else if (acMessage.ACParameter[0] is ACPropertyValueMessage)
            {
#if DEBUG
                //Messages.LogDebug(this.GetACUrl(), "WCFClientChannel.ProcessACMessage()", "ACPropertyValueMessage");
#endif
                ACPropertyValueMessage message = acMessage.ACParameter[0] as ACPropertyValueMessage;
                string prevACUrl = "";
                foreach (IACPropertyNetValueEvent propertyValueEvent in message.PropertyValues)
                {
                    if (String.IsNullOrEmpty(propertyValueEvent.ACUrl))
                        (propertyValueEvent as IACPropertyNetValueEventExt).SetACUrl(prevACUrl);
                    else
                        prevACUrl = propertyValueEvent.ACUrl;

                    bool isRoot = IsRootAddressed(propertyValueEvent.ACUrl);
                    // Falls Message nicht an Diagnose (Root-Proxy)
                    if (!isRoot)
                    {
                        acObj = this.Root.ACUrlCommand("?" + propertyValueEvent.ACUrl);
                        if (acObj != null)
                        {
                            if (acObj is ACComponent)
                            {
                                IACPropertyNetBase acProperty = (acObj as ACComponent).GetPropertyNet(propertyValueEvent.ACIdentifier);
                                if (acProperty != null)
                                    acProperty.OnValueEventReceivedRemote(propertyValueEvent);
                            }
                        }
                    }
                }

                return;
            }
            else if (acMessage.ACParameter[0] is ACMethodInvocationResult)
            {
                bool isRoot = IsRootAddressed(acMessage.ACUrl);
                if (!isRoot)
                {
                    string acURL = acMessage.ACUrlRequester;
                    if (String.IsNullOrEmpty(acURL))
                    {
                        acURL = acMessage.ACUrl;
                        int posMethod = acMessage.ACUrl.IndexOf("!");
                        if (posMethod > 0)
                            acURL = acMessage.ACUrl.Substring(0, posMethod);
                        else
                        {
                            int posProperty = acMessage.ACUrl.LastIndexOf('\\');
                            if (posProperty > 0)
                                acURL = acMessage.ACUrl.Substring(0, posProperty);
                        }
                    }
                    acObj = this.Root.ACUrlCommand(("?" + acURL));
                    if (acObj != null)
                    {
                        if (acObj is IACObjectRMI)
                        {
                            ACMethodInvocationResult message = acMessage.ACParameter[0] as ACMethodInvocationResult;
                            IACObjectRMI acObjectRMI = (IACObjectRMI)acObj;
                            acObjectRMI.OnACMethodExecuted(acMessage.MethodInvokeRequestID, message.MethodResult);
                        }
                    }
                }
                return;
            }

            if (string.IsNullOrEmpty(this.ACIdentifier))
                return;

            acObj = this.Root.ACUrlCommand("?" + acMessage.ACUrl, acMessage.ACParameter);
        }

        public WCFMessage NewACMessage(string acUrl, Object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
            {
                return WCFMessage.NewACMessage(GetACUrl() + "\\" + acUrl, acParameter);
            }
            else
            {
                return WCFMessage.NewACMessage(GetACUrl(), acParameter);
            }
        }
#endregion

#region DataShow
        static public string KeyACIdentifier
        {
            get { return "ConnectedSince"; }
        }
#endregion

        public bool IsRootAddressed(string acUrl)
        {
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            if (acUrlHelper.UrlKey == ACUrlHelper.UrlKeys.Root)
            {
                if (acUrlHelper.NextACUrl == "")
                    return true;
            }
            return false;
        }

    }
}
