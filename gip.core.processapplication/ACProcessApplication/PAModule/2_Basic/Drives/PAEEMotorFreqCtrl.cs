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
    /// Frequency controlled Motors
    /// Frequenzgeregelte Motoren
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Motor frequency controlled'}de{'Frequenzgeregelter Motor'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public abstract class PAEEMotorFreqCtrl : PAEEMotorBase
    {
        #region c'tors

        static PAEEMotorFreqCtrl()
        {
            RegisterExecuteHandler(typeof(PAEEMotorFreqCtrl), HandleExecuteACMethod_PAEEMotorFreqCtrl);
        }

        public PAEEMotorFreqCtrl(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            VFDState.PropertyChanged += VFDState_PropertyChanged;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            VFDState.PropertyChanged -= VFDState_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 700

        #region Read-Values from PLC
        [ACPropertyBindingTarget(730, "Read from PLC", "en{'current speed'}de{'aktuelle Geschwindigkeit'}", "", false, false, RemotePropID = 42)]
        public IACContainerTNet<Double> Speed { get; set; }

        [ACPropertyBindingTarget(732, "Read from PLC", "en{'desired speed'}de{'Soll Geschwindigkeit'}", "", false, false, RemotePropID = 43)]
        public IACContainerTNet<Double> DesiredSpeed { get; set; }

        [ACPropertyBindingTarget(731, "Read from PLC", "en{'State of VFD'}de{'FU-Status'}", "", false, false, RemotePropID = 44)]
        public IACContainerTNet<PANotifyState> VFDState { get; set; }

        public void OnSetVFDState(IACPropertyNetValueEvent valueEvent)
        {
            PANotifyState newVFDState = (valueEvent as ACPropertyValueEvent<PANotifyState>).Value;
            if (VFDStateACK.ValueT && newVFDState != PANotifyState.AlarmOrFault)
                VFDStateACK.ValueT = false;
            if (newVFDState != VFDState.ValueT)
            {
                if (newVFDState == PANotifyState.AlarmOrFault)
                    _VFDAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                else if (newVFDState == PANotifyState.Off)
                    _VFDAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
            }
        }

        private PAAlarmChangeState _VFDAlarmChanged = PAAlarmChangeState.NoChange;
        void VFDState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_VFDAlarmChanged != PAAlarmChangeState.NoChange) && e.PropertyName == Const.ValueT)
            {
                if (_VFDAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    OnNewAlarmOccurred(VFDState);
                else
                    OnAlarmDisappeared(VFDState);
                _VFDAlarmChanged = PAAlarmChangeState.NoChange;
            }
        }

        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(750, "Write to PLC", "en{'requested speed'}de{'angeforderte Geschwindigkeit'}", "", false, false, RemotePropID = 45)]
        public IACContainerTNet<Double> ReqSpeed { get; set; }

        [ACPropertyBindingTarget(751, "Write to PLC", "en{'Fault acknowledge VFD'}de{'Störungsquittung FU'}", "", true, false, RemotePropID = 46)]
        public IACContainerTNet<bool> VFDStateACK { get; set; }

        #endregion

        #endregion

        #region Methods, Range: 700
        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            VFDStateACK.ValueT = true;
            base.AcknowledgeAlarms();
        }

        public override bool IsEnabledAcknowledgeAlarms()
        {
            if ((VFDState.ValueT == PANotifyState.AlarmOrFault) && (!VFDStateACK.ValueT))
                return true;
            return base.IsEnabledAcknowledgeAlarms();
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEEMotorFreqCtrl(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotorBase(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
