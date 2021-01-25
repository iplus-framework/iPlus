using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    /// <summary>
    /// Klappe
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Flap'}de{'Klappe'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActFlap : PAEActuator1way
    {
        #region c'tors

        static PAEActFlap()
        {
            RegisterExecuteHandler(typeof(PAEActFlap), HandleExecuteACMethod_PAEActFlap);
        }

        public PAEActFlap(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActFlap(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
