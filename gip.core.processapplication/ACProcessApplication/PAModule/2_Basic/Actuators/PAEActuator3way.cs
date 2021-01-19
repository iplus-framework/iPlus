using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    /// <summary>
    /// Three-Way-Actuator without basic position
    /// Three-Wege-Stellglied ohne Grundstellung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'3-Way Actuator'}de{'3-Wege Stellglied'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator3way : PAEActuator2way
    {
        #region c'tors

        static PAEActuator3way()
        {
            RegisterExecuteHandler(typeof(PAEActuator3way), HandleExecuteACMethod_PAEActuator3way);
        }

        public PAEActuator3way(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatOut3 = new PAPoint(this, Const.PAPointMatOut3);
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

        #region Points

        PAPoint _PAPointMatOut3;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Pos3", true, "Position", "en{'Pos3'}de{'Pos3'}", Global.Operators.and)]
        public PAPoint PAPointMatOut3
        {
            get
            {
                return _PAPointMatOut3;
            }
        }

        #endregion

        #region Properties, Range: 700
        #region Read-Values from PLC
        [ACPropertyBindingTarget(730, "Read from PLC", "en{'Position 3'}de{'Position 3'}", "", false, false, RemotePropID=43)]
        public IACContainerTNet<Boolean> Pos3 { get; set; }
        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'Position 3 request'}de{'Position 3 Anforderung'}", "", false, false, RemotePropID=44)]
        public IACContainerTNet<Boolean> ReqPos3 { get; set; }
        #endregion
        #endregion

        #region Methods, Range: 700

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Position3":
                    Position3();
                    return true;
                case Const.IsEnabledPrefix + "Position3":
                    result = IsEnabledPosition3();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Position 3'}de{'Stellung 3'}", 700, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position3()
        {
            if (!IsEnabledPosition3())
                return;
            if (!PreExecute("Position3"))
                return;
            OnSetPosition3Values();
            PostExecute("Position3");
        }

        public virtual bool IsEnabledPosition3()
        {
            if (TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos3.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition3Values()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
            ReqPos3.ValueT = true;
        }

        protected override void OnSetPosition1Values()
        {
            base.OnSetPosition1Values();
            ReqPos3.ValueT = false;
        }

        protected override void OnSetPosition2Values()
        {
            base.OnSetPosition2Values();
            ReqPos3.ValueT = false;
        }

        protected override void GoToBasicPosition()
        {
        }

        public override void SwitchToAutomatic()
        {
            base.SwitchToAutomatic();
            ResetRequests();
        }

        protected override void OnOperatingModeChanged(Global.OperatingMode currentOperatingMode, Global.OperatingMode newOperatingMode)
        {
            if (newOperatingMode == Global.OperatingMode.Automatic)
                ResetRequests();
            base.OnOperatingModeChanged(currentOperatingMode, newOperatingMode);
        }

        private void ResetRequests()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
            ReqPos3.ValueT = false;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator3way(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
