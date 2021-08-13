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

        public UAStateACProperty(IACPropertyBase instance, NodeState parentNode, ushort namespaceIndex) : base(parentNode)
        {
            if (   !instance.IsValueType 
                && !instance.PropertyType.IsEnum 
                && !BitAccessType.IsAssignableFrom(instance.PropertyType))
            {
                throw new ArgumentException("Property is not a valid ValueType");
            }
            ACProperty = instance;
            NodeId = new NodeId(instance.GetACUrl(), namespaceIndex);
            //NodeId = new NodeId(instance.ACType.ACTypeID, namespaceIndex);
            BrowseName = new QualifiedName(instance.ACIdentifier, namespaceIndex);
            DisplayName = instance.ACIdentifier;
            Description = String.Format("{0};{1}", instance.GetACUrl(), instance.ACCaption);
            WriteMask = AttributeWriteMask.None;
            UserWriteMask = AttributeWriteMask.None;
            ReferenceTypeId = Opc.Ua.ReferenceTypeIds.HasComponent; // HasComponent = 47;
            TypeDefinitionId = VariableTypeIds.BaseVariableType;
            ModellingRuleId = null;
            NumericId = System.Convert.ToUInt32(instance.ParentACComponent.ACMemberList.IndexOf(instance));
            Type type = instance.PropertyType;
            _InnerPropertyType = InnerPropetyTypeEnum.ValueType;
            if (instance.PropertyType.IsEnum)
                _InnerPropertyType = InnerPropetyTypeEnum.Enum;
            else if (!instance.IsValueType)
            {
                IBitAccess bitAccess = instance.Value as IBitAccess;
                if (bitAccess == null)
                    throw new ArgumentException("Property is not a valid ValueType");
                type = bitAccess.UnderlyingType;
                _InnerPropertyType = InnerPropetyTypeEnum.BitAccess;
            }
            _TypeInfo = TypeInfo.Construct(type);
            DataType = TypeInfo.GetDataTypeId(_TypeInfo);
            ValueRank = ValueRanks.Scalar;
            ArrayDimensions = null;
            if (instance.PropertyInfo.IsInput && instance.PropertyInfo.IsOutput)
                AccessLevel = AccessLevels.CurrentReadOrWrite;
            else if (instance.PropertyInfo.IsInput)
                AccessLevel = AccessLevels.CurrentWrite;
            else
                AccessLevel = AccessLevels.CurrentRead;

            UserAccessLevel = AccessLevel;
            //MinimumSamplingInterval = parent.MaximumScanRate;
            Historizing = false;
            WrappedValue = ACValueAsVariant;
        }

        public enum InnerPropetyTypeEnum
        {
            ValueType,
            Enum,
            BitAccess
        }

        private InnerPropetyTypeEnum _InnerPropertyType;

        public IACPropertyBase ACProperty
        {
            get;
            private set;
        }

        public ACComponent ACComponent
        {
            get
            {
                return ACProperty.ParentACComponent as ACComponent;
            }
        }

        public IACObject ACMember
        {
            get
            {
                return ACProperty;
            }
        }

        private TypeInfo _TypeInfo;
        public TypeInfo TypeInfo
        {
            get
            {
                return _TypeInfo;
            }
        }

        public Variant ACValueAsVariant
        {
            get
            {
                return new Variant(ACValue, TypeInfo);
            }
        }

        public object ACValue
        {
            get
            {
                if (_InnerPropertyType == InnerPropetyTypeEnum.ValueType)
                    return ACProperty.Value;
                else if (_InnerPropertyType == InnerPropetyTypeEnum.Enum)
                    return ACProperty.Value;
                else
                    return (ACProperty.Value as IBitAccess).Value;
            }
            set
            {
                if (_InnerPropertyType == InnerPropetyTypeEnum.ValueType)
                    ACProperty.Value = value;
                else if (_InnerPropertyType == InnerPropetyTypeEnum.Enum)
                    ACProperty.Value = value;
                else
                    (ACProperty.Value as IBitAccess).Value = value;
                WrappedValue = ACValueAsVariant;
            }
        }
    }
}
