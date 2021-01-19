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