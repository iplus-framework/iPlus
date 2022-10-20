using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Factory
{
    /// <summary>
    /// Operations with TransferJob json presentation
    /// </summary>
    public class FactoryTransferJob
    {

       /// <summary>
       /// Read regular prepared json file or prepare it
       /// </summary>
       /// <param name="inputJob"></param>
       /// <returns></returns>
        public static TransferJob Factory(bool inputJob = false)
        {
            TransferJob transferJob = null;
            string jsonName = inputJob ? "InputTransferJob" : "TransferJob";
            string jsonContent = ReadEmbbededFile("DBSyncerUpdate.unit.test", jsonName + ".json");
            try
            {
                transferJob = JsonConvert.DeserializeObject<TransferJob>(jsonContent);
            }
            catch (Exception) { }
            return transferJob;
        }

        private static string ReadEmbbededFile(string namespaceName, string filename)
        {
            string result = "";
            var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "MyCompany.MyProduct.MyFile.txt";
            var resourceName = namespaceName + "." + filename;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// Write Builded up TransferJob file to json again
        /// </summary>
        /// <param name="transferJob"></param>
        public static void Write(TransferJob transferJob)
        {
            string fullFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\" + "TransferJob" + ".json";
            string content = JsonConvert.SerializeObject(transferJob);
            transferJob.IsBuildTransferJobJson = false;
            File.WriteAllText(fullFilePath, content);
        }
        
    }
}
