using DocumentFormat.OpenXml.Wordprocessing;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.reporthandler;

namespace gip.core.reporthandlerwpf
{
    public class LP4PrintJob : PrintJobWPF
    {
        #region c'tors

        public LP4PrintJob(LP4Printer.LP4PrinterCommands commands, string printerName, string layoutName = null, int printCopies = 1) : base()
        {
            LP4Commands = commands;
            PrinterName = printerName;
            LayoutName = layoutName;
            PrintCopies = printCopies;
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

        public int PrintCopies
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

        public string GeneratePrinterCommand()
        {
            if (string.IsNullOrEmpty(PrinterTask))
                return null;

            string printerCommand = null;

            switch (PrinterTask)
            {
                case LP4Printer.LP4PrinterCommands.C_EnumPrinters:
                    {
                        printerCommand = EnumeratePrinters();
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.C_EnumLayouts:
                    {
                        printerCommand = EnumerateLayouts();
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.C_EnumLayoutVariables:
                    {
                        printerCommand = EnumerateLayoutVariables();
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.C_PrinterStatus:
                    {
                        printerCommand = GetPrinterStatus();
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.C_ResetCommand:
                    {
                        printerCommand = ResetPrinter();
                        break;
                    }
                case LP4Printer.LP4PrinterCommands.C_PrintCommand:
                    {
                        printerCommand = Print();
                        break;
                    }
            }

            PrinterCommand = printerCommand;

            return printerCommand;
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
            if (string.IsNullOrEmpty(PrinterName))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "The printer name is not defined!");
                return null;
            }

            if (string.IsNullOrEmpty(LayoutName))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "The layout name is not defined!");
                return null;
            }

            //<STX>P:Druckername<TAB>Etikettenlayout<TAB>Druckzahl<TAB>[ExtVar1=Wert<TAB>ExtVar2=Wert<TAB>…]<ETX>

            string printCommand = string.Format("{0}{1}{2}{3}{4}{5}{6}", LP4Commands.StartCharacter, LP4Commands.PrintCommand, PrinterName, LP4Commands.SeparatorCharacterTab, LayoutName, 
                                                                            LP4Commands.SeparatorCharacterTab, PrintCopies);

            if (LayoutVariables != null && LayoutVariables.Any())
            {
                printCommand += "[";

                var lastItem = LayoutVariables.Last();

                foreach (Tuple<string,string> layoutVariable in LayoutVariables)
                {
                    printCommand += string.Format("{0}={1}", layoutVariable.Item1, layoutVariable.Item2);

                    if (lastItem != layoutVariable)
                        printCommand += LP4Commands.SeparatorCharacterTab;
                }

                printCommand += string.Format("]{0}", LP4Commands.EndCharacter);
            }
            else
            {
                printCommand += LP4Commands.EndCharacter;
            }

            return printCommand;
        }

        public byte[] GetPrinterCommand()
        {
            if (string.IsNullOrEmpty(PrinterCommand))
                GeneratePrinterCommand();

            if (string.IsNullOrEmpty(PrinterCommand))
            {
                PrintJobError = new Msg(eMsgLevel.Error, "Printer command is empty!");
                return null;
            }

            return Encoding.GetBytes(PrinterCommand);
        }

        public string GetJobInfo()
        {
            return $"PrintJobID: {PrintJobID} Name:{Name}";
        }

        #endregion
    }
}
