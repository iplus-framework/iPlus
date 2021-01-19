// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACValue.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using System.Data.Objects;
using System.Globalization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container for values that are used in the Parameter- or ResultList of ACMethod and ACEventArgs
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACContainer" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ICloneable" />
    /// <seealso cref="gip.core.datamodel.IACAttach" />
    [DataContract]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACValue", "en{'ACValue'}de{'ACValue'}", typeof(ACValue), "ACValue", Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACValue'}de{'ACValue'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACValue : IACContainer, INotifyPropertyChanged, ICloneable, IACAttach
    {

        #region ctor's
        /// <summary>
        /// Initializes a new instance of the <see cref="ACValue"/> class.
        /// </summary>
        public ACValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValue"/> class.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="objectFullType">Full type of the object.</param>
        /// <param name="value">The value.</param>
        /// <param name="option">The option.</param>
        public ACValue(string acIdentifier, Type objectFullType, object value = null, Global.ParamOption option = Global.ParamOption.Required)
        {
            ACIdentifier = acIdentifier;
            ObjectFullType = objectFullType;
            Option = option;
            if (value != null)
                Value = value;
        }

        public ACValue(string acIdentifier, ACClass valueType, object value = null, Global.ParamOption option = Global.ParamOption.Required)
        {
            ACIdentifier = acIdentifier;
            ValueTypeACClass = valueType;
            Option = option;
            if (value != null)
                Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValue"/> class.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="dataTypeName">Name of the data type.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <param name="value">The value.</param>
        /// <param name="option">The option.</param>
        public ACValue(string acIdentifier, string dataTypeName, string genericType, object value = null, Global.ParamOption option = Global.ParamOption.Required)
        {
            ACIdentifier = acIdentifier;
            DataTypeName = dataTypeName;
            GenericType = genericType;
            Option = option;
            if (value != null)
                Value = value;
        }
       
        /// <summary>
        /// Nur für lokale Verwendung, da nicht serialisierbar für WCF bzw. Datenbankpersistierung
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="value">The value.</param>
        public ACValue(string acIdentifier, object value)
        {
            ACIdentifier = acIdentifier;
            Value = value;
        }


        #endregion

        string _ACIdentifier;
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [DataMember]
        [ACPropertyInfo(10, "", "en{'Propertyname'}de{'Eigenschaftsname'}")]
        public string ACIdentifier 
        {
            get
            {
                return _ACIdentifier;
            }
            set
            {
                if (_ACIdentifier != value)
                {
                    _ACIdentifier = value;
                    OnPropertyChanged(Const.ACIdentifierPrefix);
                }
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public virtual string ACCaption
        {
            get
            {
                var parentMethod = ParentMethod;
                if (parentMethod != null)
                    return parentMethod.GetACCaptionForACValue(this);
                return ACIdentifier;
            }
        }

        public ACValueList ParentList
        {
            get
            {
                var delegateList = GetPropChangedInvocationList();
                if (delegateList == null || !delegateList.Any())
                    return null;
                foreach (Delegate delegatex in delegateList)
                {
                    ACValueList list = delegatex.Target as ACValueList;
                    if (list != null)
                    {
                        return list;
                    }
                }
                return null;
            }
        }

        public virtual ACMethod ParentMethod
        {
            get 
            {
                ACValueList parentList = this.ParentList;
                if (parentList == null)
                    return null;
                return parentList.ParentACMethod;
            }
        }

        protected Delegate[] GetPropChangedInvocationList()
        {
            if (PropertyChanged == null)
                return null;
            return PropertyChanged.GetInvocationList();
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [IgnoreDataMember]
        [ACPropertyInfo(20, "", "en{'Value'}de{'Wert'}")]
        public object Value
        {
            get
            {
                if (XMLValue is IACContainerRef)
                {
                    return (XMLValue as IACContainerRef).Value;
                }
                return XMLValue;
            }
            set
            {
                if (XMLValue == value)
                    return;
                if (value != null)
                {
                    if (value is EntityObject)
                    {
                        EntityObject entity = value as EntityObject;
                        try
                        {
                            Type acRefType = typeof(ACRef<>).MakeGenericType(value.GetType());
                            object acRefInstance = Activator.CreateInstance(acRefType, new Object[] { value, entity.GetObjectContext(), false });
                            XMLValue = acRefInstance;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACValue", "Value", msg);
                        }
                        OnPropertyChanged(Const.Value);
                        return;
                    }
                }
                if (ValueTypeACClass != null)
                    XMLValue = ACConvert.ChangeType(value, XMLValue, ObjectFullType, true, ValueTypeACClass.Database, true);
                else
                    XMLValue = value;
                OnPropertyChanged(Const.Value);
            }
        }

        Global.ParamOption _Option;
        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        /// <value>The option.</value>
        [DataMember]
        [ACPropertyInfo(30, "", "en{'Option'}de{'Option'}")]
        public Global.ParamOption Option 
        {
            get
            {
                return _Option;
            }
            set
            {
                if (_Option != value)
                {
                    _Option = value;
                    OnPropertyChanged("Option");
                }
            }
        }

        ACClass _ValueTypeACClass;
        /// <summary>
        /// Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        [ACPropertyInfo(9999, "", "en{'Data Type'}de{'Datentyp'}")]
        public ACClass ValueTypeACClass
        {
            get 
            {
                return _ValueTypeACClass; 
            }
            set
            {
                _ValueTypeACClass = value;
                OnPropertyChanged("ValueTypeACClass");
            }
        }

        /// <summary>
        /// Gets or sets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
        [DataMember]
        [ACPropertyInfo(40, "", "en{'Data Type'}de{'Datentyp'}")]
        public string DataTypeName
        {
            get
            {
                if (_ValueTypeACClass == null)
                    return null;
                return _ValueTypeACClass.ACIdentifier;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _ValueTypeACClass = null;
                }
                _ValueTypeACClass = Database.GlobalDatabase.GetACType(value);
                OnPropertyChanged("DataTypeName");
            }
        }

        string _GenericType;
        /// <summary>
        /// Gets or sets the type of the generic.
        /// </summary>
        /// <value>The type of the generic.</value>
        [DataMember]
        [ACPropertyInfo(50, "", "en{'Generic Type'}de{'Generischer Typ'}")]
        public string GenericType
        {
            get
            {
                return _GenericType;
            }
            set
            {
                if (_GenericType != value)
                {
                    _GenericType = value;
                    OnPropertyChanged("GenericType");
                }
            }
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType
        {
            get
            {
                if (ValueTypeACClass == null)
                {
                    return null;
                }
                return ValueTypeACClass.ObjectType;
            }
        }

        /// <summary>
        /// Gets the type of the object generic.
        /// </summary>
        /// <value>The type of the object generic.</value>
        public Type ObjectGenericType
        {
            get
            {
                if (String.IsNullOrEmpty(GenericType))
                    return null;
                Type typeTGeneric = Type.GetType(GenericType);
                if (typeTGeneric != null)
                    return typeTGeneric;
                return TypeAnalyser.GetTypeInAssembly(GenericType);
            }
        }

        /// <summary>
        /// Gets or sets the full type of the object.
        /// </summary>
        /// <value>The full type of the object.</value>
        public Type ObjectFullType
        {
            get
            {
                if (String.IsNullOrEmpty(GenericType))
                    return ObjectType;
                return GetGenericACPropertyType(ObjectGenericType, ObjectType, null);
            }
            set
            {
                Type t = value; 
                if (value.BaseType != null && value.BaseType.IsGenericType)
                {
                    // If-Abfrage: Damirt Wokraround für EntityTemporaryEntry und Menu
                    if (value.BaseType.IsGenericType)
                    {
                        if (value.BaseType.GetGenericTypeDefinition() == typeof(List<>))
                            t = value.BaseType;
                    }
                }
                if (t.IsGenericType)
                {
                    Type genericTypeDefLevel1 = t.GetGenericTypeDefinition();
                    Type underlyingTypeLevel1 = t.GetGenericArguments()[0];
                    if (!underlyingTypeLevel1.IsGenericType)
                    {
                        if (!underlyingTypeLevel1.IsGenericParameter)
                        {
                            ValueTypeACClass = Database.GlobalDatabase.GetACType(underlyingTypeLevel1);
                        }
                        GenericType = genericTypeDefLevel1.FullName;
                    }
                    else
                    {
                        Type underlyingTypeLevel2 = underlyingTypeLevel1.GetGenericArguments()[0];
                        if (!underlyingTypeLevel2.IsGenericParameter)
                        {
                            ValueTypeACClass = Database.GlobalDatabase.GetACType(underlyingTypeLevel1.GetGenericArguments()[0].Name);
                        }
                        GenericType = underlyingTypeLevel1.GetGenericTypeDefinition().FullName;
                    }
                }
                else
                {
                    ValueTypeACClass = Database.GlobalDatabase.GetACType(value);
                    GenericType = "";
                }
                OnPropertyChanged("ObjectFullType");
            }
        }

        /// <summary>
        /// The XML value
        /// </summary>
        [DataMember]
        private object XMLValue;

        /// <summary>
        /// Gets the value as ref.
        /// </summary>
        /// <value>The value as ref.</value>
        [IgnoreDataMember]
        public IACContainerRef ValueAsRef
        {
            get
            {
                return XMLValue as IACContainerRef;
            }
        }

        /// <summary>
        /// Values the T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>``0.</returns>
        public T ValueT<T>()
        {
            if (!(Value is T))
                return default(T);
            return (T)Value;
        }

        /// <summary>
        /// Entities the T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueContext">The queue context.</param>
        /// <returns>``0.</returns>
        public T EntityT<T>(IACEntityOpQueue queueContext) where T : EntityObject
        {
            if (ValueAsRef != null)
            {
                EntityObject entityObject = null;
                bool reAttach = true;
                if (ValueAsRef.IsAttached)
                {
                    reAttach = false;
                    entityObject = ValueAsRef.Value as EntityObject;
                    if (entityObject != null)
                    {
                        if (entityObject.GetObjectContext() != queueContext.ObjectContext)
                            reAttach = true;
                    }
                    else
                        reAttach = true;
                }

                if (reAttach)
                {
                    (queueContext as ACDelegateQueue).ProcessAction(
                        delegate()
                        {
                            ValueAsRef.AttachTo(queueContext.ObjectContext as IACObject);
                            entityObject = ValueAsRef.Value as EntityObject;
                        }
                    );
                }
                if (entityObject == null)
                    return default(T);
                return (T)entityObject;
            }
            return ValueT<T>();
        }

        /// <summary>
        /// Entities the T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns>``0.</returns>
        public T EntityT<T>(IACEntityObjectContext context) where T : EntityObject
        {
            if (ValueAsRef != null)
            {
                EntityObject entityObject = null;
                bool reAttach = true;
                if (ValueAsRef.IsAttached)
                {
                    reAttach = false;
                    entityObject = ValueAsRef.Value as EntityObject;
                    if (entityObject != null)
                    {
                        if (entityObject.GetObjectContext() != context)
                            reAttach = true;
                    }
                    else
                        reAttach = true;
                }

                if (reAttach)
                {

                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        try
                        {
                            ValueAsRef.AttachTo(context as IACObject);
                            entityObject = ValueAsRef.Value as EntityObject;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACValue", "Entity<T>", msg);
                        }
                    }
                }
                if (entityObject == null)
                    return default(T);
                return (T)entityObject;
            }
            return ValueT<T>();
        }


        /// <summary>
        /// The _ is enumerable
        /// </summary>
        Boolean _IsEnumerable = false;
        /// <summary>
        /// Gets or sets the is enumerable.
        /// </summary>
        /// <value>The is enumerable.</value>
        [DataMember]
        public Boolean IsEnumerable
        {
            get
            {
                return _IsEnumerable;
            }
            set
            {
                if (_IsEnumerable != value)
                {
                    _IsEnumerable = value;
                    OnPropertyChanged("IsEnumerable");
                }
            }
        }

        [IgnoreDataMember]
        public bool HasDefaultValue
        {
            get
            {
                return GetDefaultValue() == Value;
            }
        }

        /// <summary>
        /// Sets the value from string.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        public void SetValueFromString(string stringValue)
        {
            if (stringValue == null)
            {
                SetDefaultValue();

            }
            else
            {
                if (ObjectType.IsEnum)
                    Value = Enum.Parse(ObjectType, stringValue);
                else
                {
                    if (stringValue == "null")
                    {
                        Value = null;
                    }
                    else
                    {
                        Value = Convert.ChangeType(stringValue, ObjectType);
                    }
                }
            }
        }

        public object GetDefaultValue()
        {
            if (this.ObjectType == null)
                return null;

            if (!String.IsNullOrEmpty(GenericType))
            {
                Value = null;
                //if (this.GenericType == "System.Nullable`1")
                //    Value = null;
                //else if (this.ObjectFullType.IsInterface)
                //    Value = null;
                //else
                //{
                //    Value = null;
                //    //try
                //    //{
                //    //    Value = Activator.CreateInstance(this.ObjectFullType);
                //    //}
                //    //catch (Exception)
                //    //{
                //    //    Value = null;
                //    //}
                //}
            }
            else if (this.ObjectType == typeof(Byte))
                return (Byte)0;
            else if (this.ObjectType == typeof(Int16))
                return (Int16)0;
            else if (this.ObjectType == typeof(Int32))
                return (Int32)0;
            else if (this.ObjectType == typeof(Int64))
                return (Int64)0;
            else if (this.ObjectType == typeof(SByte))
                return (SByte)0;
            else if (this.ObjectType == typeof(UInt16))
                return (UInt16)0;
            else if (this.ObjectType == typeof(UInt32))
                return (UInt32)0;
            else if (this.ObjectType == typeof(UInt64))
                return (UInt64)0;
            else if (this.ObjectType == typeof(Single))
                return (Single)0.0;
            else if (this.ObjectType == typeof(Double))
                return (Double)0.0;
            else if (this.ObjectType == typeof(DateTime))
                return DateTime.MinValue;
            else if (this.ObjectType == typeof(TimeSpan))
                return TimeSpan.MinValue;
            return null;
        }

        /// <summary>
        /// Sets the default value.
        /// </summary>
        public void SetDefaultValue()
        {
            if (this.ObjectType == null)
                return;
            Value = GetDefaultValue();
        }

        public void UpdateFrom(ACValue from)
        {
            if (from == null)
                return;
            this.Option = from.Option;
            this.XMLValue = from.XMLValue;
        }

#region TypeConversion
        /// <summary>
        /// Gets the param as boolean.
        /// </summary>
        /// <value>The param as boolean.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Boolean</exception>
        [IgnoreDataMember]
        public Boolean ParamAsBoolean
        {
            get
            {
                if ((Value == null) || !(Value is Boolean))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToBoolean(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Boolean");
                }
                return (Boolean)Value;
            }
        }

        /// <summary>
        /// Gets the param as S byte.
        /// </summary>
        /// <value>The param as S byte.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not SByte</exception>
        [IgnoreDataMember]
        public SByte ParamAsSByte
        {
            get
            {
                if ((Value == null) || !(Value is SByte))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToSByte(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not SByte");
                }
                return (SByte)Value;
            }
        }

        /// <summary>
        /// Gets the param as int16.
        /// </summary>
        /// <value>The param as int16.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Int16</exception>
        [IgnoreDataMember]
        public Int16 ParamAsInt16
        {
            get
            {
                if ((Value == null) || (!(Value is Int16) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToInt16(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Int16");
                }
                return (Int16)Value;
            }
        }

        /// <summary>
        /// Gets the param as int32.
        /// </summary>
        /// <value>The param as int32.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Int32</exception>
        [IgnoreDataMember]
        public Int32 ParamAsInt32
        {
            get
            {
                if ((Value == null) || (!(Value is Int32) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToInt32(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Int32");
                }
                return (Int32)Value;
            }
        }

        /// <summary>
        /// Gets the param as int64.
        /// </summary>
        /// <value>The param as int64.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Int64</exception>
        [IgnoreDataMember]
        public Int64 ParamAsInt64
        {
            get
            {
                if ((Value == null) || (!(Value is Int64) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToInt64(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Int64");
                }
                return (Int64)Value;
            }
        }

        /// <summary>
        /// Gets the param as U int16.
        /// </summary>
        /// <value>The param as U int16.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not UInt16</exception>
        [IgnoreDataMember]
        public UInt16 ParamAsUInt16
        {
            get
            {
                if ((Value == null) || (!(Value is UInt16) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToUInt16(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not UInt16");
                }
                return (UInt16)Value;
            }
        }

        /// <summary>
        /// Gets the param as U int32.
        /// </summary>
        /// <value>The param as U int32.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not UInt32</exception>
        [IgnoreDataMember]
        public UInt32 ParamAsUInt32
        {
            get
            {
                if ((Value == null) || (!(Value is UInt32) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToUInt32(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not UInt32");
                }
                return (UInt32)Value;
            }
        }

        /// <summary>
        /// Gets the param as U int64.
        /// </summary>
        /// <value>The param as U int64.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not UInt64</exception>
        [IgnoreDataMember]
        public UInt64 ParamAsUInt64
        {
            get
            {
                if ((Value == null) || (!(Value is UInt64) && !Value.GetType().IsEnum))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToUInt64(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not UInt64");
                }
                return (UInt64)Value;
            }
        }

        /// <summary>
        /// Gets the param as single.
        /// </summary>
        /// <value>The param as single.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Single</exception>
        [IgnoreDataMember]
        public Single ParamAsSingle
        {
            get
            {
                if ((Value == null) || !(Value is Single))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToSingle(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Single");
                }
                return (Single)Value;
            }
        }

        /// <summary>
        /// Gets the param as double.
        /// </summary>
        /// <value>The param as double.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Double</exception>
        [IgnoreDataMember]
        public Double ParamAsDouble
        {
            get
            {
                if ((Value == null) || !(Value is Double))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToDouble(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Double");
                }
                return (Double)Value;
            }
        }

        /// <summary>
        /// Gets the param as decimal.
        /// </summary>
        /// <value>The param as decimal.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Decimal</exception>
        [IgnoreDataMember]
        public Decimal ParamAsDecimal
        {
            get
            {
                if ((Value == null) || !(Value is Decimal))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToDecimal(CultureInfo.InvariantCulture);
                    throw new InvalidCastException("Parameter is not Decimal");
                }
                return (Decimal)Value;
            }
        }

        /// <summary>
        /// Gets the param as string.
        /// </summary>
        /// <value>The param as string.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not String</exception>
        [IgnoreDataMember]
        public String ParamAsString
        {
            get
            {
                if (Value == null)
                    return null;
                if (!(Value is String))
                {
                    if (Value != null && Value is IConvertible)
                        return (Value as IConvertible).ToString();
                    throw new InvalidCastException("Parameter is not String");
                }
                return (String)Value;
            }
        }

        /// <summary>
        /// Gets the param as GUID.
        /// </summary>
        /// <value>The param as GUID.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not Guid</exception>
        [IgnoreDataMember]
        public Guid ParamAsGuid
        {
            get
            {
                if ((Value == null) || !(Value is Guid))
                    throw new InvalidCastException("Parameter is not Guid");
                return (Guid)Value;
            }
        }

        /// <summary>
        /// Gets the param as time span.
        /// </summary>
        /// <value>The param as time span.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not TimeSpan</exception>
        [IgnoreDataMember]
        public TimeSpan ParamAsTimeSpan
        {
            get
            {
                if ((Value == null) || !(Value is TimeSpan))
                    throw new InvalidCastException("Parameter is not TimeSpan");
                return (TimeSpan)Value;
            }
        }


        /// <summary>
        /// Gets the param as date time.
        /// </summary>
        /// <value>The param as date time.</value>
        /// <exception cref="System.InvalidCastException">Parameter is not DateTime</exception>
        [IgnoreDataMember]
        public DateTime ParamAsDateTime
        {
            get
            {
                if ((Value == null) || !(Value is DateTime))
                    throw new InvalidCastException("Parameter is not DateTime");
                return (DateTime)Value;
            }
        }


#endregion

#region INotifyPropertyChanged
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
#endregion

#region Hilfsmethoden
        /// <summary>
        /// Gets the type of the generic AC property.
        /// </summary>
        /// <param name="acPropertyType">Type of the ac property.</param>
        /// <param name="typeT">The type T.</param>
        /// <param name="typeTGeneric">The type T generic.</param>
        /// <returns>Type.</returns>
        public Type GetGenericACPropertyType(Type acPropertyType, Type typeT, Type typeTGeneric = null)
        {
            if (typeTGeneric == null)
                return acPropertyType.MakeGenericType(new Type[] { typeT });
            else
            {
                Type genericOfTypeT = typeTGeneric.MakeGenericType(new Type[] { typeT });
                return acPropertyType.MakeGenericType(new Type[] { genericOfTypeT });
            }
        }

        /// <summary>
        /// Determines the type of the data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="GenericType">Type of the generic.</param>
        /// <param name="dataType">Type of the data.</param>
        public void DetermineDataType(Type value, ref string GenericType, ref Type dataType)
        {
        }
#endregion

        public virtual void CloneValue(ACValue from)
        {
            if (from == null)
                return;
            this._ACIdentifier = from.ACIdentifier;
            this._ValueTypeACClass = from.ValueTypeACClass;
            this.GenericType = from.GenericType;
            this.Option = from.Option;
            this._IsEnumerable = from.IsEnumerable;
            if (from.XMLValue != null && from.XMLValue is ICloneable)
                this.XMLValue = (from.XMLValue as ICloneable).Clone();
            else
                this.XMLValue = from.XMLValue;
        }

        public virtual void CopyValue(ACValue from)
        {
            if (from == null)
                return;
            try
            {
                if (from.Value != null && this.Value != from.Value)
                {
                    if (from.Value is IConvertible)
                    {
                        if (this.ObjectType != from.ObjectType)
                            this.Value = Convert.ChangeType(from.Value, this.ObjectType);
                        else
                            this.Value = from.Value;
                    }
                    else if (this.ObjectType == from.ObjectType)
                    {
                        this.CloneValue(from);
                    }
                }
                else if (from.Value == null)
                {
                    this.Value = null;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACValue", "CopyValue", msg);
            }
        }

        public virtual object Clone()
        {
            ACValue clone = new ACValue();
            clone.CloneValue(this);
            return clone;
        }

        /// <summary>Attaches the deserialized encapuslated objects to the parent context.</summary>
        /// <param name="parentACObject">The parent context. Normally this is a EF-Context (IACEntityObjectContext).</param>
        public void AttachTo(IACObject acObject)
        {
            if ((XMLValue != null) && (XMLValue is IACAttach))
                (XMLValue as IACAttach).AttachTo(acObject);
            else if ((Value != null) && (Value is IACAttach))
                (Value as IACAttach).AttachTo(acObject);
            if (ObjectAttached != null)
                ObjectAttached(this, new EventArgs());
        }

        /// <summary>Detaches the encapuslated objects from the parent context.</summary>
        /// <param name="detachFromContext">If attached object is a Entity object, then it will be detached from Change-Tracking if this parameter is set to true.</param>
        public void Detach(bool detachFromContext = false)
        {
            if (ObjectDetaching != null)
                ObjectDetaching(this, new EventArgs());
            if ((XMLValue != null) && (XMLValue is IACAttach))
                (XMLValue as IACAttach).Detach(detachFromContext);
            else if ((Value != null) && (Value is IACAttach))
                (Value as IACAttach).Detach(detachFromContext);
            if (ObjectDetached != null)
                ObjectDetached(this, new EventArgs());
        }

        /// <summary>Gets a value indicating whether the encapuslated value is attached.</summary>
        /// <value>
        ///   <c>true</c> if the encapuslated object is attached; otherwise, <c>false</c>.</value>
        public bool IsAttached
        {
            get 
            {
                if ((XMLValue != null) && (XMLValue is IACAttach))
                    return (XMLValue as IACAttach).IsAttached;
                else if ((Value != null) && (Value is IACAttach))
                    return (Value as IACAttach).IsAttached;
                return false;
            }
        }


        /// <summary>
        /// Occurs when the encapuslated object was detached.
        /// </summary>
        public event EventHandler ObjectDetached;

        /// <summary>
        /// Occurs before the deserialized content will be attached to be able to access the encapuslated object later.
        /// </summary>
        public event EventHandler ObjectDetaching;

        /// <summary>
        /// Occurs when the encapuslated object was attached.
        /// </summary>
        public event EventHandler ObjectAttached;
    }


    [DataContract]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACValueWithCaption", "en{'ACValueWithCaption'}de{'ACValueWithCaption'}", typeof(ACValueWithCaption), "ACValueWithCaption", Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACValueWithCaption'}de{'ACValueWithCaption'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACValueWithCaption : ACValue
    {
        public ACValueWithCaption() : base()
        {

        }

        public ACValueWithCaption(string acIdentifier, ACClass valueType, string acCaptionTranslation, object value = null, Global.ParamOption option = Global.ParamOption.Required) :
            base(acIdentifier,valueType,value,option)
        {
            ACIdentifier = acIdentifier;
            ValueTypeACClass = valueType;
            ACCaptionTranslation = acCaptionTranslation;
            Option = option;
            if (value != null)
                Value = value;
        }

        [DataMember]
        private string _ACCaptionTranslation;

        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public string ACCaptionTranslation
        {
            get
            {
                return _ACCaptionTranslation;
            }
            set
            {
                _ACCaptionTranslation = value;
            }
        }

    }
}
