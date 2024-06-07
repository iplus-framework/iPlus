using gip.core.datamodel;
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
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {
                lp4PrintJob.AddLayoutVariable(inlineACMethodValue.Name, inlineACMethodValue.Text);
            }
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            LP4PrintJob lp4PrintJob = (LP4PrintJob)printJob;

            if (lp4PrintJob != null)
            {
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

                lp4PrintJob.AddLayoutVariable(inlineBarcode.Name, barcodeValue);
            }
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {
                lp4PrintJob.AddLayoutVariable(inlineBoolValue.Name, inlineBoolValue.Value.ToString());
            }
        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {
                lp4PrintJob.AddLayoutVariable(inlineContextValue.Name, inlineContextValue.Text);
            }
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {
                lp4PrintJob.AddLayoutVariable(inlineDocumentValue.Name, inlineDocumentValue.Text);
            }
        }

        public override void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue)
        {
            LP4PrintJob lp4PrintJob = printJob as LP4PrintJob;
            if (lp4PrintJob != null)
            {
                lp4PrintJob.AddLayoutVariable(inlineTableCellValue.Name, inlineTableCellValue.Text);
            }
        }

        public override void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak)
        {
        }

        public override void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph)
        {
        }

        public override void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph)
        {
        }

        public override void OnRenderRun(PrintJob printJob, Run run)
        {
        }

        public override void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
        }

        public override void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
        }

        public override void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
        }

        public override void OnRenderSectionTableFooter(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderSectionTableHeader(PrintJob printJob, Table table)
        {
        }

        public override void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn)
        {
        }

        public override void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow)
        {
        }

        public override void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup)
        {
        }

        public override void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow)
        {
        }
    }
}
