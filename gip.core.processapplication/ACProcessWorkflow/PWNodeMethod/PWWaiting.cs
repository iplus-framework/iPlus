using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWWaiting'}de{'PWWaiting'}", Global.ACKinds.TPWNodeMethod, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWWaiting : PWNodeProcessMethod
    {   
        public const string PWClassName = "PWWaiting";

        #region c´tors
        public PWWaiting(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }
        #endregion
    }
}
