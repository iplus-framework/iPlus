using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.ComponentModel;

namespace gip.core.processapplication
{
    /// <summary>
    /// Emergency-switch
    /// Not-Aus
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Emergency-Switch'}de{'Not-Aus'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEEmergencySwitch : PAESensorDigital
    {
        #region c'tors

        static PAEEmergencySwitch()
        {
            RegisterExecuteHandler(typeof(PAEEmergencySwitch), HandleExecuteACMethod_PAEEmergencySwitch);
        }

        public PAEEmergencySwitch(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEmergencySwitch(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
