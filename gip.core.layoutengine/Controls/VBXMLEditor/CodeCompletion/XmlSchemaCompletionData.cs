using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using System.Reflection;
using gip.core.datamodel;

namespace gip.core.layoutengine.CodeCompletion
{
    /// <summary>
    /// Holds the completion (intellisense) data for an xml schema.
    /// </summary>
    /// <remarks>
    /// The XmlSchema class throws an exception if we attempt to load 
    /// the xhtml1-strict.xsd schema.  It does not like the fact that
    /// this schema redefines the xml namespace, even though this is
    /// allowed by the w3.org specification.
    /// </remarks>
    public class XmlSchemaCompletionData
    {
        string namespaceUri = String.Empty;
        XmlSchema schema;
        string lastNamespace = "";
        string fileName = String.Empty;
        bool readOnly = false;
        Dictionary<string, string> declaredNamespaces;

        internal static Dictionary<string, XmlSchema> XmlSchemaCache = new Dictionary<string, XmlSchema>();
        internal static object XmlSchemaCacheLock = new object();


        /// <summary>
        /// Stores attributes that have been prohibited whilst the code
        /// generates the attribute completion data.
        /// </summary>
        XmlSchemaObjectCollection prohibitedAttributes = new XmlSchemaObjectCollection();

        public XmlSchemaCompletionData()
        {
        }

        /// <summary>
        /// Creates completion data from the schema passed in 
        /// via the reader object.
        /// </summary>
        public XmlSchemaCompletionData(TextReader reader)
        {
            ReadSchema(String.Empty, reader);
        }

        /// <summary>
        /// Creates completion data from the schema passed in 
        /// via the reader object.
        /// </summary>
        public XmlSchemaCompletionData(XmlTextReader reader)
        {
            reader.XmlResolver = null;
            ReadSchema(reader);
        }

        /// <summary>
        /// Creates the completion data from the specified schema file.
        /// </summary>
        public XmlSchemaCompletionData(string fileName)
            : this(String.Empty, fileName)
        {
        }

