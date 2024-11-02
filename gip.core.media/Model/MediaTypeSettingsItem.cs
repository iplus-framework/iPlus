// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Collections.Generic;

namespace gip.core.media
{
    public class MediaTypeSettingsItem
    {
        public List<string> Extensions { get; set; }
        public string FolderName { get; set; }
        public MediaItemTypeEnum MediaType { get; set; }
    }
}
