// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    /// <summary>
    /// One-Way-Actuator with 3 Positions (Closed, Mid, Open)
    /// Ein-Wege-Stellglied mit drei Stellungen (Dosierschieber)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'1-Way Actuator 3 pos.'}de{'1-Wege Stellglied mit 3 Pos.'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator1way_3pos : PAEActuatorBase
    {
        #region c'tors

        static PAEActuator1way_3pos()
        {
            RegisterExecuteHandler(typeof(PAEActuator1way_3pos), HandleExecuteACMethod_PAEActuator1way_3pos);
        }

        public PAEActuator1way_3pos(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
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
        [ACPointStateInfo("Pos1", true, "Position", "en{'Pos1'}de{'Pos1'}", Global.Operators.or)]
        [ACPointStateInfo("Pos2", true, "Position", "en{'Pos2'}de{'Pos2'}", Global.Operators.or)]
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
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Position 1'}de{'Position 1'}", "", false, false, RemotePropID = 36)]
        public IACContainerTNet<Boolean> Pos1 { get; set; }
        public void OnSetPos1(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != Pos1.ValueT && this.Root.Initialized)
            {
                if (newValue)
                {
                    SwitchingFrequency.ValueT++;
                    if (TurnOnInstant.ValueT > DateTime.MinValue && DateTime.Now > TurnOnInstant.ValueT)
                        OperatingTime.ValueT += DateTime.Now - TurnOnInstant.ValueT;
                    TurnOnInstant.ValueT = DateTime.Now;
                }
            }
        }

        [ACPropertyBindingTarget(631, "Read from PLC", "en{'Position 2'}de{'Position 2'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> Pos2 { get; set; }
        public void OnSetPos2(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != Pos2.ValueT && this.Root.Initialized)
            {
                if (newValue)
                {
                    SwitchingFrequency.ValueT++;
                    if (TurnOnInstant.ValueT > DateTime.MinValue && DateTime.Now > TurnOnInstant.ValueT)
                        OperatingTime.ValueT += DateTime.Now - TurnOnInstant.ValueT;
                    TurnOnInstant.ValueT = DateTime.Now;
                }
            }
        }
        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'Position 1 request'}de{'Position 1 Anforderung'}", "", false, false, RemotePropID = 38)]
        public IACContainerTNet<Boolean> ReqPos1 { get; set; }

        [ACPropertyBindingTarget(652, "Write to PLC", "en{'Position 2 request'}de{'Position 2 Anforderung'}", "", false, false, RemotePropID = 39)]
        public IACContainerTNet<Boolean> ReqPos2 { get; set; }
        #endregion
        #endregion


        #region Methods, Range: 600
        
        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Position2":
                    Position2();
                    return true;
                case "Position1":
                    Position1();
                    return true;
                case "Close":
                    Close();
                    return true;
                case Const.IsEnabledPrefix + "Position2":
                    result = IsEnabledPosition2();
                    return true;
                case Const.IsEnabledPrefix + "Position1":
                    result = IsEnabledPosition1();
                    return true;
                case Const.IsEnabledPrefix + "Close":
                    result = IsEnabledClose();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Position 2'}de{'Stellung 2'}", 600, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position2()
        {
            if (!IsEnabledPosition2())
                return;
            if (!PreExecute("Position2"))
                return;
            OnSetPosition2Values();
            PostExecute("Position2");
        }

        public virtual bool IsEnabledPosition2()
        {
            if (TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos2.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition2Values()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = true;
        }

        [ACMethodInteraction("", "en{'Position 1'}de{'Stellung 1'}", 601, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position1()
        {
            if (!IsEnabledPosition1())
                return;
            if (!PreExecute("Position1"))
                return;
            OnSetPosition1Values();
            PostExecute("Position1");
        }

        public virtual bool IsEnabledPosition1()
        {
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos1.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition1Values()
        {
            ReqPos1.ValueT = true;
            ReqPos2.ValueT = false;
        }


        [ACMethodInteraction("", "en{'Close'}de{'Schliessen'}", 700, true, "", Global.ACKinds.MSMethodPrePost)]
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
                //if (Pos1.ValueT || Pos2.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetCloseValues()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
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
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
        }

        public override void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
            if (switchOff)
                OnSetCloseValues();
            else
                OnSetPosition1Values();
        }


        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator1way_3pos(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuatorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
