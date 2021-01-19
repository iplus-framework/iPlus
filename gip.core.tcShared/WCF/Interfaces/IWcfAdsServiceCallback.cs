﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace gip.core.tcShared.WCF
{
    [ServiceKnownType(typeof(WcfAdsMessage))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryByteEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryUIntEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryIntEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryDIntEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryUDIntEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryStringEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryRealEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryLRealEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryDTEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryTimeEvent[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryMetaObj))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryMetaObj[]))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryMetaProp))]
    [ServiceKnownType(typeof(ACVariobatch.ACRMemoryMetaProp[]))]
    [ServiceKnownType(typeof(ACVariobatch.VBEvent))]
    public interface IWcfAdsServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void InvokeCallback(WcfAdsMessage message);
    }
}