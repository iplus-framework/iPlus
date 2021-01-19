﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared.ACVariobatch;
using System.Net;
using System.Net.Sockets;

namespace gip.core.tcShared.WCF
{
    public class WcfAdsServiceManager
    {
        #region private members

        public const int MaxBufferSize = 20485760; // 10 MB
        public const int MaxItemsInObjectGraph = Int32.MaxValue;
        public const int MaxStringLength = 1048576; // 2^20 byte
        public static WcfAdsServiceManager _Self;
        public IAdsAgent AdsAgent;


        #endregion

        public WcfAdsServiceManager(IAdsAgent parent)
        {
            AdsAgent = parent;
            InitWcfAdsManager();
            _Self = this;
        }

        private void InitWcfAdsManager()
        {
            NetTcpBinding netTcpBinding = new NetTcpBinding();
            if (netTcpBinding.ReaderQuotas != null)
                netTcpBinding.ReaderQuotas.MaxStringContentLength = MaxStringLength;
            netTcpBinding.MaxBufferSize = MaxBufferSize;
            netTcpBinding.MaxReceivedMessageSize = MaxBufferSize;
            //netTcpBinding.ConnectionBufferSize = WCFServiceManager.MaxBufferSize / 8;
            netTcpBinding.MaxBufferPoolSize = MaxBufferSize;
            netTcpBinding.ReceiveTimeout = ReceiveTimeout;
            netTcpBinding.Security.Mode = SecurityMode.None;
            
            //netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            _serviceHost = new ServiceHost(typeof(WcfAdsService), endpointUri);
            _serviceHost.AddServiceEndpoint(typeof(IWcfAdsService), netTcpBinding, endpointUri);

            ContractDescription cd = _serviceHost.Description.Endpoints[0].Contract;
            foreach (OperationDescription opDescr in cd.Operations)
            {
                foreach (IOperationBehavior behavior in opDescr.Behaviors)
                {
                    if (behavior is DataContractSerializerOperationBehavior)
                    {
                        DataContractSerializerOperationBehavior dataContractBeh = behavior as DataContractSerializerOperationBehavior;
                        dataContractBeh.MaxItemsInObjectGraph = MaxItemsInObjectGraph;
                        //dataContractBeh.DataContractResolver = ACConvert.MyDataContractResolver;
                    }
                }
            }
            OpenServiceHost();
        }

        private void OpenServiceHost()
        {
            if (ServiceHost.State != CommunicationState.Opened)
            {
                ServiceHost.Open();
            }
        }

        private ServiceHost _serviceHost = null;
        internal ServiceHost ServiceHost
        {
            get
            {
                return _serviceHost;
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

        private Uri _endpointUri = null;
        public Uri endpointUri
        {
            get
            {
                if (_endpointUri != null)
                    return _endpointUri;

                // Schema / protocol
                string scheme = "net.tcp";

                // Authority
                string authority = GetLocalIPAddress();
                //if (_useIPV6)
                //    authority = this.Root.Environment.UserInstance.ServerIPV6;
                //else if (_nameResolutionOn)
                //    authority = this.Root.Environment.UserInstance.Hostname;

                authority += ":" + "8020";

                // Address
                _endpointUri = new Uri(String.Format("{0}://{1}/", scheme, authority));
                return _endpointUri;
            }
        }

        public List<WcfAdsServiceChannel> WcfAdsServiceChannelList = new List<WcfAdsServiceChannel>();

        #region Methods

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryByteEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToByteEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryUIntEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToUIntEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryIntEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToIntEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryDIntEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToDIntEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryUDIntEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToUDIntEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryRealEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToRealEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryLRealEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToLRealEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryStringEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToStringEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryTimeEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToTimeEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastEventsToChannels(int currentEventIndex, int nextEventIndex, ACRMemoryDTEvent[] events)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.AddToDTEventDict(currentEventIndex, nextEventIndex, events);
            }
        }

        public void BroadcastMetadataToChannels()
        {
            foreach(WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.PrepareMetadataMessageForClient(ACVariobatch.ACVariobatch.Meta);
                channel.PrepareMemoryMessageForClient(AdsAgent.ReadMemory());
            }
        }

        public void BroadcastConnectionStateToChannels(ConnectionState connectionState)
        {
            foreach (WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if (channel.ClosingConnection)
                    continue;
                channel.SendConnectionState(connectionState);
            }
        }

        public void BroadcastResultToChannels(byte[] result)
        {
            foreach(WcfAdsServiceChannel channel in WcfAdsServiceChannelList)
            {
                if(channel.ClosingConnection)
                    continue;
                channel.SendPAFuncResult(result);
            }
        }

        public void StartReadEvents()
        {
            AdsAgent.StartReadEvent();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }

        #endregion

    }
}