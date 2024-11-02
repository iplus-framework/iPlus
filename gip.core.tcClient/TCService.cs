// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.communication;
using gip.core.datamodel;

namespace gip.core.tcClient
{
    [ACClassInfo(Const.PackName_TwinCAT, "en{'TwinCAT Service'}de{'TwinCAT Service'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class TCService : ACService
    {
        public TCService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

    }
}
