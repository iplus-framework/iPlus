// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Generates an idempotent <c>initial-install.sql</c> (schema + seed data)
    /// from a reference database, for inclusion in the distribution package.
    /// </summary>
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            RootCommand root = new RootCommand(
                "iPlus initial install script generator — emits initial-install.sql (schema + seed data) from a reference database.");

            root.Add(GenerateCommand());
            root.Add(FingerprintCommand());

            return root.Parse(args).InvokeAsync();
        }

        private static Command GenerateCommand()
        {
            Option<string> connectionString = new Option<string>("--connection-string", "-c")
            {
                Description = "Connection string of the reference database (same format as in iPlusModelUpdate.sh).",
                Required = true
            };
            Option<string> output = new Option<string>("--output", "-o")
            {
                Description = "Path of the generated initial-install.sql.",
                DefaultValueFactory = _ => "initial-install.sql"
            };
            Option<string> seedConfig = new Option<string>("--seed-config")
            {
                Description = "Path of the seed table configuration (JSON).",
                DefaultValueFactory = _ => Path.Combine(AppContext.BaseDirectory, "seed-tables.json")
            };
            Option<string> suiteVersion = new Option<string>("--suite-version")
            {
                Description = "Suite version stamped into the script header.",
                DefaultValueFactory = _ => "5.0.0"
            };
            Option<string> sqlPackage = new Option<string>("--sqlpackage")
            {
                Description = "Path of the sqlpackage executable (default: resolved from PATH)."
            };
            Option<bool> seedOnly = new Option<bool>("--seed-only")
            {
                Description = "Emit only the seed data section (no schema)."
            };
            Option<bool> keepDacpac = new Option<bool>("--keep-dacpac")
            {
                Description = "Keep the intermediate .dacpac file next to the output."
            };

            Command command = new Command("generate", "Generates initial-install.sql from a reference database.");
            command.Add(connectionString);
            command.Add(output);
            command.Add(seedConfig);
            command.Add(suiteVersion);
            command.Add(sqlPackage);
            command.Add(seedOnly);
            command.Add(keepDacpac);

            command.SetAction(async (parseResult, cancellationToken) =>
            {
                string cs = parseResult.GetValue(connectionString)!;
                string outputPath = parseResult.GetValue(output)!;
                string seedConfigPath = parseResult.GetValue(seedConfig)!;
                string version = parseResult.GetValue(suiteVersion)!;
                string? sqlPackagePath = parseResult.GetValue(sqlPackage);
                bool seedOnlyMode = parseResult.GetValue(seedOnly);
                bool keepDacpacFile = parseResult.GetValue(keepDacpac);

                if (!File.Exists(seedConfigPath))
                    return Fail("Seed configuration not found: " + seedConfigPath);

                SeedConfiguration configuration;
                try
                {
                    configuration = LoadSeedConfiguration(seedConfigPath);
                }
                catch (Exception e)
                {
                    return Fail("Could not read seed configuration: " + e.Message);
                }

                string? schemaScriptPath = null;
                string? dacpacPath = null;

                if (!seedOnlyMode)
                {
                    SchemaExtractor extractor = new SchemaExtractor(sqlPackagePath);
                    if (!extractor.IsAvailable())
                    {
                        return Fail(
                            "sqlpackage was not found. Install it with 'dotnet tool install -g microsoft.sqlpackage' " +
                            "or pass --sqlpackage <path>. Use --seed-only to skip schema extraction.");
                    }

                    string workDir = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".";
                    Directory.CreateDirectory(workDir);
                    dacpacPath = Path.Combine(workDir, "reference.dacpac");
                    schemaScriptPath = Path.Combine(workDir, "reference-schema.sql");

                    Console.WriteLine("Extracting schema to " + dacpacPath + " ...");
                    await extractor.ExtractDacpacAsync(cs, dacpacPath);

                    Console.WriteLine("Generating schema script ...");
                    await extractor.GenerateSchemaScriptAsync(dacpacPath, schemaScriptPath);
                }

                Console.WriteLine("Reading seed data ...");
                ReferenceDatabaseReader reader = new ReferenceDatabaseReader(cs);
                InitialInstallScriptBuilder builder = new InitialInstallScriptBuilder(reader, configuration);
                InitialInstallResult result = await builder.BuildAsync(
                    schemaScriptPath,
                    outputPath,
                    version,
                    includeSchema: !seedOnlyMode);

                Console.WriteLine();
                Console.WriteLine("Initial install script written to: " + result.OutputPath);
                Console.WriteLine("Exported tables (" + result.ExportedTables.Count + "):");
                foreach (string table in result.ExportedTables)
                    Console.WriteLine("  " + table);

                if (result.Warnings.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Warnings:");
                    foreach (string warning in result.Warnings)
                        Console.WriteLine("  " + warning);
                }

                if (!keepDacpacFile)
                {
                    TryDelete(dacpacPath);
                    TryDelete(schemaScriptPath);
                }

                return 0;
            });

            return command;
        }

        private static Command FingerprintCommand()
        {
            Option<string> connectionString = new Option<string>("--connection-string", "-c")
            {
                Description = "Connection string of the database.",
                Required = true
            };

            Command command = new Command("fingerprint", "Prints the license fingerprint of a database.");
            command.Add(connectionString);

            command.SetAction(async (parseResult, cancellationToken) =>
            {
                string cs = parseResult.GetValue(connectionString)!;
                ReferenceDatabaseReader reader = new ReferenceDatabaseReader(cs);
                string fingerprint = await reader.GetDatabaseFingerprintAsync();
                if (string.IsNullOrEmpty(fingerprint))
                {
                    Console.Error.WriteLine("ERROR: could not read the database metadata.");
                    return 1;
                }

                Console.WriteLine(fingerprint);
                return 0;
            });

            return command;
        }

        private static SeedConfiguration LoadSeedConfiguration(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            SeedConfiguration? configuration = JsonSerializer.Deserialize<SeedConfiguration>(
                File.ReadAllText(path), options);
            if (configuration == null)
                throw new InvalidOperationException("Seed configuration is empty.");
            return configuration;
        }

        private static void TryDelete(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception)
            {
                // Best effort cleanup.
            }
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine("ERROR: " + message);
            return 1;
        }
    }
}
