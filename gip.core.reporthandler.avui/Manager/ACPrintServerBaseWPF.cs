// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using gip.core.reporthandler.avui.Flowdoc;
using gip.core.layoutengine.avui;
using gip.core.reporthandler;

namespace gip.core.reporthandler.avui
{
    public abstract class ACPrintServerBaseWPF : ACPrintServerBase
    {
        public ACPrintServerBaseWPF(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override PrintJob OnDoPrint(ACClassDesign aCClassDesign, int codePage, ReportData reportData)
        {
            PrintJob printJob = null;

            try
            {
                // FlowDocument generate (separate thread)
                using (ReportDocument reportDocument = new ReportDocument(aCClassDesign.XAMLDesign))
                {
                    FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
                    printJob = GetPrintJob(aCClassDesign.ACIdentifier, flowDoc);
                }
            }
            catch (Exception e)
            {
                this.Messages.LogException(this.GetACUrl(), "InvokeAsync", e);
            }

            return printJob;
        }

        /// <summary>
        /// Component specific implementation writing data from ReportData to network stream 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual PrintJob GetPrintJob(string reportName, FlowDocument flowDocument)
        {
            Encoding encoder = Encoding.ASCII;
            VBFlowDocument vBFlowDocument = flowDocument as VBFlowDocument;

            int? codePage = null;

            if (vBFlowDocument != null && vBFlowDocument.CodePage > 0)
            {
                codePage = vBFlowDocument.CodePage;
            }
            else if (CodePage > 0)
            {
                codePage = CodePage;
            }


            if (codePage != null)
            {
                try
                {
                    encoder = Encoding.GetEncoding(codePage.Value);
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), nameof(GetPrintJob), ex);
                }
            }

            PrintJobWPF printJob = new PrintJobWPF();
            printJob.FlowDocument = flowDocument;
            printJob.Encoding = encoder;
            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
            OnRenderFlowDocument(printJob, printJob.FlowDocument);
            return printJob;
        }

        #region Methods -> Render

        #region Methods -> Render -> Block

        public virtual void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            OnRenderBlocks(printJob, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        protected void OnRenderBlocks(PrintJob printJob, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                OnRenderBlock(printJob, block, position);
        }

        protected void OnRenderBlock(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
            OnRenderBlockHeader(printJob, block, position);
            if (block is SectionReportHeader)
                OnRenderSectionReportHeader(printJob, (SectionReportHeader)block);
            else if (block is SectionReportFooter)
                OnRenderSectionReportFooter(printJob, (SectionReportFooter)block);
            else if (block is SectionDataGroup)
                OnRenderSectionDataGroup(printJob, (SectionDataGroup)block);
            else if (block is Table)
                OnRenderSectionTable(printJob, (Table)block);
            else if (block is Paragraph)
                OnRenderParagraph(printJob, (Paragraph)block);
            OnRenderBlockFooter(printJob, block, position);
        }

        public abstract void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position);

        public abstract void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position);

        protected void OnRenderSectionReportHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            OnRenderSectionReportHeaderHeader(printJob, sectionReportHeader);
            OnRenderBlocks(printJob, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportHeaderFooter(printJob, sectionReportHeader);
        }

        public abstract void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader);

        public abstract void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader);

        protected void OnRenderSectionReportFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            OnRenderSectionReportFooterHeader(printJob, sectionReportFooter);
            OnRenderBlocks(printJob, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportFooterFooter(printJob, sectionReportFooter);
        }

        public abstract void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter);

        public abstract void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter);

        protected virtual void OnRenderSectionDataGroup(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            OnRenderSectionDataGroupHeader(printJob, sectionDataGroup);
            OnRenderBlocks(printJob, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            OnRenderSectionDataGroupFooter(printJob, sectionDataGroup);
        }

        public abstract void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup);

        public abstract void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup);

        #endregion

        #region Methods -> Render -> Table

        protected void OnRenderSectionTable(PrintJob printJob, Table table)
        {
            OnRenderSectionTableHeader(printJob, table);

            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = table.Columns.Count;

            foreach (TableColumn tableColumn in table.Columns)
                OnRenderTableColumn(printJob, tableColumn);

            foreach (TableRowGroup tableRowGroup in table.RowGroups)
                OnRenderTableRowGroup(printJob, tableRowGroup);
            OnRenderSectionTableFooter(printJob, table);

            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
        }

        public abstract void OnRenderSectionTableHeader(PrintJob printJob, Table table);

        public abstract void OnRenderSectionTableFooter(PrintJob printJob, Table table);

        public abstract void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn);

        protected void OnRenderTableRowGroup(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            OnRenderTableRowGroupHeader(printJob, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                OnRenderTableRow(printJob, tableRow);
            OnRenderTableRowGroupFooter(printJob, tableRowGroup);
        }

        public abstract void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup);

        public abstract void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup);

        protected void OnRenderTableRow(PrintJob printJob, TableRow tableRow)
        {
            OnRenderTableRowHeader(printJob, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
            {
                printJob.ColumnMultiplier = tableRow.Cells.IndexOf(tableCell);
                OnRenderTableCell(printJob, tableCell);
            }
            OnRenderTableRowFooter(printJob, tableRow);
        }
        public abstract void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow);

        public abstract void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow);

        protected void OnRenderTableCell(PrintJob printJob, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                OnRenderBlock(printJob, block, BlockDocumentPosition.InTable);
        }

        #endregion

        #region Methods -> Render -> Inlines

        protected void OnRenderParagraph(PrintJob printJob, Paragraph paragraph)
        {
            OnRenderParagraphHeader(printJob, paragraph);
            foreach (Inline inline in paragraph.Inlines)
            {
                if (inline is InlineContextValue)
                    OnRenderInlineContextValue(printJob, (InlineContextValue)inline);
                else if (inline is InlineDocumentValue)
                    OnRenderInlineDocumentValue(printJob, (InlineDocumentValue)inline);
                else if (inline is InlineACMethodValue)
                    OnRenderInlineACMethodValue(printJob, (InlineACMethodValue)inline);
                else if (inline is InlineTableCellValue)
                    OnRenderInlineTableCellValue(printJob, (InlineTableCellValue)inline);
                else if (inline is InlineBarcode)
                    OnRenderInlineBarcode(printJob, (InlineBarcode)inline);
                else if (inline is InlineBoolValue)
                    OnRenderInlineBoolValue(printJob, (InlineBoolValue)inline);
                else if (inline is Run)
                    OnRenderRun(printJob, (Run)inline);
                else if (inline is LineBreak)
                    OnRenderLineBreak(printJob, (LineBreak)inline);

            }
            OnRenderParagraphFooter(printJob, paragraph);
        }

        public abstract void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph);

        public abstract void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph);

        public abstract void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue);

        public abstract void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue);

        public abstract void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue);

        public abstract void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue);

        public abstract void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode);

        public abstract void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue);

        public abstract void OnRenderRun(PrintJob printJob, Run run);

        public abstract void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak);
        #endregion

        #endregion
    }
}
