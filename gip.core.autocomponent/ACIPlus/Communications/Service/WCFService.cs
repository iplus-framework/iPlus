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
using CoreWCF;
using CoreWCF.Channels;

namespace gip.core.autocomponent
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WCFService'}de{'WCFService'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class WCFService : IWCFService
    {
        #region private members
        IWCFServiceCallback _callback = null;
        WCFServiceChannel _acServiceObject = null;
        IContextChannel _currentChannel = null;
        bool _ConnectionOn = true; // gibt an dass Verbindung aufrecht erhalten werden soll
        private static ACMonitorObject _20060_InitLock = new ACMonitorObject(20060);
        #endregion

        public WCFService()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IWCFServiceCallback>();
            ACComponent serviceManager = ACRoot.SRoot.ACUrlCommand("\\Communications\\WCFServiceManager") as ACComponent;
            if (serviceManager != null)
            {
                // Lock needed if two clients at the same makes a connection to the server

                using (ACMonitor.Lock(_20060_InitLock))
                {
                    _acServiceObject = serviceManager.StartComponent("WCFServiceChannel", null, new object[] { this }) as WCFServiceChannel;
                }
            }
            _currentChannel = OperationContext.Current.GetCallbackChannel<IContextChannel>();
            if (OperationContext.Current.IncomingMessageProperties != null && OperationContext.Current.IncomingMessageProperties.Values != null)
            {
                foreach (object value in OperationContext.Current.IncomingMessageProperties.Values)
                {
                    if (value is RemoteEndpointMessageProperty)
                    {
                        try
                        {
                            RemoteEndpointMessageProperty endpointProp = value as RemoteEndpointMessageProperty;
                            if (!String.IsNullOrEmpty(endpointProp.Address))
                            {
                                // IPV6-Adresse:
                                if (endpointProp.Address.Contains(':'))
                                    _RemoteAddress = new Uri(String.Format("net.tcp://[{0}]:{1}/", endpointProp.Address, endpointProp.Port));
                                // Sonst IPV4
                                else
                                    _RemoteAddress = new Uri(String.Format("net.tcp://{0}:{1}/", endpointProp.Address, endpointProp.Port));
                            }
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("WCFService", "WCFService", msg);
                        }
                        break;
                    }
                }
                if (_RemoteAddress == null)
                    _RemoteAddress = OperationContext.Current.IncomingMessageProperties.Via;
            }
            _currentChannel.Closing += Channel_Closing;
            _currentChannel.Closed += Channel_Closed;
            // Timeout wegen asnchronen Bearbeitungsprozessen:
            // Falls eine ACURl-Command-Aufruf auf Serverseite lange dauert, würde die WCF (Aufrufer meist der Client) den Kanal schliessen
            // durch auslösen des Events Closing. Das hat zur Folge, dass StopACObject aufgerufen wird.
            _currentChannel.OperationTimeout = new TimeSpan(0, 0, 30);
            //_acServiceObject.Messages.LogDebug(_acServiceObject.GetACUrl(), "WCFServiceChannel.ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)", _acServiceObject.ConnectionDetailXML);
        }

        public void DisconnectClient()
        {
            _ConnectionOn = false;
            try
            {
                if (_currentChannel != null)
                    _currentChannel.Abort();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("WCFService", "DiconnectClient", msg);
            }
        }

        public void Invoke(WCFMessage acMessage)
        {
            if (_acServiceObject == null)
                return;
            _acServiceObject.EnqeueReceivedMessageFromPeer(acMessage);
            return;
        }

        private bool _InInvocation = false;
        public bool InvokeRemote(WCFMessage acMessage)
        {
            if (_callback == null || _ClosingConnection)
                return false;
            try
            {
                _InInvocation = true;
                _callback.Invoke(acMessage);
                //#if DEBUG
                //                _InInvocation = true;
                //                if (_InInvocation)
                //                {
                //                    throw new CommunicationException("strhg");
                //                }
                //#endif
                _InInvocation = false;
            }
            catch (CommunicationException e)
            {
                if (_acServiceObject != null)
                {
                    _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(1)", e.Message);
                    if (e.InnerException != null)
                        _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(2)", e.InnerException.Message);
                    try { _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(3)", e.StackTrace); }
                    catch (Exception exc)
                    {
                        string msg = exc.Message;
                        if (exc.InnerException != null && exc.InnerException.Message != null)
                            msg += " Inner:" + exc.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("WCFService", "InvokeRemote(3b)", msg);
                    }
                }

                //_currentChannel.Close();
                DisconnectClient();
                _InInvocation = false;
                return false;
            }
            catch (Exception e)
            {
                if (_acServiceObject != null)
                {
                    _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(5)", e.Message);
                    if (e.InnerException != null)
                        _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(6)", e.InnerException.Message);
                    try { _acServiceObject.Messages.LogException(_acServiceObject.GetACUrl(), "InvokeRemote(7)", e.StackTrace); }
                    catch(Exception exc)
                    {
                        string msg = exc.Message;
                        if (exc.InnerException != null && exc.InnerException.Message != null)
                            msg += " Inner:" + exc.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("WCFService", "InvokeRemote(10)", msg);
                    }
                }

                //_currentChannel.Close();
                DisconnectClient();
                _InInvocation = false;
                return false;
            }
            return true;
        }

        public void Channel_Closing(object sender, EventArgs e)
        {
            if (WCFServiceManager == null)
                return;
            if (_acServiceObject != null && _ConnectionOn)
            {
                int tries = 0;
                while (_InInvocation)
                {
                    Thread.Sleep(50);
                    tries++;
                    if (tries > 5)
                        break;
                }

                if (!_ClosingConnection)
                {
                    _ClosingConnection = true;

                    // Falls Channel_Closing aufgerufen im gleichen Thread durch Invoke aus WCFServiceChannel 
                    if (_InInvocation)
                    {
                        new Thread(() =>
                        {
                            while (_InInvocation)
                            {
                                Thread.Sleep(100);
                                tries++;
                                if (tries > 20)
                                    break;
                            }

                            if (WCFServiceManager.StopComponent(_acServiceObject))
                                _acServiceObject = null;
                        }
                        ).Start();
                    }
                    else
                    {
                        if (WCFServiceManager.StopComponent(_acServiceObject))
                            _acServiceObject = null;
                    }
                }
            }
        }

        public void Channel_Closed(object sender, EventArgs e)
        {
            if (_acServiceObject != null && _ConnectionOn)
            {
                int tries = 0;
                while (_InInvocation)
                {
                    Thread.Sleep(50);
                    tries++;
                    if (tries > 5)
                        break;
                }
                // Falls StopACComponent bereits in Channel_Closing aufgerufen, tue nichts
                if (!_ClosingConnection)
                {
                    _ClosingConnection = true;
                    // Falls Channel_Closing aufgerufen im gleichen Thread durch Invoke aus WCFServiceChannel 
                    if (_InInvocation)
                    {
                        new Thread(() =>
                        {
                            while (_InInvocation)
                            {
                                Thread.Sleep(100);
                                tries++;
                                if (tries > 20)
                                    break;
                            }

                            if (WCFServiceManager.StopComponent(_acServiceObject))
                                _acServiceObject = null;
                        }
                        ).Start();
                    }
                    else
                    {
                        if (WCFServiceManager.StopComponent(_acServiceObject))
                            _acServiceObject = null;
                    }
                }
            }
        }

        protected WCFServiceManager WCFServiceManager
        {
            get
            {
                if (_acServiceObject == null)
                    return null;
                return _acServiceObject.WCFServiceManager;
            }
        }

        private Uri _RemoteAddress = null;
        public Uri RemoteAddress
        {
            get
            {
                return _RemoteAddress;
            }
        }

        private bool _ClosingConnection = false;
        public bool ClosingConnection
        {
            get
            {
                return _ClosingConnection;
            }
            internal set
            {
                _ClosingConnection = value;
            }
        }
    }
}
