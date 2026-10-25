// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.licensing
{
    /// <summary>
    /// Payload of a signed iPlus license file. This is the data that is
    /// serialized to JSON, signed with the issuer's private key and shipped
    /// as a <c>*.gip</c> file.
    /// </summary>
    /// <remarks>
    /// The class is intentionally a plain POCO (no attributes, no framework
    /// dependencies) so that it can be serialized with any JSON serializer and
    /// consumed by the framework, the issuer CLI and commercial modules alike.
    /// </remarks>
    public sealed class LicensePayload
    {
        /// <summary>
        /// Format version of the payload. Bump when the schema changes in an
        /// incompatible way; <see cref="LicenseVerifier"/> rejects unknown
        /// versions instead of guessing.
        /// </summary>
        public int FormatVersion { get; set; } = LicenseFormat.CurrentVersion;

        /// <summary>
        /// Unique id of this license (used for revocation lists and auditing).
        /// </summary>
        public Guid LicenseId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kind of license (developer / product).
        /// </summary>
        public LicenseKind Kind { get; set; }

        /// <summary>
        /// Human readable customer name (company or person).
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Project / order number the license was issued for.
        /// </summary>
        public string ProjectNo { get; set; } = string.Empty;

        /// <summary>
        /// Name of the company that issued the license (gip for developer
        /// licenses, the developer company for product licenses).
        /// </summary>
        public string IssuerName { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp of issuance.
        /// </summary>
        public DateTimeOffset IssuedUtc { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Optional expiry. <c>null</c> means perpetual.
        /// </summary>
        public DateTimeOffset? ExpiresUtc { get; set; }

        /// <summary>
        /// Optional maximum number of concurrent users. <c>null</c> = unlimited.
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Package names (ACPackageName) covered by this license.
        /// Empty for developer licenses (they cover everything the developer builds).
        /// </summary>
        public List<string> Packages { get; set; } = new List<string>();

        /// <summary>
        /// Fingerprint of the database this license is bound to
        /// (see <see cref="DatabaseFingerprint"/>). Empty for developer licenses
        /// that are not bound to a database.
        /// </summary>
        public string DatabaseFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Optional fingerprint of the machine the license is bound to.
        /// Empty when the license is only database-bound.
        /// </summary>
        public string MachineFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Free-form remarks shown in the license manager.
        /// </summary>
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// Public key (PEM) of the developer that is authorized to issue
        /// <see cref="LicenseKind.Product"/> licenses.
        /// Only meaningful for <see cref="LicenseKind.DeveloperIssuer"/> licenses:
        /// gip signs the developer license with the framework key and embeds the
        /// developer's own public key here. The framework then accepts product
        /// licenses that are signed with the matching developer private key.
        /// This is the second level of the two-level PKI chain.
        /// </summary>
        public string IssuerPublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the issuer key that signed this license
        /// (SHA-256 thumbprint of the issuer public key, hex, first 16 chars).
        /// Only meaningful for <see cref="LicenseKind.Product"/> licenses so that
        /// the framework can pick the right issuer key without trying all of them.
        /// </summary>
        public string IssuerKeyId { get; set; } = string.Empty;

        /// <summary>
        /// Returns <c>true</c> when the payload contains the given package name
        /// (case-insensitive). Developer licenses always return <c>true</c>.
        /// A framework runtime license does not license packages — it only
        /// grants the right to run the framework for <see cref="MaxUsers"/>
        /// concurrent users.
        /// </summary>
        public bool CoversPackage(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return false;

            if (Kind == LicenseKind.DeveloperIssuer || Kind == LicenseKind.DeveloperEndUser)
                return true;

            if (Kind == LicenseKind.FrameworkRuntime)
                return false;

            return Packages.Any(p => string.Equals(p, packageName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns <c>true</c> when the license has not expired at the given point in time.
        /// </summary>
        public bool IsTimeValid(DateTimeOffset nowUtc)
        {
            return !ExpiresUtc.HasValue || ExpiresUtc.Value > nowUtc;
        }
    }
}
