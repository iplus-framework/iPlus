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
using System.Globalization;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Numerics;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Prng;
using System.Security.Cryptography;

namespace gip.core.crypto
{
    public static class X509CertificateCreator
    {
        const string C_SignatureAlgorithm = "SHA256WithRSA";
        const int C_SignatureStrength = 2048;
        public static X509Certificate2 GenerateCertificate(string commonName, string country, string companyName, string password, X509KeyStorageFlags flags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet)
        {
            var random = new SecureRandom();
            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(Org.BouncyCastle.Math.BigInteger.One, Org.BouncyCastle.Math.BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            string certDirName = string.Format("C={0}, O={1}, CN={2}", companyName, country, commonName);
            certificateGenerator.SetIssuerDN(new X509Name(certDirName));
            certificateGenerator.SetSubjectDN(new X509Name(certDirName));
            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(10));

            var keyGenerationParameters = new KeyGenerationParameters(random, C_SignatureStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);

            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            var issuerKeyPair = subjectKeyPair;
            var signatureFactory = new Asn1SignatureFactory(C_SignatureAlgorithm, issuerKeyPair.Private);
            var bouncyCert = certificateGenerator.Generate(signatureFactory);

            // Convert to X509Certificate2 WITH private key
            var x509Cert = new X509Certificate2(DotNetUtilities.ToX509Certificate(bouncyCert));

            // Attach private key via PKCS#12 export/import
            var privateKey = DotNetUtilities.ToRSA(issuerKeyPair.Private as RsaPrivateCrtKeyParameters);
            X509Certificate2 certWithKey = x509Cert.CopyWithPrivateKey(privateKey);
            byte[] pfxBytes = certWithKey.Export(X509ContentType.Pfx, password);

            X509Certificate2 x509Certificate = new X509Certificate2(pfxBytes, password, flags);
            return x509Certificate;
        }
    }
}
