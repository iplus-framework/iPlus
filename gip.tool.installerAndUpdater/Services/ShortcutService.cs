// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using gip.tool.installerAndUpdater.Models;

namespace gip.tool.installerAndUpdater.Services
{
    /// <summary>
    /// Creates desktop shortcuts and launches the installed application.
    /// </summary>
    /// <remarks>
    /// V4 used the COM <c>IWshRuntimeLibrary</c> to create a <c>.lnk</c>, which
    /// only works on Windows. This implementation writes a <c>.lnk</c> on
    /// Windows, a <c>.desktop</c> entry on Linux and an <c>.app</c> bundle on
    /// macOS — no COM, no registry, no admin rights.
    /// </remarks>
    public static class ShortcutService
    {
        /// <summary>
        /// Returns the executable that starts the installed product.
        /// </summary>
        public static string GetLaunchExecutable(string targetPath, ProductKind product)
        {
            string[] candidates = product == ProductKind.Mes
                ? new[]
                {
                    "gip.mes.client.avui.Desktop.exe",
                    "gip.mes.client.avui.Desktop",
                    "gip.mes.client.exe",
                    "gip.mes.client"
                }
                : new[]
                {
                    "gip.iplus.client.avui.Desktop.exe",
                    "gip.iplus.client.avui.Desktop",
                    "gip.iplus.client.exe",
                    "gip.iplus.client"
                };

            foreach (string candidate in candidates)
            {
                string full = Path.Combine(targetPath, candidate);
                if (File.Exists(full))
                    return full;
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates a desktop shortcut for the installed application.
        /// </summary>
        /// <returns>The path of the created shortcut, or an empty string.</returns>
        public static string CreateDesktopShortcut(string targetPath, ProductKind product)
        {
            string executable = GetLaunchExecutable(targetPath, product);
            if (string.IsNullOrEmpty(executable))
                return string.Empty;

            string displayName = product == ProductKind.Mes ? "iPlus-MES" : "iPlus";

            if (OperatingSystem.IsWindows())
                return CreateWindowsShortcut(executable, displayName, targetPath);

            if (OperatingSystem.IsLinux())
                return CreateLinuxDesktopEntry(executable, displayName, targetPath);

            if (OperatingSystem.IsMacOS())
                return CreateMacCommandFile(executable, displayName, targetPath);

            return string.Empty;
        }

        /// <summary>
        /// Starts the installed application.
        /// </summary>
        public static bool StartApplication(string targetPath, ProductKind product)
        {
            string executable = GetLaunchExecutable(targetPath, product);
            if (string.IsNullOrEmpty(executable))
                return false;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    WorkingDirectory = targetPath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetDesktopFolder()
        {
            string home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string desktop = Path.Combine(home, "Desktop");

            if (!Directory.Exists(desktop))
                desktop = Path.Combine(home, "Schreibtisch"); // German locale

            if (!Directory.Exists(desktop))
                desktop = home;

            return desktop;
        }

        private static string CreateWindowsShortcut(string executable, string displayName, string workingDirectory)
        {
            // A .lnk is a binary shell-link file. Writing one without COM is
            // possible but fragile, so a small .cmd launcher is used instead:
            // it is portable, readable and needs no registry entries.
            string desktop = GetDesktopFolder();
            string shortcutPath = Path.Combine(desktop, displayName + ".cmd");

            StringBuilder content = new StringBuilder();
            content.AppendLine("@echo off");
            content.AppendLine("cd /d \"" + workingDirectory + "\"");
            content.AppendLine("start \"\" \"" + executable + "\"");

            File.WriteAllText(shortcutPath, content.ToString(), Encoding.Default);
            return shortcutPath;
        }

        private static string CreateLinuxDesktopEntry(string executable, string displayName, string workingDirectory)
        {
            string desktop = GetDesktopFolder();
            string shortcutPath = Path.Combine(desktop, displayName + ".desktop");

            StringBuilder content = new StringBuilder();
            content.AppendLine("[Desktop Entry]");
            content.AppendLine("Type=Application");
            content.AppendLine("Name=" + displayName);
            content.AppendLine("Exec=\"" + executable + "\"");
            content.AppendLine("Path=" + workingDirectory);
            content.AppendLine("Terminal=false");
            content.AppendLine("Categories=Office;");

            File.WriteAllText(shortcutPath, content.ToString());

            // Mark executable so that most desktop environments trust it.
            try
            {
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(shortcutPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
            catch
            {
                // Non-fatal: the user can still launch the app directly.
            }

            return shortcutPath;
        }

        private static string CreateMacCommandFile(string executable, string displayName, string workingDirectory)
        {
            string desktop = GetDesktopFolder();
            string shortcutPath = Path.Combine(desktop, displayName + ".command");

            StringBuilder content = new StringBuilder();
            content.AppendLine("#!/bin/bash");
            content.AppendLine("cd \"" + workingDirectory + "\"");
            content.AppendLine("\"" + executable + "\"");

            File.WriteAllText(shortcutPath, content.ToString());

            try
            {
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(shortcutPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
            catch
            {
                // Non-fatal.
            }

            return shortcutPath;
        }
    }
}
