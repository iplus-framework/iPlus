// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.IO;
using System.Xml;

namespace gip.core.reporthandlerwpf.Flowdoc
{
        public static class XpsPrinterUtils
        {
            public static string GetInputBinName(string printerName, int binIndex, out string nameSpaceURI)
            {
                string binName = string.Empty;
                // get PrintQueue of Printer from the PrintServer
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue printQueue = printServer.GetPrintQueue(printerName);
                // get PrintCapabilities of the printer
                MemoryStream printerCapXmlStream = printQueue.GetPrintCapabilitiesAsXml();
                // read the JobInputBins out of the PrintCapabilities
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(printerCapXmlStream);
                // create NamespaceManager and add PrintSchemaFrameWork-Namespace (should be on DocumentElement of the PrintTicket)
                // Prefix: psf  NameSpace: xmlDoc.DocumentElement.NamespaceURI = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"
                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace(xmlDoc.DocumentElement.Prefix, xmlDoc.DocumentElement.NamespaceURI);
                nameSpaceURI = xmlDoc.ChildNodes[1].GetNamespaceOfPrefix("ns0000");
                // and select all nodes of the bins
                XmlNodeList nodeList = xmlDoc.SelectNodes("//psf:Feature[@name='psk:JobInputBin']/psf:Option", manager);
                // fill Dictionary with the bin-names and values
                if (nodeList.Count > binIndex)
                {
                    binName = nodeList[binIndex].Attributes["name"].Value;
                }
                return binName;
            }

            public static PrintTicket ModifyPrintTicket(PrintTicket ticket, string featureName, string newValue, string nameSpaceURI)
            {
                if (ticket == null)
                {
                    throw new ArgumentNullException("ticket");
                }
                // read Xml of the PrintTicket
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ticket.GetXmlStream());
                // create NamespaceManager and add PrintSchemaFrameWork-Namespace (should be on DocumentElement of the PrintTicket)
                // Prefix: psf  NameSpace: xmlDoc.DocumentElement.NamespaceURI = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"
                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace(xmlDoc.DocumentElement.Prefix, xmlDoc.DocumentElement.NamespaceURI);
                // search node with desired feature we're looking for and set newValue for it
                string xpath = string.Format("//psf:Feature[@name='{0}']/psf:Option", featureName);
                XmlNode node = xmlDoc.SelectSingleNode(xpath, manager);
                if (node != null)
                {
                    if (newValue.StartsWith("ns0000"))
                    {
                        // add namespace to xml doc
                        XmlAttribute namespaceAttribute = xmlDoc.CreateAttribute("xmlns:ns0000");
                        namespaceAttribute.Value = nameSpaceURI;
                        xmlDoc.DocumentElement.Attributes.Append(namespaceAttribute);
                    }
                    node.Attributes["name"].Value = newValue;
                }
                // create a new PrintTicket out of the XML
                MemoryStream printTicketStream = new MemoryStream();
                xmlDoc.Save(printTicketStream);
                printTicketStream.Position = 0;
                PrintTicket modifiedPrintTicket = new PrintTicket(printTicketStream);
                // for testing purpose save the printticket to file
                //FileStream stream = new FileStream("modPrintticket.xml", FileMode.CreateNew, FileAccess.ReadWrite);
                //modifiedPrintTicket.GetXmlStream().WriteTo(stream);
                return modifiedPrintTicket;
            }
        }
}
