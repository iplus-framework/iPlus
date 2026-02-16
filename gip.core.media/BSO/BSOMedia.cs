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
            _MediaRootFolder = new ACPropertyConfigValue<string>(this, @"MediaRootFolder", @"C:\VarioData\Media");
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

            MediaController = ACMediaController.GetServiceInstance(this);

            return true;
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

        private IACObject CurrentACObject;

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
            if (CurrentACObject == aCObject || !Directory.Exists(MediaController.MediaSettings.MediaRootFolder))
                return;

            CurrentACObject = aCObject;

            Dictionary<MediaItemTypeEnum, MediaSet> mediaSets = MediaController.GetMediaSets(aCObject);

            ImageMediaSet = mediaSets[MediaItemTypeEnum.Image];
            DocumentMediaSet = mediaSets[MediaItemTypeEnum.Document];
            AudioMediaSet = mediaSets[MediaItemTypeEnum.Audio];
            VideoMediaSet = mediaSets[MediaItemTypeEnum.Video];

            _ImageList = MediaController.GetFiles(ImageMediaSet, 1);
            if (_ImageList != null && _ImageList.Any())
                SelectedImage = _ImageList.FirstOrDefault();

            _DocumentList = MediaController.GetFiles(DocumentMediaSet, 1);
            if (_DocumentList != null && _DocumentList.Any())
                SelectedDocument = _DocumentList.FirstOrDefault();

            _AudioList = MediaController.GetFiles(AudioMediaSet, 1);
            if (_AudioList != null && _AudioList.Any())
                SelectedAudio = _AudioList.FirstOrDefault();

            _VideoList = MediaController.GetFiles(VideoMediaSet, 1);
            if (_VideoList != null && _VideoList.Any())
                SelectedVideo = _VideoList.FirstOrDefault();

            OnPropertyChanged(nameof(ImageList));
            OnPropertyChanged(nameof(DocumentList));
            OnPropertyChanged(nameof(AudioList));
            OnPropertyChanged(nameof(VideoList));
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

        #region Methods -> ACMethods

        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("SetFilePath", "en{'...'}de{'...'}", 9999, false, false, true)]
        public void SetFilePath()
        {
            if (!IsEnabledSetFilePath())
                return;
            string filePath = MediaController.OpenFileDialog(false, SelectedMediaItemPresentation.EditFilePath, true);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                SelectedMediaItemPresentation.EditFilePath = filePath;
                SelectedMediaItemPresentation_OnPropertyChanged();
            }
        }

        public bool IsEnabledSetFilePath()
        {
            return SelectedMediaItemPresentation != null;
        }


        /// <summary>
        /// Exports the folder.
        /// </summary>
        [ACMethodInfo("SetFileThumbPath", "en{'...'}de{'...'}", 9999, false, false, true)]
        public void SetFileThumbPath()
        {
            if (!IsEnabledSetFileThumbPath())
                return;
            string filePath = MediaController.OpenFileDialog(false, SelectedMediaItemPresentation.EditFilePath, true);
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
                string extension = Path.GetExtension(SelectedMediaItemPresentation.EditFilePath);
                if (mediaSet.ExtensionQuery.Contains(extension))
                {
                    SelectedMediaItemPresentation = MediaController.Upload(mediaSet, SelectedMediaItemPresentation);
                    SelectedMediaItemPresentation_OnPropertyChanged();
                }
                else
                {
                    Messages.WarningAsync(this, "Warning50068", false, extension, mediaSet.ExtensionQuery);
                }

            }
            catch (Exception ec)
            {
                Msg msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = ec.Message };
                SendMessage(msg);
            }

        }
        public bool IsEnabledUploadFile()
        {
            return SelectedMediaItemPresentation != null
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
        public void DownloadItem()
        {
            if (!IsEnabledDownloadItem())
                return;
            DownloadFile(SelectedMediaItemPresentation.FilePath);
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

            var filePath = SelectedMediaItemPresentation.FilePath;

            if (File.Exists(filePath))
            {
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
            else
            {
                SendMessage(new Msg { MessageLevel = eMsgLevel.Warning, Message = "File does not exist." });
            }
        }

        public bool IsEnabledOpenItem()
        {
            return
               SelectedMediaItemPresentation != null
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

            // System.Diagnostics.Process.Start(Path.GetDirectoryName(SelectedMediaItemPresentation.FilePath));
            var folderPath = Path.GetDirectoryName(SelectedMediaItemPresentation.FilePath);

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
            return CurrentACObject != null;
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

            if (!string.IsNullOrEmpty(item.FilePath) && item.FilePath != MediaController.GetEmptyImagePath())
            {
                BGModel bGModel = new BGModel();
                bGModel.Command = BGWorkerMehtod_DeleteFile;
                bGModel.IsDefault = item.IsDefault;
                bGModel.ACObject = CurrentACObject;

                bGModel.FileNames = new List<string>();
                bGModel.FileNames.Add(item.FilePath);

                if (MediaController.ThumbExistAndIsNotGeneric(item))
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
            MediaController.CleanUpFolder(aCObject, true);
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
        private void DownloadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string newFilePath = MediaController.OpenFileDialog(false, filePath, false);
                if (!string.IsNullOrEmpty(newFilePath))
                {
                    File.Copy(filePath, newFilePath);
                    SelectedMediaItemPresentation_OnPropertyChanged();
                }
            }
        }

        private void LoadMediaPresentationDefaults(MediaItemPresentation item, bool isImage)
        {
            item.ThumbPath = MediaController.GetEmptyThumbImagePath();
            item.TempThumbPath = MediaController.GetEmptyThumbImagePath();
            if (isImage)
            {
                item.FilePath = MediaController.GetEmptyImagePath();
                item.TempFilePath = MediaController.GetEmptyImagePath();
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
                    SetFilePath();
                    return true;
                case nameof(IsEnabledSetFilePath):
                    result = IsEnabledSetFilePath();
                    return true;
                case nameof(SetFileThumbPath):
                    SetFileThumbPath();
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
                    DownloadItem();
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
                    _= Delete();
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
                            Messages.LogException(this.GetACUrl(), nameof(DoDeleteFile), ec);
                        }
                    }

                    success = success && isDeleted;
                }

                MediaController.CleanUpFolder(bGModel.ACObject);
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

