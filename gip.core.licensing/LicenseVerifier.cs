// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace gip.core.licensing
{
    /// <summary>
    /// Verifies signed license files against a public key and an optional
    /// database/machine binding.
    /// </summary>
    /// <remarks>
    /// The verifier is stateless and thread-safe. It never performs network
    /// calls — validation is fully offline, which is a hard requirement for
    /// on-premise MES installations.
    /// </remarks>
    public sealed class LicenseVerifier
    {
        private readonly RSA _publicKey;
        private readonly Func<string, RSA?>? _issuerKeyResolver;

        /// <summary>
        /// Creates a verifier for the given public key.
        /// </summary>
        /// <param name="publicKey">RSA public key (PEM or legacy XML).</param>
        public LicenseVerifier(RSA publicKey)
            : this(publicKey, null)
        {
        }

        /// <summary>
        /// Creates a verifier for the given public key with an optional resolver
        /// for developer issuer keys (second level of the PKI chain).
        /// </summary>
        /// <param name="publicKey">RSA public key of the framework (first PKI level).</param>
        /// <param name="issuerKeyResolver">
        /// Resolves a developer public key by <see cref="LicensePayload.IssuerKeyId"/>.
        /// Required to accept <see cref="LicenseKind.Product"/> licenses that were
        /// signed by a developer instead of by gip. May be <c>null</c>.
        /// </param>
        public LicenseVerifier(RSA publicKey, Func<string, RSA?>? issuerKeyResolver)
        {
            _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
            _issuerKeyResolver = issuerKeyResolver;
        }

        /// <summary>
        /// Creates a verifier from a public key in PEM or legacy XML text.
        /// </summary>
        public static LicenseVerifier FromPublicKeyText(string publicKeyText)
        {
            return new LicenseVerifier(LicenseKeys.ImportPublicKey(publicKeyText));
        }

        /// <summary>
        /// Creates a verifier from a public key file.
        /// </summary>
        public static LicenseVerifier FromPublicKeyFile(string path)
        {
            return new LicenseVerifier(LicenseKeys.LoadKeyFromFile(path, privateKey: false));
        }

        /// <summary>
        /// Verifies a license file text.
        /// </summary>
        /// <param name="licenseText">Complete license file content.</param>
        /// <param name="context">Optional binding context (database/machine fingerprint, package).</param>
        /// <param name="nowUtc">Point in time used for the expiry check. Defaults to <see cref="DateTimeOffset.UtcNow"/>.</param>
        public LicenseValidationResult Verify(string licenseText, LicenseBindingContext? context = null, DateTimeOffset? nowUtc = null)
        {
            if (!LicenseFile.TryParse(licenseText, out byte[] signature, out byte[] payloadBytes, out string? parseError))
                return LicenseValidationResult.Failure(LicenseValidationError.Malformed, parseError ?? "License file could not be parsed.");

            LicensePayload? payload;
            try
            {
                payload = LicenseFile.DeserializePayload(Encoding.UTF8.GetString(payloadBytes));
            }
            catch (LicenseFormatException e)
            {
                return LicenseValidationResult.Failure(LicenseValidationError.Malformed, e.Message);
            }

            if (payload.FormatVersion != LicenseFormat.CurrentVersion)
            {
                return LicenseValidationResult.Failure(
                    LicenseValidationError.UnsupportedFormatVersion,
                    $"License format version {payload.FormatVersion} is not supported (expected {LicenseFormat.CurrentVersion}).",
                    payload);
            }

            bool signatureValid;
            try
            {
                signatureValid = VerifySignature(payload, payloadBytes, signature);
            }
            catch (CryptographicException e)
            {
                return LicenseValidationResult.Failure(LicenseValidationError.InvalidSignature, "Signature verification failed: " + e.Message, payload);
            }

            if (!signatureValid)
                return LicenseValidationResult.Failure(LicenseValidationError.InvalidSignature, "License signature does not match the payload.", payload);

            DateTimeOffset now = nowUtc ?? DateTimeOffset.UtcNow;
            if (!payload.IsTimeValid(now))
            {
                return LicenseValidationResult.Failure(
                    LicenseValidationError.Expired,
                    $"License expired on {payload.ExpiresUtc:yyyy-MM-dd}.",
                    payload);
            }

            if (context != null)
            {
                if (!string.IsNullOrEmpty(payload.DatabaseFingerprint)
                    && !string.Equals(payload.DatabaseFingerprint, context.DatabaseFingerprint, StringComparison.OrdinalIgnoreCase))
                {
                    return LicenseValidationResult.Failure(
                        LicenseValidationError.DatabaseMismatch,
                        "License is bound to a different database.",
                        payload);
                }

                if (!string.IsNullOrEmpty(payload.MachineFingerprint)
                    && !string.Equals(payload.MachineFingerprint, context.MachineFingerprint, StringComparison.OrdinalIgnoreCase))
                {
                    return LicenseValidationResult.Failure(
                        LicenseValidationError.MachineMismatch,
                        "License is bound to a different machine.",
                        payload);
                }

                if (!string.IsNullOrEmpty(context.RequiredPackage) && !payload.CoversPackage(context.RequiredPackage))
                {
                    return LicenseValidationResult.Failure(
                        LicenseValidationError.PackageNotLicensed,
                        $"Package '{context.RequiredPackage}' is not covered by this license.",
                        payload);
                }

                if (context.RequiredKind.HasValue && payload.Kind != context.RequiredKind.Value)
                {
                    return LicenseValidationResult.Failure(
                        LicenseValidationError.WrongLicenseKind,
                        $"License kind '{payload.Kind}' does not match the required kind '{context.RequiredKind.Value}'.",
                        payload);
                }
            }

            return LicenseValidationResult.Success(payload);
        }

        /// <summary>
        /// Verifies a license file on disk.
        /// </summary>
        public LicenseValidationResult VerifyFile(string path, LicenseBindingContext? context = null, DateTimeOffset? nowUtc = null)
        {
            return Verify(LicenseFile.Read(path), context, nowUtc);
        }

        /// <summary>
        /// Verifies the signature using the two-level PKI chain:
        /// <list type="number">
        /// <item>The framework key (first level) — signs developer licenses and
        /// framework runtime licenses.</item>
        /// <item>The developer issuer key (second level) — signs product licenses.
        /// It is resolved through <see cref="LicensePayload.IssuerKeyId"/>.</item>
        /// </list>
        /// </summary>
        private bool VerifySignature(LicensePayload payload, byte[] payloadBytes, byte[] signature)
        {
            if (_publicKey.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return true;

            // Product licenses may be signed by a developer key instead of the
            // framework key. Only try the second level when the payload names a
            // key id and a resolver is configured.
            if (payload.Kind != LicenseKind.Product
                || string.IsNullOrEmpty(payload.IssuerKeyId)
                || _issuerKeyResolver == null)
            {
                return false;
            }

            RSA? issuerKey = _issuerKeyResolver(payload.IssuerKeyId);
            if (issuerKey == null)
                return false;

            return issuerKey.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    /// <summary>
    /// Context against which a license is validated (database/machine binding
    /// and the package that is about to be loaded).
    /// </summary>
    public sealed class LicenseBindingContext
    {
        /// <summary>
        /// Fingerprint of the current database (see <see cref="DatabaseFingerprint"/>).
        /// </summary>
        public string DatabaseFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Fingerprint of the current machine (see <see cref="MachineFingerprint"/>).
        /// </summary>
        public string MachineFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Package that must be covered by the license. Optional.
        /// </summary>
        public string? RequiredPackage { get; set; }

        /// <summary>
        /// License kind that is required for the operation. Optional.
        /// </summary>
        public LicenseKind? RequiredKind { get; set; }
    }
}
