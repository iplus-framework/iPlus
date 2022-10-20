using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.communication.ISOonTCP.Types
{
    public static class Struct
    {
        /// <summary>
        /// Gets the size of the struct in bytes.
        /// </summary>
        /// <param name="structType">the type of the struct</param>
        /// <returns>the number of bytes</returns>
        public static int GetStructSize(Type structType)
        {
            double numBytes = 0.0;

            System.Reflection.FieldInfo[] infos = structType.GetFields();
            foreach (System.Reflection.FieldInfo info in infos)
            {
                switch (info.FieldType.Name)
                {
                    case Const.TNameBoolean:
                        numBytes += 0.125;
                        break;
                    case Const.TNameByte:
                        numBytes = Math.Ceiling(numBytes);
                        numBytes++;
                        break;
                    case Const.TNameInt16:
                    case Const.TNameUInt16:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 2;
                        break;
                    case Const.TNameInt32:
                    case Const.TNameUInt32:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 4;
                        break;
                    case Const.TNameFloat: 
                    case Const.TNameDouble:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        numBytes += 4;
                        break;
                }
            }
            return (int)numBytes;
        }

        /// <summary>Creates a struct of a specified type by an array of bytes.</summary>
        /// <param name="structType">The struct type</param>
        /// <param name="bytes">The array of bytes</param>
        /// <param name="endianess"></param>
        /// <returns>The object depending on the struct type or null if fails(array-length != struct-length</returns>
        public static object FromBytes(Type structType, byte[] bytes, EndianessEnum endianess)
        {
            if (bytes == null)
                return null;

            if (bytes.Length != GetStructSize(structType))
                return null;

            // and decode it
            int bytePos = 0;
            int bitPos = 0;
            double numBytes = 0.0;
            object structValue = Activator.CreateInstance(structType);

            System.Reflection.FieldInfo[] infos = structValue.GetType().GetFields();
            foreach (System.Reflection.FieldInfo info in infos)
            {
                switch (info.FieldType.Name)
                {
                    case Const.TNameBoolean:
                        // get the value
                        bytePos = (int)Math.Floor(numBytes);
                        bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                        if ((bytes[bytePos] & (int)Math.Pow(2, bitPos)) != 0)
                            info.SetValue(structValue, true);
                        else
                            info.SetValue(structValue, false);
                        numBytes += 0.125;
                        break;
                    case Const.TNameByte:
                        numBytes = Math.Ceiling(numBytes);
                        info.SetValue(structValue, (byte)(bytes[(int)numBytes]));
                        numBytes++;
                        break;
                    case Const.TNameInt16:
                    case Const.TNameUInt16:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // hier auswerten
                        byte[] arr = new byte[] { bytes[(int)numBytes], bytes[(int)numBytes + 1] };
                        info.SetValue(structValue, gip.core.communication.ISOonTCP.Types.Word.FromByteArray(arr, endianess));
                        numBytes += 2;
                        break;
                    case Const.TNameInt32:
                    case Const.TNameUInt32:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // hier auswerten
                        byte[] arr2 = new byte[] { bytes[(int)numBytes], bytes[(int)numBytes + 1], bytes[(int)numBytes + 2] , bytes[(int)numBytes + 3]  };
                        info.SetValue(structValue, gip.core.communication.ISOonTCP.Types.DWord.FromByteArray(arr2, endianess));
                        numBytes += 4;
                        break;
                    case Const.TNameDouble:
                        numBytes = Math.Ceiling(numBytes);
                        if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                            numBytes++;
                        // hier auswerten
                        info.SetValue(structValue, gip.core.communication.ISOonTCP.Types.Real.FromByteArray(new byte[] { bytes[(int)numBytes],
                                                                           bytes[(int)numBytes + 1],
                                                                           bytes[(int)numBytes + 2],
                                                                           bytes[(int)numBytes + 3] }, 
                                                                           endianess));
                        numBytes += 4;
                        break;
                }
            }
            return structValue;
        }

        /// <summary>Creates a byte array depending on the struct type.</summary>
        /// <param name="structValue">The struct object</param>
        /// <param name="endianess"></param>
        /// <returns>A byte array or null if fails.</returns>
        public static byte[] ToBytes(object structValue, EndianessEnum endianess)
        {
            Type type = structValue.GetType();

            int size = gip.core.communication.ISOonTCP.Types.Struct.GetStructSize(type);
            byte[] bytes = new byte[size];
            byte[] bytes2 = null;

            int bytePos = 0;
            int bitPos = 0;
            double numBytes = 0.0;

            System.Reflection.FieldInfo[] infos = type.GetFields();
            foreach (System.Reflection.FieldInfo info in infos)
            {
                bytes2 = null;
                switch (info.FieldType.Name)
                {
                    case Const.TNameBoolean:
                        // get the value
                        bytePos = (int)Math.Floor(numBytes);
                        bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                        if ((bool)info.GetValue(structValue))
                            bytes[bytePos] |= (byte)Math.Pow(2, bitPos);            // is true
                        else
                            bytes[bytePos] &= (byte)(~(byte)Math.Pow(2, bitPos));   // is false
                        numBytes += 0.125;
                        break;
                    case Const.TNameByte:
                        numBytes = (int)Math.Ceiling(numBytes);
                        bytePos = (int)numBytes;
                        bytes[bytePos] = (byte)info.GetValue(structValue);
                        numBytes++;
                        break;
                    case Const.TNameInt16:
                        bytes2 = gip.core.communication.ISOonTCP.Types.Int.ToByteArray((Int16)info.GetValue(structValue), endianess);
                        break;
                    case Const.TNameUInt16:
                        bytes2 = gip.core.communication.ISOonTCP.Types.Word.ToByteArray((UInt16)info.GetValue(structValue), endianess);
                        break;
                    case Const.TNameInt32:
                        bytes2 = gip.core.communication.ISOonTCP.Types.DInt.ToByteArray((Int32)info.GetValue(structValue), endianess);
                        break;
                    case Const.TNameUInt32:
                        bytes2 = gip.core.communication.ISOonTCP.Types.DWord.ToByteArray((UInt32)info.GetValue(structValue), endianess);
                        break;
                    case Const.TNameDouble:
                        bytes2 = gip.core.communication.ISOonTCP.Types.Real.ToByteArray((float)info.GetValue(structValue), endianess);
                        break;
                }
                if (bytes2 != null)
                {
                    // add them
                    numBytes = Math.Ceiling(numBytes);
                    if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                        numBytes++;
                    bytePos = (int)numBytes;
                    for (int bCnt=0; bCnt<bytes2.Length; bCnt++)
                        bytes[bytePos + bCnt] = bytes2[bCnt];
                    numBytes += bytes2.Length;
                }
            }
            return bytes;
        }
    }
}
