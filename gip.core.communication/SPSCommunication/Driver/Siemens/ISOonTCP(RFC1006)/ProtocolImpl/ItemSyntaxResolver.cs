using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.communication.ISOonTCP
{
    public static class ItemSyntaxResolver
    {
        public static bool Resolve(string itemAddr, out DataType dataType, out VarType varType, out int dbNo, out int startByteAddr, out int length, out short bitNo)
        {
            dataType = DataType.DataBlock;
            varType = VarType.Bit;
            dbNo = 0;
            bitNo = -1;
            startByteAddr = -1;
            length = -1;

            if (String.IsNullOrEmpty(itemAddr))
                return false;
            string txt = itemAddr.ToUpper();
            txt = txt.Replace(" ", "");     // remove spaces

            try
            {
                bool varTypeFound = false;
                string[] strings = txt.Split(new char[] { '.', ',' });
                switch (txt.Substring(0, 2))
                {
                    case "DB":
                        dataType = DataType.DataBlock;
                        if (strings.Length < 2)
                            return false;

                        dbNo = Convert.ToInt32(strings[0].Substring(2));
                        string dbType = strings[1].Substring(0, 2);
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

                        if (varTypeFound)
                        {
                            startByteAddr = Convert.ToInt32(strings[1].Substring(2));
                            length = LengthOfVarType(varType);
                        }
                        else //if (!varTypeFound)
                        {
                            dbType = strings[1].Substring(0, 1);
                            startByteAddr = Convert.ToInt32(strings[1].Substring(1));
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
                                    length = Convert.ToInt32(strings[2]);
                                    break;
                                case "$":
                                    varTypeFound = true;
                                    varType = VarType.Base64String;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[2]);
                                    break;
                                case "#":
                                    varTypeFound = true;
                                    varType = VarType.Array;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[2]);
                                    break;
                                default:
                                    return false;
                            }
                        }
                        break;
                    case "EB":
                    case "IB":
                        // Eingangsbyte
                        dataType = DataType.Input;
                        varType = VarType.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Inputs;
                        break;
                    case "EW":
                    case "IW":
                        // Eingangswort
                        dataType = DataType.Input;
                        varType = VarType.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Inputs;
                        break;
                    case "ED":
                    case "ID":
                        // Eingangsdoppelwort
                        dataType = DataType.Input;
                        varType = VarType.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Inputs;
                        break;
                    case "AB":
                    case "OB":
                        // Ausgangsbyte
                        dataType = DataType.Output;
                        varType = VarType.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Outputs;
                        break;
                    case "AW":
                    case "OW":
                        // Ausgangswort
                        dataType = DataType.Output;
                        varType = VarType.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Outputs;
                        break;
                    case "AD":
                    case "OD":
                        // Ausgangsdoppelwort
                        dataType = DataType.Output;
                        varType = VarType.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Outputs;
                        break;
                    case "MB":
                        // Merkerbyte
                        dataType = DataType.Marker;
                        varType = VarType.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Marker;
                        break;
                    case "MW":
                        // Merkerwort
                        dataType = DataType.Marker;
                        varType = VarType.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Marker;
                        break;
                    case "MD":
                        // Merkerdoppelwort
                        dataType = DataType.Marker;
                        varType = VarType.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecial.Marker;
                        break;
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Eingang
                                dataType = DataType.Input;
                                varType = VarType.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecial.Inputs;
                                break;
                            case "A":
                            case "O":
                                // Ausgang
                                dataType = DataType.Output;
                                varType = VarType.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecial.Outputs;
                                break;
                            case "M":
                                // Merker
                                dataType = DataType.Marker;
                                varType = VarType.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecial.Marker;
                                break;
                            case "T":
                                // Timer
                                dataType = DataType.Timer;
                                varType = VarType.Timer;
                                startByteAddr = Convert.ToInt32(txt.Substring(1));
                                length = LengthOfVarType(varType);
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecial.Timer;
                                break;
                            case "Z":
                            case "C":
                                // Counter
                                dataType = DataType.Counter;
                                varType = VarType.Counter;
                                startByteAddr = Convert.ToInt32(txt.Substring(1));
                                length = LengthOfVarType(varType);
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecial.Counter;
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
                    datamodel.Database.Root.Messages.LogException("ISOonTCP.ItemSyntaxResolver", "Resolve", msg);
            }

            return false;
        }

        public static int LengthOfVarType(VarType varType)
        {
            switch (varType)
            {
                case VarType.Bit:
                case VarType.Byte:
                    return gip.core.communication.ISOonTCP.Types.Byte.Length;
                case VarType.Counter:
                case VarType.Timer:
                case VarType.Word:
                case VarType.Int:
                    return gip.core.communication.ISOonTCP.Types.Word.Length;
                case VarType.DWord:
                case VarType.DInt:
                case VarType.Real:
                    return gip.core.communication.ISOonTCP.Types.Real.Length;
                case VarType.String:
                case VarType.Base64String:
                case VarType.Array:
                    return -1;
            }
            return -1;
        }
    }
}
