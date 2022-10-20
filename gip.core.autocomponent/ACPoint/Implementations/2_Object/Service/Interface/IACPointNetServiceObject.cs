using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNetServiceObject<T> : IACPointNetObject<T>, IACPointNetService<T, ACPointNetWrapObject<T>>
        where T : IACObject
    {
    }
}

