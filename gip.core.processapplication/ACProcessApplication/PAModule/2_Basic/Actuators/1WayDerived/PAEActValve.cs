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
    /// Ein-Wege-Ventil
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Valve'}de{'Ventil'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActValve : PAEActuator1way
    {
        #region c'tors

        static PAEActValve()
        {
            RegisterExecuteHandler(typeof(PAEActValve), HandleExecuteACMethod_PAEActValve);
        }

        public PAEActValve(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActValve(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
