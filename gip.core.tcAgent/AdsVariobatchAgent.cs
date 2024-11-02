// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared.ACVariobatch;
using gip.core.tcShared.WCF;
using gip.core.tcShared;
using TwinCAT.Ads;
using System.Configuration;
using System.Threading;
using TwinCAT.TypeSystem;

namespace gip.core.tcAgent
{
    public class AdsVariobatchAgent : IAdsAgent
    {
        #region Private members

        WcfAdsServiceManager manager;
        SyncQueueEvents _syncReadEvents;
        Thread _readEventsThread;
        //SyncQueueEvents _syncAgentThread;
        Thread _agentThread;
        Logger _logger;

        #region Private members -> CurrentEventIndex

        int currentByteEventIndex = 0;
        int currentUIntEventIndex = 0;
        int currentIntEventIndex = 0;
        int currentDIntEventIndex = 0;
        int currentUDIntEventIndex = 0;
        int currentRealEventIndex = 0;
        int currentLRealEventIndex = 0;
        int currentStringEventIndex = 0;
        int currentTimeEventIndex = 0;
        int currentDTEventIndex = 0;

        #endregion

        #region Private members -> VariableHandle

        bool _HandlesRead = false;
        int _metadataVariableHandle;
        int _memoryVariableHandle;
        int _MemReadCycleHandle;
        int _ByteEventHandle;
        int _UIntEventHandle;
        int _IntEventHandle;
        int _DIntEventHandle;
        int _UDIntEventHandle;
        int _RealEventHandle;
        int _LRealEventHandle;
        int _StringEventHandle;
        int _TimeEventHandle;
        int _DTEventHandle;
        int _EventCycleInfoHandle;

        #endregion

        Type _typeByteEvents = typeof(ACRMemoryByteEvents);
        Type _typeUIntEvents = typeof(ACRMemoryUIntEvents);
        Type _typeIntEvents = typeof(ACRMemoryIntEvents);
        Type _typeDIntEvents = typeof(ACRMemoryDIntEvents);
        Type _typeUDIntEvents = typeof(ACRMemoryUDIntEvents);
        Type _typeRealEvents = typeof(ACRMemoryRealEvents);
        Type _typeLRealEvents = typeof(ACRMemoryLRealEvents);
        Type _typeStringEvents = typeof(ACRMemoryStringEvents);
        Type _typeTimeEvents = typeof(ACRMemoryTimeEvents);
        Type _typeDTEvents = typeof(ACRMemoryDTEvents);

        #endregion

        public AdsVariobatchAgent()
        {
            if (_logger == null)
                _logger = new Logger();

            manager = new WcfAdsServiceManager(this);

            _agentThread = new Thread(InitManager);
            _agentThread.Name = "ADSAgentThread";
            _agentThread.Start();

            _syncReadEvents = new SyncQueueEvents();
            _readEventsThread = new Thread(ReadEvents);
            _readEventsThread.Name = "ADSAgent:ReadEventsThread";
        }

        #region Properties

        private TcAdsClient _adsClient;
        public TcAdsClient AdsClient
        {
            get
            {
                if (_adsClient == null)
                    _adsClient = new TcAdsClient();
                return _adsClient;
            }
        }

        #endregion

        #region Methods

        private void InitManager()
        {
            if (!Connect())
                return;

            AdsClient.AmsRouterNotification += adsClient_AmsRouterNotification;
            WaitForDeviceReady();
            AdsClient.AdsStateChanged += adsClient_AdsStateChanged;
        }

        void adsClient_AmsRouterNotification(object sender, AmsRouterNotificationEventArgs e)
        {
            if (e.State == AmsRouterState.Stop && !IsDeviceReady())
            {
                DeinitManager();
            }
        }

        private bool IsDeviceReady()
        {
            try
            {
                AdsClient.ReadDeviceInfo();
                return true;
            }
            catch (Exception e)
            {
                _logger.Log("Method:IsDeviceReady  Exception:" + e.Message);
                return false;
            }
        }

