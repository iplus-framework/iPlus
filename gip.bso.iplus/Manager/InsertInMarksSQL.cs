// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.bso.iplus.Manager
{
    public static class InsertInMarksSQLExt
    {
        public static string InsertInMarks(this string sql)
        {
            return string.Format("'{0}'", sql);
        }
    }
}
