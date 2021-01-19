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
    /// Safety relay
    /// Schutzkreis/relais
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Safety relay'}de{'Schutzkreis/relais'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAESafetyRelay : PAESensorDigital
    {
        #region c'tors

        static PAESafetyRelay()
        {
            RegisterExecuteHandler(typeof(PAESafetyRelay), HandleExecuteACMethod_PAESafetyRelay);
        }

        public PAESafetyRelay(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAESafetyRelay(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
