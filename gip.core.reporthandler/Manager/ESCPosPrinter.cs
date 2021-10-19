using gip.core.datamodel;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using System.Text;
using System;
using System.Threading;

using static ESCPOS.Commands;
using static gip.core.reporthandler.ESCPosExt;
using ESCPOS;
using ESCPOS.Utils;
using System.Collections.Generic;
using System.Windows;

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
            printContext.PrintFormats = new List<PrintFormat>();
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
                    printContext.Main = printContext.Main.Add(Commands.FullPaperCut);
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


        public override void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            SetFormatTextElement(printContext, block);
        }

        public override void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            SetFormatTextElement(printContext, sectionReportHeader);
        }

        public override void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            SetFormatTextElement(printContext, sectionReportFooter);
        }

        public override void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            SetFormatTextElement(printContext, sectionDataGroup);
        }

        public override void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        #endregion

        #region Methods -> Render -> Table


        public override void OnRenderSectionTableHeader(PrintContext printContext, Table table)
        {
            PrintFormat printFormat = new PrintFormat();
            printFormat.FontSize = table.FontSize;
            printFormat.FontWeight = table.FontWeight;
            printFormat.TextAlignment = table.TextAlignment;
            printContext.PrintFormats.Add(printFormat);
        }

        public override void OnRenderSectionTableFooter(PrintContext printContext, Table table)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn)
        {
            //
        }

        public override void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            SetFormatTextElement(printContext, tableRowGroup);
        }

        public override void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
            SetFormatTextElement(printContext, tableRow);
        }

        public override void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        #endregion

        #region Methods -> Render -> Inlines

        public override void OnRenderParagraphHeader(PrintContext printContext, Paragraph paragraph)
        {
            SetFormatTextElement(printContext, paragraph);
        }

        public override void OnRenderParagraphFooter(PrintContext printContext, Paragraph paragraph)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
            SetFormatTextElement(printContext, inlineContextValue);

            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            printContext.Main = printContext.Main.Add(Commands.LF, SetFormat(printContext, defaultPrintFormat), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineContextValue.Text));

            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
            SetFormatTextElement(printContext, inlineDocumentValue);

            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            printContext.Main = printContext.Main.Add(Commands.LF, SetFormat(printContext, defaultPrintFormat), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineDocumentValue.Text));

            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
            SetFormatTextElement(printContext, inlineACMethodValue);

            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            printContext.Main = printContext.Main.Add(Commands.LF, SetFormat(printContext, defaultPrintFormat), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineACMethodValue.Text));


            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);

        }

        public override void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {
            SetFormatTextElement(printContext, inlineTableCellValue);

            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            printContext.Main = printContext.Main.Add(Commands.LF, SetFormat(printContext, defaultPrintFormat), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineTableCellValue.Text));

            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
            string barcodeValue = inlineBarcode.Value.ToString();
            if (inlineBarcode.BarcodeType == BarcodeType.QRCODE)
            {
                QRCodeSizeExt qRCodeSizeExt = QRCodeSizeExt.Six;
                if (inlineBarcode.BarcodeWidth >= 2 && inlineBarcode.BarcodeWidth <= 10)
                    qRCodeSizeExt = (QRCodeSizeExt)inlineBarcode.BarcodeWidth;
                printContext.Main = printContext.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Center), Commands.SelectPrintMode(PrintMode.Reset),
                    PrintQRCodeExt(barcodeValue, QRCodeModel.Model1, QRCodeCorrection.Percent30, qRCodeSizeExt));
            }
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
            SetFormatTextElement(printContext, inlineBoolValue);

            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            printContext.Main = printContext.Main.Add(Commands.LF, SetFormat(printContext, defaultPrintFormat), Commands.SelectPrintMode(PrintMode.Reset), printContext.Encoding.GetBytes(inlineBoolValue.Value.ToString()));

            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }
        #endregion


        #region Methods -> Render-> Common

        private void SetFormatTextElement(PrintContext printContext, TextElement textElement)
        {
            PrintFormat printFormat = new PrintFormat();
            printFormat.FontSize = textElement.FontSize;
            printFormat.FontWeight = textElement.FontWeight;
            printContext.PrintFormats.Add(printFormat);
        }

        protected byte[] SetFormat(PrintContext printContext, PrintFormat defaultPrintFormat)
        {
            Justification justification = Justification.Left;
            if (defaultPrintFormat.TextAlignment != null && defaultPrintFormat.TextAlignment != TextAlignment.Left)
                if (defaultPrintFormat.TextAlignment == TextAlignment.Right)
                    justification = Justification.Right;
                else if (defaultPrintFormat.TextAlignment == TextAlignment.Center)
                    justification = Justification.Center;
                else if (defaultPrintFormat.TextAlignment == TextAlignment.Justify)
                    justification = Justification.Center;


            CharSizeWidth charSizeWidth = CharSizeWidth.Normal;
            CharSizeHeight charSizeHeight = CharSizeHeight.Normal;

            if (defaultPrintFormat.FontSize != null)
            {
                if (defaultPrintFormat.FontSize >= 14)
                {
                    charSizeWidth = CharSizeWidth.Double;
                    charSizeHeight = CharSizeHeight.Double;
                }
                else if (defaultPrintFormat.FontSize >= 16)
                {
                    charSizeWidth = CharSizeWidth.Triple;
                    charSizeHeight = CharSizeHeight.Triple;
                }
                else if (defaultPrintFormat.FontSize >= 18)
                {
                    charSizeWidth = CharSizeWidth.Quadruple;
                    charSizeHeight = CharSizeHeight.Quadruple;
                }
            }

            return Commands.SelectJustification(justification).Add(SelectCharSize(charSizeWidth, charSizeHeight));
        }

        #endregion

        #endregion

        #endregion

    }
}
