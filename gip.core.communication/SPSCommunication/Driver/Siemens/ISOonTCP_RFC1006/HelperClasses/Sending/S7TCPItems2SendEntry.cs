using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication
{
    public class S7TCPItems2SendEntry
    {
        #region c'tors
        public S7TCPItems2SendEntry(S7TCPItem item, int order)
        {
            Item = item;
            Order = order;
            if ((item.ACProperty.Value != null) && item.ACProperty.Value is ACCustomTypeBase)
                Value = (item.ACProperty.Value as ACCustomTypeBase).Value;
            else
                Value = item.ACProperty.Value;
        }
        #endregion

        #region Properties
        public S7TCPItem Item
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
        public byte[] UpdateDataBlockRAM(bool chronologicalValue, S7TCPDataBlock s7DataBlock)
        {
            byte[] returnValue = null;
            object valueToConvert = Value;
            if (!chronologicalValue)
            {
                if ((Item.ACProperty.Value != null) && Item.ACProperty.Value is ACCustomTypeBase)
                    valueToConvert = (Item.ACProperty.Value as ACCustomTypeBase).Value;
                else
                    valueToConvert = Item.ACProperty.Value;
            }

            switch (Item.ItemVarType)
            {
                case VarTypeEnum.Bit:
                    BitAccessForByte bitAccess = new BitAccessForByte() { ValueT = s7DataBlock.RAMinPLC[Item.ItemStartByteAddr] };
                    Boolean Boolval = false;
                    if (Item.RequestedDatatype != typeof(Boolean))
                        Boolval = (Boolean)Convert.ChangeType(valueToConvert, typeof(Boolean));
                    else
                        Boolval = (Boolean)valueToConvert;
                    bitAccess.SetBitValue(Item.ItemBitNo, Boolval);
                    returnValue = gip.core.communication.ISOonTCP.Types.Byte.ToByteArray(bitAccess.ValueT);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Byte:
                    Byte Byteval = 0;
                    if (Item.RequestedDatatype != typeof(Byte))
                        Byteval = (Byte)Convert.ChangeType(valueToConvert, typeof(Byte));
                    else
                        Byteval = (Byte)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Byte.ToByteArray(Byteval);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Word:
                    UInt16 UInt16val = 0;
                    if (Item.RequestedDatatype != typeof(UInt16))
                        UInt16val = (UInt16)Convert.ChangeType(valueToConvert, typeof(UInt16));
                    else
                        UInt16val = (UInt16)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Word.ToByteArray(UInt16val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.DWord:
                    UInt32 UInt32val = 0;
                    if (Item.RequestedDatatype != typeof(UInt32))
                        UInt32val = (UInt32)Convert.ChangeType(valueToConvert, typeof(UInt32));
                    else
                        UInt32val = (UInt32)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.DWord.ToByteArray(UInt32val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Int:
                    Int16 Int16val = 0;
                    if (Item.RequestedDatatype != typeof(Int16))
                        Int16val = (Int16)Convert.ChangeType(valueToConvert, typeof(Int16));
                    else
                        Int16val = (Int16)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Int.ToByteArray(Int16val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.DInt:
                    Int32 Int32val = 0;
                    if (Item.RequestedDatatype != typeof(Int32))
                        Int32val = (Int32)Convert.ChangeType(valueToConvert, typeof(Int32));
                    else
                        Int32val = (Int32)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.DInt.ToByteArray(Int32val, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Real:
                    Single floatval = 0;
                    if (Item.RequestedDatatype != typeof(Single))
                        floatval = (Single)Convert.ChangeType(valueToConvert, typeof(Single));
                    else
                        floatval = (Single)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Real.ToByteArray(floatval, Item.ParentSubscription.Endianess);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.String:
                case VarTypeEnum.Base64String:
                    String Stringval;
                    if (Item.RequestedDatatype != typeof(String))
                        Stringval = (String)Convert.ChangeType(valueToConvert, typeof(String));
                    else
                        Stringval = (String)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.String.ToByteArray(Stringval, System.Convert.ToByte(Item.StringLen), Item.ItemLength, Item.ItemVarType == VarTypeEnum.Base64String);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Timer:
                    UInt16 Timerval = 0;
                    if (Item.RequestedDatatype != typeof(UInt16))
                        Timerval = (UInt16)Convert.ChangeType(valueToConvert, typeof(UInt16));
                    else
                        Timerval = (UInt16)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Timer.ToByteArray(Timerval);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
                case VarTypeEnum.Counter:
                    UInt16 Counterval = 0;
                    if (Item.RequestedDatatype != typeof(UInt16))
                        Counterval = (UInt16)Convert.ChangeType(valueToConvert, typeof(UInt16));
                    else
                        Counterval = (UInt16)valueToConvert;
                    returnValue = gip.core.communication.ISOonTCP.Types.Counter.ToByteArray(Counterval);
                    s7DataBlock.UpdateRAMArea(ref returnValue, Item.ItemStartByteAddr);
                    return returnValue;
            }
            return new byte[] { 0 };
        }
        #endregion
    }
}
