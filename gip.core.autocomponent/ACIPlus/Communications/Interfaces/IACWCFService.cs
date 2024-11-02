// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
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
