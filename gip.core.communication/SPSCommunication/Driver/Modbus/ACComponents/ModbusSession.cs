using gip.core.autocomponent;
using gip.core.communication.modbus;
using gip.core.datamodel;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ModbusSession'}de{'ModbusSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ModbusSession : ACSession
    {
        #region c´tors
        public ModbusSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (_syncSend == null)
                _syncSend = new SyncQueueEvents();

            if (_syncPoll == null)
                _syncPoll = new SyncQueueEvents();
            if (_Items2Send == null)
                _Items2Send = new ModbusItems2SendQueue();
            if (_PollingPlan == null)
                _PollingPlan = new ModbusPollingPlan();
            _DelegateConnAlarmOccurred = false;
            _DelegateConnAlarmDisappeared = false;

            if (!base.ACInit(startChildMode))
                return false;

            if (ACOperationMode == ACOperationModes.Live)
            {
                if (this.CommService != null)
                    this.CommService.ProjectWorkCycleR100ms += HandlePropertyChangedDelegate;

                _pollThread = new ACThread(PollPLC);
                _pollThread.Name = "ACUrl:" + this.GetACUrl() + ";PollPLC();";
                _pollThread.Start();

                _sendThread = new ACThread(SendPLC);
                _sendThread.Name = "ACUrl:" + this.GetACUrl() + ";SendPLC();";
                _sendThread.Start();
            }

            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (ACOperationMode == ACOperationModes.Live)
            {
                if (CommService != null)
                    CommService.ProjectWorkCycleR100ms -= HandlePropertyChangedDelegate;

                if (_pollThread != null)
                {
                    _syncPoll.TerminateThread();
                    _pollThread.Join();
                    _pollThread = null;
                }

                _PollingPlan = null;

                if (_sendThread != null)
                {
                    _syncSend.TerminateThread();
                    _sendThread.Join();
                    _sendThread = null;
                }

                if (!_StopReconnectInitiated)
                    StopReconnection();
            }

            bool result = base.ACDeInit(deleteACClassTask);

            _syncSend = null;
            _syncPoll = null;

            using (ACMonitor.Lock(_11900_SocketLockObj))
            {
                if (_TCPClient != null)
                    _TCPClient.Close();
                if (_PLCConn != null)
                    _PLCConn.Dispose();
                _PLCConn = null;
            }
            _Items2Send = null;
            _PollingPlan = null;

            return result;
        }
#endregion

#region private members

        // Producer-Consumer for Sending
        SyncQueueEvents _syncSend;
        ACThread _sendThread;

        // Producer-Consumer for Receiving
        SyncQueueEvents _syncPoll;
        ACThread _pollThread;

        ManualResetEvent _syncReconnect;
        ACThread _ReconnectThread = null;

#endregion

#region Properties
        [ACPropertyInfo(9999, DefaultValue = "localhost")]
        public string IPAddress
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = (Int16)502)]
        public Int16 Port
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public Int32 SendTimeout
        {
            get;
            set;
        }


        [ACPropertyInfo(9999)]
        public Int32 ReceiveTimeout
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = WriteModeModbus.Separately)]
        public WriteModeModbus WriteMode
        {
            get;
            set;
        }

        private bool WriteInChronolgicalOrder
        {
            get
            {
                return this.WriteMode == WriteModeModbus.Separately ? true : false;
            }
        }

        [ACPropertyInfo(9999, DefaultValue = EndianessEnum.MixedEndian)]
        public EndianessEnum Endianess
        {
            get;
            set;
        }

        private bool _Initialized = false;

        TcpClient _TCPClient = null;
        public TcpClient TCPClient
        {
            get
            {
                return _TCPClient;
            }
        }

        ModbusIpMaster _PLCConn = null;
        public ModbusIpMaster PLCConn
        {
            get
            {
                return _PLCConn;
            }
        }
        private readonly ACMonitorObject _11900_SocketLockObj = new ACMonitorObject(11900);


        [ACPropertyBindingSource]
        public IACContainerTNet<bool> ReadError { get; set; }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> WriteError { get; set; }

        private CircularBuffer<Tuple<DateTime, ModbusItemsSendPackageSegment, int>> _LogQueue = null;
        private object _LogQueueLock = new object();
#endregion

