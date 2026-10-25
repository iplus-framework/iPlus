// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

namespace gip.core.licensing
{
    /// <summary>
    /// Constants that describe the on-disk license file format.
    /// </summary>
    public static class LicenseFormat
    {
        /// <summary>
        /// Current payload schema version.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// File extension of iPlus license files.
        /// </summary>
        public const string FileExtension = ".gip";

        /// <summary>
        /// Magic marker that starts every license file. Allows a quick
        /// "is this a license file at all" check before parsing.
        /// </summary>
        public const string Magic = "IPLUS-LICENSE";

        /// <summary>
        /// Name of the embedded public key resource that ships with the
        /// framework and validates gip-issued licenses.
        /// </summary>
        public const string FrameworkPublicKeyResourceName = "gip.core.licensing.FrameworkPublicKey.pem";
    }
}
