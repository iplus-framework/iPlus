// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace DBSyncerUpdate.unit.test.DBSyncerOldModel
{
    /// <summary>
    /// 
    /// </summary>
    public class DbSyncerInfoContext
    {
        public string DbSyncerInfoContextID { get; set; }
        public string Name {get;set;}
        public string ConnectionName {get;set;}
        public int Order { get; set; }

        public override string ToString()
        {
            return DbSyncerInfoContextID;
        }
    }
}
