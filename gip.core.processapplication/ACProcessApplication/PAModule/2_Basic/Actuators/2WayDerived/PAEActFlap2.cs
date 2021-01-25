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
    /// Zwei-Wege-Klappe
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Flap 2-way'}de{'Klappe 2 Wege'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActFlap2 : PAEActuator2way
    {
        #region c'tors

        static PAEActFlap2()
        {
            RegisterExecuteHandler(typeof(PAEActFlap2), HandleExecuteACMethod_PAEActFlap2);
        }

        public PAEActFlap2(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActFlap2(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
