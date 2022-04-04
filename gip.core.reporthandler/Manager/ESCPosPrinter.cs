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
        public override bool SendDataToPrinter(byte[] bytes)
        {
            int tries = 0;
            while (tries < PrintTries)
            {
                try
                {
                    //Console.WriteLine("Print ...");
                    bytes = bytes.Add(Commands.FullPaperCut);
                    //#region Test
                    //Random random = new Random();
                    //int rndNr = random.Next(1, 20);
                    //string filePath = string.Format(@"c:\VarioData\_temp\ECS-{0}-{1}.by", DateTime.Now.ToString("yyyy-mm-dd_HH-mm"), rndNr);
                    //System.IO.File.WriteAllBytes(filePath, bytes);
                    //#endregion
                    bytes.Print(string.Format("{0}:{1}", IPAddress, Port));
                    if (IsAlarmActive(IsConnected) != null)
                        AcknowledgeAlarms();
                    IsConnected.ValueT = true;
                    return true;
                }
                catch (Exception e)
                {
                    string message = String.Format("Print failed on {0}. See log for further details.", IPAddress);
                    if (IsAlarmActive(IsConnected, message) == null)
                        Messages.LogException(GetACUrl(), "SendDataToPrinter(20)", e);
                    OnNewAlarmOccurred(IsConnected, message);
                    IsConnected.ValueT = false;
                    //Root.Messages.LogMessage(eMsgLevel.Exception, GetACUrl(), "SendDataToPrinter", "Exception: " + e.Message);
                    Thread.Sleep(5000);
                }
                tries++;
            }
            return false;
        }

        #region Methods -> Render -> Block


        public override void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {

        }

        public override void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {

        }


        public override void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            SetPrintFormat(printContext, sectionReportHeader, sectionReportHeader.TextAlignment);
        }

        public override void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            SetPrintFormat(printContext, sectionReportFooter, sectionReportFooter.TextAlignment);
        }

        public override void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            SetPrintFormat(printContext, sectionDataGroup, sectionDataGroup.TextAlignment);
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
            SetPrintFormat(printContext, tableRowGroup, null);
        }

        public override void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
            SetPrintFormat(printContext, tableRow, null);
        }

        public override void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        #endregion

        #region Methods -> Render -> Inlines

        public override void OnRenderParagraphHeader(PrintContext printContext, Paragraph paragraph)
        {
            SetPrintFormat(printContext, paragraph, paragraph.TextAlignment);
        }

        public override void OnRenderParagraphFooter(PrintContext printContext, Paragraph paragraph)
        {
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
            SetPrintFormat(printContext, inlineContextValue, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, inlineContextValue.Text);
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
            SetPrintFormat(printContext, inlineDocumentValue, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, inlineDocumentValue.Text);
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
            SetPrintFormat(printContext, inlineACMethodValue, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, inlineACMethodValue.Text);
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {
            SetPrintFormat(printContext, inlineTableCellValue, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, inlineTableCellValue.Text);
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
            if (printContext == null || inlineBarcode == null || inlineBarcode.Value == null)
                return;

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
            SetPrintFormat(printContext, inlineBoolValue, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, inlineBoolValue.Value.ToString());
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }

        public override void OnRednerRun(PrintContext printContext, Run run)
        {
            SetPrintFormat(printContext, run, null);
            PrintFormat defaultPrintFormat = printContext.GetDefaultPrintFormat();
            PrintFormattedText(printContext, defaultPrintFormat, run.Text);
            printContext.PrintFormats.RemoveAt(printContext.PrintFormats.Count - 1);
        }
        #endregion


        #region Methods -> Render-> Common

        private void SetPrintFormat(PrintContext printContext, TextElement textElement, TextAlignment? textAlignment)
        {
            PrintFormat printFormat = new PrintFormat();
            printFormat.FontSize = textElement.FontSize;
            printFormat.FontWeight = textElement.FontWeight;
            printFormat.TextAlignment = textAlignment;
            printContext.PrintFormats.Add(printFormat);
        }

        protected Tuple<Justification, CharSizeWidth, CharSizeHeight> GetESCFormat(PrintFormat defaultPrintFormat)
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
                if (defaultPrintFormat.FontSize >= 18)
                {
                    charSizeWidth = CharSizeWidth.Quadruple;
                    charSizeHeight = CharSizeHeight.Quadruple;
                }
                else if (defaultPrintFormat.FontSize >= 16)
                {
                    charSizeWidth = CharSizeWidth.Triple;
                    charSizeHeight = CharSizeHeight.Triple;
                }
                else if (defaultPrintFormat.FontSize >= 14)
                {
                    charSizeWidth = CharSizeWidth.Double;
                    charSizeHeight = CharSizeHeight.Double;
                }
            }

            return new Tuple<Justification, CharSizeWidth, CharSizeHeight>(justification, charSizeWidth, charSizeHeight);
        }

        protected void PrintFormattedText(PrintContext printContext, PrintFormat defaultPrintFormat, string text)
        {
            Tuple<Justification, CharSizeWidth, CharSizeHeight> format = GetESCFormat(defaultPrintFormat);
            printContext.Main = printContext.Main.Add(Commands.SelectPrintMode(PrintMode.Reset));
            printContext.Main = printContext.Main.Add(SelectCharSize(format.Item2, format.Item3));
            printContext.Main = printContext.Main.Add(Commands.LF, SelectJustification(format.Item1), printContext.Encoding.GetBytes(text));
        }

        #endregion

        #endregion

        #endregion

    }
}
