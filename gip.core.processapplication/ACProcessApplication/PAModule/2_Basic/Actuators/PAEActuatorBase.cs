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
    public enum PAEActuatorPos
    {
        BasicPos = 0,       // Closed Bitmap: 000
        Pos1_Open = 1,      // Open or Position 1,  Bitmap: 100
        Pos2 = 2,           // Position 2,  Bitmap: 010
        Pos1_Pos2 = 3,      // Position 1 && 2,  Bitmap: 110
        Pos3 = 4,           // Position 3,  Bitmap: 001
        Pos1_Pos3 = 5,      // Position 1 && 3,  Bitmap: 101
        Pos2_Pos3 = 6,      // Position 2 && 3,  Bitmap: 011
        Pos1_Pos2_Pos3 = 7, // Position 1 && 2 && 3,  Bitmap: 111
    }

    /// <summary>
    /// Base-Class for Actuators (Valves, Flaps, Slider...)
    /// Basisklasse für Stellglieder (Ventile, Klappen, Schieber...)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Actuator'}de{'Stellglied'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEActuatorBase : PAEControlModuleBase
    {
        #region c'tors

        static PAEActuatorBase()
        {
            RegisterExecuteHandler(typeof(PAEActuatorBase), HandleExecuteACMethod_PAEActuatorBase);
        }

        public PAEActuatorBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _Behaviour = new ACPropertyConfigValue<ushort>(this, "Behaviour", 0);

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            _ = Behaviour;
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

        #region Points
        #endregion

        #region Properties, Range:500

        #region Configuration
        private ACPropertyConfigValue<ushort> _Behaviour;
        [ACPropertyConfig("en{'Behaviour'}de{'Verhalten'}")]
        public ushort Behaviour
        {
            get
            {
                return _Behaviour.ValueT;
            }
            set
            {
                _Behaviour.ValueT = value;
            }
        }
        #endregion

        //#region Read-Values from PLC
        //[ACPropertyBindingTarget(530, "Read from PLC", "en{'Position'}de{'Stellung'}", "", false, false)]
        //public IACPropertyNet<PAEActuatorPos> Position { get; set; }
        //#endregion

        //#region Write-Values to PLC
        //[ACPropertyBindingTarget(550, "Write to PLC", "en{'Position request'}de{'Stellung Anforderung'}", "", false, false)]
        //public IACPropertyNet<PAEActuatorPos> ReqPosition { get; set; }
        //#endregion

        #endregion

        #region Methods, Range: 500
        public override void SwitchToMaintenance()
        {
            if (!IsEnabledSwitchToMaintenance())
                return;
            GoToBasicPosition();
            base.SwitchToMaintenance();
        }

        public override bool IsEnabledSwitchToMaintenance()
        {
            return base.IsEnabledSwitchToMaintenance();
        }

        protected abstract void GoToBasicPosition();

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuatorBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEControlModuleBase (out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
