using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.reporthandler
{
    public class LP4PrintJob : PrintJob
    {
        #region c'tors

        public LP4PrintJob() : base()
        {

        }

        #endregion

        #region Properties

        public string PrinterName
        {
            get;
            set;
        }

        public string PrinterTask
        {
            get;
            set;
        }

        public string PrinterCommand
        {
            get;
            set;
        }

        public string LayoutName
        {
            get;
            set;
        }

        public List<Tuple<string, string>> LayoutVariables
        {
            get;
            set;
        }

        public void AddLayoutVariable(string variableName, string variableValue)
        {
            if (LayoutVariables == null)
                LayoutVariables = new List<Tuple<string, string>>();

            LayoutVariables.Add(new Tuple<string, string>(variableName, variableValue));
        }

        public string GetPrinterCommandAsString()
        {
            if (string.IsNullOrEmpty(PrinterTask))
                return null;

            switch (PrinterTask)
            {
                case LP4Printer.LP4PrinterCommands.EnumPrinters:
                    {


                        break;
                    }
                case LP4Printer.LP4PrinterCommands.EnumLayouts:
                    {
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.EnumLayoutVariables:
                    {
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.PrinterStatus:
                    {
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.ResetCommand:
                    {
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.PrintCommand:
                    {
                        break;
                    }
            }



            return null;
        }

        public byte[] GetPrinterCommand()
        {
            return null;
        }

        #endregion
    }
}
