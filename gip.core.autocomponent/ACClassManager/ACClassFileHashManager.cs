// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
                SHA1 sha1 = SHA1.Create();
                byte[] hash = sha1.ComputeHash(bs);
                formatted = new StringBuilder(2 * hash.Length);
                foreach (byte b in hash)
                {
                    formatted.AppendFormat("{0:X2}", b);
                }
                sha1.Dispose();
            }
            return formatted.ToString();
        }
    }
}
