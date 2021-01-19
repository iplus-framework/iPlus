using DBSyncerUpdate.unit.test.Transfer.Operation.Factory;
using DBSyncerUpdate.unit.test.Transfer.Operation.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Steps
{
    public class Operation00_Prepare_Input_Json_Data : OperationBase
    {

        #region constants

        // file name:   dbsync-0014-aagincic.sql
        public static string PatternVersion = @"\d\d\d\d";
        public static string PatternDateInDump = @"201[1-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-9][0-9]:[0-9][0-9]";
        public static string PatternAuthor = @"dbsync-\d\d\d\d-(\w*)\.sql";

        #endregion

        #region ctor's

        public Operation00_Prepare_Input_Json_Data(TransferJob transferJob) : base(transferJob)
        {

        }

        #endregion

        #region OperationBase overrides

        public override string OperationName
        {
            get
            {
                return @"Operation00_Prepare_Input_Json_Data";
            }
        }

        public override bool PrepareData()
        {
            bool successPrepareData = true;
            try
            {
                if (TransferJob.IsBuildTransferJobJson)
                {
                    for (int i = 0; i < TransferJob.OperationContexts.Count(); i++)
                    {
                        Task<OperationContext> task = FactoryContext(TransferJob.OperationContexts[i]);
                        task.Wait();
                        TransferJob.OperationContexts[i] = task.Result;
                    }
                    // Define UsedScriptDate - add shift in minutes for scripts they have same svn update date
                    List<OperationFile> files = new List<OperationFile>();
                    foreach (var context in TransferJob.OperationContexts)
                        files.AddRange(context.OperationFiles);

                    var querySelectFilesWithDuplicateScriptDate = files
                    .GroupBy(c => c.UsedScriptDate)
                    .Where(c => c.Count() > 1)
                    .Select(c => new
                    {
                        Date = c.Key,
                        Scripts = c.Distinct().Select(v => new { ctx = v.DataContextID, name = v.Name, file = v }).OrderBy(a => a.ctx).ThenBy(a => a.name)
                    });

                    if (querySelectFilesWithDuplicateScriptDate.Any())
                    {
                        int addMinutes = 0;
                        foreach (var item in querySelectFilesWithDuplicateScriptDate)
                        {
                            foreach (var file in item.Scripts)
                            {
                                file.file.UsedScriptDate = file.file.UsedScriptDate.AddMinutes(addMinutes);
                                addMinutes++;
                            }
                            addMinutes = 0;
                        }
                    }

                    var checkQuery = files
                    .GroupBy(c => c.UsedScriptDate)
                    .Where(c => c.Count() > 1)
                    .Select(c => new
                    {
                        Date = c.Key,
                        Scripts = c.Distinct().Select(v => new { ctx = v.DataContextID, name = v.Name, file = v }).OrderBy(a => a.ctx).ThenBy(a => a.name)
                    });

                    if (checkQuery.Any())
                        throw new Exception("Fix of UsedScriptDate not successfully");
                }
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
            bool successDoJob = true;
            try
            {
                if (TransferJob.IsBuildTransferJobJson)
                    FactoryTransferJob.Write(TransferJob);
            }
            catch (Exception ec)
            {
                successDoJob = false;
                TransferJob.RegisterException(OperationName, ec);
            }
            return successDoJob;
        }
        #endregion

        #region Private methods

        private static async Task<OperationContext> FactoryContext(OperationContext operationContext)
        {
            string folderPath = operationContext.ContextFolders[0];
            var listFiles = Directory.GetFiles(folderPath, "*.sql").Where(c => !c.Contains("InitialScript"));
            operationContext.OperationFiles = new List<OperationFile>();
            foreach (var fullFileName in listFiles)
            {
                OperationFile operationFile = new OperationFile() { DataContextID = operationContext.DataContextID };
                FileInfo fi = new FileInfo(fullFileName);
                operationFile.Name = fi.Name;
                Match mt = Regex.Match(operationFile.Name, PatternVersion);
                operationFile.Version = int.Parse(mt.Groups[0].Value);
                Match authorMatch = Regex.Match(operationFile.Name, PatternAuthor);
                operationFile.UpdateAuthor = authorMatch.Groups[1].Value;
                KeyValuePair<string, List<DateTime>> readedSVNData = await ReadSVNData(fullFileName);
                operationFile.DumpOutput = readedSVNData.Key;
                if (readedSVNData.Value != null && readedSVNData.Value.Any())
                {
                    operationFile.MinDate = readedSVNData.Value.Min();
                    operationFile.MaxDate = readedSVNData.Value.Max();
                }
                operationFile.UsedScriptDate = operationFile.MinDate;
                operationContext.OperationFiles.Add(operationFile);
            }
            return operationContext;
        }

        private static async Task<KeyValuePair<string, List<DateTime>>> ReadSVNData(string fullFileName)
        {
            int countTry = 0;
            string dump = "";
            bool isValid = false;
            List<DateTime> dateTimes = new List<DateTime>();
            while (countTry < 3 && !isValid)
            {
                dump = ReadDump(fullFileName);
                dateTimes = GetDates(dump);
                countTry++;
                isValid = !dateTimes.Any(c => c == new DateTime());
            }
            return new KeyValuePair<string, List<DateTime>>(dump, dateTimes);
        }

        private static string ReadDump(string fullFileName)
        {
            StringBuilder sb = new StringBuilder();
            var p = new Process();
            p.StartInfo.FileName = "svn.exe";
            p.StartInfo.Arguments = @"log " + fullFileName;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.OutputDataReceived += (a, b) => sb.Append(b.Data);
            p.ErrorDataReceived += (a, b) => Console.WriteLine(b.Data);
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            return sb.ToString();
        }

        private static List<DateTime> GetDates(string dump)
        {
            List<DateTime> dates = new List<System.DateTime>();
            StringReader reader = new StringReader(dump);
            String input;
            while ((input = reader.ReadLine()) != null)
            {
                Match mt = Regex.Match(input, PatternDateInDump);
                if (mt.Success)
                {
                    DateTime dt = DateTime.Parse(mt.Groups[0].Value);
                    dates.Add(dt);
                }
            }
            return dates;
        }


        #endregion
    }
}
