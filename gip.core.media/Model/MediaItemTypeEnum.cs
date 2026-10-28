// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿

using gip.core.datamodel;

namespace gip.core.media
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemTypeEnum'}de{'MediaItemTypeEnum'}", Global.ACKinds.TACEnum)]
    public enum MediaItemTypeEnum
    {
        Image,
        Document,
        Audio,
        Video
    }
}
