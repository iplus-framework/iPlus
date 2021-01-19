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
    /// Zwei-Wege-Weiche
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Diverter'}de{'Weiche'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActDiverter : PAEActuator2way_In
    {
        #region c'tors

        static PAEActDiverter()
        {
            RegisterExecuteHandler(typeof(PAEActDiverter), HandleExecuteACMethod_PAEActDiverter);
        }

        public PAEActDiverter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActDiverter(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_In(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
        #endregion
    }
}
