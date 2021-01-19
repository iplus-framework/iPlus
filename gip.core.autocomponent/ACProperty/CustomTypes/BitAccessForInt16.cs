using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess for Int16'}de{'Bitzugriff für Int16'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class BitAccessForInt16 : BitAccess<UInt16>
    {
        #region c'tors
        public BitAccessForInt16()
        {
            //_value = 0;
        }

        public BitAccessForInt16(IACType acValueType)
            : base(acValueType)
        {
        }
        #endregion

        #region Override: Access over Bitname
        public override bool GetBitValue(string Bit)
        {
            switch (Bit)
            {
                case "Bit00":
                    return Bit00;
                case "Bit01":
                    return Bit01;
                case "Bit02":
                    return Bit02;
                case "Bit03":
                    return Bit03;
                case "Bit04":
                    return Bit04;
                case "Bit05":
                    return Bit05;
                case "Bit06":
                    return Bit06;
                case "Bit07":
                    return Bit07;
                case "Bit08":
                    return Bit08;
                case "Bit09":
                    return Bit09;
                case "Bit10":
                    return Bit10;
                case "Bit11":
                    return Bit11;
                case "Bit12":
                    return Bit12;
                case "Bit13":
                    return Bit13;
                case "Bit14":
                    return Bit14;
                case "Bit15":
                    return Bit15;
                default:
                    return false;
            }
        }

        public override bool GetBitValue(short Bit)
        {
            switch (Bit)
            {
                case 0:
                    return Bit00;
                case 1:
                    return Bit01;
                case 2:
                    return Bit02;
                case 3:
                    return Bit03;
                case 4:
                    return Bit04;
                case 5:
                    return Bit05;
                case 6:
                    return Bit06;
                case 7:
                    return Bit07;
                case 8:
                    return Bit08;
                case 9:
                    return Bit09;
                case 10:
                    return Bit10;
                case 11:
                    return Bit11;
                case 12:
                    return Bit12;
                case 13:
                    return Bit13;
                case 14:
                    return Bit14;
                case 15:
                    return Bit15;
                default:
                    return false;
            }
        }

        public override void SetBitValue(string Bit, bool value)
        {
            switch (Bit)
            {
                case "Bit00":
                    Bit00 = value;
                    break;
                case "Bit01":
                    Bit01 = value;
                    break;
                case "Bit02":
                    Bit02 = value;
                    break;
                case "Bit03":
                    Bit03 = value;
                    break;
                case "Bit04":
                    Bit04 = value;
                    break;
                case "Bit05":
                    Bit05 = value;
                    break;
                case "Bit06":
                    Bit06 = value;
                    break;
                case "Bit07":
                    Bit07 = value;
                    break;
                case "Bit08":
                    Bit08 = value;
                    break;
                case "Bit09":
                    Bit09 = value;
                    break;
                case "Bit10":
                    Bit10 = value;
                    break;
                case "Bit11":
                    Bit11 = value;
                    break;
                case "Bit12":
                    Bit12 = value;
                    break;
                case "Bit13":
                    Bit13 = value;
                    break;
                case "Bit14":
                    Bit14 = value;
                    break;
                case "Bit15":
                    Bit15 = value;
                    break;
            }
        }

        public override void SetBitValue(short Bit, bool value)
        {
            switch (Bit)
            {
                case 0:
                    Bit00 = value;
                    break;
                case 1:
                    Bit01 = value;
                    break;
                case 2:
                    Bit02 = value;
                    break;
                case 3:
                    Bit03 = value;
                    break;
                case 4:
                    Bit04 = value;
                    break;
                case 5:
                    Bit05 = value;
                    break;
                case 6:
                    Bit06 = value;
                    break;
                case 7:
                    Bit07 = value;
                    break;
                case 8:
                    Bit08 = value;
                    break;
                case 9:
                    Bit09 = value;
                    break;
                case 10:
                    Bit10 = value;
                    break;
                case 11:
                    Bit12 = value;
                    break;
                case 12:
                    Bit12 = value;
                    break;
                case 13:
                    Bit13 = value;
                    break;
                case 14:
                    Bit14 = value;
                    break;
                case 15:
                    Bit15 = value;
                    break;
            }
        }

        protected override void OnValueTChanged(IACPropertyNetValueEvent valueEvent = null)
        {
            base.OnValueTChanged(valueEvent);
            OnPropertyChanged("Bit00");
            OnPropertyChanged("Bit01");
            OnPropertyChanged("Bit02");
            OnPropertyChanged("Bit03");
            OnPropertyChanged("Bit04");
            OnPropertyChanged("Bit05");
            OnPropertyChanged("Bit06");
            OnPropertyChanged("Bit07");
            OnPropertyChanged("Bit08");
            OnPropertyChanged("Bit09");
            OnPropertyChanged("Bit10");
            OnPropertyChanged("Bit11");
            OnPropertyChanged("Bit12");
            OnPropertyChanged("Bit13");
            OnPropertyChanged("Bit14");
            OnPropertyChanged("Bit15");
        }

        private void OnBitChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
            base.OnValueTChanged();
        }

        public override string ToString()
        {
            return Convert.ToString(ValueT, 2);
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (String.IsNullOrEmpty(format))
                return this.ToString();
            return ValueT.ToString(format, formatProvider);
        }

        public override void SetFromString(string value)
        {
            ValueT = Parse(value);
        }

        public static UInt16 Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            int stringlength = value.Length;
            if (stringlength > 16)
                return 0;

            UInt16 result = 0;
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, "[01]{" + stringlength + "}") || stringlength > 16)
                return 0;
            else
            {
                for (int i = 0; i < stringlength; i++)
                {
                    if (value[i] == '1')
                        result += Convert.ToUInt16(Math.Pow(2, stringlength - 1 - i));
                }
            }
            return result;
        }

        #endregion

        #region BitAccess Get/Set
        public bool Bit00
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0001);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0001;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0001);
                if (oldVal != _valueT)
                    OnBitChanged("Bit00");
            }
        }

        public bool Bit01
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0002);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0002;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0002);
                if (oldVal != _valueT)
                    OnBitChanged("Bit01");
            }
        }

        public bool Bit02
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0004);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0004;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0004);
                if (oldVal != _valueT)
                    OnBitChanged("Bit02");
            }
        }

        public bool Bit03
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0008);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0008;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0008);
                if (oldVal != _valueT)
                    OnBitChanged("Bit03");
            }
        }

        public bool Bit04
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0010);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0010;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0010);
                if (oldVal != _valueT)
                    OnBitChanged("Bit04");
            }
        }

        public bool Bit05
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0020);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0020;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0020);
                if (oldVal != _valueT)
                    OnBitChanged("Bit05");
            }
        }

        public bool Bit06
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0040);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0040;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0040);
                if (oldVal != _valueT)
                    OnBitChanged("Bit06");
            }
        }

        public bool Bit07
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0080);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0080;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0080);
                if (oldVal != _valueT)
                    OnBitChanged("Bit07");
            }
        }

        public bool Bit08
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0100);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0100;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0100);
                if (oldVal != _valueT)
                    OnBitChanged("Bit08");
            }
        }

        public bool Bit09
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0200);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0200;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0200);
                if (oldVal != _valueT)
                    OnBitChanged("Bit09");
            }
        }

        public bool Bit10
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0400);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0400;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0400);
                if (oldVal != _valueT)
                    OnBitChanged("Bit10");
            }
        }

        public bool Bit11
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x0800);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x0800;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x0800);
                if (oldVal != _valueT)
                    OnBitChanged("Bit11");
            }
        }

        public bool Bit12
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x1000);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x1000;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x1000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit12");
            }
        }

        public bool Bit13
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x2000);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x2000;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x2000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit13");
            }
        }

        public bool Bit14
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x4000);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x4000;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x4000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit14");
            }
        }

        public bool Bit15
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x8000);
            }

            set
            {
                UInt16 oldVal = _valueT;
                if (value)
                    _valueT |= 0x8000;
                else
                    _valueT = (UInt16) unchecked(ValueT & ~0x8000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit15");
            }
        }
        #endregion
    }
}
