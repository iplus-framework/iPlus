// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static gip.core.autocomponent.PARole;

namespace gip.core.media
{
    /// <summary>
    /// Server-side media service. It performs all file system access on the machine where the
    /// media root folder is reachable and returns the content as <see cref="byte"/> arrays
    /// together with format information (<see cref="MediaContentResult"/>).
    /// <para>
    /// Clients (desktop, browser, mobile) reach this component through its proxy instance at
    /// <see cref="DefaultServiceACUrl"/>. The runtime handles the transport, therefore no
    /// client/server plumbing is required here.
    /// </para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Media Service'}de{'Media Service'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACMediaService : PARole
    {
        #region const

        public const string C_DefaultServiceACIdentifier = "ACMediaService";
        public const string DefaultServiceACUrl = "\\Service\\ACMediaService";

        public const string Const_DefImage = @"empty.jpg";
        public const string Const_DefThumbImage = @"empty_thumb.jpg";

        #endregion

        #region c'tors

        public ACMediaService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _MediaSettings = new MediaSettings(null);
            if (!_MediaSettings.IsMediaRootConfigured)
            {
                Messages.LogWarning(this.GetACUrl(), nameof(ACInit),
                    $"Media root folder is not configured. Set the config value '{_MediaSettings.MediaRootConfigName}' of BSOMedia for this operating system.");
            }
            else if (!Directory.Exists(_MediaSettings.MediaRootFolder))
            {
                Messages.LogWarning(this.GetACUrl(), nameof(ACInit),
                    $"Media root folder '{_MediaSettings.MediaRootFolder}' is not accessible on this machine. Check that the share is mounted.");
            }
            return true;
        }

        public override System.Threading.Tasks.Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _MediaSettings = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Service resolution

        public static ACMediaService GetServiceInstance(ACComponent requester)
        {
            // Prefer the configured network service at "\Service\ACMediaService".
            ACMediaService serviceInstance = GetServiceInstance<ACMediaService>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.Default);
            if (serviceInstance != null)
                return serviceInstance;

