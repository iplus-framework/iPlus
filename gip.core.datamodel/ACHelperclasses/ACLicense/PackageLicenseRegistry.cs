// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using gip.core.licensing;

namespace gip.core.datamodel.Licensing
{
    /// <summary>
    /// In-memory registry that maps an <c>ACPackageName</c> to the license
    /// expression that the package's assembly declares as
    /// <c>[assembly: AssemblyMetadata("iPlusLicense", "...")]</c>.
    /// </summary>
    /// <remarks>
    /// The registry is filled once per assembly in
    /// <c>ACClassManager.RegisterAssemblyTypes</c> — the single choke point where
    /// every framework assembly is loaded and its types are registered. It is
    /// consulted by <see cref="License.IsPackageLicensed"/> to enforce the
    /// GPL/commercial mixing rule.
    ///
    /// The registry is process-wide and thread-safe. It is deliberately
    /// fail-open: a package whose expression is unknown is treated as allowed,
    /// because a missing metadata value must never prevent the framework from
    /// starting. Enforcement happens later, at <c>ACClass.IsLicensed</c> and
    /// <c>ACComponent.ACUrlCommand</c>, which run after registration.
    /// </remarks>
    public static class PackageLicenseRegistry
    {
        /// <summary>
        /// Name of the assembly metadata key that carries the license expression.
        /// </summary>
        public const string MetadataKey = "iPlusLicense";

        private static readonly object _lock = new object();
        private static readonly Dictionary<string, string> _expressions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers (or overwrites) the license expression for a package.
        /// </summary>
        public static void Register(string packageName, string licenseExpression)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return;

            lock (_lock)
            {
                _expressions[packageName.Trim()] = licenseExpression ?? string.Empty;
            }
        }

        /// <summary>
        /// Reads the <c>iPlusLicense</c> assembly metadata from the given assembly
        /// and registers it for every package name that the assembly's types
        /// declare through <c>ACClassInfo</c>.
        /// </summary>
        /// <param name="assembly">Assembly that was just registered.</param>
        /// <param name="packageNames">
        /// Package names found in the assembly's <c>ACClassInfo</c> attributes.
        /// </param>
        /// <returns>The expression that was registered, or <c>null</c> when the
        /// assembly carries no metadata.</returns>
        public static string RegisterFromAssembly(Assembly assembly, IEnumerable<string> packageNames)
        {
            if (assembly == null)
                return null;

            string expression = ReadExpression(assembly);
            if (expression == null)
                return null;

            if (packageNames != null)
            {
                foreach (string packageName in packageNames.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
                    Register(packageName, expression);
            }

            return expression;
        }

        /// <summary>
        /// Reads the <c>iPlusLicense</c> assembly metadata value from an assembly.
        /// Returns <c>null</c> when the assembly carries no such metadata.
        /// </summary>
        public static string ReadExpression(Assembly assembly)
        {
            if (assembly == null)
                return null;

            try
            {
                foreach (AssemblyMetadataAttribute attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    if (string.Equals(attribute.Key, MetadataKey, StringComparison.OrdinalIgnoreCase))
                        return attribute.Value;
                }
            }
            catch (Exception)
            {
                // A partially loadable assembly must not break registration.
            }

            return null;
        }

        /// <summary>
        /// Returns the registered license expression for a package, or <c>null</c>
        /// when the package is unknown.
        /// </summary>
        public static string GetExpression(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return null;

            lock (_lock)
            {
                return _expressions.TryGetValue(packageName.Trim(), out string expression) ? expression : null;
            }
        }

        /// <summary>
        /// Returns the classification of a package's license expression.
        /// Unknown packages yield <see cref="LicenseCompatibility.Unknown"/>.
        /// </summary>
        public static LicenseCompatibility GetCompatibility(string packageName)
        {
            return PackageLicenseExpression.Classify(GetExpression(packageName));
        }

        /// <summary>
        /// Returns <c>true</c> when the package may run under a commercial runtime
        /// license. Unknown packages are allowed (fail-open).
        /// </summary>
        public static bool IsAllowedUnderCommercialRuntime(string packageName)
        {
            return PackageLicenseExpression.IsAllowedUnderCommercialRuntime(GetExpression(packageName));
        }

        /// <summary>
        /// Returns a snapshot of all registered package → expression pairs.
        /// Intended for diagnostics and the license manager UI.
        /// </summary>
        public static IReadOnlyDictionary<string, string> Snapshot()
        {
            lock (_lock)
            {
                return new Dictionary<string, string>(_expressions, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Clears the registry. Intended for tests.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _expressions.Clear();
            }
        }
    }
}
