﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;

namespace gip.iplus.startup
{
    /// <summary>
    /// Rules for the install scripts(methods): 
    /// Name: developerName_yyyy-MM-dd HH-mm_scriptMode.cs
    /// Method signature: public bool MyScriptMethod(string sourceInstallPath, string localInstallPath)
    /// PreStartup example(runs before copy/delete operation): ihrastinski_2018-10-18 07-55_pre.cs
    /// PostStartup example(runs after copy/delete operation): ihrastinski_2018-10-18 07-56_post.cs
    /// </summary>
    class Program
    {
        private const string InstallScriptsFolderName = "InstallScripts";
        public const string iPlusExeName = "gip.iplus.client.exe";
        public const string iPlusMESExeName = "gip.mes.client.exe";
        public const string iPlusVBExeName = "gip.variobatch.client.exe";

        static void Main(string[] args)
        {
            if (args.Count() <= 0)
            {
                System.Console.WriteLine("gip.iplus.startup.exe [Installation path Server] [Installations path local] optional:[\"Startparameter\"] optional: [Working directory of iPlus] optional: [DisableDelete (true/false default:false)]");
                System.Console.Read();
                return;
            }

            string sourceInstallPath = "";
            string localInstallPath = "";
            string workingDirectory = "";
            string commandLineArgs = "";
            bool disableDelete = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    sourceInstallPath = args[i];
                else if (i == 1)
                    localInstallPath = args[i];
                else if (i == 2)
                    commandLineArgs = args[i];
                else if (i == 3)
                    workingDirectory = args[i];
                else if (i == 4)
                    if(args[i].ToLower() == "true")
                        disableDelete = true;
            }

            if (String.IsNullOrEmpty(sourceInstallPath))
            {
                System.Console.WriteLine("Installationspfad des Servers nicht angegeben.");
                System.Console.Read();
                return;
            }

            if (!Directory.Exists(sourceInstallPath))
            {
                System.Console.WriteLine("Installationspfad des Servers ist ungültig.");
                System.Console.Read();
                return;
            }

            if (String.IsNullOrEmpty(localInstallPath))
            {
                System.Console.WriteLine("Installationspfad auf Client nicht angegeben.");
                System.Console.Read();
                return;
            }

            if (!Directory.Exists(localInstallPath))
            {
                System.Console.WriteLine("Installationspfad des Clients ist ungültig.");
                System.Console.Read();
                return;
            }

            bool isWriteAccess = false;
            try
            {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(localInstallPath).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in collection)
                {
                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        isWriteAccess = true;
                        break;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                isWriteAccess = false;
                System.Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                isWriteAccess = false;
                System.Console.WriteLine(ex.Message);
            }
            if (!isWriteAccess)
            {
                System.Console.WriteLine("Keine Schreibrechte auf Installationspfad des Clients.");
                System.Console.Read();
                return;
            }

            InitializeInstallScripts(sourceInstallPath);
            if (!ExecuteInstallScriptsPreStartup(sourceInstallPath, localInstallPath))
                return;

            string pathIPlus = "";
            string[] extensions = { ".dll", ".exe", ".csdl", ".ssdl", ".msl", ".chm", ".cpl", ".inf", ".llx", ".ocx", ".lng", ".cpl", ".pdb" };
            IEnumerable<string> fileListSourcePath = Directory.EnumerateFiles(sourceInstallPath)
                                                              .Where(f =>      extensions.Any(ext => ext == Path.GetExtension(f).ToLower())
                                                                            || f.ToLower().EndsWith(iPlusExeName+ ".config") 
                                                                            || f.ToLower().EndsWith(iPlusMESExeName+ ".config")
                                                                            || f.ToLower().EndsWith(iPlusVBExeName + ".config")
                                                                     );
            foreach (string sourcefilePath in fileListSourcePath)
            {
                string fileName = Path.GetFileName(sourcefilePath);
                string destFilePath = localInstallPath + "\\" + fileName;
                if (fileName == iPlusExeName || fileName == iPlusMESExeName || fileName == iPlusVBExeName)
                    pathIPlus = destFilePath;
                bool copyFile = !File.Exists(destFilePath);
                if (!copyFile)
                {
                    DateTime destFileTime = File.GetLastWriteTime(sourcefilePath);
                    DateTime sourceFileTime = File.GetLastWriteTime(destFilePath);
                    if (sourceFileTime != destFileTime)
                        copyFile = true;
                }
                if (copyFile)
                {
                    File.Copy(sourcefilePath, destFilePath, true);
                }
            }

