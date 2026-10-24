// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.media
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Media'}de{'Media'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOMedia : ACBSO
    {
        #region Events

        public event EventHandler OnDefaultImageDelete;

        #endregion

        #region const


        #endregion

        #region Configuration

        private ACPropertyConfigValue<string> _MediaRootFolder;
        [ACPropertyConfig("MediaRootFolder")]
        public string MediaRootFolder
        {
            get
            {
                return _MediaRootFolder.ValueT;
            }
            set
            {
                _MediaRootFolder.ValueT = value;
            }
        }

        #endregion

        #region Settings

        public const string BGWorkerMehtod_DeleteFile = @"DeleteFile";


        /// <summary>
        /// Client-side UI bridge. Only used for file dialogs, because they must run on the client.
        /// </summary>
        public ACMediaController MediaController { get; private set; }

        /// <summary>
        /// Server-side media service. All file system access is delegated to it, so that the
        /// component also works when the client runs as browser or mobile application.
        /// </summary>
        public ACMediaService MediaService { get; private set; }

        #endregion

        #region c´tors
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOMedia(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MediaRootFolder = new ACPropertyConfigValue<string>(this, MediaSettings.GetMediaRootConfigName(), string.Empty);
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _ = MediaRootFolder;
            MediaController = ACMediaController.GetServiceInstance(this);
            MediaService = ACMediaService.GetServiceInstance(this);

            if (!IsMediaRootConfigured)
            {
                SendMessage(new Msg
                {
                    MessageLevel = eMsgLevel.Error,
                    Message = $"Media root folder is not configured. Set the config value '{MediaSettings.GetMediaRootConfigName()}' of BSOMedia for this operating system."
                });
            }

            return true;
        }

        /// <summary>
        /// False when no media root folder is configured for this operating system. All media operations stay disabled.
        /// </summary>
        public bool IsMediaRootConfigured
        {
            get
            {
                return MediaService != null && MediaService.IsMediaRootConfigured;
            }
        }

        public async override Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _MediaRootFolder = null;
            return await base.ACDeInit(deleteACClassTask);
        }

        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);

            Global.ControlModes result = base.OnGetControlModes(vbControl);
            if (result < Global.ControlModes.Enabled)
                return result;


            return result;
        }

        public override void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.TabItemActivated)
            {
                string tablItem = actionArgs.DropObject.VBContent;
                if (tablItem.StartsWith("*"))
                    tablItem = tablItem.TrimStart('*');
                MediaItemTypeEnum activeTab = ActiveTab;
                if (Enum.TryParse<MediaItemTypeEnum>(tablItem, out activeTab))
                    ActiveTab = activeTab;
            }
            else
                base.ACAction(actionArgs);
        }

        #endregion

        #region Properties


        #region Properties -> MediaSets

        public MediaSet ImageMediaSet { get; private set; }
        public MediaSet DocumentMediaSet { get; private set; }
        public MediaSet AudioMediaSet { get; private set; }
        public MediaSet VideoMediaSet { get; private set; }

        #endregion

        #region Properties -> Image

        private MediaItemPresentation _SelectedImage;
        /// <summary>
        /// Selected property for string
        /// </summary>
        /// <value>The selected Image</value>
        [ACPropertySelected(9999, "Image", "en{'TODO: Image'}de{'TODO: Image'}")]
        public MediaItemPresentation SelectedImage
        {
            get
            {
                return _SelectedImage;
            }
            set
            {
                if (_SelectedImage != value)
                {
                    _SelectedImage = value;
                    OnPropertyChanged();
                    OnPropertyChanged($"{nameof(SelectedImage)}\\{nameof(SelectedImage.TempFilePath)}");
                }
            }
        }

        private List<MediaItemPresentation> _ImageList;
        /// <summary>
        /// List property for string
        /// </summary>
        /// <value>The Image list</value>
        [ACPropertyList(9999, "Image")]
        public List<MediaItemPresentation> ImageList
        {
            get
            {
                if (_ImageList == null)
                    _ImageList = new List<MediaItemPresentation>();
                return _ImageList;
            }
            set
            {
                _ImageList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Properties -> Document

        private MediaItemPresentation _SelectedDocument;
        /// <summary>
        /// Selected property for string
        /// </summary>
        /// <value>The selected Document</value>
        [ACPropertySelected(9999, "Document", "en{'TODO: Document'}de{'TODO: Document'}")]
        public MediaItemPresentation SelectedDocument
        {
            get
            {
                return _SelectedDocument;
            }
            set
            {
                if (_SelectedDocument != value)
                {
                    _SelectedDocument = value;
                    OnPropertyChanged();
                    OnPropertyChanged($"{nameof(SelectedDocument)}\\{nameof(SelectedDocument.TempFilePath)}");
                }
            }
        }

        private List<MediaItemPresentation> _DocumentList;
        /// <summary>
        /// List property for string
        /// </summary>
        /// <value>The Document list</value>
        [ACPropertyList(9999, "Document")]
        public List<MediaItemPresentation> DocumentList
        {
            get
            {
                if (_DocumentList == null)
                    _DocumentList = new List<MediaItemPresentation>();
                return _DocumentList;
            }
            set
            {
                _DocumentList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Properties -> Audio

        private MediaItemPresentation _SelectedAudio;
        /// <summary>
        /// Selected property for string
        /// </summary>
        /// <value>The selected Audio</value>
        [ACPropertySelected(9999, "Audio", "en{'TODO: Audio'}de{'TODO: Audio'}")]
        public MediaItemPresentation SelectedAudio
        {
            get
            {
                return _SelectedAudio;
            }
            set
            {
                if (_SelectedAudio != value)
                {
                    _SelectedAudio = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<MediaItemPresentation> _AudioList;
        /// <summary>
        /// List property for string
        /// </summary>
        /// <value>The Audio list</value>
        [ACPropertyList(9999, "Audio")]
        public List<MediaItemPresentation> AudioList
        {
            get
            {
                if (_AudioList == null)
                    _AudioList = new List<MediaItemPresentation>();
                return _AudioList;
            }
            set
            {
                _AudioList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Properties -> Video

        private MediaItemPresentation _SelectedVideo;
        /// <summary>
        /// Selected property for string
        /// </summary>
        /// <value>The selected Video</value>
        [ACPropertySelected(9999, "Video", "en{'TODO: Video'}de{'TODO: Video'}")]
        public MediaItemPresentation SelectedVideo
        {
            get
            {
                return _SelectedVideo;
            }
            set
            {
                if (_SelectedVideo != value)
                {
                    _SelectedVideo = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<MediaItemPresentation> _VideoList;
        /// <summary>
        /// List property for string
        /// </summary>
        /// <value>The Video list</value>
        [ACPropertyList(9999, "Video")]
        public List<MediaItemPresentation> VideoList
        {
            get
            {
                if (_VideoList == null)
                    _VideoList = new List<MediaItemPresentation>();
                return _VideoList;
            }
            set
            {
                _VideoList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Properties -> SelectedTab


        private MediaItemTypeEnum _ActiveTab = MediaItemTypeEnum.Image;
        [ACPropertyInfo(80, "ActiveTab", "en{'ActiveTab'}de{'ActiveTab'}")]
        public MediaItemTypeEnum ActiveTab
        {
            get
            {
                return _ActiveTab;
            }
            set
            {
                if (_ActiveTab != value)
                {
                    _ActiveTab = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                }
            }
        }

        [ACPropertyInfo(81, "SelectedMediaItemPresentation", "en{'SelectedMediaItemPresentation'}de{'SelectedMediaItemPresentation'}")]
        public MediaItemPresentation SelectedMediaItemPresentation
        {
            get
            {
                MediaItemPresentation item = null;
                switch (ActiveTab)
                {
                    case MediaItemTypeEnum.Image:
                        item = SelectedImage;
                        break;
                    case MediaItemTypeEnum.Document:
                        item = SelectedDocument;
                        break;
                    case MediaItemTypeEnum.Audio:
                        item = SelectedAudio;
                        break;
                    case MediaItemTypeEnum.Video:
                        item = SelectedVideo;
                        break;
                }
                return item;
            }
            set
            {
                switch (ActiveTab)
                {
                    case MediaItemTypeEnum.Image:
                        SelectedImage = value;
                        break;
                    case MediaItemTypeEnum.Document:
                        SelectedDocument = value;
                        break;
                    case MediaItemTypeEnum.Audio:
                        SelectedAudio = value;
                        break;
                    case MediaItemTypeEnum.Video:
                        SelectedVideo = value;
                        break;
                }
                OnPropertyChanged();
                SelectedMediaItemPresentation_OnPropertyChanged();
            }
        }

        public void SelectedMediaItemPresentation_OnPropertyChanged()
        {
            switch (ActiveTab)
            {
                case MediaItemTypeEnum.Image:
                    OnPropertyChanged(nameof(SelectedImage));
                    break;
                case MediaItemTypeEnum.Document:
                    OnPropertyChanged(nameof(SelectedDocument));
                    break;
                case MediaItemTypeEnum.Audio:
                    OnPropertyChanged(nameof(SelectedAudio));
                    break;
                case MediaItemTypeEnum.Video:
                    OnPropertyChanged(nameof(SelectedVideo));
                    break;
            }
        }

        [ACPropertyInfo(82, "MediaItemPresentationList", "en{'MediaItemPresentationList'}de{'MediaItemPresentationList'}")]
        public List<MediaItemPresentation> MediaItemPresentationList
        {
            get
            {
                List<MediaItemPresentation> list = null;
                switch (ActiveTab)
                {
                    case MediaItemTypeEnum.Image:
                        list = ImageList;
                        break;
                    case MediaItemTypeEnum.Document:
                        list = DocumentList;
                        break;
                    case MediaItemTypeEnum.Audio:
                        list = AudioList;
                        break;
                    case MediaItemTypeEnum.Video:
                        list = VideoList;
                        break;
                }
                return list;
            }
            set
            {
                switch (ActiveTab)
                {
                    case MediaItemTypeEnum.Image:
                        _ImageList = value;
                        break;
                    case MediaItemTypeEnum.Document:
                        _DocumentList = value;
                        break;
                    case MediaItemTypeEnum.Audio:
                        _AudioList = value;
                        break;
                    case MediaItemTypeEnum.Video:
                        _VideoList = value;
                        break;
                }
                MediaItemPresentationList_OnPropertyChanged();
            }
        }

        public void MediaItemPresentationList_OnPropertyChanged()
        {
            switch (ActiveTab)
            {
                case MediaItemTypeEnum.Image:
                    OnPropertyChanged(nameof(ImageList));
                    break;
                case MediaItemTypeEnum.Document:
                    OnPropertyChanged(nameof(DocumentList));
                    break;
                case MediaItemTypeEnum.Audio:
                    OnPropertyChanged(nameof(AudioList));
                    break;
                case MediaItemTypeEnum.Video:
                    OnPropertyChanged(nameof(VideoList));
                    break;
            }
            OnPropertyChanged(nameof(MediaItemPresentationList));
        }

        #endregion

        #endregion


        #region Methods

        private IACObject _CurrentACObject;
        [ACPropertyInfo(83, "CurrentACObject", "en{'CurrentACObject'}de{'CurrentACObject'}")]
        public IACObject CurrentACObject
        {
            get
            {
                return _CurrentACObject;
            }
            private set
            {
                if (_CurrentACObject != value)
                {
                    _CurrentACObject = value;
                    OnPropertyChanged(nameof(CurrentACObject));
                }
            }
        }

        private static readonly string[] MediaItemSelectionProps = new string[]
        {
            nameof(ActiveTab), nameof(SelectedImage), nameof(SelectedDocument), nameof(SelectedAudio), nameof(SelectedVideo)
        };

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                case nameof(SetFilePath):
                case nameof(IsEnabledSetFilePath):
                case nameof(SetFileThumbPath):
                case nameof(IsEnabledSetFileThumbPath):
                case nameof(UploadFile):
                case nameof(IsEnabledUploadFile):
                case nameof(DownloadItem):
                case nameof(IsEnabledDownloadItem):
                case nameof(OpenItem):
                case nameof(IsEnabledOpenItem):
                case nameof(ShowInFolder):
                case nameof(IsEnabledShowInFolder):
                case nameof(Delete):
                case nameof(IsEnabledDelete):
                    return MediaItemSelectionProps;
                case nameof(Add):
                case nameof(IsEnabledAdd):
                    return new string[] { nameof(CurrentACObject) };
            }
            return base.GetPropsToObserveForIsEnabled(acMethodName);
        }

        public void Clean()
        {
            CurrentACObject = null;
            ImageList = null;
            DocumentList = null;
            AudioList = null;
            VideoList = null;

            SelectedImage = null;
            SelectedDocument = null;
            SelectedAudio = null;
            SelectedVideo = null;
        }

        public void LoadMedia(IACObject aCObject)
        {
            if (CurrentACObject == aCObject || !IsMediaRootConfigured)
                return;

            CurrentACObject = aCObject;

            ImageMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Image);
            DocumentMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Document);
            AudioMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Audio);
            VideoMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Video);

            _ImageList = LoadItems(ImageMediaSet);
            if (_ImageList != null && _ImageList.Any())
                SelectedImage = _ImageList.FirstOrDefault();

            _DocumentList = LoadItems(DocumentMediaSet);
            if (_DocumentList != null && _DocumentList.Any())
                SelectedDocument = _DocumentList.FirstOrDefault();

            _AudioList = LoadItems(AudioMediaSet);
            if (_AudioList != null && _AudioList.Any())
                SelectedAudio = _AudioList.FirstOrDefault();

            _VideoList = LoadItems(VideoMediaSet);
            if (_VideoList != null && _VideoList.Any())
                SelectedVideo = _VideoList.FirstOrDefault();

            OnPropertyChanged(nameof(ImageList));
            OnPropertyChanged(nameof(DocumentList));
            OnPropertyChanged(nameof(AudioList));
            OnPropertyChanged(nameof(VideoList));
        }

        private MediaSet GetMediaSet(IACObject aCObject, MediaItemTypeEnum mediaType)
        {
            MediaSet mediaSet = new MediaSet();
            mediaSet.MediaType = mediaType;
            mediaSet.MediaSettings = MediaController.MediaSettings;
            mediaSet.MediaTypeSettings = mediaSet.MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            mediaSet.PageSize = mediaSet.MediaSettings.PageSize;
            mediaSet.ExtensionQuery = string.Join(";", mediaSet.MediaTypeSettings.Extensions.Select(c => "*" + c));
            return mediaSet;
        }

        public MediaSet GetMediaSet(MediaItemTypeEnum mediaType)
        {
            MediaSet mediaSet = null;
            switch (mediaType)
            {
                case MediaItemTypeEnum.Image:
                    mediaSet = ImageMediaSet;
                    break;
                case MediaItemTypeEnum.Document:
                    mediaSet = DocumentMediaSet;
                    break;
                case MediaItemTypeEnum.Audio:
                    mediaSet = AudioMediaSet;
                    break;
                case MediaItemTypeEnum.Video:
                    mediaSet = VideoMediaSet;
                    break;
            }
            return mediaSet;
        }

        private List<MediaItemPresentation> LoadItems(MediaSet mediaSet)
        {
            if (mediaSet == null || CurrentACObject == null)
                return null;

            MediaItemListResult result = MediaService.GetFiles(
                CurrentACObject.ACType.GetACUrl(),
                CurrentACObject.GetACUrl(),
                mediaSet.MediaType,
                1,
                null,
                mediaSet.Order,
                mediaSet.IsAscending);

            if (result == null)
                return null;

            if (result.Message != null && result.Message.MessageLevel >= eMsgLevel.Error)
                SendMessage(result.Message);

            mediaSet.ItemsCount = result.ItemsCount;
            mediaSet.ItemRootFolder = result.ItemRootFolder;
            mediaSet.ExtensionQuery = result.ExtensionQuery;

            if (result.Items == null)
                return null;

            List<MediaItemPresentation> items = new List<MediaItemPresentation>();
            foreach (MediaItemInfo info in result.Items)
                items.Add(CreatePresentationItem(info, mediaSet.MediaType));
            return items;
        }

        /// <summary>
        /// Creates the client-side presentation item and materializes the content that the
        /// server returned into local temp files, so that the UI can display it.
        /// </summary>
        private MediaItemPresentation CreatePresentationItem(MediaItemInfo info, MediaItemTypeEnum mediaType)
        {
            MediaItemPresentation item = new MediaItemPresentation();
            item.MediaType = mediaType;
            item.Name = info.Name;
            item.FilePath = info.FilePath;
            item.ThumbPath = info.ThumbPath;
            item.IsDefault = info.IsDefault;
            item.IsNew = false;

            Guid id = Guid.NewGuid();
            item.TempFilePath = CreateTempFileFromService(info.FilePath, id.ToString());
            if (!string.IsNullOrEmpty(info.ThumbPath))
                item.TempThumbPath = CreateTempFileFromService(info.ThumbPath, id + "_temp");
            else if (mediaType == MediaItemTypeEnum.Image)
                item.TempThumbPath = item.TempFilePath;
            else
                item.TempThumbPath = CreateTempFileFromService(MediaService.GetIconFilePath(Path.GetExtension(info.FilePath)), id + "_temp");

            return item;
        }

        private string CreateTempFileFromService(string filePath, string tempFileName)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            MediaContentResult content = MediaService.GetFileContent(filePath);
            if (content == null || !content.Success || content.Content == null)
                return null;

            return WriteTempFile(content.Content, content.Extension, tempFileName);
        }

        private string WriteTempFile(byte[] content, string extension, string tempFileName)
        {
            string tempFolder = MediaController.MediaSettings.TempFolder;
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string tempFile = Path.Combine(tempFolder, tempFileName + extension);
            File.WriteAllBytes(tempFile, content);
            return tempFile;
        }

        #region Methods -> ACMethods

        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("SetFilePath", "en{'...'}de{'...'}", 9999, false, false, true)]
        public async Task SetFilePath()
        {
            if (!IsEnabledSetFilePath())
                return;
            string filePath = await MediaController.OpenFileDialog(false, SelectedMediaItemPresentation.EditFilePath, true);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                SelectedMediaItemPresentation.EditFilePath = filePath;
                SelectedMediaItemPresentation_OnPropertyChanged();
            }
        }

        public bool IsEnabledSetFilePath()
        {
            return IsMediaRootConfigured && SelectedMediaItemPresentation != null;
        }


        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("SetFileThumbPath", "en{'...'}de{'...'}", 9999, false, false, true)]
        public async Task SetFileThumbPath()
        {
            if (!IsEnabledSetFileThumbPath())
                return;
            string filePath = await MediaController.OpenFileDialog(false, SelectedMediaItemPresentation.EditFilePath, true);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                SelectedMediaItemPresentation.EditThumbPath = filePath;
                SelectedMediaItemPresentation_OnPropertyChanged();
            }
        }

        public bool IsEnabledSetFileThumbPath()
        {
            return IsEnabledSetFilePath();
        }

        /// <summary>
        /// Method UploadFile
        /// </summary>
        [ACMethodInfo("UploadFile", "en{'Upload file'}de{'Datei hochladen'}", 9999, false, false, true)]
        public void UploadFile()
        {
            if (!IsEnabledUploadFile())
                return;
            try
            {
                MediaSet mediaSet = GetMediaSet(ActiveTab);
                MediaItemPresentation item = SelectedMediaItemPresentation;
                string extension = Path.GetExtension(item.EditFilePath);
                if (!mediaSet.ExtensionQuery.Contains(extension))
                {
                    Messages.WarningAsync(this, "Warning50068", false, extension, mediaSet.ExtensionQuery);
                    return;
                }

                byte[] content = File.ReadAllBytes(item.EditFilePath);
                byte[] thumbContent = null;
                if (!string.IsNullOrEmpty(item.EditThumbPath) && File.Exists(item.EditThumbPath))
                    thumbContent = File.ReadAllBytes(item.EditThumbPath);

                MediaContentResult result = MediaService.UploadFile(
                    CurrentACObject.ACType.GetACUrl(),
                    CurrentACObject.GetACUrl(),
                    mediaSet.MediaType,
                    Path.GetFileName(item.EditFilePath),
                    content,
                    item.IsDefault,
                    item.IsGenerateThumb,
                    thumbContent);

                if (result == null || !result.Success)
                {
                    SendMessage(result?.Message ?? new Msg { MessageLevel = eMsgLevel.Error, Message = "Upload failed." });
                    return;
                }

                item.FilePath = result.FilePath ?? Path.Combine(mediaSet.ItemRootFolder, result.FileName);
                item.Name = result.FileName;
                item.IsNew = false;
                item.IsGenerateThumb = false;
                item.EditFilePath = null;
                item.EditThumbPath = null;

                Guid id = Guid.NewGuid();
                item.TempFilePath = WriteTempFile(result.Content, result.Extension, id.ToString());

                MediaContentResult thumbResult = MediaService.GetThumbContent(item.FilePath, mediaSet.MediaType);
                if (thumbResult != null && thumbResult.Success && thumbResult.Content != null)
                {
                    string thumbPath = MediaService.GetThumbPath(item.FilePath);
                    item.ThumbPath = string.IsNullOrEmpty(thumbPath) ? item.FilePath : MediaService.GetRelativePath(thumbPath);
                    item.TempThumbPath = WriteTempFile(thumbResult.Content, thumbResult.Extension, id + "_temp");
                }

                SelectedMediaItemPresentation_OnPropertyChanged();
            }
            catch (Exception ec)
            {
                Msg msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = ec.Message };
                SendMessage(msg);
            }

        }
        public bool IsEnabledUploadFile()
        {
            return IsMediaRootConfigured
                && SelectedMediaItemPresentation != null
                && !string.IsNullOrEmpty(SelectedMediaItemPresentation.EditFilePath)
                && File.Exists(SelectedMediaItemPresentation.EditFilePath)
                && (!SelectedMediaItemPresentation.IsDefault ||
                    (

                        SelectedMediaItemPresentation.IsGenerateThumb ||
                        (!string.IsNullOrEmpty(SelectedMediaItemPresentation.EditThumbPath) && File.Exists(SelectedMediaItemPresentation.EditFilePath))
                     )
                 );
        }

        #endregion

        #region Methods -> ACMethods -> Common

        /// <summary>
        /// Method DownloadImage
        /// </summary>
        [ACMethodInfo("DownloadImage", "en{'Download'}de{'Herunterladen'}", 9999, false, false, true)]
        public async Task DownloadItem()
        {
            if (!IsEnabledDownloadItem())
                return;
            await DownloadFile(SelectedMediaItemPresentation.FilePath);
        }
        public bool IsEnabledDownloadItem()
        {
            return IsEnabledOpenItem();
        }

        /// <summary>
        /// Method DownloadDocument
        /// </summary>
        [ACMethodInfo("OpenItem", "en{'Open'}de{'Öffnen'}", 9999, false, false, true)]
        public void OpenItem()
        {
            if (!IsEnabledOpenItem())
                return;

            string filePath = GetLocalFilePath(SelectedMediaItemPresentation);
            if (string.IsNullOrEmpty(filePath))
            {
                SendMessage(new Msg { MessageLevel = eMsgLevel.Warning, Message = "File does not exist." });
                return;
            }

            try
            {
                // .NET 5+ way to open a file with the default application
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // Handle error (e.g., show a message to the user)
                SendMessage(new Msg { MessageLevel = eMsgLevel.Error, Message = $"Could not open file: {ex.Message}" });
            }
        }

        public bool IsEnabledOpenItem()
        {
            return IsMediaRootConfigured
               && SelectedMediaItemPresentation != null
               && !string.IsNullOrEmpty(SelectedMediaItemPresentation.FilePath);
        }

        /// <summary>
        /// Method DownloadDocument
        /// </summary>
        [ACMethodInfo("ShowInFolder", "en{'Show in folder'}de{'Im Ordner anzeigen'}", 9999, false, false, true)]
        public void ShowInFolder()
        {
            if (!IsEnabledOpenItem())
                return;

            string filePath = GetLocalFilePath(SelectedMediaItemPresentation);
            var folderPath = string.IsNullOrEmpty(filePath) ? null : Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                try
                {
                    // .NET 5+ way to open a file with the default application
                    var psi = new ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    // Handle error (e.g., show a message to the user)
                    SendMessage(new Msg { MessageLevel = eMsgLevel.Error, Message = $"Could not open file: {ex.Message}" });
                }
            }
            else
            {
                SendMessage(new Msg { MessageLevel = eMsgLevel.Warning, Message = "File does not exist." });
            }
        }

        public bool IsEnabledShowInFolder()
        {
            return IsEnabledOpenItem();
        }

        /// <summary>
        /// Returns a path that is accessible on the client. The content is fetched from the
        /// server and materialized as a local temp file when it is not available yet.
        /// </summary>
        private string GetLocalFilePath(MediaItemPresentation item)
        {
            if (item == null)
                return null;

            if (!string.IsNullOrEmpty(item.TempFilePath) && File.Exists(item.TempFilePath))
                return item.TempFilePath;

            // FilePath is stored relative to the media root folder and is therefore not
            // necessarily reachable from the client. The content is fetched from the service.
            item.TempFilePath = CreateTempFileFromService(item.FilePath, Guid.NewGuid().ToString());
            return item.TempFilePath;
        }

        [ACMethodInfo("Add", "en{'Add'}de{'Neu'}", 9999, false, false, true)]
        public void Add()
        {
            if (!IsEnabledAdd())
                return;
            MediaItemPresentation item = new MediaItemPresentation();
            item.MediaType = ActiveTab;
            LoadMediaPresentationDefaults(item, ActiveTab == MediaItemTypeEnum.Image);
            MediaItemPresentationList.Add(item);
            MediaItemPresentationList_OnPropertyChanged();
            SelectedMediaItemPresentation = item;
        }

        public bool IsEnabledAdd()
        {
            return IsMediaRootConfigured && CurrentACObject != null;
        }

        /// <summary>
        /// Method DeleteImage
        /// </summary>
        [ACMethodInfo("DeleteImage", "en{'Delete'}de{'Lösche'}", 9999, false, false, true)]
        public async Task Delete()
        {
            if (!IsEnabledDelete())
                return;

            MediaItemPresentation item = SelectedMediaItemPresentation;
            SelectedMediaItemPresentation = null;

            MediaItemPresentationList.Remove(item);
            MediaItemPresentationList = MediaItemPresentationList.ToList();
            SelectedMediaItemPresentation = MediaItemPresentationList.FirstOrDefault();

            if (!string.IsNullOrEmpty(item.FilePath) && item.FilePath != MediaService.GetRelativePath(MediaService.GetEmptyImagePath()))
            {
                BGModel bGModel = new BGModel();
                bGModel.Command = BGWorkerMehtod_DeleteFile;
                bGModel.IsDefault = item.IsDefault;
                bGModel.ACObject = CurrentACObject;

                bGModel.FileNames = new List<string>();
                bGModel.FileNames.Add(item.FilePath);

                if (ThumbExistAndIsNotGeneric(item))
                    bGModel.FileNames.Add(item.ThumbPath);

                BackgroundWorker.RunWorkerAsync(bGModel);
                await ShowDialogAsync(this, DesignNameProgressBar);
            }
        }

        public bool IsEnabledDelete()
        {
            return IsEnabledOpenItem();
        }

        public void DeleteACObject(IACObject aCObject)
        {
            if (aCObject == null || !IsMediaRootConfigured)
                return;
            MediaService.CleanUpFolder(aCObject.ACType.GetACUrl(), aCObject.GetACUrl(), true);
        }

        private bool ThumbExistAndIsNotGeneric(MediaItemPresentation item)
        {
            string fileName = Path.GetFileNameWithoutExtension(item.FilePath);
            return !string.IsNullOrEmpty(item.ThumbPath) && item.ThumbPath.Contains(fileName);
        }

        #endregion

        #region Helper methods
        private MediaItemTypeEnum? GetUpladedFileType(string extension)
        {
            KeyValuePair<MediaItemTypeEnum, MediaSet>? searchItem = null;

            Dictionary<MediaItemTypeEnum, MediaSet> items = new Dictionary<MediaItemTypeEnum, MediaSet>
            {
                { MediaItemTypeEnum.Image, ImageMediaSet },
                { MediaItemTypeEnum.Document, DocumentMediaSet },
                { MediaItemTypeEnum.Audio, AudioMediaSet },
                { MediaItemTypeEnum.Video, VideoMediaSet }
            };

            foreach (var tmp in items)
            {
                if (tmp.Value.MediaTypeSettings.Extensions.Contains(extension))
                    searchItem = tmp;
            }
            if (searchItem != null)
                return searchItem.Value.Key;
            return null;
        }
        private async Task DownloadFile(string filePath)
        {
            string localPath = GetLocalFilePath(SelectedMediaItemPresentation);
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                return;

            string newFilePath = await MediaController.OpenFileDialog(false, localPath, false);
            if (!string.IsNullOrEmpty(newFilePath))
            {
                File.Copy(localPath, newFilePath);
                SelectedMediaItemPresentation_OnPropertyChanged();
            }
        }

        private void LoadMediaPresentationDefaults(MediaItemPresentation item, bool isImage)
        {
            string emptyThumbPath = MediaService.GetRelativePath(MediaService.GetEmptyThumbImagePath());
            item.ThumbPath = emptyThumbPath;
            item.TempThumbPath = CreateTempFileFromService(emptyThumbPath, Guid.NewGuid() + "_temp");
            if (isImage)
            {
                string emptyImagePath = MediaService.GetRelativePath(MediaService.GetEmptyImagePath());
                item.FilePath = emptyImagePath;
                item.TempFilePath = CreateTempFileFromService(emptyImagePath, Guid.NewGuid().ToString());
            }
        }

        #endregion

        #endregion

        #region Messages

        public void SendMessage(Msg msg)
        {
            MsgList.Add(msg);
        }


        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(9999, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Msg> _MsgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(9999, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (_MsgList == null)
                    _MsgList = new ObservableCollection<Msg>();
                return _MsgList;
            }
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(SetFilePath):
                    result = SetFilePath();
                    return true;
                case nameof(IsEnabledSetFilePath):
                    result = IsEnabledSetFilePath();
                    return true;
                case nameof(SetFileThumbPath):
                    result = SetFileThumbPath();
                    return true;
                case nameof(IsEnabledSetFileThumbPath):
                    result = IsEnabledSetFileThumbPath();
                    return true;
                case nameof(UploadFile):
                    UploadFile();
                    return true;
                case nameof(IsEnabledUploadFile):
                    result = IsEnabledUploadFile();
                    return true;
                case nameof(DownloadItem):
                    result = DownloadItem();
                    return true;
                case nameof(IsEnabledDownloadItem):
                    result = IsEnabledDownloadItem();
                    return true;
                case nameof(OpenItem):
                    OpenItem();
                    return true;
                case nameof(IsEnabledOpenItem):
                    result = IsEnabledOpenItem();
                    return true;
                case nameof(ShowInFolder):
                    ShowInFolder();
                    return true;
                case nameof(IsEnabledShowInFolder):
                    result = IsEnabledShowInFolder();
                    return true;
                case nameof(Add):
                    Add();
                    return true;
                case nameof(Delete):
                    result = Delete();
                    return true;
                case nameof(IsEnabledDelete):
                    result = IsEnabledDelete();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        #region BackgroundWorker

        #region BackgroundWorker -> BGMethod

        /// <summary>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            BGModel bGModel = (BGModel)e.Argument;
            switch (bGModel.Command)
            {
                case BGWorkerMehtod_DeleteFile:
                    e.Result = DoDeleteFile(worker, e, bGModel);
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            CloseWindow(this, DesignNameProgressBar);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            Tuple<string, bool, bool> command = (Tuple<string, bool, bool>)worker.EventArgs.Result;

            if (e.Cancelled)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format(@"Operation {0} canceled by user!", command) });
            }
            else if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                switch (command.Item1)
                {
                    case BGWorkerMehtod_DeleteFile:
                        if (OnDefaultImageDelete != null && command.Item2 && command.Item3)
                            OnDefaultImageDelete(this, new EventArgs());
                        break;
                }
            }
        }
        #endregion

        #region BackgroundWorker -> BGWorker mehtods -> Methods for call

        private Tuple<string, bool, bool> DoDeleteFile(ACBackgroundWorker worker, DoWorkEventArgs e, BGModel bGModel)
        {
            bool success = true;
            try
            {
                worker.ProgressInfo.OnlyTotalProgress = true;
                worker.ProgressInfo.AddSubTask(BGWorkerMehtod_DeleteFile, 0, bGModel.FileNames.Count);
                worker.ProgressInfo.TotalProgress.ProgressText = "Start deleting files...";
                int nr = 0;
                foreach (string file in bGModel.FileNames)
                {
                    nr++;
                    worker.ProgressInfo.ReportProgress(BGWorkerMehtod_DeleteFile, nr, string.Format(@"Deleting {0} / {1} ...", nr, bGModel.FileNames.Count));
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return new Tuple<string, bool, bool>(BGWorkerMehtod_DeleteFile, false, bGModel.IsDefault);
                    }

                    bool isDeleted = MediaService.DeleteFile(file);
                    success = success && isDeleted;
                }

                MediaService.CleanUpFolder(bGModel.ACObject.ACType.GetACUrl(), bGModel.ACObject.GetACUrl(), false);
            }
            catch (Exception ex)
            {
                Messages.ExceptionAsync(this, ex.Message);
            }

            return new Tuple<string, bool, bool>(BGWorkerMehtod_DeleteFile, success, bGModel.IsDefault);
        }

        #endregion

        #endregion

        #region BG model

        class BGModel
        {
            public string Command { get; set; }
            public List<string> FileNames { get; set; }

            public IACObject ACObject { get; set; }
            public bool IsDefault { get; set; }
        }

        #endregion
    }



}

