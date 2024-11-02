// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared;

namespace gip.core.tcShared.ACVariobatch
{
    public static class ACVariobatch
    {
        // Metadata about Memory-Structure, Index in Array represents Instance-IDs
        public static ACRMemoryMetaObj[] Meta;

        // Copy of Memory, Index in Array represents Instance-ID
        public static byte[] Memory;
    }
}