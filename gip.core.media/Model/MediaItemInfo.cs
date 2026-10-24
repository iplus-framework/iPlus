// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gip.core.media
{
    /// <summary>
    /// Transportable description of a single media file. Contains only paths and metadata,
    /// the binary content is requested separately via <see cref="ACMediaService.GetFileContent"/>.
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemInfo'}de{'MediaItemInfo'}", Global.ACKinds.TACSimpleClass)]
    public class MediaItemInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string ThumbPath { get; set; }

        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public MediaItemTypeEnum MediaType { get; set; }
    }

    /// <summary>
    /// Transportable result of a media listing request.
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemListResult'}de{'MediaItemListResult'}", Global.ACKinds.TACSimpleClass)]
    public class MediaItemListResult
    {
        [DataMember]
        public List<MediaItemInfo> Items { get; set; }

        [DataMember]
        public int ItemsCount { get; set; }

        [DataMember]
        public string ItemRootFolder { get; set; }

        [DataMember]
        public string ExtensionQuery { get; set; }

        [DataMember]
        public Msg Message { get; set; }
    }
}
