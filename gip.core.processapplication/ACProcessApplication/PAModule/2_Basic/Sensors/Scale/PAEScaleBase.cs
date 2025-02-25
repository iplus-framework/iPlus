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
    /// General scale 
    /// Waage allgemein
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scale general'}de{'Waage allgemein)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEScaleBase : PAESensorAnalog, IRoutableModule
    {
        #region c'tors

        static PAEScaleBase()
        {
            RegisterExecuteHandler(typeof(PAEScaleBase), HandleExecuteACMethod_PAEScaleBase);
        }

        public PAEScaleBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _RouteItemID = new ACPropertyConfigValue<string>(this, "RouteItemID", "0");
            _PAPointMatIn1 = new PAPoint(this, nameof(PAPointMatIn1));
            _PAPointMatOut1 = new PAPoint(this, nameof(PAPointMatOut1));
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            StateScale.PropertyChanged += StateScale_PropertyChanged;
            StateTolerance.PropertyChanged += StateTolerance_PropertyChanged;
            StateLackOfMaterial.PropertyChanged += StateLackOfMaterial_PropertyChanged;
            StateDosingTime.PropertyChanged += StateDosingTime_PropertyChanged;
            ActualWeight.PropertyChanged += ActualWeight_PropertyChanged;
            DesiredWeight.PropertyChanged += DesiredWeight_PropertyChanged;
            _ = RouteItemID;
            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StateScale.PropertyChanged -= StateScale_PropertyChanged;
            StateTolerance.PropertyChanged -= StateTolerance_PropertyChanged;
            StateLackOfMaterial.PropertyChanged -= StateLackOfMaterial_PropertyChanged;
            StateDosingTime.PropertyChanged -= StateDosingTime_PropertyChanged;
            ActualWeight.PropertyChanged -= ActualWeight_PropertyChanged;
            DesiredWeight.PropertyChanged -= DesiredWeight_PropertyChanged;
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
        public PAPoint PAPointMatOut1
        {
            get
            {
                return _PAPointMatOut1;
            }
        }
        #endregion

        #region Properties Range 600

        #region Configuration
        [ACPropertyBindingTarget(600, "Configuration", "en{'Max. dosingtime'}de{'Maximale Dosierzeit'}", "", true, true, RemotePropID = 52)]
        public IACContainerTNet<TimeSpan> MaxDosingTime { get; set; }

        [ACPropertyBindingTarget(601, "Configuration", "en{'Min. dosing weight [kg]'}de{'Minimale Dosiermenge [kg]'}", "", true, true, RemotePropID = 53)]
        public IACContainerTNet<Double> MinDosingWeight { get; set; }

        //TODO:Beckhoff TwinCat - add RemotePropID
        [ACPropertyBindingTarget(601, "Configuration", "en{'Max. scale weight [kg]'}de{'Max. Waagengewicht [kg]'}", "", true, true)]
        public IACContainerTNet<Double> MaxScaleWeight { get; set; }

        [ACPropertyBindingTarget(602, "Configuration", "en{'Registrationtime'}de{'Registrierzeit'}", "", true, true, RemotePropID = 54)]
        public IACContainerTNet<TimeSpan> RegistrationTime { get; set; }

        [ACPropertyBindingTarget(603, "Configuration", "en{'follow-on dosing impulse time'}de{'Nachdosierimpulszeit'}", "", true, true, RemotePropID = 55)]
        public IACContainerTNet<TimeSpan> FollowOnDosingImpulsTime { get; set; }

        [ACPropertyBindingTarget(604, "Configuration", "en{'Digit/Precision [g]'}de{'Teilung/Ziffernschritt [g]'}", "", true, true)]
        public IACContainerTNet<Double> DigitWeight { get; set; }
        // https://de.wikipedia.org/wiki/Genauigkeitsklasse_eines_Wiegesystems

        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Desired weight [kg]'}de{'Sollgewicht [kg]'}", "", false, false, RemotePropID = 56)]
        public IACContainerTNet<Double> DesiredWeight { get; set; }
        private void DesiredWeight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DifferenceWeight.ValueT = DesiredWeight.ValueT <= 0.00000001 ? 0.0 : ActualWeight.ValueT - DesiredWeight.ValueT;
        }

        [ACPropertyBindingTarget(631, "Read from PLC", "en{'Actual-/Netweight [kg]'}de{'Ist-/Nettogewicht [kg]'}", "", false, false, RemotePropID = 57)]
        public IACContainerTNet<Double> ActualWeight { get; set; }
        private void ActualWeight_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DifferenceWeight.ValueT = DesiredWeight.ValueT <= 0.00000001 ? 0.0 : ActualWeight.ValueT - DesiredWeight.ValueT; ;
        }

        #region Scale State
        [ACPropertyBindingTarget(634, "Read from PLC", "en{'Scale malfunction'}de{'Störung Waage'}", "", false, false, RemotePropID = 58)]
        public IACContainerTNet<PANotifyState> StateScale { get; set; }
        public void OnSetStateScale(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckScale.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckScale.ValueT = false;
            if (newSensorState != StateScale.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateScaleAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateScaleAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateScaleAlarmChanged = PAAlarmChangeState.NoChange;
        void StateScale_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateScaleAlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(StateScale);
                if (_StateScaleAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateScale);
                else
                    OnAlarmDisappeared(StateScale);
                _StateScaleAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        [ACPropertyBindingTarget(653, "Write to PLC", "en{'Fault acknowledge Scale'}de{'Störungsquittung Waage'}", "", true, false, RemotePropID = 59)]
        public IACContainerTNet<bool> FaultAckScale { get; set; }
        #endregion

        #region Tolerance State
        [ACPropertyBindingTarget(635, "Read from PLC", "en{'Out of Tolerance'}de{'Außerhalb Toleranz'}", "", false, false, RemotePropID = 60)]
        public IACContainerTNet<PANotifyState> StateTolerance { get; set; }
        public void OnSetStateTolerance(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckTolerance.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckTolerance.ValueT = false;
            if (newSensorState != StateTolerance.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateToleranceAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateToleranceAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateToleranceAlarmChanged = PAAlarmChangeState.NoChange;
        void StateTolerance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateToleranceAlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(StateTolerance);
                if (_StateToleranceAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateTolerance);
                else
                    OnAlarmDisappeared(StateTolerance);
                _StateToleranceAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        [ACPropertyBindingTarget(654, "Write to PLC", "en{'Fault acknowledge Tolerance'}de{'Toleranzquittung'}", "", true, false, RemotePropID = 61)]
        public IACContainerTNet<bool> FaultAckTolerance { get; set; }
        #endregion

        #region LackOfMaterial State
        [ACPropertyBindingTarget(636, "Read from PLC", "en{'Lack of material'}de{'Materialmangel'}", "", false, false, RemotePropID = 62)]
        public IACContainerTNet<PANotifyState> StateLackOfMaterial { get; set; }
        public void OnSetStateLackOfMaterial(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckLackOfMaterial.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckLackOfMaterial.ValueT = false;
            if (newSensorState != StateLackOfMaterial.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateLackOfMaterialAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateLackOfMaterialAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateLackOfMaterialAlarmChanged = PAAlarmChangeState.NoChange;
        void StateLackOfMaterial_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateLackOfMaterialAlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(StateLackOfMaterial);
                if (_StateLackOfMaterialAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateLackOfMaterial);
                else
                    OnAlarmDisappeared(StateLackOfMaterial);
                _StateLackOfMaterialAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        [ACPropertyBindingTarget(655, "Write to PLC", "en{'Fault acknowledge lack of material'}de{'Materialmangelquittung'}", "", true, false, RemotePropID = 63)]
        public IACContainerTNet<bool> FaultAckLackOfMaterial { get; set; }
        #endregion

        #region DosingTime State
        [ACPropertyBindingTarget(637, "Read from PLC", "en{'Dosing time exceeded'}de{'Dosierzeitüberschreitung'}", "", false, false, RemotePropID = 64)]
        public IACContainerTNet<PANotifyState> StateDosingTime { get; set; }
        public void OnSetStateDosingTime(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newSensorState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultAckDosingTime.ValueT && newSensorState != PANotifyState.AlarmOrFault)
                FaultAckDosingTime.ValueT = false;
            if (newSensorState != StateDosingTime.ValueT)
            {
                if (newSensorState == PANotifyState.AlarmOrFault)
                    _StateDosingTimeAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newSensorState == PANotifyState.Off)
                    _StateDosingTimeAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StateDosingTimeAlarmChanged = PAAlarmChangeState.NoChange;
        void StateDosingTime_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_StateDosingTimeAlarmChanged != PAAlarmChangeState.NoChange && e.PropertyName == Const.ValueT)
            {
                OnAlarmDisappeared(StateDosingTime);
                if (_StateDosingTimeAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StateDosingTime);
                else
                    OnAlarmDisappeared(StateDosingTime);
                _StateDosingTimeAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        [ACPropertyBindingTarget(656, "Write to PLC", "en{'Fault acknowledge dosingtime-fault'}de{'Dosierzeitfehlerquittung'}", "", true, false, RemotePropID = 65)]
        public IACContainerTNet<bool> FaultAckDosingTime { get; set; }
        #endregion

        [ACPropertyBindingTarget(638, "Read from PLC", "en{'Dosing'}de{'Dosiert'}", "", false, false, RemotePropID = 66)]
        public IACContainerTNet<Boolean> IsDosing { get; set; }

        [ACPropertyBindingTarget(639, "Read from PLC", "en{'Rough dosing'}de{'Grobdosierung'}", "", false, false, RemotePropID = 67)]
        public IACContainerTNet<Boolean> IsRough { get; set; }

        [ACPropertyBindingTarget(640, "Read from PLC", "en{'Fine dosing'}de{'Feindosierung'}", "", false, false, RemotePropID = 68)]
        public IACContainerTNet<Boolean> IsFine { get; set; }

        [ACPropertyBindingTarget]
        public IACContainerTNet<double> DifferenceWeight { get; set; }

        [ACPropertyBindingTarget(641, "Read from PLC", "en{'Not Standstill - Moving'}de{'Kein Stillstand - In Bewegung'}", "", false, false)]
        public IACContainerTNet<Boolean> NotStandStill { get; set; }


        #endregion

        #region Occupation
        private string _OccupiedFrom;
        [ACPropertyInfo(true, 640, "", "en{'Occupied from'}de{'Belegt von'}")]
        public string OccupiedFrom
        {
            get
            {
                return _OccupiedFrom;
            }
            set
            {
                _OccupiedFrom = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyBindingTarget(441, "Read from PLC", "en{'Allocated by Way'}de{'Belegt von Wegesteuerung'}", "", false, false)]
        public IACContainerTNet<BitAccessForAllocatedByWay> AllocatedByWay { get; set; }

        protected ACPropertyConfigValue<string> _RouteItemID;
        [ACPropertyConfig("en{'ID/Number in PLC'}de{'ID/Nummer in SPS'}")]
        public virtual string RouteItemID
        {
            get
            {
                return _RouteItemID.ValueT;
            }
            //set
            //{
            //    this["RouteItemID"] = value;
            //}
        }

        protected Nullable<int> _RouteItemIDAsNum;
        public virtual int RouteItemIDAsNum
        {
            get
            {
                if (_RouteItemIDAsNum.HasValue)
                    return _RouteItemIDAsNum.Value;
                _RouteItemIDAsNum = -1;
                if (!String.IsNullOrEmpty(RouteItemID))
                {
                    try { _RouteItemIDAsNum = System.Convert.ToInt32(RouteItemID); }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException(nameof(PAEScaleBase), "RouteItemIDAsNum", msg);
                    }
                }
                return _RouteItemIDAsNum.Value;
            }
        }


        public string IsScaleOccupied()
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                return _OccupiedFrom;
            }
        }

        public bool OccupyScale(string acUrlFunc, bool withCheck, bool release)
        {
            using (ACMonitor.Lock(_20015_LockValue))
            {
                if (!withCheck
                    || (String.IsNullOrEmpty(_OccupiedFrom) || _OccupiedFrom == acUrlFunc))
                {
                    _OccupiedFrom = release ? null : acUrlFunc;
                }
                else
                    return false;
            }
            OnPropertyChanged(nameof(OccupiedFrom));
            return true;
        }

        [ACMethodInteraction("", "en{'Release occupation'}de{'Reservierung entfernen'}", 700, true)]
        public void ReleaseScale()
        {
            OccupyScale(null, false, true);
        }

        #endregion

        #endregion

        #region Methods, Range: 600
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (StateScale.ValueT == PANotifyState.AlarmOrFault)
                FaultAckScale.ValueT = true;
            if (StateTolerance.ValueT == PANotifyState.AlarmOrFault)
                FaultAckTolerance.ValueT = true;
            if (StateLackOfMaterial.ValueT == PANotifyState.AlarmOrFault)
                FaultAckLackOfMaterial.ValueT = true;
            if (StateDosingTime.ValueT == PANotifyState.AlarmOrFault)
                FaultAckDosingTime.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if (((StateScale.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckScale.ValueT))
                || ((StateTolerance.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckTolerance.ValueT))
                || ((StateLackOfMaterial.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckLackOfMaterial.ValueT))
                || ((StateDosingTime.ValueT == PANotifyState.AlarmOrFault) && (!FaultAckDosingTime.ValueT)))
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }

        public double VerifyScaleTolerance(double desiredTolerance)
        {
            if (DigitWeight.ValueT >= 0.000001)
            {
                double digitWeight = DigitWeight.ValueT * 0.001;
                if (desiredTolerance < digitWeight)
                    return digitWeight;
            }

            return desiredTolerance;
        }

        #endregion

        #region Handle execute helpers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;

            switch (acMethodName)
            {
                case nameof(ReleaseScale):
                    ReleaseScale();
                    return true;
            }

            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PAEScaleBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAESensorAnalog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        public virtual void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
        }

        public virtual void SimulateAllocationState(RouteItem item, bool switchOff)
        {
            if (AllocatedByWay != null && AllocatedByWay.ValueT != null)
            {
                AllocatedByWay.ValueT.Bit00_Reserved = false;
                AllocatedByWay.ValueT.Bit01_Allocated = !switchOff;
            }
        }

        #endregion
    }

}
