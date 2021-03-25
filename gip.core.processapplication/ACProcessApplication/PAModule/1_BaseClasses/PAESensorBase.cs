using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{''}de{''}", Global.ACKinds.TACEnum)]
    public enum PAESensorRole : short
    {
        None = 0,
        Indicator = 1,
        FaultSensor = 2,
    }

    /// <summary>
    /// Baseclass for sensors
    /// Basisklasse für Sensoren
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass sensors'}de{'Basisklasse Sensoren'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAESensorBase : PAModule
    {
        #region c'tors

        static PAESensorBase()
        {
            RegisterExecuteHandler(typeof(PAESensorBase), HandleExecuteACMethod_PAESensorBase);
        }

        public PAESensorBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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
        #endregion

        #region Properties, Range: 400

        #region Configuration
        [ACPropertyBindingTarget(400,"Configuration","en{'Address of Input'}de{'Eingangsadresse'}", "", true, true, RemotePropID = 19)]
        public IACContainerTNet<String> InputAddress { get; set; }
        #endregion

        #endregion
        // Methods, Range: 400

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAESensorBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
