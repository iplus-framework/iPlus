// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.licensing
{
    /// <summary>
    /// How a package's license expression behaves with respect to the two
    /// iPlus editions (open-source runtime vs. commercial runtime).
    /// </summary>
    public enum LicenseCompatibility
    {
        /// <summary>
        /// The expression could not be classified. Treated as "allowed" so that
        /// startup never breaks because of a missing or unusual metadata value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Copyleft only (e.g. <c>GPL-3.0-or-later</c>). May run under the open
        /// runtime, must not run under a commercial runtime license.
        /// </summary>
        OpenOnly = 10,

        /// <summary>
        /// Commercial only (e.g. <c>Commercial</c>). Requires a product license;
        /// may run under either runtime edition.
        /// </summary>
        CommercialOnly = 20,

        /// <summary>
        /// Dual-licensed or permissive (e.g. <c>GPL-3.0-or-later OR LicenseRef-Commercial</c>,
        /// <c>MIT</c>). May run under either runtime edition.
        /// </summary>
        Both = 30
    }

    /// <summary>
    /// Classifies SPDX license expressions that are attached to iPlus packages
    /// as assembly metadata (<c>AssemblyMetadata("iPlusLicense", ...)</c>).
    /// </summary>
    /// <remarks>
    /// The vocabulary is plain SPDX — no custom syntax is invented:
    /// <list type="bullet">
    /// <item><c>GPL-3.0-or-later</c> — pure copyleft, blocked under a commercial runtime license.</item>
    /// <item><c>GPL-3.0-or-later OR LicenseRef-Commercial</c> — dual-licensed, allowed everywhere.</item>
    /// <item><c>Commercial</c> / <c>LicenseRef-Commercial</c> — proprietary, requires a product license.</item>
    /// <item><c>MIT</c>, <c>Apache-2.0</c>, <c>BSD-3-Clause</c>, … — permissive, allowed everywhere.</item>
    /// </list>
    /// The <c>OR</c> operator is what makes dual licensing expressible without a
    /// new format. Classification is deliberately lenient: an unparsable value
    /// yields <see cref="LicenseCompatibility.Unknown"/> and is allowed, because a
    /// wrong metadata value must never prevent the framework from starting.
    /// </remarks>
    public static class PackageLicenseExpression
    {
        /// <summary>
        /// Value that marks a package as proprietary / commercially licensed.
        /// </summary>
        public const string CommercialToken = "Commercial";

        /// <summary>
        /// SPDX-correct form of <see cref="CommercialToken"/> (a non-standard license
        /// must be expressed as a <c>LicenseRef-</c>).
        /// </summary>
        public const string CommercialLicenseRef = "LicenseRef-Commercial";

        /// <summary>
        /// Default expression for gip's own packages: usable under both editions.
        /// </summary>
        public const string DualLicensedExpression = "GPL-3.0-or-later OR LicenseRef-Commercial";

        private static readonly HashSet<string> _permissive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "MIT", "MIT-0", "Apache-2.0", "Apache-1.1", "BSD-2-Clause", "BSD-3-Clause",
            "ISC", "MS-PL", "MS-RL", "Zlib", "Unlicense", "CC0-1.0", "0BSD", "BSL-1.0",
            "PostgreSQL", "X11", "NCSA", "WTFPL"
        };

        private static readonly HashSet<string> _copyleft = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GPL-1.0-only", "GPL-1.0-or-later",
            "GPL-2.0-only", "GPL-2.0-or-later",
            "GPL-3.0-only", "GPL-3.0-or-later",
            "AGPL-1.0-only", "AGPL-1.0-or-later",
            "AGPL-3.0-only", "AGPL-3.0-or-later",
            "LGPL-2.0-only", "LGPL-2.0-or-later",
            "LGPL-2.1-only", "LGPL-2.1-or-later",
            "LGPL-3.0-only", "LGPL-3.0-or-later",
            "MPL-2.0", "EPL-1.0", "EPL-2.0", "CDDL-1.0", "CDDL-1.1", "OSL-3.0"
        };

        /// <summary>
        /// Splits an SPDX expression into its alternatives (the <c>OR</c> operator).
        /// Parentheses and the <c>WITH</c> suffix are stripped so that
        /// <c>(GPL-3.0-or-later OR LicenseRef-Commercial)</c> classifies correctly.
        /// </summary>
        public static IReadOnlyList<string> SplitAlternatives(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return Array.Empty<string>();

            string normalized = expression.Replace("(", " ").Replace(")", " ");
            string[] parts = normalized.Split(new[] { " OR ", " or ", " Or " }, StringSplitOptions.RemoveEmptyEntries);

            List<string> result = new List<string>();
            foreach (string part in parts)
            {
                string token = part.Trim();
                int withIndex = token.IndexOf(" WITH ", StringComparison.OrdinalIgnoreCase);
                if (withIndex > 0)
                    token = token.Substring(0, withIndex).Trim();
                if (token.Length > 0)
                    result.Add(token);
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when the expression contains the commercial token
        /// (either <c>Commercial</c> or <c>LicenseRef-Commercial</c>).
        /// </summary>
        public static bool IsCommercial(string expression)
        {
            return SplitAlternatives(expression).Any(t =>
                string.Equals(t, CommercialToken, StringComparison.OrdinalIgnoreCase)
                || string.Equals(t, CommercialLicenseRef, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns <c>true</c> when the expression contains a permissive license.
        /// </summary>
        public static bool IsPermissive(string expression)
        {
            return SplitAlternatives(expression).Any(t => _permissive.Contains(t));
        }

        /// <summary>
        /// Returns <c>true</c> when the expression contains a copyleft license.
        /// </summary>
        public static bool IsCopyleft(string expression)
        {
            return SplitAlternatives(expression).Any(t => _copyleft.Contains(t));
        }

        /// <summary>
        /// Classifies the expression into <see cref="LicenseCompatibility"/>.
        /// </summary>
        public static LicenseCompatibility Classify(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return LicenseCompatibility.Unknown;

            bool commercial = IsCommercial(expression);
            bool permissive = IsPermissive(expression);
            bool copyleft = IsCopyleft(expression);

            if (commercial || permissive)
                return LicenseCompatibility.Both;

            if (copyleft)
                return LicenseCompatibility.OpenOnly;

            return LicenseCompatibility.Unknown;
        }

        /// <summary>
        /// The enforcement rule: a package may run under a <b>commercial</b> runtime
        /// license only when its expression contains the commercial token or is
        /// permissive. A pure copyleft package cannot — a third party cannot
        /// relicense GPL code, so combining it with commercially licensed iPlus
        /// would violate the GPL.
        /// </summary>
        /// <returns>
        /// <c>true</c> when the package is allowed; <c>false</c> when it must be blocked.
        /// Unknown expressions are allowed (fail-open).
        /// </returns>
        public static bool IsAllowedUnderCommercialRuntime(string expression)
        {
            LicenseCompatibility compatibility = Classify(expression);
            return compatibility != LicenseCompatibility.OpenOnly;
        }
    }
}
