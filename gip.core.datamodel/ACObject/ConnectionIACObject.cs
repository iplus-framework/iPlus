using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ConnectionIACObject'}de{'ConnectionIACObject'}", Global.ACKinds.TACClass, Global.ACStorableTypes.Required, true, false)]
    public class ConnectionIACObject : Tuple<IACObject, IACObject>
    {
        public ConnectionIACObject(IACObject IACObjectFrom, IACObject IACObjectTo)
            : base(IACObjectFrom, IACObjectTo)
        {

        }
    }
}
