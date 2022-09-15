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
    [ACSerializeableInfo]
    public enum PAEMotorRunState : short
    {
        Off = 0, // Off, Bitmask: 000
        On_Right = 1, // On, Or Right/Slow Bitmask: 100
        On_Left = 3,  // On, Or Left/Slow  Bitmask: 110
        On_Right_Fast = 5, // On,  Right and Fast Bitmask: 101
        On_Left_Fast = 7 // On,  Right and Fast Bitmask: 111
    }

    /// <summary>
    /// Baseclass for AC-, DC- and Inductionmotors
    /// Basisklasse für Wechselstrom-, Gleichstrom- und Drehstrommotoren
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'electric motor'}de{'Elektromotor'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEEMotorBase : PAEDriveBase
    {
        #region c'tors

        static PAEEMotorBase()
        {
            RegisterExecuteHandler(typeof(PAEEMotorBase), HandleExecuteACMethod_PAEEMotorBase);
        }

        public PAEEMotorBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            //if (RunState != null)
            //    (RunState as IACPropertyNetServer).ValueUpdatedOnReceival += PAEEMotorBase_ValueUpdatedOnReceival;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_RefContactor != null)
            {
                _RefContactor.Detach();
                _RefContactor = null;
            }

            if (_RefThermistor != null)
            {
                _RefThermistor.Detach();
                _RefThermistor = null;
            }

            if (_RefAmmeter != null)
            {
                _RefAmmeter.Detach();
                _RefAmmeter = null;
            }

            //if (RunState != null)
            //    (RunState as IACPropertyNetServer).ValueUpdatedOnReceival -= PAEEMotorBase_ValueUpdatedOnReceival;

            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 600

        #region Optional Members
        private ACRef<PAEContactor> _RefContactor = null;
        public PAEContactor Contactor
        {
            get
            {
                if (_RefContactor == null)
                {
                    PAEContactor result = FindMemberACComponent(typeof(PAEContactor)) as PAEContactor;
                    if (result != null)
                        _RefContactor = new ACRef<PAEContactor>(result, this);
                }
                if (_RefContactor != null)
                    return _RefContactor.ValueT;
                return null;
            }
        }

        private ACRef<PAEThermistor> _RefThermistor = null;
        public PAEThermistor Thermistor
        {
            get
            {
                if (_RefThermistor == null)
                {
                    PAEThermistor result = FindMemberACComponent(typeof(PAEThermistor)) as PAEThermistor;
                    if (result != null)
                        _RefThermistor = new ACRef<PAEThermistor>(result, this);
                }
                if (_RefThermistor != null)
                    return _RefThermistor.ValueT;
                return null;
            }
        }

        private ACRef<PAEAmmeter> _RefAmmeter = null;
        public PAEAmmeter Ammeter
        {
            get
            {
                if (_RefAmmeter == null)
                {
                    PAEAmmeter result = FindMemberACComponent(typeof(PAEAmmeter)) as PAEAmmeter;
                    if (result != null)
                        _RefAmmeter = new ACRef<PAEAmmeter>(result, this);
                }
                if (_RefAmmeter != null)
                    return _RefAmmeter.ValueT;
                return null;
            }
        }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'is running'}de{'Läuft/Eingeschaltet'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> RunState { get; set; }
        public void OnSetRunState(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != RunState.ValueT && this.Root.Initialized)
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

        #region Write-Value to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'request to run'}de{'Anforderung ein'}", "", false, false, RemotePropID = 38)]
        public IACContainerTNet<Boolean> ReqRunState { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 600

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOff":
                    TurnOff();
                    return true;
                case Const.IsEnabledPrefix + "TurnOff":
                    result = IsEnabledTurnOff();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if (!base.IsEnabledAcknowledgeAlarms())
                return false;
            return true;
        }


        [ACMethodInteraction("", "en{'turn off'}de{'Ausschalten'}", 600, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOff()
        {
            if (!IsEnabledTurnOff())
                return;
            if (!PreExecute("TurnOff"))
                return;
            ReqRunState.ValueT = false;
            PostExecute("TurnOff");
        }

        public virtual bool IsEnabledTurnOff()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!RunState.ValueT)
                    //return false;
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
            ReqRunState.ValueT = false;
        }

        public override void SwitchToMaintenance()
        {
            if (!IsEnabledSwitchToMaintenance())
                return;
            TurnOff();
            base.SwitchToMaintenance();
        }

        public override bool IsEnabledSwitchToMaintenance()
        {
            return base.IsEnabledSwitchToMaintenance();
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotorBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEDriveBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region Misc

        public const string RunStateGroupNameConst = "en{'Run state'}de{'Laufzustand'}";

        #endregion
    }

}
