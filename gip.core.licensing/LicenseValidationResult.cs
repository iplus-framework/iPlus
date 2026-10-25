// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace gip.core.licensing
{
    /// <summary>
    /// Result of a license validation. Contains the parsed payload on success and
    /// a machine readable reason on failure so that callers can show a helpful
    /// message instead of a generic "invalid license".
    /// </summary>
    public sealed class LicenseValidationResult
    {
        private LicenseValidationResult(bool isValid, LicensePayload? payload, LicenseValidationError error, string message)
        {
            IsValid = isValid;
            Payload = payload;
            Error = error;
            Message = message;
        }

        /// <summary>
        /// <c>true</c> when the license is valid for the requested context.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Parsed payload. Only set when the file could be parsed and the
        /// signature was valid (even if a later check failed).
        /// </summary>
        public LicensePayload? Payload { get; }

        /// <summary>
        /// Machine readable failure reason.
        /// </summary>
        public LicenseValidationError Error { get; }

        /// <summary>
        /// Human readable failure description (empty on success).
        /// </summary>
        public string Message { get; }

        public static LicenseValidationResult Success(LicensePayload payload)
        {
            return new LicenseValidationResult(true, payload, LicenseValidationError.None, string.Empty);
        }

        public static LicenseValidationResult Failure(LicenseValidationError error, string message, LicensePayload? payload = null)
        {
            return new LicenseValidationResult(false, payload, error, message);
        }

        public override string ToString()
        {
            return IsValid ? "Valid" : Error + ": " + Message;
        }
    }

    /// <summary>
    /// Machine readable reasons why a license was rejected.
    /// </summary>
    public enum LicenseValidationError
    {
        None = 0,
        /// <summary>The file could not be parsed (header/signature/payload).</summary>
        Malformed = 10,
        /// <summary>The payload schema version is not supported by this framework build.</summary>
        UnsupportedFormatVersion = 20,
        /// <summary>The signature does not match the payload with the given public key.</summary>
        InvalidSignature = 30,
        /// <summary>The license has expired.</summary>
        Expired = 40,
        /// <summary>The license is bound to a different database.</summary>
        DatabaseMismatch = 50,
        /// <summary>The license is bound to a different machine.</summary>
        MachineMismatch = 60,
        /// <summary>The requested package is not covered by the license.</summary>
        PackageNotLicensed = 70,
        /// <summary>The license kind is not acceptable for the requested operation.</summary>
        WrongLicenseKind = 80,
        /// <summary>The license was signed by a key that is not trusted for its kind.</summary>
        UntrustedIssuer = 90
    }
}
