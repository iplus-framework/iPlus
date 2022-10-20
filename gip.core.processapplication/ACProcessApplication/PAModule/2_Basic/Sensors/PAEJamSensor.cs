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
    /// Jam-Sensor
    /// Staumelder
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Jam-sensor'}de{'Staumelder'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEJamSensor : PAESensorDigital
    {
        #region c'tors

        static PAEJamSensor()
        {
            RegisterExecuteHandler(typeof(PAEJamSensor), HandleExecuteACMethod_PAEJamSensor);
        }

        public PAEJamSensor(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEJamSensor(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
