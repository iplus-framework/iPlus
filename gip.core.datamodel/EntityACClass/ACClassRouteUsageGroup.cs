using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ClassRouteUsageGroup'}de{'ClassRouteUsageGroup'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable)]
    public partial class ACClassRouteUsageGroup : VBEntityObject
    {
        public static ACClassRouteUsageGroup NewACObject(Database db)
        {
            ACClassRouteUsageGroup entity = new ACClassRouteUsageGroup();
            entity.ACClassRouteUsageGroupID = Guid.NewGuid();
            entity.UseFactor = 0;

            db.ACClassRouteUsageGroup.Add(entity);

            return entity;
        }
    }
}
