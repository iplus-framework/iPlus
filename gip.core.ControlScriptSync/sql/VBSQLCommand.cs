using gip.core.ControlScriptSync.VBSettings;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

namespace gip.core.ControlScriptSync.sql
{
    /// <summary>
    /// Command object for database information - store update information into database and 
    /// search last (maximal) VersionTime
    /// </summary>
    public class VBSQLCommand
    {
       

        #region ctor's

        public VBSQLCommand(string connectionString)
        {
            ConnectionString = connectionString;
        }

        #endregion

        public string ConnectionStringRemoveEntityPart(string connString)
        {
            string regex = @"connection string=(.*)Framework";
            return Regex.Match(connString, regex).Groups[0].Value.Replace(@"connection string=""", "");
        }

        public string _ConnectionString;

        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }

        

        public void Create()
        {
            string sql = VBSQLResource.Create;
            using (DbContext db = new DbContext(ConnectionString))
            {
                db.Database.ExecuteSqlCommand(sql);
            }
        }

        public ControlScriptSyncInfo MaxVersion()
        {
            ControlScriptSyncInfo maxVersion = null;
            string sql = VBSQLResource.MaxVersion;
            try
            {
                using (DbContext db = new DbContext(ConnectionString))
                {
                    var query = db.Database.SqlQuery<ControlScriptSyncInfo>(sql);
                    // NOTE: aagincic: Exception of first ran is usual because table VBControlScriptInfo not exist
                    // TODO: aagincic: change this behavior to excecute without exception on first run
                    if(query.Any())
                        maxVersion = query.First();
                }
            }
            catch (Exception e)
            {
                Create();
                maxVersion = MaxVersion();

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBSQLCommand", "MaxVersion", msg);
            }
            return maxVersion;
        }


        public List<ControlScriptSyncInfo> AllVersions()
        {
            List<ControlScriptSyncInfo> allVersions = null;
            string sql = VBSQLResource.AllVersions;
            try
            {
                using (DbContext db = new DbContext(ConnectionString))
                {
                    var query = db.Database.SqlQuery<ControlScriptSyncInfo>(sql);
                    if (query.Any())
                        allVersions = query.ToList();
                }
            }
            catch (Exception e)
            {
                Create();
                allVersions = AllVersions();

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBSQLCommand", "AllVersions", msg);
            }
            return allVersions;
        }

        public void Insert(DateTime version, string updateAuthor)
        {
            string sql = VBSQLResource.Insert;
            sql = string.Format(sql, string.Format("convert(datetime, '{0}', 120)",version.ToString(ControlSyncSettings.DateSQLFormat)), string.Format("convert(datetime, '{0}', 120)",DateTime.Now.ToString(ControlSyncSettings.DateSQLFormat)), updateAuthor);
            using (DbContext db = new DbContext(ConnectionString))
            {
                db.Database.ExecuteSqlCommand(sql);
            }
        }

        public bool ExistVersion(DateTime version)
        {
            bool existversion = false;
            string sql = VBSQLResource.ExistVersion;
            sql = string.Format(sql, string.Format("convert(datetime, '{0}', 120)", version.ToString(ControlSyncSettings.DateSQLFormat)), string.Format("convert(datetime, '{0}', 120)", DateTime.Now.ToString(ControlSyncSettings.DateSQLFormat)));
            using (DbContext db = new DbContext(ConnectionString))
            {
                DbRawSqlQuery<int> result = db.Database.SqlQuery<int>(sql);
                int count = result.FirstOrDefault();
                existversion = count > 0;
            }
            return existversion;
        }
    }
}
