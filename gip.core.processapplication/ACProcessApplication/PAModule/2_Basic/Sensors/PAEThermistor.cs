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
    /// Thermal relay (Thermistor-Switch, depends on temperature)
    /// Themischer Motorschutz (Temperaturabhängig, Kaltleiter, PTC)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Thermistor Relay(PTC)'}de{'Thermischer Motorschutz(PTC)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEThermistor : PAESensorDigital
    {
        #region c'tors

        static PAEThermistor()
        {
            RegisterExecuteHandler(typeof(PAEThermistor), HandleExecuteACMethod_PAEThermistor);
        }

        public PAEThermistor(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEThermistor(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
