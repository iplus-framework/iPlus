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
    /// Baseclass for electrical drives generally
    /// Basisklasse für Antriebe generell
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass Drives'}de{'Basisklasse Antriebe'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEDriveBase : PAEControlModuleBase
    {
        #region c'tors

        static PAEDriveBase()
        {
            RegisterExecuteHandler(typeof(PAEDriveBase), HandleExecuteACMethod_PAEDriveBase);
        }

        public PAEDriveBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 500

        #region Read-Values from PLC
        [ACPropertyBindingTarget(500, "Read from PLC", "en{'Maintenance switch'}de{'Wartungsschalter'}", "", false, false, RemotePropID = 33)]
        public IACContainerTNet<Boolean> MaintenanceSwitch { get; set; }
        #endregion

        #endregion

        //Methods, Range: 500

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEDriveBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEControlModuleBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
