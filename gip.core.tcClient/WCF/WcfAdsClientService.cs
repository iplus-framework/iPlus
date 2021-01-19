﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared.WCF;

namespace gip.core.tcClient
{
    public class WcfAdsClientServiceCallback : IWcfAdsServiceCallback
    {
        private WcfAdsClientChannel _WcfAdsClientChannel = null;

        public WcfAdsClientServiceCallback(WcfAdsClientChannel clientChannel)
        {
            _WcfAdsClientChannel = clientChannel;
        }

        public void InvokeCallback(WcfAdsMessage message)
        {
            if (_WcfAdsClientChannel == null)
                return;
            _WcfAdsClientChannel.EnqeueReceivedMessageFromService(message);
        }
    }
}