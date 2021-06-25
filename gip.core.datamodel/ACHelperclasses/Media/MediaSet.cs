using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.datamodel
{
    public class MediaSet
    {

        public MediaController MediaController { get; private set; }

        public MediaTypeSettingsItem MediaTypeSettings { get; private set; }

        public int ItemsCount { get; private set; }

        public string ItemRootFolder { get; private set; }

        public string ExtensionQuery { get; private set; }

        public int PageSize { get; private set; }

        public string Order { get; set; }
        public bool IsAscending { get; set; }


        #region ctor's

        public MediaSet(MediaController mediaController, int pageIndex, MediaTypeSettingsItem mediaTypeSettings, string filterName, string order, bool ascending)
        {
            MediaController = mediaController;
            PageSize = MediaController.MediaSettings.PageSize;
            MediaTypeSettings = mediaTypeSettings;
            ItemRootFolder = Path.Combine(MediaController.ItemRootFolder, mediaTypeSettings.FolderName);
            ExtensionQuery = JoinStrings(mediaTypeSettings.Extensions, filterName);
            Order = order;
            IsAscending = ascending;
            if (Directory.Exists(ItemRootFolder))
            {
                ItemsCount = QueryFilesWithExtensionWithoutDefault().Count();
            }
        }

        public List<MediaItemPresentation> GetFiles(int pageIndex)
        {
            if (Directory.Exists(ItemRootFolder))
            {
                var query = QueryFilesWithExtensionWithoutDefault();
                if (!string.IsNullOrEmpty(Order))
                    query = query.OrderBy(Order, IsAscending);
                else
                    query = query.OrderBy(c => c.Name);
                var files = query.Skip((pageIndex - 1) * PageSize).Take(PageSize).Select(c => Path.Combine(ItemRootFolder, c.Name)).ToList();
                List<MediaItemPresentation> result = new List<MediaItemPresentation>();
                foreach (var file in files)
                {
                    MediaItemPresentation presentationItem = new MediaItemPresentation();
                    presentationItem.FilePath = file;
                    presentationItem.Name = Path.GetFileName(file);

                    string thumbNamePattern = Path.GetFileNameWithoutExtension(file) + MediaController.MediaSettings.DefaultThumbSuffix;
                    FileInfo thumbFileInfo = new DirectoryInfo(Path.GetDirectoryName(file)).GetFiles().Where(c=>c.Name.Contains(thumbNamePattern)).FirstOrDefault();
                    if (thumbFileInfo != null)
                    {
                        presentationItem.ThumbPath = thumbFileInfo.FullName;
                        presentationItem.HaveOwnThumb = true;
                    }
                    if (string.IsNullOrEmpty(presentationItem.ThumbPath))
                        if (MediaTypeSettings.MediaType == MediaItemTypeEnum.Image)
                            presentationItem.ThumbPath = file;
                        else
                            presentationItem.ThumbPath = MediaController.GetIconFilePath(Path.GetExtension(file));
                    result.Add(presentationItem);
                }
                return result;
            }
            return null;
        }

        public virtual IEnumerable<FileInfo> QueryFilesWithExtensionWithoutDefault()
        {
            DirectoryInfo di = new DirectoryInfo(ItemRootFolder);
            return
                di
                .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(c =>
                        ExtensionQuery.Contains(c.Extension) 
                        && Path.GetFileNameWithoutExtension(c.Name) != "default"
                        && Path.GetFileNameWithoutExtension(c.Name) != "default_thumb"
                        && !Path.GetFileNameWithoutExtension(c.Name).EndsWith(MediaController.MediaSettings.DefaultThumbSuffix)
                );
        }

        #endregion

        #region private helper methods
        private string JoinStrings(List<string> extensions, string filterName)
        {
            if (!string.IsNullOrEmpty(filterName))
                filterName = "*" + filterName + "*";
            else
                filterName = "*";
            return string.Join(";", extensions.Select(c => filterName + c));
        }
        #endregion
    }
}
