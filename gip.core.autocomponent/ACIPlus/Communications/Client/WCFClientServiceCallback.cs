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
using CoreWCF;

namespace gip.core.autocomponent
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = WCFServiceManager.MaxItemsInObjectGraph)]
    public class WCFClientServiceCallback : IWCFServiceCallback
    {
        private WCFClientChannel _WCFClientChannel = null;
         
        public WCFClientServiceCallback(WCFClientChannel acWCFClientChannel)
        {
            _WCFClientChannel = acWCFClientChannel;
        }

        #region IWCFServiceCallback Member

        public void Invoke(WCFMessage acMessage)
        {
            if (_WCFClientChannel == null)
                return;
            _WCFClientChannel.EnqeueReceivedMessageFromPeer(acMessage);
        }

        #endregion
    }
}
