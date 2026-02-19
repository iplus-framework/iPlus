using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using gip.ext.xamldom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;

namespace gip.core.layoutengine.CodeCompletion
{
    /// <summary>
    /// Reflection-based XML completion provider for XAML editing.
    /// Uses runtime type information instead of XSD schemas.
    /// </summary>
    public class ReflectionBasedXmlCompletionProvider : ICompletionDataProvider
    {
        private readonly XamlTypeFinder _typeFinder;
        private Dictionary<string, string> _namespaceCache;

        public ReflectionBasedXmlCompletionProvider()
        {
            _typeFinder = XamlTypeFinder.CreateWpfTypeFinder();
            _namespaceCache = new Dictionary<string, string>();
            
            // Register common assemblies
            RegisterCommonAssemblies();
        }

        /// <summary>
        /// Register additional assemblies for completion
        /// </summary>
        public void RegisterAssembly(Assembly assembly)
        {
            _typeFinder.RegisterAssembly(assembly, withPrefixes: true);
        }

        private void RegisterCommonAssemblies()
        {
            // The XamlTypeFinder already registers core WPF assemblies
            // Add custom assemblies here if needed
            try
            {
                // Try to register iPlus custom controls
                var currentAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && 
                               (a.FullName.Contains("gip.") || 
                                a.FullName.Contains("iplus") ||
                                a.FullName.Contains("PresentationFramework") ||
                                a.FullName.Contains("PresentationCore") ||
                                a.FullName.Contains("WindowsBase")));
                
                foreach (var assembly in loadedAssemblies)
                {
                    try
                    {
                        _typeFinder.RegisterAssembly(assembly, withPrefixes: true);
                    }
                    catch
                    {
                        // Skip assemblies that can't be registered
                    }
                }
            }
            catch
            {
                // Continue with core assemblies only
            }
        }

        #region ICompletionDataProvider Members

        private ImageList _imageList;

        public ImageList ImageList
        {
            get
            {
                if (_imageList == null)
                {
                    _imageList = new ImageList();
                }
                return _imageList;
            }
        }

        public int DefaultIndex => 0;

