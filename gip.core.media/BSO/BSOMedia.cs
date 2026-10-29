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
        /// Shared media logic. It talks to the ACMediaService (proxy or real instance, always via ExecuteMethod)
        /// and falls back to direct file access when the service is not reachable but the media root folder is.
        /// BSOMedia never calls the service directly.
        /// </summary>
        public ACMediaController MediaController { get; private set; }

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
                return MediaController != null && MediaController.IsMediaRootConfigured;
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
                    if (_ActiveTab == MediaItemTypeEnum.Image)
                        _SelectedMediaItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                    OnPropertyChanged($"{nameof(SelectedImage)}\\{nameof(SelectedImage.TempFilePath)}");
                }
            }
        }

        /// <summary>
        /// Items of this media type. The items of all types are held in <see cref="MediaItemPresentationList"/>.
        /// </summary>
        [ACPropertyList(9999, "Image")]
        public List<MediaItemPresentation> ImageList
        {
            get { return GetItemsOfType(MediaItemTypeEnum.Image); }
            set { SetItemsOfType(MediaItemTypeEnum.Image, value); }
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
                    if (_ActiveTab == MediaItemTypeEnum.Document)
                        _SelectedMediaItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                    OnPropertyChanged($"{nameof(SelectedDocument)}\\{nameof(SelectedDocument.TempFilePath)}");
                }
            }
        }

        /// <summary>
        /// Items of this media type. The items of all types are held in <see cref="MediaItemPresentationList"/>.
        /// </summary>
        [ACPropertyList(9999, "Document")]
        public List<MediaItemPresentation> DocumentList
        {
            get { return GetItemsOfType(MediaItemTypeEnum.Document); }
            set { SetItemsOfType(MediaItemTypeEnum.Document, value); }
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
                    if (_ActiveTab == MediaItemTypeEnum.Audio)
                        _SelectedMediaItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                }
            }
        }


        /// <summary>
        /// Items of this media type. The items of all types are held in <see cref="MediaItemPresentationList"/>.
        /// </summary>
        [ACPropertyList(9999, "Audio")]
        public List<MediaItemPresentation> AudioList
        {
            get { return GetItemsOfType(MediaItemTypeEnum.Audio); }
            set { SetItemsOfType(MediaItemTypeEnum.Audio, value); }
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
                    if (_ActiveTab == MediaItemTypeEnum.Video)
                        _SelectedMediaItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                }
            }
        }


        /// <summary>
        /// Items of this media type. The items of all types are held in <see cref="MediaItemPresentationList"/>.
        /// </summary>
        [ACPropertyList(9999, "Video")]
        public List<MediaItemPresentation> VideoList
        {
            get { return GetItemsOfType(MediaItemTypeEnum.Video); }
            set { SetItemsOfType(MediaItemTypeEnum.Video, value); }
        }

        #endregion

        #region Properties -> SelectedTab


        private MediaItemTypeEnum _ActiveTab = MediaItemTypeEnum.Image;
        /// <summary>
        /// Media type of the selected item and type for new items. The views show the items of all media types
        /// in one list and use this property to switch the details (image or document preview).
        /// Designs with one tab per media type (WPF) set it via the tab items.
        /// </summary>
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
                    _SelectedMediaItem = GetSelectionOfType(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedMediaItemPresentation));
                }
            }
        }

        private MediaItemPresentation _SelectedMediaItem;
        [ACPropertyInfo(81, "SelectedMediaItemPresentation", "en{'SelectedMediaItemPresentation'}de{'SelectedMediaItemPresentation'}")]
        public MediaItemPresentation SelectedMediaItemPresentation
        {
            get
            {
                return _SelectedMediaItem;
            }
            set
            {
                _SelectedMediaItem = value;
                if (value != null)
                {
                    SetActiveTabFromItem(value);
                    switch (value.MediaType)
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
                }
                else
                {
                    switch (ActiveTab)
                    {
                        case MediaItemTypeEnum.Image:
                            SelectedImage = null;
                            break;
                        case MediaItemTypeEnum.Document:
                            SelectedDocument = null;
                            break;
                        case MediaItemTypeEnum.Audio:
                            SelectedAudio = null;
                            break;
                        case MediaItemTypeEnum.Video:
                            SelectedVideo = null;
                            break;
                    }
                }
                OnPropertyChanged();
            }
        }

        private MediaItemPresentation GetSelectionOfType(MediaItemTypeEnum mediaType)
        {
            switch (mediaType)
            {
                case MediaItemTypeEnum.Image:
                    return SelectedImage;
                case MediaItemTypeEnum.Document:
                    return SelectedDocument;
                case MediaItemTypeEnum.Audio:
                    return SelectedAudio;
                case MediaItemTypeEnum.Video:
                    return SelectedVideo;
            }
            return null;
        }

        private void SetActiveTabFromItem(MediaItemPresentation item)
        {
            if (_ActiveTab != item.MediaType)
            {
                _ActiveTab = item.MediaType;
                OnPropertyChanged(nameof(ActiveTab));
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
            OnPropertyChanged(nameof(SelectedMediaItemPresentation));
        }

        private List<MediaItemPresentation> _MediaItemList;
        /// <summary>
        /// The items of all media types. A new list instance must be assigned for changes,
        /// because the Avalonia DataGrid does not re-read the same List instance.
        /// </summary>
        [ACPropertyInfo(82, "MediaItemPresentationList", "en{'MediaItemPresentationList'}de{'MediaItemPresentationList'}")]
        public List<MediaItemPresentation> MediaItemPresentationList
        {
            get
            {
                if (_MediaItemList == null)
                    _MediaItemList = new List<MediaItemPresentation>();
                return _MediaItemList;
            }
            set
            {
                _MediaItemList = value;
                MediaItemPresentationList_OnPropertyChanged();
            }
        }

        public void MediaItemPresentationList_OnPropertyChanged()
        {
            OnPropertyChanged(nameof(ImageList));
            OnPropertyChanged(nameof(DocumentList));
            OnPropertyChanged(nameof(AudioList));
            OnPropertyChanged(nameof(VideoList));
            OnPropertyChanged(nameof(MediaItemPresentationList));
        }

        private List<MediaItemPresentation> GetItemsOfType(MediaItemTypeEnum mediaType)
        {
            return MediaItemPresentationList.Where(c => c.MediaType == mediaType).ToList();
        }

        private void SetItemsOfType(MediaItemTypeEnum mediaType, List<MediaItemPresentation> items)
        {
            List<MediaItemPresentation> newList = MediaItemPresentationList.Where(c => c.MediaType != mediaType).ToList();
            if (items != null)
                newList.AddRange(items);
            MediaItemPresentationList = newList;
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
            nameof(ActiveTab), nameof(SelectedMediaItemPresentation), nameof(SelectedImage), nameof(SelectedDocument), nameof(SelectedAudio), nameof(SelectedVideo)
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
                case nameof(GetItemContent):
                case nameof(IsEnabledGetItemContent):
                    return MediaItemSelectionProps;
                case nameof(Add):
                case nameof(IsEnabledAdd):
                case nameof(UploadFileContent):
                case nameof(IsEnabledUploadFileContent):
                    return new string[] { nameof(CurrentACObject) };
            }
            return base.GetPropsToObserveForIsEnabled(acMethodName);
        }

        public void Clean()
        {
            CurrentACObject = null;
            _MediaItemList = null;
            _SelectedMediaItem = null;

            SelectedImage = null;
            SelectedDocument = null;
            SelectedAudio = null;
            SelectedVideo = null;
            NotifyMediaChanged();
        }

        /// <param name="aCObject">Object whose media should be shown</param>
        /// <param name="forceReload">Reads the items again from the media service although the object didn't change (media may have been changed elsewhere).</param>
        public void LoadMedia(IACObject aCObject, bool forceReload = false)
        {
            if ((CurrentACObject == aCObject && !forceReload) || !IsMediaRootConfigured)
                return;

            CurrentACObject = aCObject;

            ImageMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Image);
            DocumentMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Document);
            AudioMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Audio);
            VideoMediaSet = GetMediaSet(aCObject, MediaItemTypeEnum.Video);

            List<MediaItemPresentation> images = LoadItems(ImageMediaSet) ?? new List<MediaItemPresentation>();
            List<MediaItemPresentation> documents = LoadItems(DocumentMediaSet) ?? new List<MediaItemPresentation>();
            List<MediaItemPresentation> audios = LoadItems(AudioMediaSet) ?? new List<MediaItemPresentation>();
            List<MediaItemPresentation> videos = LoadItems(VideoMediaSet) ?? new List<MediaItemPresentation>();

            // The selection must be reset even if the new list is empty, otherwise the details keep showing the previous object
            _MediaItemList = images.Concat(documents).Concat(audios).Concat(videos).ToList();
            SelectedImage = images.FirstOrDefault();
            SelectedDocument = documents.FirstOrDefault();
            SelectedAudio = audios.FirstOrDefault();
            SelectedVideo = videos.FirstOrDefault();
            SelectedMediaItemPresentation = _MediaItemList.FirstOrDefault();

            NotifyMediaChanged();
        }

        /// <summary>
        /// The views are bound to the generic properties (MediaItemPresentationList / SelectedMediaItemPresentation),
        /// so changes of the type specific lists and selections must be signaled for them as well.
        /// </summary>
        private void NotifyMediaChanged()
        {
            MediaItemPresentationList_OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedImage));
            OnPropertyChanged(nameof(SelectedDocument));
            OnPropertyChanged(nameof(SelectedAudio));
            OnPropertyChanged(nameof(SelectedVideo));
            OnPropertyChanged(nameof(ActiveTab));
            OnPropertyChanged(nameof(SelectedMediaItemPresentation));
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

            MediaItemListResult result = MediaController.GetMediaFiles(
                CurrentACObject,
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
        /// Creates the client-side presentation item. Images and thumbnails are kept as byte arrays
        /// (the client may be Browser/Android without access to the media folder or a file system).
        /// Only documents are materialized into a local temp file, because the document viewer needs a path.
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

            MediaContentResult content = MediaController.GetFileContent(info.FilePath);
            ApplyContent(item, content);

            MediaContentResult thumb = null;
            if (!string.IsNullOrEmpty(info.ThumbPath))
                thumb = MediaController.GetFileContent(info.ThumbPath);
            else if (mediaType != MediaItemTypeEnum.Image)
                thumb = MediaController.GetIconContent(Path.GetExtension(info.FilePath));
            ApplyThumbContent(item, thumb);

            return item;
        }

        private static bool HasContent(MediaContentResult content)
        {
            return content != null && content.Success && content.Content != null;
        }

        /// <summary>
        /// Takes over the main content: images as byte array, other types as local temp file.
        /// </summary>
        private void ApplyContent(MediaItemPresentation item, MediaContentResult content)
        {
            if (!HasContent(content))
                return;

            if (item.MediaType == MediaItemTypeEnum.Image)
            {
                item.ImageContent = content.Content;
                if (item.ThumbContent == null)
                    item.ThumbContent = content.Content;
            }
            else
                item.TempFilePath = TryWriteTempFile(content.Content, content.Extension, Guid.NewGuid().ToString());
        }

        private void ApplyThumbContent(MediaItemPresentation item, MediaContentResult thumb)
        {
            if (HasContent(thumb))
                item.ThumbContent = thumb.Content;
        }

        private string CreateTempFileFromService(string filePath, string tempFileName)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            MediaContentResult content = MediaController.GetFileContent(filePath);
            if (!HasContent(content))
                return null;

            return TryWriteTempFile(content.Content, content.Extension, tempFileName);
        }

        /// <summary>
        /// Writes a local temp file. Returns null if the client has no usable file system.
        /// </summary>
        private string TryWriteTempFile(byte[] content, string extension, string tempFileName)
        {
            try
            {
                return WriteTempFile(content, extension, tempFileName);
            }
            catch (Exception ec)
            {
                Messages.LogException(this.GetACUrl(), nameof(TryWriteTempFile), ec);
                return null;
            }
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
                MediaItemTypeEnum? fileType = GetUpladedFileType(Path.GetExtension(filePath));
                if (fileType.HasValue)
                    ChangeMediaType(SelectedMediaItemPresentation, fileType.Value);
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
                MediaItemPresentation item = SelectedMediaItemPresentation;
                string extension = Path.GetExtension(item.EditFilePath);
                MediaItemTypeEnum? fileType = GetUpladedFileType(extension);
                if (!fileType.HasValue)
                {
                    Messages.WarningAsync(this, "Warning50068", false, extension, GetAllExtensionQuery());
                    return;
                }
                ChangeMediaType(item, fileType.Value);

                byte[] content = File.ReadAllBytes(item.EditFilePath);
                byte[] thumbContent = null;
                if (!string.IsNullOrEmpty(item.EditThumbPath) && File.Exists(item.EditThumbPath))
                    thumbContent = File.ReadAllBytes(item.EditThumbPath);

                StoreUploadedContent(item, fileType.Value, Path.GetFileName(item.EditFilePath), content, thumbContent);
            }
            catch (Exception ec)
            {
                Msg msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = ec.Message };
                SendMessage(msg);
            }

        }

        /// <summary>
        /// Sends the content to the media service and takes over the result into the presentation item.
        /// </summary>
        private bool StoreUploadedContent(MediaItemPresentation item, MediaItemTypeEnum mediaType, string fileName, byte[] content, byte[] thumbContent)
        {
            MediaSet mediaSet = GetMediaSet(mediaType);
            MediaContentResult result = MediaController.UploadMediaFile(
                CurrentACObject,
                mediaSet.MediaType,
                fileName,
                content,
                item.IsDefault,
                item.IsGenerateThumb,
                thumbContent);

            if (result == null || !result.Success)
            {
                SendMessage(result?.Message ?? new Msg { MessageLevel = eMsgLevel.Error, Message = "Upload failed." });
                return false;
            }

            item.FilePath = result.FilePath ?? Path.Combine(mediaSet.ItemRootFolder, result.FileName);
            item.Name = result.FileName;
            item.IsNew = false;
            item.IsGenerateThumb = false;
            item.EditFilePath = null;
            item.EditThumbPath = null;

            item.ImageContent = null;
            item.ThumbContent = null;
            ApplyContent(item, result);

            MediaContentResult thumbResult = MediaController.GetThumbContent(item.FilePath, mediaSet.MediaType);
            if (thumbResult != null && thumbResult.Success && thumbResult.Content != null)
            {
                item.ThumbPath = string.IsNullOrEmpty(thumbResult.FilePath) ? item.FilePath : thumbResult.FilePath;
                ApplyThumbContent(item, thumbResult);
            }

            SelectedMediaItemPresentation_OnPropertyChanged();
            return true;
        }

        /// <summary>
        /// Uploads a file whose content was picked on the client device (mobile app, browser).
        /// The client has no access to the media folder and the server has no access to the client file system,
        /// so the file name and the content are passed as parameters. A new item is created and selected.
        /// </summary>
        [ACMethodInfo("UploadFileContent", "en{'Upload from device'}de{'Vom Gerät hochladen'}", 9999, false, false, true)]
        public void UploadFileContent(string fileName, byte[] content)
        {
            if (string.IsNullOrEmpty(fileName) || content == null || content.Length == 0)
            {
                Messages.LogWarning(this.GetACUrl(), nameof(UploadFileContent), "No file name or content received.");
                return;
            }
            if (!IsEnabledUploadFileContent())
            {
                Messages.LogWarning(this.GetACUrl(), nameof(UploadFileContent),
                    String.Format("Upload of {0} rejected: MediaRootConfigured={1}, CurrentACObject={2}", fileName, IsMediaRootConfigured, CurrentACObject));
                return;
            }
            try
            {
                fileName = Path.GetFileName(fileName);
                string extension = Path.GetExtension(fileName);
                MediaItemTypeEnum? fileType = GetUpladedFileType(extension);
                if (!fileType.HasValue)
                {
                    Messages.WarningAsync(this, "Warning50068", false, extension, GetAllExtensionQuery());
                    return;
                }

                MediaItemPresentation item = new MediaItemPresentation();
                item.MediaType = fileType.Value;
                item.IsNew = true;
                item.IsGenerateThumb = fileType.Value == MediaItemTypeEnum.Image;
                if (!StoreUploadedContent(item, fileType.Value, fileName, content, null))
                    return;

                // A new list instance is required (see Add())
                MediaItemPresentationList = new List<MediaItemPresentation>(MediaItemPresentationList) { item };
                SelectedMediaItemPresentation = item;
            }
            catch (Exception ec)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = ec.Message });
            }
        }

        public bool IsEnabledUploadFileContent()
        {
            return IsEnabledAdd();
        }

        /// <summary>
        /// Delivers the content of the selected item to the client, which opens it with the viewer of the device
        /// (VBButton with OpenFileOnClick). A server-side BSO cannot start a viewer on the client.
        /// </summary>
        [ACMethodInfo("GetItemContent", "en{'Open on device'}de{'Auf Gerät öffnen'}", 9999, false, false, true)]
        public MediaContentResult GetItemContent()
        {
            if (!IsEnabledGetItemContent())
                return null;
            return MediaController.GetFileContent(SelectedMediaItemPresentation.FilePath);
        }

        public bool IsEnabledGetItemContent()
        {
            return IsEnabledOpenItem();
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
            item.IsNew = true;
            LoadMediaPresentationDefaults(item, ActiveTab == MediaItemTypeEnum.Image);
            // A new list instance is required: raising PropertyChanged for the same List instance
            // does not make the Avalonia DataGrid re-read the items (same pattern as in Delete()).
            List<MediaItemPresentation> newList = new List<MediaItemPresentation>(MediaItemPresentationList) { item };
            MediaItemPresentationList = newList;
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

            MediaItemPresentationList = MediaItemPresentationList.Where(c => c != item).ToList();
            SelectedMediaItemPresentation = MediaItemPresentationList.FirstOrDefault(c => c.MediaType == item.MediaType)
                                            ?? MediaItemPresentationList.FirstOrDefault();
            if (SelectedMediaItemPresentation == null)
                NotifyMediaChanged();

            if (!string.IsNullOrEmpty(item.FilePath) && item.FilePath != ACMediaController.RelativeEmptyImagePath)
            {
                if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsBrowser())
                {
                    // Mobile: no progress dialog, the files are deleted on the media service without a background worker
                    List<string> mobileFiles = new List<string> { item.FilePath };
                    if (ThumbExistAndIsNotGeneric(item))
                        mobileFiles.Add(item.ThumbPath);
                    IACObject acObject = CurrentACObject;
                    bool isDefault = item.IsDefault;
                    bool deleted = await Task.Run(() =>
                    {
                        bool success = true;
                        foreach (string file in mobileFiles)
                            success = MediaController.DeleteMediaFile(file) && success;
                        MediaController.CleanUpMediaFolder(acObject, false);
                        return success;
                    });
                    if (deleted && isDefault && OnDefaultImageDelete != null)
                        OnDefaultImageDelete(this, new EventArgs());
                    else if (!deleted)
                        Messages.LogWarning(this.GetACUrl(), nameof(Delete), $"Not all files of {item.FilePath} could be deleted");
                    return;
                }

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
            MediaController.CleanUpMediaFolder(aCObject, true);
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
            if (string.IsNullOrEmpty(extension))
                return null;

            foreach (MediaSet mediaSet in new MediaSet[] { ImageMediaSet, DocumentMediaSet, AudioMediaSet, VideoMediaSet })
            {
                if (mediaSet?.MediaTypeSettings?.Extensions != null
                    && mediaSet.MediaTypeSettings.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    return mediaSet.MediaType;
            }
            return null;
        }

        private string GetAllExtensionQuery()
        {
            return string.Join(";", new MediaSet[] { ImageMediaSet, DocumentMediaSet, AudioMediaSet, VideoMediaSet }
                .Where(c => c != null)
                .Select(c => c.ExtensionQuery));
        }

        /// <summary>
        /// The media type is derived from the file. All types share one list, so only the item and the selections need to follow.
        /// </summary>
        private void ChangeMediaType(MediaItemPresentation item, MediaItemTypeEnum mediaType)
        {
            if (item == null || item.MediaType == mediaType)
                return;

            MediaItemTypeEnum oldType = item.MediaType;
            if (GetSelectionOfType(oldType) == item)
                SetSelectionOfType(oldType, null);

            item.MediaType = mediaType;
            if (item.IsNew)
            {
                item.ImageContent = null;
                item.TempFilePath = null;
                item.FilePath = null;
                LoadMediaPresentationDefaults(item, mediaType == MediaItemTypeEnum.Image);
            }

            SetSelectionOfType(mediaType, item);
            _SelectedMediaItem = item;
            _ActiveTab = mediaType;
            MediaItemPresentationList = MediaItemPresentationList.ToList();
            OnPropertyChanged(nameof(ActiveTab));
            OnPropertyChanged(nameof(SelectedMediaItemPresentation));
        }

        private void SetSelectionOfType(MediaItemTypeEnum mediaType, MediaItemPresentation item)
        {
            switch (mediaType)
            {
                case MediaItemTypeEnum.Image:
                    _SelectedImage = item;
                    break;
                case MediaItemTypeEnum.Document:
                    _SelectedDocument = item;
                    break;
                case MediaItemTypeEnum.Audio:
                    _SelectedAudio = item;
                    break;
                case MediaItemTypeEnum.Video:
                    _SelectedVideo = item;
                    break;
            }
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
            string emptyThumbPath = ACMediaController.RelativeEmptyThumbImagePath;
            item.ThumbPath = emptyThumbPath;
            ApplyThumbContent(item, MediaController.GetFileContent(emptyThumbPath));
            if (isImage)
            {
                string emptyImagePath = ACMediaController.RelativeEmptyImagePath;
                item.FilePath = emptyImagePath;
                item.ImageContent = null;
                MediaContentResult emptyImage = MediaController.GetFileContent(emptyImagePath);
                if (HasContent(emptyImage))
                    item.ImageContent = emptyImage.Content;
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
                case nameof(GetItemContent):
                    result = GetItemContent();
                    return true;
                case nameof(IsEnabledGetItemContent):
                    result = IsEnabledGetItemContent();
                    return true;
                case nameof(UploadFileContent):
                    UploadFileContent(acParameter[0] as string, acParameter[1] as byte[]);
                    return true;
                case nameof(IsEnabledUploadFileContent):
                    result = IsEnabledUploadFileContent();
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

                    bool isDeleted = MediaController.DeleteMediaFile(file);
                    success = success && isDeleted;
                }

                MediaController.CleanUpMediaFolder(bGModel.ACObject, false);
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

