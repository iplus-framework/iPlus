using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNetClientObject<T> : IACPointNetObject<T>, IACPointNetClient
        where T : IACObject
    {
        ACPointNetWrapObject<T> AddToServicePoint(IACComponent acObject, string pointName);
        
        bool RemoveFromServicePoint(IACComponent acObject, string pointName);
    }
}

