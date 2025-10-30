// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using gip.core.layoutengine.avui;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace gip.core.visualcontrols.avui
{
    public class CodeCompletionXsdGeneratorVBV
    {
        internal static XmlQualifiedName vbvNs = new XmlQualifiedName("http://www.iplus-framework.com/visual/xaml", "vbv");
        internal static string vbvSchemaPath = @"\gip.core.visualcontrols.avui\VBXMLEditorSchemas\VBVSchema.xsd";

        public static void RunTool()
        {
            CodeCompletionXsdGenerator gen = new CodeCompletionXsdGenerator();
            CodeCompletionXsdGenerator.baseDir = AppContext.BaseDirectory + @"..\..\..\..\";

            XmlSchemaImport importSchema = new XmlSchemaImport();
            importSchema.Namespace = CodeCompletionXsdGenerator.vbNs.Name;
            importSchema.SchemaLocation = "pack://application:,,,/gip.core.layoutengine.avui;component/VBXMLEditorSchemas/VBSchema.xsd";

            XmlSchemaImport importSchemaWPF = new XmlSchemaImport();
            importSchemaWPF.Namespace = CodeCompletionXsdGenerator.msNs.Name;
            importSchemaWPF.SchemaLocation = "pack://application:,,,/gip.core.layoutengine.avui;component/VBXMLEditorSchemas/XamlPresentation2006.xsd";

            List<Tuple<XmlSchemaImport, XmlQualifiedName>> importSchemaList = new List<Tuple<XmlSchemaImport, XmlQualifiedName>>();
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchema, CodeCompletionXsdGenerator.vbNs));
            importSchemaList.Add(new Tuple<XmlSchemaImport, XmlQualifiedName>(importSchemaWPF, CodeCompletionXsdGenerator.msNs));

            gen.RunTool("gip.core.visualcontrols.avui", vbvNs, new Type[] { typeof(AvaloniaObject), typeof(AvaloniaObject) }, importSchemaList, "VBVisualControls",
                        null, CodeCompletionXsdGenerator.baseDir + vbvSchemaPath);
        }
    }
}
