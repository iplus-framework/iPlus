﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using gip.core.tcShared.ACVariobatch;

namespace gip.core.tcShared.WCF
{
    public class WcfAdsServiceChannel
    {
        #region Private members

        WcfAdsService _wcfAdsService;

        SyncQueueEvents _syncSend;
        Thread _sendThread;

        SyncQueueEvents _syncReceive;
        Thread _recieveThread;

        WcfAdsMessage _MetadataMessage;
        WcfAdsMessage _MemoryMessage;
        WcfAdsMessage _ConnectionState;
        WcfAdsMessage _PAFuncResult;

        Dictionary<int, ACRMemoryByteEvent> _dictByteEvent;
        Dictionary<int, ACRMemoryUIntEvent> _dictUIntEvent;
        Dictionary<int, ACRMemoryIntEvent> _dictIntEvent;
        Dictionary<int, ACRMemoryDIntEvent> _dictDIntEvent;
        Dictionary<int, ACRMemoryUDIntEvent> _dictUDIntEvent;
        Dictionary<int, ACRMemoryRealEvent> _dictRealEvent;
        Dictionary<int, ACRMemoryLRealEvent> _dictLRealEvent;
        Dictionary<int, ACRMemoryStringEvent> _dictStringEvent;
        Dictionary<int, ACRMemoryTimeEvent> _dictTimeEvent;
        Dictionary<int, ACRMemoryDTEvent> _dictDTEvent;

        MonitorObject _byteLock20010 = new MonitorObject(20010);
        MonitorObject _uIntLock20020 = new MonitorObject(20020);
        MonitorObject _intLock20030 = new MonitorObject(20030);
        MonitorObject _dIntLock20040 = new MonitorObject(20040);
        MonitorObject _uDIntLock20050 = new MonitorObject(20050);
        MonitorObject _realLock20060 = new MonitorObject(20060);
        MonitorObject _lRealLock20070 = new MonitorObject(20070);
        MonitorObject _stringLock20080 = new MonitorObject(20080);
        MonitorObject _timeLock20090 = new MonitorObject(20090);
        MonitorObject _dTLock20100 = new MonitorObject(20100);

        #endregion

        #region c'tors

        public WcfAdsServiceChannel(WcfAdsService wcfAdsService)
        {
            _wcfAdsService = wcfAdsService;

            _syncSend = new SyncQueueEvents();
            _sendThread = new Thread(SendMessage);
            _sendThread.Name = "SendThread";
            _sendThread.Start();

            _syncReceive = new SyncQueueEvents();
            _recieveThread = new Thread(RecieveMessage);
            _recieveThread.Name = "ReceiveThread";
            _recieveThread.Start();
        }

        #endregion

        #region Properties

        public bool ClosingConnection
        {
            get
            {
                if (_wcfAdsService == null || _wcfAdsService.ClosingConnection)
                    return true;
                return false;
            }
        }

        #endregion

        #region Sending

        void AddNewEvents<T>(int currentEventIndex, int nextEventIndex, T[] events, out Dictionary<int,T> eventsDict)
        {
            int eventsSize = events.Count();
            eventsDict = new Dictionary<int, T>();
            for (int i = currentEventIndex; i < nextEventIndex; i++)
            {
                if (i == eventsSize)
                    i = 0;
                if (nextEventIndex > eventsSize)
                    nextEventIndex = 0;
                T outEvent;
                if(!eventsDict.TryGetValue(events[i].GetHashCode(), out outEvent))
                    eventsDict.Add(events[i].GetHashCode(), events[i]);
                else //problem with same events in dictionary
                    eventsDict[outEvent.GetHashCode()] = events[i];  
            }
        }

        void BufferEvents<T>(int currentEventIndex, int nextEventIndex, T[] events, Dictionary<int, T> eventsDict)
        {
            if (eventsDict == null)
            {
                AddNewEvents<T>(currentEventIndex, nextEventIndex, events, out eventsDict);
                return;
            }
            int eventsSize = events.Count();
            for (int i = currentEventIndex; i < nextEventIndex; i++)
            {
                if (i == eventsSize)
                    i = 0;
                if (nextEventIndex > eventsSize)
                    nextEventIndex = 0;
                T outEvent;
                if (eventsDict.TryGetValue(events[i].GetHashCode(), out outEvent))
                    eventsDict[outEvent.GetHashCode()] = events[i];
                else
                    eventsDict.Add(events[i].GetHashCode(), events[i]);
            }
        }

