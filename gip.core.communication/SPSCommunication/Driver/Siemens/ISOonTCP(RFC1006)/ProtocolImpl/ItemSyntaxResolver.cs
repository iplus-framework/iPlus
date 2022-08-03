using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.communication.ISOonTCP
{
    public static class ItemSyntaxResolver
    {
        public static bool Resolve(string itemAddr, out DataTypeEnum dataType, out VarTypeEnum varType, out int dbNo, out int startByteAddr, out int length, out short bitNo)
        {
            dataType = DataTypeEnum.DataBlock;
            varType = VarTypeEnum.Bit;
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
                        dataType = DataTypeEnum.DataBlock;
                        if (strings.Length < 2)
                            return false;

                        dbNo = Convert.ToInt32(strings[0].Substring(2));
                        string dbType = strings[1].Substring(0, 2);
                        switch (dbType)
                        {
                            case "DW":
                                varTypeFound = true;
                                varType = VarTypeEnum.DWord;
                                break;
                            case "DI":
                                varTypeFound = true;
                                varType = VarTypeEnum.DInt;
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
                                    varType = VarTypeEnum.Bit;
                                    length = LengthOfVarType(varType);
                                    if (strings.Length < 3)
                                        return false;
                                    bitNo = Convert.ToInt16(strings[2]);
                                    if (bitNo > 7)
                                        return false;
                                    break;
                                case "B":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Byte;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "W":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Word;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "I":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Int;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "R":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Real;
                                    length = LengthOfVarType(varType);
                                    break;
                                case "S":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.String;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[2]);
                                    break;
                                case "$":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Base64String;
                                    if (strings.Length < 3)
                                        return false;
                                    length = Convert.ToInt32(strings[2]);
                                    break;
                                case "#":
                                    varTypeFound = true;
                                    varType = VarTypeEnum.Array;
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
                        dataType = DataTypeEnum.Input;
                        varType = VarTypeEnum.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Inputs;
                        break;
                    case "EW":
                    case "IW":
                        // Eingangswort
                        dataType = DataTypeEnum.Input;
                        varType = VarTypeEnum.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Inputs;
                        break;
                    case "ED":
                    case "ID":
                        // Eingangsdoppelwort
                        dataType = DataTypeEnum.Input;
                        varType = VarTypeEnum.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Inputs;
                        break;
                    case "AB":
                    case "OB":
                        // Ausgangsbyte
                        dataType = DataTypeEnum.Output;
                        varType = VarTypeEnum.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Outputs;
                        break;
                    case "AW":
                    case "OW":
                        // Ausgangswort
                        dataType = DataTypeEnum.Output;
                        varType = VarTypeEnum.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Outputs;
                        break;
                    case "AD":
                    case "OD":
                        // Ausgangsdoppelwort
                        dataType = DataTypeEnum.Output;
                        varType = VarTypeEnum.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Outputs;
                        break;
                    case "MB":
                        // Merkerbyte
                        dataType = DataTypeEnum.Marker;
                        varType = VarTypeEnum.Byte;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Marker;
                        break;
                    case "MW":
                        // Merkerwort
                        dataType = DataTypeEnum.Marker;
                        varType = VarTypeEnum.Word;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Marker;
                        break;
                    case "MD":
                        // Merkerdoppelwort
                        dataType = DataTypeEnum.Marker;
                        varType = VarTypeEnum.DWord;
                        length = LengthOfVarType(varType);
                        startByteAddr = Convert.ToInt32(txt.Substring(2));
                        varTypeFound = true;
                        dbNo = (int)DBNoSpecialEnum.Marker;
                        break;
                    default:
                        switch (txt.Substring(0, 1))
                        {
                            case "E":
                            case "I":
                                // Eingang
                                dataType = DataTypeEnum.Input;
                                varType = VarTypeEnum.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecialEnum.Inputs;
                                break;
                            case "A":
                            case "O":
                                // Ausgang
                                dataType = DataTypeEnum.Output;
                                varType = VarTypeEnum.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecialEnum.Outputs;
                                break;
                            case "M":
                                // Merker
                                dataType = DataTypeEnum.Marker;
                                varType = VarTypeEnum.Bit;
                                if (strings.Length < 2)
                                    return false;
                                length = LengthOfVarType(varType);
                                startByteAddr = Convert.ToInt32(strings[0].Substring(1));
                                bitNo = Convert.ToInt16(strings[1]);
                                if (bitNo > 7)
                                    return false;
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecialEnum.Marker;
                                break;
                            case "T":
                                // Timer
                                dataType = DataTypeEnum.Timer;
                                varType = VarTypeEnum.Timer;
                                startByteAddr = Convert.ToInt32(txt.Substring(1));
                                length = LengthOfVarType(varType);
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecialEnum.Timer;
                                break;
                            case "Z":
                            case "C":
                                // Counter
                                dataType = DataTypeEnum.Counter;
                                varType = VarTypeEnum.Counter;
                                startByteAddr = Convert.ToInt32(txt.Substring(1));
                                length = LengthOfVarType(varType);
                                varTypeFound = true;
                                dbNo = (int)DBNoSpecialEnum.Counter;
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

        public static int LengthOfVarType(VarTypeEnum varType)
        {
            switch (varType)
            {
                case VarTypeEnum.Bit:
                case VarTypeEnum.Byte:
                    return gip.core.communication.ISOonTCP.Types.Byte.Length;
                case VarTypeEnum.Counter:
                case VarTypeEnum.Timer:
                case VarTypeEnum.Word:
                case VarTypeEnum.Int:
                    return gip.core.communication.ISOonTCP.Types.Word.Length;
                case VarTypeEnum.DWord:
                case VarTypeEnum.DInt:
                case VarTypeEnum.Real:
                    return gip.core.communication.ISOonTCP.Types.Real.Length;
                case VarTypeEnum.String:
                case VarTypeEnum.Base64String:
                case VarTypeEnum.Array:
                    return -1;
            }
            return -1;
        }
    }
}
