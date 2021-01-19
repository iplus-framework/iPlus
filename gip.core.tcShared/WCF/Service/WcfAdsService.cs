﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Threading;

namespace gip.core.tcShared.WCF
{
    public class WcfAdsService : IWcfAdsService
    {
        #region Private members

        IWcfAdsServiceCallback _callback;
        WcfAdsServiceChannel _wcfAdsServiceChannel;
        IContextChannel _currentChannel;
        WcfAdsServiceManager _WcfAdsServiceManager;
        bool _ConnectionOn = true;
        private bool _InInvocation = false;

        #endregion

        public WcfAdsService()
        {
            _WcfAdsServiceManager = WcfAdsServiceManager._Self;
            _callback = OperationContext.Current.GetCallbackChannel<IWcfAdsServiceCallback>();
            _wcfAdsServiceChannel = new WcfAdsServiceChannel(this);
            _WcfAdsServiceManager.WcfAdsServiceChannelList.Add(_wcfAdsServiceChannel);

            _currentChannel = OperationContext.Current.Channel;
            if (OperationContext.Current.IncomingMessageProperties != null && OperationContext.Current.IncomingMessageProperties.Values != null)
            {
                foreach (object value in OperationContext.Current.IncomingMessageProperties.Values)
                {
                    if (value is System.ServiceModel.Channels.RemoteEndpointMessageProperty)
                    {
                        try
                        {
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpointProp = value as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
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
                        catch (Exception)
                        {
                            
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

        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            if (_WcfAdsServiceManager == null)
                return;
            if (_wcfAdsServiceChannel != null && _ConnectionOn)
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

                            _wcfAdsServiceChannel = null;
                        }
                        ).Start();
                    }
                    else
                    {
                        _wcfAdsServiceChannel = null;
                    }
                }
            }
        }

        private void Channel_Closing(object sender, EventArgs e)
        {
            if (_WcfAdsServiceManager == null)
                return;
            if (_wcfAdsServiceChannel != null && _ConnectionOn)
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

                            _wcfAdsServiceChannel = null;
                        }
                        ).Start();
                    }
                    else
                    {
                        _wcfAdsServiceChannel = null;
                    }
                }
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

        public void Invoke(WcfAdsMessage message)
        {
            _wcfAdsServiceChannel.EnqeueReceivedMessageFromClient(message);
        }

        public void TryInvoke(WcfAdsMessage message)
        {
            try
            {
                _callback.InvokeCallback(message);
            }
            catch (Exception)
            {
                //message
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