using System;
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
        public TcAdsClient adsClient
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

            adsClient.AmsRouterNotification += adsClient_AmsRouterNotification;
            WaitForDeviceReady();
            adsClient.AdsStateChanged += adsClient_AdsStateChanged;

            if (_logger == null)
                _logger = new Logger();
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
                adsClient.ReadDeviceInfo();
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
                adsClient.ReadDeviceInfo();
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
            adsClient.AdsStateChanged -= adsClient_AdsStateChanged;
            adsClient.AmsRouterNotification -= adsClient_AmsRouterNotification;
            //DeleteVariableHandles();
            adsClient.Dispose();
            adsClient.Disconnect();
            _adsClient = null;
            if(withReinit)
                InitManager();
        }

        public void StopAdsAgent()
        {
            DeinitManager(false);
            _agentThread.Join();
        }

        private bool Connect()
        {
            if (adsClient.IsConnected)
                return adsClient.IsConnected;

            string netID = ConfigurationManager.AppSettings["ADSNetID"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["ADSPort"]);

            try
            {
                adsClient.Connect(port);
                adsClient.Synchronize = false;
            }
            catch (Exception e)
            {
                _logger.Log("Method:Connect  Exception:" + e.Message);
            }

            return adsClient.IsConnected;
        }

        private void ReadMemoryMetaObj()
        {
            try
            {
                ACVariobatch.Meta = (ACRMemoryMetaObj[])adsClient.ReadAny(_metadataVariableHandle, typeof(ACRMemoryMetaObj[]), new int[] { GCL.cMetaObjMAX });
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
                ACVariobatch.Memory = (byte[])adsClient.ReadAny(_memoryVariableHandle, typeof(byte[]), new int[] { GCL.cMemorySizeMAX });
            }
            catch (Exception e)
            {
                _logger.Log("Method:ReadMemoryValues  Exception:" + e.Message);
            }
        }

        private void CreateVariableHandles()
        {
            _metadataVariableHandle = adsClient.CreateVariableHandle(GCL.cPathMemoryMetaObj);
            _memoryVariableHandle = adsClient.CreateVariableHandle(GCL.cPathMemory);
            _MemReadCycleHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventCycleInfo._MemReadCycle");
            _EventCycleInfoHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventCycleInfo");
            _ByteEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsByte");
            _UIntEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsUInt");
            _IntEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsInt");
            _UDIntEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsUDInt");
            _DIntEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsDInt");
            _RealEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsReal");
            _LRealEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsLReal");
            _StringEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsString");
            _TimeEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsTime");
            _DTEventHandle = adsClient.CreateVariableHandle(GCL.cPathVB + "._EventsDT");
        }

        private void DeleteVariableHandles()
        {
            adsClient.DeleteVariableHandle(_metadataVariableHandle);
            adsClient.DeleteVariableHandle(_memoryVariableHandle);
            adsClient.DeleteVariableHandle(_MemReadCycleHandle);
            adsClient.DeleteVariableHandle(_EventCycleInfoHandle);
            adsClient.DeleteVariableHandle(_ByteEventHandle);
            adsClient.DeleteVariableHandle(_UIntEventHandle);
            adsClient.DeleteVariableHandle(_IntEventHandle);
            adsClient.DeleteVariableHandle(_UDIntEventHandle);
            adsClient.DeleteVariableHandle(_DIntEventHandle);
            adsClient.DeleteVariableHandle(_RealEventHandle);
            adsClient.DeleteVariableHandle(_LRealEventHandle);
            adsClient.DeleteVariableHandle(_StringEventHandle);
            adsClient.DeleteVariableHandle(_TimeEventHandle);
            adsClient.DeleteVariableHandle(_DTEventHandle);
        }

        private void ReadEvents()
        {
            while (!_syncReadEvents.ExitThreadEvent.WaitOne(100, false))
            {
                try
                {
                    ACREventCycleInfo cycleInfo = (ACREventCycleInfo)adsClient.ReadAny(_EventCycleInfoHandle, typeof(ACREventCycleInfo));
                    if (cycleInfo.MemWriteCycle == 1 && cycleInfo.MemReadCycle == 0)
                    {
                        ReadMemoryMetaObj();
                        manager.BroadcastMetadataToChannels();
                    }
                    if (cycleInfo.MemWriteCycle != cycleInfo.MemReadCycle)
                    {
                        if (currentByteEventIndex != cycleInfo.NextByteEventIndex)
                        {
                            ACRMemoryByteEvents byteEvents = (ACRMemoryByteEvents)adsClient.ReadAny(_ByteEventHandle, _typeByteEvents);
                            manager.BroadcastEventsToChannels(currentByteEventIndex, cycleInfo.NextByteEventIndex, byteEvents.Values);
                            currentByteEventIndex = cycleInfo.NextByteEventIndex;
                        }
                        if (currentUIntEventIndex != cycleInfo.NextUIntEventIndex)
                        {
                            ACRMemoryUIntEvents uIntEvents = (ACRMemoryUIntEvents)adsClient.ReadAny(_UIntEventHandle, _typeUIntEvents);
                            manager.BroadcastEventsToChannels(currentUIntEventIndex, cycleInfo.NextUIntEventIndex, uIntEvents.Values);
                            currentUIntEventIndex = cycleInfo.NextUIntEventIndex;
                        }
                        if (currentIntEventIndex != cycleInfo.NextIntEventIndex)
                        {
                            ACRMemoryIntEvents intEvents = (ACRMemoryIntEvents)adsClient.ReadAny(_IntEventHandle, _typeIntEvents);
                            manager.BroadcastEventsToChannels(currentIntEventIndex, cycleInfo.NextIntEventIndex, intEvents.Values);
                            currentIntEventIndex = cycleInfo.NextIntEventIndex;
                        }
                        if (currentDIntEventIndex != cycleInfo.NextDIntEventIndex)
                        {
                            ACRMemoryDIntEvents dIntEvents = (ACRMemoryDIntEvents)adsClient.ReadAny(_DIntEventHandle, _typeDIntEvents);
                            manager.BroadcastEventsToChannels(currentDIntEventIndex, cycleInfo.NextDIntEventIndex, dIntEvents.Values);
                            currentDIntEventIndex = cycleInfo.NextDIntEventIndex;
                        }
                        if (currentUDIntEventIndex != cycleInfo.NextUDIntEventIndex)
                        {
                            ACRMemoryUDIntEvents uDIntEvents = (ACRMemoryUDIntEvents)adsClient.ReadAny(_UDIntEventHandle, _typeUDIntEvents);
                            manager.BroadcastEventsToChannels(currentUDIntEventIndex, cycleInfo.NextUDIntEventIndex, uDIntEvents.Values);
                            currentUDIntEventIndex = cycleInfo.NextUDIntEventIndex;
                        }
                        if (currentRealEventIndex != cycleInfo.NextRealEventIndex)
                        {
                            ACRMemoryRealEvents realEvents = (ACRMemoryRealEvents)adsClient.ReadAny(_RealEventHandle, _typeRealEvents);
                            manager.BroadcastEventsToChannels(currentRealEventIndex, cycleInfo.NextRealEventIndex, realEvents.Values);
                            currentRealEventIndex = cycleInfo.NextRealEventIndex;
                        }
                        if (currentLRealEventIndex != cycleInfo.NextLRealEventIndex)
                        {
                            ACRMemoryLRealEvents lRealEvents = (ACRMemoryLRealEvents)adsClient.ReadAny(_LRealEventHandle, _typeLRealEvents);
                            manager.BroadcastEventsToChannels(currentLRealEventIndex, cycleInfo.NextLRealEventIndex, lRealEvents.Values);
                            currentLRealEventIndex = cycleInfo.NextLRealEventIndex;
                        }
                        if (currentStringEventIndex != cycleInfo.NextStringEventIndex)
                        {
                            ACRMemoryStringEvents stringEvents = (ACRMemoryStringEvents)adsClient.ReadAny(_StringEventHandle, _typeStringEvents);
                            manager.BroadcastEventsToChannels(currentStringEventIndex, cycleInfo.NextStringEventIndex, stringEvents.Values);
                            currentStringEventIndex = cycleInfo.NextStringEventIndex;
                        }
                        if (currentTimeEventIndex != cycleInfo.NextTimeEventIndex)
                        {
                            ACRMemoryTimeEvents timeEvents = (ACRMemoryTimeEvents)adsClient.ReadAny(_TimeEventHandle, _typeTimeEvents);
                            manager.BroadcastEventsToChannels(currentTimeEventIndex, cycleInfo.NextTimeEventIndex, timeEvents.Values);
                            currentTimeEventIndex = cycleInfo.NextTimeEventIndex;
                        }
                        if (currentDIntEventIndex != cycleInfo.NextDTEventIndex)
                        {
                            ACRMemoryDTEvents dtEvents = (ACRMemoryDTEvents)adsClient.ReadAny(_DTEventHandle, _typeDTEvents);
                            manager.BroadcastEventsToChannels(currentDTEventIndex, cycleInfo.NextDTEventIndex, dtEvents.Values);
                            currentDTEventIndex = cycleInfo.NextDTEventIndex;
                        }
                        adsClient.WriteAny(_MemReadCycleHandle, cycleInfo.MemWriteCycle);
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
            string acurl = "MAIN." + metaInstance._ACUrl + "._ACMethod._Parameter";

            try
            {
                int handle = adsClient.CreateVariableHandle(acurl);
                AdsStream stream = new AdsStream(paramPart);
                adsClient.Write(handle, stream);
                adsClient.DeleteVariableHandle(handle);
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

            string acMethodValue = "_Result";
            if (readParameters)
                acMethodValue = "_Parameter";

            string acUrl = "MAIN." + metaInstance._ACUrl + "._ACMethod." + acMethodValue;

            try
            {
                int handle = adsClient.CreateVariableHandle(acUrl);
                AdsStream stream = new AdsStream(arrayLength);
                adsClient.Read(handle, stream);

                byte[] sendArray = new byte[9 + stream.Length];
                Array.Copy(info, 13, sendArray, 0, 2); //acMethodIndex short
                Array.Copy(info, 9, sendArray, 2, 4); //requestID int32
                Array.Copy(info, 8, sendArray, 6, 1); // readParam bool
                Array.Copy(stream.ToArray(), 0, sendArray, 7, stream.Length);

                manager.BroadcastResultToChannels(sendArray);
                adsClient.DeleteVariableHandle(handle);
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

            string acUrl = "MAIN." + instance._ACUrl;
            string acUrlProp = acUrl + "._" + vbEvent.PropertyACIdentifier;

            try
            {
                ITcAdsSymbol smb = adsClient.ReadSymbolInfo(acUrl);
                ITcAdsSymbol smbP = adsClient.ReadSymbolInfo(acUrlProp);
                if (smbP == null)
                    return;

                ITcAdsSymbol5 smb5 = smbP as ITcAdsSymbol5;
                if (smb5.Category == DataTypeCategory.Enum)
                    adsClient.InvokeRpcMethod(smb, "SetEnumPropertyValue", new object[] { vbEvent.PropertyID, (short)vbEvent.PropertyValue });

                else if (smb5.DataType.Name == "TIME")
                    adsClient.InvokeRpcMethod(smb, "SetTimePropertyValue", new object[] { vbEvent.PropertyID, (int)vbEvent.PropertyValue });

                else if (smb5.DataType.Name == "DATE_AND_TIME")
                    adsClient.InvokeRpcMethod(smb, "SetDTPropertyValue", new object[] { vbEvent.PropertyID, (int)vbEvent.PropertyValue });

                else
                    adsClient.InvokeRpcMethod(smb, "__set" + vbEvent.PropertyACIdentifier, new object[] { vbEvent.PropertyValue });
            }
            catch (Exception e)
            {
                _logger.Log("Method:SendValueToPLC  Exception:" + e.Message + " @ACUrl:" + acUrl + "(" + vbEvent.PropertyACIdentifier + ")");
            }
        }

        #endregion
    }
}
