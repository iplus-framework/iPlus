// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.InteropServices.JavaScript;
using gip.core.datamodel;

namespace gip.iplus.client.avui
{
    /// <summary>
    /// Persists client-local settings in the browser's localStorage (via JS interop).
    ///
    /// The browser (WASM) has no writable file system, so the two file-based settings
    /// stores of the desktop client have no effect there:
    /// - <see cref="CommandLineHelper.Settings"/> (settingsiPlus.config next to the exe)
    /// - the App user settings (usersettings.json in %APPDATA%)
    ///
    /// <see cref="Register"/> installs a storage backend into <see cref="CommandLineHelper"/>
    /// and <see cref="LoadAppSettingsJson"/>/<see cref="SaveAppSettingsJson"/> serve the
    /// App user settings. The JS side is defined in wwwroot/index.html of the browser head
    /// (window.iPlusSettings). All members are no-ops / return null on non-browser platforms.
    /// </summary>
    internal static partial class BrowserSettingsStorage
    {
        private const string CommandLineSettingsKey = "iPlus.CommandLineSettings";
        private const string AppSettingsKey = "iPlus.AppSettings";

        [JSImport("globalThis.iPlusSettings.getItem")]
        private static partial string GetItem(string key);

        [JSImport("globalThis.iPlusSettings.setItem")]
        private static partial void SetItem(string key, string value);

        /// <summary>
        /// Installs the localStorage backend into <see cref="CommandLineHelper"/>. No-op on
        /// non-browser platforms.
        /// </summary>
        internal static void Register()
        {
            if (!OperatingSystem.IsBrowser())
                return;
            CommandLineHelper.LoadSettingsFromStorage = () => GetItem(CommandLineSettingsKey);
            CommandLineHelper.SaveSettingsToStorage = xml => SetItem(CommandLineSettingsKey, xml);
        }

        /// <summary>
        /// Loads the App user settings JSON from localStorage. Returns null on non-browser
        /// platforms or when nothing is stored yet.
        /// </summary>
        internal static string LoadAppSettingsJson()
        {
            if (!OperatingSystem.IsBrowser())
                return null;
            try
            {
                return GetItem(AppSettingsKey);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Persists the App user settings JSON to localStorage. No-op on non-browser platforms.
        /// </summary>
        internal static void SaveAppSettingsJson(string json)
        {
            if (!OperatingSystem.IsBrowser() || String.IsNullOrEmpty(json))
                return;
            try
            {
                SetItem(AppSettingsKey, json);
            }
            catch (Exception)
            {
                // ignore — storage may be unavailable (private mode, quota exceeded)
            }
        }
    }
}
