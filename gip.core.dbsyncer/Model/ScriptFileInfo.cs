using gip.core.dbsyncer.helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace gip.core.dbsyncer.model
{
    /// <summary>
    /// Collect data about file system files in format:
    /// dbsync-2018-08-23_10-08-aagincic.sql
    /// used for database update
    /// </summary>
    public class ScriptFileInfo
    {

        #region DI
        public string RootFolder { get; set; }
        #endregion

        #region ctor's

        /// <summary>
        /// Constructor parse sql file and prepare data
        /// for use in update process
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fi"></param>
        public ScriptFileInfo(DbSyncerInfoContext context, FileInfo fi)
        {
            Context = context;
            FileName = fi.Name;

            List<string> fileNameParts = FileName.Split('_').ToList();
            if(fileNameParts.Count > 3)
            {
                UpdateAuthor = fileNameParts[fileNameParts.Count - 1].Replace(".sql", "");
                string datePart = fileNameParts[1] + " " + fileNameParts[2];
                ScriptDate = DateTime.ParseExact(datePart, @"yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture);
            }

            UpdateSettings updateSettings = new UpdateSettings();
            RootFolder = DbSyncerSettings.GetScriptFolderPath(Context.DbSyncerInfoContextID, updateSettings.RootFolder);
        }


        public ScriptFileInfo(DbSyncerInfoContext context, FileInfo fi, string rootFolder)
            :this(context,fi)
        {
            RootFolder = rootFolder;
        }

        #endregion

        #region properties

        /// <summary>
        /// Short file name 
        /// </summary>
        public string FileName{ get; set; }

        /// <summary>
        /// Time of script creation parsed from FileName
        /// </summary>
        public DateTime ScriptDate { get; set; }

        /// <summary>
        /// Context reference
        /// </summary>
        public DbSyncerInfoContext Context { get; set; }

        #endregion

        #region helper methods

        /// <summary>
        /// Fetching SQL content from file
        /// </summary>
        /// <returns></returns>
        public string GetSqlContent()
        {
            return File.ReadAllText(FilePath());
        }

        /// <summary>
        /// Build a full file path
        /// </summary>
        /// <returns></returns>
        public string FilePath()
        {
            return RootFolder + @"\" + FileName;
        }

        /// <summary>
        /// Author name parsed from FileName
        /// </summary>
        public string UpdateAuthor { get; set; }

        /// <summary>
        /// Generate DbSyncerInfo for insert into database as 
        /// info of executed db update
        /// </summary>
        /// <returns></returns>
        public DbSyncerInfo GetDbInfo()
        {
            FileInfo fi = new FileInfo(FilePath());
            return new DbSyncerInfo()
            {
                DbSyncerInfoContextID = Context.DbSyncerInfoContextID,
                ScriptDate = ScriptDate,
                UpdateDate = DateTime.Now,
                UpdateAuthor = UpdateAuthor
            };
        }

        #endregion

        #region overrides

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(@"[{0}] {1}", Context.DbSyncerInfoContextID, FileName);
        }

        #endregion

    }
}