        public string PreSelection => null;

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            return CompletionDataProviderKeyResult.NormalKey;
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            return false;
        }

        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            string text = string.Concat(textArea.Document.GetText(0, textArea.Caret.Offset), charTyped);
            var declaredNamespaces = ParseDeclaredNamespaces(text);

            switch (charTyped)
            {
                case '<':
                    return GetElementCompletionData(text, declaredNamespaces);

                case ':':
                    // fileName parameter contains the prefix when colon is typed (e.g., "vb" from "<vb:")
                    return GetElementCompletionDataForPrefix(text, declaredNamespaces, fileName);

                case ' ':
                    return GetAttributeCompletionData(text);

                case '\'':
                case '\"':
                    return GetAttributeValueCompletionData(text, charTyped);

                default:
                    if (char.IsLetter(charTyped))
                    {
                        // Could be continuing an attribute name
                        if (!XmlParser.IsInsideAttributeValue(text, text.Length))
                        {
                            return GetAttributeCompletionData(text);
                        }
                    }
                    break;
            }

            return null;
        }

        #endregion

        #region Element Completion

        private ICompletionData[] GetElementCompletionData(string text, Dictionary<string, string> declaredNamespaces)
        {
            var completionData = new List<ICompletionData>();
            var addedElements = new HashSet<string>();

            // Get parent element context
            XmlElementPath parentPath = XmlParser.GetParentElementPath(text);
            Type parentType = null;

            if (parentPath.Elements.Count > 0)
            {
                var parentElement = parentPath.Elements[parentPath.Elements.Count - 1];
                parentType = _typeFinder.GetType(parentElement.Namespace, parentElement.Name);
            }

            // Add available elements from all registered namespaces
            if (declaredNamespaces != null && declaredNamespaces.Any())
            {
                foreach (var ns in declaredNamespaces)
                {
                    string xmlNamespace = ns.Key;
                    string prefix = ns.Value;
                    
                    var types = GetTypesInNamespace(xmlNamespace);
                    foreach (var type in types)
                    {
                        if (IsValidXamlElement(type))
                        {
                            string elementName = string.IsNullOrEmpty(prefix) ? type.Name : $"{prefix}:{type.Name}";
                            
                            if (addedElements.Add(elementName))
                            {
                                var dataType = GetDataTypeForType(type);
                                completionData.Add(new XmlCompletionData(elementName, GetTypeDescription(type), dataType));
                            }
                        }
                    }
                }
            }

            return completionData.OrderBy(x => x.Text).ToArray();
        }

        private ICompletionData[] GetElementCompletionDataForPrefix(string text, Dictionary<string, string> declaredNamespaces, string prefix)
        {
            var completionData = new List<ICompletionData>();

            if (declaredNamespaces == null || string.IsNullOrEmpty(prefix))
                return completionData.ToArray();

            // Find the XML namespace for this prefix
            var namespaceEntry = declaredNamespaces.FirstOrDefault(ns => ns.Value == prefix);
            if (namespaceEntry.Key == null)
                return completionData.ToArray();

            string xmlNamespace = namespaceEntry.Key;
            var types = GetTypesInNamespace(xmlNamespace);

            foreach (var type in types)
            {
                if (IsValidXamlElement(type))
                {
                    // Only return the type name without prefix, since prefix is already typed
                    var dataType = GetDataTypeForType(type);
                    completionData.Add(new XmlCompletionData(type.Name, GetTypeDescription(type), dataType));
                }
            }

            return completionData.OrderBy(x => x.Text).ToArray();
        }

        private IEnumerable<Type> GetTypesInNamespace(string xmlNamespace)
        {
            var types = new List<Type>();

            foreach (var assembly in _typeFinder.LoadedAssemblies)
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    foreach (var type in assemblyTypes)
                    {
                        if (type.IsPublic && !type.IsAbstract)
                        {
                            // Check if this type's namespace maps to the XML namespace
                            string prefix;
                            string typeXmlNamespace = _typeFinder.GetXmlNamespaceFor(assembly, type.Namespace ?? string.Empty, out prefix);
                            if (typeXmlNamespace == xmlNamespace || 
                                typeXmlNamespace.StartsWith("clr-namespace:" + type.Namespace))
                            {
                                types.Add(type);
                            }
                        }
                    }
                }
                catch
                {
                    // Skip problematic assemblies
                }
            }

            return types;
        }

        private bool IsValidXamlElement(Type type)
        {
            // Must be public, not abstract, and instantiable
            if (!type.IsPublic || type.IsAbstract || type.IsInterface || type.IsEnum)
                return false;

            // Must have a public parameterless constructor or be a known XAML type
            if (!type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0))
            {
                // Some XAML types don't need default constructors (like markup extensions)
                if (!typeof(MarkupExtension).IsAssignableFrom(type) &&
                    !type.Name.EndsWith("Extension"))
                {
                    return false;
                }
            }

            return true;
        }

        private XmlCompletionData.DataType GetDataTypeForType(Type type)
        {
            if (type.IsEnum)
                return XmlCompletionData.DataType.Enum;
            
            if (type.IsInterface)
                return XmlCompletionData.DataType.Interface;
            
            if (type.IsClass)
                return XmlCompletionData.DataType.Class;
            
            return XmlCompletionData.DataType.XmlElement;
        }

        #endregion

        #region Attribute Completion

        private ICompletionData[] GetAttributeCompletionData(string text)
        {
            var completionData = new List<ICompletionData>();
            var addedAttributes = new HashSet<string>();

            XmlElementPath path = XmlParser.GetActiveElementStartPath(text, text.Length);
            if (path.Elements.Count == 0)
                return null;

            var element = path.Elements[path.Elements.Count - 1];
            Type elementType = _typeFinder.GetType(element.Namespace, element.Name);
            
            if (elementType == null)
                return null;

            // Get regular properties
            var properties = TypeDescriptor.GetProperties(elementType);
            foreach (PropertyDescriptor prop in properties)
            {
                if (IsValidXamlProperty(prop))
                {
                    if (addedAttributes.Add(prop.Name))
                    {
                        completionData.Add(new XmlCompletionData(prop.Name, GetPropertyDescription(prop), XmlCompletionData.DataType.Property));
                    }
                }
            }

            // Get WPF dependency properties
            var dependencyProperties = GetDependencyProperties(elementType);
            foreach (var dp in dependencyProperties)
            {
                if (addedAttributes.Add(dp.Name))
                {
                    completionData.Add(new XmlCompletionData(dp.Name, $"Dependency Property: {dp.PropertyType.Name}", XmlCompletionData.DataType.Property));
                }
            }

            // Get attached properties from common types
            AddAttachedProperties(completionData, addedAttributes);

            // Get events
            var events = TypeDescriptor.GetEvents(elementType);
            foreach (EventDescriptor evt in events)
            {
                if (addedAttributes.Add(evt.Name))
                {
                    completionData.Add(new XmlCompletionData(evt.Name, $"Event: {evt.EventType.Name}", XmlCompletionData.DataType.Event));
                }
            }

            return completionData.OrderBy(x => x.Text).ToArray();
        }

        private void AddAttachedProperties(List<ICompletionData> completionData, HashSet<string> addedAttributes)
        {
            // Common types that provide attached properties
            var attachedPropertyProviders = new[]
            {
                "System.Windows.Controls.Grid",
                "System.Windows.Controls.DockPanel",
                "System.Windows.Controls.Canvas",
                "System.Windows.Controls.Panel",
                "System.Windows.Controls.ToolTip",
                "System.Windows.Controls.Button",
            };

            foreach (var typeName in attachedPropertyProviders)
            {
                try
                {
                    var type = Type.GetType(typeName);
                    if (type == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var assembly in _typeFinder.LoadedAssemblies)
                        {
                            type = assembly.GetType(typeName);
                            if (type != null) break;
                        }
                    }

                    if (type != null)
                    {
                        var attachedProps = GetAttachedProperties(type);
                        foreach (var prop in attachedProps)
                        {
                            string attachedName = $"{type.Name}.{prop.Name}";
                            if (addedAttributes.Add(attachedName))
                            {
                                completionData.Add(new XmlCompletionData(attachedName, 
                                    $"Attached Property: {prop.PropertyType.Name}", XmlCompletionData.DataType.AttachedProperty));
                            }
                        }
                    }
                }
                catch
                {
                    // Skip if type can't be loaded
                }
            }
        }

        private IEnumerable<DependencyProperty> GetDependencyProperties(Type type)
        {
            var properties = new List<DependencyProperty>();

            try
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    if (typeof(DependencyProperty).IsAssignableFrom(field.FieldType) && 
                        field.Name.EndsWith("Property"))
                    {
                        var dp = field.GetValue(null) as DependencyProperty;
                        if (dp != null)
                        {
                            properties.Add(dp);
                        }
                    }
                }
            }
            catch
            {
                // Continue without dependency properties
            }

            return properties;
        }

        private IEnumerable<DependencyProperty> GetAttachedProperties(Type type)
        {
            var properties = new List<DependencyProperty>();

            try
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (typeof(DependencyProperty).IsAssignableFrom(field.FieldType) &&
                        field.Name.EndsWith("Property"))
                    {
                        // Check if it has Get/Set methods (attached property pattern)
                        string propName = field.Name.Substring(0, field.Name.Length - "Property".Length);
                        var getMethod = type.GetMethod("Get" + propName, BindingFlags.Public | BindingFlags.Static);
                        var setMethod = type.GetMethod("Set" + propName, BindingFlags.Public | BindingFlags.Static);

                        if (getMethod != null && setMethod != null)
                        {
                            var dp = field.GetValue(null) as DependencyProperty;
                            if (dp != null)
                            {
                                properties.Add(dp);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Continue without attached properties
            }

            return properties;
        }

        private bool IsValidXamlProperty(PropertyDescriptor property)
        {
            // Must be public and readable
            if (property.IsReadOnly && !IsCollectionProperty(property))
                return false;

            // Skip internal properties
            if (property.Attributes.OfType<BrowsableAttribute>().Any(a => !a.Browsable))
                return false;

            return true;
        }

        private bool IsCollectionProperty(PropertyDescriptor property)
        {
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                   property.PropertyType != typeof(string);
        }

        #endregion

        #region Attribute Value Completion

        private ICompletionData[] GetAttributeValueCompletionData(string text, char quote)
        {
            text = text.Substring(0, text.Length - 1); // Remove the quote
            
            string attributeName = XmlParser.GetAttributeName(text, text.Length);
            if (string.IsNullOrEmpty(attributeName))
                return null;

            XmlElementPath path = XmlParser.GetActiveElementStartPath(text, text.Length);
            if (path.Elements.Count == 0)
                return null;

            var element = path.Elements[path.Elements.Count - 1];
            Type elementType = _typeFinder.GetType(element.Namespace, element.Name);
            
            if (elementType == null)
                return null;

            // Get property type
            Type propertyType = GetPropertyType(elementType, attributeName);
            if (propertyType == null)
                return null;

            return GetValueCompletionForType(propertyType);
        }

        private Type GetPropertyType(Type elementType, string propertyName)
        {
            // Check for attached property
            if (propertyName.Contains("."))
            {
                var parts = propertyName.Split('.');
                if (parts.Length == 2)
                {
                    // Try to find the attached property provider type
                    var providerTypes = _typeFinder.LoadedAssemblies
                        .SelectMany(a => 
                        {
                            try { return a.GetTypes(); }
                            catch { return Enumerable.Empty<Type>(); }
                        })
                        .Where(t => t.Name == parts[0]);

                    foreach (var providerType in providerTypes)
                    {
                        var field = providerType.GetField(parts[1] + "Property", 
                            BindingFlags.Public | BindingFlags.Static);
                        
                        if (field != null && typeof(DependencyProperty).IsAssignableFrom(field.FieldType))
                        {
                            var dp = field.GetValue(null) as DependencyProperty;
                            return dp?.PropertyType;
                        }
                    }
                }
            }

            // Regular property
            var properties = TypeDescriptor.GetProperties(elementType);
            var prop = properties[propertyName];
            if (prop != null)
                return prop.PropertyType;

            // Dependency property
            var dpField = elementType.GetField(propertyName + "Property", 
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            
            if (dpField != null && typeof(DependencyProperty).IsAssignableFrom(dpField.FieldType))
            {
                var dp = dpField.GetValue(null) as DependencyProperty;
                return dp?.PropertyType;
            }

            return null;
        }

        private ICompletionData[] GetValueCompletionForType(Type propertyType)
        {
            var completionData = new List<ICompletionData>();

            // Enum completion
            if (propertyType.IsEnum)
            {
                foreach (var value in Enum.GetNames(propertyType))
                {
                    completionData.Add(new XmlCompletionData(value, $"{propertyType.Name}.{value}", XmlCompletionData.DataType.Enum));
                }
            }
            // Boolean completion
            else if (propertyType == typeof(bool))
            {
                completionData.Add(new XmlCompletionData("True", "Boolean value", XmlCompletionData.DataType.XmlAttributeValue));
                completionData.Add(new XmlCompletionData("False", "Boolean value", XmlCompletionData.DataType.XmlAttributeValue));
            }
            // Add more type-specific completions as needed

            return completionData.Any() ? completionData.ToArray() : null;
        }

        #endregion

        #region Helper Methods

        private Dictionary<string, string> ParseDeclaredNamespaces(string text)
        {
            var namespaces = new Dictionary<string, string>();

            try
            {
                var lines = text.Split(new[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                var rootElement = lines.FirstOrDefault(l => l.Contains("xmlns"));
                
                if (string.IsNullOrEmpty(rootElement))
                    return namespaces;

                // Parse xmlns declarations
                var xmlns = System.Text.RegularExpressions.Regex.Matches(
                    rootElement, 
                    @"xmlns(?::(\w+))?=""([^""]+)""");

                foreach (System.Text.RegularExpressions.Match match in xmlns)
                {
                    string prefix = match.Groups[1].Value;
                    string ns = match.Groups[2].Value;
                    
                    if (!namespaces.ContainsKey(ns))
                    {
                        namespaces.Add(ns, prefix);
                    }
                }
            }
            catch
            {
                // Return empty if parsing fails
            }

            return namespaces;
        }

        private string GetTypeDescription(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }
            return type.FullName;
        }

        private string GetPropertyDescription(PropertyDescriptor property)
        {
            if (!string.IsNullOrEmpty(property.Description))
                return property.Description;
            
            return $"{property.PropertyType.Name} property";
        }

        #endregion
    }
}
