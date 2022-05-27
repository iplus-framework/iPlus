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
    /// Two-Way-Actuator without basic position
    /// Zwei-Wege-Stellglied ohne Grundstellung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'2-Way Actuator'}de{'2-Wege Stellglied'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator2way : PAEActuatorBase
    {
        #region c'tors

        static PAEActuator2way()
        {
            RegisterExecuteHandler(typeof(PAEActuator2way), HandleExecuteACMethod_PAEActuator2way);
        }

        public PAEActuator2way(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn1 = new PAPoint(this, nameof(PAPointMatIn1));
            _PAPointMatOut1 = new PAPoint(this, nameof(PAPointMatOut1));
            _PAPointMatOut2 = new PAPoint(this, nameof(PAPointMatOut2));
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
        PAPoint _PAPointMatIn1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatIn1
        {
            get
            {
                return _PAPointMatIn1;
            }
        }

        protected PAPoint _PAPointMatOut1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Pos1", true, "Position", "en{'Pos1'}de{'Pos1'}", Global.Operators.and)]
        public virtual PAPoint PAPointMatOut1
        {
            get
            {
                return _PAPointMatOut1;
            }
        }

        protected PAPoint _PAPointMatOut2;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Pos2",true, "Position", "en{'Pos2'}de{'Pos2'}", Global.Operators.and)]
        public virtual PAPoint PAPointMatOut2
        {
            get
            {
                return _PAPointMatOut2;
            }
        }

        #endregion

        #region Properties, Range: 600
        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Position 1'}de{'Position 1'}", "", false, false, RemotePropID = 36)]
        public IACContainerTNet<Boolean> Pos1 { get; set; }

        [ACPropertyBindingTarget(631, "Read from PLC", "en{'Position 2'}de{'Position 2'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> Pos2 { get; set; }
        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'Position 1 request'}de{'Position 1 Anforderung'}", "", false, false, RemotePropID = 38)]
        public IACContainerTNet<Boolean> ReqPos1 { get; set; }

        [ACPropertyBindingTarget(652, "Write to PLC", "en{'Position 2 request'}de{'Position 2 Anforderung'}", "", false, false, RemotePropID = 39)]
        public IACContainerTNet<Boolean> ReqPos2 { get; set; }
        #endregion
        #endregion


        #region Methods, Range: 600

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Position2":
                    Position2();
                    return true;
                case "Position1":
                    Position1();
                    return true;
                case Const.IsEnabledPrefix + "Position2":
                    result = IsEnabledPosition2();
                    return true;
                case Const.IsEnabledPrefix + "Position1":
                    result = IsEnabledPosition1();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Position 2'}de{'Stellung 2'}", 600, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position2()
        {
            if (!IsEnabledPosition2())
                return;
            if (!PreExecute("Position2"))
                return;
            OnSetPosition2Values();
            PostExecute("Position2");
        }

        public virtual bool IsEnabledPosition2()
        {
            if (TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos2.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition2Values()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = true;
        }

        [ACMethodInteraction("", "en{'Position 1'}de{'Stellung 1'}", 601, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position1()
        {
            if (!IsEnabledPosition1())
                return;
            if (!PreExecute("Position1"))
                return;
            OnSetPosition1Values();
            PostExecute("Position1");
        }

        public virtual bool IsEnabledPosition1()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos1.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition1Values()
        {
            ReqPos1.ValueT = true;
            ReqPos2.ValueT = false;
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
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator2way(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuatorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
