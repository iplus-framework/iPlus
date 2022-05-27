using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.ComponentModel;

namespace gip.core.processapplication
{
    /// <summary>
    /// Motors with one direction of rotation
    /// Motoren mit einer Drehrichtung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor 1 direction'}de{'Motor 1 Drehrichtung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEEMotor1D : PAEEMotorStartCtrl
    {
        #region c'tors

        static PAEEMotor1D()
        {
            RegisterExecuteHandler(typeof(PAEEMotor1D), HandleExecuteACMethod_PAEEMotor1D);
        }

        public PAEEMotor1D(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn1 = new PAPoint(this, nameof(PAPointMatIn1));
            _PAPointMatOut1 = new PAPoint(this, nameof(PAPointMatOut1));
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

        PAPoint _PAPointMatOut1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("RunState", true, RunStateGroupNameConst, "en{'running'}de{'Läuft'}", Global.Operators.and)]
        public PAPoint PAPointMatOut1
        {
            get
            {
                return _PAPointMatOut1;
            }
        }
        #endregion

        #region Methods, Range: 800

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOn":
                    TurnOn();
                    return true;
                case Const.IsEnabledPrefix + "TurnOn":
                    result = IsEnabledTurnOn();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'turn on'}de{'Einschalten'}", 800, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOn()
        {
            if (!IsEnabledTurnOn())
                return;
            if (!PreExecute("TurnOn"))
                return;
            ReqRunState.ValueT = true;
            PostExecute("TurnOn");
        }

        public virtual bool IsEnabledTurnOn()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (RunState.ValueT)
                    return false;
                return true;
            }
            return false;
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotor1D(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorStartCtrl(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
