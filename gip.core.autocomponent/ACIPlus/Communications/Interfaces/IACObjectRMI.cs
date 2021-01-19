using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACObjectRMI
    {
        void OnACMethodExecuted(int MethodInvokeRequestID, object MethodResult);
    }
}
