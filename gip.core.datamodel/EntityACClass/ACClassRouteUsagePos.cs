using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ClassRouteUsagePos'}de{'ClassRouteUsagePos'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable)]
    public partial class ACClassRouteUsagePos
    {
        public static ACClassRouteUsagePos NewACObject(Database db, ACClassRouteUsage routeUsage)
        {
            ACClassRouteUsagePos entity = new ACClassRouteUsagePos();
            entity.ACClassRouteUsagePosID = Guid.NewGuid();
            entity.ACClassRouteUsageID = routeUsage.ACClassRouteUsageID;

            db.ACClassRouteUsagePos.Add(entity);
            return entity;
        }
    }
}
