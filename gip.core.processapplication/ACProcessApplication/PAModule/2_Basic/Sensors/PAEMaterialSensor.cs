// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
    /// Material-Sensor Sensor
    /// Materialmelder
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Material sensor'}de{'Materialmelder'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEMaterialSensor : PAESensorDigital
    {
        #region c'tors

        static PAEMaterialSensor()
        {
            RegisterExecuteHandler(typeof(PAEMaterialSensor), HandleExecuteACMethod_PAEMaterialSensor);
        }

        public PAEMaterialSensor(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEMaterialSensor(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorDigital(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
