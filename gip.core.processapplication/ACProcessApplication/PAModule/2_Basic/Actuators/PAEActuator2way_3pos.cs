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
    /// Two-Way-Actuator with basic position
    /// Zwei-Wege-Stellglied mit Grundstellung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'2-Way Actuator with basic position'}de{'2-Wege Stellglied mit Grundstellung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator2way_3pos : PAEActuator2way
    {
        #region c'tors

        static PAEActuator2way_3pos()
        {
            RegisterExecuteHandler(typeof(PAEActuator2way_3pos), HandleExecuteACMethod_PAEActuator2way_3pos);
        }

        public PAEActuator2way_3pos(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        #region Methods, Range: 700

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Close":
                    Close();
                    return true;
                case Const.IsEnabledPrefix + "Close":
                    result = IsEnabledClose();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Close'}de{'Schliessen'}", 700, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Close()
        {
            if (!IsEnabledClose())
                return;
            if (!PreExecute("Close"))
                return;
            OnSetCloseValues();
            PostExecute("Close");
        }

        public bool IsEnabledClose()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (Pos1.ValueT || Pos2.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetCloseValues()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
        }


        protected override void GoToBasicPosition()
        {
            Close();
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

        public static bool HandleExecuteACMethod_PAEActuator2way_3pos(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
