// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using gip.core.datamodel;
using gip.core.media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gip.core.wpfservices.avui
{
    public class MediaControllerProxy: IVBMediaControllerProxy
    {

        public MediaControllerProxy(IACComponent component)
        {
            _MediaControllerComp = component;
        }

        #region Proxy

        private IACComponent _MediaControllerComp;
        public ACMediaController MediaControllerComp
        {
            get
            {
                if (_MediaControllerComp == null)
                    return null;
                return _MediaControllerComp as ACMediaController;
            }
        }

        #endregion

        #region License Image

        public SKBitmap CreateLicenseImage(VBLicense CurrentVBLicense)
        {
            if (CurrentVBLicense == null
                || CurrentVBLicense.SystemCommon == null
                || !CurrentVBLicense.SystemCommon.Any())
                return null;
            byte[] licenseArr = CurrentVBLicense.SystemCommon;

            SKBitmap bitmap = new SKBitmap(new SKImageInfo(510, 510, SKColorType.Rgba8888, SKAlphaType.Premul));
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                int segmentPos = 0;
                short shape = 0;
                int alpha = 255, red = 0, green = 0, blue = 0, x = 0, y = 0, width = 0, height = 0;

                for (int i = 0; i < licenseArr.Length; i++)
                {
                    switch (segmentPos)
                    {
                        case 0:
                            alpha = licenseArr[i];
                            break;
                        case 1:
                            red = licenseArr[i];
                            break;
                        case 2:
                            green = licenseArr[i];
                            break;
                        case 3:
                            blue = licenseArr[i];
                            break;
                        case 4:
                            x = licenseArr[i];
                            break;
                        case 5:
                            y = licenseArr[i];
                            break;
                        case 6:
                            width = licenseArr[i];
                            break;
                        case 7:
                            height = licenseArr[i];
                            break;
                        default:
                            break;
                    }

                    if (segmentPos == 7)
                    {
                        segmentPos = 0;
                        SKColor color = new SKColor((byte)red, (byte)green, (byte)blue, (byte)alpha);
                        using (SKPaint paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill })
                        {
                            if (shape == 0)
                            {
                                float centerX = x + width / 2f;
                                float centerY = y + height / 2f;
                                float radiusX = width / 2f;
                                float radiusY = height / 2f;
                                canvas.DrawOval(centerX, centerY, radiusX, radiusY, paint);
                                shape = 1;
                            }
                            else if (shape == 1)
                            {
                                canvas.DrawRect(x, y, width, height, paint);
                                shape = 0;
                            }
                        }
                    }
                    else
                        segmentPos++;
                }
            }
            return bitmap;
        }

        #endregion

        #region Methods

        public byte[] ResizeImage(string fileName, int maxWidth, int maxHeight, string quality = "Medium")
        {
            SKData data = null;
            using (FileStream ms = new FileStream(fileName, FileMode.Open))
            {
                using (SKBitmap sourceBitmap = SKBitmap.Decode(ms))
                {
                    int height = Math.Min(maxHeight, sourceBitmap.Height);
                    int width = Math.Min(maxWidth, sourceBitmap.Width);
                    SKSamplingOptions samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Nearest);
                    using (SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), samplingOptions))
                    {
                        using (SKImage scaledImage = SKImage.FromBitmap(scaledBitmap))
                        {
                            data = scaledImage.Encode();
                        }
                    }
                }
            }

            return data != null ? data.ToArray() : null;
        }


        public string OpenFileDialog(bool isFolderPicker, string initialDirectory, bool useExisting, string defaultExtension = null, Dictionary<string, string> filters = null)
        {
            Application app = Database.Root.RootPageWPF.WPFApplication as Application;
            if (app == null || app.ApplicationLifetime == null)
                return null;
            TopLevel topLevel = app.ApplicationLifetime as TopLevel;
            if (topLevel == null || topLevel.StorageProvider == null)
                return null;
            IStorageProvider provider = topLevel.StorageProvider;
            return Dispatcher.UIThread.InvokeAsync<string>(() => OpenFileDialog(provider, isFolderPicker, initialDirectory, useExisting, defaultExtension, filters)).GetAwaiter().GetResult();
        }

        public static async Task<string> OpenFileDialog(IStorageProvider provider, bool isFolderPicker, string initialDirectory, bool useExisting, string defaultExtension = null, Dictionary<string, string> filters = null)
        {
            IStorageFolder startingLocation = null;
            if (!string.IsNullOrEmpty(initialDirectory))
            {
                if (isFolderPicker)
                    startingLocation = await provider.TryGetFolderFromPathAsync(initialDirectory);
                else
                    startingLocation = await provider.TryGetFolderFromPathAsync(Path.GetDirectoryName(initialDirectory));
            }

            if (isFolderPicker)
            {
                var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Folder",
                    SuggestedStartLocation = startingLocation
                });

                if (folders.Count > 0)
                {
                    var folder = folders.First();
                    bool exists = Directory.Exists(folder.Path.ToString());
                    if (!useExisting || exists)
                        return folder.Path.ToString();
                }
            }
            else
            {
                List<FilePickerFileType> fileTypes = null;
                if (filters != null)
                {
                    fileTypes = new List<FilePickerFileType>();
                    foreach (var filter in filters)
                    {
                        fileTypes.Add(new FilePickerFileType(filter.Key)
                        {
                            Patterns = filter.Value.Split(';').Select(s => s.Trim()).ToArray()
                        });
                    }
                }

                var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open File",
                    AllowMultiple = false,
                    SuggestedStartLocation = startingLocation,
                    SuggestedFileName = Path.GetFileName(initialDirectory),
                    FileTypeFilter = fileTypes                    
                });

                if (files.Count > 0)
                {
                    var file = files.First();
                    bool exists = File.Exists(file.Path.ToString());
                    if (!useExisting || exists)
                        return file.Path.ToString();
                }
            }

            return null;
        }


        public string SaveFileDialog(string initialDirectory, string defaultExtension = null)
        {
            Application app = Database.Root.RootPageWPF.WPFApplication as Application;
            if (app == null || app.ApplicationLifetime == null)
                return null;
            TopLevel topLevel = app.ApplicationLifetime as TopLevel;
            if (topLevel == null || topLevel.StorageProvider == null)
                return null;
            IStorageProvider provider = topLevel.StorageProvider;
            return Dispatcher.UIThread.InvokeAsync<string>(() => SaveFileDialog(provider, initialDirectory, defaultExtension)).GetAwaiter().GetResult();
        }

        public static async Task<string> SaveFileDialog(IStorageProvider provider, string initialDirectory, string defaultExtension = null)
        {
            IStorageFolder startingLocation = null;
            if (!string.IsNullOrEmpty(initialDirectory))
                startingLocation = await provider.TryGetFolderFromPathAsync(Path.GetDirectoryName(initialDirectory));

            var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Open File",
                SuggestedStartLocation = startingLocation,
                SuggestedFileName = Path.GetFileName(initialDirectory),
                DefaultExtension = defaultExtension
            });

            return file != null ? file.Path.ToString() : null;
        }

        #endregion
    }
}
