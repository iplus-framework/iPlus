using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Timers;
using System.Net;
using System.Threading;
using gip.core.datamodel;
using Microsoft.AspNetCore.Hosting;
using CoreWCF.IdentityModel.Protocols.WSTrust;
using Microsoft.AspNetCore;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF;
using CoreWCF.Channels;
using System.Collections;
using System.Reflection;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WCFServiceManager'}de{'WCFServiceManager'}", Global.ACKinds.TACWCFServiceManager, Global.ACStorableTypes.NotStorable, false, false)]
    public class WCFServiceManager : ACComponent
    {
        #region private members
        public const int MaxBufferSize = 4194304; // 2^22 byte
        public const int MaxItemsInObjectGraph = Int32.MaxValue;
        public const int MaxStringLength = 1048576; // 2^20 byte

        public const int HTTPS_PORT = 8443;

        bool _useHttpConnection = false;
        bool _useIPV6 = false;
        bool _useTextEncoding = false;
        bool _nameResolutionOn = false;
        bool _useNetTCPBinding = false;

        SyncQueueEvents _syncDispatch;
        ACThread _ACPDispatchThread = null;

        SyncQueueEvents _syncDispatchPoints;
        ACThread _ACPDispatchPointsThread = null;

        ACThread _ACHostStartThread = null;

        #endregion

        #region c´tors

        public WCFServiceManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            if (parameter != null)
            {
                int i = 0;
                foreach (Object param in parameter)
                {
                    switch (i)
                    {
                        case 0:
                            if (param is bool)
                                _useHttpConnection = (bool)param;
                            break;
                        case 1:
                            if (param is bool)
                                _useIPV6 = (bool)param;
                            break;
                        case 2:
                            if (param is bool)
                                _useTextEncoding = (bool)param;
                            break;
                    }
                    i++;
                }
            }
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);

            _useHttpConnection = this.Root.Environment.UserInstance.ServiceAppEnbledHTTP;
            _useIPV6 = this.Root.Environment.UserInstance.UseIPV6;
            _useTextEncoding = this.Root.Environment.UserInstance.UseTextEncoding;
            if (_useHttpConnection && (_useTextEncoding == false))
                _useTextEncoding = true;
            _nameResolutionOn = this.Root.Environment.UserInstance.NameResolutionOn;

            // Host generieren
            //_serviceHost = new ServiceHost(typeof(WCFService), endpointUri);
            _host = WebHost.CreateDefaultBuilder()
                .UseNetTcp(endpointUri.Port)
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(endpointUri.Port, listenOptions =>
                    {
                        if (Debugger.IsAttached)
                        {
                            listenOptions.UseConnectionLogging();
                        }
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddServiceModelServices()
                    .AddServiceModelMetadata()
                    .AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
                })
                .Configure(app =>
                {
                    app.UseServiceModel(builder =>
                    {
                        // Service Endpoint

                        if (_useHttpConnection)
                        {
                            /*
                            WSDualHttpBinding wsDualHttpBinding = new WSDualHttpBinding();
                            wsDualHttpBinding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                            _serviceHost.AddServiceEndpoint(typeof(IWCFService), wsDualHttpBinding, endpointUri);
                            */
                            throw new NotImplementedException();
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
                                builder.AddService<WCFService>();
                                builder.AddServiceEndpoint<WCFService>(typeof(IWCFService), netTcpBinding, endpointUri);

                                // When to use reliable session
                                // http://msdn.microsoft.com/en-us/library/ms733136(v=vs.110).aspx
                                //if (netTcpBinding.ReliableSession != null)
                                //    netTcpBinding.ReliableSession.Enabled = true;
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

                                builder.AddService<WCFService>();
                                builder.AddServiceEndpoint<WCFService>(typeof(IWCFService), netTcpBinding, endpointUri);
                            }
                        }
                    });
                })
                .Build();

            return result;
        }

        public override bool ACPostInit()
        {
            if (!base.ACPostInit())
                return false;

            OnPropertyChanged("WCFServiceChannelList");
            return true;
        }
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
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
            _ACPDispatchToProxies = null;
            _host.StopAsync();

            if (!base.ACDeInit(deleteACClassTask))
                return false;
            OnPropertyChanged("WCFServiceChannelList");
            return true;
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
        public Uri endpointUri
        {
            get
            {
                if (_endpointUri != null)
                    return _endpointUri;

                // Schema / protocol
                string scheme = "net.tcp";
                if (_useHttpConnection)
                    scheme = "http";

                // Authority
                string authority = this.Root.Environment.UserInstance.ServerIPV4;
                if (_useIPV6)
                    authority = this.Root.Environment.UserInstance.ServerIPV6;
                else if (_nameResolutionOn)
                    authority = this.Root.Environment.UserInstance.Hostname;

                if (_useHttpConnection)
                    authority += ":" + this.Root.Environment.UserInstance.ServicePortHTTP;
                else
                    authority += ":" + this.Root.Environment.UserInstance.ServicePortTCP;

                // Address
                _endpointUri = new Uri(String.Format("{0}://{1}/", scheme, authority));
                return _endpointUri;
            }
        }

        private IWebHost _host = null;
        internal IWebHost host
        {
            get
            {
                return _host;
            }
        }

        public ServiceHostBase serviceHost
        {
            get
            {
                var realizedServicesProperty = _host.Services.GetType().GetField("_realizedServices", BindingFlags.NonPublic | BindingFlags.Instance);
                IEnumerable realizedServices = realizedServicesProperty.GetValue(_host.Services) as IEnumerable;
                foreach (Object obj in realizedServices)
                {
                    if (obj.ToString().Contains("CoreWCF.ServiceHostObjectModel") && !obj.ToString().Contains("Logger"))
                    {
                        var objValues = obj.GetType().GetProperty("Value");
                        var objValue = objValues.GetValue(obj);
                        var serviceHostVal = objValue.GetType().GetProperty("Target").GetValue(objValue);
                        var serviceHostBase = serviceHostVal.GetType().GetField("value").GetValue(serviceHostVal) as ServiceHostBase;
                        return serviceHostBase;
                    }
                }
                return null;
            }
        }

        public Communications Communications
        {
            get
            {
                if (this.ParentACComponent is Communications)
                    return this.ParentACComponent as Communications;
                return null;
            }
        }

        private int _TotalCountConnects = 0;
        [ACPropertyInfo(9999)]
        public int TotalCountConnects
        {
            get
            {
                return _TotalCountConnects;
            }

            set
            {
                _TotalCountConnects = value;
                OnPropertyChanged("TotalCountConnects");
                OnPropertyChanged("CountConnected");
                OnPropertyChanged("ConnectionQuality");
                OnPropertyChanged("ConnectionShortInfo");
            }
        }


        private int _TotalCountDisconnects = 0;
        [ACPropertyInfo(9999)]
        public int TotalCountDisconnects
        {
            get
            {
                return _TotalCountDisconnects;
            }

            set
            {
                _TotalCountDisconnects = value;
                OnPropertyChanged("TotalCountDisconnects");
                OnPropertyChanged("CountConnected");
                OnPropertyChanged("ConnectionQuality");
                OnPropertyChanged("ConnectionShortInfo");
                if (CountConnected <= 0)
                {

                    using (ACMonitor.Lock(_20056_ACPLock))
                    {
                        ACPSubscrService.EmtpySubscrACObjectOverAllProxies();
                        this.ACPDispatchToProxies.RemoveAllEvents();
                    }
                }
            }
        }

        [ACPropertyInfo(9999)]
        public int CountConnected
        {
            get
            {
                return TotalCountConnects - TotalCountDisconnects;
            }
        }

        [ACPropertyInfo(9999)]
        public ConnectionQuality ConnectionQuality
        {
            get
            {
                if (CountConnected <= 0)
                    return ConnectionQuality.NoConnections;

                return ConnectionQuality.Good;
            }
        }

        [ACPropertyInfo(9999)]
        public string ConnectionShortInfo
        {
            get
            {
                return String.Format("{0} Ʃ{1}", CountConnected, TotalCountConnects);
            }
        }


        public string ConnectionDetailXML
        {
            get
            {
                var wcfServiceChannelList = WCFServiceChannelList;
                if (!wcfServiceChannelList.Any())
                    return "";
                string xaml = "<WCFServiceManager>";
                xaml += String.Format(
                        "<TotalCountConnects>{0}</TotalCountConnects>" +
                        "<TotalCountDisconnects>{1}</TotalCountDisconnects>",
                    TotalCountConnects,
                    TotalCountDisconnects
                    );
                int nRowCount = 3;

                foreach (WCFServiceChannel channel in wcfServiceChannelList)
                {
                    xaml += channel.ConnectionDetailXML;
                    nRowCount++;
                }
                xaml += "</WCFServiceManager>";
                return xaml;
            }
        }


        internal readonly ACMonitorObject _20056_ACPLock = new ACMonitorObject(20056);

        /// <summary>
        /// ACPDispatch sammelt alle aufgetretenen Property-Events und verteilt diese an alle 
        /// Clients, die diese Properties abonniert haben.
        /// </summary>
        private ACPDispatchService _ACPDispatchToProxies = null;
        internal ACPDispatchService ACPDispatchToProxies
        {
            get
            {
                return _ACPDispatchToProxies;
            }
        }

        private static TimeSpan _ReceiveTimeout = new TimeSpan(6, 0, 0);
        public static TimeSpan ReceiveTimeout
        {
            get
            {
                return _ReceiveTimeout;
            }
        }
