using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.communication.modbus
{
    public static class ItemSyntaxResolver
    {
        /// <summary>
        /// Syntax:
        /// A57	Ausgang 56 = A7.0	
        /// AB49	Ausgangsbyte 48 = A6.0-6.7	
        /// AD1	Ausgangsdoppelwort 0 = A0.0-3.7	
        /// AW33	Ausgangswort 32 = A4.0-5.7	
        /// E57	Eingang 56 = E7.0 (56 => 56 / 8 = 7 (Byteaddr.) + 56 mod 8 = 0 (Bitno))
        /// EB49	Eingangsbyte 48 = E6.0-6.7	
        /// ED1	Eingangsdoppelwort 0 = E0.0-3.7	
        /// EW33	Eingangswort 32 = E4.0-5.7	
        /// HR1.DW	H-Register 0, doppelwort	
        /// HR3.B	H-Register 2, byte	
        /// HR3.W	H-Register 2, wort	
        /// HR3.X.0	H-Register 2, Bit 1	
        /// HR4.R	H-Register 3, real	
        /// IR1.DW	I-Register 0, doppelwort	
        /// IR3.B	I-Register 2, byte	
        /// IR3.W	I-Register 2, wort	
        /// IR3.X.0	I-Register 2, Bit 1	
        /// IR4.R	I-Register 3, real	
        /// </summary>
        /// <param name="itemAddr"></param>
        /// <param name="dataType"></param>
        /// <param name="varType"></param>
        /// <param name="dbNo"></param>
        /// <param name="startByteAddr"></param>
        /// <param name="length"></param>
        /// <param name="bitNo"></param>
        /// <returns></returns>
        public static bool Resolve(string itemAddr, out TableType dataType, out VarType varType, out int dbNo, out int startByteAddr, out int length, out short bitNo)
        {
            dataType = TableType.Input;
            varType = VarType.Bit;
            dbNo = 0;
            bitNo = -1;
            startByteAddr = -1;
            length = -1;
            int modbusBitNo = 0;

            if (String.IsNullOrEmpty(itemAddr))
                return false;
            string txt = itemAddr.ToUpper();
            txt = txt.Replace(" ", "");     // remove spaces

            try
            {
                bool varTypeFound = false;
                string[] strings = txt.Split(new char[] { '.', ',' });
                string substr = txt.Substring(0, 2);
                switch (substr)
                {
                    case "IR": // Input-Register    RO
                    case "RO": // Input-Register    RO
                    case "HR": // Holding-Register  RW
                    case "RW": // Holding-Register  RW
                        if (substr == "IR" || substr == "RO")
                            dataType = TableType.ReadOnlyRegister;
                        else
                            dataType = TableType.ReadWriteRegister;
                        if (strings.Length < 2)
                            return false;

                        dbNo = (int)dataType;
                        string dbType = "";
                        if (strings[1].Length > 1)
                            dbType = strings[1].Substring(0, 2);
                        else
                            dbType = strings[1].Substring(0, 1);
                        switch (dbType)
                        {
                            case "DW":
                                varTypeFound = true;
                                varType = VarType.DWord;
                                break;
                            case "DI":
                                varTypeFound = true;
                                varType = VarType.DInt;
                                break;
                            default:
                                varTypeFound = false;
                                break;
                        }

                        startByteAddr = Convert.ToInt32(strings[0].Substring(2)) - 1;
                        if (startByteAddr < 0)
                            startByteAddr = 0;
                        else
                            startByteAddr *= 2;

                        if (varTypeFound)
                            length = LengthOfVarType(varType);
                        else //if (!varTypeFound)
                        {
                            dbType = strings[1].Substring(0, 1);
                            switch (dbType)
                            {
                                case "X":
                                    varTypeFound = true;
                                    varType = VarType.Bit;
                                    length = LengthOfVarType(varType);
                                    if (strings.Length < 3)
                                        return false;
                                    bitNo = Convert.ToInt16(strings[2]);
                                    if (bitNo > 7)
                                        return false;
                                    break;
                                case "B":
                                    varTypeFound = true;
                                    varType = VarType.Byte;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "W":
                                    varTypeFound = true;
                                    varType = VarType.Word;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "I":
                                    varTypeFound = true;
                                    varType = VarType.Int;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "R":
                                    varTypeFound = true;
                                    varType = VarType.Real;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "S":
                                    varTypeFound = true;
                                    varType = VarType.String;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[1].Substring(1));
                                    break;
                                case "$":
                                    varTypeFound = true;
                                    varType = VarType.Base64String;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[1].Substring(1));
                                    break;
                                default:
                                    return false;
                            }
                        }
                        break;
                    case "EB":
                    case "IB":
                        // Eingangsbyte
                        dataType = TableType.Input;
                        varType = VarType.Byte;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Input;
                        break;
                    case "EW":
                    case "IW":
                        // Eingangswort
                        dataType = TableType.Input;
                        varType = VarType.Word;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Input;
                        break;
                    case "ED":
                    case "ID":
                        // Eingangsdoppelwort
                        dataType = TableType.Input;
                        varType = VarType.DWord;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Input;
                        break;
                    case "AB":
                    case "OB":
                        // Ausgangsbyte
                        dataType = TableType.Output;
                        varType = VarType.Byte;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Output;
                        break;
                    case "AW":
                    case "OW":
                        // Ausgangswort
                        dataType = TableType.Output;
                        varType = VarType.Word;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Output;
                        break;
                    case "AD":
                    case "OD":
                        // Ausgangsdoppelwort
                        dataType = TableType.Output;
                        varType = VarType.DWord;
                        length = LengthOfVarType(varType);
                        modbusBitNo = Convert.ToInt32(txt.Substring(2)) - 1;
                        RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                        if (bitNo == 0)
                            varTypeFound = true;
                        dbNo = (int)TableType.Output;
                        break;
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Eingang
                                dataType = TableType.Input;
                                varType = VarType.Bit;
                                //if (strings.Length < 2)
                                //    return false;
                                length = LengthOfVarType(varType);
                                modbusBitNo = Convert.ToInt32(strings[0].Substring(1)) - 1;
                                RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                                varTypeFound = true;
                                dbNo = (int)TableType.Input;
                                break;
                            case "A":
                            case "O":
                                // Ausgang
                                dataType = TableType.Output;
                                varType = VarType.Bit;
                                //if (strings.Length < 2)
                                //    return false;
                                length = LengthOfVarType(varType);
                                modbusBitNo = Convert.ToInt32(strings[0].Substring(1)) - 1;
                                RemapToByteAddr(modbusBitNo, out startByteAddr, out bitNo);
                                varTypeFound = true;
                                dbNo = (int)TableType.Output;
                                break;
                        }

                        if (!varTypeFound)
                            return false;
                        return true;
                }
                return varTypeFound;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ItemSyntaxResolver", "Resolve", msg);
            }

            return false;
        }

        private static void RemapToByteAddr(int modbusBitNo, out int startByteAddr, out short bitNo)
        {
            if (modbusBitNo <= 7)
            {
                startByteAddr = 0;
                bitNo = System.Convert.ToInt16(modbusBitNo);
            }
            else
            {
                startByteAddr = modbusBitNo / 8;
                bitNo = System.Convert.ToInt16(modbusBitNo % 8);
            }
        }

        public static int LengthOfVarType(VarType varType)
        {
            switch (varType)
            {
                case VarType.Bit:
                    return 1;
                case VarType.Byte:
                    return 1;
                case VarType.Word:
                case VarType.Int:
                    return 2;
                case VarType.DWord:
                case VarType.DInt:
                case VarType.Real:
                    return 4;
                case VarType.String:
                case VarType.Base64String:
                    return -1;
            }
            return -1;
        }
    }
}
