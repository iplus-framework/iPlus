using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using gip.core.dbsyncer;
using gip.core.dbsyncer.Command;
using gip.core.dbsyncer.helper;
using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using gip.core.datamodel;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Steps
{
    public class Operation04_SQL_Update : OperationBase
    {
        #region constants
        public const string Update_2_0_0_0 = @"\DbScripts\DbSyncerUpdate2.0.0.0.sql";
        #endregion

        #region ctor's 

        public Operation04_SQL_Update(TransferJob transferJob) : base(transferJob)
        {

        }

        #endregion

        #region OperationBase overrides

        public override string OperationName
        {
            get
            {
                return @"Operation04_SQL_Update";
            }
        }

        public override bool PrepareData()
        {
            bool successPrepareData = true;
            try
            {
                string fullFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Update_2_0_0_0;
                TransferJob.SQLToExecute = File.ReadAllText(fullFilePath);
                TransferJob.SQLToExecute += Environment.NewLine;
                foreach (OperationContext operationContext in TransferJob.OperationContexts)
                    foreach (OperationFile operationFile in operationContext.OperationFiles)
                    {
                        if(operationFile.OldDbSyncerInfo != null)
                        {
                            TransferJob.SQLToExecute += operationFile.NewDbSyncerInfo.ToInsertSql();
                            TransferJob.SQLToExecute += Environment.NewLine;
                        }
                    }
                TransferJob.SQLToExecute = TransferJob.SQLToExecute.TrimEnd(Environment.NewLine.ToCharArray());
                TransferJob.SQLToExecute = TransferJob.SQLToExecute.TrimEnd(("GO" + Environment.NewLine).ToCharArray());
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
            bool successPrepareData = true;
            try
            {
                using (Database db = new Database(DbSyncerSettings.GetDefaultConnectionString(DbSyncerSettings.ConfigCurrentDir)))
                {
                    string prepraredSQL = null;
                    DbSyncerInfoCommand.UpdateSqlContent(db, TransferJob.SQLToExecute, ref prepraredSQL);
                }
            }
            catch (Exception ec)
            {
                successPrepareData = false;
                TransferJob.RegisterException(OperationName, ec);
            }
            return successPrepareData;
        }

        #endregion

        #region private methods

        #endregion
    }
}
