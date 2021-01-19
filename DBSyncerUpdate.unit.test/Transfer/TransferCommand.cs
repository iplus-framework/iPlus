using DBSyncerUpdate.unit.test.Transfer.Operation.Factory;
using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using DBSyncerUpdate.unit.test.Transfer.Operation.Steps;

namespace DBSyncerUpdate.unit.test.Transfer
{
    /// <summary>
    /// Building a transfer job definition
    /// </summary>
    public class TransferCommand
    {

        public bool DoTransfer()
        {

            TransferJob transferJob = FactoryTransferJob.Factory(false);
            if(transferJob == null)
            {
                transferJob = FactoryTransferJob.Factory(true);
            }

            Operation00_Prepare_Input_Json_Data operation00 = new Operation00_Prepare_Input_Json_Data(transferJob);

            Operation01_Collect_Data operation01 = new Operation01_Collect_Data(transferJob);
            operation00.Next = operation01;
            operation01.Previous = operation00;

            Operation02_Rename_Files operation02 = new Operation02_Rename_Files(transferJob);
            operation02.Previous = operation01;
            operation01.Next = operation02;

            Operation03_Update_csProj_Files operation03 = new Operation03_Update_csProj_Files(transferJob);
            operation03.Previous = operation02;
            operation02.Next = operation03;

            Operation04_SQL_Update operation04 = new Operation04_SQL_Update(transferJob);
            operation03.Next = operation04;

            return operation00.Run();

        }
       
    }
}
