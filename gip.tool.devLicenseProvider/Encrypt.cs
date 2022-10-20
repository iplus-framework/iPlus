using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.devLicenseProvider
{
    public static class Encrypt
    {
        public static string EncryptString(string plainText, byte[] pass)
        {
            // Instantiate a new RijndaelManaged object to perform string symmetric encryption
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            // Set key and IV
            rijndaelCipher.Key = pass;
            rijndaelCipher.IV = pass.Take(rijndaelCipher.BlockSize / 8).ToArray();

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our RijndaelManaged object
            ICryptoTransform rijndaelEncryptor = rijndaelCipher.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelEncryptor, CryptoStreamMode.Write);

            // Convert the plainText string into a byte array
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert the encrypted byte array to a base64 encoded string
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            // Return the encrypted data as a string
            return cipherText;
        }

        public static string DecryptString(string cipherText, byte[] pass)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Key = pass;
            rijndaelCipher.IV = pass.Take(rijndaelCipher.BlockSize / 8).ToArray();

            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform rijndaelDecryptor = rijndaelCipher.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelDecryptor, CryptoStreamMode.Write);

            string plainText = String.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                cryptoStream.FlushFinalBlock();

                byte[] plainBytes = memoryStream.ToArray();
                plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return plainText;
        }
    }
}
