// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace gip.core.licensing
{
    /// <summary>
    /// Computes the database fingerprint that licenses are bound to.
    /// </summary>
    /// <remarks>
    /// The fingerprint is derived from the same SQL Server metadata that the V4
    /// license system used (<c>sys.databases.create_date</c>,
    /// <c>sys.databases.service_broker_guid</c>, <c>sys.database_recovery_status.database_guid</c>).
    /// Keeping the same inputs means an existing installation keeps a stable
    /// identity across restarts, backups and restores — but a copied database
    /// (restored under a different name/instance) gets a different fingerprint,
    /// which is exactly what a per-database product license needs.
    ///
    /// This class does not talk to SQL Server; the caller passes the three
    /// values it read from the database.
    /// </remarks>
    public static class DatabaseFingerprint
    {
        /// <summary>
        /// Computes the fingerprint from the SQL Server metadata values.
        /// </summary>
        /// <param name="createDate">Value of <c>sys.databases.create_date</c>.</param>
        /// <param name="serviceBrokerGuid">Value of <c>sys.databases.service_broker_guid</c>.</param>
        /// <param name="databaseGuid">Value of <c>sys.database_recovery_status.database_guid</c>.</param>
        public static string Compute(DateTime createDate, Guid serviceBrokerGuid, Guid databaseGuid)
        {
            // Same textual composition as the V4 DBInfo.GetCheckSum() so that
            // fingerprints stay comparable with historical data.
            string text = serviceBrokerGuid.ToString()
                + string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}.{2}-{3}.{4}.{5}.{6}",
                    createDate.ToString("dd", CultureInfo.InvariantCulture),
                    createDate.ToString("MM", CultureInfo.InvariantCulture),
                    createDate.ToString("yyyy", CultureInfo.InvariantCulture),
                    createDate.ToString("hh", CultureInfo.InvariantCulture),
                    createDate.ToString("mm", CultureInfo.InvariantCulture),
                    createDate.ToString("ss", CultureInfo.InvariantCulture),
                    createDate.ToString("fff", CultureInfo.InvariantCulture))
                + databaseGuid.ToString();

            return Hash(text);
        }

        /// <summary>
        /// Computes a fingerprint from an already formatted V4 style checksum code
        /// (the dash separated byte list produced by <c>DBInfo.GetCheckSumCode()</c>).
        /// </summary>
        public static string FromLegacyChecksumCode(string legacyCode)
        {
            if (string.IsNullOrWhiteSpace(legacyCode))
                return string.Empty;
            return Hash(legacyCode.Trim());
        }

        private static string Hash(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
