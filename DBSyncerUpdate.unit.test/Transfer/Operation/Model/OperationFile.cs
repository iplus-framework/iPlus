using gip.core.dbsyncer.model;
using Newtonsoft.Json;
using System;
using System.IO;
using modelOld = DBSyncerUpdate.unit.test.DBSyncerOldModel;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Model
{
    public class OperationFile
    {

        #region OperationFile -> As prepared data from analysis

        public string GetFullFileName(string contextFolder)
        {
            return Path.Combine(contextFolder, Name);
        }


        public string Name { get; set; }

        public int Version { get; set; }

        public string UpdateAuthor { get; set; }

        public DateTime MaxDate { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime UsedScriptDate { get; set; }

        [JsonIgnore]
        public string DumpOutput { get; set; }

        #endregion


        #region OperationData -> Produced Data

        [JsonIgnore]
        public modelOld.ScriptFileInfo ScriptFileInfo { get; set; }

        [JsonIgnore]
        public modelOld.DbSyncerInfo OldDbSyncerInfo { get; set; }

        [JsonIgnore]
        public DbSyncerInfo NewDbSyncerInfo { get; set; }

        [JsonIgnore]
        public string DataContextID { get; set; }

        #endregion

        #region Override basic

        public override string ToString()
        {
            string newContent = "";
            if(NewDbSyncerInfo != null)
            {
                newContent = NewDbSyncerInfo.ToString();
            }
            return string.Format(@"[Current: Name:{0}| Version: {1}| Author: {2}][NewVersion: {3}", Name, Version, UpdateAuthor, newContent);
        }

        #endregion
    }
}
