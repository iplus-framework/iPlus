using gip.core.layoutengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Schema;

namespace gip.core.visualcontrols
{
    public class CodeCompletionXsdGeneratorVBV : MarshalByRefObject
    {
        internal static XmlQualifiedName vbvNs = new XmlQualifiedName("http://www.iplus-framework.com/visual/xaml", "vbv");
        internal static string vbvSchemaPath = @"\gip.core.visualcontrols\VBXMLEditorSchemas\VBVSchema.xsd";

        public override object InitializeLifetimeService()
        {
            var result = base.InitializeLifetimeService();

            RunTool();

            return result;
        }

        public void RunTool()
        {
            CodeCompletionXsdGenerator gen = new CodeCompletionXsdGenerator();
            CodeCompletionXsdGenerator.baseDir = AppContext.BaseDirectory + @"..\..\";

            XmlSchemaImport importSchema = new XmlSchemaImport();
            importSchema.Namespace = CodeCompletionXsdGenerator.vbNs.Name;
            importSchema.SchemaLocation = "pack://application:,,,/gip.core.layoutengine;component/VBXMLEditorSchemas/VBSchema.xsd";

            XmlSchemaImport importSchemaWPF = new XmlSchemaImport();
            importSchemaWPF.Namespace = CodeCompletionXsdGenerator.msNs.Name;
            importSchemaWPF.SchemaLocation = "pack://application:,,,/gip.core.layoutengine;component/VBXMLEditorSchemas/XamlPresentation2006.xsd";

            List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchemaList = new List<Tuple<XmlSchemaImport, XmlQualifiedName>>();
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchema, CodeCompletionXsdGenerator.vbNs));
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchemaWPF, CodeCompletionXsdGenerator.msNs));

            gen.RunTool("gip.core.visualcontrols", vbvNs, new Type[] { typeof(DependencyObject), typeof(DependencyObject) }, importSchemaList, "VBVisualControls",
                        null, CodeCompletionXsdGenerator.baseDir + vbvSchemaPath);
        }
    }
}
