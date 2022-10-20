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
    /// Two-Way-Valve with basic position
    /// Zwei-Wege-Ventil mit Grundstellung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Valve 2-way with basic position'}de{'Ventil 2 Wege mit Grundstellung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve2_3pos : PAEActuator2way_3pos
    {
        #region c'tors

        static PAEActValve2_3pos()
        {
            RegisterExecuteHandler(typeof(PAEActValve2_3pos), HandleExecuteACMethod_PAEActValve2_3pos);
        }

        public PAEActValve2_3pos(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve2_3pos(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_3pos(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