            if (!disableDelete)
            {
                IEnumerable<string> fileListLocalPath = Directory.EnumerateFiles(sourceInstallPath)
                                                                 .Where(f => extensions.Any(ext => ext == Path.GetExtension(f).ToLower())
                                                                            || f.ToLower().EndsWith(iPlusExeName + ".config")
                                                                            || f.ToLower().EndsWith(iPlusMESExeName + ".config")
                                                                            || f.ToLower().EndsWith(iPlusVBExeName + ".config")
                                                                     );
                foreach (string localFilePath in fileListLocalPath)
                {
                    string fileName = Path.GetFileName(localFilePath);
                    string sourceFilePath = sourceInstallPath + "\\" + fileName;
                    if (!File.Exists(sourceFilePath))
                        File.Delete(localFilePath);
                }
            }

            DirectoryInfo dir = new DirectoryInfo(sourceInstallPath);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(localInstallPath, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true);
            }

            if (!disableDelete)
            {
                DirectoryInfo destDir = new DirectoryInfo(localInstallPath);
                DirectoryInfo[] destDirs = destDir.GetDirectories();

                foreach(DirectoryInfo subDir in destDirs)
                {
                    string sourceDir = Path.Combine(sourceInstallPath, subDir.Name);
                    DirectoryDelete(subDir.FullName, sourceDir, true);
                }
            }

            if (!ExecuteInstallScriptsPostStartup(sourceInstallPath, localInstallPath))
                return;

            if (!String.IsNullOrEmpty(pathIPlus))
            {
                ProcessStartInfo startInfo;
                if (String.IsNullOrEmpty(commandLineArgs))
                    startInfo = new ProcessStartInfo(pathIPlus);
                else
                {
                    startInfo = new ProcessStartInfo(pathIPlus, commandLineArgs.Replace("\"", ""));
                    startInfo.UseShellExecute = false;
                }
                if (!String.IsNullOrEmpty(workingDirectory))
                    startInfo.WorkingDirectory = workingDirectory;

                Process.Start(startInfo);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string destFilePath = Path.Combine(destDirName, file.Name);
                string sourcefilePath = file.FullName;
                bool copyFile = !File.Exists(destFilePath);
                if (!copyFile)
                {
                    DateTime destFileTime = File.GetLastWriteTime(sourcefilePath);
                    DateTime sourceFileTime = File.GetLastWriteTime(destFilePath);
                    if (sourceFileTime != destFileTime)
                        copyFile = true;
                }
                if (copyFile)
                {
                    file.CopyTo(destFilePath, true);
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void DirectoryDelete(string localInstallDirName, string sourceInstallDirName, bool checkSubDirs)
        {
            if(!Directory.Exists(sourceInstallDirName))
            {
                Directory.Delete(localInstallDirName, true);
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(localInstallDirName);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string sourceFileName = Path.Combine(sourceInstallDirName, file.Name);
                if (!File.Exists(sourceFileName))
                    File.Delete(file.FullName);
            }

            if (checkSubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subDir in dirs)
                {
                    string sourceDirName = Path.Combine(sourceInstallDirName, subDir.Name);
                    DirectoryDelete(subDir.FullName, sourceDirName, checkSubDirs);
                }
            }
        }

        private static void InitializeInstallScripts(string sourceInstallPath)
        {
            string installScriptsFolderPath = Path.Combine(sourceInstallPath, InstallScriptsFolderName);
            if (!Directory.Exists(installScriptsFolderPath))
                return;

            IEnumerable<string> scriptsFilePath = Directory.EnumerateFiles(installScriptsFolderPath, "*.cs", SearchOption.TopDirectoryOnly);
            if (!scriptsFilePath.Any())
                return;

            List<ScriptFile> scriptFileList = new List<ScriptFile>();

            foreach(string scriptFilePath in scriptsFilePath)
            {
                ScriptFile scriptFile = ScriptFile.InitializeScriptFile(scriptFilePath);
                if (scriptFile != null)
                    scriptFileList.Add(scriptFile);
            }

            if(!scriptFileList.Any())
                return;

            if (_ScriptEngine == null)
                _ScriptEngine = new ScriptEngine();

            foreach(ScriptFile scriptFile in scriptFileList.OrderBy(c => c.ScriptDate))
            {
                _ScriptEngine.RegisterScript(scriptFile.MethodName, scriptFile.ScriptSourceMethod, false, scriptFile.InstallScriptMode, scriptFile.ScriptDate);
            }

            if(!_ScriptEngine.Compile())
            {
                foreach (string error in _ScriptEngine.CompileErrors)
                    Console.WriteLine(error);
                Console.Read();
                throw new Exception();
            }
        }

        private static bool ExecuteInstallScriptsPreStartup(string sourceInstallPath, string localInstallPath)
        {
            if (_ScriptEngine == null)
                return true;

            return _ScriptEngine.Execute(ScriptMode.PreStartup, sourceInstallPath, localInstallPath);
        }

        private static bool ExecuteInstallScriptsPostStartup(string sourceInstallPath, string localInstallPath)
        {
            if (_ScriptEngine == null)
                return true;

            return _ScriptEngine.Execute(ScriptMode.PostStartup, sourceInstallPath, localInstallPath);
        }

        private static ScriptEngine _ScriptEngine;
    }
}
