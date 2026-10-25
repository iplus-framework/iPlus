// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Renders SQL Server literals and INSERT statements for the seed data.
    /// </summary>
    /// <remarks>
    /// All values are rendered as literals (no parameters) because the output is
    /// a standalone script that is executed by the installer / dbsyncer. Every
    /// value is escaped properly, so the generated script is safe to run.
    /// </remarks>
    public static class SqlLiteralWriter
    {
        /// <summary>
        /// Renders a value as a T-SQL literal.
        /// </summary>
        public static string Render(object? value)
        {
            if (value == null || value is DBNull)
                return "NULL";

            switch (value)
            {
                case bool b:
                    return b ? "1" : "0";
                case byte by:
                    return by.ToString(CultureInfo.InvariantCulture);
                case sbyte sb:
                    return sb.ToString(CultureInfo.InvariantCulture);
                case short s:
                    return s.ToString(CultureInfo.InvariantCulture);
                case ushort us:
                    return us.ToString(CultureInfo.InvariantCulture);
                case int i:
                    return i.ToString(CultureInfo.InvariantCulture);
                case uint ui:
                    return ui.ToString(CultureInfo.InvariantCulture);
                case long l:
                    return l.ToString(CultureInfo.InvariantCulture);
                case ulong ul:
                    return ul.ToString(CultureInfo.InvariantCulture);
                case float f:
                    return f.ToString("R", CultureInfo.InvariantCulture);
                case double d:
                    return d.ToString("R", CultureInfo.InvariantCulture);
                case decimal m:
                    return m.ToString(CultureInfo.InvariantCulture);
                case DateTime dt:
                    return "CAST('" + dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "' AS datetime2)";
                case DateTimeOffset dto:
                    return "CAST('" + dto.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture) + "' AS datetimeoffset)";
                case TimeSpan ts:
                    return "CAST('" + ts.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture) + "' AS time)";
                case Guid g:
                    return "'" + g.ToString("D") + "'";
                case byte[] bytes:
                    return bytes.Length == 0 ? "0x" : "0x" + Convert.ToHexString(bytes);
                case string str:
                    return "N'" + str.Replace("'", "''") + "'";
                default:
                    return "N'" + Convert.ToString(value, CultureInfo.InvariantCulture)?.Replace("'", "''") + "'";
            }
        }

        /// <summary>
        /// Renders a bracketed identifier, escaping closing brackets.
        /// </summary>
        public static string QuoteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier is empty.", nameof(identifier));
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        /// <summary>
        /// Renders a fully qualified table name.
        /// </summary>
        public static string QuoteTable(string schema, string table)
        {
            return QuoteIdentifier(schema) + "." + QuoteIdentifier(table);
        }

        /// <summary>
        /// Builds a multi-row INSERT statement.
        /// </summary>
        /// <param name="schema">Table schema.</param>
        /// <param name="table">Table name.</param>
        /// <param name="columns">Column names to insert.</param>
        /// <param name="rows">Row values, in the same order as <paramref name="columns"/>.</param>
        /// <param name="batchSize">Maximum number of rows per INSERT statement.</param>
        /// <param name="emitBatchSeparators">
        /// When <c>true</c> a <c>GO</c> is emitted after every batch. Must be
        /// <c>false</c> when the statement is wrapped in a <c>BEGIN...END</c>
        /// block, because <c>GO</c> terminates the batch and would leave the
        /// block unterminated.
        /// </param>
        public static string BuildInsert(
            string schema,
            string table,
            IReadOnlyList<string> columns,
            IReadOnlyList<IReadOnlyList<object?>> rows,
            int batchSize = 500,
            bool emitBatchSeparators = true)
        {
            if (columns == null || columns.Count == 0)
                throw new ArgumentException("No columns to insert.", nameof(columns));
            if (rows == null || rows.Count == 0)
                return string.Empty;

            string columnList = string.Join(", ", columns.Select(QuoteIdentifier));
            string target = QuoteTable(schema, table);

            StringBuilder sb = new StringBuilder(rows.Count * columns.Count * 16);
            for (int offset = 0; offset < rows.Count; offset += batchSize)
            {
                int count = Math.Min(batchSize, rows.Count - offset);
                sb.Append("INSERT INTO ").Append(target).Append(" (").Append(columnList).Append(") VALUES").Append('\n');
                for (int r = 0; r < count; r++)
                {
                    IReadOnlyList<object?> row = rows[offset + r];
                    sb.Append("    (");
                    for (int c = 0; c < columns.Count; c++)
                    {
                        if (c > 0)
                            sb.Append(", ");
                        sb.Append(Render(c < row.Count ? row[c] : null));
                    }
                    sb.Append(')');
                    sb.Append(r == count - 1 ? ";\n" : ",\n");
                }
                if (emitBatchSeparators)
                    sb.Append("GO\n");
            }

            return sb.ToString();
        }
    }
}