        public void AddToByteEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryByteEvent[] events)
        {
            lock (_byteLock20010)
            {
                if (_dictByteEvent == null)
                    AddNewEvents<ACRMemoryByteEvent>(currentEventIndex, nextEventIndex, events, out _dictByteEvent);
                else
                    BufferEvents<ACRMemoryByteEvent>(currentEventIndex, nextEventIndex, events, _dictByteEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToUIntEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryUIntEvent[] events)
        {
            lock (_uIntLock20020)
            {
                if (_dictByteEvent == null)
                    AddNewEvents<ACRMemoryUIntEvent>(currentEventIndex, nextEventIndex, events, out _dictUIntEvent);
                else
                    BufferEvents<ACRMemoryUIntEvent>(currentEventIndex, nextEventIndex, events, _dictUIntEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToIntEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryIntEvent[] events)
        {
            lock (_intLock20030)
            {
                if (_dictByteEvent == null)
                    AddNewEvents<ACRMemoryIntEvent>(currentEventIndex, nextEventIndex, events, out _dictIntEvent);
                else
                    BufferEvents<ACRMemoryIntEvent>(currentEventIndex, nextEventIndex, events, _dictIntEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToDIntEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryDIntEvent[] events)
        {
            lock (_dIntLock20040)
            {
                if (_dictByteEvent == null)
                    AddNewEvents<ACRMemoryDIntEvent>(currentEventIndex, nextEventIndex, events, out _dictDIntEvent);
                else
                    BufferEvents<ACRMemoryDIntEvent>(currentEventIndex, nextEventIndex, events, _dictDIntEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToUDIntEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryUDIntEvent[] events)
        {
            lock (_uDIntLock20050)
            {
                if (_dictByteEvent == null)
                    AddNewEvents<ACRMemoryUDIntEvent>(currentEventIndex, nextEventIndex, events, out _dictUDIntEvent);
                else
                    BufferEvents<ACRMemoryUDIntEvent>(currentEventIndex, nextEventIndex, events, _dictUDIntEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToRealEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryRealEvent[] events)
        {
            lock (_realLock20060)
            {
                if (_dictLRealEvent == null)
                    AddNewEvents<ACRMemoryRealEvent>(currentEventIndex, nextEventIndex, events, out _dictRealEvent);
                else
                    BufferEvents<ACRMemoryRealEvent>(currentEventIndex, nextEventIndex, events, _dictRealEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToLRealEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryLRealEvent[] events)
        {
            lock (_lRealLock20070)
            {
                if (_dictLRealEvent == null)
                    AddNewEvents<ACRMemoryLRealEvent>(currentEventIndex, nextEventIndex, events, out _dictLRealEvent);
                else
                    BufferEvents<ACRMemoryLRealEvent>(currentEventIndex, nextEventIndex, events, _dictLRealEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToStringEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryStringEvent[] events)
        {
            lock (_stringLock20080)
            {
                if (_dictLRealEvent == null)
                    AddNewEvents<ACRMemoryStringEvent>(currentEventIndex, nextEventIndex, events, out _dictStringEvent);
                else
                    BufferEvents<ACRMemoryStringEvent>(currentEventIndex, nextEventIndex, events, _dictStringEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToTimeEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryTimeEvent[] events)
        {
            lock (_timeLock20090)
            {
                if (_dictLRealEvent == null)
                    AddNewEvents<ACRMemoryTimeEvent>(currentEventIndex, nextEventIndex, events, out _dictTimeEvent);
                else
                    BufferEvents<ACRMemoryTimeEvent>(currentEventIndex, nextEventIndex, events, _dictTimeEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void AddToDTEventDict(int currentEventIndex, int nextEventIndex, ACRMemoryDTEvent[] events)
        {
            lock (_dTLock20100)
            {
                if (_dictLRealEvent == null)
                    AddNewEvents<ACRMemoryDTEvent>(currentEventIndex, nextEventIndex, events, out _dictDTEvent);
                else
                    BufferEvents<ACRMemoryDTEvent>(currentEventIndex, nextEventIndex, events, _dictDTEvent);
            }
            _syncSend.NewItemEvent.Set();
        }

        public void PrepareMetadataMessageForClient(ACRMemoryMetaObj[] message)
        {
            lock(_syncSend._20009_MetaSyncLock)
                _MetadataMessage = new WcfAdsMessage() { Metadata = message };

            _syncSend.NewItemEvent.Set();
        }

        public void PrepareMemoryMessageForClient(byte[] message)
        {
            lock (_syncSend._20011_QueueSyncLock)
                _MemoryMessage = new WcfAdsMessage() { Memory = message };

            _syncSend.NewItemEvent.Set();
        }

        public void SendConnectionState(ConnectionState connectionState)
        {
            WcfAdsMessage message = new WcfAdsMessage() { ConnectionState = (byte)connectionState };
            lock(_syncSend._20011_QueueSyncLock)
                _ConnectionState = message;
            _syncSend.NewItemEvent.Set();
        }

        public void SendPAFuncResult(byte[] result)
        {
            lock(_syncSend._20012_QueueSyncLock)
                _PAFuncResult = new WcfAdsMessage() { Result = result };
            _syncSend.NewItemEvent.Set();
        }

        void SendMessage()
        {
            if (_wcfAdsService == null && _wcfAdsService.ClosingConnection)
                return;

            while (!_syncSend.ExitThreadEvent.WaitOne(0, false))
            {
                _syncSend.NewItemEvent.WaitOne();

                if (_PAFuncResult != null || _MemoryMessage != null || _MetadataMessage != null || _ConnectionState != null)
                {
                    if (_MetadataMessage != null)
                    {
                        WcfAdsMessage messageMeta;
                        lock (_syncSend._20009_MetaSyncLock)
                        {
                            messageMeta = _MetadataMessage;
                            _MetadataMessage = null;
                        }
                        if (_wcfAdsService.ClosingConnection || messageMeta == null)
                            continue;
                        _wcfAdsService.TryInvoke(messageMeta);
                    }

                    else if (_MemoryMessage != null)
                    {
                        WcfAdsMessage messageMem;
                        lock (_syncSend._20010_QueueSyncLock)
                        {
                            messageMem = _MemoryMessage;
                            _MemoryMessage = null;
                        }
                        if (_wcfAdsService.ClosingConnection || messageMem == null)
                            continue;
                        _wcfAdsService.TryInvoke(messageMem);
                    }

                    else if (_ConnectionState != null)
                    {
                        WcfAdsMessage messageConn;
                        lock (_syncSend._20011_QueueSyncLock)
                        {
                            messageConn = _ConnectionState;
                            _ConnectionState = null;
                        }
                        if (_wcfAdsService.ClosingConnection || messageConn == null)
                            continue;
                        _wcfAdsService.TryInvoke(messageConn);
                    }
                    else if (_PAFuncResult != null)
                    {
                        WcfAdsMessage messageResult;
                        lock (_syncSend._20012_QueueSyncLock)
                        {
                            messageResult = _PAFuncResult;
                            _PAFuncResult = null;
                        }
                        if (_wcfAdsService.ClosingConnection || messageResult == null)
                            continue;
                        _wcfAdsService.TryInvoke(messageResult);
                    }
                }
                else
                {
                    WcfAdsMessage messageEvents = new WcfAdsMessage();
                    if (_dictByteEvent != null)
                        lock (_byteLock20010)
                            messageEvents.ByteEvents = CopyDictionary<ACRMemoryByteEvent>(ref _dictByteEvent);

                    if (_dictUIntEvent != null)
                        lock (_uIntLock20020)
                            messageEvents.UIntEvents = CopyDictionary<ACRMemoryUIntEvent>(ref _dictUIntEvent);

                    if (_dictIntEvent != null)
                        lock (_intLock20030)
                            messageEvents.IntEvents = CopyDictionary<ACRMemoryIntEvent>(ref _dictIntEvent);

                    if (_dictDIntEvent != null)
                        lock (_dIntLock20040)
                            messageEvents.DIntEvents = CopyDictionary<ACRMemoryDIntEvent>(ref _dictDIntEvent);

                    if (_dictUDIntEvent != null)
                        lock (_uDIntLock20050)
                            messageEvents.UDIntEvents = CopyDictionary<ACRMemoryUDIntEvent>(ref _dictUDIntEvent);

                    if (_dictRealEvent != null)
                        lock (_realLock20060)
                            messageEvents.RealEvents = CopyDictionary<ACRMemoryRealEvent>(ref _dictRealEvent);

                    if (_dictLRealEvent != null)
                        lock (_lRealLock20070)
                            messageEvents.LRealEvents = CopyDictionary<ACRMemoryLRealEvent>(ref _dictLRealEvent);

                    if (_dictStringEvent != null)
                        lock (_stringLock20080)
                            messageEvents.StringEvents = CopyDictionary<ACRMemoryStringEvent>(ref _dictStringEvent);

                    if (_dictTimeEvent != null)
                        lock (_timeLock20090)
                            messageEvents.TimeEvents = CopyDictionary<ACRMemoryTimeEvent>(ref _dictTimeEvent);

                    if (_dictDTEvent != null)
                        lock (_dTLock20100)
                            messageEvents.DTEvents = CopyDictionary<ACRMemoryDTEvent>(ref _dictDTEvent);

                    if (_wcfAdsService.ClosingConnection)
                        continue;
                    
                    _wcfAdsService.TryInvoke(messageEvents);
                }
            }
            _syncSend.ThreadTerminated();
        }

        T[] CopyDictionary<T>(ref Dictionary<int,T> copyFrom)
        {
            Dictionary<int, T> tempDict = copyFrom;
            copyFrom = null;
            return tempDict.Values.ToArray();
        }

        #endregion

        #region Receiving

        private Queue<WcfAdsMessage> _ReceiveQueue = new Queue<WcfAdsMessage>();
        internal Queue<WcfAdsMessage> ReceiveQueue
        {
            get { return _ReceiveQueue; }
            set { _ReceiveQueue = value; }
        }

        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueReceivedMessageFromClient(WcfAdsMessage message)
        {
            if (!_syncReceive.NewItemsEnqueueable)
                return;
            if (_wcfAdsService == null || _wcfAdsService.ClosingConnection)
                return;

            lock (this._syncReceive._20010_QueueSyncLock)
            {
                this.ReceiveQueue.Enqueue(message);
            }

            // Signalisiere Thread, dass neue Message ansteht
            _syncReceive.NewItemEvent.Set();
        }

        void RecieveMessage()
        {
            while (!_syncReceive.ExitThreadEvent.WaitOne(0, false))
            {
                _syncReceive.NewItemEvent.WaitOne();
                while (ReceiveQueue.Count > 0)
                {
                    if (_wcfAdsService == null || _wcfAdsService.ClosingConnection)
                        break;
                    WcfAdsMessage acMessage = null;
                    //dequeue a message from the send queue
                    lock (this._syncReceive._20010_QueueSyncLock)
                    {
                        if (ReceiveQueue.Count <= 0)
                            break;
                        acMessage = ReceiveQueue.Dequeue();
                    }
                    if (_wcfAdsService == null || _wcfAdsService.ClosingConnection)
                        break;
                    if (acMessage != null)
                        ProcessMessage(acMessage);
                }
            }
            _syncReceive.ThreadTerminated();
        }

        private bool _IsConnected = false;
        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }
        }

        void ProcessMessage(WcfAdsMessage message)
        {
            if(message.ConnectionState != null)
            {
                ConnectionState connState = (ConnectionState)Enum.ToObject(typeof(ConnectionState), message.ConnectionState);
                if (connState == ConnectionState.Connect && ACVariobatch.ACVariobatch.Meta != null && ACVariobatch.ACVariobatch.Memory != null)
                {
                    PrepareMetadataMessageForClient(ACVariobatch.ACVariobatch.Meta);
                    PrepareMemoryMessageForClient(WcfAdsServiceManager._Self.AdsAgent.ReadMemory());
                    _IsConnected = true;
                }
                else if(connState == ConnectionState.ReadyForEvents)
                {
                    WcfAdsServiceManager._Self.StartReadEvents();
                }
                else if (connState == ConnectionState.Disconnect)
                {
                    WcfAdsServiceManager._Self.WcfAdsServiceChannelList.Remove(this);
                }
            }
            else if(message.Parameters != null)
            {
                WcfAdsServiceManager._Self.AdsAgent.SendParameters(message.Parameters);
            }
            else if(message.VBEvent != null)
            {
                WcfAdsServiceManager._Self.AdsAgent.SendValueToPLC(message.VBEvent);
            }
            else if(message.Result != null)
            {
                WcfAdsServiceManager._Self.AdsAgent.ReadResult(message.Result);
            }
        }

        #endregion
    }
}