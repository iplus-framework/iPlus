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
    /// Hygrometer (maesures celsius)
    /// Hygrometer (misst Celsisus)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Hygrometer'}de{'Hygrometer)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEHygrometer : PAESensorAnalog
    {
        #region c'tors

        static PAEHygrometer()
        {
            RegisterExecuteHandler(typeof(PAEHygrometer), HandleExecuteACMethod_PAEHygrometer);
        }

        public PAEHygrometer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEHygrometer(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorAnalog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
