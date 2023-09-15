using System.Collections.Generic;

namespace gip.core.media
{
    public class MediaTypeSettingsItem
    {
        public List<string> Extensions { get; set; }
        public string FolderName { get; set; }
        public MediaItemTypeEnum MediaType { get; set; }
    }
}
