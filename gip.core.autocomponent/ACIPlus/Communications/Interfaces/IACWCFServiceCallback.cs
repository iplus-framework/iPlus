using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ServiceContract]
    public interface IWCFServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Invoke(WCFMessage acMessage);
    }
}
