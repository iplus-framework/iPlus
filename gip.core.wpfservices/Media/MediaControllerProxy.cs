// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using gip.core.media;
using Microsoft.WindowsAPICodePack.Dialogs;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gip.core.wpfservices
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


        public async Task<string> OpenFileDialog(bool isFolderPicker, string initialDirectory, bool useExisting, string defaultExtension = null, Dictionary<string, string> filters = null)
        {
            string path = null;
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = isFolderPicker;
                if (isFolderPicker)
                {
                    if (Directory.Exists(initialDirectory))
                    {
                        dialog.InitialDirectory = initialDirectory;
                    }
                }
                else
                {
                    if (File.Exists(initialDirectory))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialDirectory);
                    }
                    else if (Directory.Exists(Path.GetDirectoryName(initialDirectory)))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialDirectory);
                    }
                    dialog.DefaultFileName = Path.GetFileName(initialDirectory);
                }

                if (defaultExtension != null)
                {
                    dialog.DefaultExtension = defaultExtension;
                }

                if (filters != null)
                {
                    foreach (KeyValuePair<string, string> filter in filters)
                    {
                        dialog.Filters.Add(new CommonFileDialogFilter(filter.Key, filter.Value));
                    }
                }

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    bool selectedItemExist = false;
                    if (isFolderPicker)
                    {
                        selectedItemExist = Directory.Exists(dialog.FileName);
                    }
                    else
                    {
                        selectedItemExist = File.Exists(dialog.FileName);
                    }

                    if (!string.IsNullOrEmpty(dialog.FileName) && (!useExisting || selectedItemExist))
                    {
                        path = dialog.FileName;
                    }
                }
            }
            return path;

        }

        public async Task<string> SaveFileDialog(string initialDirectory, string defaultExtension = null)
        {
            CommonSaveFileDialog dlg = new CommonSaveFileDialog();
            if (!String.IsNullOrEmpty(initialDirectory))
                dlg.InitialDirectory = initialDirectory;
            if (!String.IsNullOrEmpty(defaultExtension))
                dlg.DefaultExtension = defaultExtension;
            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return null;
            return dlg.FileName;
        }

        #endregion
    }
}
