using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using System;
using System.IO;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Steps
{
    public class Operation02_Rename_Files : OperationBase
    {

        #region #ctors
        public Operation02_Rename_Files(TransferJob transferJob) : base(transferJob)
        {

        }

        #endregion

        #region OperationBase overrides

        public override string OperationName
        {
            get
            {
                return @"Operation02_Rename_Files";
            }
        }

        public override bool PrepareData()
        {
            return true;
        }

        public override bool DoJob()
        {
            bool successPrepareData = true;
            if (TransferJob.IsClientUpdate) return true;
            try
            {
                foreach (OperationContext operationContex in TransferJob.OperationContexts)
                {
                    foreach(OperationFile operationFile in operationContex.OperationFiles)
                    {
                        foreach(var dir in operationContex.ContextFolders)
                        {
                            string fullFileName = operationFile.GetFullFileName(operationContex.ContextFolders[0]);
                            if (File.Exists(fullFileName))
                            {
                                string newFullName = Path.Combine(Path.GetDirectoryName(fullFileName), operationFile.NewDbSyncerInfo.ToString());
                                File.Copy(fullFileName, newFullName);
                                File.Delete(fullFileName);

                                string header = string.Format(@"-- old sql file name: {0}", operationFile.Name) + Environment.NewLine;
                                string content = File.ReadAllText(newFullName);
                                File.WriteAllText(newFullName, header + content);
                            }
                        }
                    }
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
    }
}
