using gip.core.autocomponent;
using gip.core.datamodel;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.communication
{
    public class OPCUAClientMonitoredItem : MonitoredItem
    {
        #region c'tors

        public OPCUAClientMonitoredItem(IACPropertyNetServer acProperty, string opcAddr, OPCUAClientACSubscr opcUASubscr) : base()
        {
            try
            {
                StartNodeId = new Opc.Ua.NodeId(opcAddr);
                _ACProperty = acProperty;
                _ACProperty.ValueUpdatedOnReceival += OnSendValueToOPCServer;
                this.Notification += OPCUAClientMonitoredItem_Notification;
                _OPCUAClientACSubscr = opcUASubscr;
                opcUASubscr.UASubscription.AddItem(this);
            }
            catch (Exception e)
            {
                if(ACRoot.SRoot != null && ACRoot.SRoot.Messages != null)
                    ACRoot.SRoot.Messages.LogException("Constructor", "OPCUAClientMonitoredItem", e);
            }
        }

        #endregion

        #region Properties

        private IACPropertyNetServer _ACProperty;
        public IACPropertyNetServer ACProperty
        {
            get
            {
                return _ACProperty;
            }
        }

        private OPCUAClientACSubscr _OPCUAClientACSubscr;

        public enum ResendLock
        {
            Unlocked = 0,
            Locked = 1,
            ResendDone = 2
        }

        internal ResendLock _ReSendLocked = ResendLock.Unlocked;
        private WriteValueCollection _WriteValues = new WriteValueCollection();
        //private bool _IsWriteable = false;

        #endregion

        #region Methods

        internal void DeInit()
        {
            if (_ACProperty != null)
                _ACProperty.ValueUpdatedOnReceival -= OnSendValueToOPCServer;
            this.Notification -= OPCUAClientMonitoredItem_Notification;
            _ACProperty = null;
        }

        void OnSendValueToOPCServer(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.AfterBroadcast || ACProperty == null) // || !_IsWriteable)
                return;

            if (_ReSendLocked >= ResendLock.Locked)
            {
                if (e.ValueEvent.InvokerInfo != null && e.ValueEvent.InvokerInfo is MonitoredItem)
                    return;
                else if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase && (ACProperty.Value as ACCustomTypeBase).Value == ((MonitoredItemNotification)this.LastValue).Value.Value)
                    return;
                else if (ACProperty.Value == ((MonitoredItemNotification)this.LastValue).Value.Value)
                    return;
                else
                    _ReSendLocked = ResendLock.ResendDone;
            }

            bool success = false;

            if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase)
                success = SetValue((ACProperty.Value as ACCustomTypeBase).Value);
            else
                success = SetValue(ACProperty.Value);

            if (!success)
                return;

            StatusCodeCollection status = null;
            DiagnosticInfoCollection diagnostic = null;
            ResponseHeader response = null;

            try
            {
                Session activeSession = _OPCUAClientACSubscr?.OPCUASession?.UASession;
                if (activeSession == null || activeSession.Disposed)
                    activeSession = Subscription.Session;
                response = activeSession.Write(null, _WriteValues, out status, out diagnostic);
                ClientBase.ValidateResponse(status, _WriteValues);
                ClientBase.ValidateDiagnosticInfos(diagnostic, _WriteValues);
            }
            catch (Exception ex)
            {
                _OPCUAClientACSubscr.OPCUASession.AddAlarm(new Msg("Write error: "+ex.Message, _OPCUAClientACSubscr, eMsgLevel.Error, "OPCUAClientMonitoredItem", "OnSendValueToOPCServer", 115));
                if (ACRoot.SRoot != null && ACRoot.SRoot.Messages != null)
                    ACRoot.SRoot.Messages.LogException("OnSendValueToOPCServer", "OPCUAClientMonitoredItem", ex);
            }

            if (status != null && StatusCode.IsBad(status[0]) && response != null)
            {
                string error = "";

                if (response.StringTable.Any())
                    error = response.StringTable.ToString();
                else
                {
                    if (status.Any())
                        error = status.FirstOrDefault().ToString();
                }

                _OPCUAClientACSubscr.OPCUASession.AddAlarm(new Msg("Write error: " + error, _OPCUAClientACSubscr, eMsgLevel.Error, "OPCUAClientMonitoredItem", "OnSendValueToOPCServer", 122));
                if (ACRoot.SRoot != null && ACRoot.SRoot.Messages != null)
                    ACRoot.SRoot.Messages.LogError("OnSendValueToOPCServer", "OPCUAClientMonitoredItem", error);
            }
        }

        private void OPCUAClientMonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;

            if (notification == null)
                return;

            ReadValue(notification.Value);

            if (notification.Value != null)
            {
                bool isGood = StatusCode.IsGood(notification.Value.StatusCode);
                if (!isGood && _OPCUAClientACSubscr != null)
                {
                     _OPCUAClientACSubscr.Messages.LogInfo(this.ResolvedNodeId.ToString(), "OPCUA KeepAlive status:", notification.Value.StatusCode.ToString());
                }
            }
        }

        internal void ReadInitValue(OPCUAClientACSubscr opcUASubscr)
        {
            ReadValue(opcUASubscr.OPCUASession.UASession.ReadValue(StartNodeId));
        }

        private void ReadValue(DataValue dataValue)
        {
            if (dataValue == null)
                return;

            // Falls Konfigurationsvariable: (Beschreiben von der SPS nicht erlaubt und Target-Property)
            if (!ACProperty.PropertyInfo.IsInput && ACProperty is IACPropertyNetTarget)
                return;

            _ReSendLocked = ResendLock.Locked;

            if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase)
                (ACProperty.Value as ACCustomTypeBase).Value = dataValue.Value;

            else
                ACProperty.ChangeValueServer(dataValue.Value, ACProperty.ForceBroadcast, this);

            _ReSendLocked = ResendLock.Unlocked;
        }

        private bool SetValue(object value)
        {
            WriteValue writeValue = GetWriteValue();

            try
            {
                DataValue dataValue = new DataValue(new Variant(value), new StatusCode(), DateTime.Now);
                writeValue.Value = dataValue;
            }
            catch(Exception e)
            {
                if (ACRoot.SRoot != null && ACRoot.SRoot.Messages != null)
                    ACRoot.SRoot.Messages.LogException("SetValue", "OPCUAClientMonitoredItem", e);

                return false;
            }

            return true;
        }

        private WriteValue GetWriteValue()
        {
            WriteValue writeValue = _WriteValues.FirstOrDefault();
            if (writeValue != null)
                return writeValue;

            writeValue = new WriteValue();
            writeValue.NodeId = this.ResolvedNodeId;
            writeValue.AttributeId = Attributes.Value;
            _WriteValues.Add(writeValue);

            return writeValue;
        }

        internal void ResolveAccess()
        {
            //VariableNode nodeItem = this.Subscription.Session.NodeCache.Find(StartNodeId) as VariableNode;
            //if (nodeItem != null)
            //{
            //    if (nodeItem.AccessLevel == AccessLevels.None 
            //        || nodeItem.AccessLevel == AccessLevels.CurrentReadOrWrite
            //        || nodeItem.AccessLevel == AccessLevels.CurrentWrite)
            //    {
            //        _IsWriteable = true;
            //    }
            //}
        }
        #endregion
    }
}
