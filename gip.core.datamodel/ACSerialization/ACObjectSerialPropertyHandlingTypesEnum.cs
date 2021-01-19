using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{

    /// <summary>
    ///  Enum defined for enable possibility to make property order by type
    ///  Order in enumeration defines fetching order by setting property
    /// </summary>
    public enum ACObjectSerialPropertyHandlingTypesEnum
    {
        Primitive,
        String,
        DateTime,
        ACClassDesignByte,
        IACObject
    }
}
