using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;
using System.Security.Cryptography.X509Certificates;
using gip.core.datamodel;
using System.Linq;
using System.Collections.Concurrent;
using gip.core.autocomponent;

namespace gip.core.communication
{
    /// <summary>
    /// Keeps track of the monitored items for a single node.
    /// </summary>
    public class OPCUAServerMonitoredItem : MonitoredItem
    {
        #region Constructors
        public OPCUAServerMonitoredItem(
            IServerInternal server,
            INodeManager nodeManager,
            UAStateACProperty mangerHandle,
            uint subscriptionId,
            uint id,
            Session session,
            ReadValueId itemToMonitor,
            DiagnosticsMasks diagnosticsMasks,
            TimestampsToReturn timestampsToReturn,
            MonitoringMode monitoringMode,
            uint clientHandle,
            MonitoringFilter originalFilter,
            MonitoringFilter filterToUse,
            Opc.Ua.Range range,
            double samplingInterval,
            uint queueSize,
            bool discardOldest,
            double minimumSamplingInterval)
        :
            base(
                server,
                nodeManager,
                mangerHandle,
                subscriptionId,
                id,
                session,
                itemToMonitor,
                diagnosticsMasks,
                timestampsToReturn,
                monitoringMode,
                clientHandle,
                originalFilter,
                filterToUse,
                range,
                samplingInterval,
                queueSize,
                discardOldest,
                minimumSamplingInterval)
        {
            Subscribe();
        }
        #endregion

        #region Methods
        public void Subscribe()
        {
            IACPropertyNetServer serverProperty = ACServerProperty;
            if (serverProperty != null)
                serverProperty.ValueUpdatedOnReceival += ServerProperty_ValueUpdatedOnReceival;
            else
                ACProperty.PropertyChanged += ACProperty_PropertyChanged;
        }

        public void UnSubscribe()
        {
            IACPropertyNetServer serverProperty = ACServerProperty;
            if (serverProperty != null)
                serverProperty.ValueUpdatedOnReceival -= ServerProperty_ValueUpdatedOnReceival;
            else
                ACProperty.PropertyChanged -= ACProperty_PropertyChanged;
        }

        public UAStateACProperty StateProperty
        {
            get
            {
                return this.ManagerHandle as UAStateACProperty;
            }
        }

        public IACPropertyNetServer ACServerProperty
        {
            get
            {
                if (StateProperty == null)
                    return null;
                return StateProperty.ACProperty as IACPropertyNetServer;
            }
        }


        public IACPropertyBase ACProperty
        {
            get
            {
                if (StateProperty == null)
                    return null;
                return StateProperty.ACProperty;
            }
        }


        private void ServerProperty_ValueUpdatedOnReceival(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.AfterBroadcast)
                BroadcastCurrentValue();
        }

        private void ACProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BroadcastCurrentValue();
        }

        public void BroadcastCurrentValue()
        {
            if (StateProperty.WrappedValue != StateProperty.ACValueAsVariant)
                StateProperty.WrappedValue = StateProperty.ACValueAsVariant;
            DataValue dtValue = new DataValue(StateProperty.ACValueAsVariant);
            //dtValue.Value = StateProperty.ACValue;
            dtValue.ServerTimestamp = DateTime.UtcNow;
            dtValue.SourceTimestamp = DateTime.UtcNow;
            dtValue.StatusCode = StatusCodes.Good;

            //OPCUANodeManager nodeManager = this.NodeManager as OPCUANodeManager;
            //ServiceResult error = StateProperty.ReadAttribute(
            //                nodeManager.InternalServer,
            //                StateProperty.NodeId.AttributeId,
            //                itemToCreate.ItemToMonitor.ParsedIndexRange,
            //                itemToCreate.ItemToMonitor.DataEncoding,
            //                initialValue);

            this.QueueValue(dtValue, null);
        }

        /// <summary>
        /// Modifies the monitored item parameters,
        /// </summary>
        public ServiceResult Modify(
            DiagnosticsMasks diagnosticsMasks,
            TimestampsToReturn timestampsToReturn,
            uint clientHandle,
            double samplingInterval)
        {
            return base.ModifyAttributes(diagnosticsMasks,
                timestampsToReturn,
                clientHandle,
                null,
                null,
                null,
                samplingInterval,
                0,
                false);
        }
        #endregion
    }
}
