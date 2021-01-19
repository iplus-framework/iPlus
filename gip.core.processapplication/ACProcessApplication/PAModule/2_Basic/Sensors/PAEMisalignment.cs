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
    /// Misalignment Sensor
    /// Schieflaufwächter
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Misalignment Sensor'}de{'Schieflaufwächter'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEMisalignment : PAESensorDigital
    {
        #region c'tors

        static PAEMisalignment()
        {
            RegisterExecuteHandler(typeof(PAEMisalignment), HandleExecuteACMethod_PAEMisalignment);
        }

        public PAEMisalignment(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEMisalignment(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
