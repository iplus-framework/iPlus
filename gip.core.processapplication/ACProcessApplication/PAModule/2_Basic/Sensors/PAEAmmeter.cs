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
    /// Ammeter (maesures ampere)
    /// Strommessgerät (misst Ampere)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Ammeter'}de{'Strommessgerät)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEAmmeter : PAESensorAnalog
    {
        #region c'tors

        static PAEAmmeter()
        {
            RegisterExecuteHandler(typeof(PAEAmmeter), HandleExecuteACMethod_PAEAmmeter);
        }

        public PAEAmmeter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        // Methods, Range: 600

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEAmmeter(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorAnalog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
