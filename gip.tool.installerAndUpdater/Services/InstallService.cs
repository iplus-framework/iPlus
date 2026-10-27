// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gip.tool.installerAndUpdater.Models;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// Runs the whole install/update sequence.
    /// </summary>
    /// <remarks>
    /// The sequence mirrors the V4 one-click installer
    /// (<c>gip.iplus.startup</c>): resolve payload → copy files → write
    /// <c>ConnectionStrings.config</c> → optionally apply the initial-install
    /// script → create shortcut → optionally start the app. No registry keys, no
    /// environment variables, no admin rights.
    /// </remarks>
    public sealed class InstallService
    {
        private readonly PayloadService _payload = new PayloadService();
        private readonly SqlServerService _sql = new SqlServerService();
        private readonly GitHubReleaseService _github = new GitHubReleaseService();

        /// <summary>
        /// Runs the install or update described by <paramref name="options"/>.
        /// </summary>
        public async Task<InstallResult> RunAsync(
            InstallOptions options,
            IProgress<ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            string stagingFolder = string.Empty;

            try
            {
                Validate(options);

                // 1. Obtain the payload.
                string payloadSource = options.SourcePath;

                if (options.Source == SourceKind.GitHubRelease)
                {
                    progress?.Report(new ProgressReport("Contacting GitHub…", -1));

                    GitHubRelease? release = await _github.GetReleaseAsync(
                        options.GitHubRepository, options.GitHubTag, includePrereleases: true, cancellationToken);

                    if (release == null)
                        return InstallResult.Fail("No release found in " + options.GitHubRepository + ".");

                    string productPrefix = options.Product == ProductKind.Mes ? "imes-" : "iplus-";
                    GitHubAsset? asset = GitHubReleaseService.SelectAsset(
                        release, productPrefix, options.PlatformFolder);

                    if (asset == null)
                        return InstallResult.Fail(
                            "Release " + release.TagName + " has no asset for platform " +
                            options.PlatformFolder + ".");

                    stagingFolder = Path.Combine(Path.GetTempPath(), "iplus-install-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(stagingFolder);

                    string zipPath = Path.Combine(stagingFolder, asset.Name);
                    await _github.DownloadAsync(asset, zipPath, progress, cancellationToken);

                    progress?.Report(new ProgressReport("Extracting release…", -1));
                    string extractFolder = Path.Combine(stagingFolder, "payload");
                    await _payload.ExtractAsync(zipPath, extractFolder, progress, cancellationToken);

                    payloadSource = extractFolder;
                    options.InstalledVersion = release.Version;
                }
                else if (options.Source == SourceKind.LocalZip)
                {
                    stagingFolder = Path.Combine(Path.GetTempPath(), "iplus-install-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(stagingFolder);

                    progress?.Report(new ProgressReport("Extracting " + Path.GetFileName(options.SourcePath) + "…", -1));
                    string extractFolder = Path.Combine(stagingFolder, "payload");
                    await _payload.ExtractAsync(options.SourcePath, extractFolder, progress, cancellationToken);

                    payloadSource = extractFolder;
                }

                // 2. Resolve the folder that holds the application files.
                string payloadRoot = PayloadService.ResolvePayloadRoot(payloadSource, options.PlatformFolder);
                progress?.Report(new ProgressReport("Payload: " + payloadRoot, -1));

                // 3. Copy the files into the target folder.
                progress?.Report(new ProgressReport("Copying application files…", 0));
                await _payload.CopyPayloadAsync(payloadRoot, options.TargetPath, progress, cancellationToken);

                // 4. Write ConnectionStrings.config.
                progress?.Report(new ProgressReport("Writing " + ConnectionStringWriter.FileName + "…", -1));
                ConnectionStringWriter.Write(options.TargetPath, options.Product, options.Sql);

                // 5. Optionally create the database and apply the initial-install script.
                if (options.ApplyInitialInstallScript)
                {
                    bool exists = await _sql.DatabaseExistsAsync(options.Sql, cancellationToken);

                    if (!exists)
                    {
                        progress?.Report(new ProgressReport(
                            "Creating database " + options.Sql.DatabaseName + "…", -1));
                        await _sql.CreateDatabaseAsync(options.Sql, cancellationToken);
                    }

                    string scriptPath = ResolveInitialInstallScript(options, payloadRoot);
                    if (!string.IsNullOrEmpty(scriptPath))
                    {
                        progress?.Report(new ProgressReport(
                            "Applying " + Path.GetFileName(scriptPath) + "…", 0));
                        await _sql.ExecuteScriptAsync(options.Sql, scriptPath, progress, cancellationToken);
                    }
                    else if (!exists)
                    {
                        return InstallResult.Fail(
                            "The database was created but no initial-install script was found. " +
                            "Pass the script path explicitly or copy it next to the release.");
                    }
                }

                // 6. Desktop shortcut.
                if (options.CreateDesktopShortcut)
                {
                    progress?.Report(new ProgressReport("Creating desktop shortcut…", -1));
                    ShortcutService.CreateDesktopShortcut(options.TargetPath, options.Product);
                }

                // 7. Start the application.
                if (options.StartAfterInstall)
                {
                    progress?.Report(new ProgressReport("Starting application…", -1));
                    ShortcutService.StartApplication(options.TargetPath, options.Product);
                }

                string version = options.InstalledVersion;
                if (string.IsNullOrEmpty(version))
                    version = ReadInstalledVersion(options.TargetPath);

                progress?.Report(new ProgressReport("Done.", 1));

                return InstallResult.Ok(
                    (options.Operation == OperationKind.Update ? "Update" : "Installation") +
                    " completed successfully.",
                    version,
                    ShortcutService.GetLaunchExecutable(options.TargetPath, options.Product));
            }
            catch (OperationCanceledException)
            {
                return InstallResult.Fail("Cancelled by the user.");
            }
            catch (Exception ex)
            {
                return InstallResult.Fail(ex.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(stagingFolder) && Directory.Exists(stagingFolder))
                {
                    try
                    {
                        Directory.Delete(stagingFolder, recursive: true);
                    }
                    catch
                    {
                        // Temp cleanup is best-effort.
                    }
                }
            }
        }

        /// <summary>
        /// Finds the initial-install script: explicit path first, then the
        /// payload folder, then the target folder.
        /// </summary>
        private static string ResolveInitialInstallScript(InstallOptions options, string payloadRoot)
        {
            if (!string.IsNullOrWhiteSpace(options.InitialInstallScriptPath)
                && File.Exists(options.InitialInstallScriptPath))
            {
                return options.InitialInstallScriptPath;
            }

            string[] names = options.Product == ProductKind.Mes
                ? new[] { "initial-install-imes.sql", "initial-install-iplusmes.sql", "initial-install.sql" }
                : new[] { "initial-install-iplus.sql", "initial-install.sql" };

            foreach (string folder in new[] { payloadRoot, options.TargetPath })
            {
                foreach (string name in names)
                {
                    string candidate = Path.Combine(folder, name);
                    if (File.Exists(candidate))
                        return candidate;
                }

                // Fall back to any initial-install*.sql in the folder.
                string? any = Directory.GetFiles(folder, "initial-install*.sql")
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (any != null)
                    return any;
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads the version of an already installed application.
        /// </summary>
        public static string ReadInstalledVersion(string targetPath)
        {
            string[] candidates =
            {
                "gip.core.datamodel.dll",
                "gip.iplus.client.avui.Desktop.dll",
                "gip.mes.client.avui.Desktop.dll"
            };

            foreach (string candidate in candidates)
            {
                string path = Path.Combine(targetPath, candidate);
                if (!File.Exists(path))
                    continue;

                try
                {
                    return System.Diagnostics.FileVersionInfo.GetVersionInfo(path).FileVersion ?? string.Empty;
                }
                catch
                {
                    // Ignore and try the next candidate.
                }
            }

            return string.Empty;
        }

        private static void Validate(InstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.TargetPath))
                throw new InvalidOperationException("No target folder selected.");

            if (options.Source == SourceKind.LocalFolder && string.IsNullOrWhiteSpace(options.SourcePath))
                throw new InvalidOperationException("No source folder selected.");

            if (options.Source == SourceKind.LocalZip && !File.Exists(options.SourcePath))
                throw new InvalidOperationException("Source zip not found: " + options.SourcePath);

            if (options.Source == SourceKind.GitHubRelease && string.IsNullOrWhiteSpace(options.GitHubRepository))
                throw new InvalidOperationException("No GitHub repository configured.");

            if (options.ApplyInitialInstallScript)
            {
                if (string.IsNullOrWhiteSpace(options.Sql.ServerName))
                    throw new InvalidOperationException("No SQL Server specified.");

                if (string.IsNullOrWhiteSpace(options.Sql.DatabaseName))
                    throw new InvalidOperationException("No database name specified.");

                if (options.Sql.AuthMode == SqlAuthMode.SqlServerAuthentication
                    && string.IsNullOrWhiteSpace(options.Sql.UserName))
                {
                    throw new InvalidOperationException("No SQL Server user specified.");
                }
            }
        }

        public void Dispose()
        {
            _github.Dispose();
        }
    }
}
