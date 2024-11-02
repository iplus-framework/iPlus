using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using gip.core.dbsyncer.model;
using gip.core.dbsyncer.helper;
using System.Text;

namespace gip.core.dbsyncer.Command
{
    public class DbSyncerInfoContextCommand
    {
        #region Initial script problem
        /// <summary>
        /// Check if shema base exist
        /// </summary>
        /// <returns></returns>
        public static bool IsInitialDbSync(DbContext db)
        {
            bool isInitialDbSync = false;
            try
            {
                db.Database.SqlQuery<DateTime?>(SQLScripts.CheckDbSyncerInfoExist).FirstOrDefault<DateTime?>();
            }
            catch (SqlException cc)
            {
                if (cc.Number != 208) //cc.Message != @"Invalid object name 'dbo.@DbSyncerInfo'."
                {
                    throw new Exception(cc.Message, cc);
                }
                else
                {
                    isInitialDbSync = true;
                }
            }
            return isInitialDbSync;
        }

        public static bool IsOldDBStructurePresent(DbContext db)
        {
            return db.Database.SqlQuery<int?>(SQLScripts.CheckDbSyncerInfoExist_OLD).FirstOrDefault() > 0;
        }

        /// <summary>
        /// Generate a initial script
        /// </summary>
        /// <returns></returns>
        public static ScriptFileInfo InitialScript(string rootFolder)
        {
            DbSyncerInfoContext context = new DbSyncerInfoContext() { DbSyncerInfoContextID = DbSyncerSettings.IPlusContext, ConnectionName = DbSyncerSettings.DefaultConnectionStringName };
            ScriptFileInfo scrInfo = new ScriptFileInfo(context, new FileInfo(DbSyncerSettings.InitialSQLScriptName), rootFolder);
            return scrInfo;
        }

        /// <summary>
        /// Run initial script
        /// </summary>
        public static void RunInitialScript(DbContext db, string rootFolder)
        {
            ScriptFileInfo sf = InitialScript(rootFolder);
            DbSyncerInfoCommand.Update(db, sf);
        }

        public static void RunOldStructureTransformationScript(DbContext db, string rootFolder)
        {
            string fileName = Path.Combine(rootFolder , @"DbSyncerUpdate1.0.0.0.sql");
            string prepraredSQL = null;
            string sqlContent = File.ReadAllText(fileName);
            DbSyncerInfoCommand.UpdateSqlContent(db, sqlContent, ref prepraredSQL);
        }

        #endregion

        #region DBContexts

        public static List<DbSyncerInfoContext> FileContexts(string rootFolder)
        {
            List<DbSyncerInfoContext> list = new List<DbSyncerInfoContext>();
            DirectoryInfo dirInfo = new DirectoryInfo(DbSyncerSettings.GetScriptFolderPath(null, rootFolder));
            List<DirectoryInfo> subDirs = dirInfo.GetDirectories().ToList();
            foreach (DirectoryInfo subDirInfo in subDirs)
            {
                string infoFile = subDirInfo.FullName + @"\info.xml";
                FileStream st = new FileStream(infoFile, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DbSyncerInfoContext));
                DbSyncerInfoContext contextInfo = (DbSyncerInfoContext)xmlSerializer.Deserialize(st);
                list.Add(contextInfo);
                st.Close();
            }
            return list;
        }

        public static List<DbSyncerInfoContext> DatabaseContexts(DbContext db)
        {
            return db.Database.SqlQuery<DbSyncerInfoContext>(SQLScripts.DbSyncerInfoContextSelect).ToList<DbSyncerInfoContext>();
        }

        /// <summary>
        /// Build a list of missing contexts
        /// </summary>
        /// <returns></returns>
        public static List<DbSyncerInfoContext> MissingContexts(DbContext db, string rootFolder)
        {
            List<DbSyncerInfoContext> fileContexts = FileContexts(rootFolder);
            List<DbSyncerInfoContext> dbContexts = DatabaseContexts(db);
            fileContexts.RemoveAll(x => dbContexts.Select(n => n.DbSyncerInfoContextID.Trim()).Contains(x.DbSyncerInfoContextID.Trim()));
            return fileContexts;
        }

        /// <summary>
        /// Inserting new context info into database
        /// </summary>
        /// <param name="dbInfoContext"></param>
        public static void InsertContext(DbContext db, DbSyncerInfoContext dbInfoContext)
        {
            string sql = "";
            try
            {
                sql = string.Format(SQLScripts.DbSyncerInfoContextInsert, dbInfoContext.DbSyncerInfoContextID, dbInfoContext.Name, dbInfoContext.ConnectionName, dbInfoContext.Order);
                db.Database.ExecuteSqlCommand(sql);
            }
            catch (SqlException sqlException)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while inserting new context!");
                sb.AppendLine("Error:");
                sb.AppendLine(sqlException.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(sql);
                throw new Exception(sb.ToString(), sqlException);
            }
            catch (Exception ec)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error while inserting new context!");
                sb.AppendLine("Error:");
                sb.AppendLine(ec.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(sql);
                throw new Exception(sb.ToString(), ec);
            }
        }

        #endregion
    }
}
