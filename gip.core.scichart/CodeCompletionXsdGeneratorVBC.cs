using gip.core.layoutengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Schema;

namespace gip.core.scichart
{
    public class CodeCompletionXsdGeneratorVBC : MarshalByRefObject
    {
        internal static XmlQualifiedName sNs = new XmlQualifiedName("http://schemas.abtsoftware.co.uk/scichart", "s");
        internal static XmlQualifiedName vbcNs = new XmlQualifiedName("http://www.iplus-framework.com/scichart/xaml", "vbc");
        internal static string vbcSchemaPath = @"\gip.core.scichart\VBXMLEditorSchemas\VBCSchema.xsd";
        internal static string sciChartSchemaPath = @"\gip.core.scichart\VBXMLEditorSchemas\SciChartSchema.xsd";


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
            importSchema.Namespace = CodeCompletionXsdGenerator.msNs.Name;
            importSchema.SchemaLocation = "pack://application:,,,/gip.core.layoutengine;component/VBXMLEditorSchemas/XamlPresentation2006.xsd";

            List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchemaList = new List<Tuple<XmlSchemaImport, XmlQualifiedName>>();
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchema, CodeCompletionXsdGenerator.msNs));

            // generates xsd schema from the SciChart library
            gen.RunTool("Abt.Controls.SciChart.Wpf", sNs, new Type[] { typeof(DependencyObject), typeof(DependencyObject) }, importSchemaList, "SciChartItems", null,
                       CodeCompletionXsdGenerator.baseDir + sciChartSchemaPath);

            XmlSchemaImport importSciChartSchema = new XmlSchemaImport();
            importSciChartSchema.Namespace = sNs.Name;
            importSciChartSchema.SchemaLocation = "pack://application:,,,/gip.core.scichart;component/VBXMLEditorSchemas/SciChartSchema.xsd";
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSciChartSchema, sNs));

            gen.RunTool("gip.core.scichart", vbcNs, new Type[] { typeof(DependencyObject), typeof(SciChart.Charting.Visuals.Axes.LabelProviders.LabelProviderBase) }, importSchemaList, 
                        "VBSciChartItems", null, CodeCompletionXsdGenerator.baseDir + vbcSchemaPath);
        }
    }
}
