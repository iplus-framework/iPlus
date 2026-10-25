// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

namespace gip.core.licensing
{
    /// <summary>
    /// Kind of license that is contained in a license file.
    /// Mirrors the three-division model of the iPlus license system
    /// (gip / developers / end users) but without any hardware dongle.
    /// </summary>
    public enum LicenseKind
    {
        /// <summary>
        /// Unknown / not specified. Never valid.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Development license issued by gip to a developer company.
        /// Grants the right to use the development environment and to
        /// issue <see cref="Product"/> licenses for its own packages.
        /// </summary>
        DeveloperIssuer = 10,

        /// <summary>
        /// Development license issued by gip to an end user that only
        /// grants access to the engineering/development features.
        /// </summary>
        DeveloperEndUser = 20,

        /// <summary>
        /// Product license issued by a developer (or by gip) for a concrete
        /// database installation. Enumerates the licensed packages.
        /// </summary>
        Product = 30,

        /// <summary>
        /// Runtime license issued by gip for a concrete database installation.
        /// Grants the right to run the iPlus framework itself for a maximum
        /// number of concurrent users (<see cref="LicensePayload.MaxUsers"/>).
        /// A <see cref="Product"/> license is only effective when a valid
        /// framework runtime license is present.
        /// </summary>
        FrameworkRuntime = 40
    }
}
