// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gip.core.datamodel.Licensing
{
    internal class CSP
    {
        internal static bool Verify(byte[] content, byte[] signature, string key)
        {
            using (var csp = new RSACryptoServiceProvider())
            {
                try
                {
                    csp.FromXmlString(key);
                    return csp.VerifyData(content, SHA256.Create(), signature);
                }
                catch (SEHException)
                {
                    return VerifyRsaBouncy(content, signature, key);
                }
                catch (CryptographicException)
                {
                    return VerifyRsaBouncy(content, signature, key);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CSP", "Verify", msg);

                    return false;
                }
            }
        }

        internal static bool VerifyDSA(byte[] content, byte[] signature, string key)
        {
            using (var dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    dsa.FromXmlString(key);
                    return dsa.VerifyData(content, signature);
                }
                catch (SEHException)
                {
                    return VerifyDsaBouncy(content, signature, key);
                }
                catch (CryptographicException)
                {
                    return VerifyDsaBouncy(content, signature, key);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CSP", "VerifyDSA", msg);

                    return false;
                }
            }
        }

        internal static SignInfo SignData(byte[] content)
        {
            try
            {
                SignInfo info = new SignInfo();
                CspParameters cspParams = new CspParameters();
                cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048, cspParams))
                {
                    rsa.PersistKeyInCsp = false;
                    info.PrivateKey = rsa.ToXmlString(true);
                    info.PublicKey = rsa.ToXmlString(false);
                    info.Signature = rsa.SignData(content, SHA256.Create());
                }
                return info;
            }
            catch (SEHException)
            {
                return SignDataRsaBouncy(content);
            }
            catch (CryptographicException)
            {
                return SignDataRsaBouncy(content);
            }
        }

        internal static SignInfo SignData(byte[] content, string privateKey)
        {
            try
            {
                SignInfo info = new SignInfo();
                CspParameters cspParams = new CspParameters();
                cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspParams))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(privateKey);
                    info.PrivateKey = privateKey;
                    info.PublicKey = GetRsaPublicKeyFromPrivateXml(privateKey);
                    info.Signature = rsa.SignData(content, SHA256.Create());
                }
                return info;
            }
            catch (SEHException)
            {
                return SignDataRsaBouncy(content, privateKey);
            }
            catch (CryptographicException)
            {
                return SignDataRsaBouncy(content, privateKey);
            }
        }

        internal static SignInfo SignDataDSA(byte[] content)
        {
            try
            {
                SignInfo info = new SignInfo();
                // Use PROV_DSS (13) with MachineKeyStore to avoid WINE User profile issues
                CspParameters cspParams = new CspParameters(13);
                cspParams.Flags = CspProviderFlags.UseMachineKeyStore;

                using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(cspParams))
                {
                    // Do not persist the key to the store
                    dsa.PersistKeyInCsp = false;
                    info.PrivateKey = dsa.ToXmlString(true);
                    info.PublicKey = dsa.ToXmlString(false);
                    info.Signature = dsa.SignData(content);
                }
                return info;
            }
            catch (SEHException)
            {
                return SignDataDsaBouncy(content);
            }
            catch (CryptographicException)
            {
                return SignDataDsaBouncy(content);
            }
        }

        private static SignInfo SignDataRsaBouncy(byte[] content)
        {
            var random = new SecureRandom();
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(new KeyGenerationParameters(random, 2048));
            var keyPair = keyGen.GenerateKeyPair();

            var signer = SignerUtilities.GetSigner("SHA-256withRSA");
            signer.Init(true, keyPair.Private);
            signer.BlockUpdate(content, 0, content.Length);
            var signature = signer.GenerateSignature();

            var privateKey = keyPair.Private as RsaPrivateCrtKeyParameters;
            var publicKey = keyPair.Public as RsaKeyParameters;

            return new SignInfo
            {
                PrivateKey = RsaToXml(publicKey, privateKey),
                PublicKey = RsaToXml(publicKey, null),
                Signature = signature
            };
        }

        private static SignInfo SignDataRsaBouncy(byte[] content, string privateKeyXml)
        {
            var privateKey = ParseRsaPrivateKey(privateKeyXml);
            var publicKey = new RsaKeyParameters(false, privateKey.Modulus, privateKey.PublicExponent);

            var signer = SignerUtilities.GetSigner("SHA-256withRSA");
            signer.Init(true, privateKey);
            signer.BlockUpdate(content, 0, content.Length);
            var signature = signer.GenerateSignature();

            return new SignInfo
            {
                PrivateKey = privateKeyXml,
                PublicKey = GetRsaPublicKeyFromPrivateXml(privateKeyXml),
                Signature = signature
            };
        }

        private static bool VerifyRsaBouncy(byte[] content, byte[] signature, string publicKeyXml)
        {
            var publicKey = ParseRsaPublicKey(publicKeyXml);
            var verifier = SignerUtilities.GetSigner("SHA-256withRSA");
            verifier.Init(false, publicKey);
            verifier.BlockUpdate(content, 0, content.Length);
            return verifier.VerifySignature(signature);
        }

        private static SignInfo SignDataDsaBouncy(byte[] content)
        {
            var random = new SecureRandom();
            var paramGen = new DsaParametersGenerator();
            paramGen.Init(1024, 80, random);
            var dsaParams = paramGen.GenerateParameters();

            var keyGen = new DsaKeyPairGenerator();
            keyGen.Init(new DsaKeyGenerationParameters(random, dsaParams));
            var keyPair = keyGen.GenerateKeyPair();

            var signer = SignerUtilities.GetSigner("SHA-1withDSA");
            signer.Init(true, keyPair.Private);
            signer.BlockUpdate(content, 0, content.Length);
            var signature = signer.GenerateSignature();

            var privateKey = keyPair.Private as DsaPrivateKeyParameters;
            var publicKey = keyPair.Public as DsaPublicKeyParameters;

            return new SignInfo
            {
                PrivateKey = DsaToXml(publicKey, privateKey),
                PublicKey = DsaToXml(publicKey, null),
                Signature = signature
            };
        }

        private static bool VerifyDsaBouncy(byte[] content, byte[] signature, string publicKeyXml)
        {
            var publicKey = ParseDsaPublicKey(publicKeyXml);
            var verifier = SignerUtilities.GetSigner("SHA-1withDSA");
            verifier.Init(false, publicKey);
            verifier.BlockUpdate(content, 0, content.Length);
            return verifier.VerifySignature(signature);
        }

        private static string GetRsaPublicKeyFromPrivateXml(string privateKeyXml)
        {
            var publicKey = ParseRsaPublicKey(privateKeyXml);
            return RsaToXml(publicKey, null);
        }

        private static RsaKeyParameters ParseRsaPublicKey(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "RSAKeyValue")
                throw new CryptographicException("Invalid RSA XML key format.");

            var modulus = ReadXmlElement(root, "Modulus");
            var exponent = ReadXmlElement(root, "Exponent");
            if (modulus == null || exponent == null)
                throw new CryptographicException("Invalid RSA public key data.");

            return new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(1, modulus), new Org.BouncyCastle.Math.BigInteger(1, exponent));
        }

        private static RsaPrivateCrtKeyParameters ParseRsaPrivateKey(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "RSAKeyValue")
                throw new CryptographicException("Invalid RSA XML key format.");

            var modulus = ReadXmlElement(root, "Modulus");
            var exponent = ReadXmlElement(root, "Exponent");
            var d = ReadXmlElement(root, "D");
            var p = ReadXmlElement(root, "P");
            var q = ReadXmlElement(root, "Q");
            var dp = ReadXmlElement(root, "DP");
            var dq = ReadXmlElement(root, "DQ");
            var inverseQ = ReadXmlElement(root, "InverseQ");

            if (modulus == null || exponent == null || d == null || p == null || q == null || dp == null || dq == null || inverseQ == null)
                throw new CryptographicException("Invalid RSA private key data.");

            return new RsaPrivateCrtKeyParameters(
                new Org.BouncyCastle.Math.BigInteger(1, modulus),
                new Org.BouncyCastle.Math.BigInteger(1, exponent),
                new Org.BouncyCastle.Math.BigInteger(1, d),
                new Org.BouncyCastle.Math.BigInteger(1, p),
                new Org.BouncyCastle.Math.BigInteger(1, q),
                new Org.BouncyCastle.Math.BigInteger(1, dp),
                new Org.BouncyCastle.Math.BigInteger(1, dq),
                new Org.BouncyCastle.Math.BigInteger(1, inverseQ));
        }

        private static DsaPublicKeyParameters ParseDsaPublicKey(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            if (root == null || root.Name != "DSAKeyValue")
                throw new CryptographicException("Invalid DSA XML key format.");

            var p = ReadXmlElement(root, "P");
            var q = ReadXmlElement(root, "Q");
            var g = ReadXmlElement(root, "G");
            var y = ReadXmlElement(root, "Y");

            if (p == null || q == null || g == null || y == null)
                throw new CryptographicException("Invalid DSA public key data.");

            var dsaParams = new DsaParameters(new Org.BouncyCastle.Math.BigInteger(1, p), new Org.BouncyCastle.Math.BigInteger(1, q), new Org.BouncyCastle.Math.BigInteger(1, g));
            return new DsaPublicKeyParameters(new Org.BouncyCastle.Math.BigInteger(1, y), dsaParams);
        }

        private static string RsaToXml(RsaKeyParameters publicKey, RsaPrivateCrtKeyParameters privateKey)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("RSAKeyValue");
            doc.AppendChild(root);

            AppendXmlElement(doc, root, "Modulus", publicKey.Modulus.ToByteArrayUnsigned());
            AppendXmlElement(doc, root, "Exponent", publicKey.Exponent.ToByteArrayUnsigned());

            if (privateKey != null)
            {
                AppendXmlElement(doc, root, "P", privateKey.P.ToByteArrayUnsigned());
                AppendXmlElement(doc, root, "Q", privateKey.Q.ToByteArrayUnsigned());
                AppendXmlElement(doc, root, "DP", privateKey.DP.ToByteArrayUnsigned());
                AppendXmlElement(doc, root, "DQ", privateKey.DQ.ToByteArrayUnsigned());
                AppendXmlElement(doc, root, "InverseQ", privateKey.QInv.ToByteArrayUnsigned());
                AppendXmlElement(doc, root, "D", privateKey.Exponent.ToByteArrayUnsigned());
            }

            return doc.OuterXml;
        }

        private static string DsaToXml(DsaPublicKeyParameters publicKey, DsaPrivateKeyParameters privateKey)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("DSAKeyValue");
            doc.AppendChild(root);

            var dsaParams = publicKey.Parameters;
            AppendXmlElement(doc, root, "P", dsaParams.P.ToByteArrayUnsigned());
            AppendXmlElement(doc, root, "Q", dsaParams.Q.ToByteArrayUnsigned());
            AppendXmlElement(doc, root, "G", dsaParams.G.ToByteArrayUnsigned());
            AppendXmlElement(doc, root, "Y", publicKey.Y.ToByteArrayUnsigned());

            if (privateKey != null)
            {
                AppendXmlElement(doc, root, "X", privateKey.X.ToByteArrayUnsigned());
            }

            return doc.OuterXml;
        }

        private static void AppendXmlElement(XmlDocument doc, XmlElement root, string name, byte[] value)
        {
            var element = doc.CreateElement(name);
            element.InnerText = Convert.ToBase64String(value);
            root.AppendChild(element);
        }

        private static byte[] ReadXmlElement(XmlElement root, string name)
        {
            var node = root.SelectSingleNode(name);
            if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                return null;
            return Convert.FromBase64String(node.InnerText);
        }
    }

    public class SignInfo
    {
        public string PrivateKey
        {
            get;
            set;
        }

        public string PublicKey
        {
            get;
            set;
        }

        public byte[] Signature
        {
            get;
            set;
        }
    }

    internal class VBCryptoStream : CryptoStream
    {
        private readonly Stream underlyingStream;
        public VBCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) : base(stream, transform, mode)
        {
            underlyingStream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch (CryptographicException ce)
            {
                if (disposing)
                {
                    this.underlyingStream.Dispose();
                }

                string msg = ce.Message;
                if (ce.InnerException != null && ce.InnerException.Message != null)
                    msg += " Inner:" + ce.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("CSP", "Dispose", msg);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("CSP", "Dispose(1)", msg);
            }
        }
    }
}
