// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    /// <summary>
    /// Tests for the two-level PKI chain (framework key -> developer issuer key)
    /// and for the framework runtime license kind.
    /// </summary>
    public class IssuerChainTests
    {
        [Fact]
        public void DeveloperIssuerLicense_CarriesIssuerPublicKey()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            LicenseIssuer issuer = new LicenseIssuer(frameworkKey);
            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.DeveloperIssuer,
                CustomerName = "ACME GmbH",
                IssuerName = "gip",
                IssuerPublicKey = LicenseKeys.ExportPublicKeyPem(developerKey)
            };

            string text = issuer.Issue(payload);
            LicenseValidationResult result = new LicenseVerifier(frameworkKey).Verify(text);

            Assert.True(result.IsValid, result.ToString());
            Assert.Equal(LicenseKind.DeveloperIssuer, result.Payload!.Kind);
            Assert.False(string.IsNullOrEmpty(result.Payload.IssuerPublicKey));
        }

        [Fact]
        public void ProductLicense_SignedByDeveloper_IsAcceptedThroughResolver()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            string developerKeyId = LicenseKeys.ComputeKeyId(developerKey);
            string developerPublicKey = LicenseKeys.ExportPublicKeyPem(developerKey);

            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "ACME GmbH",
                IssuerName = "ACME GmbH",
                Packages = { "ACME.Mes" },
                DatabaseFingerprint = "ABCDEF0123456789"
            };

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(payload);

            LicenseVerifier verifier = new LicenseVerifier(frameworkKey, keyId =>
                keyId == developerKeyId ? LicenseKeys.ImportPublicKey(developerPublicKey) : null);

            LicenseValidationResult result = verifier.Verify(text, new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                RequiredPackage = "ACME.Mes"
            });

            Assert.True(result.IsValid, result.ToString());
            Assert.Equal(developerKeyId, result.Payload!.IssuerKeyId);
        }

        [Fact]
        public void ProductLicense_SignedByDeveloper_IsRejectedWithoutResolver()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "ACME GmbH",
                Packages = { "ACME.Mes" }
            };

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(payload);
            LicenseValidationResult result = new LicenseVerifier(frameworkKey).Verify(text);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.InvalidSignature, result.Error);
        }

        private static string IssueDeveloperLicense(RSA frameworkKey, RSA developerKey)
        {
            return new LicenseIssuer(frameworkKey).Issue(new LicensePayload
            {
                Kind = LicenseKind.DeveloperIssuer,
                CustomerName = "ACME GmbH",
                IssuerPublicKey = LicenseKeys.ExportPublicKeyPem(developerKey)
            });
        }

        [Fact]
        public void ProductLicense_WithEmbeddedIssuerLicense_IsAcceptedWithoutResolver()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "Customer",
                Packages = { "ACME.Mes" },
                IssuerLicense = IssueDeveloperLicense(frameworkKey, developerKey)
            });

            LicenseValidationResult result = new LicenseVerifier(frameworkKey).Verify(text);

            Assert.True(result.IsValid, result.ToString());
        }

        [Fact]
        public void ProductLicense_EmbeddedIssuerLicenseOfOtherDeveloper_IsRejected()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();
            using RSA otherDeveloperKey = LicenseKeys.CreateKeyPair();

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "Customer",
                Packages = { "ACME.Mes" },
                IssuerLicense = IssueDeveloperLicense(frameworkKey, otherDeveloperKey)
            });

            Assert.False(new LicenseVerifier(frameworkKey).Verify(text).IsValid);
        }

        [Fact]
        public void ProductLicense_EmbeddedIssuerLicenseNotSignedByFramework_IsRejected()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA rogueFrameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "Customer",
                Packages = { "ACME.Mes" },
                IssuerLicense = IssueDeveloperLicense(rogueFrameworkKey, developerKey)
            });

            Assert.False(new LicenseVerifier(frameworkKey).Verify(text).IsValid);
        }

        [Fact]
        public void ProductLicense_UnknownIssuerKeyId_IsRejected()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "ACME GmbH",
                Packages = { "ACME.Mes" }
            };

            string text = new LicenseIssuer(developerKey).IssueWithKeyId(payload);
            LicenseVerifier verifier = new LicenseVerifier(frameworkKey, _ => null);

            LicenseValidationResult result = verifier.Verify(text);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.InvalidSignature, result.Error);
        }

        [Fact]
        public void DeveloperLicense_IsNotAcceptedThroughIssuerResolver()
        {
            using RSA frameworkKey = LicenseKeys.CreateKeyPair();
            using RSA developerKey = LicenseKeys.CreateKeyPair();

            string developerKeyId = LicenseKeys.ComputeKeyId(developerKey);
            string developerPublicKey = LicenseKeys.ExportPublicKeyPem(developerKey);

            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.DeveloperIssuer,
                CustomerName = "ACME GmbH"
            };

            // A developer must never be able to issue a developer license for itself.
            string text = new LicenseIssuer(developerKey).IssueWithKeyId(payload);

            LicenseVerifier verifier = new LicenseVerifier(frameworkKey, keyId =>
                keyId == developerKeyId ? LicenseKeys.ImportPublicKey(developerPublicKey) : null);

            LicenseValidationResult result = verifier.Verify(text);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.InvalidSignature, result.Error);
        }

        [Fact]
        public void ComputeKeyId_IsStableAndDiffersPerKey()
        {
            using RSA key1 = LicenseKeys.CreateKeyPair();
            using RSA key2 = LicenseKeys.CreateKeyPair();

            string id1 = LicenseKeys.ComputeKeyId(key1);
            string id2 = LicenseKeys.ComputeKeyId(key2);

            Assert.Equal(id1, LicenseKeys.ComputeKeyId(key1));
            Assert.NotEqual(id1, id2);
            Assert.Equal(16, id1.Length);
        }

        [Fact]
        public void ComputeKeyIdFromText_MatchesComputeKeyId()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
            string pem = LicenseKeys.ExportPublicKeyPem(key);

            Assert.Equal(LicenseKeys.ComputeKeyId(key), LicenseKeys.ComputeKeyIdFromText(pem));
        }

        [Fact]
        public void FrameworkRuntimeLicense_DoesNotCoverPackages()
        {
            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.FrameworkRuntime,
                CustomerName = "ACME GmbH",
                MaxUsers = 25,
                DatabaseFingerprint = "ABCDEF0123456789"
            };

            Assert.False(payload.CoversPackage("ACME.Mes"));
            Assert.False(payload.CoversPackage("gip.VarioSystem"));
            Assert.Equal(25, payload.MaxUsers);
        }

        [Fact]
        public void FrameworkRuntimeLicense_RoundTrips()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.FrameworkRuntime,
                CustomerName = "ACME GmbH",
                IssuerName = "gip",
                MaxUsers = 25,
                DatabaseFingerprint = "ABCDEF0123456789"
            };

            string text = new LicenseIssuer(key).Issue(payload);
            LicenseValidationResult result = new LicenseVerifier(key).Verify(text, new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                RequiredKind = LicenseKind.FrameworkRuntime
            });

            Assert.True(result.IsValid, result.ToString());
            Assert.Equal(25, result.Payload!.MaxUsers);
        }

        [Fact]
        public void FrameworkRuntimeLicense_WrongRequiredKind_Fails()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.FrameworkRuntime,
                CustomerName = "ACME GmbH",
                MaxUsers = 25
            };

            string text = new LicenseIssuer(key).Issue(payload);
            LicenseValidationResult result = new LicenseVerifier(key).Verify(text, new LicenseBindingContext
            {
                RequiredKind = LicenseKind.Product
            });

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.WrongLicenseKind, result.Error);
        }
    }
}
