using gip.core.reporthandler.Flowdoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    public partial class LP4Printer 
    {
        public override void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            base.OnRenderFlowDocument(printJob, flowDoc);
        }

        public override void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
        }

        public override void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue)
        {
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {

            }
        }

        public override void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderRun(PrintJob printJob, Run run)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionTableFooter(PrintJob printJob, Table table)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderSectionTableHeader(PrintJob printJob, Table table)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow)
        {
            throw new NotImplementedException();
        }
    }
}
