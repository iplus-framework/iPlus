// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System.Collections.Generic;

namespace gip.core.autocomponent
{
    public class ValidateConfigStoreModel
    {
        public bool IsValid { get; set; }

        public List<IACConfigStore> NotValidConfigStores{get;set;}
    }
}
