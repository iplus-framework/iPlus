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
    /// Limit Switch (can be used for controlling machinery as part of a control system, as a safety interlock, or as a counter enumerating objects passing a point.)
    /// Endlagenschalter (Erkennet, wenn ein bewegter Gegenstand eine bestimmte Position erreicht hat.)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Limit Switch'}de{'Endlagenschalter'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAELimitSwitch : PAESensorDigital
    {
        #region c'tors

        static PAELimitSwitch()
        {
            RegisterExecuteHandler(typeof(PAEControlModuleBase), HandleExecuteACMethod_PAELimitSwitch);
        }

        public PAELimitSwitch(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAELimitSwitch(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
