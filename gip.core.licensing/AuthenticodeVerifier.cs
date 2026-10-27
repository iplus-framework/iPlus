// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace gip.core.licensing
{
    /// <summary>
    /// Result of an Authenticode signature check.
    /// </summary>
    public sealed class AuthenticodeResult
    {
        /// <summary>
        /// <c>true</c> when the file carries a valid Authenticode signature that
        /// chains to a trusted root.
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// <c>true</c> when the signature is valid and the certificate chains to a
        /// trusted root.
        /// </summary>
        public bool IsTrusted { get; set; }

        /// <summary>
        /// Subject of the signing certificate, e.g. <c>CN=gipSoft d.o.o.</c>.
        /// </summary>
        public string SignerSubject { get; set; } = string.Empty;

        /// <summary>
        /// SHA-1 thumbprint of the signing certificate (uppercase, no separators).
        /// </summary>
        public string SignerThumbprint { get; set; } = string.Empty;

        /// <summary>
        /// SHA-256 thumbprint of the signing certificate.
        /// </summary>
        public string SignerThumbprintSha256 { get; set; } = string.Empty;

        /// <summary>
        /// Human readable status, suitable for logs.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// <c>true</c> when the platform cannot check Authenticode at all
        /// (Linux, macOS). Callers must treat this as "not verifiable", not as
        /// "invalid".
        /// </summary>
        public bool IsPlatformUnsupported { get; set; }

        public override string ToString()
        {
            return Status;
        }
    }

    /// <summary>
    /// Reads the Authenticode signature of a PE file (Windows <c>.exe</c> /
    /// <c>.dll</c>) and reports who signed it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Authenticode is the only mechanism that proves <em>who built</em> a
    /// binary. Strong-name signing (<c>GIP-SNKey.snk</c>) does not: the key is
    /// public, so anyone can produce a valid strong name. The
    /// <c>iPlusLicense</c> assembly metadata is a plain string and therefore
    /// forgeable. Authenticode fills that gap.
    /// </para>
    /// <para>
    /// On Windows the check uses <c>WinVerifyTrust</c> from <c>wintrust.dll</c>
    /// and then extracts the signer certificate from the embedded PKCS#7
    /// signature. On Linux and macOS the check is a no-op that reports
    /// <see cref="AuthenticodeResult.IsPlatformUnsupported"/>; the runtime must
    /// not refuse to start there.
    /// </para>
    /// <para>
    /// This type is deliberately dependency-free (BCL only) so that
    /// <c>gip.core.datamodel</c> can use it without pulling in extra packages.
    /// </para>
    /// </remarks>
    public static class AuthenticodeVerifier
    {
        private const int WTD_UI_NONE = 2;
        private const int WTD_REVOKE_NONE = 0;
        private const int WTD_CHOICE_FILE = 1;
        private const int WTD_STATEACTION_VERIFY = 1;
        private const int WTD_STATEACTION_CLOSE = 2;
        private const int WTD_SAFER_FLAG = 0x100;

        private const uint TRUST_E_NOSIGNATURE = 0x800B0100;
        private const uint TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800B0003;
        private const uint TRUST_E_PROVIDER_UNKNOWN = 0x800B0001;

        /// <summary>
        /// Verifies the Authenticode signature of <paramref name="filePath"/>.
        /// </summary>
        public static AuthenticodeResult Verify(string filePath)
        {
            AuthenticodeResult result = new AuthenticodeResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Status = "File not found.";
                return result;
            }

            if (!OperatingSystem.IsWindows())
            {
                result.IsPlatformUnsupported = true;
                result.Status = "Authenticode verification is only available on Windows.";
                return result;
            }

            try
            {
                uint status = VerifyTrust(filePath);
                result.IsSigned = status == 0;
                result.IsTrusted = status == 0;

                if (status != 0)
                {
                    result.Status = DescribeStatus(status);
                    // Even an untrusted signature may carry a certificate; try to
                    // read it so that the log can name the publisher.
                    TryReadSigner(filePath, result);
                    return result;
                }

                TryReadSigner(filePath, result);

                if (string.IsNullOrEmpty(result.SignerSubject))
                    result.Status = "Signature is valid but the signer certificate could not be read.";
                else
                    result.Status = "Signed by " + result.SignerSubject;

                return result;
            }
            catch (Exception ex)
            {
                result.Status = "Authenticode check failed: " + ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the file is signed by one of the given
        /// certificate thumbprints (SHA-1 or SHA-256, case-insensitive, ignoring
        /// spaces and separators).
        /// </summary>
        public static bool IsSignedByAnyOf(string filePath, IEnumerable<string> trustedThumbprints)
        {
            if (trustedThumbprints == null)
                return false;

            AuthenticodeResult result = Verify(filePath);
            if (!result.IsTrusted)
                return false;

            string sha1 = NormalizeThumbprint(result.SignerThumbprint);
            string sha256 = NormalizeThumbprint(result.SignerThumbprintSha256);

            foreach (string thumbprint in trustedThumbprints)
            {
                string normalized = NormalizeThumbprint(thumbprint);
                if (normalized.Length == 0)
                    continue;

                if (string.Equals(normalized, sha1, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, sha256, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes spaces, dashes and colons from a thumbprint and upper-cases it.
        /// </summary>
        public static string NormalizeThumbprint(string thumbprint)
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
                return string.Empty;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(thumbprint.Length);
            foreach (char c in thumbprint)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToUpperInvariant(c));
            }

            return builder.ToString();
        }

        private static uint VerifyTrust(string filePath)
        {
            WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO
            {
                cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>(),
                pcwszFilePath = filePath,
                hFile = IntPtr.Zero,
                pgKnownSubject = IntPtr.Zero
            };

            IntPtr fileInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());
            IntPtr dataPtr = IntPtr.Zero;

            try
            {
                Marshal.StructureToPtr(fileInfo, fileInfoPtr, false);

                WINTRUST_DATA data = new WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>(),
                    pPolicyCallbackData = IntPtr.Zero,
                    pSIPClientData = IntPtr.Zero,
                    dwUIChoice = WTD_UI_NONE,
                    fdwRevocationChecks = WTD_REVOKE_NONE,
                    dwUnionChoice = WTD_CHOICE_FILE,
                    pFile = fileInfoPtr,
                    dwStateAction = WTD_STATEACTION_VERIFY,
                    hWVTStateData = IntPtr.Zero,
                    pwszURLReference = null,
                    dwProvFlags = WTD_SAFER_FLAG,
                    dwUIContext = 0
                };

                dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_DATA>());
                Marshal.StructureToPtr(data, dataPtr, false);

                Guid actionId = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE"); // WINTRUST_ACTION_GENERIC_VERIFY_V2
                uint status = WinVerifyTrust(IntPtr.Zero, ref actionId, dataPtr);

                // Always close the state, otherwise WinVerifyTrust leaks memory.
                data.dwStateAction = WTD_STATEACTION_CLOSE;
                Marshal.StructureToPtr(data, dataPtr, false);
                WinVerifyTrust(IntPtr.Zero, ref actionId, dataPtr);

                return status;
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtr);
                Marshal.FreeHGlobal(fileInfoPtr);
            }
        }

        private static void TryReadSigner(string filePath, AuthenticodeResult result)
        {
            try
            {
                X509Certificate2 certificate = X509CertificateLoader.LoadCertificateFromFile(filePath);
                result.SignerSubject = certificate.Subject;
                result.SignerThumbprint = certificate.Thumbprint ?? string.Empty;
                result.SignerThumbprintSha256 = certificate.GetCertHashString(HashAlgorithmName.SHA256);
                certificate.Dispose();
            }
            catch
            {
                // The file may be unsigned or the certificate may be unreadable.
            }
        }

        private static string DescribeStatus(uint status)
        {
            switch (status)
            {
                case TRUST_E_NOSIGNATURE:
                    return "File is not Authenticode-signed.";
                case TRUST_E_SUBJECT_FORM_UNKNOWN:
                case TRUST_E_PROVIDER_UNKNOWN:
                    return "File has no Authenticode signature.";
                case 0x800B0109: // CERT_E_UNTRUSTEDROOT
                    return "Signature chains to an untrusted root (self-signed certificate?).";
                case 0x800B0101: // CERT_E_EXPIRED
                    return "Signing certificate has expired.";
                case 0x800B010C: // CERT_E_REVOKED
                    return "Signing certificate has been revoked.";
                case 0x80096010: // TRUST_E_BAD_DIGEST
                    return "Signature is invalid: the file was modified after signing.";
                default:
                    return "Authenticode verification failed (0x" + status.ToString("X8") + ").";
            }
        }

        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern uint WinVerifyTrust(IntPtr hwnd, ref Guid pgActionID, IntPtr pWVTData);

        [StructLayout(LayoutKind.Sequential)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public int dwUIChoice;
            public int fdwRevocationChecks;
            public int dwUnionChoice;
            public IntPtr pFile;
            public int dwStateAction;
            public IntPtr hWVTStateData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? pwszURLReference;
            public int dwProvFlags;
            public int dwUIContext;
        }
    }
}
