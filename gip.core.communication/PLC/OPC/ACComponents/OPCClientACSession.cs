using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCClientACSession'}de{'OPCClientACSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class OPCClientACSession : ACSession
    {
        #region c´tors
        public OPCClientACSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
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
        [ACPropertyInfo(9999, DefaultValue = "localhost")]
        public string HostOfOPCServer
        {
            get; set;
        }

        [ACPropertyInfo(9999)]
        public string OPCServerName
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public string OPCServerCLSID
        {
            get;
            set;
        }
        #endregion
    }
}
