// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    /// <summary>
    /// Two-Way-Actuator analog (mixing valve)
    /// Zwei-Wege-Stellglied analog (Mischventil)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'2-Way Actuator analog'}de{'2-Wege Stellglied Analog'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator2way_Analog : PAEActuator1way_Analog
    {
        #region c'tors

        static PAEActuator2way_Analog()
        {
            RegisterExecuteHandler(typeof(PAEActuator2way_Analog), HandleExecuteACMethod_PAEActuator2way_Analog);
        }

        public PAEActuator2way_Analog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn2 = new PAPoint(this, nameof(PAPointMatIn2));
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Points
        PAPoint _PAPointMatIn2;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatIn2
        {
            get
            {
                return _PAPointMatIn2;
            }
        }

        #endregion


        // Properties, Range:700
        // Methods, Range: 701

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator2way_Analog(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way_Analog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
