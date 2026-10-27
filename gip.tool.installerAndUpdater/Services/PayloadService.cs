// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gip.tool.installerAndUpdater.Models;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// Resolves the payload folder of a release and copies it to the target.
    /// </summary>
    /// <remarks>
    /// A release zip has one of two shapes:
    /// <list type="bullet">
    /// <item>platform zip: <c>iplus-5.0.0-desktop-linux-x64.zip</c> whose root
    /// already contains the application files;</item>
    /// <item>combined zip: <c>iplus-5.0.0-all.zip</c> whose root contains one
    /// sub-folder per platform (<c>desktop-linux-x64/</c>, <c>wpf-win-x64/</c>, …).</item>
    /// </list>
    /// Both are handled by <see cref="ResolvePayloadRoot"/>.
    /// </remarks>
    public sealed class PayloadService
    {
        /// <summary>
        /// Extracts a zip into <paramref name="destination"/>.
        /// </summary>
        public async Task ExtractAsync(
            string zipPath,
            string destination,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(destination);

            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            long totalBytes = archive.Entries.Sum(e => e.Length);
            long doneBytes = 0;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string targetPath = Path.GetFullPath(Path.Combine(destination, entry.FullName));

                // Zip-slip guard: never write outside the destination.
                if (!targetPath.StartsWith(Path.GetFullPath(destination), StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Archive entry escapes the target folder: " + entry.FullName);

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(targetPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? destination);

                await using Stream source = entry.Open();
                await using FileStream target = new FileStream(
                    targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await source.CopyToAsync(target, cancellationToken);

                doneBytes += entry.Length;
                if (totalBytes > 0)
                {
                    progress?.Report(new ProgressReport(
                        "Extracting " + entry.Name,
                        (double)doneBytes / totalBytes));
                }
            }
        }

        /// <summary>
        /// Finds the folder that actually holds the application files.
        /// </summary>
        /// <param name="root">Extracted release folder or a folder the user picked.</param>
        /// <param name="platformFolder">
        /// Preferred platform sub-folder (e.g. <c>desktop-linux-x64</c>). When empty
        /// or missing, the folder is auto-detected.
        /// </param>
        public static string ResolvePayloadRoot(string root, string platformFolder)
        {
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException("Payload folder not found: " + root);

            // 1. Explicit platform folder requested and present.
            if (!string.IsNullOrWhiteSpace(platformFolder))
            {
                string candidate = Path.Combine(root, platformFolder);
                if (Directory.Exists(candidate))
                    return candidate;
            }

            // 2. The root itself already contains the application.
            if (LooksLikeApplication(root))
                return root;

            // 3. Combined bundle: pick the best matching platform sub-folder.
            List<string> subFolders = Directory.GetDirectories(root)
                .Where(LooksLikeApplication)
                .ToList();

            // 4. A zip may still carry a single wrapper folder
            //    ("iplus-<ver>-all/" or "iplus-<ver>-<platform>-<rid>/"). Descend
            //    into it when it is the only entry and holds the payload.
            if (subFolders.Count == 0)
            {
                List<string> children = Directory.GetDirectories(root).ToList();
                if (children.Count == 1)
                {
                    string inner = children[0];
                    if (LooksLikeApplication(inner))
                        return inner;

                    subFolders = Directory.GetDirectories(inner)
                        .Where(LooksLikeApplication)
                        .ToList();

                    if (subFolders.Count > 0)
                        root = inner;
                }
            }

            if (subFolders.Count == 0)
                throw new InvalidOperationException(
                    "No application files found in " + root +
                    ". Expected a release zip or an extracted release folder.");

            // Folder names in a release carry a version prefix, e.g.
            // "iplus-5.0.0-preview.4-desktop-linux-x64", so match on the suffix
            // rather than on equality.
            string? best = subFolders.FirstOrDefault(f =>
                MatchesPlatform(Path.GetFileName(f), platformFolder));

            best ??= subFolders.FirstOrDefault(f =>
                MatchesPlatform(Path.GetFileName(f), GetCurrentPlatformFolder(ProductKind.Framework)));

            best ??= subFolders.FirstOrDefault(f =>
                Path.GetFileName(f).Contains(GetCurrentPlatformPrefix(), StringComparison.OrdinalIgnoreCase));

            return best ?? subFolders[0];
        }

        /// <summary>
        /// Lists the platform folders contained in a combined bundle.
        /// </summary>
        /// <remarks>
        /// Handles both layouts: platform folders directly under
        /// <paramref name="root"/>, and a single wrapper folder
        /// (<c>iplus-&lt;ver&gt;-all/</c>) that contains them.
        /// </remarks>
        public static List<string> GetPlatformFolders(string root)
        {
            if (!Directory.Exists(root))
                return new List<string>();

            List<string> folders = Directory.GetDirectories(root)
                .Where(LooksLikeApplication)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => n!)
                .ToList();

            if (folders.Count == 0)
            {
                List<string> children = Directory.GetDirectories(root).ToList();
                if (children.Count == 1)
                {
                    folders = Directory.GetDirectories(children[0])
                        .Where(LooksLikeApplication)
                        .Select(Path.GetFileName)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .Select(n => n!)
                        .ToList();
                }
            }

            return folders.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Returns the platform folder name used by the release pipeline for the
        /// machine this installer is running on.
        /// </summary>
        /// <remarks>
        /// The release script names folders <c>&lt;platform&gt;-&lt;arch&gt;</c>,
        /// e.g. <c>desktop-linux-x64</c>, <c>desktop-win-x64</c> or
        /// <c>wpf-win-x64</c>.
        /// </remarks>
        public static string GetCurrentPlatformFolder(ProductKind product)
        {
            return GetCurrentPlatformPrefix() + "-" + GetCurrentArchitecture();
        }

        private static string GetCurrentPlatformPrefix()
        {
            if (OperatingSystem.IsWindows())
                return "desktop-win";

            if (OperatingSystem.IsLinux())
                return "desktop-linux";

            if (OperatingSystem.IsMacOS())
                return "desktop-osx";

            return "desktop";
        }

        private static string GetCurrentArchitecture()
        {
            return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.X64 => "x64",
                System.Runtime.InteropServices.Architecture.Arm64 => "arm64",
                System.Runtime.InteropServices.Architecture.X86 => "x86",
                System.Runtime.InteropServices.Architecture.Arm => "arm",
                _ => "x64"
            };
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="folderName"/> denotes the
        /// requested platform, tolerating a version prefix such as
        /// <c>iplus-5.0.0-preview.4-</c>.
        /// </summary>
        private static bool MatchesPlatform(string folderName, string platformFolder)
        {
            if (string.IsNullOrWhiteSpace(platformFolder))
                return false;

            if (string.Equals(folderName, platformFolder, StringComparison.OrdinalIgnoreCase))
                return true;

            return folderName.EndsWith("-" + platformFolder, StringComparison.OrdinalIgnoreCase)
                || folderName.Contains("-" + platformFolder + "-", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Heuristic: a folder is an application payload when it contains the
        /// iPlus runtime assembly or a launch executable.
        /// </summary>
        private static bool LooksLikeApplication(string folder)
        {
            if (!Directory.Exists(folder))
                return false;

            return File.Exists(Path.Combine(folder, "gip.core.datamodel.dll"))
                || File.Exists(Path.Combine(folder, "gip.iplus.client.exe"))
                || File.Exists(Path.Combine(folder, "gip.iplus.client"))
                || File.Exists(Path.Combine(folder, "gip.mes.client.exe"))
                || File.Exists(Path.Combine(folder, "gip.mes.client"))
                || File.Exists(Path.Combine(folder, "gip.iplus.client.avui.Desktop.exe"))
                || File.Exists(Path.Combine(folder, "gip.iplus.client.avui.Desktop"))
                || File.Exists(Path.Combine(folder, "gip.mes.client.avui.Desktop.exe"))
                || File.Exists(Path.Combine(folder, "gip.mes.client.avui.Desktop"));
        }

        /// <summary>
        /// Copies the payload into the target folder, preserving user files.
        /// </summary>
        /// <remarks>
        /// <c>ConnectionStrings.config</c> and the <c>UserFiles</c> folder are
        /// never overwritten: they belong to the customer.
        /// </remarks>
        public async Task CopyPayloadAsync(
            string payloadRoot,
            string targetPath,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(targetPath);

            List<string> files = Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories).ToList();
            int index = 0;

            foreach (string sourceFile in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relative = Path.GetRelativePath(payloadRoot, sourceFile);

                if (IsUserOwnedFile(relative))
                {
                    index++;
                    continue;
                }

                string targetFile = Path.Combine(targetPath, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? targetPath);

                await using (FileStream source = new FileStream(
                    sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                await using (FileStream target = new FileStream(
                    targetFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await source.CopyToAsync(target, cancellationToken);
                }

                index++;
                progress?.Report(new ProgressReport(
                    "Copying " + relative,
                    (double)index / files.Count));
            }
        }

        /// <summary>
        /// Files that must survive an update because the customer owns them.
        /// </summary>
        public static bool IsUserOwnedFile(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');

            if (string.Equals(normalized, "ConnectionStrings.config", StringComparison.OrdinalIgnoreCase))
                return true;

            if (normalized.StartsWith("UserFiles/", StringComparison.OrdinalIgnoreCase))
                return true;

            if (normalized.StartsWith("Logs/", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
