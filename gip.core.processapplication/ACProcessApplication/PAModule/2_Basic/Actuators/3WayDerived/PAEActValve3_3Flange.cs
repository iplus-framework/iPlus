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
    /// Three-Way-Valve with three flange
    /// Drei-Wege-Ventil mit drei Flanschen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Valve 3-way 3-flange'}de{'Ventil 3 Wege, 3 Flansche'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve3_3Flange : PAEActuator3way
    {
        #region c'tors

        static PAEActValve3_3Flange()
        {
            RegisterExecuteHandler(typeof(PAEActValve3_3Flange), HandleExecuteACMethod_PAEActValve3_3Flange);
        }

        public PAEActValve3_3Flange(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve3_3Flange(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator3way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
