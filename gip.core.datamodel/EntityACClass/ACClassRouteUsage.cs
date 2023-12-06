using System;
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
            db.ACClassRouteUsage.AddObject(entity);
            return entity;
        }
    }
}
