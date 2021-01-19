using System;
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

        public int LastRevisionNumber
        {
            get;
            set;
        }

        #endregion

        #region Methods

        internal void PublishApplication(UserData currentUserdata, IProgress<string> progress)
        {
            Task.Run( async () => await Publish(currentUserdata, progress));
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

        private async Task Publish(UserData currentUserdata, IProgress<string> progress)
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
                        progress.Report(string.Format("Version with revision number {0} already exist!!!", LastRevisionNumber));
                        break;
                }
                return;
            }

            progress.Report("2) Checking maximum versions on server...");
            CheckMaxVersionsOnServer(currentUserdata);

            progress.Report("3) Archiving bin folder...");
            ZipBinFolder(AppDomain.CurrentDomain.BaseDirectory+"\\"+currentUserdata.ApplicationFileName, currentUserdata.BinFolderPath);

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
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\version.xml"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\version.xml");

            bool fileExistOnFtp = true;
            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                fileExistOnFtp = await client.FileExistsAsync(string.Format("{0}\\{1}\\version.xml", userData.FtpPublishPath, userData.UserDataName));
                if(fileExistOnFtp)
                    await client.DownloadFileAsync(AppDomain.CurrentDomain.BaseDirectory+"\\version.xml", string.Format("{0}\\{1}\\version.xml",userData.FtpPublishPath, userData.UserDataName));
            }
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<Version>));
                    
            if (fileExistOnFtp)
            {
                try
                {
                    using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\version.xml", FileMode.Open))
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

            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\version.xml", FileMode.Create))
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
                await client.CreateDirectoryAsync(string.Format("{0}\\{1}\\{2}", userData.FtpPublishPath, userData.UserDataName, "deploy"));
            }
        }

        private async Task UploadToServer(UserData userData, IProgress<string> progress)
        {
            string folderPath = string.Format("{0}\\{1}\\{2}", userData.FtpPublishPath, userData.UserDataName, "deploy");

            using (FtpClient client = new FtpClient(userData.FtpServerHost, userData.FtpUserName, userData.FtpPassword))
            {
                client.ReadTimeout = 60000;
                client.Connect();
                progress.Report("6) Uploading "+ userData.ApplicationFileName+" file...");
                await client.UploadFileAsync(string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, userData.ApplicationFileName), folderPath + "\\" + userData.ApplicationFileName,
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
                    client.UploadFile(userData.SqlScriptFilePath.Replace("sql","zip"), folderPath + "\\" + userData.DatabaseFileName, FtpExists.Overwrite,
                                      true, FtpVerify.Retry);
                }
                catch (Exception e)
                {
                    progress.Report("Database upload error: "+e.Message);
                    return;
                }

                progress.Report("8) Commit on svn in progress");
                if (VersionList.Any(c => c.SvnRevision == "-rev-"))
                {
                    int revision = CommitOnSvnServer(userData.VersionControlServer, userData.VersionControlDeployFilePath, userData.UserDataName, LastRevisionNumber,
                                                     userData.ChangeLogMessage);

                    if (revision == 0)
                    {
                        progress.Report("Error with commit!!!");
                        return;
                    }

                    VersionList.FirstOrDefault(c => c.SvnRevision == "-rev-").SvnRevision = revision.ToString();

                    using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\version.xml", FileMode.Create))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<Version>));
                        serializer.WriteObject(fs, VersionList);
                    }

                    string newFolderPath = string.Format("{0}\\{1}\\{2}", userData.FtpPublishPath, userData.UserDataName, revision.ToString());
                    await client.MoveDirectoryAsync(folderPath, newFolderPath);
                }
                else
                {
                    progress.Report("Error: problem with versions!!!");
                    return;
                }

                progress.Report("9) Uploading version.xml file...");
                await client.UploadFileAsync(AppDomain.CurrentDomain.BaseDirectory + "version.xml", userData.FtpPublishPath + "\\" + userData.UserDataName+"\\version.xml");
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

        internal bool TryGetLastRevisonNumber(string svnServerPath, out int lastRevisionNo)
        {
            lastRevisionNo = 0;
            using (SvnClient svn = new SvnClient())
            {
                try
                {
                    Uri path = new Uri(svnServerPath);
                    SvnInfoEventArgs args;
                    svn.GetInfo(path, out args);
                    LastRevisionNumber = (int)args.LastChangeRevision;
                    lastRevisionNo = LastRevisionNumber;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return true;
        }

        private int CommitOnSvnServer(string svnServerPath, string deployFilePath, string iPlusVersion, int lastRevisionNo, string changelogMessage)
        {
            PrepareDeployFile(deployFilePath, lastRevisionNo);

            string svnDeployFilePath = string.Format("{0}\\{1}Deploy.txt", svnServerPath, iPlusVersion);
            SvnCommitResult commitResult;
            using (SvnClient svn = new SvnClient())
            {
                try
                {
                    SvnCommitArgs commitArgs = new SvnCommitArgs();
                    commitArgs.LogMessage = "GIP Deploy: "+ Environment.NewLine + changelogMessage;
                    svn.Commit(deployFilePath, commitArgs, out commitResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return 0;
                }
            }
            return (int)commitResult.Revision;
        }

        private void PrepareDeployFile(string deployFilePath, int lastRevisionNumber)
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
                        string folderPath = string.Format("{0}\\{1}\\{2}", userData.FtpPublishPath, userData.UserDataName, folderName);
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
