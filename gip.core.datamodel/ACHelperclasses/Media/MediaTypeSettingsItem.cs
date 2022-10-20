using System.Collections.Generic;

namespace gip.core.datamodel
{
    public class MediaTypeSettingsItem
    {
        public List<string> Extensions { get; set; }
        public string FolderName { get; set; }
        public MediaItemTypeEnum MediaType { get; set; }
    }
}
