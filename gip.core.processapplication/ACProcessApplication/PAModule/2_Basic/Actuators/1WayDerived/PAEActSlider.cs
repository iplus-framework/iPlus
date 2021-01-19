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
    /// Schieber
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Slider'}de{'Schieber'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActSlider : PAEActuator1way
    {
        #region c'tors

        static PAEActSlider()
        {
            RegisterExecuteHandler(typeof(PAEActSlider), HandleExecuteACMethod_PAEActSlider);
        }

        public PAEActSlider(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActSlider(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
