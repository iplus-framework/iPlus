// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Configuration that describes which tables of the reference database
    /// contain seed/master data that must be part of the initial install script.
    /// </summary>
    /// <remarks>
    /// Customer data (production orders, postings, logs, ...) must never be
    /// exported. The list is therefore explicit and lives in
    /// <c>seed-tables.json</c> next to the tool, so that it can be reviewed and
    /// versioned together with the schema.
    /// </remarks>
    public sealed class SeedConfiguration
    {
        /// <summary>
        /// Schema that is used when a table entry does not specify one.
        /// </summary>
        public string DefaultSchema { get; set; } = "dbo";

        /// <summary>
        /// Tables whose rows are exported.
        /// </summary>
        public List<SeedTable> Tables { get; set; } = new List<SeedTable>();

        /// <summary>
        /// Tables that are always excluded, even if they match a wildcard entry.
        /// </summary>
        public List<string> ExcludeTables { get; set; } = new List<string>();
    }

    /// <summary>
    /// One table (or wildcard pattern) whose rows are exported.
    /// </summary>
    public sealed class SeedTable
    {
        /// <summary>
        /// Table name. A trailing <c>*</c> acts as a prefix wildcard
        /// (e.g. <c>ACClass*</c> matches <c>ACClass</c>, <c>ACClassProperty</c>, ...).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional schema override.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Optional SQL filter (without the <c>WHERE</c> keyword) that restricts
        /// the exported rows, e.g. <c>IsCustomerData = 0</c>.
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// Columns that must not be exported (e.g. machine specific values).
        /// </summary>
        public List<string> ExcludeColumns { get; set; } = new List<string>();

        /// <summary>
        /// When <c>true</c> the rows are only inserted when the table is empty.
        /// Defaults to <c>true</c> (idempotent initial install).
        /// </summary>
        public bool InsertOnlyWhenEmpty { get; set; } = true;

        [JsonIgnore]
        public bool IsWildcard => Name.EndsWith("*", StringComparison.Ordinal);
    }
}
