using gip.core.dbsyncer.helper;
using gip.core.dbsyncer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace gip.core.dbsyncer.Command
{
    public class DBSyncerVersionCommand
    {
        public static string GetLatestVersion(DbContext db)
        {
            string version = "";
            try
            {
                List<DBSyncerVersion> versionList = db.Database.SqlQuery<DBSyncerVersion>(FormattableStringFactory.Create(SQLScripts.DBSyncerVersionSelect)).ToList<DBSyncerVersion>();
                if (versionList != null && versionList.Any())
                    version = versionList.OrderByDescending(c => c.UpdateDate).FirstOrDefault().Version;
            }
            catch (Exception cc)
            {
                var test = cc; // do nothing
                CreateVersionTable(db);
                return GetLatestVersion(db);
            }
            return version;
        }

        public static void SetVersion(DbContext db, string version)
        {
            string sql = string.Format(SQLScripts.DBSyncerVersionInsert, version);
            try
            {
                db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
            }
            catch (SqlException sqlException)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while inserting new version!");
                sb.AppendLine("Error:");
                sb.AppendLine(sqlException.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(sql);
                throw new Exception(sb.ToString(), sqlException);
            }
            catch (Exception ec)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while inserting new version!");
                sb.AppendLine("Error:");
                sb.AppendLine(ec.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(sql);
                throw new Exception(sb.ToString(), ec);
            }
        }

        private static void CreateVersionTable(DbContext db)
        {
            string preparedSQL = "";
            try
            {
                DbSyncerInfoCommand.UpdateSqlContent(db, SQLScripts.DBSyncerVersionCreate, ref preparedSQL);
            }
            catch (SqlException sqlException)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while creating new version-table!");
                sb.AppendLine("Error:");
                sb.AppendLine(sqlException.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(preparedSQL);
                throw new Exception(sb.ToString(), sqlException);
            }
            catch (Exception ec)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while inserting new version-table!");
                sb.AppendLine("Error:");
                sb.AppendLine(ec.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(preparedSQL);
                throw new Exception(sb.ToString(), ec);
            }
        }
    }
}
