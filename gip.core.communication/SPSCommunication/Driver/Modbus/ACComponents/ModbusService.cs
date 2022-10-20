﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ModbusService'}de{'ModbusService'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ModbusService : ACService
    {
        #region c´tors
        public ModbusService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;           
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        #endregion

        #region methods

        #endregion
    }
}
