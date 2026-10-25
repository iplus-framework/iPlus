// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Builds the complete <c>initial-install.sql</c> from a schema script and
    /// the seed data of a reference database.
    /// </summary>
    public sealed class InitialInstallScriptBuilder
    {
        private readonly ReferenceDatabaseReader _reader;
        private readonly SeedConfiguration _configuration;

        public InitialInstallScriptBuilder(ReferenceDatabaseReader reader, SeedConfiguration configuration)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Generates the script.
        /// </summary>
        /// <param name="schemaScriptPath">Path of the schema-only script produced by <see cref="SchemaExtractor"/>.</param>
        /// <param name="outputPath">Path of the generated <c>initial-install.sql</c>.</param>
        /// <param name="suiteVersion">Suite version that is stamped into the script header.</param>
        /// <param name="includeSchema">When <c>false</c> only the seed data is emitted.</param>
        public async Task<InitialInstallResult> BuildAsync(
            string? schemaScriptPath,
            string outputPath,
            string suiteVersion,
            bool includeSchema = true)
        {
            List<(string Schema, string Table)> tables = await _reader.GetTablesAsync();
            List<string> warnings = new List<string>();
            List<string> exportedTables = new List<string>();

            StringBuilder sb = new StringBuilder(1024 * 1024);
            WriteHeader(sb, suiteVersion, includeSchema);

            if (includeSchema)
            {
                if (string.IsNullOrWhiteSpace(schemaScriptPath) || !File.Exists(schemaScriptPath))
                    throw new FileNotFoundException("Schema script not found.", schemaScriptPath);

                sb.Append("/* ============================================================\n");
                sb.Append("   SECTION 1: SCHEMA\n");
                sb.Append("   ============================================================ */\n");
                sb.Append(File.ReadAllText(schemaScriptPath, Encoding.UTF8));
                sb.Append("\nGO\n\n");
            }

            sb.Append("/* ============================================================\n");
            sb.Append("   SECTION 2: SEED / MASTER DATA\n");
            sb.Append("   ============================================================ */\n");

            foreach (SeedTable seedTable in _configuration.Tables)
            {
                List<(string Schema, string Table)> matches = ResolveTables(seedTable, tables);
                if (matches.Count == 0)
                {
                    warnings.Add("Seed table '" + seedTable.Name + "' does not exist in the reference database.");
                    continue;
                }

                foreach ((string schema, string table) in matches)
                {
                    if (IsExcluded(schema, table))
                        continue;

                    string section = await BuildTableSectionAsync(schema, table, seedTable, warnings);
                    if (string.IsNullOrEmpty(section))
                        continue;

                    sb.Append(section);
                    exportedTables.Add(schema + "." + table);
                }
            }

            sb.Append("\n/* ============================================================\n");
            sb.Append("   END OF INITIAL INSTALL SCRIPT\n");
            sb.Append("   ============================================================ */\n");

            string? directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(outputPath, sb.ToString(), new UTF8Encoding(false));

            return new InitialInstallResult
            {
                OutputPath = outputPath,
                ExportedTables = exportedTables,
                Warnings = warnings
            };
        }

        private async Task<string> BuildTableSectionAsync(
            string schema,
            string table,
            SeedTable seedTable,
            List<string> warnings)
        {
            List<string> allColumns = await _reader.GetColumnsAsync(schema, table);
            if (allColumns.Count == 0)
            {
                warnings.Add("Table " + schema + "." + table + " has no readable columns.");
                return string.Empty;
            }

            HashSet<string> excluded = new HashSet<string>(
                seedTable.ExcludeColumns ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase);

            List<string> columns = allColumns.Where(c => !excluded.Contains(c)).ToList();
            if (columns.Count == 0)
            {
                warnings.Add("All columns of " + schema + "." + table + " are excluded.");
                return string.Empty;
            }

            List<IReadOnlyList<object?>> rows = await _reader.ReadRowsAsync(schema, table, columns, seedTable.Filter);
            if (rows.Count == 0)
                return string.Empty;

            List<string> identityColumns = await _reader.GetIdentityColumnsAsync(schema, table);
            bool hasIdentity = identityColumns.Count > 0;

            StringBuilder sb = new StringBuilder(rows.Count * columns.Count * 16);
            sb.Append("\n/* --- ").Append(schema).Append('.').Append(table)
              .Append(" (").Append(rows.Count).Append(" rows) --- */\n");

            if (seedTable.InsertOnlyWhenEmpty)
            {
                sb.Append("IF NOT EXISTS (SELECT 1 FROM ").Append(SqlLiteralWriter.QuoteTable(schema, table)).Append(")\n");
                sb.Append("BEGIN\n");
            }

            if (hasIdentity)
                sb.Append("    SET IDENTITY_INSERT ").Append(SqlLiteralWriter.QuoteTable(schema, table)).Append(" ON;\n");

            string insert = SqlLiteralWriter.BuildInsert(
                schema, table, columns, rows,
                emitBatchSeparators: !seedTable.InsertOnlyWhenEmpty);
            if (seedTable.InsertOnlyWhenEmpty)
                sb.Append(Indent(insert, "    "));
            else
                sb.Append(insert);

            if (hasIdentity)
                sb.Append("    SET IDENTITY_INSERT ").Append(SqlLiteralWriter.QuoteTable(schema, table)).Append(" OFF;\n");

            if (seedTable.InsertOnlyWhenEmpty)
                sb.Append("END\n");

            sb.Append("GO\n");
            return sb.ToString();
        }

        private List<(string Schema, string Table)> ResolveTables(
            SeedTable seedTable,
            List<(string Schema, string Table)> tables)
        {
            string schema = string.IsNullOrWhiteSpace(seedTable.Schema) ? _configuration.DefaultSchema : seedTable.Schema;

            if (!seedTable.IsWildcard)
            {
                return tables
                    .Where(t => string.Equals(t.Schema, schema, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(t.Table, seedTable.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            string prefix = seedTable.Name.Substring(0, seedTable.Name.Length - 1);
            return tables
                .Where(t => string.Equals(t.Schema, schema, StringComparison.OrdinalIgnoreCase)
                         && t.Table.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private bool IsExcluded(string schema, string table)
        {
            string fullName = schema + "." + table;
            return _configuration.ExcludeTables.Any(e =>
                string.Equals(e, fullName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, table, StringComparison.OrdinalIgnoreCase));
        }

        private static void WriteHeader(StringBuilder sb, string suiteVersion, bool includeSchema)
        {
            sb.Append("/*\n");
            sb.Append("   iPlus initial install script\n");
            sb.Append("   Generated : ").Append(DateTime.UtcNow.ToString("u")).Append('\n');
            sb.Append("   Suite     : ").Append(suiteVersion).Append('\n');
            sb.Append("   Sections  : ").Append(includeSchema ? "schema + seed data" : "seed data only").Append('\n');
            sb.Append('\n');
            sb.Append("   Apply this script ONLY to a fresh database. Existing installations\n");
            sb.Append("   must skip it and run the incremental DbScripts updates instead\n");
            sb.Append("   (gip.core.dbsyncer / Ctrl-login model sync).\n");
            sb.Append("*/\n\n");
            sb.Append("SET NOCOUNT ON;\n");
            sb.Append("SET XACT_ABORT ON;\n");
            sb.Append("GO\n\n");
        }

        private static string Indent(string text, string indent)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            StringBuilder sb = new StringBuilder(text.Length + lines.Length * indent.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    sb.Append(indent).Append(lines[i]);
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Result of an initial install script generation.
    /// </summary>
    public sealed class InitialInstallResult
    {
        public string OutputPath { get; set; } = string.Empty;
        public List<string> ExportedTables { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
