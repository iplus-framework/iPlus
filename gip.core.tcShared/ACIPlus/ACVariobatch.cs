﻿using System;
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