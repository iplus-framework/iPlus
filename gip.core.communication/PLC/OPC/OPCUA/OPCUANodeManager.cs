using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using gip.core.autocomponent;
using gip.core.datamodel;
using Opc.Ua;
using Opc.Ua.Server;
using System.Linq;

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
            _SystemContext = _InternalServer.DefaultSystemContext.Copy();
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

        public OPCUASrvACService ParentACService
        {
            get
            {
                return ParentUAServer.ACService;
            }
        }

        private IServerInternal _InternalServer;
        private ServerSystemContext _SystemContext;
        protected ServerSystemContext SystemContext
        {
            get { return _SystemContext; }
        }

        private class NodeStateInfos
        {
            public NodeState Node { get; set; }
            public NodeStateReference Reference { get; set; }
            public ReferenceDescription ReferencesDesc { get; set; }

            public UAStateACComponent NodeComp
            {
                get
                {
                    return Node as UAStateACComponent;
                }
            }

            public UAStateACProperty NodeProp
            {
                get
                {
                    return Node as UAStateACProperty;
                }
            }
        }

        private ConcurrentDictionary<NodeId, NodeState> _MapNodeID2Member = new ConcurrentDictionary<NodeId, NodeState>();
        private ConcurrentDictionary<IACMember, NodeState> _MapMemberToNodeState = new ConcurrentDictionary<IACMember, NodeState>();
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
            foreach (KeyValuePair<NodeId, IList<IReference>> current in references)
            {
                // check for valid handle.
                NodeState source = GetManagerHandle(current.Key) as NodeState;
                if (source == null)
                    continue;

                using (ACMonitor.Lock(_30209_LockValue))
                {
                    // add reference to external target.
                    foreach (IReference reference in current.Value)
                    {
                        source.AddReference(reference.ReferenceTypeId, reference.IsInverse, reference.TargetId);
                    }
                }
            }
        }

        public void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            // 0. Beim Starten vom Server
            using (ACMonitor.Lock(_30209_LockValue))
            {
                // add the uris to the server's namespace table and cache the indexes.
                for (int i = 0; i < _CountNamespaces; i++)
                {
                    _NamespaceIndexes[i] = _InternalServer.NamespaceUris.GetIndexOrAppend(_NamespaceUris[i]);
                }

                NodeState rootNodeState = null;
                if (!_MapMemberToNodeState.TryGetValue(ParentACService.Root, out rootNodeState))
                {
                    rootNodeState = new UAStateACComponent(ParentACService.Root as ACComponent, null, _NamespaceIndexes[1]);
                    _MapMemberToNodeState.TryAdd(ParentACService.Root, rootNodeState);
                    _MapNodeID2Member.TryAdd(rootNodeState.NodeId, rootNodeState);

                    IList<IReference> references = new List<IReference>();
                    rootNodeState.GetReferences(SystemContext, references);
                    IReference refInCoreNodeManager = references.FirstOrDefault();
                    if (refInCoreNodeManager != null)
                    {
                        NodeId nodeIdInCoreManager = (NodeId)refInCoreNodeManager.TargetId;
                        IList<IReference> referencesToAdd = null;
                        if (!externalReferences.TryGetValue(nodeIdInCoreManager, out referencesToAdd))
                        {
                            externalReferences[nodeIdInCoreManager] = referencesToAdd = new List<IReference>();
                        }

                        // add reserve reference from external node.
                        ReferenceNode referenceToAdd = new ReferenceNode();
                        referenceToAdd.ReferenceTypeId = refInCoreNodeManager.ReferenceTypeId;
                        referenceToAdd.IsInverse = !refInCoreNodeManager.IsInverse;
                        referenceToAdd.TargetId = rootNodeState.NodeId;
                        referencesToAdd.Add(referenceToAdd);
                    }
                }
            }
        }

        public object GetManagerHandle(NodeId nodeId)
        {
            // 1. Bei Connect von Client
            // Rückgabe eines NodeState-Objektes
            NodeState node;
            if (_MapNodeID2Member.TryGetValue(nodeId, out node))
                return node;
            return null;
        }

        public NodeMetadata GetNodeMetadata(OperationContext context, object targetHandle, BrowseResultMask resultMask)
        {
            // 2. Das zurückegebene NodeState-Objekt wird dann als targetHandle wrder hier übergeben
            // Rückgabe der Metadaten
            NodeState targetNode = targetHandle as NodeState;
            if (targetNode == null)
                return null;
            if (!(targetNode is IUAStateIACMember))
                return null;

            ServerSystemContext systemContext = _SystemContext.Copy(context);

            using (ACMonitor.Lock(_30209_LockValue))
            {
                List<object> values = targetNode.ReadAttributes(
                systemContext,
                Attributes.WriteMask,
                Attributes.UserWriteMask,
                Attributes.DataType,
                Attributes.ValueRank,
                Attributes.ArrayDimensions,
                Attributes.AccessLevel,
                Attributes.UserAccessLevel,
                Attributes.EventNotifier,
                Attributes.Executable,
                Attributes.UserExecutable);

                // construct the metadata object.

                NodeMetadata metadata = new NodeMetadata(targetNode, targetNode.NodeId);

                metadata.NodeClass = targetNode.NodeClass;
                metadata.BrowseName = targetNode.BrowseName;
                metadata.DisplayName = targetNode.DisplayName;

                if (values[0] != null && values[1] != null)
                {
                    metadata.WriteMask = (AttributeWriteMask)(((uint)values[0]) & ((uint)values[1]));
                }

                metadata.DataType = (NodeId)values[2];

                if (values[3] != null)
                {
                    metadata.ValueRank = (int)values[3];
                }

                metadata.ArrayDimensions = (IList<uint>)values[4];

                if (values[5] != null && values[6] != null)
                {
                    metadata.AccessLevel = (byte)(((byte)values[5]) & ((byte)values[6]));
                }

                if (values[7] != null)
                {
                    metadata.EventNotifier = (byte)values[7];
                }

                if (values[8] != null && values[9] != null)
                {
                    metadata.Executable = (((bool)values[8]) && ((bool)values[9]));
                }

                // get instance references.
                BaseInstanceState instance = targetNode as BaseInstanceState;
                if (instance != null)
                {
                    metadata.TypeDefinition = instance.TypeDefinitionId;
                    metadata.ModellingRule = instance.ModellingRuleId;
                }

                // fill in the common attributes.
                return metadata;
            }
        }


        public void Browse(OperationContext context, ref ContinuationPoint continuationPoint, IList<ReferenceDescription> references)
        {
            // 3. Browse wird aufgerufen, nachdem der Client den Baum aufklappt
            // hier muss dann die references-Liste gefüllt werden
            if (continuationPoint == null) 
                throw new ArgumentNullException("continuationPoint");
            if (references == null) 
                throw new ArgumentNullException("references");

            // check for view.
            if (!ViewDescription.IsDefault(continuationPoint.View))
            {
                throw new ServiceResultException(StatusCodes.BadViewIdUnknown);
            }

            if (!(continuationPoint.NodeToBrowse is IUAStateIACMember))
            {
                throw new ServiceResultException(StatusCodes.BadNodeIdUnknown);
            }

            UAStateACComponent componentState2Browse = continuationPoint.NodeToBrowse as UAStateACComponent;
            if (componentState2Browse != null)
            {
                //ServerSystemContext systemContext = _SystemContext.Copy(context);

                IEnumerable<IACComponent> childs = null;
                IEnumerable<IACPropertyBase> properties = null;
                if (componentState2Browse.ACComponent is IRoot)
                    childs = componentState2Browse.ACComponent.FindChildComponents<ApplicationManager>(c => c is ApplicationManager, null, 1);
                else
                {
                    childs = componentState2Browse.ACComponent.ACComponentChilds.Where(c => !(c is IACComponentPWNode)
                                                                                    && !c.ACIdentifier.Contains(ACUrlHelper.Delimiter_InstanceNoOpen)
                                                                                    /*&& !c.ComponentClass.IsMultiInstanceInherited*/);
                    properties = componentState2Browse.ACComponent.ACPropertyList.Where(c => c.IsValueType
                                                                                            || c.PropertyType.IsEnum
                                                                                            || UAStateACProperty.BitAccessType.IsAssignableFrom(c.PropertyType));
                }

                using (ACMonitor.Lock(_30209_LockValue))
                {
                    if (childs != null)
                    {
                        foreach (IACComponent iACComponent in childs)
                        {
                            IReference reference = CreateNewReference(iACComponent as ACComponent, componentState2Browse);
                            if (reference != null)
                            {
                                ReferenceDescription refDesc = GetReferenceDescription(context, reference, continuationPoint);
                                if (refDesc != null)
                                    references.Add(refDesc);
                            }
                        }
                    }
                    if (properties != null)
                    {
                        foreach (IACPropertyBase iACProp in properties)
                        {
                            IReference reference = CreateNewReference(iACProp, componentState2Browse);
                            if (reference != null)
                            {
                                ReferenceDescription refDesc = GetReferenceDescription(context, reference, continuationPoint);
                                if (refDesc != null)
                                    references.Add(refDesc);
                            }
                        }
                    }
                }
            }

            continuationPoint.Dispose();
            continuationPoint = null;
        }

        public void Call(OperationContext context, IList<CallMethodRequest> methodsToCall, IList<CallMethodResult> results, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        public ServiceResult ConditionRefresh(OperationContext context, IList<IEventMonitoredItem> monitoredItems)
        {
            throw new NotImplementedException();
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
            ServerSystemContext systemContext = _SystemContext.Copy(context);
            IDictionary<NodeId, NodeState> operationCache = new NodeIdDictionary<NodeState>();
            List<ReadWriteOperationState> nodesToValidate = new List<ReadWriteOperationState>();

            for (int ii = 0; ii < nodesToRead.Count; ii++)
            {
                ReadValueId nodeToRead = nodesToRead[ii];

                // skip items that have already been processed.
                if (nodeToRead.Processed)
                {
                    continue;
                }

                // check for valid handle.
                UAStateACProperty source = GetManagerHandle(nodeToRead.NodeId) as UAStateACProperty;
                if (source == null)
                {
                    continue;
                }

                // owned by this node manager.
                nodeToRead.Processed = true;

                // create an initial value.
                DataValue value = values[ii] = new DataValue();

                value.Value = source.ACProperty.Value;
                value.ServerTimestamp = DateTime.UtcNow;
                value.SourceTimestamp = DateTime.Now;
                value.StatusCode = StatusCodes.Good;

                // check if the node is ready for reading.
                if (source.ValidationRequired)
                {
                    errors[ii] = StatusCodes.BadNodeIdUnknown;

                    // must validate node in a seperate operation.
                    ReadWriteOperationState operation = new ReadWriteOperationState();

                    operation.Source = source;
                    operation.Index = ii;

                    nodesToValidate.Add(operation);

                    continue;
                }

                // read the attribute value.
                errors[ii] = source.ReadAttribute(
                    systemContext,
                    nodeToRead.AttributeId,
                    nodeToRead.ParsedIndexRange,
                    nodeToRead.DataEncoding,
                    value);
            }

            // check for nothing to do.
            if (nodesToValidate.Count == 0)
            {
                return;
            }

            // validates the nodes (reads values from the underlying data source if required).
            for (int ii = 0; ii < nodesToValidate.Count; ii++)
            {
                ReadWriteOperationState operation = nodesToValidate[ii];

                if (!ValidateNode(systemContext, operation.Source))
                {
                    continue;
                }

                ReadValueId nodeToRead = nodesToRead[operation.Index];
                DataValue value = values[operation.Index];

                // update the attribute value.
                errors[operation.Index] = operation.Source.ReadAttribute(
                    systemContext,
                    nodeToRead.AttributeId,
                    nodeToRead.ParsedIndexRange,
                    nodeToRead.DataEncoding,
                    value);
            }
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
        //protected virtual bool IsNodeIdInNamespace(NodeId nodeId)
        //{
        //    if (NodeId.IsNull(nodeId))
        //    {
        //        return false;
        //    }

        //    // quickly exclude nodes that not in the namespace.
        //    for (int ii = 0; ii < _NamespaceIndexes.Length; ii++)
        //    {
        //        if (nodeId.NamespaceIndex == _NamespaceIndexes[ii])
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private IReference CreateNewReference(IACPropertyBase property, UAStateACComponent parent)
        {
            NodeState checkIfExists = null;
            if (_MapMemberToNodeState.TryGetValue(property, out checkIfExists))
            {
                return new NodeStateReference(ReferenceTypeIds.HasComponent, false, checkIfExists);
            }

            UAStateACProperty newStateObj = new UAStateACProperty(property, parent, _NamespaceIndexes[1]);
            _MapMemberToNodeState.TryAdd(property, newStateObj);
            _MapNodeID2Member.TryAdd(newStateObj.NodeId, newStateObj);
            return new NodeStateReference(ReferenceTypeIds.HasComponent, false, newStateObj);
        }


        private IReference CreateNewReference(ACComponent component, UAStateACComponent parent)
        {
            NodeState checkIfExists = null;
            if (_MapMemberToNodeState.TryGetValue(component, out checkIfExists))
            {
                return new NodeStateReference(ReferenceTypeIds.Organizes, false, checkIfExists);
            }

            UAStateACComponent newStateObj = new UAStateACComponent(component, parent, _NamespaceIndexes[1]);
            _MapMemberToNodeState.TryAdd(component, newStateObj);
            _MapNodeID2Member.TryAdd(newStateObj.NodeId, newStateObj);
            return new NodeStateReference(ReferenceTypeIds.Organizes, false, newStateObj);
        }

        private ReferenceDescription GetReferenceDescription(
            OperationContext context,
            IReference reference,
            ContinuationPoint continuationPoint)
        {
            // create the type definition reference.        
            ReferenceDescription description = new ReferenceDescription();

            description.NodeId = reference.TargetId;
            description.SetReferenceType(continuationPoint.ResultMask, reference.ReferenceTypeId, !reference.IsInverse);

            // do not cache target parameters for remote nodes.
            if (reference.TargetId.IsAbsolute)
            {
                // only return remote references if no node class filter is specified.
                if (continuationPoint.NodeClassMask != 0)
                {
                    return null;
                }

                return description;
            }

            NodeState target = null;

            // check for local reference.
            NodeStateReference referenceInfo = reference as NodeStateReference;

            if (referenceInfo != null)
            {
                target = referenceInfo.Target;
            }

            // check for internal reference.
            if (target == null)
            {
                NodeId targetId = (NodeId)reference.TargetId;
                NodeState node;
                if (!_MapNodeID2Member.TryGetValue(targetId, out node))
                    target = null;
            }

            // the target may be a reference to a node in another node manager. In these cases
            // the target attributes must be fetched by the caller. The Unfiltered flag tells the
            // caller to do that.
            if (target == null)
            {
                description.Unfiltered = true;
                return description;
            }

            // apply node class filter.
            if (continuationPoint.NodeClassMask != 0 && ((continuationPoint.NodeClassMask & (uint)target.NodeClass) == 0))
            {
                return null;
            }

            NodeId typeDefinition = null;

            BaseInstanceState instance = target as BaseInstanceState;

            if (instance != null)
            {
                typeDefinition = instance.TypeDefinitionId;
            }

            // set target attributes.
            description.SetTargetAttributes(
                continuationPoint.ResultMask,
                target.NodeClass,
                target.BrowseName,
                target.DisplayName,
                typeDefinition);

            return description;
        }

        /// <summary>
        /// Stores the state of a call method operation.
        /// </summary>
        private struct ReadWriteOperationState
        {
            public NodeState Source;
            public int Index;
        }


        protected virtual bool ValidateNode(ServerSystemContext context, NodeState node)
        {
            // validate node only if required.
            if (node.ValidationRequired)
            {
                return node.Validate(context);
            }

            return true;
        }


        #endregion
    }
}
