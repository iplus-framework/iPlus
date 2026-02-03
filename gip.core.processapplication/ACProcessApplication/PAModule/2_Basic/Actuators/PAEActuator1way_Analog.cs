// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.processapplication
{
    /// <summary>
    /// One-Way-Actuator analog
    /// Ein-Wege-Stellglied analog
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'1-Way Actuator analog'}de{'1-Wege Stellglied Analog'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator1way_Analog : PAEActuator1way
    {
        #region c'tors

        static PAEActuator1way_Analog()
        {
            RegisterExecuteHandler(typeof(PAEActuator1way_Analog), HandleExecuteACMethod_PAEActuator1way_Analog);
        }

        public PAEActuator1way_Analog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range:700

        #region Read-Values from PLC
        [ACPropertyBindingTarget(730, "Read from PLC", "en{'Opening width'}de{'Öffnungsweite'}", "", false, false, RemotePropID = 41)]
        public IACContainerTNet<Double> OpeningWidth { get; set; }

        [ACPropertyBindingTarget(770, "Read from PLC", "en{'Desired width'}de{'Soll Öffnungsweite'}", "", false, false, RemotePropID = 45)]
        public IACContainerTNet<Double> DesiredWidth { get; set; }

        [ACPropertyBindingTarget(740, "Read from PLC", "en{'Stop Actuator'}de{'Stoppe Stellglied'}", "", false, false, RemotePropID = 43)]
        public IACContainerTNet<Boolean> StopAct { get; set; }

        public void OnSetActuatorStopped(IACPropertyNetValueEvent valueEvent)
        {

        }
        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'Opening width request'}de{'Öffnungsweite Anforderung'}", "", false, false, RemotePropID = 42)]
        public IACContainerTNet<Double> ReqOpeningWidth { get; set; }

        [ACPropertyBindingTarget(760, "Write to PLC", "en{'Stop Actuator Request'}de{'Stoppe Stellglied Anforderung'}", "", false, false, RemotePropID = 44)]
        public IACContainerTNet<Boolean> ReqStopAct { get; set; }

        #endregion
        #endregion

        #region Methods, Range: 701

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "OpenWidth":
                    OpenWidth((double)acParameter[0]);
                    return true;
                case "StopActuator":
                    StopActuator();
                    return true;
                case Const.IsEnabledPrefix + "StopActuator":
                    result = IsEnabledStopActuator();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("", "en{'Stop Actuator'}de{'Stoppe Stellglied'}", 810, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void StopActuator()
        {
            if (!IsEnabledStopActuator())
                return;
            if (!PreExecute("StopActuator"))
                return;
            OnSetStopActuatorValue();
            PostExecute("StopActuator");
        }

        public bool IsEnabledStopActuator()
        {
            return OperatingMode.ValueT == Global.OperatingMode.Manual && !StopAct.ValueT;
        }

        protected virtual void OnSetStopActuatorValue()
        {
            ReqStopAct.ValueT = true;
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
            ReqStopAct.ValueT = false;
        }


        [ACMethodInfo("", "en{'open width'}de{'Öffnen mit Weite'}", 801, false, Global.ACKinds.MSMethodPrePost)]
        public virtual void OpenWidth(double width)
        {
            if (!PreExecute("OpenWidth"))
                return;
            ReqOpeningWidth.ValueT = width;
            Open();
            PostExecute("OpenWidth");
        }

        public override bool IsEnabledOpen()
        {
            bool result = base.IsEnabledOpen();
            if (result)
            {
                if (ReqOpeningWidth.ValueT <= 0.0001)
                    return false;
            }
            return result;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEActuator1way_Analog(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
