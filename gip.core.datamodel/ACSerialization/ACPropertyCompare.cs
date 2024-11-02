// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACPropertyCompare
    {
        [ACPropertyInfo(1, "PropertyName", "en{'Property Name'}de{'ACIdentifier'}")]
        public string PropertyName { get; set; }
        public string PropertyTitle { get; set; }

        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public object OldValueStr { get; set; }
        public object NewValueStr { get; set; }
    }
}
