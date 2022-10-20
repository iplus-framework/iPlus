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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess for Int32'}de{'Bitzugriff für Int32'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class BitAccessForInt32 : BitAccess<UInt32>
    {
        #region c'tors
        public BitAccessForInt32()
        {
            //_value = 0;
        }

        public BitAccessForInt32(IACType acValueType)
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
                case "Bit16":
                    return Bit16;
                case "Bit17":
                    return Bit17;
                case "Bit18":
                    return Bit18;
                case "Bit19":
                    return Bit19;
                case "Bit20":
                    return Bit20;
                case "Bit21":
                    return Bit21;
                case "Bit22":
                    return Bit22;
                case "Bit23":
                    return Bit23;
                case "Bit24":
                    return Bit24;
                case "Bit25":
                    return Bit25;
                case "Bit26":
                    return Bit26;
                case "Bit27":
                    return Bit27;
                case "Bit28":
                    return Bit28;
                case "Bit29":
                    return Bit29;
                case "Bit30":
                    return Bit30;
                case "Bit31":
                    return Bit31;
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
                case 16:
                    return Bit16;
                case 17:
                    return Bit17;
                case 18:
                    return Bit18;
                case 19:
                    return Bit19;
                case 20:
                    return Bit20;
                case 21:
                    return Bit21;
                case 22:
                    return Bit22;
                case 23:
                    return Bit23;
                case 24:
                    return Bit24;
                case 25:
                    return Bit25;
                case 26:
                    return Bit26;
                case 27:
                    return Bit27;
                case 28:
                    return Bit28;
                case 29:
                    return Bit29;
                case 30:
                    return Bit30;
                case 31:
                    return Bit31;
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
                    Bit12 = value;
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
                case "Bit16":
                    Bit16 = value;
                    break;
                case "Bit17":
                    Bit17 = value;
                    break;
                case "Bit18":
                    Bit18 = value;
                    break;
                case "Bit19":
                    Bit19 = value;
                    break;
                case "Bit20":
                    Bit20 = value;
                    break;
                case "Bit21":
                    Bit21 = value;
                    break;
                case "Bit22":
                    Bit22 = value;
                    break;
                case "Bit23":
                    Bit23 = value;
                    break;
                case "Bit24":
                    Bit24 = value;
                    break;
                case "Bit25":
                    Bit25 = value;
                    break;
                case "Bit26":
                    Bit26 = value;
                    break;
                case "Bit27":
                    Bit27 = value;
                    break;
                case "Bit28":
                    Bit28 = value;
                    break;
                case "Bit29":
                    Bit29 = value;
                    break;
                case "Bit30":
                    Bit30 = value;
                    break;
                case "Bit31":
                    Bit31 = value;
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
                case 16:
                    Bit16 = value;
                    break;
                case 17:
                    Bit17 = value;
                    break;
                case 18:
                    Bit18 = value;
                    break;
                case 19:
                    Bit19 = value;
                    break;
                case 20:
                    Bit20 = value;
                    break;
                case 21:
                    Bit21 = value;
                    break;
                case 22:
                    Bit22 = value;
                    break;
                case 23:
                    Bit23 = value;
                    break;
                case 24:
                    Bit24 = value;
                    break;
                case 25:
                    Bit25 = value;
                    break;
                case 26:
                    Bit26 = value;
                    break;
                case 27:
                    Bit27 = value;
                    break;
                case 28:
                    Bit28 = value;
                    break;
                case 29:
                    Bit29 = value;
                    break;
                case 30:
                    Bit30 = value;
                    break;
                case 31:
                    Bit31 = value;
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
            OnPropertyChanged("Bit16");
            OnPropertyChanged("Bit17");
            OnPropertyChanged("Bit18");
            OnPropertyChanged("Bit19");
            OnPropertyChanged("Bit20");
            OnPropertyChanged("Bit21");
            OnPropertyChanged("Bit22");
            OnPropertyChanged("Bit23");
            OnPropertyChanged("Bit24");
            OnPropertyChanged("Bit25");
            OnPropertyChanged("Bit26");
            OnPropertyChanged("Bit27");
            OnPropertyChanged("Bit28");
            OnPropertyChanged("Bit29");
            OnPropertyChanged("Bit30");
            OnPropertyChanged("Bit31");
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

        public static UInt32 Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            int stringlength = value.Length;
            if (stringlength > 32)
                return 0;

            UInt32 result = 0;
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, "[01]{" + stringlength + "}") || stringlength > 32)
                return 0;
            else
            {
                for (int i = 0; i < stringlength; i++)
                {
                    if (value[i] == '1')
                        result += Convert.ToUInt32(Math.Pow(2, stringlength - 1 - i));
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
                return Convert.ToBoolean(ValueT & 0x00000001);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000001;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000001);
                if (oldVal != _valueT)
                    OnBitChanged("Bit00");
            }
        }

        public bool Bit01
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000002);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000002;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000002);
                if (oldVal != _valueT)
                    OnBitChanged("Bit01");
            }
        }

        public bool Bit02
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000004);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000004;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000004);
                if (oldVal != _valueT)
                    OnBitChanged("Bit02");
            }
        }

        public bool Bit03
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000008);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000008;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000008);
                if (oldVal != _valueT)
                    OnBitChanged("Bit03");
            }
        }

        public bool Bit04
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000010);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000010;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000010);
                if (oldVal != _valueT)
                    OnBitChanged("Bit04");
            }
        }

        public bool Bit05
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000020);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000020;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000020);
                if (oldVal != _valueT)
                    OnBitChanged("Bit05");
            }
        }

        public bool Bit06
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000040);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000040;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000040);
                if (oldVal != _valueT)
                    OnBitChanged("Bit06");
            }
        }

        public bool Bit07
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000080);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000080;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000080);
                if (oldVal != _valueT)
                    OnBitChanged("Bit07");
            }
        }

        public bool Bit08
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000100);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000100;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000100);
                if (oldVal != _valueT)
                    OnBitChanged("Bit08");
            }
        }

        public bool Bit09
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000200);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000200;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000200);
                if (oldVal != _valueT)
                    OnBitChanged("Bit09");
            }
        }

        public bool Bit10
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000400);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000400;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000400);
                if (oldVal != _valueT)
                    OnBitChanged("Bit10");
            }
        }

        public bool Bit11
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00000800);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00000800;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00000800);
                if (oldVal != _valueT)
                    OnBitChanged("Bit11");
            }
        }

        public bool Bit12
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00001000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00001000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00001000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit12");
            }
        }

        public bool Bit13
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00002000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00002000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00002000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit13");
            }
        }

        public bool Bit14
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00004000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00004000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00004000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit14");
            }
        }

        public bool Bit15
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00008000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00008000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00008000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit15");
            }
        }

        public bool Bit16
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00010000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00010000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00010000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit16");
            }
        }

        public bool Bit17
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00020000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00020000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00020000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit17");
            }
        }

        public bool Bit18
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00040000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00040000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00040000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit18");
            }
        }

        public bool Bit19
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00080000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00080000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00080000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit19");
            }
        }

        public bool Bit20
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00100000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00100000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00100000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit20");
            }
        }

        public bool Bit21
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00200000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00200000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00200000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit21");
            }
        }

        public bool Bit22
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00400000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00400000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00400000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit22");
            }
        }

        public bool Bit23
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x00800000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x00800000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x00800000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit23");
            }
        }

        public bool Bit24
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x01000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x01000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x01000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit24");
            }
        }

        public bool Bit25
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x02000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x02000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x02000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit25");
            }
        }

        public bool Bit26
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x04000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x04000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x04000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit26");
            }
        }

        public bool Bit27
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x08000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x08000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x08000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit27");
            }
        }

        public bool Bit28
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x10000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x10000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x10000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit28");
            }
        }

        public bool Bit29
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x20000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x20000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x20000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit29");
            }
        }

        public bool Bit30
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x40000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x40000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x40000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit30");
            }
        }

        public bool Bit31
        {
            get
            {
                return Convert.ToBoolean(ValueT & 0x80000000);
            }

            set
            {
                UInt32 oldVal = _valueT;
                if (value)
                    _valueT |= 0x80000000;
                else
                    _valueT = (UInt32) unchecked(ValueT & ~0x80000000);
                if (oldVal != _valueT)
                    OnBitChanged("Bit31");
            }
        }
        #endregion
    }
}
