using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    /// <summary>
    /// Endianess. Windows stores as LitteEndian. Therefore the examples of byte values means: 0A is least significant byte...3D is most significant byte
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Endianess'}de{'Endianess'}", Global.ACKinds.TACEnum)]
    public enum EndianessEnum : short
    {
        /// <summary>
        /// BigEndian: Most significant BYTE is stored first. Example: [0]:3D [1]:2C [2]:1B [3]:0A
        /// </summary>
        BigEndian = 0,

        /// <summary>
        /// LittleEndian: Least significant BYTE is stored first. Example: [0]:0A [1]:1B [2]:2C [3]:3D
        /// </summary>
        LittleEndian = 1,

        /// <summary>
        /// MiddleEndian: Most significant WORD is stored first. Each WORD is stored as LitteEndian. Example: [0]:2C [1]:3D [2]:0A [3]:1B
        /// </summary>
        MiddleEndian = 2,

        /// <summary>
        /// MixedEndian: Least significant WORD is stored first. Each WORD is stored as BigEndian. Example: [0]:1B [1]:0A [2]:3D [3]:2C
        /// </summary>
        MixedEndian = 3
    }

    public static class EndianessHelper
    {
        #region Int16
        public static Int16 Int16FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess)
        {
            byte[] sub = new byte[2];
            Array.Copy(bytes, startindex, sub, 0, 2);
            return Int16FromByteArray(sub, endianess);
        }
        
        public static Int16 Int16FromByteArray(byte[] bytes, EndianessEnum endianess)
        {
            Int16 value = 0;
            byte[] littleEndian = new byte[2];
            if ((endianess == EndianessEnum.BigEndian)
                || (endianess == EndianessEnum.MixedEndian))
            {
                littleEndian[0] = bytes[1];
                littleEndian[1] = bytes[0];
            }
            else if ((endianess == EndianessEnum.LittleEndian)
                    || (endianess == EndianessEnum.MiddleEndian))
            {
                littleEndian = bytes;
            }
            value = BitConverter.ToInt16(littleEndian, 0);
            return value;
        }


        public static byte[] Int16ToByteArray(Int16 value, EndianessEnum endianess)
        {
            byte[] littleEndian = BitConverter.GetBytes(value);

            byte[] bytes = new byte[2];
            if ((endianess == EndianessEnum.BigEndian)
                || (endianess == EndianessEnum.MixedEndian))
            {
                bytes[1] = littleEndian[0];
                bytes[0] = littleEndian[1];
            }
            else if ((endianess == EndianessEnum.LittleEndian)
                    || (endianess == EndianessEnum.MiddleEndian))
            {
                bytes = littleEndian;
            }
            return bytes;
        }


        public static byte[] Int16ToByteArray(Int16[] value, EndianessEnum endianess)
        {
            ByteArray arr = new ByteArray();
            foreach (Int16 val in value)
                arr.Add(Int16ToByteArray(val, endianess));
            return arr.array;
        }


        public static Int16[] Int16ToArray(byte[] bytes, EndianessEnum endianess)
        {
            Int16[] values = new Int16[bytes.Length / 2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = Int16FromByteArray(new byte[] { bytes[counter++], bytes[counter++] }, endianess);

            return values;
        }
        #endregion


        #region Int32
        public static Int32 Int32FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess)
        {
            byte[] sub = new byte[4];
            Array.Copy(bytes, startindex, sub, 0, 4);
            return Int32FromByteArray(sub, endianess);
        }
        
        public static Int32 Int32FromByteArray(byte[] bytes, EndianessEnum endianess)
        {
            Int32 value = 0;
            byte[] littleEndian = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                littleEndian[0] = bytes[3];
                littleEndian[1] = bytes[2];
                littleEndian[2] = bytes[1];
                littleEndian[3] = bytes[0];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                littleEndian = bytes;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                littleEndian[0] = bytes[2];
                littleEndian[1] = bytes[3];
                littleEndian[2] = bytes[0];
                littleEndian[3] = bytes[1];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                littleEndian[0] = bytes[1];
                littleEndian[1] = bytes[0];
                littleEndian[2] = bytes[3];
                littleEndian[3] = bytes[2];
            }
            value = BitConverter.ToInt32(littleEndian, 0);
            return value;
        }

        public static byte[] Int32ToByteArray(Int32 value, EndianessEnum endianess)
        {
            byte[] littleEndian = BitConverter.GetBytes(value);

            byte[] bytes = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                bytes[3] = littleEndian[0];
                bytes[2] = littleEndian[1];
                bytes[1] = littleEndian[2];
                bytes[0] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                bytes = littleEndian;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                bytes[2] = littleEndian[0];
                bytes[3] = littleEndian[1];
                bytes[0] = littleEndian[2];
                bytes[1] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                bytes[1] = littleEndian[0];
                bytes[0] = littleEndian[1];
                bytes[3] = littleEndian[2];
                bytes[2] = littleEndian[3];
            }
            return bytes;
        }

        public static byte[] Int32ToByteArray(Int32[] value, EndianessEnum endianess)
        {
            ByteArray arr = new ByteArray();
            foreach (Int32 val in value)
                arr.Add(Int32ToByteArray(val, endianess));
            return arr.array;
        }

        public static Int32[] Int32ToArray(byte[] bytes, EndianessEnum endianess)
        {
            Int32[] values = new Int32[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = Int32FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] }, endianess);

            return values;
        }
        #endregion


        #region UInt16
        public static UInt16 UInt16FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess)
        {
            byte[] sub = new byte[2];
            Array.Copy(bytes, startindex, sub, 0, 2);
            return UInt16FromByteArray(sub, endianess);
        }

        public static UInt16 UInt16FromByteArray(byte[] bytes, EndianessEnum endianess)
        {
            UInt16 value = 0;
            byte[] littleEndian = new byte[2];
            if ((endianess == EndianessEnum.BigEndian)
                || (endianess == EndianessEnum.MixedEndian))
            {
                littleEndian[0] = bytes[1];
                littleEndian[1] = bytes[0];
            }
            else if ((endianess == EndianessEnum.LittleEndian)
                    || (endianess == EndianessEnum.MiddleEndian))
            {
                littleEndian = bytes;
            }
            value = BitConverter.ToUInt16(littleEndian, 0);
            return value;
        }

        public static byte[] UInt16ToByteArray(UInt16 value, EndianessEnum endianess)
        {
            byte[] littleEndian = BitConverter.GetBytes(value);

            byte[] bytes = new byte[2];
            if ((endianess == EndianessEnum.BigEndian)
                || (endianess == EndianessEnum.MixedEndian))
            {
                bytes[1] = littleEndian[0];
                bytes[0] = littleEndian[1];
            }
            else if ((endianess == EndianessEnum.LittleEndian)
                    || (endianess == EndianessEnum.MiddleEndian))
            {
                bytes = littleEndian;
            }
            return bytes;
        }

        public static byte[] UInt16ToByteArray(UInt16[] value, EndianessEnum endianess)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt16 val in value)
                arr.Add(UInt16ToByteArray(val, endianess));
            return arr.array;
        }

        public static UInt16[] UInt16ToArray(byte[] bytes, EndianessEnum endianess)
        {
            UInt16[] values = new UInt16[bytes.Length / 2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = UInt16FromByteArray(new byte[] { bytes[counter++], bytes[counter++] }, endianess);

            return values;
        }
        #endregion

        
        #region UInt32
        public static UInt32 UInt32FromByteArray(byte[] bytes, int startindex, EndianessEnum endianess)
        {
            byte[] sub = new byte[4];
            Array.Copy(bytes, startindex, sub, 0, 4);
            return UInt32FromByteArray(sub, endianess);
        }

        public static UInt32 UInt32FromByteArray(byte[] bytes, EndianessEnum endianess)
        {
            UInt32 value = 0;
            byte[] littleEndian = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                littleEndian[0] = bytes[3];
                littleEndian[1] = bytes[2];
                littleEndian[2] = bytes[1];
                littleEndian[3] = bytes[0];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                littleEndian = bytes;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                littleEndian[0] = bytes[2];
                littleEndian[1] = bytes[3];
                littleEndian[2] = bytes[0];
                littleEndian[3] = bytes[1];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                littleEndian[0] = bytes[1];
                littleEndian[1] = bytes[0];
                littleEndian[2] = bytes[3];
                littleEndian[3] = bytes[2];
            }
            value = BitConverter.ToUInt32(littleEndian, 0);
            return value;
        }


        public static byte[] UInt32ToByteArray(UInt32 value, EndianessEnum endianess)
        {
            byte[] littleEndian = BitConverter.GetBytes(value);

            byte[] bytes = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                bytes[3] = littleEndian[0];
                bytes[2] = littleEndian[1];
                bytes[1] = littleEndian[2];
                bytes[0] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                bytes = littleEndian;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                bytes[2] = littleEndian[0];
                bytes[3] = littleEndian[1];
                bytes[0] = littleEndian[2];
                bytes[1] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                bytes[1] = littleEndian[0];
                bytes[0] = littleEndian[1];
                bytes[3] = littleEndian[2];
                bytes[2] = littleEndian[3];
            }
            return bytes;
        }


        public static byte[] UInt32ToByteArray(UInt32[] value, EndianessEnum endianess)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt32 val in value)
                arr.Add(UInt32ToByteArray(val, endianess));
            return arr.array;
        }


        public static UInt32[] UInt32ToArray(byte[] bytes, EndianessEnum endianess)
        {
            UInt32[] values = new UInt32[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = UInt32FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] }, endianess);

            return values;
        }
        #endregion


        #region Single
        public static Single SingleFromByteArray(byte[] bytes, int startindex, EndianessEnum endianess)
        {
            byte[] sub = new byte[4];
            Array.Copy(bytes, startindex, sub, 0, 4);
            return SingleFromByteArray(sub, endianess);
        }

        public static Single SingleFromByteArray(byte[] bytes, EndianessEnum endianess)
        {
            Single value = 0;
            byte[] littleEndian = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                littleEndian[0] = bytes[3];
                littleEndian[1] = bytes[2];
                littleEndian[2] = bytes[1];
                littleEndian[3] = bytes[0];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                littleEndian = bytes;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                littleEndian[0] = bytes[2];
                littleEndian[1] = bytes[3];
                littleEndian[2] = bytes[0];
                littleEndian[3] = bytes[1];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                littleEndian[0] = bytes[1];
                littleEndian[1] = bytes[0];
                littleEndian[2] = bytes[3];
                littleEndian[3] = bytes[2];
            }
            value = BitConverter.ToSingle(littleEndian, 0);
            return value;
        }

        public static byte[] SingleToByteArray(Single value, EndianessEnum endianess)
        {

            byte[] littleEndian = BitConverter.GetBytes(value);

            byte[] bytes = new byte[4];
            if (endianess == EndianessEnum.BigEndian)
            {
                bytes[3] = littleEndian[0];
                bytes[2] = littleEndian[1];
                bytes[1] = littleEndian[2];
                bytes[0] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.LittleEndian)
            {
                bytes = littleEndian;
            }
            else if (endianess == EndianessEnum.MiddleEndian)
            {
                bytes[2] = littleEndian[0];
                bytes[3] = littleEndian[1];
                bytes[0] = littleEndian[2];
                bytes[1] = littleEndian[3];
            }
            else if (endianess == EndianessEnum.MixedEndian)
            {
                bytes[1] = littleEndian[0];
                bytes[0] = littleEndian[1];
                bytes[3] = littleEndian[2];
                bytes[2] = littleEndian[3];
            }
            return bytes;
        }

        public static byte[] SingleToByteArray(Single[] value, EndianessEnum endianess)
        {
            ByteArray arr = new ByteArray();
            foreach (Single val in value)
                arr.Add(SingleToByteArray(val, endianess));
            return arr.array;
        }

        public static Single[] SingleToArray(byte[] bytes, EndianessEnum endianess)
        {
            Single[] values = new Single[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = SingleFromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] }, endianess);

            return values;
        }
        #endregion
    }
}
