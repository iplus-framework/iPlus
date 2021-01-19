using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;
using System.Timers;
using System.Threading;
using System.IO;

namespace gip.core.communication
{
    /// <summary>
    /// Endianess. Windows stores as LitteEndian. Therefore the examples of byte values means: 0A is least significant byte...3D is most significant byte
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'HashCodeValidation'}de{'HashCodeValidation'}", Global.ACKinds.TACEnum)]
    public enum HashCodeValidationEnum : short
    {
        /// <summary>
        /// Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Write Hashcode at END of Telegram/Datablock-Stuct
        /// </summary>
        End = 1,

        /// <summary>
        /// Write Hashcode at END of Telegram/Datablock-Stuct and Read Hashcode from PLC to verify if data was send
        /// </summary>
        End_WithRead = 2,

        /// <summary>
        /// Write Hashcode at HEAD of Telegram/Datablock-Stuct
        /// </summary>
        Head = 3,

        /// <summary>
        /// Write Hashcode at HEAD of Telegram/Datablock-Stuct and Read Hashcode from PLC to verify if data was send
        /// </summary>
        Head_WithRead = 4
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'S7TCPSession'}de{'S7TCPSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class S7TCPSession : ACSession
    {
        #region c´tors
        public S7TCPSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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
                _Items2Send = new S7TCPItems2SendQueue();
            if (_PollingPlan == null)
                _PollingPlan = new S7TCPPollingPlan();
            _DelegateConnAlarmOccurred = false;
            _DelegateConnAlarmDisappeared = false;

            if (!base.ACInit(startChildMode))
                return false;

            _UsePingForConnectTest = new ACPropertyConfigValue<bool>(this, "UsePingForConnectTest", true);

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
                if (this.CommService != null)
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
            if (_PLCConn != null)
                _PLCConn.Close();
            _PLCConn = null;
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

        [ACPropertyInfo(9999, DefaultValue = CPU_Type.S7400)]
        public CPU_Type CPU
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = (Int16)0)]
        public Int16 Rack
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = (Int16)2)]
        public Int16 Slot
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = WriteMode.Separately)]
        public WriteMode WriteMode
        {
            get;
            set;
        }

        private bool WriteInChronolgicalOrder
        {
            get
            {
                return this.WriteMode == WriteMode.Separately ? true : false;
            }
        }

        [ACPropertyInfo(9999, DefaultValue = EndianessEnum.BigEndian)]
        public EndianessEnum Endianess
        {
            get;
            set;
        }


        [ACPropertyInfo(9999, DefaultValue = HashCodeValidationEnum.Off)]
        public HashCodeValidationEnum HashCodeValidation
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public string PLCUrl
        {
            get
            {
                if (String.IsNullOrEmpty(IPAddress))
                    return "";

                return string.Format("opcda://{0}:{1}@{2},{3}", IPAddress, gip.core.communication.ISOonTCP.PLC.RFC1006Port, Rack, Slot);
            }
        }

        gip.core.communication.ISOonTCP.PLC _PLCConn = null;
        public gip.core.communication.ISOonTCP.PLC PLCConn
        {
            get
            {
                return _PLCConn;
            }
        }

        public bool IsConnectionLocalSim
        {
            get
            {
                return IPAddress == "localhost" || IPAddress == "127.0.0.1" || IPAddress == "172.0.0.1";
            }
        }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> ReadError { get; set; }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> WriteError { get; set; }


        private ACPropertyConfigValue<bool> _UsePingForConnectTest;
        [ACPropertyConfig("en{'Use ping for connection test'}de{'Verwende ping für Verbindungstest'}")]
        public bool UsePingForConnectTest
        {
            get
            {
                return _UsePingForConnectTest.ValueT;
            }
            set
            {
                _UsePingForConnectTest.ValueT = value;
            }
        }


        private CircularBuffer<Tuple<DateTime, S7TCPItemsSendPackageSegment, int>> _LogQueue = null;
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
                Messages.LogDebug(this.GetACUrl(), "S7TCPSession.InitSession()", "Start InitSession");
            if (_PLCConn != null)
                return true;

            // 1. Instanz der Session erzeugen
            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "S7TCPSession.InitSession()", "New Session");
            _PLCConn = new gip.core.communication.ISOonTCP.PLC(CPU, IPAddress, Rack, Slot, Endianess, UsePingForConnectTest);
            _PLCConn.PropertyChanged += OnPLCConn_PropertyChanged;

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "S7TCPSession.InitSession()", "Init Subscriptions");
            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is S7TCPSubscr)
                {
                    S7TCPSubscr acSubscription = child as S7TCPSubscr;
                    acSubscription.InitSubscription();
                    acSubscription.PropertyChanged += OnACSubscription_PropertyChanged;
                }
            }

            if (_ReconnectTries <= 0)
                Messages.LogDebug(this.GetACUrl(), "S7TCPSession.InitSession()", "Init Completed");
            return true;
        }

        public override bool IsEnabledInitSession()
        {
            if (_PLCConn != null)
                return false;
            if (String.IsNullOrEmpty(PLCUrl))
                return false;
            return true;
        }

        public override bool DeInitSession()
        {
            if (_PLCConn == null)
                return true;

            // 1. Disconnect
            if (!DisConnect())
                return false;

            // 2. Remove Events
            _PLCConn.PropertyChanged -= OnPLCConn_PropertyChanged;

            // 3. Deinit Subscriptions. (Is already done, when called ACDeInit())
            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is S7TCPSubscr)
                {
                    S7TCPSubscr acSubscription = child as S7TCPSubscr;
                    acSubscription.PropertyChanged -= OnACSubscription_PropertyChanged;
                    acSubscription.DeInitSubscription();
                }
            }

            _PLCConn = null;
            return true;
        }

        public override bool IsEnabledDeInitSession()
        {
            if (_PLCConn == null)
                return false;
            return true;
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
            if (_PLCConn.IsConnected)
                return true;

            ErrorCode res = ErrorCode.NoError;

            using (ACMonitor.Lock(_PLCConn._11900_SocketLockObj))
            {
                if (_PLCConn.IsConnected)
                    return true;
                if (_PLCConn.IP != IPAddress)
                    _PLCConn.IP = IPAddress;
                if (_PLCConn.Rack != Rack)
                    _PLCConn.Rack = Rack;
                else if (_PLCConn.Slot != Slot)
                    _PLCConn.Slot = Slot;
                if (_ReconnectTries <= 0)
                    Messages.LogDebug(this.GetACUrl(), "S7TCPSession.Connect(0)", "Start Connect");
                res = _PLCConn.Open();
            }
            if (res != ErrorCode.NoError)
            {
                if (_ReconnectTries <= 0)
                    Messages.LogDebug(this.GetACUrl(), "S7TCPSession.Connect(1)", "Not Connected: " + _PLCConn.lastErrorString);
                _ReconnectTries++;
                return false;
            }
            _ReconnectTries = 0;
            Messages.LogDebug(this.GetACUrl(), "S7TCPSession.Connect(2)", "Connected");

            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is S7TCPSubscr)
                {
                    S7TCPSubscr acSubscription = child as S7TCPSubscr;
                    acSubscription.Connect();
                }
            }
            return PLCConn.IsConnected;
        }

        public override bool IsEnabledConnect()
        {
            if (!base.IsEnabledConnect())
                return false;
            if (_PLCConn == null)
                return true;
            if (_PLCConn.IsConnected)
                return false;
            return true;
        }

        private bool _ManuallyDisConnected = false;
        public override bool DisConnect()
        {
            if (!IsEnabledDisConnect())
                return true;
            if (_PLCConn == null)
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
                if (child is S7TCPSubscr)
                {
                    S7TCPSubscr acSubscription = child as S7TCPSubscr;
                    acSubscription.DisConnect();
                }
            }

            if (calledFromPropChanged)
                return true;



            using (ACMonitor.Lock(_PLCConn._11900_SocketLockObj))
            {
                if (_PLCConn.IsConnected)
                {
                    _PLCConn.Close();
                    if (_PLCConn.IsConnected)
                        return false;
                    return true;
                }
            }
            return true;
        }

        public override bool IsEnabledDisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (_PLCConn == null)
                return false;
            if (_PLCConn.IsConnected)
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
                    _LogQueue = new CircularBuffer<Tuple<DateTime, S7TCPItemsSendPackageSegment, int>>(1000, true);
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
            CircularBuffer<Tuple<DateTime, S7TCPItemsSendPackageSegment, int>> logQueueCopy = null;
            lock (_LogQueueLock)
            {
                logQueueCopy = _LogQueue;
                _LogQueue = new CircularBuffer<Tuple<DateTime, S7TCPItemsSendPackageSegment, int>>(1000, true);
            }
            if (logQueueCopy != null)
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (Tuple<DateTime, S7TCPItemsSendPackageSegment, int> tuple in logQueueCopy)
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

                        string fileName = String.Format("S7Dump_{0:yyyyMMdd_HHmmss}_{1}.txt", DateTime.Now, Guid.NewGuid().GetHashCode());
                        fileName = Path.Combine(Path.GetTempPath(), fileName);
                        File.WriteAllText(fileName, sb.ToString());
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("S7TCPSession", "DumpLog", msg);
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

        void OnPLCConn_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsConnected" && (ACOperationMode == ACOperationModes.Live))
            {
                IsConnected.ValueT = PLCConn.IsConnected;
                if (!PLCConn.IsConnected)
                    InternalDisconnect(true);
                if (!_ManuallyDisConnected && !PLCConn.IsConnected && AutoReconnect && _ReconnectThread == null)
                    StartReconnection();
            }
            /*else if (e.PropertyName == "ReadError")
            {
                ReadError.ValueT = PLCConn.ReadError;
            }
            else if (e.PropertyName == "WriteError")
            {
                WriteError.ValueT = PLCConn.WriteError;
            }*/
        }

        void OnACSubscription_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReadAlarm")
            {
                bool hasReadErrors = false;
                foreach (IACComponent child in this.ACComponentChilds)
                {
                    if (child is S7TCPSubscr)
                    {
                        S7TCPSubscr acSubscription = child as S7TCPSubscr;
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
                    if (child is S7TCPSubscr)
                    {
                        S7TCPSubscr acSubscription = child as S7TCPSubscr;
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
            if (ACOperationMode == ACOperationModes.Live && !_ManuallyDisConnected && !PLCConn.IsConnected && AutoReconnect && _ReconnectThread == null)
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
                if (PLCConn.IsConnected)
                {
                    InitiateStopReconnection();
                    continue;
                }

                _ReconnectThread.StartReportingExeTime();
                try
                {
                    if (_PLCConn != null && !PLCConn.IsConnected)
                        Connect();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "Reconnect(0)", e.Message);
                    Messages.LogException(this.GetACUrl(), "Reconnect(1)", e.StackTrace);
                }
                _ReconnectThread.StopReportingExeTime();

                if (PLCConn.IsConnected)
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
                        datamodel.Database.Root.Messages.LogException("S7TCPSession", "HandlePropertyChangedDelegate", msg);
                }
                finally
                {
                }
            }
        }

