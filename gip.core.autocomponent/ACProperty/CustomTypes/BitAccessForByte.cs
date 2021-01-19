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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess for Byte'}de{'Bitzugriff für Byte'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class BitAccessForByte : BitAccess<Byte>
    {
        #region c'tors
        public BitAccessForByte()
        {
            //_value = 0;
        }

        public BitAccessForByte(IACType acValueType) 
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

        public static byte Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            int stringlength = value.Length;
            if (stringlength > 8)
                return 0;

            byte result = 0;
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, "[01]{" + stringlength + "}") || stringlength > 8)
                return 0;
            else
            {
                for (int i = 0; i < stringlength; i++)
                {
                    if (value[i] == '1')
                        result += Convert.ToByte(Math.Pow(2, stringlength - 1 - i));
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
                return Convert.ToBoolean(ValueT & 0x01);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x01;
                else
                    ValueT = (Byte) (ValueT & ~0x01);
                if (oldVal != _valueT)
                    OnBitChanged("Bit00");
            }
        }

        public bool Bit01
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x02);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x02;
                else
                    ValueT = (Byte)(ValueT & ~0x02);
                if (oldVal != _valueT)
                    OnBitChanged("Bit01");
            }
        }

        public bool Bit02
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x04);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x04;
                else
                    ValueT = (Byte)(ValueT & ~0x04);
                if (oldVal != _valueT)
                    OnBitChanged("Bit02");
            }
        }

        public bool Bit03
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x08);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x08;
                else
                    ValueT = (Byte)(ValueT & ~0x08);
                if (oldVal != _valueT)
                    OnBitChanged("Bit03");
            }
        }

        public bool Bit04
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x10);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x10;
                else
                    ValueT = (Byte)(ValueT & ~0x10);
                if (oldVal != _valueT)
                    OnBitChanged("Bit04");
            }
        }

        public bool Bit05
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x20);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x20;
                else
                    ValueT = (Byte)(ValueT & ~0x20);
                if (oldVal != _valueT)
                    OnBitChanged("Bit05");
            }
        }

        public bool Bit06
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x40);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x40;
                else
                    ValueT = (Byte)(ValueT & ~0x40);
                if (oldVal != _valueT)
                    OnBitChanged("Bit06");
            }
        }

        public bool Bit07
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x80);
            }

            set
            {
                Byte oldVal = _valueT;
                if (value)
                    _valueT |= 0x80;
                else
                    ValueT = (Byte)(ValueT & ~0x80);
                if (oldVal != _valueT)
                    OnBitChanged("Bit07");
            }
        }
        #endregion

    }
}
