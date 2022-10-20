using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.tcShared.ACVariobatch;
using gip.core.communication;
using System.IO;
using gip.core.tcShared.WCF;
using gip.core.tcShared;

namespace gip.core.tcClient
{
    [ACClassInfo(Const.PackName_TwinCAT, "en{'TwinCAT Session'}de{'TwinCAT Session'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class TCSession : ACSession
    {
        #region c'tors 

        public TCSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier) : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            //channel = new WcfAdsClientChannel(this);
            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            Connect();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            DisConnect();
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Private members

        WcfAdsClientChannel _Channel;
        public readonly ACMonitorObject _30200_ChannelLock = new ACMonitorObject(30200);

        bool _isMapFinished = false, _isInitMemoryFinished = false;
        internal Dictionary<uint, TCItem> TCItemsDict = new Dictionary<uint, TCItem>();

        public Dictionary<string, uint> TCEdgesDict = new Dictionary<string, uint>();
        private static List<string> _ACMethodList = new List<string>();

        Type _typeBool = typeof(bool);
        ushort _typeBoolLength = 1;

        Type _typeShort = typeof(short);
        ushort _typeShortLength = 2;

        Type _typeInt = typeof(int);
        ushort _typeIntLength = 4;

        Type _typeDouble = typeof(double);
        ushort _typeDoubleLength = 8;

        Type _typeTimeSpan = typeof(TimeSpan);
        ushort _typeTimeSpanLength = 4;

        Type _typeDateTime = typeof(DateTime);
        ushort _typeDateTimeLength = 4;

        Type _typeString = typeof(string);

        Type _typeUShort = typeof(ushort);
        ushort _typeUShortLength = 2;

        Type _typeUInt = typeof(uint);
        ushort _typeUIntLength = 4;

        Type _typeFloat = typeof(float);
        ushort _typeFloatLength = 4;

        #endregion

        #region Properties

        ACRMemoryMetaObj[] _Metadata;
        public ACRMemoryMetaObj[] Metadata
        {
            get
            {
                return _Metadata;
            }
            set
            {
                if (_Metadata != value)
                {
                    _Metadata = value;
                    MapPropertiesToMetadata();
                }
            }
        }

        byte[] _Memory;
        public byte[] Memory
        {
            get
            {
                return _Memory;
            }
            set
            {
                if (_Memory != value)
                {
                    _Memory = value;
                    InitMemory();
                }
            }
        }

        public bool IsPropertiesMapped
        {
            get { return _isMapFinished; }
        }

        public bool IsMemoryInitialized
        {
            get { return _isInitMemoryFinished; }
        }

        private string _IPAddress = "localhost";
        [ACPropertyInfo(9999)]
        public string IPAddress
        {
            get
            {
                return _IPAddress;
            }
            set
            {
                _IPAddress = value;

                using (ACMonitor.Lock(_30200_ChannelLock))
                {
                    if (_Channel != null)
                        _Channel.ResetEndpointUri();
                }
            }
        }

        private int _TcpPort;
        [ACPropertyInfo(9999)]
        public int TcpPort
        {
            get
            {
                return _TcpPort;
            }
            set
            {
                _TcpPort = value;

                using (ACMonitor.Lock(_30200_ChannelLock))
                {
                    if (_Channel != null)
                        _Channel.ResetEndpointUri();
                }
            }
        }

        bool _IsPlcConnected = false;
        [ACPropertyInfo(999)]
        public bool IsPlcConnected
        {
            get
            {
                return _IsPlcConnected;
            }
            set
            {
                _IsPlcConnected = value;
                OnPropertyChanged("IsPlcConnected");
            }
        }

        #endregion

        #region Methods

        public static string ResolveACUrlToTwinCATUrl(string acUrl)
        {
            acUrl = acUrl.Replace(ACUrlHelper.Delimiter_DirSeperator.ToString(), GCL.Delimiter_DirSeperator);
            acUrl = acUrl.Substring(2);
            return acUrl;
        }

        /// <summary>
        /// Map properties from Variobatch to TwinCAT metadata.
        /// </summary>
        private void MapPropertiesToMetadata()
        {
            _isMapFinished = false;
            RemoveEvents();
            TCItemsDict.Clear();
            _isMapFinished = false;
            IEnumerable<ApplicationManager> managers = Root.FindChildComponents<ApplicationManager>(c => c is ApplicationManager, null, 1);
            foreach (ApplicationManager manager in managers)
            {
                MapProperties(manager);
            }
            _isMapFinished = true;

            MapEdges();

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (_isInitMemoryFinished && _Channel != null)
                {
                    WcfAdsMessage message = new WcfAdsMessage() { ConnectionState = (byte)gip.core.tcShared.WCF.ConnectionState.ReadyForEvents };
                    _Channel.EnqeueMessageForService(message);
                }
            }
        }

        /// <summary>
        /// Map properties.
        /// </summary>
        /// <param name="manager">The application manager.</param>
        private void MapProperties(ApplicationManager manager)
        {
            foreach(var item in manager.ACCompUrlDict)
            {
                string acUrl = item.Key.Replace(ACUrlHelper.Delimiter_DirSeperator.ToString(), GCL.Delimiter_DirSeperator).Substring(2);
                ACRMemoryMetaObj metaobj = Metadata.FirstOrDefault(c => c._ACUrl == acUrl);
                if (metaobj != null)
                {
                    uint instanceID = (uint)Array.IndexOf(Metadata, metaobj) + 1;
                    if (TCItemsDict.ContainsKey(instanceID))
                        continue;
                    TCItem tcItem = new TCItem(this, instanceID);
                    tcItem.AddProperties(item.Value.ValueT.ACMemberList.OfType<IACPropertyNetServer>());
                    TCItemsDict.Add(instanceID, tcItem);
                }
            }
        }

        /// <summary>
        /// Map edges from metadata to TCEDgesDict.
        /// </summary>
        private void MapEdges()
        {
            TCEdgesDict.Clear();
            foreach(ACRMemoryMetaObj metaObj in Metadata.Where(c => c._TypeOfACObject == TypeOfACObject.Edge))
            {
                string processedACUrl = metaObj._ACUrl.Replace(GCL.cRootACIdentifier , "").Replace("_","").Replace(".","\\");
                TCEdgesDict.Add(processedACUrl, (uint)Array.IndexOf(Metadata, metaObj)+1);
            }
        }

        /// <summary>
        /// Remove all RaiseOnValueUpdated events from TCProperties.
        /// </summary>
        private void RemoveEvents()
        {
            if (TCItemsDict == null && !TCItemsDict.Any())
                return;
            foreach(var item in TCItemsDict)
            {
                item.Value.RemovePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Initialize memory from TwinCAT.
        /// </summary>
        private void InitMemory()
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (Metadata == null || _Channel == null)
                    return;

                _isInitMemoryFinished = false;
                MemoryStream MemoryMS = new MemoryStream(Memory);
                BinaryReader binRead = new BinaryReader(MemoryMS);

                foreach (ACRMemoryMetaObj item in Metadata.Where(c => c._MaxPropertyID > 0))
                {
                    uint instanceID = (uint)Array.IndexOf(Metadata, item) + 1;
                    TCItem outTCItem;

                    if (TCItemsDict.TryGetValue(instanceID, out outTCItem))
                    {
                        foreach (ACRMemoryMetaProp itemProp in item._MetaProp.Where(c => c._Length > 0))
                        {
                            int propID = Array.IndexOf(item._MetaProp, itemProp) + 1;
                            TCProperty tcProperty;

                            if (outTCItem.TCProperties.TryGetValue(propID, out tcProperty))
                            {
                                MemoryMS.Position = item._OffsetInMemory + itemProp._Offset;

                                if (tcProperty.TCNetProperty.PropertyType == _typeBool)
                                {
                                    if (_typeBoolLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeBool)", String.Format("Size mismatch {0} <> {1}", _typeBoolLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadBoolean());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType.IsAssignableFrom(_typeShort))
                                {
                                    if (_typeShortLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeShort)", String.Format("Size mismatch {0} <> {1}", _typeShortLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadInt16());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeInt)
                                {
                                    if (_typeIntLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeInt)", String.Format("Size mismatch {0} <> {1}", _typeIntLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadInt32());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeDouble)
                                {
                                    if (_typeDoubleLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeDouble)", String.Format("Size mismatch {0} <> {1}", _typeDoubleLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadDouble());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeTimeSpan)
                                {
                                    if (_typeTimeSpanLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeTimeSpan)", String.Format("Size mismatch {0} <> {1}", _typeTimeSpanLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(TimeSpan.FromMilliseconds(binRead.ReadInt32()));
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeDateTime)
                                {
                                    if (_typeDateTimeLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeDateTime)", String.Format("Size mismatch {0} <> {1}", _typeDateTimeLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(new DateTime(1970, 1, 1).AddSeconds(binRead.ReadInt32()));
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeString)
                                {
                                    if (tcProperty.TCNetProperty.ACIdentifier == Const.ACState)
                                        tcProperty.SetPropertyValue(binRead.ReadInt16());
                                    else
                                        tcProperty.SetPropertyValue(binRead.ReadString());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeUShort)
                                {
                                    if (_typeUShortLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeUShort)", String.Format("Size mismatch {0} <> {1}", _typeUShortLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadUInt16());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeUInt)
                                {
                                    if (_typeUIntLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeUInt)", String.Format("Size mismatch {0} <> {1}", _typeUIntLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadUInt32());
                                }

                                else if (tcProperty.TCNetProperty.PropertyType == _typeFloat)
                                {
                                    if (_typeFloatLength != itemProp._Length)
                                        Messages.LogError(GetACUrl(), "InitMemory(_typeFloat)", String.Format("Size mismatch {0} <> {1}", _typeFloatLength, itemProp._Length));
                                    tcProperty.SetPropertyValue(binRead.ReadSingle());
                                }
                            }
                        }
                    }
                }
                _isInitMemoryFinished = true;
                binRead.Close();
                binRead = null;
                MemoryMS.Close();
                MemoryMS = null;
                WcfAdsMessage message = new WcfAdsMessage() { ConnectionState = (byte)gip.core.tcShared.WCF.ConnectionState.ReadyForEvents };
                _Channel.EnqeueMessageForService(message);
            }
        }

        /// <summary>
        /// Raise byte events.
        /// </summary>
        /// <param name="events">The byte events array.</param>
        internal void RaiseEventByte(ACRMemoryByteEvent[] events)
        {
            foreach (ACRMemoryByteEvent byteEvent in events.Cast<ACRMemoryByteEvent>())
                TCItemsDict[byteEvent.InstanceID].SetPropertyValue(byteEvent.PropertyID, byteEvent.Value);
        }

        /// <summary>
        /// Raise uInt events.
        /// </summary>
        /// <param name="events">The uInt events array.</param>
        internal void RaiseEventUInt(ACRMemoryUIntEvent[] events)
        {
            foreach (ACRMemoryUIntEvent uintEvent in events)
                TCItemsDict[uintEvent.InstanceID].SetPropertyValue(uintEvent.PropertyID, uintEvent.Value);
        }

        /// <summary>
        /// Raise int events.
        /// </summary>
        /// <param name="events">The int events array.</param>
        internal void RaiseEventInt(ACRMemoryIntEvent[] events)
        {
            foreach (ACRMemoryIntEvent intEvent in events)
                TCItemsDict[intEvent.InstanceID].SetPropertyValue(intEvent.PropertyID, intEvent.Value);
        }

        /// <summary>
        /// Raise dInt events.
        /// </summary>
        /// <param name="events">The dInt events array.</param>
        internal void RaiseEventDInt(ACRMemoryDIntEvent[] events)
        {
            foreach (ACRMemoryDIntEvent dIntEvent in events)
                TCItemsDict[dIntEvent.InstanceID].SetPropertyValue(dIntEvent.PropertyID, dIntEvent.Value);
        }

        /// <summary>
        /// Raise uDInt events.
        /// </summary>
        /// <param name="events">The uDInt events array.</param>
        internal void RaiseEventUDInt(ACRMemoryUDIntEvent[] events)
        {
            foreach (ACRMemoryUDIntEvent uDIntEvent in events)
                TCItemsDict[uDIntEvent.InstanceID].SetPropertyValue(uDIntEvent.PropertyID, uDIntEvent.Value);
        }

        /// <summary>
        /// Raise real events.
        /// </summary>
        /// <param name="events">The real events array.</param>
        internal void RaiseEventReal(ACRMemoryRealEvent[] events)
        {
            foreach (ACRMemoryRealEvent realEvent in events)
                TCItemsDict[realEvent.InstanceID].SetPropertyValue(realEvent.PropertyID, realEvent.Value);
        }

        /// <summary>
        /// Raise lReal events.
        /// </summary>
        /// <param name="events">The lReal events array.</param>
        internal void RaiseEventLReal(ACRMemoryLRealEvent[] events)
        {
            foreach (ACRMemoryLRealEvent lRealEvent in events)
                TCItemsDict[lRealEvent.InstanceID].SetPropertyValue(lRealEvent.PropertyID, lRealEvent.Value);
        }

        /// <summary>
        /// Raise string events.
        /// </summary>
        /// <param name="events">The string events array.</param>
        internal void RaiseEventString(ACRMemoryStringEvent[] events)
        {
            foreach (ACRMemoryStringEvent stringEvent in events)
                TCItemsDict[stringEvent.InstanceID].SetPropertyValue(stringEvent.PropertyID, stringEvent.Value);
        }

        /// <summary>
        /// Raise time events.
        /// </summary>
        /// <param name="events">The time events array.</param>
        internal void RaiseEventTime(ACRMemoryTimeEvent[] events)
        {
            foreach (ACRMemoryTimeEvent timeEvent in events)
                TCItemsDict[timeEvent.InstanceID].SetPropertyValue(timeEvent.PropertyID, TimeSpan.FromMilliseconds(timeEvent.Value));
        }

        /// <summary>
        /// Raise date and time events.
        /// </summary>
        /// <param name="events">The date and time events array.</param>
        internal void RaiseEventDT(ACRMemoryDTEvent[] events)
        {
            foreach (ACRMemoryDTEvent dtEvent in events)
                TCItemsDict[dtEvent.InstanceID].SetPropertyValue(dtEvent.PropertyID, new DateTime(1970, 1, 1).AddSeconds(dtEvent.Value));
        }

        /// <summary>
        /// Send parameters to TwinCAT.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void SendParameters(byte[] parameters)
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (parameters != null && _Channel != null)
                {
                    WcfAdsMessage message = new WcfAdsMessage() { Parameters = parameters };
                    _Channel.EnqeueMessageForService(message);
                }
            }
        }

        /// <summary>
        /// Send request to TwinCAT for reading parameters or result.
        /// </summary>
        /// <param name="acUrl">The function ACUrl.</param>
        /// <param name="resultLength">The length of result.</param>
        /// <param name="readParameters">Is read parameters.</param>
        /// <param name="requestCounter">The request counter for wait handle.</param>
        /// <param name="acMethodName">The acMethod name.</param>
        public void ReadResult(string acUrl, int resultLength, bool readParameters, int requestCounter, string acMethodName)
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (string.IsNullOrEmpty(acUrl) || Metadata == null || _Channel == null)
                    return;

                string tcACUrl = TCSession.ResolveACUrlToTwinCATUrl(acUrl);
                ACRMemoryMetaObj metadataInstance = Metadata.FirstOrDefault(c => c._ACUrl == tcACUrl);
                if (metadataInstance != null)
                {
                    if (!_ACMethodList.Contains(acMethodName))
                        _ACMethodList.Add(acMethodName);

                    byte[] info = new byte[15];
                    uint instanceID = (uint)Array.IndexOf(Metadata, metadataInstance) + 1;
                    Array.Copy(BitConverter.GetBytes(instanceID), 0, info, 0, 4);
                    Array.Copy(BitConverter.GetBytes(resultLength), 0, info, 4, 4);
                    Array.Copy(BitConverter.GetBytes(readParameters), 0, info, 8, 1);
                    Array.Copy(BitConverter.GetBytes(requestCounter), 0, info, 9, 4);
                    Array.Copy(BitConverter.GetBytes((short)_ACMethodList.IndexOf(acMethodName)), 0, info, 13, 2);
                    WcfAdsMessage message = new WcfAdsMessage() { Result = info };
                    _Channel.EnqeueMessageForService(message);
                }
            }
        }

        /// <summary>
        /// This method will be called when TwinCAT send parameters or result to Variobatch.
        /// </summary>
        /// <param name="result">The result.</param>
        internal void OnResultRead(byte[] result)
        {
            string acMethodName = _ACMethodList[BitConverter.ToInt16(result, 0)];

            if (string.IsNullOrEmpty(acMethodName))
                return;

            ACSessionObjSerializer converter = FindChildComponents<ACSessionObjSerializer>(c => c is ACSessionObjSerializer
                                                                                && (c as ACSessionObjSerializer).IsSerializerFor(acMethodName)).FirstOrDefault();
            if (converter != null)
            {
                byte[] tempArr = new byte[result.Length - 2];
                Array.Copy(result, 2, tempArr, 0, tempArr.Length);
                converter.OnObjectRead(tempArr);
            }
        }

        /// <summary>
        /// Send changed value to TwinCAT.
        /// </summary>
        /// <param name="instanceID">The TwinCAT instance ID.</param>
        /// <param name="propACIdentifier">The property ACIdentifier</param>
        /// <param name="RemotePropID">The TwinCAT remote property identifier.</param>
        /// <param name="value">The changed value.</param>
        public void OnReceivedValueUpdated(uint instanceID, string propACIdentifier, int RemotePropID, object value)
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (_Channel != null)
                {
                    VBEvent vbEvent = new VBEvent() { InstanceID = instanceID, PropertyACIdentifier = propACIdentifier, PropertyID = (ushort)RemotePropID, PropertyValue = value };
                    WcfAdsMessage message = new WcfAdsMessage() { VBEvent = vbEvent };
                    _Channel.EnqeueMessageForService(message);
                }
            }
        }

#endregion

#region Other 

        public override bool InitSession()
        {
            return true; 
        }

        public override bool IsEnabledInitSession()
        {
            return true; 
        }

        public override bool DeInitSession()
        {
            return true; 
        }

        public override bool IsEnabledDeInitSession()
        {
            return true; 
        }

        public override bool Connect()
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (!IsEnabledConnect())
                    return true;
                _Channel = new WcfAdsClientChannel(this);
            }
            return _Channel != null; 
        }

        public override bool IsEnabledConnect()
        {
            return _Channel == null; 
        }

        public override bool DisConnect()
        {

            using (ACMonitor.Lock(_30200_ChannelLock))
            {
                if (!IsEnabledDisConnect())
                    return true;

                this.RemoveEvents();
                _Channel.DeinitWcfAdsClientChannel();
                _Channel = null;
                IsConnected.ValueT = false;
                IsPlcConnected = false;
                IsReadyForWriting = false;
            }

            return _Channel == null; 
        }

        public override bool IsEnabledDisConnect()
        {
            return _Channel != null;
        }

        protected override void StartReconnection()
        {

        }

#endregion
    }
}
