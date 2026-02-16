// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Data;
using System.Data.Sql;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using sc = System.Configuration;
using System.ServiceProcess;
using Ionic.Zip;
using rl = IWshRuntimeLibrary;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Reflection;
using System.Xml;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Management;

namespace gip.tool.installerAndUpdater
{
    public class InstallerAndUpdaterManager
    {
        public InstallerAndUpdaterManager(MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
            if (_LoginManager == null)
                _LoginManager = new LoginManager();
        }

        private MainWindow _mainWindow;

        public const string IPlusMES = "IPlusMES";

        private LoginManager _LoginManager;

        public gip.tool.publish.Version AppVersion
        {
            get;
            set;
        }

        public bool Login(string username, string password)
        {
            Task<bool> task = Task.Run(async () => await _LoginManager.LoginAsync(username, password));
            task.Wait();
            return task.Result;
        }

        public bool ReadGipInstall(out string installationPath, out string iPlusVersion)
        {
            installationPath = "";
            iPlusVersion = "";
            string gipInstallPath = System.Environment.ExpandEnvironmentVariables("%LocalAppData%\\gipInstall");
            string gipFileInstallPath = Path.Combine(gipInstallPath, "install.gip");
            if (!File.Exists(gipFileInstallPath))
                return false;

            string xml = File.ReadAllText(gipFileInstallPath);
            if(string.IsNullOrEmpty(xml))
                return false;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNode root = doc.SelectSingleNode("xml");
                installationPath = root.SelectSingleNode("installationFolder").InnerText;
                iPlusVersion = root.SelectSingleNode("iplusVer").InnerText;
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region Installation

        public async void InstallVB(string installationPath, bool desktopShortcut, string iPlusVersion,  IProgress<double> currentOperationProgress, CancellationToken cancelToken,
                                    gip.tool.publish.Version appVersion = null)
        {
            if (!_LoginManager.IsUserLogged)
                return;

            try
            {
                CheckOrCreateInstallationFolder(installationPath);
                SaveCredentials(installationPath, iPlusVersion);

                if (appVersion == null)
                {
                    if (!await DownloadVersion(installationPath, iPlusVersion, currentOperationProgress, cancelToken))
                        return;
                }
                else
                    AppVersion = appVersion;

                await DownloadApp(installationPath, iPlusVersion, currentOperationProgress, cancelToken);
                await DownloadDB(installationPath, iPlusVersion, currentOperationProgress, cancelToken);
                //SaveInstalledFileNames(installationPath);
                if(cancelToken.IsCancellationRequested)
                {
                    _mainWindow._resetEvent.Set();
                    return;
                }

                if (desktopShortcut)
                {
                    _mainWindow.ReportCurrentOperation(5);
                    string ver = iPlusVersion == IPlusMES ? "mes" : iPlusVersion;
                    CreateShortcut(iPlusVersion, System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonDesktopDirectory), 
                                   installationPath, Path.Combine(installationPath, string.Format("gip.{0}.client.exe", ver.ToLower())));
                    int i = 0;
                    while (i != 100)
                        currentOperationProgress.Report(i++);
                }
                else
                {
                    _mainWindow.ReportCurrentOperation(5);
                    currentOperationProgress.Report(99);
                }

                _mainWindow.ReportCurrentOperation(6);
                _mainWindow.IsInstallationFinished = true;
                if (appVersion != null)
                    _mainWindow.CompletedInstallationText = TextResources.TextResource.TbFinishInfoRollback;
            }
            catch (Exception e)
            {
                _mainWindow.ReportError(e.Message);
            }
        }

        public async Task<bool> DownloadVersion(string installationPath, string iPlusVersion, IProgress<double> progress, CancellationToken cancelToken)
        {
            _mainWindow.ReportCurrentOperation(0);
            string downloadMehtodName = "Serve";
            string verDownloadUrl = LoginManager.RemoteServer + string.Format(@"/en/{0}/{1}{2}?name=version.xml", LoginManager.ControllerName, downloadMehtodName, iPlusVersion);
            using (var client = new HttpClientDownloadWithProgress(verDownloadUrl, installationPath + "\\versionList.xml"))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    progress.Report(progressPercentage.Value);
                };
                await client.StartDownload(cancelToken);
            }

            if (cancelToken.IsCancellationRequested)
                return false;

