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
    /// Three-Way-Valve with four flange
    /// Drei-Wege-Ventil mit vier Flanschen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Valve 3-way'}de{'Ventil 3 Wege'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve3 : PAEActuator3way
    {
        #region c'tors

        static PAEActValve3()
        {
            RegisterExecuteHandler(typeof(PAEActValve3), HandleExecuteACMethod_PAEActValve3);
        }

        public PAEActValve3(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve3(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator3way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
