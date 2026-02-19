// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace gip.ext.xamldom.avui
{
    sealed class XamlTypeResolverProvider : IXamlTypeResolver, IServiceProvider
    {
        XamlDocument document;
        XamlObject containingObject;

        public XamlTypeResolverProvider(XamlObject containingObject)
        {
            if (containingObject == null)
                throw new ArgumentNullException("containingObject");
            this.document = containingObject.OwnerDocument;
            this.containingObject = containingObject;
        }

        XmlElement ContainingElement
        {
            get { return containingObject.XmlElement; }
        }

        private string GetNamespaceOfPrefix(string prefix)
        {
            var ns = ContainingElement.GetNamespaceOfPrefix(prefix);
            if (!string.IsNullOrEmpty(ns))
                return ns;
            var obj = containingObject;
            while (obj != null)
            {
                ns = obj.XmlElement.GetNamespaceOfPrefix(prefix);
                if (!string.IsNullOrEmpty(ns))
                    return ns;
                obj = obj.ParentObject;
            }
            return null;
        }

        public Type Resolve(string typeName)
        {
            Type result = null;
            string typeNamespaceUri;
            string typeLocalName;
            if (typeName.Contains(":"))
            {
                string prefix = typeName.Substring(0, typeName.IndexOf(':'));
                typeNamespaceUri = GetNamespaceOfPrefix(typeName.Substring(0, typeName.IndexOf(':')));
                typeLocalName = typeName.Substring(typeName.IndexOf(':') + 1);
                if (String.IsNullOrEmpty(typeNamespaceUri))
                {
                    result = document.TypeFinder.GetTypeOverPrefix(prefix, typeLocalName);
                    if (result != null)
                        return result;
                }
            }
            else
            {
                if (containingObject.ParentObject != null)
                {
                    typeNamespaceUri = containingObject.ParentObject.XmlElement.GetNamespaceOfPrefix("");
                    typeLocalName = typeName;
                }
                else
                {
                    typeNamespaceUri = GetNamespaceOfPrefix("");
                    typeLocalName = typeName;
                }
            }
            if (string.IsNullOrEmpty(typeNamespaceUri))
            {
                var documentResolver = this.document.RootElement.ServiceProvider.Resolver;
                if (documentResolver != null && documentResolver != this)
                {
                    return documentResolver.Resolve(typeName);
                }
                throw new XamlMarkupExtensionParseException("Unrecognized namespace prefix in type " + typeName);
            }
            result = document.TypeFinder.GetType(typeNamespaceUri, typeLocalName);
            return result;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IXamlTypeResolver) || serviceType == typeof(XamlTypeResolverProvider))
                return this;
            else
                return document.ServiceProvider.GetService(serviceType);
        }

        public XamlPropertyInfo ResolveProperty(string propertyName)
        {
            string propertyNamespace;
            if (propertyName.Contains(":"))
            {
                propertyNamespace = ContainingElement.GetNamespaceOfPrefix(propertyName.Substring(0, propertyName.IndexOf(':')));
                propertyName = propertyName.Substring(propertyName.IndexOf(':') + 1);
            }
            else
            {
                propertyNamespace = ContainingElement.GetNamespaceOfPrefix("");
            }
            Type elementType = null;
            XamlObject obj = containingObject;
            while (obj != null)
            {
                ControlTheme style = obj.Instance as ControlTheme;
                if (style != null && style.TargetType != null)
                {
                    elementType = style.TargetType;
                    break;
                }
                obj = obj.ParentObject;
            }
            if (propertyName.Contains("."))
            {
                return XamlParser.GetPropertyInfo(document.TypeFinder, null, elementType, propertyNamespace, propertyName);
            }
            else if (elementType != null)
            {
                XamlPropertyInfo info = null;
                try
                {
                    info = XamlParser.FindProperty(null, elementType, null, propertyName);
                }
                catch (Exception)
                {
                }
                return info;
            }
            else
            {
                return null;
            }
        }

        public object FindResource(object key)
        {
            XamlObject obj = containingObject;
            while (obj != null)
            {
                Control el = obj.Instance as Control;
                if (el != null)
                {
                    object val = el.Resources[key];
                    if (val != null)
                        return val;
                }
                obj = obj.ParentObject;
            }
            return null;
        }

        public object FindLocalResource(object key)
        {
            Control el = containingObject.Instance as Control;
            if (el != null)
            {
                return el.Resources[key];
            }
            return null;
        }
    }
}
