using DocumentFormat.OpenXml.Wordprocessing;
using gip.core.datamodel;
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

        public LP4PrintJob(LP4Printer.LP4PrinterCommands commands, string printerName, string layoutName = null) : base()
        {
            LP4Commands = commands;
            PrinterName = printerName;
            LayoutName = layoutName;
        }

        #endregion

        #region Properties

        public LP4Printer.LP4PrinterCommands LP4Commands
        {
            get;
            set;
        }

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

        public Msg PrintJobError
        {
            get;
            set;
        }

        #endregion

        #region Methods

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
                case LP4Printer.LP4PrinterCommands.C_EnumPrinters:
                    {
                        return EnumeratePrinters();
                    }
                case LP4Printer.LP4PrinterCommands.C_EnumLayouts:
                    {
                        return EnumerateLayouts();
                    }
                case LP4Printer.LP4PrinterCommands.C_EnumLayoutVariables:
                    {
                        return EnumerateLayoutVariables();
                    }
                case LP4Printer.LP4PrinterCommands.C_PrinterStatus:
                    {
                        return GetPrinterStatus();
                    }
                case LP4Printer.LP4PrinterCommands.C_ResetCommand:
                    {
                        return ResetPrinter();
                    }
                case LP4Printer.LP4PrinterCommands.C_PrintCommand:
                    {
                        return Print();
                    }
            }

            return null;
        }

        private string EnumeratePrinters()
        {
            return string.Format("{0}{1}{2}", LP4Commands.StartCharacter, LP4Commands.EnumPrinters, LP4Commands.EndCharacter);
        }

        private string EnumerateLayouts()
        {
            return string.Format("{0}{1}{2}", LP4Commands.StartCharacter, LP4Commands.EnumLayouts, LP4Commands.EndCharacter);
        }

        private string EnumerateLayoutVariables()
        {
            if (string.IsNullOrEmpty(LayoutName))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "The layout name is not defined!");
                return null;
            }

            return string.Format("{0}{1}{2}{3}", LP4Commands.StartCharacter, LP4Commands.EnumLayoutVariables, LayoutName, LP4Commands.EndCharacter );
        }

        private string GetPrinterStatus()
        {
            if (string.IsNullOrEmpty(PrinterName))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "The printer name is not defined!");
                return null;
            }

            return string.Format("{0}{1}{2}{3}", LP4Commands.StartCharacter, LP4Commands.PrinterStatus, PrinterName, LP4Commands.EndCharacter);
        }

        public string ResetPrinter()
        {
            if (string.IsNullOrEmpty(PrinterName))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "The printer name is not defined!");
                return null;
            }

            return string.Format("{0}{1}{2}{3}", LP4Commands.StartCharacter, LP4Commands.ResetCommand, PrinterName, LP4Commands.EndCharacter);
        }

        public string Print()
        {
            return null;
        }

        public byte[] GetPrinterCommand()
        {
            return null;
        }

        #endregion
    }
}