#region Methods
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledInitSession":
                    result = IsEnabledInitSession();
                    return true;
                case "IsEnabledDeInitSession":
                    result = IsEnabledDeInitSession();
                    return true;
                case "IsEnabledConnect":
                    result = IsEnabledConnect();
                    return true;
                case "IsEnabledDisConnect":
                    result = IsEnabledDisConnect();
                    return true;
                case "SwitchLoggingOn":
                    SwitchLoggingOn();
                    return true;
                case "IsEnabledSwitchLoggingOn":
                    result = IsEnabledSwitchLoggingOn();
                    return true;
                case "SwitchLoggingOff":
                    SwitchLoggingOff();
                    return true;
                case "IsEnabledSwitchLoggingOff":
                    result = IsEnabledSwitchLoggingOff();
                    return true;
                case "DumpLog":
                    DumpLog();
                    return true;
                case "IsEnabledDumpLog":
                    result = IsEnabledDumpLog();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override bool InitSession()
        {
            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "ModbusSession.InitSession()", "Start InitSession");
            if (_Initialized)
                return true;

            // 1. Instanz der Session erzeugen
            Messages.LogDebug(this.GetACUrl(), "ModbusSession.InitSession()", "New Session");

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "ModbusSession.InitSession()", "Init Subscriptions");
            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is ModbusSubscr)
                {
                    ModbusSubscr acSubscription = child as ModbusSubscr;
                    acSubscription.InitSubscription();
                    acSubscription.PropertyChanged += OnACSubscription_PropertyChanged;
                }
            }

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "ModbusSession.InitSession()", "Init Completed");
            _Initialized = true;
            return true;
        }

        public override bool IsEnabledInitSession()
        {
            return !_Initialized;
        }

        public override bool DeInitSession()
        {
            if (!_Initialized)
                return true;

            // 1. Disconnect
            if (!DisConnect())
                return false;

            // 2. Remove Events
            //_PLCConn.PropertyChanged -= OnPLCConn_PropertyChanged;

            // 3. Deinit Subscriptions. (Is already done, when called ACDeInit())
            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is ModbusSubscr)
                {
                    ModbusSubscr acSubscription = child as ModbusSubscr;
                    acSubscription.PropertyChanged -= OnACSubscription_PropertyChanged;
                    acSubscription.DeInitSubscription();
                }
            }

            _Initialized = false;
            return true;
        }

        public override bool IsEnabledDeInitSession()
        {
            return _Initialized;
        }

        private int _ReconnectTries = 0;
        public override bool Connect()
        {
            _ManuallyDisConnected = false;

            if (!InitSession())
                return false;

            if (!IsEnabledConnect())
            {
                _ReconnectTries++;
                return false;
            }

            if (ACOperationMode != ACOperationModes.Live)
                return true;

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "ModbusSession.Connect(0)", "Start Connect");
            if (_TCPClient != null && !_TCPClient.Connected)
            {

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    _TCPClient.Close();
                    _TCPClient = null;
                    if (_PLCConn != null)
                        _PLCConn.Dispose();
                    _PLCConn = null;
                }
            }
            try
            {

                using (ACMonitor.Lock(_11900_SocketLockObj))
                {
                    _TCPClient = new TcpClient(IPAddress, Port);
                    if (SendTimeout > 0)
                        _TCPClient.SendTimeout = SendTimeout;
                    if (ReceiveTimeout > 0)
                        _TCPClient.ReceiveTimeout = ReceiveTimeout;
                }
                IsConnected.ValueT = _TCPClient.Connected;
            }
            catch (Exception e)
            {
                if (_PLCConn != null)
                    _PLCConn.Dispose();
                _PLCConn = null;
                if (_ReconnectTries <= 0)
                    Messages.LogException(this.GetACUrl(), "ModbusSession.Connect(1)", "Not Connected" + e.Message);
                _ReconnectTries++;
                return false;
            }
            _ReconnectTries = 0;
            Messages.LogDebug(this.GetACUrl(), "ModbusSession.Connect(2)", "Connected");

            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is ModbusSubscr)
                {
                    ModbusSubscr acSubscription = child as ModbusSubscr;
                    acSubscription.Connect();
                }
            }
            using (ACMonitor.Lock(_11900_SocketLockObj))
            {
                _PLCConn = ModbusIpMaster.CreateIp(_TCPClient);
            }
            return _TCPClient.Connected;
        }

        public override bool IsEnabledConnect()
        {
            if (!base.IsEnabledConnect())
                return false;
            if (_TCPClient == null)
                return true;
            if (!_TCPClient.Connected)
                return true;
            return false;
        }

        private bool _ManuallyDisConnected = false;
        public override bool DisConnect()
        {
            if (!IsEnabledDisConnect())
                return true;
            if (_TCPClient == null)
                return true;

            return InternalDisconnect(false);
        }

        private bool InternalDisconnect(bool calledFromPropChanged)
        {
            if (calledFromPropChanged && _ManuallyDisConnected)
                return true;
            else if (!calledFromPropChanged)
                _ManuallyDisConnected = true;

            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is ModbusSubscr)
                {
                    ModbusSubscr acSubscription = child as ModbusSubscr;
                    acSubscription.DisConnect();
                }
            }

            if (calledFromPropChanged)
                return true;

            using (ACMonitor.Lock(_11900_SocketLockObj))
            {
                if (_TCPClient.Connected)
                {
                    _TCPClient.Close();
                    _TCPClient = null;
                }
                IsConnected.ValueT = false;
                if (_PLCConn != null)
                    _PLCConn.Dispose();
                _PLCConn = null;
            }
            return true;
        }

        public override bool IsEnabledDisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (_TCPClient == null)
                return false;
            if (_TCPClient.Connected)
                return true;
            return false;
        }


        [ACMethodInteraction("Log", "en{'Activate Logging'}de{'Sendelog aktivieren'}", 300, true)]
        public void SwitchLoggingOn()
        {
            if (!IsEnabledSwitchLoggingOn())
                return;
            lock (_LogQueueLock)
            {
                if (_LogQueue == null)
                    _LogQueue = new CircularBuffer<Tuple<DateTime, ModbusItemsSendPackageSegment, int>>(1000, true);
            }
        }

        public bool IsEnabledSwitchLoggingOn()
        {
            return _LogQueue == null;
        }

        [ACMethodInteraction("Log", "en{'Deactivate Logging'}de{'Sendelog ausschalten'}", 301, true)]
        public void SwitchLoggingOff()
        {
            if (!IsEnabledSwitchLoggingOff())
                return;
            lock (_LogQueueLock)
            {
                _LogQueue = null;
            }
        }

        public bool IsEnabledSwitchLoggingOff()
        {
            return _LogQueue != null;
        }

        [ACMethodInteraction("Log", "en{'Dump Log'}de{'Sendelog in Datei schreiben'}", 302, true)]
        public void DumpLog()
        {
            if (!IsEnabledDumpLog())
                return;
            CircularBuffer<Tuple<DateTime, ModbusItemsSendPackageSegment, int>> logQueueCopy = null;
            lock (_LogQueueLock)
            {
                logQueueCopy = _LogQueue;
                _LogQueue = new CircularBuffer<Tuple<DateTime, ModbusItemsSendPackageSegment, int>>(1000, true);
            }
            if (logQueueCopy != null)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (Tuple<DateTime, ModbusItemsSendPackageSegment, int> tuple in logQueueCopy)
                        {
                            sb.AppendFormat("{0:HH:mm:ss.fff} DB{1:00000} SI{2:00000} ", tuple.Item1, tuple.Item3, tuple.Item2.StartIndex);
                            int i = 0;
                            foreach (byte byteVal in tuple.Item2.WriteSegment)
                            {
                                //sb.AppendFormat("[{0:000}]{1:x2} ", i, byteVal);
                                sb.AppendFormat("{0:x2} ", byteVal);
                                i++;
                            }
                            sb.AppendLine();
                        }

                        string fileName = String.Format("ModbusDump_{0:yyyyMMdd_HHmmss}_{1}.txt", DateTime.Now, Guid.NewGuid().GetHashCode());
                        fileName = Path.Combine(Path.GetTempPath(), fileName);
                        File.WriteAllText(fileName, sb.ToString());
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ModbusSession", "DumpLog", msg);
                    }
                });
            }
        }

        public bool IsEnabledDumpLog()
        {
            return _LogQueue != null;
        }

