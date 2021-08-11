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

    public class UAStateACComponent : FolderState, IUAStateIACMember
    {
        public UAStateACComponent(ACComponent instance, NodeState parentNode) : base(parentNode)
        {
            ACComponent = instance;
            //this.NodeId = new NodeId(instance.GetACUrl());
            this.NodeId = new NodeId(instance.ComponentClass.ACClassID);
            this.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            BrowseName = new QualifiedName(instance.ACIdentifier); // , parent.TypeDefinitionId.NamespaceIndex
            DisplayName = instance.ACIdentifier;
            Description = String.Format("{0};{1}", instance.GetACUrl(), instance.ACCaption);
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.Organizes;
            TypeDefinitionId = ObjectIds.ObjectsFolder;
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

        public UAStateACProperty(IACPropertyBase instance, NodeState parentNode, byte userAccessLevel = AccessLevels.CurrentReadOrWrite) : base(parentNode)
        {
            if (   !instance.IsValueType 
                && !instance.PropertyType.IsEnum 
                && BitAccessType.IsAssignableFrom(instance.PropertyType))
            {
                throw new ArgumentException("Property is not a valid ValueType");
            }
            ACProperty = instance;
            NodeId = new NodeId(instance.ACType.ACTypeID);
            BrowseName = new QualifiedName(instance.ACIdentifier); // , parent.TypeDefinitionId.NamespaceIndex
            DisplayName = instance.ACIdentifier;
            Description = String.Format("{0};{1}", instance.GetACUrl(), instance.ACCaption);
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent;
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
}
