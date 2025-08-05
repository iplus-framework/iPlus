using gip.core.ControlScriptSync.VBSettings;
using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            using (Database db = new Database(ConnectionString))
            {
                db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
            }
        }

        public datamodel.ControlScriptSyncInfo MaxVersion()
        {
            datamodel.ControlScriptSyncInfo maxVersion = null;
            string sql = VBSQLResource.MaxVersion;
            try
            {
                using (Database db = new Database(ConnectionString))
                {
                    var query = db.ControlScriptSyncInfo.FromSql<datamodel.ControlScriptSyncInfo>(FormattableStringFactory.Create(sql));
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


        public List<datamodel.ControlScriptSyncInfo> AllVersions()
        {
            List<datamodel.ControlScriptSyncInfo> allVersions = null;
            string sql = VBSQLResource.AllVersions;
            try
            {
                using (Database db = new Database(ConnectionString))
                {
                    var query = db.ControlScriptSyncInfo.FromSql<datamodel.ControlScriptSyncInfo>(FormattableStringFactory.Create(sql));
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
            using (Database db = new Database(ConnectionString))
            {
                db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
            }
        }

        public bool ExistVersion(DateTime version)
        {
            bool existversion = false;
            string sql = VBSQLResource.ExistVersion;
            sql = string.Format(sql, string.Format("convert(datetime, '{0}', 120)", version.ToString(ControlSyncSettings.DateSQLFormat)), string.Format("convert(datetime, '{0}', 120)", DateTime.Now.ToString(ControlSyncSettings.DateSQLFormat)));
            using (Database db = new Database(ConnectionString))
            {
                IEnumerable<int> result = db.Database.SqlQuery<int>(FormattableStringFactory.Create(sql)).ToArray();
                int count = result.FirstOrDefault();
                existversion = count > 0;
            }
            return existversion;
        }
    }
}
