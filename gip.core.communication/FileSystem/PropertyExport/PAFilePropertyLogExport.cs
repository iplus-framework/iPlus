// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;


namespace gip.core.communication
{
    /// <summary>
    /// Exporter for Property-Logs (Root-Node)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Propertylog Export'}de{'Propertylog Export'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFilePropertyLogExport : PAFileCyclicExport
    {
        #region c´tors
        public PAFilePropertyLogExport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool result = await base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion
    }
}