            // Fall back to a local instance so that the component also works when the
            // service has not been configured in the database yet.
            return GetServiceInstance<ACMediaService>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.FirstOrDefault);
        }

        public static ACRef<ACMediaService> ACRefToServiceInstance(ACComponent requester)
        {
            ACMediaService serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<ACMediaService>(serviceInstance, requester);
            return null;
        }

        public static void DetachACRefFromServiceInstance(ACComponent requester, ACRef<ACMediaService> acRef)
        {
            if (acRef == null)
                return;
            acRef.Detach();
        }

        #endregion

        #region Properties

        private MediaSettings _MediaSettings;
        public MediaSettings MediaSettings
        {
            get { return _MediaSettings; }
        }

        public bool IsMediaRootConfigured
        {
            get { return _MediaSettings != null && _MediaSettings.IsMediaRootConfigured; }
        }

        #endregion

        #region Methods -> Content

        /// <summary>
        /// Returns the content of a media file together with its format information.
        /// </summary>
        [ACMethodInfo("", "en{'Get file content'}de{'Dateiinhalt lesen'}", 300)]
        public MediaContentResult GetFileContent(string filePath)
        {
            return ReadContent(filePath);
        }

        /// <summary>
        /// Returns the content of the thumbnail that belongs to a media file. If no thumbnail
        /// exists the file itself is returned for images, otherwise the file type icon.
        /// </summary>
        [ACMethodInfo("", "en{'Get thumb content'}de{'Daumenbildinhalt lesen'}", 301)]
        public MediaContentResult GetThumbContent(string filePath, MediaItemTypeEnum mediaType)
        {
            if (string.IsNullOrEmpty(filePath))
                return MediaContentResult.Failed("No file path passed.");

            string thumbPath = GetThumbPath(filePath);
            if (!string.IsNullOrEmpty(thumbPath) && File.Exists(thumbPath))
                return ReadContent(thumbPath);

            if (mediaType == MediaItemTypeEnum.Image)
                return ReadContent(filePath);

            return ReadContent(GetIconFilePath(Path.GetExtension(filePath)));
        }

        [ACMethodInfo("", "en{'Get empty image'}de{'Standardbild lesen'}", 302)]
        public MediaContentResult GetEmptyImageContent()
        {
            return ReadContent(GetEmptyImagePath());
        }

        [ACMethodInfo("", "en{'Get empty thumb image'}de{'Standard-Daumenbild lesen'}", 303)]
        public MediaContentResult GetEmptyThumbImageContent()
        {
            return ReadContent(GetEmptyThumbImagePath());
        }

        [ACMethodInfo("", "en{'Get file type icon'}de{'Dateityp-Symbol lesen'}", 304)]
        public MediaContentResult GetIconContent(string extension)
        {
            return ReadContent(GetIconFilePath(extension));
        }

        #endregion

        #region Methods -> Listing

        /// <summary>
        /// Lists the media files of an AC object. The AC object is identified by its ACUrl and
        /// the ACUrl of its type, so that no server-side instance is required.
        /// </summary>
        [ACMethodInfo("", "en{'Get media files'}de{'Mediendateien lesen'}", 310)]
        public MediaItemListResult GetFiles(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, int pageIndex, string filterName, string order, bool ascending)
        {
            MediaItemListResult result = new MediaItemListResult { Items = new List<MediaItemInfo>() };
            if (!IsMediaRootConfigured)
            {
                result.Message = new Msg { MessageLevel = eMsgLevel.Error, Message = "Media root folder is not configured." };
                return result;
            }

            MediaTypeSettingsItem typeSettings = _MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            if (typeSettings == null)
            {
                result.Message = new Msg { MessageLevel = eMsgLevel.Error, Message = $"Unknown media type '{mediaType}'." };
                return result;
            }

            string itemRootFolder = GetItemRootFolder(typeACUrl, itemACUrl, typeSettings);
            result.ItemRootFolder = GetRelativePath(itemRootFolder);
            result.ExtensionQuery = JoinStrings(typeSettings.Extensions, filterName);

            if (!Directory.Exists(itemRootFolder))
                return result;

            IEnumerable<FileInfo> query = QueryFiles(itemRootFolder, result.ExtensionQuery);
            result.ItemsCount = query.Count();

            if (!string.IsNullOrEmpty(order))
                query = query.OrderBy(order, ascending);
            else
                query = query.OrderBy(c => c.Name);

            if (pageIndex < 1)
                pageIndex = 1;

            foreach (FileInfo file in query.Skip((pageIndex - 1) * _MediaSettings.PageSize).Take(_MediaSettings.PageSize))
            {
                string thumbPath = GetThumbPath(file.FullName);
                result.Items.Add(new MediaItemInfo
                {
                    Name = file.Name,
                    FilePath = GetRelativePath(file.FullName),
                    ThumbPath = string.IsNullOrEmpty(thumbPath) ? null : GetRelativePath(thumbPath),
                    IsDefault = file.Name.StartsWith(_MediaSettings.DefaultImageName + "."),
                    MediaType = mediaType
                });
            }

            return result;
        }

        #endregion

        #region Methods -> Upload / Delete

        /// <summary>
        /// Stores the passed content in the media folder of an AC object and returns the
        /// resulting file description.
        /// </summary>
        [ACMethodInfo("", "en{'Upload media file'}de{'Mediendatei speichern'}", 320)]
        public MediaContentResult UploadFile(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, string fileName, byte[] content, bool isDefault, bool generateThumb, byte[] thumbContent)
        {
            if (!IsMediaRootConfigured)
                return MediaContentResult.Failed("Media root folder is not configured.");
            if (content == null || content.Length == 0)
                return MediaContentResult.Failed("No content passed.");

            MediaTypeSettingsItem typeSettings = _MediaSettings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            if (typeSettings == null)
                return MediaContentResult.Failed($"Unknown media type '{mediaType}'.");

            string itemRootFolder = GetItemRootFolder(typeACUrl, itemACUrl, typeSettings);
            CheckDirectory(itemRootFolder);

            string targetName = isDefault
                ? _MediaSettings.FullDefaultImageName
                : Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(targetName))
                return MediaContentResult.Failed("No file name passed.");

            string targetPath = Path.Combine(itemRootFolder, targetName);
            try
            {
                WriteAllBytes(targetPath, content);

                string thumbPath = null;
                if (generateThumb && mediaType == MediaItemTypeEnum.Image)
                {
                    thumbPath = Path.Combine(itemRootFolder, GetThumbFileName(targetPath));
                    byte[] thumb = ResizeImageContent(content, _MediaSettings.MaxThumbWidth, _MediaSettings.MaxThumbHeight);
                    if (thumb != null)
                        WriteAllBytes(thumbPath, thumb);
                }
                else if (thumbContent != null && thumbContent.Length > 0)
                {
                    thumbPath = Path.Combine(itemRootFolder, GetThumbFileName(targetPath));
                    WriteAllBytes(thumbPath, thumbContent);
                }

                return new MediaContentResult
                {
                    Success = true,
                    FileName = targetName,
                    FilePath = GetRelativePath(targetPath),
                    Extension = Path.GetExtension(targetPath),
                    MimeType = GetMimeType(Path.GetExtension(targetPath)),
                    Content = content
                };
            }
            catch (Exception ec)
            {
                Messages.LogException(this.GetACUrl(), nameof(UploadFile), ec);
                return MediaContentResult.Failed(ec.Message);
            }
        }

        [ACMethodInfo("", "en{'Delete media file'}de{'Mediendatei löschen'}", 321)]
        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            return DeleteWithRetry(GetFullPath(filePath));
        }

        [ACMethodInfo("", "en{'Delete media folder'}de{'Medienordner löschen'}", 322)]
        public bool CleanUpFolder(string typeACUrl, string itemACUrl, bool forceDelete)
        {
            if (!IsMediaRootConfigured)
                return false;

            string itemRootFolder = GetACObjectRootPath(GetTypeRootPath(_MediaSettings.MediaRootFolder, typeACUrl), itemACUrl);
            bool? isEmpty = IsFolderEmpty(itemRootFolder);
            if (isEmpty == null)
                return false;
            if (!isEmpty.Value && !forceDelete)
                return false;

            try
            {
                Directory.Delete(itemRootFolder, true);
                return true;
            }
            catch (Exception ec)
            {
                Messages.LogException(this.GetACUrl(), nameof(CleanUpFolder), ec);
                return false;
            }
        }

        #endregion

        #region Methods -> Image

        /// <summary>
        /// Scales the passed image content down to the given maximum size and returns the
        /// encoded result. Returns null when the content cannot be decoded.
        /// </summary>
        [ACMethodInfo("", "en{'Resize image'}de{'Bild skalieren'}", 330)]
        public MediaContentResult ResizeImage(byte[] content, int maxWidth, int maxHeight)
        {
            if (content == null || content.Length == 0)
                return MediaContentResult.Failed("No content passed.");

            byte[] resized = ResizeImageContent(content, maxWidth, maxHeight);
            if (resized == null)
                return MediaContentResult.Failed("Image could not be decoded.");

            return new MediaContentResult
            {
                Success = true,
                Content = resized,
                Extension = ".png",
                MimeType = "image/png"
            };
        }

        private byte[] ResizeImageContent(byte[] content, int maxWidth, int maxHeight)
        {
            try
            {
                using (MemoryStream sourceStream = new MemoryStream(content))
                using (SKBitmap sourceBitmap = SKBitmap.Decode(sourceStream))
                {
                    if (sourceBitmap == null)
                        return null;

                    int width = Math.Min(maxWidth, sourceBitmap.Width);
                    int height = Math.Min(maxHeight, sourceBitmap.Height);
                    SKSamplingOptions samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Nearest);
                    using (SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), samplingOptions))
                    using (SKImage scaledImage = SKImage.FromBitmap(scaledBitmap))
                    using (SKData data = scaledImage.Encode())
                    {
                        return data?.ToArray();
                    }
                }
            }
            catch (Exception ec)
            {
                Messages.LogException(this.GetACUrl(), nameof(ResizeImageContent), ec);
                return null;
            }
        }

        #endregion

        #region Methods -> Path

        /// <summary>
        /// Converts a full path below the media root folder into a path relative to it.
        /// Relative paths are stored so that media items stay portable between Windows and Linux.
        /// </summary>
        public string GetRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(_MediaSettings?.MediaRootFolder))
                return fullPath;

            string root = _MediaSettings.MediaRootFolder;
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return fullPath;

            string relative = fullPath.Substring(root.Length).TrimStart('\\', '/');
            return relative.Replace('\\', '/');
        }

        /// <summary>
        /// Resolves a stored path against the media root folder. Absolute paths of older
        /// installations are returned unchanged so that existing data keeps working.
        /// </summary>
        public string GetFullPath(string storedPath)
        {
            if (string.IsNullOrEmpty(storedPath) || string.IsNullOrEmpty(_MediaSettings?.MediaRootFolder))
                return storedPath;

            if (Path.IsPathRooted(storedPath))
                return storedPath;

            string relative = storedPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_MediaSettings.MediaRootFolder, relative);
        }

        [ACMethodInfo("", "en{'Get media root folder'}de{'Medien-Stammordner lesen'}", 340)]
        public string GetMediaRootFolder()
        {
            return _MediaSettings?.MediaRootFolder;
        }

        public string GetEmptyImagePath()
        {
            return Path.Combine(_MediaSettings.MediaRootFolder, Const_DefImage);
        }

        public string GetEmptyThumbImagePath()
        {
            return Path.Combine(_MediaSettings.MediaRootFolder, Const_DefThumbImage);
        }

        public string GetIconFilePath(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return null;

            string folder = Path.Combine(_MediaSettings.MediaRootFolder, _MediaSettings.FileTypeIconFolder);
            if (!Directory.Exists(folder))
                return null;

            string iconPath = Path.Combine(folder, extension.TrimStart('.').ToLower() + "." + _MediaSettings.DefaultIconExtension);
            if (!File.Exists(iconPath))
                iconPath = Path.Combine(folder, _MediaSettings.NoFileTypeIcon);
            return iconPath;
        }

        public string GetThumbPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            string fullPath = GetFullPath(filePath);
            string folder = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                return null;

            string thumbName = GetThumbFileName(fullPath);
            string thumbPath = Path.Combine(folder, thumbName);
            return File.Exists(thumbPath) ? thumbPath : null;
        }

        private string GetThumbFileName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath) + _MediaSettings.DefaultThumbSuffix + Path.GetExtension(filePath);
        }

        public string GetTypeRootPath(string rootFolder, string typeACUrl)
        {
            string path = rootFolder;
            if (string.IsNullOrEmpty(typeACUrl))
                return path;

            typeACUrl = typeACUrl.TrimStart(_MediaSettings.TrimStartString.ToCharArray());
            foreach (string item in typeACUrl.Split('\\'))
            {
                if (!string.IsNullOrEmpty(item))
                    path = Path.Combine(path, item);
            }
            return path;
        }

        public string GetACObjectRootPath(string rootFolder, string itemACUrl)
        {
            if (string.IsNullOrEmpty(itemACUrl))
                return rootFolder;

            itemACUrl = itemACUrl.TrimStart(_MediaSettings.TrimStartString.ToCharArray());
            return Path.Combine(rootFolder, itemACUrl);
        }

        private string GetItemRootFolder(string typeACUrl, string itemACUrl, MediaTypeSettingsItem typeSettings)
        {
            string typeRootFolder = GetTypeRootPath(_MediaSettings.MediaRootFolder, typeACUrl);
            string itemRootFolder = GetACObjectRootPath(typeRootFolder, itemACUrl);
            return Path.Combine(itemRootFolder, typeSettings.FolderName);
        }

        #endregion

        #region Helper methods

        private MediaContentResult ReadContent(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return MediaContentResult.Failed("No file path passed.");

            string fullPath = GetFullPath(filePath);
            if (!File.Exists(fullPath))
                return MediaContentResult.Failed($"File '{filePath}' does not exist.", eMsgLevel.Warning);

            try
            {
                string extension = Path.GetExtension(fullPath);
                return new MediaContentResult
                {
                    Success = true,
                    Content = File.ReadAllBytes(fullPath),
                    FileName = Path.GetFileName(fullPath),
                    FilePath = GetRelativePath(fullPath),
                    Extension = extension,
                    MimeType = GetMimeType(extension)
                };
            }
            catch (Exception ec)
            {
                Messages.LogException(this.GetACUrl(), nameof(ReadContent), ec);
                return MediaContentResult.Failed(ec.Message);
            }
        }

        private static IEnumerable<FileInfo> QueryFiles(string folder, string extensionQuery)
        {
            return new DirectoryInfo(folder)
                .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(c => extensionQuery.Contains(c.Extension)
                         && !Path.GetFileNameWithoutExtension(c.Name).EndsWith("_thumb"));
        }

        private static string JoinStrings(List<string> extensions, string filterName)
        {
            filterName = string.IsNullOrEmpty(filterName) ? "*" : "*" + filterName + "*";
            return string.Join(";", extensions.Select(c => filterName + c));
        }

        private void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void WriteAllBytes(string path, byte[] content)
        {
            if (File.Exists(path))
                DeleteWithRetry(path);
            File.WriteAllBytes(path, content);
        }

        private bool DeleteWithRetry(string file)
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
                    Messages.LogException(this.GetACUrl(), nameof(DeleteWithRetry), ec);
                }
            }
            return isDeleted;
        }

        private bool? IsFolderEmpty(string folder)
        {
            if (!Directory.Exists(folder))
                return null;

            bool isEmpty = !Directory.GetFiles(folder).Any();
            foreach (string dir in Directory.GetDirectories(folder))
                isEmpty = isEmpty && (IsFolderEmpty(dir) ?? false);
            return isEmpty;
        }

        private static string GetMimeType(string extension)
        {
            switch ((extension ?? string.Empty).ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".xml":
                    return "application/xml";
                case ".mp3":
                    return "audio/mpeg";
                case ".avi":
                    return "video/x-msvideo";
                case ".mov":
                    return "video/quicktime";
                case ".flv":
                    return "video/x-flv";
                default:
                    return "application/octet-stream";
            }
        }

        #endregion

        #region HandleExecuteACMethod

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(GetFileContent):
                    result = GetFileContent(acParameter[0] as string);
                    return true;
                case nameof(GetThumbContent):
                    result = GetThumbContent(acParameter[0] as string, (MediaItemTypeEnum)acParameter[1]);
                    return true;
                case nameof(GetEmptyImageContent):
                    result = GetEmptyImageContent();
                    return true;
                case nameof(GetEmptyThumbImageContent):
                    result = GetEmptyThumbImageContent();
                    return true;
                case nameof(GetIconContent):
                    result = GetIconContent(acParameter[0] as string);
                    return true;
                case nameof(GetFiles):
                    result = GetFiles(acParameter[0] as string, acParameter[1] as string, (MediaItemTypeEnum)acParameter[2],
                                      (int)acParameter[3], acParameter[4] as string, acParameter[5] as string, (bool)acParameter[6]);
                    return true;
                case nameof(UploadFile):
                    result = UploadFile(acParameter[0] as string, acParameter[1] as string, (MediaItemTypeEnum)acParameter[2],
                                        acParameter[3] as string, acParameter[4] as byte[], (bool)acParameter[5], (bool)acParameter[6], acParameter[7] as byte[]);
                    return true;
                case nameof(DeleteFile):
                    result = DeleteFile(acParameter[0] as string);
                    return true;
                case nameof(CleanUpFolder):
                    result = CleanUpFolder(acParameter[0] as string, acParameter[1] as string, (bool)acParameter[2]);
                    return true;
                case nameof(ResizeImage):
                    result = ResizeImage(acParameter[0] as byte[], (int)acParameter[1], (int)acParameter[2]);
                    return true;
                case nameof(GetMediaRootFolder):
                    result = GetMediaRootFolder();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
