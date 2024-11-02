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
    /// Two-Way-Valve without basic position
    /// Zwei-Wege-Ventil ohne Grundstellung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Valve 2-way without basic position'}de{'Ventil 2 Wege ohne Grundstellung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve2_In : PAEActuator2way_In
    {
        #region c'tors

        static PAEActValve2_In()
        {
            RegisterExecuteHandler(typeof(PAEActValve2_In), HandleExecuteACMethod_PAEActValve2_In);
        }

        public PAEActValve2_In(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve2_In(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_In(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
