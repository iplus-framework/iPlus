// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for converting State and Types between Standard-Model-Components and DataAccess-/Vendor Model
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAStateConverterBase'}de{'PAStateConverterBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, true)]
    public abstract class PAFuncStateConvBase : PAStateConverterBase
    {
        #region c'tors
        public PAFuncStateConvBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

#if DEBUG
            if (!LoggingEnabled)
                LoggingEnabled = true;
#endif

            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (ACState != null)
                (ACState as IACPropertyNetServer).ValueUpdatedOnReceival -= ModelProperty_ValueUpdatedOnReceival;
            ACState = null;

            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        public IACContainerTNet<ACStateEnum> ACState { get; set; }

        [ACPropertyBindingSource(210, "Error", "en{'Conversion Alarm'}de{'Konvertierungs Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> ConversionAlarm { get; set; }

        public abstract bool IsReadyForSending { get; }
        public abstract bool IsReadyForReading { get; }

        [ACPropertyInfo(true, 9999, "", "en{'Write log'}de{'Log schreiben'}", DefaultValue = false)]
        public bool LoggingEnabled { get; set; }

        #endregion

        #region override methods
        protected override bool OnParentServerPropertyFound(IACPropertyNetServer parentProperty)
        {
            switch (parentProperty.ACIdentifier)
            {
                case Const.ACState:
                    ACState = parentProperty as IACContainerTNet<ACStateEnum>;
                    return true;
                default:
                    break;
            }

            return false;
        }
        #endregion

        #region abstract methods
        public abstract ACStateEnum GetNextACState(PAProcessFunction sender, string transitionMethod = "");
        public abstract bool IsEnabledTransition(PAProcessFunction sender, string transitionMethod);
        public abstract MsgWithDetails SendACMethod(PAProcessFunction sender, ACMethod acMethod, ACMethod previousParams = null);
        public abstract PAProcessFunction.CompleteResult ReceiveACMethodResult(PAProcessFunction sender, ACMethod acMethod, out MsgWithDetails msg);
        public virtual void OnProjSpecFunctionEvent(PAProcessFunction sender, string eventName, params object[] projSpecParams) { }

        #endregion

        #region static methods
        public static ACStateEnum GetDefaultNextACState(ACStateEnum acState, string transitionMethod = "")
        {
            switch (acState)
            {
                case ACStateEnum.SMIdle:
                    if (transitionMethod == ACStateConst.TMStart)
                        return ACStateEnum.SMStarting;
                    return acState;
                case ACStateEnum.SMStarting:
                    if (transitionMethod == ACStateConst.TMReset)
                        return ACStateEnum.SMResetting;
                    else if (transitionMethod == ACStateConst.TMRun)
                        return ACStateEnum.SMRunning;
                    return acState;
                case ACStateEnum.SMRunning:
                    if (transitionMethod == ACStateConst.TMPause)
                        return ACStateEnum.SMPausing;
                    else if (transitionMethod == ACStateConst.TMHold)
                        return ACStateEnum.SMHolding;
                    else if (transitionMethod == ACStateConst.TMAbort)
                        return ACStateEnum.SMAborting;
                    else if (transitionMethod == ACStateConst.TMStopp)
                        return ACStateEnum.SMStopping;
                    return ACStateEnum.SMCompleted;
                case ACStateEnum.SMCompleted:
                case ACStateEnum.SMStopped:
                case ACStateEnum.SMAborted:
                    if (transitionMethod == ACStateConst.TMReset)
                        return ACStateEnum.SMResetting;
                    return acState;
                case ACStateEnum.SMResetting:
                    return ACStateEnum.SMIdle;

                case ACStateEnum.SMPausing:
                    return ACStateEnum.SMPaused;
                case ACStateEnum.SMPaused:
                    if (transitionMethod == ACStateConst.TMResume)
                        return ACStateEnum.SMResuming;
                    else if (transitionMethod == ACStateConst.TMAbort)
                        return ACStateEnum.SMAborting;
                    else if (transitionMethod == ACStateConst.TMStopp)
                        return ACStateEnum.SMStopping;
                    return ACStateEnum.SMPaused;
                case ACStateEnum.SMResuming:
                    return ACStateEnum.SMRunning;

                case ACStateEnum.SMHolding:
                    return ACStateEnum.SMHeld;
                case ACStateEnum.SMHeld:
                    if (transitionMethod == ACStateConst.TMRestart)
                        return ACStateEnum.SMRestarting;
                    return ACStateEnum.SMHeld;
                case ACStateEnum.SMRestarting:
                    return ACStateEnum.SMRunning;

                case ACStateEnum.SMAborting:
                    return ACStateEnum.SMAborted;
                case ACStateEnum.SMStopping:
                    if (transitionMethod == ACStateConst.TMAbort) // Sonderfall gip: Abbruch im Stoppmodus erlaubt
                        return ACStateEnum.SMAborting;
                    return ACStateEnum.SMStopped;
                default:
                    return acState;
            }
        }
        public static bool IsEnabledTransitionDefault(ACStateEnum acState, string transitionMethod, PAProcessFunction paProcessFunction)
        {
            switch (transitionMethod)
            {
                case ACStateConst.TMStart:
                    return acState == ACStateEnum.SMIdle;
                case ACStateConst.TMRun:
                    return acState == ACStateEnum.SMStarting;
                case ACStateConst.TMReset:
                    return acState == ACStateEnum.SMStarting || acState == ACStateEnum.SMCompleted || acState == ACStateEnum.SMAborted || acState == ACStateEnum.SMStopped || acState == ACStateEnum.SMIdle;
                case ACStateConst.TMStopp:
                    return acState == ACStateEnum.SMRunning || acState == ACStateEnum.SMHeld || acState == ACStateEnum.SMPaused;
                case ACStateConst.TMAbort:
                    return acState == ACStateEnum.SMRunning || acState == ACStateEnum.SMHeld || acState == ACStateEnum.SMPaused || acState == ACStateEnum.SMStopping;
                
                case ACStateConst.TMPause:
                    return acState == ACStateEnum.SMRunning;
                case ACStateConst.TMResume:
                    return acState == ACStateEnum.SMPaused;

                case ACStateConst.TMHold:
                    return acState == ACStateEnum.SMRunning;
                case ACStateConst.TMRestart:
                    return acState == ACStateEnum.SMHeld;
            }
            return false;
        }
        #endregion
    }
}
