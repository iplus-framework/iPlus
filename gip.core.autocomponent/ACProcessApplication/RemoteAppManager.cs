using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Root-class that represents another iplus-System.
    /// Below this class instances that are responsible for synchronizing data beetween this and the remote System
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'APP-Manager Remote'}de{'APP-Manager Remote'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class RemoteAppManager : ApplicationManager
    {
        #region c'tors
        public RemoteAppManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _RemoteConnString = new ACPropertyConfigValue<string>(this, "RemoteConnString", "");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _ = _RemoteConnString.ValueT;
            return result;
        }
        #endregion

        #region Config
        private ACPropertyConfigValue<string> _RemoteConnString;
        [ACPropertyConfig("en{'Remote Connection String'}de{'Remote Connection string'}")]
        public string RemoteConnString
        {
            get
            {
                return _RemoteConnString.ValueT;
            }
        }
        #endregion
    }
}
