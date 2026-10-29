// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using gip.core.datamodel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui.Helperclasses
{
    /// <summary>
    /// Opens a file that was delivered by a server method with the viewer of the device.
    /// Platforms that need special handling (e.g. Android requires a content:// URI with a read grant
    /// for files from the app cache) set <see cref="PlatformHandler"/> at startup.
    /// </summary>
    public static class VBFileOpener
    {
        /// <summary>
        /// Parameters: file name, mime type, content. Returns true if a viewer was started.
        /// </summary>
        public static Func<string, string, byte[], Task<bool>>? PlatformHandler { get; set; }

        public static async Task<bool> OpenAsync(Visual? owner, IVBFileContent file)
        {
            if (file == null || file.Content == null || file.Content.Length == 0)
                return false;

            string fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrEmpty(fileName))
                fileName = "file" + file.Extension;
            else if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                fileName += file.Extension;

            if (PlatformHandler != null)
                return await PlatformHandler(fileName, file.MimeType, file.Content);

            TopLevel? topLevel = owner != null ? TopLevel.GetTopLevel(owner) : null;
            if (topLevel == null)
                return false;

            // A separate folder per file keeps the original file name for the viewer
            string folder = Path.Combine(Path.GetTempPath(), "iPlusOpenFile", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(path, file.Content);

            IStorageFile? storageFile = await topLevel.StorageProvider.TryGetFileFromPathAsync(path);
            if (storageFile == null)
                return false;
            return await topLevel.Launcher.LaunchFileAsync(storageFile);
        }
    }
}
