// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.SourceGenerator;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// Represents a property assignable in XAML.
    /// This can be a normal .NET property or an attached property.
    /// </summary>
    public abstract class XamlPropertyInfo
    {
        public abstract object GetValue(object instance);
        public abstract void SetValue(object instance, object value);
        public abstract void ResetValue(object instance);
        public abstract TypeConverter TypeConverter { get; }
        public abstract Type TargetType { get; }
        public abstract Type ReturnType { get; }
        public abstract string Name { get; }
        public abstract string FullyQualifiedName { get; }
        public abstract bool IsAttached { get; }
        public abstract bool IsCollection { get; }
        public virtual bool IsEvent { get { return false; } }
        public virtual bool IsAdvanced { get { return false; } }
        public virtual AvaloniaProperty DependencyProperty { get { return null; } }
        public abstract string Category { get; }


        internal static T GetCustomAttribute<T>(Type type) where T : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), true);
            if (attributes != null &&attributes.Length > 0)
            {
                return (T)attributes[0];
            }
            return null;
        }

        internal static Type GetTypeFromName(string typeName, Type componentType)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            //  try the generic method.
            Type typeFromGetType = Type.GetType(typeName);

            // If we didn't get a type from the generic method, or if the assembly we found the type
            // in is the same as our Component's assembly, use or Component's assembly instead. This is
            // because the CLR may have cached an older version if the assembly's version number didn't change
            Type typeFromComponent = null;
            if (componentType != null)
            {
                if ((typeFromGetType == null) ||
                    (componentType.Assembly.FullName!.Equals(typeFromGetType.Assembly.FullName)))
                {
                    int comma = typeName.IndexOf(',');

                    if (comma != -1)
                        typeName = typeName.Substring(0, comma);

                    typeFromComponent = componentType.Assembly.GetType(typeName);
                }
            }

            return typeFromComponent ?? typeFromGetType;
        }

        internal static object CreateInstance(Type type, Type propertyType)
        {
            Type[] typeArgs = new Type[] { typeof(Type) };
            ConstructorInfo ctor = type.GetConstructor(typeArgs);
            if (ctor != null)
            {
                return TypeDescriptor.CreateInstance(null, type, typeArgs, new object[] { propertyType });
            }

            return TypeDescriptor.CreateInstance(null, type, null, null);
        }

        public static readonly TypeConverter StringTypeConverter = TypeDescriptor.GetConverter(typeof(string));

        public static TypeConverter GetCustomTypeConverter(Type propertyType)
        {
            if (propertyType == typeof(object))
                return StringTypeConverter;
            else if (propertyType == typeof(Type))
                return TypeTypeConverter.Instance;
            else if (propertyType == typeof(AvaloniaProperty))
                return DependencyPropertyConverter.Instance;
            else
                return null;
        }

        sealed class TypeTypeConverter : TypeConverter
        {
            public readonly static TypeTypeConverter Instance = new TypeTypeConverter();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                    return null;
                if (value is string)
                {
                    IXamlTypeResolver xamlTypeResolver = (IXamlTypeResolver)context.GetService(typeof(IXamlTypeResolver));
                    if (xamlTypeResolver == null)
                        throw new XamlLoadException("IXamlTypeResolver not found in type descriptor context.");
                    return xamlTypeResolver.Resolve((string)value);
                }
                else
                {
                    return base.ConvertFrom(context, culture, value);
                }
            }
        }

        sealed class DependencyPropertyConverter : TypeConverter
        {
            public readonly static DependencyPropertyConverter Instance = new DependencyPropertyConverter();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                    return null;
                if (value is string)
                {
                    XamlTypeResolverProvider xamlTypeResolver = (XamlTypeResolverProvider)context.GetService(typeof(XamlTypeResolverProvider));
                    if (xamlTypeResolver == null)
                        xamlTypeResolver = (XamlTypeResolverProvider)context.GetService(typeof(IXamlTypeResolver));
                    if (xamlTypeResolver == null)
                        throw new XamlLoadException("XamlTypeResolverProvider not found in type descriptor context.");
                    XamlPropertyInfo prop = xamlTypeResolver.ResolveProperty((string)value);
                    if (prop == null)
                        throw new XamlLoadException("Could not find property " + value + ".");
                    XamlDependencyPropertyInfo depProp = prop as XamlDependencyPropertyInfo;
                    if (depProp != null)
                        return depProp.DependencyProperty;
                    FieldInfo field = prop.TargetType.GetField(prop.Name + "Property", BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(AvaloniaProperty))
                    {
                        return (AvaloniaProperty)field.GetValue(null);
                    }
                    throw new XamlLoadException("Property " + value + " is not a dependency property.");
                }
                else
                {
                    return base.ConvertFrom(context, culture, value);
                }
            }
        }        
    }

    #region XamlDependencyPropertyInfo
    internal class XamlDependencyPropertyInfo : XamlPropertyInfo
    {
        readonly AvaloniaProperty _avaloniaProperty;
        readonly bool _isAttached;
        readonly bool _isCollection;
        readonly string _dependencyPropertyGetterName;
        Func<object, object> _attachedGetter;

        public override AvaloniaProperty DependencyProperty
        {
            get { return _avaloniaProperty; }
        }

        public XamlDependencyPropertyInfo(AvaloniaProperty avaloniaProperty, bool isAttached, string dependencyPropertyGetterName, Func<object, object> attachedGetter = null)
        {
            Debug.Assert(avaloniaProperty != null);
            this._avaloniaProperty = avaloniaProperty;
            this._isAttached = isAttached;
            this._isCollection = CollectionSupport.IsCollectionType(avaloniaProperty.PropertyType);
            this._attachedGetter = attachedGetter;
            this._dependencyPropertyGetterName = dependencyPropertyGetterName;
        }

        // public override TypeConverter TypeConverter
        // {
        //     get
        //     {
        //         return TypeDescriptor.GetConverter(this.ReturnType);
        //     }
        // }

        private bool _TypeConverterInitialized;
        private TypeConverter _TypeConverter;
        public override TypeConverter TypeConverter
        {
            get
            {
                if (_TypeConverterInitialized)
                    return _TypeConverter;                    
                _TypeConverterInitialized = true;
                _TypeConverter = GetCustomTypeConverter(_avaloniaProperty.PropertyType);
                if (_TypeConverter != null)
                    return _TypeConverter;
                TypeConverterAttribute attr = GetCustomAttribute<TypeConverterAttribute>(_avaloniaProperty.PropertyType);
                if (attr != null)
                {
                    if (attr.ConverterTypeName != null && attr.ConverterTypeName.Length > 0)
                    {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName, _avaloniaProperty.PropertyType);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType))
                        {
                            _TypeConverter = (TypeConverter)CreateInstance(converterType, _avaloniaProperty.PropertyType);
                        }
                    }
                }
                if (_TypeConverter == null)
                    _TypeConverter = TypeDescriptor.GetConverter(_avaloniaProperty.PropertyType);
                return _TypeConverter;
            }
        }

        public override string FullyQualifiedName
        {
            get
            {
                return this.TargetType.FullName + "." + this.Name;
            }
        }

        public override Type TargetType
        {
            get { return _avaloniaProperty.OwnerType; }
        }

        public override Type ReturnType
        {
            get { return _avaloniaProperty.PropertyType; }
        }

        public override string Name
        {
            get { return _avaloniaProperty.Name; }
        }

        public override string Category
        {
            get { return "Misc"; }
        }

        public override bool IsAttached
        {
            get { return _isAttached; }
        }

        public override bool IsCollection
        {
            get { return false; }
        }

        public override object GetValue(object instance)
        {
            try
            {
                if (_attachedGetter != null)
                {
                    return _attachedGetter(instance);
                }
            }
            catch (Exception)
            {

            }

            var dependencyObject = instance as AvaloniaObject;
            if (dependencyObject != null)
            {
                return dependencyObject.GetValue(_avaloniaProperty);
            }

            return null;
        }

        public override void SetValue(object instance, object value)
        {
            ((AvaloniaObject)instance).SetValue(_avaloniaProperty, value);
        }

        public override void ResetValue(object instance)
        {
            ((AvaloniaObject)instance).ClearValue(_avaloniaProperty);
        }
    }
    #endregion

    #region XamlNormalPropertyInfo
    internal sealed class XamlNormalPropertyInfo : XamlPropertyInfo
    {
        PropertyDescriptor _propertyDescriptor;
        AvaloniaProperty _avaloniaProperty;
        Type _elementType;

        public XamlNormalPropertyInfo(AvaloniaProperty avaloniaProperty, Type elementType)
        {
            _avaloniaProperty = avaloniaProperty;
            _elementType = elementType;   
        }

        public XamlNormalPropertyInfo(PropertyDescriptor propertyDescriptor, Type elementType)
        {
            _propertyDescriptor = propertyDescriptor;
            _elementType = elementType;   
        }

        public override AvaloniaProperty DependencyProperty
        {
            get
            {
                return _avaloniaProperty;
            }
        }

        public override object GetValue(object instance)
        {
            if (instance is AvaloniaObject avaloniaObject && _avaloniaProperty != null)
            {
                return avaloniaObject.GetValue(_avaloniaProperty);
            }
            else if (_propertyDescriptor != null)
            {
                return _propertyDescriptor.GetValue(instance);
            }
            throw new InvalidOperationException("Instance is not an AvaloniaObject or DependencyProperty is null.");
        }

        public override void SetValue(object instance, object value)
        {
            if (instance is AvaloniaObject avaloniaObject && _avaloniaProperty != null)
            {
                avaloniaObject.SetValue(_avaloniaProperty, value);
                return;
            }
            else if (_propertyDescriptor != null)
            {
                _propertyDescriptor.SetValue(instance, value);
                return;
            }
            throw new InvalidOperationException("Instance is not an AvaloniaObject or DependencyProperty is null.");            
        }

        public override void ResetValue(object instance)
        {
            if (instance is AvaloniaObject avaloniaObject && _avaloniaProperty != null)
            {
                avaloniaObject.ClearValue(_avaloniaProperty);
                return;
            }
            else if (_propertyDescriptor != null)
            {
                _propertyDescriptor.ResetValue(instance);
                return;
            }
            throw new InvalidOperationException("Instance is not an AvaloniaObject or DependencyProperty is null.");       
        }

        public override Type ReturnType
        {
            get { return _avaloniaProperty != null ? _avaloniaProperty.PropertyType : _propertyDescriptor.PropertyType; }
        }

        public override Type TargetType
        {
            get { return _avaloniaProperty != null ? _avaloniaProperty.OwnerType : _propertyDescriptor.ComponentType; }
        }

        public override string Category 
        {
            get 
            {
                if (_propertyDescriptor != null)
                {
                    return _propertyDescriptor.Category;
                }
                if (_elementType != null)
                {
                    var categoryAttribute = GetCustomAttribute<CategoryAttribute>(_elementType);
                    if (categoryAttribute != null)
                    {
                        return categoryAttribute.Category;
                    }
                }
                return ""; 
            }
        }

        private bool _TypeConverterInitialized;
        private TypeConverter _TypeConverter;
        public override TypeConverter TypeConverter
        {
            get
            {
                if (_TypeConverterInitialized)
                    return _TypeConverter;                    
                _TypeConverterInitialized = true;
                if (_propertyDescriptor != null)
                    _TypeConverter = GetCustomTypeConverter(_propertyDescriptor.PropertyType) ?? _propertyDescriptor.Converter;
                if (_TypeConverter != null)
                    return _TypeConverter;
                _TypeConverter = GetCustomTypeConverter(_avaloniaProperty.PropertyType);
                if (_TypeConverter != null)
                    return _TypeConverter;
                TypeConverterAttribute attr = GetCustomAttribute<TypeConverterAttribute>(_avaloniaProperty.PropertyType);
                if (attr != null)
                {
                    if (attr.ConverterTypeName != null && attr.ConverterTypeName.Length > 0)
                    {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName, _avaloniaProperty.PropertyType);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType))
                        {
                            _TypeConverter = (TypeConverter)CreateInstance(converterType, _avaloniaProperty.PropertyType);
                        }
                    }
                }
                if (_TypeConverter == null)
                    _TypeConverter = TypeDescriptor.GetConverter(_avaloniaProperty.PropertyType);
                return _TypeConverter;
            }
        }

        public override string FullyQualifiedName
        {
            get
            {
                if (_propertyDescriptor != null)
                    return _propertyDescriptor.ComponentType.FullName + "." + _propertyDescriptor.Name;
                return _avaloniaProperty.OwnerType.FullName + "." + _avaloniaProperty.Name;
            }
        }

        public override string Name
        {
            get 
            {
                if (_propertyDescriptor != null)
                    return _propertyDescriptor.Name; 
                return _avaloniaProperty.Name; 
            }
        }

        public override bool IsAttached
        {
            get { return false; }
        }

        public override bool IsCollection
        {
            get
            {
                if (_propertyDescriptor != null)
                    return CollectionSupport.IsCollectionType(_propertyDescriptor.PropertyType);
                return CollectionSupport.IsCollectionType(_avaloniaProperty.PropertyType);
            }
        }

        public override bool IsAdvanced
        {
            get
            {
                EditorBrowsableAttribute attr = null;
                if (_propertyDescriptor != null)
                {
                    attr = (EditorBrowsableAttribute)_propertyDescriptor.Attributes[typeof(EditorBrowsableAttribute)];
                    if (attr != null)
                    {
                        return attr.State == EditorBrowsableState.Advanced;
                    }
                    return false;
                }
                attr = GetCustomAttribute<EditorBrowsableAttribute>(_avaloniaProperty.PropertyType);
                if (attr != null)
                {
                    return attr.State == EditorBrowsableState.Advanced;
                }
                return false;
            }
        }
    }
    #endregion

    #region XamlEventPropertyInfo
    sealed class XamlEventPropertyInfo : XamlPropertyInfo
    {
        readonly EventDescriptor _eventDescriptor;

        public XamlEventPropertyInfo(EventDescriptor eventDescriptor)
        {
            this._eventDescriptor = eventDescriptor;
        }

        public override object GetValue(object instance)
        {
            return TargetType.GetEvent(Name);
        }

        public override void SetValue(object instance, object value)
        {

        }

        public override void ResetValue(object instance)
        {

        }

        public override Type ReturnType
        {
            get { return _eventDescriptor.EventType; }
        }

        public override Type TargetType
        {
            get { return _eventDescriptor.ComponentType; }
        }

        public override string Category
        {
            get { return _eventDescriptor.Category; }
        }

        public override TypeConverter TypeConverter
        {
            get { return XamlNormalPropertyInfo.StringTypeConverter; }
        }

        public override string FullyQualifiedName
        {
            get
            {
                return _eventDescriptor.ComponentType.FullName + "." + _eventDescriptor.Name;
            }
        }

        public override string Name
        {
            get { return _eventDescriptor.Name; }
        }

        public override bool IsEvent
        {
            get { return true; }
        }

        public override bool IsAttached
        {
            get { return false; }
        }

        public override bool IsCollection
        {
            get { return false; }
        }
    }
    #endregion
}
