using System;
using gip.core.datamodel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.media
{
    public class MediaSet
    {
        public MediaItemTypeEnum MediaType { get; set; }
        public ACMediaController MediaController { get; private set; }
        public MediaSettings MediaSettings { get; private set; }

        public MediaTypeSettingsItem MediaTypeSettings { get; set; }

        public int ItemsCount { get; set; }

        public string ItemRootFolder { get; set; }

        public string ExtensionQuery { get; set; }

        public int PageSize { get; set; }

        public string Order { get; set; }
        public bool IsAscending { get; set; }

        #region ctor's

        public MediaSet()
        {

        }

        #endregion
        
    }
}
