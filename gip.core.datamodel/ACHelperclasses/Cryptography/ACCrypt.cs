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
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
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

        #region Rijndael

        static byte[] RijndaelEncrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();

            // Algorithm. Rijndael is available on all platforms.

            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

            //CryptoStream is for pumping our data.

            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        static byte[] RijndaelDecrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
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
        public static string RijndaelEncrypt(string clearText, string Password)
        {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            // PasswordDeriveBytes is for getting Key and IV.
            // Using PasswordDeriveBytes object we are first getting 32 bytes for the Key (the default
            //Rijndael key length is 256bit = 32bytes) and then 16 bytes for the IV.
            // IV should always be the block size, which is by default 16 bytes (128 bit) for Rijndael.

            byte[] encryptedData = RijndaelEncrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            string rez = Convert.ToBase64String(encryptedData);
            return rez;
        }

        /// <summary>
        /// Static string decrypt
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static string RijndaelDecrypt(string cipherText, string Password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
            new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = RijndaelDecrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        #endregion
    }
}