        private void WaitForDeviceReady()
        {
            try
            {
                AdsClient.ReadDeviceInfo();
                return;
            }
            catch (Exception e)
            {
                Thread.Sleep(500);
                WaitForDeviceReady();

                _logger.Log("Method:IsDeviceReady  Exception:" + e.Message);
            }
        }

        private void DeinitManager(bool withReinit = true)
        {
            WcfAdsServiceManager._Self.BroadcastConnectionStateToChannels(ConnectionState.DisconnectPLC);
            _syncReadEvents.NewItemEvent.Reset();
            if (_adsClient != null)
            {
                try
                {
                    _adsClient.AdsStateChanged -= adsClient_AdsStateChanged;
                    _adsClient.AmsRouterNotification -= adsClient_AmsRouterNotification;
                    //DeleteVariableHandles();
                    _adsClient.Disconnect();
                    _adsClient.Dispose();
                }
                catch (Exception)
                {
                }
            }
            _adsClient = null;
            if (withReinit)
                InitManager();
        }

        public void StopAdsAgent()
        {
            DeinitManager(false);
            _agentThread.Join();
        }

        private bool Connect()
        {
            if (AdsClient.IsConnected)
                return AdsClient.IsConnected;

            string netID = ConfigurationManager.AppSettings["ADSNetID"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["ADSPort"]);

            try
            {
                AdsClient.Connect(port);
                AdsClient.Synchronize = false;
            }
            catch (Exception e)
            {
                _logger.Log("Method:Connect  Exception:" + e.Message);
            }

            return AdsClient.IsConnected;
        }

        private void ReadMemoryMetaObj()
        {
            try
            {
                ACVariobatch.Meta = (ACRMemoryMetaObj[])AdsClient.ReadAny(_metadataVariableHandle, typeof(ACRMemoryMetaObj[]), new int[] { GCL.cMetaObjMAX });
            }
            catch (Exception e)
            {
                _logger.Log("Method:ReadMemoryMetaObj  Exception:" + e.Message);
            }
        }

        private void ReadMemoryValues()
        {
            try
            {
                ACVariobatch.Memory = (byte[])AdsClient.ReadAny(_memoryVariableHandle, typeof(byte[]), new int[] { GCL.cMemorySizeMAX });
            }
            catch (Exception e)
            {
                _logger.Log("Method:ReadMemoryValues  Exception:" + e.Message);
            }
        }

