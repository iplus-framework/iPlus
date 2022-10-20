// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace gip.ext.xamldom
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

        public Type Resolve(string typeName)
        {
            Type result = null;
            string typeNamespaceUri;
            string typeLocalName;
            if (typeName.Contains(":"))
            {
                string prefix = typeName.Substring(0, typeName.IndexOf(':'));
                typeNamespaceUri = ContainingElement.GetNamespaceOfPrefix(prefix);
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
                    typeNamespaceUri = ContainingElement.GetNamespaceOfPrefix("");
                    typeLocalName = typeName;
                }
            }
            if (string.IsNullOrEmpty(typeNamespaceUri))
                throw new XamlMarkupExtensionParseException("Unrecognized namespace prefix in type " + typeName);
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
                Style style = obj.Instance as Style;
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
                    info = XamlParser.FindProperty(null, elementType, propertyName);
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
                FrameworkElement el = obj.Instance as FrameworkElement;
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
    }
}
