using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using System;
using System.IO;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Steps
{
    public class Operation03_Update_csProj_Files : OperationBase
    {

        #region ctor's

        public Operation03_Update_csProj_Files(TransferJob transferJob) : base(transferJob)
        {

        }

        #endregion

        #region OperationBase overrides

        public override string OperationName
        {
            get
            {
                return @"Operation03_Update_csProj_Files";
            }
        }

        public override bool PrepareData()
        {
            bool successPrepareData = true;
            if (TransferJob.IsClientUpdate || !TransferJob.IsUpdateProjectFile) return successPrepareData;
            try
            {
                foreach (OperationContext operationContext in TransferJob.OperationContexts)
                    PrepareContextData(operationContext);
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
                if (TransferJob.IsUpdateProjectFile && !TransferJob.IsClientUpdate)
                    foreach (OperationContext operationContext in TransferJob.OperationContexts)
                        File.WriteAllText(operationContext.ContextProjectFile, operationContext.CSProjRenameModel.NewContent);
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

        private void PrepareContextData(OperationContext operationContext)
        {
            CSProjRenameModel renameModel = new CSProjRenameModel();
            renameModel.OldContent = File.ReadAllText(operationContext.ContextProjectFile);
            renameModel.NewContent = renameModel.OldContent;
            operationContext.CSProjRenameModel = renameModel;

            foreach (OperationFile operationFile in operationContext.OperationFiles)
            {
                renameModel.NewContent = renameModel.NewContent.Replace(operationFile.Name, operationFile.NewDbSyncerInfo.ToString());
            }
        }

        #endregion

    }
}
