using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using gip.core.dbsyncer.model;
using gip.core.dbsyncer.helper;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

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
                var result = db.Database.SqlQuery<DateTime>(FormattableStringFactory.Create(SQLScripts.CheckDbSyncerInfoExist)).AsEnumerable<DateTime>();
                if (!result.Any())
                    isInitialDbSync = true;
            }
            catch (SqlException cc)
            {
                if (cc.Number != 208) //cc.Message != @"Invalid object name 'dbo.@DbSyncerInfo'."
                {
                    throw cc;
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
            return db.Database.SqlQuery<int>(FormattableStringFactory.Create(SQLScripts.CheckDbSyncerInfoExist_OLD)).ToArray().FirstOrDefault() > 0;
        }

        /// <summary>
        /// Generate a initial script
        /// </summary>
        /// <returns></returns>
        public static ScriptFileInfo InitialScript(string rootFolder)
        {
            gip.core.datamodel.DbSyncerInfoContext context = new gip.core.datamodel.DbSyncerInfoContext() { DbSyncerInfoContextID = DbSyncerSettings.IPlusContext, ConnectionName = DbSyncerSettings.DefaultConnectionStringName };
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

        public static List<gip.core.dbsyncer.model.DbSyncerInfoContext> FileContexts(string rootFolder)
        {
            List<gip.core.dbsyncer.model.DbSyncerInfoContext> list = new List<gip.core.dbsyncer.model.DbSyncerInfoContext>();
            DirectoryInfo dirInfo = new DirectoryInfo(DbSyncerSettings.GetScriptFolderPath(null, rootFolder));
            List<DirectoryInfo> subDirs = dirInfo.GetDirectories().ToList();
            foreach (DirectoryInfo subDirInfo in subDirs)
            {
                string infoFile = subDirInfo.FullName + @"\info.xml";
                FileStream st = new FileStream(infoFile, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(gip.core.dbsyncer.model.DbSyncerInfoContext));
                gip.core.dbsyncer.model.DbSyncerInfoContext contextInfo = (gip.core.dbsyncer.model.DbSyncerInfoContext)xmlSerializer.Deserialize(st);
                list.Add(contextInfo);
                st.Close();
            }
            return list;
        }

        public static List<gip.core.datamodel.DbSyncerInfoContext> DatabaseContexts(gip.core.datamodel.Database db)
        {
            return db.DbSyncerInfoContext.FromSql<gip.core.datamodel.DbSyncerInfoContext>(FormattableStringFactory.Create(SQLScripts.DbSyncerInfoContextSelect)).ToList<gip.core.datamodel.DbSyncerInfoContext>();
        }

        /// <summary>
        /// Build a list of missing contexts
        /// </summary>
        /// <returns></returns>
        public static List<gip.core.dbsyncer.model.DbSyncerInfoContext> MissingContexts(gip.core.datamodel.Database db, string rootFolder)
        {
            List<gip.core.dbsyncer.model.DbSyncerInfoContext> fileContexts = FileContexts(rootFolder);
            List<gip.core.datamodel.DbSyncerInfoContext> dbContexts = DatabaseContexts(db);
            fileContexts.RemoveAll(x => dbContexts.Select(n => n.DbSyncerInfoContextID.Trim()).Contains(x.DbSyncerInfoContextID.Trim()));
            return fileContexts;
        }

        /// <summary>
        /// Inserting new context info into database
        /// </summary>
        /// <param name="dbInfoContext"></param>
        public static void InsertContext(gip.core.datamodel.Database db, gip.core.dbsyncer.model.DbSyncerInfoContext dbInfoContext)
        {
            string sql = "";
            try
            {
                sql = string.Format(SQLScripts.DbSyncerInfoContextInsert, dbInfoContext.DbSyncerInfoContextID, dbInfoContext.Name, dbInfoContext.ConnectionName, dbInfoContext.Order);
                db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
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
