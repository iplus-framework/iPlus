// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace gip.core.dbsyncer
{
    public static class Update

    {


        public static Dictionary<string, string> Versions
        {
            get

            {
                Dictionary<string, string> versions = new Dictionary<string, string>();
                versions.Add("1.0.0.0", "");
                versions.Add("2.0.0.0", "DBSyncerUpdate.unit.test");
                return versions;
            }
        }
    }
}