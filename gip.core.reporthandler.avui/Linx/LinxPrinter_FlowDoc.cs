// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.reporthandler;
using gip.core.reporthandler.avui.Flowdoc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using static gip.core.reporthandler.avui.LinxPrintJob;

namespace gip.core.reporthandler.avui
{
    public partial class LinxPrinter
    {

        #region Rendering
        public override void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;

            //if (UseRemoteReport)
            //{
            //    LoadPrintMessage(linxPrintJob, flowDoc);
            //}

            base.OnRenderFlowDocument(printJob, flowDoc);

            if (!UseRemoteReport)
            {
                AddDeleteReportToJob(linxPrintJob, flowDoc);

                int msgLengthInBytes = linxPrintJob.LinxFields.Sum(c => BitConverter.ToInt16(c.Header.FieldLengthInBytes, 0)) + LinxMessageHeader.DefaultHeaderLength;
                int msgLengthInRasters = linxPrintJob.LinxFields.Sum(c => BitConverter.ToInt16(c.Header.FieldLengthInRasters, 0)) + LinxMessageHeader.DefaultHeaderLength;
                LinxMessageHeader linxMessageHeader = GetLinxMessageHeader(linxPrintJob.Name, linxPrintJob.RasterName, 1, (short)msgLengthInBytes, (short)msgLengthInRasters);
                List<byte[]> headerBytes = linxMessageHeader.GetBytes();

                List<byte[]> fieldData = new List<byte[]>();

                foreach (LinxField linxField in linxPrintJob.LinxFields)
                {
                    List<byte[]> fieldBytes = linxField.GetBytes();
                    fieldData.AddRange(fieldBytes);
                }


                string headerPresentation = ByteStrPresentation(headerBytes);
                string fieldPresentation = ByteStrPresentation(fieldData);

                List<byte[]> downloadData = new List<byte[]>();
                downloadData.AddRange(headerBytes);
                downloadData.AddRange(fieldData);

                byte[] data = GetData(LinxASCIControlCharacterEnum.EM, downloadData.ToArray().SelectMany(c => c).ToArray());
                linxPrintJob.PacketsForPrint.Add(new LinxPrintJob.Telegram(LinxPrintJobTypeEnum.DownloadReport, data));
            }

            AddPrintMessageToJob(linxPrintJob, flowDoc);

            if (UseRemoteReport)
            {
                // field length
                int dataLength = linxPrintJob.RemoteFieldValues.Sum(c => c.Length);
                byte[] dataLengthBy = BitConverter.GetBytes(dataLength);

                // prepare data
                List<byte[]> dataArr = linxPrintJob.RemoteFieldValues;
                dataArr.Insert(0, dataLengthBy);
                byte[] inputData = LinxHelper.Combine(dataArr);

                // generate request array and add to queue
                byte[] data = GetData(LinxASCIControlCharacterEnum.GS, inputData);
                linxPrintJob.PacketsForPrint.Add(new LinxPrintJob.Telegram(LinxPrintJobTypeEnum.PrintRemote, data));
            }

            AddPrintCommandToJob(linxPrintJob);
        }


        public override void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionTableHeader(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderSectionTableFooter(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn)
        {
        }

        public override void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow)
        {
        }

        public override void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow)
        {
        }

        public override void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph)
        {
        }

        public override void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph)
        {

        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
            if (UseRemoteReport)
            {

            }
            else
            {

            }
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                AddTextValueForRemoteField(linxPrintJob, inlineDocumentValue.Name, inlineDocumentValue.Text);
            }
            else
            {
                AddTextValueToPrintMessage(linxPrintJob, inlineDocumentValue, inlineDocumentValue.AggregateGroup, inlineDocumentValue.Text);
            }
        }

        public override void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                AddTextValueForRemoteField(linxPrintJob, inlineACMethodValue.Name, inlineACMethodValue.Text);
            }
            else
            {
                AddTextValueToPrintMessage(linxPrintJob, inlineACMethodValue, inlineACMethodValue.AggregateGroup, inlineACMethodValue.Text);
            }
        }

        public override void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                AddTextValueForRemoteField(linxPrintJob, inlineTableCellValue.Name, inlineTableCellValue.Text);
            }
            else
            {
                AddTextValueToPrintMessage(linxPrintJob, inlineTableCellValue, inlineTableCellValue.AggregateGroup, inlineTableCellValue.Text);
            }
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            string barcodeValue = "";

            BarcodeType[] ianBarcodeTypes = new BarcodeType[] { BarcodeType.CODE128, BarcodeType.CODE128A, BarcodeType.CODE128B, BarcodeType.CODE128C };
            bool isIanCodeType = ianBarcodeTypes.Contains(inlineBarcode.BarcodeType);
            Dictionary<string, string> barcodeValues = new Dictionary<string, string>();

            if (isIanCodeType)
            {
                if (inlineBarcode.BarcodeValues != null && inlineBarcode.BarcodeValues.Any())
                {
                    foreach (BarcodeValue tmpBarcodeValue in inlineBarcode.BarcodeValues)
                    {
                        try
                        {
                            string ai = tmpBarcodeValue.AI;
                            string tmpValue = tmpBarcodeValue.Value?.ToString();
                            if (tmpValue != null)
                            {
                                barcodeValues.Add(ai, tmpValue);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                barcodeValue = GS1.Generate(barcodeValues, isIanCodeType);
            }
            else
            {
                barcodeValue = inlineBarcode.Value.ToString();
            }

            if (UseRemoteReport)
            {
                AddTextValueForRemoteField(linxPrintJob, inlineBarcode.Name, barcodeValue);
            }
            else
            {
                AddBarcodeValueToPrintMessage(linxPrintJob, inlineBarcode.AggregateGroup, barcodeValue);
            }
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
            LinxPrintJob linxPrintJob = (LinxPrintJob)printJob;
            if (UseRemoteReport)
            {
                AddTextValueForRemoteField(linxPrintJob, inlineBoolValue.Name, inlineBoolValue.Value.ToString());
            }
            else
            {
                AddTextValueToPrintMessage(linxPrintJob, null, inlineBoolValue.AggregateGroup, inlineBoolValue.Value.ToString());
            }
        }

        public override void OnRenderRun(PrintJob printJob, Run run)
        {
        }

        public override void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak)
        {
        }
        #endregion
    
    
    }
}
