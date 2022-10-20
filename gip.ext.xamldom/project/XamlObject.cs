// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Data;
using System.Windows;
using System.Collections.ObjectModel;

namespace gip.ext.xamldom
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

        /// <summary>For use by XamlParser only.</summary>
        internal XamlObject(XamlDocument document, XmlElement element, Type elementType, object instance)
        {
            this.document = document;
            this.element = element;
            this.elementType = elementType;
            this.instance = instance;

            if (element.Name == "Border")
            {
                int i = 0;
                i++;
            }

            var contentAttrs = elementType.GetCustomAttributes(typeof(ContentPropertyAttribute), true) as ContentPropertyAttribute[];
            if (contentAttrs != null && contentAttrs.Length > 0)
            {
                this.contentPropertyName = contentAttrs[0].Name;
            }

            ServiceProvider = new XamlObjectServiceProvider(this);
            CreateWrapper();
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
                && ((targetProperty == null)
                    || ((targetProperty != null) && !typeof(TriggerBase).IsAssignableFrom(targetProperty.TargetType) && !typeof(Condition).IsAssignableFrom(targetProperty.TargetType))
                   )
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

        XmlAttribute xmlAttribute;

        internal XmlAttribute XmlAttribute
        {
            get { return xmlAttribute; }
            set
            {
                xmlAttribute = value;
                element = VirualAttachTo(XmlElement, value.OwnerElement);
            }
        }

        static XmlElement VirualAttachTo(XmlElement e, XmlElement target)
        {
            // Fehler in originalcode auskommentiert:
            // var prefix = target.GetPrefixOfNamespace(e.NamespaceURI);
            var prefix = e.GetPrefixOfNamespace(e.NamespaceURI);
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

        internal override void AddNodeTo(XamlProperty property)
        {
            if (!UpdateXmlAttribute(true, property))
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
                if (!UpdateXmlAttribute(false, null))
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
            UpdateXmlAttribute(false, property);
            UpdateMarkupExtensionChain();
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

        bool UpdateXmlAttribute(bool force, XamlProperty changedProperty)
        {
            var holder = FindXmlAttributeHolder();
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
                return ((instance is SetterBase)
                        || (instance is Condition)
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

        string contentPropertyName;

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
            if (propertyInfo != null)
            {
                newProperty = new XamlProperty(this, new XamlNormalPropertyInfo(propertyInfo));
            }
            else
            {
                EventDescriptorCollection events = TypeDescriptor.GetEvents(instance);
                EventDescriptor eventInfo = events[propertyName];
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
            return element.GetAttribute(name, XamlConstants.XamlNamespace);
        }

        /// <summary>
        /// Sets an attribute in the x:-namespace.
        /// </summary>
        public void SetXamlAttribute(string name, string value)
        {
            if (value == null)
                element.RemoveAttribute(name, XamlConstants.XamlNamespace);
            else
                element.SetAttribute(name, XamlConstants.XamlNamespace, value);
        }

        /// <summary>
        /// Gets/Sets the <see cref="XamlObjectServiceProvider"/> associated with this XamlObject.
        /// </summary>
        public XamlObjectServiceProvider ServiceProvider { get; set; }

        MarkupExtensionWrapper wrapper;
        SealedObjectsWrapper sealedObjectWrapper;

        void CreateWrapper()
        {
            if (Instance is Binding)
            {
                wrapper = new BindingWrapper();
            }
            else if (Instance is MultiBinding)
            {
                wrapper = new MultiBindingWrapper();
            }
            else if (Instance is StaticResourceExtension)
            {
                wrapper = new StaticResourceWrapper();
            }
            if (wrapper != null)
            {
                wrapper.XamlObject = this;
            }

            if (instance is SetterBase)
            {
                sealedObjectWrapper = new SetterBaseWrapper();
            }
            else if (instance is Condition)
            {
                sealedObjectWrapper = new ConditionWrapper();
            }
            if (sealedObjectWrapper != null)
                sealedObjectWrapper.XamlObject = this;
        }

        object ProvideValue()
        {
            if (wrapper != null)
            {
                return wrapper.ProvideValue();
            }
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
            return XmlElement.Name;
        }
    }

    abstract class SealedObjectsWrapper
    {
        public XamlObject XamlObject { get; set; }
        public abstract object GetClonedInstance();
    }

    abstract class MarkupExtensionWrapper : SealedObjectsWrapper
    {
        public abstract object ProvideValue();
    }

    class BindingWrapper : MarkupExtensionWrapper
    {
        public override object ProvideValue()
        {
            var target = XamlObject.Instance as Binding;
            //TODO: XamlObject.Clone()
            var b = GetClonedInstance() as Binding;
            object providedValue = null;
            try
            {
                providedValue = b.ProvideValue(XamlObject.ServiceProvider);
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
        public override object ProvideValue()
        {
            var b = GetClonedInstance() as MultiBinding;
            return b.ProvideValue(XamlObject.ServiceProvider);
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

    class SetterBaseWrapper : SealedObjectsWrapper
    {
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

    class ConditionWrapper : SealedObjectsWrapper
    {
        public override object GetClonedInstance()
        {
            var target = XamlObject.Instance as Condition;
            var b = Activator.CreateInstance(XamlObject.Instance.GetType()) as Condition;
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

}
