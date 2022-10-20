using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.Threading;
using System.ComponentModel;

namespace gip.core.autocomponent
{

    /// <summary>
    /// Baseclass for ACComponents, which has an alarming behaviour
    /// Basisklasse für ACComponents, die Alarme melden
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Simulator generic'}de{'Simulator allgemein'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class PAClassSimulator : ACComponent
    {
        #region c'tors
        static PAClassSimulator()
        {
            RegisterExecuteHandler(typeof(PAClassSimulator), HandleExecuteACMethod_PAClassSimulator);
        }

        public PAClassSimulator(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            ApplicationManager objectManager = FindParentComponent<ApplicationManager>(c => c is ApplicationManager);
            if (objectManager != null)
            {
                _AppManager = new ACRef<ApplicationManager>(objectManager, this);
                (Root as ACRoot).OnSendPropertyValueEvent += PAClassSimulator_OnSendPropertyValueEvent;
                objectManager.ProjectWorkCycleR1sec += objectManager_ProjectWorkCycle;
            }
            (RunSimulation as IACPropertyNetServer).ValueUpdatedOnReceival += PAClassSimulator_ValueUpdatedOnReceival;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_AppManager != null)
            {
                (Root as ACRoot).OnSendPropertyValueEvent -= PAClassSimulator_OnSendPropertyValueEvent;
                _AppManager.ValueT.ProjectWorkCycleR1sec -= objectManager_ProjectWorkCycle;
                _AppManager = null;
            }
            (RunSimulation as IACPropertyNetServer).ValueUpdatedOnReceival -= PAClassSimulator_ValueUpdatedOnReceival;
            return base.ACDeInit(deleteACClassTask);
        }

        private ACRef<ApplicationManager> _AppManager = null;
        protected ApplicationManager AppManager
        {
            get
            {
                if (_AppManager == null)
                    return null;
                return _AppManager.ValueT;
            }
        }

        #endregion

        #region Methods
        protected virtual void objectManager_ProjectWorkCycle(object sender, EventArgs e)
        {
            if (_AppManager == null)
                return;
            if (RunSimulation.ValueT)
            {
                _SimCycle++;
                ACUrlCommand("!RunScriptCyclic", new object[] { });
            }

            using (ACMonitor.Lock(_20250_LockSimItems))
            {
                if (_SimItems != null)
                {
                    foreach (SimulationItem simItem in _SimItems.ToList())
                    {
                        if (simItem.ReqProperty == null)
                            continue;
                        if (simItem.ReqProperty.ACIdentifier.StartsWith("Req"))
                        {
                            if (simItem.ReqProperty.ACType.ObjectType == simItem.Property.ACType.ObjectType)
                            {
                                simItem.Property.Value = simItem.ReqProperty.Value;
                                OnSimulateItem(simItem);
                            }
                        }
                        else if (simItem.ReqProperty.ACIdentifier.EndsWith("ACK"))
                        {
                            if (simItem.ReqProperty.ACType.ObjectType == typeof(bool) && simItem.Property.ACType.ObjectType == typeof(PANotifyState))
                            {
                                if ((Boolean)simItem.ReqProperty.Value == true)
                                {
                                    simItem.Property.Value = PANotifyState.Off;
                                    OnSimulateItem(simItem);
                                }
                            }
                        }
                    }
                }
                _SimItems = null;
            }
            if (RunSimulation.ValueT)
            {
                ACUrlCommand("!RunPostScriptCyclic", new object[] { });
            }
        }

