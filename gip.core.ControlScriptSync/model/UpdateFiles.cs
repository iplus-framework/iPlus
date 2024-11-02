// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.ControlScriptSync.file;
using System.Collections.Generic;

namespace gip.core.ControlScriptSync.model
{
    public class UpdateFiles
    {
        public List<ScriptFileInfo> Items { get; set; }

        public List<ScriptFileInfo> ExcludedItems { get; set; }

        public string MaxVersion { get;set;}
    }
}
