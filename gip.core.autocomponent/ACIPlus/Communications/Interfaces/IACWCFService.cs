using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreWCF;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    //[System.ServiceModel.ServiceContractAttribute(Namespace = "http://Microsoft.ServiceModel.Samples", ConfigurationName = "Microsoft.ServiceModel.Samples.ICalculatorDuplex", CallbackContract = typeof(Microsoft.ServiceModel.Samples.ICalculatorDuplexCallback), SessionMode = System.ServiceModel.SessionMode.Required)]
    [ServiceContract(CallbackContract = typeof(IWCFServiceCallback), SessionMode = SessionMode.Required)]
    public interface IWCFService
    {
        [OperationContract(IsOneWay=true)]
        void Invoke(WCFMessage acMessage);
    }
}