        protected virtual void PAClassSimulator_OnSendPropertyValueEvent(object sender, ACPropertyNetSendEventArgs e)
        {
            if (!RunSimulation.ValueT || _AppManager == null)
                return;
            ACComponent component = e.ForACComponent as ACComponent;
            if (component == null)
                return;
            var root1 = component.FindParentComponent<ApplicationManager>(c => c is ApplicationManager);
            if (_AppManager.ValueT != root1)
                return;

            // TODO: Property zum ein und Ausschalten
            if (e.NetValueEventArgs.ACIdentifier.StartsWith("Req"))
            {
                string propertyNameResponse = e.NetValueEventArgs.ACIdentifier.Substring(3);
                IACPropertyBase propertyResponse = (e.ForACComponent as ACComponent).GetProperty(propertyNameResponse);
                if (propertyResponse != null)
                {
                    SimulationItem simItem = new SimulationItem()
                    {
                        Component = e.ForACComponent,
                        Property = propertyResponse,
                        ReqProperty = (e.ForACComponent as ACComponent).GetProperty(e.NetValueEventArgs.ACIdentifier)
                    };
                    // Trage in Liste für Simulation ein

                    using (ACMonitor.Lock(_20250_LockSimItems))
                    {
                        SimItems.Add(simItem);
                    }
                }
            }
            else if (e.NetValueEventArgs.ACIdentifier.EndsWith("ACK"))
            {
                int index = e.NetValueEventArgs.ACIdentifier.LastIndexOf("ACK");
                string propertyNameResponse = e.NetValueEventArgs.ACIdentifier.Substring(0, index);
                IACPropertyBase propertyResponse = (e.ForACComponent as ACComponent).GetProperty(propertyNameResponse);
                if (propertyResponse != null)
                {
                    SimulationItem simItem = new SimulationItem()
                    {
                        Component = e.ForACComponent,
                        Property = propertyResponse,
                        ReqProperty = (e.ForACComponent as ACComponent).GetProperty(e.NetValueEventArgs.ACIdentifier)
                    };
                    // Trage in Liste für Simulation ein

                    using (ACMonitor.Lock(_20250_LockSimItems))
                    {
                        SimItems.Add(simItem);
                    }
                }
            }
        }

#region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SimulationOn":
                    SimulationOn();
                    return true;
                case "SimulationOff":
                    SimulationOff();
                    return true;
                case "ManualSimulation":
                    ManualSimulation();
                    return true;
                case "AutoSimulation":
                    AutoSimulation();
                    return true;
                case "RunScriptCyclic":
                    RunScriptCyclic();
                    return true;
                case "RunPostScriptCyclic":
                    RunPostScriptCyclic();
                    return true;
                case Const.IsEnabledPrefix + "SimulationOn":
                    result = IsEnabledSimulationOn();
                    return true;
                case Const.IsEnabledPrefix + "SimulationOff":
                    result = IsEnabledSimulationOff();
                    return true;
                case Const.IsEnabledPrefix + "ManualSimulation":
                    result = IsEnabledManualSimulation();
                    return true;
                case Const.IsEnabledPrefix + "AutoSimulation":
                    result = IsEnabledAutoSimulation();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PAClassSimulator(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case Const.AskUserPrefix + "ManualSimulation":
                    result = AskUserManualSimulation(acComponent);
                    return true;
                case Const.AskUserPrefix + "AutoSimulation":
                    result = AskUserAutoSimulation(acComponent);
                    return true;
            }
            return false;
        }

        #endregion

        [ACMethodInteraction("", "en{'Simulation on'}de{'Simulation ein'}", 200, true)]
        public virtual void SimulationOn()
        {
            if (!IsEnabledSimulationOn())
                return;
            RunSimulation.ValueT = true;
        }

        public virtual bool IsEnabledSimulationOn()
        {
            if (RunSimulation.ValueT)
                return false;
            return true;
        }

        [ACMethodInteraction("", "en{'Simulation off'}de{'Simulation aus'}", 202, true)]
        public virtual void SimulationOff()
        {
            if (!IsEnabledSimulationOff())
                return;
            RunSimulation.ValueT = false;
        }

        public virtual bool IsEnabledSimulationOff()
        {
            return !IsEnabledSimulationOn();
        }

        [ACMethodInteraction("", "en{'Manual simulation'}de{'Manuelle Simulation'}", 203, true)]
        public virtual void ManualSimulation()
        {
            if (!IsEnabledManualSimulation())
                return;
            ManualSimulationMode.ValueT = true;
        }

        public virtual bool IsEnabledManualSimulation()
        {
            if (!IsEnabledSimulationOff())
                return false;
            return !ManualSimulationMode.ValueT;
        }

        public static bool AskUserManualSimulation(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            // Do you want to switch to simulation mode?
            return acComponent.Messages.Question(acComponent, "Question50041", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }


        [ACMethodInteraction("", "en{'Automatic simulation'}de{'Automatische Simulation'}", 204, true)]
        public virtual void AutoSimulation()
        {
            if (!IsEnabledAutoSimulation())
                return;
            ManualSimulationMode.ValueT = false;
        }

        public virtual bool IsEnabledAutoSimulation()
        {
            if (!IsEnabledSimulationOff())
                return false;
            return !IsEnabledManualSimulation();
        }

        public static bool AskUserAutoSimulation(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            // Do you want to switch to simulation mode?
            return acComponent.Messages.Question(acComponent, "Question50041", Global.MsgResult.Yes) == Global.MsgResult.Yes;
        }


        [ACMethodInfo("Function", "en{'RunScript'}de{'RunScript'}", 9999)]
        public virtual void RunScriptCyclic()
        {
        }

        [ACMethodInfo("Function", "en{'Run Post-Script'}de{'Run Post-Script'}", 9999)]
        public virtual void RunPostScriptCyclic()
        {
        }

        protected virtual void OnSimulateItem(SimulationItem simItem)
        {
        }

#endregion


        [ACPropertyBindingSource(200, "", "en{'Simulation on'}de{'Simulation eingeschaltet'}", "", true, true, DefaultValue = false)]
        public IACContainerTNet<Boolean> RunSimulation { get; set; }

        private void PAClassSimulator_ValueUpdatedOnReceival(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (!RunSimulation.ValueT)
                _SimCycle = 0;
        }


        [ACPropertyBindingSource(201, "", "en{'Manual simulation'}de{'Manuelle Simulation'}", "", true, true, DefaultValue = false)]
        public IACContainerTNet<Boolean> ManualSimulationMode { get; set; }


        private int _SimCycle = 0;
        public int SimCycle
        {
            get
            {
                return _SimCycle;
            }
        }

        public readonly ACMonitorObject _20250_LockSimItems = new ACMonitorObject(20250);
        private List<SimulationItem> _SimItems;
        public List<SimulationItem> SimItems
        {
            get
            {
                if (_SimItems == null)
                    _SimItems = new List<SimulationItem>();
                return _SimItems;
            }
        }

        public class SimulationItem
        {
            public IACComponent Component
            {
                get;
                set;
            }

            public IACPropertyBase Property
            {
                get;
                set;
            }

            public IACPropertyBase ReqProperty
            {
                get;
                set;
            }
        }

    }
}
