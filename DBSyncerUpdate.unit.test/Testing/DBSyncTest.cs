using DBSyncerUpdate.unit.test.Transfer;
using DBSyncerUpdate.unit.test.Transfer.Operation.Factory;
using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using DBSyncerUpdate.unit.test.Transfer.Operation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DBSyncerUpdate.unit.test.Testing
{
    [TestClass]
    public class DBSyncTest
    {
        // Operation00_Prepare_Input_Json_Data
        // Operation01_Collect_Data
        // Operation02_Rename_Files
        // Operation03_Update_csProj_Files
        // Operation04_SQL_Update

        [TestMethod]
        public void TestAll()
        {
            TransferCommand cmd = new TransferCommand();
            bool transferSuccess = cmd.DoTransfer();
        }

        [TestMethod]
        public void Test00_Operation00_Prepare_Input_Json_Data()
        {
            TransferJob transferJob = FactoryTransferJob.Factory(true);
            Operation00_Prepare_Input_Json_Data operation00 = new Operation00_Prepare_Input_Json_Data(transferJob);
            operation00.Run();
        }

        [TestMethod]
        public void Test01_Operation01_Collect_Data()
        {
            TransferJob transferJob = FactoryTransferJob.Factory(false);
            Operation01_Collect_Data operation01_Collect_Data = new Operation01_Collect_Data(transferJob);
            bool success = operation01_Collect_Data.Run();
            Assert.IsTrue(success);
        }


        [TestMethod]
        public void Test02_Operation02_Rename_Files()
        {

        }


        [TestMethod]
        public void Test03_Operation03_Update_csProj_Files()
        {
            TransferJob transferJob = FactoryTransferJob.Factory(false);

            Operation01_Collect_Data operation01_Collect_Data = new Operation01_Collect_Data(transferJob);
            bool success = operation01_Collect_Data.Run();

            Operation03_Update_csProj_Files operation03_Update_csProj_Files = new Operation03_Update_csProj_Files(transferJob);
            success = success && operation03_Update_csProj_Files.Run();
            Assert.IsTrue(success);
        }


        [TestMethod]
        public void Test04_Operation04_SQL_Update()
        {
            TransferJob transferJob = FactoryTransferJob.Factory(false);

            Operation01_Collect_Data operation01_Collect_Data = new Operation01_Collect_Data(transferJob);
            bool success = operation01_Collect_Data.Run();

            Operation04_SQL_Update operation04_SQL_Update = new Operation04_SQL_Update(transferJob);
            success = success && operation04_SQL_Update.Run();
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void Test05_ReadAssemblyVersion()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(gip.core.dbsyncer.UpdateSettings));
            string version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(
                assembly,
                typeof(AssemblyFileVersionAttribute), false)
            ).Version;
            Assert.IsNotNull(version);
        }


        [TestMethod]
        public void Test06_SearchingForScriptWithSameDate()
        {
            TransferJob transferJob = FactoryTransferJob.Factory(false);
            Operation01_Collect_Data operation01_Collect_Data = new Operation01_Collect_Data(transferJob);
            bool success = operation01_Collect_Data.Run();

            List<OperationFile> files = new List<OperationFile>();
            foreach (var context in transferJob.OperationContexts)
                files.AddRange(context.OperationFiles);

            var query = files
                .GroupBy(c => c.NewDbSyncerInfo.ScriptDate)
                .Where(c => c.Count() > 1)
                .Select(c => new
                {
                    Date = c.Key,
                    Scripts = c.Distinct().Select(v => new {ctx=v.DataContextID, name = v.Name })
                });

            var test = query.ToList();
        }

    }
}
