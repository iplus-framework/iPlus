using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.manager
{
    [ACClassInfo("VarioSystem", "en{'VBPresenterProgramLog'}de{'VBPresenterProgramLog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, true)]
    public class VBPresenterProgramLog : ACBSO
    {
        #region c´tors
        public VBPresenterProgramLog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            DatabaseMode = DatabaseModes.ParentDB;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

    }
}
