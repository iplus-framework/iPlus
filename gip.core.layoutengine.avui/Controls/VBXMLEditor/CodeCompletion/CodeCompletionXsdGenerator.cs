using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Code completion generator
    /// </summary>
    public class CodeCompletionXsdGenerator
    {
        internal static string xsNs = "http://www.w3.org/2001/XMLSchema";
        public static XmlQualifiedName vbNs = new XmlQualifiedName("http://www.iplus-framework.com/axaml", "vb");
        public static XmlQualifiedName msNs = new XmlQualifiedName(ACxmlnsResolver.C_AvaloniaNamespaceMapping[0].AvaloniaNamespace,"");
        public static string baseDir = "";
        internal static string vbSchemaPath = @"\gip.core.layoutengine.avui\VBXMLEditorSchemas\VBSchema.xsd";
        internal static string msSchemaPath = @"\gip.core.layoutengine.avui\VBXMLEditorSchemas\XamlPresentation2006.xsd";
        internal static XmlSchemaGroup mainGroup;
        internal static string mainSchema;
        internal static XmlQualifiedName currentNs;

        static XmlDocument documentation;

        /// <summary>
        /// Runs generator tool for code completion schema.
        /// </summary>
        /// <param name="targetAssembly">The name of target assembly.</param>
        /// <param name="currentNs">The namespace of current assembly.</param>
        /// <param name="allowedInheritedTypes">Determines is inherited types allowed.</param>
        /// <param name="importSchema">The schemas which this schema imports.</param>
        /// <param name="mainGroupName">The name of main group.</param>
        /// <param name="typeSpecFunc">The custom function.</param>
        /// <param name="currentSchemaPath">The current schema path.</param>
        /// <param name="itemPrefix">The item name prefix.</param>
        /// <param name="customAttCheck">The custom attribute check.</param>
        public void RunTool(string targetAssembly, XmlQualifiedName currentNs, Type[] allowedInheritedTypes, List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchema, string mainGroupName, 
                            Func<XmlSchemaElement, Type, bool> typeSpecFunc, string currentSchemaPath, string itemPrefix = "", string customAttCheck = "")
        {
            CodeCompletionXsdGenerator.currentNs = currentNs;

            var currentAssembly = Assembly.Load(targetAssembly);
            Type[] types = currentAssembly.GetTypes();

            var depenTypes = types.Where(x => allowedInheritedTypes[0].IsAssignableFrom(x) || allowedInheritedTypes[1].IsAssignableFrom(x));
            mainSchema = File.ReadAllText(baseDir+msSchemaPath);

            documentation = new XmlDocument();
            try { documentation.Load(string.Format("{0}\\{1}.xml", AppContext.BaseDirectory, targetAssembly));}
            catch { documentation = null; }

            XmlSchema schema = new XmlSchema();
            schema.Namespaces.Add(currentNs.Namespace, currentNs.Name);
            schema.TargetNamespace = currentNs.Name;

            foreach (var sch in importSchema)
            {
                schema.Includes.Add(sch.Item1);
                schema.Namespaces.Add(sch.Item2.Namespace, sch.Item2.Name);
            }

            List<string> addedEnums = new List<string>();

            mainGroup = new XmlSchemaGroup();
            mainGroup.Name = mainGroupName;

            XmlSchemaChoice choice = new XmlSchemaChoice();

            foreach (var depType in depenTypes)
            {
                if (!string.IsNullOrEmpty(itemPrefix) &&  !depType.Name.StartsWith(itemPrefix))
                    continue;

                if (depType.Name.Any(ch => !XmlConvert.IsXmlChar(ch)) || depType.IsGenericType)
                    continue;

                XmlSchemaElement schemaElement = new XmlSchemaElement();
                schemaElement.Name = depType.Name;
                schemaElement.SchemaTypeName = new XmlQualifiedName(currentNs.Namespace+":"+depType.Name);

                XmlSchemaAnnotation xmlSchemaAnnotation = new XmlSchemaAnnotation();
                var docNodes = ReadDocumentation(string.Format("T:{0}", depType.FullName));
                if (docNodes != null && docNodes.Any())
                {
                    schemaElement.Annotation = xmlSchemaAnnotation;

                    if (docNodes[0] != null)
                    {
                        XmlSchemaDocumentation xmlSchemaDocumentation = new XmlSchemaDocumentation();
                        xmlSchemaAnnotation.Items.Add(xmlSchemaDocumentation);
                        xmlSchemaDocumentation.Markup = docNodes[0];
                    }

                    if (docNodes[1] != null)
                    {
                        XmlSchemaDocumentation xmlSchemaDocumentationDE = new XmlSchemaDocumentation();
                        xmlSchemaAnnotation.Items.Add(xmlSchemaDocumentationDE);
                        xmlSchemaDocumentationDE.Language = "de";
                        xmlSchemaDocumentationDE.Markup = docNodes[1];
                    }
                }

                if(typeSpecFunc != null)
                    typeSpecFunc.Invoke(schemaElement, depType);

                XmlSchemaElement schemaElForGroup = new XmlSchemaElement();
                schemaElForGroup.RefName = new XmlQualifiedName(schemaElement.Name, currentNs.Name);
                choice.Items.Add(schemaElForGroup);

                XmlSchemaComplexType cType = new XmlSchemaComplexType();
                cType.Name = depType.Name;

                XmlSchemaComplexContent complexContent = new XmlSchemaComplexContent();
                complexContent.IsMixed = true;

                var baseType = depType.BaseType;
                if (baseType != null && baseType.Assembly.FullName.StartsWith("PresentationFramework"))
                {
                    string nameOfType = "d" + baseType.Name;
                    XmlSchemaComplexContentExtension cmxContentExt = new XmlSchemaComplexContentExtension();
                    cmxContentExt.BaseTypeName = new XmlQualifiedName(nameOfType, msNs.Namespace);
                    complexContent.Content = cmxContentExt;
                    cType.ContentModel = complexContent;

                    var props = depType.GetProperties();
                    if (props.Any())
                        CreateElementsAndAttributes(props, depType, schema, addedEnums, mainGroupName, cmxContentExt, null, customAttCheck);
                }
                else if (baseType != null && baseType.Assembly.FullName.StartsWith("gip.core.layoutengine.avui"))
                {
                    XmlSchemaComplexContentExtension cmxContentExt = new XmlSchemaComplexContentExtension();
                    cmxContentExt.BaseTypeName = new XmlQualifiedName(baseType.Name, vbNs.Name);
                    complexContent.Content = cmxContentExt;
                    cType.ContentModel = complexContent;

                    var props = depType.GetProperties();
                    if (props.Any())
                        CreateElementsAndAttributes(props, depType, schema, addedEnums, mainGroupName, cmxContentExt, null, customAttCheck);
                }
                else
                {
                    var props = depType.GetProperties();
                    if (props.Any())
                        CreateElementsAndAttributes(props, depType, schema, addedEnums, mainGroupName, null, cType, customAttCheck);
                }

                schema.Items.Add(cType);
                schema.Items.Add(schemaElement);
            }

            mainGroup.Particle = choice;
            schema.Items.Add(mainGroup);

            if (File.Exists(currentSchemaPath))
                File.Delete(currentSchemaPath);

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            schemaSet.Add(schema);
            schemaSet.Compile();

            using (FileStream fs = new FileStream(currentSchemaPath, FileMode.CreateNew))
            {
                schema.Write(fs);
            }
        }

        static void ValidationCallBack(object sender, ValidationEventArgs e)
        {

        }

        private static void CreateElementsAndAttributes(IEnumerable<PropertyInfo> props, Type depType, XmlSchema schema, List<string> addedEnums, string mainGroupName, 
                                                        XmlSchemaComplexContentExtension cmxContentExt = null, XmlSchemaComplexType complType = null, string customAttCheck = "")
        {
            XmlSchemaSequence seq = new XmlSchemaSequence();
            XmlSchemaGroupRef gRef = new XmlSchemaGroupRef();
            gRef.RefName = new XmlQualifiedName(mainGroupName, currentNs.Name);
            seq.Items.Add(gRef);
            foreach (var prop in props)
            {
                if (!prop.CanWrite)
                    continue;

                if (!string.IsNullOrEmpty(customAttCheck) && prop.DeclaringType == depType)
                {
                    var categoryAtt = prop.GetCustomAttribute<CategoryAttribute>(true);
                    if (categoryAtt == null || categoryAtt.Category != customAttCheck)
                        continue;
                }

                XmlSchemaElement elProp = new XmlSchemaElement();
                elProp.Name = depType.Name + "." + prop.Name;
                seq.Items.Add(elProp);

                XmlSchemaAttribute att = new XmlSchemaAttribute();

                List<XmlNode[]> docNodes = ReadDocumentation(string.Format("P:{0}", depType.FullName + "." + prop.Name));
                if (docNodes != null && docNodes.Any())
                {
                    XmlSchemaAnnotation xmlSchemaAnnotation = new XmlSchemaAnnotation();
                    att.Annotation = xmlSchemaAnnotation;
                    if (docNodes[0] != null)
                    {
                        XmlSchemaDocumentation xmlSchemaDocumentation = new XmlSchemaDocumentation();
                        xmlSchemaAnnotation.Items.Add(xmlSchemaDocumentation);
                        xmlSchemaDocumentation.Markup = docNodes[0];
                    }

                    if (docNodes[1] != null)
                    {
                        XmlSchemaDocumentation xmlSchemaDocumentationDE = new XmlSchemaDocumentation();
                        xmlSchemaAnnotation.Items.Add(xmlSchemaDocumentationDE);
                        xmlSchemaDocumentationDE.Language = "de";
                        xmlSchemaDocumentationDE.Markup = docNodes[1];
                    }
                }

                att.Name = prop.Name;

                if (prop.PropertyType == typeof(bool))
                {
                    att.SchemaTypeName = new XmlQualifiedName("frlrfSystemBooleanClassTopic", msNs.Name);
                }

                else if (prop.PropertyType.IsEnum)
                {
                    if (!addedEnums.Contains(prop.PropertyType.Name))
                        CreateEnum(prop, ref schema, ref addedEnums);

                    att.SchemaTypeName = new XmlQualifiedName("enum" + prop.PropertyType.Name,  currentNs.Name);
                }
                else
                {
                    string prpName = string.Format("d{0}Container", prop.PropertyType.Name);

                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Any())
                    {
                        prpName = "d" + prop.PropertyType.Name.TrimEnd(new char[] { '`', '1' });
                        prpName += "Of" + prop.PropertyType.GenericTypeArguments[0].Name;
                        if (!prop.Name.EndsWith("Container"))
                            prpName = prpName + "Container";
                    }


                    if (mainSchema.Contains(prpName))
                    {
                        elProp.SchemaTypeName = new XmlQualifiedName(prpName, msNs.Name);
                    }
                }

                if (cmxContentExt != null)
                    cmxContentExt.Attributes.Add(att);
                else if (complType != null)
                    complType.Attributes.Add(att);
            }
            if (cmxContentExt != null)
                cmxContentExt.Particle = seq;
            else if (complType != null)
                complType.Particle = seq;

        }

        private static void CreateEnum(PropertyInfo propInfo, ref XmlSchema schema, ref List<string> addedEnums)
        {
            var enumValues = Enum.GetValues(propInfo.PropertyType);
            XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType();
            simpleType.Name = "enum" + propInfo.PropertyType.Name;
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
            restriction.BaseTypeName = new XmlQualifiedName("string", xsNs);

            foreach (var val in enumValues)
            {
                XmlSchemaEnumerationFacet eFacet = new XmlSchemaEnumerationFacet();
                eFacet.Value = val.ToString();
                restriction.Facets.Add(eFacet);
            }
            simpleType.Content = restriction;
            schema.Items.Add(simpleType);
            addedEnums.Add(propInfo.PropertyType.Name);
        }

        private static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] {doc.CreateTextNode(text)};
        }

        private static List<XmlNode[]> ReadDocumentation(string elementName)
        {
            if (documentation == null)
                return null;

            var nodes = documentation.GetElementsByTagName("member");
            var node = nodes.OfType<XmlNode>().FirstOrDefault(c => c.Attributes.OfType<XmlAttribute>().FirstOrDefault().InnerText == elementName);

            if (node == null)
                return null;

            //[0] en
            //[1] de
            List<XmlNode[]> result = new List<XmlNode[]>();

            try
            {
                var enSummary = node.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => x.Attributes.Count == 0);
                var deSummary = node.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => x.Attributes.OfType<XmlAttribute>().FirstOrDefault(c => c.Name == "xml:lang" && c.Value == "de") != null);

                if (enSummary != null && !string.IsNullOrEmpty(enSummary.InnerXml))
                    result.Add(TextToNodeArray(enSummary.InnerXml.Trim().Replace("<see cref=\"T:", "")));
                else
                    result.Add(null);

                if (deSummary != null && !string.IsNullOrEmpty(deSummary.InnerXml))
                    result.Add(TextToNodeArray(deSummary.InnerXml.Trim()));
                else
                    result.Add(null);
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("CodeCompletionXsdGenerator", "ReadDocumentation", e.Message + e.InnerException != null ? System.Environment.NewLine + "Inner:" + e.InnerException.Message : "");
            }

            return result;
        }
    }

    /// <summary>
    /// Code completion schema generator for VB controls
    /// </summary>
    public class CodeCompletionXsdGeneratorVB
    {
        /// <summary>
        /// Run tool for gip.core.layoutengine.avui
        /// </summary>
        public static void RunTool()
        {
            CodeCompletionXsdGenerator gen = new CodeCompletionXsdGenerator();
            CodeCompletionXsdGenerator.baseDir =  AppContext.BaseDirectory + @"..\..\..\..\";

            XmlSchemaImport importSchema = new XmlSchemaImport();
            importSchema.Namespace = CodeCompletionXsdGenerator.msNs.Name;
            importSchema.SchemaLocation = "pack://application:,,,/gip.core.layoutengine.avui;component/VBXMLEditorSchemas/XamlPresentation2006.xsd";

            List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchemaList = new List<Tuple<XmlSchemaImport, XmlQualifiedName>>();
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchema, CodeCompletionXsdGenerator.msNs));

            gen.RunTool("gip.core.layoutengine.avui", CodeCompletionXsdGenerator.vbNs, new Type[] { typeof(AvaloniaObject), typeof(MarkupExtension) }, importSchemaList, "VBControls",
                        new Func<XmlSchemaElement, Type, bool>((e, t) => AddTypeSpecificSubGroup(e, t)), CodeCompletionXsdGenerator.baseDir + CodeCompletionXsdGenerator.vbSchemaPath, 
                        "VB", "VBControl");

        }
        
        private static bool AddTypeSpecificSubGroup(XmlSchemaElement schemaElement, Type depType)
        {
            if (typeof(DataGridColumn).IsAssignableFrom(depType))
                schemaElement.SubstitutionGroup = new XmlQualifiedName("sgDataGrid", CodeCompletionXsdGenerator.msNs.Name);

            if (typeof(Binding).IsAssignableFrom(depType))
                schemaElement.SubstitutionGroup = new XmlQualifiedName("sgBindingBase", CodeCompletionXsdGenerator.msNs.Name);

            return true;
        }

    }
}
