// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ExportErrorEventArgs
    {
        public ExportErrosEnum ExportErrorType { get; set; }
        public Exception Exception { get; set; }

        public string ACUrl { get; set; }

        public string Path { get; set; }
    }
}