#endregion

#region Event-Handler

        void OnACSubscription_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReadAlarm")
            {
                bool hasReadErrors = false;
                foreach (IACComponent child in this.ACComponentChilds)
                {
                    if (child is ModbusSubscr)
                    {
                        ModbusSubscr acSubscription = child as ModbusSubscr;
                        if (acSubscription.IsReadErrorMessageOn)
                        {
                            hasReadErrors = true;
                            break;
                        }
                    }
                }
                ReadError.ValueT = hasReadErrors;
            }
            else if (e.PropertyName == "WriteAlarm")
            {
                bool hasWriteErrors = false;
                foreach (IACComponent child in this.ACComponentChilds)
                {
                    if (child is ModbusSubscr)
                    {
                        ModbusSubscr acSubscription = child as ModbusSubscr;
                        if (acSubscription.IsWriteErrorMessageOn)
                        {
                            hasWriteErrors = true;
                            break;
                        }
                    }
                }
                WriteError.ValueT = hasWriteErrors;
            }
        }

        protected override void StartReconnection()
        {
            _StopReconnectInitiated = false;
            if (ACOperationMode == ACOperationModes.Live &&
                !_ManuallyDisConnected && AutoReconnect && _ReconnectThread == null && (_TCPClient == null || !_TCPClient.Connected))
            {
                _syncReconnect = new ManualResetEvent(false);
                _ReconnectThread = new ACThread(Reconnect);
                _ReconnectThread.Name = "ACUrl:" + this.GetACUrl() + ";Reconnect();";
                _ReconnectThread.Start();
            }
        }

        private bool _StopReconnectInitiated = false;
        private void StopReconnection()
        {
            if (_ReconnectThread != null)
            {
                if (_syncReconnect != null && _syncReconnect.SafeWaitHandle != null && !_syncReconnect.SafeWaitHandle.IsClosed)
                    _syncReconnect.Set();
                if (!_ReconnectThread.Join(10000))
                    _ReconnectThread.Abort();
                _ReconnectThread = null;
            }
            _StopReconnectInitiated = false;
        }

        private void InitiateStopReconnection()
        {
            if (_StopReconnectInitiated)
                return;
            _StopReconnectInitiated = true;
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                try
                {
                    StopReconnection();
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "InitiateStopReconnection(0)", ex.Message);
                }
            });
        }


        private void Reconnect()
        {
            while (!_syncReconnect.WaitOne(2000, false))
            {
                if (_TCPClient != null && _TCPClient.Connected)
                {
                    InitiateStopReconnection();
                    continue;
                }

                _ReconnectThread.StartReportingExeTime();
                try
                {
                    if (_TCPClient == null || !_TCPClient.Connected)
                        Connect();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "Reconnect(0)", e.Message);
                    Messages.LogException(this.GetACUrl(), "Reconnect(1)", e.StackTrace);
                }
                _ReconnectThread.StopReportingExeTime();

                if (_TCPClient != null && _TCPClient.Connected)
                    InitiateStopReconnection();
            }
        }


        private bool _DelegateConnAlarmOccurred = false;
        private bool _DelegateConnAlarmDisappeared = false;
        private object _DelegateLock = new object();
        protected override void IsConnected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Const.ValueT)
            {
                if (_ConnectedAlarmChanged != PAAlarmChangeState.NoChange)
                {
                    if (_ConnectedAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    {
                        lock (_DelegateLock)
                        {
                            _DelegateConnAlarmOccurred = true;
                        }
                        DeactivateAutoBackup();
                    }
                    else
                    {
                        lock (_DelegateLock)
                        {
                            _DelegateConnAlarmDisappeared = true;
                        }
                    }
                    _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
                }
            }
        }

        // Weil Datenbank-Kontext im Thread vom ApplicationManager läuft, müssen Alarme delegiert werden
        private void HandlePropertyChangedDelegate(object sender, EventArgs e)
        {
            if (_DelegateConnAlarmOccurred || _DelegateConnAlarmDisappeared)
            {
                try
                {
                    bool delegateConnAlarmOccurred = false;
                    bool delegateConnAlarmDisappeared = false;
                    lock (_DelegateLock)
                    {
                        delegateConnAlarmOccurred = _DelegateConnAlarmOccurred;
                        delegateConnAlarmDisappeared = _DelegateConnAlarmDisappeared;
                    }

                    if (delegateConnAlarmOccurred)
                    {
                        OnNewAlarmOccurred(IsConnectedAlarm);
                    }
                    if (delegateConnAlarmDisappeared)
                    {
                        OnAlarmDisappeared(IsConnectedAlarm);
                    }

                    lock (_DelegateLock)
                    {
                        _DelegateConnAlarmOccurred = false;
                        _DelegateConnAlarmDisappeared = false;
                    }
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ModbusSession", "HandlePropertyChangedDelegate", msg);
                }
                finally
                {
                }
            }
        }
