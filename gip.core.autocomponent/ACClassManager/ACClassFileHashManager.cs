using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Read SHA1 from file 
    /// </summary>
    public class ACClassFileHashManager
    {
        /// <summary>
        /// Get hash key for file (*)
        /// </summary>
        /// <param name="assemblyFilePath">define file path</param>
        /// <returns>40 chars string - represent SHA1 hash from given file</returns>
        public static string GetHash(string assemblyFilePath)
        {
            StringBuilder formatted = null;
            using (FileStream fs = new FileStream(assemblyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                }
            }
            return formatted.ToString();
        }
    }
}
