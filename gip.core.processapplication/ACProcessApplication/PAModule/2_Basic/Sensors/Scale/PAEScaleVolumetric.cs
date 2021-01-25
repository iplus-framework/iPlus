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
    /// Volumetrically Scale (Count of Impulse is PAESensorAnalog.ActualValue)
    /// Volumetrische Waage(Anzahl der Impulse ist PAESensorAnalog.ActualValue)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scale volumetric'}de{'Waage volumetrisch)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleVolumetric : PAEScaleBase
    {
        #region c'tors

        static PAEScaleVolumetric()
        {
            RegisterExecuteHandler(typeof(PAEScaleVolumetric), HandleExecuteACMethod_PAEScaleVolumetric);
        }

        public PAEScaleVolumetric(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        #region Properties Range 700
        #region Configuration
        [ACPropertyBindingTarget(700, "Configuration", "en{'Pulse pro Liter'}de{'Maximale Dosierzeit'}", "", true, true, RemotePropID=72)]
        public IACContainerTNet<Double> PulsePerLiter { get; set; }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(750, "Read from PLC", "en{'Desired rate of flow'}de{'Solldosierleistung'}", "", false, false, RemotePropID=73)]
        public IACContainerTNet<Double> DesiredRateOfFlow { get; set; }

        [ACPropertyBindingTarget(731, "Read from PLC", "en{'Actual rate of flow'}de{'Istdosierleistung'}", "", false, false, RemotePropID=74)]
        public IACContainerTNet<Double> ActualRateOfFlow { get; set; }
        #endregion

        #endregion

        // Methods, Range: 700

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScaleVolumetric(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEScaleBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
