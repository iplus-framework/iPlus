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
    /// Computes a stable machine fingerprint for hardware-bound licenses.
    /// </summary>
    /// <remarks>
    /// Only stable, non-personal identifiers are used: machine name, OS
    /// architecture and the ids of the physical disks that host the OS. No
    /// user names, MAC addresses or serial numbers of removable devices are
    /// included, so the fingerprint survives hardware maintenance and does not
    /// constitute personal data.
    /// </remarks>
    public static class MachineFingerprint
    {
        /// <summary>
        /// Computes the fingerprint of the current machine.
        /// </summary>
        public static string ComputeCurrent()
        {
            List<string> parts = new List<string>
            {
                Environment.MachineName ?? string.Empty,
                Environment.OSVersion.Platform.ToString(),
                Environment.Is64BitOperatingSystem ? "x64" : "x86"
            };

            parts.AddRange(GetDiskIds());
            return Compute(parts);
        }

        /// <summary>
        /// Computes a fingerprint from an explicit list of components.
        /// Useful for tests and for re-computing a fingerprint on another host.
        /// </summary>
        public static string Compute(IEnumerable<string> components)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));

            string joined = string.Join("|", components
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .OrderBy(c => c, StringComparer.Ordinal));

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
            return Convert.ToHexString(hash);
        }

        private static IEnumerable<string> GetDiskIds()
        {
            // Reading physical disk ids is platform specific and may fail in
            // restricted environments (containers, WASM). A failure simply
            // means the fingerprint is based on machine name + OS only.
            List<string> ids = new List<string>();
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    foreach (var drive in System.IO.DriveInfo.GetDrives())
                    {
                        if (drive.DriveType == System.IO.DriveType.Fixed && drive.IsReady)
                            ids.Add(drive.Name);
                    }
                }
                else if (OperatingSystem.IsLinux())
                {
                    const string byId = "/dev/disk/by-id";
                    if (System.IO.Directory.Exists(byId))
                    {
                        foreach (string entry in System.IO.Directory.EnumerateFileSystemEntries(byId))
                        {
                            string name = System.IO.Path.GetFileName(entry);
                            if (name.StartsWith("ata-", StringComparison.Ordinal)
                                || name.StartsWith("nvme-", StringComparison.Ordinal)
                                || name.StartsWith("scsi-", StringComparison.Ordinal))
                            {
                                ids.Add(name);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Best effort only.
            }
            return ids;
        }
    }
}
