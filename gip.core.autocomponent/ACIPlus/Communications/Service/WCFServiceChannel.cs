// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Timers;
using System.Net;
using System.Threading;
using System.Runtime.Serialization;
using System.Xml;
using gip.core.datamodel;
using System.Transactions;


namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WCFServiceChannel'}de{'WCFServiceChannel'}", Global.ACKinds.TACWCFServiceChannel, Global.ACStorableTypes.NotStorable, true, false)]
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {"ServiceOfPeer", Global.ParamOption.Optional, typeof(WCFService)}
        }
    )]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "WCFServiceChannel", "en{'WCFServiceChannel'}de{'WCFServiceChannel'}", typeof(WCFServiceChannel), "WCFServiceChannel", "ConnectedSince", "ConnectedSince")]
    public class WCFServiceChannel : ACComponent
    {
        #region private members

        WCFService _serviceOfPeer = null;
        internal WCFService ServiceOfPeer
        {
            get
            {
                return _serviceOfPeer;
            }
        }
        private ACPSubscrService _SubscriptionOfPeer = null;
        internal ACPSubscrService SubscriptionOfPeer
        {
            get
            {
                return _SubscriptionOfPeer;
            }
        }

        // Producer-Consumer for Sending
        SyncQueueEvents _syncSend;
        ACThread _sendThread;

        // Producer-Consumer for Receiving
        SyncQueueEvents _syncReceive;
        ACThread _receiveThread;

        #endregion

        #region c´tors

        public WCFServiceChannel(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _SubscriptionOfPeer = new ACPSubscrService(this);

            _serviceOfPeer = ParameterValue("ServiceOfPeer") as WCFService;

            _syncSend = new SyncQueueEvents();
            _sendThread = new ACThread(SendMessageToPeer);
            _sendThread.Start();

            _syncReceive = new SyncQueueEvents();
            _receiveThread = new ACThread(ProcessMessageFromPeer);
            _receiveThread.Start();
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _receiveThread.Name = "ACUrl:" + this.GetACUrl() + ";SendMessageToPeer();";
            _sendThread.Name = "ACUrl:" + this.GetACUrl() + ";ProcessMessageFromPeer();";

            if (WCFServiceManager != null)
                WCFServiceManager.TotalCountConnects++;

            Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACInit(1)", ConnectionDetailXML);
            Environment environment = Root.Environment as Environment;
            if (environment != null)
            {
                environment.RefreshWCFConnectionStat(WCFServiceManager.CountConnected);
                Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACInit(2)", String.Format("Current Connection Count: {0}", WCFServiceManager.CountConnected));
                if (environment.IsMaxWCFConnectionsExceeded(WCFServiceManager.CountConnected))
                    Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACInit(3)", String.Format("MaxLicensedWCFConnections of {0} is exceeded!", environment.MaxLicensedWCFConnections));
            }

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACDeInit(1)", ConnectionDetailXML);

            _SubscriptionOfPeer.UnSubscribeAll();

            if (WCFServiceManager != null)
            {
                //WCFServiceManager.ACPDispatchToProxies.EmptyAllValueEvents = true;
                WCFServiceManager.TotalCountDisconnects++;
                Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACDeInit(2)", String.Format("Current Connection Count: {0}", WCFServiceManager.CountConnected));
            }

            if (_receiveThread != null)
            {
                _syncReceive.TerminateThread();
                _receiveThread.Join();
                _receiveThread = null;
            }

            if (_sendThread != null)
            {
                _syncSend.TerminateThread();
                _sendThread.Join();
                _sendThread = null;
            }

            try
            {
                if (_serviceOfPeer != null)
                    _serviceOfPeer.DisconnectClient();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFServiceChannel", "ACDeInit", msg);
            }

            _serviceOfPeer = null;


            bool acDeinitSucc = true;

            Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ACDeInit(2)", ConnectionDetailXML);

            WCFServiceManager serviceManager = WCFServiceManager;

            using (ACMonitor.Lock(serviceManager._20056_ACPLock))
            {
                acDeinitSucc = base.ACDeInit(deleteACClassTask);
            }
            return acDeinitSucc;
        }
#endregion

#region public members
        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        public WCFServiceManager WCFServiceManager
        {
            get
            {
                if (this.ParentACComponent is WCFServiceManager)
                    return this.ParentACComponent as WCFServiceManager;
                return null;
            }
        }

        public Communications Communications
        {
            get
            {
                if (WCFServiceManager == null)
                    return null;
                if (WCFServiceManager.ParentACComponent is Communications)
                    return WCFServiceManager.ParentACComponent as Communications;
                return null;
            }
        }

        private DateTime _ConnectedSince = DateTime.Now;
        [ACPropertyInfo(1)]
        public DateTime ConnectedSince
        {
            get
            {
                return _ConnectedSince;
            }

            set
            {
                _ConnectedSince = value;
                OnPropertyChanged("ConnectedSince");
            }
        }

        [ACPropertyInfo(2)]
        public string IPAddressOfPeer
        {
            get
            {
                if (RemoteAddress == null)
                    return "";
                return RemoteAddress.Authority;
            }
        }

        [ACPropertyInfo(9999)]
        public Uri RemoteAddress
        {
            get
            {
                if (_serviceOfPeer == null)
                    return null;
                return _serviceOfPeer.RemoteAddress;
            }
        }

        private string _UserName;
        [ACPropertyInfo(3)]
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
                OnPropertyChanged("UserName");
            }
        }

        private VBUser _ConnectedUser;
        public VBUser ConnectedUser
        {
            get
            {
                if (_ConnectedUser != null)
                    return _ConnectedUser;
                if (String.IsNullOrEmpty(UserName))
                    return null;
                using (ACMonitor.Lock(Database.ContextIPlus.QueryLock_1X000))
                {
                    _ConnectedUser = Database.ContextIPlus.VBUser.Where(c => c.VBUserName == UserName).FirstOrDefault();
                }
                return _ConnectedUser;
            }
        }

        public string ConnectionDetailXML
        {
            get
            {
                string xaml = String.Format("<WCFServiceChannel>" +
                    "<IPAddressOfPeer>{0}</IPAddressOfPeer>" +
                    "<UserName>{1}</UserName>" +
                    "<ConnectedSince>{2}</ConnectedSince>" +
                "</WCFServiceChannel>",
                IPAddressOfPeer,
                UserName,
                ConnectedSince.ToString()
                );

                return xaml;
            }
        }

        [ACPropertyInfo(9999)]
        public bool TestSerializerOn
        {
            get;
            set;
        }

        public bool ClosingConnection
        {
            get
            {
                if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                    return true;
                return false;
            }
        }

        internal ACPropertyValueMessage _PropValuesToSend = null;

        string _ACUrlForLogger = null;
        public string ACUrlForLogger
        {
            get
            {
                if (_ACUrlForLogger != null)
                    return _ACUrlForLogger;
                _ACUrlForLogger = this.GetACUrl();
                return _ACUrlForLogger;
            }
        }