#endregion

#region Polling
        private ModbusPollingPlan _PollingPlan = new ModbusPollingPlan();
        protected ModbusPollingPlan PollingPlan
        {
            get
            {
                if (_PollingPlan == null)
                    _PollingPlan = new ModbusPollingPlan();
                return _PollingPlan;
            }
        }

        internal bool AddToPollingPlan(ModbusSubscr subscription, int dbNo = -10)
        {
            if (!IsConnected.ValueT || (ACOperationMode != ACOperationModes.Live))
                return false;

            if (PollingPlan == null)
                return false;
            if (!_syncPoll.NewItemsEnqueueable)
                return false;

            bool enqueued = PollingPlan.Enqueue(subscription, dbNo);
            // Signalisiere Thread, dass neuer Poll-Auftrag ansteht
            if (enqueued)
                _syncPoll.NewItemEvent.Set();

            return enqueued;
        }

        private void PollPLC()
        {
            while (!_syncPoll.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncPoll.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(50);

                if ((PollingPlan == null) || (Root == null))
                    continue;
                _pollThread.StartReportingExeTime();
                List<ModbusPollingPlanEntry> PollingPlanList = PollingPlan.GetAllEntrys(true);
                PollingPlanList.ForEach(c => ReadFromPLC(c));
                _pollThread.StopReportingExeTime();
            }
            _syncPoll.ThreadTerminated();
        }

        private void ReadFromPLC(ModbusPollingPlanEntry pollEntry)
        {
            if (pollEntry.DBNo <= -10)
            {
                bool allBlocksPolledSuccessfully = true;
                foreach (ModbusDataBlock dataBlock in pollEntry.Subscr.PLCRAMOfDataBlocks.DataBlocks)
                {
                    if (!ReadFromPLC(dataBlock, pollEntry.Subscr.ModbusSlaveUnitId))
                        allBlocksPolledSuccessfully = false;
                }
                if (!pollEntry.Subscr.IsReadyForWriting && allBlocksPolledSuccessfully)
                    pollEntry.Subscr.IsReadyForWriting = true;
            }
            else
            {
                try
                {
                    ReadFromPLC(pollEntry.Subscr.PLCRAMOfDataBlocks[pollEntry.DBNo], pollEntry.Subscr.ModbusSlaveUnitId);
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "ModbusSubscr.ReadFromPLC(ModbusPollingPlanEntry)", ex.Message);
                }
            }
        }

        private bool ReadFromPLC(ModbusDataBlock dataBlock, byte slaveUnitID)
        {
            if (_TCPClient == null || (dataBlock.RequestedSize <= 0) || !_TCPClient.Connected || PLCConn == null)
                return false;
            if (dataBlock.RequestedSize > 50000)
            {
                string message = String.Format("Requested Size {0} is to large", dataBlock.RequestedSize);
                bool logMessage = dataBlock.ReadErrorMessage != message;
                dataBlock.ReadErrorMessage = message;
                if (logMessage)
                    Messages.LogFailure(this.GetACUrl(), "ModbusSubscr.ReadFromPLC(ModbusDataBlock)", message);
                return false;
            }

            bool readSucc = true;
            try
            {
                foreach (ModbusDataBlockReadSegment readSegment in dataBlock.ReadSegmentsList)
                {
                    bool[] readResultBits = null;
                    ushort[] readResultUShort = null;

                    int numberOfPoints = 1;
                    switch (dataBlock.TableType)
                    {
                        case TableType.Input:
                        case TableType.Output:
                            int startAddress = readSegment.StartIndex * 8;
                            numberOfPoints = readSegment.ReadLength * 8;
                            if (readSegment.FirstItemBitNo.HasValue && readSegment.FirstItemBitNo.Value > 0)
                            {
                                startAddress += readSegment.FirstItemBitNo.Value;
                                numberOfPoints -= readSegment.FirstItemBitNo.Value;
                            }
                            if (readSegment.LastItemBitNo.HasValue && readSegment.LastItemBitNo.Value < 7)
                                numberOfPoints = numberOfPoints - 7 + readSegment.LastItemBitNo.Value;
                            if (dataBlock.TableType == TableType.Input)
                            {
                                using (ACMonitor.Lock(_11900_SocketLockObj))
                                {
                                    readResultBits = PLCConn.ReadInputs(slaveUnitID, System.Convert.ToUInt16(startAddress), System.Convert.ToUInt16(numberOfPoints)); // * 8 Bits
                                }
                                dataBlock.RefreshItems(ref readResultBits, readSegment);
                            }
                            else
                            {
                                using (ACMonitor.Lock(_11900_SocketLockObj))
                                {
                                    readResultBits = PLCConn.ReadCoils(slaveUnitID, System.Convert.ToUInt16(startAddress), System.Convert.ToUInt16(numberOfPoints)); // * 8 Bits
                                }
                                dataBlock.RefreshItems(ref readResultBits, readSegment);
                            }
                            break;
                        case TableType.ReadOnlyRegister:
                        case TableType.ReadWriteRegister:
                            ushort readStartIndex = System.Convert.ToUInt16(readSegment.StartIndex / 2);
                            //if (readStartIndex <= 0)
                                //readStartIndex = 1;
                            if (readSegment.ReadLength >= 2)
                            {
                                int count = readSegment.ReadLength / 2;
                                if (readSegment.ReadLength % 2 != 0)
                                    count++;
                                numberOfPoints = count;
                            }
                            if (dataBlock.TableType == TableType.ReadOnlyRegister)
                            {
                                using (ACMonitor.Lock(_11900_SocketLockObj))
                                {
                                    readResultUShort = PLCConn.ReadInputRegisters(slaveUnitID, readStartIndex, System.Convert.ToUInt16(numberOfPoints)); // multiply by 2 because Registers have a length of 2 bytes = 16bits
                                }
                            }
                            else
                            {
                                using (ACMonitor.Lock(_11900_SocketLockObj))
                                {
                                    readResultUShort = PLCConn.ReadHoldingRegisters(slaveUnitID, readStartIndex, System.Convert.ToUInt16(numberOfPoints));
                                }
                            }
                            dataBlock.RefreshItems(ref readResultUShort, readSegment.StartIndex);
                            break;
                    }
                    //if (readResult.Length >= readSegment.ReadLength)
                    //{
                    //    dataBlock.RefreshItems(ref readResult, readSegment.StartIndex);
                    //}
                    //else
                    //{
                    //    dataBlock.RefreshItems(ref readResult, readSegment.StartIndex);
                    //    // TODO: Check ob DB überhaupt so groß ??, Ist das so richtig?
                    //    Messages.LogFailure(this.GetACUrl(), "ModbusSubscr.ReadFromPLC(ModbusDataBlock)", String.Format("DataBlock-Size in only PLC {0}, Requested Size was {1}", readResult.Length, dataBlock.RequestedSize));
                    //}
                }
            }
            catch (Exception e)
            {
                bool logMessage = dataBlock.ReadErrorMessage != e.Message;
                if (logMessage)
                {
                    dataBlock.ReadErrorMessage = e.Message;
                    Messages.LogFailure(this.GetACUrl(), "ModbusSubscr.ReadFromPLC(ModbusDataBlock)", "No Data received from PLC: " + e.Message);
                }
                readSucc = false;
                if (_TCPClient == null || !_TCPClient.Connected)
                {
                    using (ACMonitor.Lock(_11900_SocketLockObj))
                    {
                        if (_PLCConn != null)
                            _PLCConn.Dispose();
                        _PLCConn = null;
                        _TCPClient = null;
                    }
                    IsConnected.ValueT = false;
                    InternalDisconnect(true);
                    StartReconnection();
                }
            }
            if (readSucc)
            {
                dataBlock.ReadErrorMessage = "";
            }

            return readSucc;
        }
