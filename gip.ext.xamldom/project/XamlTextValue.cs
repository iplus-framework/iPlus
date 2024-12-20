﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Markup;
using System.Xml;

namespace gip.ext.xamldom
{
	/// <summary>
	/// A textual value in a .xaml file.
	/// </summary>
	public sealed class XamlTextValue : XamlPropertyValue
	{
		XamlDocument document;
		XmlAttribute attribute;
		XmlText textNode;
		XmlSpace xmlSpace;
		string textValue;
		XmlCDataSection cDataSection;
		
		internal XamlTextValue(XamlDocument document, XmlAttribute attribute)
		{
			this.document = document;
			this.attribute = attribute;
		}
		
		internal XamlTextValue(XamlDocument document, string textValue)
		{
			this.document = document;
			this.textValue = textValue;
		}
		
		internal XamlTextValue(XamlDocument document, XmlText textNode, XmlSpace xmlSpace)
		{
			this.document = document;
			this.xmlSpace = xmlSpace;
			this.textNode = textNode;
		}
		
		internal XamlTextValue(XamlDocument document, XmlCDataSection cDataSection, XmlSpace xmlSpace)
		{
			this.document = document;
			this.xmlSpace = xmlSpace;
			this.cDataSection = cDataSection;
		}
		
		/// <summary>
		/// The text represented by the value.
		/// </summary>
		public string Text {
			get {
				if (attribute != null) {
					if (attribute.Value.StartsWith("{}", StringComparison.Ordinal))
						return attribute.Value.Substring(2);
					else
						return attribute.Value;
				} else if (textValue != null)
					return textValue;
				else if (cDataSection != null)
					return cDataSection.Value;
				else
					return NormalizeWhitespace(textNode.Value);
			}
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				
				if (attribute != null)
					attribute.Value = value;
				else if (textValue != null)
					textValue = value;
				else if (cDataSection != null)
					cDataSection.Value = value;
				else
					textNode.Value = value;
			}
		}
		
		string NormalizeWhitespace(string text)
		{
			if (xmlSpace == XmlSpace.Preserve) {
				return text.Replace("\r", "");
			}
			StringBuilder b = new StringBuilder();
			bool wasWhitespace = true;
			foreach (char c in text) {
				if (char.IsWhiteSpace(c)) {
					if (!wasWhitespace) {
						b.Append(' ');
					}
					wasWhitespace = true;
				} else {
					wasWhitespace = false;
					b.Append(c);
				}
			}
			if (b.Length > 0 && wasWhitespace)
				b.Length -= 1;
			return b.ToString();
		}
		
		internal override object GetValueFor(XamlPropertyInfo targetProperty)
		{
			if (ParentProperty == null)
				throw new InvalidOperationException("Cannot call GetValueFor while ParentProperty is null");
			
			if (targetProperty == null)
				return this.Text;

			return XamlParser.CreateObjectFromAttributeText(Text, targetProperty, ParentProperty.ParentObject);
		}
		
		internal override void RemoveNodeFromParent()
		{
			if (attribute != null && attribute.OwnerElement != null)
				attribute.OwnerElement.RemoveAttribute(attribute.Name);
			else if (textNode != null)
				textNode.ParentNode.RemoveChild(textNode);
			else if (cDataSection != null)
				cDataSection.ParentNode.RemoveChild(cDataSection);
		}
		
		internal override void AddNodeTo(XamlProperty property)
		{
			if (attribute != null) {
				property.ParentObject.XmlElement.Attributes.Append(attribute);
			} else if (textValue != null) {
				attribute = property.SetAttribute(textValue);
				textValue = null;
			} else if (cDataSection != null) {
				property.AddChildNodeToProperty(cDataSection);
			} else {
				property.AddChildNodeToProperty(textNode);
			}
		}
		
		internal override XmlNode GetNodeForCollection()
		{
			if (textNode != null)
				return textNode;
			else if (cDataSection != null)
				return cDataSection;
			else
				throw new NotImplementedException();
		}
	}
}
