// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Xaml.Interactivity;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// Represents a xaml object element.
    /// </summary>
    [DebuggerDisplay("XamlObject: {Instance}")]
    public sealed class XamlObject : XamlPropertyValue
    {
        XamlDocument document;
        XmlElement element;
        Type elementType;
        object instance;
        List<XamlProperty> properties = new List<XamlProperty>();
        string contentPropertyName;
        XamlProperty nameProperty;
        string runtimeNameProperty;

        /// <summary>For use by XamlParser only.</summary>
        internal XamlObject(XamlDocument document, XmlElement element, Type elementType, object instance)
        {
            this.document = document;
            this.element = element;
            this.elementType = elementType;
            this.instance = instance;

            this.contentPropertyName = GetContentPropertyName(elementType);
            //XamlSetTypeConverter = GetTypeConverterDelegate(elementType); // Doesn't exiast in Avalonia

            ServiceProvider = new XamlObjectServiceProvider(this);
            CreateWrapper();

            // Doesn't exist in Avalonia:
            //var rnpAttrs = elementType.GetCustomAttributes(typeof(RuntimeNamePropertyAttribute), true) as RuntimeNamePropertyAttribute[];
            //if (rnpAttrs != null && rnpAttrs.Length > 0 && !String.IsNullOrEmpty(rnpAttrs[0].Name))
            //{
            //    runtimeNameProperty = rnpAttrs[0].Name;
            //}
            // x:Name can be ujsed for all StyledElement
            if (typeof(StyledElement).IsAssignableFrom(elementType))
                runtimeNameProperty = nameof(StyledElement.Name);
        }

        /// <summary>For use by XamlParser only.</summary>
        internal void AddProperty(XamlProperty property)
        {
#if DEBUG
            if (property.IsAttached == false)
            {
                foreach (XamlProperty p in properties)
                {
                    if (p.IsAttached == false && p.PropertyName == property.PropertyName)
                    {
                        //throw new XamlLoadException("duplicate property:" + property.PropertyName);
                        //Debug.Fail("duplicate property");
                        return;
                    }
                }
            }
#endif
            properties.Add(property);
        }

        #region XamlPropertyValue implementation
        internal override object GetValueFor(XamlPropertyInfo targetProperty)
        {
            if (IsMarkupExtension
                // TODO: Handle Xaml.Behaviors
                //&& ((targetProperty == null)
                //    || ((targetProperty != null) && !typeof(StyledElementTrigger).IsAssignableFrom(targetProperty.TargetType) && !typeof(Condition).IsAssignableFrom(targetProperty.TargetType))
                //   )
                )
            {
                var value = ProvideValue();
                if (value is string && targetProperty != null && targetProperty.ReturnType != typeof(string))
                {
                    return XamlParser.CreateObjectFromAttributeText((string)value, targetProperty, this);
                }
                return value;
            }
            else if (IsSealedObject)
            {
                return GetClonedInstance();
            }
            else
            {
                return instance;
            }
        }

        internal override XmlNode GetNodeForCollection()
        {
            return element;
        }
        #endregion

        XamlObject parentObject;

        /// <summary>
        /// Gets the parent object.
        /// </summary>
        public XamlObject ParentObject
        {
            get
            {
                return parentObject;
            }
            internal set { parentObject = value; }
        }

        internal override void OnParentPropertyChanged()
        {
            parentObject = (ParentProperty != null) ? ParentProperty.ParentObject : null;
            base.OnParentPropertyChanged();
        }

        internal XmlElement XmlElement
        {
            get { return element; }
        }

        public PositionXmlElement PositionXmlElement
        {
            get { return element as PositionXmlElement; }
        }

        XmlAttribute xmlAttribute;

        internal XmlAttribute XmlAttribute
        {
            get { return xmlAttribute; }
            set
            {
                xmlAttribute = value;
                element = VirtualAttachTo(XmlElement, value.OwnerElement);
            }
        }

        string GetPrefixOfNamespace(string ns, XmlElement target)
        {
            if (target.NamespaceURI == ns)
                return null;
            var prefix = target.GetPrefixOfNamespace(ns);
            if (!string.IsNullOrEmpty(prefix))
                return prefix;
            var obj = this;
            while (obj != null)
            {
                prefix = obj.XmlElement.GetPrefixOfNamespace(ns);
                if (!string.IsNullOrEmpty(prefix))
                    return prefix;
                obj = obj.ParentObject;
            }
            return null;
        }

        XmlElement VirtualAttachTo(XmlElement e, XmlElement target)
        {
            string prefix = GetPrefixOfNamespace(e.NamespaceURI, target);
            if (string.IsNullOrEmpty(prefix))
                prefix = e.GetPrefixOfNamespace(e.NamespaceURI);
            XmlElement newElement = e.OwnerDocument.CreateElement(prefix, e.LocalName, e.NamespaceURI);

            foreach (XmlAttribute a in target.Attributes)
            {
                if (a.Prefix == "xmlns" || a.Name == "xmlns")
                {
                    newElement.Attributes.Append(a.Clone() as XmlAttribute);
                }
            }

            while (e.HasChildNodes)
            {
                newElement.AppendChild(e.FirstChild);
            }

            XmlAttributeCollection ac = e.Attributes;
            while (ac.Count > 0)
            {
                newElement.Attributes.Append(ac[0]);
            }

            return newElement;
        }

        /// <summary>
        /// Gets the name of the content property for the specified element type, or null if not available.
        /// </summary>
        /// <param name="elementType">The element type to get the content property name for.</param>
        /// <returns>The name of the content property for the specified element type, or null if not available.</returns>
        internal static string GetContentPropertyName(Type elementType)
        {
            // Search for properties with TemplateContentAttribute
            var properties = elementType.GetProperties();
            foreach (var property in properties)
            {
                var contentAttrs = property.GetCustomAttributes(typeof(TemplateContentAttribute), true) as TemplateContentAttribute[];
                if (contentAttrs != null && contentAttrs.Length > 0)
                {
                    return property.Name;
                }
            }

            return null;
        }

#if AVALONIA_SUPPORTS_FROM_WPF
        //internal delegate void TypeConverterDelegate(Object targetObject, XamlSetTypeConverterEventArgs eventArgs);

        //internal TypeConverterDelegate XamlSetTypeConverter { get; private set; }

        //internal static TypeConverterDelegate GetTypeConverterDelegate(Type elementType)
        //{
        //    var attrs = elementType.GetCustomAttributes(typeof(XamlSetTypeConverterAttribute), true) as XamlSetTypeConverterAttribute[];
        //    if (attrs != null && attrs.Length > 0)
        //    {
        //        var name = attrs[0].XamlSetTypeConverterHandler;
        //        var method = elementType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        //        return (TypeConverterDelegate)TypeConverterDelegate.CreateDelegate(typeof(TypeConverterDelegate), method);
        //    }

        //    return null;
        //}

        //private XamlType _systemXamlTypeForProperty = null;

        ///// <summary>
        ///// Gets a <see cref="XamlType"/> representing the <see cref="XamlObject.ElementType"/>.
        ///// </summary>
        //public XamlType SystemXamlTypeForProperty
        //{
        //    get
        //    {
        //        if (_systemXamlTypeForProperty == null)
        //            _systemXamlTypeForProperty = new XamlType(this.ElementType, this.ServiceProvider.SchemaContext);
        //        return _systemXamlTypeForProperty;
        //    }
        //}
#endif

        internal override void AddNodeTo(XamlProperty property)
        {
            XamlObject holder;
            if (!UpdateXmlAttribute(true, property, out holder))
            {
                property.AddChildNodeToProperty(element);
            }
            UpdateMarkupExtensionChain();
        }

        internal override void RemoveNodeFromParent()
        {
            if (XmlAttribute != null)
            {
                XmlAttribute.OwnerElement.RemoveAttribute(XmlAttribute.Name);
                xmlAttribute = null;
            }
            else
            {
                XamlObject holder;
                if (!UpdateXmlAttribute(false, null, out holder))
                {
                    element.ParentNode.RemoveChild(element);
                }
            }
            //TODO: PropertyValue still there
            //UpdateMarkupExtensionChain();
        }

        //TODO: reseting path property for binding doesn't work in XamlProperty
        //use CanResetValue()
        internal void OnPropertyChanged(XamlProperty property)
        {
            XamlObject holder;
            if (!UpdateXmlAttribute(false, property, out holder))
            {
                if (holder != null &&
                    holder.XmlAttribute != null)
                {
                    holder.XmlAttribute.OwnerElement.RemoveAttributeNode(holder.XmlAttribute);
                    holder.xmlAttribute = null;
                    holder.ParentProperty.AddChildNodeToProperty(holder.element);

                    bool isThisUpdated = false;
                    foreach (XamlProperty holderProp in holder.Properties.Where((prop) => prop.IsSet))
                    //foreach (XamlObject propXamlObject in holder.Properties.Where((prop) => prop.IsSet).Select((prop) => prop.PropertyValue).OfType<XamlObject>())
                    {
                        XamlObject propXamlObject = holderProp.PropertyValue as XamlObject;
                        if (propXamlObject != null)
                        {
                            XamlObject innerHolder;
                            bool updateResult = propXamlObject.UpdateXmlAttribute(true, holderProp, out innerHolder);
                            Debug.Assert(updateResult || innerHolder == null);
                        }

                        if (propXamlObject == this)
                            isThisUpdated = true;
                    }
                    if (!isThisUpdated)
                        this.UpdateXmlAttribute(true, property, out holder);
                }
            }
            UpdateMarkupExtensionChain();
        }

        void UpdateChildMarkupExtensions(XamlObject obj)
        {
            foreach (var prop in obj.Properties)
            {
                if (prop.IsSet)
                {
                    var propXamlObject = prop.PropertyValue as XamlObject;
                    if (propXamlObject != null)
                    {
                        UpdateChildMarkupExtensions(propXamlObject);
                    }
                }
                else if (prop.IsCollection)
                {
                    foreach (var propXamlObject in prop.CollectionElements.OfType<XamlObject>())
                    {
                        UpdateChildMarkupExtensions(propXamlObject);
                    }
                }
            }

            if (obj.IsMarkupExtension && obj.ParentProperty != null)
            {
                obj.ParentProperty.UpdateValueOnInstance();
            }
        }

        void UpdateMarkupExtensionChain()
        {
            var obj = this;
            while (obj != null && obj.IsMarkupExtension)
            {
                if (obj.ParentProperty != null)
                {
                    obj.ParentProperty.UpdateValueOnInstance();
                    obj = obj.ParentObject;
                }
                else
                    break;
            }
        }

        bool UpdateXmlAttribute(bool force, XamlProperty changedProperty, out XamlObject holder)
        {
            holder = FindXmlAttributeHolder();
            if (holder == null && force && IsMarkupExtension)
            {
                holder = this;
            }
            bool canPrint = false;
            if (holder != null)
                canPrint = MarkupExtensionPrinter.CanPrint(holder, changedProperty);

            if (holder != null)
            {
                if (canPrint)
                {
                    var s = MarkupExtensionPrinter.Print(holder, changedProperty);
                    holder.XmlAttribute = holder.ParentProperty.SetAttribute(s);
                    return true;
                }
                //else
                //{
                //    ResetMarkupExtToPropElementSyntax();
                //}
            }
            return false;
        }

        XamlObject FindXmlAttributeHolder()
        {
            var obj = this;
            while (obj != null && obj.IsMarkupExtension)
            {
                if (obj.XmlAttribute != null)
                {
                    return obj;
                }
                obj = obj.ParentObject;
            }
            return null;
        }

        private void ResetMarkupExtToPropElementSyntax()
        {
            var obj = this;
            while (obj != null && obj.IsMarkupExtension)
            {
                if (obj.XmlAttribute != null)
                {
                    obj.XmlAttribute.OwnerElement.RemoveAttribute(obj.XmlAttribute.Name);
                    //obj.XmlAttribute = null;
                }
                obj = obj.ParentObject;
            }
        }

        /// <summary>
        /// Gets the XamlDocument where this XamlObject is declared in.
        /// </summary>
        public XamlDocument OwnerDocument
        {
            get { return document; }
        }

        /// <summary>
        /// Gets the instance created by this object element.
        /// </summary>
        public object Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Gets whether this instance represents a MarkupExtension.
        /// </summary>
        public bool IsMarkupExtension
        {
            get { return instance is MarkupExtension; }
        }

        public bool IsSealedObject
        {
            get
            {
                // TODO:
                return (   (instance is SetterBase)
                        || (instance is StyledElementTrigger)
                        //|| (instance is Condition)
                        || ((wrapper != null)
                             && ((wrapper is BindingWrapper) || (wrapper is MultiBindingWrapper))
                           )
                       );
            }
        }

        /// <summary>
        /// Gets whether there were load errors for this object.
        /// </summary>
        public bool HasErrors { get; internal set; }

        /// <summary>
        /// Gets the type of this object element.
        /// </summary>
        public Type ElementType
        {
            get { return elementType; }
        }

        /// <summary>
        /// Gets a read-only collection of properties set on this XamlObject.
        /// This includes both attribute and element properties.
        /// </summary>
        public IList<XamlProperty> Properties
        {
            get
            {
                return properties.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the name of the content property.
        /// </summary>
        public string ContentPropertyName
        {
            get
            {
                return contentPropertyName;
            }
        }


        /// <summary>
        /// Gets which property name of the type maps to the XAML x:Name attribute.
        /// </summary>
        public string RuntimeNameProperty
        {
            get
            {
                return runtimeNameProperty;
            }
        }

        /// <summary>
        /// Gets which property of the type maps to the XAML x:Name attribute.
        /// </summary>
        public XamlProperty NameProperty
        {
            get
            {
                if (nameProperty == null && runtimeNameProperty != null)
                    nameProperty = FindOrCreateProperty(runtimeNameProperty);

                return nameProperty;
            }
        }

        /// <summary>
        /// Gets/Sets the name of this XamlObject.
        /// </summary>
        public string Name
        {
            get
            {
                string name = GetXamlAttribute("Name");

                if (String.IsNullOrEmpty(name))
                {
                    if (NameProperty != null && NameProperty.IsSet)
                        name = (string)NameProperty.ValueOnInstance;
                }

                if (name == String.Empty)
                    name = null;

                return name;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    this.SetXamlAttribute("Name", null);
                else
                    this.SetXamlAttribute("Name", value);
            }
        }

        public XamlProperty FindProperty(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            foreach (XamlProperty p in properties)
            {
                if (!p.IsAttached && p.PropertyName == propertyName)
                    return p;
            }

            return null;
        }

        /// <summary>
        /// Finds the specified property, or creates it if it doesn't exist.
        /// </summary>
        public XamlProperty FindOrCreateProperty(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            //			if (propertyName == ContentPropertyName)
            //				return 

            foreach (XamlProperty p in properties)
            {
                if (!p.IsAttached && p.PropertyName == propertyName)
                    return p;
            }
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(instance);
            PropertyDescriptor propertyInfo = propertyDescriptors[propertyName];
            XamlProperty newProperty;

            if (propertyInfo == null)
            {
                propertyDescriptors = TypeDescriptor.GetProperties(this.elementType);
                propertyInfo = propertyDescriptors[propertyName];
            }
            if (propertyInfo != null)
            {
                newProperty = new XamlProperty(this, new XamlNormalPropertyInfo(propertyInfo));
            }
            else
            {
                EventDescriptorCollection events = TypeDescriptor.GetEvents(instance);
                EventDescriptor eventInfo = events[propertyName];

                if (eventInfo == null)
                {
                    events = TypeDescriptor.GetEvents(this.elementType);
                    eventInfo = events[propertyName];
                }

                if (eventInfo != null)
                {
                    newProperty = new XamlProperty(this, new XamlEventPropertyInfo(eventInfo));
                }
                else
                {
                    return null;
                    //throw new ArgumentException("The property '" + propertyName + "' doesn't exist on " + elementType.FullName, "propertyName");
                }
            }
            properties.Add(newProperty);
            return newProperty;
        }

        /// <summary>
        /// Finds the specified property, or creates it if it doesn't exist.
        /// </summary>
        public XamlProperty FindOrCreateAttachedProperty(Type ownerType, string propertyName)
        {
            if (ownerType == null)
                throw new ArgumentNullException("ownerType");
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            foreach (XamlProperty p in properties)
            {
                if (p.IsAttached && p.PropertyTargetType == ownerType && p.PropertyName == propertyName)
                    return p;
            }
            XamlPropertyInfo info = XamlParser.TryFindAttachedProperty(ownerType, propertyName);
            if (info == null)
            {
                throw new ArgumentException("The attached property '" + propertyName + "' doesn't exist on " + ownerType.FullName, "propertyName");
            }
            XamlProperty newProperty = new XamlProperty(this, info);
            properties.Add(newProperty);
            return newProperty;
        }

        /// <summary>
        /// Gets an attribute in the x:-namespace.
        /// </summary>
        public string GetXamlAttribute(string name)
        {
            var value = element.GetAttribute(name, XamlConstants.XamlNamespace);

            if (string.IsNullOrEmpty(value))
            {
                value = element.GetAttribute(name, XamlConstants.Xaml2009Namespace);
            }

            return value;
        }

        /// <summary>
        /// Sets an attribute in the x:-namespace.
        /// </summary>
        public void SetXamlAttribute(string name, string value)
        {
            XamlProperty runtimeNameProperty = null;
            bool isNameChange = false;

            if (name == "Name")
            {
                isNameChange = true;
                string oldName = GetXamlAttribute("Name");

                if (String.IsNullOrEmpty(oldName))
                {
                    runtimeNameProperty = this.NameProperty;
                    if (runtimeNameProperty != null)
                    {
                        if (runtimeNameProperty.IsSet)
                            oldName = (string)runtimeNameProperty.ValueOnInstance;
                        else
                            runtimeNameProperty = null;
                    }
                }

                if (String.IsNullOrEmpty(oldName))
                    oldName = null;

                NameScopeHelper.NameChanged(this, oldName, value);
            }

            if (value == null)
                element.RemoveAttribute(name, XamlConstants.XamlNamespace);
            else
            {
                var prefix = element.GetPrefixOfNamespace(XamlConstants.XamlNamespace);
                var prefix2009 = element.GetPrefixOfNamespace(XamlConstants.Xaml2009Namespace);

                if (!string.IsNullOrEmpty(prefix))
                {
                    var attribute = element.OwnerDocument.CreateAttribute(prefix, name, XamlConstants.XamlNamespace);
                    attribute.InnerText = value;
                    element.SetAttributeNode(attribute);
                }
                else if (!string.IsNullOrEmpty(prefix2009))
                {
                    var attribute = element.OwnerDocument.CreateAttribute(prefix, name, XamlConstants.Xaml2009Namespace);
                    attribute.InnerText = value;
                    element.SetAttributeNode(attribute);
                }
                else
                    element.SetAttribute(name, XamlConstants.XamlNamespace, value);
            }

            if (isNameChange)
            {
                bool nameChangedAlreadyRaised = false;
                if (runtimeNameProperty != null)
                {
                    var handler = new EventHandler((sender, e) => nameChangedAlreadyRaised = true);
                    this.NameChanged += handler;

                    try
                    {
                        runtimeNameProperty.Reset();
                    }
                    finally
                    {
                        this.NameChanged -= handler;
                    }
                }

                if (NameChanged != null && !nameChangedAlreadyRaised)
                    NameChanged(this, EventArgs.Empty);
            }

        }

        /// <summary>
        /// Gets/Sets the <see cref="XamlObjectServiceProvider"/> associated with this XamlObject.
        /// </summary>
        public XamlObjectServiceProvider ServiceProvider { get; set; }

        MarkupExtensionWrapper wrapper;
        MarkupExtensionWrapper sealedObjectWrapper;

        void CreateWrapper()
        {
            if (Instance is BindingBase)
            {
                wrapper = new BindingWrapper(this);
            }
            else if (Instance is MultiBinding)
            {
                wrapper = new MultiBindingWrapper(this);
            }
            else if (Instance is StaticResourceExtension)
            {
                wrapper = new StaticResourceWrapper(this);
            }
            //if (wrapper != null)
            //{
            //    wrapper.XamlObject = this;
            //}

            if (instance is SetterBase)
            {
                sealedObjectWrapper = new SetterBaseWrapper(this);
            }
            // TODO: Xaml.Behaviors support
            //else if (instance is StyledElementTrigger)
            //{
            //    sealedObjectWrapper = new ConditionWrapper();
            //}
            // TODO: Avalonia doesn't have Condition class
            //else if (instance is Condition)
            //{
            //    sealedObjectWrapper = new ConditionWrapper();
            //}
            if (sealedObjectWrapper != null)
                sealedObjectWrapper.XamlObject = this;

            if (wrapper == null && IsMarkupExtension)
            {
                var markupExtensionWrapperAttribute = Instance.GetType().GetCustomAttributes(typeof(MarkupExtensionWrapperAttribute), false).FirstOrDefault() as MarkupExtensionWrapperAttribute;
                if (markupExtensionWrapperAttribute != null)
                {
                    wrapper = MarkupExtensionWrapper.CreateWrapper(markupExtensionWrapperAttribute.MarkupExtensionWrapperType, this);
                }
                else
                {
                    wrapper = MarkupExtensionWrapper.TryCreateWrapper(Instance.GetType(), this);
                }
            }
        }

        object ProvideValue()
        {
            if (wrapper != null)
            {
                return wrapper.ProvideValue();
            }
            if (this.ParentObject != null && this.ParentObject.ElementType == typeof(Setter) && this.ElementType == typeof(DynamicResourceExtension))
                return Instance;
            return (Instance as MarkupExtension).ProvideValue(ServiceProvider);
        }

        internal object GetClonedInstance()
        {
            if (wrapper != null)
                return wrapper.GetClonedInstance();
            if (sealedObjectWrapper != null)
                return sealedObjectWrapper.GetClonedInstance();
            return null;
        }

        internal string GetNameForMarkupExtension()
        {
            string markupExtensionName = XmlElement.Name;

            // By convention a markup extension class name typically includes an "Extension" suffix.
            // When you reference the markup extension in XAML the "Extension" suffix is optional.
            // If present remove it to avoid bloating the XAML.
            if (markupExtensionName.EndsWith("Extension", StringComparison.Ordinal))
            {
                markupExtensionName = markupExtensionName.Substring(0, markupExtensionName.Length - 9);
            }

            return markupExtensionName;
        }

        /// <summary>
        /// Is raised when the name of this XamlObject changes.
        /// </summary>
        public event EventHandler NameChanged;
    }


    class BindingWrapper : MarkupExtensionWrapper
    {
        public BindingWrapper(XamlObject xamlObject) : base(xamlObject)
        {
        }

        public override object ProvideValue()
        {
            var target = XamlObject.Instance as Binding;
            //TODO: XamlObject.Clone()
            var b = GetClonedInstance() as Binding;
            if (b == null)
                return null;
            object providedValue = null;
            try
            {
                IProvideValueTarget service = (IProvideValueTarget)XamlObject.ServiceProvider.GetService(typeof(IProvideValueTarget));
                if (service == null)
                    return null;
                providedValue = (b as IBinding).Initiate(service.TargetObject as AvaloniaObject, service.TargetProperty as AvaloniaProperty);
                // Future Avalonia Version 12:
                //providedValue = (b as IBinding2).Instance(service.TargetObject as AvaloniaObject, service.TargetProperty as AvaloniaProperty, null);
                //providedValue = b.ProvideValue(XamlObject.ServiceProvider);
            }
            catch (Exception /*e*/)
            {
            }
            return providedValue;
        }

        public override object GetClonedInstance()
        {
            var target = XamlObject.Instance as Binding;
            var b = Activator.CreateInstance(target.GetType()) as Binding;
            //var b = new Binding();
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
            {
                if (pd.IsReadOnly) continue;
                try
                {
                    var val1 = pd.GetValue(b);
                    var val2 = pd.GetValue(target);
                    if (object.Equals(val1, val2)) continue;
                    pd.SetValue(b, val2);
                }
                catch { }
            }
            return b;
        }
    }

    class MultiBindingWrapper : MarkupExtensionWrapper
    {
        public MultiBindingWrapper(XamlObject xamlObject) : base(xamlObject)
        {
        }

        public override object ProvideValue()
        {
            var b = GetClonedInstance() as MultiBinding;
            if (b == null)
                return null;
            object providedValue = null;
            try
            {
                IProvideValueTarget service = (IProvideValueTarget)XamlObject.ServiceProvider.GetService(typeof(IProvideValueTarget));
                if (service == null)
                    return null;
                providedValue = (b as IBinding).Initiate(service.TargetObject as AvaloniaObject, service.TargetProperty as AvaloniaProperty);
                // Future Avalonia Version 12:
                //providedValue = (b as IBinding2).Instance(service.TargetObject as AvaloniaObject, service.TargetProperty as AvaloniaProperty, null);
                //providedValue = b.ProvideValue(XamlObject.ServiceProvider);
            }
            catch (Exception /*e*/)
            {
            }
            return providedValue;
        }

        public override object GetClonedInstance()
        {
            var target = XamlObject.Instance as MultiBinding;
            var b = Activator.CreateInstance(XamlObject.Instance.GetType()) as MultiBinding;
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
            {
                if (pd.IsReadOnly)
                {
                    if (pd.Name == "Bindings")
                    {
                        Collection<BindingBase> col1 = pd.GetValue(b) as Collection<BindingBase>;
                        Collection<BindingBase> col2 = pd.GetValue(target) as Collection<BindingBase>;
                        foreach (BindingBase itemBinding in col2)
                        {
                            col1.Add(itemBinding);
                        }
                    }
                    continue;
                }
                try
                {
                    var val1 = pd.GetValue(b);
                    var val2 = pd.GetValue(target);
                    if (object.Equals(val1, val2)) continue;
                    pd.SetValue(b, val2);
                }
                catch { }
            }
            return b;
        }

    }

    class StaticResourceWrapper : MarkupExtensionWrapper
    {
        public StaticResourceWrapper(XamlObject xamlObject) : base(xamlObject)
        {
        }

        public override object ProvideValue()
        {
            var target = XamlObject.Instance as StaticResourceExtension;
            object result = null;
            if (target.ResourceKey != null)
                XamlObject.ServiceProvider.Resolver.FindResource(target.ResourceKey);
            if ((result == null) && (target is StaticResourceExtension))
            {
                result = (target as StaticResourceExtension).ProvideValue(XamlObject.ServiceProvider);
            }
            return result;
        }

        public override object GetClonedInstance()
        {
            var target = XamlObject.Instance as StaticResourceExtension;
            var b = Activator.CreateInstance(target.GetType()) as StaticResourceExtension;
            //var b = new Binding();
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
            {
                if (pd.IsReadOnly) continue;
                try
                {
                    var val1 = pd.GetValue(b);
                    var val2 = pd.GetValue(target);
                    if (object.Equals(val1, val2)) continue;
                    pd.SetValue(b, val2);
                }
                catch { }
            }
            return b;
        }
    }

    class SetterBaseWrapper : MarkupExtensionWrapper
    {
        public SetterBaseWrapper(XamlObject xamlObject) : base(xamlObject)
        {
        }

        public override object ProvideValue()
        {
            throw new NotImplementedException();
        }

        public override object GetClonedInstance()
        {
            var target = XamlObject.Instance as SetterBase;
            var b = Activator.CreateInstance(XamlObject.Instance.GetType()) as SetterBase;
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
            {
                if (pd.IsReadOnly) continue;
                try
                {
                    var val1 = pd.GetValue(b);
                    var val2 = pd.GetValue(target);
                    if (object.Equals(val1, val2)) continue;
                    pd.SetValue(b, val2);
                }
                catch { }
            }
            return b;
        }
    }

    //class ConditionWrapper : MarkupExtensionWrapper
    //{
    //    public ConditionWrapper(XamlObject xamlObject) : base(xamlObject)
    //    {
    //    }

    //    public override object ProvideValue()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override object GetClonedInstance()
    //    {
    //        var target = XamlObject.Instance as Condition;
    //        var b = Activator.CreateInstance(XamlObject.Instance.GetType()) as Condition;
    //        foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
    //        {
    //            if (pd.IsReadOnly) continue;
    //            try
    //            {
    //                var val1 = pd.GetValue(b);
    //                var val2 = pd.GetValue(target);
    //                if (object.Equals(val1, val2)) continue;
    //                pd.SetValue(b, val2);
    //            }
    //            catch { }
    //        }
    //        return b;
    //    }
    //}

}
