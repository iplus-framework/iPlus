using gip.core.datamodel;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using System.Text;
using System;
using System.Threading;

using static ESCPOS.Commands;
using ESCPOS;
using ESCPOS.Utils;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ESCPosPrinter'}de{'ESCPosPrinter'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ESCPosPrinter : ACPrintServerBase
    {

        #region ctor's
        public ESCPosPrinter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Methods (ACPrintServerBase)

        #region Methods -> Render

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void SendDataToPrinter(FlowDocument flowDoc)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            PrintContext printContext = new PrintContext();
            printContext.FlowDocument = flowDoc;
            printContext.Encoding = encoder;
            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = 1;

            WriteToStream(printContext);

            int tries = 0;
            while (tries < PrintTries)
            {
                try
                {
                    printContext.Main.Print(string.Format("{0}:{1}", IPAddress, Port));
                    return;
                }
                catch (Exception e)
                {
                    Root.Messages.LogMessage(eMsgLevel.Exception, GetACUrl(), "SendDataToPrinter", "Exception: " + e.Message);
                    Thread.Sleep(5000);
                }
                tries++;
            }
        }

        #region Methods -> Render -> Block

        public override void OnRenderFlowDocment(PrintContext printContext, FlowDocument flowDoc)
        {
            base.OnRenderFlowDocment(printContext, flowDoc);
            printContext.Main = printContext.Main.Add(Commands.FullPaperCut);
        }

        public override void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            //printContext.Main = printContext.Main.Add(Commands.LF);
        }

        public override void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }


        public override void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }

        public override void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }



        public override void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {

        }

        public override void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            // 
        }


        public override void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            //
        }

        public override void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            // 
        }


        #endregion

        #region Methods -> Render -> Table


        public override void OnRenderSectionTableHeader(PrintContext printContext, Table table)
        {
            //printContext.Main = printContext.Main.Add(Commands.LF);
        }

        public override void OnRenderSectionTableFooter(PrintContext printContext, Table table)
        {
            //
        }


        public override void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn)
        {

        }

        public override void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }

        public override void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }


        public override void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
            //printContext.Main = printContext.Main.Add(Commands.LF);
        }

        public override void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
            //
        }

        public override void OnRenderTableCell(PrintContext printContext, TableCell tableCell)
        {
            base.OnRenderTableCell(printContext, tableCell);
        }

        #endregion

        #region Methods -> Render -> Inlines


        public override void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
            printContext.Main = printContext.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Left), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineContextValue.Text));
        }

        public override void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
            printContext.Main = printContext.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Left), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineDocumentValue.Text));
        }

        public override void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
            // inline.Text
        }

        public override void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {

        }

        public override void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
            string barcodeValue = inlineBarcode.Value.ToString();
            if (inlineBarcode.BarcodeType == BarcodeType.QRCODE)
                printContext.Main = printContext.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Center), Commands.SelectPrintMode(PrintMode.Reset), PrintQRCode(barcodeValue, QRCodeModel.Model1, QRCodeCorrection.Percent30, QRCodeSize.Large));
            else
            {
                BarCodeType barCodeType = BarCodeType.EAN8;
                if (Enum.TryParse(inlineBarcode.BarcodeType.ToString(), out barCodeType))
                    printContext.Main = printContext.Main.Add(Commands.LF, PrintBarCode(barCodeType, barcodeValue));
            }
            printContext.Main = printContext.Main.Add(Commands.LF, Commands.LF, Commands.LF, Commands.LF, Commands.LF);
        }

        public override void OnRenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue)
        {
            printContext.Main = printContext.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Left), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineBoolValue.Value.ToString()));

        }
        #endregion

        #endregion

        #endregion

    }
}
