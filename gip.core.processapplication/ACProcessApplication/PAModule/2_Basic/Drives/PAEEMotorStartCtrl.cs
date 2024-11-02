// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
    /// Motors with start controller (Star-delta start-up, Softstarter)
    /// Motoren mit Anlaufschaltung (Stern-Dreieck, Softstarter)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor with starter control'}de{'Motor mit Anlaufschaltung'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEEMotorStartCtrl : PAEEMotorBase
    {
        #region c'tors

        static PAEEMotorStartCtrl()
        {
            RegisterExecuteHandler(typeof(PAEEMotorStartCtrl), HandleExecuteACMethod_PAEEMotorStartCtrl);
        }

        public PAEEMotorStartCtrl(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
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

        #region Properties, Range: 700

        #region Configuration
        [ACPropertyBindingTarget(700, "Configuration", "en{'transit time in star'}de{'Laufzeit im Stern'}", "", true, true, RemotePropID = 42)]
        public IACContainerTNet<TimeSpan> TransitTimeStar { get; set; }

        [ACPropertyBindingTarget(701, "Configuration", "en{'changeover delaytime to delta'}de{'Umschaltverzögerungszeit auf Dreieck'}", "", true, true, RemotePropID = 43)]
        public IACContainerTNet<TimeSpan> ChangeoverDelay { get; set; }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(731, "Read from PLC", "en{'State of Starter'}de{'FU-Status'}", "", false, false, RemotePropID = 44)]
        public IACContainerTNet<PANotifyState> StarterState { get; set; }

        public void OnSetStarterState(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newStarterState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (StarterStateACK.ValueT && newStarterState != PANotifyState.AlarmOrFault)
                StarterStateACK.ValueT = false;
            if (newStarterState != StarterState.ValueT)
            {
                if (newStarterState == PANotifyState.AlarmOrFault)
                    _StarterAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newStarterState == PANotifyState.Off)
                    _StarterAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _StarterAlarmChanged = PAAlarmChangeState.NoChange;
        void StarterState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_StarterAlarmChanged != PAAlarmChangeState.NoChange) && e.PropertyName == Const.ValueT)
            {
                if (_StarterAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(StarterState);
                else
                    OnAlarmDisappeared(StarterState);
                _StarterAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'requested speed'}de{'angeforderte Geschwindigkeit'}", "", false, false, RemotePropID = 45)]
        public IACContainerTNet<Double> ReqSpeed { get; set; }

        [ACPropertyBindingTarget(751, "Write to PLC", "en{'Fault acknowledge Starter'}de{'Störungsquittung FU'}", "", true, false, RemotePropID = 46)]
        public IACContainerTNet<bool> StarterStateACK { get; set; }

        #endregion

        #endregion

        #region Methods, Range: 700
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            StarterStateACK.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if ((StarterState.ValueT == PANotifyState.AlarmOrFault) && (!StarterStateACK.ValueT))
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotorStartCtrl(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
