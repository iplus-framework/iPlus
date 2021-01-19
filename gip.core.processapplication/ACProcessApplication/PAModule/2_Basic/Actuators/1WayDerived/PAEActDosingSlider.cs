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
    /// dosierschieber
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Dosing-slider'}de{'Dosierschieber'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActDosingSlider : PAEActuator1way_3pos
    {
        #region c'tors

        static PAEActDosingSlider()
        {
            RegisterExecuteHandler(typeof(PAEActDosingSlider), HandleExecuteACMethod_PAEActDosingSlider);
        }

        public PAEActDosingSlider(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActDosingSlider(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way_3pos(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
