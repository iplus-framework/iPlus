// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Extracts the schema of a reference database as a T-SQL script by invoking
    /// the official <c>sqlpackage</c> CLI.
    /// </summary>
    /// <remarks>
    /// DacFx is deliberately *not* referenced as a NuGet package: it pins
    /// <c>Microsoft.Data.SqlClient</c> 5.x, which conflicts with the iPlus
    /// SqlClient fork (same assembly identity, different version). Calling the
    /// standalone <c>sqlpackage</c> executable keeps the tool free of that
    /// dependency and works on Windows, Linux and macOS.
    ///
    /// Install: <c>dotnet tool install -g microsoft.sqlpackage</c>
    /// </remarks>
    public sealed class SchemaExtractor
    {
        private readonly string _sqlPackagePath;

        public SchemaExtractor(string? sqlPackagePath = null)
        {
            _sqlPackagePath = string.IsNullOrWhiteSpace(sqlPackagePath) ? "sqlpackage" : sqlPackagePath;
        }

        /// <summary>
        /// Returns <c>true</c> when the sqlpackage executable can be started.
        /// </summary>
        public bool IsAvailable()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(_sqlPackagePath, "/version")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process? process = Process.Start(info);
                if (process == null)
                    return false;
                process.WaitForExit(15000);
                return process.HasExited;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the schema of the database into a <c>.dacpac</c> file.
        /// </summary>
        public async Task<string> ExtractDacpacAsync(string connectionString, string outputPath)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".";
            Directory.CreateDirectory(directory);

            List<string> arguments = new List<string>
            {
                "/Action:Extract",
                "/SourceConnectionString:" + connectionString,
                "/TargetFile:" + outputPath,
                "/p:ExtractAllTableData=False",
                "/p:VerifyExtraction=False"
            };

            await RunAsync(arguments);
            if (!File.Exists(outputPath))
                throw new InvalidOperationException("sqlpackage did not produce a dacpac at " + outputPath);
            return outputPath;
        }

        /// <summary>
        /// Generates a T-SQL script that creates the schema contained in a dacpac.
        /// </summary>
        public async Task<string> GenerateSchemaScriptAsync(string dacpacPath, string outputPath)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".";
            Directory.CreateDirectory(directory);

            List<string> arguments = new List<string>
            {
                "/Action:Script",
                "/SourceFile:" + dacpacPath,
                "/TargetFile:" + outputPath,
                "/TargetServerVersion:Sql160",
                "/p:ScriptDatabaseOptions=False",
                "/p:ScriptDeployStateChecks=False",
                "/p:ScriptDatabaseCompatibility=False",
                "/p:IncludeTransactionalScripts=False",
                "/p:BlockOnPossibleDataLoss=False"
            };

            await RunAsync(arguments);
            if (!File.Exists(outputPath))
                throw new InvalidOperationException("sqlpackage did not produce a script at " + outputPath);
            return outputPath;
        }

        private async Task RunAsync(IEnumerable<string> arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo(_sqlPackagePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (string argument in arguments)
                info.ArgumentList.Add(argument);

            using Process process = Process.Start(info)
                ?? throw new InvalidOperationException("Could not start '" + _sqlPackagePath + "'.");

            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            string output = await stdout;
            string error = await stderr;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "'" + _sqlPackagePath + "' failed with exit code " + process.ExitCode + "." +
                    Environment.NewLine + output + Environment.NewLine + error);
            }
        }
    }
}