#endregion

#region methods

        /// <summary>
        /// Sends Property-Values to Client
        /// </summary>
        public void BroadcastPropertyValues()
        {
            if ((WCFServiceManager == null) || (_serviceOfPeer == null) || _serviceOfPeer.ClosingConnection)
                return;
            if ((WCFServiceManager.ACPDispatchToProxies == null) || (_SubscriptionOfPeer == null))
                return;
            ACPropertyValueMessage message = new ACPropertyValueMessage();
            message.PropertyValues = WCFServiceManager.ACPDispatchToProxies.GetValueEventsForSubscription(_SubscriptionOfPeer);
            if (message.PropertyValues.Count <= 0)
                return;

            WCFServiceManager.UpdateStatisticOnBroadcast(message.PropertyValues.Count);

            var listOfPreparedToSend = message.PropertyValues.Where(c => c.EventBroadcasted == ACPropertyBroadcastState.PreparedToSend).ToArray();
            //var listOfPreparedToSend = message.PropertyValues.Where(c => c.SubscriptionSendCount > 0).ToArray();
            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);

            WCFServiceManager.ACPDispatchToProxies.MarkEventsAsSended(listOfPreparedToSend);
        }

        public void BroadcastPreparedPropertyValues()
        {
            if ((WCFServiceManager == null) || (_serviceOfPeer == null) || _serviceOfPeer.ClosingConnection || this._PropValuesToSend == null)
                return;

            WCFServiceManager.UpdateStatisticOnBroadcast(this._PropValuesToSend.PropertyValues.Count);

            WCFMessage acMessage = NewACMessage("", new Object[] { this._PropValuesToSend });
            EnqeueMessageForPeer(acMessage);
            this._PropValuesToSend = null;
        }

        public void DispatchPoints()
        {
            if ((WCFServiceManager == null) || (_serviceOfPeer == null) || _serviceOfPeer.ClosingConnection)
                return;
            if ((WCFServiceManager.ACPDispatchToProxies == null) || (_SubscriptionOfPeer == null))
                return;

            ACSubscriptionServiceMessage message = new ACSubscriptionServiceMessage();
            message.ACPSubscribe = WCFServiceManager.ACPDispatchToProxies.GetChangedPointsForSubscription(_SubscriptionOfPeer);
            if (message.ACPSubscribe.ProjectSubscriptionList.Count <= 0)
                return;

            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);
        }

        public void ShutdownClient()
        {
            ACDisconnect message = new ACDisconnect();
            message.UserName = this.Root.Environment.User.VBUserName;
            message.ShutdownClient = true;

            WCFMessage acMessage = NewACMessage("", new Object[] { message });
            EnqeueMessageForPeer(acMessage);
        }


        /// <summary>
        /// Sendet eine serverseitige Nachricht den Client
        /// </summary>
        /// <param name="acMessage"></param>
        public void BroadcastACMessageToClient(WCFMessage acMessage)
        {
            if (_serviceOfPeer == null)
                return;
            EnqeueMessageForPeer(acMessage);
        }

        public WCFMessage NewACMessage(string acUrl, Object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
            {
                return WCFMessage.NewACMessage(GetACUrl() + "\\" + acUrl, acParameter);
            }
            else
            {
                return WCFMessage.NewACMessage(GetACUrl(), acParameter);
            }
        }
