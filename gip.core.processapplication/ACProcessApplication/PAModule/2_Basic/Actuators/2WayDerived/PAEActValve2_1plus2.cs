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
    /// Two-Way-Valve with both positions simultaneous
    /// Two-Wege-Ventil mit 2 Positionen gleichzeitig
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Valve 2-way simultaneous'}de{'Ventil 2 Wege gleichzeitig'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve2_1plus2 : PAEActuator2way_1plus2
    {
        #region c'tors

        static PAEActValve2_1plus2()
        {
            RegisterExecuteHandler(typeof(PAEActValve2_1plus2), HandleExecuteACMethod_PAEActValve2_1plus2);
        }

        public PAEActValve2_1plus2(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve2_1plus2(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_1plus2(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
