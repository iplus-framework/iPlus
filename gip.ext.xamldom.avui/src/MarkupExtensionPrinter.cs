// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// Static class that can generate XAML markup extension code ("{Binding Path=...}").
    /// </summary>
    public static class MarkupExtensionPrinter
    {
        /// <summary>
        /// Gets whether shorthand XAML markup extension code can be generated for the object.
        /// </summary>
        public static bool CanPrint(XamlObject obj, XamlProperty changedProperty)
        {
            // Multibindings must always be printed in Property-Element-Syntax
            if (typeof(MultiBinding).IsAssignableFrom(obj.ElementType))
                return false;
            //var value = changedProperty.PropertyValue;
            //if (value is XamlTextValue)
            //{
            //    if ((value as XamlTextValue).Text.Length > 0)
            //    {
            //        if (!(value as XamlTextValue).Text.IsMarkupPrintable())
            //            return false;
            //    }
            //}
            return CanPrint(obj, false, GetNonMarkupExtensionParent(obj));
        }

        public static bool IsMarkupPrintable(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;
            foreach (char c in value)
            {
                // "!" 0x21
                // "(" 0x28
                // ")" 0x29
                // "/" 0x2F
                // "0" 0x30 - "9" 0x39
                // "?" 0x3F - "Z" 0x5A
                // "\" 0x5C
                // "a" 0x61 - "z" 0x7A
                if (!((c == '!')
                        || (c == '(')
                        || (c == ')')
                        || (c == '/')
                        || (c == '_')
                        || (c == ' ')
                        || (c >= '0' && c <= '9')
                        || (c >= '?' && c <= 'Z')
                        || (c == '\\')
                        || (c >= 'a' && c <= 'z')))
                {
                    return false;
                }
            }

            return true;
        }

        public static string EscapeMarkupValueString(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                sb.Append("&#x" + Convert.ToInt32(c).ToString("X") + ";");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates XAML markup extension code for the object.
        /// </summary>
        public static string Print(XamlObject obj, XamlProperty changedProperty)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(obj.GetNameForMarkupExtension());

            bool first = true;
            var properties = obj.Properties.ToList();

            if (obj.ElementType == typeof(Binding))
            {
                var p = obj.Properties.FirstOrDefault(x => x.PropertyName == "Path");
                if (p != null && p.IsSet)
                {
                    sb.Append(" ");
                    AppendPropertyValue(sb, p.PropertyValue, false);
                    properties.Remove(p);
                    first = false;
                }
            }
            // doesn't exist in Avalonia:
            //else if (obj.ElementType == typeof(Reference))
            //{
            //    var p = obj.Properties.FirstOrDefault(x => x.PropertyName == "Name");
            //    if (p != null && p.IsSet)
            //    {
            //        sb.Append(" ");
            //        AppendPropertyValue(sb, p.PropertyValue, false);
            //        properties.Remove(p);
            //        first = false;
            //    }
            //}
            else if (obj.ElementType == typeof(StaticResourceExtension))
            {
                var p = obj.Properties.FirstOrDefault(x => x.PropertyName == "ResourceKey");
                if (p != null && p.IsSet)
                {
                    sb.Append(" ");
                    AppendPropertyValue(sb, p.PropertyValue, false);
                    properties.Remove(p);
                    first = false;
                }
            }

            foreach (var property in properties)
            {
                if (!property.IsSet) continue;

                if (first)
                    sb.Append(" ");
                else
                    sb.Append(", ");
                first = false;

                sb.Append(property.GetNameForMarkupExtension());
                sb.Append("=");

                AppendPropertyValue(sb, property.PropertyValue, property.ReturnType == typeof(string));
            }
            sb.Append("}");
            return sb.ToString();
        }

        //public static string Print(XamlObject obj, XamlProperty changedProperty)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("{");
        //    sb.Append(obj.GetNameForMarkupExtension());

        //    bool bTypeNameProp = false;
        //    if (obj.Instance is System.Windows.Markup.TypeExtension)
        //        bTypeNameProp = obj.Properties.Where(c => c.PropertyName == "TypeName").Any() ? true : false;

        //    bool first = true;
        //    foreach (var property in obj.Properties)
        //    {
        //        if (!property.IsSet) continue;
        //        var value = property.PropertyValue;
        //        if (value is XamlTextValue)
        //        {
        //            if ((value as XamlTextValue).Text.Length <= 0)
        //                continue;
        //        }
        //        else if ((value is XamlObject)
        //            && (obj.Instance is System.Windows.Markup.TypeExtension)
        //            && property.PropertyName == "Type"
        //            && value is XamlObject
        //            && ((value as XamlObject).Instance != null))
        //        {
        //            if (bTypeNameProp)
        //                continue;
        //            string prefix;
        //            string snamespace = obj.OwnerDocument.GetNamespaceFor(((value as XamlObject).Instance as Type), out prefix);
        //            string xName;
        //            if (!String.IsNullOrEmpty(prefix))
        //                xName = prefix + ":" + ((value as XamlObject).Instance as Type).Name;
        //            else
        //                xName = ((value as XamlObject).Instance as Type).Name;
        //            if (first)
        //            {
        //                sb.Append(" " + xName);
        //                first = false;
        //                continue;
        //            }
        //            else
        //            {
        //                sb.Append(", TypeName=" + xName);
        //                continue;
        //            }
        //        }

        //        if (first)
        //            sb.Append(" ");
        //        else
        //            sb.Append(", ");
        //        first = false;

        //        sb.Append(property.GetNameForMarkupExtension());
        //        sb.Append("=");

        //        if (value is XamlTextValue)
        //        {
        //            if ((value as XamlTextValue).Text.IsMarkupPrintable())
        //            {
        //                string doubleBackslash = (value as XamlTextValue).Text.Replace("\\", "\\\\");
        //                sb.Append(doubleBackslash);
        //            }
        //            else
        //            {
        //                //string doubleBackslash = (value as XamlTextValue).Text.Replace("\\", "\\\\");
        //                sb.Append("'" + (value as XamlTextValue).Text + "'");
        //                //sb.Append("'" + (value as XamlTextValue).Text.EscapeMarkupValueString() + "'");
        //            }
        //        }
        //        else if (value is XamlObject)
        //        {
        //            sb.Append(Print(value as XamlObject, changedProperty));
        //        }
        //    }
        //    sb.Append("}");
        //    return sb.ToString();
        //}

        private static void AppendPropertyValue(StringBuilder sb, XamlPropertyValue value, bool isStringProperty)
        {
            var textValue = value as XamlTextValue;
            if (textValue != null)
            {
                string text = textValue.Text;
                bool containsSpace = text.Contains(' ');

                if (containsSpace)
                {
                    sb.Append('\'');
                }

                if (isStringProperty)
                    sb.Append(text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}"));
                else
                    sb.Append(text.Replace("\\", "\\\\"));

                if (containsSpace)
                {
                    sb.Append('\'');
                }
            }
            else if (value is XamlObject)
            {
                sb.Append(Print(value as XamlObject, null));
            }
        }

        private static bool CanPrint(XamlObject obj, bool isNested, XamlObject nonMarkupExtensionParent)
        {
            if ((isNested || obj.ParentObject == nonMarkupExtensionParent) && IsStaticResourceThatReferencesLocalResource(obj, nonMarkupExtensionParent))
            {
                return false;
            }

            foreach (var property in obj.Properties.Where((prop) => prop.IsSet))
            {
                var value = property.PropertyValue;
                if (value is XamlTextValue)
                    continue;
                else
                {
                    var xamlObject = value as XamlObject;
                    if (xamlObject == null || !xamlObject.IsMarkupExtension)
                        return false;
                    else if (!CanPrint(xamlObject, true, nonMarkupExtensionParent))
                        return false;
                }
            }

            return true;
        }

        private static XamlObject GetNonMarkupExtensionParent(XamlObject markupExtensionObject)
        {
            System.Diagnostics.Debug.Assert(markupExtensionObject.IsMarkupExtension);

            XamlObject obj = markupExtensionObject;
            while (obj != null && obj.IsMarkupExtension)
            {
                obj = obj.ParentObject;
            }
            return obj;
        }

        private static bool IsStaticResourceThatReferencesLocalResource(XamlObject obj, XamlObject nonMarkupExtensionParent)
        {
            var staticResource = obj.Instance as StaticResourceExtension;
            if (staticResource != null && staticResource.ResourceKey != null && nonMarkupExtensionParent != null)
            {

                var parentLocalResource = nonMarkupExtensionParent.ServiceProvider.Resolver.FindLocalResource(staticResource.ResourceKey);

                // If resource with the specified key is declared locally on the same object as the StaticResource is being used the markup extension
                // must be printed as element to find the resource, otherwise it will search from parent-parent and find none or another resource.
                if (parentLocalResource != null)
                    return true;
            }

            return false;
        }
    }
}

