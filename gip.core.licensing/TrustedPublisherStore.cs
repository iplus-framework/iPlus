// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.licensing
{
    /// <summary>
    /// How strictly the runtime treats assemblies whose Authenticode signature
    /// does not match a trusted publisher.
    /// </summary>
    public enum PublisherVerificationMode
    {
        /// <summary>
        /// Do not check signatures at all. Default, so that existing
        /// installations and non-Windows hosts keep working unchanged.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Check and log, but always allow the assembly to load.
        /// </summary>
        Warn = 10,

        /// <summary>
        /// Refuse to register types from assemblies that are unsigned or signed
        /// by an unknown publisher.
        /// </summary>
        Enforce = 20
    }

    /// <summary>
    /// Stores the list of Authenticode certificate thumbprints that are allowed
    /// to contribute assemblies to the runtime, and decides what happens when an
    /// assembly does not match.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The list lives in <c>&lt;application base&gt;/Licenses/TrustedPublishers.txt</c>
    /// — one thumbprint per line, <c>#</c> starts a comment. This mirrors the
    /// existing license-file layout (<see cref="License"/>.LicenseFolder) and
    /// needs no database access, so it also works during early startup.
    /// </para>
    /// <para>
    /// The mode is read from the same file (<c>mode=warn</c>) or from the
    /// <c>IPLUS_PUBLISHER_VERIFICATION</c> environment variable, which wins.
    /// </para>
    /// <para>
    /// Verification is only possible on Windows; on Linux and macOS the check
    /// reports "not verifiable" and the assembly is always allowed, because
    /// refusing to start there would break every non-Windows deployment.
    /// </para>
    /// </remarks>
    public static class TrustedPublisherStore
    {
        /// <summary>
        /// File name inside the license folder.
        /// </summary>
        public const string FileName = "TrustedPublishers.txt";

        /// <summary>
        /// Environment variable that overrides the mode.
        /// </summary>
        public const string ModeEnvironmentVariable = "IPLUS_PUBLISHER_VERIFICATION";

        private static readonly object _lock = new object();
        private static List<string>? _thumbprints;
        private static PublisherVerificationMode? _mode;
        private static string? _loadedFrom;

        /// <summary>
        /// Returns the configured trusted thumbprints (normalized, upper-case,
        /// no separators).
        /// </summary>
        public static IReadOnlyList<string> GetTrustedThumbprints()
        {
            lock (_lock)
            {
                return EnsureLoaded().ToList();
            }
        }

        /// <summary>
        /// Returns the effective verification mode.
        /// </summary>
        public static PublisherVerificationMode GetMode()
        {
            lock (_lock)
            {
                EnsureLoaded();
                return _mode ?? PublisherVerificationMode.Off;
            }
        }

        /// <summary>
        /// Path of the configuration file that was read (empty when none exists).
        /// </summary>
        public static string LoadedFrom
        {
            get
            {
                lock (_lock)
                {
                    EnsureLoaded();
                    return _loadedFrom ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Decides whether an assembly may contribute types to the runtime.
        /// </summary>
        /// <param name="assemblyPath">Full path of the assembly file.</param>
        /// <param name="reason">Human readable explanation for the log.</param>
        /// <returns><c>true</c> when the assembly is allowed.</returns>
        public static bool IsAllowed(string assemblyPath, out string reason)
        {
            PublisherVerificationMode mode = GetMode();

            if (mode == PublisherVerificationMode.Off)
            {
                reason = "Publisher verification is off.";
                return true;
            }

            if (!OperatingSystem.IsWindows())
            {
                reason = "Authenticode verification is not available on this platform.";
                return true;
            }

            AuthenticodeResult result = AuthenticodeVerifier.Verify(assemblyPath);

            if (result.IsPlatformUnsupported)
            {
                reason = result.Status;
                return true;
            }

            IReadOnlyList<string> trusted = GetTrustedThumbprints();

            if (trusted.Count == 0)
            {
                // Nothing configured: behave like "warn" regardless of the mode,
                // otherwise a fresh installation would refuse every assembly.
                reason = "No trusted publishers configured; " + result.Status;
                return true;
            }

            if (result.IsTrusted && AuthenticodeVerifier.IsSignedByAnyOf(assemblyPath, trusted))
            {
                reason = "Signed by trusted publisher " + result.SignerSubject;
                return true;
            }

            reason = result.IsTrusted
                ? "Signed by untrusted publisher " + result.SignerSubject
                : result.Status;

            return mode != PublisherVerificationMode.Enforce;
        }

        /// <summary>
        /// Replaces the trusted thumbprint list. Intended for tests and for the
        /// license manager UI.
        /// </summary>
        public static void SetTrustedThumbprints(IEnumerable<string> thumbprints)
        {
            lock (_lock)
            {
                _thumbprints = (thumbprints ?? Enumerable.Empty<string>())
                    .Select(AuthenticodeVerifier.NormalizeThumbprint)
                    .Where(t => t.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                _loadedFrom = string.Empty;
            }
        }

        /// <summary>
        /// Overrides the verification mode. Intended for tests.
        /// </summary>
        public static void SetMode(PublisherVerificationMode mode)
        {
            lock (_lock)
            {
                _mode = mode;
            }
        }

        /// <summary>
        /// Clears the cached configuration so that the next call re-reads the
        /// file. Intended for tests.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _thumbprints = null;
                _mode = null;
                _loadedFrom = null;
            }
        }

        private static List<string> EnsureLoaded()
        {
            if (_thumbprints != null)
                return _thumbprints;

            _thumbprints = new List<string>();
            _loadedFrom = string.Empty;

            // Environment variable wins over the file.
            string? environmentMode = Environment.GetEnvironmentVariable(ModeEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentMode)
                && Enum.TryParse(environmentMode, ignoreCase: true, out PublisherVerificationMode parsedMode))
            {
                _mode = parsedMode;
            }

            string path = Path.Combine(AppContext.BaseDirectory, "Licenses", FileName);
            if (!File.Exists(path))
                return _thumbprints;

            _loadedFrom = path;

            try
            {
                foreach (string rawLine in File.ReadAllLines(path))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                        continue;

                    int separator = line.IndexOf('=');
                    if (separator > 0)
                    {
                        string key = line.Substring(0, separator).Trim();
                        string value = line.Substring(separator + 1).Trim();

                        if (string.Equals(key, "mode", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_mode == null
                                && Enum.TryParse(value, ignoreCase: true, out PublisherVerificationMode fileMode))
                            {
                                _mode = fileMode;
                            }
                        }
                        else if (string.Equals(key, "thumbprint", StringComparison.OrdinalIgnoreCase))
                        {
                            AddThumbprint(value);
                        }

                        continue;
                    }

                    AddThumbprint(line);
                }
            }
            catch (Exception)
            {
                // A malformed file must not prevent startup.
            }

            return _thumbprints;
        }

        private static void AddThumbprint(string value)
        {
            string normalized = AuthenticodeVerifier.NormalizeThumbprint(value);
            List<string> thumbprints = EnsureLoaded();
            if (normalized.Length > 0 && !thumbprints.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                thumbprints.Add(normalized);
        }
    }
}
