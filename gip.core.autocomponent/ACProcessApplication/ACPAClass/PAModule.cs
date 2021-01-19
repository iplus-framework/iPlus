using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for simple Equipments (basic parts)
    /// Basisklasse für einfache Geräte (elemetare Bauteile)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'PAModule'}de{'PAModule'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAModule : PAClassPhysicalBase
    {
        static PAModule()
        {
            RegisterExecuteHandler(typeof(PAModule), HandleExecuteACMethod_PAModule);
        }

        public PAModule(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        //public override Global.ACStorableTypes ACStorableType
        //{
        //    get
        //    {
        //        return Global.ACStorableTypes.Required;
        //    }
        //}

        // Methods, Range: 300

        /// <summary>
        /// Stellt die Verknüpfung zum ACProgramLog her für Alarme die geloggt werden solllen
        /// </summary>
        /// <param name="newLog"></param>
        protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        {
            PAProcessModule processModule = FindParentComponent<PAProcessModule>(c => c is PAProcessModule);
            if (processModule != null)
                processModule.RelateAlarmLogWithProgramLog(newLog);
        }

        public static bool HandleExecuteACMethod_PAModule(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAClassPhysicalBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
    }
}