        /// <summary>
        /// Creates the completion data from the specified schema file and uses
        /// the specified baseUri to resolve any referenced schemas.
        /// </summary>
        public XmlSchemaCompletionData(string baseUri, string fileName)
        {
            using (var reader = XmlReader.Create(fileName))
            {
                //ReadSchema(baseUri, reader);
                ReadSchema(reader);
                this.fileName = fileName;
            }
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        public XmlSchema Schema
        {
            get
            {
                return schema;
            }
        }

        /// <summary>
        /// Read only schemas are those that are installed with 
        /// SharpDevelop.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }

            set
            {
                readOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the schema's file name.
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        /// <summary>
        /// Gets the namespace URI for the schema.
        /// </summary>
        public string NamespaceUri
        {
            get
            {
                return namespaceUri;
            }
        }

        /// <summary>
        /// Converts the filename into a valid Uri.
        /// </summary>
        public static string GetUri(string fileName)
        {
            string uri = String.Empty;

            if (fileName != null)
            {
                if (fileName.Length > 0)
                {
                    uri = String.Concat("file:///", fileName.Replace('\\', '/'));
                }
            }

            return uri;
        }

        /// <summary>
        /// Gets the possible root elements for an xml document using this schema.
        /// </summary>
        public ICompletionData[] GetElementCompletionData()
        {
            return GetElementCompletionData(String.Empty, schema);
        }

        /// <summary>
        /// Gets the possible root elements for an xml document using this schema.
        /// </summary>
        public ICompletionData[] GetElementCompletionData(string namespacePrefix, XmlSchema targetSchema)
        {
            string ns = declaredNamespaces != null ? declaredNamespaces.FirstOrDefault(c => c.Value == namespacePrefix).Key : "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

            if (targetSchema.TargetNamespace == ns)
            {
                XmlCompletionDataCollection data = new XmlCompletionDataCollection();

                foreach (XmlSchemaElement element in targetSchema.Elements.Values)
                {
                    if (element.Name != null)
                    {
                        AddElement(data, element.Name, namespacePrefix, element.Annotation);
                    }
                    else
                    {
                        // Do not add reference element.
                    }
                }

                return data.ToArray();
            }
            else if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import != null && import.Schema != null)
                    {
                        var result = GetElementCompletionData(namespacePrefix, import.Schema);
                        if (result == null)
                            continue;
                        if (result.Any())
                            return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the possible root elements for an xml document using this schema.
        /// </summary>
        public ICompletionData[] GetElementCompletionData(string namespacePrefix, XmlSchema targetSchema, XmlElementPath parentPath)
        {
            if (parentPath == null)
                return GetElementCompletionData(namespacePrefix, targetSchema);

            XmlSchemaElement el = FindElement(parentPath);
            if (el == null)
                return GetElementCompletionData(namespacePrefix, targetSchema);

            var subGroupName = GetElementAsComplexType(el);

            if (declaredNamespaces == null)
                return null;

            string ns = declaredNamespaces.FirstOrDefault(c => c.Value == namespacePrefix).Key;

            if (targetSchema.TargetNamespace == ns)
            {
                XmlCompletionDataCollection data = new XmlCompletionDataCollection();

                foreach (XmlSchemaElement element in targetSchema.Elements.Values)
                {
                    if (element.Name != null)
                    {
                        AddElement(data, element.Name, namespacePrefix, element.Annotation);
                    }
                    else
                    {
                        // Do not add reference element.
                    }
                }

                return data.ToArray();
            }
            else if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import != null && import.Schema != null)
                    {
                        var result = GetElementCompletionData(namespacePrefix, import.Schema);
                        if (result == null)
                            continue;
                        if (result.Any())
                            return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the attribute completion data for the xml element that exists
        /// at the end of the specified path.
        /// </summary>
        public ICompletionData[] GetAttributeCompletionData(XmlElementPath path)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            // Locate matching element.
            XmlSchemaElement element = FindElement(path);

            // Get completion data.
            if (element != null)
            {
                prohibitedAttributes.Clear();
                data = GetAttributeCompletionData(element);
            }

            return data.ToArray();
        }

        /// <summary>
        /// Gets the child element completion data for the xml element that exists
        /// at the end of the specified path.
        /// </summary>
        public ICompletionData[] GetChildElementCompletionData(XmlElementPath path, Dictionary<string, string> declNs)
        {
            declaredNamespaces = declNs;

            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            // Locate matching element.
            XmlSchemaElement element = FindElement(path);

            // Get completion data.
            if (element != null)
            {
                data = GetChildElementCompletionData(element, path.Elements.LastPrefix);
            }

            if (declNs == null)
                return null;

            foreach (string ns in declNs.Values.Where(c => !string.IsNullOrEmpty(c)))
            {
                if (path.Elements.LastPrefix != ns)
                    data.Add(new XmlCompletionData(ns + ":"));
            }

            return data.ToArray();
        }

        /// <summary>
        /// Gets the autocomplete data for the specified attribute value.
        /// </summary>
        public ICompletionData[] GetAttributeValueCompletionData(XmlElementPath path, string name)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            // Locate matching element.
            XmlSchemaElement element = FindElement(path);

            // Get completion data.
            if (element != null)
            {
                data = GetAttributeValueCompletionData(element, name);
            }

            return data.ToArray();
        }

        /// <summary>
        /// Finds the element that exists at the specified path.
        /// </summary>
        /// <remarks>This method is not used when generating completion data,
        /// but is a useful method when locating an element so we can jump
        /// to its schema definition.</remarks>
        /// <returns><see langword="null"/> if no element can be found.</returns>
        public XmlSchemaElement FindElement(XmlElementPath path)
        {
            XmlSchemaElement element = null;
            for (int i = 0; i < path.Elements.Count; ++i)
            {
                QualifiedName name = path.Elements[i];
                if (i == 0)
                {
                    // Look for root element.
                    element = FindElement(name, schema);
                    if (element == null)
                    {
                        break;
                    }
                }
                else
                {
                    if (name.Name.Contains(".") || element.QualifiedName.Namespace == name.Namespace)
                        element = FindChildElement(element, name);
                    else
                        element = FindElement(name, schema);
                    if (element == null)
                    {
                        break;
                    }
                }
            }
            return element;
        }

        /// <summary>
        /// Finds an element in the schema.
        /// </summary>
        /// <remarks>
        /// Only looks at the elements that are defined in the 
        /// root of the schema so it will not find any elements
        /// that are defined inside any complex types.
        /// </remarks>
        public XmlSchemaElement FindElement(QualifiedName name, XmlSchema targetSchema)
        {
            foreach (XmlSchemaElement element in targetSchema.Elements.Values)
            {
                if (name.Equals(element.QualifiedName))
                {
                    lastNamespace = targetSchema.TargetNamespace;
                    return element;
                }
                //if (name.Name.Equals(element.QualifiedName.Name)) /* && 
                //   ((!element.RefName.IsEmpty && name.Namespace.Equals(element.RefName.Namespace)) || !element.QualifiedName.IsEmpty && name.Namespace.Equals(element.QualifiedName.Namespace)))*/
                //{
                //    return element;
                //}
            }
            if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal sch in targetSchema.Includes)
                {
                    if (sch.Schema == null)
                    {
                        continue;
                    }
                    var elmn = FindElement(name, sch.Schema);
                    if (elmn != null)
                        return elmn;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the complex type with the specified name.
        /// </summary>
        public XmlSchemaComplexType FindComplexType(QualifiedName name)
        {
            XmlQualifiedName qualifiedName = new XmlQualifiedName(name.Name, name.Namespace);
            return FindNamedType(schema, qualifiedName);
        }

        /// <summary>
        /// Finds the specified attribute name given the element.
        /// </summary>
        /// <remarks>This method is not used when generating completion data,
        /// but is a useful method when locating an attribute so we can jump
        /// to its schema definition.</remarks>
        /// <returns><see langword="null"/> if no attribute can be found.</returns>
        public XmlSchemaAttribute FindAttribute(XmlSchemaElement element, string name)
        {
            XmlSchemaAttribute attribute = null;
            XmlSchemaComplexType complexType = GetElementAsComplexType(element);
            if (complexType != null)
            {
                attribute = FindAttribute(complexType, name);
            }
            return attribute;
        }

        /// <summary>
        /// Finds the attribute group with the specified name.
        /// </summary>
        public XmlSchemaAttributeGroup FindAttributeGroup(string name)
        {
            return FindAttributeGroup(schema, name);
        }

        /// <summary>
        /// Finds the simple type with the specified name.
        /// </summary>
        public XmlSchemaSimpleType FindSimpleType(string name)
        {
            XmlQualifiedName qualifiedName = new XmlQualifiedName(name, namespaceUri);
            return FindSimpleType(qualifiedName);
        }

        /// <summary>
        /// Finds the specified attribute in the schema. This method only checks
        /// the attributes defined in the root of the schema.
        /// </summary>
        public XmlSchemaAttribute FindAttribute(string name)
        {
            foreach (XmlSchemaAttribute attribute in schema.Attributes.Values)
            {
                if (attribute.Name == name)
                {
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the schema group with the specified name.
        /// </summary>
        public XmlSchemaGroup FindGroup(string name, XmlSchema targetSchema)
        {
            if (name != null)
            {
                foreach (XmlSchemaObject schemaObject in targetSchema.Groups.Values)
                {
                    XmlSchemaGroup group = schemaObject as XmlSchemaGroup;
                    if (group != null)
                    {
                        if (group.Name == name)
                        {
                            return group;
                        }
                    }
                }
            }
            if (schema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import.Schema != null)
                    {
                        var grp = FindGroup(name, import.Schema);
                        if (grp != null)
                            return grp;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Takes the name and creates a qualified name using the namespace of this
        /// schema.
        /// </summary>
        /// <remarks>If the name is of the form myprefix:mytype then the correct 
        /// namespace is determined from the prefix. If the name is not of this
        /// form then no prefix is added.</remarks>
        public QualifiedName CreateQualifiedName(string name)
        {
            int index = name.IndexOf(":");
            if (index >= 0)
            {
                string prefix = name.Substring(0, index);
                name = name.Substring(index + 1);
                foreach (XmlQualifiedName xmlQualifiedName in schema.Namespaces.ToArray())
                {
                    if (xmlQualifiedName.Name == prefix)
                    {
                        return new QualifiedName(name, xmlQualifiedName.Namespace, prefix);
                    }
                }
            }

            // Default behaviour just return the name with the namespace uri.
            return new QualifiedName(name, namespaceUri);
        }

        /// <summary>
        /// Converts the element to a complex type if possible.
        /// </summary>
        public XmlSchemaComplexType GetElementAsComplexType(XmlSchemaElement element)
        {
            XmlSchemaComplexType complexType = element.SchemaType as XmlSchemaComplexType;
            if (complexType == null)
            {
                complexType = FindNamedType(schema, element.SchemaTypeName);
            }
            return complexType;
        }

        /// <summary>
        /// Handler for schema validation errors.
        /// </summary>
        void SchemaValidation(object source, ValidationEventArgs e)
        {
            // Do nothing.
        }

        /// <summary>
        /// Loads the schema.
        /// </summary>
        void ReadSchema(XmlReader reader)
        {
            try
            {
                schema = XmlSchema.Read(reader, new ValidationEventHandler(SchemaValidation));
                XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
                xmlSchemaSet.XmlResolver = null;
                xmlSchemaSet.Add(schema);
                xmlSchemaSet.ValidationEventHandler += SchemaValidation;
                xmlSchemaSet.Compile();

                ReadImportedSchemas(schema);

                namespaceUri = schema.TargetNamespace;
                xmlSchemaSet.ValidationEventHandler -= SchemaValidation;
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("XmlSchemaCompletionData", "ReadSchema", e.Message);
            }
            finally
            {
                reader.Close();
            }
        }

        void ReadImportedSchemas(XmlSchema schema)
        {
            foreach (XmlSchemaExternal incSchema in schema.Includes)
            {
                try
                {
                    XmlSchema cachedSchema;

                    lock (XmlSchemaCacheLock)
                    {
                        XmlSchemaCache.TryGetValue(incSchema.SchemaLocation, out cachedSchema);
                    }

                    if (cachedSchema != null)
                        incSchema.Schema = cachedSchema;
                    else
                    {
                        Stream stream = LoadSchema(incSchema.SchemaLocation);
                        if (stream == null)
                            continue;

                        using (var xmlReader = XmlReader.Create(stream))
                        {
                            incSchema.Schema = ReadSchema1(xmlReader);
                            lock (XmlSchemaCacheLock)
                            {
                                if (!XmlSchemaCache.Keys.Contains(incSchema.SchemaLocation))
                                    XmlSchemaCache.Add(incSchema.SchemaLocation, incSchema.Schema);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Database.Root.Messages.LogException("XmlSchemaCompletionData", "ReadImportedSchemas", e.Message);
                }

                if (incSchema.Schema != null)
                    ReadImportedSchemas(incSchema.Schema);
            }
        }

        XmlSchema ReadSchema1(XmlReader reader)
        {
            try
            {
                var schema1 = XmlSchema.Read(reader, new ValidationEventHandler(SchemaValidation));
                XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
                xmlSchemaSet.XmlResolver = new XmlSchemaResolver();
                xmlSchemaSet.Add(schema1);
                xmlSchemaSet.ValidationEventHandler += SchemaValidation;
                xmlSchemaSet.Compile();
                xmlSchemaSet.ValidationEventHandler -= SchemaValidation;
                return schema1;

            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("XmlSchemaCompletionData", "ReadSchema1", e.Message);
            }
            finally
            {
                reader.Close();
            }
            return null;
        }

        void ReadSchema(string baseUri, TextReader reader)
        {
            using (var xmlReader = new XmlTextReader(baseUri, reader))
            {
                // Setting the resolver to null allows us to
                // load the xhtml1-strict.xsd without any exceptions if
                // the referenced dtds exist in the same folder as the .xsd
                // file.  If this is not set to null the dtd files are looked
                // for in the assembly's folder.
                xmlReader.XmlResolver = null;
                ReadSchema(xmlReader);
            }
        }

        internal static Stream LoadSchema(string schemaPath)
        {
            Uri uri;
            if (!Uri.TryCreate(schemaPath, UriKind.Absolute, out uri))
                Uri.TryCreate(Path.Combine(Path.Combine(AppContext.BaseDirectory, "VBXMLEditorSchemas"), schemaPath), UriKind.Absolute, out uri);

            if (uri == null)
                return null;

            if (uri.IsFile && File.Exists(uri.AbsolutePath))
            {
                return new FileStream(uri.AbsolutePath, FileMode.Open);
            }
            else
            {
                string assemblyName = uri.Segments[1].Split(';').FirstOrDefault();
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(c => c.FullName.Contains(assemblyName));
                if (assembly == null)
                    assembly = AppDomain.CurrentDomain.Load(assemblyName);

                if (assembly == null)
                    return null;

                string resourceName = uri.AbsolutePath.StartsWith("/") ? uri.AbsolutePath.Remove(0, 1).Replace(";component", "").Replace('/', '.')
                                                                       : uri.AbsolutePath.Replace(";component", "").Replace('/', '.');

                return assembly.GetManifestResourceStream(resourceName);
            }
        }

        /// <summary>
        /// Finds an element in the schema.
        /// </summary>
        /// <remarks>
        /// Only looks at the elements that are defined in the 
        /// root of the schema so it will not find any elements
        /// that are defined inside any complex types.
        /// </remarks>
        XmlSchemaElement FindElement(XmlQualifiedName name)
        {
            XmlSchemaElement matchedElement = null;
            foreach (XmlSchemaElement element in schema.Elements.Values)
            {
                if (name.Equals(element.QualifiedName))
                {
                    matchedElement = element;
                    break;
                }
            }

            return matchedElement;
        }

        XmlSchemaElement FindElement(XmlQualifiedName name, XmlSchema targetSchema)
        {
            XmlSchemaElement matchedElement = null;

            if (targetSchema.TargetNamespace == name.Namespace)
            {
                foreach (XmlSchemaElement element in targetSchema.Elements.Values)
                {
                    if (name.Equals(element.QualifiedName))
                    {
                        matchedElement = element;
                        break;
                    }
                }
            }
            else if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import != null && import.Schema != null)
                    {
                        var result = FindElement(name, import.Schema);
                        if (result != null)
                            return result;
                    }
                }
            }

            return matchedElement;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaElement element, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaComplexType complexType = GetElementAsComplexType(element);

            if (complexType != null)
            {
                data = GetChildElementCompletionData(complexType, prefix);
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaComplexType complexType, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaSequence sequence = complexType.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = complexType.Particle as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = complexType.Particle as XmlSchemaGroupRef;
            XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;
            XmlSchemaAll all = complexType.Particle as XmlSchemaAll;

            if (sequence != null)
            {
                data = GetChildElementCompletionData(sequence.Items, prefix);
            }
            else if (choice != null)
            {
                data = GetChildElementCompletionData(choice.Items, prefix);
            }
            else if (complexContent != null)
            {
                data = GetChildElementCompletionData(complexContent, prefix);
            }
            else if (groupRef != null)
            {
                data = GetChildElementCompletionData(groupRef, prefix);
            }
            else if (all != null)
            {
                data = GetChildElementCompletionData(all.Items, prefix);
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaObjectCollection items, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            foreach (XmlSchemaObject schemaObject in items)
            {

                XmlSchemaElement childElement = schemaObject as XmlSchemaElement;
                XmlSchemaSequence childSequence = schemaObject as XmlSchemaSequence;
                XmlSchemaChoice childChoice = schemaObject as XmlSchemaChoice;
                XmlSchemaGroupRef groupRef = schemaObject as XmlSchemaGroupRef;

                if (childElement != null)
                {
                    string name = childElement.Name;

                    if (name == null)
                    {
                        name = childElement.RefName.Name;

                        if (childElement.RefName != null && !string.IsNullOrEmpty(childElement.RefName.Namespace))
                        {
                            string elementPrefix;
                            if (declaredNamespaces.TryGetValue(childElement.RefName.Namespace, out elementPrefix))
                                prefix = elementPrefix;
                        }

                        XmlSchemaElement element = FindElement(childElement.RefName, schema);
                        if (element != null)
                        {
                            if (element.IsAbstract)
                            {
                                AddSubstitionGroupElements(data, element.QualifiedName, prefix, schema);
                            }
                            else
                            {
                                AddElement(data, name, prefix, element.Annotation);
                            }
                        }
                        else
                        {
                            AddElement(data, name, prefix, childElement.Annotation);
                        }
                    }
                    else
                    {
                        if (childElement.QualifiedName != null && !string.IsNullOrEmpty(childElement.QualifiedName.Namespace))
                        {
                            string elementPrefix;
                            if (declaredNamespaces.TryGetValue(childElement.QualifiedName.Namespace, out elementPrefix))
                                prefix = elementPrefix;
                        }
                        AddElement(data, name, prefix, childElement.Annotation);
                    }
                }
                else if (childSequence != null)
                {
                    AddElements(data, GetChildElementCompletionData(childSequence.Items, prefix));
                }
                else if (childChoice != null)
                {
                    AddElements(data, GetChildElementCompletionData(childChoice.Items, prefix));
                }
                else if (groupRef != null)
                {
                    AddElements(data, GetChildElementCompletionData(groupRef, prefix));
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaComplexContent complexContent, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaComplexContentExtension extension = complexContent.Content as XmlSchemaComplexContentExtension;
            if (extension != null)
            {
                data = GetChildElementCompletionData(extension, prefix);
            }
            else
            {
                XmlSchemaComplexContentRestriction restriction = complexContent.Content as XmlSchemaComplexContentRestriction;
                if (restriction != null)
                {
                    data = GetChildElementCompletionData(restriction, prefix);
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaComplexContentExtension extension, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaComplexType complexType = FindNamedType(schema, extension.BaseTypeName);
            if (complexType != null)
            {
                data = GetChildElementCompletionData(complexType, prefix);
            }

            // Add any elements.
            if (extension.Particle != null)
            {
                XmlSchemaSequence sequence = extension.Particle as XmlSchemaSequence;
                XmlSchemaChoice choice = extension.Particle as XmlSchemaChoice;
                XmlSchemaGroupRef groupRef = extension.Particle as XmlSchemaGroupRef;

                if (sequence != null)
                {
                    data.AddRange(GetChildElementCompletionData(sequence.Items, prefix));
                }
                else if (choice != null)
                {
                    data.AddRange(GetChildElementCompletionData(choice.Items, prefix));
                }
                else if (groupRef != null)
                {
                    data.AddRange(GetChildElementCompletionData(groupRef, prefix));
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaGroupRef groupRef, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaGroup group = FindGroup(groupRef.RefName.Name, schema);
            if (group != null)
            {
                XmlSchemaSequence sequence = group.Particle as XmlSchemaSequence;
                XmlSchemaChoice choice = group.Particle as XmlSchemaChoice;

                if (sequence != null)
                {
                    data = GetChildElementCompletionData(sequence.Items, prefix);
                }
                else if (choice != null)
                {
                    data = GetChildElementCompletionData(choice.Items, prefix);
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetChildElementCompletionData(XmlSchemaComplexContentRestriction restriction, string prefix)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            // Add any elements.
            if (restriction.Particle != null)
            {
                XmlSchemaSequence sequence = restriction.Particle as XmlSchemaSequence;
                XmlSchemaChoice choice = restriction.Particle as XmlSchemaChoice;
                XmlSchemaGroupRef groupRef = restriction.Particle as XmlSchemaGroupRef;

                if (sequence != null)
                {
                    data = GetChildElementCompletionData(sequence.Items, prefix);
                }
                else if (choice != null)
                {
                    data = GetChildElementCompletionData(choice.Items, prefix);
                }
                else if (groupRef != null)
                {
                    data = GetChildElementCompletionData(groupRef, prefix);
                }
            }

            return data;
        }

        /// <summary>
        /// Adds an element completion data to the collection if it does not 
        /// already exist.
        /// </summary>
        void AddElement(XmlCompletionDataCollection data, string name, string prefix, string documentation)
        {
            if (!data.Contains(name))
            {
                if (prefix != null && prefix.Length > 0)
                {
                    name = String.Concat(prefix, ":", name);
                }
                XmlCompletionData completionData = new XmlCompletionData(name, documentation);
                data.Add(completionData);
            }
        }

        /// <summary>
        /// Adds an element completion data to the collection if it does not 
        /// already exist.
        /// </summary>
        void AddElement(XmlCompletionDataCollection data, string name, string prefix, XmlSchemaAnnotation annotation)
        {
            // Get any annotation documentation.
            string documentation = GetDocumentation(annotation);
            AddElement(data, name, prefix, documentation);
        }

        /// <summary>
        /// Adds elements to the collection if it does not already exist.
        /// </summary>
        void AddElements(XmlCompletionDataCollection lhs, XmlCompletionDataCollection rhs)
        {
            foreach (XmlCompletionData data in rhs)
            {
                if (!lhs.Contains(data))
                {
                    lhs.Add(data);
                }
            }
        }

        /// <summary>
        /// Gets the documentation from the annotation element.
        /// </summary>
        /// <remarks>
        /// All documentation elements are added.  All text nodes inside
        /// the documentation element are added.
        /// </remarks>
        string GetDocumentation(XmlSchemaAnnotation annotation)
        {
            string documentation = String.Empty;

            if (annotation != null)
            {
                StringBuilder documentationBuilder = new StringBuilder();
                XmlSchemaDocumentation schemaDocumentation = null;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Environment != null &&
                        datamodel.Database.Root.Environment.VBLanguageCode == "de")
                {
                    schemaDocumentation = annotation.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault(x => x.Language == "de");

                    if (schemaDocumentation == null)
                        schemaDocumentation = annotation.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault();
                }
                else
                {
                    schemaDocumentation = annotation.Items.OfType<XmlSchemaDocumentation>().FirstOrDefault(x => x.Language == null);
                }

                if (schemaDocumentation != null)
                {
                    foreach (XmlNode node in schemaDocumentation.Markup)
                    {
                        XmlText textNode = node as XmlText;
                        if (textNode != null)
                        {
                            if (textNode.Data != null)
                            {
                                if (textNode.Data.Length > 0)
                                {
                                    documentationBuilder.Append(textNode.Data);
                                }
                            }
                        }
                    }

                }
                documentation = documentationBuilder.ToString();
            }

            return documentation;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaElement element)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaComplexType complexType = GetElementAsComplexType(element);

            if (complexType != null)
            {
                data.AddRange(GetAttributeCompletionData(complexType));
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaComplexContentRestriction restriction)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            data.AddRange(GetAttributeCompletionData(restriction.Attributes));

            XmlSchemaComplexType baseComplexType = FindNamedType(schema, restriction.BaseTypeName);
            if (baseComplexType != null)
            {
                data.AddRange(GetAttributeCompletionData(baseComplexType));
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaComplexType complexType)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            data = GetAttributeCompletionData(complexType.Attributes);

            // Add any complex content attributes.
            XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;
            if (complexContent != null)
            {
                XmlSchemaComplexContentExtension extension = complexContent.Content as XmlSchemaComplexContentExtension;
                XmlSchemaComplexContentRestriction restriction = complexContent.Content as XmlSchemaComplexContentRestriction;
                if (extension != null)
                {
                    data.AddRange(GetAttributeCompletionData(extension));
                }
                else if (restriction != null)
                {
                    data.AddRange(GetAttributeCompletionData(restriction));
                }
            }
            else
            {
                XmlSchemaSimpleContent simpleContent = complexType.ContentModel as XmlSchemaSimpleContent;
                if (simpleContent != null)
                {
                    data.AddRange(GetAttributeCompletionData(simpleContent));
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaComplexContentExtension extension)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            data.AddRange(GetAttributeCompletionData(extension.Attributes));
            XmlSchemaComplexType baseComplexType = FindNamedType(schema, extension.BaseTypeName);
            if (baseComplexType != null)
            {
                data.AddRange(GetAttributeCompletionData(baseComplexType));
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaSimpleContent simpleContent)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaSimpleContentExtension extension = simpleContent.Content as XmlSchemaSimpleContentExtension;
            if (extension != null)
            {
                data.AddRange(GetAttributeCompletionData(extension));
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaSimpleContentExtension extension)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            data.AddRange(GetAttributeCompletionData(extension.Attributes));

            return data;
        }

        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaObjectCollection attributes)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            foreach (XmlSchemaObject schemaObject in attributes)
            {
                XmlSchemaAttribute attribute = schemaObject as XmlSchemaAttribute;
                XmlSchemaAttributeGroupRef attributeGroupRef = schemaObject as XmlSchemaAttributeGroupRef;
                if (attribute != null)
                {
                    if (!IsProhibitedAttribute(attribute))
                    {
                        AddAttribute(data, attribute);
                    }
                    else
                    {
                        prohibitedAttributes.Add(attribute);
                    }
                }
                else if (attributeGroupRef != null)
                {
                    data.AddRange(GetAttributeCompletionData(attributeGroupRef));
                }
            }
            return data;
        }

        /// <summary>
        /// Checks that the attribute is prohibited or has been flagged
        /// as prohibited previously. 
        /// </summary>
        bool IsProhibitedAttribute(XmlSchemaAttribute attribute)
        {
            bool prohibited = false;
            if (attribute.Use == XmlSchemaUse.Prohibited)
            {
                prohibited = true;
            }
            else
            {
                foreach (XmlSchemaAttribute prohibitedAttribute in prohibitedAttributes)
                {
                    if (prohibitedAttribute.QualifiedName == attribute.QualifiedName)
                    {
                        prohibited = true;
                        break;
                    }
                }
            }

            return prohibited;
        }

        /// <summary>
        /// Adds an attribute to the completion data collection.
        /// </summary>
        /// <remarks>
        /// Note the special handling of xml:lang attributes.
        /// </remarks>
        void AddAttribute(XmlCompletionDataCollection data, XmlSchemaAttribute attribute)
        {
            string name = attribute.Name;
            if (name == null)
            {
                if (attribute.RefName.Namespace == "http://www.w3.org/XML/1998/namespace")
                {
                    name = String.Concat("xml:", attribute.RefName.Name);
                }
            }

            if (name != null)
            {
                string documentation = GetDocumentation(attribute.Annotation);
                XmlCompletionData completionData = new XmlCompletionData(name, documentation, XmlCompletionData.DataType.XmlAttribute);
                data.Add(completionData);
            }
        }

        /// <summary>
        /// Gets attribute completion data from a group ref.
        /// </summary>
        XmlCompletionDataCollection GetAttributeCompletionData(XmlSchemaAttributeGroupRef groupRef)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();
            XmlSchemaAttributeGroup group = FindAttributeGroup(schema, groupRef.RefName.Name);
            if (group != null)
            {
                data = GetAttributeCompletionData(group.Attributes);
            }

            return data;
        }

        static XmlSchemaComplexType FindNamedType(XmlSchema schema, XmlQualifiedName name)
        {
            XmlSchemaComplexType matchedComplexType = null;

            if (name != null)
            {
                foreach (XmlSchemaObject schemaObject in schema.Items)
                {
                    XmlSchemaComplexType complexType = schemaObject as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        if (complexType.QualifiedName.Name == name.Name)
                        {
                            matchedComplexType = complexType;
                            break;
                        }
                    }
                }

                // Try included schemas.
                if (matchedComplexType == null)
                {
                    foreach (XmlSchemaExternal external in schema.Includes)
                    {
                        XmlSchemaInclude include = external as XmlSchemaInclude;
                        if (include != null)
                        {
                            if (include.Schema != null)
                            {
                                matchedComplexType = FindNamedType(include.Schema, name);
                            }
                        }
                        else
                        {
                            XmlSchemaImport import = external as XmlSchemaImport;
                            if (import == null || import.Schema == null)
                                continue;
                            matchedComplexType = FindNamedType(import.Schema, name);
                        }
                        if (matchedComplexType != null)
                            break;
                    }
                }
            }

            return matchedComplexType;
        }

        /// <summary>
        /// Finds an element that matches the specified <paramref name="name"/>
        /// from the children of the given <paramref name="element"/>.
        /// </summary>
        XmlSchemaElement FindChildElement(XmlSchemaElement element, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;

            XmlSchemaComplexType complexType = GetElementAsComplexType(element);
            if (complexType != null)
            {
                matchedElement = FindChildElement(complexType, name);
            }

            return matchedElement;
        }

        XmlSchemaElement FindChildElement(XmlSchemaComplexType complexType, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;

            XmlSchemaSequence sequence = complexType.Particle as XmlSchemaSequence;
            XmlSchemaChoice choice = complexType.Particle as XmlSchemaChoice;
            XmlSchemaGroupRef groupRef = complexType.Particle as XmlSchemaGroupRef;
            XmlSchemaAll all = complexType.Particle as XmlSchemaAll;
            XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;

            if (sequence != null)
            {
                matchedElement = FindElement(sequence.Items, name);
            }
            else if (choice != null)
            {
                matchedElement = FindElement(choice.Items, name);
            }
            else if (complexContent != null)
            {
                XmlSchemaComplexContentExtension extension = complexContent.Content as XmlSchemaComplexContentExtension;
                XmlSchemaComplexContentRestriction restriction = complexContent.Content as XmlSchemaComplexContentRestriction;
                if (extension != null)
                {
                    matchedElement = FindChildElement(extension, name);
                }
                else if (restriction != null)
                {
                    matchedElement = FindChildElement(restriction, name);
                }
            }
            else if (groupRef != null)
            {
                matchedElement = FindElement(groupRef, name);
            }
            else if (all != null)
            {
                matchedElement = FindElement(all.Items, name);
            }

            return matchedElement;
        }

        /// <summary>
        /// Finds the named child element contained in the extension element.
        /// </summary>
        XmlSchemaElement FindChildElement(XmlSchemaComplexContentExtension extension, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;

            XmlSchemaComplexType complexType = FindNamedType(schema, extension.BaseTypeName);
            if (complexType != null)
            {
                matchedElement = FindChildElement(complexType, name);

                if (matchedElement == null)
                {

                    XmlSchemaSequence sequence = extension.Particle as XmlSchemaSequence;
                    XmlSchemaChoice choice = extension.Particle as XmlSchemaChoice;
                    XmlSchemaGroupRef groupRef = extension.Particle as XmlSchemaGroupRef;

                    if (sequence != null)
                    {
                        matchedElement = FindElement(sequence.Items, name);
                    }
                    else if (choice != null)
                    {
                        matchedElement = FindElement(choice.Items, name);
                    }
                    else if (groupRef != null)
                    {
                        matchedElement = FindElement(groupRef, name);
                    }
                }
            }

            return matchedElement;
        }

        /// <summary>
        /// Finds the named child element contained in the restriction element.
        /// </summary>
        XmlSchemaElement FindChildElement(XmlSchemaComplexContentRestriction restriction, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;
            XmlSchemaSequence sequence = restriction.Particle as XmlSchemaSequence;
            XmlSchemaGroupRef groupRef = restriction.Particle as XmlSchemaGroupRef;

            if (sequence != null)
            {
                matchedElement = FindElement(sequence.Items, name);
            }
            else if (groupRef != null)
            {
                matchedElement = FindElement(groupRef, name);
            }

            return matchedElement;
        }

        /// <summary>
        /// Finds the element in the collection of schema objects.
        /// </summary>
        XmlSchemaElement FindElement(XmlSchemaObjectCollection items, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;

            foreach (XmlSchemaObject schemaObject in items)
            {
                XmlSchemaElement element = schemaObject as XmlSchemaElement;
                XmlSchemaSequence sequence = schemaObject as XmlSchemaSequence;
                XmlSchemaChoice choice = schemaObject as XmlSchemaChoice;
                XmlSchemaGroupRef groupRef = schemaObject as XmlSchemaGroupRef;

                if (element != null)
                {
                    if (element.Name != null)
                    {
                        if (name.Name == element.Name)
                        {
                            matchedElement = element;
                        }
                    }
                    else if (element.RefName != null)
                    {
                        if (name.Name == element.RefName.Name)
                        {
                            matchedElement = FindElement(element.RefName, schema);
                        }
                        else
                        {
                            // Abstract element?
                            XmlSchemaElement abstractElement = FindElement(element.RefName, schema);
                            if (abstractElement != null && abstractElement.IsAbstract)
                            {
                                matchedElement = FindSubstitutionGroupElement(abstractElement.QualifiedName, name, schema);
                            }
                        }
                    }
                }
                else if (sequence != null)
                {
                    matchedElement = FindElement(sequence.Items, name);
                }
                else if (choice != null)
                {
                    matchedElement = FindElement(choice.Items, name);
                }
                else if (groupRef != null)
                {
                    matchedElement = FindElement(groupRef, name);
                }

                // Did we find a match?
                if (matchedElement != null)
                {
                    break;
                }
            }

            return matchedElement;
        }

        XmlSchemaElement FindElement(XmlSchemaGroupRef groupRef, QualifiedName name)
        {
            XmlSchemaElement matchedElement = null;

            XmlSchemaGroup group = FindGroup(groupRef.RefName.Name, schema);
            if (group != null)
            {
                XmlSchemaSequence sequence = group.Particle as XmlSchemaSequence;
                XmlSchemaChoice choice = group.Particle as XmlSchemaChoice;

                if (sequence != null)
                {
                    matchedElement = FindElement(sequence.Items, name);
                }
                else if (choice != null)
                {
                    matchedElement = FindElement(choice.Items, name);
                }
            }

            return matchedElement;
        }

        static XmlSchemaAttributeGroup FindAttributeGroup(XmlSchema schema, string name)
        {
            XmlSchemaAttributeGroup matchedGroup = null;

            if (name != null)
            {
                foreach (XmlSchemaObject schemaObject in schema.Items)
                {

                    XmlSchemaAttributeGroup group = schemaObject as XmlSchemaAttributeGroup;
                    if (group != null)
                    {
                        if (group.Name == name)
                        {
                            matchedGroup = group;
                            break;
                        }
                    }
                }

                // Try included schemas.
                if (matchedGroup == null)
                {
                    foreach (XmlSchemaExternal external in schema.Includes)
                    {
                        XmlSchemaInclude include = external as XmlSchemaInclude;
                        if (include != null)
                        {
                            if (include.Schema != null)
                            {
                                matchedGroup = FindAttributeGroup(include.Schema, name);
                            }
                        }
                        else
                        {
                            XmlSchemaImport import = external as XmlSchemaImport;
                            if (import == null || import.Schema == null)
                                continue;
                            matchedGroup = FindAttributeGroup(import.Schema, name);
                        }
                    }
                }
            }

            return matchedGroup;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaElement element, string name)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaComplexType complexType = GetElementAsComplexType(element);
            if (complexType != null)
            {
                XmlSchemaAttribute attribute = FindAttribute(complexType, name);
                if (attribute != null)
                {
                    data.AddRange(GetAttributeValueCompletionData(attribute));
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaAttribute attribute)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            if (attribute.SchemaType != null)
            {
                XmlSchemaSimpleTypeRestriction simpleTypeRestriction = attribute.SchemaType.Content as XmlSchemaSimpleTypeRestriction;
                if (simpleTypeRestriction != null)
                {
                    data.AddRange(GetAttributeValueCompletionData(simpleTypeRestriction));
                }
            }
            else if (attribute.AttributeSchemaType != null)
            {
                XmlSchemaSimpleType simpleType = attribute.AttributeSchemaType as XmlSchemaSimpleType;

                if (simpleType != null)
                {
                    if (simpleType.Name == "boolean")
                    {
                        data.AddRange(GetBooleanAttributeValueCompletionData());
                    }
                    else
                    {
                        data.AddRange(GetAttributeValueCompletionData(simpleType));
                    }
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaSimpleTypeRestriction simpleTypeRestriction)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            foreach (XmlSchemaObject schemaObject in simpleTypeRestriction.Facets)
            {
                XmlSchemaEnumerationFacet enumFacet = schemaObject as XmlSchemaEnumerationFacet;
                if (enumFacet != null)
                {
                    AddAttributeValue(data, enumFacet.Value, enumFacet.Annotation);
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaSimpleTypeUnion union)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            foreach (XmlSchemaObject schemaObject in union.BaseTypes)
            {
                XmlSchemaSimpleType simpleType = schemaObject as XmlSchemaSimpleType;
                if (simpleType != null)
                {
                    data.AddRange(GetAttributeValueCompletionData(simpleType));
                }
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaSimpleType simpleType)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            XmlSchemaSimpleTypeRestriction simpleTypeRestriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            XmlSchemaSimpleTypeUnion union = simpleType.Content as XmlSchemaSimpleTypeUnion;
            XmlSchemaSimpleTypeList list = simpleType.Content as XmlSchemaSimpleTypeList;

            if (simpleTypeRestriction != null)
            {
                data.AddRange(GetAttributeValueCompletionData(simpleTypeRestriction));
            }
            else if (union != null)
            {
                data.AddRange(GetAttributeValueCompletionData(union));
            }
            else if (list != null)
            {
                data.AddRange(GetAttributeValueCompletionData(list));
            }

            return data;
        }

        XmlCompletionDataCollection GetAttributeValueCompletionData(XmlSchemaSimpleTypeList list)
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            if (list.ItemType != null)
            {
                data.AddRange(GetAttributeValueCompletionData(list.ItemType));
            }
            else if (list.ItemTypeName != null)
            {
                XmlSchemaSimpleType simpleType = FindSimpleType(list.ItemTypeName);
                if (simpleType != null)
                {
                    data.AddRange(GetAttributeValueCompletionData(simpleType));
                }
            }

            return data;
        }

        /// <summary>
        /// Gets the set of attribute values for an xs:boolean type.
        /// </summary>
        XmlCompletionDataCollection GetBooleanAttributeValueCompletionData()
        {
            XmlCompletionDataCollection data = new XmlCompletionDataCollection();

            AddAttributeValue(data, "0");
            AddAttributeValue(data, "1");
            AddAttributeValue(data, "true");
            AddAttributeValue(data, "false");

            return data;
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaComplexType complexType, string name)
        {
            XmlSchemaAttribute matchedAttribute = null;

            XmlSchemaObjectCollection attributes = complexType.Attributes;
            if (complexType.ContentModel != null && complexType.ContentModel.Content != null)
            {
                FindAttributesInBase(complexType, attributes);
                //XmlSchemaComplexContentExtension ext = complexType.ContentModel.Content as XmlSchemaComplexContentExtension;
                //if(ext != null)
                //{
                //    if(ext.BaseTypeName != null)
                //    {
                //        XmlQualifiedName targetName = new XmlQualifiedName(ext.BaseTypeName.Name.StartsWith("d") ? 
                //                                                           ext.BaseTypeName.Name.Substring(1) : ext.BaseTypeName.Name, ext.BaseTypeName.Namespace);
                //        var bType = FindElement(targetName, schema);
                //        if (bType != null && bType.ElementSchemaType != null)
                //        {
                //            XmlSchemaComplexType cmplType = bType.ElementSchemaType as XmlSchemaComplexType;
                //            if (cmplType != null)
                //                attributes = cmplType.Attributes;
                //        }
                //        foreach (var item in ext.Attributes)
                //            attributes.Add(item);

                //    }

                //    //attributes = ext.Attributes;
                //}
            }

            matchedAttribute = FindAttribute(attributes, name);

            if (matchedAttribute == null)
            {
                XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;
                if (complexContent != null)
                {
                    matchedAttribute = FindAttribute(complexContent, name);
                }
            }

            return matchedAttribute;
        }

        private void FindAttributesInBase(XmlSchemaComplexType complexType, XmlSchemaObjectCollection attributes)
        {
            XmlSchemaComplexContentExtension ext = complexType.ContentModel.Content as XmlSchemaComplexContentExtension;
            if (ext != null)
            {
                if (ext.BaseTypeName != null)
                {
                    XmlQualifiedName targetName = new XmlQualifiedName(ext.BaseTypeName.Name.StartsWith("d") ?
                                                                       ext.BaseTypeName.Name.Substring(1) : ext.BaseTypeName.Name, ext.BaseTypeName.Namespace);
                    var bType = FindElement(targetName, schema);
                    if (bType != null && bType.ElementSchemaType != null)
                    {
                        XmlSchemaComplexType cmplType = bType.ElementSchemaType as XmlSchemaComplexType;
                        if (cmplType != null)
                        {
                            foreach (var item in cmplType.Attributes)
                                attributes.Add(item);
                        }

                        if (cmplType.ContentModel != null && cmplType.ContentModel.Content != null)
                            FindAttributesInBase(cmplType, attributes);
                    }
                }
            }
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaObjectCollection schemaObjects, string name)
        {
            XmlSchemaAttribute matchedAttribute = null;

            foreach (XmlSchemaObject schemaObject in schemaObjects)
            {
                XmlSchemaAttribute attribute = schemaObject as XmlSchemaAttribute;
                XmlSchemaAttributeGroupRef groupRef = schemaObject as XmlSchemaAttributeGroupRef;

                if (attribute != null)
                {
                    if (attribute.Name == name)
                    {
                        matchedAttribute = attribute;
                        break;
                    }
                }
                else if (groupRef != null)
                {
                    matchedAttribute = FindAttribute(groupRef, name);
                    if (matchedAttribute != null)
                    {
                        break;
                    }
                }
            }

            return matchedAttribute;
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaAttributeGroupRef groupRef, string name)
        {
            XmlSchemaAttribute matchedAttribute = null;

            if (groupRef.RefName != null)
            {
                XmlSchemaAttributeGroup group = FindAttributeGroup(schema, groupRef.RefName.Name);
                if (group != null)
                {
                    matchedAttribute = FindAttribute(group.Attributes, name);
                }
            }

            return matchedAttribute;
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaComplexContent complexContent, string name)
        {
            XmlSchemaAttribute matchedAttribute = null;

            XmlSchemaComplexContentExtension extension = complexContent.Content as XmlSchemaComplexContentExtension;
            XmlSchemaComplexContentRestriction restriction = complexContent.Content as XmlSchemaComplexContentRestriction;

            if (extension != null)
            {
                matchedAttribute = FindAttribute(extension, name);
            }
            else if (restriction != null)
            {
                matchedAttribute = FindAttribute(restriction, name);
            }

            return matchedAttribute;
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaComplexContentExtension extension, string name)
        {
            return FindAttribute(extension.Attributes, name);
        }

        XmlSchemaAttribute FindAttribute(XmlSchemaComplexContentRestriction restriction, string name)
        {
            XmlSchemaAttribute matchedAttribute = FindAttribute(restriction.Attributes, name);

            if (matchedAttribute == null)
            {
                XmlSchemaComplexType complexType = FindNamedType(schema, restriction.BaseTypeName);
                if (complexType != null)
                {
                    matchedAttribute = FindAttribute(complexType, name);
                }
            }

            return matchedAttribute;
        }

        /// <summary>
        /// Adds an attribute value to the completion data collection.
        /// </summary>
        void AddAttributeValue(XmlCompletionDataCollection data, string valueText)
        {
            XmlCompletionData completionData = new XmlCompletionData(valueText, XmlCompletionData.DataType.XmlAttributeValue);
            data.Add(completionData);
        }

        /// <summary>
        /// Adds an attribute value to the completion data collection.
        /// </summary>
        void AddAttributeValue(XmlCompletionDataCollection data, string valueText, XmlSchemaAnnotation annotation)
        {
            string documentation = GetDocumentation(annotation);
            XmlCompletionData completionData = new XmlCompletionData(valueText, documentation, XmlCompletionData.DataType.XmlAttributeValue);
            data.Add(completionData);
        }

        XmlSchemaSimpleType FindSimpleType(XmlQualifiedName name)
        {
            XmlSchemaSimpleType matchedSimpleType = null;

            foreach (XmlSchemaObject schemaObject in schema.SchemaTypes.Values)
            {
                XmlSchemaSimpleType simpleType = schemaObject as XmlSchemaSimpleType;
                if (simpleType != null)
                {
                    if (simpleType.QualifiedName == name)
                    {
                        matchedSimpleType = simpleType;
                        break;
                    }
                }
            }

            return matchedSimpleType;
        }

        /// <summary>
        /// Adds any elements that have the specified substitution group.
        /// </summary>
        void AddSubstitionGroupElements(XmlCompletionDataCollection data, XmlQualifiedName group, string prefix, XmlSchema targetSchema)
        {
            foreach (XmlSchemaElement element in targetSchema.Elements.Values)
            {
                if (element.SubstitutionGroup.Name == group.Name)
                {
                    declaredNamespaces.TryGetValue(element.QualifiedName.Namespace, out prefix);
                    AddElement(data, element.Name, prefix, element.Annotation);
                }
            }
            if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import != null && import.Schema != null)
                    {
                        AddSubstitionGroupElements(data, group, prefix, import.Schema);
                    }
                }
            }
        }

        /// <summary>
        /// Looks for the substitution group element of the specified name.
        /// </summary>
        XmlSchemaElement FindSubstitutionGroupElement(XmlQualifiedName group, QualifiedName name, XmlSchema targetSchema)
        {
            XmlSchemaElement matchedElement = null;

            //if(group.Namespace == targetSchema.TargetNamespace)
            //{
            foreach (XmlSchemaElement element in targetSchema.Elements.Values)
            {
                if (element.SubstitutionGroup == group)
                {
                    if (element.Name != null)
                    {
                        if (element.Name == name.Name)
                        {
                            matchedElement = element;
                            break;
                        }
                    }
                }
            }
            //}
            if (targetSchema.Includes.Count > 0)
            {
                foreach (XmlSchemaExternal import in targetSchema.Includes)
                {
                    if (import != null && import.Schema != null)
                    {
                        var result = FindSubstitutionGroupElement(group, name, import.Schema);
                        if (result != null)
                            return result;
                    }
                }
            }


            return matchedElement;
        }
    }

    public class XmlSchemaResolver : XmlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            XmlSchema schema;
            lock (XmlSchemaCompletionData.XmlSchemaCacheLock)
            {
                if (XmlSchemaCompletionData.XmlSchemaCache.TryGetValue(absoluteUri.AbsoluteUri, out schema))
                    return schema;
            }

            var stream = XmlSchemaCompletionData.LoadSchema(absoluteUri.AbsoluteUri);
            var reader = XmlReader.Create(stream);
            schema = XmlSchema.Read(reader, SchemaValidation);

            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            xmlSchemaSet.XmlResolver = new XmlSchemaResolver();
            xmlSchemaSet.Add(schema);
            xmlSchemaSet.ValidationEventHandler += SchemaValidation;
            xmlSchemaSet.Compile();
            xmlSchemaSet.ValidationEventHandler -= SchemaValidation;

            lock (XmlSchemaCompletionData.XmlSchemaCacheLock)
            {
                if (!XmlSchemaCompletionData.XmlSchemaCache.Keys.Contains(absoluteUri.AbsoluteUri))
                    XmlSchemaCompletionData.XmlSchemaCache.Add(absoluteUri.AbsoluteUri, schema);
            }

            return schema;
        }

        /// <summary>
        /// Handler for schema validation errors.
        /// </summary>
        void SchemaValidation(object source, ValidationEventArgs e)
        {
            // Do nothing.
        }
    }
}
