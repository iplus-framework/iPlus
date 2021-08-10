using System;
using System.Collections.Generic;
using gip.core.datamodel;
using Opc.Ua;
using Opc.Ua.Server;

namespace gip.core.communication
{
    /// <summary>
    /// A class which implements an instance of a UA server.
    /// </summary>
    public class OPCUANodeManager : INodeManager
    {
        #region c'tors
        public OPCUANodeManager(
                    Opc.Ua.Server.IServerInternal server,
                    ApplicationConfiguration configuration,
                    OPCUASrvServer parentUAServer)
        {
            _ParentUAServer = parentUAServer;
            _InternalServer = server;
            _NamespaceIndex = server.NamespaceUris.GetIndexOrAppend(_NamespaceUris[1]);
        }
        #endregion

        #region Properties
        private const int _CountNamespaces = 2;
        public readonly ACMonitorObject _30209_LockValue = new ACMonitorObject(30209);
        OPCUASrvServer _ParentUAServer = null;
        public OPCUASrvServer ParentUAServer
        {
            get
            {
                using (ACMonitor.Lock(_30209_LockValue))
                {
                    return _ParentUAServer;
                }
            }
        }

        private IServerInternal _InternalServer;
        #endregion


        #region Implementation INodeManager
        private readonly string[] _NamespaceUris = new string[] { OPCUASrvACService.Namespace_UA_App, OPCUASrvACService.Namespace_UA_App + "/Instance" };
        public IEnumerable<string> NamespaceUris
        {
            get
            {
                return _NamespaceUris;
            }
        }
        private ushort _NamespaceIndex;
        private ushort[] _NamespaceIndexes = new ushort[_CountNamespaces];

        public void AddReferences(IDictionary<NodeId, IList<IReference>> references)
        {
            throw new NotImplementedException();
        }

        public void Browse(OperationContext context, ref ContinuationPoint continuationPoint, IList<ReferenceDescription> references)
        {
            throw new NotImplementedException();
        }

        public void Call(OperationContext context, IList<CallMethodRequest> methodsToCall, IList<CallMethodResult> results, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public ServiceResult ConditionRefresh(OperationContext context, IList<IEventMonitoredItem> monitoredItems)
        {
            throw new NotImplementedException();
        }

        public void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            using (ACMonitor.Lock(_30209_LockValue))
            {
                // add the uris to the server's namespace table and cache the indexes.
                for (int i = 0; i < _CountNamespaces; i++)
                {
                    _NamespaceIndexes[i] = _InternalServer.NamespaceUris.GetIndexOrAppend(_NamespaceUris[i]);
                }

                //LoadPredefinedNodes(m_systemContext, externalReferences);
            }
        }

        public void CreateMonitoredItems(OperationContext context, uint subscriptionId, double publishingInterval, TimestampsToReturn timestampsToReturn, IList<MonitoredItemCreateRequest> itemsToCreate, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterErrors, IList<IMonitoredItem> monitoredItems, ref long globalIdCounter)
        {
            throw new NotImplementedException();
        }

        public void DeleteAddressSpace()
        {
            throw new NotImplementedException();
        }

        public void DeleteMonitoredItems(OperationContext context, IList<IMonitoredItem> monitoredItems, IList<bool> processedItems, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public ServiceResult DeleteReference(object sourceHandle, NodeId referenceTypeId, bool isInverse, ExpandedNodeId targetId, bool deleteBidirectional)
        {
            throw new NotImplementedException();
        }

        public object GetManagerHandle(NodeId nodeId)
        {
            throw new NotImplementedException();
        }

        public NodeMetadata GetNodeMetadata(OperationContext context, object targetHandle, BrowseResultMask resultMask)
        {
            throw new NotImplementedException();
        }

        public void HistoryRead(OperationContext context, HistoryReadDetails details, TimestampsToReturn timestampsToReturn, bool releaseContinuationPoints, IList<HistoryReadValueId> nodesToRead, IList<HistoryReadResult> results, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public void HistoryUpdate(OperationContext context, Type detailsType, IList<HistoryUpdateDetails> nodesToUpdate, IList<HistoryUpdateResult> results, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public void ModifyMonitoredItems(OperationContext context, TimestampsToReturn timestampsToReturn, IList<IMonitoredItem> monitoredItems, IList<MonitoredItemModifyRequest> itemsToModify, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterErrors)
        {
            throw new NotImplementedException();
        }

        public void Read(OperationContext context, double maxAge, IList<ReadValueId> nodesToRead, IList<DataValue> values, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public void SetMonitoringMode(OperationContext context, MonitoringMode monitoringMode, IList<IMonitoredItem> monitoredItems, IList<bool> processedItems, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public ServiceResult SubscribeToAllEvents(OperationContext context, uint subscriptionId, IEventMonitoredItem monitoredItem, bool unsubscribe)
        {
            throw new NotImplementedException();
        }

        public ServiceResult SubscribeToEvents(OperationContext context, object sourceId, uint subscriptionId, IEventMonitoredItem monitoredItem, bool unsubscribe)
        {
            throw new NotImplementedException();
        }

        public void TranslateBrowsePath(OperationContext context, object sourceHandle, RelativePathElement relativePath, IList<ExpandedNodeId> targetIds, IList<NodeId> unresolvedTargetIds)
        {
            throw new NotImplementedException();
        }

        public void Write(OperationContext context, IList<WriteValue> nodesToWrite, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper-Methods
        protected virtual bool IsNodeIdInNamespace(NodeId nodeId)
        {
            if (NodeId.IsNull(nodeId))
            {
                return false;
            }

            // quickly exclude nodes that not in the namespace.
            for (int ii = 0; ii < _NamespaceIndexes.Length; ii++)
            {
                if (nodeId.NamespaceIndex == _NamespaceIndexes[ii])
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
