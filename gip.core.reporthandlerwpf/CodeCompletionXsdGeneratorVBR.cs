// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Windows.Controls;
using gip.core.layoutengine;

namespace gip.core.reporthandlerwpf
{
    /// <summary>
    /// Code completion schema generator for VB report controls
    /// </summary>
    public class CodeCompletionXsdGeneratorVBR : MarshalByRefObject
    {
        internal static XmlQualifiedName vbrNs = new XmlQualifiedName("http://www.iplus-framework.com/report/xaml", "vbr");
        internal static string vbrSchemaPath = @"\gip.core.reporthandlerwpf\VBXMLEditorSchemas\VBRSchema.xsd";

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
