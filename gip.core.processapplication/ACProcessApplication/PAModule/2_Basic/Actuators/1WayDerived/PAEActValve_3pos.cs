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
    /// Dosing-Valve Rough/Fine
    /// Dosierventil Grob/Fein
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Dosing-Valve Rough/Fine'}de{'Dosierventil Grob/Fein'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve_3pos : PAEActuator1way_3pos
    {
        #region c'tors

        static PAEActValve_3pos()
        {
            RegisterExecuteHandler(typeof(PAEActValve_3pos), HandleExecuteACMethod_PAEActValve_3pos);
        }

        public PAEActValve_3pos(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve_3pos(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way_3pos(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
