using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.datamodel
{
    public class MediaSettings
    {
        #region ctor's

        public MediaSettings(string mediaRootFolder = null)
        {
            _MediaItemTypes = LoadExtensionList();
            DefaultImageName = "default";
            DefaultImageExtension = ".jpg";
            DefaultThumbSuffix = "_thumb";
            FileTypeIconFolder = @"file-icon";
            NoFileTypeIcon = @"_page.png";
            DefaultIconExtension = @"png";
            PageSize = 50;
            TrimStartString = @"Database\";

            MaxThumbWidth = 60;
            MaxThumbHeight = 60;

            if (mediaRootFolder != null)
                MediaRootFolder = mediaRootFolder;
            else
            {
                MediaRootFolder = @"C:\VarioData\Media";
                using(Database database = new Database())
                {
                    ACClass bsoMedia = database.ACClass.FirstOrDefault(c => c.ACIdentifier == @"BSOMedia");
                    ACClassConfig configItem = bsoMedia.ACClassConfig_ACClass.FirstOrDefault(c => c.ACIdentifier == @"ACClassConfig(MediaRootFolder)");
                    if (configItem != null)
                    {
                        object configValue = configItem.Value.ToString();
                        if (configValue != null)
                            MediaRootFolder = configValue.ToString();
                    }
                }
            }
        }
        
        #endregion

        #region Load

        public void LoadTypeFolder(IACObject item)
        {
            TypeRootFolder = GetItemTypeRootFolder(MediaRootFolder, item);
        }

        /// <summary>
        /// Loading image for supported type
        /// </summary>
        /// <param name="imageInfo"></param>
        public void LoadImage(IImageInfo imageInfo)
        {
            string tmpFolder = GetItemRootFolder(TypeRootFolder, imageInfo as IACObject);
            tmpFolder = Path.Combine(tmpFolder, "images");
            if (Directory.Exists(tmpFolder))
            {
                string defImage = Path.Combine(tmpFolder, FullDefaultImageName);
                if (File.Exists(defImage))
                    imageInfo.DefaultImage = defImage;
                string thumbImage = Path.Combine(tmpFolder, FullDefaultThumbImageName);
                if (File.Exists(thumbImage))
                    imageInfo.DefaultThumbImage = thumbImage;
            }
        }

        #endregion

        #region Definitions

        private List<MediaTypeSettingsItem> _MediaItemTypes;
        public List<MediaTypeSettingsItem> MediaItemTypes
        {
            get
            {
                return _MediaItemTypes;
            }
        }

        public string DefaultImageName { get; private set; }
        public string DefaultImageExtension { get; private set; }
        public string DefaultThumbSuffix { get; private set; }

        public string FileTypeIconFolder { get; private set; }
        public string NoFileTypeIcon { get; private set; }
        public string DefaultIconExtension { get; private set; }

        public int PageSize { get; private set; }

        public string TrimStartString { get; private set; }

        public int MaxThumbWidth { get; private set; }
        public int MaxThumbHeight { get; private set; }

        public string FullDefaultImageName
        {
            get

            {
                return DefaultImageName + DefaultImageExtension;
            }
        }

        public string FullDefaultThumbImageName
        {
            get
            {
                return DefaultImageName + DefaultThumbSuffix + DefaultImageExtension;
            }
        }

        #endregion

        #region Places
        public string MediaRootFolder { get; set; }

        public string TypeRootFolder { get; private set; }

        #endregion

        #region Methods

        #region Methods ->  RootFolder creation
        /*
            material: 10025/Röstkaffee A-Entkoffeiniert 500g
            typeACPathFirst: 
            typeACPath: 
            typeACUrl: Database\ACProject(Root)\ACClass(Root)\ACClass(DatabaseApp)\ACClass(Material)
            dataItemACURL: Database\Material(10025)
        */

        public string GetItemTypeRootFolder(string rootFolder, IACObject aCObject)
        {
            string path = rootFolder;
            if (aCObject == null)
                return path;
            string typeACUrl = aCObject.ACType.GetACUrl();
            typeACUrl = typeACUrl.TrimStart(TrimStartString.ToCharArray());

            foreach (var item in typeACUrl.Split('\\'))
                path = Path.Combine(path, item);

            return path;
        }
        public string GetItemRootFolder(string rootFolder, IACObject aCObject)
        {
            string itemACUrl = aCObject.GetACUrl();
            itemACUrl = itemACUrl.TrimStart(TrimStartString.ToCharArray());
            string paht = Path.Combine(rootFolder, itemACUrl);
            return paht;
        }

        #endregion

        #endregion

        #region private methods
        private List<MediaTypeSettingsItem> LoadExtensionList()
        {
            List<MediaTypeSettingsItem> extensionList = new List<MediaTypeSettingsItem>();

            MediaTypeSettingsItem imageType = new MediaTypeSettingsItem()
            {
                MediaType = MediaItemTypeEnum.Image,
                FolderName = "images",
                Extensions = ".jpg,.jpeg,.png,.gif".Split(',').ToList()
            };

            MediaTypeSettingsItem documentType = new MediaTypeSettingsItem()
            {
                MediaType = MediaItemTypeEnum.Document,
                FolderName = "documents",
                Extensions = ".doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf".Split(',').ToList()
            };

            MediaTypeSettingsItem audioType = new MediaTypeSettingsItem()
            {
                MediaType = MediaItemTypeEnum.Audio,
                FolderName = "audio",
                Extensions = ".mp3".Split(',').ToList()
            };

            MediaTypeSettingsItem videoType = new MediaTypeSettingsItem()
            {
                MediaType = MediaItemTypeEnum.Video,
                FolderName = "video",
                Extensions = ".avi,.mov,.flv".Split(',').ToList()
            };

            extensionList.Add(imageType);
            extensionList.Add(documentType);
            extensionList.Add(audioType);
            extensionList.Add(videoType);

            return extensionList;
        }
        #endregion
    }
}
