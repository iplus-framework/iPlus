// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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
                // Xaml.Behaviors animations can specify x:SetterTargetType (e.g. Canvas) so
                // Setter.Property resolution works outside ControlTheme contexts.
                var setterTargetTypeName = obj.XmlElement.GetAttribute("SetterTargetType", XamlConstants.XamlNamespace);
                if (string.IsNullOrEmpty(setterTargetTypeName))
                    setterTargetTypeName = obj.XmlElement.GetAttribute("SetterTargetType", XamlConstants.Xaml2009Namespace);
                if (string.IsNullOrEmpty(setterTargetTypeName))
                    setterTargetTypeName = obj.XmlElement.GetAttribute("SetterTargetType");
                if (!string.IsNullOrEmpty(setterTargetTypeName))
                {
                    var resolvedSetterTargetType = TryResolveTypeReference(setterTargetTypeName);
                    if (resolvedSetterTargetType != null)
                    {
                        elementType = resolvedSetterTargetType;
                        break;
                    }
                }

                ControlTheme style = obj.Instance as ControlTheme;
                if (style != null && style.TargetType != null)
                {
                    elementType = style.TargetType;
                    break;
                }

                Style normalStyle = obj.Instance as Style;
                if (normalStyle != null)
                {
                    var selectorTargetType = TryGetSelectorTargetType(normalStyle.Selector);
                    if (selectorTargetType == null)
                    {
                        var selectorProperty = obj.FindProperty("Selector");
                        var selectorText = selectorProperty?.TextValueOnInstance;
                        selectorTargetType = TryGetSelectorTargetTypeFromText(selectorText);
                    }

                    if (selectorTargetType != null)
                    {
                        elementType = selectorTargetType;
                        break;
                    }
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

        private Type TryResolveTypeReference(string typeReference)
        {
            if (string.IsNullOrWhiteSpace(typeReference))
                return null;

            var normalized = NormalizeTypeReference(typeReference);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            try
            {
                return Resolve(normalized);
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeTypeReference(string typeReference)
        {
            var normalized = typeReference.Trim();
            if (normalized.Length == 0)
                return normalized;

            // Handle markup-extension syntax such as "{x:Type Canvas}".
            if (normalized.StartsWith("{", StringComparison.Ordinal)
                && normalized.EndsWith("}", StringComparison.Ordinal)
                && normalized.IndexOf("x:Type", StringComparison.Ordinal) >= 0)
            {
                var markerIndex = normalized.IndexOf("x:Type", StringComparison.Ordinal);
                if (markerIndex >= 0)
                {
                    normalized = normalized.Substring(markerIndex + "x:Type".Length)
                        .Trim()
                        .TrimEnd('}')
                        .Trim();
                }
            }

            // Avalonia selector-style type tokens can use prefix|Type.
            if (normalized.IndexOf('|') >= 0 && normalized.IndexOf(':') < 0)
                normalized = normalized.Replace('|', ':');

            return normalized;
        }

        private static Type TryGetSelectorTargetType(Selector selector)
        {
            if (selector == null)
                return null;

            try
            {
                var targetTypeProperty = typeof(Selector).GetProperty(
                    "TargetType",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                return targetTypeProperty?.GetValue(selector) as Type;
            }
            catch
            {
                return null;
            }
        }

        private Type TryGetSelectorTargetTypeFromText(string selectorText)
        {
            if (string.IsNullOrWhiteSpace(selectorText))
                return null;

            var parserType = Type.GetType("Avalonia.Markup.Parsers.SelectorParser, Avalonia.Markup", throwOnError: false);
            if (parserType == null)
                return null;

            try
            {
                Func<string, string, Type> typeResolver = (xmlNsPrefix, typeName) =>
                {
                    var qualifiedName = string.IsNullOrEmpty(xmlNsPrefix)
                        ? typeName
                        : xmlNsPrefix + ":" + typeName;
                    return Resolve(qualifiedName);
                };

                var ctor = parserType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(Func<string, string, Type>) },
                    modifiers: null);
                if (ctor == null)
                    return null;

                var parser = ctor.Invoke(new object[] { typeResolver });
                var parseMethod = parserType.GetMethod(
                    "Parse",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(string) },
                    modifiers: null);
                if (parseMethod == null)
                    return null;

                var selector = parseMethod.Invoke(parser, new object[] { selectorText }) as Selector;
                return TryGetSelectorTargetType(selector);
            }
            catch
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
