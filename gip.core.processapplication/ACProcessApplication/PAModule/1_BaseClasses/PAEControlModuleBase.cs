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
    /// Baseclass for controllable modules/elements/components
    /// Basisklasse für steuerbare Bauteile/Elemente
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass Controlmodules'}de{'Basisklasse Steuerungsmodule'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEControlModuleBase : PAModule, IRoutableModule
    {
        public const string ClassName = "PAEControlModuleBase";

        #region c'tors

        static PAEControlModuleBase()
        {
            RegisterExecuteHandler(typeof(PAEControlModuleBase), HandleExecuteACMethod_PAEControlModuleBase);
        }

        public PAEControlModuleBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            FaultState.PropertyChanged += FaultState_PropertyChanged;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            FaultState.PropertyChanged -= FaultState_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 400
        #region Statistics
        [ACPropertyBindingTarget(400, "Statistics", "en{'Operating time'}de{'Betriebsdauer'}", "", true, true, RemotePropID = 12)]
        public IACContainerTNet<TimeSpan> OperatingTime { get; set; }

        [ACPropertyBindingTarget(401, "Statistics", "en{'Switching frequency'}de{'Schalthäufigkeit'}", "", true, true, RemotePropID = 13)]
        public IACContainerTNet<Int32> SwitchingFrequency { get; set; }

        [ACPropertyBindingTarget(402, "Statistics", "en{'Total alarms'}de{'Anzahl Störungen'}", "", true, true, RemotePropID = 14)]
        public IACContainerTNet<Int32> TotalAlarms { get; set; }

        [ACPropertyBindingTarget(403, "Statistics", "en{'turn-on instant'}de{'Einschaltzeitpunkt'}", "", true, true, RemotePropID = 15)]
        public IACContainerTNet<DateTime> TurnOnInstant { get; set; }

        [ACPropertyBindingTarget(404, "Statistics", "en{'turn-off instant'}de{'Ausschaltzeitpunkt'}", "", true, true, RemotePropID = 16)]
        public IACContainerTNet<DateTime> TurnOffInstant { get; set; }

        [ACPropertyBindingTarget(405, "Statistics", "en{'on-time'}de{'Einschaltdauer'}", "", true, false, RemotePropID = 17)]
        public IACContainerTNet<TimeSpan> OnTime { get; set; }
        #endregion

        #region Configuration
        [ACPropertyBindingTarget(420, "Configuration", "en{'turn-on delay'}de{'Einschaltverzögerung'}", "", true, true, RemotePropID = 18)]
        public IACContainerTNet<TimeSpan> TurnOnDelay { get; set; }

        [ACPropertyBindingTarget(421, "Configuration", "en{'turn-off delay'}de{'Ausschaltverzögerung'}", "", true, true, RemotePropID = 19)]
        public IACContainerTNet<TimeSpan> TurnOffDelay { get; set; }

        [ACPropertyBindingTarget(422, "Configuration", "en{'Fault delaytime'}de{'Störzeit'}", "", true, true, RemotePropID = 20)]
        public IACContainerTNet<TimeSpan> FaultDelayTime { get; set; }

        [ACPropertyBindingTarget(423, "Configuration", "en{'turn-on interlock delay'}de{'Wiedereinschaltsperrzeit'}", "", true, true, RemotePropID = 21)]
        public IACContainerTNet<TimeSpan> TurnOnInterlockDelay { get; set; }

        [ACPropertyBindingTarget(424, "Configuration", "en{'Depleting time'}de{'Leerfahrzeit'}", "", false, false, RemotePropID = 22)]
        public IACContainerTNet<TimeSpan> DepletingTime { get; set; }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(440, "Read from PLC", "en{'General Fault'}de{'Allgemeine Störung'}", "", false, false, RemotePropID = 23)]
        public IACContainerTNet<PANotifyState> FaultState { get; set; }

        public void OnSetFaultState(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newFaultState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (FaultStateACK.ValueT && newFaultState != PANotifyState.AlarmOrFault)
                FaultStateACK.ValueT = false;
            if (newFaultState != FaultState.ValueT)
            {
                if (newFaultState == PANotifyState.AlarmOrFault)
                    _FaultAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newFaultState == PANotifyState.Off)
                    _FaultAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _FaultAlarmChanged = PAAlarmChangeState.NoChange;
        void FaultState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_FaultAlarmChanged != PAAlarmChangeState.NoChange) && e.PropertyName == Const.ValueT)
            {
                if (_FaultAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(FaultState);
                else
                    OnAlarmDisappeared(FaultState);
                _FaultAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        {
            base.OnNewMsgAlarmLogCreated(newLog);
            TotalAlarms.ValueT++;
        }


        [ACPropertyBindingTarget(441, "Read from PLC", "en{'Allocated by Way'}de{'Belegt von Wegesteuerung'}", "", false, false, RemotePropID = 24)]
        public IACContainerTNet<BitAccessForAllocatedByWay> AllocatedByWay { get; set; }

        [ACPropertyBindingTarget(442, "Read from PLC", "en{'on site turned on'}de{'Vorort eingeschaltet'}", "", false, false, RemotePropID = 25)]
        public IACContainerTNet<ushort> OnSiteTurnedOn { get; set; }

        [ACPropertyBindingTarget(443, "Read from PLC", "en{'turn-on interlock'}de{'Einschaltverriegelung'}", "", false, false, RemotePropID = 26)]
        public IACContainerTNet<Boolean> TurnOnInterlock { get; set; }

        [ACPropertyBindingTarget(444, "Read from PLC", "en{'running time'}de{'Laufzeit'}", "", false, false, RemotePropID = 27)]
        public IACContainerTNet<TimeSpan> RunningTime { get; set; }

        [ACPropertyBindingTarget(445, "Read from PLC", "en{'is triggered'}de{'Angesteuert'}", "", false, false, RemotePropID = 28)]
        public IACContainerTNet<Boolean> IsTriggered { get; set; }

        public string RouteItemID
        {
            get
            {
                IRouteItemIDProvider idProvider = FindChildComponents<IRouteItemIDProvider>(c => c is IRouteItemIDProvider).FirstOrDefault();
                if (idProvider != null)
                    return idProvider.RouteItemID;
                return null;
            }
        }

        public int RouteItemIDAsNum
        {
            get
            {
                IRouteItemIDProvider idProvider = FindChildComponents<IRouteItemIDProvider>(c => c is IRouteItemIDProvider).FirstOrDefault();
                if (idProvider != null)
                    return idProvider.RouteItemIDAsNum;
                return 0;
            }
        }
        // TODO: Grund der Verriegelung (GIP-Spezifisch)
        // TODO: Phase (GIP-Spezifisch)
        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(450, "Write to PLC", "en{'Fault acknowledge'}de{'Störungsquittung'}", "", true, false, RemotePropID = 29)]
        public IACContainerTNet<bool> FaultStateACK { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 400
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            FaultStateACK.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if (FaultState.ValueT == PANotifyState.AlarmOrFault)
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }

        public abstract void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff);

        #region Simulation
        public static void ActivateRouteOnSimulation(Route route, bool switchOff)
        {
            if (route == null || route.Items == null)
                return;
            foreach (RouteItem routeItem in route.Items)
            {
                IRoutableModule module = routeItem.SourceACComponent as IRoutableModule;
                module?.SimulateAllocationState(routeItem, switchOff);
                module?.ActivateRouteItemOnSimulation(routeItem, switchOff);
                module = routeItem.TargetACComponent as IRoutableModule;
                module?.SimulateAllocationState(routeItem, switchOff);
                module?.ActivateRouteItemOnSimulation(routeItem, switchOff);
            }
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
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEControlModuleBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
