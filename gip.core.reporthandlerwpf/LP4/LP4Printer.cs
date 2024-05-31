using gip.core.datamodel;
using gip.core.reporthandlerwpf.Flowdoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using gip.core.reporthandler;

namespace gip.core.reporthandlerwpf
{
    public class LP4Printer : ACPrintServerBaseWPF
    {
        #region c'tors

        public LP4Printer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        #region Methods => Render

        public override void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
            throw new NotImplementedException();
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            throw new NotImplementedException();
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

        #endregion

        #endregion
    }
}
