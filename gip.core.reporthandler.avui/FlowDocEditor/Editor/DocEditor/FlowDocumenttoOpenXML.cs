// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Documents;

namespace Document.Editor
{
    public class FlowDocumenttoOpenXML
    {

        // private FlowDocument flow = null;

        public FlowDocumenttoOpenXML()
        {
        }

        public void Close()
        {
           // flow = null;
        }

        public void Convert(FlowDocument flowdoc, string fileloc)
        {
            using (WordprocessingDocument myDoc = WordprocessingDocument.Create(fileloc, WordprocessingDocumentType.Document))
            {
                // Add a new main document part. 
                MainDocumentPart mainPart = myDoc.AddMainDocumentPart();
                //Create Document tree for simple document. 
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                //Create Body (this element contains
                //other elements that we want to include 
                Body body = new Body();
                //Create paragraph
                foreach (System.Windows.Documents.Paragraph par in flowdoc.Blocks)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    DocumentFormat.OpenXml.Wordprocessing.Run run_paragraph = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    // we want to put that text into the output document
                    TextRange t = new TextRange(par.ElementStart, par.ElementEnd);
                    Text text_paragraph = new Text(t.Text);
                    //Append elements appropriately.
                    run_paragraph.Append(text_paragraph);
                    paragraph.Append(run_paragraph);
                    body.Append(paragraph);
                }
                mainPart.Document.Append(body);
                // Save changes to the main document part.
                mainPart.Document.Save();
            }
        }
    }
}
