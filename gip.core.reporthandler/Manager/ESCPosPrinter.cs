using gip.core.datamodel;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using System;
using System.Threading;
using ESCPOS;
using ESCPOS.Utils;
using System.Windows;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ESCPosPrinter'}de{'ESCPosPrinter'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
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

        #region Methods -> Character Set
        public static byte[] SelectCodeTable(byte codeTable)
        {
            return new byte[3]
            {
                27,
                116,
                codeTable
            };
        }

        public static byte[] SelectInternationalCharacterSet(CharSet charSet)
        {
            return new byte[3]
            {
                27,
                82,
                (byte)charSet
            };
        }

        public virtual byte[] GetESCPosCodePage(int codePage)
        {
            byte[] bytes = null;
            switch (codePage)
            {
                case 20127: // Encoding.ASCII.CodePage (437)
                    bytes = Commands.SelectCodeTable(CodeTable.USA); 
                    break;
                case 850:
                    bytes = Commands.SelectCodeTable(CodeTable.Multilingual);
                    break;
                case 852:
                    bytes = Commands.SelectCodeTable(CodeTable.Latin2);
                    break;
                case 855:
                    bytes = Commands.SelectCodeTable(CodeTable.Cyrillic);
                    break;
                case 1252:
                    bytes = Commands.SelectCodeTable(CodeTable.Windows1252);
                    break;
                default:
                    bytes = Commands.SelectCodeTable(CodeTable.USA);
                    break;
            }
            return bytes;
        }

        //public byte[] GetInternationalCharacterSet(string language)
        //{
        //    byte[] bytes = null;
        //    switch (language)
        //    {
        //        case "de-DE":
        //            bytes = Commands.SelectInternationalCharacterSet(CharSet.Germany);
        //            break;
        //        case "hr-HR":
        //            bytes = ESCPosPrinter.SelectInternationalCharacterSet(CharSet.Germany);
        //            break;
        //    }
        //    return bytes;
        //}

        #endregion

        #region Methods -> Render

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override bool SendDataToPrinter(PrintJob printJob)
        {
            if (printJob == null || printJob.Main == null)
            {
                return false;
            }
            byte[] bytes = printJob.Main;
            for (int tries = 0; tries < PrintTries; tries++)
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
                    Thread.Sleep(5000);
                }
            }
            return false;
        }

        #region Methods -> Render -> FlowDoc

        public override void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            printJob.Main.Add(Commands.InitializePrinter);
            printJob.Main = printJob.Main.Add(GetESCPosCodePage(printJob.Encoding.CodePage));
            base.OnRenderFlowDocument(printJob, flowDoc);
        }

       

        #endregion

        #region Methods -> Render -> Block


        public override void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position)
        {

        }

        public override void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position)
        {

        }


        public override void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            SetPrintFormat(printJob, sectionReportHeader, sectionReportHeader.TextAlignment);
        }

        public override void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            SetPrintFormat(printJob, sectionReportFooter, sectionReportFooter.TextAlignment);
        }

        public override void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }


        public override void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            SetPrintFormat(printJob, sectionDataGroup, sectionDataGroup.TextAlignment);
        }

        public override void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }


        #endregion

        #region Methods -> Render -> Table


        public override void OnRenderSectionTableHeader(PrintJob printJob, Table table)
        {
            PrintFormat printFormat = new PrintFormat();
            printFormat.FontSize = table.FontSize;
            printFormat.FontWeight = table.FontWeight;
            printFormat.TextAlignment = table.TextAlignment;
            printJob.PrintFormats.Add(printFormat);
        }

        public override void OnRenderSectionTableFooter(PrintJob printJob, Table table)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }


        public override void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn)
        {
            //
        }

        public override void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            SetPrintFormat(printJob, tableRowGroup, null);
        }

        public override void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow)
        {
            SetPrintFormat(printJob, tableRow, null);
        }

        public override void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        #endregion

        #region Methods -> Render -> Inlines

        public override void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph)
        {
            SetPrintFormat(printJob, paragraph, paragraph.TextAlignment);
        }

        public override void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph)
        {
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue)
        {
            SetPrintFormat(printJob, inlineContextValue, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, inlineContextValue.Text);
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue)
        {
            SetPrintFormat(printJob, inlineDocumentValue, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, inlineDocumentValue.Text);
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue)
        {
            SetPrintFormat(printJob, inlineACMethodValue, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, inlineACMethodValue.Text);
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue)
        {
            SetPrintFormat(printJob, inlineTableCellValue, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, inlineTableCellValue.Text);
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode)
        {
            if (printJob == null || inlineBarcode == null || inlineBarcode.Value == null)
                return;

            string barcodeValue = inlineBarcode.Value.ToString();
            if (inlineBarcode.BarcodeType == BarcodeType.QRCODE)
            {
                QRCodeSizeExt qRCodeSizeExt = QRCodeSizeExt.Six;
                if (inlineBarcode.BarcodeWidth >= 2 && inlineBarcode.BarcodeWidth <= 10)
                    qRCodeSizeExt = (QRCodeSizeExt)inlineBarcode.BarcodeWidth;
                printJob.Main = printJob.Main.Add(Commands.LF, Commands.SelectJustification(Justification.Center), Commands.SelectPrintMode(PrintMode.Reset),
                    ESCPosExt.PrintQRCodeExt(barcodeValue, QRCodeModel.Model1, QRCodeCorrection.Percent30, qRCodeSizeExt));
            }
            else
            {
                BarCodeType barCodeType = BarCodeType.EAN8;
                if (Enum.TryParse(inlineBarcode.BarcodeType.ToString(), out barCodeType))
                    printJob.Main = printJob.Main.Add(Commands.LF, Commands.PrintBarCode(barCodeType, barcodeValue));
            }
            printJob.Main = printJob.Main.Add(Commands.LF, Commands.LF, Commands.LF, Commands.LF, Commands.LF);
        }

        public override void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue)
        {
            SetPrintFormat(printJob, inlineBoolValue, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, inlineBoolValue.Value.ToString());
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderRun(PrintJob printJob, Run run)
        {
            SetPrintFormat(printJob, run, null);
            PrintFormat defaultPrintFormat = printJob.GetDefaultPrintFormat();
            PrintFormattedText(printJob, defaultPrintFormat, run.Text);
            printJob.PrintFormats.RemoveAt(printJob.PrintFormats.Count - 1);
        }

        public override void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak)
        {
            printJob.Main = printJob.Main.Add(Commands.LF);
        }

        #endregion


        #region Methods -> Render-> Common

        private void SetPrintFormat(PrintJob printJob, TextElement textElement, TextAlignment? textAlignment)
        {
            PrintFormat printFormat = new PrintFormat();
            printFormat.FontSize = textElement.FontSize;
            printFormat.FontWeight = textElement.FontWeight;
            printFormat.TextAlignment = textAlignment;
            printJob.PrintFormats.Add(printFormat);
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

        protected void PrintFormattedText(PrintJob printJob, PrintFormat defaultPrintFormat, string text)
        {
            Tuple<Justification, CharSizeWidth, CharSizeHeight> format = GetESCFormat(defaultPrintFormat);
            printJob.Main = printJob.Main.Add(Commands.SelectPrintMode(PrintMode.Reset));
            printJob.Main = printJob.Main.Add(Commands.SelectCharSize(format.Item2, format.Item3));
            printJob.Main = printJob.Main.Add(Commands.LF, Commands.SelectJustification(format.Item1), printJob.Encoding.GetBytes(text));
        }

        #endregion

        #endregion

        #endregion

    }
}
