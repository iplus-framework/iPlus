using gip.core.dbsyncer.helper;
using System;

namespace DBSyncerUpdate.unit.test.DBSyncerOldModel
{
  
    public class DbSyncerInfo
    {

        public int DbSyncerInfoID { get; set; }
        public string DbSyncerInfoContextID { get; set; }
        public int Version { get; set; }
        public DateTime ScriptDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateAuthor { get; set; }

        public string ToInsertSql()
        {
            return string.Format(SQLScripts.DbSyncerInfoInsert, DbSyncerInfoContextID, Version, string.Format("convert(datetime, '{0}', 120)", ScriptDate.ToString("yyyy-MM-dd HH:mm:ss")), string.Format("convert(datetime, '{0}', 120)", UpdateDate.ToString("yyyy-MM-dd HH:mm:ss")), UpdateAuthor);
        }

        public override string ToString()
        {
            return string.Format(@"dbsync-{0}-{1}.sql", Version.ToString("{0:0000}"), UpdateAuthor);
        }
    }
}
