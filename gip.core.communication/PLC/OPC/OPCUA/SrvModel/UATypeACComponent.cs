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
    public class UATypeACComponent : FolderTypeState
    {
        public UATypeACComponent() : base()
        {
        }

        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            this.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            //this.NodeId = new NodeId()
        }
    }

    public interface IUAStateIACMember
    {
        IACMember ACMember { get; }
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


    public class UAStateACComponent : FolderState, IUAStateIACMember
    {
        public UAStateACComponent(ACComponent instance, NodeState parentNode, ushort namespaceIndex) : base(parentNode)
        {
            ACComponent = instance;
            //this.NodeId = new NodeId(instance.GetACUrl());
            this.NodeId = new NodeId(instance.ComponentClass.ACClassID, namespaceIndex);
            this.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            BrowseName = new QualifiedName(instance.ACIdentifier, namespaceIndex); // , parent.TypeDefinitionId.NamespaceIndex
            DisplayName = instance.ACIdentifier;
            Description = String.Format("{0};{1}", instance.GetACUrl(), instance.ACCaption);
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent; // HasComponent = 47;
            TypeDefinitionId = ObjectTypeIds.FolderType; // FolderType = 61;
            ModellingRuleId = null;
            NumericId = System.Convert.ToUInt32(instance.ParentACComponent == null ? 0 : instance.ParentACComponent.ACMemberList.IndexOf(instance));
        }

        public ACComponent ACComponent
        {
            get;
            private set;
        }

        public IACMember ACMember
        {
            get
            {
                return ACComponent;
            }
        }
    }

    public class UAStateACProperty : BaseVariableState, IUAStateIACMember
    {
        public static Type _BitAccessType = typeof(IBitAccess);
        public static Type BitAccessType
        {
            get
            {
                return _BitAccessType;
            }
        }

        public UAStateACProperty(IACPropertyBase instance, NodeState parentNode, ushort namespaceIndex, byte userAccessLevel = AccessLevels.CurrentReadOrWrite) : base(parentNode)
        {
            if (   !instance.IsValueType 
                && !instance.PropertyType.IsEnum 
                && BitAccessType.IsAssignableFrom(instance.PropertyType))
            {
                throw new ArgumentException("Property is not a valid ValueType");
            }
            ACProperty = instance;
            NodeId = new NodeId(instance.ACType.ACTypeID, namespaceIndex);
            BrowseName = new QualifiedName(instance.ACIdentifier, namespaceIndex); // , parent.TypeDefinitionId.NamespaceIndex
            DisplayName = instance.ACIdentifier;
            Description = String.Format("{0};{1}", instance.GetACUrl(), instance.ACCaption);
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent; // HasComponent = 47;
            TypeDefinitionId = VariableTypeIds.BaseVariableType;
            ModellingRuleId = null;
            NumericId = System.Convert.ToUInt32(instance.ParentACComponent.ACMemberList.IndexOf(instance));
            Type type = instance.PropertyType;
            if (!instance.IsValueType && !instance.PropertyType.IsEnum)
            {
                IBitAccess bitAccess = instance.Value as IBitAccess;
                if (bitAccess == null)
                    throw new ArgumentException("Property is not a valid ValueType");
                type = bitAccess.UnderlyingType;
            }
            DataType = TypeInfo.GetDataTypeId(instance.PropertyType);
            ValueRank = ValueRanks.Scalar;
            ArrayDimensions = null;
            if (instance.PropertyInfo.IsInput && instance.PropertyInfo.IsOutput)
                AccessLevel = AccessLevels.CurrentReadOrWrite;
            else if (instance.PropertyInfo.IsInput)
                AccessLevel = AccessLevels.CurrentWrite;
            else
                AccessLevel = AccessLevels.CurrentRead;

            UserAccessLevel = userAccessLevel;
            //MinimumSamplingInterval = parent.MaximumScanRate;
            Historizing = false;
        }

        public IACPropertyBase ACProperty
        {
            get;
            private set;
        }

        public IACMember ACMember
        {
            get
            {
                return ACProperty;
            }
        }

    }


    public class UAStateACMethod : MethodState, IUAStateIACMember
    {
    }
}
