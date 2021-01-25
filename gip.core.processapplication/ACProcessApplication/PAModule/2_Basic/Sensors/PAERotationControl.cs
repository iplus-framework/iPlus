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
    /// Rotation control
    /// Drehwächter
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Rotation control'}de{'Drehwächter'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAERotationControl : PAESensorDigital
    {
        #region c'tors

        static PAERotationControl()
        {
            RegisterExecuteHandler(typeof(PAERotationControl), HandleExecuteACMethod_PAERotationControl);
        }

        public PAERotationControl(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        #region Properties, Range: 500

        #region Configuration
        // Methods, Range: 600
        [ACPropertyBindingTarget(605, "Configuration", "en{'Signal is impulse'}de{'Sensor-Signal ist Impuls'}", "", true, true, RemotePropID=36)]
        public IACContainerTNet<bool> SignalIsImpulse { get; set; }
        #endregion

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAERotationControl(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