#endregion

#region Polling
        private S7TCPPollingPlan _PollingPlan = new S7TCPPollingPlan();
        protected S7TCPPollingPlan PollingPlan
        {
            get
            {
                if (_PollingPlan == null)
                    _PollingPlan = new S7TCPPollingPlan();
                return _PollingPlan;
            }
        }

        internal bool AddToPollingPlan(S7TCPSubscr subscription, int dbNo = -10)
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
                List<S7TCPPollingPlanEntry> PollingPlanList = PollingPlan.GetAllEntrys(true);
                PollingPlanList.ForEach(c => ReadFromPLC(c));
                _pollThread.StopReportingExeTime();
            }
            _syncPoll.ThreadTerminated();
        }

        private void ReadFromPLC(S7TCPPollingPlanEntry pollEntry)
        {
            if (pollEntry.DBNo <= -10)
            {
                bool allBlocksPolledSuccessfully = true;
                foreach (S7TCPDataBlock dataBlock in pollEntry.Subscr.PLCRAMOfDataBlocks.DataBlocks)
                {
                    if (!ReadFromPLC(dataBlock))
                        allBlocksPolledSuccessfully = false;
                }
                if (!pollEntry.Subscr.IsReadyForWriting && allBlocksPolledSuccessfully)
                    pollEntry.Subscr.IsReadyForWriting = true;
            }
            else
            {
                try
                {
                    ReadFromPLC(pollEntry.Subscr.PLCRAMOfDataBlocks[pollEntry.DBNo]);
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "S7TCPSubscr.ReadFromPLC(S7TCPPollingPlanEntry)", ex.Message);
                }
            }
        }

        private bool ReadFromPLC(S7TCPDataBlock dataBlock)
        {
            if ((dataBlock.RequestedSize <= 0) || (!IsConnected.ValueT))
                return false;
            if (dataBlock.RequestedSize > 50000)
            {
                Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.ReadFromPLC(S7TCPDataBlock)", String.Format("Requested Size {0} is to large", dataBlock.RequestedSize));
                return false;
            }

            //String perfUrl = String.Format("{0}.DB{1}",ACIdentifier, dataBlock.DBNo);
            //var vbDump = this.Root.VBDump;
            //PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLogger.Start(perfUrl, 100) : null;
            
            bool readSucc = true;
            foreach (S7TCPDataBlockReadSegment readSegment in dataBlock.ReadSegmentsList)
            {
                byte[] readResult;
                ErrorCode plcError = PLCConn.ReadBytes(dataBlock.S7DataType, dataBlock.DBNoForISOonTCP, readSegment.StartIndex, readSegment.ReadLength, out readResult);
                if (plcError == ErrorCode.NoError)
                {
                    if (readResult.Length >= readSegment.ReadLength)
                    {
                        dataBlock.RefreshItems(ref readResult, readSegment.StartIndex);
                    }
                    else
                    {
                        dataBlock.RefreshItems(ref readResult, readSegment.StartIndex);
                        // TODO: Check ob DB überhaupt so groß ??, Ist das so richtig?
                        Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.ReadFromPLC(S7TCPDataBlock)", String.Format("DataBlock-Size in only DB{0}, Start: {1}, PLC {2}, Requested Size was {3}", dataBlock.DBNoForISOonTCP, readSegment.StartIndex, readResult.Length, dataBlock.RequestedSize));
                    }
                }
                else if (plcError == ErrorCode.DBRangeToSmall || plcError == ErrorCode.DBNotExist)
                {
                    dataBlock.ReadErrorMessage = PLCConn.lastErrorString;
                    Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.ReadFromPLC(S7TCPDataBlock)", String.Format("No Data received from PLC. DB{0}, Start: {1}, Length: {2}, ErrorString: {3} ", dataBlock.DBNoForISOonTCP, readSegment.StartIndex, readSegment.ReadLength, PLCConn.lastErrorString));
                    readSucc = false;
                    break;
                }
                else
                {
                    dataBlock.ReadErrorMessage = PLCConn.lastErrorString;
                    Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.ReadFromPLC(S7TCPDataBlock)", String.Format("No Data received from PLC. DB{0}, Start: {1}, Length: {2}, ErrorString: {3} ", dataBlock.DBNoForISOonTCP, readSegment.StartIndex, readSegment.ReadLength, PLCConn.lastErrorString));
                    readSucc = false;
                    break;
                }
            }
            if (readSucc)
            {
                dataBlock.ReadErrorMessage = "";
            }

            //if (perfEvent != null)
            //    vbDump.PerfLogger.Stop(perfUrl, 100, perfEvent);

            return readSucc;
        }
