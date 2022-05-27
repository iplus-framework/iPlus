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
    /// Motors with one direction of rotation and 2 Poles (Speed)
    /// Motoren mit einer Drehrichtung und 2 Polen (Geschwindigkeiten)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor 2 dir., 2 poles'}de{'Motor 2 Dreh., 2 Pole'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEEMotor2D2P : PAEEMotorStartCtrl
    {
        #region c'tors

        static PAEEMotor2D2P()
        {
            RegisterExecuteHandler(typeof(PAEEMotor2D2P), HandleExecuteACMethod_PAEEMotor2D2P);
        }

        public PAEEMotor2D2P(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
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
        //[ACPropertyInfo(800, "Configuration", "en{'Faststart off'}de{'Schnellanlauf erlaubt'}", "", true)]
        //public bool FaststartAllowed
        //{
        //    get;
        //    set;
        //}
        [ACPropertyBindingTarget(801, "Configuration", "en{'transit time slow/fast'}de{'Umschaltzeit im Langsam/Schnell'}", "", true, true, RemotePropID = 50)]
        public IACContainerTNet<TimeSpan> TransitTimeSlowFast { get; set; }

        [ACPropertyInfo(801, "Configuration", "en{'direct toggle allowed'}de{'Direktes umschalten erlaubt'}", "", true)]
        public bool ToggleAllowed
        {
            get;
            set;
        }

        #region Read-Values from PLC
        [ACPropertyBindingTarget(831, "Read from PLC", "en{'left direction'}de{'Laufrichtung links'}", "", false, false, RemotePropID = 51)]
        public IACContainerTNet<Boolean> DirectionLeft { get; set; }

        [ACPropertyBindingTarget(832, "Read from PLC", "en{'fast speed'}de{'Schnelle drehzahl'}", "", false, false, RemotePropID = 52)]
        public IACContainerTNet<Boolean> SpeedFast { get; set; }
        #endregion

        #region Write-Value to PLC
        [ACPropertyBindingTarget(851, "Write to PLC", "en{'request left direction'}de{'Anforderung links'}", "", false, false, RemotePropID = 53)]
        public IACContainerTNet<Boolean> ReqDirectionLeft { get; set; }

        [ACPropertyBindingTarget(852, "Write to PLC", "en{'Request fast speed'}de{'Anforderung schnell'}", "", false, false, RemotePropID = 54)]
        public IACContainerTNet<Boolean> ReqSpeedFast { get; set; }
        #endregion

        #endregion

        #endregion

        #region Methods, Range: 800

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOnSlowLeft":
                    TurnOnSlowLeft();
                    return true;
                case "TurnOnSlowRight":
                    TurnOnSlowRight();
                    return true;
                case "TurnOnFastLeft":
                    TurnOnFastLeft();
                    return true;
                case "TurnOnFastRight":
                    TurnOnFastRight();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnSlowLeft":
                    result = IsEnabledTurnOnSlowLeft();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnSlowRight":
                    result = IsEnabledTurnOnSlowRight();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnFastLeft":
                    result = IsEnabledTurnOnFastLeft();
                    return true;
                case Const.IsEnabledPrefix + "TurnOnFastRight":
                    result = IsEnabledTurnOnFastRight();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion



        [ACMethodInteraction("", "en{'turn on slow left'}de{'Langsam ein links'}", 800, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnSlowLeft()
        {
            if (!IsEnabledTurnOnSlowLeft())
                return;
            if (!PreExecute("TurnOnSlowLeft"))
                return;
            ReqSpeedFast.ValueT = false;
            ReqDirectionLeft.ValueT = true;
            ReqRunState.ValueT = true;
            PostExecute("TurnOnSlowLeft");
        }

        public virtual bool IsEnabledTurnOnSlowLeft()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                    return true;
                else
                {
                    if (ReqDirectionLeft.ValueT)
                    {
                        if (!SpeedFast.ValueT)
                            return false;
                        return true;
                    }
                    else
                    {
                        if (ToggleAllowed)
                            return true;
                        return false;
                    }
                }
            }
            return false;
        }

        [ACMethodInteraction("", "en{'turn on slow right'}de{'Langsam ein rechts'}", 800, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnSlowRight()
        {
            if (!IsEnabledTurnOnSlowRight())
                return;
            if (!PreExecute("TurnOnSlowRight"))
                return;
            ReqSpeedFast.ValueT = false;
            ReqDirectionLeft.ValueT = false;
            ReqRunState.ValueT = true;
            PostExecute("TurnOnSlowRight");
        }

        public virtual bool IsEnabledTurnOnSlowRight()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                    return true;
                else
                {
                    if (!ReqDirectionLeft.ValueT)
                    {
                        if (!SpeedFast.ValueT)
                            return false;
                        return true;
                    }
                    else
                    {
                        if (ToggleAllowed)
                            return true;
                        return false;
                    }
                }
            }
            return false;
        }

        [ACMethodInteraction("", "en{'turn on fast left'}de{'Schnell ein links'}", 801, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnFastLeft()
        {
            if (!IsEnabledTurnOnFastLeft())
                return;
            if (!PreExecute("TurnOnFastLeft"))
                return;
            ReqSpeedFast.ValueT = true;
            ReqDirectionLeft.ValueT = true;
            ReqRunState.ValueT = true;
            PostExecute("TurnOnFastLeft");
        }

        public virtual bool IsEnabledTurnOnFastLeft()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                {
                    if (TransitTimeSlowFast.ValueT > TimeSpan.Zero)
                        return true;
                }
                else
                {
                    if (ReqDirectionLeft.ValueT)
                    {
                        if (SpeedFast.ValueT)
                            return false;
                        return true;
                    }
                    else
                    {
                        if ((ToggleAllowed) && (TransitTimeSlowFast.ValueT > TimeSpan.Zero))
                            return true;
                        return false;
                    }
                }
            }
            return false;
        }

        [ACMethodInteraction("", "en{'turn on fast right'}de{'Schnell ein rechts'}", 801, true, "", Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnFastRight()
        {
            if (!IsEnabledTurnOnFastRight())
                return;
            if (!PreExecute("TurnOnFastRight"))
                return;
            ReqSpeedFast.ValueT = true;
            ReqDirectionLeft.ValueT = false;
            ReqRunState.ValueT = true;
            PostExecute("TurnOnFastRight");
        }

        public virtual bool IsEnabledTurnOnFastRight()
        {
            if (this.TurnOnInterlock.ValueT)
                return false;
            if (OperatingMode.ValueT == Global.OperatingMode.Manual)
            {
                if (!RunState.ValueT)
                {
                    if (TransitTimeSlowFast.ValueT > TimeSpan.Zero)
                        return true;
                }
                else
                {
                    if (!ReqDirectionLeft.ValueT)
                    {
                        if (SpeedFast.ValueT)
                            return false;
                        return true;
                    }
                    else
                    {
                        if ((ToggleAllowed) && (TransitTimeSlowFast.ValueT > TimeSpan.Zero))
                            return true;
                        return false;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotor2D2P(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorStartCtrl(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }


}
