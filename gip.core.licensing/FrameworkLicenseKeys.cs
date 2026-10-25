// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace gip.core.licensing
{
    /// <summary>
    /// Provides the public key that the framework uses to validate licenses
    /// issued by gip.
    /// </summary>
    /// <remarks>
    /// The key is resolved in this order:
    /// <list type="number">
    /// <item>Environment variable <c>IPLUS_LICENSE_PUBLIC_KEY</c> (path to a PEM/XML file) —
    ///       intended for development and for commercial builds that embed their own key.</item>
    /// <item>Embedded resource <c>FrameworkPublicKey.pem</c> in this assembly — the
    ///       production key that gip commits to the repository.</item>
    /// <item>File <c>FrameworkPublicKey.pem</c> next to the executing assembly.</item>
    /// </list>
    /// If none is found, <see cref="TryGetPublicKey"/> returns <c>false</c> and
    /// license validation reports a configuration error instead of silently
    /// accepting or rejecting everything.
    /// </remarks>
    public static class FrameworkLicenseKeys
    {
        /// <summary>
        /// Environment variable that overrides the framework public key.
        /// </summary>
        public const string PublicKeyEnvironmentVariable = "IPLUS_LICENSE_PUBLIC_KEY";

        /// <summary>
        /// File name of the framework public key.
        /// </summary>
        public const string PublicKeyFileName = "FrameworkPublicKey.pem";

        private static readonly object _lock = new object();
        private static RSA? _cachedKey;
        private static bool _resolved;

        /// <summary>
        /// Returns the framework public key, or <c>null</c> when it is not configured.
        /// </summary>
        public static RSA? TryGetPublicKey()
        {
            lock (_lock)
            {
                if (_resolved)
                    return _cachedKey;

                _resolved = true;
                _cachedKey = ResolvePublicKey();
                return _cachedKey;
            }
        }

        /// <summary>
        /// Returns the framework public key or throws when it is not configured.
        /// </summary>
        public static RSA GetPublicKey()
        {
            RSA? key = TryGetPublicKey();
            if (key == null)
            {
                throw new InvalidOperationException(
                    "The iPlus framework public key is not configured. Provide '" + PublicKeyFileName +
                    "' next to the application, embed it as a resource, or set the environment variable '" +
                    PublicKeyEnvironmentVariable + "'.");
            }
            return key;
        }

        /// <summary>
        /// Creates a verifier that uses the framework public key.
        /// </summary>
        public static LicenseVerifier CreateVerifier()
        {
            return new LicenseVerifier(GetPublicKey());
        }

        /// <summary>
        /// Creates a verifier that uses the framework public key and additionally
        /// accepts <see cref="LicenseKind.Product"/> licenses signed by a developer
        /// key. The developer keys are resolved through
        /// <paramref name="issuerKeyResolver"/> by <see cref="LicensePayload.IssuerKeyId"/>.
        /// </summary>
        public static LicenseVerifier CreateVerifier(Func<string, RSA?>? issuerKeyResolver)
        {
            return new LicenseVerifier(GetPublicKey(), issuerKeyResolver);
        }

        /// <summary>
        /// Clears the cached key. Intended for tests.
        /// </summary>
        public static void ResetCache()
        {
            lock (_lock)
            {
                _resolved = false;
                _cachedKey?.Dispose();
                _cachedKey = null;
            }
        }

        private static RSA? ResolvePublicKey()
        {
            string? envPath = Environment.GetEnvironmentVariable(PublicKeyEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
                return LicenseKeys.LoadKeyFromFile(envPath, privateKey: false);

            Assembly assembly = typeof(FrameworkLicenseKeys).Assembly;
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(PublicKeyFileName, StringComparison.OrdinalIgnoreCase))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        continue;
                    using StreamReader reader = new StreamReader(stream);
                    return LicenseKeys.ImportPublicKey(reader.ReadToEnd());
                }
            }

            string localPath = Path.Combine(AppContext.BaseDirectory, PublicKeyFileName);
            if (File.Exists(localPath))
                return LicenseKeys.LoadKeyFromFile(localPath, privateKey: false);

            return null;
        }
    }
}
