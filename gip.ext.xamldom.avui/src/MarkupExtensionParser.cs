// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Reflection;
using Avalonia.Markup.Xaml;
using Avalonia.Data;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Tokenizer for markup extension attributes.
	/// [MS-XAML 6.6.7.1]
	/// </summary>
	sealed class MarkupExtensionTokenizer
	{
		private MarkupExtensionTokenizer() {}
		
		string text;
		int pos;
		List<MarkupExtensionToken> tokens = new List<MarkupExtensionToken>();
		
		public static List<MarkupExtensionToken> Tokenize(string text)
		{
			MarkupExtensionTokenizer t = new MarkupExtensionTokenizer();
			t.text = text;
			t.Parse();
			return t.tokens;
		}
		
		void AddToken(MarkupExtensionTokenKind kind, string val)
		{
			tokens.Add(new MarkupExtensionToken(kind, val));
		}
		
		void Parse()
		{
			AddToken(MarkupExtensionTokenKind.OpenBrace, "{");
			Expect('{');
			ConsumeWhitespace();
			CheckNotEOF();
			
			StringBuilder b = new StringBuilder();
			while (pos < text.Length && !char.IsWhiteSpace(text, pos) && text[pos] != '}')
				b.Append(text[pos++]);
			AddToken(MarkupExtensionTokenKind.TypeName, b.ToString());
			
			ConsumeWhitespace();
			while (pos < text.Length) {
				switch (text[pos]) {
					case '}':
						AddToken(MarkupExtensionTokenKind.CloseBrace, "}");
						pos++;
						break;
					case '=':
						AddToken(MarkupExtensionTokenKind.Equals, "=");
						pos++;
						break;
					case ',':
						AddToken(MarkupExtensionTokenKind.Comma, ",");
						pos++;
						break;
					default:
						MembernameOrString();
						break;
				}
				ConsumeWhitespace();
			}
		}

        void MembernameOrString()
        {
            StringBuilder b = new StringBuilder();
            if (text[pos] == '"' || text[pos] == '\'')
            {
                char quote = text[pos++];
                CheckNotEOF();
                int lastBackslashPos = -1;
                while (!(text[pos] == quote && text[pos - 1] != '\\'))
                {
                    int current = pos;
                    char c = text[pos++];
                    //check if string is \\ and that the last backslash is not the previously saved char, ie that \\\\ does not become \\\ but just \\
                    bool isEscapedBackslash = string.Concat(text[current - 1], c) == "\\\\" && current - 1 != lastBackslashPos;
                    if (c != '\\' || isEscapedBackslash)
                    {
                        b.Append(c);
                        if (isEscapedBackslash)
                            lastBackslashPos = current;
                    }
                    CheckNotEOF();
                }
                pos++; // consume closing quote
                ConsumeWhitespace();
            }
            else
            {
                int braceTotal = 0;
                while (true)
                {
                    CheckNotEOF();
                    switch (text[pos])
                    {
                        case '\\':
                            pos++;
                            CheckNotEOF();
                            b.Append(text[pos++]);
                            break;
                        case '{':
                            b.Append(text[pos++]);
                            braceTotal++;
                            break;
                        case '}':
                            if (braceTotal == 0) goto stop;
                            b.Append(text[pos++]);
                            braceTotal--;
                            break;
                        case ',':
                        case '=':
                            if (braceTotal == 0) goto stop;
                            b.Append(text[pos++]);
                            break;
                        default:
                            b.Append(text[pos++]);
                            break;
                    }
                }
            stop:;
            }
            CheckNotEOF();
            string valueText = b.ToString();
            if (text[pos] == '=')
            {
                AddToken(MarkupExtensionTokenKind.Membername, valueText.Trim());
            }
            else
            {
                AddToken(MarkupExtensionTokenKind.String, valueText);
            }
        }

        void Expect(char c)
		{
			CheckNotEOF();
			if (text[pos] != c)
				throw new XamlMarkupExtensionParseException("Expected '" + c + "'");
			pos++;
		}
		
		void ConsumeWhitespace()
		{
			while (pos < text.Length && char.IsWhiteSpace(text, pos))
				pos++;
		}
		
		void CheckNotEOF()
		{
			if (pos >= text.Length)
				throw new XamlMarkupExtensionParseException("Unexpected end of markup extension");
		}
	}
	
	/// <summary>
	/// Exception thrown when XAML loading fails because there is a syntax error in a markup extension.
	/// </summary>
	[Serializable]
	public class XamlMarkupExtensionParseException : XamlLoadException
	{
		/// <summary>
		/// Create a new XamlMarkupExtensionParseException instance.
		/// </summary>
		public XamlMarkupExtensionParseException()
		{
		}
		
		/// <summary>
		/// Create a new XamlMarkupExtensionParseException instance.
		/// </summary>
		public XamlMarkupExtensionParseException(string message) : base(message)
		{
		}
		
		/// <summary>
		/// Create a new XamlMarkupExtensionParseException instance.
		/// </summary>
		public XamlMarkupExtensionParseException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		/// <summary>
		/// Create a new XamlMarkupExtensionParseException instance.
		/// </summary>
		//protected XamlMarkupExtensionParseException(SerializationInfo info, StreamingContext context)
		//	: base(info, context)
		//{
		//}
	}
	
	enum MarkupExtensionTokenKind
	{
		OpenBrace,
		CloseBrace,
		Equals,
		Comma,
		TypeName,
		Membername,
		String
	}
	
	sealed class MarkupExtensionToken
	{
		public readonly MarkupExtensionTokenKind Kind;
		public readonly string Value;
		
		public MarkupExtensionToken(MarkupExtensionTokenKind kind, string value)
		{
			this.Kind = kind;
			this.Value = value;
		}
		
		public override string ToString()
		{
			return "[" + Kind + " " + Value + "]";
		}
	}
	
	/// <summary>
	/// [MS-XAML 6.6.7.2]
	/// </summary>
	static class MarkupExtensionParser
	{
		static bool IsXamlIntrinsicNamespace(string ns)
		{
			return ns == XamlConstants.XamlNamespace || ns == XamlConstants.Xaml2009Namespace;
		}

		static bool TrySplitTypeName(string qualifiedName, out string prefix, out string localName)
		{
			int idx = qualifiedName.IndexOf(':');
			if (idx >= 0)
			{
				prefix = qualifiedName.Substring(0, idx);
				localName = qualifiedName.Substring(idx + 1);
				return true;
			}

			prefix = string.Empty;
			localName = qualifiedName;
			return false;
		}

		static string ResolvePrefixNamespace(XamlObject parent, string prefix)
		{
			var obj = parent;
			while (obj != null)
			{
				var ns = obj.XmlElement.GetNamespaceOfPrefix(prefix);
				if (!string.IsNullOrEmpty(ns))
					return ns;
				obj = obj.ParentObject;
			}

			return null;
		}

		static string ResolveArgumentOrValue(string extension, string name, List<string> positionalArgs, List<KeyValuePair<string, string>> namedArgs)
		{
			string value = null;

			if (positionalArgs.Count == 1 && namedArgs.Count == 0)
				value = positionalArgs[0];
			else if (positionalArgs.Count == 0 && namedArgs.Count == 1 && string.Equals(namedArgs[0].Key, name, StringComparison.Ordinal))
				value = namedArgs[0].Value;

			if (value == null)
				throw new XamlMarkupExtensionParseException(
					extension + " extension should take exactly one constructor parameter without any content OR " + name + " property");

			return value;
		}

		static XamlObject ParseXamlIntrinsicExtension(
			XamlObject parent,
			XmlAttribute attribute,
			string intrinsicName,
			List<string> positionalArgs,
			List<KeyValuePair<string, string>> namedArgs)
		{
			if (intrinsicName == "Null")
				return (XamlObject)parent.OwnerDocument.CreateNullValue();
			if (intrinsicName == "True")
				return parent.OwnerDocument.CreateObject(true);
			if (intrinsicName == "False")
				return parent.OwnerDocument.CreateObject(false);

			var typeResolver = parent.ServiceProvider.Resolver;

			if (intrinsicName == "Type")
			{
				var typeName = ResolveArgumentOrValue("x:Type", "TypeName", positionalArgs, namedArgs).Trim();
				var resolvedType = typeResolver.Resolve(typeName);
				if (resolvedType == null)
					throw new XamlMarkupExtensionParseException("Unable to resolve type " + typeName);
				return parent.OwnerDocument.CreateObject(resolvedType);
			}

			if (intrinsicName == "Static")
			{
				var memberRef = ResolveArgumentOrValue("x:Static", "Member", positionalArgs, namedArgs).Trim();

				string nsPrefix = string.Empty;
				string typeAndMember = memberRef;
				var nsp = memberRef.Split(new[] { ':' }, 2);
				if (nsp.Length == 2)
				{
					nsPrefix = nsp[0];
					typeAndMember = nsp[1];
				}

				var pair = typeAndMember.Split(new[] { '.' }, 2);
				if (pair.Length != 2)
					throw new XamlMarkupExtensionParseException("Unable to parse " + typeAndMember + " as 'type.member'");

				var typeName = string.IsNullOrEmpty(nsPrefix) ? pair[0] : nsPrefix + ":" + pair[0];
				var declaringType = typeResolver.Resolve(typeName);
				if (declaringType == null)
					throw new XamlMarkupExtensionParseException("Unable to resolve type " + typeName + " for x:Static");

				var memberName = pair[1];
				var flags = BindingFlags.Public | BindingFlags.Static;
				var prop = declaringType.GetProperty(memberName, flags);
				if (prop != null)
					return parent.OwnerDocument.CreateObject(prop.GetValue(null, null));

				var field = declaringType.GetField(memberName, flags);
				if (field != null)
					return parent.OwnerDocument.CreateObject(field.GetValue(null));

				throw new XamlMarkupExtensionParseException("Unable to resolve static member " + memberRef);
			}

			throw new XamlMarkupExtensionParseException("Unknown intrinsic markup extension x:" + intrinsicName);
		}

		static bool TryParseRelativeSourceEnum<TEnum>(string text, out TEnum value) where TEnum : struct
		{
			if (Enum.TryParse<TEnum>(text, true, out value))
				return true;

			// Common legacy alias used in WPF XAML.
			if (typeof(TEnum) == typeof(RelativeSourceMode)
				&& string.Equals(text, "Ancestor", StringComparison.OrdinalIgnoreCase))
			{
				value = (TEnum)(object)RelativeSourceMode.FindAncestor;
				return true;
			}

			return false;
		}

		static XamlObject ParseRelativeSourceExtension(
			XamlObject parent,
			XmlAttribute attribute,
			List<string> positionalArgs,
			List<KeyValuePair<string, string>> namedArgs)
		{
			var rs = new RelativeSource();

			if (positionalArgs.Count > 1)
				throw new XamlMarkupExtensionParseException("RelativeSource extension takes at most one positional argument");

			if (positionalArgs.Count == 1)
			{
				if (!TryParseRelativeSourceEnum(positionalArgs[0], out RelativeSourceMode mode))
					throw new XamlMarkupExtensionParseException("Invalid RelativeSource mode " + positionalArgs[0]);
				rs.Mode = mode;
			}

			var resolver = parent.ServiceProvider.Resolver;
			foreach (var pair in namedArgs)
			{
				if (string.Equals(pair.Key, "Mode", StringComparison.OrdinalIgnoreCase))
				{
					if (!TryParseRelativeSourceEnum(pair.Value, out RelativeSourceMode mode))
						throw new XamlMarkupExtensionParseException("Invalid RelativeSource mode " + pair.Value);
					rs.Mode = mode;
				}
				else if (string.Equals(pair.Key, "AncestorLevel", StringComparison.OrdinalIgnoreCase))
				{
					rs.AncestorLevel = int.Parse(pair.Value, CultureInfo.InvariantCulture);
				}
				else if (string.Equals(pair.Key, "AncestorType", StringComparison.OrdinalIgnoreCase))
				{
					var t = resolver.Resolve(pair.Value);
					if (t == null)
						throw new XamlMarkupExtensionParseException("Unable to resolve type " + pair.Value + " for RelativeSource.AncestorType");
					rs.AncestorType = t;
				}
				else if (string.Equals(pair.Key, "Tree", StringComparison.OrdinalIgnoreCase))
				{
					if (!TryParseRelativeSourceEnum(pair.Value, out TreeType treeType))
						throw new XamlMarkupExtensionParseException("Invalid RelativeSource tree type " + pair.Value);
					rs.Tree = treeType;
				}
				else
				{
					throw new XamlMarkupExtensionParseException("Unknown RelativeSource argument " + pair.Key);
				}
			}

			var result = parent.OwnerDocument.CreateObject(rs);
			if (attribute != null)
				result.XmlAttribute = attribute;
			result.ParentObject = parent;
			return result;
		}

		static bool IsSupportedExtensionType(Type extensionType)
		{
			if (extensionType == null)
				return false;

			if (typeof(MarkupExtension).IsAssignableFrom(extensionType)
				|| typeof(Avalonia.Data.Converters.IValueConverter).IsAssignableFrom(extensionType)
				|| typeof(Binding).IsAssignableFrom(extensionType))
			{
				return true;
			}

			var provideValue = extensionType.GetMethod(
				"ProvideValue",
				BindingFlags.Public | BindingFlags.Instance,
				null,
				new[] { typeof(IServiceProvider) },
				null);

			return provideValue != null;
		}

		public static XamlObject Parse(string text, XamlObject parent, XmlAttribute attribute)
		{
			var tokens = MarkupExtensionTokenizer.Tokenize(text);
			if (tokens.Count < 3
			    || tokens[0].Kind != MarkupExtensionTokenKind.OpenBrace
			    || tokens[1].Kind != MarkupExtensionTokenKind.TypeName
			    || tokens[tokens.Count-1].Kind != MarkupExtensionTokenKind.CloseBrace)
			{
				throw new XamlMarkupExtensionParseException("Invalid markup extension");
			}
			
			string typeName = tokens[1].Value;

			List<string> positionalArgs = new List<string>();
			List<KeyValuePair<string, string>> namedArgs = new List<KeyValuePair<string, string>>();
			for (int i = 2; i < tokens.Count - 1; i++) {
				if (tokens[i].Kind == MarkupExtensionTokenKind.String) {
					positionalArgs.Add(tokens[i].Value);
				} else if (tokens[i].Kind == MarkupExtensionTokenKind.Membername) {
					if (tokens[i+1].Kind != MarkupExtensionTokenKind.Equals
					    || tokens[i+2].Kind != MarkupExtensionTokenKind.String)
					{
						throw new XamlMarkupExtensionParseException("Invalid markup extension");
					}
					namedArgs.Add(new KeyValuePair<string, string>(tokens[i].Value, tokens[i+2].Value));
					i += 2;
				}
			}

			TrySplitTypeName(typeName, out var typePrefix, out var typeLocalName);
			if (string.Equals(typeName, "RelativeSource", StringComparison.Ordinal)
				|| string.Equals(typeName, "RelativeSourceExtension", StringComparison.Ordinal))
			{
				return ParseRelativeSourceExtension(parent, attribute, positionalArgs, namedArgs);
			}

			if (!string.IsNullOrEmpty(typePrefix))
			{
				var ns = ResolvePrefixNamespace(parent, typePrefix);
				if (IsXamlIntrinsicNamespace(ns))
				{
					var intrinsic = ParseXamlIntrinsicExtension(parent, attribute, typeLocalName, positionalArgs, namedArgs);
					if (attribute != null)
						intrinsic.XmlAttribute = attribute;
					intrinsic.ParentObject = parent;
					return intrinsic;
				}
			}

			var typeResolver = parent.ServiceProvider.Resolver;

            Type extensionType = null;
            try
            {
                if (typeName.EndsWith("Extension"))
                    extensionType = typeResolver.Resolve(typeName);
                else
                    extensionType = typeResolver.Resolve(typeName + "Extension");
            }
            catch (XamlMarkupExtensionParseException /*e*/)
            {
            }
			if (extensionType == null) 
                extensionType = typeResolver.Resolve(typeName);
			if (!IsSupportedExtensionType(extensionType))
            {
				throw new XamlMarkupExtensionParseException("Unknown markup extension " + typeName + "Extension");
			}
			
			// Find the constructor with positionalArgs.Count arguments (considering optional parameters)
			var ctors = extensionType.GetConstructors()
				.Where(c => {
					var parameters = c.GetParameters();
					int requiredParams = parameters.Count(p => !p.IsOptional);
					int totalParams = parameters.Length;
					return positionalArgs.Count >= requiredParams && positionalArgs.Count <= totalParams;
				})
				.OrderBy(c => c.GetParameters().Length) // Prefer constructor with fewer parameters
				.ToList();

			if (ctors.Count < 1)
				throw new XamlMarkupExtensionParseException("No constructor for " + 
					extensionType.FullName + " found that takes " + positionalArgs.Count + " arguments");
			if (ctors.Count > 1) {
				Debug.WriteLine("Multiple constructors for " + 
					extensionType.FullName + " found that take " + positionalArgs.Count + " arguments");
			}

			var ctor = ctors[0];
			var defaultCtor = extensionType.GetConstructor(Type.EmptyTypes);
			bool mappingToProperties = defaultCtor != null;
			List<PropertyInfo> map = new List<PropertyInfo>();

			if (mappingToProperties) {
				var ctorParams = ctor.GetParameters();
				for (int i = 0; i < positionalArgs.Count; i++) {
					var prop = extensionType.GetProperty(ctorParams[i].Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (prop == null) {
						mappingToProperties = false;
						break;
					}
					map.Add(prop);
				}
			}

			object instance;
			if (mappingToProperties) {
				instance = defaultCtor.Invoke(null);
			} else {
				var ctorParamsInfo = ctor.GetParameters();
				var ctorParams = new object[ctorParamsInfo.Length];
				for (int i = 0; i < positionalArgs.Count; i++) {
					var paramType = ctorParamsInfo[i].ParameterType;
					ctorParams[i] = XamlParser.CreateObjectFromAttributeText(positionalArgs[i], paramType, parent);
				}
				// Fill remaining optional parameters with their default values
				for (int i = positionalArgs.Count; i < ctorParamsInfo.Length; i++) {
					ctorParams[i] = ctorParamsInfo[i].DefaultValue;
				}
				instance = ctor.Invoke(ctorParams);
				//TODO
				//XamlObject.ConstructorArgsProperty - args collection
				//Reinvoke ctor when needed
			}
				
			XamlObject result = parent.OwnerDocument.CreateObject(instance);
			if (attribute != null) result.XmlAttribute = attribute;
			result.ParentObject = parent;

			if (mappingToProperties) {
				for (int i = 0; i < positionalArgs.Count; i++) {
					var a = parent.OwnerDocument.XmlDocument.CreateAttribute(map[i].Name);
					a.Value = positionalArgs[i];
					XamlParser.ParseObjectAttribute(result, a, false);
				}
			}
			foreach (var pair in namedArgs) {
				var a = parent.OwnerDocument.XmlDocument.CreateAttribute(pair.Key);
				a.Value = pair.Value;
				XamlParser.ParseObjectAttribute(result, a, false);
			}
			return result;
		}
	}
}
