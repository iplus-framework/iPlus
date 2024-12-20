// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IVBFindAndReplaceDBSearch
    {
        IEnumerable<IACObjectEntityWithCheckTrans> FARSearchInDB(VBBSOFindAndReplace bso, string wordToFind);
        bool IsEnabledFARSearchInDB(VBBSOFindAndReplace bso);
        void FARSearchItemSelected(VBBSOFindAndReplace bso, IACObjectEntityWithCheckTrans item);
    }
}
