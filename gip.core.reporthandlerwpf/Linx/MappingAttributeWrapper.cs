// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Reflection;

namespace gip.core.reporthandlerwpf
{
    public class MappingAttributeWrapper
    {
        public PropertyInfo PropertyInfo { get; set; }
        public LinxByteMappingAttribute LinxByteMapping { get; set; }
    }
}
