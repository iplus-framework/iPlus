// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Rule types'}", Global.ACKinds.TACEnum)]
    public enum ACClassWFRuleTypes : short
    {
        Parallelization = 0,
        Allowed_instances = 10,
        ActiveRoutes = 20,
        Excluded_module_types = 30,
        Excluded_process_modules = 40,
        Breakpoint = 50,
    }
}