#endregion

#region Producer-Consumer for Sending

        private Queue<WCFMessage> _SendQueue = new Queue<WCFMessage>();
        internal Queue<WCFMessage> SendQueue
        {
            get { return _SendQueue; }
            set { _SendQueue = value; }
        }

        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueMessageForPeer(WCFMessage message)
        {
            if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                return;

            if (!_syncSend.NewItemsEnqueueable)
                return;


            using (ACMonitor.Lock(this._syncSend._20010_QueueSyncLock))
                this.SendQueue.Enqueue(message);

            // Signalisiere Thread, dass neue Message ansteht
            _syncSend.NewItemEvent.Set();
        }

        /// <summary>
        /// Consumer-Method
        /// </summary>
        private void SendMessageToPeer()
        {
            if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                return;
            bool invokeSucc = true;
            while (!_syncSend.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncSend.NewItemEvent.WaitOne();

                _sendThread.StartReportingExeTime();
                invokeSucc = true;
                while (SendQueue.Count > 0)
                {
                    if (_serviceOfPeer.ClosingConnection)
                        break;
                    WCFMessage acMessage = null;
                    //dequeue a message from the send queue

                    using (ACMonitor.Lock(this._syncSend._20010_QueueSyncLock))
                    {
                        if (SendQueue.Count <= 0)
                            break;
                        acMessage = SendQueue.Dequeue();
                    }
                    if (_serviceOfPeer.ClosingConnection)
                        break;
                    if (acMessage != null)
                    {
                        if (TestSerializerOn)
                        {
                            string testResult = SerializeTest(acMessage);
                        }
                        if (_serviceOfPeer.ClosingConnection)
                            break;

                        var vbDump = Root.VBDump;
                        PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACUrlForLogger, 121) : null;
                        invokeSucc = _serviceOfPeer.InvokeRemote(acMessage);
                        if (perfEvent != null)
                            vbDump.PerfLoggerStop(ACUrlForLogger, 121, perfEvent);

                        if (invokeSucc && (acMessage.ACParameter != null) && (acMessage.ACParameter[0] != null))
                        {
                            if (acMessage.ACParameter[0] is ACPropertyValueMessage)
                            {
                                //#if DEBUG
                                //                                Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.SendMessageToPeer()", "ACPropertyValueMessage sended");
                                //#endif
                            }
                            else if (acMessage.ACParameter[0] is ACMethodInvocationResult)
                            {
                                //Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.SendMessageToPeer()", String.Format("ACMethodInvocationResult: {0} {1}", acMessage.ACUrl, acMessage.MethodInvokeRequestID));
                            }
                            else if (acMessage.ACParameter[0] is ACDisconnect)
                            {
                                Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.SendMessageToPeer()", "ACDisconnect sended");
                            }
                            else if (acMessage.ACParameter[0] is ACSubscriptionServiceMessage)
                            {
                                (acMessage.ACParameter[0] as ACSubscriptionServiceMessage).Detach();
                            }
                        }
                        if (!invokeSucc)
                            break;
                    }
                }
                if (!invokeSucc)
                {
                    break;
                }
                _sendThread.StopReportingExeTime();
            }
            _syncSend.ThreadTerminated();
            if (!invokeSucc && _serviceOfPeer != null && !_serviceOfPeer.ClosingConnection)
            {
                _serviceOfPeer.ClosingConnection = true;
                new Thread(() =>
                {
                    WCFServiceManager.StopComponent(this);
                }
                ).Start();
            }
        }
