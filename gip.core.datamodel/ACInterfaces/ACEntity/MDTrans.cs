// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    public class MDTrans : IMDTrans
    {
        public MDTrans(string mDNameTrans, string mDKey)
        {
            MDNameTrans = mDNameTrans;
            MDKey = mDKey;
        }

        public string MDNameTrans { get; set; }
        public string MDKey { get; set; }
    }
}
