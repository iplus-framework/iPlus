using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Web.Services.Description;
using CoreWCF.Description;
using CoreWCF.Channels;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Custom behavior to export XML documentation comments to WSDL.
    /// </summary>
    public class WsdlDocumentationAttribute : Attribute, IContractBehavior, IWsdlExportExtension
    {
        private readonly string _xmlPath;
        private XDocument _xmlDoc;

        public WsdlDocumentationAttribute(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            if (_xmlDoc == null && System.IO.File.Exists(_xmlPath))
            {
                try
                {
                    _xmlDoc = XDocument.Load(_xmlPath);
                }
                catch
                {
                    // Ignore XML load errors
                }
            }

            if (_xmlDoc == null) return;

            // 1. Add documentation to the PortType (the Interface)
            var contractType = context.Contract.ContractType;
            string typeKey = $"T:{contractType.FullName}";
            AddDocumentation(context.WsdlPortType, typeKey);

            // 2. Add documentation to Operations
            foreach (var operation in context.Contract.Operations)
            {
                var wsdlOperation = context.GetOperation(operation);
                if (wsdlOperation == null) continue;

                // Operation documentation
                string methodKey = GetMethodKey(operation.SyncMethod ?? operation.BeginMethod ?? operation.TaskMethod);
                AddDocumentation(wsdlOperation, methodKey);
            }

            // 3. Add documentation to XSD Types (xs:complexType) and Elements (xs:element)
            foreach (XmlSchema schema in exporter.GeneratedXmlSchemas.Schemas())
            {
                foreach (XmlSchemaObject schemaObject in schema.Items)
                {
                    if (schemaObject is XmlSchemaComplexType complexType)
                    {
                        ProcessComplexType(complexType);
                    }
                    else if (schemaObject is XmlSchemaElement element)
                    {
                        ProcessElement(element, null);
                    }
                }
            }
        }

        private void ProcessComplexType(XmlSchemaComplexType complexType)
        {
            if (complexType.QualifiedName.IsEmpty) return;

            // Map XSD type to C# type key. 
            // CoreWCF typically uses http://schemas.datacontract.org/2004/07/Namespace
            string clrNamespace = MapXmlNamespaceToClr(complexType.QualifiedName.Namespace);
            string typeKey = $"T:{clrNamespace}.{complexType.Name}";

            AddXsdAnnotation(complexType, typeKey);

            if (complexType.Particle is XmlSchemaSequence sequence)
            {
                foreach (XmlSchemaObject item in sequence.Items)
                {
                    if (item is XmlSchemaElement element)
                    {
                        ProcessElement(element, typeKey);
                    }
                }
            }
        }

        private void ProcessElement(XmlSchemaElement element, string parentTypeKey)
        {
            if (string.IsNullOrEmpty(parentTypeKey))
            {
                // Global element
                string clrNamespace = MapXmlNamespaceToClr(element.QualifiedName.Namespace);
                string typeKey = $"T:{clrNamespace}.{element.Name}";
                AddXsdAnnotation(element, typeKey);
            }
            else
            {
                // Property element
                // parentTypeKey is "T:Namespace.TypeName", replace 'T' with 'P' and add property name
                string propKey = $"P" + parentTypeKey.Substring(1) + "." + element.Name;
                AddXsdAnnotation(element, propKey);
            }
        }

        private string MapXmlNamespaceToClr(string xmlNamespace)
        {
            if (string.IsNullOrEmpty(xmlNamespace)) return string.Empty;

            const string dataContractPrefix = "http://schemas.datacontract.org/2004/07/";
            if (xmlNamespace.StartsWith(dataContractPrefix))
            {
                return xmlNamespace.Substring(dataContractPrefix.Length);
            }

            // Fallback for custom namespaces if any are known or common
            return xmlNamespace;
        }

        private void AddXsdAnnotation(XmlSchemaAnnotated annotated, string key)
        {
            var summary = GetSummary(key);
            if (string.IsNullOrEmpty(summary)) return;

            var annotation = annotated.Annotation ?? new XmlSchemaAnnotation();
            annotated.Annotation = annotation;

            var documentation = new XmlSchemaDocumentation();
            XmlDocument doc = new XmlDocument();
            documentation.Markup = new XmlNode[] { doc.CreateTextNode(summary) };

            annotation.Items.Add(documentation);
        }

        private string GetSummary(string key)
        {
            return _xmlDoc.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == key)?
                .Element("summary")?.Value.Trim();
        }

        private void AddDocumentation(DocumentableItem item, string key)
        {
            var summary = GetSummary(key);
            if (!string.IsNullOrEmpty(summary))
            {
                item.Documentation = summary;
            }
        }

        private string GetMethodKey(MethodInfo method)
        {
            if (method == null) return string.Empty;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return $"M:{method.DeclaringType.FullName}.{method.Name}";
            }

            var paramString = string.Join(",", parameters.Select(p => p.ParameterType.FullName));
            return $"M:{method.DeclaringType.FullName}.{method.Name}({paramString})";
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context) { }
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, CoreWCF.Dispatcher.ClientRuntime clientRuntime) { }
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, CoreWCF.Dispatcher.DispatchRuntime dispatchRuntime) { }
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint) { }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
    }
}
