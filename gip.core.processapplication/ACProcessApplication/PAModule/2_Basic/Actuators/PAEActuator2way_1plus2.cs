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
    /// Two-Way-Actuator with both positions simultaneous
    /// Two-Wege-Stellglied mit 2 Positionen gleichzeitig
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'2-Way Actuator simultaneous'}de{'2-Wege Stellglied gleichzeitig'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator2way_1plus2 : PAEActuator2way
    {
        #region c'tors

        static PAEActuator2way_1plus2()
        {
            RegisterExecuteHandler(typeof(PAEActuator2way_1plus2), HandleExecuteACMethod_PAEActuator2way_1plus2);
        }

        public PAEActuator2way_1plus2(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
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
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Pos3", true, "Position", "en{'Pos3'}de{'Pos3'}", Global.Operators.or)]
        public override PAPoint PAPointMatOut1
        {
            get
            {
                return base.PAPointMatOut1;
            }
        }

        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Pos3", true, "Position", "en{'Pos3'}de{'Pos3'}", Global.Operators.or)]
        public override PAPoint PAPointMatOut2
        {
            get
            {
                return base.PAPointMatOut2;
            }
        }
        #endregion

        #region Properties, Range: 700
        #region Read-Values from PLC
        [ACPropertyBindingTarget(730, "Read from PLC", "en{'Position 1+2'}de{'Stellung 1+2'}", "", false, false, RemotePropID = 43)]
        public IACContainerTNet<Boolean> Pos3 { get; set; }
        public void OnSetPos3(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != Pos3.ValueT && this.Root.Initialized)
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
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'Position 3 request'}de{'Position 3 Anforderung'}", "", false, false, RemotePropID = 44)]
        public IACContainerTNet<Boolean> ReqPos3 { get; set; }
        #endregion
        #endregion

        #region Methods, Range: 700

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Position1And2":
                    Position1And2();
                    return true;
                case Const.IsEnabledPrefix + "Position1And2":
                    result = IsEnabledPosition1And2();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Position 1+2'}de{'Stellung 1+2'}", 700, true, "", Global.ACKinds.MSMethodPrePost)]
        public void Position1And2()
        {
            if (!IsEnabledPosition1And2())
                return;
            if (!PreExecute("Position1And2"))
                return;
            OnSetPosition1And2Values();
            PostExecute("Position1And2");
        }

        public bool IsEnabledPosition1And2()
        {
            if (TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                //if (!Pos3.ValueT)
                //    return true;
                //return false;
                return true;
            }
            return false;
        }

        protected virtual void OnSetPosition1And2Values()
        {
            ReqPos1.ValueT = false;
            ReqPos2.ValueT = false;
            ReqPos3.ValueT = true;
        }

        protected override void OnSetPosition1Values()
        {
            base.OnSetPosition1Values();
            ReqPos3.ValueT = false;
        }

        protected override void OnSetPosition2Values()
        {
            base.OnSetPosition2Values();
            ReqPos3.ValueT = false;
        }


        protected override void GoToBasicPosition()
        {
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
            ReqPos3.ValueT = false;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator2way_1plus2(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator2way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
