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
    /// One-Way-Actuator
    /// Ein-Wege-Stellglied: Ein-Wege-Ventil, Klappe, Schieber
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'1-Way Actuator'}de{'1-Wege Stellglied'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator1way : PAEActuatorBase
    {
        #region c'tors

        static PAEActuator1way()
        {
            RegisterExecuteHandler(typeof(PAEActuator1way), HandleExecuteACMethod_PAEActuator1way);
        }

        public PAEActuator1way(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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
        [ACPointStateInfo("Pos1Open", true, "Position", "en{'Open'}de{'Offen'}", Global.Operators.and)]
        public PAPoint PAPointMatOut1
        {
            get
            {
                return _PAPointMatOut1;
            }
        }
        #endregion

        #region Properties, Range: 600
        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Open'}de{'Offen'}", "", false, false, RemotePropID = 36)]
        public IACContainerTNet<Boolean> Pos1Open { get; set; }
        public void OnSetPos1Open(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != Pos1Open.ValueT && this.Root.Initialized)
            {
                if (newValue)
                {
                    TurnOnInstant.ValueT = DateTime.Now;
                    SwitchingFrequency.ValueT++;
                }
                else
                {
                    TurnOffInstant.ValueT = DateTime.Now;
                    if (TurnOnInstant.ValueT > DateTime.MinValue && TurnOnInstant.ValueT < TurnOffInstant.ValueT)
                        OperatingTime.ValueT += TurnOffInstant.ValueT - TurnOnInstant.ValueT;
                }
            }
        }

        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'open request'}de{'Öffnen Anforderung'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> ReqPos1Open { get; set; }
        #endregion
        #endregion

        #region Methods, Range: 600

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Open":
                    Open();
                    return true;
                case "Close":
                    Close();
                    return true;
                case Const.IsEnabledPrefix + "Open":
                    result = IsEnabledOpen();
                    return true;
                case Const.IsEnabledPrefix + "Close":
                    result = IsEnabledClose();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Open'}de{'Öffnen'}", 600, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Open()
        {
            if (!IsEnabledOpen())
                return;
            if (!PreExecute("Open"))
                return;
            OnSetOpenValues();
            PostExecute("Open");
        }

        public virtual bool IsEnabledOpen()
        {
            if (TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!Pos1Open.ValueT)
                    return true;
                return false;
            }
            return false;
        }

        protected virtual void OnSetOpenValues()
        {
            if (Behaviour == 1)
                ReqPos1Open.ValueT = false;
            else
                ReqPos1Open.ValueT = true;
        }


        [ACMethodInteraction("", "en{'Close'}de{'Schliessen'}", 601, true, "", Global.ACKinds.MSMethodPrePost)]
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
                //if (Pos1Open.ValueT)
                    //return true;
                return true;
            }
            return false;
        }

        protected virtual void OnSetCloseValues()
        {
            if (Behaviour == 1)
                ReqPos1Open.ValueT = true;
            else
                ReqPos1Open.ValueT = false;
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
            ReqPos1Open.ValueT = false;
        }

        public override void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
            if (switchOff)
                OnSetCloseValues();
            else
                OnSetOpenValues();
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator1way(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuatorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion
    }

}
