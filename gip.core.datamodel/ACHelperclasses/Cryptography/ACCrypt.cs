// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.IO;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACCrypt
    /// </summary>
    public static class ACCrypt
    {
        #region MD5
        /// <summary>
        /// Gets the MD5 hash.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        public static string GetMd5Hash(string input)
        {
            byte[] data = ComputeMd5(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Computes the MD5 hash of the given data. Uses the platform <see cref="MD5"/> when
        /// available; falls back to a pure managed RFC-1321 implementation on platforms where
        /// MD5 is not supported (e.g. Browser/WASM: Cryptography_UnknownHashAlgorithm, MD5).
        /// </summary>
        public static byte[] ComputeMd5(byte[] input)
        {
            try
            {
                using (MD5 md5Hasher = MD5.Create())
                    return md5Hasher.ComputeHash(input);
            }
            catch (CryptographicException)
            {
                return ManagedMd5.ComputeHash(input);
            }
        }

        /// <summary>
        /// Pure managed MD5 (RFC 1321) for platforms without a built-in MD5 implementation.
        /// </summary>
        internal static class ManagedMd5
        {
            private static readonly int[] S = {
                7,12,17,22, 7,12,17,22, 7,12,17,22, 7,12,17,22,
                5, 9,14,20, 5, 9,14,20, 5, 9,14,20, 5, 9,14,20,
                4,11,16,23, 4,11,16,23, 4,11,16,23, 4,11,16,23,
                6,10,15,21, 6,10,15,21, 6,10,15,21, 6,10,15,21 };

            private static readonly uint[] K = new uint[64];
            static ManagedMd5()
            {
                for (int i = 0; i < 64; i++)
                    K[i] = (uint)Math.Floor(Math.Abs(Math.Sin(i + 1)) * 4294967296.0);
            }

            public static byte[] ComputeHash(byte[] input)
            {
                uint a0 = 0x67452301, b0 = 0xefcdab89, c0 = 0x98badcfe, d0 = 0x10325476;

                // Padding: append 0x80, pad with zeros, append 64-bit little-endian bit length
                int newLen = ((input.Length + 8) / 64 + 1) * 64;
                byte[] msg = new byte[newLen];
                input.CopyTo(msg, 0);
                msg[input.Length] = 0x80;
                ulong bitLen = (ulong)input.Length * 8;
                for (int i = 0; i < 8; i++)
                    msg[newLen - 8 + i] = (byte)(bitLen >> (8 * i));

                for (int chunk = 0; chunk < newLen; chunk += 64)
                {
                    uint[] m = new uint[16];
                    for (int j = 0; j < 16; j++)
                        m[j] = msg[chunk + j * 4]
                             | ((uint)msg[chunk + j * 4 + 1] << 8)
                             | ((uint)msg[chunk + j * 4 + 2] << 16)
                             | ((uint)msg[chunk + j * 4 + 3] << 24);

                    uint a = a0, b = b0, c = c0, d = d0;
                    for (int i = 0; i < 64; i++)
                    {
                        uint f, g;
                        if (i < 16) { f = (b & c) | (~b & d); g = (uint)i; }
                        else if (i < 32) { f = (d & b) | (~d & c); g = (uint)((5 * i + 1) % 16); }
                        else if (i < 48) { f = b ^ c ^ d; g = (uint)((3 * i + 5) % 16); }
                        else { f = c ^ (b | ~d); g = (uint)((7 * i) % 16); }
                        f = unchecked(f + a + K[i] + m[g]);
                        a = d; d = c; c = b;
                        b = unchecked(b + ((f << S[i]) | (f >> (32 - S[i]))));
                    }
                    a0 = unchecked(a0 + a); b0 = unchecked(b0 + b);
                    c0 = unchecked(c0 + c); d0 = unchecked(d0 + d);
                }

                byte[] hash = new byte[16];
                WriteLe(hash, 0, a0); WriteLe(hash, 4, b0);
                WriteLe(hash, 8, c0); WriteLe(hash, 12, d0);
                return hash;
            }

            private static void WriteLe(byte[] buf, int offset, uint value)
            {
                buf[offset] = (byte)value;
                buf[offset + 1] = (byte)(value >> 8);
                buf[offset + 2] = (byte)(value >> 16);
                buf[offset + 3] = (byte)(value >> 24);
            }
        }

        /// <summary>
        /// Verifies the MD5 hash.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="hash">The hash.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool VerifyMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfInput, hash))
                return true;
            else
                return false;
        }
        #endregion

        #region Aes / old Rijndael

        static byte[] AesEncrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Aes alg = Aes.Create();

            // Algorithm. Rijndael is available on all platforms.
            // Rjindael is now obsolete. Use Aes instead, the functionality is the same

            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

            //CryptoStream is for pumping our data.

            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        static byte[] AesDecrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Aes alg = Aes.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="clearText"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static string AesEncrypt(string clearText, string Password)
        {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            // PasswordDeriveBytes is for getting Key and IV.
            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key (the default
            //Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV.
            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael.

            byte[] encryptedData = AesEncrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            string rez = Convert.ToBase64String(encryptedData);
            return rez;
        }

        /// <summary>
        /// Static string decrypt
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static string AesDecrypt(string cipherText, string Password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = AesDecrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        #endregion
    }
}
