// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace gip.tool.dbscript
{
    /// <summary>
    /// Reads schema and seed data from a reference database.
    /// </summary>
    public sealed class ReferenceDatabaseReader
    {
        private readonly string _connectionString;

        public ReferenceDatabaseReader(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is empty.", nameof(connectionString));
            _connectionString = connectionString;
        }

        /// <summary>
        /// Returns all user tables of the database.
        /// </summary>
        public async Task<List<(string Schema, string Table)>> GetTablesAsync()
        {
            const string sql = @"
SELECT s.name AS SchemaName, t.name AS TableName
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;";

            List<(string, string)> tables = new List<(string, string)>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                tables.Add((reader.GetString(0), reader.GetString(1)));
            return tables;
        }

        /// <summary>
        /// Returns the column names of a table, in ordinal order.
        /// </summary>
        public async Task<List<string>> GetColumnsAsync(string schema, string table)
        {
            const string sql = @"
SELECT c.name
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = @schema AND t.name = @table
ORDER BY c.column_id;";

            List<string> columns = new List<string>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@schema", SqlDbType.NVarChar, 128) { Value = schema });
            command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = table });
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));
            return columns;
        }

        /// <summary>
        /// Returns the number of rows in a table.
        /// </summary>
        public async Task<long> GetRowCountAsync(string schema, string table)
        {
            string sql = "SELECT COUNT_BIG(*) FROM " + SqlLiteralWriter.QuoteTable(schema, table) + ";";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            object? result = await command.ExecuteScalarAsync();
            return result == null || result is DBNull ? 0 : Convert.ToInt64(result);
        }

        /// <summary>
        /// Reads all rows of a table (optionally filtered) as raw values.
        /// </summary>
        public async Task<List<IReadOnlyList<object?>>> ReadRowsAsync(
            string schema,
            string table,
            IReadOnlyList<string> columns,
            string? filter)
        {
            if (columns == null || columns.Count == 0)
                return new List<IReadOnlyList<object?>>();

            string columnList = string.Join(", ", columns.Select(SqlLiteralWriter.QuoteIdentifier));
            string sql = "SELECT " + columnList + " FROM " + SqlLiteralWriter.QuoteTable(schema, table);
            if (!string.IsNullOrWhiteSpace(filter))
                sql += " WHERE " + filter;
            sql += ";";

            List<IReadOnlyList<object?>> rows = new List<IReadOnlyList<object?>>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = 600;
            await using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            while (await reader.ReadAsync())
            {
                object?[] values = new object?[columns.Count];
                for (int i = 0; i < columns.Count; i++)
                    values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(values);
            }
            return rows;
        }

        /// <summary>
        /// Returns all self-referencing foreign keys as (referencingColumn, referencedColumn).
        /// Tables like ACClass have several (BasedOnACClassID AND ParentACClassID) — every
        /// one must be satisfied by the emitted row order.
        /// </summary>
        public async Task<List<(string ReferencingColumn, string ReferencedColumn)>> GetSelfReferencesAsync(
            string schema, string table)
        {
            const string sql = @"
SELECT fc.name, rc.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns fc ON fc.object_id = fkc.parent_object_id AND fc.column_id = fkc.parent_column_id
INNER JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
WHERE fk.parent_object_id = fk.referenced_object_id
  AND fk.parent_object_id = OBJECT_ID(@qualifiedTable);";

            List<(string, string)> result = new List<(string, string)>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@qualifiedTable", SqlDbType.NVarChar, 384)
                { Value = SqlLiteralWriter.QuoteIdentifier(schema) + "." + SqlLiteralWriter.QuoteIdentifier(table) });
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add((reader.GetString(0), reader.GetString(1)));
            return result;
        }

        /// <summary>
        /// Returns one row per cross-table foreign key: (referencing table,
        /// referenced table, isConstraining). Used to order the table sections
        /// of the install script so parent tables are inserted before their
        /// children — the natural alphabetical order of sys.tables does not
        /// guarantee that (e.g. MDCountry references MDCurrency but sorts
        /// before it). A FK is "constraining" when at least one referencing
        /// column is NOT NULL: nullable FKs can be ignored when resolving
        /// circular references (e.g. ACClass &lt;-&gt; ACProject), but real seed
        /// data may still populate the nullable column, so they are respected
        /// as soft preferences wherever they do not conflict.
        /// </summary>
        public async Task<List<(string ReferencingSchema, string ReferencingTable, string ReferencedSchema, string ReferencedTable, bool IsConstraining)>>
            GetTableDependenciesAsync()
        {
            const string sql = @"
SELECT DISTINCT fs.name, ft.name, ps.name, pt.name,
       MAX(CASE WHEN c.is_nullable = 0 THEN 1 ELSE 0 END) AS hasNotNull
FROM sys.foreign_keys fk
INNER JOIN sys.tables ft ON ft.object_id = fk.parent_object_id
INNER JOIN sys.schemas fs ON fs.schema_id = ft.schema_id
INNER JOIN sys.tables pt ON pt.object_id = fk.referenced_object_id
INNER JOIN sys.schemas ps ON ps.schema_id = pt.schema_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id <> fk.referenced_object_id
GROUP BY fs.name, ft.name, ps.name, pt.name;";

            List<(string, string, string, string, bool)> result = new List<(string, string, string, string, bool)>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4) == 1));
            return result;
        }

        /// <summary>
        /// Returns all rowversion (timestamp) columns of a table. These are
        /// system-generated and read-only — they must never be INSERTed and are
        /// excluded from the export.
        /// </summary>
        public async Task<List<string>> GetRowVersionColumnsAsync(string schema, string table)
        {
            const string sql = @"
SELECT c.name
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = @schema AND t.name = @table
  AND c.system_type_id = TYPE_ID('timestamp')
ORDER BY c.column_id;";

            List<string> columns = new List<string>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@schema", SqlDbType.NVarChar, 128) { Value = schema });
            command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = table });
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));
            return columns;
        }

        /// <summary>
        /// Returns the identity columns of a table (needed for SET IDENTITY_INSERT).
        /// </summary>
        public async Task<List<string>> GetIdentityColumnsAsync(string schema, string table)
        {
            const string sql = @"
SELECT c.name
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = @schema AND t.name = @table AND c.is_identity = 1;";

            List<string> columns = new List<string>();
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@schema", SqlDbType.NVarChar, 128) { Value = schema });
            command.Parameters.Add(new SqlParameter("@table", SqlDbType.NVarChar, 128) { Value = table });
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));
            return columns;
        }

        /// <summary>
        /// Returns the database fingerprint (see <c>gip.core.licensing.DatabaseFingerprint</c>).
        /// </summary>
        public async Task<string> GetDatabaseFingerprintAsync()
        {
            const string sql = @"
SELECT d.create_date, d.service_broker_guid, r.database_guid
FROM sys.databases d
INNER JOIN sys.database_recovery_status r ON d.database_id = r.database_id
WHERE d.name = DB_NAME();";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using SqlCommand command = new SqlCommand(sql, connection);
            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return string.Empty;

            DateTime createDate = reader.GetDateTime(0);
            Guid brokerGuid = reader.GetGuid(1);
            Guid databaseGuid = reader.GetGuid(2);
            return gip.core.licensing.DatabaseFingerprint.Compute(createDate, brokerGuid, databaseGuid);
        }
    }
}
