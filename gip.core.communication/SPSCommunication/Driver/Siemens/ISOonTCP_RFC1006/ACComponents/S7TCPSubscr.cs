using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'S7TCPSubscr'}de{'S7TCPSubscr'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class S7TCPSubscr : ACSubscription
    {
        #region c´tors
        static S7TCPSubscr()
        {
            RegisterExecuteHandler(typeof(S7TCPSubscr), HandleExecuteACMethod_S7TCPSubscr);
        }

        public S7TCPSubscr(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _IsReadErrorMessageOn = false;
            _IsWriteErrorMessageOn = false;
            _SubscriptionInitialized = false;
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            _PLCRAMOfDataBlocks = null;
            _PollPLCOn = false;
            return result;
        }

        #endregion

        #region Properties
        [ACPropertyPointProperty(9999, "", typeof(OPCItemConfig))]
        public IEnumerable<IACPropertyNetServer> S7Properties
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    try
                    {
                        var query = ACMemberList.Where(c => (c is IACPropertyNetServer)
                                                            && (c.ACType != null)
                                                            && (c.ACType.ACKind == Global.ACKinds.PSPropertyExt))
                                                .Select(c => c as IACPropertyNetServer)
                                                .ToArray();
                        return query;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("S7TCPSession", "S7Properties", msg);
                    }
                }
                return null;
            }
        }

        [ACPropertyInfo(9999, DefaultValue = Global.MaxRefreshRates.R1sec)]
        public Global.MaxRefreshRates RequiredUpdateRate
        {
            get;
            set;
        }


        private S7TCPDataBlocksRAM _PLCRAMOfDataBlocks = null;
        internal S7TCPDataBlocksRAM PLCRAMOfDataBlocks
        {
            get
            {
                if (_PLCRAMOfDataBlocks == null)
                {
                    _PLCRAMOfDataBlocks = new S7TCPDataBlocksRAM();
                    _PLCRAMOfDataBlocks.PropertyChanged += PLCRAMOfDataBlocks_PropertyChanged;
                }
                return _PLCRAMOfDataBlocks;
            }
        }

        protected S7TCPSession S7TCPSession
        {
            get
            {
                return ParentACComponent as S7TCPSession;
            }
        }

        public EndianessEnum Endianess
        {
            get
            {
                return S7TCPSession.Endianess;
            }
        }


        private bool _IsReadErrorMessageOn = false;
        public bool IsReadErrorMessageOn
        {
            get
            {
                return _IsReadErrorMessageOn;
            }
        }


        private object _DelegateLock = new object();
        private String _DelegateReadAlarmOccurred;
        private bool _DelegateReadAlarmDisappeared = false;

        [ACPropertyBindingSource(400, "Read from PLC", "en{'Read Alarm'}de{'Read Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> ReadAlarm { get; set; }


        private bool _IsWriteErrorMessageOn = false;
        public bool IsWriteErrorMessageOn
        {
            get
            {
                return _IsWriteErrorMessageOn;
            }
        }
        private String _DelegateWriteAlarmOccurred;
        private bool _DelegateWriteAlarmDisappeared = false;

        [ACPropertyBindingSource(401, "Read from PLC", "en{'Write Alarm'}de{'Write Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> WriteAlarm { get; set; }

        #endregion

        #region Methods

        #region HandleExecute
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledInitSubscription":
                    result = IsEnabledInitSubscription();
                    return true;
                case "IsEnabledDeInitSubscription":
                    result = IsEnabledDeInitSubscription();
                    return true;
                case "IsEnabledConnect":
                    result = IsEnabledConnect();
                    return true;
                case "IsEnabledDisConnect":
                    result = IsEnabledDisConnect();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_S7TCPSubscr(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_ACSubscription(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        #region Instance-Methods
        private bool _SubscriptionInitialized = false;
        public override bool InitSubscription()
        {
            if (ACOperationMode != ACOperationModes.Live)
            {
                _SubscriptionInitialized = true;
                return true;
            }

            Messages.LogDebug(this.GetACUrl(), "S7TCPSubscr.InitSubscription(1)", "");

            var s7Properties = S7Properties;
            if (s7Properties != null && s7Properties.Any())
            {
                Messages.LogDebug(this.GetACUrl(), "S7TCPSubscr.InitSubscription(3)", String.Format("Count S7Properties: {0}", s7Properties.Count()));
                foreach (IACPropertyNetServer opcProperty in s7Properties)
                {
                    OPCItemConfig opcConfig = (OPCItemConfig)(opcProperty.ACType as ACClassProperty)["OPCItemConfig"];
                    if ((opcConfig != null) && !String.IsNullOrEmpty(opcConfig.OPCAddr))
                    {
                        //Messages.LogDebug(this.GetACUrl(), "S7TCPSubscr.InitSubscription(4)", String.Format("Addr: {0}", opcConfig.OPCAddr));
                        S7TCPItem daItem = new S7TCPItem(opcProperty, opcConfig.OPCAddr, this);
                        if (daItem.IsValidItemSyntax)
                        {
                            switch (daItem.ItemDataType)
                            {
                                case ISOonTCP.DataTypeEnum.DataBlock:
                                    PLCRAMOfDataBlocks.Add(daItem);
                                    break;
                                case ISOonTCP.DataTypeEnum.Input:
                                case ISOonTCP.DataTypeEnum.Output:
                                case ISOonTCP.DataTypeEnum.Marker:
                                case ISOonTCP.DataTypeEnum.Timer:
                                case ISOonTCP.DataTypeEnum.Counter:
                                    PLCRAMOfDataBlocks.Add(daItem);
                                    break;
                            }
                        }
                        else
                            Messages.LogError(this.GetACUrl(), "S7TCPSubscr.InitSubscription(5)", String.Format("Invalid Item-Syntax: {0}", opcConfig.OPCAddr));


                        //_ItemsStartIndexMap.Where(c => c.Key >= 1 && c.Key < 30).Select(c => c.Value).ToList();
                    }
                }
            }
            Messages.LogDebug(this.GetACUrl(), "S7TCPSubscr.InitSubscription(5)", "");

            //_DaSubscription.StateChangeCompleted += OnStateChanged;

            _SubscriptionInitialized = true;
            return true;
        }

        public override bool IsEnabledInitSubscription()
        {
            return !_SubscriptionInitialized;
        }

        public override bool DeInitSubscription()
        {
            // 1. Disconnect
            if (!DisConnect())
                return false;

            // 2. Remove Events
            //_DaSubscription.StateChangeCompleted -= OnStateChanged;

            // 3. DeInit Items
            PLCRAMOfDataBlocks.DeInit();

            if (_PLCRAMOfDataBlocks != null)
                _PLCRAMOfDataBlocks.PropertyChanged -= PLCRAMOfDataBlocks_PropertyChanged;

            // 4. Empty DataBocksMap
            _PLCRAMOfDataBlocks = new S7TCPDataBlocksRAM();

            _SubscriptionInitialized = false;
            return true;
        }

        public override bool IsEnabledDeInitSubscription()
        {
            return _SubscriptionInitialized;
        }

        private bool _PollPLCOn = false;
        public override bool Connect()
        {
            if (!IsEnabledConnect())
                return true;
            if (!_SubscriptionInitialized)
            {
                if (!InitSubscription())
                    return false;
            }
            else
            {
                if (PLCRAMOfDataBlocks != null)
                {
                    foreach (var dataBlock in PLCRAMOfDataBlocks)
                    {
                        dataBlock.Value.Reconnected();
                    }
                }
            }
            _PollPLCOn = true;
            InitWorkCycleEventHandler(false);
            return true;
        }

        public override bool IsEnabledConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (_PollPLCOn)
                return false;
            return true;
        }

        public override bool DisConnect()
        {
            if (!IsEnabledDisConnect())
                return true;
            _PollPLCOn = false;
            InitWorkCycleEventHandler(true);
            return true;
        }

        public override bool IsEnabledDisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (!_PollPLCOn)
                return false;
            return true;
        }
        #endregion

        #endregion

        #region Event-Handler
        private void InitWorkCycleEventHandler(bool Remove)
        {
            if (!IsWriteOnly)
                IsReadyForWriting = false;
            if (S7TCPSession != null && !IsWriteOnly)
                S7TCPSession.AddToPollingPlan(this);
            else if (S7TCPSession != null && IsWriteOnly && !IsReadyForWriting)
                IsReadyForWriting = true;

            if (this.CommService != null)
            {
                if (Remove)
                    this.CommService.ProjectWorkCycleR100ms -= HandlePropertyChangedDelegate;
                else
                    this.CommService.ProjectWorkCycleR100ms += HandlePropertyChangedDelegate;

                switch (RequiredUpdateRate)
                {
                    case Global.MaxRefreshRates.R100ms:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR100ms -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR100ms += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R200ms:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR200ms -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR200ms += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R500ms:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR500ms -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR500ms += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R1sec:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR1sec -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR1sec += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R2sec:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR2sec -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR2sec += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R5sec:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR5sec -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR5sec += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R10sec:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR10sec -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR10sec += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R20sec:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR20sec -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR20sec += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R1min:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR1min -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR1min += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R2min:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR2min -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR2min += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R5min:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR5min -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR5min += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R10min:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR10min -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR10min += objectManager_ProjectWorkCycle;
                        break;
                    case Global.MaxRefreshRates.R20min:
                        if (Remove)
                            this.CommService.ProjectWorkCycleR20min -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleR20min += objectManager_ProjectWorkCycle;
                        break;
                    default:
                        if (Remove)
                            this.CommService.ProjectWorkCycleHourly -= objectManager_ProjectWorkCycle;
                        else
                            this.CommService.ProjectWorkCycleHourly += objectManager_ProjectWorkCycle;
                        break;
                }
            }
        }

        private void objectManager_ProjectWorkCycle(object sender, EventArgs e)
        {
            if (S7TCPSession != null && !IsWriteOnly)
                S7TCPSession.AddToPollingPlan(this);
            else if (S7TCPSession != null && IsWriteOnly && !IsReadyForWriting)
                IsReadyForWriting = true;
            RunAutomaticBackupIfInterval();
        }

        //private void OnStateChanged(ObjectSpaceElement obj, EnumObjectState state)
        //{
        //    if (IsConnected != null)
        //    {
        //        if ((state == EnumObjectState.CONNECTED) || (state == EnumObjectState.ACTIVATED))
        //            IsConnected.ValueT = true;
        //        else
        //            IsConnected.ValueT = false;
        //    }
        //}

        void PLCRAMOfDataBlocks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReadErrorMessage")
            {
                string readErrorMessage = PLCRAMOfDataBlocks.ReadErrorMessage;
                bool isNewMessageOn = !String.IsNullOrEmpty(readErrorMessage);
                if (isNewMessageOn != _IsReadErrorMessageOn)
                {
                    _IsReadErrorMessageOn = isNewMessageOn;
                    if (isNewMessageOn)
                    {
                        ReadAlarm.ValueT = PANotifyState.AlarmOrFault;
                        lock (_DelegateLock)
                        {
                            _DelegateReadAlarmOccurred = readErrorMessage;
                        }
                    }
                    else
                    {
                        ReadAlarm.ValueT = PANotifyState.Off;
                        lock (_DelegateLock)
                        {
                            _DelegateReadAlarmDisappeared = true;
                        }
                    }
                    OnPropertyChanged("ReadAlarm");
                }
                else if (isNewMessageOn && ReadAlarm.ValueT == PANotifyState.AlarmOrFault)
                {
                    OnAlarmUpdateMessage("ReadAlarm", readErrorMessage);
                }
            }
            else if (e.PropertyName == "WriteErrorMessage")
            {
                string writeErrorMessage = PLCRAMOfDataBlocks.WriteErrorMessage;
                bool isNewMessageOn = !String.IsNullOrEmpty(writeErrorMessage);
                if (isNewMessageOn != _IsWriteErrorMessageOn)
                {
                    _IsWriteErrorMessageOn = isNewMessageOn;
                    if (isNewMessageOn)
                    {
                        WriteAlarm.ValueT = PANotifyState.AlarmOrFault;
                        lock (_DelegateLock)
                        {
                            _DelegateWriteAlarmOccurred = writeErrorMessage;
                        }
                    }
                    else
                    {
                        WriteAlarm.ValueT = PANotifyState.Off;
                        lock (_DelegateLock)
                        {
                            _DelegateWriteAlarmDisappeared = true;
                        }
                    }
                    OnPropertyChanged("WriteAlarm");
                }
                else if (isNewMessageOn && ReadAlarm.ValueT == PANotifyState.AlarmOrFault)
                {
                    OnAlarmUpdateMessage("WriteAlarm", writeErrorMessage);
                }
            }
        }

        // Weil Datenbank-Kontext im Thread vom ApplicationManager läuft, müssen Alarme delegiert werden
        private void HandlePropertyChangedDelegate(object sender, EventArgs e)
        {
            if ((_DelegateReadAlarmOccurred != null)
                || (_DelegateWriteAlarmOccurred != null)
                || _DelegateReadAlarmDisappeared
                || _DelegateWriteAlarmDisappeared)
            {
                try
                {
                    string delegateConnAlarmOccurred = null;
                    string delegateWriteAlarmOccurred = null;
                    bool delegateReadAlarmDisappeared = false;
                    bool delegateWriteAlarmDisappeared = false;
                    lock (_DelegateLock)
                    {
                        delegateConnAlarmOccurred = _DelegateReadAlarmOccurred;
                        delegateWriteAlarmOccurred = _DelegateWriteAlarmOccurred;
                        delegateReadAlarmDisappeared = _DelegateReadAlarmDisappeared;
                        delegateWriteAlarmDisappeared = _DelegateWriteAlarmDisappeared;
                    }


                    if (delegateConnAlarmOccurred != null)
                    {
                        OnNewAlarmOccurred(ReadAlarm, new Msg(delegateConnAlarmOccurred, this, eMsgLevel.Error, "S7TCPSubscr", "HandlePropertyChangedDelegate", 1000));
                    }
                    if (delegateWriteAlarmOccurred != null)
                    {
                        OnNewAlarmOccurred(WriteAlarm, new Msg(delegateWriteAlarmOccurred, this, eMsgLevel.Error, "S7TCPSubscr", "HandlePropertyChangedDelegate", 1010));
                    }
                    if (delegateReadAlarmDisappeared)
                    {
                        OnAlarmDisappeared(ReadAlarm);
                    }
                    if (delegateWriteAlarmDisappeared)
                    {
                        OnAlarmDisappeared(WriteAlarm);
                    }

                    lock (_DelegateLock)
                    {
                        _DelegateReadAlarmOccurred = null;
                        _DelegateWriteAlarmOccurred = null;
                        _DelegateReadAlarmDisappeared = false;
                        _DelegateWriteAlarmDisappeared = false;
                    }
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("S7TCPSubscr", "HandleDelegatePropertyChanged", msg);
                }
                finally
                {
                }
            }
        }


        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (ReadAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                ReadAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(ReadAlarm);
            }
            if (WriteAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                WriteAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(WriteAlarm);
            }
            base.AcknowledgeAlarms();
        }

        #endregion
    }
}
