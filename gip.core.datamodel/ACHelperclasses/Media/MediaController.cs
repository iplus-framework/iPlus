using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace gip.core.datamodel
{
    public class MediaController
    {

        #region DI

        public string RootFolder { get; private set; }
        public MediaSettings MediaSettings { get; private set; }


        public string ItemRootFolder { get; set; }
        public IACObject ACObject { get; set; }

        public string DefaultImage { get; private set; }
        public string DefaultThumbImage { get; private set; }
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
            LoadDefaultImage();
        }

        #endregion

        public Dictionary<MediaItemTypeEnum, MediaSet> Items { get; private set; }

        #region public methods


       

        public void UploadFile(string localFileName)
        {
            string recomendedFileName = Path.GetFileName(localFileName);
            UploadFile(localFileName, recomendedFileName);
        }

        public void UploadFile(string localFileName, string recomendedFileName)
        {
            if (File.Exists(localFileName))
            {
                string extension = Path.GetExtension(recomendedFileName);
                MediaSet mediaSet = Items.Where(c => c.Value.MediaTypeSettings.Extensions.Contains(extension)).Select(c => c.Value).FirstOrDefault();
                string path = Path.Combine(mediaSet.ItemRootFolder, recomendedFileName);
                if (!Directory.Exists(mediaSet.ItemRootFolder))
                    Directory.CreateDirectory(mediaSet.ItemRootFolder);
                File.Copy(localFileName, path);
            }
        }

        public void UploadImage(string fileName, string thumbFileName, bool generateThumb, bool isDefault)
        {
            string extension = Path.GetExtension(fileName);
            MediaSet mediaSet = Items[MediaItemTypeEnum.Image];
            string newFileName = "";
            string newThumbFileName = "";
            if (isDefault)
            {
                newFileName = MediaSettings.FullDefaultImageName;
                newThumbFileName = MediaSettings.FullDefaultThumbImageName;
            }
            else
            {
                newFileName = Path.GetFileNameWithoutExtension(fileName) + extension;
                newThumbFileName = Path.GetFileNameWithoutExtension(fileName) + MediaSettings.DefaultThumbSuffix + extension;
            }
            UploadFile(fileName, newFileName);
            if(!string.IsNullOrEmpty(thumbFileName) && File.Exists(thumbFileName))
                UploadFile(thumbFileName, newThumbFileName);
            else if (string.IsNullOrEmpty(thumbFileName) && generateThumb)
            {
                using (Image image = Image.FromFile(fileName))
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
                        string thumbFullPath = Path.Combine(mediaSet.ItemRootFolder, newThumbFileName);
                        resizedImage.Save(thumbFullPath);
                        resizedImage.Dispose();
                    }
                    image.Dispose();
                }
            }
        }

        public void DeleteFile(MediaItemTypeEnum mediaType, string fileName)
        {
            MediaSet mediaSet = Items[mediaType];
            string path = Path.Combine(mediaSet.ItemRootFolder, fileName);
            if (File.Exists(path))
                File.Delete(path);
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


        public void LoadDefaultImage()
        {
            var imageMediaSettings = MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == MediaItemTypeEnum.Image);
            MediaSet imageMediaSet = new MediaSet(this, 1, imageMediaSettings, MediaSettings.DefaultImageName, "", true);
            if (Directory.Exists(imageMediaSet.ItemRootFolder))
            {
                string defaultPath = Path.Combine(imageMediaSet.ItemRootFolder, MediaSettings.FullDefaultImageName);
                if (File.Exists(defaultPath))
                    DefaultImage = defaultPath;

                string defaultThumbPath = Path.Combine(imageMediaSet.ItemRootFolder, MediaSettings.FullDefaultThumbImageName);
                if (File.Exists(defaultThumbPath))
                    DefaultThumbImage = defaultThumbPath;
            }
        }

        #endregion
    }
}
