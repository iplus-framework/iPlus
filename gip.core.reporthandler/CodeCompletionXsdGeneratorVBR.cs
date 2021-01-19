﻿using gip.core.layoutengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Windows.Controls;

namespace gip.core.reporthandler
{
    /// <summary>
    /// Code completion schema generator for VB report controls
    /// </summary>
    public class CodeCompletionXsdGeneratorVBR : MarshalByRefObject
    {
        internal static XmlQualifiedName vbrNs = new XmlQualifiedName("http://www.iplus-framework.com/report/xaml", "vbr");
        internal static string vbrSchemaPath = @"\gip.core.reporthandler\VBXMLEditorSchemas\VBRSchema.xsd";

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
            importSchema.SchemaLocation = "pack://application:,,,/gip.core.layoutengine;component/VBXMLEditorSchemas/XamlPresentation2006.xsd"; ;

            List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchemaList = new List<Tuple<XmlSchemaImport, XmlQualifiedName>>();
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchema, CodeCompletionXsdGenerator.msNs));

            gen.RunTool("gip.core.reporthandler", vbrNs, new Type[] { typeof(FrameworkContentElement), typeof(Image) }, importSchemaList, "VBReportItems",
                        null, CodeCompletionXsdGenerator.baseDir + vbrSchemaPath);
        }
    }
}
