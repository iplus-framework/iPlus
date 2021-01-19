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
    /// Zwei-Wege-Ventil
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Valve 2-way'}de{'Ventil 2 Wege'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve2 : PAEActuator2way
    {
        #region c'tors

        static PAEActValve2()
        {
            RegisterExecuteHandler(typeof(PAEActValve2), HandleExecuteACMethod_PAEActValve2);
        }

        public PAEActValve2(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve2(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
