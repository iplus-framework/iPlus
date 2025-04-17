// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACCrypt.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq;

namespace gip.core.datamodel
{
    public static class ByteExtension
    {
        internal static string ToByteString(this byte[] array)
        {
            string result = "";
            if (array == null)
                return result;

            foreach (byte b in array)
                result += b;
            return result;
        }

        public static string ToByteStringKey(this byte[] array)
        {
            string result = "-";
            foreach (byte b in array)
                result += b + "-";
            result = result.TrimStart('-').TrimEnd('-');
            return result;
        }

        public static byte[] FromByteStringKey(string byteArray)
        {
            string[] parts = byteArray.Split('-');
            int count = parts.Count();
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (byte)int.Parse(parts[i]);
            }
            return result;
        }
    }
}
