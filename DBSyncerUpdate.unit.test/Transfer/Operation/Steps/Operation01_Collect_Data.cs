using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using gip.core.dbsyncer;
using gip.core.dbsyncer.helper;
using gip.core.dbsyncer.model;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using modelOld = DBSyncerUpdate.unit.test.DBSyncerOldModel;
using gip.core.datamodel;
using System.Runtime.CompilerServices;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Steps
{
    public class Operation01_Collect_Data : OperationBase
    {

        #region Const definitions

        public const string NewFileNameTemplate = @"dbsync_{0}_{1}.sql";
        public static readonly DateTime OperatingTime = new DateTime(2018, 8, 27, 0, 0, 0);

        #endregion

        #region ctor's

        public Operation01_Collect_Data(TransferJob transferJob) : base(transferJob)
        {

        }


        #endregion

        #region OperationBase overrides

        public override string OperationName
        {
            get
            {
                return @"Operation01_Collect_Data";
            }
        }

        public override bool PrepareData()
        {
            bool successPrepareData = true;
            try
            {
                List<modelOld.DbSyncerInfo> dbFileList = GetAllDBDbSyncerInfo();
                foreach (var operationContext in TransferJob.OperationContexts)
                    PrepareOperationContext(operationContext, dbFileList);
            }
            catch (Exception ec)
            {
                successPrepareData = false;
                TransferJob.RegisterException(OperationName, ec);
            }
            return successPrepareData;
        }

        public override bool DoJob()
        {
            return true;
        }

        #endregion

        #region Private methods

        private void PrepareOperationContext(OperationContext operationContext, List<modelOld.DbSyncerInfo> dbFileList)
        {
            foreach (OperationFile operationFile in operationContext.OperationFiles)
            {
                operationFile.DataContextID = operationContext.DataContextID;
                PrepareOperationFile(operationFile, operationContext, dbFileList);
            }
        }

        private void PrepareOperationFile(OperationFile operationFile, OperationContext operationContext, List<modelOld.DbSyncerInfo> dbFileList)
        {
            UpdateSettings updateSettings = new UpdateSettings();
            string fullFileName = operationFile.GetFullFileName(operationContext.ContextFolders[0]);
            if (dbFileList.Any())
                operationFile.OldDbSyncerInfo = dbFileList.FirstOrDefault(c => c.DbSyncerInfoContextID == operationContext.DataContextID && c.Version == operationFile.Version && c.UpdateAuthor == operationFile.UpdateAuthor);
            if (File.Exists(fullFileName))
                operationFile.ScriptFileInfo = new modelOld.ScriptFileInfo(null, new FileInfo(fullFileName), updateSettings.RootFolder);
            operationFile.NewDbSyncerInfo = new gip.core.dbsyncer.model.DbSyncerInfo()
            {
                ScriptDate = operationFile.UsedScriptDate,
                DbSyncerInfoContextID = operationContext.DataContextID,
                UpdateAuthor = operationFile.UpdateAuthor,
                UpdateDate = DateTime.Now
            };
        }

        private static List<modelOld.DbSyncerInfo> GetAllDBDbSyncerInfo()
        {
            List<modelOld.DbSyncerInfo> list = new List<modelOld.DbSyncerInfo>();
            using (Database db = new Database(DbSyncerSettings.GetDefaultConnectionString(DbSyncerSettings.ConfigCurrentDir)))
            {
                list = db.Database.SqlQuery<modelOld.DbSyncerInfo>(FormattableStringFactory.Create(SQLScripts.AllDbSyncerInfo)).ToList<modelOld.DbSyncerInfo>();
            }
            return list;
        }

        #endregion

    }
}
