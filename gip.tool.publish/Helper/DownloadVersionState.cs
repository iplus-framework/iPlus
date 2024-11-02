// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.publish
{
    public enum DownloadVersionState : short
    {
        DownloadFail = 0,
        SameVersionExist = 10,
        DeseralizationFail = 20,
        Ok = 30
    }
}