#endregion

#region Producer-Consumer for Receiving

        private Queue<WCFMessage> _ReceiveQueue = new Queue<WCFMessage>();
        internal Queue<WCFMessage> ReceiveQueue
        {
            get { return _ReceiveQueue; }
            set { _ReceiveQueue = value; }
        }


        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="message"></param>
        public void EnqeueReceivedMessageFromPeer(WCFMessage message)
        {
            if (!_syncReceive.NewItemsEnqueueable)
                return;
            if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                return;


            using (ACMonitor.Lock(this._syncReceive._20010_QueueSyncLock))
                this.ReceiveQueue.Enqueue(message);

            // Signalisiere Thread, dass neue Message ansteht
            _syncReceive.NewItemEvent.Set();
        }


        /// <summary>
        /// Consumer-Method
        /// </summary>
        private void ProcessMessageFromPeer()
        {
            while (!_syncReceive.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Message ansteht
                _syncReceive.NewItemEvent.WaitOne();

                _receiveThread.StartReportingExeTime();
                while (ReceiveQueue.Count > 0)
                {
                    if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                        break;
                    WCFMessage acMessage = null;
                    //dequeue a message from the send queue

                    using (ACMonitor.Lock(this._syncReceive._20010_QueueSyncLock))
                    {
                        if (ReceiveQueue.Count <= 0)
                            break;
                        acMessage = ReceiveQueue.Dequeue();
                    }
                    if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                        break;
                    if (acMessage != null)
                        ProcessACMessage(acMessage);
                }
                _receiveThread.StopReportingExeTime();
            }
            _syncReceive.ThreadTerminated();
        }


        /// <summary>
        /// Analyzing and Processing Recieved Messages
        /// </summary>
        /// <param name="acMessage"></param>
        private void ProcessACMessage(WCFMessage acMessage)
        {
            if (acMessage == null)
                return;
            if (_serviceOfPeer == null || _serviceOfPeer.ClosingConnection)
                return;

            if (acMessage.ACParameter != null)
            {
                if ((acMessage.ACParameter.Any()) && (acMessage.ACParameter[0] != null))
                {
                    if (acMessage.ACParameter[0] is ACConnect)
                    {
                        // Alles ok!
                        ACConnect connectMsg = (ACConnect)acMessage.ACParameter[0];
                        UserName = connectMsg.UserName;
                        Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ProcessACMessage(ACConnect)", ConnectionDetailXML);
                        return;
                    }
                    else if (acMessage.ACParameter[0] is ACDisconnect)
                    {
                        Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ProcessACMessage()", "ACDisconnect");
                        ACDisconnect disconnect = acMessage.ACParameter[0] as ACDisconnect;
                        if (disconnect.ShutdownClient)
                        {
                            this.WCFServiceManager.ShutdownClients();
                        }
                        return;
                    }
                    else if (acMessage.ACParameter[0] is ACSubscriptionMessage)
                    {
                        ACSubscriptionMessage message = acMessage.ACParameter[0] as ACSubscriptionMessage;

#if DEBUG
                        //Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ProcessACMessage()", String.Format("SubscribeRequest: {0}, UnSubscribeRequest: {1}", message.ACPSubscribe.ProjectSubscriptionList.Count(), message.ACPUnSubscribe.ProjectSubscriptionList.Count()));
#endif

                        if (message.ACPSubscribe.ProjectSubscriptionList.Count > 0)
                        {
                            // Lock zuerst auf WCFServiceManager wegen Deadlock8.txt

                            using (ACMonitor.Lock(WCFServiceManager._20056_ACPLock))
                            {
                                // Aktualisiere Subscription
                                _SubscriptionOfPeer.Subscribe(message.ACPSubscribe);

                                // Übertrage Erstmalig Daten
                                List<IACPropertyNetValueEvent> eventList = _SubscriptionOfPeer.GetPropertyValuesOfNewSubscribedObjects(this.Root, this);
                                if (eventList != null)
                                {
                                    if (eventList.Any())
                                    {
                                        ACPropertyValueMessage messageEvents = new ACPropertyValueMessage();
                                        messageEvents.PropertyValues = eventList;
                                        WCFMessage acMessageEvents = NewACMessage("", new Object[] { messageEvents });
                                        EnqeueMessageForPeer(acMessageEvents);
                                    }
                                }

                                if (WCFServiceManager != null)
                                    WCFServiceManager.OnSubscriptionUpdated();
                            }
                        }
                        if (message.ACPUnSubscribe.ProjectSubscriptionList.Count > 0)
                        {
                            // Lock zuerst auf WCFServiceManager wegen Deadlock8.txt

                            using (ACMonitor.Lock(WCFServiceManager._20056_ACPLock))
                            {
                                // Aktualisiere Subscription
                                _SubscriptionOfPeer.UnSubscribe(message.ACPUnSubscribe);
                            }
                        }
                        return;
                    }
                    else if (acMessage.ACParameter[0] is ACPropertyValueMessage)
                    {
#if DEBUG
                        Messages.LogDebug(this.GetACUrl(), "ACWCFServerChannel.ProcessACMessage()", "ACPropertyValueMessage");
#endif

                        ACPropertyValueMessage message = acMessage.ACParameter[0] as ACPropertyValueMessage;
                        string prevACUrl = "";
                        foreach (IACPropertyNetValueEvent propertyValueEvent in message.PropertyValues)
                        {
                            if (String.IsNullOrEmpty(propertyValueEvent.ACUrl))
                                (propertyValueEvent as IACPropertyNetValueEventExt).SetACUrl(prevACUrl);
                            else
                                prevACUrl = propertyValueEvent.ACUrl;
                            object result = null;
                            result = Root.ACUrlCommand(propertyValueEvent.ACUrl);
                            if (result != null)
                            {
                                if (result is ACComponent)
                                {
                                    IACPropertyNetBase acProperty = (result as ACComponent).GetPropertyNet(propertyValueEvent.ACIdentifier);
                                    if (acProperty != null)
                                        acProperty.OnValueEventReceivedRemote(propertyValueEvent);
                                }
                            }
                        }
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(this.ACIdentifier))
                return;

            ACMethodInvocationResult messageResult = new ACMethodInvocationResult();
            var vbDump = Root.VBDump;
            if (acMessage.ACUrl[0] != '&')
            {
                PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACUrlForLogger, 101) : null;
                try
                {
                    messageResult.MethodResult = Root.ACUrlCommand(acMessage.ACUrl, acMessage.ACParameter);
                }
                finally
                {
                    if (perfEvent != null && vbDump != null)
                    {
                        vbDump.PerfLoggerStop(ACUrlForLogger, 101, perfEvent);
                        if (perfEvent.IsTimedOut)
                            Messages.LogDebug(this.GetACUrl(), "ProcessACMessage(Duration 101)", acMessage.ACUrl);
                    }
                }
                WCFMessage acMessageResult = WCFMessage.NewACMessage(acMessage.ACUrlRequester, acMessage.ACUrl, acMessage.MethodInvokeRequestID, new Object[] { messageResult });
                if (!acMessage.ACUrl.Contains("GetChildInstanceInfo"))
                    Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ProcessACMessage()", String.Format("ACMethodInvocationResult: {0} {1}", acMessageResult.ACUrl, acMessageResult.MethodInvokeRequestID));
                perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACUrlForLogger, 102) : null;
                EnqeueMessageForPeer(acMessageResult);
                if (perfEvent != null && vbDump != null)
                {
                    vbDump.PerfLoggerStop(ACUrlForLogger, 102, perfEvent);
                    if (perfEvent.IsTimedOut)
                        Messages.LogDebug(this.GetACUrl(), "ProcessACMessage(Duration 102)", acMessage.ACUrl);
                }
            }
            else
            {
                PerformanceEvent perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACUrlForLogger, 111) : null;
                try
                {
                    messageResult.MethodResult = Root.IsEnabledACUrlCommand(acMessage.ACUrl.Substring(1), acMessage.ACParameter);
                }
                finally
                {
                    if (perfEvent != null && vbDump != null)
                    {
                        vbDump.PerfLoggerStop(ACUrlForLogger, 111, perfEvent);
                        if (perfEvent.IsTimedOut)
                            Messages.LogDebug(this.GetACUrl(), "ProcessACMessage(Duration 111)", acMessage.ACUrl);
                    }
                }
                WCFMessage acMessageResult = WCFMessage.NewACMessage(acMessage.ACUrlRequester, acMessage.ACUrl.Substring(1), acMessage.MethodInvokeRequestID, new Object[] { messageResult });
                //Messages.LogDebug(this.GetACUrl(), "WCFServiceChannel.ProcessACMessage()", String.Format("ACMethodInvocationResult: {0} {1}", acMessageResult.ACUrl, acMessageResult.MethodInvokeRequestID));
                perfEvent = vbDump != null ? vbDump.PerfLoggerStart(ACUrlForLogger, 112) : null;
                try
                {
                    EnqeueMessageForPeer(acMessageResult);
                }
                finally
                {
                    if (perfEvent != null && vbDump != null)
                    {
                        vbDump.PerfLoggerStop(ACUrlForLogger, 112, perfEvent);
                        if (perfEvent.IsTimedOut)
                            Messages.LogDebug(this.GetACUrl(), "ProcessACMessage(Duration 112)", acMessage.ACUrl);
                    }
                }
            }
        }

#endregion

#region DataShow
        static public string KeyACIdentifier
        {
            get
            {
                return "ConnectedSince";
            }
        }
#endregion

#region SerializerTest
        private string SerializeTest(WCFMessage message)
        {
            if (message == null)
                return "";
            try
            {
                DataContractSerializer Serializer = new DataContractSerializer(typeof(WCFMessage), new DataContractSerializerSettings(){ KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    Serializer.WriteObject(xmlWriter, message);
                    return sw.ToString();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("WCFServiceChannel", "SerializeTest", msg);
            }
            return "";
        }
#endregion
    }
}
