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
    /// Mischventil
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Mixingvalve'}de{'Mischventil'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActMixingValve : PAEActuator2way_Analog
    {
        #region c'tors

        static PAEActMixingValve()
        {
            RegisterExecuteHandler(typeof(PAEActMixingValve), HandleExecuteACMethod_PAEActMixingValve);
        }

        public PAEActMixingValve(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActMixingValve(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way_Analog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
