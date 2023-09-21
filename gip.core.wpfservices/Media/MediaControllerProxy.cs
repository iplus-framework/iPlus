using gip.core.datamodel;
using gip.core.manager;
using gip.core.media;
using gip.core.wpfservices.Manager;
using Microsoft.WindowsAPICodePack.Dialogs;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

        #region Methods

        public byte[] ResizeImage(string fileName, int maxWidth, int maxHeight, string quality = "Medium")
        {
            SKData data = null;
            SKFilterQuality qualitySK;

            switch (quality.ToLower())
            {
                case "medium":
                    qualitySK = SKFilterQuality.Medium;
                    break;
                case "low":
                    qualitySK = SKFilterQuality.Low;
                    break;
                case "high":
                    qualitySK = SKFilterQuality.High;
                    break;
                case "none":
                    qualitySK = SKFilterQuality.None;
                    break;
                default:
                    throw new ArgumentException("Invalid quality value.");
            }

            using (FileStream ms = new FileStream(fileName, FileMode.Open))
            {
                using (SKBitmap sourceBitmap = SKBitmap.Decode(ms))
                {
                    int height = Math.Min(maxHeight, sourceBitmap.Height);
                    int width = Math.Min(maxWidth, sourceBitmap.Width);
                    using (SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), qualitySK))
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

        #endregion
    }
}
