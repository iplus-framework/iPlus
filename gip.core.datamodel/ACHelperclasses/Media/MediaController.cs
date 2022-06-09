using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;

namespace gip.core.datamodel
{
    public class MediaController
    {

        #region const
        public const string Const_DefImage = @"empty.jpg";
        public const string Const_DefThumbImage = @"empty_thumb.jpg";
        #endregion

        #region DI

        public string RootFolder { get; private set; }
        public MediaSettings MediaSettings { get; private set; }


        public string ItemRootFolder { get; set; }
        public IACObject ACObject { get; set; }

        #endregion

        #region c'tors 

        public MediaController(MediaSettings mediaSettings, IACObject aCObject)
        {
            ACObject = aCObject;
            MediaSettings = mediaSettings;
            RootFolder = MediaSettings.MediaRootFolder;
            string typeRootFolder = MediaSettings.GetItemTypeRootFolder(RootFolder, ACObject);
            ItemRootFolder = MediaSettings.GetItemRootFolder(typeRootFolder, ACObject);
            Items = new Dictionary<MediaItemTypeEnum, MediaSet>();
            LoadMediaSets();
        }

        #endregion

        public Dictionary<MediaItemTypeEnum, MediaSet> Items { get; private set; }

        #region public methods

        public MediaItemPresentation Upload(MediaItemPresentation item)
        {
            if (string.IsNullOrEmpty(item.EditFilePath) || !File.Exists(item.EditFilePath))
                return item;

            string extension = Path.GetExtension(item.EditFilePath);
            MediaSet mediaSet = Items.Where(c => c.Value.MediaTypeSettings.Extensions.Contains(extension)).Select(c => c.Value).FirstOrDefault();
            if (mediaSet.MediaTypeSettings.MediaType == MediaItemTypeEnum.Image)
                UploadImage(mediaSet, item);
            else
                UploadDocument(mediaSet, item);
            return item;
        }


