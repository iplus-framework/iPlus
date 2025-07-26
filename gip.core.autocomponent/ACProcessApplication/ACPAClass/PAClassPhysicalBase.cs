// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for modules/elements/components which exists in the real word (physical)
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PAClassAlarmingBase" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAClassPhysicalBase'}de{'PAClassPhysicalBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Required, false, true,
        Description = "Abstract base class for representing physical components, devices, and equipment that exist in the real world within industrial automation systems. This class provides the foundational framework for managing physical assets such as sensors, actuators, motors, valves, pumps, and other field devices that interface with PLCs and control systems. It extends PAClassAlarmingBase to include alarm management capabilities and adds operating mode management with three standard modes: Automatic (device operates according to programmed sequences), Manual (operator can directly control the device), and Maintenance (device is prepared for service activities with safety measures). The class handles bidirectional communication with PLCs through OperatingMode (read from PLC) and ReqOperatingMode (write to PLC) properties, supports property logging for equipment analysis and OEE calculations, and provides context menu operations for switching between operating modes. Use this class as the base for any component that represents a tangible physical device in your automation system that requires operating mode management, alarm handling, and integration with plant control systems.")]
    public abstract class PAClassPhysicalBase : PAClassAlarmingBase
    {
        #region c'tors
        static PAClassPhysicalBase()
        {
            RegisterExecuteHandler(typeof(PAClassPhysicalBase), HandleExecuteACMethod_PAClassPhysicalBase);
        }

        public PAClassPhysicalBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            OperatingMode.PropertyChanged += OperatingMode_PropertyChanged;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            OperatingMode.PropertyChanged -= OperatingMode_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range 200

        #region Operating-Mode        
        /// <summary>
        /// Gets or sets the operating mode.
        /// </summary>
        /// <value>
        /// The operating mode.
        /// </value>
        [ACPropertyBindingTarget(230, "Read from PLC", "en{'Operating mode'}de{'Betriebsart'}", "", false, false, RemotePropID = 10)]
        public IACContainerTNet<Global.OperatingMode> OperatingMode { get; set; }
        #endregion

        #region Operating-Mode        
        /// <summary>
        /// Gets or sets the Request to change the operating mode.
        /// </summary>
        /// <value>
        /// The request operating mode.
        /// </value>
        [ACPropertyBindingTarget(250, "Write to PLC", "en{'Operating mode request'}de{'Betriebsart Anforderung'}", "", false, false, RemotePropID = 11)]
        public IACContainerTNet<Global.OperatingMode> ReqOperatingMode { get; set; }
        #endregion

        #endregion

        #region Methods, Range 200

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SwitchToAutomatic":
                    SwitchToAutomatic();
                    return true;
                case "SwitchToManual":
                    SwitchToManual();
                    return true;
                case "SwitchToMaintenance":
                    SwitchToMaintenance();
                    return true;
                case Const.IsEnabledPrefix + "SwitchToAutomatic":
                    result = IsEnabledSwitchToAutomatic();
                    return true;
                case Const.IsEnabledPrefix + "SwitchToManual":
                    result = IsEnabledSwitchToManual();
                    return true;
                case Const.IsEnabledPrefix + "SwitchToMaintenance":
                    result = IsEnabledSwitchToMaintenance();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Operating-Mode        
        /// <summary>
        /// Switches this physical object to automatic operation mode.
        /// </summary>
        [ACMethodInteraction("OperatingMode", "en{'Automatic mode'}de{'Automatikbetrieb'}", 200, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands, 
            Description = "Switches the physical component from manual to automatic operating mode. In automatic mode, the component operates according to programmed sequences and workflows without manual intervention. This method can only be called when the current operating mode is Manual.")]
        public virtual void SwitchToAutomatic()
        {
            if (!IsEnabledSwitchToAutomatic())
                return;
            ReqOperatingMode.ValueT = Global.OperatingMode.Automatic;
        }

        public virtual bool IsEnabledSwitchToAutomatic()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
                return true;
            return false;
        }


        /// <summary>
        /// Switches this physical object to manual operation mode.
        /// </summary>
        [ACMethodInteraction("OperatingMode", "en{'Manual mode'}de{'Handbetrieb'}", 201, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands,
            Description = "Switches the physical component to manual operating mode. In manual mode, operators can directly control the component through manual commands and interactions. This method can be called when the current operating mode is either Automatic or Maintenance.")]
        public virtual void SwitchToManual()
        {
            if (!IsEnabledSwitchToManual())
                return;
            ReqOperatingMode.ValueT = Global.OperatingMode.Manual;
        }

        public virtual bool IsEnabledSwitchToManual()
        {
            if (OperatingMode.ValueT != Global.OperatingMode.Manual)
                return true;
            return false;
        }


        /// <summary>
        /// Switches this physical object to maintenance mode.
        /// </summary>
        [ACMethodInteraction("OperatingMode", "en{'Maintenance mode'}de{'Wartungsbetrieb'}", 202, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands,
            Description = "Switches the physical component to maintenance operating mode. In maintenance mode, the component is prepared for service, repair, or calibration activities with appropriate safety measures activated. This method can only be called when the current operating mode is Manual. Switching to maintenance mode triggers alarm notifications.")]
        public virtual void SwitchToMaintenance()
        {
            if (!IsEnabledSwitchToMaintenance())
                return;
            ReqOperatingMode.ValueT = Global.OperatingMode.Maintenance;
        }

        public virtual bool IsEnabledSwitchToMaintenance()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
                return true;
            return false;
        }

        public void OnSetOperatingMode(IACPropertyNetValueEvent valueEvent)
        {
            Global.OperatingMode newOperatingMode = (valueEvent as ACPropertyValueEvent<Global.OperatingMode>).Value;
            if (newOperatingMode != OperatingMode.ValueT)
            {
                if (newOperatingMode == Global.OperatingMode.Maintenance)
                    _OperatingModeAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else
                    _OperatingModeAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
                OnOperatingModeChanged(OperatingMode.ValueT, newOperatingMode);
            }
        }

        protected virtual void OnOperatingModeChanged(Global.OperatingMode currentOperatingMode, Global.OperatingMode newOperatingMode)
        {
        }

        private PAAlarmChangeState _OperatingModeAlarmChanged = PAAlarmChangeState.NoChange;
        void OperatingMode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_OperatingModeAlarmChanged != PAAlarmChangeState.NoChange) && e.PropertyName == Const.ValueT)
            {
                if (_OperatingModeAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                {
                    var method = this.ACClassMethods.Where(c => c.ACIdentifier == "SwitchToMaintenance").FirstOrDefault();
                    OnNewAlarmOccurred(OperatingMode, method != null ? method.ACCaption : null);
                }
                else
                    OnAlarmDisappeared(OperatingMode);
                _OperatingModeAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }
        #endregion

        #endregion

        #region Methods => PropertiesLogging

        /// <summary>
        /// Activates the propertylogging for this instance
        /// </summary>
        /// <param name="acComponent">The ac component.</param>
        [ACMethodInteractionClient("", "en{'Activate properties logging'}de{'Eigenschaftsprotokollierung aktivieren'}", (short)MISort.ComponentExplorer - 100, true, "", false, Global.ContextMenuCategory.Utilities)]
        public static void ComponentPropertiesLoggingOn(IACComponent acComponent)
        {
            IACBSO bsoPropLogRules = GetBSOPropertyLogRules(acComponent as ACComponent);
            if (bsoPropLogRules == null)
            {
                acComponent.Messages.Info(acComponent, "Properties logging is not activated. Access to the BSOPropertyLogRules is denied.");
                return;
            }

            bsoPropLogRules.ExecuteMethod("ComponentPropertiesLoggingOn", acComponent.ComponentClass);
        }

        public static bool IsEnabledComponentPropertiesLoggingOn(IACComponent acComponent)
        {
            if (acComponent.ComponentClass == null)
                return false;

            if (!acComponent.ComponentClass.Properties.Any(c => c.ACClassPropertyRelation_TargetACClassProperty.Any(x => x.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState)))
                return false;

            IACBSO bsoPropLogRules = GetBSOPropertyLogRules(acComponent as ACComponent);
            if (bsoPropLogRules == null)
                return false;

            return (bool)bsoPropLogRules.ExecuteMethod("IsEnabledComponentPropertiesLoggingOn", acComponent.ComponentClass);
        }

        /// <summary>
        /// Dectivates the propertylogging for this instance
        /// </summary>
        /// <param name="acComponent">The ac component.</param>
        [ACMethodInteractionClient("", "en{'Deactivate properties logging'}de{'Deaktivieren der Eigenschaftsprotokollierung'}", (short)MISort.ComponentExplorer - 100, true, "", false, Global.ContextMenuCategory.Utilities)]
        public static void ComponentPropertiesLoggingOff(IACComponent acComponent)
        {
            IACBSO bsoPropLogRules = GetBSOPropertyLogRules(acComponent as ACComponent);
            if (bsoPropLogRules == null)
            {
                acComponent.Messages.Info(acComponent, "Properties logging is not deactivated. Access to the BSOPropertyLogRules is denied.");
                return;
            }

            bsoPropLogRules.ExecuteMethod("ComponentPropertiesLoggingOff", acComponent.ComponentClass);
        }

        public static bool IsEnabledComponentPropertiesLoggingOff(IACComponent acComponent)
        {
            if (acComponent.ComponentClass == null)
                return false;

            if (!acComponent.ComponentClass.Properties.Any(c => c.ACClassPropertyRelation_TargetACClassProperty.Any(x => x.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState)))
                return false;

            IACBSO bsoPropLogRules = GetBSOPropertyLogRules(acComponent as ACComponent);
            if (bsoPropLogRules == null)
                return false;

            return (bool)bsoPropLogRules.ExecuteMethod("IsEnabledComponentPropertiesLoggingOff", acComponent.ComponentClass);
        }

        private static IACBSO GetBSOPropertyLogRules(ACComponent acComponent)
        {
            if (acComponent == null || acComponent.Root == null)
                return null;

            IACBSO bsoPropLogRules = acComponent.Root.Businessobjects.GetChildComponent("BSOPropertyLogRules(1)") as IACBSO;
            if (bsoPropLogRules == null)
                bsoPropLogRules = acComponent.Root.Businessobjects.StartComponent("BSOPropertyLogRules", null, null) as IACBSO;

            return bsoPropLogRules;
        }

        /// <summary>
        /// Opens a window for analyzing the property log and OEE
        /// </summary>
        /// <param name="acComponent">The ac component.</param>
        [ACMethodInteractionClient("", "en{'Show the equipment analysis (OEE)'}de{'Anzeige der Geräteanalyse (OEE)'}", (short)MISort.ComponentExplorer - 101, true, "", false, Global.ContextMenuCategory.Utilities)]
        public static void ShowPropertyLogsDialog(IACComponent acComponent)
        {
            PAShowDlgManagerBase service = PAShowDlgManagerBase.GetServiceInstance(acComponent.Root as ACComponent);
            if (service != null)
                service.ShowPropertyLogViewer(acComponent);
        }

        public static bool IsEnabledShowPropertyLogsDialog(IACComponent acComponent)
        {
            //PAShowDlgManagerBase service = PAShowDlgManagerBase.GetServiceInstance(acComponent.Root as ACComponent);
            //if (service == null)
            //    return false;
            //return service.IsEnabledShowShowPropertyLogViewer(acComponent);
            return true;
        }


        #endregion

        #region Execute-helper-handlers

        public static bool HandleExecuteACMethod_PAClassPhysicalBase(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ComponentPropertiesLoggingOn":
                    ComponentPropertiesLoggingOn(acComponent);
                    return true;
                case Const.IsEnabledPrefix + "ComponentPropertiesLoggingOn":
                    result = IsEnabledComponentPropertiesLoggingOn(acComponent);
                    return true;
                case "ComponentPropertiesLoggingOff":
                    ComponentPropertiesLoggingOff(acComponent);
                    return true;
                case Const.IsEnabledPrefix + "ComponentPropertiesLoggingOff":
                    result = IsEnabledComponentPropertiesLoggingOff(acComponent);
                    return true;
                case "ShowPropertyLogsDialog":
                    ShowPropertyLogsDialog(acComponent);
                    return true;
                case Const.IsEnabledPrefix + "ShowPropertyLogsDialog":
                    result = IsEnabledShowPropertyLogsDialog(acComponent);
                    return true;
            }
            return false;
        }

        #endregion 

    }
}
