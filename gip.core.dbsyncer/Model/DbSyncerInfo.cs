using gip.core.dbsyncer.helper;
using System;

namespace gip.core.dbsyncer.model
{
    /// <summary>
    /// Mapped table DbSyncerInfo
    /// </summary>
    public class DbSyncerInfo
    {
        /// <summary>
        /// ID table
        /// </summary>
        public int DbSyncerInfoID { get; set; }

        /// <summary>
        /// Context reference to table DbSyncerInfoContext
        /// </summary>
        public string DbSyncerInfoContextID { get; set; }

        /// <summary>
        /// Date of script creation - used for define script executing order
        /// </summary>
        public DateTime ScriptDate { get; set; }

        /// <summary>
        /// Time when update is realized
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Script author
        /// </summary>
        public string UpdateAuthor { get; set; }

        /// <summary>
        /// Generating insert SQL for store information of update success into table DbSyncerInfo
        /// </summary>
        /// <returns></returns>
        public string ToInsertSql()
        {
            return string.Format(SQLScripts.DbSyncerInfoInsert, DbSyncerInfoContextID, string.Format("convert(datetime, '{0}', 120)", ScriptDate.ToString("yyyy-MM-dd HH:mm:ss")), string.Format("convert(datetime, '{0}', 120)", UpdateDate.ToString("yyyy-MM-dd HH:mm:ss")), UpdateAuthor);
        }

        public string ToDeleteSql()
        {
            return string.Format(SQLScripts.DbSyncerInfoDelete, DbSyncerInfoContextID, string.Format("convert(datetime, '{0}', 120)", ScriptDate.ToString("yyyy-MM-dd HH:mm:ss")), string.Format("convert(datetime, '{0}', 120)", UpdateDate.ToString("yyyy-MM-dd HH:mm:ss")), UpdateAuthor);
        }

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(@"dbsync_{0}_{1}.sql", UpdateDate.ToString("yyyy-MM-dd_HH-mm"), UpdateAuthor);
        }
    }
}
