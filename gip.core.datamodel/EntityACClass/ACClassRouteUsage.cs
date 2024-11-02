// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ClassRouteUsage'}de{'ClassRouteUsage'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable)]
    public partial class ACClassRouteUsage : VBEntityObject, IInsertInfo, IUpdateInfo
    {
        public static ACClassRouteUsage NewACObject(Database db)
        {
            ACClassRouteUsage entity = new ACClassRouteUsage();
            entity.ACClassRouteUsageID = Guid.NewGuid();
            db.ACClassRouteUsage.Add(entity);
            return entity;
        }
    }
}