#endregion

#region Sending
        private S7TCPItems2SendQueue _Items2Send = new S7TCPItems2SendQueue();
        protected S7TCPItems2SendQueue Items2Send
        {
            get
            {
                if (_Items2Send == null)
                    _Items2Send = new S7TCPItems2SendQueue();
                return _Items2Send;
            }
        }

        internal bool SendItem(S7TCPItem s7Item)
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

        private List<S7TCPDataBlock> GetAffectedDatablocks(IEnumerable<S7TCPItems2SendEntry> sortedItems2Send)
        {
            List<S7TCPDataBlock> affectedBlocks = new List<S7TCPDataBlock>();
            int lastDBNo = -9999999;
            S7TCPSubscr lastSubscr = null;
            foreach (S7TCPItems2SendEntry entry in sortedItems2Send)
            {
                if (lastSubscr != entry.Item.ParentSubscription || lastDBNo != entry.Item.ItemDBNo)
                {
                    lastDBNo = entry.Item.ItemDBNo;
                    lastSubscr = entry.Item.ParentSubscription;
                    S7TCPDataBlock block = null;
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
                List<S7TCPDataBlock> affectedBlocks = null;
                List<S7TCPItems2SendEntry> itemList2Send = Items2Send.GetAllEntrys(true);

                IEnumerable<S7TCPItems2SendEntry> sortedList2Send = itemList2Send;
                if ((WriteMode == ISOonTCP.WriteMode.AllNeigboursWithLatestValue)
                    || (WriteMode == ISOonTCP.WriteMode.DataBlockLatestValue))
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
                        foreach (S7TCPDataBlock affectedBlock in affectedBlocks)
                        {
                            affectedBlock.SetLockReadUpdates();
                        }
                    }

                    S7TCPItems2SendEntry prevEntry = null;
                    S7TCPItemsSendPackage package2Send = new S7TCPItemsSendPackage();
                    foreach (S7TCPItems2SendEntry entry in sortedList2Send)
                    {
                        if (prevEntry != null)
                        {
                            if (WriteMode == ISOonTCP.WriteMode.Separately)
                            {
                                if (package2Send.ExistsAPreviousEntry(entry.Item) ||
                                    !package2Send.IsItemADirectNeighbour(entry.Item))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new S7TCPItemsSendPackage();
                                }
                            }
                            else if (WriteMode == ISOonTCP.WriteMode.AllNeigboursWithLatestValue)
                            {
                                if (!package2Send.IsItemADirectNeighbour(entry.Item))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new S7TCPItemsSendPackage();
                                }
                            }
                            else if (WriteMode == ISOonTCP.WriteMode.DataBlockLatestValue)
                            {
                                if ((prevEntry.Item.ParentSubscription != entry.Item.ParentSubscription)
                                    || (prevEntry.Item.ItemDBNo != entry.Item.ItemDBNo))
                                {
                                    SendPackage(package2Send);
                                    package2Send.MarkItemsAsWritten();
                                    package2Send = new S7TCPItemsSendPackage();
                                }
                            }
                        }
                        package2Send.Add(entry);
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
                        datamodel.Database.Root.Messages.LogException("S7TCPSession", "SendPLC", msg);
                }
                finally
                {
                    if (affectedBlocks != null)
                    {
                        foreach (S7TCPDataBlock affectedBlock in affectedBlocks)
                        {
                            affectedBlock.ReleaseLockReadUpdates();
                        }
                    }
                }
                _sendThread.StopReportingExeTime();
            }
            _syncSend.ThreadTerminated();
        }

        private void SendPackage(S7TCPItemsSendPackage package2Send)
        {
            IEnumerable<S7TCPItemsSendPackageSegment> array2Send = package2Send.BuildTransferArray(WriteInChronolgicalOrder);
            if (array2Send != null)
            {
                bool writeSucc = true;
                foreach (S7TCPItemsSendPackageSegment segment in array2Send)
                {
                    ErrorCode plcError = PLCConn.WriteBytes(DataType.DataBlock, package2Send.DBNo, segment.StartIndex, ref segment._WriteSegment);
                    lock (_LogQueueLock)
                    {
                        if (_LogQueue != null)
                        {
                            _LogQueue.Put(new Tuple<DateTime, S7TCPItemsSendPackageSegment, int>(DateTime.Now, segment, package2Send.DBNo));
                        }
                    }
                    if (plcError != ErrorCode.NoError)
                    {
                        if (plcError == ErrorCode.DBRangeToSmall || plcError == ErrorCode.DBNotExist)
                        {
                            S7TCPDataBlock s7DataBlock = package2Send.ParentSubscription.PLCRAMOfDataBlocks[package2Send.DBNo];
                            s7DataBlock.WriteErrorMessage = PLCConn.lastErrorString;
                            Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.SendPackage(S7TCPDataBlock)", "No Data send to PLC: " + PLCConn.lastErrorString);
                        }
                        else
                        {
                            S7TCPDataBlock s7DataBlock = package2Send.ParentSubscription.PLCRAMOfDataBlocks[package2Send.DBNo];
                            s7DataBlock.WriteErrorMessage = PLCConn.lastErrorString;
                            Messages.LogFailure(this.GetACUrl(), "S7TCPSubscr.SendPackage(S7TCPDataBlock)", "No Data send to PLC: " + PLCConn.lastErrorString);
                        }
                        writeSucc = false;
                        break;
                    }
                }
                if (writeSucc)
                {
                    S7TCPDataBlock s7DataBlock = package2Send.ParentSubscription.PLCRAMOfDataBlocks[package2Send.DBNo];
                    s7DataBlock.WriteErrorMessage = "";
                }
            }
        }

#endregion

    }
}
