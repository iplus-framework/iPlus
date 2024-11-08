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
        public IServerInternal InternalServer
        {
            get
            {
                return _InternalServer;
            }
        }

        private ServerSystemContext _SystemContext;
        protected ServerSystemContext SystemContext
        {
            get { return _SystemContext; }
        }

        public class NodeStateInfo
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

            public UAStateACMethod NodeMethod
            {
                get
                {
                    return Node as UAStateACMethod;
                }
            }
        }

        internal ConcurrentDictionary<NodeId, NodeStateInfo> _MapNodeID2Member = new ConcurrentDictionary<NodeId, NodeStateInfo>();
        internal ConcurrentDictionary<IACObject, NodeStateInfo> _MapMemberToNodeState = new ConcurrentDictionary<IACObject, NodeStateInfo>();
        #endregion


        #region Implementation INodeManager
        private readonly string[] _NamespaceUris = new string[] { OPCUASrvACService.Namespace_UA_App, OPCUASrvACService.Namespace_UA_App + "/Instance" };
        /// <summary>
        /// 1. Call from UA-Server
        /// </summary>
        public IEnumerable<string> NamespaceUris
        {
            get
            {
                return _NamespaceUris;
            }
        }
        private ushort _NamespaceIndex;
        internal ushort[] _NamespaceIndexes = new ushort[_CountNamespaces];

        public void AddReferences(IDictionary<NodeId, IList<IReference>> references)
        {
            foreach (KeyValuePair<NodeId, IList<IReference>> current in references)
            {
                // check for valid handle.
                NodeStateInfo sourceInfo = GetManagerHandleInfo(current.Key, true, null) as NodeStateInfo;
                if (sourceInfo == null)
                    continue;

                using (ACMonitor.Lock(_30209_LockValue))
                {
                    // add reference to external target.
                    foreach (IReference reference in current.Value)
                    {
                        sourceInfo.Node.AddReference(reference.ReferenceTypeId, reference.IsInverse, reference.TargetId);
                    }
                }
            }
        }

        /// <summary>
        /// 2. Call from UA-Server
        /// </summary>
        /// <param name="externalReferences"></param>
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

                NodeStateInfo rootNodeStateInfo = null;
                if (!_MapMemberToNodeState.TryGetValue(ParentACService.Root, out rootNodeStateInfo))
                {
                    NodeState rootNodeState = new UAStateACComponent(ParentACService.Root as ACComponent, null, _NamespaceIndexes[1], 0);

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

                    rootNodeStateInfo = new NodeStateInfo() { Node = rootNodeState };
                    _MapMemberToNodeState.TryAdd(ParentACService.Root, rootNodeStateInfo);
                    _MapNodeID2Member.TryAdd(rootNodeState.NodeId, rootNodeStateInfo);
                }
            }
        }


        /// <summary>
        ///  3. Call from UA-Server an clients to get a NodaeState-Instances throut the NodeId
        ///  The NodeId he has whether retrieved via Browsing and GetNodeMetadata
        ///  or the client has a persisted list of Urls's NodeId's) on it's side
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public object GetManagerHandle(NodeId nodeId)
        {
            NodeStateInfo node = GetManagerHandleInfo(nodeId, true, null);
            return node != null ? node.Node : null;
        }


        /// <summary>
        /// 4. Call from UA-Server when Client has establisehd a connection an needs informations about a node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="targetHandle"></param>
        /// <param name="resultMask"></param>
        /// <returns></returns>
        public NodeMetadata GetNodeMetadata(OperationContext context, object targetHandle, BrowseResultMask resultMask)
        {
            // 2. Das zurückegebene NodeState-Objekt wird dann als targetHandle wrder hier übergeben
            // Rückgabe der Metadaten
            NodeState targetNode = targetHandle as NodeState;
            if (targetNode == null)
                return null;
            IUAStateIACMember acNode = targetNode as IUAStateIACMember;
            if (acNode == null)
                return null;

            using (ACMonitor.Lock(_30209_LockValue))
            {
                ServerSystemContext systemContext = _SystemContext.Copy(context);
                ACComponent component = acNode.ACComponent;
                ClassRightManager rightManager = GetRightManager(component, systemContext);
                if (rightManager == null)
                    return null;
                Global.ControlModes controlModes = acNode.ACMember.ACType.IsRightmanagement ? rightManager.GetControlMode(acNode.ACMember is ACClassMethod ? acNode.ACMember as ACClassMethod : acNode.ACMember.ACType) : rightManager.GetControlMode(component.ACType);
                if (   (acNode.ACMember is IACPropertyBase && controlModes <= Global.ControlModes.Hidden)
                    || (!(acNode.ACMember is IACPropertyBase) && controlModes <= Global.ControlModes.Disabled))
                    return null;

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

                //metadata.RolePermissions = GetPermissions(controlModes);
                //metadata.UserRolePermissions = GetPermissions(controlModes);

                metadata.AccessLevel = Convert2UARights(controlModes);
                //if (values[5] != null && values[6] != null)
                //{
                //    metadata.AccessLevel = (byte)(((byte)values[5]) & ((byte)values[6]));
                //}

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


        /// <summary>
        /// 5. client browses inside the tree 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="continuationPoint"></param>
        /// <param name="references"></param>
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
                IEnumerable<IACComponent> childs = null;
                IEnumerable<IACPropertyBase> properties = null;
                IEnumerable<ACClassMethod> methods = null;
                if (componentState2Browse.ACComponent is IRoot)
                    childs = componentState2Browse.ACComponent.FindChildComponents<ApplicationManager>(c => c is ApplicationManager, null, 1)
                                                                                                    .OrderBy(c => c.ACIdentifier);
                else
                {
                    childs = componentState2Browse.ACComponent.ACComponentChilds.Where(c => !(c is IACComponentPWNode)
                                                                                    && !c.ACIdentifier.Contains(ACUrlHelper.Delimiter_InstanceNoOpen)
                                                                                    /*&& !c.ComponentClass.IsMultiInstanceInherited*/)
                                                                                    .OrderBy(c => c.ACIdentifier);
                    properties = componentState2Browse.ACComponent.ACPropertyList.Where(c => UAStateACProperty.IsUAType(c))
                                                                                    .OrderBy(c => c.ACIdentifier);
                    methods = componentState2Browse.ACComponent.ACClassMethods.Where(c => c.IsCommand || c.IsInteraction);
                }

                using (ACMonitor.Lock(_30209_LockValue))
                {
                    ServerSystemContext systemContext = _SystemContext.Copy(context);
                    ClassRightManager rightManager = GetRightManager(componentState2Browse.ACComponent, systemContext);
                    if (rightManager != null)
                    {
                        uint i = 0;
                        if (childs != null)
                        {
                            i = 0;
                            foreach (IACComponent iACComponent in childs)
                            {
                                ClassRightManager rightManagerChild = GetRightManager(iACComponent as ACComponent, systemContext);
                                if (rightManagerChild != null)
                                {
                                    Global.ControlModes controlModes = rightManagerChild.GetControlMode(iACComponent.ACType);
                                    if (controlModes <= Global.ControlModes.Disabled)
                                        continue;

                                    NodeStateInfo nodeStateInfo = GetOrCreateNewNodeState(iACComponent as ACComponent, componentState2Browse, i);
                                    i++;
                                    if (nodeStateInfo.ReferencesDesc == null || nodeStateInfo.Reference != null)
                                        nodeStateInfo.ReferencesDesc = GetReferenceDescription(context, nodeStateInfo.Reference, continuationPoint);
                                    if (nodeStateInfo.ReferencesDesc != null)
                                        references.Add(nodeStateInfo.ReferencesDesc);
                                }
                            }
                        }
                        if (properties != null)
                        {
                            i = 0;
                            foreach (IACPropertyBase iACProp in properties)
                            {
                                Global.ControlModes controlModes = iACProp.ACType.IsRightmanagement ? rightManager.GetControlMode(iACProp.ACType) : rightManager.GetControlMode(componentState2Browse.ACComponent.ACType);
                                if (controlModes <= Global.ControlModes.Hidden)
                                    continue;

                                NodeStateInfo nodeStateInfo = GetOrCreateNewNodeState(iACProp, componentState2Browse, i);
                                i++;
                                if (nodeStateInfo.ReferencesDesc == null || nodeStateInfo.Reference != null)
                                    nodeStateInfo.ReferencesDesc = GetReferenceDescription(context, nodeStateInfo.Reference, continuationPoint);
                                if (nodeStateInfo.ReferencesDesc != null)
                                    references.Add(nodeStateInfo.ReferencesDesc);
                            }
                        }
                        if (methods != null)
                        {
                            i = 0;
                            foreach (ACClassMethod method in methods)
                            {
                                Global.ControlModes controlModes = rightManager.GetControlMode(method);
                                //Global.ControlModes controlModes = method.ACType.IsRightmanagement ? rightManager.GetControlMode(method.ACType) : rightManager.GetControlMode(componentState2Browse.ACComponent.ACType);
                                if (controlModes <= Global.ControlModes.Disabled)
                                    continue;

                                NodeStateInfo nodeStateInfo = GetOrCreateNewNodeState(method, componentState2Browse, i);
                                i++;
                                if (nodeStateInfo.ReferencesDesc == null || nodeStateInfo.Reference != null)
                                    nodeStateInfo.ReferencesDesc = GetReferenceDescription(context, nodeStateInfo.Reference, continuationPoint);
                                if (nodeStateInfo.ReferencesDesc != null)
                                    references.Add(nodeStateInfo.ReferencesDesc);
                            }
                        }
                    }
                }
            }

            continuationPoint.Dispose();
            continuationPoint = null;
        }


        /// <summary>
        /// 6. Reading Property Values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxAge"></param>
        /// <param name="nodesToRead"></param>
        /// <param name="values"></param>
        /// <param name="errors"></param>
        public void Read(OperationContext context, double maxAge, IList<ReadValueId> nodesToRead, IList<DataValue> values, IList<ServiceResult> errors)
        {
            IDictionary<NodeId, NodeState> operationCache = new NodeIdDictionary<NodeState>();
            List<ReadWriteOperationState> nodesToValidate = new List<ReadWriteOperationState>();

            ServerSystemContext systemContext = _SystemContext.Copy(context);
            for (int ii = 0; ii < nodesToRead.Count; ii++)
            {
                ReadValueId nodeToRead = nodesToRead[ii];

                // skip items that have already been processed.
                if (nodeToRead.Processed)
                {
                    continue;
                }

                // check for valid handle.
                NodeStateInfo sourceInfo = GetManagerHandleInfo(nodeToRead.NodeId, true, systemContext) as NodeStateInfo;
                if (sourceInfo == null)
                    continue;
                IUAStateIACMember source = sourceInfo.Node as IUAStateIACMember;
                if (source == null)
                    continue;

                // owned by this node manager.
                nodeToRead.Processed = true;

                // create an initial value.
                UAStateACProperty uaProperty = source as UAStateACProperty;
                UAStateACMethod uaMethod = source as UAStateACMethod;

                DataValue value = null;
                if (uaProperty != null)
                {
                    if (uaProperty.WrappedValue != uaProperty.ACValueAsVariant)
                        uaProperty.WrappedValue = uaProperty.ACValueAsVariant;
                    value = new DataValue(uaProperty.ACValueAsVariant);
                }
                else //if (uaMethod != null)
                    value = new DataValue();
                values[ii] = value;
                value.ServerTimestamp = DateTime.UtcNow;
                value.SourceTimestamp = DateTime.MinValue;
                value.StatusCode = StatusCodes.Good;

                // check if the node is ready for reading.
                if (uaProperty != null && uaProperty.ValidationRequired)
                {
                    errors[ii] = StatusCodes.BadNodeIdUnknown;

                    // must validate node in a seperate operation.
                    ReadWriteOperationState operation = new ReadWriteOperationState();

                    operation.Source = uaProperty;
                    operation.Index = ii;

                    nodesToValidate.Add(operation);

                    continue;
                }

                //read the attribute value.
                errors[ii] = sourceInfo.Node.ReadAttribute(
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
                //errors[ii] = ServiceResult.Good;
            }
        }

        /// <summary>
        /// 7. writing property values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="nodesToWrite"></param>
        /// <param name="errors"></param>
        public void Write(OperationContext context, IList<WriteValue> nodesToWrite, IList<ServiceResult> errors)
        {
            ServerSystemContext systemContext = _SystemContext.Copy(context);
            IDictionary<NodeId, NodeState> operationCache = new NodeIdDictionary<NodeState>();
            List<ReadWriteOperationState> nodesToValidate = new List<ReadWriteOperationState>();

            for (int ii = 0; ii < nodesToWrite.Count; ii++)
            {
                WriteValue nodeToWrite = nodesToWrite[ii];

                // skip items that have already been processed.
                if (nodeToWrite.Processed)
                    continue;

                // check for valid handle.
                NodeStateInfo sourceInfo = GetManagerHandleInfo(nodeToWrite.NodeId, true, systemContext) as NodeStateInfo;
                if (sourceInfo == null)
                    continue;
                UAStateACProperty source = sourceInfo.Node as UAStateACProperty;
                if (source == null)
                    continue;

                // owned by this node manager.
                nodeToWrite.Processed = true;

                // index range is not supported.
                if (!String.IsNullOrEmpty(nodeToWrite.IndexRange))
                {
                    errors[ii] = StatusCodes.BadWriteNotSupported;
                    continue;
                }

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

                // write the attribute value.
                //errors[ii] = source.WriteAttribute(
                //    systemContext,
                //    nodeToWrite.AttributeId,
                //    nodeToWrite.ParsedIndexRange,
                //    nodeToWrite.Value);
                errors[ii] = ServiceResult.Good;
                if (nodeToWrite.Value != null && nodeToWrite.Value.Value != null)
                    source.ACValue = nodeToWrite.Value.Value;

                // updates to source finished - report changes to monitored items.
                source.ClearChangeMasks(systemContext, false);
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
                    continue;

                WriteValue nodeToWrite = nodesToWrite[operation.Index];

                NodeStateInfo sourceInfo = GetManagerHandleInfo(nodeToWrite.NodeId, true, systemContext) as NodeStateInfo;
                if (sourceInfo == null)
                    continue;
                UAStateACProperty source = sourceInfo.Node as UAStateACProperty;
                if (source == null)
                    continue;

                // write the attribute value.
                //errors[operation.Index] = operation.Source.WriteAttribute(
                //    systemContext,
                //    nodeToWrite.AttributeId,
                //    nodeToWrite.ParsedIndexRange,
                //    nodeToWrite.Value);
                errors[ii] = ServiceResult.Good;
                source.Value = nodeToWrite.Value;

                // updates to source finished - report changes to monitored items.
                operation.Source.ClearChangeMasks(systemContext, false);
            }
        }



        public void Call(OperationContext context, IList<CallMethodRequest> methodsToCall, IList<CallMethodResult> results, IList<ServiceResult> errors)
        {
            ServerSystemContext systemContext = _SystemContext.Copy(context);
            IDictionary<NodeId, NodeState> operationCache = new NodeIdDictionary<NodeState>();
            List<CallOperationState> nodesToValidate = new List<CallOperationState>();

            for (int ii = 0; ii < methodsToCall.Count; ii++)
            {
                CallMethodRequest methodToCall = methodsToCall[ii];

                // skip items that have already been processed.
                if (methodToCall.Processed)
                {
                    continue;
                }

                // check for valid handle.
                NodeStateInfo sourceInfo = GetManagerHandleInfo(methodToCall.MethodId, true, systemContext) as NodeStateInfo;
                if (sourceInfo == null)
                    continue;
                UAStateACMethod method = sourceInfo.Node as UAStateACMethod;
                // owned by this node manager.
                methodToCall.Processed = true;
                if (method == null)
                {
                    errors[ii] = StatusCodes.BadMethodInvalid;
                    continue;
                }

                CallMethodResult result = results[ii] = new CallMethodResult();

                // check if the node is ready for reading.
                if (method.ValidationRequired)
                {
                    errors[ii] = StatusCodes.BadNodeIdUnknown;

                    // must validate node in a seperate operation.
                    CallOperationState operation = new CallOperationState();

                    operation.Method = method;
                    operation.Index = ii;

                    nodesToValidate.Add(operation);

                    continue;
                }

                // call the method.
                errors[ii] = Call(
                    systemContext,
                    methodToCall,
                    method,
                    result);
            }

            // check for nothing to do.
            if (nodesToValidate.Count == 0)
            {
                return;
            }

            // validates the nodes (reads values from the underlying data source if required).
            for (int ii = 0; ii < nodesToValidate.Count; ii++)
            {
                CallOperationState operation = nodesToValidate[ii];

                // validate the object.
                if (!ValidateNode(systemContext, operation.Method))
                {
                    continue;
                }

                // call the method.
                CallMethodResult result = results[operation.Index];

                errors[operation.Index] = Call(
                    systemContext,
                    methodsToCall[operation.Index],
                    operation.Method,
                    result);
            }
        }

        public ServiceResult ConditionRefresh(OperationContext context, IList<IEventMonitoredItem> monitoredItems)
        {
            throw new NotImplementedException();
        }


        public void CreateMonitoredItems(OperationContext context, uint subscriptionId, double publishingInterval, TimestampsToReturn timestampsToReturn, IList<MonitoredItemCreateRequest> itemsToCreate, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterErrors, IList<IMonitoredItem> monitoredItems, ref long globalIdCounter)
        {
            ServerSystemContext systemContext = _SystemContext.Copy(context);
            IDictionary<NodeId, NodeState> operationCache = new NodeIdDictionary<NodeState>();
            List<ReadWriteOperationState> nodesToValidate = new List<ReadWriteOperationState>();
            List<IMonitoredItem> createdItems = new List<IMonitoredItem>();

            using (ACMonitor.Lock(_30209_LockValue))
            {
                for (int ii = 0; ii < itemsToCreate.Count; ii++)
                {
                    MonitoredItemCreateRequest itemToCreate = itemsToCreate[ii];

                    // skip items that have already been processed.
                    if (itemToCreate.Processed)
                    {
                        continue;
                    }

                    ReadValueId itemToMonitor = itemToCreate.ItemToMonitor;

                    NodeStateInfo sourceInfo = GetManagerHandleInfo(itemToMonitor.NodeId, true, systemContext) as NodeStateInfo;
                    if (sourceInfo == null)
                        continue;
                    UAStateACProperty source = sourceInfo.Node as UAStateACProperty;
                    if (source == null)
                        continue;

                    // owned by this node manager.
                    itemToCreate.Processed = true;

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

                    MonitoringFilterResult filterError = null;
                    IMonitoredItem monitoredItem = null;

                    errors[ii] = CreateMonitoredItem(
                        systemContext,
                        source,
                        subscriptionId,
                        publishingInterval,
                        context.DiagnosticsMask,
                        timestampsToReturn,
                        itemToCreate,
                        ref globalIdCounter,
                        out filterError,
                        out monitoredItem);

                    // save any filter error details.
                    filterErrors[ii] = filterError;

                    if (ServiceResult.IsBad(errors[ii]))
                    {
                        continue;
                    }

                    // save the monitored item.
                    monitoredItems[ii] = monitoredItem;
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

                    // validate the object.
                    if (!ValidateNode(systemContext, operation.Source))
                    {
                        continue;
                    }

                    MonitoredItemCreateRequest itemToCreate = itemsToCreate[operation.Index];

                    MonitoringFilterResult filterError = null;
                    IMonitoredItem monitoredItem = null;

                    errors[operation.Index] = CreateMonitoredItem(
                        systemContext,
                        operation.Source,
                        subscriptionId,
                        publishingInterval,
                        context.DiagnosticsMask,
                        timestampsToReturn,
                        itemToCreate,
                        ref globalIdCounter,
                        out filterError,
                        out monitoredItem);

                    // save any filter error details.
                    filterErrors[operation.Index] = filterError;

                    if (ServiceResult.IsBad(errors[operation.Index]))
                    {
                        continue;
                    }

                    // save the monitored item.
                    monitoredItems[operation.Index] = monitoredItem;
                }

            }

            // do any post processing.
            //OnCreateMonitoredItemsComplete(systemContext, createdItems);
        }


        public void ModifyMonitoredItems(OperationContext context, TimestampsToReturn timestampsToReturn, IList<IMonitoredItem> monitoredItems, IList<MonitoredItemModifyRequest> itemsToModify, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterErrors)
        {
            ServerSystemContext systemContext = _SystemContext.Copy(context);

            using (ACMonitor.Lock(_30209_LockValue))
            {
                for (int ii = 0; ii < monitoredItems.Count; ii++)
                {
                    MonitoredItemModifyRequest itemToModify = itemsToModify[ii];

                    // skip items that have already been processed.
                    if (itemToModify.Processed)
                    {
                        continue;
                    }

                    // modify the monitored item.
                    MonitoringFilterResult filterError = null;

                    errors[ii] = ModifyMonitoredItem(
                        systemContext,
                        context.DiagnosticsMask,
                        timestampsToReturn,
                        monitoredItems[ii],
                        itemToModify,
                        out filterError);

                    // save any filter error details.
                    filterErrors[ii] = filterError;
                }
            }
        }

        public void DeleteMonitoredItems(OperationContext context, IList<IMonitoredItem> monitoredItems, IList<bool> processedItems, IList<ServiceResult> errors)
        {
            ServerSystemContext systemContext = _SystemContext.Copy(context);

            using (ACMonitor.Lock(_30209_LockValue))
            {
                for (int ii = 0; ii < monitoredItems.Count; ii++)
                {
                    // skip items that have already been processed.
                    if (processedItems[ii])
                    {
                        continue;
                    }

                    // delete the monitored item.
                    bool processed = false;

                    errors[ii] = DeleteMonitoredItem(
                        systemContext,
                        monitoredItems[ii],
                        out processed);

                    // indicate whether it was processed or not.
                    processedItems[ii] = processed;
                }
            }
        }

        public void DeleteAddressSpace()
        {
            //throw new NotImplementedException();
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
        #endregion

        #region Helper-Methods

        #region NodeID-Building
        private NodeStateInfo GetManagerHandleInfo(NodeId nodeId, bool createIfNotExist, ServerSystemContext systemContext)
        {
            // 1. Bei Connect von Client
            // Rückgabe eines NodeState-Objektes
            NodeStateInfo node;
            if (_MapNodeID2Member.TryGetValue(nodeId, out node))
                return node;
            if (createIfNotExist)
            {
                using (ACMonitor.Lock(_30209_LockValue))
                {
                    try
                    {
                        if (nodeId.Identifier == null)
                            return null;
                        string acURL = nodeId.Identifier as string;
                        if (String.IsNullOrEmpty(acURL))
                            return null;
                        if (!acURL.ContainsACUrlDelimiters())
                            return null;
                        string methodName = null;
                        string propertyName = null;
                        ACComponent component = null;
                        IACPropertyBase property = null;
                        //int indexOfMethod = acURL.IndexOf(ACUrlHelper.AttachedMethodIDConcatenator);
                        int indexOfMethod = acURL.IndexOf(ACUrlHelper.Delimiter_InvokeMethod);
                        if (indexOfMethod > 0)
                        {
                            methodName = acURL.Substring(indexOfMethod + 1);
                            acURL = acURL.Substring(0, indexOfMethod);
                            component = this.ParentACService.ACUrlCommand(acURL) as ACComponent;
                            if (component == null)
                                return null;
                        }
                        component = this.ParentACService.ACUrlCommand(acURL) as ACComponent;
                        if (component == null)
                        {
                            int indexOfLastSegment = acURL.LastIndexOf(ACUrlHelper.Delimiter_DirSeperator);
                            if (indexOfLastSegment > 0)
                            {
                                propertyName = acURL.Substring(indexOfLastSegment + 1);
                                acURL = acURL.Substring(0, indexOfLastSegment);
                                component = this.ParentACService.ACUrlCommand(acURL) as ACComponent;
                                if (component != null)
                                    property = component.GetProperty(propertyName);
                            }
                            if (property == null)
                                return null;
                        }

                        NodeStateInfo stateInfoOfComp = null;
                        if (!_MapMemberToNodeState.TryGetValue(component, out stateInfoOfComp))
                            stateInfoOfComp = GetOrCreateNewNodeState(component, null, null);
                        if (stateInfoOfComp == null)
                            return null;
                        UAStateACComponent stateACComp = stateInfoOfComp.NodeComp;
                        if (stateACComp == null)
                            return null;

                        //ClassRightManager rightManager = GetRightManager(component, systemContext);
                        //if (rightManager == null)
                        //    return null;

                        //Global.ControlModes controlModes = Global.ControlModes.Enabled;
                        string normalizedMethodName = null;
                        if (property != null)
                        {
                            //controlModes = rightManager.GetControlMode(property.ACType);
                            //if (controlModes < Global.ControlModes.Enabled)
                            //    return null;
                            node = GetOrCreateNewNodeState(property, stateACComp, null);
                            return node;
                        }
                        else if (!String.IsNullOrEmpty(methodName))
                        {
                            ACClassMethod acClassMethod = component.GetACClassMethod(methodName, out normalizedMethodName);
                            if (acClassMethod == null)
                                return null;
                            //controlModes = rightManager.GetControlMode(acClassMethod.ACType);
                            //if (controlModes < Global.ControlModes.Enabled)
                            //    return null;
                            node = GetOrCreateNewNodeState(acClassMethod, stateACComp, null);
                            return node;
                        }
                    }
                    catch (Exception e)
                    {
                        this.ParentACService.Messages.LogException(this.ParentACService.GetACUrl(), "OPCUANodeManager.GetManagerHandleInfo(10)", e);
                    }
                }
            }
            return null;
        }

        private byte Convert2UARights(Global.ControlModes controlMode)
        {
            if (controlMode == Global.ControlModes.Disabled)
                return AccessLevels.CurrentRead;
            //else if (controlMode >= Global.ControlModes.Enabled)
            return AccessLevels.CurrentReadOrWrite;
        }

        private RolePermissionTypeCollection GetPermissions(Global.ControlModes controlMode)
        {
            if (controlMode == Global.ControlModes.Disabled)
            {
                return new RolePermissionTypeCollection()
                    {
                        new RolePermissionType()
                        {
                            RoleId = ObjectIds.WellKnownRole_AuthenticatedUser,
                            Permissions = (uint)(PermissionType.Browse |PermissionType.Read |PermissionType.ReadRolePermissions | PermissionType.Write)
                        },
                    };
            }
            else
            {
                return new RolePermissionTypeCollection()
                    {
                        new RolePermissionType()
                        {
                            RoleId = ObjectIds.WellKnownRole_AuthenticatedUser,
                            Permissions = (uint)(PermissionType.Browse |PermissionType.Read |PermissionType.ReadRolePermissions)
                        },
                    };
            }
        }

        private NodeStateInfo GetOrCreateNewNodeState(IACPropertyBase property, UAStateACComponent parent, uint? sortOrder)
        {
            NodeStateInfo checkIfExists = null;
            if (_MapMemberToNodeState.TryGetValue(property, out checkIfExists))
            {
                return checkIfExists;
            }

            UAStateACProperty newStateObj = new UAStateACProperty(property, parent, _NamespaceIndexes[1], sortOrder);
            checkIfExists = new NodeStateInfo() { Node = newStateObj };
            checkIfExists.Reference = new NodeStateReference(ReferenceTypeIds.HasComponent, false, newStateObj);
            _MapMemberToNodeState.TryAdd(property, checkIfExists);
            _MapNodeID2Member.TryAdd(newStateObj.NodeId, checkIfExists);
            return checkIfExists;
        }

        private NodeStateInfo GetOrCreateNewNodeState(ACComponent component, UAStateACComponent parent, uint? sortOrder)
        {
            if (component == null)
                return null;
            NodeStateInfo checkIfExists = null;
            if (_MapMemberToNodeState.TryGetValue(component, out checkIfExists))
            {
                return checkIfExists;
            }

            if (parent == null)
            {
                ACComponent parentACComponent = component.ParentACComponent as ACComponent;
                if (parentACComponent == null)
                    return null;
                if (!_MapMemberToNodeState.TryGetValue(parentACComponent, out checkIfExists))
                {
                    checkIfExists = GetOrCreateNewNodeState(parentACComponent, null, null);
                }
                if (checkIfExists == null)
                    return null;
                parent = checkIfExists.NodeComp;
            }

            UAStateACComponent newStateObj = new UAStateACComponent(component, parent, _NamespaceIndexes[1], sortOrder);
            checkIfExists = new NodeStateInfo() { Node = newStateObj };
            checkIfExists.Reference = new NodeStateReference(ReferenceTypeIds.Organizes, false, newStateObj);
            _MapMemberToNodeState.TryAdd(component, checkIfExists);
            _MapNodeID2Member.TryAdd(newStateObj.NodeId, checkIfExists);
            return checkIfExists;
        }

        private NodeStateInfo GetOrCreateNewNodeState(ACClassMethod method, UAStateACComponent parent, uint? sortOrder)
        {
            return parent.GetOrCreateNewNodeState(method, parent, this, sortOrder);
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
                NodeStateInfo node;
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
        #endregion


        #region Validation
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


        #region Method Invocation

        private struct CallOperationState
        {
            public UAStateACMethod Method;
            public int Index;
        }

        protected ServiceResult IsStartExecutable(ISystemContext context, NodeState node, ref bool value)
        {
            return ServiceResult.Good;
        }

        protected virtual ServiceResult Call(ISystemContext context, CallMethodRequest methodToCall, /*NodeState source, */ MethodState method, CallMethodResult result)
        {
            ServerSystemContext systemContext = context as ServerSystemContext;
            List<ServiceResult> argumentErrors = new List<ServiceResult>();
            VariantCollection outputArguments = new VariantCollection();

            ServiceResult error = method.Call(
                context,
                methodToCall.ObjectId,
                methodToCall.InputArguments,
                argumentErrors,
                outputArguments);

            if (ServiceResult.IsBad(error))
            {
                return error;
            }

            // check for argument errors.
            bool argumentsValid = true;

            for (int jj = 0; jj < argumentErrors.Count; jj++)
            {
                ServiceResult argumentError = argumentErrors[jj];

                if (argumentError != null)
                {
                    result.InputArgumentResults.Add(argumentError.StatusCode);

                    if (ServiceResult.IsBad(argumentError))
                    {
                        argumentsValid = false;
                    }
                }
                else
                {
                    result.InputArgumentResults.Add(StatusCodes.Good);
                }

                // only fill in diagnostic info if it is requested.
                if ((systemContext.OperationContext.DiagnosticsMask & DiagnosticsMasks.OperationAll) != 0)
                {
                    if (ServiceResult.IsBad(argumentError))
                    {
                        argumentsValid = false;
                        result.InputArgumentDiagnosticInfos.Add(new DiagnosticInfo(argumentError, systemContext.OperationContext.DiagnosticsMask, false, systemContext.OperationContext.StringTable));
                    }
                    else
                    {
                        result.InputArgumentDiagnosticInfos.Add(null);
                    }
                }
            }

            // check for validation errors.
            if (!argumentsValid)
            {
                result.StatusCode = StatusCodes.BadInvalidArgument;
                return result.StatusCode;
            }

            // do not return diagnostics if there are no errors.
            result.InputArgumentDiagnosticInfos.Clear();

            // return output arguments.
            result.OutputArguments = outputArguments;

            return ServiceResult.Good;
        }
        #endregion


        #region Monitored Items
        /// <summary>
        /// Stores the state of a call method operation.
        /// </summary>
        private struct ReadWriteOperationState
        {
            public UAStateACProperty Source;
            public int Index;
        }

        protected ServiceResult CreateMonitoredItem(
            ServerSystemContext context,
            UAStateACProperty source,
            uint subscriptionId,
            double publishingInterval,
            DiagnosticsMasks diagnosticsMasks,
            TimestampsToReturn timestampsToReturn,
            MonitoredItemCreateRequest itemToCreate,
            ref long globalIdCounter,
            out MonitoringFilterResult filterError,
            out IMonitoredItem monitoredItem)
        {
            filterError = null;
            monitoredItem = null;

            // validate parameters.
            MonitoringParameters parameters = itemToCreate.RequestedParameters;

            // no filters supported at this time.
            MonitoringFilter filter = (MonitoringFilter)ExtensionObject.ToEncodeable(parameters.Filter);

            if (filter != null)
            {
                return StatusCodes.BadFilterNotAllowed;
            }

            // index range not supported.
            if (itemToCreate.ItemToMonitor.ParsedIndexRange != NumericRange.Empty)
            {
                return StatusCodes.BadIndexRangeInvalid;
            }

            // data encoding not supported.
            if (!QualifiedName.IsNull(itemToCreate.ItemToMonitor.DataEncoding))
            {
                return StatusCodes.BadDataEncodingInvalid;
            }

            // read initial value.
            if (source.WrappedValue != source.ACValueAsVariant)
                source.WrappedValue = source.ACValueAsVariant;
            DataValue initialValue = new DataValue(source.ACValueAsVariant);
            initialValue.ServerTimestamp = DateTime.UtcNow;
            initialValue.SourceTimestamp = DateTime.MinValue;
            initialValue.StatusCode = StatusCodes.Good;

            ServiceResult error = source.ReadAttribute(
                context,
                itemToCreate.ItemToMonitor.AttributeId,
                itemToCreate.ItemToMonitor.ParsedIndexRange,
                itemToCreate.ItemToMonitor.DataEncoding,
                initialValue);

            initialValue.WrappedValue = source.ACValueAsVariant;

            if (ServiceResult.IsBad(error))
            {
                return error;
            }

            // create a globally unique identifier.
            uint monitoredItemId = Utils.IncrementIdentifier(ref globalIdCounter);

            // determine the sampling interval.
            double samplingInterval = itemToCreate.RequestedParameters.SamplingInterval;

            if (samplingInterval < 0)
            {
                samplingInterval = publishingInterval;
            }

            // create the item.
            OPCUAServerMonitoredItem datachangeItem = new OPCUAServerMonitoredItem(
                    _InternalServer,
                    this,
                    source,
                    subscriptionId,
                    monitoredItemId,
                    itemToCreate.ItemToMonitor,
                    diagnosticsMasks,
                    timestampsToReturn,
                    itemToCreate.MonitoringMode,
                    itemToCreate.RequestedParameters.ClientHandle,
                    null,
                    null,
                    null,
                    samplingInterval,
                    0,
                    false,
                    0);

            // report the initial value.
            datachangeItem.QueueValue(initialValue, null);

            // update monitored item list.
            monitoredItem = datachangeItem;

            return ServiceResult.Good;
        }


        protected ServiceResult ModifyMonitoredItem(
            ISystemContext context,
            DiagnosticsMasks diagnosticsMasks,
            TimestampsToReturn timestampsToReturn,
            IMonitoredItem monitoredItem,
            MonitoredItemModifyRequest itemToModify,
            out MonitoringFilterResult filterError)
        {
            filterError = null;


            // owned by this node manager.
            itemToModify.Processed = true;

            // check for valid monitored item.
            OPCUAServerMonitoredItem datachangeItem = monitoredItem as OPCUAServerMonitoredItem;
            if (datachangeItem == null)
            {
                return StatusCodes.BadMonitoredItemIdInvalid;
            }
            // check for valid handle.
            UAStateACProperty monitoredNode = monitoredItem.ManagerHandle as UAStateACProperty;
            if (monitoredNode == null)
            {
                return StatusCodes.BadMonitoredItemIdInvalid;
            }

            // validate parameters.
            MonitoringParameters parameters = itemToModify.RequestedParameters;

            // no filters supported at this time.
            MonitoringFilter filter = (MonitoringFilter)ExtensionObject.ToEncodeable(parameters.Filter);

            if (filter != null)
            {
                return StatusCodes.BadFilterNotAllowed;
            }

            // modify the monitored item parameters.
            ServiceResult error = datachangeItem.Modify(
                diagnosticsMasks,
                timestampsToReturn,
                itemToModify.RequestedParameters.ClientHandle,
                itemToModify.RequestedParameters.SamplingInterval);

            return ServiceResult.Good;
        }


        protected ServiceResult DeleteMonitoredItem(
            ISystemContext context,
            IMonitoredItem monitoredItem,
            out bool processed)
        {
            processed = false;

            // owned by this node manager.
            processed = true;

            // get the monitored item.
            OPCUAServerMonitoredItem datachangeItem = monitoredItem as OPCUAServerMonitoredItem;
            if (datachangeItem == null)
            {
                return StatusCodes.BadMonitoredItemIdInvalid;
            }
            // check for valid handle. 
            UAStateACProperty monitoredNode = monitoredItem.ManagerHandle as UAStateACProperty;
            if (monitoredNode == null)
            {
                return StatusCodes.BadMonitoredItemIdInvalid;
            }

            datachangeItem.UnSubscribe();

            return ServiceResult.Good;
        }


        /// <summary>
        /// Validates a data change filter provided by the client.
        /// </summary>
        /// <param name="context">The system context.</param>
        /// <param name="source">The node being monitored.</param>
        /// <param name="attributeId">The attribute being monitored.</param>
        /// <param name="requestedFilter">The requested monitoring filter.</param>
        /// <param name="filter">The validated data change filter.</param>
        /// <param name="range">The EU range associated with the value if required by the filter.</param>
        /// <returns>Any error condition. Good if no errors occurred.</returns>
        protected ServiceResult ValidateDataChangeFilter(
            ISystemContext context,
            NodeState source,
            uint attributeId,
            ExtensionObject requestedFilter,
            out DataChangeFilter filter,
            out Opc.Ua.Range range)
        {
            filter = null;
            range = null;

            // check for valid filter type.
            filter = requestedFilter.Body as DataChangeFilter;

            if (filter == null)
            {
                return StatusCodes.BadMonitoredItemFilterUnsupported;
            }

            // only supported for value attributes.
            if (attributeId != Attributes.Value)
            {
                return StatusCodes.BadMonitoredItemFilterUnsupported;
            }

            // only supported for variables.
            BaseVariableState variable = source as BaseVariableState;

            if (variable == null)
            {
                return StatusCodes.BadMonitoredItemFilterUnsupported;
            }

            // check the datatype.
            if (filter.DeadbandType != (uint)DeadbandType.None)
            {
                BuiltInType builtInType = TypeInfo.GetBuiltInType(variable.DataType, this._InternalServer.TypeTree);

                if (!TypeInfo.IsNumericType(builtInType))
                {
                    return StatusCodes.BadMonitoredItemFilterUnsupported;
                }
            }

            // validate filter.
            ServiceResult error = filter.Validate();

            if (ServiceResult.IsBad(error))
            {
                return error;
            }

            if (filter.DeadbandType == (uint)DeadbandType.Percent)
            {
                BaseVariableState euRange = variable.FindChild(context, BrowseNames.EURange) as BaseVariableState;

                if (euRange == null)
                {
                    return StatusCodes.BadMonitoredItemFilterUnsupported;
                }

                range = euRange.Value as Opc.Ua.Range;

                if (range == null)
                {
                    return StatusCodes.BadMonitoredItemFilterUnsupported;
                }
            }

            // all good.
            return ServiceResult.Good;
        }
        #endregion

        private ClassRightManager GetRightManager(ACComponent component, ServerSystemContext systemContext)
        {
            ClassRightManager rightManager = null;
            if (systemContext.UserIdentity != null)
            {
                VBUser user = null;
                OPCUAUserIdentity vbIdentity = systemContext.UserIdentity as OPCUAUserIdentity;
                if (vbIdentity != null)
                    user = vbIdentity.VBUser;
                else
                    user = this.ParentUAServer.GetLoggedOnUser(systemContext.UserIdentity.DisplayName);
                if (user != null)
                    rightManager = component.GetRightsForUser(user);
            }
            return rightManager;
        }

        public void TransferMonitoredItems(OperationContext context, bool sendInitialValues, IList<IMonitoredItem> monitoredItems, IList<bool> processedItems, IList<ServiceResult> errors)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
