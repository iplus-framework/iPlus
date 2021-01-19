using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.communication;
using gip.core.datamodel;

namespace gip.core.tcClient
{
    [ACClassInfo(Const.PackName_TwinCAT, "en{'TCService'}de{'TCService'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class TCService : ACService
    {
        public TCService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

    }
}
