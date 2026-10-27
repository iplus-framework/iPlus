// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace gip.tool.installerAndUpdater.Models
{
    /// <summary>
    /// Which product is being installed. Determines the launch executable, the
    /// database name default and which initial-install script is applied.
    /// </summary>
    public enum ProductKind
    {
        Framework = 0,
        Mes = 10
    }

    /// <summary>
    /// What the user wants to do.
    /// </summary>
    public enum OperationKind
    {
        Install = 0,
        Update = 10
    }

    /// <summary>
    /// Where the payload comes from.
    /// </summary>
    public enum SourceKind
    {
        /// <summary>A folder the user picked (unzipped release, network share, USB stick).</summary>
        LocalFolder = 0,

        /// <summary>A zip file the user picked.</summary>
        LocalZip = 10,

        /// <summary>Downloaded from a GitHub Release.</summary>
        GitHubRelease = 20
    }

    /// <summary>
    /// SQL Server authentication mode.
    /// </summary>
    public enum SqlAuthMode
    {
        WindowsAuthentication = 0,
        SqlServerAuthentication = 1
    }

    /// <summary>
    /// Everything the installer needs to perform an install or update.
    /// </summary>
    public sealed class InstallOptions
    {
        public ProductKind Product { get; set; } = ProductKind.Framework;

        public OperationKind Operation { get; set; } = OperationKind.Install;

        public SourceKind Source { get; set; } = SourceKind.LocalFolder;

        /// <summary>Folder or zip file the payload is read from.</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>GitHub repository ("owner/name") when <see cref="Source"/> is GitHubRelease.</summary>
        public string GitHubRepository { get; set; } = string.Empty;

        /// <summary>Release tag to download; empty means "latest".</summary>
        public string GitHubTag { get; set; } = string.Empty;

        /// <summary>Target installation folder.</summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>Platform sub-folder inside the release bundle (e.g. "desktop-linux-x64").</summary>
        public string PlatformFolder { get; set; } = string.Empty;

        public SqlConnectionSettings Sql { get; set; } = new SqlConnectionSettings();

        /// <summary>Apply the initial-install script (fresh database only).</summary>
        public bool ApplyInitialInstallScript { get; set; } = true;

        /// <summary>Path of the initial-install script; empty means "look next to the payload".</summary>
        public string InitialInstallScriptPath { get; set; } = string.Empty;

        /// <summary>Create a desktop shortcut after a successful install.</summary>
        public bool CreateDesktopShortcut { get; set; } = true;

        /// <summary>Start the application after a successful install.</summary>
        public bool StartAfterInstall { get; set; } = true;

        /// <summary>
        /// Version being installed. Filled in by <c>InstallService</c> when the
        /// payload comes from a GitHub release; otherwise read from the binaries.
        /// </summary>
        public string InstalledVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// SQL Server connection settings entered by the user.
    /// </summary>
    public sealed class SqlConnectionSettings
    {
        public string ServerName { get; set; } = string.Empty;

        public SqlAuthMode AuthMode { get; set; } = SqlAuthMode.SqlServerAuthentication;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Builds the connection string used to reach the server (master database).
        /// </summary>
        public string BuildServerConnectionString()
        {
            return BuildConnectionString("master");
        }

        /// <summary>
        /// Builds the connection string for the application database.
        /// </summary>
        public string BuildApplicationConnectionString()
        {
            return BuildConnectionString(DatabaseName);
        }

        private string BuildConnectionString(string database)
        {
            List<string> parts = new List<string>
            {
                "Data Source=" + ServerName,
                "Initial Catalog=" + database,
                "Persist Security Info=True",
                "MultipleActiveResultSets=True",
                "Encrypt=False",
                "Application Name=iPlus"
            };

            if (AuthMode == SqlAuthMode.WindowsAuthentication)
            {
                parts.Add("Integrated Security=True");
                parts.Add("Trusted_Connection=True");
            }
            else
            {
                parts.Add("Integrated Security=False");
                parts.Add("Trusted_Connection=False");
                parts.Add("User Id=" + UserName);
                parts.Add("Password=" + Password);
            }

            return string.Join(";", parts) + ";";
        }
    }

    /// <summary>
    /// Progress report for the UI.
    /// </summary>
    public sealed class ProgressReport
    {
        public ProgressReport(string message, double fraction)
        {
            Message = message;
            Fraction = fraction;
        }

        public string Message { get; }

        /// <summary>0.0 – 1.0, or a negative value for "indeterminate".</summary>
        public double Fraction { get; }
    }

    /// <summary>
    /// Result of an install/update run.
    /// </summary>
    public sealed class InstallResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string InstalledVersion { get; set; } = string.Empty;

        public string LaunchExecutable { get; set; } = string.Empty;

        public static InstallResult Ok(string message, string version, string launchExecutable)
        {
            return new InstallResult
            {
                Success = true,
                Message = message,
                InstalledVersion = version,
                LaunchExecutable = launchExecutable
            };
        }

        public static InstallResult Fail(string message)
        {
            return new InstallResult { Success = false, Message = message };
        }
    }
}
