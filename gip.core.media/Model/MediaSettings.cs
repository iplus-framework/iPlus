using gip.core.datamodel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.media
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

            TempFolder = Path.Combine(Path.GetTempPath(), "MediaTemp");
            if(!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
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

        private string _TempFolder;
        public string TempFolder
        {
            get
            {
                if(!Directory.Exists(_TempFolder))
                {
                    Directory.CreateDirectory(_TempFolder);
                }
                return _TempFolder;
            }
            private set
            {
                _TempFolder = value;
            }
        }

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
                Extensions = ".doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf,.xml".Split(',').ToList()
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
