using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Process module group'}de{'Prozessmodulgruppe'}", Global.ACKinds.TPAProcessModuleGroup, Global.ACStorableTypes.Required, false, true)]
    public class PAProcessModuleGroup : PAModule
    {
        static PAProcessModuleGroup()
        {
            RegisterExecuteHandler(typeof(PAProcessModuleGroup), HandleExecuteACMethod_PAProcessModuleGroup);
        }

        public PAProcessModuleGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #region Points
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAProcessModuleGroup(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
