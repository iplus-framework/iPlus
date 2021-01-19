using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'On/Off-Module'}de{'Ein/Aus-Modul'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEOnOffModule : PAEDriveBase
    {
        #region c'tors
        static PAEOnOffModule()
        {
            RegisterExecuteHandler(typeof(PAEOnOffModule), HandleExecuteACMethod_PAEOnOffModule);
        }

        public PAEOnOffModule(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        #region Properties, Range: 600

        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Is switched on'}de{'Eingeschaltet'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> IsSwitchedOn { get; set; }
        //public void OnSetIsSwitchedOn(IACPropertyNetValueEvent valueEvent)
        //{
        //    bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
        //    if (newValue != IsSwitchedOn.ValueT && this.Root.Initialized)
        //    {
        //    }
        //}

        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'Request to switch on'}de{'Anforderung Einschalten'}", "", false, false, RemotePropID = 38)]
        public IACContainerTNet<Boolean> ReqIsSwitchedOn { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 600

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SwitchOff":
                    SwitchOff();
                    return true;
                case Const.IsEnabledPrefix + "SwitchOff":
                    result = IsEnabledSwitchOff();
                    return true;
                case "SwitchOn":
                    SwitchOn();
                    return true;
                case Const.IsEnabledPrefix + "SwitchOn":
                    result = IsEnabledSwitchOn();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Switch off'}de{'Ausschalten'}", 600, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void SwitchOff()
        {
            if (!IsEnabledSwitchOff())
                return;
            if (!PreExecute("SwitchOff"))
                return;
            ReqIsSwitchedOn.ValueT = false;
            PostExecute("SwitchOff");
        }

        public virtual bool IsEnabledSwitchOff()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
                return true;
            return false;
        }

        [ACMethodInteraction("", "en{'Switch on'}de{'Einschalten'}", 60, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void SwitchOn()
        {
            if (!IsEnabledSwitchOn())
                return;
            if (!PreExecute("SwitchOn"))
                return;
            ReqIsSwitchedOn.ValueT = true;
            PostExecute("SwitchOn");
        }

        public virtual bool IsEnabledSwitchOn()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (IsSwitchedOn.ValueT)
                    return false;
                return true;
            }
            return false;
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
            ReqIsSwitchedOn.ValueT = false;
        }

        public override void SwitchToMaintenance()
        {
            if (!IsEnabledSwitchToMaintenance())
                return;
            SwitchOff();
            base.SwitchToMaintenance();
        }

        public override bool IsEnabledSwitchToMaintenance()
        {
            return base.IsEnabledSwitchToMaintenance();
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEOnOffModule(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
