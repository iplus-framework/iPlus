// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using System.Text;

namespace gip.core.licensing
{
    /// <summary>
    /// Creates signed license files. Used by the license issuer tooling
    /// (<c>gip.tool.licenseIssuer</c>) and by developers that issue product
    /// licenses for their own packages.
    /// </summary>
    public sealed class LicenseIssuer
    {
        private readonly RSA _privateKey;

        /// <summary>
        /// Creates an issuer for the given private key.
        /// </summary>
        public LicenseIssuer(RSA privateKey)
        {
            _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        /// <summary>
        /// Creates an issuer from a private key in PEM or legacy XML text.
        /// </summary>
        public static LicenseIssuer FromPrivateKeyText(string privateKeyText)
        {
            return new LicenseIssuer(LicenseKeys.ImportPrivateKey(privateKeyText));
        }

        /// <summary>
        /// Creates an issuer from a private key file.
        /// </summary>
        public static LicenseIssuer FromPrivateKeyFile(string path)
        {
            return new LicenseIssuer(LicenseKeys.LoadKeyFromFile(path, privateKey: true));
        }

        /// <summary>
        /// Signs the payload and returns the complete license file text.
        /// </summary>
        public string Issue(LicensePayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            string json = LicenseFile.SerializePayload(payload);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(json);
            byte[] signature = _privateKey.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return LicenseFile.Build(payload, signature);
        }

        /// <summary>
        /// Signs the payload and writes the license file to disk.
        /// </summary>
        public void IssueToFile(LicensePayload payload, string path)
        {
            string text = Issue(payload);
            string? directory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
                System.IO.Directory.CreateDirectory(directory);
            System.IO.File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        /// <summary>
        /// Returns the public key (PEM) that corresponds to the issuer's private key.
        /// Consumers need this key to verify the issued licenses.
        /// </summary>
        public string GetPublicKeyPem()
        {
            return LicenseKeys.ExportPublicKeyPem(_privateKey);
        }

        /// <summary>
        /// Returns the issuer key id (see <see cref="LicenseKeys.ComputeKeyId"/>)
        /// that corresponds to the issuer's private key.
        /// </summary>
        public string GetKeyId()
        {
            return LicenseKeys.ComputeKeyId(_privateKey);
        }

        /// <summary>
        /// Signs the payload and returns the complete license file text, stamping
        /// <see cref="LicensePayload.IssuerKeyId"/> with this issuer's key id so
        /// that the verifier can resolve the matching public key.
        /// </summary>
        public string IssueWithKeyId(LicensePayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            payload.IssuerKeyId = GetKeyId();
            return Issue(payload);
        }
    }
}
