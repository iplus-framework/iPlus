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