#endregion

#region Sending
        private ModbusItems2SendQueue _Items2Send = new ModbusItems2SendQueue();
        protected ModbusItems2SendQueue Items2Send
        {
            get
            {
                if (_Items2Send == null)
                    _Items2Send = new ModbusItems2SendQueue();
                return _Items2Send;
            }
        }

        internal bool SendItem(ModbusItem s7Item)
        {
            if (!IsConnected.ValueT || !IsReadyForWriting || (ACOperationMode != ACOperationModes.Live))
                return false;
            if (Items2Send == null)
                return false;
            if (!_syncSend.NewItemsEnqueueable)
                return false;

            bool enqueued = Items2Send.Enqueue(s7Item, WriteInChronolgicalOrder);
            // Signalisiere Thread, dass neuer Send-Auftrag ansteht
            if (enqueued)
                _syncSend.NewItemEvent.Set();

            return enqueued;
        }

        private List<ModbusDataBlock> GetAffectedDatablocks(IEnumerable<ModbusItems2SendEntry> sortedItems2Send)
        {
            List<ModbusDataBlock> affectedBlocks = new List<ModbusDataBlock>();
            int lastDBNo = -9999999;
            ModbusSubscr lastSubscr = null;
            foreach (ModbusItems2SendEntry entry in sortedItems2Send)
            {
                if (lastSubscr != entry.Item.ParentSubscription || lastDBNo != entry.Item.ItemDBNo)
                {
                    lastDBNo = entry.Item.ItemDBNo;
                    lastSubscr = entry.Item.ParentSubscription;
                    ModbusDataBlock block = null;
                    if (entry.Item.ParentSubscription.PLCRAMOfDataBlocks.TryGetValue(entry.Item.ItemDBNo, out block))
                    {
                        if (block != null && !affectedBlocks.Contains(block))
                            affectedBlocks.Add(block);
                    }
                }
            }
            return affectedBlocks;
        }

        private void SendPLC()
        {
            while (!_syncSend.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _syncSend.NewItemEvent.WaitOne();

                // Sammle zuerst ein paar Events
                Thread.Sleep(100);

                if ((Items2Send == null) || (Root == null))
                    continue;
                _sendThread.StartReportingExeTime();
                List<ModbusDataBlock> affectedBlocks = null;
                List<ModbusItems2SendEntry> itemList2Send = Items2Send.GetAllEntrys(true);
                IEnumerable<ModbusItems2SendEntry> sortedList2Send = itemList2Send;
                if ((WriteMode == WriteModeModbus.AllNeigboursWithLatestValue)
                    || (WriteMode == WriteModeModbus.DataBlockLatestValue))
                {
                    sortedList2Send = itemList2Send.OrderBy(b => b.Item.ParentSubscription).ThenBy(c => c.Item.ItemDBNo).ThenBy(d => d.Item.ItemStartByteAddr).ThenBy(f => f.Order);
                    affectedBlocks = GetAffectedDatablocks(sortedList2Send);
                }
                else
                {
                    affectedBlocks = GetAffectedDatablocks(itemList2Send.OrderBy(b => b.Item.ParentSubscription).ThenBy(c => c.Item.ItemDBNo));
                }

                try
                {
                    if (affectedBlocks != null)
                    {
                        foreach (ModbusDataBlock affectedBlock in affectedBlocks)
                        {
                            affectedBlock.SetLockReadUpdates();
                        }
                    }
                    ModbusItems2SendEntry prevEntry = null;
                    ModbusItemsSendPackage package2Send = new ModbusItemsSendPackage();
                    foreach (ModbusItems2SendEntry entry in sortedList2Send)
                    {
                        if (prevEntry != null)
                        {
                            if (WriteMode == WriteModeModbus.Separately)
                            {
                                if (package2Send.ExistsAPreviousEntry(entry.Item) ||
                                    !package2Send.IsItemADirectNeighbour(entry.Item))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new ModbusItemsSendPackage();
                                }
                            }
                            else if (WriteMode == WriteModeModbus.AllNeigboursWithLatestValue)
                            {
                                if (!package2Send.IsItemADirectNeighbour(entry.Item))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new ModbusItemsSendPackage();
                                }
                            }
                            else if (WriteMode == WriteModeModbus.DataBlockLatestValue)
                            {
                                if ((prevEntry.Item.ParentSubscription != entry.Item.ParentSubscription)
                                    || (prevEntry.Item.ItemDBNo != entry.Item.ItemDBNo))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new ModbusItemsSendPackage();
                                }
                            }
                        }
                        package2Send.Add(entry);
                        package2Send.MarkItemsAsWritten();
                        prevEntry = entry;
                    }
                    SendPackage(package2Send);
                    package2Send.MarkItemsAsWritten();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ModbusSession", "SendPLC", msg);
                }
                finally
                {
                    if (affectedBlocks != null)
                    {
                        foreach (ModbusDataBlock affectedBlock in affectedBlocks)
                        {
                            affectedBlock.ReleaseLockReadUpdates();
                        }
                    }
                }
                _sendThread.StopReportingExeTime();
            }
            _syncSend.ThreadTerminated();
        }

        private void SendPackage(ModbusItemsSendPackage package2Send)
        {
            if (package2Send.TableType == TableType.ReadOnlyRegister || package2Send.TableType == TableType.Input)
                return;
            if (_TCPClient == null || !_TCPClient.Connected || PLCConn == null)
                return;
            try
            {
                IEnumerable<ModbusItemsSendPackageSegment> array2Send = package2Send.BuildTransferArray(WriteInChronolgicalOrder);
                if (array2Send != null)
                {
                    bool writeSucc = true;
                    foreach (ModbusItemsSendPackageSegment segment in array2Send)
                    {
                        if (package2Send.TableType == TableType.Output)
                        {
                            int startAddress = segment.StartIndex * 8;
                            //int numberOfPoints = readSegment.ReadLength * 8;
                            short? removeBitsFromStart = null;
                            if (segment.FirstBooleanItem != null && segment.FirstBooleanItem.Item.ItemBitNo > 0)
                            {
                                startAddress += segment.FirstBooleanItem.Item.ItemBitNo;
                                removeBitsFromStart = segment.FirstBooleanItem.Item.ItemBitNo;
                            }
                            short? removeBitsFromEnd = null;
                            if (segment.LastBooleanItem != null && segment.LastBooleanItem.Item.ItemBitNo < 7)
                                removeBitsFromEnd = System.Convert.ToInt16(7 - segment.LastBooleanItem.Item.ItemBitNo);

                            using (ACMonitor.Lock(_11900_SocketLockObj))
                            {
                                if (PLCConn != null)
                                {
                                    PLCConn.WriteMultipleCoils(package2Send.SlaveUnitID, System.Convert.ToUInt16(startAddress), modbus.Types.Byte.Convert(segment.WriteSegment, removeBitsFromStart, removeBitsFromEnd));
                                    lock (_LogQueueLock)
                                    {
                                        if (_LogQueue != null)
                                        {
                                            _LogQueue.Put(new Tuple<DateTime, ModbusItemsSendPackageSegment, int>(DateTime.Now, segment, package2Send.DBNo));
                                        }
                                    }
                                }
                            }
                        }
                        else if (package2Send.TableType == TableType.ReadWriteRegister)
                        {
                            using (ACMonitor.Lock(_11900_SocketLockObj))
                            {
                                if (PLCConn != null)
                                {
                                    PLCConn.WriteMultipleRegisters(package2Send.SlaveUnitID, System.Convert.ToUInt16(segment.StartIndex / 2), modbus.Types.Word.ToArray(segment.WriteSegment, Endianess));
                                    lock (_LogQueueLock)
                                    {
                                        if (_LogQueue != null)
                                        {
                                            _LogQueue.Put(new Tuple<DateTime, ModbusItemsSendPackageSegment, int>(DateTime.Now, segment, package2Send.DBNo));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (writeSucc)
                    {
                        ModbusDataBlock s7DataBlock = package2Send.ParentSubscription.PLCRAMOfDataBlocks[package2Send.DBNo];
                        s7DataBlock.WriteErrorMessage = "";
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogFailure(this.GetACUrl(), "ModbusSubscr.SendPackage(ModbusDataBlock)", "No Data send to PLC: " + e.Message);
                if (!_TCPClient.Connected)
                {
                    using (ACMonitor.Lock(_11900_SocketLockObj))
                    {
                        if (_PLCConn != null)
                            _PLCConn.Dispose();
                        _PLCConn = null;
                        _TCPClient = null;
                    }
                    IsConnected.ValueT = false;
                    InternalDisconnect(true);
                    StartReconnection();
                }
            }
        }

#endregion

    }
}
