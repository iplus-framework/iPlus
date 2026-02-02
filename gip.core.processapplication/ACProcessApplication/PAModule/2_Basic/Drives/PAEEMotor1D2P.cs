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
using System.ComponentModel;

namespace gip.core.processapplication
{
    /// <summary>
    /// Motors with one direction of rotation and 2 Poles (Speed)
    /// Motoren mit einer Drehrichtung und 2 Polen (Geschwindigkeiten)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor 1 dir., 2 poles'}de{'Motor 1 Dreh., 2 Pole'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEEMotor1D2P : PAEEMotorStartCtrl
    {
        #region c'tors

        static PAEEMotor1D2P()
        {
            RegisterExecuteHandler(typeof(PAEEMotor1D2P), HandleExecuteACMethod_PAEEMotor1D2P);
        }

        public PAEEMotor1D2P(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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
        [ACPointStateInfo("RunState", true, RunStateGroupNameConst, "en{'running'}de{'Läuft'}", Global.Operators.and)]
        public PAPoint PAPointMatOut1
        {
            get
            {
                return _PAPointMatOut1;
            }
        }
        #endregion

        #region Properties, Range 800

        #region Configuration
        [ACPropertyBindingTarget(801, "Configuration", "en{'transit time slow/fast'}de{'Umschaltzeit im Langsam/Schnell'}", "", true, true, RemotePropID = 50)]
        public IACContainerTNet<TimeSpan> TransitTimeSlowFast { get; set; }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(832, "Read from PLC", "en{'fast speed'}de{'Schnelle drehzahl'}", "", false, false, RemotePropID = 51)]
        public IACContainerTNet<Boolean> SpeedFast { get; set; }
        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(852, "Write to PLC", "en{'Request fast speed'}de{'Anforderung schnell'}", "", false, false, RemotePropID = 52)]
        public IACContainerTNet<Boolean> ReqSpeedFast { get; set; }
        #endregion


        #endregion

        #region Methods, Range: 800

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOnSlow":
                    TurnOnSlow();
                    return true;
                case "TurnOnFast":
                    TurnOnFast();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnSlow":
                    result = IsEnabledTurnOnSlow();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnFast":
                    result = IsEnabledTurnOnFast();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        [ACMethodInteraction("", "en{'turn on slow'}de{'Langsam ein'}", 800, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnSlow()
        {
            if (!IsEnabledTurnOnSlow())
                return;
            if (!PreExecute("TurnOnSlow"))
                return;
            ReqSpeedFast.ValueT = false;
            ReqRunState.ValueT = true;
            PostExecute("TurnOnSlow");
        }

        public virtual void OnTurnOnSlow()
        {
            ReqSpeedFast.ValueT = false;
            ReqRunState.ValueT = true;
        }

        public virtual bool IsEnabledTurnOnSlow()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                    return true;
                else if (!SpeedFast.ValueT)
                    return false;
                return true;
            }
            return false;
        }

        [ACMethodInteraction("", "en{'turn on fast'}de{'Schnell ein'}", 801, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnFast()
        {
            if (!IsEnabledTurnOnFast())
                return;
            if (!PreExecute("TurnOnFast"))
                return;
            OnTurnOnFast();
            PostExecute("TurnOnFast");
        }

        public virtual void OnTurnOnFast()
        {
            ReqSpeedFast.ValueT = true;
            ReqRunState.ValueT = true;
        }

        public virtual bool IsEnabledTurnOnFast()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (RunState.ValueT)
                {
                    if (SpeedFast.ValueT)
                        return false;
                    return true;
                }
                else if (TransitTimeSlowFast.ValueT > TimeSpan.Zero)
                    return true;
                return false;
            }
            return false;
        }

        public override void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
            if (!switchOff)
                OnTurnOnFast();
            else
                OnTurnOff();
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotor1D2P(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorStartCtrl(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
