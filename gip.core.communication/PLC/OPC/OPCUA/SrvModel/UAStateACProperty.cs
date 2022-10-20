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
    public class UATypeMapper
    {
        public UATypeMapper(uint dataType, NodeId nodeId, TypeInfo typeInfo, Type netType, bool isNullable)
        {
            _DataType = dataType;
            _NodeId = nodeId;
            _TypeInfo = typeInfo;
            _NetType = netType;
            _IsNullable = isNullable;
        }

        private bool _IsNullable;
        public bool IsNullable { get { return _IsNullable; } }

        // Opc.Ua.DataTypes
        private uint _DataType;
        public uint DataType { get { return _DataType; } }

        private NodeId _NodeId;
        public NodeId NodeId { get { return _NodeId; } }

        private TypeInfo _TypeInfo;
        public TypeInfo TypeInfo { get { return _TypeInfo; } }

        private Type _NetType;
        public Type NetType { get { return _NetType; } }
    }

    public class UAStateACProperty : BaseVariableState, IUAStateIACMember
    {
        public static readonly UATypeMapper[] _KnownTypes = new UATypeMapper[] 
        { 
            new UATypeMapper(DataTypes.Boolean, DataTypeIds.Boolean, TypeInfo.Scalars.Boolean, typeof(bool), false),
            new UATypeMapper(DataTypes.Boolean, DataTypeIds.Boolean, TypeInfo.Scalars.Boolean, typeof(bool?), true),
            new UATypeMapper(DataTypes.Byte, DataTypeIds.Byte, TypeInfo.Scalars.Byte, typeof(byte), false),
            new UATypeMapper(DataTypes.Byte, DataTypeIds.Byte, TypeInfo.Scalars.Byte, typeof(byte?), true),
            new UATypeMapper(DataTypes.SByte, DataTypeIds.SByte, TypeInfo.Scalars.SByte, typeof(sbyte), false),
            new UATypeMapper(DataTypes.SByte, DataTypeIds.SByte, TypeInfo.Scalars.SByte, typeof(sbyte?), true),
            new UATypeMapper(DataTypes.Int16, DataTypeIds.Int16, TypeInfo.Scalars.Int16, typeof(short), false),
            new UATypeMapper(DataTypes.Int16, DataTypeIds.Int16, TypeInfo.Scalars.Int16, typeof(short?), true),
            new UATypeMapper(DataTypes.Int32, DataTypeIds.Int32, TypeInfo.Scalars.Int32, typeof(int), false),
            new UATypeMapper(DataTypes.Int32, DataTypeIds.Int32, TypeInfo.Scalars.Int32, typeof(int?), true),
            new UATypeMapper(DataTypes.Int64, DataTypeIds.Int64, TypeInfo.Scalars.Int64, typeof(long), false),
            new UATypeMapper(DataTypes.Int64, DataTypeIds.Int64, TypeInfo.Scalars.Int64, typeof(long?), true),
            new UATypeMapper(DataTypes.UInt16, DataTypeIds.UInt16, TypeInfo.Scalars.UInt16, typeof(ushort), false),
            new UATypeMapper(DataTypes.UInt16, DataTypeIds.UInt16, TypeInfo.Scalars.UInt16, typeof(ushort?), true),
            new UATypeMapper(DataTypes.UInt32, DataTypeIds.UInt32, TypeInfo.Scalars.UInt32, typeof(uint), false),
            new UATypeMapper(DataTypes.UInt32, DataTypeIds.UInt32, TypeInfo.Scalars.UInt32, typeof(uint?), true),
            new UATypeMapper(DataTypes.UInt64, DataTypeIds.UInt64, TypeInfo.Scalars.UInt64, typeof(ulong), false),
            new UATypeMapper(DataTypes.UInt64, DataTypeIds.UInt64, TypeInfo.Scalars.UInt64, typeof(ulong?), true),
            new UATypeMapper(DataTypes.Float, DataTypeIds.Float, TypeInfo.Scalars.Float, typeof(float), false),
            new UATypeMapper(DataTypes.Float, DataTypeIds.Float, TypeInfo.Scalars.Float, typeof(float?), true),
            new UATypeMapper(DataTypes.Double, DataTypeIds.Double, TypeInfo.Scalars.Double, typeof(double), false),
            new UATypeMapper(DataTypes.Double, DataTypeIds.Double, TypeInfo.Scalars.Double, typeof(double?), true),
            new UATypeMapper(DataTypes.Decimal, DataTypeIds.Decimal, TypeInfo.Scalars.ExtensionObject, typeof(decimal), false),
            new UATypeMapper(DataTypes.Decimal, DataTypeIds.Decimal, TypeInfo.Scalars.ExtensionObject, typeof(decimal?), true),
            new UATypeMapper(DataTypes.String, DataTypeIds.String, TypeInfo.Scalars.String, typeof(string), false),
            new UATypeMapper(DataTypes.Guid, DataTypeIds.Guid, TypeInfo.Scalars.Guid, typeof(Guid), false),
            new UATypeMapper(DataTypes.DateTime, DataTypeIds.DateTime, TypeInfo.Scalars.DateTime, typeof(DateTime), false),
            new UATypeMapper(DataTypes.Duration, DataTypeIds.Duration, TypeInfo.Scalars.Double, typeof(TimeSpan), false),
            new UATypeMapper(DataTypes.Byte, DataTypeIds.Byte, TypeInfo.Scalars.Byte, typeof(BitAccessForByte), false),
            new UATypeMapper(DataTypes.UInt16, DataTypeIds.UInt16, TypeInfo.Scalars.UInt16, typeof(BitAccessForInt16), false),
            new UATypeMapper(DataTypes.UInt32, DataTypeIds.UInt32, TypeInfo.Scalars.UInt32, typeof(BitAccessForInt32), false),
            //new UATypeMapper(DataTypes.Enumeration, DataTypeIds.Enumeration, TypeInfo.Scalars.Int32, typeof(enum), false)
        };

        public static Type _BitAccessType = typeof(IBitAccess);
        public static Type BitAccessType
        {
            get
            {
                return _BitAccessType;
            }
        }

        public UAStateACProperty(IACPropertyBase instance, NodeState parentNode, ushort namespaceIndex, uint? sortOrder) : base(parentNode)
        {
            InnerPropertyTypeEnum propertyType;
            _TypeMapper = GetMapperForType(instance, out propertyType);
            if (_TypeMapper == null)
            {
                throw new ArgumentException("Property is not a valid ValueType");
            }
            _InnerPropertyType = propertyType;
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
            NumericId = sortOrder.HasValue ? sortOrder.Value + 1000 : System.Convert.ToUInt32(instance.ParentACComponent.ACMemberList.IndexOf(instance)) + 1000;
            DataType = _TypeMapper.NodeId; //TypeInfo.GetDataTypeId(_TypeInfo);
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

        public enum InnerPropertyTypeEnum
        {
            ValueType,
            Enum,
            BitAccess
        }

        private InnerPropertyTypeEnum _InnerPropertyType;

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

        private UATypeMapper _TypeMapper;
        public UATypeMapper TypeMapper
        {
            get
            {
                return _TypeMapper;
            }
        }

        public Variant ACValueAsVariant
        {
            get
            {
                return new Variant(ACValue, TypeMapper.TypeInfo);
            }
        }

        public object ACValue
        {
            get
            {
                if (_InnerPropertyType == InnerPropertyTypeEnum.ValueType)
                {
                    if (TypeMapper.IsNullable)
                    {
                        if (ACProperty.Value == null)
                            return TypeInfo.GetDefaultValue(TypeMapper.NodeId, ValueRanks.Scalar);
                        else
                            return ACProperty.Value;
                    }
                    else if (TypeMapper.DataType == DataTypes.Duration)
                    {
                        return ((TimeSpan)ACProperty.Value).TotalMilliseconds;
                    }
                    return ACProperty.Value;
                }
                else if (_InnerPropertyType == InnerPropertyTypeEnum.Enum)
                    return ACProperty.Value;
                else
                    return (ACProperty.Value as IBitAccess).Value;
            }
            set
            {
                if (_InnerPropertyType == InnerPropertyTypeEnum.ValueType)
                {
                    if (TypeMapper.IsNullable)
                    {
                        ACProperty.Value = value;
                    }
                    else if (TypeMapper.DataType == DataTypes.Duration)
                    {
                        ACProperty.Value = TimeSpan.FromMilliseconds((double)value);
                    }
                    else
                        ACProperty.Value = value;
                }
                else if (_InnerPropertyType == InnerPropertyTypeEnum.Enum)
                    ACProperty.Value = value;
                else
                    (ACProperty.Value as IBitAccess).Value = value;
                WrappedValue = ACValueAsVariant;
            }
        }

        public static bool IsUAType(IACPropertyBase instance)
        {
            InnerPropertyTypeEnum innerPropertyType;
            return (GetMapperForType(instance, out innerPropertyType) != null);
            //if (instance.ContainsAssignableTypeT(_KnownTypes.Select(c => c.NetType), false) >= 0)
            //    return true;
            //return instance.PropertyType.IsEnum || BitAccessType.IsAssignableFrom(instance.PropertyType);
        }

        public static UATypeMapper GetMapperForType(IACPropertyBase instance, out InnerPropertyTypeEnum innerPropertyType)
        {
            int indexFound = instance.ContainsAssignableTypeT(_KnownTypes.Select(c => c.NetType), false);
            if (indexFound >= 0)
            {
                innerPropertyType = InnerPropertyTypeEnum.ValueType;
                return _KnownTypes[indexFound];
            }
            if (instance.PropertyType.IsEnum)
            {
                innerPropertyType = InnerPropertyTypeEnum.Enum;
                //_TypeInfo = TypeInfo.Construct(instance.PropertyType);
                //DataType = TypeInfo.GetDataTypeId(_TypeInfo);
                return new UATypeMapper(DataTypes.Enumeration, DataTypeIds.Enumeration, TypeInfo.Scalars.Int32, typeof(Int32), false);
            }
            if (BitAccessType.IsAssignableFrom(instance.PropertyType))
            {
                indexFound = instance.ContainsAssignableTypeT(_KnownTypes.Select(c => c.NetType), true);
                if (indexFound >= 0)
                {
                    innerPropertyType = InnerPropertyTypeEnum.BitAccess;
                    return _KnownTypes[indexFound];
                }
            }
            innerPropertyType = InnerPropertyTypeEnum.ValueType;
            return null;
        }
    }
}
