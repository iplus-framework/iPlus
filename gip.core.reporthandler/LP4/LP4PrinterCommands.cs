using DocumentFormat.OpenXml.Drawing;
using gip.core.autocomponent;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.reporthandler
{
    public partial class LP4Printer
    {
        private char _StartCharacter = '\x02';
        public char StartCharacter
        {
            get => _StartCharacter;
            set
            {
                _StartCharacter = value;
                OnPropertyChanged();
            }
        }

        private char _EndCharacter = '\x03';
        public char EndCharacter
        {
            get => _EndCharacter;
            set
            {
                _EndCharacter = value;
                OnPropertyChanged();
            }
        }
        
        private char _SeparatorCharacterTab = '\x09';
        public char SeparatorCharacterTab
        {
            get => _SeparatorCharacterTab;
            set
            {
                _SeparatorCharacterTab = value;
                OnPropertyChanged();
            }
        }

        private char _SeparatorCharachterCR = '\x13';
        public char SeparatorCharachterCR
        {
            get => _SeparatorCharachterCR;
            set
            {
                _SeparatorCharachterCR = value;
                OnPropertyChanged();
            }
        }
        
        public class LP4PrinterCommands
        {
            public LP4PrinterCommands(char startChar, char endChar, char separatorTab, char separatorCR)
            {
                StartCharacter = startChar;
                EndCharacter = endChar;
                SeparatorCharacterTab = separatorTab;
                SeparatorCharachterCR = separatorCR;
            }

            public char StartCharacter
            {
                get;
                internal set;
            }

            public char EndCharacter
            {
                get;
                internal set;
            }

            public char SeparatorCharacterTab
            {
                get;
                internal set;
            }

            public char SeparatorCharachterCR
            {
                get;
                internal set;
            }

            public string EnumPrinters
            {
                get => C_EnumPrinters;
            }

            public string EnumLayouts
            {
                get => C_EnumLayouts;
            }

            public string EnumLayoutVariables 
            {
                get => C_EnumLayoutVariables;
            }

            public string PrintCommand
            {
                get => C_PrintCommand;
            }

            public string ResetCommand
            {
                get => C_ResetCommand;
            }

            public string PrinterStatus
            {
                get => C_PrinterStatus;
            }

            public const string C_EnumPrinters = "E";

            public const string C_EnumLayouts = "L";

            public const string C_EnumLayoutVariables = "V:";

            public const string C_PrintCommand = "P";

            public const string C_ResetCommand = "R:";

            public const string C_PrinterStatus = "S:";


            public const string C_PrintOptionNormalPrinting = "0";

            public const string C_PrintOptionSpoolFileCreatedNotTransfered = "99999A";

            public const string C_PrintOptionNormalPrintingWithoutWindowForVariables = "999999998";

            public const string C_PrintOptionWithoutInputWindowAndNoTransfer = "999999999";

        }

        public enum LP4PrinterType : short
        {
            Unknown = 0,
            WindowsDriver = 1,
            Zebra = 2,
            IntermecUbi = 3,
            Fargo = 4,
            Easyprint = 5,
            Novexx = 6,
            Unicontrol = 7,
            EPL = 8,
            SATO = 9,
            HapaRC_bbPrint = 10,
            Alltec = 11,
            Markem2000 = 12,
            CAB = 13,
            AtlanticZeiser = 14
        }

        public enum LP4CommType : short
        {
            NoCommunication = 0,
            Bidirectional = 1,
            Unidirectional_AlwaysOpen = 2,
            Unidirectional_WhenRequired = 3
        }

        public enum LP4PrintError : short
        {
            UnknownPrinterName = 1,
            LayoutNotExistOrDamaged = 2,
            InternalError = 3
        }
    }
}
