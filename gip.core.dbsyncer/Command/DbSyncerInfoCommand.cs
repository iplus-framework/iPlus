// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using gip.core.dbsyncer.helper;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using gip.core.datamodel;
using gip.core.dbsyncer.model;

namespace gip.core.dbsyncer.Command
{

    // ***********************************************************************
    // Assembly         : gip.core.dbsyncer
    // Author           : Aagincic
    // Created          : 2013-10-13 10:20
    //
    // Last Modified By : Aagincic
    // Last Modified On : 2013-10-13
    // ***********************************************************************
    // <copyright file="LicenceCreator.cs" company="gip mbh, Oftersheim, Germany">
    //     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
    // </copyright>
    // <summary></summary>
    // ***********************************************************************
    /// <summary>
    /// Make Variobacth database up to date
    /// </summary>
    public static class DbSyncerInfoCommand
    {

        #region File informations
        /// <summary>
        /// Generate list of available files
        /// </summary>
        /// <returns></returns>
        public static List<ScriptFileInfo> FileAvailableVersions(gip.core.datamodel.DbSyncerInfoContext dbInfoContext, string rootFolder)
        {
            DirectoryInfo scriptDir = new DirectoryInfo(DbSyncerSettings.GetScriptFolderPath(dbInfoContext.DbSyncerInfoContextID, rootFolder));
            List<FileInfo> fileNames = scriptDir.GetFiles().ToList().Where(x => x.Extension.ToLower() == ".sql" && x.Name.StartsWith("dbsync_")).OrderBy(c => c.Name).ToList();
            List<ScriptFileInfo> files = fileNames.Select(x => new ScriptFileInfo(dbInfoContext, x, scriptDir.FullName)).ToList();
            return files;
        }

        #endregion end file informatons

        #region Database informatons

        /// <summary>
        /// Check version into database
        /// </summary>
        /// <returns></returns>
        public static DateTime? DatabaseMaxScriptDate(DbContext db, gip.core.datamodel.DbSyncerInfoContext dbInfoContext)
        {
            string sql = string.Format(SQLScripts.MaxScriptDate, dbInfoContext.DbSyncerInfoContextID.Trim());
            return db.Database.SqlQuery<DateTime>(FormattableStringFactory.Create(sql)).ToArray().FirstOrDefault<DateTime>();
        }
        #endregion database informations

        #region Execution

        /// <summary>
        /// Proceed with update
        /// </summary>
        /// <param name="updateFiles"></param>
        public static void Update(DbContext db, ScriptFileInfo updateFile)
        {
            string prepraredSQL = null;
            try
            {
                string sqlContent = updateFile.GetSqlContent();
                UpdateSqlContent(db, sqlContent, ref prepraredSQL);
                if (updateFile.FileName != "InitialScript.sql")
                {
                    gip.core.dbsyncer.model.DbSyncerInfo dbInfo = updateFile.GetDbInfo();
                    db.Database.ExecuteSql(FormattableStringFactory.Create(dbInfo.ToInsertSql()));
                }
            }
            catch (SqlException sqlException)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error by executing script!");
                sb.AppendLine(string.Format(@"Context: {0}", updateFile.Context.DbSyncerInfoContextID));
                sb.AppendLine(string.Format(@"File: {0}", updateFile.FileName));
                sb.AppendLine(string.Format(@"Line nr: {0}", sqlException.LineNumber));
                sb.AppendLine("Error:");
                sb.AppendLine(sqlException.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(prepraredSQL);
                throw new Exception(sb.ToString(), sqlException);
            }
            catch (Exception ec)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error by executing script!");
                sb.AppendLine(string.Format(@"Context: {0}", updateFile.Context.DbSyncerInfoContextID));
                sb.AppendLine(string.Format(@"File: {0}", updateFile.FileName));
                sb.AppendLine("Error:");
                sb.AppendLine(ec.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(prepraredSQL);
                throw new Exception(sb.ToString(), ec);
            }
        }

        public static void DeleteScriptFile(DbContext db, ScriptFileInfo updateFile)
        {
            string prepraredSQL = null;
            try
            {
                string sqlContent = updateFile.GetSqlContent();
                gip.core.dbsyncer.model.DbSyncerInfo dbInfo = updateFile.GetDbInfo();
                db.Database.ExecuteSql(FormattableStringFactory.Create(dbInfo.ToDeleteSql()));
            }
            catch (SqlException sqlException)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error by executing script!");
                sb.AppendLine(string.Format(@"Context: {0}", updateFile.Context.DbSyncerInfoContextID));
                sb.AppendLine(string.Format(@"File: {0}", updateFile.FileName));
                sb.AppendLine(string.Format(@"Line nr: {0}", sqlException.LineNumber));
                sb.AppendLine("Error:");
                sb.AppendLine(sqlException.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(prepraredSQL);
                throw new Exception(sb.ToString(), sqlException);
            }
            catch (Exception ec)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DBSyncer Error by executing script!");
                sb.AppendLine(string.Format(@"Context: {0}", updateFile.Context.DbSyncerInfoContextID));
                sb.AppendLine(string.Format(@"File: {0}", updateFile.FileName));
                sb.AppendLine("Error:");
                sb.AppendLine(ec.Message);
                sb.AppendLine("SQL:");
                sb.AppendLine(prepraredSQL);
                throw new Exception(sb.ToString(), ec);
            }
        }

        /// <summary>
        /// SQL command execution in details
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sqlContent"></param>
        /// <param name="prepraredSQL"></param>
        public static void UpdateSqlContent(DbContext db, string sqlContent, ref string prepraredSQL)
        {
            string line = null;
            // Updated regex to handle both Windows (\r\n) and Linux (\n) line endings
            List<string> sqlLines = Regex.Split(sqlContent, @"^\s*GO\s*$", RegexOptions.Multiline).ToList();
            foreach (string sql in sqlLines)
            {
                prepraredSQL = sql.Trim(); // Trim to remove any leading/trailing whitespace
                if (string.IsNullOrEmpty(prepraredSQL)) continue;

                List<string> lines = new List<string>();
                using (StringReader reader = new StringReader(prepraredSQL))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("--"))
                        {
                            lines.Add(line);
                        }
                    }
                }

                prepraredSQL = string.Join(Environment.NewLine, lines);
                var oldTimeout = db.Database.GetCommandTimeout;
                db.Database.SetCommandTimeout(60 * 5);

                try
                {
                    if (!string.IsNullOrEmpty(prepraredSQL))
                    {
                        prepraredSQL = prepraredSQL.Replace("{", "{{");
                        prepraredSQL = prepraredSQL.Replace("}", "}}");
                        db.Database.ExecuteSql(FormattableStringFactory.Create(prepraredSQL));
                    }
                }
                finally
                {
                    db.Database.SetCommandTimeout(oldTimeout());
                }
            }
        }



        #endregion
    }
}
