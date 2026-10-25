// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace gip.core.media
{
    /// <summary>
    /// Shared file logic of the media subsystem. It is used by <see cref="ACMediaService"/> on the
    /// machine that owns the media root folder and by <see cref="ACMediaController"/> as local
    /// fallback when the service is not reachable but the media root folder is accessible
    /// (desktop client in the same network, SMB share or Linux mount).
    /// All paths that leave this class are relative to the media root folder.
    /// </summary>
    public class MediaFileOperations
    {
        public const string Const_DefImage = @"empty.jpg";
        public const string Const_DefThumbImage = @"empty_thumb.jpg";

        private readonly MediaSettings _Settings;
        private readonly Action<string, Exception> _LogException;

        public MediaFileOperations(MediaSettings settings, Action<string, Exception> logException = null)
        {
            _Settings = settings;
            _LogException = logException;
        }

        public MediaSettings MediaSettings
        {
            get { return _Settings; }
        }

        public bool IsMediaRootConfigured
        {
            get { return _Settings != null && _Settings.IsMediaRootConfigured; }
        }

        public bool IsMediaRootAccessible
        {
            get { return IsMediaRootConfigured && Directory.Exists(_Settings.MediaRootFolder); }
        }

        private void LogException(string method, Exception ec)
        {
            _LogException?.Invoke(method, ec);
        }

        #region Methods -> Content

        /// <summary>
        /// Returns the content of a media file together with its format information.
        /// </summary>
        public MediaContentResult GetFileContent(string filePath)
        {
            return ReadContent(filePath);
        }

        /// <summary>
        /// Returns the content of the thumbnail that belongs to a media file. If no thumbnail
        /// exists the file itself is returned for images, otherwise the file type icon.
        /// </summary>
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
        public MediaContentResult GetEmptyImageContent()
        {
            return ReadContent(GetEmptyImagePath());
        }
        public MediaContentResult GetEmptyThumbImageContent()
        {
            return ReadContent(GetEmptyThumbImagePath());
        }
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
        public MediaItemListResult GetFiles(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, int pageIndex, string filterName, string order, bool ascending)
        {
            MediaItemListResult result = new MediaItemListResult { Items = new List<MediaItemInfo>() };
            if (!IsMediaRootConfigured)
            {
                result.Message = new Msg { MessageLevel = eMsgLevel.Error, Message = "Media root folder is not configured." };
                return result;
            }

            MediaTypeSettingsItem typeSettings = _Settings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
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

            foreach (FileInfo file in query.Skip((pageIndex - 1) * _Settings.PageSize).Take(_Settings.PageSize))
            {
                string thumbPath = GetThumbPath(file.FullName);
                result.Items.Add(new MediaItemInfo
                {
                    Name = file.Name,
                    FilePath = GetRelativePath(file.FullName),
                    ThumbPath = string.IsNullOrEmpty(thumbPath) ? null : GetRelativePath(thumbPath),
                    IsDefault = file.Name.StartsWith(_Settings.DefaultImageName + "."),
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
        public MediaContentResult UploadFile(string typeACUrl, string itemACUrl, MediaItemTypeEnum mediaType, string fileName, byte[] content, bool isDefault, bool generateThumb, byte[] thumbContent)
        {
            if (!IsMediaRootConfigured)
                return MediaContentResult.Failed("Media root folder is not configured.");
            if (content == null || content.Length == 0)
                return MediaContentResult.Failed("No content passed.");

            MediaTypeSettingsItem typeSettings = _Settings.MediaItemTypes.FirstOrDefault(c => c.MediaType == mediaType);
            if (typeSettings == null)
                return MediaContentResult.Failed($"Unknown media type '{mediaType}'.");

            string itemRootFolder = GetItemRootFolder(typeACUrl, itemACUrl, typeSettings);
            CheckDirectory(itemRootFolder);

            string targetName = isDefault
                ? _Settings.FullDefaultImageName
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
                    byte[] thumb = ResizeImageContent(content, _Settings.MaxThumbWidth, _Settings.MaxThumbHeight);
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
                LogException(nameof(UploadFile), ec);
                return MediaContentResult.Failed(ec.Message);
            }
        }
        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            return DeleteWithRetry(GetFullPath(filePath));
        }
        public bool CleanUpFolder(string typeACUrl, string itemACUrl, bool forceDelete)
        {
            if (!IsMediaRootConfigured)
                return false;

            string itemRootFolder = GetACObjectRootPath(GetTypeRootPath(_Settings.MediaRootFolder, typeACUrl), itemACUrl);
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
                LogException(nameof(CleanUpFolder), ec);
                return false;
            }
        }

        #endregion

        #region Methods -> Image

        /// <summary>
        /// Scales the passed image content down to the given maximum size and returns the
        /// encoded result. Returns null when the content cannot be decoded.
        /// </summary>
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
                LogException(nameof(ResizeImageContent), ec);
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
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(_Settings?.MediaRootFolder))
                return fullPath;

            string root = _Settings.MediaRootFolder;
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
            if (string.IsNullOrEmpty(storedPath) || string.IsNullOrEmpty(_Settings?.MediaRootFolder))
                return storedPath;

            if (Path.IsPathRooted(storedPath))
                return storedPath;

            string relative = storedPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_Settings.MediaRootFolder, relative);
        }
        public string GetMediaRootFolder()
        {
            return _Settings?.MediaRootFolder;
        }

        public string GetEmptyImagePath()
        {
            return Path.Combine(_Settings.MediaRootFolder, Const_DefImage);
        }

        public string GetEmptyThumbImagePath()
        {
            return Path.Combine(_Settings.MediaRootFolder, Const_DefThumbImage);
        }

        public string GetIconFilePath(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return null;

            string folder = Path.Combine(_Settings.MediaRootFolder, _Settings.FileTypeIconFolder);
            if (!Directory.Exists(folder))
                return null;

            string iconPath = Path.Combine(folder, extension.TrimStart('.').ToLower() + "." + _Settings.DefaultIconExtension);
            if (!File.Exists(iconPath))
                iconPath = Path.Combine(folder, _Settings.NoFileTypeIcon);
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
            return Path.GetFileNameWithoutExtension(filePath) + _Settings.DefaultThumbSuffix + Path.GetExtension(filePath);
        }

        public string GetTypeRootPath(string rootFolder, string typeACUrl)
        {
            string path = rootFolder;
            if (string.IsNullOrEmpty(typeACUrl))
                return path;

            typeACUrl = typeACUrl.TrimStart(_Settings.TrimStartString.ToCharArray());
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

            itemACUrl = itemACUrl.TrimStart(_Settings.TrimStartString.ToCharArray());
            return Path.Combine(rootFolder, itemACUrl);
        }

        private string GetItemRootFolder(string typeACUrl, string itemACUrl, MediaTypeSettingsItem typeSettings)
        {
            string typeRootFolder = GetTypeRootPath(_Settings.MediaRootFolder, typeACUrl);
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
                LogException(nameof(ReadContent), ec);
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
                    LogException(nameof(DeleteWithRetry), ec);
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
    }
}
