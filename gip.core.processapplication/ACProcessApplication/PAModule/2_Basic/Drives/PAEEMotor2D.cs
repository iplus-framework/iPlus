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
    /// Motors with two directions of rotation
    /// Motoren mit zwei Drehrichtungen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor 2 direction'}de{'Motor 2 Drehrichtung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEEMotor2D : PAEEMotorStartCtrl
    {
        #region c'tors

        static PAEEMotor2D()
        {
            RegisterExecuteHandler(typeof(PAEEMotor2D), HandleExecuteACMethod_PAEEMotor2D);
        }

        public PAEEMotor2D(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatInOut1 = new PAPoint(this, nameof(PAPointMatInOut1));
            _PAPointMatInOut2 = new PAPoint(this, nameof(PAPointMatInOut2));
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
        PAPoint _PAPointMatInOut1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("DirectionLeft", true, RunStateGroupNameConst, "en{'Left'}de{'Links'}", Global.Operators.and)]
        [ACPointStateInfo("RunState", true, RunStateGroupNameConst, "en{'Left'}de{'Links'}", Global.Operators.and)]
        public PAPoint PAPointMatInOut1
        {
            get
            {
                return _PAPointMatInOut1;
            }
        }

        PAPoint _PAPointMatInOut2;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("DirectionLeft", false, RunStateGroupNameConst, "en{'Right'}de{'Rechts'}", Global.Operators.and)]
        [ACPointStateInfo("RunState", true, RunStateGroupNameConst, "en{'Right'}de{'Rechts'}", Global.Operators.and)]
        public PAPoint PAPointMatInOut2
        {
            get
            {
                return _PAPointMatInOut2;
            }
        }
        #endregion

        #region Properties, Range 800

        #region Configuration
        [ACPropertyInfo(800, "Configuration", "en{'direct toggle allowed'}de{'Direktes umschalten erlaubt'}", "", true)]
        public bool ToggleAllowed
        {
            get;
            set;
        }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(831, "Read from PLC", "en{'left direction'}de{'Laufrichtung links'}", "", false, false, RemotePropID = 50)]
        public IACContainerTNet<Boolean> DirectionLeft { get; set; }
        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(851, "Write to PLC", "en{'request left direction'}de{'Anforderung links'}", "", false, false, RemotePropID = 51)]
        public IACContainerTNet<Boolean> ReqDirectionLeft { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 800

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOnLeft":
                    TurnOnLeft();
                    return true;
                case "TurnOnRight":
                    TurnOnRight();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnLeft":
                    result = IsEnabledTurnOnLeft();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnRight":
                    result = IsEnabledTurnOnRight();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        [ACMethodInteraction("", "en{'turn on left'}de{'Links ein'}", 800, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnLeft()
        {
            if (!IsEnabledTurnOnLeft())
                return;
            if (!PreExecute("TurnOnLeft"))
                return;
            OnTurnOnLeft();
            PostExecute("TurnOnLeft");
        }

        public virtual void OnTurnOnLeft()
        {
            ReqDirectionLeft.ValueT = true;
            ReqRunState.ValueT = true;
        }

        public virtual bool IsEnabledTurnOnLeft()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                    return true;
                else if (DirectionLeft.ValueT)
                    return false;
                else if (ToggleAllowed)
                    return true;
                return false;
            }
            return false;
        }


        [ACMethodInteraction("", "en{'turn on right'}de{'Rechts ein'}", 801, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnRight()
        {
            if (!IsEnabledTurnOnRight())
                return;
            if (!PreExecute("TurnOnRight"))
                return;
            OnTurnOnRight();
            PostExecute("TurnOnRight");
        }

        public virtual void OnTurnOnRight()
        {
            ReqDirectionLeft.ValueT = false;
            ReqRunState.ValueT = true;
        }

        public virtual bool IsEnabledTurnOnRight()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                    return true;
                else if (!DirectionLeft.ValueT)
                    return false;
                else if (ToggleAllowed)
                    return true;
                return false;
            }
            return false;
        }

        public override void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
            if (!switchOff)
                OnTurnOnRight();
            else
                OnTurnOff();
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotor2D(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorStartCtrl(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
