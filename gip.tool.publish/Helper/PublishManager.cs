// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using FluentFTP;
using System.Threading;
using System.Xml;
using SharpSvn;
using System.Runtime.Serialization;
using System.Management.Automation;

namespace gip.tool.publish
{
    public class PublishManager
    {
        #region Properties

        public List<Version> VersionList
        {
            get;
            set;
        }

        public string LastRevisionNumber
        {
            get;
            set;
        }

        #endregion

        #region Methods

        internal void PublishApplication(UserData currentUserdata, IProgress<string> progress, string passToClear)
        {
            Task.Run( async () => await Publish(currentUserdata, progress, passToClear));
        }

        private void ZipDatabaseScript(string sqlScriptPath)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(sqlScriptPath, "");
                zip.Save(sqlScriptPath.Replace("sql", "zip"));
            }
        }

        private void ZipBinFolder(string zipFilePath, string binDirPath)
        {
            using(ZipFile zip = new ZipFile())
            {
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);
                zip.AddDirectory(binDirPath);
                zip.Save(zipFilePath);
            }
        }

        private void ClearPasswords(string binDirPath, string passToClear)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(binDirPath);
                var configs = dirInfo.EnumerateFiles("*.config", SearchOption.AllDirectories);

                foreach (var config in configs)
                {
                    string fileContent = File.ReadAllText(config.FullName);
                    if (fileContent.Contains(passToClear))
                    {
                        fileContent = fileContent.Replace(passToClear, "password");
                        File.WriteAllText(config.FullName, fileContent);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task Publish(UserData currentUserdata, IProgress<string> progress, string passToClear)
        {
            progress.Report("1) Downloading and preparing version...");
            var versionResult = await DownloadVersion(currentUserdata);
            if(versionResult != DownloadVersionState.Ok)
            {
                switch(versionResult)
                {
                    case DownloadVersionState.DownloadFail:
                        progress.Report("Can't download version!!!");
                        break;
                    case DownloadVersionState.DeseralizationFail:
                        progress.Report("Can't deserialize version!!!");
                        break;
                    case DownloadVersionState.SameVersionExist:
                        progress.Report(string.Format("Version with revision ID {0} already exist!!!", LastRevisionNumber));
                        break;
                }
                return;
            }

            progress.Report("2) Checking maximum versions on server...");
            CheckMaxVersionsOnServer(currentUserdata);

            if(!string.IsNullOrEmpty(passToClear))
                ClearPasswords(currentUserdata.BinFolderPath, passToClear);

            progress.Report("3) Archiving bin folder...");
            ZipBinFolder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentUserdata.ApplicationFileName), currentUserdata.BinFolderPath);

            progress.Report("4) Archiving database sql script...");
            ZipDatabaseScript(currentUserdata.SqlScriptFilePath);
            //BackupDatabase(new SqlConnectionInfo(currentUserdata.SqlServerName, currentUserdata.DatabaseName, currentUserdata.AuthenticationMode, 
            //                                     currentUserdata.DBUserName, currentUserdata.DBPass), currentUserdata.DatabaseFileName);

            progress.Report("5) Creating folder deploy ...");
            await CreateFolderOnFtp(currentUserdata);

            await UploadToServer(currentUserdata, progress);
        }

        private async Task<DownloadVersionState> DownloadVersion(UserData userData)
        {
            string versionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.xml");
            if (File.Exists(versionFilePath))
                File.Delete(versionFilePath);

            bool fileExistOnFtp = true;
            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                fileExistOnFtp = await client.FileExistsAsync(Path.Combine(userData.FtpPublishPath, userData.UserDataName, "version.xml"));
                if(fileExistOnFtp)
                    await client.DownloadFileAsync(versionFilePath, Path.Combine(userData.FtpPublishPath, userData.UserDataName, "version.xml"));
            }
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<Version>));
                    
            if (fileExistOnFtp)
            {
                try
                {
                    using (FileStream fs = new FileStream(versionFilePath, FileMode.Open))
                    {
                        try
                        {
                            VersionList = serializer.ReadObject(fs) as List<Version>;
                            if (VersionList.Any(c => c.SvnRevision == LastRevisionNumber.ToString()))
                                return DownloadVersionState.SameVersionExist;
                        }
                        catch
                        {
                            return DownloadVersionState.DeseralizationFail;
                        }
                    }
                }
                catch
                {
                    return DownloadVersionState.DownloadFail;
                }
            }
            else
            {
                VersionList = new List<Version>();
            }

            VersionList.Add(new Version()
            {
                ChangeLogEn = userData.ChangeLogMessage,
                ChangeLogDe = userData.ChangeLogMessageDE,
                SvnRevision = "-rev-",
                ApplicationFileName = userData.ApplicationFileName,
                DatabaseFileName = userData.DatabaseFileName,
                PublishDateTime = DateTime.Now
            });

            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.xml"), FileMode.Create))
            {
                serializer.WriteObject(fs, VersionList);
            }

            serializer = null;

            return DownloadVersionState.Ok;
        }

        private async Task CreateFolderOnFtp(UserData userData)
        {
            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                await client.CreateDirectoryAsync(Path.Combine(userData.FtpPublishPath, userData.UserDataName, "deploy"));
            }
        }

        private async Task UploadToServer(UserData userData, IProgress<string> progress)
        {
            string folderPath = Path.Combine(userData.FtpPublishPath, userData.UserDataName, "deploy");

            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                client.ReadTimeout = 60000;
                client.Connect();
                progress.Report("6) Uploading "+ userData.ApplicationFileName+" file...");
                await client.UploadFileAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, userData.ApplicationFileName), Path.Combine(folderPath, userData.ApplicationFileName),
                                                FtpExists.Overwrite, true, FtpVerify.Throw);
            }
            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                client.ReadTimeout = 120000;
                client.DataConnectionReadTimeout = 120000;
                client.DownloadRateLimit = 0;
                client.UploadRateLimit = 0;
                client.RetryAttempts = 2;
                client.Connect();
                progress.Report("7) Uploading "+userData.DatabaseFileName+" file...");
                try
                {
                    client.UploadFile(userData.SqlScriptFilePath.Replace("sql","zip"),  Path.Combine(folderPath, userData.DatabaseFileName), FtpExists.Overwrite,
                                      true, FtpVerify.Retry);
                }
                catch (Exception e)
                {
                    progress.Report("Database upload error: "+e.Message);
                    return;
                }

                progress.Report("8) Commit on version control in progress");
                if (VersionList.Any(c => c.SvnRevision == "-rev-"))
                {
                    string revision = CommitOnSvnServer(userData.VersionControlServer, userData.VersionControlDeployFilePath, userData.UserDataName, LastRevisionNumber,
                                                        userData.ChangeLogMessage, userData.VersionControl);

                    if (revision == "0")
                    {
                        progress.Report("Error with commit!!!");
                        return;
                    }

                    VersionList.FirstOrDefault(c => c.SvnRevision == "-rev-").SvnRevision = revision.ToString();

                    using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.xml"), FileMode.Create))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<Version>));
                        serializer.WriteObject(fs, VersionList);
                    }

                    string newFolderPath = Path.Combine(userData.FtpPublishPath, userData.UserDataName, revision.ToString());
                    await client.MoveDirectoryAsync(folderPath, newFolderPath);
                }
                else
                {
                    progress.Report("Error: problem with versions!!!");
                    return;
                }

                progress.Report("9) Uploading version.xml file...");
                await client.UploadFileAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.xml"), Path.Combine(userData.FtpPublishPath, userData.UserDataName, "version.xml"));
            }
            progress.Report("10) Publishing is completed.");
        }

        internal bool TryConnectToFtpServer(string host, string username, string pass)
        {
            using(FtpClient ftp = new FtpClient(host, username, pass))
            {
                try
                {
                    ftp.Connect();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        internal bool TryGetLastRevisonNumber(string vcServerPath, string deployFilePath, int versionControl)
        {
            if (versionControl == 0)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(deployFilePath);

                    using (PowerShell shell = PowerShell.Create())
                    {
                        shell.AddScript(@"cd '" + fileInfo.Directory.FullName+"'");
                        shell.Invoke();

                        shell.AddScript(@"git show |out-string");
                        LastRevisionNumber = shell.Invoke().FirstOrDefault()?.ToString().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None).FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }

            else if(versionControl == 1)
            {
                using (SvnClient svn = new SvnClient())
                {
                    try
                    {
                        Uri path = new Uri(vcServerPath);
                        SvnInfoEventArgs args;
                        svn.GetInfo(path, out args);
                        LastRevisionNumber = args.LastChangeRevision.ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }

            return true;
        }

        private string CommitOnSvnServer(string serverPath, string deployFilePath, string iPlusVersion, string lastRevisionNo, string changelogMessage, int versionControl)
        {
            PrepareDeployFile(deployFilePath, lastRevisionNo);

            if (versionControl == 0)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(deployFilePath);
                    using (PowerShell shell = PowerShell.Create())
                    {
                        shell.AddScript(@"cd '" + fileInfo.Directory.FullName + "'");
                        shell.Invoke();

                        shell.AddScript("git add '" + fileInfo.Name + "' |out-null");
                        shell.Invoke();

                        shell.AddScript("git commit -m \"" + changelogMessage + "\" |out-null");
                        shell.Invoke();

                        shell.AddScript("git push " + serverPath + " |out-null");
                        shell.Invoke();

                        shell.AddScript(@"git show |out-string");
                        var result = shell.Invoke().FirstOrDefault()?.ToString().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None).FirstOrDefault();
                        return result;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return "0";
                }

            }
            else if (versionControl == 1)
            {
                string svnDeployFilePath = Path.Combine(serverPath, iPlusVersion + "Deploy.txt");
                SvnCommitResult commitResult;
                using (SvnClient svn = new SvnClient())
                {
                    try
                    {
                        SvnCommitArgs commitArgs = new SvnCommitArgs();
                        commitArgs.LogMessage = "GIP Deploy: " + Environment.NewLine + changelogMessage;
                        svn.Commit(deployFilePath, commitArgs, out commitResult);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return "0";
                    }
                }
                return commitResult.Revision.ToString();
            }
            return "0";
        }

        private void PrepareDeployFile(string deployFilePath, string lastRevisionNumber)
        {
            List<string> list = new List<string>();
            list.Add(string.Format("Last Revision: {0} ==> {1}", lastRevisionNumber, DateTime.Now));
            File.AppendAllLines(deployFilePath, list);
        }

        private async void CheckMaxVersionsOnServer(UserData userData)
        {
            if (VersionList == null)
                return;

            string maxVersions = System.Configuration.ConfigurationManager.AppSettings["maxVersionsOnServer"];
            int maxVer = 0;
            if(int.TryParse(maxVersions, out maxVer))
            {
                if(VersionList.Count >= maxVer)
                {
                    using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
                    {
                        client.Connect();
                        Version version = VersionList.FirstOrDefault(c => c.PublishDateTime == VersionList.Min(x => x.PublishDateTime));
                        string folderName = version.SvnRevision;
                        string folderPath = Path.Combine(userData.FtpPublishPath, userData.UserDataName, folderName);
                        await client.DeleteDirectoryAsync(folderPath);
                        VersionList.Remove(version);
                    }

                }
            }
        }

        private void CopyPDBFiles()
        {

        }

        #endregion
    }
}
