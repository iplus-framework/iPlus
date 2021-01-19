using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'Serial Bus Scanner Dragon'}de{'Serieller Bus Scanner Dragon'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, PWGroup.PWClassName, true)]
    public class PAMSerialBusScannerDragon : PAMSerialBus
    {
        public PAMSerialBusScannerDragon(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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

        #region Methods

        public override void WriteOnBus(string DeviceID, object[] parameter)
        {
            if (this.SerialPort == null)
                return;
            if (!this.SerialPort.IsOpen)
                return;
            //this.SerialPort.DiscardOutBuffer();
            if (parameter == null)
                return;
            if (String.IsNullOrEmpty(DeviceID))
                return;

            bool bLED = true;
            string[] display = new string[4];
            int i = 0;
            foreach (object param in parameter)
            {
                if (i == 0)
                {
                    if (param is bool)
                        bLED = (bool)param;
                    else
                        return;
                }
                else
                {
                    if (param is string)
                        display[i - 1] = (string)param;
                    else
                        return;
                }
                i++;
                if (i >= 5)
                    break;
            }


            this.SerialPort.NewLine = "\r"; // CR (0x0d)
            string telegrammDisplay = DeviceID;

            telegrammDisplay += "\x1B[2J"; // ESC [ 2 J : Display löschen + Cursor oben links

            for (int j = 0; j < 4; j++)
            {
                telegrammDisplay += display[j];
                if (j < 3)
                    telegrammDisplay += "\x1B" + "E"; // ESC E : Zeilenumbruch
            }

            if (bLED) // grün
                telegrammDisplay += "\x1B[6q\x1B[3q\x1B[5q\x1B[7q"; // Grüne LED, guter ton, 100ms Warten, Grüne LED aus
            else // rot
                telegrammDisplay += "\x1B[7q\x1B[8q\x1B[4q\x1B[5q\x1B[9q"; // Grüne LED aus, rote led and, fehlerton, 100 ms warten, rote LED aus

            //System.Text.Encoding.ASCII.GetString(

            this.SerialPort.WriteLine(telegrammDisplay);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("WriteOnBusEvent", VirtualEventArgs);
            eventArgs.GetACValue("DeviceID").Value = DeviceID;
            eventArgs.GetACValue("WriteData").Value = parameter;
            WriteOnBusEvent.Raise(eventArgs);
 
        }

        protected override void ReadOnBus()
        {
            // Ansprechpartner:
            // Herr Tröß: 06841 9710-72, Firma BSS Systemtechnik GMBH, www.bss-systemtechnik.de
            // TELEGRAMMFORMAT:
            // --------------------------------------------
            // [Header] [Gun_Addr] [Gun_Addr_delimiter] ] [Cradle_Addr] [Cradle_Addr_delimiter]
            // [Time stamp] [Ts_delimiter] [Code ID] [Code Length] CODE [Terminator]
            // [Items in square brackets are optional.]

            // Das Telegrammformat wird über den Scanner und den zu Scannenden Einstellcodes
            // eingestellt. Es sollten alle [Felder] deaktiviert werden ausser
            // dem Terminator, so dass folgendes Telegrammformat entsteht:

            /* 3 Unterscheiedliche Barcodes
  
            CODE: [*] [xxxx] [Terminator]
            -------------------------------------
            Sternchen (*) bedeutet: Scan der Aggregatgruppe (Waage), dahinter folgt vierstellige Aggregatgruppennummer
            Beispiel: *5011

            CODE: [+] [xxxxx...] [Terminator]
            -------------------------------------
            Plus (+) bedeutet: Scan der UniqueKompId aus ChargePos
            Beispiel: +3209
    
            CODE: [xxxxxxx0000000000] [Terminator]
            -------------------------------------
            Ohne Vorzeichen: Scan der Mäurer und Wirtz Chargennnummer -> Struktur: 7 stellige Materialnummer + 10 stellige Chargennummer mit führenden Nullen
            Beispiel: 23997290000015262
    
            [Terminator]: CR (0x0d) + LF (0x0a)
            */

            if (this.SerialPort == null)
                return;
            if (!this.SerialPort.IsOpen)
                return;
            try
            {
                while (true)
                {
                    string readResult = this.SerialPort.ReadTo("\u0003"); // ETX
                    if (readResult.Length > 4)
                    {
                        string[] scanList = readResult.Split(';');
                        foreach (string substring in scanList)
                        {
                            if (substring.Length <= 4)
                                continue;
                            string deviceID = substring.Substring(0, 4);
                            string data = substring.Substring(4);

                            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("ReadOnBusEvent", VirtualEventArgs);
                            eventArgs.GetACValue("DeviceID").Value = deviceID;
                            eventArgs.GetACValue("ScanData").Value = data;
                            ReadOnBusEvent.Raise(eventArgs);
                        }
                    }
                }

            }
            catch (TimeoutException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAMSerialBusScannerDragon", "ReadOnBus", msg);
            }
        }

        [ACMethodInfo("Function", "en{'ReadOnBusSimulation'}de{'ReadOnBusSimulation'}", 9999)]
        public void ReadOnBusSimulation(string deviceID, string data)
        {
            if (String.IsNullOrEmpty(deviceID) || String.IsNullOrEmpty(data))
                return;

            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("ReadOnBusEvent", VirtualEventArgs);
            eventArgs.GetACValue("DeviceID").Value = deviceID;
            eventArgs.GetACValue("ScanData").Value = data;
            ReadOnBusEvent.Raise(eventArgs);
        }

        #endregion

        #region Properties
        //[ACPropertyBindingSource]
        //[DefaultValueAttribute(40)]
        //public IACProperty<int> ReadTelegrammLength { get; set; }
        #endregion

        #region Points and Events
        #endregion
    }
}
