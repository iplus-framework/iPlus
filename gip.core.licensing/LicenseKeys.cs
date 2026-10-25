// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace gip.core.licensing
{
    /// <summary>
    /// Loads and stores RSA key pairs used for signing and verifying license files.
    /// </summary>
    /// <remarks>
    /// Two textual formats are supported so that keys created by older tooling can
    /// still be imported:
    /// <list type="bullet">
    /// <item>PEM (<c>-----BEGIN RSA PRIVATE KEY-----</c> / <c>-----BEGIN PUBLIC KEY-----</c>) — preferred.</item>
    /// <item>Legacy .NET XML (<c>&lt;RSAKeyValue&gt;...&lt;/RSAKeyValue&gt;</c>) — as exported by
    /// <c>RSACryptoServiceProvider.ToXmlString()</c>.</item>
    /// </list>
    /// </remarks>
    public static class LicenseKeys
    {
        /// <summary>
        /// Creates a new 2048 bit RSA key pair.
        /// </summary>
        public static RSA CreateKeyPair(int keySize = 2048)
        {
            return RSA.Create(keySize);
        }

        /// <summary>
        /// Exports the private key as PEM (PKCS#1).
        /// </summary>
        public static string ExportPrivateKeyPem(RSA key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return key.ExportRSAPrivateKeyPem();
        }

        /// <summary>
        /// Exports the public key as PEM (SubjectPublicKeyInfo).
        /// </summary>
        public static string ExportPublicKeyPem(RSA key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return key.ExportSubjectPublicKeyInfoPem();
        }

        /// <summary>
        /// Imports a private key from PEM or legacy XML text.
        /// </summary>
        public static RSA ImportPrivateKey(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Private key text is empty.", nameof(text));

            RSA rsa = RSA.Create();
            if (IsPem(text))
            {
                rsa.ImportFromPem(text);
                return rsa;
            }

            rsa.FromXmlString(text);
            return rsa;
        }

        /// <summary>
        /// Imports a public key from PEM or legacy XML text.
        /// </summary>
        public static RSA ImportPublicKey(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Public key text is empty.", nameof(text));

            RSA rsa = RSA.Create();
            if (IsPem(text))
            {
                rsa.ImportFromPem(text);
                return rsa;
            }

            rsa.FromXmlString(text);
            return rsa;
        }

        /// <summary>
        /// Loads a key from a file. The format is detected from the content.
        /// </summary>
        public static RSA LoadKeyFromFile(string path, bool privateKey)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Key path is empty.", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("License key file not found.", path);

            string text = File.ReadAllText(path, Encoding.UTF8);
            return privateKey ? ImportPrivateKey(text) : ImportPublicKey(text);
        }

        /// <summary>
        /// Writes a key to a file using PEM encoding.
        /// </summary>
        public static void SaveKeyToFile(RSA key, string path, bool privateKey)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Key path is empty.", nameof(path));

            string text = privateKey ? ExportPrivateKeyPem(key) : ExportPublicKeyPem(key);
            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        /// <summary>
        /// Returns <c>true</c> when the text looks like a PEM block.
        /// </summary>
        public static bool IsPem(string text)
        {
            return text != null && text.Contains("-----BEGIN", StringComparison.Ordinal);
        }

        /// <summary>
        /// Computes the issuer key id for a public key: the first 16 hex chars of
        /// the SHA-256 hash over the DER-encoded SubjectPublicKeyInfo.
        /// This is what <see cref="LicensePayload.IssuerKeyId"/> stores so that the
        /// framework can pick the right developer key without trying all of them.
        /// </summary>
        public static string ComputeKeyId(RSA key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            byte[] der = key.ExportSubjectPublicKeyInfo();
            byte[] hash = SHA256.HashData(der);
            return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        }

        /// <summary>
        /// Computes the issuer key id from a public key in PEM or legacy XML text.
        /// </summary>
        public static string ComputeKeyIdFromText(string publicKeyText)
        {
            using (RSA key = ImportPublicKey(publicKeyText))
            {
                return ComputeKeyId(key);
            }
        }
    }
}