#endregion

#region public methods
        internal void OpenServiceHost()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;
            if (serviceHost == null || serviceHost.State != CommunicationState.Opened)
            {
                _ACHostStartThread = new ACThread(StartHost);
                _ACHostStartThread.Name = "ACUrl:" + this.GetACUrl() + ";StartHost();";
                _ACHostStartThread.Start();

                // Initialisiere Event-Verteiler
                _syncDispatch = new SyncQueueEvents();
                if (_ACPDispatchToProxies == null)
                    _ACPDispatchToProxies = new ACPDispatchService();
                _ACPDispatchThread = new ACThread(DispatchProperties);
                _ACPDispatchThread.Name = "ACUrl:" + this.GetACUrl() + ";DispatchProperties();";
                _ACPDispatchThread.Start();

                _syncDispatchPoints = new SyncQueueEvents();
                _ACPDispatchPointsThread = new ACThread(DispatchPoints);
                _ACPDispatchPointsThread.Name = "ACUrl:" + this.GetACUrl() + ";DispatchPoints();";
                _ACPDispatchPointsThread.Start();
            }
        }

        /// <summary>
        /// Sendet eine serverseitige Nachricht an alle Clients
        /// </summary>
        /// <param name="acMessage"></param>
        public void BroadcastACMessageToClients(WCFMessage acMessage)
        {
            if (ACOperationMode != ACOperationModes.Live)
                return;

            try
            {
                foreach (WCFServiceChannel serviceChannelObject in WCFServiceChannelList)
                {
                    if (serviceChannelObject.ClosingConnection)
                        continue;
                    lock ((_20056_ACPLock))
                    {
                        serviceChannelObject.BroadcastACMessageToClient(acMessage);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFServiceManager", "BroadcastACMessageToClients", msg);
            }
        }

        /// <summary>Method sends a PropertyValueEvent from this Real/Server-Object
        /// to all Proxy-Object which has subscribed ist</summary>
        /// <param name="eventArgs"></param>
        /// <param name="forACComponent"></param>
        public bool BroadcastPropertyValueToClients(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent)
        {
            if ((_ACPDispatchToProxies == null) || (ACOperationMode != ACOperationModes.Live))
                return false;
            if (!_syncDispatch.NewItemsEnqueueable)
                return false;
            bool enqueued = false;
            // Falls noch kein ClientChannel connected, dann kein Eintrag in DispatchListe
            if (WCFServiceChannelList == null || !WCFServiceChannelList.Any())
            {
                if (this.CountConnected != 0)
                {
                    Messages.LogError(this.GetACUrl(), "WCFServiceManager.BroadcastPropertyValueToClients()", String.Format("TotalCountConnects {0} of TotalCountDisconnects {1}", TotalCountConnects, TotalCountDisconnects));
                    TotalCountDisconnects = TotalCountConnects;
                }

                return false;
            }
            var vbDump = Root.VBDump;

            //PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACIdentifier, 100, true) : null;
            IACComponent projectService = forACComponent as ACComponentManager;
            if (projectService == null)
                projectService = forACComponent.FindParentComponent<ACComponentManager>(c => c is ACComponentManager);
            //if (perfEvent != null)
            //    vbDump.PerfLoggerStop(ACIdentifier, 100, perfEvent);
            if (projectService == null)
            {
                return false;
            }

            //perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACIdentifier, 101, true) : null;
            enqueued = _ACPDispatchToProxies.Enqueue(projectService, eventArgs);
            //if (perfEvent != null)
            //    vbDump.PerfLoggerStop(ACIdentifier, 101, perfEvent);

            // Signalisiere Thread, dass neues Event ansteht
            if (enqueued)
                _syncDispatch.NewItemEvent.Set();

            return enqueued;
        }

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent"></param>
        public void MarkACObjectOnChangedPoint(IACComponent forACComponent)
        {
            if ((forACComponent == null) || (_ACPDispatchToProxies == null) || (ACOperationMode != ACOperationModes.Live))
                return;
            if (!_syncDispatchPoints.NewItemsEnqueueable)
                return;
            bool enqueued = false;
            IACComponent projectService = GetProjectForObject(forACComponent);
            if (projectService == null)
                return;
            enqueued = _ACPDispatchToProxies.MarkACObjectOnChangedPoint(projectService, forACComponent);

            // Signalisiere Thread, dass neues Event ansteht
            if (enqueued)
                _syncDispatchPoints.NewItemEvent.Set();
        }

        public event EventHandler SubscriptionUpdatedEvent;
        internal void OnSubscriptionUpdated()
        {
            if (SubscriptionUpdatedEvent != null)
            {
                SubscriptionUpdatedEvent(this, new EventArgs());
            }
        }

        #endregion

        #region private methods
        internal IACComponent GetProjectForObject(IACComponent forACComponent)
        {
            if (forACComponent == null)
                return null;
            IACComponent result = null;
            if (forACComponent is ACComponentManager)
                result = forACComponent;
            else if (forACComponent is ApplicationManagerProxy)
                result = forACComponent;
            else
            {
                result = forACComponent.FindParentComponent<ACComponentManager>(c => c is ACComponentManager);
                if (result == null)
                    result = forACComponent.FindParentComponent<ApplicationManagerProxy>(c => c is ApplicationManagerProxy);
            }
            return result;
        }


        private Int64 _BroadcastedItemsTotal = 0;
        private Int64 _BroadcastMessagesSent = 0;
        private Double _AverageBroadcastSize = 0;

        internal void UpdateStatisticOnBroadcast(Int32 countBroadcastedItems)
        {
            _BroadcastMessagesSent++;
            _BroadcastedItemsTotal += countBroadcastedItems;
            _AverageBroadcastSize = _BroadcastedItemsTotal / _BroadcastMessagesSent;
        }

        private Int32 DynDispatchInterval
        {
            get
            {
                if (_BroadcastedItemsTotal < 0)
                    return 50;
                else if (_AverageBroadcastSize < 50)
                    return 50;
                else if (_AverageBroadcastSize < 2000)
                    return Convert.ToInt32(_AverageBroadcastSize);
                else
                    return 2000;
                //else if (_AverageBroadcastSize < 200)
                //    return 400;
                //else if (_AverageBroadcastSize < 400)
                //    return 800;
                //else if (_AverageBroadcastSize < 1000)
                //    return 800;
            }
        }

        /// <summary>
        /// Consumer-Method for dispatching Properties
        /// </summary>
        private void DispatchProperties()
        {
            while (!_syncDispatch.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncDispatch.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(DynDispatchInterval);

                _ACPDispatchThread.StartReportingExeTime();
                try
                {
                    if (ACPDispatchToProxies != null)
                    {
                        IEnumerable<WCFServiceChannel> channelList = WCFServiceChannelList.ToArray();

                        using (ACMonitor.Lock(_20056_ACPLock))
                        {
                            ACPDispatchToProxies.GetValueEventsForSubscription(channelList);
                        }

                        foreach (WCFServiceChannel serviceChannelObject in channelList)
                        {
                            if (serviceChannelObject.ClosingConnection)
                                continue;
                            serviceChannelObject.BroadcastPreparedPropertyValues();
                        }
                    }
                    //foreach (WCFServiceChannel serviceChannelObject in WCFServiceChannelList)
                    //{
                    //    if (serviceChannelObject.ClosingConnection)
                    //        continue;
                    //    EnterCS();
                    //    try
                    //    {
                    //        serviceChannelObject.BroadcastPropertyValues();
                    //    }
                    //    finally
                    //    {
                    //        LeaveCS();
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DispatchProperties(1)", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "DispatchProperties(2)", e.InnerException.Message);
                }

                // TODO: Lösche Liste (Was ist wenn nicht alle Empfänger erreicht?)
                try
                {
                    if (ACPDispatchToProxies != null)
                        ACPDispatchToProxies.RemoveAllSentEvents();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DispatchProperties(3)", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "DispatchProperties(4)", e.InnerException.Message);
                }
                _ACPDispatchThread.StopReportingExeTime();
            }
            _syncDispatch.ThreadTerminated();
        }

        private void DispatchPoints()
        {
            while (!_syncDispatchPoints.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncDispatchPoints.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(50);

                _ACPDispatchPointsThread.StartReportingExeTime();
                try
                {
                    foreach (WCFServiceChannel serviceChannelObject in WCFServiceChannelList)
                    {
                        if (serviceChannelObject.ClosingConnection)
                            continue;

                        using (ACMonitor.Lock(_20056_ACPLock))
                        {
                            serviceChannelObject.DispatchPoints();
                        }
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DispatchPoints(1)", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "DispatchPoints(2)", e.InnerException.Message);
                }

                try
                {
                    if (ACPDispatchToProxies != null)
                        ACPDispatchToProxies.RemoveAllMarkedObjects();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DispatchPoints(3)", e.Message);
                    if (e.InnerException != null)
                        Messages.LogException(this.GetACUrl(), "DispatchPoints(4)", e.InnerException.Message);
                }


                _ACPDispatchPointsThread.StopReportingExeTime();
            }
            _syncDispatchPoints.ThreadTerminated();
        }

        public void StartHost()
        {
            //while (!_syncHostStart.ExitThreadEvent.WaitOne(0, false))
            //{
            host.Start();

            if (serviceHost != null)
            {
                ContractDescription cd = serviceHost.Description.Endpoints[0].Contract;
                foreach (OperationDescription opDescr in cd.Operations)
                {
                    foreach (IOperationBehavior behavior in opDescr.OperationBehaviors)
                    {
                        if (behavior is DataContractSerializerOperationBehavior)
                        {
                            DataContractSerializerOperationBehavior dataContractBeh = behavior as DataContractSerializerOperationBehavior;
                            dataContractBeh.MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph;
                            dataContractBeh.DataContractResolver = ACConvert.MyDataContractResolver;
                        }
                    }
                }
            }
            //}
        }

        public void ShutdownClients()
        {
            using (ACMonitor.Lock(_20056_ACPLock))
            {
                foreach (WCFServiceChannel serviceChannelObject in WCFServiceChannelList)
                {
                    serviceChannelObject.ShutdownClient();
                }
            }
        }

#endregion

#region DataShow
        WCFServiceChannel _CurrentWCFServiceChannel;
        [ACPropertyCurrent(9999, "WCFServiceChannel")]
        public WCFServiceChannel CurrentWCFServiceChannel
        {
            get
            {
                return _CurrentWCFServiceChannel;
            }
            set
            {
                _CurrentWCFServiceChannel = value;
                OnPropertyChanged("CurrentWCFServiceChannel");
            }
        }

        [ACPropertyList(9999, "WCFServiceChannel")]
        public IEnumerable<WCFServiceChannel> WCFServiceChannelList
        {
            get
            {
                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    return ACMemberList.Where(c => c is WCFServiceChannel).Select(c => c as WCFServiceChannel).ToArray();
                }
            }
        }


        WCFServiceChannel _SelectedWCFServiceChannel;
        [ACPropertySelected(9999, "WCFServiceChannel")]
        public WCFServiceChannel SelectedWCFServiceChannel
        {
            get
            {
                return _SelectedWCFServiceChannel;
            }
            set
            {
                _SelectedWCFServiceChannel = value;
                OnPropertyChanged("SelectedWCFServiceChannel");
            }
        }
#endregion
    }
}
