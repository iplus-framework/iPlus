// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.datamodel
{
    [Serializable]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'FindMatchingUrlsParam'}de{'FindMatchingUrlsParam'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class FindMatchingUrlsParam
    {
        public Func<IACComponent, bool> Query { get; set; }
    }
}