        private void CreateVariableHandles()
        {
            _metadataVariableHandle = AdsClient.CreateVariableHandle(GCL.cPathMemoryMetaObj);
            _memoryVariableHandle = AdsClient.CreateVariableHandle(GCL.cPathMemory);
            _MemReadCycleHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventCycleInfo + GCL.cMemReadCycle);
            _EventCycleInfoHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventCycleInfo);
            _ByteEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsByte);
            _UIntEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsUInt);
            _IntEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsInt);
            _UDIntEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsUDInt);
            _DIntEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsDInt);
            _RealEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsReal);
            _LRealEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsLReal);
            _StringEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsString);
            _TimeEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsTime);
            _DTEventHandle = AdsClient.CreateVariableHandle(GCL.cPathVB + GCL.cEventsDT);
            _HandlesRead = true;
        }

        private void DeleteVariableHandles()
        {
            if (_HandlesRead)
            {
                AdsClient.DeleteVariableHandle(_metadataVariableHandle);
                AdsClient.DeleteVariableHandle(_memoryVariableHandle);
                AdsClient.DeleteVariableHandle(_MemReadCycleHandle);
                AdsClient.DeleteVariableHandle(_EventCycleInfoHandle);
                AdsClient.DeleteVariableHandle(_ByteEventHandle);
                AdsClient.DeleteVariableHandle(_UIntEventHandle);
                AdsClient.DeleteVariableHandle(_IntEventHandle);
                AdsClient.DeleteVariableHandle(_UDIntEventHandle);
                AdsClient.DeleteVariableHandle(_DIntEventHandle);
                AdsClient.DeleteVariableHandle(_RealEventHandle);
                AdsClient.DeleteVariableHandle(_LRealEventHandle);
                AdsClient.DeleteVariableHandle(_StringEventHandle);
                AdsClient.DeleteVariableHandle(_TimeEventHandle);
                AdsClient.DeleteVariableHandle(_DTEventHandle);
                _HandlesRead = false;
            }
        }

        private void ReadEvents()
        {
            while (!_syncReadEvents.ExitThreadEvent.WaitOne(100, false))
            {
                try
                {
                    ACREventCycleInfo cycleInfo = (ACREventCycleInfo)AdsClient.ReadAny(_EventCycleInfoHandle, typeof(ACREventCycleInfo));
                    if (cycleInfo.MemWriteCycle == 1 && cycleInfo.MemReadCycle == 0)
                    {
                        ReadMemoryMetaObj();
                        manager.BroadcastMetadataToChannels();
                    }
                    if (cycleInfo.MemWriteCycle != cycleInfo.MemReadCycle)
                    {
                        if (currentByteEventIndex != cycleInfo.NextByteEventIndex)
                        {
                            ACRMemoryByteEvents byteEvents = (ACRMemoryByteEvents)AdsClient.ReadAny(_ByteEventHandle, _typeByteEvents);
                            manager.BroadcastEventsToChannels(currentByteEventIndex, cycleInfo.NextByteEventIndex, byteEvents.Values);
                            currentByteEventIndex = cycleInfo.NextByteEventIndex;
                        }
                        if (currentUIntEventIndex != cycleInfo.NextUIntEventIndex)
                        {
                            ACRMemoryUIntEvents uIntEvents = (ACRMemoryUIntEvents)AdsClient.ReadAny(_UIntEventHandle, _typeUIntEvents);
                            manager.BroadcastEventsToChannels(currentUIntEventIndex, cycleInfo.NextUIntEventIndex, uIntEvents.Values);
                            currentUIntEventIndex = cycleInfo.NextUIntEventIndex;
                        }
                        if (currentIntEventIndex != cycleInfo.NextIntEventIndex)
                        {
                            ACRMemoryIntEvents intEvents = (ACRMemoryIntEvents)AdsClient.ReadAny(_IntEventHandle, _typeIntEvents);
                            manager.BroadcastEventsToChannels(currentIntEventIndex, cycleInfo.NextIntEventIndex, intEvents.Values);
                            currentIntEventIndex = cycleInfo.NextIntEventIndex;
                        }
                        if (currentDIntEventIndex != cycleInfo.NextDIntEventIndex)
                        {
                            ACRMemoryDIntEvents dIntEvents = (ACRMemoryDIntEvents)AdsClient.ReadAny(_DIntEventHandle, _typeDIntEvents);
                            manager.BroadcastEventsToChannels(currentDIntEventIndex, cycleInfo.NextDIntEventIndex, dIntEvents.Values);
                            currentDIntEventIndex = cycleInfo.NextDIntEventIndex;
                        }
                        if (currentUDIntEventIndex != cycleInfo.NextUDIntEventIndex)
                        {
                            ACRMemoryUDIntEvents uDIntEvents = (ACRMemoryUDIntEvents)AdsClient.ReadAny(_UDIntEventHandle, _typeUDIntEvents);
                            manager.BroadcastEventsToChannels(currentUDIntEventIndex, cycleInfo.NextUDIntEventIndex, uDIntEvents.Values);
                            currentUDIntEventIndex = cycleInfo.NextUDIntEventIndex;
                        }
                        if (currentRealEventIndex != cycleInfo.NextRealEventIndex)
                        {
                            ACRMemoryRealEvents realEvents = (ACRMemoryRealEvents)AdsClient.ReadAny(_RealEventHandle, _typeRealEvents);
                            manager.BroadcastEventsToChannels(currentRealEventIndex, cycleInfo.NextRealEventIndex, realEvents.Values);
                            currentRealEventIndex = cycleInfo.NextRealEventIndex;
                        }
                        if (currentLRealEventIndex != cycleInfo.NextLRealEventIndex)
                        {
                            ACRMemoryLRealEvents lRealEvents = (ACRMemoryLRealEvents)AdsClient.ReadAny(_LRealEventHandle, _typeLRealEvents);
                            manager.BroadcastEventsToChannels(currentLRealEventIndex, cycleInfo.NextLRealEventIndex, lRealEvents.Values);
                            currentLRealEventIndex = cycleInfo.NextLRealEventIndex;
                        }
                        if (currentStringEventIndex != cycleInfo.NextStringEventIndex)
                        {
                            ACRMemoryStringEvents stringEvents = (ACRMemoryStringEvents)AdsClient.ReadAny(_StringEventHandle, _typeStringEvents);
                            manager.BroadcastEventsToChannels(currentStringEventIndex, cycleInfo.NextStringEventIndex, stringEvents.Values);
                            currentStringEventIndex = cycleInfo.NextStringEventIndex;
                        }
                        if (currentTimeEventIndex != cycleInfo.NextTimeEventIndex)
                        {
                            ACRMemoryTimeEvents timeEvents = (ACRMemoryTimeEvents)AdsClient.ReadAny(_TimeEventHandle, _typeTimeEvents);
                            manager.BroadcastEventsToChannels(currentTimeEventIndex, cycleInfo.NextTimeEventIndex, timeEvents.Values);
                            currentTimeEventIndex = cycleInfo.NextTimeEventIndex;
                        }
                        if (currentDIntEventIndex != cycleInfo.NextDTEventIndex)
                        {
                            ACRMemoryDTEvents dtEvents = (ACRMemoryDTEvents)AdsClient.ReadAny(_DTEventHandle, _typeDTEvents);
                            manager.BroadcastEventsToChannels(currentDTEventIndex, cycleInfo.NextDTEventIndex, dtEvents.Values);
                            currentDTEventIndex = cycleInfo.NextDTEventIndex;
                        }
                        AdsClient.WriteAny(_MemReadCycleHandle, cycleInfo.MemWriteCycle);
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(2000);
                    _logger.Log("Method:ReadEvents  Exception:" + e.Message);
                }
            }
        }

        void adsClient_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            if (e.State.AdsState == AdsState.Run)
            {
                CreateVariableHandles();
                ReadMemoryMetaObj();
                ReadMemoryValues();
                manager.BroadcastMetadataToChannels();
            }
            else if (e.State.AdsState == AdsState.Stop)
            {
                DeleteVariableHandles();
            }
        }

        #endregion

        #region IAdsAgent

        private bool IsReadEventStarted = false;
        public void StartReadEvent()
        {
            if (!IsReadEventStarted && _readEventsThread != null)
            {
                _readEventsThread.Start();
                IsReadEventStarted = true;
            }
        }

        public byte[] ReadMemory()
        {
            ReadMemoryValues();
            return ACVariobatch.Memory;
        }

        public void SendParameters(byte[] parameters)
        {
            int instanceIndex = BitConverter.ToInt32(parameters, 0) - 1;
            ACRMemoryMetaObj metaInstance = ACVariobatch.Meta[instanceIndex];

            byte[] paramPart = new byte[parameters.Length - 4];
            Array.Copy(parameters, 4, paramPart, 0, parameters.Length - 4);
            string acurl = GCL.cMainIdentifier + GCL.cADSPathSeparator  + metaInstance._ACUrl + GCL.cACMethod + GCL.cMethodParameter;

            try
            {
                int handle = AdsClient.CreateVariableHandle(acurl);
                AdsStream stream = new AdsStream(paramPart);
                AdsClient.Write(handle, stream);
                AdsClient.DeleteVariableHandle(handle);
            }
            catch (Exception e)
            {
                _logger.Log("Method:SendParameters  Exception:" + e.Message);
            }
        }

        public void ReadResult(byte[] info)
        {
            int instanceIndex = BitConverter.ToInt32(info, 0) - 1;
            int arrayLength = BitConverter.ToInt16(info, 4);
            bool readParameters = BitConverter.ToBoolean(info, 8);
            int requestCounter = BitConverter.ToInt32(info, 9);
            short acMethodIndex = BitConverter.ToInt16(info, 13);
            ACRMemoryMetaObj metaInstance = ACVariobatch.Meta[instanceIndex];

            string acMethodValue = GCL.cMethodResult;
            if (readParameters)
                acMethodValue = GCL.cMethodParameter;

            string acUrl = GCL.cMainIdentifier + GCL.cADSPathSeparator + metaInstance._ACUrl + GCL.cACMethod + acMethodValue;

            try
            {
                int handle = AdsClient.CreateVariableHandle(acUrl);
                AdsStream stream = new AdsStream(arrayLength);
                AdsClient.Read(handle, stream);

                byte[] sendArray = new byte[9 + stream.Length];
                Array.Copy(info, 13, sendArray, 0, 2); //acMethodIndex short
                Array.Copy(info, 9, sendArray, 2, 4); //requestID int32
                Array.Copy(info, 8, sendArray, 6, 1); // readParam bool
                Array.Copy(stream.ToArray(), 0, sendArray, 7, stream.Length);

                manager.BroadcastResultToChannels(sendArray);
                AdsClient.DeleteVariableHandle(handle);
            }
            catch (Exception e)
            {
                _logger.Log("Method:ReadResult  Exception:" + e.Message);
            }
        }

        public void SendValueToPLC(VBEvent vbEvent)
        {
            if (vbEvent == null || ACVariobatch.Meta == null)
                return;

            ACRMemoryMetaObj instance = ACVariobatch.Meta[vbEvent.InstanceID - 1];

            if (instance == null)
                return;

            string acUrl = GCL.cMainIdentifier + GCL.cADSPathSeparator + instance._ACUrl;
            string acUrlProp = acUrl + GCL.cADSChildSeparator + vbEvent.PropertyACIdentifier;

            try
            {
                ITcAdsSymbol smb = AdsClient.ReadSymbolInfo(acUrl);
                ITcAdsSymbol smbP = AdsClient.ReadSymbolInfo(acUrlProp);
                if (smbP == null)
                    return;

                ITcAdsSymbol5 smb5 = smbP as ITcAdsSymbol5;
                if (smb5.Category == DataTypeCategory.Enum)
                    AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterEnum, new object[] { vbEvent.PropertyID, (short)vbEvent.PropertyValue });
                else if (smb5.DataType.Name == GCL.cADSType_TimeSpan)
                    AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterTime, new object[] { vbEvent.PropertyID, (int)vbEvent.PropertyValue });
                else if (smb5.DataType.Name == GCL.cADSType_DateTime)
                    AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterDT, new object[] { vbEvent.PropertyID, (int)vbEvent.PropertyValue });
                else
                {
                    switch (smb5.DataTypeId)
                    {
                        case AdsDatatypeId.ADST_BIT:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterBool, new object[] { vbEvent.PropertyID, (bool)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_INT8:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterByte, new object[] { vbEvent.PropertyID, (byte)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_INT16:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterInt, new object[] { vbEvent.PropertyID, (short)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_INT32:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterDInt, new object[] { vbEvent.PropertyID, (int)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_UINT16:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterUInt, new object[] { vbEvent.PropertyID, (ushort)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_UINT32:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterUDInt, new object[] { vbEvent.PropertyID, (uint)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_REAL32:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterReal, new object[] { vbEvent.PropertyID, (float)vbEvent.PropertyValue });
                            break;
                        case AdsDatatypeId.ADST_REAL64:
                            AdsClient.InvokeRpcMethod(smb, GCL.cRPC_InvokeSetterLReal, new object[] { vbEvent.PropertyID, (double)vbEvent.PropertyValue });
                            break;
                    }
                    //AdsClient.InvokeRpcMethod(smb, "__set" + vbEvent.PropertyACIdentifier, new object[] { vbEvent.PropertyValue });
                }
            }
            catch (Exception e)
            {
                _logger.Log("Method:SendValueToPLC  Exception:" + e.Message + " @ACUrl:" + acUrl + "(" + vbEvent.PropertyACIdentifier + ")");
            }
        }

        #endregion
    }
}