            List<gip.tool.publish.Version> versionList;
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<gip.tool.publish.Version>));
            try
            {
                using (FileStream fs = new FileStream(installationPath + "\\versionList.xml", FileMode.Open))
                {
                    versionList = serializer.ReadObject(fs) as List<gip.tool.publish.Version>;
                }
            }
            catch
            {
                return false;
            }

            serializer = null;

            if(versionList != null)
                AppVersion = versionList.FirstOrDefault(c => c.PublishDateTime.Equals(versionList.Max(x => x.PublishDateTime)));

            if (File.Exists(Path.Combine(installationPath, "versionList.xml")))
                File.Delete(Path.Combine(installationPath, "versionList.xml"));

            try
            {
                serializer = new DataContractSerializer(typeof(gip.tool.publish.Version));
                using (FileStream fs = new FileStream(Path.Combine(installationPath, "version.xml"), FileMode.Create))
                {
                    serializer.WriteObject(fs, AppVersion);
                }
            }
            catch
            {
                return false;
            }

            if (cancelToken.IsCancellationRequested)
                return false;

            return true;
        }

        public async Task DownloadApp(string installationPath, string iPlusVersion, IProgress<double> progress, CancellationToken cancelToken )
        {
            _mainWindow.ReportCurrentOperation(1);
            string downloadMethodName = "Serve";
            string appDownloadUrl = LoginManager.RemoteServer + string.Format(@"/en/{0}/{1}{2}?name={3}\\{4}",LoginManager.ControllerName, 
                                                                              downloadMethodName, iPlusVersion, AppVersion.SvnRevision, AppVersion.ApplicationFileName);
           
            using (var client = new HttpClientDownloadWithProgress(appDownloadUrl, Path.Combine(installationPath, AppVersion.ApplicationFileName)))
            {
                   client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => 
                   {
                       progress.Report(progressPercentage.Value);
                   };
                await client.StartDownload(cancelToken);
            }
            
            if (cancelToken.IsCancellationRequested)
                return;

            _mainWindow.ReportCurrentOperation(2);
            Task unpack = Task.Run(() => UnpackFile(installationPath, AppVersion.ApplicationFileName, cancelToken));
            await unpack;

            CopyFileList(installationPath);
        }

        private void CopyFileList(string installationPath)
        {
            string gipInstallPath = System.Environment.ExpandEnvironmentVariables("%LocalAppData%\\gipInstall");
            if (!Directory.Exists(gipInstallPath))
                Directory.CreateDirectory(gipInstallPath);

            string gipFileInstallPath = Path.Combine(gipInstallPath, "fileList.xml");
            string fileListPath = Path.Combine(installationPath, "fileList.xml");
            if (File.Exists(fileListPath))
            {
                File.Copy(fileListPath, gipFileInstallPath, true);
                File.Delete(fileListPath);
            }
        }

        public async Task DownloadDB(string installationPath, string iPlusVersion, IProgress<double> progress, CancellationToken cancelToken)
        {
            _mainWindow.ReportCurrentOperation(3);
            string downloadMehtodName = "Serve";
            string dbDownloadUrl = LoginManager.RemoteServer + string.Format(@"/en/{0}/{1}{2}?name={3}\\{4}", LoginManager.ControllerName, downloadMehtodName, 
                                                                             iPlusVersion, AppVersion.SvnRevision, AppVersion.DatabaseFileName);

            using (var client = new HttpClientDownloadWithProgress(dbDownloadUrl, Path.Combine(installationPath, AppVersion.DatabaseFileName)))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    progress.Report(progressPercentage.Value);
                };
                await client.StartDownload(cancelToken);
            }
            _mainWindow.ReportCurrentOperation(4);

            if (cancelToken.IsCancellationRequested)
                return;

            Task unpack = Task.Run(() => UnpackFile(installationPath, AppVersion.DatabaseFileName, cancelToken));
            await unpack;
        }

        public async Task DownloadVer(string installationPath, string iPlusVersion)
        {
            string downloadMehtodName = "Serve";
            string appDownloadUrl = LoginManager.RemoteServer + string.Format(@"/en/{0}/{1}{2}?name=version.xml", LoginManager.ControllerName, downloadMehtodName, iPlusVersion);

            using (var client = new HttpClientDownloadWithProgress(appDownloadUrl, Path.Combine(installationPath, "version.xml")))
            {
                await client.StartDownload();
            }
        }

        private void UnpackFile(string installationPath, string fileName, CancellationToken cancelToken)
        {
            using (VBZipFile archive = new VBZipFile(Path.Combine(installationPath, fileName), cancelToken))
            {
                archive.ExtractProgress += archive_ExtractProgress;
                try
                {
                    archive.ExtractAll(installationPath, ExtractExistingFileAction.OverwriteSilently);
                }
                catch
                {
                    _mainWindow.ReportCurrentOperation(10);
                }
            }
            string filePath = Path.Combine(installationPath, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        void archive_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if(sender is VBZipFile)
            {
                VBZipFile zipFile = sender as VBZipFile;
                if (zipFile.CancellationToken.IsCancellationRequested)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (e.EntriesExtracted > 1 && e.EntriesTotal > 1)
            {
                var calc = Math.Round((double)e.EntriesExtracted / e.EntriesTotal * 100, 2);
                switch(_mainWindow.ApplicationMode)
                {
                    case AppMode.Update:
                        _mainWindow.ReportUpdateProgress(calc);
                        break;
                    case AppMode.Install:
                        _mainWindow.ReportCurrentOperationProgress(calc);
                        break;
                }
            }
        }

        public static void SaveCredentials(string installationPath, string iPlusVersion)
        {
            string xml = string.Format("<xml><user>{0}</user><pass>{1}</pass></xml>", LoginManager.Username, LoginManager.Password);
            string fileName = Path.Combine(installationPath, "userInfo.gip");

            byte[] encodedFileName = new UTF8Encoding().GetBytes(fileName);
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(encodedFileName);
            }
            string encXml = Encrypt.EncryptString(xml, hash);
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllText(fileName, encXml);
        }

        public static void CreateShortcut(string shortcutName, string shortcutPath, string installationPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            rl.WshShell shell = new rl.WshShell();
            rl.IWshShortcut shortcut = (rl.IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.Description = "iPlus shortcut";
            //shortcut.IconLocation = @"c:\myicon.ico";           
            shortcut.TargetPath = Path.Combine(installationPath, targetFileLocation)            ;
            shortcut.WorkingDirectory = installationPath;
            shortcut.Save();
        }

        private bool CheckOrCreateInstallationFolder(string installationPath)
        {
            _mainWindow.ReportCurrentOperation(0);
            if(!Directory.Exists(installationPath))
            {
                string[] parts = installationPath.Split(new char[]{Path.DirectorySeparatorChar});
                string tempPath = "";
                foreach(string part in parts)
                {
                    if (string.IsNullOrEmpty(tempPath))     
                        tempPath = part;
                    else
                        tempPath = Path.Combine(tempPath, part);
                    if (!Directory.Exists(tempPath))
                        Directory.CreateDirectory(tempPath);
                }
            }
            return true;
        }

        public async Task<SqlConnectionInfo> CheckSqlServerUserRoleAsync(SqlConnectionInfo connectionInfo)
        {
           return await Task.Run(() => CheckSqlServerUserRoleInternal(connectionInfo));
        }

        private SqlConnectionInfo CheckSqlServerUserRoleInternal(SqlConnectionInfo connectionInfo)
        {
            if (string.IsNullOrEmpty(connectionInfo.ConnectionStrings))
            {
                connectionInfo.ConnectionState = SqlConfigurationState.NoConnection;
                return connectionInfo;
            }

            SqlConfigurationState returnState = SqlConfigurationState.UserNotSysAdmin;
            try
            {
                string binPath = AppDomain.CurrentDomain.BaseDirectory;
                string script = File.ReadAllText(binPath+@"\DatabaseConfiguration\CreateFunctionCheckUserRole.sql");
                string runScript = File.ReadAllText(binPath+@"\DatabaseConfiguration\CheckUserRole.sql");

                SqlConnection conn = new SqlConnection(connectionInfo.ConnectionStrings);
                conn.Open();

                using (SqlCmd cmd = new SqlCmd(conn))
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(script)))
                        cmd.ExecuteStream(ms);
                }

                bool result = false;
                using (SqlCommand comm = new SqlCommand(runScript, conn))
                {
                    var res = comm.ExecuteScalar();
                    if (res is bool)
                        result = (bool)res;
                }

                if (result)
                    returnState = SqlConfigurationState.AllOk;

                conn.Close();
                conn.Dispose();
            }
            catch(SqlException sqlEx)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "excCheckUser.txt"), sqlEx.Message);
                if(sqlEx.Number == 229)
                    returnState = SqlConfigurationState.UserNotSysAdmin;
                else
                    returnState = SqlConfigurationState.NoConnection;
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "excCheckUser.txt"), e.Message);
                returnState = SqlConfigurationState.NoConnection;
            }
            connectionInfo.ConnectionState = returnState;
            return connectionInfo;
        }

        public async Task<bool> SqlConfigureAsync(SqlConnectionInfo connectionInfo, IProgress<int> progress, string installationFolder, string iPlusVersion, CancellationToken cancelToken,
                                                  string dbBackupFileName = null)
        {
            int progressCount = 0;
            if (cancelToken.IsCancellationRequested)
                return false;
            if (string.IsNullOrEmpty(connectionInfo.ConnectionStrings))
            {
                connectionInfo.ConnectionState = SqlConfigurationState.NoConnection;
                return false;
            }
            try
            {
                SqlConnection conn = new SqlConnection(connectionInfo.ConnectionStrings);
                conn.Open();

                if (cancelToken.IsCancellationRequested)
                    return false;
                while (progressCount <= 20)
                {
                    Thread.Sleep(30);
                    progress.Report(progressCount++);
                }

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                if (string.IsNullOrEmpty(dbBackupFileName))
                    dbBackupFileName = Path.Combine(installationFolder, AppVersion.DatabaseFileName.Replace("zip", "bak"));

                string script = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseConfiguration", "DatabaseRestore.sql"));
                script = script.Replace("-location-", dbBackupFileName);
                string dbName = string.Format("{0}V4demo", iPlusVersion);
                script = script.Replace("-databaseName-", dbName);

                while (progressCount <= 30)
                {
                    Thread.Sleep(30);
                    progress.Report(progressCount++);
                }

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);
                await Task.Run(() => RestoreDatabase(conn, script));

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                conn.Close();
                conn.Dispose();

                while (progressCount <= 70)
                {
                    Thread.Sleep(30);
                    progress.Report(progressCount++);
                }

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);
                await Task.Run(() => PrepareAppConfig(connectionInfo, installationFolder, iPlusVersion, dbName));
                while (progressCount <= 100)
                {
                    Thread.Sleep(10);
                    progress.Report(progressCount++);
                }
                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CancelDBConfigOperation(SqlConnection sqlConnection)
        {
            sqlConnection.Close();
            sqlConnection.Dispose();
            return false;
        }

        private bool RestoreDatabase(SqlConnection connection, string script)
        {
            try
            {
                using(SqlCmd sqlCmd = new SqlCmd(connection))
                {
                    using(MemoryStream ms  = new MemoryStream(Encoding.UTF8.GetBytes(script)))
                        sqlCmd.ExecuteStream(ms);
                }
                return true;
            }
            catch (Exception e)
            {
                _mainWindow.ReportError(string.Format("Error: {0}", e.Message));
                return false;
            }
        }

        #region Restore database via SQL script

        public async Task<bool> InstallationSqlConfigureAsync(SqlConnectionInfo connectionInfo, IProgress<double> progress, string installationFolder, string iPlusVersion, 
                                                              CancellationToken cancelToken)
        {
            int progressCount = 0;
            if (cancelToken.IsCancellationRequested)
                return false;
            if (string.IsNullOrEmpty(connectionInfo.ConnectionStrings))
            {
                connectionInfo.ConnectionState = SqlConfigurationState.NoConnection;
                return false;
            }
            try
            {
                SqlConnection conn = new SqlConnection(connectionInfo.ConnectionStrings);
                while (progressCount <= 20)
                {
                    Thread.Sleep(30);
                    progress.Report(progressCount++);
                }
                if (cancelToken.IsCancellationRequested)
                    return false;

                string dbName = string.Format("{0}V4demo", iPlusVersion);

                if (!CheckIfDatabaseExist(conn, dbName))
                {
                    progress.Report(-1);
                    return true;
                }

                while (progressCount <= 30)
                {
                    Thread.Sleep(30);
                    progress.Report(progressCount++);
                }

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                string dbScriptPath = Path.Combine(installationFolder, AppVersion.DatabaseFileName.Replace("zip", "sql"));
                string script = File.ReadAllText(dbScriptPath);
                int branchesCount = script.Split(new string[]{"GO"}, StringSplitOptions.None).Count();
                script = script.Replace("-dbName-", dbName);
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(script));
                script = null;

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                _tempProgress = progress;
                var progressDB = new Progress<double>(DBProgress);
                await Task.Run(() => RestoreDatabaseFromScript(conn, stream, progressDB, branchesCount));
                conn.Close();
                stream.Close();
                stream.Dispose();
                _tempProgress = null;
                progressCount = 70;

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                if (connectionInfo.AuthMode == 0)
                    CheckAndAddUser(conn, dbName);

                conn.Close();
                conn.Dispose();

                await Task.Run(() => PrepareAppConfig(connectionInfo, installationFolder, iPlusVersion, dbName));
                while (progressCount <= 100)
                {
                    Thread.Sleep(10);
                    progress.Report(progressCount++);
                }

                if (cancelToken.IsCancellationRequested)
                    return CancelDBConfigOperation(conn);

                return true;
            }
            catch (Exception e)
            {
                _mainWindow.AddInstallFlagToUserAppData();
                _mainWindow.ReportError("Application installation is OK, but database insertion fail!!! Please insert database manually. Error message:" + e.Message);
                return false;
            }
        }

        private bool RestoreDatabaseFromScript(SqlConnection conn, Stream scriptStream, IProgress<double> progress, int batchCount)
        {
            try
            {
                conn.Open();
                using (SqlCmd cmd = new SqlCmd(conn, batchCount))
                {
                    cmd.ExecuteStream(scriptStream, null, progress);
                }
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckIfDatabaseExist(SqlConnection conn, string dbName)
        {
            string dbExist = string.Format("SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '{0}' OR name = '{0}')", dbName);
            conn.Open();
            
            if (CheckIfDatabaseExistRunScript(conn, dbExist, dbName))
            {
                string newDatabase = string.Format("CREATE DATABASE {0}", dbName);
                using(MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(newDatabase)))
                    SqlCmd.ExecuteStream(conn, ms);

                conn.Close();
                return true;
            }
            conn.Close();
            return false;
        }

        private bool CheckIfDatabaseExistRunScript(SqlConnection connection, string script, string dbName)
        {
            SqlCommand comm = new SqlCommand(script, connection);
            
            var result = comm.ExecuteScalar();
            if (result != null)
            {
                if (MsgBox.Show(string.Format(TextResources.TextResource.MsgDBAlreadyExist, dbName), TextResources.TextResource.MsgHeaderWarning, System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                    return CheckIfDatabaseExistRunScript(connection, script, dbName);

                else
                    return false;
            }
            return true;
        }

        private void CheckAndAddUser(SqlConnection conn, string databaseName)
        {
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");

            ManagementObjectCollection collection = searcher.Get();
            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];

            if (username != System.Environment.UserDomainName+"\\"+System.Environment.UserName)
            {
                try
                {
                    string script = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseConfiguration", "AddLogin.sql"));
                    script = script.Replace("-databaseName-", conn.Database);
                    script = script.Replace("-username-", username);
                    Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(script));

                    string scriptAddUser = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseConfiguration", "AddUser.sql"));
                    scriptAddUser = scriptAddUser.Replace("-databaseName-", databaseName);
                    scriptAddUser = scriptAddUser.Replace("-username-", username);
                    Stream streamAddUser = new MemoryStream(Encoding.UTF8.GetBytes(scriptAddUser));

                    conn.Open();
                    using (SqlCmd cmd = new SqlCmd(conn))
                    {
                        cmd.ExecuteStream(stream);
                    }

                    using (SqlCmd cmd = new SqlCmd(conn))
                    {
                        cmd.ExecuteStream(streamAddUser);
                    }

                    stream.Dispose();
                    streamAddUser.Dispose();
                    conn.Close();
                }
                catch
                {
                }
            }
        }

        private IProgress<double> _tempProgress;

        private void DBProgress(double value)
        {
            double calc = Math.Round(value * 0.39 + 30,2);
            if (_tempProgress != null)
                _tempProgress.Report(calc);
        }

        #endregion

        private bool PrepareAppConfig(SqlConnectionInfo connectionInfo, string path, string iPlusVersion, string dbName)
        {
            string ver = iPlusVersion == IPlusMES ? "mes" : iPlusVersion;

            string configPath = Path.Combine(path, $"gip.{ver.ToLower()}.client.exe.config");
            string exePath = Path.Combine(path, $"gip.{ver.ToLower()}.client.exe");

            if (File.Exists(configPath))
            {
                sc.Configuration configuration = sc.ConfigurationManager.OpenExeConfiguration(exePath);
                configuration.ConnectionStrings.ConnectionStrings.Remove("iPlusV4_Entities");
                configuration.ConnectionStrings.ConnectionStrings.Remove("iPlusMESV4_Entities");

                string iplusConnString = string.Format(@"metadata=res://*/iPlusV4.csdl|res://*/iPlusV4.ssdl|res://*/iPlusV4.msl;provider=System.Data.SqlClient;provider connection string='{2}" +
                                                       "    Integrated Security=SSPI;{2}" +
                                                       "    data source={0};{2}"+
                                                       "    initial catalog={1};{2}"+
                                                       "    persist security info=True;{2}"+
                                                       "    multipleactiveresultsets=True;{2}"+
                                                       "    application name=iPlus_db'",connectionInfo.ServerName, dbName, System.Environment.NewLine);

                if (connectionInfo.AuthMode == 1)
                    iplusConnString = string.Format(@"metadata=res://*/iPlusV4.csdl|res://*/iPlusV4.ssdl|res://*/iPlusV4.msl;provider=System.Data.SqlClient;provider connection string='{4}" +
                                                    "   data source={0};{4}" +
                                                    "   initial catalog={3};{4}"+
                                                    "   persist security info=True;{4}"+
                                                    "   user id={1};{4}" +
                                                    "   password={2};{4}"+
                                                    "   multipleactiveresultsets=True;{4}"+
                                                    "   application name=iPlus_db'", connectionInfo.ServerName, connectionInfo.Username, connectionInfo.Password, dbName, System.Environment.NewLine);
                
                sc.ConnectionStringSettings varioIplusSettings = new sc.ConnectionStringSettings("iPlusV4_Entities", iplusConnString, "System.Data.EntityClient");
                configuration.ConnectionStrings.ConnectionStrings.Add(varioIplusSettings);

                if (iPlusVersion == IPlusMES)
                {
                    string varioConnString = string.Format(@"metadata=.\iPlusMESV4.csdl|.\iPlusMESV4.ssdl|.\iPlusMESV4.msl;provider=System.Data.SqlClient;provider connection string='{2}" +
                                                           "    Integrated Security=SSPI;{2}"+
                                                           "    data source={0};{2}"+
                                                           "    initial catalog={1};{2}" +
                                                           "    persist security info=True;{2}"+
                                                           "    MultipleActiveResultSets=True;{2}"+
                                                           "    App=iPlus_dbApp'", connectionInfo.ServerName, dbName, System.Environment.NewLine);

                    if (connectionInfo.AuthMode == 1)
                        varioConnString = string.Format(@"metadata=.\iPlusMESV4.csdl|.\iPlusMESV4.ssdl|.\iPlusMESV4.msl;provider=System.Data.SqlClient;provider connection string='{4}" +
                                                        "   data source={0};{4}"+
                                                        "   initial catalog={3};{4}"+
                                                        "   persist security info=True;{4}"+
                                                        "   user id={1};{4}"+
                                                        "   password={2};{4}" +
                                                        "   MultipleActiveResultSets=True;{4}"+
                                                        "   App=iPlus_dbApp'", connectionInfo.ServerName, connectionInfo.Username, connectionInfo.Password, dbName, System.Environment.NewLine);

                    sc.ConnectionStringSettings variobatchSettings = new sc.ConnectionStringSettings("iPlusMESV4_Entities", varioConnString, "System.Data.EntityClient");
                    configuration.ConnectionStrings.ConnectionStrings.Add(variobatchSettings);
                }
                configuration.Save();

                string connStringPath = Path.Combine(path, "ConnectionStrings.config");
                if(File.Exists(connStringPath))
                {
                    try
                    {
                        string content = File.ReadAllText(connStringPath);
                        content = content.Replace("&#xD;&#xA;", System.Environment.NewLine);
                        File.WriteAllText(connStringPath, content);
                    }
                    catch
                    {
                    }
                }
            }
            return true;
        }

        public async Task<List<String>> FindSqlServersAsync()
        {
            return await Task.Run(() => FindSqlServer());
        }

        public List<string> FindSqlServer()
        {
            List<string> sqlServerList = new List<string>();
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            DataTable table = instance.GetDataSources();
            foreach (DataRow row in table.Rows)
            {
                string serverName = row[table.Columns["ServerName"]].ToString();
                string instanceName = row[table.Columns["InstanceName"]].ToString();
                if(!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(instanceName))
                    sqlServerList.Add(serverName + "\\" + instanceName);
            }
            _mainWindow.SqlServerSearchingCompleted(sqlServerList);
            return sqlServerList;
        }

        public SqlBrowserServiceState CheckSqlBrowserService()
        {
            using (ServiceController service = new ServiceController("SQLBrowser"))
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                        return SqlBrowserServiceState.ServiceRunning;
                    return SqlBrowserServiceState.ServiceNotRunning;
                }
                catch
                {
                    return SqlBrowserServiceState.ServiceNotExist;
                }
            }
        }

        public void DeleteFolder(string installationPath, bool withClose = true)
        {
            if(_mainWindow._resetEvent != null)
                _mainWindow._resetEvent.WaitOne(10000);
            DeleteFolderInternal(installationPath, withClose);
        }

        internal void DeleteFolderInternal(string installationPath, bool withClose = true)
        {
            if(Directory.Exists(installationPath))
            {
                try
                {
                    Directory.Delete(installationPath, true);
                    if(withClose)
                        _mainWindow.CloseAfterDelete();
                }
                catch (Exception e)
                {
                    _mainWindow.ReportError(e.Message);
                }
            }
        }

        #endregion

        #region Update

        public List<gip.tool.publish.Version> AppVersionList
        {
            get;
            set;
        }

        internal void CheckForUpdate(string iPlusVersion, string installationPath)
        {
            Task task = Task.Run(async () => await CheckUpdateVersion(installationPath, iPlusVersion));
        }

        public static string RestoreCredentials(string installationPath)
        {
            string fileName = Path.Combine(installationPath, "userInfo.gip");
            if (!File.Exists(fileName))
                return "";
            string encXml = File.ReadAllText(fileName);
            byte[] encodedFileName = new UTF8Encoding().GetBytes(fileName);
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(encodedFileName);
            }
            string xml = Encrypt.DecryptString(encXml, hash);
            return xml;
        }

        internal async Task CheckUpdateVersion(string installationPath, string iPlusVersion)
        {
            Thread.Sleep(1000);
            string versionFileName = Path.Combine(installationPath, "version.xml");
            string versionListFileName = Path.Combine(installationPath, "versionList.xml");

            if (!Directory.Exists(installationPath) || !File.Exists(versionFileName))
                return;

            DataContractSerializer serializer = new DataContractSerializer(typeof(gip.tool.publish.Version));
            try
            {
                using (FileStream fs = new FileStream(versionFileName, FileMode.Open))
                {
                    AppVersion = serializer.ReadObject(fs) as gip.tool.publish.Version;
                }
            }
            catch
            {
                return;
            }

            string downloadMehtodName = "Serve";
            string verDownloadUrl = LoginManager.RemoteServer + string.Format(@"/en/{0}/{1}{2}?name=version.xml", LoginManager.ControllerName, downloadMehtodName, iPlusVersion);

            using (var client = new HttpClientDownloadWithProgress(verDownloadUrl, versionListFileName))
            {
                await client.StartDownload();
            }

            serializer = new DataContractSerializer(typeof(List<gip.tool.publish.Version>));
            if(!File.Exists(Path.Combine(installationPath, "versionList.xml")))
                return;

            try
            {
                using(FileStream fs = new FileStream(versionListFileName, FileMode.Open))
                {
                    AppVersionList = serializer.ReadObject(fs) as List<gip.tool.publish.Version>;
                }
            }
            catch
            {
                return;
            }

            bool isCurrentVersionInVersionList = AppVersionList.Any(c => c.PublishDateTime == AppVersion.PublishDateTime);
            if (isCurrentVersionInVersionList)
            {
                if (AppVersionList.FirstOrDefault(c => c.PublishDateTime == AppVersionList.Max(x => x.PublishDateTime)).PublishDateTime == AppVersion.PublishDateTime)
                    _mainWindow.IsNewUpdateAvailable = false;
                else
                    _mainWindow.IsNewUpdateAvailable = true;
            }
            else
                _mainWindow.IsNewUpdateAvailable = true;

            string changeLogMessage = "";
            List<gip.tool.publish.Version> changeLogVersions;

            if (isCurrentVersionInVersionList)
                changeLogVersions = AppVersionList.Where(c => c.PublishDateTime > AppVersion.PublishDateTime).OrderBy(x => x.PublishDateTime).ToList();
            else
                changeLogVersions = AppVersionList.OrderBy(c => c.PublishDateTime).ToList();

            foreach (var ver in changeLogVersions)
            {
                string langChangeLog;
                if (System.Globalization.CultureInfo.CurrentUICulture.Name == "de-DE")
                    langChangeLog = ver.ChangeLogDe;
                else
                    langChangeLog = ver.ChangeLogEn;

                changeLogMessage += string.Format("Version {0}{1}{2}{3}------------------------------------------------------------------------------{4}",
                                    ver.SvnRevision, System.Environment.NewLine, langChangeLog, System.Environment.NewLine, System.Environment.NewLine);
            }

            File.Delete(versionListFileName);

            _mainWindow.ChangeLogMessage = changeLogMessage;
            _mainWindow.IsUpdatePBIndeterminate = false;
            _mainWindow.CheckForUpdateStatus = TextResources.TextResource.CheckForUpdateStatus2;
            _mainWindow.IsCheckForUpdateFinished = true;
        }

        public void CopyCurrentAppVersion(string installationPath)
        {
            CopyDir.Copy(installationPath, GenerateUpdateBackupPath(installationPath));
        }

        public string GenerateUpdateBackupPath(string installationFolder)
        {
            string backupPath = "";
            int last = installationFolder.LastIndexOf(Path.DirectorySeparatorChar);
            if (last > 0)
                backupPath = installationFolder.Substring(0, last) + Path.DirectorySeparatorChar + "rollback";
            return backupPath;
        }

        internal void DeleteUpdate(string installationPath)
        {
            string backupPath = GenerateUpdateBackupPath(installationPath);
            string backupPathOld = backupPath + "Old";
            DeleteFolder(installationPath, false);
            Directory.Move(backupPath, installationPath);
            if (Directory.Exists(backupPathOld))
                Directory.Move(backupPathOld, backupPath);
            _mainWindow.CloseAfterDelete();
        }

        public SqlConnectionInfo TryGetDBLoginFromAppConfig(string installationPath, string iPlusVersion)
        {
            sc.Configuration config = null;
            try
            {
                string ver = iPlusVersion == IPlusMES ? "mes" : iPlusVersion;
                config = sc.ConfigurationManager.OpenExeConfiguration(Path.Combine(_mainWindow.InstallationFolder, $"gip.{ver.ToLower()}.client.exe"));
            }
            catch (Exception e)
            {
                config = null;
                _mainWindow.ReportError(e.Message);
            }
            if (config == null || config.ConnectionStrings == null)
                return null;
            string connStringName = string.Format("{0}V4_Entities", iPlusVersion);
            string connString =config.ConnectionStrings.ConnectionStrings[connStringName].ConnectionString;
            return ReadConnectionStrings(connString);
        }

        public SqlConnectionInfo ReadConnectionStrings(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            string server = "";
            string database = "";
            string user = "";
            string pass = "";
            int authMode = 0;

            int indexOfConn = connectionString.ToLower().IndexOf("provider connection string=") + "provider connection string='".Length;
            string cleanConnString = connectionString.Substring(indexOfConn, connectionString.Length - indexOfConn - 1);
            string[] parts = cleanConnString.Split(';');

            try
            {
                server = parts.FirstOrDefault(c => c.ToLower().Contains("data source=")).Split('=').LastOrDefault();
                database = parts.FirstOrDefault(c => c.ToLower().Contains("initial catalog=")).Split('=').LastOrDefault();

                string integratedSecurity = "";
                if (parts.Any(c => c.ToLower().Contains("integrated security=")))
                    integratedSecurity = parts.FirstOrDefault(c => c.ToLower().Contains("integrated security=")).Split('=').LastOrDefault();

                if (integratedSecurity.ToLower() == "false" || integratedSecurity.ToLower() == "no" || string.IsNullOrEmpty(integratedSecurity))
                {
                    authMode = 1;
                    user = parts.FirstOrDefault(c => c.ToLower().Contains("user id=")).Split('=').LastOrDefault();
                    pass = parts.FirstOrDefault(c => c.ToLower().Contains("password=")).Split('=').LastOrDefault();
                }
            }
            catch
            {
                return null;
            }
            return new SqlConnectionInfo() { ServerName = server, DatabaseName = database, AuthMode = authMode, Username = user, Password = pass };
        }

        public delegate void CallbackDelegate(SqlConfigurationState result);
        public async Task TryConnectToDatabase(SqlConnectionInfo connectionInfo, CallbackDelegate callbackFunction)
        {
            var connInfo = await CheckSqlServerUserRoleAsync(connectionInfo);
            callbackFunction(connInfo.ConnectionState);
        }

        public void PrepareRollbackFolder(string installationPath)
        {
            string rollbackDir = GenerateUpdateBackupPath(installationPath);
            string rollbackDirOld = rollbackDir+"Old";
            if (Directory.Exists(rollbackDirOld))
                Directory.Delete(rollbackDirOld, true);
            if (Directory.Exists(rollbackDir))
                Directory.Move(rollbackDir, rollbackDirOld);

            Directory.Move(installationPath, rollbackDir);
            Directory.CreateDirectory(installationPath);
        }

        public void BackupDatabase(SqlConnectionInfo connectionInfo, string rollbackPath)
        {
            string dbBackupPath = rollbackPath;

            if (File.Exists(dbBackupPath))
                File.Delete(dbBackupPath);

            try
            {
                SqlConnection conn = new SqlConnection(connectionInfo.ConnectionStrings);
                conn.Open();

                string script = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\DatabaseConfiguration\Backup.sql");
                script = script.Replace("-location-", dbBackupPath);
                script = script.Replace("-databaseName-", connectionInfo.DatabaseName);

                using(MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(script)))
                {
                    SqlCmd.ExecuteStream(conn, ms);
                }

                conn.Close();
                conn.Dispose();

                //using (ZipFile zip = new ZipFile())
                //{
                //    zip.AddFile(dbBackupPath, "");
                //    zip.Save(dbBackupPath.Replace(".bak", ".zip"));
                //}
                //File.Delete(dbBackupPath);

                //if (File.Exists(rollbackPath + "\\" + AppVersion.DatabaseFileName))
                //    File.Delete(rollbackPath + "\\" + AppVersion.DatabaseFileName);

                //File.Copy(dbBackupPath.Replace(".bak", ".zip"), rollbackPath + "\\" + AppVersion.DatabaseFileName);

                //File.Delete(dbBackupPath.Replace(".bak", ".zip"));

            }
            catch (Exception e)
            {
                MsgBox.Show(e.Message, TextResources.TextResource.MsgHeaderWarning, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        public void CopyUserFiles(string installationPath, string iPlusVersion)
        {
            string rollbackPath = GenerateUpdateBackupPath(installationPath);
            CopyUserFilesRecursive(installationPath, rollbackPath);

            string ver = iPlusVersion == IPlusMES ? "mes" : iPlusVersion;
            string appConfigPath = string.Format("gip.{0}.client.exe.config", ver.ToLower());
            string connectionStringsPath = "ConnectionStrings.config";

            if (File.Exists(Path.Combine(rollbackPath, appConfigPath)))
                File.Copy(Path.Combine(rollbackPath, appConfigPath), Path.Combine(installationPath, appConfigPath), true);

            if(File.Exists(Path.Combine(rollbackPath, connectionStringsPath)))
                File.Copy(Path.Combine(rollbackPath, connectionStringsPath), Path.Combine(installationPath, connectionStringsPath), true);
        }

        private void CopyUserFilesRecursive(string installatioPath, string rollbackPath)
        {
            IEnumerable<string> installFiles = Directory.EnumerateFiles(installatioPath).Select(c => c.Split(Path.DirectorySeparatorChar).LastOrDefault());
            IEnumerable<string> rollbackFiles = Directory.EnumerateFiles(rollbackPath).Select(c => c.Split(Path.DirectorySeparatorChar).LastOrDefault());
            IEnumerable<string> diff = rollbackFiles.Except(installFiles);
            foreach (string item in diff)
            {
                string rollbackFile = Path.Combine(rollbackPath, item);
                string installFile = Path.Combine(installatioPath, item);
                File.Copy(rollbackFile, installFile, true);
            }

            IEnumerable<string> rollbackDir = Directory.EnumerateDirectories(rollbackPath).Select(x => x.Split(Path.DirectorySeparatorChar).LastOrDefault()).ToArray().Where(c => c != "VBControlScripts" && c != "DbScripts");

            foreach (string dir in rollbackDir)
            {
                if (!Directory.Exists(Path.Combine(installatioPath, dir)))
                    Directory.CreateDirectory(Path.Combine(installatioPath, dir));
                CopyUserFilesRecursive(Path.Combine(installatioPath, dir), Path.Combine(rollbackPath, dir));
            }
        }

        public void SetNewVersion(string installationPath)
        {
            AppVersion = AppVersionList.FirstOrDefault(c => c.PublishDateTime == AppVersionList.Max(x => x.PublishDateTime));
            string versionPath = Path.Combine(installationPath, "version.xml");
            using(FileStream fs = new FileStream(versionPath, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(gip.tool.publish.Version));
                serializer.WriteObject(fs, AppVersion);
            }
        }

        public void DeleteRollbackOldVersion(string installationPath)
        {
            string installFolder = GenerateUpdateBackupPath(installationPath);
            if (Directory.Exists(Path.Combine(installFolder, "Old")))
                Directory.Delete(Path.Combine(installFolder, "Old"), true);
        }

        public void DeleteRollbackVersion(string installationPath)
        {
            string installFolder = GenerateUpdateBackupPath(installationPath);
            if (Directory.Exists(installFolder))
                Directory.Delete(installFolder, true);
        }

        #endregion

        #region Rollback

        public RollbackType DetermineRollbackType(string installationPath)
        {
            string rollbackPath = GenerateUpdateBackupPath(installationPath);
            if (!Directory.Exists(rollbackPath))
                return RollbackType.Disabled;

            ReadRollbackVersion(installationPath);
            //string dbFileName = Path.Combine(rollbackPath, AppVersion.DatabaseFileName);
            //if (File.Exists(dbFileName))
            //    return RollbackType.EnabledAutomatic; 

            return RollbackType.EnabledManual;
        }

        public void ReadRollbackVersion(string installationPath)
        {
            string rollbackVersionPath = Path.Combine(GenerateUpdateBackupPath(installationPath), "version.xml");
            if(AppVersion == null)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(gip.tool.publish.Version));
                try
                {
                    using (FileStream fs = new FileStream(rollbackVersionPath, FileMode.Open))
                    {
                        AppVersion = serializer.ReadObject(fs) as gip.tool.publish.Version;
                    }
                }
                catch
                {
                    return;
                }
            }
        }

        public delegate void RollbackOperationCallback(int currentOperation);
        public async void RollbackApp(string installationPath, string iPlusVersion, RollbackType rollbackType, CancellationToken cancelToken, SqlConnectionInfo connectionInfo,
                                      RollbackOperationCallback callback, string dbBackupFilePath = null)
        {
            callback(1);
            string rollbackPath = GenerateUpdateBackupPath(installationPath);

            if (rollbackType == RollbackType.EnabledAutomatic)
            {
                callback(2);
                await Task.Run(() => UnpackFile(rollbackPath, AppVersion.DatabaseFileName, cancelToken));

                callback(3);
                await SqlConfigureAsync(connectionInfo, new Progress<int>(), installationPath, iPlusVersion, cancelToken);
            }
            else if(rollbackType == RollbackType.EnabledManual && !string.IsNullOrEmpty(dbBackupFilePath))
            {
                callback(3);
                await SqlConfigureAsync(connectionInfo, new Progress<int>(), installationPath, iPlusVersion, cancelToken, dbBackupFilePath);
            }

            callback(4);
            Directory.Delete(installationPath, true);

            callback(5);
            Directory.Move(GenerateUpdateBackupPath(installationPath), installationPath);

            callback(6);
        }

        public bool SaveAppVersion(string installationPath, gip.tool.publish.Version appVersion)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(gip.tool.publish.Version));
                using (FileStream fs = new FileStream(Path.Combine(installationPath, "version.xml"), FileMode.Create))
                {
                    serializer.WriteObject(fs, appVersion);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Uninstall

        public void UninstallApp(string installationPath, bool deleteUserFiles, string iPlusVersion, IProgress<double> progress)
        {
            //List<string> listGipFiles;
            string gipInstallPath = System.Environment.ExpandEnvironmentVariables("%LocalAppData%\\gipInstall");

            if (!deleteUserFiles)
            {
                //string gipFileInstallPath = gipInstallPath + "\\fileList.xml";
                //if (!File.Exists(gipFileInstallPath))
                //    return;

                //DataContractSerializer serializer = new DataContractSerializer(typeof(List<string>));
                //using (FileStream fs = new FileStream(gipFileInstallPath, FileMode.Open))
                //{
                //    listGipFiles = serializer.ReadObject(fs) as List<string>;
                //}
                //if (listGipFiles == null)
                //    return;

                //int listGipCount = listGipFiles.Count;
                //int i = 1;
                //foreach (string filePath in listGipFiles)
                //{
                //    if (File.Exists(filePath))
                //        File.Delete(filePath);
                //    progress.Report(Math.Round((double)i / listGipCount * 100,2));
                //    i++;
                //}

                //ClearEmptyFolders(installationPath);

                //DeleteRollbackVersion(installationPath);

                //foreach(string remainFile in Directory.EnumerateFiles(installationPath))
                //{
                //    if(remainFile.Contains(string.Format("gip.{0}.client", iPlusVersion.ToLower())))
                //        File.Delete(remainFile);
                //}

                ////if (Directory.Exists(gipInstallPath))
                ////    Directory.Delete(gipInstallPath, true);

                //progress.Report(-1);
            }
            else
            {
                int count = Directory.EnumerateFiles(installationPath, "*", SearchOption.AllDirectories).Count();
                int i = 1;
                foreach (string filePath in Directory.EnumerateFiles(installationPath, "*", SearchOption.AllDirectories))
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    progress.Report(Math.Round((double)i / count * 100, 2));
                    i++;
                }

                DeleteRollbackVersion(installationPath);

                if(Directory.Exists(installationPath))
                    Directory.Delete(installationPath, true);

                if (Directory.Exists(gipInstallPath))
                    Directory.Delete(gipInstallPath, true);

                progress.Report(-1);
            }
        }

        private void ClearEmptyFolders(string dirPath)
        {
            foreach (string dir in Directory.EnumerateDirectories(dirPath))
            {
                ClearEmptyFolders(dir);
                if (!Directory.EnumerateFiles(dir).Any() && !Directory.EnumerateDirectories(dir).Any())
                    Directory.Delete(dir);
            }
        }

        #endregion
    }
}
