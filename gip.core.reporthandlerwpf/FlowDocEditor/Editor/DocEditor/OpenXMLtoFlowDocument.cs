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
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using gip.core.datamodel;
using gip.core.layoutengine;

namespace Document.Editor
{
    public class OpenXMLtoFlowDocument
    {

        private WordprocessingDocument OpenXMLFile = null;
        public OpenXMLtoFlowDocument(string openxmlfilename)
        {
            if (File.Exists(openxmlfilename))
            {
                DocumentFormat.OpenXml.Validation.OpenXmlValidator validator = new DocumentFormat.OpenXml.Validation.OpenXmlValidator();
                int count = 0;
                //For Each [error] As Validation.ValidationErrorInfo In validator.Validate(WordprocessingDocument.Open(openxmlfilename, True))
                //    count += 1
                //    MessageBox.Show([error].Description)
                //Next
                if (count == 0)
                {
                    OpenXMLFile = WordprocessingDocument.Open(openxmlfilename, true);
                }
                else
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "OpenXML File is invalid!", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, null);
                    vbMessagebox.ShowMessageBox();

                    //MessageBoxDialog m = new MessageBoxDialog("OpenXML File is invalid!", "Error", null, 0);
                    //m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));                    
                    //m.Owner = App._GlobalApp.MainWindow;
                    //m.ShowDialog();
                }
            }
            else
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "File Not Found!", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, null);
                vbMessagebox.ShowMessageBox();

                //MessageBoxDialog m = new MessageBoxDialog("File Not Found!", "Error", null, 0);
                //m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
                //m.Owner = App._GlobalApp.MainWindow;
                //m.ShowDialog();
                throw new Exception();
            }
        }

        public void Close()
        {
            OpenXMLFile.Dispose();
        }

        public object Convert()
        {
            FlowDocument flowdoc = new FlowDocument();
            //For Each p As OpenXmlPart In OpenXMLFile.MainDocumentPart.
            //Next
            flowdoc.PageWidth = 816;
            flowdoc.PageHeight = 1056;
            DocumentFormat.OpenXml.Wordprocessing.Body body = OpenXMLFile.MainDocumentPart.Document.Body;
            foreach (DocumentFormat.OpenXml.OpenXmlElement oxmlobject in body.ChildElements)
            {
                DocumentFormat.OpenXml.Wordprocessing.Paragraph par = oxmlobject as DocumentFormat.OpenXml.Wordprocessing.Paragraph;
                if (par != null)
                {
                    System.Windows.Documents.Paragraph p = new System.Windows.Documents.Paragraph();
                    flowdoc.Blocks.Add(p);
                    foreach (DocumentFormat.OpenXml.OpenXmlElement paroxmlobject in par.ChildElements)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.Run ru = paroxmlobject as DocumentFormat.OpenXml.Wordprocessing.Run;
                        if (ru != null)
                        {
                            System.Windows.Documents.Run r = new System.Windows.Documents.Run();
                            r.Text = ru.InnerText;
                            p.Inlines.Add(r);
                        }
                    }
                    //flowdoc.ContentStart.InsertTextInRun(par.InnerText)
                }
            }
            return flowdoc;
        }
    }

}
