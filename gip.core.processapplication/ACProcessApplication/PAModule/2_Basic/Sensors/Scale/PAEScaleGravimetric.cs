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
    /// Gravimetrically Scale (Gross-weight is PAESensorAnalog.ActualValue)
    /// Gravimetrische Waage (Bruttogewicht ist PAESensorAnalog.ActualValue)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scale gravimetric'}de{'Waage gravimetrisch)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleGravimetric : PAEScaleBase
    {
        #region c'tors

        static PAEScaleGravimetric()
        {
            RegisterExecuteHandler(typeof(PAEScaleGravimetric), HandleExecuteACMethod_PAEScaleGravimetric);
        }

        public PAEScaleGravimetric(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _TareInternal = new ACPropertyConfigValue<bool>(this, "TareInternal", false);
            _TolerancePlus = new ACPropertyConfigValue<double>(this, "TolerancePlus", 0.0);
            _ToleranceMinus = new ACPropertyConfigValue<double>(this, "ToleranceMinus", 0.0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            bool tare = _TareInternal.ValueT;
            double tol = _TolerancePlus.ValueT;
            tol = _ToleranceMinus.ValueT;

            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();

            var target = ActualWeightExternal as IACPropertyNetTarget;
            if (target != null && target.Source != null)
                IsVisibleExtActualWeight.ValueT = true;
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            UnSubscribeStandStillDetection();
            return result;
        }

        #endregion

        #region Properties Range 700

        #region Configuration
        [ACPropertyBindingTarget(700, "Configuration", "en{'Calmingtime'}de{'Beruhigungszeit'}", "", true, true, RemotePropID = 72)]
        public IACContainerTNet<TimeSpan> CalmingTime { get; set; }

        [ACPropertyBindingTarget(701, "Configuration", "en{'Calmingweight'}de{'Beruhigungsgewicht '}", "", true, true, RemotePropID = 73)]
        public IACContainerTNet<double> CalmingWeight { get; set; }

        private ACPropertyConfigValue<bool> _TareInternal;
        [ACPropertyConfig("en{'Tare internal'}de{'Internes Tarieren'}")]
        public bool TareInternal
        {
            get
            {
                return _TareInternal.ValueT;
            }
            set
            {
                _TareInternal.ValueT = value;
            }
        }

        private ACPropertyConfigValue<double> _TolerancePlus;
        [ACPropertyConfig("en{'Tolerance + [+=kg/-=%]l'}de{'Toleranz + [+=kg/-=%]'}")]
        public double TolerancePlus
        {
            get
            {
                return _TolerancePlus.ValueT;
            }
            set
            {
                _TolerancePlus.ValueT = value;
            }
        }

        private ACPropertyConfigValue<double> _ToleranceMinus;
        [ACPropertyConfig("en{'Tolerance - [+=kg/-=%]'}de{'Tolerance - [+=kg/-=%]'}")]
        public double ToleranceMinus
        {
            get
            {
                return _ToleranceMinus.ValueT;
            }
            set
            {
                _ToleranceMinus.ValueT = value;
            }
        }

        /// <summary>
        /// Internal netweight calculation takes place, wether TareInternal is set to true
        /// or no Source-Property was bound to the ActualWeight-Property
        /// </summary>
        /// <value>
        ///   <c>true</c> if is internal actual weight calculation; otherwise, <c>false</c>.
        /// </value>
        public bool IsInternalActualWeightCalculation
        {
            get
            {
                if (TareInternal)
                    return TareInternal;
                var weightAsTarget = ActualWeight as IACPropertyNetTarget;
                return weightAsTarget != null && weightAsTarget.Source == null;
            }
        }
        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'Set zero'}de{'Null setzen'}", "", false, false, RemotePropID = 74)]
        public IACContainerTNet<Boolean> ReqZeroSet { get; set; }

        [ACPropertyBindingTarget(751, "Write to PLC", "en{'Reset zero'}de{'Null rücksetzen'}", "", false, false, RemotePropID = 75)]
        public IACContainerTNet<Boolean> ReqZeroReset { get; set; }

        [ACPropertyBindingTarget(752, "Write to PLC", "en{'Tare'}de{'Tarieren'}", "", false, false, RemotePropID = 82)]
        public IACContainerTNet<Boolean> ReqIsTared { get; set; }

        [ACPropertyBindingTarget(753, "Write to PLC", "en{'Tarecounter Req.'}de{'Tarierzaehler Anf.'}", "", false, false, RemotePropID = 83)]
        public IACContainerTNet<Int32> TareCounterReq { get; set; }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(730, "Read from PLC", "en{'Tare weight'}de{'Tariertes gewicht'}", "", false, false, RemotePropID = 76)]
        public IACContainerTNet<Double> TareWeight { get; set; }

        [ACPropertyBindingSource(731, "Write", "en{'Atored tare weight'}de{'Gespeichertes Taragewicht'}", "", true, true, RemotePropID = 77)]
        public IACContainerTNet<Double> StoredTareWeight { get; set; }

        [ACPropertyBindingTarget(732, "Read from PLC", "en{'Is discharging'}de{'Entleert'}", "", false, false, RemotePropID = 78)]
        public IACContainerTNet<Boolean> IsDischarging { get; set; }

        [ACPropertyBindingTarget(733, "Read from PLC", "en{'Is tared'}de{'Tariert'}", "", false, true, RemotePropID = 79)]
        public IACContainerTNet<Boolean> IsTared { get; set; }

        [ACPropertyBindingTarget(734, "Read from PLC", "en{'Is set zero'}de{'Ist Nullgesetzt'}", "", false, false, RemotePropID = 80)]
        public IACContainerTNet<Boolean> ZeroSet { get; set; }

        [ACPropertyBindingTarget(735, "Read from PLC", "en{'Is manual dosing'}de{'Ist Handdosierung'}", "", false, false, RemotePropID = 81)]
        public IACContainerTNet<Boolean> IsManualDosing { get; set; }

        [ACPropertyBindingTarget(736, "Read from PLC", "en{'Actual-/Netweight [kg]'}de{'Ist-/Nettogewicht [kg]'}", "", false, false, RemotePropID = 84)]
        public IACContainerTNet<Double> ActualWeightExternal { get; set; }

        [ACPropertyBindingSource(703, "Config", "en{'External Weight is used'}de{'Zeige externes Gewicht an'}", "", false, false)]
        public IACContainerTNet<Boolean> IsVisibleExtActualWeight { get; set; }

        [ACPropertyBindingTarget(737, "Write to PLC", "en{'Tarecounter Res.'}de{'Tarierzaehler Antw.'}", "", false, false, RemotePropID = 85)]
        public IACContainerTNet<Int32> TareCounterRes { get; set; }

        [ACPropertyBindingSource(738, "Write", "en{'Last stored paosting weight'}de{'Zuletzt gespeichertes Buchungsgewicht'}", "", true, true)]
        public IACContainerTNet<Double> StoredWeightForPosting { get; set; }

        #endregion

        #endregion

        #region Methods, Range: 700
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SetZero":
                    SetZero();
                    return true;
                case "ResetZero":
                    ResetZero();
                    return true;
                case "TareOn":
                    TareOn();
                    return true;
                case "TareOff":
                    TareOff();
                    return true;
                case "Tare":
                    Tare();
                    return true;
                case Const.IsEnabledPrefix + "SetZero":
                    result = IsEnabledSetZero();
                    return true;
                case Const.IsEnabledPrefix + "ResetZero":
                    result = IsEnabledResetZero();
                    return true;
                case Const.IsEnabledPrefix + "TareOn":
                    result = IsEnabledTareOn();
                    return true;
                case Const.IsEnabledPrefix + "TareOff":
                    result = IsEnabledTareOff();
                    return true;
                case Const.IsEnabledPrefix + "Tare":
                    result = IsEnabledTare();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInteraction("", "en{'Set zero'}de{'Setzen Nullwert'}", 700, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void SetZero()
        {
            if (!IsEnabledSetZero())
                return;
            if (!PreExecute("SetZero"))
                return;
            ReqZeroSet.ValueT = true;
            ReqZeroReset.ValueT = false;
            PostExecute("SetZero");
        }

        public virtual bool IsEnabledSetZero()
        {
            //if ((OperatingMode.ValueT == Global.OperatingMode.Maintenance) || (OperatingMode.ValueT == Global.OperatingMode.Manual))
            //{
            //    if (ZeroSet.ValueT)
            //        return false;
            //    return true;
            //}
            //return false;
            return true;
        }

        [ACMethodInteraction("", "en{'Reset zero'}de{'Nullwert-Rücksetzen'}", 701, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void ResetZero()
        {
            if (!IsEnabledResetZero())
                return;
            if (!PreExecute("ResetZero"))
                return;
            ReqZeroSet.ValueT = false;
            ReqZeroReset.ValueT = true;
            PostExecute("ResetZero");
        }

        public virtual bool IsEnabledResetZero()
        {
            //if ((OperatingMode.ValueT == Global.OperatingMode.Maintenance) || (OperatingMode.ValueT == Global.OperatingMode.Manual))
            //{
            //    if (!ZeroSet.ValueT)
            //        return false;
            //    return true;
            //}
            //return false;
            return true;
        }

        [ACMethodInteraction("", "en{'Tare on'}de{'Tarieren ein'}", 702, true)]
        public virtual void TareOn()
        {
            if (!IsEnabledTareOn())
                return;
            ReqIsTared.ValueT = true;
            //TareScale(true, false);
        }

        public virtual bool IsEnabledTareOn()
        {
            return !IsTared.ValueT;
        }

        [ACMethodInteraction("", "en{'Tare off'}de{'Tarieren aus'}", 703, true)]
        public virtual void TareOff()
        {
            if (!IsEnabledTareOff())
                return;
            ReqIsTared.ValueT = false;
            //TareScale(false, false);
        }

        public virtual bool IsEnabledTareOff()
        {
            return IsTared.ValueT;
        }


        [ACMethodInteraction("", "en{'Tare'}de{'Tarieren'}", 704, true)]
        public virtual void Tare()
        {
            if (!IsEnabledTare())
                return;
            TareScale(true, false);
        }

        public virtual bool IsEnabledTare()
        {
            return true;
        }

        [ACPropertyInfo(790)]
        public bool IsTareRequestAcknowledged
        {
            get
            {
                return TareCounterReq.ValueT == TareCounterRes.ValueT;
            }
        }

        public virtual void TareScale(bool activateTaring, bool forceIfSimulation)
        {
            if (activateTaring)
            {
                if (TareCounterReq.ValueT >= Int16.MaxValue)
                    TareCounterReq.ValueT = 0;
                else
                    TareCounterReq.ValueT++;
                if (forceIfSimulation || IsInternalActualWeightCalculation)
                {
                    StoredTareWeight.ValueT = ActualValue.ValueT;
                    ActualWeight.ValueT = 0;
                    OnResetActualWeight();
                }
            }
            else
            {
                if (forceIfSimulation || IsInternalActualWeightCalculation)
                    TareCounterRes.ValueT = TareCounterReq.ValueT;
            }
        }

        protected virtual void OnResetActualWeight()
        {
        }

        public virtual void SimulateWeight(double increaseValue)
        {
            ActualValue.ValueT += increaseValue;
            if (!IsInternalActualWeightCalculation)
                RecalcActualWeight();
        }

        protected override void ActualValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ActualValue_PropertyChanged(sender, e);
            if (IsInternalActualWeightCalculation)
                RecalcActualWeight();
            HandleStandStill();
        }

        protected virtual void RecalcActualWeight()
        {
            if (ActualValue != null && StoredTareWeight != null)
                ActualWeight.ValueT = ActualValue.ValueT - StoredTareWeight.ValueT;
        }

        #region Stand-Still-Handling
        private ACMonitorObject _80000_StandStillLock = new ACMonitorObject(80000);
        private double _PrevActualValue = 0.0;
        private System.Diagnostics.Stopwatch _StandStillStopWatch = new System.Diagnostics.Stopwatch();
        protected virtual void HandleStandStill()
        {
            if ((NotStandStill as IACPropertyNetTarget).Source == null
                && CalmingTime.ValueT > TimeSpan.Zero
                && CalmingWeight.ValueT >= 0.000001)
            {
                double actualValue = ActualValue.ValueT;
                double calmingWeight = CalmingWeight.ValueT;
                bool notStandStill = false;
                using (ACMonitor.Lock(_80000_StandStillLock))
                {
                    if (Math.Abs(_PrevActualValue - actualValue) > calmingWeight)
                    {
                        if (_StandStillStopWatch.IsRunning)
                            _StandStillStopWatch.Restart();
                        else
                            _StandStillStopWatch.Start();
                        _PrevActualValue = actualValue;
                        notStandStill = true;
                    }
                }
                if (notStandStill)
                    NotStandStill.ValueT = notStandStill;
                SubscribeStandStillDetection();
            }
            else
            {
                UnSubscribeStandStillDetection();
            }
        }

        private bool _StandStillSubsc = false;
        private void SubscribeStandStillDetection()
        {
            if (InitState != ACInitState.Initialized)
                return;
            using (ACMonitor.Lock(_80000_StandStillLock))
            {
                if (_StandStillSubsc)
                    return;
                this.ApplicationManager.ProjectWorkCycleR200ms += StandStillSubsc_ProjectWorkCycleR200ms;
                _StandStillSubsc = true;
            }
        }

        private void UnSubscribeStandStillDetection()
        {
            using (ACMonitor.Lock(_80000_StandStillLock))
            {
                if (!_StandStillSubsc)
                    return;
                _StandStillSubsc = false;
                _StandStillStopWatch.Reset();
                this.ApplicationManager.ProjectWorkCycleR200ms -= StandStillSubsc_ProjectWorkCycleR200ms;
            }
        }

        private void StandStillSubsc_ProjectWorkCycleR200ms(object sender, EventArgs e)
        {
            TimeSpan calmingTime = CalmingTime.ValueT;
            bool resetStandStill = false;

            using (ACMonitor.Lock(_80000_StandStillLock))
            {
                if (_StandStillStopWatch.IsRunning && _StandStillStopWatch.Elapsed > calmingTime)
                {
                    resetStandStill = true;
                    _StandStillStopWatch.Reset();
                }
            }

            if (resetStandStill)
                NotStandStill.ValueT = false;
        }
        #endregion

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScaleGravimetric(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEScaleBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
