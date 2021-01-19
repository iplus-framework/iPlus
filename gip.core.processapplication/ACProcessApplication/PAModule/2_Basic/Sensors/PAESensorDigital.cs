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
    /// Klasse for digital sensors
    /// klasse für digitale Sensoren
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Digital sensor'}de{'Digitaler Sensor'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAESensorDigital : PAESensorBase
    {
        #region c'tors

        static PAESensorDigital()
        {
            RegisterExecuteHandler(typeof(PAESensorDigital), HandleExecuteACMethod_PAESensorDigital);
        }

        public PAESensorDigital(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            SensorState.PropertyChanged += SensorState_PropertyChanged;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            SensorState.PropertyChanged -= SensorState_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 500

        #region Configuration
        [ACPropertyBindingTarget(500, "Configuration", "en{'Delaytime signal comes'}de{'Verzögerungszeit Signal kommt'}", "", true, true, RemotePropID=23)]
        public IACContainerTNet<TimeSpan> DelayTimeSignalOn { get; set; }

        [ACPropertyBindingTarget(501, "Configuration", "en{'Delaytime signal goes'}de{'Verzögerungszeit Signal geht'}", "", true, true, RemotePropID=24)]
        public IACContainerTNet<TimeSpan> DelayTimeSignalOff { get; set; }

        [ACPropertyBindingTarget(502, "Configuration", "en{'Delaytime for Fault'}de{'Verzögerungszeit Störmeldung'}", "", true, true, RemotePropID=25)]
        public IACContainerTNet<TimeSpan> FaultDelayTime { get; set; }

        [ACPropertyBindingTarget(503, "Configuration", "en{'Signal is active low'}de{'Signal ist 0-Aktiv'}", "", true, true, RemotePropID=26)]
        public IACContainerTNet<bool> ActiveLow { get; set; }

        [ACPropertyBindingTarget(504, "Configuration", "en{'Role of Sensor'}de{'Sensorfunktion'}", "", true, true, DefaultValue = PAESensorRole.Indicator, RemotePropID=27)]
        public IACContainerTNet<PAESensorRole> SensorRole { get; set; }

        [ACPropertyBindingTarget(505, "Configuration", "en{'Force signal'}de{'Erzwinge Signal'}", "", true, true, RemotePropID=28)]
        public IACContainerTNet<bool> ForceSensorSignal { get; set; }

        [ACPropertyBindingTarget(506, "Configuration", "en{'Collective Acknowledgement'}de{'Über Sammelquittung quittierbar'}", "", true, true, RemotePropID=29)]
        public IACContainerTNet<bool> CollectiveAck { get; set; }

        [ACPropertyBindingTarget(507, "Configuration", "en{'Ignore delay times'}de{'Verzögerungszeiten ignorieren'}", "", true, true, RemotePropID=30)]
        public IACContainerTNet<bool> IgnoreDelayTimes { get; set; }

        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(530, "Read from PLC", "en{'State of Sensor'}de{'Sensorstatus'}", "", false, false, RemotePropID=31)]
        [ACPointStateInfo("SensorState", PANotifyState.Off, "en{'State'}de{'Status'}", "", Global.Operators.none)]
        public IACContainerTNet<PANotifyState> SensorState { get; set; }

        public void OnSetSensorState(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            // Fehlerbehandlung: Falls Sensor ein Fault-Sensor ist und ein Boolscher wert gebunden ist, erhält der Sensor-State InfoOrActive
            if ((SensorRole.ValueT == PAESensorRole.FaultSensor) && (newSensorState == PANotifyState.InfoOrActive))
            {
                newSensorState = PANotifyState.AlarmOrFault;
                (valueEvent as ACPropertyValueEvent<PANotifyState>).Value = newSensorState;
            }

            if (FaultACK.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultACK.ValueT = false;
            if (newSensorState != SensorState.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _SensorAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _SensorAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _SensorAlarmChanged = PAAlarmChangeState.NoChange;
        void SensorState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_SensorAlarmChanged != PAAlarmChangeState.NoChange) && e.PropertyName == Const.ValueT)
            {
                if (_SensorAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(SensorState);
                else
                    OnAlarmDisappeared(SensorState);
                _SensorAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(550, "Write to PLC", "en{'Fault acknowledge'}de{'Störungsquittung'}", "", true, false, RemotePropID=32)]
        public IACContainerTNet<bool> FaultACK { get; set; }
        #endregion

        #endregion

        #region Points and Events
        #endregion

        #region Methods, Range: 500

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ForceSignal":
                    ForceSignal();
                    return true;
                case "ResetSignal":
                    ResetSignal();
                    return true;
                case Const.IsEnabledPrefix + "ForceSignal":
                    result = IsEnabledForceSignal();
                    return true;
                case Const.IsEnabledPrefix + "ResetSignal":
                    result = IsEnabledResetSignal();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            FaultACK.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if ((SensorState.ValueT == PANotifyState.AlarmOrFault) && (!FaultACK.ValueT))
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }

        [ACMethodInteraction("", "en{'Force signal'}de{'Erzwinge Signal'}", 500, true)]
        public virtual void ForceSignal()
        {
            if (!IsEnabledForceSignal())
                return;
            ForceSensorSignal.ValueT = true;
        }

        public virtual bool IsEnabledForceSignal()
        {
            if ((OperatingMode.ValueT == Global.OperatingMode.Manual) && (ForceSensorSignal.ValueT == false))
                return true;
            return false;
        }

        [ACMethodInteraction("", "en{'Reset signal'}de{'Rücksetze Signal'}", 501, true)]
        public virtual void ResetSignal()
        {
            if (!IsEnabledResetSignal())
                return;
            ForceSensorSignal.ValueT = false;
        }

        public virtual bool IsEnabledResetSignal()
        {
            if ((OperatingMode.ValueT == Global.OperatingMode.Manual) && (ForceSensorSignal.ValueT == true))
                return true;
            return false;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAESensorDigital(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
