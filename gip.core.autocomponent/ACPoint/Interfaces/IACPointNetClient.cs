using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNetClient
    {
        IACPointNetBase GetServicePoint(IACObject fromACComponent, string fromPointName);

        void InvokeSetMethod(IACPointNetBase point);
    }
}

