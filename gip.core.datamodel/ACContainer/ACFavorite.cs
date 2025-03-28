// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{

    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACFavorite'}de{'ACFavorite'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.Optional, false, true)]
    public class ACFavorite : EntityBase, IVBTileGrid
    {
        [DataMember]
        public short? TileColumn
        {
            get;
            set;
        }

        [DataMember]
        public short TileRow
        {
            get;
            set;
        }

        [DataMember]
        public string Title
        {
            get;
            set;
        }

        [DataMember]
        public string ACUrl
        {
            get;
            set;
        }

        [DataMember]
        public string IconACUrl
        {
            get;
            set;
        }

        [DataMember]
        public ACValueList Parameters
        {
            get;
            set;
        }

        Global.VBTileType IVBTileGrid.VBTileType
        {
            get
            {
                if (!string.IsNullOrEmpty(ACUrl))
                    return Global.VBTileType.Tile;
                return Global.VBTileType.Group;
            }
            set
            {
            }
        }
    }
}