// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync
{
    /// <summary>
    /// Simple sync message
    /// </summary>
    public class SyncMessage
    {
        public MessageLevel MessageLevel { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
