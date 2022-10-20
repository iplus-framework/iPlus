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
    /// Class for analog sensors
    /// Klasse für analoge Sensoren
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Analogue sensor'}de{'Analoger Sensor'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAESensorAnalog : PAESensorBase
    {
        #region c'tors

        static PAESensorAnalog()
        {
            RegisterExecuteHandler(typeof(PAESensorAnalog), HandleExecuteACMethod_PAESensorAnalog);
        }

        public PAESensorAnalog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            StateUL1.PropertyChanged += StateUL1_PropertyChanged;
            StateUL2.PropertyChanged += StateUL2_PropertyChanged;
            StateLL1.PropertyChanged += StateLL1_PropertyChanged;
            StateLL2.PropertyChanged += StateLL2_PropertyChanged;
            ActualValue.PropertyChanged += ActualValue_PropertyChanged;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StateUL1.PropertyChanged -= StateUL1_PropertyChanged;
            StateUL2.PropertyChanged -= StateUL2_PropertyChanged;
            StateLL1.PropertyChanged -= StateLL1_PropertyChanged;
            StateLL2.PropertyChanged -= StateLL2_PropertyChanged;
            ActualValue.PropertyChanged -= ActualValue_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties Range 500

        #region Configuration

        [ACPropertyBindingTarget(500, "Configuration", "en{'Upper limit 1'}de{'Oberer Grenzwert 1'}", "", true, true, RemotePropID = 23)]
        public IACContainerTNet<Double> UpperLimit1 { get; set; }

        [ACPropertyBindingTarget(501, "Configuration", "en{'Role of UL1'}de{'Funktion OG1'}", "", true, true, DefaultValue = PAESensorRole.Indicator, RemotePropID = 24)]
        public IACContainerTNet<PAESensorRole> UpperLimit1Role { get; set; }

        [ACPropertyBindingTarget(502, "Configuration", "en{'Faulttime UL1'}de{'Störzeit OG1'}", "", true, true, RemotePropID = 25)]
        public IACContainerTNet<TimeSpan> FaultDelayTimeUL1 { get; set; }

        [ACPropertyBindingTarget(503, "Configuration", "en{'Upper limit 2'}de{'Oberer Grenzwert 2'}", "", true, true, RemotePropID = 26)]
        public IACContainerTNet<Double> UpperLimit2 { get; set; }

        [ACPropertyBindingTarget(504, "Configuration", "en{'Role of UL2'}de{'Funktion OG2'}", "", true, true, DefaultValue = PAESensorRole.FaultSensor, RemotePropID = 27)]
        public IACContainerTNet<PAESensorRole> UpperLimit2Role { get; set; }

        [ACPropertyBindingTarget(505, "Configuration", "en{'Faulttime UL2'}de{'Störzeit OG2'}", "", true, true, RemotePropID = 28)]
        public IACContainerTNet<TimeSpan> FaultDelayTimeUL2 { get; set; }
        

        [ACPropertyBindingTarget(506, "Configuration", "en{'Lower limit 1'}de{'Unterer Grenzwert 1'}", "", true, true, RemotePropID = 29)]
        public IACContainerTNet<Double> LowerLimit1 { get; set; }

        [ACPropertyBindingTarget(507, "Configuration", "en{'Role of LL1'}de{'Funktion UG1'}", "", true, true, DefaultValue = PAESensorRole.Indicator, RemotePropID = 30)]
        public IACContainerTNet<PAESensorRole> LowerLimit1Role { get; set; }

        [ACPropertyBindingTarget(508, "Configuration", "en{'Faulttime LL1'}de{'Störzeit UG1'}", "", true, true, RemotePropID = 31)]
        public IACContainerTNet<TimeSpan> FaultDelayTimeLL1 { get; set; }


        [ACPropertyBindingTarget(509, "Configuration", "en{'Lower limit 2'}de{'Unterer Grenzwert 2'}", "", true, true, RemotePropID = 32)]
        public IACContainerTNet<Double> LowerLimit2 { get; set; }

        [ACPropertyBindingTarget(510, "Configuration", "en{'Role of LL2'}de{'Funktion UG2'}", "", true, true, DefaultValue = PAESensorRole.FaultSensor, RemotePropID = 33)]
        public IACContainerTNet<PAESensorRole> LowerLimit2Role { get; set; }

        [ACPropertyBindingTarget(511, "Configuration", "en{'Faulttime LL2'}de{'Störzeit UG2'}", "", true, true, RemotePropID = 34)]
        public IACContainerTNet<TimeSpan> FaultDelayTimeLL2 { get; set; }


        [ACPropertyBindingTarget(512, "Configuration", "en{'Upper value'}de{'Oberer Messwert'}", "", true, true, RemotePropID = 35)]
        public IACContainerTNet<Double> UpperScaleValue { get; set; }

        [ACPropertyBindingTarget(513, "Configuration", "en{'Lower value'}de{'Unterer Messwert'}", "", true, true, RemotePropID = 36)]
        public IACContainerTNet<Double> LowerScaleValue { get; set; }

        [ACPropertyBindingTarget(514, "Configuration", "en{'Offset actual value'}de{'Offset auf Istwert'}", "", true, true, RemotePropID = 37)]
        public IACContainerTNet<Double> ValueOffset { get; set; }


        [ACPropertyBindingTarget(520, "Configuration", "en{'Force signal'}de{'Erzwinge Signal'}", "", true, true, RemotePropID = 38)]
        public IACContainerTNet<bool> ForceSensorValue { get; set; }

        [ACPropertyBindingTarget(521, "Configuration", "en{'Simulated value'}de{'Simulierter wert'}", "", true, true, RemotePropID = 39)]
        public IACContainerTNet<Double> ForcedSensorValue { get; set; }

        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(500, "Read from PLC", "en{'Actual value'}de{'Istwert'}", "", false, false, RemotePropID = 40)]
        public IACContainerTNet<Double> ActualValue { get; set; }
        protected virtual void ActualValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if ((Math.Abs(LowerLimit2.ValueT - 0) > Double.Epsilon)
            //    && (Math.Abs(LowerLimit1.ValueT - 0) > Double.Epsilon)
            //    && (Math.Abs(UpperLimit1.ValueT - 0) > Double.Epsilon)
            //    && (Math.Abs(UpperLimit2.ValueT - 0) > Double.Epsilon))
            if (Math.Abs(LowerLimit2.ValueT - 0) > Double.Epsilon)
            {
                if (ActualValue.ValueT <= LowerLimit2.ValueT)
                {
                    if (LowerLimit2Role.ValueT == PAESensorRole.FaultSensor)
                        StateLL2.ValueT = PANotifyState.AlarmOrFault;
                    else if (LowerLimit2Role.ValueT == PAESensorRole.Indicator)
                        StateLL2.ValueT = PANotifyState.InfoOrActive;
                }
                else
                    StateLL2.ValueT = PANotifyState.Off;
            }
            if (Math.Abs(LowerLimit1.ValueT - 0) > Double.Epsilon)
            {
                if (ActualValue.ValueT <= LowerLimit1.ValueT)
                {
                    if (LowerLimit1Role.ValueT == PAESensorRole.FaultSensor)
                        StateLL1.ValueT = PANotifyState.AlarmOrFault;
                    else if (LowerLimit1Role.ValueT == PAESensorRole.Indicator)
                        StateLL1.ValueT = PANotifyState.InfoOrActive;
                }
                else
                    StateLL1.ValueT = PANotifyState.Off;
            }
            if (Math.Abs(UpperLimit1.ValueT - 0) > Double.Epsilon)
            {
                if (ActualValue.ValueT >= UpperLimit1.ValueT)
                {
                    if (UpperLimit1Role.ValueT == PAESensorRole.FaultSensor)
                        StateUL1.ValueT = PANotifyState.AlarmOrFault;
                    else if (UpperLimit1Role.ValueT == PAESensorRole.Indicator)
                        StateUL1.ValueT = PANotifyState.InfoOrActive;
                }
                else
                    StateUL1.ValueT = PANotifyState.Off;
            }
            if (Math.Abs(UpperLimit2.ValueT - 0) > Double.Epsilon)
            {
                if (ActualValue.ValueT >= UpperLimit2.ValueT)
                {
                    if (UpperLimit2Role.ValueT == PAESensorRole.FaultSensor)
                        StateUL2.ValueT = PANotifyState.AlarmOrFault;
                    else if (UpperLimit2Role.ValueT == PAESensorRole.Indicator)
                        StateUL2.ValueT = PANotifyState.InfoOrActive;
                }
                else
                    StateUL2.ValueT = PANotifyState.Off;
            }
        }

        [ACPropertyBindingTarget(501, "Read from PLC", "en{'State of UL1'}de{'Status OG1'}", "", false, false, RemotePropID = 41)]
        public IACContainerTNet<PANotifyState> StateUL1 { get; set; }
        public void OnSetStateUL1(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckUL1.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckUL1.ValueT = false;
            if (newSensorState != StateUL1.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateUL1AlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateUL1AlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateUL1AlarmChanged = PAAlarmChangeState.NoChange;
        void StateUL1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateUL1AlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                if (_StateUL1AlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateUL1);
                else
                    OnAlarmDisappeared(StateUL1);
                _StateUL1AlarmChanged = PAAlarmChangeState.NoChange;
            }
        }


        [ACPropertyBindingTarget(502, "Read from PLC", "en{'State of UL2'}de{'Status OG2'}", "", false, false, RemotePropID = 42)]
        public IACContainerTNet<PANotifyState> StateUL2 { get; set; }
        public void OnSetStateUL2(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckUL2.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckUL2.ValueT = false;
            if (newSensorState != StateUL2.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateUL2AlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateUL2AlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateUL2AlarmChanged = PAAlarmChangeState.NoChange;
        void StateUL2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateUL2AlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                if (_StateUL2AlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateUL2);
                else
                    OnAlarmDisappeared(StateUL2);
                _StateUL2AlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        [ACPropertyBindingTarget(203, "Read from PLC", "en{'State of LL1'}de{'Status UG1'}", "", false, false, RemotePropID = 43)]
        public IACContainerTNet<PANotifyState> StateLL1 { get; set; }
        public void OnSetStateLL1(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckLL1.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckLL1.ValueT = false;
            if (newSensorState != StateLL1.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateLL1AlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateLL1AlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateLL1AlarmChanged = PAAlarmChangeState.NoChange;
        void StateLL1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateLL1AlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                if (_StateLL1AlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateLL1);
                else
                    OnAlarmDisappeared(StateLL1);
                _StateLL1AlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        [ACPropertyBindingTarget(504, "Read from PLC", "en{'State of LL2'}de{'Status UG2'}", "", false, false, RemotePropID = 44)]
        public IACContainerTNet<PANotifyState> StateLL2 { get; set; }
        public void OnSetStateLL2(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckLL2.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckLL2.ValueT = false;
            if (newSensorState != StateLL2.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateLL2AlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateLL2AlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateLL2AlarmChanged = PAAlarmChangeState.NoChange;
        void StateLL2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateLL2AlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(StateLL2);
                if (_StateLL2AlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateLL2);
                else
                    OnAlarmDisappeared(StateLL2);
                _StateLL2AlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(550, "Write to PLC", "en{'Fault acknowledge UL1'}de{'Störungsquittung OG1'}", "", true, false, RemotePropID = 45)]
        public IACContainerTNet<bool> FaultAckUL1 { get; set; }

        [ACPropertyBindingTarget(551, "Write to PLC", "en{'Fault acknowledge UL2'}de{'Störungsquittung OG2'}", "", true, false, RemotePropID = 46)]
        public IACContainerTNet<bool> FaultAckUL2 { get; set; }

        [ACPropertyBindingTarget(552, "Write to PLC", "en{'Fault acknowledge LL1'}de{'Störungsquittung UG1'}", "", true, false, RemotePropID = 47)]
        public IACContainerTNet<bool> FaultAckLL1 { get; set; }

        [ACPropertyBindingTarget(553, "Write to PLC", "en{'Fault acknowledge LL2'}de{'Störungsquittung UG2'}", "", true, false, RemotePropID = 48)]
        public IACContainerTNet<bool> FaultAckLL2 { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 500

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ForceValue":
                    ForceValue();
                    return true;
                case "ResetValue":
                    ResetValue();
                    return true;
                case Const.IsEnabledPrefix + "ForceValue":
                    result = IsEnabledForceValue();
                    return true;
                case Const.IsEnabledPrefix + "ResetValue":
                    result = IsEnabledResetValue();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (StateUL1.ValueT == PANotifyState.AlarmOrFault)
                FaultAckUL1.ValueT = true;
            if (StateUL2.ValueT == PANotifyState.AlarmOrFault)
                FaultAckUL2.ValueT = true;
            if (StateLL1.ValueT == PANotifyState.AlarmOrFault)
                FaultAckLL1.ValueT = true;
            if (StateLL2.ValueT == PANotifyState.AlarmOrFault)
                FaultAckLL2.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if (   ((StateUL1.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckUL1.ValueT))
                || ((StateUL2.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckUL2.ValueT))
                || ((StateLL1.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckLL1.ValueT))
                || ((StateLL2.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckLL2.ValueT)))
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }

        [ACMethodInteraction("", "en{'Force signal'}de{'Erzwinge Wert'}", 500, true)]
        public virtual void ForceValue()
        {
            if (!IsEnabledForceValue())
                return;
            ForceSensorValue.ValueT = true;
        }

        public virtual bool IsEnabledForceValue()
        {
            if ((OperatingMode.ValueT == Global.OperatingMode.Manual) && (ForceSensorValue.ValueT == false))
                return true;
            return false;
        }

        [ACMethodInteraction("", "en{'Reset signal'}de{'Rücksetze Wert'}", 501, true)]
        public virtual void ResetValue()
        {
            if (!IsEnabledResetValue())
                return;
            ForceSensorValue.ValueT = false;
        }

        public virtual bool IsEnabledResetValue()
        {
            if ((OperatingMode.ValueT == Global.OperatingMode.Manual) && (ForceSensorValue.ValueT == true))
                return true;
            return false;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAESensorAnalog(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