        public MediaItemPresentation UploadImage(MediaSet mediaSet, MediaItemPresentation item)
        {
            string extension = Path.GetExtension(item.EditFilePath);

            if (item.IsDefault)
            {
                item.FilePath = Path.Combine(mediaSet.ItemRootFolder, MediaSettings.FullDefaultImageName);
                item.Name = MediaSettings.FullDefaultImageName;
            }
            else if (item.IsNew)
            {
                item.FilePath = Path.Combine(mediaSet.ItemRootFolder, Path.GetFileName(item.EditFilePath));
                item.Name = Path.GetFileName(item.EditFilePath);
            }

            CheckDirectory(item.FilePath);
            if (!string.IsNullOrEmpty(item.EditFilePath))
                UpladFile(item.EditFilePath, item.FilePath);
            else if(item.Image != null)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)item.Image));
                using (FileStream stream = new FileStream(item.FilePath, FileMode.Create))
                    encoder.Save(stream);
            }

            string thumbFileName = MediaSettings.FullDefaultThumbImageName;
            if (!item.IsDefault)
                thumbFileName = Path.GetFileNameWithoutExtension(item.FilePath) + MediaSettings.DefaultThumbSuffix + extension;
            string fullThumbFileName = Path.Combine(mediaSet.ItemRootFolder, thumbFileName);
            if (item.IsGenerateThumb)
            {
                RenderThumbImage(item.EditFilePath, fullThumbFileName);
                item.ThumbPath = fullThumbFileName;
                item.IsGenerateThumb = false;
            }
            else if (!string.IsNullOrEmpty(item.EditThumbPath) && File.Exists(item.EditThumbPath))
            {
                UpladFile(item.EditThumbPath, fullThumbFileName);
                item.ThumbPath = fullThumbFileName;
            }

            item.EditFilePath = null;
            item.EditThumbPath = null;
            item.LoadImage(true);
            item.IsNew = false;
            return item;
        }

        private MediaItemPresentation UploadDocument(MediaSet mediaSet, MediaItemPresentation item)
        {
            string extension = Path.GetExtension(item.EditFilePath);
            if (item.IsNew)
                item.FilePath = Path.Combine(mediaSet.ItemRootFolder, Path.GetFileName(item.EditFilePath));
            CheckDirectory(item.FilePath);
            UpladFile(item.EditFilePath, item.FilePath);
            item.Name = Path.GetFileName(item.EditFilePath);

            if (!string.IsNullOrEmpty(item.EditThumbPath) && File.Exists(item.EditThumbPath))
            {
                if (item.ThumbExistAndIsNotGeneric())
                    DeleteWithRetry(item.ThumbPath);
                item.ThumbPath =
                            Path.GetFileNameWithoutExtension(item.FilePath)
                            + MediaSettings.DefaultThumbSuffix
                            + Path.GetExtension(item.EditThumbPath);
                item.ThumbPath = Path.Combine(mediaSet.ItemRootFolder, item.ThumbPath);
                UpladFile(item.EditThumbPath, item.ThumbPath);
            }
            else
                item.ThumbPath = GetIconFilePath(extension);
            item.EditFilePath = null;
            item.EditThumbPath = null;
            item.LoadImage(false);
            item.IsNew = false;
            return item;
        }

        public void CheckDirectory(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private void UpladFile(string sourcePath, string targetPath)
        {
            if (File.Exists(targetPath))
                DeleteWithRetry(targetPath);
            File.Copy(sourcePath, targetPath);
        }

        public  void RenderThumbImage(string sourceFile, string targetFile)
        {
            using (Image image = Image.FromFile(sourceFile))
            {
                RenderThumbImage(image, targetFile);
            }
        }

        public void RenderThumbImage(Image image, string targetFile)
        {
            double thumbWidth = 0;
            double thumbHeight = 0;

            if (image.Width >= image.Height)
            {
                thumbWidth = MediaSettings.MaxThumbWidth;
                thumbHeight = image.Height * (((double)MediaSettings.MaxThumbWidth) / ((double)image.Width));
            }
            else
            {
                thumbHeight = MediaSettings.MaxThumbHeight;
                thumbWidth = image.Width * (((double)MediaSettings.MaxThumbHeight) / ((double)image.Height));
            }

            using (Image resizedImage = ImageResize.ResizeImage(image, thumbWidth, thumbHeight))
            {
                if (File.Exists(targetFile))
                    DeleteWithRetry(targetFile);
                resizedImage.Save(targetFile);
                resizedImage.Dispose();
            }
            image.Dispose();
        }

        public void DeleteFile(MediaItemTypeEnum mediaType, string fileName)
        {
            MediaSet mediaSet = Items[mediaType];
            string path = Path.Combine(mediaSet.ItemRootFolder, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public bool DeleteWithRetry(string file)
        {
            bool isDeleted = false;
            int cntTry = 0;
            while (!isDeleted && cntTry < 3)
            {
                cntTry++;
                try
                {
                    Thread.Sleep(100 * 5);
                    if (File.Exists(file))
                        File.Delete(file);
                    isDeleted = true;
                }
                catch (Exception ec)
                {
                    if (gip.core.datamodel.Database.Root != null)
                    {
                        gip.core.datamodel.Database.Root.Messages.LogException("MediaController", "DeleteWithRetry", ec);
                    }
                }
            }
            return isDeleted;
        }

        public string GetEmptyImagePath()
        {
            return Path.Combine(RootFolder, Const_DefImage);
        }

        public string GetEmptyThumbImagePath()
        {
            return Path.Combine(RootFolder, Const_DefThumbImage);
        }

        public string GetIconFilePath(string extension)
        {
            string iconPath = "";
            string folder = Path.Combine(RootFolder, MediaSettings.FileTypeIconFolder);
            if (Directory.Exists(folder))
            {
                iconPath = Path.Combine(folder, extension.TrimStart('.').ToLower() + "." + MediaSettings.DefaultIconExtension);
                if (!File.Exists(iconPath))
                    iconPath = Path.Combine(folder, MediaSettings.NoFileTypeIcon + MediaSettings.DefaultIconExtension);
            }
            return iconPath;
        }

        #endregion

        #region private methods

        private void LoadMediaSets()
        {
            Items.Add(MediaItemTypeEnum.Image, GetMediaSet(MediaItemTypeEnum.Image));
            Items.Add(MediaItemTypeEnum.Document, GetMediaSet(MediaItemTypeEnum.Document));
            Items.Add(MediaItemTypeEnum.Audio, GetMediaSet(MediaItemTypeEnum.Audio));
            Items.Add(MediaItemTypeEnum.Video, GetMediaSet(MediaItemTypeEnum.Video));
        }

        public MediaSet GetMediaSet(MediaItemTypeEnum mediaType)
        {
            var settings = MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            return new MediaSet(this, 1, settings, "", "", true);
        }

        #endregion
    }
}
