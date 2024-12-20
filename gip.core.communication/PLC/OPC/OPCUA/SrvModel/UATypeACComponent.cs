﻿using System;
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
    public class UAStateACComponent : FolderState, IUAStateIACMember
    {
        public UAStateACComponent(ACComponent instance, NodeState parentNode, ushort namespaceIndex, uint? sortOrder) : base(parentNode)
        {
            ACComponent = instance;
            this.NodeId = new NodeId(instance.GetACUrl(), namespaceIndex);
            //this.NodeId = new NodeId(instance.ComponentClass.ACClassID, namespaceIndex);
            this.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            BrowseName = new QualifiedName(instance.ACIdentifier, namespaceIndex); // , parent.TypeDefinitionId.NamespaceIndex
            DisplayName = instance.ACIdentifier;
            Description = instance.ACCaption;
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent; // HasComponent = 47;
            TypeDefinitionId = ObjectTypeIds.FolderType; // FolderType = 61;
            ModellingRuleId = null;
            NumericId = sortOrder.HasValue ? sortOrder.Value : System.Convert.ToUInt32(instance.ParentACComponent == null ? 0 : instance.ParentACComponent.ACMemberList.IndexOf(instance));
        }

        public ACComponent ACComponent
        {
            get;
            private set;
        }

        public IACObject ACMember
        {
            get
            {
                return ACComponent;
            }
        }

        private ConcurrentDictionary<ACClassMethod, OPCUANodeManager.NodeStateInfo> _MapMemberToNodeState = new ConcurrentDictionary<ACClassMethod, OPCUANodeManager.NodeStateInfo>();

        public OPCUANodeManager.NodeStateInfo GetOrCreateNewNodeState(ACClassMethod method, UAStateACComponent parent, OPCUANodeManager nodeManager, uint? sortOrder)
        {
            OPCUANodeManager.NodeStateInfo checkIfExists = null;
            if (_MapMemberToNodeState.TryGetValue(method, out checkIfExists))
            {
                return checkIfExists;
            }

            UAStateACMethod newStateObj = new UAStateACMethod(parent.ACComponent, method, parent, nodeManager._NamespaceIndexes[1], sortOrder);
            newStateObj.OnCallMethod = new GenericMethodCalledEventHandler(OnVoidCall);
            checkIfExists = new OPCUANodeManager.NodeStateInfo() { Node = newStateObj };
            checkIfExists.Reference = new NodeStateReference(ReferenceTypeIds.HasComponent, false, newStateObj);
            _MapMemberToNodeState.TryAdd(method, checkIfExists);
            nodeManager._MapNodeID2Member.TryAdd(newStateObj.NodeId, checkIfExists);
            return checkIfExists;
        }

        private ServiceResult OnVoidCall(ISystemContext context,
                                    MethodState method,
                                    IList<object> inputArguments,
                                    IList<object> outputArguments)
        {
            UAStateACMethod stateMethod = method as UAStateACMethod;
            if (stateMethod != null)
            {
                stateMethod.ACComponent.ExecuteMethod(stateMethod.ACMethod.ACIdentifier);
            }
            return ServiceResult.Good;
        }
    }

    //public const uint HasComponent = 47;
    //public static readonly NodeId HasComponent = new NodeId(Opc.Ua.ReferenceTypes.HasComponent);

    //public const uint FolderType = 61;
    //public static readonly NodeId FolderType = new NodeId(Opc.Ua.ObjectTypes.FolderType);

    //public const uint BaseVariableType = 62;
    //public static readonly NodeId BaseVariableType = new NodeId(Opc.Ua.VariableTypes.BaseVariableType);

    //public const uint RootFolder = 84;
    //public static readonly NodeId RootFolder = new NodeId(Opc.Ua.Objects.RootFolder);

    //public const uint ObjectsFolder = 85;
    //public static readonly NodeId ObjectsFolder = new NodeId(Opc.Ua.Objects.ObjectsFolder);
}
