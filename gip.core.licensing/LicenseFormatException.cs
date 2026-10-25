// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;

namespace gip.core.licensing
{
    /// <summary>
    /// Thrown when a license file cannot be parsed or does not have the expected structure.
    /// </summary>
    public sealed class LicenseFormatException : Exception
    {
        public LicenseFormatException(string message) : base(message)
        {
        }

        public LicenseFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
