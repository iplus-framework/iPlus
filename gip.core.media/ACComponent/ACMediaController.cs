// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Mime;
using SkiaSharp;
using System.Threading.Tasks;

namespace gip.core.media
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMediaController'}de{'ACMediaController'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACMediaController : ACComponent, IACMediaController
    {

        #region const
        public const string Const_DefImage = @"empty.jpg";
        public const string Const_DefThumbImage = @"empty_thumb.jpg";

        public static string MediaControllerPath = $"\\{nameof(ACRoot.LocalServiceObjects)}\\MediaController";
        #endregion

        #region DI

        public string RootFolder { get; private set; }
        public MediaSettings MediaSettings { get; set; }

        public enum Quality
        {
            Medium,
            Low,
            High,
            None
        }

        #endregion

        #region c´tors

        public ACMediaController(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool initialized = base.ACInit(startChildMode);
            if (initialized)
                _VBMediaController = Root?.WPFServices?.VBMediaControllerService?.GetMediaControllerProxy(this);
            return initialized;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (_VBMediaController != null)
            {
                Root?.WPFServices?.VBMediaControllerService?.RemoveMediaControllerProxy(this);
                _VBMediaController = null;
            }
            bool result = await base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        IVBMediaControllerProxy _VBMediaController;
        protected IVBMediaControllerProxy VBMediaController
        {
            get
            {
                if (_VBMediaController == null)
                    _VBMediaController = Root?.WPFServices?.VBMediaControllerService.GetMediaControllerProxy(this);
                return _VBMediaController;
            }
        }

        #region License Image

        public SKBitmap CreateLicenseImage(VBLicense CurrentVBLicense)
        {
            return VBMediaController.CreateLicenseImage(CurrentVBLicense);
        }

        #endregion

        #region Static methods

        public static ACMediaController GetServiceInstance(ACComponent acComponent, string mediaRootFolder = null)
        {
            ACMediaController mediaController = acComponent.ACUrlCommand(MediaControllerPath) as ACMediaController;
            if (mediaController == null)
            {
                throw new Exception($"{MediaControllerPath} not found!");
            }

            mediaController.MediaSettings = new MediaSettings(mediaRootFolder);
            return mediaController;
        }

        #endregion

        #region Methods

        #region Methods -> MediaSet

        public Dictionary<MediaItemTypeEnum, MediaSet> GetMediaSets(IACObject aCObject)
        {
            return new Dictionary<MediaItemTypeEnum, MediaSet>
            {
                { MediaItemTypeEnum.Image, GetMediaSet(aCObject, MediaItemTypeEnum.Image, "", "", true) },
                { MediaItemTypeEnum.Document, GetMediaSet(aCObject, MediaItemTypeEnum.Document, "", "", true) },
                { MediaItemTypeEnum.Audio, GetMediaSet(aCObject, MediaItemTypeEnum.Audio, "", "", true) },
                { MediaItemTypeEnum.Video, GetMediaSet(aCObject, MediaItemTypeEnum.Video, "", "", true) }
            };
        }

        public MediaSet GetMediaSet(IACObject aCObject, MediaItemTypeEnum mediaType, string filterName, string order, bool ascending)
        {
            MediaSet mediaSet = new MediaSet();
            mediaSet.PageSize = MediaSettings.PageSize;
            mediaSet.MediaType = mediaType;
            mediaSet.MediaTypeSettings = MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            string typeRootFolder = GetTypeRootPath(MediaSettings.MediaRootFolder, aCObject);
            string itemRootFolder = GetACObjectRootPath(typeRootFolder, aCObject);
            mediaSet.ItemRootFolder = Path.Combine(itemRootFolder, mediaSet.MediaTypeSettings.FolderName);

            mediaSet.ExtensionQuery = JoinStrings(mediaSet.MediaTypeSettings.Extensions, filterName);
            mediaSet.Order = order;
            mediaSet.IsAscending = ascending;
            if (Directory.Exists(mediaSet.ItemRootFolder))
            {
                mediaSet.ItemsCount = QueryFilesWithExtensionWithoutDefault(mediaSet).Count();
            }

            return mediaSet;
        }

        public List<MediaItemPresentation> GetFiles(MediaSet mediaSet, int pageIndex)
        {
            if (Directory.Exists(mediaSet.ItemRootFolder))
            {
                var query = QueryFilesWithExtensionWithoutDefault(mediaSet);
                if (!string.IsNullOrEmpty(mediaSet.Order))
                {
                    query = query.OrderBy(mediaSet.Order, mediaSet.IsAscending);
                }
                else
                {
                    query = query.OrderBy(c => c.Name);
                }
                query = query.OrderBy(c => c.Name);
                var files = query.Skip((pageIndex - 1) * mediaSet.PageSize).Take(mediaSet.PageSize).Select(c => Path.Combine(mediaSet.ItemRootFolder, c.Name)).ToList();
                List<MediaItemPresentation> result = new List<MediaItemPresentation>();
                foreach (var file in files)
                {
                    Guid id = Guid.NewGuid();
                    MediaItemPresentation presentationItem = new MediaItemPresentation();
                    presentationItem.MediaType = mediaSet.MediaType;
                    presentationItem.FilePath = file;
                    presentationItem.Name = Path.GetFileName(file);
                    presentationItem.IsDefault = presentationItem.Name.StartsWith(MediaSettings.DefaultImageName + ".");

                    // Create temp file
                    CreateTempFile(presentationItem, id);

                    string thumbNamePattern = Path.GetFileNameWithoutExtension(file) + MediaSettings.DefaultThumbSuffix;
                    FileInfo thumbFileInfo = new DirectoryInfo(Path.GetDirectoryName(file)).GetFiles().Where(c => c.Name.Contains(thumbNamePattern)).FirstOrDefault();
                    // have thumb
                    if (thumbFileInfo != null)
                    {
                        presentationItem.ThumbPath = thumbFileInfo.FullName;
                        CreateTempThumbFile(presentationItem, id);
                    }
                    if (string.IsNullOrEmpty(presentationItem.ThumbPath))
                    {
                        // not have thumb but is image - use self
                        if (mediaSet.MediaTypeSettings.MediaType == MediaItemTypeEnum.Image)
                        {
                            presentationItem.ThumbPath = file;
                            presentationItem.TempThumbPath = presentationItem.TempFilePath;
                        }
                        else
                        {
                            // not image, not have thumb - use file extension default icon
                            presentationItem.ThumbPath = GetIconFilePath(Path.GetExtension(file));
                            presentationItem.TempThumbPath = presentationItem.ThumbPath;
                        }
                    }

                    presentationItem.IsNew = false;

                    result.Add(presentationItem);
                }
                return result;
            }
            return null;
        }

        public virtual IEnumerable<FileInfo> QueryFilesWithExtensionWithoutDefault(MediaSet mediaSet)
        {
            DirectoryInfo di = new DirectoryInfo(mediaSet.ItemRootFolder);
            return
                di
                .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(c =>
                        mediaSet.ExtensionQuery.Contains(c.Extension)
                        && !Path.GetFileNameWithoutExtension(c.Name).EndsWith(MediaSettings.DefaultThumbSuffix)
                );
        }

        private string JoinStrings(List<string> extensions, string filterName)
        {
            if (!string.IsNullOrEmpty(filterName))
                filterName = "*" + filterName + "*";
            else
                filterName = "*";
            return string.Join(";", extensions.Select(c => filterName + c));
        }

        #endregion

        #region Methods -> Upload
        public MediaItemPresentation Upload(MediaSet mediaSet, MediaItemPresentation item)
        {
            if (string.IsNullOrEmpty(item.EditFilePath) || !File.Exists(item.EditFilePath))
                return item;

            if (mediaSet.MediaTypeSettings.MediaType == MediaItemTypeEnum.Image)
                UploadImage(mediaSet, item);
            else
                UploadDocument(mediaSet, item);

            return item;
        }
        public MediaItemPresentation UploadImage(MediaSet mediaSet, MediaItemPresentation item)
        {
            Guid id = Guid.NewGuid();
            if (item.IsDefault)
            {
                item.FilePath = GetDefaultFilePath(mediaSet);
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

            string thumbFileName = MediaSettings.FullDefaultThumbImageName;
            if (!item.IsDefault)
            {
                thumbFileName = GetDefaultThumbFilePath(item.EditFilePath, item.FilePath);
            }

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
            item.IsNew = false;

            CreateTempFile(item, id);

            if (!string.IsNullOrEmpty(item.ThumbPath) && File.Exists(item.ThumbPath))
            {
                if (item.ThumbPath == GetEmptyThumbImagePath())
                {
                    item.TempThumbPath = item.TempFilePath;
                }
                else
                {
                    CreateTempThumbFile(item, id);
                }
            }

            return item;
        }
        public MediaItemPresentation UploadDocument(MediaSet mediaSet, MediaItemPresentation item)
        {
            string extension = Path.GetExtension(item.EditFilePath);
            Guid id = Guid.NewGuid();
            if (item.IsNew)
                item.FilePath = Path.Combine(mediaSet.ItemRootFolder, Path.GetFileName(item.EditFilePath));
            CheckDirectory(item.FilePath);
            UpladFile(item.EditFilePath, item.FilePath);
            item.Name = Path.GetFileName(item.EditFilePath);
            CreateTempFile(item, id);

            if (!string.IsNullOrEmpty(item.EditThumbPath) && File.Exists(item.EditThumbPath))
            {
                if (ThumbExistAndIsNotGeneric(item))
                    DeleteWithRetry(item.ThumbPath);
                item.ThumbPath =
                            Path.GetFileNameWithoutExtension(item.FilePath)
                            + MediaSettings.DefaultThumbSuffix
                            + Path.GetExtension(item.EditThumbPath);
                item.ThumbPath = Path.Combine(mediaSet.ItemRootFolder, item.ThumbPath);
                UpladFile(item.EditThumbPath, item.ThumbPath);
                CreateTempFile(item, id);
            }
            else
            {
                item.ThumbPath = GetIconFilePath(extension);
                item.TempThumbPath = item.ThumbPath;
            }

            item.EditFilePath = null;
            item.EditThumbPath = null;
            item.IsNew = false;

            return item;
        }
        private void UpladFile(string sourcePath, string targetPath)
        {
            if (File.Exists(targetPath))
                DeleteWithRetry(targetPath);
            File.Copy(sourcePath, targetPath);
        }

        #endregion

        #region Methods -> Delete
        public void DeleteFile(MediaSet mediaSet, string fileName)
        {
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

        #endregion

        #region Methods -> Path

        public string GetDefaultFilePath(MediaSet mediaSet)
        {
            return Path.Combine(mediaSet.ItemRootFolder, MediaSettings.FullDefaultImageName);
        }

        public string GetDefaultThumbFilePath(string sourceFile, string targetFolder)
        {
            string extension = Path.GetExtension(sourceFile);
            return Path.GetFileNameWithoutExtension(targetFolder) + MediaSettings.DefaultThumbSuffix + extension;
        }

        public void CheckDirectory(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public string GetEmptyImagePath()
        {
            return Path.Combine(MediaSettings.MediaRootFolder, Const_DefImage);
        }

        public string GetEmptyThumbImagePath()
        {
            return Path.Combine(MediaSettings.MediaRootFolder, Const_DefThumbImage);
        }

        public string GetIconFilePath(string extension)
        {
            string iconPath = "";
            string folder = Path.Combine(MediaSettings.MediaRootFolder, MediaSettings.FileTypeIconFolder);
            if (Directory.Exists(folder))
            {
                iconPath = Path.Combine(folder, extension.TrimStart('.').ToLower() + "." + MediaSettings.DefaultIconExtension);
                if (!File.Exists(iconPath))
                    iconPath = Path.Combine(folder, MediaSettings.NoFileTypeIcon + MediaSettings.DefaultIconExtension);
            }
            return iconPath;
        }

        public string GetTypeRootPath(string rootFolder, IACObject aCObject)
        {
            string path = rootFolder;
            if (aCObject == null)
                return path;
            string typeACUrl = aCObject.ACType.GetACUrl();
            typeACUrl = typeACUrl.TrimStart(MediaSettings.TrimStartString.ToCharArray());

            foreach (var item in typeACUrl.Split('\\'))
                path = Path.Combine(path, item);

            return path;
        }

        public string GetACObjectRootPath(string rootFolder, IACObject aCObject)
        {
            string itemACUrl = aCObject.GetACUrl();
            itemACUrl = itemACUrl.TrimStart(MediaSettings.TrimStartString.ToCharArray());
            string paht = Path.Combine(rootFolder, itemACUrl);
            return paht;
        }

        public bool ThumbExistAndIsNotGeneric(MediaItemPresentation item)
        {
            string fileName = Path.GetFileNameWithoutExtension(item.FilePath);
            return !string.IsNullOrEmpty(item.ThumbPath) && item.ThumbPath.Contains(fileName);
        }

        #endregion

        #region Methods -> Image

        /// <summary>
        /// Loading image for supported type
        /// </summary>
        /// <param name="imageInfo"></param>
        public void LoadIImageInfo(IImageInfo imageInfo, bool useTempImage = false)
        {
            Guid id = Guid.NewGuid();
            string typeRootFolder = GetTypeRootPath(MediaSettings.MediaRootFolder, imageInfo as IACObject);
            string tmpFolder = GetACObjectRootPath(typeRootFolder, imageInfo as IACObject);
            tmpFolder = Path.Combine(tmpFolder, "images");
            if (Directory.Exists(tmpFolder))
            {
                string defImage = Path.Combine(tmpFolder, MediaSettings.FullDefaultImageName);
                if (File.Exists(defImage))
                {
                    imageInfo.DefaultImage = defImage;

                    if(useTempImage)
                    {
                        imageInfo.DefaultImage = CreateTempItem(MediaSettings.TempFolder, defImage, id.ToString());
                    }

                }
                string thumbImage = Path.Combine(tmpFolder, MediaSettings.FullDefaultThumbImageName);
                if (File.Exists(thumbImage))
                {
                    imageInfo.DefaultThumbImage = thumbImage;

                    if (useTempImage)
                    {
                        string temp_name = $"{id}_temp";
                        imageInfo.DefaultImage = CreateTempItem(MediaSettings.TempFolder, defImage, temp_name);
                    }
                }
            }
        }

        public void RenderThumbImage(string sourceFile, string targetFile)
        {

            byte[] thumbImage = ResizeImage(sourceFile, MediaSettings.MaxThumbWidth, MediaSettings.MaxThumbHeight);
            if (thumbImage != null)
            {
                if (File.Exists(targetFile))
                    DeleteWithRetry(targetFile);
                File.WriteAllBytes(targetFile, thumbImage);
            }
        }

        public byte[] ResizeImage(string fileName, int maxWidth, int maxHeight, Quality quality = Quality.Medium)
        {
            return VBMediaController.ResizeImage(fileName, maxWidth, maxHeight, quality.ToString());
        }

        #endregion

        #region Methods -> CreateTempFile

        public void CreateTempFile(MediaItemPresentation item, Guid id)
        {
            string name = id.ToString();
            if (!string.IsNullOrEmpty(item.FilePath) && File.Exists(item.FilePath))
            {
                item.TempFilePath = CreateTempItem(MediaSettings.TempFolder, item.FilePath, name);
            }
        }

        public void CreateTempThumbFile(MediaItemPresentation item, Guid id)
        {
            string temp_name = $"{id}_temp";
            if (!string.IsNullOrEmpty(item.ThumbPath) && File.Exists(item.ThumbPath))
            {
                item.TempThumbPath = CreateTempItem(MediaSettings.TempFolder, item.ThumbPath, temp_name);
            }
        }

        public string CreateTempItem(string tempFolder, string filePath, string tempFileName)
        {
            string extension = Path.GetExtension(filePath);
            string tempFile = Path.Combine(tempFolder, tempFileName + extension);
            File.Copy(filePath, tempFile, true);
            return tempFile;
        }

        #endregion

        #region Methods -> CleanUp

        public void CleanUpFolder(IACObject aCObject, bool forceDelete = false)
        {
            string typeRootFolder = GetTypeRootPath(MediaSettings.MediaRootFolder, aCObject);
            string itemRootFolder = GetACObjectRootPath(typeRootFolder, aCObject);
            CleanUpFolder(itemRootFolder, forceDelete);
        }

        public void CleanUpFolder(string rootACObjectFolder, bool forceDelete = false)
        {
            bool? isEmpty = IsACObjectFolderEmpty(rootACObjectFolder);
            if (isEmpty != null)
            {
                if (isEmpty.Value || forceDelete)
                {
                    Directory.Delete(rootACObjectFolder, true);
                }
            }
        }

        public bool? IsACObjectFolderEmpty(string rootACObjectFolder)
        {
            bool? isEmpty = null;
            if (Directory.Exists(rootACObjectFolder))
            {
                string[] dirs = Directory.GetDirectories(rootACObjectFolder);
                isEmpty = !Directory.GetFiles(rootACObjectFolder).Any();
                foreach (string dir in dirs)
                {
                    isEmpty = (isEmpty ?? false) && (IsACObjectFolderEmpty(dir) ?? false);
                }
            }

            return isEmpty;
        }

        #endregion

        #region Methods -> OpenFileDialog

        public async Task<string> OpenFileDialog(bool isFolderPicker, string initialDirectory, bool useExisting, string defaultExtension = null, Dictionary<string, string> filters = null)
        {
            return await VBMediaController.OpenFileDialog(isFolderPicker, initialDirectory, useExisting, defaultExtension, filters);
        }

        #endregion

        #endregion

    }
}
