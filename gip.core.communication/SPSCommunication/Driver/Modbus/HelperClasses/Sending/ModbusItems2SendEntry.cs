using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.modbus;

namespace gip.core.communication
{
    public class ModbusItems2SendEntry
    {
        #region c'tors
        public ModbusItems2SendEntry(ModbusItem item, int order)
        {
            Item = item;
            Order = order;
            if ((item.ACProperty.Value != null) && item.ACProperty.Value is ACCustomTypeBase)
            {
                Value = (item.ACProperty.Value as ACCustomTypeBase).Value;
            }
            else
                Value = item.ACProperty.Value;
        }
        #endregion

        #region Properties
        public ModbusItem Item
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }
        #endregion

        #region Methods
        public byte[] UpdateDataBlockRAM(bool chronologicalValue, ModbusDataBlock s7DataBlock)
        {
            byte[] returnValue = null;
            object valueToConvert = Value;
            if (!chronologicalValue)
                valueToConvert = Item.ACProperty.Value;

            switch (Item.ItemVarType)
            {
                case VarType.Bit:
                    BitAccessForByte bitAccess = new BitAccessForByte() { ValueT = s7DataBlock.RAMinPLC[Item.ItemStartByteAddr] };
                    Boolean Boolval = false;
                    if (Item.RequestedDatatype != typeof(Boolean))
                        Boolval = (Boolean)Convert.ChangeType(valueToConvert, typeof(Boolean));
                    else
                        Boolval = (Boolean)valueToConvert;
                    bitAccess.SetBitValue(Item.ItemBitNo, Boolval);
                    returnValue = gip.core.communication.modbus.Types.Byte.ToByteArray(bitAccess.ValueT);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.Byte:
                    Byte Byteval = 0;
                    if (Item.RequestedDatatype != typeof(Byte))
                        Byteval = (Byte)Convert.ChangeType(valueToConvert, typeof(Byte));
                    else
                        Byteval = (Byte)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.Byte.ToByteArray(Byteval);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.Word:
                    UInt16 UInt16val = 0;
                    if (Item.RequestedDatatype != typeof(UInt16))
                        UInt16val = (UInt16)Convert.ChangeType(valueToConvert, typeof(UInt16));
                    else
                        UInt16val = (UInt16)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.Word.ToByteArray(UInt16val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.DWord:
                    UInt32 UInt32val = 0;
                    if (Item.RequestedDatatype != typeof(UInt32))
                        UInt32val = (UInt32)Convert.ChangeType(valueToConvert, typeof(UInt32));
                    else
                        UInt32val = (UInt32)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.DWord.ToByteArray(UInt32val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.Int:
                    Int16 Int16val = 0;
                    if (Item.RequestedDatatype != typeof(Int16))
                        Int16val = (Int16)Convert.ChangeType(valueToConvert, typeof(Int16));
                    else
                        Int16val = (Int16)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.Int.ToByteArray(Int16val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.DInt:
                    Int32 Int32val = 0;
                    if (Item.RequestedDatatype != typeof(Int32))
                        Int32val = (Int32)Convert.ChangeType(valueToConvert, typeof(Int32));
                    else
                        Int32val = (Int32)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.DInt.ToByteArray(Int32val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.Real:
                    Single floatVal = 0;
                    if (Item.RequestedDatatype != typeof(Single))
                        floatVal = (Single)Convert.ChangeType(valueToConvert, typeof(Single));
                    else
                        floatVal = (Single)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.Real.ToByteArray(floatVal, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarType.String:
                case VarType.Base64String:
                    String Stringval;
                    if (Item.RequestedDatatype != typeof(String))
                        Stringval = (String)Convert.ChangeType(valueToConvert, typeof(String));
                    else
                        Stringval = (String)valueToConvert;
                    returnValue = gip.core.communication.modbus.Types.String.ToByteArray(Stringval, System.Convert.ToByte(Item.StringLen), Item.ItemLength, Item.ItemVarType == VarType.Base64String);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
            }
            return new byte[] { 0 };
        }
        #endregion
    }
}
