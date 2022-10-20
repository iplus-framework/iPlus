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
    /// Two-Way-Actuator analog (mixing valve)
    /// Zwei-Wege-Stellglied analog (Mischventil)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Valve 2-way analog'}de{'Ventil 2 Wege Analog'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve2_Analog : PAEActuator2way_Analog
    {
        #region c'tors

        static PAEActValve2_Analog()
        {
            RegisterExecuteHandler(typeof(PAEActValve2_Analog), HandleExecuteACMethod_PAEActValve2_Analog);
        }

        public PAEActValve2_Analog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve2_Analog(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_Analog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
