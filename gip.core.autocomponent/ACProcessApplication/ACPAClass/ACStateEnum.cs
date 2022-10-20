using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Xml;
using System.Text.RegularExpressions;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Enum for describing the current state of an PAPase-Instance according to the ISA-S88-State
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'State of a process object'}de{'Zustand eines Prozessobjekts'}", Global.ACKinds.TACEnum)]
    public enum ACStateEnum : short
    {
        SMIdle = 0,
        SMStarting = 100,
        SMRunning = 200,
        SMCompleted = 300,
        SMResetting = 400,

        SMPausing = 210,
        SMPaused = 211,
        SMResuming = 212,

        SMHolding = 220,
        SMHeld = 221,
        SMRestarting = 222,

        SMStopping = 310,
        SMStopped = 311,

        SMAborting = 320,
        SMAborted = 321,

        SMBreakPoint = 90,
        SMBreakPointStart = 91,
    }


    /// <summary>
    /// Constants for declaring the allowed Names for State-Methods and Transition-Methods according to the ACStateEnum
    /// </summary>
    public static class ACStateConst
    {
        #region State-Method-Names        
        /// <summary>
        /// State-Method-Name for the mainstate IDLE
        /// </summary>
        public const string SMIdle = "SMIdle";

        /// <summary>
        /// State-Method-Name for the mainstate STARTING
        /// </summary>
        public const string SMStarting = "SMStarting";

        /// <summary>
        /// State-Method-Name for the mainstate RUNNING
        /// </summary>
        public const string SMRunning = "SMRunning";

        /// <summary>
        /// State-Method-Name for the mainstate COMPLETED
        /// </summary>
        public const string SMCompleted = "SMCompleted";

        /// <summary>
        /// State-Method-Name for the mainstate RESETTING
        /// </summary>
        public const string SMResetting = "SMResetting";

        /// <summary>
        /// State-Method-Name for the substate PAUSING
        /// </summary>
        public const string SMPausing = "SMPausing";

        /// <summary>
        /// State-Method-Name for the substate PAUSED
        /// </summary>
        public const string SMPaused = "SMPaused";

        /// <summary>
        /// State-Method-Name for the substate RESUMING
        /// </summary>
        public const string SMResuming = "SMResuming";

        /// <summary>
        /// State-Method-Name for the substate HOLDING
        /// </summary>
        public const string SMHolding = "SMHolding";

        /// <summary>
        /// State-Method-Name for the substate HELD
        /// </summary>
        public const string SMHeld = "SMHeld";

        /// <summary>
        /// State-Method-Name for the substate RESTARTING
        /// </summary>
        public const string SMRestarting = "SMRestarting";

        /// <summary>
        /// State-Method-Name for the substate STOPPING
        /// </summary>
        public const string SMStopping = "SMStopping";

        /// <summary>
        /// State-Method-Name for the substate STOPPED
        /// </summary>
        public const string SMStopped = "SMStopped";

        /// <summary>
        /// State-Method-Name for the substate ABORTING
        /// </summary>
        public const string SMAborting = "SMAborting";

        /// <summary>
        /// State-Method-Name for the substate ABORTED
        /// </summary>
        public const string SMAborted = "SMAborted";

        /// <summary>
        /// State-Method-Name for the substate BREAKPOINT in IDLE-State
        /// </summary>
        public const string SMBreakPoint = "SMBreakPoint";

        /// <summary>
        /// State-Method-Name for the substate BREAKPOINT in STARTING-State
        /// </summary>
        public const string SMBreakPointStart = "SMBreakPointStart";
        #endregion


        #region Transition-Methods

        /// <summary>
        /// Transition-Method-Name for START
        /// </summary>
        public const string TMStart = "Start";

        /// <summary>
        /// Transition-Method-Name for RUN
        /// </summary>
        public const string TMRun = "Run";

        /// <summary>
        /// Transition-Method-Name for PAUSE
        /// </summary>
        public const string TMPause = "Pause";

        /// <summary>
        /// Transition-Method-Name for RESUME
        /// </summary>
        public const string TMResume = "Resume";

        /// <summary>
        /// Transition-Method-Name for HOLD
        /// </summary>
        public const string TMHold = "Hold";

        /// <summary>
        /// Transition-Method-Name for RESTART
        /// </summary>
        public const string TMRestart = "Restart";

        /// <summary>
        /// Transition-Method-Name for ABORT
        /// </summary>
        public const string TMAbort = "Abort";

        /// <summary>
        /// Transition-Method-Name for STOPP
        /// </summary>
        public const string TMStopp = "Stopp";

        /// <summary>
        /// Transition-Method-Name for RESET
        /// </summary>
        public const string TMReset = "Reset";
        #endregion


        /// <summary>
        /// Converts the name of a State-Method to the corresponding enum ACStateEnum
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public static ACStateEnum GetEnum(string state)
        {
            if (String.IsNullOrEmpty(state))
                return ACStateEnum.SMIdle;
            //PABaseState laststate = (PABaseState)Enum.Parse(typeof(PABaseState), state);

            switch (state)
            {
                case SMIdle:
                    return ACStateEnum.SMIdle;
                case SMStarting:
                    return ACStateEnum.SMStarting;
                case SMRunning:
                    return ACStateEnum.SMRunning;
                case SMCompleted:
                    return ACStateEnum.SMCompleted;
                case SMResetting:
                    return ACStateEnum.SMResetting;
                case SMPausing:
                    return ACStateEnum.SMPausing;
                case SMPaused:
                    return ACStateEnum.SMPaused;
                case SMResuming:
                    return ACStateEnum.SMResuming;
                case SMHolding:
                    return ACStateEnum.SMHolding;
                case SMHeld:
                    return ACStateEnum.SMHeld;
                case SMRestarting:
                    return ACStateEnum.SMRestarting;
                case SMStopping:
                    return ACStateEnum.SMStopping;
                case SMStopped:
                    return ACStateEnum.SMStopped;
                case SMAborting:
                    return ACStateEnum.SMAborting;
                case SMAborted:
                    return ACStateEnum.SMAborted;
                case SMBreakPoint:
                    return ACStateEnum.SMBreakPoint;
                case SMBreakPointStart:
                    return ACStateEnum.SMBreakPointStart;
                default:
                    return ACStateEnum.SMIdle;
            }
        }


        /// <summary>
        /// Converts the enum ACStateEnum to the corresponding name of  the State-Method
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToString(this ACStateEnum state)
        {
            switch (state)
            {
                case ACStateEnum.SMIdle:
                    return SMIdle;
                case ACStateEnum.SMStarting:
                    return SMStarting;
                case ACStateEnum.SMRunning:
                    return SMRunning;
                case ACStateEnum.SMCompleted:
                    return SMCompleted;
                case ACStateEnum.SMResetting:
                    return SMResetting;
                case ACStateEnum.SMPausing:
                    return SMPausing;
                case ACStateEnum.SMPaused:
                    return SMPaused;
                case ACStateEnum.SMResuming:
                    return SMResuming;
                case ACStateEnum.SMHolding:
                    return SMHolding;
                case ACStateEnum.SMHeld:
                    return SMHeld;
                case ACStateEnum.SMRestarting:
                    return SMRestarting;
                case ACStateEnum.SMStopping:
                    return SMStopping;
                case ACStateEnum.SMStopped:
                    return SMStopped;
                case ACStateEnum.SMAborting:
                    return SMAborting;
                case ACStateEnum.SMAborted:
                    return SMAborted;
                case ACStateEnum.SMBreakPoint:
                    return SMBreakPoint;
                case ACStateEnum.SMBreakPointStart:
                    return SMBreakPointStart;
                default:
                    return SMIdle;
            }
        }

    }
}
