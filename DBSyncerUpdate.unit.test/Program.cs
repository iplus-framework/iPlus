using DBSyncerUpdate.unit.test.Transfer;
using System;

namespace DBSyncerUpdate.unit.test
{
    public static class Program
    {
        public static void Main()
        {
            TransferCommand cmd = new TransferCommand();
            bool transferSuccess = cmd.DoTransfer();
            Console.WriteLine(transferSuccess);
        }
    }
}
