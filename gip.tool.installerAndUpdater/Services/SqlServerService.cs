// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gip.tool.installerAndUpdater.Models;
using Microsoft.Data.SqlClient;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// SQL Server operations needed by the installer: connectivity checks,
    /// database creation, script execution and login provisioning.
    /// </summary>
    /// <remarks>
    /// Everything here is cross-platform. The V4 implementation read the backup
    /// directory through <c>xp_instance_regread</c>, which only exists on
    /// Windows; that call is now guarded and simply skipped elsewhere.
    /// </remarks>
    public sealed class SqlServerService
    {
        /// <summary>
        /// Opens a connection to <c>master</c> and reports whether it worked.
        /// </summary>
        public async Task<(bool Ok, string Error)> TestConnectionAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
                await connection.OpenAsync(cancellationToken);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Returns the SQL Server version string, e.g. "Microsoft SQL Server 2019".
        /// </summary>
        public async Task<string> GetServerVersionAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using SqlCommand command = new SqlCommand(
                "SELECT CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128)) + ' / ' + " +
                "CAST(SERVERPROPERTY('ProductLevel') AS nvarchar(128)) + ' / ' + " +
                "CAST(SERVERPROPERTY('Edition') AS nvarchar(128))", connection);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Lists the databases visible on the server.
        /// </summary>
        public async Task<List<string>> GetDatabasesAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            List<string> databases = new List<string>();

            await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using SqlCommand command = new SqlCommand(
                "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", connection);

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                databases.Add(reader.GetString(0));

            return databases;
        }

        /// <summary>
        /// Returns <c>true</c> when the database already exists.
        /// </summary>
        public async Task<bool> DatabaseExistsAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
            await connection.OpenAsync(cancellationToken);

            await using SqlCommand command = new SqlCommand(
                "SELECT COUNT(*) FROM sys.databases WHERE name = @name", connection);
            command.Parameters.AddWithValue("@name", settings.DatabaseName);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result ?? 0) > 0;
        }

        /// <summary>
        /// Creates an empty database.
        /// </summary>
        public async Task CreateDatabaseAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
            await connection.OpenAsync(cancellationToken);

            // Database names cannot be parameterised; quote the identifier instead.
            string quoted = "[" + settings.DatabaseName.Replace("]", "]]") + "]";

            await using SqlCommand command = new SqlCommand(
                "IF DB_ID(@name) IS NULL EXEC('CREATE DATABASE " + quoted.Replace("'", "''") + "')", connection);
            command.Parameters.AddWithValue("@name", settings.DatabaseName);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Executes a <c>.sql</c> script that uses <c>GO</c> batch separators.
        /// </summary>
        public async Task ExecuteScriptAsync(
            SqlConnectionSettings settings,
            string scriptPath,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("SQL script not found.", scriptPath);

            string script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            await ExecuteScriptTextAsync(settings, script, Path.GetFileName(scriptPath), progress, cancellationToken);
        }

        /// <summary>
        /// Executes SQL text that uses <c>GO</c> batch separators.
        /// </summary>
        public async Task ExecuteScriptTextAsync(
            SqlConnectionSettings settings,
            string script,
            string scriptName,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            List<string> batches = SplitBatches(script);
            if (batches.Count == 0)
                return;

            await using SqlConnection connection = new SqlConnection(settings.BuildApplicationConnectionString());
            await connection.OpenAsync(cancellationToken);

            for (int i = 0; i < batches.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using SqlCommand command = new SqlCommand(batches[i], connection)
                {
                    CommandTimeout = 0
                };
                await command.ExecuteNonQueryAsync(cancellationToken);

                progress?.Report(new ProgressReport(
                    $"Executing {scriptName}: batch {i + 1} of {batches.Count}",
                    (double)(i + 1) / batches.Count));
            }
        }

        /// <summary>
        /// Splits a script on <c>GO</c> lines, ignoring occurrences inside
        /// comments and string literals.
        /// </summary>
        public static List<string> SplitBatches(string script)
        {
            List<string> batches = new List<string>();
            if (string.IsNullOrWhiteSpace(script))
                return batches;

            string[] lines = script.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            System.Text.StringBuilder current = new System.Text.StringBuilder();

            foreach (string line in lines)
            {
                if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
                {
                    string batch = current.ToString().Trim();
                    if (batch.Length > 0)
                        batches.Add(batch);
                    current.Clear();
                }
                else
                {
                    current.AppendLine(line);
                }
            }

            string tail = current.ToString().Trim();
            if (tail.Length > 0)
                batches.Add(tail);

            return batches;
        }

        /// <summary>
        /// Creates a SQL login and maps it to a database user with the given role.
        /// </summary>
        public async Task EnsureLoginAsync(
            SqlConnectionSettings settings,
            string loginName,
            string password,
            string role,
            CancellationToken cancellationToken)
        {
            await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
            await connection.OpenAsync(cancellationToken);

            string quotedLogin = "[" + loginName.Replace("]", "]]") + "]";
            string quotedPassword = password.Replace("'", "''");
            string quotedDatabase = "[" + settings.DatabaseName.Replace("]", "]]") + "]";
            string quotedRole = "[" + role.Replace("]", "]]") + "]";

            string sql =
                "IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @login)\n" +
                "    EXEC('CREATE LOGIN " + quotedLogin.Replace("'", "''") + " WITH PASSWORD = N''" + quotedPassword + "'', CHECK_POLICY = OFF');\n" +
                "ELSE\n" +
                "    EXEC('ALTER LOGIN " + quotedLogin.Replace("'", "''") + " WITH PASSWORD = N''" + quotedPassword + "''');\n";

            await using (SqlCommand command = new SqlCommand(sql, connection) { CommandTimeout = 0 })
            {
                command.Parameters.AddWithValue("@login", loginName);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            string userSql =
                "USE " + quotedDatabase + ";\n" +
                "IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @login)\n" +
                "    EXEC('CREATE USER " + quotedLogin.Replace("'", "''") + " FOR LOGIN " + quotedLogin.Replace("'", "''") + "');\n" +
                "IF IS_ROLEMEMBER('" + role.Replace("'", "''") + "', @login) = 0\n" +
                "    EXEC sp_addrolemember '" + role.Replace("'", "''") + "', @login;\n";

            await using (SqlCommand command = new SqlCommand(userSql, connection) { CommandTimeout = 0 })
            {
                command.Parameters.AddWithValue("@login", loginName);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Returns the default backup directory of the server, or an empty string
        /// when it cannot be determined (non-Windows SQL Server, permissions).
        /// </summary>
        public async Task<string> GetDefaultBackupDirectoryAsync(
            SqlConnectionSettings settings,
            CancellationToken cancellationToken)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(settings.BuildServerConnectionString());
                await connection.OpenAsync(cancellationToken);

                await using SqlCommand command = new SqlCommand(
                    "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))", connection);

                object? result = await command.ExecuteScalarAsync(cancellationToken);
                return result?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Discovers SQL Server instances reachable from this machine.
        /// </summary>
        /// <remarks>
        /// SQL Browser enumeration (<c>SqlDataSourceEnumerator</c>) is Windows-only
        /// and unreliable across firewalls, so this returns a small set of
        /// conventional names instead and lets the user type anything else.
        /// </remarks>
        public static List<string> GetSuggestedServerNames()
        {
            List<string> names = new List<string>
            {
                "localhost",
                @".\SQLEXPRESS",
                @"(localdb)\MSSQLLocalDB",
                "localhost,1433"
            };

            if (OperatingSystem.IsWindows())
                names.Insert(0, System.Environment.MachineName);

            return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
