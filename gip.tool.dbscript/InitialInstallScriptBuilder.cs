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

            // Rowversion (timestamp) columns are system-generated and read-only:
            // inserting them always fails with "Cannot insert an explicit value
            // into a timestamp column.", so they must not be exported.
            List<string> rowVersionColumns = await _reader.GetRowVersionColumnsAsync(schema, table);
            List<string> removedRowVersions = rowVersionColumns
                .Where(rv => columns.RemoveAll(c => string.Equals(c, rv, StringComparison.OrdinalIgnoreCase)) > 0)
                .ToList();
            if (removedRowVersions.Count > 0)
                warnings.Add("Table " + schema + "." + table + ": excluded rowversion column(s) "
                             + string.Join(", ", removedRowVersions) + ".");

            List<IReadOnlyList<object?>> rows = await _reader.ReadRowsAsync(schema, table, columns, seedTable.Filter);
            if (rows.Count == 0)
                return string.Empty;

            List<(string, string)> selfReferences = await _reader.GetSelfReferencesAsync(schema, table);
            if (selfReferences.Count > 0)
                rows = OrderRowsBySelfReferences(rows, columns, selfReferences, table, warnings);

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
                sb.Append(insert);

            if (hasIdentity)
                sb.Append("    SET IDENTITY_INSERT ").Append(SqlLiteralWriter.QuoteTable(schema, table)).Append(" OFF;\n");

            if (seedTable.InsertOnlyWhenEmpty)
                sb.Append("END\n");

            sb.Append("GO\n");
            return sb.ToString();
        }

        /// <summary>
        /// Stable topological sort of rows satisfying ALL self-referencing FKs of the
        /// table (e.g. ACClass has BasedOnACClassID AND ParentACClassID): no row is
        /// emitted while any of its self-FK parent rows is still pending, so the
        /// inserts pass the constraints without disabling them. Rows whose parent is
        /// not part of the exported set (or NULL parents) impose no constraint.
        /// Cycles are left in original order and reported as a warning.
        /// </summary>
        private List<IReadOnlyList<object?>> OrderRowsBySelfReferences(
            List<IReadOnlyList<object?>> rows,
            List<string> columns,
            List<(string ReferencingColumn, string ReferencedColumn)> selfReferences,
            string table,
            List<string> warnings)
        {
            List<(int ReferencingIndex, int ReferencedIndex)> edges = new List<(int, int)>();
            foreach (var (referencing, referenced) in selfReferences)
            {
                int referencingIndex = columns.FindIndex(
                    c => string.Equals(c, referencing, StringComparison.OrdinalIgnoreCase));
                int referencedIndex = columns.FindIndex(
                    c => string.Equals(c, referenced, StringComparison.OrdinalIgnoreCase));
                edges.Add((referencingIndex, referencedIndex));
            }
            if (edges.Any(e => e.ReferencingIndex < 0 || e.ReferencedIndex < 0))
            {
                warnings.Add("Table " + table + ": self-FK columns not exported, rows left unordered.");
                return rows;
            }

            // Map every FK key -> all row indexes owning that key (one edge may match several rows).
            List<Dictionary<string, int>> positionsByKey = new List<Dictionary<string, int>>();
            foreach ((_, int referencedIndex) in edges)
            {
                Dictionary<string, int> positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rows.Count; i++)
                    positions[SqlLiteralWriter.ToKeyString(rows[i][referencedIndex])] = i;
                positionsByKey.Add(positions);
            }

            bool[] visited = new bool[rows.Count];
            bool[] inStack = new bool[rows.Count];
            List<IReadOnlyList<object?>> result = new List<IReadOnlyList<object?>>(rows.Count);
            var stack = new Stack<int>();

            void Visit(int index)
            {
                if (visited[index] || inStack[index])
                    return;

                inStack[index] = true;
                stack.Push(index);
                try
                {
                    // Visit all self-FK parents first (depth first along every FK edge).
                    for (int e = 0; e < edges.Count; e++)
                    {
                        object? parentKey = rows[index][edges[e].ReferencingIndex];
                        if (parentKey != null
                            && positionsByKey[e].TryGetValue(SqlLiteralWriter.ToKeyString(parentKey), out int parentIndex)
                            && parentIndex != index)
                        {
                            Visit(parentIndex);
                        }
                    }
                }
                finally
                {
                    stack.Pop();
                    inStack[index] = false;
                    visited[index] = true;
                    result.Add(rows[index]);
                }
            }

            for (int i = 0; i < rows.Count; i++)
                Visit(i);

            // Verification: every self-FK parent row must precede its child row.
            int violations = 0;
            Dictionary<IReadOnlyList<object?>, int> resultPosition = new Dictionary<IReadOnlyList<object?>, int>();
            for (int i = 0; i < result.Count; i++)
                resultPosition[result[i]] = i;
            HashSet<string> reportedCycles = new HashSet<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                foreach ((int referencingIndex, _) in edges)
                {
                    object? parentKey = rows[i][referencingIndex];
                    if (parentKey == null)
                        continue;
                    string key = SqlLiteralWriter.ToKeyString(parentKey);
                    for (int e = 0; e < edges.Count; e++)
                    {
                        if (!positionsByKey[e].TryGetValue(key, out int parentIndex))
                            continue;
                        if (parentIndex == i)
                            continue;
                        int childPos = resultPosition[rows[i]];
                        int parentPos = resultPosition[rows[parentIndex]];
                        if (parentPos > childPos)
                        {
                            // Cycle between the two FK edges: cannot be satisfied by ordering.
                            string cycleKey = key;
                            if (reportedCycles.Add(cycleKey))
                                violations++;
                        }
                    }
                }
            }
            int reordered = 0;
            for (int i = 0; i < result.Count; i++)
                if (!ReferenceEquals(result[i], rows[i]))
                    reordered++;
            if (reordered > 0)
                warnings.Add("Table " + table + ": " + reordered + " row(s) reordered by self-referencing hierarchy.");
            if (violations > 0)
                warnings.Add("Table " + table + ": " + violations
                             + " self-FK cycle(s) detected — rows left in original order (manual check needed).");
            return result;
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
    }

    /// <summary>
    /// Result of the initial-install script generation.
    /// </summary>
    public sealed class InitialInstallResult
    {
        public string OutputPath { get; set; } = string.Empty;
        public List<string> ExportedTables { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
