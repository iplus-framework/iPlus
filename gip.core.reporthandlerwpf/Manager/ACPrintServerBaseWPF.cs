using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using gip.core.reporthandlerwpf.Flowdoc;
using gip.core.layoutengine;
using gip.core.reporthandler;

namespace gip.core.reporthandlerwpf
{
    public abstract class ACPrintServerBaseWPF : ACPrintServerBase
    {
        public ACPrintServerBaseWPF(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override byte[] OnDoPrint(ACClassDesign aCClassDesign, int codePage, ReportData reportData)
        {
            byte[] bytes = null;
            try
            {
                // FlowDocument generate (separate thread)
                using (ReportDocument reportDocument = new ReportDocument(aCClassDesign.XMLDesign))
                {
                    FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
                    PrintContext printContext = GetPrintContext(flowDoc, codePage);
                    bytes = printContext.Main;
                    return bytes;
                }
            }
            catch (Exception e)
            {
                this.Messages.LogException(this.GetACUrl(), "InvokeAsync", e);
            }
            return bytes;
        }

        /// <summary>
        /// Component specific implementation writing data from ReportData to network stream 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected PrintContext GetPrintContext(FlowDocument flowDocument, int CodePage)
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
                    Messages.LogException(GetACUrl(), nameof(GetPrintContext), ex);
                }
            }

            PrintContext printContext = new PrintContext();
            printContext.FlowDocument = flowDocument;
            printContext.Encoding = encoder;
            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = 1;
            OnRenderFlowDocment(printContext, printContext.FlowDocument);
            return printContext;
        }

        #region Methods -> Render

        #region Methods -> Render -> Block
        public virtual void OnRenderFlowDocment(PrintContext printContext, FlowDocument flowDoc)
        {
            OnRenderBlocks(printContext, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        protected void OnRenderBlocks(PrintContext printContext, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                OnRenderBlock(printContext, block, position);
        }

        protected void OnRenderBlock(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            OnRenderBlockHeader(printContext, block, position);
            if (block is SectionReportHeader)
                OnRenderSectionReportHeader(printContext, (SectionReportHeader)block);
            else if (block is SectionReportFooter)
                OnRenderSectionReportFooter(printContext, (SectionReportFooter)block);
            else if (block is SectionDataGroup)
                OnRenderSectionDataGroup(printContext, (SectionDataGroup)block);
            else if (block is Table)
                OnRenderSectionTable(printContext, (Table)block);
            else if (block is Paragraph)
                OnRenderParagraph(printContext, (Paragraph)block);
            OnRenderBlockFooter(printContext, block, position);
        }

        public abstract void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position);

        public abstract void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position);

        protected void OnRenderSectionReportHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            OnRenderSectionReportHeaderHeader(printContext, sectionReportHeader);
            OnRenderBlocks(printContext, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportHeaderFooter(printContext, sectionReportHeader);
        }

        public abstract void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader);

        public abstract void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader);

        protected void OnRenderSectionReportFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            OnRenderSectionReportFooterHeader(printContext, sectionReportFooter);
            OnRenderBlocks(printContext, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportFooterFooter(printContext, sectionReportFooter);
        }

        public abstract void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter);

        public abstract void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter);

        protected virtual void OnRenderSectionDataGroup(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            OnRenderSectionDataGroupHeader(printContext, sectionDataGroup);
            OnRenderBlocks(printContext, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            OnRenderSectionDataGroupFooter(printContext, sectionDataGroup);
        }

        public abstract void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup);

        public abstract void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup);
        #endregion

        #region Methods -> Render -> Table

        protected void OnRenderSectionTable(PrintContext printContext, Table table)
        {
            OnRenderSectionTableHeader(printContext, table);

            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = table.Columns.Count;

            foreach (TableColumn tableColumn in table.Columns)
                OnRenderTableColumn(printContext, tableColumn);

            foreach (TableRowGroup tableRowGroup in table.RowGroups)
                OnRenderTableRowGroup(printContext, tableRowGroup);
            OnRenderSectionTableFooter(printContext, table);

            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = 1;
        }

        public abstract void OnRenderSectionTableHeader(PrintContext printContext, Table table);

        public abstract void OnRenderSectionTableFooter(PrintContext printContext, Table table);

        public abstract void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn);

        protected void OnRenderTableRowGroup(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            OnRenderTableRowGroupHeader(printContext, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                OnRenderTableRow(printContext, tableRow);
            OnRenderTableRowGroupFooter(printContext, tableRowGroup);
        }

        public abstract void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup);

        public abstract void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup);

        protected void OnRenderTableRow(PrintContext printContext, TableRow tableRow)
        {
            OnRenderTableRowHeader(printContext, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
            {
                printContext.ColumnMultiplier = tableRow.Cells.IndexOf(tableCell);
                OnRenderTableCell(printContext, tableCell);
            }
            OnRenderTableRowFooter(printContext, tableRow);
        }
        public abstract void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow);

        public abstract void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow);

        protected void OnRenderTableCell(PrintContext printContext, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                OnRenderBlock(printContext, block, BlockDocumentPosition.InTable);
        }

        #endregion


        #region Methods -> Render -> Inlines

        protected void OnRenderParagraph(PrintContext printContext, Paragraph paragraph)
        {
            OnRenderParagraphHeader(printContext, paragraph);
            foreach (Inline inline in paragraph.Inlines)
            {
                if (inline is InlineContextValue)
                    OnRenderInlineContextValue(printContext, (InlineContextValue)inline);
                else if (inline is InlineDocumentValue)
                    OnRenderInlineDocumentValue(printContext, (InlineDocumentValue)inline);
                else if (inline is InlineACMethodValue)
                    OnRenderInlineACMethodValue(printContext, (InlineACMethodValue)inline);
                else if (inline is InlineTableCellValue)
                    OnRenderInlineTableCellValue(printContext, (InlineTableCellValue)inline);
                else if (inline is InlineBarcode)
                    OnRenderInlineBarcode(printContext, (InlineBarcode)inline);
                else if (inline is InlineBoolValue)
                    OnRenderInlineBoolValue(printContext, (InlineBoolValue)inline);
                else if (inline is Run)
                    OnRenderRun(printContext, (Run)inline);
                else if (inline is LineBreak)
                    OnRenderLineBreak(printContext, (LineBreak)inline);

            }
            OnRenderParagraphFooter(printContext, paragraph);
        }

        public abstract void OnRenderParagraphHeader(PrintContext printContext, Paragraph paragraph);

        public abstract void OnRenderParagraphFooter(PrintContext printContext, Paragraph paragraph);

        public abstract void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue);

        public abstract void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue);

        public abstract void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue);

        public abstract void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue);

        public abstract void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode);

        public abstract void OnRenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue);

        public abstract void OnRenderRun(PrintContext printContext, Run run);

        public abstract void OnRenderLineBreak(PrintContext printContext, LineBreak lineBreak);
        #endregion

        #endregion
    }
}
