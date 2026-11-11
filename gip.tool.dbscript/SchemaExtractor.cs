// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
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
        /// <remarks>
        /// The generated script is a "deploy dacpac to an empty database" script: it
        /// starts with an SQLCMD prelude (:setvar DatabaseName, CREATE/DROP DATABASE
        /// batches) and then contains only CREATE TABLE/VIEW/PROCEDURE statements.
        /// Callers that only need the schema statements can strip the prelude with
        /// <see cref="StripSqlCmdPrelude"/>; the installer runs the script inside an
        /// already created database and must not execute the DROP/CREATE DATABASE part.
        /// </remarks>
        public async Task<string> GenerateSchemaScriptAsync(string dacpacPath, string outputPath, string? targetConnectionString = null)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".";
            Directory.CreateDirectory(directory);

            // SqlPackage keeps database users and their role memberships in the extracted
            // model, marked with an UndeployableElementAnnotation (SQL74502) because the
            // referenced server logins do not exist on the target server. The initial-install
            // script must not create users, so the model is stripped of SqlUser and
            // SqlRoleMembership elements before scripting. Editing model.xml inside the
            // dacpac would break the package checksum; the /ModelFilePath override applies
            // the modified model after the checksum check instead.
            string modelPath = PrepareStrippedModel(dacpacPath);

            List<string> arguments = new List<string>
            {
                "/Action:Script",
                "/SourceFile:" + dacpacPath,
                "/ModelFilePath:" + modelPath,
                // The Script action requires a target definition. It connects to the target
                // server to determine the target database state; a non-existent database on
                // a reachable server makes sqlpackage script a fresh CREATE. Pointing at an
                // unreachable dummy address instead makes sqlpackage block for minutes in
                // connection retries (observed with sqlpackage 170.5.96).
                "/TargetConnectionString:" + (targetConnectionString
                    ?? throw new ArgumentNullException(nameof(targetConnectionString),
                        "A reachable target connection string is required; an unreachable dummy address makes sqlpackage block in connection retries.")),
                "/OutputPath:" + outputPath,
                "/p:ScriptDatabaseOptions=False",
                "/p:ScriptDeployStateChecks=False",
                "/p:ScriptDatabaseCompatibility=False",
                "/p:IncludeTransactionalScripts=False",
                "/p:BlockOnPossibleDataLoss=False"
            };

            await RunAsync(arguments);

            try { File.Delete(modelPath); } catch { /* best effort */ }

            if (!File.Exists(outputPath))
                throw new InvalidOperationException("sqlpackage did not produce a script at " + outputPath);
            return outputPath;
        }

        /// <summary>
        /// Removes the SQLCMD prelude sqlpackage adds to a generated schema script:
        /// the header comment, sqlcmd-mode detection, the USE [master] batch and the
        /// DROP/CREATE DATABASE batches. Everything from the first schema statement
        /// (SET ANSI_NULLS / CREATE / PRINT N'Tabelle ...') onwards is kept.
        /// </summary>
        public static void StripSqlCmdPrelude(string scriptPath)
        {
            List<string> lines = new List<string>(File.ReadAllLines(scriptPath));
            int start = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].TrimStart().StartsWith("CREATE TABLE [", StringComparison.OrdinalIgnoreCase) ||
                    lines[i].TrimStart().StartsWith("PRINT N'Tabelle", StringComparison.OrdinalIgnoreCase))
                {
                    start = i;
                    break;
                }
            }
            if (start < 0)
                throw new InvalidOperationException(
                    "Could not locate the schema section in the generated script '" + scriptPath + "'.");

            File.WriteAllLines(scriptPath, lines.GetRange(start, lines.Count - start));
        }

        /// <summary>
        /// Writes a copy of the dacpac's model.xml without <c>SqlUser</c> and
        /// <c>SqlRoleMembership</c> elements and returns its path.
        /// </summary>
        private static string PrepareStrippedModel(string dacpacPath)
        {
            string tempModelPath = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "-model.xml");

            using (System.IO.Compression.ZipArchive archive =
                System.IO.Compression.ZipFile.OpenRead(dacpacPath))
            {
                ZipArchiveEntry? entry = archive.GetEntry("model.xml")
                    ?? throw new InvalidOperationException("The dacpac does not contain a model.xml entry.");
                using Stream stream = entry.Open();
                using StreamReader reader = new StreamReader(stream);
                string model = reader.ReadToEnd();

                // The element bodies are XML fragments without nested SqlUser elements, so
                // a non-greedy match per element type is sufficient here.
                model = System.Text.RegularExpressions.Regex.Replace(
                    model, @"\s*<Element Type=""SqlUser"".*?</Element>", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                model = System.Text.RegularExpressions.Regex.Replace(
                    model, @"\s*<Element Type=""SqlRoleMembership"".*?</Element>", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.Singleline);

                File.WriteAllText(tempModelPath, model);
            }
            return tempModelPath;
        }

        /// <summary>
        /// Runs a sqlpackage action and validates it produced the expected files.
        /// </summary>
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
