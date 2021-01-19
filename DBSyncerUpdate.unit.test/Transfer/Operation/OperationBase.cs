using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using System;

namespace DBSyncerUpdate.unit.test.Transfer.Operation
{
    public abstract class OperationBase
    {
        #region ctor's

        public OperationBase(TransferJob transferJob)
        {
            TransferJob = transferJob;
        }

        #endregion

        #region Properties
        public TransferJob TransferJob { get; set; }

        public virtual string OperationName { get; }

        public bool? SucceessPrepareData { get; set; }
        public bool? SucceessDoJob { get; set; }

        public OperationBase Previous { get; set; }
        public OperationBase Next { get; set; }

        #endregion

        #region basic working methods for define in ancestors

        public virtual bool PrepareData()
        {
            bool successPrepareData = true;
            try
            {
                
            }
            catch (Exception ec)
            {
                successPrepareData = false;
                TransferJob.RegisterException(OperationName, ec);
            }
            return successPrepareData;
        }

        public virtual bool DoJob()
        {
            bool successDoJob = true;
            try
            {

            }
            catch (Exception ec)
            {
                successDoJob = false;
                TransferJob.RegisterException(OperationName, ec);
            }
            return successDoJob;
        }

        #endregion

        #region Executing job method

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool Run()
        {
            SucceessPrepareData = PrepareData();
            if (TransferJob.IsDoJob)
            {
                SucceessDoJob = DoJob();
            }
            bool successOfRun = SucceessPrepareData ?? false && (!TransferJob.IsDoJob || (SucceessDoJob ?? false));
            if (successOfRun && Next != null)
                Next.Run();
            return successOfRun;
        }

        #endregion

    }
}
